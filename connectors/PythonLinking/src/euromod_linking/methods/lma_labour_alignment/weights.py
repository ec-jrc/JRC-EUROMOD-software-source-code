"""Weight-split exact alignment: select individuals (ranked by a score) until a
weighted target is hit exactly, splitting the boundary household's sample
weight between a transitioning branch and a staying copy.

Economic intuition
------------------
Each survey row represents ``dwt`` real-world people (the grossing weight).
Macro targets are continuous, so after selecting whole persons the residual
gap is almost always a *fraction* of the next candidate's weight — say 40% of
the 5,000 people a boundary person represents should enter employment. Rather
than over- or under-shooting the target, the split represents exactly that:
one branch of the household (40% of its weight) where the person transitions,
one copy (60%) where they don't. The population total is unchanged — weight is
partitioned, never created.

Whole *households* are split, not individuals, because EUROMOD assesses taxes
and benefits on complete household contexts (couple assessments, child
benefits, household-level means tests). Splitting a person out of their
household would simulate them against a truncated family and mis-price their
taxes and benefits; copying the household keeps every branch fiscally
coherent, at the cost of a few duplicated rows with rescaled weights.

Two properties the split must preserve, both easy to get wrong:

1. A household copy does not inherit the boundary person's transition flag.
   The transitioning branch carries it, the staying copy does not — otherwise
   the boundary person transitions with their full weight and the target is
   overshot by exactly the fraction the split was meant to avoid.
2. When several boundary persons share one household, its weight is
   partitioned over all 2^k transition combinations (a product measure), so
   each boundary person's transitioning weight is exactly ``orig_weight *
   ratio`` and the total is conserved. Compounding the ratios onto a single
   copy conserves the total but gets the individual masses wrong.

Determinism: mergesort + idperson tie-break everywhere; new ids allocated in
sorted split order.
"""

import logging
from itertools import product

import pandas as pd

logger = logging.getLogger(__name__)

RELATION_COLS = ("idfather", "idmother", "idpartner")


def select_for_target(df: pd.DataFrame, candidates_idx: pd.Index, target_gap: float,
                      ascending: bool, prob_col: str) -> tuple[pd.Index, dict | None]:
    """Select candidates (sorted by prob_col, idperson tie-break) whose weights
    cumulate to target_gap. Returns (selected row index, boundary_info | None);
    boundary_info marks the last person, whose weight must be split.

    Walking the propensity ranking and taking cumulative *weighted* mass is
    the micro analogue of "the N most likely people transition": the weighted
    count of movers — not the row count — is what must match the macro target,
    since each row stands for dwt real people."""
    if target_gap <= 0 or len(candidates_idx) == 0:
        return pd.Index([]), None

    # Rank candidates by transition propensity (idperson breaks ties so the
    # ordering never depends on incidental row order).
    candidates = df.loc[candidates_idx, [prob_col, "idperson", "dwt", "idhh"]].sort_values(
        [prob_col, "idperson"], ascending=[ascending, True], kind="mergesort")

    # Walk down the ranking accumulating represented population until the
    # target mass is reached: everyone strictly before the crossing point
    # transitions with their full weight.
    cumsum = candidates["dwt"].cumsum()
    fully = cumsum < target_gap
    selected = list(candidates.index[fully])
    remaining = target_gap - (cumsum.iloc[fully.sum() - 1] if fully.sum() else 0.0)

    # The person AT the crossing point usually overshoots: only `remaining`
    # of the people they represent should transition. weight_ratio is that
    # fraction — apply_boundary_splits later partitions their household
    # accordingly. (remaining == their full weight -> clean full selection,
    # no split needed.)
    boundary_info = None
    if fully.sum() < len(candidates):
        b_idx = candidates.index[fully.sum()]
        b_weight = float(candidates.loc[b_idx, "dwt"])
        if 0 < remaining < b_weight:
            boundary_info = {
                "idx": b_idx,
                "household_id": int(candidates.loc[b_idx, "idhh"]),
                "person_id": int(candidates.loc[b_idx, "idperson"]),
                "weight_ratio": remaining / b_weight,
            }
            selected.append(b_idx)
        elif remaining >= b_weight:
            selected.append(b_idx)
    return pd.Index(selected), boundary_info


