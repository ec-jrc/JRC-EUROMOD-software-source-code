"""Weight-split exact alignment: selection, boundary splits (incl. the
multi-split fix), id integrity, weight conservation, determinism."""

import numpy as np
import pandas as pd
import pytest

from euromod_linking.methods.lma_labour_alignment import weights


def small_df():
    """3 households, 6 persons. Weights chosen for hand-computable targets."""
    return pd.DataFrame({
        "idhh":     [1,   1,   2,   2,   3,   3],
        "idperson": [1,   2,   3,   4,   5,   6],
        "idfather": [0,   0,   0,   0,   0,   0],
        "idmother": [0,   0,   0,   0,   0,   0],
        "idpartner":[2,   1,   4,   3,   6,   5],
        "dwt":      [10., 10., 20., 20., 40., 40.],
        "prob":     [0.9, 0.8, 0.7, 0.6, 0.5, 0.4],
        "flag":     [0,   0,   0,   0,   0,   0],
    })


class TestSelectForTarget:
    def test_exact_integer_target(self):
        df = small_df()
        sel, binfo = weights.select_for_target(df, df.index, 30.0, ascending=False, prob_col="prob")
        # Sorted desc by prob: p1(10) + p2(10) + p3(20) -> cumsum 10,20,40.
        # 30 falls inside p3 -> p1, p2 full + p3 split at ratio 0.5
        assert list(df.loc[sel, "idperson"]) == [1, 2, 3]
        assert binfo["person_id"] == 3
        assert binfo["weight_ratio"] == pytest.approx(0.5)

    def test_no_split_when_exact(self):
        df = small_df()
        sel, binfo = weights.select_for_target(df, df.index, 20.0, ascending=False, prob_col="prob")
        assert list(df.loc[sel, "idperson"]) == [1, 2]
        assert binfo is None

    def test_zero_or_empty(self):
        df = small_df()
        assert len(weights.select_for_target(df, df.index, 0.0, True, "prob")[0]) == 0
        assert len(weights.select_for_target(df, pd.Index([]), 5.0, True, "prob")[0]) == 0

    def test_tie_break_by_idperson(self):
        df = small_df()
        df["prob"] = 0.5  # all tied
        sel, _ = weights.select_for_target(df, df.index, 15.0, ascending=True, prob_col="prob")
        assert list(df.loc[sel, "idperson"]) == [1, 2]  # idperson order on ties

    def test_determinism_under_row_shuffle(self):
        df = small_df()
        shuffled = df.sample(frac=1.0, random_state=7)
        a, _ = weights.select_for_target(df, df.index, 55.0, ascending=False, prob_col="prob")
        b, _ = weights.select_for_target(shuffled, shuffled.index, 55.0, ascending=False, prob_col="prob")
        assert sorted(df.loc[a, "idperson"]) == sorted(shuffled.loc[b, "idperson"])


class TestBoundarySplits:
    def test_single_split(self):
        df = small_df()
        # Person 3 (hh 2, weight 20) transitions with ratio 0.6
        df.loc[df["idperson"] == 3, "flag"] = 1
        info = {"idx": 2, "household_id": 2, "person_id": 3, "weight_ratio": 0.6,
                "flag_col": "flag"}
        out = weights.apply_boundary_splits(df.copy(), [info])

        assert out["dwt"].sum() == pytest.approx(df["dwt"].sum())
        # Original hh 2 members rescaled to 0.6
        orig = out[out["idhh"] == 2]
        assert list(orig["dwt"]) == pytest.approx([12.0, 12.0])
        # One copy household with 0.4 x weights, flag zeroed for the boundary person
        copy = out[out["idhh"] == 4]
        assert len(copy) == 2
        assert list(copy["dwt"]) == pytest.approx([8.0, 8.0])
        assert copy["flag"].sum() == 0
        # Transitioning weight of person 3 == 20 * 0.6 exactly
        w_flag = (out["flag"] * out["dwt"]).sum()
        assert w_flag == pytest.approx(12.0)

    def test_id_integrity(self):
        df = small_df()
        df.loc[df["idperson"] == 3, "flag"] = 1
        out = weights.apply_boundary_splits(
            df.copy(), [{"idx": 2, "household_id": 2, "person_id": 3,
                         "weight_ratio": 0.5, "flag_col": "flag"}])
        assert out["idperson"].is_unique
        assert out["idhh"].nunique() == 4
        copy = out[out["idhh"] == 4].sort_values("idperson")
        # partners remapped within the copy, not pointing at the original household
        p = list(copy["idperson"])
        assert list(copy["idpartner"]) == [p[1], p[0]]

    def test_multi_split_same_household(self):
        """Two boundary persons in one household: each person's transitioning
        weight must equal w * their own ratio, not the product of both ratios."""
        df = small_df()
        df["flag2"] = 0
        df.loc[df["idperson"] == 5, "flag"] = 1   # ratio 0.25
        df.loc[df["idperson"] == 6, "flag2"] = 1  # ratio 0.5
        infos = [
            {"idx": 4, "household_id": 3, "person_id": 5, "weight_ratio": 0.25, "flag_col": "flag"},
            {"idx": 5, "household_id": 3, "person_id": 6, "weight_ratio": 0.5, "flag_col": "flag2"},
        ]
        out = weights.apply_boundary_splits(df.copy(), infos)

        assert out["dwt"].sum() == pytest.approx(df["dwt"].sum())
        # 2^2 strata: original + 3 copies of household 3
        assert out["idhh"].nunique() == 3 + 3
        p5 = out[out["flag"] == 1]
        p6 = out[out["flag2"] == 1]
        assert (p5["dwt"]).sum() == pytest.approx(40 * 0.25)
        assert (p6["dwt"]).sum() == pytest.approx(40 * 0.5)

    def test_no_infos_noop(self):
        df = small_df()
        out = weights.apply_boundary_splits(df.copy(), [])
        pd.testing.assert_frame_equal(out, df)

    def test_deterministic_output(self):
        df = small_df()
        df.loc[df["idperson"].isin([3, 5]), "flag"] = 1
        infos = [
            {"idx": 4, "household_id": 3, "person_id": 5, "weight_ratio": 0.3, "flag_col": "flag"},
            {"idx": 2, "household_id": 2, "person_id": 3, "weight_ratio": 0.7, "flag_col": "flag"},
        ]
        a = weights.apply_boundary_splits(df.copy(), list(infos))
        b = weights.apply_boundary_splits(df.copy(), list(reversed(infos)))
        pd.testing.assert_frame_equal(a, b)
