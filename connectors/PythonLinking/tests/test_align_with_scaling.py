"""align_with_scaling: dispatch, the fixed order, and what composition preserves.

The point of this method is that neither half changes behaviour by being
composed, and that the order is a property of the code rather than of how a
caller happened to write the records. Both are what these tests hold down.
"""

import pandas as pd
import pytest

import euromod_linking.methods  # noqa: F401  (registers the methodologies)
from euromod_linking import shock_table
from euromod_linking.methods.align_with_scaling import ORDER, AlignWithScaling
from euromod_linking.methods.base import MethodContext
from euromod_linking.methods.lma_labour_alignment import INJECTED, LmaLabourAlignment
from euromod_linking.methods.scale_variables import ScaleVariables
from euromod_linking.registry import MethodLookupError, code_fingerprint, resolve, resolve_for_channels

CTX = MethodContext(country_code="AT", system_name="AT_2024")

SCALE = {"channel": "scale", "metric": "yem", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.10}
ALIGN = {"channel": "align", "metric": "employment", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.05}
PARAMS = {"period": "1"}


class TestDispatch:
    """A composing method declares every channel it can take, which by the
    subset rule also makes it a candidate for each channel alone. The dedicated
    method has to keep winning those."""

    def test_both_channels_resolve_to_the_composite(self):
        spec = resolve_for_channels({"align", "scale"}, {"employment", "yem"})
        assert spec.name == "align_with_scaling"

    @pytest.mark.parametrize("channels, metrics, expected", [
        ({"scale"}, {"yem"}, "scale_variables"),
        ({"align"}, {"employment"}, "lma_labour_alignment"),
    ])
    def test_a_single_channel_still_resolves_to_its_own_method(self, channels, metrics, expected):
        assert resolve_for_channels(channels, metrics).name == expected

    def test_genuine_ambiguity_is_still_an_error(self):
        """Narrowing to the fewest declared channels must not swallow two
        methods making the same claim."""
        from euromod_linking.registry import MethodSpec, _REGISTRY, register
        spec = MethodSpec(name="_rival", summary="x", description="x",
                          channels_consumed=("scale",), metrics_consumed=(),
                          cell_variables="x", dataset_requirements=(),
                          addon_requirements=((), ()), injected_columns=(),
                          params_schema={"type": "object"}, factory=object)
        register(spec)
        try:
            with pytest.raises(MethodLookupError) as e:
                resolve_for_channels({"scale"}, {"yem"})
            assert "_rival" in str(e.value) and "scale_variables" in str(e.value)
        finally:
            del _REGISTRY["_rival"]


class TestSpec:
    def test_contract_is_the_union_of_the_two_halves(self):
        spec = resolve("align_with_scaling")
        lma = resolve("lma_labour_alignment")
        assert set(spec.channels_consumed) == {"align", "scale"}
        # Everything the alignment half needs is still declared: it is the half
        # that restructures rows and needs the add-on.
        assert spec.restructures_rows and lma.restructures_rows
        assert spec.addon_requirements == lma.addon_requirements
        assert spec.min_model_release == lma.min_model_release
        assert set(lma.dataset_requirements) <= set(spec.dataset_requirements)
        assert spec.injected_columns == INJECTED

    def test_fingerprint_covers_the_delegates(self):
        """A composing method's own file says almost nothing about what it does.
        Without this, editing scale_variables would leave a composed run's
        cached results in place."""
        from euromod_linking.registry import MethodSpec, _REGISTRY, _code_fp_cache, register

        base = MethodSpec(name="_leaf", summary="x", description="x",
                          channels_consumed=("scale",), metrics_consumed=(),
                          cell_variables="x", dataset_requirements=(),
                          addon_requirements=((), ()), injected_columns=(),
                          params_schema={"type": "object"}, factory=ScaleVariables)
        composite = MethodSpec(name="_composite", summary="x", description="x",
                               channels_consumed=("scale",), metrics_consumed=(),
                               cell_variables="x", dataset_requirements=(),
                               addon_requirements=((), ()), injected_columns=(),
                               params_schema={"type": "object"},
                               composes=("_leaf",), factory=ScaleVariables)
        register(base)
        register(composite)
        try:
            plain = code_fingerprint(base)
            composed = code_fingerprint(composite)
            # Same factory, same file: only the folded-in delegate differs.
            assert plain and composed and plain != composed
        finally:
            for name in ("_leaf", "_composite"):
                _REGISTRY.pop(name, None)
                _code_fp_cache.pop(name, None)

    def test_declares_what_it_composes_in_run_order(self):
        assert resolve("align_with_scaling").composes == (
            "scale_variables", "lma_labour_alignment")


class TestCheckDataset:
    def test_one_channel_alone_points_at_the_dedicated_method(self, synthetic_microdata):
        cols = synthetic_microdata.columns
        only_scale = shock_table.normalize([SCALE])
        only_align = shock_table.normalize([ALIGN])
        assert any("scale_variables" in p
                   for p in AlignWithScaling().check_dataset(cols, only_scale))
        assert any("lma_labour_alignment" in p
                   for p in AlignWithScaling().check_dataset(cols, only_align))

    def test_both_halves_contracts_are_enforced(self, synthetic_microdata):
        cols = synthetic_microdata.columns
        shocks = shock_table.normalize([{**SCALE, "metric": "not_a_column"}, ALIGN])
        problems = AlignWithScaling().check_dataset(cols, shocks)
        assert any("not_a_column" in p for p in problems), problems

    def test_accepts_a_well_formed_pair(self, synthetic_microdata):
        shocks = shock_table.normalize([SCALE, ALIGN])
        assert AlignWithScaling().check_dataset(synthetic_microdata.columns, shocks) == []


