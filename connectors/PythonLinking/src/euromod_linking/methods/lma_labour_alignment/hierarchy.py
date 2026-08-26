"""Two-level hierarchical alignment: activity (level 1), then unemployment
within the active population (level 2); employment is the residual.

Economic rationale for the hierarchy
------------------------------------
Labour-market outcomes are modelled as a two-stage decision, mirroring
standard labour-supply theory: first the *participation* margin (in or out of
the labour force), then the *employment* margin (employed vs unemployed,
conditional on participating). Aligning in that order guarantees the macro
accounting identities hold exactly at every cell::

    labour force  = employment target + unemployment target   (level 1)
    unemployment  = its own target within the labour force    (level 2)
    employment    = labour force - unemployment  (residual, never adjusted
                    directly — adjusting it independently could break the
                    identity and double-count transitions)

A one-shot alignment of employment and unemployment separately could hit both
targets while implying an impossible participation path (e.g. someone entering
employment straight from inactivity being counted as an unemployment exit).
The hierarchy makes every implied transition economically interpretable:
inactive -> active (participation entry), active non-unemployed -> unemployed
(job loss / unsuccessful entry), unemployed -> employed (residual job finding).

Who transitions is decided by ranked propensities (see scoring.py): people
whose observable characteristics make the destination state most likely move
first — the micro pattern a gradual macro adjustment would plausibly follow —
rather than by random draw, which would attribute transitions to
unrepresentative individuals and distort the distributional results.

Operates on the FULL dataset (households intact): candidate selection is
restricted to the eligible (working-age) mask, but boundary splits copy whole
households and rescale every member's weight, because taxes and benefits are
assessed on complete household contexts (see weights.py).
"""

import logging

import pandas as pd

from euromod_linking.methods.base import MethodError
from euromod_linking.methods.lma_labour_alignment import weights
from euromod_linking.methods.lma_labour_alignment.targets import CELL_COL

logger = logging.getLogger(__name__)

FLAG_COLS = ("transition_to_active", "transition_to_inactive",
             "transition_to_unemp", "transition_from_unemp")


def _cells_in_order(targets: pd.DataFrame):
    return list(targets.index)  # already sorted by groupby(sort=True)


def align_level1_activity(df: pd.DataFrame, targets: pd.DataFrame,
                          eligible: pd.Series) -> tuple[pd.DataFrame, list[dict]]:
    """Move people between inactive and active to close each cell's active_gap.

    A positive gap means the macro model projects labour-force growth in the
    cell: entrants are recruited from the *inactive* (domestic tasks, other —
    never students/retirees/disabled, who sit in the shielded 'other' state),
    highest predicted activity propensity first — the most labour-market-
    attached inactive individuals are the plausible marginal entrants. A
    negative gap withdraws active individuals with the *lowest* propensity —
    the weakest attachment leaves first (last-in-first-out intuition).
    """
    df["transition_to_active"] = 0
    df["transition_to_inactive"] = 0

    unfillable: list[dict] = []
    boundary_infos: list[dict] = []
    to_active: list = []
    to_inactive: list = []

    for cell in _cells_in_order(targets):
        # gap = target labour force minus current labour force in this cell
        # (weighted persons). NaN target = cell not shocked -> leave untouched.
        gap = float(targets.loc[cell, "active_gap"])
        if pd.isna(gap) or gap == 0:
            continue
        in_cell = (df[CELL_COL] == cell) & eligible
        if gap > 0:
            # Labour force must GROW: recruit from the inactive (the only
            # recruitable pool — 'other' is shielded), most attached first
            # (highest predicted activity propensity, hence ascending=False).
            candidates = df.index[in_cell & (df["inactive"] == 1)]
            available = float(df.loc[candidates, "dwt"].sum())
            if available < gap:
                # The macro projection wants more entrants than this cell has
                # inactive people. Take everyone available and report the
                # shortfall — a substantive scenario finding, not an error.
                unfillable.append({"cell": cell, "direction": "to_active", "gap": gap,
                                   "available": available, "shortfall": gap - available})
            sel, binfo = weights.select_for_target(df, candidates, gap,
                                                   ascending=False, prob_col="prob_active")
            to_active.extend(sel.tolist())
            if binfo:
                # Boundary person: only a fraction of their weight transitions.
                # Deferred — all splits are applied in one batch below.
                binfo["flag_col"] = "transition_to_active"
                boundary_infos.append(binfo)
        else:
            # Labour force must SHRINK: withdraw active people with the LOWEST
            # activity propensity first (weakest attachment exits first).
            candidates = df.index[in_cell & (df["active"] == 1)]
            available = float(df.loc[candidates, "dwt"].sum())
            if available < -gap:
                unfillable.append({"cell": cell, "direction": "to_inactive", "gap": -gap,
                                   "available": available, "shortfall": -gap - available})
            sel, binfo = weights.select_for_target(df, candidates, -gap,
                                                   ascending=True, prob_col="prob_active")
            to_inactive.extend(sel.tolist())
            if binfo:
                binfo["flag_col"] = "transition_to_inactive"
                boundary_infos.append(binfo)

    # Flags first, splits second: apply_boundary_splits reads the flags to
    # zero them on the "staying" household copies, so the split person's
    # transitioning weight is exactly the fractional remainder.
    if to_active:
        df.loc[to_active, "transition_to_active"] = 1
    if to_inactive:
        df.loc[to_inactive, "transition_to_inactive"] = 1
    df = weights.apply_boundary_splits(df, boundary_infos)

    # Post-alignment participation state: start from the observed state and
    # overlay the transitions. 'other' (students, retirees, disabled) is
    # forced out of both labour-force states regardless — they were never
    # candidates and must not be counted into the labour force.
    df["new_active"] = df["active"]
    df.loc[df["transition_to_active"] == 1, "new_active"] = 1
    df.loc[df["transition_to_inactive"] == 1, "new_active"] = 0
    df["new_inactive"] = df["inactive"]
    df.loc[df["transition_to_inactive"] == 1, "new_inactive"] = 1
    df.loc[df["transition_to_active"] == 1, "new_inactive"] = 0
    df.loc[df["other"] == 1, ["new_active", "new_inactive"]] = 0

    return df, unfillable


