"""Target construction: shock-table rows -> per-cell weighted targets.

Cells are built from whatever dimensions appear in the shock groups: region x
education, age x gender, or any other combination of input variables works
identically. The region dimension is collapsed to the level the dataset
actually supports (drgn2 -> drgn1 -> country) by truncating shock region codes
to the data's code length and averaging growth-type shock values within the
collapsed cell — an equal-weight average, since the microdata by definition
cannot weight a level it does not resolve.
"""

import logging

import pandas as pd

from euromod_linking.methods.base import MethodError  # noqa: F401  (re-export for callers)
from euromod_linking.methods.cells import (  # noqa: F401  (shared machinery, re-exported)
    CELL_COL, apply_op, collapse_shock_groups, prepare_cells, specs_in_shocks,
    data_region_len as _data_region_len,
)

logger = logging.getLogger(__name__)

# Metrics, and which margin of the labour market each one fixes.
#
# Rates exist so a request already phrased as a rate needs no conversion: a
# growth rate derived by hand from some other population count is the classic
# way a shock ends up the wrong size. Their denominator is the cell's
# working-age population, which the alignment leaves unchanged (only the
# inactive <-> active margin moves), so a rate target converts to a level by one
# multiplication — no fixed point to solve — and before/after rates are directly
# comparable.
#
# 'inactivity_rate' is the CONVENTIONAL complement of the participation rate:
# everyone of working age who is neither employed nor unemployed, students and
# pensioners included. That is wider than the pool the methodology can actually
# move (see states.definition), which is a feasibility question, not a
# definitional one.
LEVEL_CURRENT = {"employment": "current_emp", "unemployment": "current_unemp"}
RATE_NUMERATOR = {"employment_rate": "current_emp",
                  "participation_rate": "current_active",
                  "inactivity_rate": "current_inactive_conventional"}
RATE_METRICS = frozenset(RATE_NUMERATOR)
ALL_METRICS = frozenset(LEVEL_CURRENT) | RATE_METRICS

#: Which quantity each metric pins: employment, the labour force, or unemployment.
MARGIN_OF = {"employment": "employment", "employment_rate": "employment",
             "participation_rate": "activity", "inactivity_rate": "activity",
             "unemployment": "unemployment"}


