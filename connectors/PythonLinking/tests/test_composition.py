"""Several shocks in one scenario: per-channel dispatch, stage order, and what
the engine's composition preserves.

The point of composing in the engine rather than in a method is that no
method has to know about any other: each consumes its channel, declares a
stage, and the order is a property of the stages. These tests hold down that
the order is the engine's and not the caller's, that neither method behaves
differently for being composed, and that a mistake on one channel is refused
by name whatever else the table carries.
"""

import pandas as pd
import pytest

import euromod_linking.methods  # noqa: F401  (registers the methodologies)
from euromod_linking import registry, scenarios, shock_table
from euromod_linking.methods.base import MethodContext
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment
from euromod_linking.methods.scale_variables import ScaleVariables
from euromod_linking.registry import (MethodLookupError, MethodSpec, pipeline, pipeline_fingerprint,
                                      pipeline_name, resolve, resolve_for_channel,
                                      resolve_for_channels, resolve_pipeline)

CTX = MethodContext(country_code="AT", system_name="AT_2024")

SCALE = {"channel": "scale", "metric": "yem", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.10}
ALIGN = {"channel": "align", "metric": "employment", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.05}
PARAMS = {"period": "1"}

LEAF = dict(summary="x", description="x", metrics_consumed=(), cell_variables="x",
            dataset_requirements=(), addon_requirements=((), ()), injected_columns=(),
            params_schema={"type": "object"}, factory=object)


def scenario(*records, **kw):
    doc = {"country_code": "AT", "system_name": "AT_2024",
           "shocks": {"inline": list(records)}, "params": dict(PARAMS)}
    doc.update(kw)
    return doc


@pytest.fixture()
def engine(monkeypatch):
    """apply_scenario without a model: no session to adopt, no model to check."""
    monkeypatch.setattr("euromod_linking.session.adopt", lambda system: None)
    monkeypatch.setattr(scenarios, "_compatibility", lambda system, spec: None)

    def run(df, *records, validate_only=False, **kw):
        return scenarios.apply_scenario(object(), df, scenario(*records, **kw),
                                        validate_only=validate_only)
    return run


class TestDispatch:
    """One channel, one method. A table with several channels resolves each
    on its own, so nothing about one channel can change what another gets."""

    def test_each_channel_resolves_to_its_own_method(self):
        assert resolve_for_channel("scale", {"yem"}).name == "scale_variables"
        assert resolve_for_channel("align", {"employment"}).name == "lma_labour_alignment"

    def test_a_table_with_both_channels_gets_both_methods_in_stage_order(self):
        specs = resolve_for_channels({"align": {"employment"}, "scale": {"yem"}})
        assert [s.name for s in specs] == ["scale_variables", "lma_labour_alignment"]

    def test_a_bad_metric_is_refused_by_name_whatever_else_the_table_carries(self):
        """The failure this replaces: a composite declaring both channels was
        the sole candidate for a mistyped align metric, so the typo surfaced
        after a model load as 'needs a scale shock'."""
        with pytest.raises(MethodLookupError) as e:
            resolve_for_channels({"align": {"participation"}, "scale": {"yem"}})
        assert "participation" in str(e.value) and "'align'" in str(e.value)

    def test_an_unknown_channel_is_refused(self):
        with pytest.raises(MethodLookupError) as e:
            resolve_for_channel("reweight", set())
        assert "'reweight'" in str(e.value)

    def test_genuine_ambiguity_is_still_an_error(self):
        spec = MethodSpec(name="_rival", channels_consumed=("scale",), **LEAF)
        registry.register(spec)
        try:
            with pytest.raises(MethodLookupError) as e:
                resolve_for_channel("scale", {"yem"})
            assert "_rival" in str(e.value) and "scale_variables" in str(e.value)
        finally:
            del registry._REGISTRY["_rival"]


