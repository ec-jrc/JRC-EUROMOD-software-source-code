"""Hierarchy + full method apply(): exact target hits, state consistency,
generic dimensions (age_band x gender), determinism."""

import numpy as np
import pandas as pd
import pytest

from euromod_linking import shock_table
from euromod_linking.methods.base import MethodContext, MethodError
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment
import euromod_linking.methods  # noqa: F401


CTX = MethodContext(country_code="AT", system_name="AT_2024")


def targets_for(shocks_records):
    return shock_table.normalize(shocks_records)


def achieved(out: pd.DataFrame, original: pd.DataFrame, les_codes_employed=(1, 2, 3, 4)):
    """Weighted count of employed people in the method output: originally
    employed (per les2) who did not leave, plus lma==1 entrants."""
    was_employed = out["les2"].isin(les_codes_employed)
    # lma==2 marks entrants to unemployment; employed leavers to inactivity are
    # not marked in lma, so recompute from the les2/lma logic is not possible
    # from the output alone — tests therefore shock cells where no employed
    # leave (positive employment gaps).
    emp_now = (was_employed & (out["lma"] != 2)) | (out["lma"] == 1)
    return float(out.loc[emp_now, "dwt"].sum())


class TestMethodApply:
    def test_employment_growth_by_skill_hits_target_exactly(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "deh=3-4",
             "period": "1", "op": "grow", "value": 0.05},
        ])
        result = LmaLabourAlignment().apply(df, shocks, {"period": "1"}, CTX)

        diag = result.diagnostics
        emp_rows = [t for t in diag["targets"] if t["metric"] == "employment"
                    and "deh=3-4" in t["cell"]]
        assert emp_rows and all(r["grade"] == "exact" for r in emp_rows), emp_rows
        # Weight totals conserved (splits partition, never add weight)
        assert diag["weight_totals"]["after"] == pytest.approx(
            diag["weight_totals"]["before"], rel=1e-9)

    def test_generic_dimensions_age_gender(self, synthetic_microdata):
        """Cells built from raw dag x dgn variables — no region anywhere."""
        df = synthetic_microdata
        # Growth sized within each cell's inactive candidate pool (see conftest).
        shocks = targets_for([
            {"channel": "align", "metric": "employment",
             "group": "dag=25-34;dgn=0", "period": "1", "op": "grow", "value": 0.05},
            {"channel": "align", "metric": "employment",
             "group": "dag=35-44;dgn=1", "period": "1", "op": "grow", "value": 0.04},
        ])
        result = LmaLabourAlignment().apply(df, shocks, {"period": "1"}, CTX)
        diag = result.diagnostics
        shocked = [t for t in diag["targets"] if t["metric"] == "employment"
                   and t["cell"] in ("dag=25-34;dgn=0", "dag=35-44;dgn=1")]
        assert len(shocked) == 2
        assert all(r["grade"] == "exact" for r in shocked), shocked

    def test_unemployment_target_and_output_schema(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = targets_for([
            {"channel": "align", "metric": "unemployment", "group": "region=1",
             "period": "2", "op": "grow", "value": 0.20},
        ])
        result = LmaLabourAlignment().apply(df, shocks, {"period": "2"}, CTX)
        out = result.data
        # Output = original columns + the 4 injected, sorted by household
        assert list(out.columns) == list(df.columns) + ["lma", "yem_a", "yemmy_a", "lhw_a"]
        assert out["idperson"].is_unique
        assert (out.groupby("idhh")["idperson"].count() > 0).all()
        assert set(out["lma"].unique()) <= {0, 1, 2}
        # New unemployed marked lma=2
        assert (out["lma"] == 2).any()
        u_rows = [t for t in result.diagnostics["targets"]
                  if t["metric"] == "unemployment" and t["cell"] == "region=1"]
        assert u_rows[0]["grade"] == "exact"

    def _entrant_shock(self):
        return targets_for([
            {"channel": "align", "metric": "employment", "group": "",
             "period": "1", "op": "grow", "value": 0.08},
        ])

    def test_entrants_get_full_time_wage_from_yivwg(self, synthetic_microdata):
        """Default take-up: full-time all year at the person's predicted wage."""
        from euromod_linking.methods.lma_labour_alignment import (
            DEFAULT_FULLTIME_HOURS_PER_WEEK, MONTHS_PER_YEAR, WEEKS_PER_MONTH)

        df = synthetic_microdata
        result = LmaLabourAlignment().apply(df, self._entrant_shock(), {"period": "1"}, CTX)
        out = result.data
        entrants = out[out["lma"] == 1]
        assert len(entrants) > 0
        assert (entrants["yem_a"] > 0).all()
        assert (entrants["yemmy_a"] == MONTHS_PER_YEAR).all()
        assert (entrants["lhw_a"] == DEFAULT_FULLTIME_HOURS_PER_WEEK).all()

        expected = DEFAULT_FULLTIME_HOURS_PER_WEEK * WEEKS_PER_MONTH * entrants["yivwg"]
        pd.testing.assert_series_equal(entrants["yem_a"], expected, check_names=False)

        e = result.diagnostics["earnings"]
        assert e["n_entrants"] == len(entrants)
        assert e["n_from_predicted_wage"] == len(entrants)
        assert e["n_donor_matched_fallback"] == 0

    def test_donor_matching_is_the_fallback(self, synthetic_microdata):
        """Entrants without a usable predicted wage fall back to matching."""
        df = synthetic_microdata.copy()
        df.loc[df.index[::2], "yivwg"] = 0.0  # no predicted wage for half the sample
        result = LmaLabourAlignment().apply(df, self._entrant_shock(), {"period": "1"}, CTX)
        e = result.diagnostics["earnings"]
        assert e["n_donor_matched_fallback"] > 0
        assert e["n_from_predicted_wage"] + e["n_donor_matched_fallback"] == e["n_entrants"]
        assert result.diagnostics["imputation"]["n_donors"] > 0
        assert (result.data.loc[result.data["lma"] == 1, "yem_a"] > 0).all()

    def test_no_yivwg_column_uses_matching_only(self, synthetic_microdata):
        df = synthetic_microdata.drop(columns=["yivwg"])
        result = LmaLabourAlignment().apply(df, self._entrant_shock(), {"period": "1"}, CTX)
        e = result.diagnostics["earnings"]
        assert e["n_from_predicted_wage"] == 0
        assert e["n_donor_matched_fallback"] == e["n_entrants"] > 0

    def test_determinism(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "deh=5-99",
             "period": "1", "op": "grow", "value": 0.07},
        ])
        a = LmaLabourAlignment().apply(df.copy(), shocks, {"period": "1"}, CTX)
        b = LmaLabourAlignment().apply(df.sample(frac=1.0, random_state=3), shocks,
                                       {"period": "1"}, CTX)
        pd.testing.assert_frame_equal(a.data, b.data)
        assert a.diagnostics["targets"] == b.diagnostics["targets"]

    def test_unfillable_gap_reported_not_fatal(self, synthetic_microdata):
        df = synthetic_microdata
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "",
             "period": "1", "op": "grow", "value": 5.0},  # +500%: impossible
        ])
        result = LmaLabourAlignment().apply(df, shocks, {"period": "1"}, CTX)
        assert result.diagnostics["unfillable_gaps"]
        emp = [t for t in result.diagnostics["targets"] if t["metric"] == "employment"][0]
        assert emp["grade"] == "failed"  # unemployment (unshocked) may still be exact

    def test_missing_period_raises(self, synthetic_microdata):
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "",
             "period": "1", "op": "grow", "value": 0.02},
        ])
        with pytest.raises(MethodError, match="No align shocks for period"):
            LmaLabourAlignment().apply(synthetic_microdata, shocks, {"period": "9"}, CTX)

    def test_region_collapse_nuts2_to_nuts1(self, synthetic_microdata):
        """Data has drgn1 ('1','2'); NUTS-2 shocks ('11','12') collapse by
        truncation with growth rates averaged."""
        df = synthetic_microdata
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "region=11",
             "period": "1", "op": "grow", "value": 0.02},
            {"channel": "align", "metric": "employment", "group": "region=12",
             "period": "1", "op": "grow", "value": 0.06},
        ])
        result = LmaLabourAlignment().apply(df, shocks, {"period": "1"}, CTX)
        diag = result.diagnostics
        assert any("collapsed" in w for w in diag["warnings"])
        row = [t for t in diag["targets"] if t["metric"] == "employment"
               and t["cell"] == "region=1"][0]
        # Average of 0.02 and 0.06 -> +4% on region 1's current employment
        assert row["grade"] == "exact"

    def test_baseline_transform_neutral(self, synthetic_microdata):
        out = LmaLabourAlignment().baseline_transform(synthetic_microdata, CTX)
        assert (out["lma"] == 0).all()
        assert (out[["yem_a", "yemmy_a", "lhw_a"]] == 0).all().all()
        assert len(out) == len(synthetic_microdata)


