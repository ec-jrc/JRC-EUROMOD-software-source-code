"""The diagnostic contract that makes a mis-sized shock visible.

Every test here exists because a shock derived from the wrong population base
ran three times without anything noticing: the alignment hit the target it was
handed, graded itself "exact", and reported movers as a row count next to
population-level euro aggregates. What follows pins the numbers that would have
exposed it.
"""

import pandas as pd
import pytest

from euromod_linking import shock_table
from euromod_linking.methods.base import MethodContext
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment
from euromod_linking.methods.lma_labour_alignment import states, targets as targets_mod
import euromod_linking.methods  # noqa: F401


CTX = MethodContext(country_code="AT", system_name="AT_2024")
GROW_5PCT = [{"channel": "align", "metric": "employment", "group": "deh=3-4",
              "period": "1", "op": "grow", "value": 0.05}]


def run(df, records=None):
    return LmaLabourAlignment().apply(
        df, shock_table.normalize(records or GROW_5PCT), {"period": "1"}, CTX)


def preview(df, records=None):
    return LmaLabourAlignment().preview(
        df, shock_table.normalize(records or GROW_5PCT), {"period": "1"}, CTX)


class TestBaselineIsReported:
    def test_targets_carry_the_baseline_they_were_built_from(self, synthetic_microdata):
        rows = run(synthetic_microdata).diagnostics["targets"]
        for r in rows:
            assert "baseline" in r, r
            assert r["requested_change"] == pytest.approx(r["target"] - r["baseline"], abs=0.02)

    def test_shocked_cell_shows_the_growth_rate_it_was_given(self, synthetic_microdata):
        rows = run(synthetic_microdata).diagnostics["targets"]
        shocked = [r for r in rows if r["cell"] == "deh=3-4" and r["metric"] == "employment"]
        assert shocked, rows
        assert shocked[0]["requested_change_pct"] == pytest.approx(5.0, abs=0.01)

    def test_unshocked_metric_is_distinguishable_from_a_real_alignment(self, synthetic_microdata):
        """The failure that made 'exact' meaningless: a target equal to its own
        baseline graded exactly like one that moved a hundred thousand people."""
        rows = run(synthetic_microdata).diagnostics["targets"]
        unemp = [r for r in rows if r["cell"] == "deh=3-4" and r["metric"] == "unemployment"][0]
        assert unemp["grade"] == "exact"
        assert unemp["requested_change"] == pytest.approx(0.0, abs=0.02)


class TestCellReport:
    def test_states_account_for_the_whole_working_age_population(self, synthetic_microdata):
        for cell in run(synthetic_microdata).diagnostics["cells"]:
            b = cell["baseline"]
            assert (b["employed"] + b["unemployed"] + b["inactive_recruitable"]
                    + b["other_shielded"]) == pytest.approx(b["working_age_pop"], abs=0.05)

    def test_inactivity_is_the_complement_of_participation(self, synthetic_microdata):
        """Conventional definition: everyone of working age who is neither
        employed nor unemployed — wider than the recruitable pool."""
        for cell in run(synthetic_microdata).diagnostics["cells"]:
            b = cell["baseline"]
            assert b["participation_rate"] + b["inactivity_rate"] == pytest.approx(1.0, abs=1e-6)
            assert b["inactive_recruitable"] <= (b["inactivity_rate"] * b["working_age_pop"]) + 0.05

    def test_change_is_reported_in_percentage_points(self, synthetic_microdata):
        """The field that catches the failure: the request was phrased in pp,
        the shock in a growth rate, and nothing converted between them."""
        cells = {c["cell"]: c for c in run(synthetic_microdata).diagnostics["cells"]}
        c = cells["deh=3-4"]
        pp = c["change"]["participation_rate_pp"]
        assert pp == pytest.approx(-c["change"]["inactivity_rate_pp"], abs=1e-6)
        expected = c["change"]["people_moved"] / c["baseline"]["working_age_pop"] * 100
        assert pp == pytest.approx(expected, abs=1e-3)

    def test_denominator_is_invariant_under_the_alignment(self, synthetic_microdata):
        """Before/after rates are only comparable because working_age_pop and
        the shielded 'other' state do not move."""
        for cell in run(synthetic_microdata).diagnostics["cells"]:
            assert cell["target"]["employment_rate"] is not None
            b, t = cell["baseline"], cell["target"]
            assert (t["participation_rate"] - b["participation_rate"]) == pytest.approx(
                cell["change"]["participation_rate_pp"] / 100, abs=1e-6)