class TestPipeline:
    def test_order_is_by_stage_then_name_whatever_order_they_are_given(self):
        lma, scale = resolve("lma_labour_alignment"), resolve("scale_variables")
        assert scale.stage < lma.stage
        assert pipeline([lma, scale]) == [scale, lma]
        assert pipeline([scale, lma, scale]) == [scale, lma]

    def test_two_methods_on_one_channel_cannot_share_a_scenario(self):
        a = MethodSpec(name="_a", channels_consumed=("scale",), **LEAF)
        b = MethodSpec(name="_b", channels_consumed=("scale",), **LEAF)
        with pytest.raises(MethodLookupError) as e:
            pipeline([a, b])
        assert "_a" in str(e.value) and "_b" in str(e.value)

    def test_name_states_the_run_order_and_is_accepted_back_as_a_pin(self):
        specs = resolve_for_channels({"align": {"employment"}, "scale": {"yem"}})
        name = pipeline_name(specs)
        assert name == "scale_variables+lma_labour_alignment"
        assert resolve_pipeline(name) == specs
        # A pin written the other way round still runs in stage order.
        assert resolve_pipeline("lma_labour_alignment+scale_variables") == specs
        assert resolve_pipeline("lma_labour_alignment") == [resolve("lma_labour_alignment")]

    def test_fingerprint_covers_every_method_and_their_order(self):
        lma, scale = resolve("lma_labour_alignment"), resolve("scale_variables")
        both = pipeline_fingerprint([scale, lma])
        assert both and both != pipeline_fingerprint([scale]) == registry.code_fingerprint(scale)
        # The order is part of what a run does, so it is part of the identity.
        assert both != pipeline_fingerprint([lma, scale])
        assert pipeline_fingerprint([]) == ""


class TestCheckScenario:
    def test_reports_the_methods_in_run_order(self):
        out = scenarios.check_scenario(scenario(ALIGN, SCALE))
        assert out["methodology"] == "scale_variables+lma_labour_alignment"
        assert out["methods"] == ["scale_variables", "lma_labour_alignment"]
        assert [s["stage"] for s in out["stages"]] == sorted(s["stage"] for s in out["stages"])
        assert out["stages"][0]["channels"] == ["scale"]

    def test_single_channel_is_unchanged(self):
        out = scenarios.check_scenario(scenario(ALIGN))
        assert out["methodology"] == "lma_labour_alignment"
        assert out["methods"] == ["lma_labour_alignment"]

    def test_params_are_the_union_of_the_methods(self):
        """One scenario has one set of params, so a param one method declares
        is accepted when that method runs — and still refused when it does not."""
        both = scenario(ALIGN, SCALE, params={"period": "1", "tolerance_pct": 2.0})
        assert scenarios.check_scenario(both)["methods"]
        with pytest.raises(scenarios.ScenarioError) as e:
            scenarios.check_scenario(scenario(SCALE, params={"period": "1", "tolerance_pct": 2.0}))
        assert any("tolerance_pct" in p for p in e.value.problems)
        with pytest.raises(scenarios.ScenarioError) as e:
            scenarios.check_scenario(scenario(ALIGN, SCALE, params={"period": "1", "knob": 1}))
        assert any("knob" in p for p in e.value.problems)

    def test_run_arguments_are_the_union_of_the_methods(self):
        out = scenarios.check_scenario(scenario(ALIGN, SCALE))
        assert out["addons"] == [["LMA", "LMA_AT"]]
        assert out["extensions"] == [["LMA_trans", True]]
        assert scenarios.check_scenario(scenario(SCALE))["addons"] == []

    def test_a_pin_must_cover_every_channel(self):
        with pytest.raises(scenarios.ScenarioError) as e:
            scenarios.check_scenario(scenario(ALIGN, SCALE, methodology="lma_labour_alignment"))
        assert any("consumes channel 'scale'" in p for p in e.value.problems)
        with pytest.raises(scenarios.ScenarioError) as e:
            scenarios.check_scenario(scenario(ALIGN, methodology="scale_variables+lma_labour_alignment"))
        assert any("scale_variables" in p and "carries none" in p for p in e.value.problems)