class TestCheckDataset:
    def test_ok(self, synthetic_microdata):
        shocks = targets_for([{"channel": "align", "metric": "employment",
                               "group": "deh=3-4", "period": "1",
                               "op": "grow", "value": 0.02}])
        assert LmaLabourAlignment().check_dataset(list(synthetic_microdata.columns), shocks) == []

    def test_missing_columns(self):
        shocks = targets_for([{"channel": "align", "metric": "employment",
                               "group": "deh=3-4", "period": "1",
                               "op": "grow", "value": 0.02}])
        problems = LmaLabourAlignment().check_dataset(["idhh", "idperson"], shocks)
        assert any("Missing required" in p for p in problems)
        assert any("les" in p for p in problems)
        # The cell variable itself must exist in the dataset too.
        assert any("'deh'" in p and "not a column of the input dataset" in p for p in problems)


class TestPairedBaseline:
    """The method emits a baseline built on the counterfactual's own rows, so
    baseline-vs-reform statistics can pair observations (fixed poverty line,
    baseline-defined deciles)."""

    def _split_result(self, df):
        # A fractional target forces at least one boundary household split.
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "deh=3-4",
             "period": "1", "op": "grow", "value": 0.037},
        ])
        return LmaLabourAlignment().apply(df, shocks, {"period": "1"}, CTX)

    def test_baseline_is_row_aligned_with_counterfactual(self, synthetic_microdata):
        r = self._split_result(synthetic_microdata)
        assert r.baseline is not None
        assert len(r.baseline) == len(r.data)
        assert list(r.baseline["idperson"]) == list(r.data["idperson"])
        assert list(r.baseline["idhh"]) == list(r.data["idhh"])
        pd.testing.assert_series_equal(r.baseline["dwt"], r.data["dwt"])

    def test_baseline_has_no_transitions(self, synthetic_microdata):
        r = self._split_result(synthetic_microdata)
        assert (r.baseline["lma"] == 0).all()
        assert (r.baseline[["yem_a", "yemmy_a", "lhw_a"]] == 0).all().all()
        assert (r.data["lma"] != 0).any()  # the counterfactual does transition

    def test_paired_baseline_is_the_same_population_as_the_input(self, synthetic_microdata):
        """Splitting only partitions weight, so the rebuilt baseline reproduces
        the original population exactly — it is a representation change."""
        df = synthetic_microdata
        r = self._split_result(df)
        assert r.baseline["dwt"].sum() == pytest.approx(df["dwt"].sum())
        for var in ("yem", "yemmy", "lhw"):
            assert (r.baseline[var] * r.baseline["dwt"]).sum() == pytest.approx(
                (df[var] * df["dwt"]).sum())

    def test_splits_actually_happened(self, synthetic_microdata):
        r = self._split_result(synthetic_microdata)
        assert r.diagnostics["paired_baseline"]["rows_added_by_splits"] > 0

    def test_scale_method_needs_no_pairing(self, synthetic_microdata):
        """scale_variables never adds rows, so its baseline stays shareable."""
        from euromod_linking.methods.scale_variables import ScaleVariables
        from euromod_linking import registry
        shocks = targets_for([{"channel": "scale", "metric": "yem", "group": "",
                               "period": "1", "op": "grow", "value": 0.03}])
        r = ScaleVariables().apply(synthetic_microdata, shocks, {}, CTX)
        assert r.baseline is None
        assert len(r.data) == len(synthetic_microdata)
        assert registry.resolve("scale_variables").restructures_rows is False
        assert registry.resolve("lma_labour_alignment").restructures_rows is True