class TestApply:
    def apply(self, df, records=(SCALE, ALIGN)):
        return AlignWithScaling().apply(df, shock_table.normalize(list(records)), PARAMS, CTX)

    def test_scaling_happens_and_alignment_hits_its_target(self, synthetic_microdata):
        result = self.apply(synthetic_microdata)
        emp = [t for t in result.diagnostics["align"]["targets"]
               if t["metric"] == "employment" and "deh=3-4" in t["cell"]]
        assert emp and all(r["grade"] == "exact" for r in emp), emp
        assert result.diagnostics["scale"]["n_shocks_applied"] == 1

    def test_order_is_scale_then_align_and_is_reported(self, synthetic_microdata):
        assert ORDER == ("scale", "align")
        assert self.apply(synthetic_microdata).diagnostics["order"] == ["scale", "align"]

    def test_record_order_does_not_change_the_result(self, synthetic_microdata):
        """The order is the method's, not the caller's. Writing align first must
        not run align first — that is the whole reason the order lives in code."""
        a = self.apply(synthetic_microdata, (SCALE, ALIGN))
        b = self.apply(synthetic_microdata, (ALIGN, SCALE))
        pd.testing.assert_frame_equal(a.data, b.data)

    def test_composition_equals_running_the_two_in_order(self, synthetic_microdata):
        """Neither half behaves differently for being composed."""
        df = synthetic_microdata
        scaled = ScaleVariables().apply(df, shock_table.normalize([SCALE]), PARAMS, CTX)
        aligned = LmaLabourAlignment().apply(scaled.data, shock_table.normalize([ALIGN]),
                                             PARAMS, CTX)
        pd.testing.assert_frame_equal(self.apply(df).data, aligned.data)

    def test_scaling_first_is_what_entrants_are_paid_from(self, synthetic_microdata):
        """yivwg is an input variable, so scaling it before the alignment is what
        makes a new worker enter at counterfactual wages. Aligning first would
        leave entrants on baseline wages, because their earnings land in yem_a."""
        df = synthetic_microdata
        wage = {**SCALE, "metric": "yivwg", "value": 0.20}
        with_scale = self.apply(df, (wage, ALIGN))
        without = LmaLabourAlignment().apply(df, shock_table.normalize([ALIGN]), PARAMS, CTX)

        entrants = with_scale.data["lma"] == 1
        assert entrants.any()
        paid = with_scale.data.loc[entrants, "yem_a"].mean()
        baseline_paid = without.data.loc[without.data["lma"] == 1, "yem_a"].mean()
        assert paid == pytest.approx(baseline_paid * 1.20, rel=1e-6)

    def test_diagnostics_stay_unflattened(self, synthetic_microdata):
        diag = self.apply(synthetic_microdata).diagnostics
        assert set(diag) == {"order", "scale", "align", "warnings"}
        assert "income_list_expansions" in diag["scale"]
        assert {"targets", "grades", "transitions"} <= set(diag["align"])

    def test_paired_baseline_is_row_aligned_and_carries_the_scaling(self, synthetic_microdata):
        """restructures_rows promises the two frames pair row for row; the
        baseline undoes the transitions, not the scaling."""
        result = self.apply(synthetic_microdata)
        assert result.baseline is not None
        assert list(result.baseline["idperson"]) == list(result.data["idperson"])
        assert (result.baseline["lma"] == 0).all()
        # The scale shock is present in both frames, so the delta is the alignment.
        cell = result.baseline["deh"].between(3, 4)
        pd.testing.assert_series_equal(result.baseline.loc[cell, "yem"],
                                       result.data.loc[cell, "yem"])

    def test_weight_is_conserved_through_the_composition(self, synthetic_microdata):
        result = self.apply(synthetic_microdata)
        totals = result.diagnostics["align"]["weight_totals"]
        assert totals["after"] == pytest.approx(totals["before"], rel=1e-9)


class TestPreview:
    def test_previews_the_alignment_against_the_scaled_frame(self, synthetic_microdata):
        """A preview that sized the alignment on unscaled data would disagree
        with the run it previews."""
        preview = AlignWithScaling().preview(
            synthetic_microdata, shock_table.normalize([SCALE, ALIGN]), PARAMS, CTX)
        assert preview["order"] == ["scale", "align"]
        assert "income_list_expansions" in preview["scale"]
        assert preview["align"]["cells"]

    def test_preview_cells_match_what_apply_reports(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = shock_table.normalize([SCALE, ALIGN])
        preview = AlignWithScaling().preview(df, shocks, PARAMS, CTX)
        applied = AlignWithScaling().apply(df, shocks, PARAMS, CTX)
        assert ([c["cell"] for c in preview["align"]["cells"]]
                == [c["cell"] for c in applied.diagnostics["align"]["cells"]])