def align_level2_employment(df: pd.DataFrame, targets: pd.DataFrame,
                            eligible: pd.Series) -> tuple[pd.DataFrame, list[dict]]:
    """Within the (new) active population, close each cell's unemp_gap;
    employment is the residual state.

    Rising unemployment recruits from active non-unemployed ranked by
    unemployment propensity (highest first: observably most at risk of job
    loss); falling unemployment releases the unemployed with the *lowest*
    propensity first (observably most employable — the plausible first hires
    in an upswing). Employment is then defined as active-and-not-unemployed,
    so the participation decided at level 1 and the unemployment decided here
    pin down employment by identity — it is never adjusted directly.
    """
    df["transition_to_unemp"] = 0
    df["transition_from_unemp"] = 0

    unfillable: list[dict] = []
    boundary_infos: list[dict] = []
    to_unemp: list = []
    from_unemp: list = []

    for cell in _cells_in_order(targets):
        # gap = unemployment target minus current unemployment (weighted).
        # Note the gap was computed against the PRE-alignment population;
        # level 1 only moved people across the participation margin, so
        # current unemployment counts are still valid here.
        gap = float(targets.loc[cell, "unemp_gap"])
        if pd.isna(gap) or gap == 0:
            continue
        in_cell = (df[CELL_COL] == cell) & eligible
        if gap > 0:
            # Unemployment must RISE: recruit among the (post-level-1) active
            # who are not already unemployed — i.e. incumbents losing their
            # job and fresh labour-force entrants failing to find one —
            # highest unemployment propensity first (observably most at risk).
            candidates = df.index[in_cell & (df["new_active"] == 1) & (df["unemployed"] == 0)]
            available = float(df.loc[candidates, "dwt"].sum())
            if available < gap:
                unfillable.append({"cell": cell, "direction": "to_unemp", "gap": gap,
                                   "available": available, "shortfall": gap - available})
            sel, binfo = weights.select_for_target(df, candidates, gap,
                                                   ascending=False, prob_col="prob_unemployed")
            to_unemp.extend(sel.tolist())
            if binfo:
                binfo["flag_col"] = "transition_to_unemp"
                boundary_infos.append(binfo)
        else:
            # Unemployment must FALL: the unemployed with the LOWEST
            # unemployment propensity leave first (observably most
            # employable — the plausible first hires in an upswing). Only
            # those still active after level 1 qualify; leaving the labour
            # force entirely was level 1's decision, not a job finding.
            candidates = df.index[in_cell & (df["unemployed"] == 1) & (df["new_active"] == 1)]
            available = float(df.loc[candidates, "dwt"].sum())
            if available < -gap:
                unfillable.append({"cell": cell, "direction": "from_unemp", "gap": -gap,
                                   "available": available, "shortfall": -gap - available})
            sel, binfo = weights.select_for_target(df, candidates, -gap,
                                                   ascending=True, prob_col="prob_unemployed")
            from_unemp.extend(sel.tolist())
            if binfo:
                binfo["flag_col"] = "transition_from_unemp"
                boundary_infos.append(binfo)

    # Same flags-then-splits order as level 1 (splits zero the flags on the
    # staying household copies). Households already split at level 1 can be
    # split again here — the machinery is generic over flag columns.
    if to_unemp:
        df.loc[to_unemp, "transition_to_unemp"] = 1
    if from_unemp:
        df.loc[from_unemp, "transition_from_unemp"] = 1
    df = weights.apply_boundary_splits(df, boundary_infos)

    # Post-alignment unemployment: observed state + transitions; anyone who
    # is out of the labour force (inactive after level 1, or shielded
    # 'other') cannot be unemployed by definition.
    df["new_unemployed"] = df["unemployed"]
    df.loc[df["transition_to_unemp"] == 1, "new_unemployed"] = 1
    df.loc[df["transition_from_unemp"] == 1, "new_unemployed"] = 0
    df.loc[df["other"] == 1, "new_unemployed"] = 0
    df.loc[df["new_inactive"] == 1, "new_unemployed"] = 0

    # Employment is never set directly: it is the residual of the accounting
    # identity (active and not unemployed), which is what guarantees
    # employment + unemployment = labour force in every cell.
    df["new_employed"] = 0
    df.loc[(df["new_active"] == 1) & (df["new_unemployed"] == 0), "new_employed"] = 1
    df.loc[df["other"] == 1, "new_employed"] = 0
    df.loc[df["new_inactive"] == 1, "new_employed"] = 0

    return df, unfillable


