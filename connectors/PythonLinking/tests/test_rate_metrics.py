"""Rate metrics: express a request in the unit it arrived in.

Every one of these exists because converting a rate into a level by hand, across
the tool boundary, is how a shock silently ends up the wrong size. `add` on a
*_rate metric is a percentage-point change written as a fraction, so
"inactivity down 3.5pp" is a shock record rather than an arithmetic problem.
"""

import pandas as pd
import pytest

from euromod_linking import shock_table
from euromod_linking.methods.base import MethodContext, MethodError
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment
from euromod_linking.methods.lma_labour_alignment import targets as targets_mod
import euromod_linking.methods  # noqa: F401


CTX = MethodContext(country_code="AT", system_name="AT_2024")


def shock(metric, op, value, group="deh=3-4"):
    return [{"channel": "align", "metric": metric, "group": group,
             "period": "1", "op": op, "value": value}]


def preview(df, records):
    return LmaLabourAlignment().preview(
        df, shock_table.normalize(records), {"period": "1"}, CTX)


def cell_of(df, records, name="deh=3-4"):
    return {c["cell"]: c for c in preview(df, records)["cells"]}[name]


class TestPercentagePointSemantics:
    def test_add_on_inactivity_rate_is_a_pp_change(self, synthetic_microdata):
        c = cell_of(synthetic_microdata, shock("inactivity_rate", "add", -0.035))
        assert c["change"]["inactivity_rate_pp"] == pytest.approx(-3.5, abs=1e-6)
        assert c["change"]["participation_rate_pp"] == pytest.approx(3.5, abs=1e-6)

    def test_it_moves_exactly_that_share_of_working_age(self, synthetic_microdata):
        c = cell_of(synthetic_microdata, shock("inactivity_rate", "add", -0.035))
        assert c["change"]["people_moved"] == pytest.approx(
            0.035 * c["baseline"]["working_age_pop"], abs=0.05)

    def test_participation_is_the_mirror_of_inactivity(self, synthetic_microdata):
        up = cell_of(synthetic_microdata, shock("participation_rate", "add", 0.035))
        down = cell_of(synthetic_microdata, shock("inactivity_rate", "add", -0.035))
        assert up["change"] == down["change"]

    def test_set_pins_the_rate_outright(self, synthetic_microdata):
        c = cell_of(synthetic_microdata, shock("participation_rate", "set", 0.75))
        assert c["target"]["participation_rate"] == pytest.approx(0.75, abs=1e-6)

    def test_inactivity_rate_is_the_conventional_complement(self, synthetic_microdata):
        """Wider than the movable pool: students and pensioners are inactive by
        any published definition, and are never moved."""
        c = cell_of(synthetic_microdata, shock("inactivity_rate", "add", -0.01))
        b = c["baseline"]
        assert b["inactivity_rate"] + b["participation_rate"] == pytest.approx(1.0, abs=1e-9)
        conventional = b["inactivity_rate"] * b["working_age_pop"]
        assert conventional == pytest.approx(b["inactive_recruitable"] + b["other_shielded"],
                                             abs=0.05)
        assert b["inactive_recruitable"] < conventional


class TestRateAndLevelAgree:
    def test_employment_rate_matches_the_equivalent_level_shock(self, synthetic_microdata):
        by_level = cell_of(synthetic_microdata, shock("employment", "grow", 0.05))
        target = by_level["target"]["employed"]
        pop = by_level["baseline"]["working_age_pop"]
        by_rate = cell_of(synthetic_microdata, shock("employment_rate", "set", target / pop))
        assert by_rate["target"]["employed"] == pytest.approx(target, abs=0.05)

    def test_level_only_behaviour_is_unchanged(self, synthetic_microdata):
        """The three-step resolution must be the old emp+unemp identity when no
        activity-margin metric is used."""
        c = cell_of(synthetic_microdata, shock("employment", "grow", 0.05))
        b, t = c["baseline"], c["target"]
        assert t["employed"] == pytest.approx(b["employed"] * 1.05, abs=0.05)
        assert t["unemployed"] == pytest.approx(b["unemployed"], abs=0.05)


