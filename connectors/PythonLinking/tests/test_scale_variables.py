"""scale_variables: op semantics, subset-group matching, income-list
expansion, region collapse, determinism — plus the pure extension-aware
income-list resolution (no .NET needed)."""

import pandas as pd
import pytest

from euromod_linking import shock_table
from euromod_linking.methods.base import MethodContext, MethodError
from euromod_linking.methods.cells import resolve_period
from euromod_linking.methods.scale_variables import ScaleVariables
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment
from euromod_linking.query import IncomeListLookupError, resolve_income_list
import euromod_linking.methods  # noqa: F401


CTX = MethodContext(country_code="AT", system_name="AT_2024")


def shocks_of(records):
    return shock_table.normalize(records)


def scale_rec(**kw):
    base = {"channel": "scale", "metric": "yem", "group": "region=1",
            "period": "1", "op": "grow", "value": 0.03}
    base.update(kw)
    return base


class TestOps:
    def test_grow_in_region_cell(self, synthetic_microdata):
        df = synthetic_microdata
        out = ScaleVariables().apply(df, shocks_of([scale_rec()]), {}, CTX).data
        in_cell = df["drgn1"] == 1
        pd.testing.assert_series_equal(out.loc[in_cell, "yem"], df.loc[in_cell, "yem"] * 1.03,
                                       check_names=False)
        pd.testing.assert_series_equal(out.loc[~in_cell, "yem"], df.loc[~in_cell, "yem"].astype(float),
                                       check_names=False)

    def test_mult_add_set(self, synthetic_microdata):
        df = synthetic_microdata
        r = ScaleVariables()
        out = r.apply(df, shocks_of([scale_rec(op="mult", value=1.10)]), {}, CTX).data
        assert out.loc[df["drgn1"] == 1, "yem"].equals(df.loc[df["drgn1"] == 1, "yem"] * 1.10)
        out = r.apply(df, shocks_of([scale_rec(metric="lhw", op="add", value=2.0)]), {}, CTX).data
        assert out.loc[df["drgn1"] == 1, "lhw"].equals(df.loc[df["drgn1"] == 1, "lhw"] + 2.0)
        out = r.apply(df, shocks_of([scale_rec(metric="lhw", op="set", value=38.0)]), {}, CTX).data
        assert (out.loc[df["drgn1"] == 1, "lhw"] == 38.0).all()

    def test_whole_population_group(self, synthetic_microdata):
        df = synthetic_microdata
        out = ScaleVariables().apply(df, shocks_of([scale_rec(group="")]), {}, CTX).data
        pd.testing.assert_series_equal(out["yem"], df["yem"] * 1.03, check_names=False)

    def test_output_schema_and_row_order(self, synthetic_microdata):
        df = synthetic_microdata
        result = ScaleVariables().apply(df, shocks_of([scale_rec()]), {}, CTX)
        assert list(result.data.columns) == list(df.columns)
        assert list(result.data["idperson"]) == list(df["idperson"])
        assert result.diagnostics["applied"][0]["n_rows"] > 0