def validate_states(df: pd.DataFrame) -> None:
    """Internal consistency of the post-alignment states; raises MethodError."""
    state_sum = df["new_employed"] + df["new_unemployed"] + df["new_inactive"] + df["other"]
    if not (state_sum == 1).all():
        bad = df[state_sum != 1]
        raise MethodError(f"{len(bad)} rows not in exactly one labour state "
                          f"(sum distribution: {state_sum.value_counts().to_dict()})")
    if not (df["new_active"] == df["new_employed"] + df["new_unemployed"]).all():
        raise MethodError("new_active inconsistent with new_employed + new_unemployed")
    for col in ("new_employed", "new_unemployed", "new_inactive", "new_active"):
        if not df[col].isin([0, 1]).all():
            raise MethodError(f"{col} contains non-binary values")


# Alignment-quality ladder (per-cell |achieved - target| as % of target).
# Anything above "exact" means the cell ran short of transition candidates:
# the macro projection asked for more movement than the surveyed population in
# that cell can supply — a substantive finding about the scenario (reported in
# unfillable_gaps), not a numerical failure of the algorithm.
GRADE_LADDER = (("exact", 0.01), ("excellent", 1.0), ("good", 2.0),
                ("acceptable", 5.0), ("poor", 10.0))


def _grade(error_pct: float) -> str:
    if pd.isna(error_pct):
        return "n/a"
    for name, bound in GRADE_LADDER:
        if abs(error_pct) <= bound:
            return name
    return "failed"


def accuracy(df: pd.DataFrame, targets: pd.DataFrame, eligible: pd.Series) -> list[dict]:
    """Per-cell achieved vs target with tolerance grades.

    Carries the *baseline* each target was built from. Without it a grade is
    uninterpretable: "exact" only ever means the alignment hit the number it was
    handed, and a target that silently equals the baseline grades exactly the
    same as one that moved a hundred thousand people.
    """
    e = df[eligible]
    achieved = (e.assign(_e=e["new_employed"] * e["dwt"], _u=e["new_unemployed"] * e["dwt"])
                .groupby(CELL_COL, sort=True)[["_e", "_u"]].sum())
    out = []
    for cell in targets.index:
        for metric, tcol, ccol, acol in (("employment", "emp_target", "current_emp", "_e"),
                                         ("unemployment", "unemp_target", "current_unemp", "_u")):
            target = float(targets.loc[cell, tcol])
            baseline = float(targets.loc[cell, ccol])
            got = float(achieved.loc[cell, acol]) if cell in achieved.index else 0.0
            err = (got - target) / target * 100 if target > 0 else float("nan")
            out.append({"cell": cell, "metric": metric,
                        "baseline": round(baseline, 2), "target": round(target, 2),
                        "requested_change": round(target - baseline, 2),
                        "requested_change_pct": (round((target - baseline) / baseline * 100, 4)
                                                 if baseline > 0 else None),
                        "achieved": round(got, 2),
                        "error_pct": None if pd.isna(err) else round(err, 4),
                        "grade": _grade(err)})
    return out