class TestResolutionAlgebra:
    def test_employment_is_the_residual_of_the_identity(self, synthetic_microdata):
        for records in (shock("employment", "grow", 0.05),
                        shock("unemployment", "grow", -0.10),
                        shock("inactivity_rate", "add", -0.02),
                        shock("participation_rate", "grow", 0.03),
                        shock("employment_rate", "add", 0.01)):
            c = cell_of(synthetic_microdata, records)
            pop = c["baseline"]["working_age_pop"]
            active = c["target"]["participation_rate"] * pop
            assert c["target"]["employed"] == pytest.approx(
                active - c["target"]["unemployed"], abs=0.05), records

    def test_activity_plus_unemployment_is_the_useful_pairing(self, synthetic_microdata):
        """Fixing the labour force and unemployment leaves employment determined."""
        c = cell_of(synthetic_microdata,
                    shock("inactivity_rate", "add", -0.03) + shock("unemployment", "grow", -0.10))
        b, t = c["baseline"], c["target"]
        assert c["change"]["inactivity_rate_pp"] == pytest.approx(-3.0, abs=1e-6)
        assert t["unemployed"] == pytest.approx(b["unemployed"] * 0.90, abs=0.05)

    def test_activity_and_employment_together_are_rejected(self, synthetic_microdata):
        """Both would also fix unemployment, which defaults to its current level —
        so one of the three would be silently discarded."""
        with pytest.raises(MethodError, match="over-determined"):
            preview(synthetic_microdata,
                    shock("inactivity_rate", "add", -0.03) + shock("employment", "grow", 0.05))


class TestGuards:
    def test_rate_outside_zero_one_is_rejected(self, synthetic_microdata):
        with pytest.raises(MethodError, match=r"outside \[0, 1\]"):
            preview(synthetic_microdata, shock("inactivity_rate", "add", -3.5))

    def test_the_rejection_says_how_to_write_it(self, synthetic_microdata):
        with pytest.raises(MethodError, match="3.5pp is 0.035"):
            preview(synthetic_microdata, shock("participation_rate", "add", 3.5))

    def test_percent_sized_add_warns_even_when_it_stays_in_range(self, synthetic_microdata):
        """Out of range this is caught outright; in range it is merely enormous,
        so it warns rather than blocks (baseline here is ~0.23, so +0.6 fits)."""
        w = preview(synthetic_microdata, shock("inactivity_rate", "add", 0.6))["warnings"]
        assert any("percentage-point change" in x for x in w), w

    def test_negligible_level_shock_still_warns(self, synthetic_microdata):
        """`add 0.035` on a LEVEL is 0.035 people — the original units trap."""
        w = preview(synthetic_microdata, shock("employment", "add", 0.035))["warnings"]
        assert any("effectively nothing" in x for x in w), w
        assert any("*_rate metric" in x for x in w), w


class TestRegionCollapseIsMetricAware:
    def _collapse(self, metric, op):
        from euromod_linking.methods.cells import collapse_shock_groups
        shocks = shock_table.normalize([
            {"channel": "align", "metric": metric, "group": "region=11",
             "period": "1", "op": op, "value": -0.035},
            {"channel": "align", "metric": metric, "group": "region=12",
             "period": "1", "op": op, "value": -0.035},
        ])
        out, _ = collapse_shock_groups(shocks, {"region": ["11", "12"]}, region_len=1,
                                       intensive_metrics=targets_mod.RATE_METRICS)
        return float(out["value"].iloc[0])

    def test_pp_shocks_average_rather_than_sum(self):
        """Two NUTS-2 shocks of -3.5pp are -3.5pp in the parent, not -7pp.
        Summing them would silently double the shock."""
        assert self._collapse("inactivity_rate", "add") == pytest.approx(-0.035)

    def test_level_shocks_still_sum(self):
        """Counts of people are extensive: subregional amounts add up."""
        assert self._collapse("employment", "add") == pytest.approx(-0.07)


class TestAlignmentHitsRateTargets:
    def test_rate_target_is_hit_exactly(self, synthetic_microdata):
        result = LmaLabourAlignment().apply(
            synthetic_microdata, shock_table.normalize(shock("inactivity_rate", "add", -0.02)),
            {"period": "1"}, CTX)
        rows = [t for t in result.diagnostics["targets"]
                if t["cell"] == "deh=3-4" and t["metric"] == "employment"]
        assert rows and rows[0]["grade"] == "exact", rows

    def test_movers_match_the_requested_share(self, synthetic_microdata):
        result = LmaLabourAlignment().apply(
            synthetic_microdata, shock_table.normalize(shock("inactivity_rate", "add", -0.02)),
            {"period": "1"}, CTX)
        cell = {c["cell"]: c for c in result.diagnostics["cells"]}["deh=3-4"]
        assert result.diagnostics["transitions"]["to_active"]["people"] == pytest.approx(
            0.02 * cell["baseline"]["working_age_pop"], abs=1.0)