class TestComposition:
    def test_heterogeneous_groups_compose(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = shocks_of([scale_rec(op="mult", value=1.10),
                            scale_rec(group="deh=3-4", op="grow", value=0.10)])
        out = ScaleVariables().apply(df, shocks, {}, CTX).data
        medium = df["deh"].between(3, 4)
        both = (df["drgn1"] == 1) & medium
        only_region = (df["drgn1"] == 1) & ~medium
        assert out.loc[both, "yem"].equals(df.loc[both, "yem"] * 1.10 * 1.10)
        assert out.loc[only_region, "yem"].equals(df.loc[only_region, "yem"] * 1.10)

    def test_overlapping_set_rejected(self, synthetic_microdata):
        shocks = shocks_of([scale_rec(op="set", value=100.0, group=""),
                            scale_rec(op="mult", value=1.1)])
        with pytest.raises(MethodError, match="do not commute"):
            ScaleVariables().apply(synthetic_microdata, shocks, {}, CTX)

    def test_two_overlapping_sets_rejected(self, synthetic_microdata):
        """'set' does not commute with itself either — the later one silently wins."""
        shocks = shocks_of([scale_rec(op="set", value=100.0, group=""),
                            scale_rec(op="set", value=200.0, group="dgn=1")])
        with pytest.raises(MethodError, match="do not commute"):
            ScaleVariables().apply(synthetic_microdata, shocks, {}, CTX)

    def test_overlapping_add_and_mult_rejected(self, synthetic_microdata):
        """(x + a) * m is not x * m + a, so which cell sorts first decides the
        answer. Alphabetical group order is no basis for an economic result."""
        shocks = shocks_of([scale_rec(op="add", value=25.0, group="dgn=1"),
                            scale_rec(op="mult", value=2.0, group="deh=3-4")])
        with pytest.raises(MethodError, match="do not commute"):
            ScaleVariables().apply(synthetic_microdata, shocks, {}, CTX)

    def test_overlapping_adds_compose(self, synthetic_microdata):
        """add commutes with add, so overlapping additive shocks are fine."""
        df = synthetic_microdata
        shocks = shocks_of([scale_rec(metric="lhw", op="add", value=2.0, group="dgn=1"),
                            scale_rec(metric="lhw", op="add", value=3.0, group="deh=3-4")])
        out = ScaleVariables().apply(df, shocks, {}, CTX).data
        both = (df["dgn"] == 1) & df["deh"].between(3, 4)
        assert out.loc[both, "lhw"].equals(df.loc[both, "lhw"] + 5.0)

    def test_disjoint_cells_may_use_any_ops(self, synthetic_microdata):
        """The rule is about overlap, not about mixing ops in one table."""
        df = synthetic_microdata
        shocks = shocks_of([scale_rec(op="add", value=25.0, group="dgn=1"),
                            scale_rec(op="mult", value=2.0, group="dgn=0")])
        out = ScaleVariables().apply(df, shocks, {}, CTX).data
        men, women = df["dgn"] == 1, df["dgn"] == 0
        assert out.loc[men, "yem"].equals(df.loc[men, "yem"] + 25.0)
        assert out.loc[women, "yem"].equals(df.loc[women, "yem"] * 2.0)

    def test_unmatched_group_warns(self, synthetic_microdata):
        result = ScaleVariables().apply(
            synthetic_microdata, shocks_of([scale_rec(group="region=9")]), {}, CTX)
        assert any("matches no one" in w for w in result.diagnostics["warnings"])

    def test_nuts2_collapse_averages(self, synthetic_microdata):
        df = synthetic_microdata  # data at drgn1 level
        shocks = shocks_of([scale_rec(group="region=11", value=0.02),
                            scale_rec(group="region=12", value=0.06)])
        result = ScaleVariables().apply(df, shocks, {}, CTX)
        out = result.data
        in_cell = df["drgn1"] == 1
        pd.testing.assert_series_equal(out.loc[in_cell, "yem"], df.loc[in_cell, "yem"] * 1.04,
                                       check_names=False)
        assert any("collapsed" in w for w in result.diagnostics["warnings"])

    def test_coarser_region_broadcasts_by_prefix(self, synthetic_microdata):
        """A NUTS-1 shock against NUTS-2 data scales all subregions (prefix match)."""
        df = synthetic_microdata.copy()
        df["drgn2"] = df["drgn1"].map({1: "11", 2: "21"})
        df.loc[df.index[::3], "drgn2"] = df.loc[df.index[::3], "drgn1"].map({1: "12", 2: "22"})
        out = ScaleVariables().apply(df, shocks_of([scale_rec(group="region=1")]), {}, CTX).data
        in_r1 = df["drgn2"].str.startswith("1")
        pd.testing.assert_series_equal(out.loc[in_r1, "yem"], df.loc[in_r1, "yem"] * 1.03,
                                       check_names=False)
        pd.testing.assert_series_equal(out.loc[~in_r1, "yem"], df.loc[~in_r1, "yem"].astype(float),
                                       check_names=False)

    def test_determinism_under_row_shuffle(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = shocks_of([scale_rec(), scale_rec(group="dgn=0", op="mult", value=1.05)])
        a = ScaleVariables().apply(df, shocks, {}, CTX).data.set_index("idperson").sort_index()
        b = ScaleVariables().apply(df.sample(frac=1.0, random_state=5), shocks, {}, CTX
                                   ).data.set_index("idperson").sort_index()
        pd.testing.assert_frame_equal(a, b)


@pytest.fixture()
def income_lists(monkeypatch):
    """Stand in for the model's own income-list definitions.

    The method resolves lists against the live model itself, so a test that
    exercises expansion stubs the lookup rather than pre-supplying the answer."""
    def setter(mapping):
        def fake(country_code, system_name, metric, dataset=None, extensions=None):
            from euromod_linking.query import IncomeListLookupError
            if metric not in mapping:
                raise IncomeListLookupError(f"{metric!r} is not defined for this system",
                                            sorted(mapping))
            return mapping[metric]
        monkeypatch.setattr("euromod_linking.query.income_list_components", fake)
    return setter


class TestIncomeListExpansion:
    def test_fan_out_and_skip(self, synthetic_microdata, income_lists):
        income_lists({"ils_udb_yem": [("yem", "+"), ("yem_extra", "+")]})
        df = synthetic_microdata
        result = ScaleVariables().apply(
            df, shocks_of([scale_rec(metric="ils_udb_yem", group="")]), {}, CTX)
        out = result.data
        pd.testing.assert_series_equal(out["yem"], df["yem"] * 1.03, check_names=False)
        exp = result.diagnostics["income_list_expansions"]["ils_udb_yem"]
        assert exp["scaled"] == ["yem"]
        assert exp["skipped_not_in_input"] == ["yem_extra"]
        assert result.diagnostics["applied"][0]["expanded_from"] == "ils_udb_yem"

    def test_preview_reports_the_same_split_as_apply(self, synthetic_microdata, income_lists):
        """The preview and the run must not disagree about what will be scaled."""
        income_lists({"ils_udb_yem": [("yem", "+"), ("yem_extra", "+")]})
        shocks = shocks_of([scale_rec(metric="ils_udb_yem", group="")])
        method = ScaleVariables()
        previewed = method.preview(synthetic_microdata, shocks, {}, CTX)
        applied = method.apply(synthetic_microdata, shocks, {}, CTX)
        assert (previewed["income_list_expansions"]
                == applied.diagnostics["income_list_expansions"])

    def test_add_op_rejected_for_lists(self, synthetic_microdata, income_lists):
        income_lists({"ils_udb_yem": [("yem", "+")]})
        with pytest.raises(MethodError, match="mult.*grow"):
            ScaleVariables().apply(
                synthetic_microdata,
                shocks_of([scale_rec(metric="ils_udb_yem", op="add", value=100.0)]), {}, CTX)

    def test_unknown_list_errors(self, synthetic_microdata, income_lists):
        income_lists({"ils_udb_yse": [("yse", "+")]})
        with pytest.raises(MethodError, match="could not be resolved"):
            ScaleVariables().apply(
                synthetic_microdata, shocks_of([scale_rec(metric="ils_udb_yem")]), {}, CTX)

    def test_unreachable_model_is_a_method_error_not_a_session_error(self, synthetic_microdata):
        """No model loaded is still 'this list could not be resolved' — a session
        error must not escape a method call."""
        with pytest.raises(MethodError, match="could not be resolved"):
            ScaleVariables().apply(
                synthetic_microdata, shocks_of([scale_rec(metric="ils_udb_yem")]), {}, CTX)


class TestPeriodDefaulting:
    def test_single_period_defaults(self, synthetic_microdata):
        result = ScaleVariables().apply(synthetic_microdata, shocks_of([scale_rec()]), {}, CTX)
        assert result.diagnostics["period"] == "1"

    def test_multi_period_requires_param(self, synthetic_microdata):
        shocks = shocks_of([scale_rec(period="1"), scale_rec(period="2")])
        with pytest.raises(MethodError, match="params.period is required"):
            ScaleVariables().apply(synthetic_microdata, shocks, {}, CTX)
        out = ScaleVariables().apply(synthetic_microdata, shocks, {"period": "2"}, CTX)
        assert out.diagnostics["period"] == "2"

    def test_lma_single_period_defaults(self, synthetic_microdata):
        shocks = shocks_of([{"channel": "align", "metric": "employment", "group": "deh=3-4",
                             "period": "7", "op": "grow", "value": 0.02}])
        result = LmaLabourAlignment().apply(synthetic_microdata, shocks, {}, CTX)
        assert result.diagnostics["period"] == "7"

    def test_resolve_period_helper(self):
        df = shocks_of([scale_rec(period="3")])
        assert resolve_period(df, {}) == "3"
        assert resolve_period(df, {"period": "9"}) == "9"


class TestCheckDataset:
    def test_missing_variable(self, synthetic_microdata):
        cols = list(synthetic_microdata.columns)
        problems = ScaleVariables().check_dataset(cols, shocks_of([scale_rec(metric="nope")]))
        assert any("'nope'" in p for p in problems)

    def test_income_list_metric_deferred(self, synthetic_microdata):
        cols = list(synthetic_microdata.columns)
        assert ScaleVariables().check_dataset(cols, shocks_of([scale_rec(metric="ils_udb_yem")])) == []


# --- pure extension-aware income-list resolution ------------------------------

def raw_system(**il_occ):
    return {
        "name": "AT_2024",
        "dataset_names": {"AT_2024_ds"}, "bestmatch": "AT_2024_ds",
        "switch_defaults": {},
        "policies": [{"name": "ildef_at", "switch": "on", "ext": ()},
                     {"name": "ildef_off_at", "switch": "off", "ext": ()}],
        "il_occ": il_occ,
    }


def occ(components, key=(5, 17), policy="ildef_at", fun_switch="on", fun_ext=()):
    return {"key": key, "fun_ext": fun_ext, "fun_switch": fun_switch,
            "policy": policy, "components": components}


class TestIncomeListResolution:
    def test_plain(self):
        rsys = raw_system(ils_udb_yem=[occ([("yem", "+", ()), ("yemse", "+", ())])])
        assert resolve_income_list(rsys, {}, "ils_udb_yem") == [("yem", "+"), ("yemse", "+")]

    def test_extension_gated_component(self):
        # (BTA, baseOff=False): included only when the BTA extension is on.
        rsys = raw_system(ils_udb_yem=[occ([("yem", "+", ()),
                                            ("yembta", "+", (("BTA", False),))])])
        assert resolve_income_list(rsys, {}, "ils_udb_yem") == [("yem", "+")]
        assert resolve_income_list(rsys, {"BTA": True}, "ils_udb_yem") == [
            ("yem", "+"), ("yembta", "+")]

    def test_extension_gated_function(self):
        rsys = raw_system(ils_x=[occ([("yem", "+", ())], fun_ext=(("HHoT", False),))])
        with pytest.raises(IncomeListLookupError, match="not active"):
            resolve_income_list(rsys, {}, "ils_x")
        assert resolve_income_list(rsys, {"HHoT": True}, "ils_x") == [("yem", "+")]

    def test_inactive_policy(self):
        rsys = raw_system(ils_x=[occ([("yem", "+", ())], policy="ildef_off_at")])
        with pytest.raises(IncomeListLookupError, match="not active"):
            resolve_income_list(rsys, {}, "ils_x")

    def test_spine_order_last_active_wins(self):
        rsys = raw_system(ils_x=[occ([("yem", "+", ())], key=(5, 1)),
                                 occ([("yse", "+", ())], key=(7, 1))])
        assert resolve_income_list(rsys, {}, "ils_x") == [("yse", "+")]

    def test_nested_lists_and_signs(self):
        rsys = raw_system(
            ils_udb_yem=[occ([("yem", "+", ())])],
            ils_net=[occ([("ils_udb_yem", "+", ()), ("tin", "-", ())], key=(6, 1))])
        assert resolve_income_list(rsys, {}, "ils_net") == [("yem", "+"), ("tin", "-")]
        rsys2 = raw_system(
            ils_udb_yem=[occ([("yem", "+", ())])],
            ils_neg=[occ([("ils_udb_yem", "-", ())], key=(6, 1))])
        assert resolve_income_list(rsys2, {}, "ils_neg") == [("yem", "-")]

    def test_cycle_guard(self):
        rsys = raw_system(
            ils_a=[occ([("ils_b", "+", ())])],
            ils_b=[occ([("ils_a", "+", ())], key=(6, 1))])
        with pytest.raises(IncomeListLookupError, match="Cyclic"):
            resolve_income_list(rsys, {}, "ils_a")

    def test_unknown_list(self):
        rsys = raw_system(ils_x=[occ([("yem", "+", ())])])
        with pytest.raises(IncomeListLookupError) as e:
            resolve_income_list(rsys, {}, "ils_nope")
        assert e.value.available == ["ils_x"]


class TestIncomeListCatalogue:
    """The ils_udb_* catalogue published via describe_shocks.

    It exists so a caller shocking an economic concept ("investment income")
    names the model's own aggregate instead of guessing which raw variables
    belong to it — so it has to stay true to the model.
    """

    def test_catalogue_is_grouped_and_named(self):
        from euromod_linking.methods.scale_variables import INCOME_LISTS, income_lists
        cat = income_lists()
        assert set(cat) == {"lists", "groups", "aliases", "notes"}
        flat = {k: v for grp in INCOME_LISTS.values() for k, v in grp.items()}
        assert all(n.startswith("ils_udb_") for n in flat), sorted(flat)
        assert all(d and d[0].isupper() for d in flat.values()), flat

    def test_market_income_is_marked_scalable_and_benefits_are_not(self):
        from euromod_linking.income_lists import BY_NAME
        for name in ("ils_udb_yem", "ils_udb_yse", "ils_udb_yiy", "ils_udb_kfbcc",
                     "ils_udb_xmp"):
            assert BY_NAME[name].scalable, name
        for name in ("ils_udb_tis", "ils_udb_tpr", "ils_udb_bun"):
            assert not BY_NAME[name].scalable, name

    def test_disposable_income_is_an_aggregate_not_market_income(self):
        """ils_udb_yds holds no variables of its own, only the other twenty lists
        including taxes. Filing it as scalable market income sent a caller after
        an aggregate that fans out over everything."""
        from euromod_linking.income_lists import BY_NAME
        yds = BY_NAME["ils_udb_yds"]
        assert yds.group == "aggregate"
        assert not yds.scalable
        assert "disposable" in yds.label.lower()
        assert yds.note

    def test_notes_state_the_mult_grow_restriction(self):
        from euromod_linking.methods.scale_variables import income_lists
        notes = " ".join(income_lists()["notes"])
        assert "'mult' and 'grow' only" in notes
        assert "extension-specific" in notes

    def test_every_list_has_a_label_and_at_least_one_alias(self):
        from euromod_linking.income_lists import CATALOGUE
        for e in CATALOGUE:
            assert e.label and e.label[0].isupper(), e.name
            assert e.accepted[0] == e.label, e.name
            assert len(e.accepted) >= 2, e.name

    def test_no_alias_is_claimed_by_two_lists(self):
        """_build_index raises on a collision, so importing at all proves it —
        but assert the shape too, since a silent last-wins would be worse than
        an import error."""
        from euromod_linking import income_lists as il
        seen = {}
        for e in il.CATALOGUE:
            for spelling in e.accepted:
                k = il._key(spelling)
                assert seen.setdefault(k, e.name) == e.name, spelling
        assert len(il._INDEX) >= len(il.CATALOGUE) * 2

    @pytest.mark.live
    def test_catalogue_is_exactly_what_the_model_defines(self, be_system_raw):
        """Guards against the catalogue drifting from the model in either
        direction — a name that no longer resolves, or a list the model gained
        that nobody can name. Marked live: needs EUROMOD_MODEL_PATH."""
        from euromod_linking.income_lists import names
        defined = {n for n in be_system_raw["il_occ"] if n.startswith("ils_udb_")}
        assert set(names()) == defined, {
            "missing from catalogue": sorted(defined - set(names())),
            "absent from model": sorted(set(names()) - defined),
        }


class TestSimulatedComponentsAreNotScalable:
    """A variable ending in _s is a simulated output: EUROMOD computes it during
    the run, so it is not a column of the input and cannot be scaled."""

    def test_report_splits_scaled_from_skipped(self):
        from euromod_linking.methods.scale_variables import expansion_report
        report, warnings = expansion_report(
            {"ils_udb_bun": [("bun", "+"), ("bun_s", "+"), ("byr", "+"), ("bwkmcee_s", "+")]},
            columns=["bun", "byr", "dwt"])
        assert report["ils_udb_bun"] == {"scaled": ["bun", "byr"],
                                         "skipped_not_in_input": ["bun_s", "bwkmcee_s"]}
        assert warnings and "will NOT be scaled" in warnings[0]
        assert "constant" in warnings[0], "should point at the channel that does work"

    def test_fully_scalable_list_warns_about_nothing(self):
        from euromod_linking.methods.scale_variables import expansion_report
        report, warnings = expansion_report(
            {"ils_udb_yem": [("yem", "+")]}, columns=["yem", "dwt"])
        assert report["ils_udb_yem"] == {"scaled": ["yem"], "skipped_not_in_input": []}
        assert warnings == []

    def test_validate_and_run_report_the_same_split(self, synthetic_microdata):
        """The preview promised five components once while the run scaled two.
        Both paths now derive the split from the same columns."""
        from euromod_linking.methods.scale_variables import expansion_report
        exp = {"ils_udb_yem": [("yem", "+"), ("yem_s", "+")]}
        cols = list(synthetic_microdata.columns)
        assert expansion_report(exp, cols) == expansion_report(exp, cols)
        report, _ = expansion_report(exp, cols)
        assert report["ils_udb_yem"]["skipped_not_in_input"] == ["yem_s"]