def build_targets(df: pd.DataFrame, shocks_period: pd.DataFrame, used: dict[str, list[str]],
                  eligible: pd.Series) -> tuple[pd.DataFrame, list[str]]:
    """Per-cell current weighted counts + targets for employment/unemployment.

    The macro model's numbers are anchored on the *survey's own* baseline:
    a growth-rate shock becomes ``target = current_weighted_count * (1+rate)``,
    so only the macro model's projected *change* is imposed — any level
    disagreement between the macro model's employment stock and the weighted
    survey count (different definitions, reference periods) is deliberately
    not "corrected", which would move thousands of people for purely
    statistical reasons.

    Cells without a shock for a metric keep target = current: no shock, no
    artificial churn. Shock cells matching no data cell are reported as
    warnings (the macro model may cover regions the survey lacks)."""
    # Bring the shocks down to the granularity the data can support (e.g.
    # NUTS-2 growth rates averaged into NUTS-1 cells).
    region_len = _data_region_len(df, used)
    collapsed, warnings = collapse_shock_groups(shocks_period, used, region_len,
                                                intensive_metrics=RATE_METRICS)

    # Current weighted stocks per cell — the survey's own baseline, computed
    # over the working-age (eligible) population only, since only they can be
    # moved by the alignment.
    e = df[eligible]
    cur = (e.assign(_w_emp=e["employed"] * e["dwt"],
                    _w_unemp=e["unemployed"] * e["dwt"],
                    _w_act=e["active"] * e["dwt"],
                    _w_inact=e["inactive"] * e["dwt"],
                    _w_other=e["other"] * e["dwt"],
                    _w_pop=e["dwt"])
           .groupby(CELL_COL, sort=True)[["_w_emp", "_w_unemp", "_w_act", "_w_inact",
                                          "_w_other", "_w_pop"]].sum()
           .rename(columns={"_w_emp": "current_emp", "_w_unemp": "current_unemp",
                            "_w_act": "current_active", "_w_inact": "current_inactive",
                            "_w_other": "current_other", "_w_pop": "working_age_pop"}))

    # Conventional inactivity: everyone of working age who is neither employed
    # nor unemployed. Wider than 'current_inactive', which is only the states
    # the alignment can move — students, pensioners, the long-term sick and
    # conscripts are inactive by any published definition but are never moved.
    cur["current_inactive_conventional"] = cur["working_age_pop"] - cur["current_active"]

    # Default: unshocked metrics keep target = current, so they generate no gap
    # and hence no artificial churn.
    cur["unemp_target"] = cur["current_unemp"]
    cur["emp_target"] = cur["current_emp"]
    cur["active_target"] = cur["current_active"]

    # Resolve each cell's shocks into at most one employment-margin target and
    # at most one activity-margin target, both as levels (people).
    emp_shock: dict = {}       # cell -> employment level asked for
    active_shock: dict = {}    # cell -> labour-force level asked for
    seen: dict = {}            # cell -> {margin: metric} for the clash check
    for t in collapsed.itertuples():
        if t.metric not in ALL_METRICS:
            continue
        if t.group not in cur.index:
            warnings.append(f"Shock cell {t.group!r} ({t.metric}) matches no one in the data")
            continue
        margin = MARGIN_OF[t.metric]
        clash = seen.setdefault(t.group, {})
        if margin in ("employment", "activity") and "employment" in clash and "activity" in clash:
            pass  # reported below
        clash[margin] = t.metric

        pop = float(cur.loc[t.group, "working_age_pop"])
        if t.metric in RATE_METRICS:
            level, rate_warnings = _rate_target(cur, t, pop)
            warnings += rate_warnings
        else:
            level = apply_op(float(cur.loc[t.group, LEVEL_CURRENT[t.metric]]),
                             t.op, float(t.value))

        if margin == "employment":
            emp_shock[t.group] = level
        elif margin == "activity":
            active_shock[t.group] = level
        else:
            cur.loc[t.group, "unemp_target"] = level

    for cell, margins in seen.items():
        if "employment" in margins and "activity" in margins:
            raise MethodError(
                f"Cell {cell!r} is over-determined: {margins['activity']!r} fixes the labour "
                f"force and {margins['employment']!r} fixes employment, which between them "
                "also fix unemployment — but unemployment defaults to its current level, so "
                "one of the three would be silently discarded. Shock the activity margin "
                "together with 'unemployment', or shock employment and unemployment.")

    # Three steps, two degrees of freedom. Identical to the previous
    # emp + unemp identity when no activity-margin metric is used.
    #   1. unemployment: its shock, else held at current (no shock, no churn)
    #   2. labour force: stated outright by an activity-margin metric, else
    #      implied by the employment target plus unemployment
    #   3. employment:   the residual, by the identity active = employed + unemployed
    for cell in cur.index:
        unemp_t = float(cur.loc[cell, "unemp_target"])
        if cell in active_shock:
            active_t = active_shock[cell]
        else:
            active_t = emp_shock.get(cell, float(cur.loc[cell, "current_emp"])) + unemp_t
        cur.loc[cell, "active_target"] = active_t
        cur.loc[cell, "emp_target"] = active_t - unemp_t

    for label, shocked, current_col in (("employment", emp_shock, "current_emp"),
                                        ("labour force", active_shock, "current_active")):
        for cell, level in shocked.items():
            _warn_negligible(warnings, label, cell, level, float(cur.loc[cell, current_col]))

    cur["active_gap"] = cur["active_target"] - cur["current_active"]
    cur["unemp_gap"] = cur["unemp_target"] - cur["current_unemp"]
    cur["emp_gap"] = cur["emp_target"] - cur["current_emp"]
    return cur, warnings


