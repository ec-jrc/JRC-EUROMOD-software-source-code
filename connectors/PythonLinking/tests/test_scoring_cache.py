"""Propensity-score caching: content addressing, correctness, invalidation.

The cache must be indistinguishable from refitting — identical inputs give
identical scores, and *any* relevant data change (or a scoring-version bump)
must miss rather than serve stale propensities.
"""

import numpy as np
import pandas as pd
import pytest

from euromod_linking import shock_table
from euromod_linking.methods.base import MethodContext
from euromod_linking.methods.lma_labour_alignment import LmaLabourAlignment, scoring, states


CTX = MethodContext(country_code="AT", system_name="AT_2024")


def scored_population(df: pd.DataFrame) -> pd.DataFrame:
    """The frame the method passes to scoring: states classified, working age."""
    les_var, codes = states.les_variable(df.columns)
    out = pd.concat([df, states.classify_labour_status(df[les_var], codes)], axis=1)
    return out[out["dag"].between(*states.WORKING_AGE)]


def target_shocks():
    return shock_table.normalize([
        {"channel": "align", "metric": "employment", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.02}])


class TestCacheBehaviour:
    def test_miss_then_hit(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        first, info1 = scoring.estimate_probabilities(pop)
        assert info1["cached"] is False
        second, info2 = scoring.estimate_probabilities(pop)
        assert info2["cached"] is True
        assert info2["key"] == info1["key"]
        # A hit must be numerically identical to the refit, not merely close.
        pd.testing.assert_frame_equal(first[list(scoring.PROB_COLUMNS)],
                                      second[list(scoring.PROB_COLUMNS)])

    def test_hit_survives_memory_eviction_via_disk(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        first, _ = scoring.estimate_probabilities(pop)
        scoring.clear_cache()  # in-memory only; disk entry remains
        second, info = scoring.estimate_probabilities(pop)
        assert info["cached"] is True
        pd.testing.assert_frame_equal(first[list(scoring.PROB_COLUMNS)],
                                      second[list(scoring.PROB_COLUMNS)])

    def test_disabled_by_env(self, synthetic_microdata, monkeypatch):
        pop = scored_population(synthetic_microdata)
        monkeypatch.setenv("EUROMOD_SCORE_CACHE", "0")
        _, info1 = scoring.estimate_probabilities(pop)
        _, info2 = scoring.estimate_probabilities(pop)
        assert info1["cached"] is False and info2["cached"] is False
        assert info1["key"] is None

    def test_use_cache_false(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        scoring.estimate_probabilities(pop)
        _, info = scoring.estimate_probabilities(pop, use_cache=False)
        assert info["cached"] is False


class TestInvalidation:
    def test_changed_feature_misses(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        _, info1 = scoring.estimate_probabilities(pop)
        changed = pop.copy()
        changed.loc[changed.index[0], "dag"] = int(changed["dag"].iloc[0]) + 1
        _, info2 = scoring.estimate_probabilities(changed)
        assert info2["cached"] is False
        assert info2["key"] != info1["key"]

    def test_changed_outcome_misses(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        _, info1 = scoring.estimate_probabilities(pop)
        changed = pop.copy()
        i = changed.index[0]
        changed.loc[i, "employed"] = 1 - int(changed.loc[i, "employed"])
        _, info2 = scoring.estimate_probabilities(changed)
        assert info2["cached"] is False
        assert info2["key"] != info1["key"]

    def test_different_row_subset_misses(self, synthetic_microdata):
        pop = scored_population(synthetic_microdata)
        _, info1 = scoring.estimate_probabilities(pop)
        _, info2 = scoring.estimate_probabilities(pop.iloc[:-1])
        assert info2["cached"] is False
        assert info2["key"] != info1["key"]

    def test_scoring_version_bump_invalidates(self, synthetic_microdata, monkeypatch):
        pop = scored_population(synthetic_microdata)
        _, info1 = scoring.estimate_probabilities(pop)
        monkeypatch.setattr(scoring, "SCORING_VERSION", scoring.SCORING_VERSION + 1)
        _, info2 = scoring.estimate_probabilities(pop)
        assert info2["cached"] is False
        assert info2["key"] != info1["key"]

    def test_irrelevant_column_change_still_hits(self, synthetic_microdata):
        """Columns the fit never reads must not force a refit."""
        pop = scored_population(synthetic_microdata)
        _, info1 = scoring.estimate_probabilities(pop)
        other = pop.copy()
        other["yem"] = other["yem"] * 2.0  # not a scoring input
        _, info2 = scoring.estimate_probabilities(other)
        assert info2["cached"] is True
        assert info2["key"] == info1["key"]


class TestMethodIntegration:
    def test_second_period_reuses_scores(self, synthetic_microdata):
        """A period sweep refits once: the shock changes, the population doesn't."""
        shocks = shock_table.normalize([
            {"channel": "align", "metric": "employment", "group": "deh=3-4",
             "period": p, "op": "grow", "value": v}
            for p, v in (("1", 0.02), ("2", 0.04))])
        first = LmaLabourAlignment().apply(synthetic_microdata, shocks, {"period": "1"}, CTX)
        second = LmaLabourAlignment().apply(synthetic_microdata, shocks, {"period": "2"}, CTX)
        assert first.diagnostics["scoring"]["cached"] is False
        assert second.diagnostics["scoring"]["cached"] is True
        assert second.diagnostics["scoring"]["key"] == first.diagnostics["scoring"]["key"]

    def test_cached_run_matches_uncached_run(self, synthetic_microdata, monkeypatch):
        """Caching must not change a single output value."""
        monkeypatch.setenv("EUROMOD_SCORE_CACHE", "0")
        uncached = LmaLabourAlignment().apply(synthetic_microdata, target_shocks(), {}, CTX)
        monkeypatch.setenv("EUROMOD_SCORE_CACHE", "1")
        LmaLabourAlignment().apply(synthetic_microdata, target_shocks(), {}, CTX)  # warm
        cached = LmaLabourAlignment().apply(synthetic_microdata, target_shocks(), {}, CTX)
        assert cached.diagnostics["scoring"]["cached"] is True
        pd.testing.assert_frame_equal(uncached.data, cached.data)
        assert uncached.diagnostics["targets"] == cached.diagnostics["targets"]

    def test_different_shocks_share_scores(self, synthetic_microdata):
        a = LmaLabourAlignment().apply(synthetic_microdata, target_shocks(), {}, CTX)
        other = shock_table.normalize([
            {"channel": "align", "metric": "unemployment", "group": "region=1",
             "period": "1", "op": "grow", "value": 0.10}])
        b = LmaLabourAlignment().apply(synthetic_microdata, other, {}, CTX)
        assert b.diagnostics["scoring"]["cached"] is True
        assert b.diagnostics["scoring"]["key"] == a.diagnostics["scoring"]["key"]