class TestApply:
    def test_composition_equals_running_the_two_in_order(self, engine, synthetic_microdata):
        """Neither method behaves differently for being composed."""
        df = synthetic_microdata
        scaled = ScaleVariables().apply(df, shock_table.normalize([SCALE]), PARAMS, CTX)
        aligned = LmaLabourAlignment().apply(scaled.data, shock_table.normalize([ALIGN]),
                                             PARAMS, CTX)
        pd.testing.assert_frame_equal(engine(df, SCALE, ALIGN)["counterfactual"], aligned.data)

    def test_record_order_does_not_change_the_result(self, engine, synthetic_microdata):
        """The order is the engine's, not the caller's."""
        a = engine(synthetic_microdata, SCALE, ALIGN)
        b = engine(synthetic_microdata, ALIGN, SCALE)
        pd.testing.assert_frame_equal(a["counterfactual"], b["counterfactual"])
        assert a["methodology"] == b["methodology"] == "scale_variables+lma_labour_alignment"

    def test_values_change_before_people_move(self, engine, synthetic_microdata):
        """yivwg is an input variable, so scaling it before the alignment is what
        makes a new worker enter at counterfactual wages. Moving people first
        would leave entrants on baseline wages, because their earnings land in
        yem_a, which a scale shock does not reach."""
        df = synthetic_microdata
        wage = {**SCALE, "metric": "yivwg", "value": 0.20}
        with_scale = engine(df, wage, ALIGN)["counterfactual"]
        without = engine(df, ALIGN)["counterfactual"]

        entrants = with_scale["lma"] == 1
        assert entrants.any()
        paid = with_scale.loc[entrants, "yem_a"].mean()
        baseline_paid = without.loc[without["lma"] == 1, "yem_a"].mean()
        assert paid == pytest.approx(baseline_paid * 1.20, rel=1e-6)

    def test_diagnostics_nest_under_each_method_with_the_order(self, engine, synthetic_microdata):
        diag = engine(synthetic_microdata, SCALE, ALIGN)["diagnostics"]
        assert diag["order"] == ["scale_variables", "lma_labour_alignment"]
        assert [s["method"] for s in diag["stages"]] == diag["order"]
        assert "income_list_expansions" in diag["scale_variables"]
        assert {"targets", "grades", "transitions"} <= set(diag["lma_labour_alignment"])
        assert "cell_population" in diag

    def test_a_single_method_reports_flat_diagnostics(self, engine, synthetic_microdata):
        """The common case, and what every reader of one method expects."""
        diag = engine(synthetic_microdata, ALIGN)["diagnostics"]
        assert "order" not in diag and "targets" in diag

    def test_paired_baseline_is_row_aligned_and_carries_the_earlier_stage(
            self, engine, synthetic_microdata):
        """The baseline is built by the method that restructured rows, on the
        frame it received — it undoes the transitions, not the scaling."""
        plan = engine(synthetic_microdata, SCALE, ALIGN)
        base, cf = plan["baseline"], plan["counterfactual"]
        assert base is not None
        assert list(base["idperson"]) == list(cf["idperson"])
        assert (base["lma"] == 0).all()
        cell = base["deh"].between(3, 4)
        pd.testing.assert_series_equal(base.loc[cell, "yem"], cf.loc[cell, "yem"])

    def test_scaling_alone_has_no_paired_baseline(self, engine, synthetic_microdata):
        assert engine(synthetic_microdata, SCALE)["baseline"] is None


class TestPreview:
    def test_previews_each_method_and_the_later_one_against_the_earlier_frame(
            self, engine, synthetic_microdata):
        plan = engine(synthetic_microdata, SCALE, ALIGN, validate_only=True)
        diag = plan["diagnostics"]
        assert diag["order"] == ["scale_variables", "lma_labour_alignment"]
        assert "income_list_expansions" in diag["scale_variables"]
        assert diag["lma_labour_alignment"]["cells"]
        assert plan["counterfactual"] is None

    def test_preview_cells_match_what_apply_reports(self, engine, synthetic_microdata):
        preview = engine(synthetic_microdata, SCALE, ALIGN, validate_only=True)["diagnostics"]
        applied = engine(synthetic_microdata, SCALE, ALIGN)["diagnostics"]
        assert ([c["cell"] for c in preview["lma_labour_alignment"]["cells"]]
                == [c["cell"] for c in applied["lma_labour_alignment"]["cells"]])

    def test_later_stage_is_sized_against_the_scaled_frame(self, engine, synthetic_microdata):
        """scale_variables declares preview_by_applying, so the alignment's
        preview sees what the run will hand it. Scaling the labour-status
        variable itself is nonsense as a shock, but it is the one change the
        preview can see: on the untouched frame the cell would be full."""
        df = synthetic_microdata
        empty_cell = {"channel": "scale", "metric": "les2", "group": "deh=3-4",
                      "period": "1", "op": "set", "value": 7}
        preview = engine(df, empty_cell, ALIGN, validate_only=True)["diagnostics"]
        cells = preview["lma_labour_alignment"]["cells"]
        assert all(c["baseline"]["employed"] == 0 for c in cells if "deh=3-4" in c["cell"])