def _rate_target(cur: pd.DataFrame, t, pop: float) -> tuple[float, list[str]]:
    """Resolve a rate shock into a level (people), guarding the arithmetic.

    On a rate metric ``add`` is a percentage-point change written as a fraction:
    0.035 is +3.5pp. That is the unit these requests arrive in, which is the
    whole reason the metrics exist.
    """
    warnings: list[str] = []
    if pop <= 0:
        raise MethodError(
            f"Cell {t.group!r} has no working-age population, so {t.metric!r} has no "
            "denominator. Check the cell's variable and codes.")

    current_level = float(cur.loc[t.group, RATE_NUMERATOR[t.metric]])
    current_rate = current_level / pop
    target_rate = apply_op(current_rate, t.op, float(t.value))

    if t.op == "add" and abs(float(t.value)) > 0.5:
        warnings.append(
            f"{t.metric} shock for cell {t.group!r} uses op='add' with value {t.value:g}. On a "
            "rate metric 'add' is a percentage-point change written as a fraction — 3.5pp is "
            "0.035, not 3.5.")
    if not 0.0 <= target_rate <= 1.0:
        raise MethodError(
            f"{t.metric} shock for cell {t.group!r} resolves to a rate of {target_rate:.4f}, "
            f"outside [0, 1] (baseline {current_rate:.4f}, op {t.op!r}, value {t.value:g}). On a "
            "rate metric 'add' is a percentage-point change written as a fraction: 3.5pp is "
            "0.035, not 3.5.")

    # inactivity is the complement: an inactivity target fixes the labour force.
    rate = (1.0 - target_rate) if t.metric == "inactivity_rate" else target_rate
    return rate * pop, warnings


def _warn_negligible(warnings: list[str], metric: str, cell, target: float, current: float):
    """A shock that moves nobody is almost always a mis-specified op, not a
    deliberate null: `add` on a LEVEL adds a count of people, so op=add
    value=0.035 on a cell of two million is 0.035 persons — it reads like +3.5%
    and behaves like nothing, then reports an "exact" alignment of the baseline
    to itself."""
    if target != current and abs(target - current) < 1.0:
        warnings.append(
            f"{metric} shock for cell {cell!r} moves {abs(target - current):.3g} people — "
            "effectively nothing. Check the op and the metric: on a level metric 'add' adds a "
            "COUNT of people and 'grow' applies a rate (0.035 = +3.5%); to shock a rate "
            "directly use a *_rate metric, where 'add' is a percentage-point change.")


# --- reporting ---------------------------------------------------------------
# Everything below is pure: it reads only the frame build_targets returns, never
# the microdata. That is deliberate — a caller must not be able to reconstruct
# "who counts as employed" a second way, which is exactly how a shock derived
# from one population ends up applied to another.

#: Share of the recruitable pool above which a shock is worth flagging.
POOL_WARN_PCT = 25.0


def _num(frame, cell, col):
    return float(frame.loc[cell, col])


def _rate(numerator, denominator):
    return round(numerator / denominator, 6) if denominator > 0 else None


def _pct(change, base):
    return round(change / base * 100, 4) if base > 0 else None


def _pp(new_rate, old_rate):
    if new_rate is None or old_rate is None:
        return None
    return round((new_rate - old_rate) * 100, 4)