@pytest.fixture()
def country_hours(monkeypatch):
    """Pin the country's full-time week, which the method resolves from the model."""
    def setter(hours):
        monkeypatch.setattr(
            "euromod_linking.methods.lma_labour_alignment.fulltime_hours",
            lambda cc, system_name: (hours, f"$lhw = {hours} h/week"))
    return setter


class TestFullTimeHoursLookup:
    """$lhw is read from the model, and only believed when it is a working week."""

    def _index(self, monkeypatch, value):
        from types import SimpleNamespace

        idx = SimpleNamespace(systems={"FR_2026": SimpleNamespace(
            constant_values={"$lhw": value} if value is not None else {})})
        monkeypatch.setattr("euromod_linking.query.get_country_index", lambda cc: idx)

    def test_uses_the_country_constant(self, monkeypatch):
        from euromod_linking.methods.lma_labour_alignment import fulltime_hours

        self._index(monkeypatch, "35")
        assert fulltime_hours("FR", "FR_2026") == (35.0, "$lhw = 35.0 h/week")

    def test_implausible_value_is_ignored(self, monkeypatch):
        """HR ships $lhw = 0, which would pay every new worker nothing."""
        from euromod_linking.methods.lma_labour_alignment import fulltime_hours

        self._index(monkeypatch, "0")
        hours, note = fulltime_hours("FR", "FR_2026")
        assert hours is None and "outside" in note

    def test_absent_constant_is_not_an_error(self, monkeypatch):
        from euromod_linking.methods.lma_labour_alignment import fulltime_hours

        self._index(monkeypatch, None)
        hours, note = fulltime_hours("FR", "FR_2026")
        assert hours is None and "not defined" in note