class TestShockedFlag:
    def test_only_the_shocked_cell_is_flagged(self, synthetic_microdata):
        """The residual cell is reported too (it evidences that the rest of the
        population was held constant) and is trivially easy to read by mistake."""
        cells = {c["cell"]: c for c in run(synthetic_microdata).diagnostics["cells"]}
        assert cells["deh=3-4"]["shocked"] is True
        others = [c for k, c in cells.items() if k != "deh=3-4"]
        assert others, "expected a residual cell"
        assert all(c["shocked"] is False for c in others)
        assert all(c["change"]["people_moved"] == 0 for c in others)


class TestFeasibility:
    def test_pool_is_the_recruitable_inactive_when_activating(self, synthetic_microdata):
        cells = {c["cell"]: c for c in run(synthetic_microdata).diagnostics["cells"]}
        f = cells["deh=3-4"]["feasibility"]
        assert f["direction"] == "inactive->active"
        assert f["pool"] == pytest.approx(cells["deh=3-4"]["baseline"]["inactive_recruitable"])
        assert 0 < f["pool_used_pct"] <= 100

    def test_oversized_shock_is_flagged_infeasible_with_the_largest_that_fits(
            self, synthetic_microdata):
        cells = preview(synthetic_microdata, [
            {"channel": "align", "metric": "employment", "group": "deh=3-4",
             "period": "1", "op": "grow", "value": 5.0},   # +500%
        ])["cells"]
        c = {x["cell"]: x for x in cells}["deh=3-4"]
        assert c["feasibility"]["severity"] == "infeasible"
        assert c["feasibility"]["pool_used_pct"] > 100
        assert c["feasibility"]["max_feasible_change"]["people"] > 0

    def test_modest_shock_is_silent(self, synthetic_microdata):
        cells = preview(synthetic_microdata, [
            {"channel": "align", "metric": "employment", "group": "deh=3-4",
             "period": "1", "op": "grow", "value": 0.005},
        ])["cells"]
        c = {x["cell"]: x for x in cells}["deh=3-4"]
        assert c["feasibility"]["severity"] == "low"
        assert not targets_mod.feasibility_warnings(cells)


class TestWeightedMovers:
    def test_transitions_report_people_and_rows(self, synthetic_microdata):
        t = run(synthetic_microdata).diagnostics["transitions"]["to_employment_lma1"]
        assert set(t) == {"people", "rows"}
        assert t["people"] > t["rows"], "weights are >1, so people must exceed rows"

    def test_people_match_the_weighted_movers_and_the_employment_change(
            self, synthetic_microdata):
        result = run(synthetic_microdata)
        out, diag = result.data, result.diagnostics
        moved = float(out.loc[out["lma"] == 1, "dwt"].sum())
        assert diag["transitions"]["to_employment_lma1"]["people"] == pytest.approx(moved, abs=0.05)
        assert moved == pytest.approx(diag["totals"]["employment_change"], abs=1.0)

    def test_the_transform_is_visible_in_the_data_not_just_the_diagnostics(
            self, synthetic_microdata):
        """What arms the no-effect guard is the input actually changing.

        The guard compares frames rather than reading diagnostics, so no
        reshaping of a method's own diagnostic keys can silently disarm it —
        and a method whose diagnostics went missing entirely would still be
        caught."""
        from euromod_linking.runner import frames_identical

        result = run(synthetic_microdata)
        assert not frames_identical(synthetic_microdata, result.data)
        assert result.baseline is not None                    # restructures rows
        assert not frames_identical(result.baseline, result.data)


class TestPreviewMatchesApply:
    def test_preview_cells_equal_apply_cells(self, synthetic_microdata):
        """The anti-drift test. A preview computed a second way could disagree
        with the run it previews — which is the exact failure being fixed."""
        assert preview(synthetic_microdata)["cells"] == run(synthetic_microdata).diagnostics["cells"]

    def test_preview_does_not_align_or_fit(self, synthetic_microdata):
        p = preview(synthetic_microdata)
        assert "transitions" not in p and "scoring" not in p
        assert p["totals"]["employment_change"] > 0

    def test_preview_publishes_the_population_definition(self, synthetic_microdata):
        d = preview(synthetic_microdata)["population_definition"]
        assert d["states"]["employed"] == sorted(states.LES2_CODES["employed"])
        assert d["movable"] == ["inactive"]
        assert d["working_age"]["min"] == states.WORKING_AGE[0]


class TestScoringCacheUnaffected:
    def test_cell_columns_do_not_change_the_scoring_fingerprint(self, synthetic_microdata):
        """apply() now scores after the cell columns are added; the score cache
        key must not move, or every stored scenario silently refits."""
        from euromod_linking.methods.lma_labour_alignment import scoring
        df = synthetic_microdata.copy()
        status = states.classify_labour_status(df["les2"], states.LES2_CODES)
        df = pd.concat([df, status], axis=1)
        before = scoring.fingerprint(df)
        with_cells, _ = targets_mod.prepare_cells(df, {"deh": ["3-4"]})
        assert scoring.fingerprint(with_cells) == before