def _copy_household(hh: pd.DataFrame, new_idhh: int, next_idperson: int) -> tuple[pd.DataFrame, dict, int]:
    """One household copy with fresh ids and remapped relationship links.
    Returns (copy, old->new idperson mapping, next free idperson)."""
    copy = hh.copy()
    copy["idhh"] = new_idhh
    old_ids = copy["idperson"].astype(int).tolist()
    new_ids = list(range(next_idperson, next_idperson + len(old_ids)))
    id_map = dict(zip(old_ids, new_ids))
    copy["idperson"] = [id_map[i] for i in old_ids]
    for col in RELATION_COLS:
        if col in copy.columns:
            copy[col] = [id_map.get(int(v), 0) if pd.notna(v) and int(v) != 0 else 0
                         for v in copy[col]]
    return copy, id_map, new_ids[-1] + 1


def apply_boundary_splits(df: pd.DataFrame, boundary_infos: list[dict]) -> pd.DataFrame:
    """Apply all boundary splits. Each info must carry 'flag_col' — the
    transition-flag column its selection set (already 1 on the original row).

    For a household with k boundary splits, the original rows become the
    all-transition stratum (``weight *= prod(ratios)``) and 2^k - 1 copies cover
    the remaining combinations, with the flag zeroed for each staying boundary
    person. Total dwt is conserved to float precision.

    The 2^k enumeration is a product measure: with two boundary members whose
    transition shares are r1 and r2, the households this row represents divide
    into four subpopulations (both move, only one, only the other, neither)
    with weights ``r1*r2``, ``r1*(1-r2)``, ``(1-r1)*r2``, ``(1-r1)*(1-r2)`` —
    treating the two members' transitions as independent events. This is what
    makes *each* member's transitioning mass exactly ``w*r_i`` while household
    weight is only partitioned; compounding the ratios onto a single copy
    satisfies neither."""
    if not boundary_infos:
        return df

    by_household: dict[int, list[dict]] = {}
    for info in boundary_infos:
        by_household.setdefault(info["household_id"], []).append(info)

    total_before = float(df["dwt"].sum())
    next_idhh = int(df["idhh"].max()) + 1
    next_idperson = int(df["idperson"].max()) + 1
    new_frames = []

    for hh_id in sorted(by_household):
        splits = sorted(by_household[hh_id], key=lambda i: i["person_id"])
        if len(splits) > 6:
            raise ValueError(f"Household {hh_id} has {len(splits)} boundary splits (>6); "
                             "cannot enumerate strata")
        hh_mask = df["idhh"] == hh_id
        hh_rows = df.loc[hh_mask]
        # Snapshot of the pre-split weights: every stratum's weight is a
        # fraction of THESE, never of already-rescaled ones, or the ratios
        # compound and each member's transitioning mass comes out wrong.
        base_weights = hh_rows["dwt"].copy()

        # Enumerate every combination of "which boundary members transition".
        # Each combination is one stratum of the represented households, with
        # weight = product of the members' transition shares (product measure).
        for combo in product([True, False], repeat=len(splits)):
            factor = 1.0
            for s, transitions in zip(splits, combo):
                factor *= s["weight_ratio"] if transitions else (1.0 - s["weight_ratio"])
            if all(combo):
                # The all-transition stratum reuses the ORIGINAL rows (their
                # transition flags are already set); only the weight shrinks.
                df.loc[hh_mask, "dwt"] = base_weights * factor
                continue
            if factor == 0.0:
                continue
            # Every other stratum is a fresh household copy (new ids, remapped
            # family links) whose boundary members that DON'T transition in
            # this stratum get their inherited transition flag zeroed —
            # otherwise the "staying" branch would transition too and the
            # split would achieve nothing.
            copy, id_map, next_idperson = _copy_household(hh_rows, next_idhh, next_idperson)
            next_idhh += 1
            copy["dwt"] = base_weights.values * factor
            for s, transitions in zip(splits, combo):
                if not transitions:
                    person_pos = hh_rows.index.get_loc(s["idx"])
                    copy.iloc[person_pos, copy.columns.get_loc(s["flag_col"])] = 0
            new_frames.append(copy)

    if new_frames:
        df = pd.concat([df] + new_frames, ignore_index=True)

    total_after = float(df["dwt"].sum())
    if abs(total_after - total_before) > 1e-6 * max(1.0, abs(total_before)):
        raise AssertionError(
            f"Boundary splits changed total dwt: {total_before} -> {total_after}")
    return df