class TestFullTimeHours:
    """Full-time weekly hours come from the country's $lhw where it is sane."""

    def _shock(self):
        return targets_for([{"channel": "align", "metric": "employment", "group": "",
                             "period": "1", "op": "grow", "value": 0.08}])

    def test_country_hours_used_when_supplied(self, synthetic_microdata, country_hours):
        from euromod_linking.methods.lma_labour_alignment import WEEKS_PER_MONTH

        country_hours(35.0)
        r = LmaLabourAlignment().apply(synthetic_microdata, self._shock(), {"period": "1"}, CTX)
        out = r.data
        entrants = out[out["lma"] == 1]
        assert (entrants["lhw_a"] == 35.0).all()
        expected = 35.0 * WEEKS_PER_MONTH * entrants["yivwg"]
        pd.testing.assert_series_equal(entrants["yem_a"], expected, check_names=False)
        assert r.diagnostics["earnings"]["fulltime_hours_per_week"] == 35.0
        assert "$lhw" in r.diagnostics["earnings"]["fulltime_hours_source"]

    def test_falls_back_to_40_when_absent(self, synthetic_microdata):
        """No model reachable is the same as no $lhw: the default applies."""
        from euromod_linking.methods.lma_labour_alignment import DEFAULT_FULLTIME_HOURS_PER_WEEK

        r = LmaLabourAlignment().apply(synthetic_microdata, self._shock(), {"period": "1"}, CTX)
        e = r.diagnostics["earnings"]
        assert e["fulltime_hours_per_week"] == DEFAULT_FULLTIME_HOURS_PER_WEEK
        assert "default" in e["fulltime_hours_source"]

    def test_hours_scale_earnings_proportionally(self, synthetic_microdata, country_hours):
        country_hours(42.0)
        a = LmaLabourAlignment().apply(
            synthetic_microdata, self._shock(), {"period": "1"}, CTX)
        country_hours(38.0)
        b = LmaLabourAlignment().apply(
            synthetic_microdata, self._shock(), {"period": "1"}, CTX)
        ea = a.data.loc[a.data["lma"] == 1, "yem_a"].sum()
        eb = b.data.loc[b.data["lma"] == 1, "yem_a"].sum()
        assert ea / eb == pytest.approx(42.0 / 38.0, rel=1e-9)


class TestNegligibleShock:
    """`add` takes a count of people; a rate-sized value moves nobody."""

    def test_add_with_rate_sized_value_warns(self, synthetic_microdata):
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "",
             "period": "1", "op": "add", "value": 0.035},   # 0.035 PEOPLE
        ])
        result = LmaLabourAlignment().apply(synthetic_microdata, shocks, {"period": "1"}, CTX)
        warnings = result.diagnostics["warnings"]
        assert any("effectively nothing" in w for w in warnings), warnings
        assert any("'grow' applies a rate" in w for w in warnings)

    def test_real_shock_does_not_warn(self, synthetic_microdata):
        shocks = targets_for([
            {"channel": "align", "metric": "employment", "group": "",
             "period": "1", "op": "grow", "value": 0.035},
        ])
        result = LmaLabourAlignment().apply(synthetic_microdata, shocks, {"period": "1"}, CTX)
        assert not any("effectively nothing" in w
                       for w in result.diagnostics["warnings"])