def cell_report(cell_targets: pd.DataFrame) -> list[dict]:
    """Per-cell baseline, resolved target, implied change and feasibility.

    The baseline is the number the alignment will actually move people away
    from, so a caller can check its own arithmetic against it *before* running:
    a growth rate derived from some other population count shows up here as a
    change that does not match what was asked for.

    ``change`` is reported in people, in percent, and in percentage points of
    the participation and inactivity rates — the last of these is usually the
    unit the request was phrased in ("inactivity down 3.5pp"), even when the
    shock itself was expressed as a level.

    Rates use the cell's working-age population as denominator; it and the
    shielded ``other`` state are invariant under the alignment (only the
    inactive <-> active margin moves), so the rates are directly comparable
    before and after. ``inactivity_rate`` is the conventional complement of the
    participation rate — everyone of working age who is neither employed nor
    unemployed, students and pensioners included — which is *wider* than the
    ``inactive_recruitable`` pool the methodology is able to move.
    """
    out = []
    for cell in cell_targets.index:
        pop = _num(cell_targets, cell, "working_age_pop")
        emp = _num(cell_targets, cell, "current_emp")
        unemp = _num(cell_targets, cell, "current_unemp")
        active = _num(cell_targets, cell, "current_active")
        recruitable = _num(cell_targets, cell, "current_inactive")
        shielded = _num(cell_targets, cell, "current_other")

        emp_t = _num(cell_targets, cell, "emp_target")
        unemp_t = _num(cell_targets, cell, "unemp_target")
        active_t = _num(cell_targets, cell, "active_target")
        active_gap = _num(cell_targets, cell, "active_gap")

        part_rate, part_rate_t = _rate(active, pop), _rate(active_t, pop)
        inact_rate, inact_rate_t = _rate(pop - active, pop), _rate(pop - active_t, pop)

        # Level 1 moves the inactive <-> active margin, so the pool is the state
        # it draws from: the recruitable inactive when activating, the currently
        # active when de-activating. Same sets hierarchy.align_level1_activity uses.
        pool = recruitable if active_gap >= 0 else active
        required = abs(active_gap)
        used_pct = _pct(required, pool)
        if required == 0:
            severity = "none"
        elif used_pct is None or used_pct > 100:
            severity = "infeasible"
        elif used_pct > POOL_WARN_PCT:
            severity = "high"
        else:
            severity = "low"

        out.append({
            "cell": cell,
            # Cells the shocks did not touch are reported too — they are the
            # evidence the rest of the population was held constant — but a
            # reader must not mistake one for the cell it shocked, so say which
            # is which rather than leaving it to be inferred from a zero.
            "shocked": bool(emp_t != emp or unemp_t != unemp),
            "baseline": {
                "working_age_pop": round(pop, 2),
                "employed": round(emp, 2), "unemployed": round(unemp, 2),
                "inactive_recruitable": round(recruitable, 2),
                "other_shielded": round(shielded, 2),
                "employment_rate": _rate(emp, pop),
                "participation_rate": part_rate, "inactivity_rate": inact_rate,
            },
            "target": {
                "employed": round(emp_t, 2), "unemployed": round(unemp_t, 2),
                "employment_rate": _rate(emp_t, pop),
                "participation_rate": part_rate_t, "inactivity_rate": inact_rate_t,
            },
            "change": {
                "employed": round(emp_t - emp, 2), "employed_pct": _pct(emp_t - emp, emp),
                "unemployed": round(unemp_t - unemp, 2),
                "people_moved": round(active_gap, 2),
                "participation_rate_pp": _pp(part_rate_t, part_rate),
                "inactivity_rate_pp": _pp(inact_rate_t, inact_rate),
            },
            "feasibility": {
                "margin": "participation",
                "direction": "inactive->active" if active_gap >= 0 else "active->inactive",
                "people_required": round(required, 2), "pool": round(pool, 2),
                "pool_used_pct": used_pct,
                "max_feasible_change": {
                    "people": round(pool, 2),
                    "inactivity_rate_pp": _pp(_rate(pop - (active + pool), pop), inact_rate)
                    if active_gap >= 0 else _pp(_rate(pop, pop), inact_rate),
                    "employed_pct": _pct(pool, emp),
                },
                "severity": severity,
            },
        })
    return out


def feasibility_warnings(report: list[dict]) -> list[str]:
    """Warnings for cells whose shock consumes an implausible share of the pool.

    Only the ``inactive`` state can be moved, so a target derived from a broader
    notion of inactivity (one counting students, pensioners or everyone without
    employment income) asks for people who are not there to be moved.
    """
    out = []
    for row in report:
        f = row["feasibility"]
        if f["severity"] not in ("high", "infeasible"):
            continue
        b = row["baseline"]
        out.append(
            f"Cell {row['cell']!r}: this shock moves {f['people_required']:,.0f} people — "
            f"{f['pool_used_pct']:.1f}% of the {f['pool']:,.0f} who are recruitable. Only the "
            f"'inactive' state can move; the {b['other_shielded']:,.0f} students, pensioners, "
            "long-term sick and conscripts here never do. If your target came from a wider "
            "definition of inactivity, it is too large.")
    return out


def totals(cell_targets: pd.DataFrame, accuracy: list[dict] | None = None) -> dict:
    """Scenario-wide sums over the shocked cells (unshocked cells contribute 0)."""
    out = {
        "working_age_pop": round(float(cell_targets["working_age_pop"].sum()), 2),
        "employment_baseline": round(float(cell_targets["current_emp"].sum()), 2),
        "employment_target": round(float(cell_targets["emp_target"].sum()), 2),
        "employment_change": round(float(cell_targets["emp_gap"].sum()), 2),
        "unemployment_change": round(float(cell_targets["unemp_gap"].sum()), 2),
        "people_moved": round(float(cell_targets["active_gap"].sum()), 2),
    }
    if accuracy:
        out["employment_achieved"] = round(
            sum(r["achieved"] for r in accuracy if r["metric"] == "employment"), 2)
    return out
