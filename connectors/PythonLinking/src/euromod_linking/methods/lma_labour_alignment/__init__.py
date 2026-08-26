"""lma_labour_alignment — align EUROMOD microdata to external labour-market
targets and drive the LMA add-on.

Works over any population cells the shock table defines, on full-population
input, deterministically. Methodology is fixed here; scenarios can only set the
declared params (period, tolerance_pct).
"""

import logging

import numpy as np
import pandas as pd

from euromod_linking.methods.base import MethodContext, MethodError, MethodResult
from euromod_linking.methods.lma_labour_alignment import (  # noqa: F401
    hierarchy, imputation, scoring, states, targets as targets_mod, weights,
)
from euromod_linking.registry import MethodSpec, register

logger = logging.getLogger(__name__)

INJECTED = ("lma", "yem_a", "yemmy_a", "lhw_a")

# Take-up assumption for people entering employment: a full-time job, all year,
# paid at the person's own predicted hourly wage (yivwg). 4.3333 = 52/12, the
# average number of weeks in a month, converting a weekly wage bill to monthly
# (EUROMOD monetary variables are monthly). Weekly hours come from the country's
# $lhw constant when the model defines it; this is the fallback for the models
# that do not (IE, LV, NL) or that define it implausibly (HR ships 0).
DEFAULT_FULLTIME_HOURS_PER_WEEK = 40.0
WEEKS_PER_MONTH = 4.3333
MONTHS_PER_YEAR = 12

#: A $lhw outside this range is not a working week. HR ships 0, which would pay
#: every new worker nothing.
FULLTIME_HOURS_BOUNDS = (10.0, 60.0)


def fulltime_hours(country_code: str, system_name: str) -> tuple[float | None, str]:
    """The country's standard full-time week, from the model's own $lhw constant.

    Full-time differs by country (35 in FR, 38 in DK, 42 in AT), so the hardcoded
    40 above would misstate entrants' earnings. Returns (hours, note); hours is
    None when $lhw is absent (IE, LV, NL) or implausible, and the default applies.
    """
    from euromod_linking.query import get_country_index

    try:
        idx = get_country_index(country_code)
        value = float(idx.systems[system_name].constant_values.get("$lhw"))
    except Exception:
        return None, "$lhw not defined for this country/system"
    lo, hi = FULLTIME_HOURS_BOUNDS
    if not (lo <= value <= hi):
        return None, f"$lhw = {value} is outside {lo}-{hi} h/week and was ignored"
    return value, f"$lhw = {value} h/week"


def _movers(df: pd.DataFrame, mask: pd.Series) -> dict:
    """Weighted people and raw rows behind a transition flag.

    Both are reported because they answer different questions and are easy to
    confuse: ``rows`` is a microdata record count (useful for judging whether a
    result rests on a handful of observations), ``people`` is the population
    quantity — the one that belongs in an answer.
    """
    return {"people": round(float(df.loc[mask, "dwt"].sum()), 2), "rows": int(mask.sum())}


class LmaLabourAlignment:
    """See module docstring. Instances are stateless between calls."""

    def baseline_transform(self, data: pd.DataFrame, ctx: MethodContext) -> pd.DataFrame:
        """Neutral LMA columns: identical for every shock table/period, so the
        baseline simulation is shared via the cache.

        The baseline runs WITH the LMA add-on active but lma=0 for everyone
        (no transitions), rather than without the add-on: both runs then go
        through an identical policy configuration, so the baseline/
        counterfactual delta isolates the labour-market transitions and cannot
        pick up incidental differences from toggling the add-on itself."""
        df = data.copy()
        df["lma"] = 0
        df["yem_a"] = 0.0
        df["yemmy_a"] = 0.0
        df["lhw_a"] = 0.0
        return df

    def check_dataset(self, columns, shocks: pd.DataFrame) -> list[str]:
        problems = []
        required = ["idhh", "idperson", "dwt", "dag", "dgn", "yem"]
        missing = [c for c in required if c not in columns]
        if missing:
            problems.append(f"Missing required columns: {missing}")
        if "les" not in columns and "les2" not in columns:
            problems.append("Neither 'les' nor 'les2' labour-status variable present")
        from euromod_linking import dimensions as dims_mod
        for g in sorted(shocks.loc[shocks["channel"] != "constant", "group"].unique()):
            problems.extend(dims_mod.validate_group_columns(g, columns))
        periods = sorted(shocks.loc[shocks["channel"] == "align", "period"].unique())
        if not periods:
            problems.append("Shock table contains no 'align' channel records")
        return problems

    def _prepare_targets(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict) -> dict:
        """Classify, build cells, resolve targets — the head apply() and
        preview() share.

        Sharing it is the point: a preview that built its own targets could
        disagree with the run it claims to preview, which is the whole failure
        mode these diagnostics exist to catch.
        """
        from euromod_linking.methods.cells import resolve_period
        aligned = shocks[shocks["channel"] == "align"]
        period = resolve_period(aligned, params)

        shocks_p = aligned[aligned["period"] == period]
        if shocks_p.empty:
            available = sorted(shocks.loc[shocks["channel"] == "align", "period"].unique())
            raise MethodError(f"No align shocks for period {period!r}; available: {available}")

        # Map every person into the four labour states and derive the target
        # cells from whatever dimensions the shocks use. Eligibility (working
        # age) is a MASK, not a row filter: households stay whole so the
        # simulation still prices taxes/benefits on complete families.
        df = data.copy()
        les_var, les_codes = states.les_variable(df.columns)
        status = states.classify_labour_status(df[les_var], les_codes)
        df = pd.concat([df, status], axis=1)
        eligible = df["dag"].between(*states.WORKING_AGE)

        used = targets_mod.specs_in_shocks(shocks_p)
        df, dim_info = targets_mod.prepare_cells(df, used)
        cell_targets, warnings = targets_mod.build_targets(df, shocks_p, used, eligible)
        return {"df": df, "eligible": eligible, "cell_targets": cell_targets, "used": used,
                "dim_info": dim_info, "warnings": warnings, "period": period,
                "les_var": les_var, "les_codes": les_codes}

    def preview(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
                ctx: MethodContext) -> dict:
        """What apply() would target, without fitting or aligning anything.

        Cheap enough to run at validation time, which is where a mis-sized shock
        has to be caught: after the run there is nothing left to do but discard
        two simulations.
        """
        prep = self._prepare_targets(data, shocks, params)
        report = targets_mod.cell_report(prep["cell_targets"])
        _, hours_note = fulltime_hours(ctx.country_code, ctx.system_name)
        return {
            "period": prep["period"], "les_variable": prep["les_var"],
            "dimensions": prep["dim_info"], "cells": report,
            "totals": targets_mod.totals(prep["cell_targets"]),
            "population_definition": states.definition(prep["les_var"]),
            "fulltime_hours_note": hours_note,
            "warnings": list(prep["warnings"]) + targets_mod.feasibility_warnings(report),
        }

    def apply(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
              ctx: MethodContext) -> MethodResult:
        tolerance = float(params.get("tolerance_pct", 5.0))
        original_columns = list(data.columns)

        prep = self._prepare_targets(data, shocks, params)
        df, eligible, cell_targets = prep["df"], prep["eligible"], prep["cell_targets"]
        les_var, les_codes, used = prep["les_var"], prep["les_codes"], prep["used"]
        period, dim_info, warnings = prep["period"], prep["dim_info"], list(prep["warnings"])

        # --- scoring (fit on the eligible population) --------------------------
        # The propensity fit reads only observed characteristics and current
        # labour states, never targets or cells, so it is identical for every
        # scenario on this dataset — and is therefore content-addressed and
        # cached (see scoring.py); the cell columns added above are not part of
        # its fingerprint. Estimated on working-age adults only: including
        # children/retirees would let structurally out-of-market groups shape
        # the coefficients that rank marginal entrants.
        scored, score_info = scoring.estimate_probabilities(df[eligible])
        for col in scoring.PROB_COLUMNS:
            df[col] = np.nan
            df.loc[scored.index, col] = scored[col]

        # --- alignment ---------------------------------------------------------
        # Level 1 sizes the labour force per cell, level 2 splits it into
        # employment/unemployment; validate_states then proves the accounting
        # identities held, and accuracy grades each cell target-vs-achieved.
        cell_report = targets_mod.cell_report(cell_targets)
        warnings += targets_mod.feasibility_warnings(cell_report)
        df, unfillable1 = hierarchy.align_level1_activity(df, cell_targets, self._mask(df, eligible))
        eligible2 = df["dag"].between(*states.WORKING_AGE)  # recompute: splits added rows
        df, unfillable2 = hierarchy.align_level2_employment(df, cell_targets, eligible2)
        hierarchy.validate_states(df)
        accuracy = hierarchy.accuracy(df, cell_targets, df["dag"].between(*states.WORKING_AGE))

        # --- LMA variables -----------------------------------------------------
        # The LMA add-on rewrites a person's labour situation inside EUROMOD
        # only where lma != 0. Crucially, only genuine status *changers* are
        # flagged (someone already employed per the survey stays lma=0 even if
        # new_employed==1), so everyone else is simulated on their original
        # survey incomes — the counterfactual differs from the baseline through
        # the transitions alone.
        df["lma"] = 0
        df["yem_a"] = 0.0
        df["yemmy_a"] = 0.0
        df["lhw_a"] = 0.0
        df.loc[(~df[les_var].isin(les_codes["employed"])) & (df["new_employed"] == 1), "lma"] = 1
        df.loc[(~df[les_var].isin(les_codes["unemployed"])) & (df["new_unemployed"] == 1), "lma"] = 2

        # Earnings of new workers: FULL-TIME TAKE-UP at the person's own
        # predicted hourly wage. yivwg ("Hourly predicted wage") is estimated by
        # the national team for every working-age person, including those not
        # currently working — precisely the counterfactual wage an entrant needs.
        # A full-time month is 40 hours x 4.3333 weeks, and a full year of it is
        # 12 such months.
        # Full-time hours come from the country's own $lhw constant where the
        # model defines it (35 in FR, 38 in DK, 42 in AT), falling back to 40.
        resolved_hours, hours_note = fulltime_hours(ctx.country_code, ctx.system_name)
        hours = float(resolved_hours or DEFAULT_FULLTIME_HOURS_PER_WEEK)
        entrants = df["lma"] == 1
        df.loc[entrants, "yemmy_a"] = float(MONTHS_PER_YEAR)
        df.loc[entrants, "lhw_a"] = hours
        if "yivwg" in df.columns:
            wage = pd.to_numeric(df.loc[entrants, "yivwg"], errors="coerce")
            df.loc[entrants, "yem_a"] = (hours * WEEKS_PER_MONTH * wage).fillna(0.0)
            has_wage = entrants & (df["yem_a"] > 0)
        else:
            has_wage = pd.Series(False, index=df.index)

        # Donor matching is the FALLBACK, for entrants with no usable predicted
        # wage (yivwg missing or non-positive — e.g. groups the national team
        # excludes from the wage equation). Statistical matching only for those.
        region_col = "_dim_region" if "region" in used else None
        final_eligible = df["dag"].between(*states.WORKING_AGE)
        needs_fallback = entrants & ~has_wage
        imput_diag = {"n_recipients": 0}
        if needs_fallback.any():
            df, imput_diag = imputation.impute_transition_incomes(
                df, final_eligible, region_col, recipients=df.index[needs_fallback])

        earn = df.loc[entrants, "yem_a"]
        earnings_diag = {
            "assumption": (f"full-time take-up: 12 months x {hours:g} h/week at the person's "
                           "predicted hourly wage (yivwg)"),
            "fulltime_hours_per_week": hours,
            "fulltime_hours_source": ("$lhw (country constant)" if resolved_hours
                                      else f"default {DEFAULT_FULLTIME_HOURS_PER_WEEK:g} "
                                           "($lhw undefined or implausible)"),
            "fulltime_hours_note": hours_note,
            "n_entrants": int(entrants.sum()),
            "n_from_predicted_wage": int(has_wage.sum()),
            "n_donor_matched_fallback": int(needs_fallback.sum()),
            "monthly_yem_a": {
                "median": round(float(earn.median()), 2) if len(earn) else None,
                "min": round(float(earn.min()), 2) if len(earn) else None,
                "max": round(float(earn.max()), 2) if len(earn) else None,
            },
        }

        # --- diagnostics & cleanup --------------------------------------------
        grades: dict = {}
        for row in accuracy:
            grades[row["grade"]] = grades.get(row["grade"], 0) + 1
        n_over_tolerance = sum(1 for r in accuracy
                               if r["error_pct"] is not None and abs(r["error_pct"]) > tolerance)
        n_new_households = int(df["idhh"].nunique() - data["idhh"].nunique())
        diagnostics = {
            "period": period,
            "les_variable": les_var,
            "dimensions": dim_info,
            "targets": accuracy,
            "grades": grades,
            "n_cells_over_tolerance_pct": n_over_tolerance,
            "tolerance_pct": tolerance,
            "unfillable_gaps": unfillable1 + unfillable2,
            # people, not rows: each row carries dwt real individuals, and the
            # boundary splits rescale dwt, so a row can stand for a fraction of
            # a person. The weighted count is the one that matches the target
            # (see weights.py) and the only one safe to report as "people".
            "transitions": {
                "to_employment_lma1": _movers(df, df["lma"] == 1),
                "to_unemployment_lma2": _movers(df, df["lma"] == 2),
                "to_active": _movers(df, df["transition_to_active"] == 1),
                "to_inactive": _movers(df, df["transition_to_inactive"] == 1),
            },
            "cells": cell_report,
            "totals": targets_mod.totals(cell_targets, accuracy),
            "n_split_households": n_new_households,
            "weight_totals": {"before": round(float(data["dwt"].sum()), 2),
                              "after": round(float(df["dwt"].sum()), 2)},
            "scoring": score_info,   # cached: True => propensities reused, not refitted
            "earnings": earnings_diag,
            "imputation": imput_diag,   # donor-matching fallback only
            "warnings": warnings,
        }

        out = df[original_columns + list(INJECTED)].sort_values(
            ["idhh", "idperson"], kind="mergesort").reset_index(drop=True)

        # The matched baseline: the very same rows (including the household
        # copies created by the weight splits, with their split weights) but
        # with every transition switched off. Because a split only partitions
        # dwt and never alters incomes, the two branches of a split household
        # are identical here and their weights sum back to the original — so
        # this baseline is economically the same population as the unsplit
        # data, while being row-aligned with the counterfactual.
        paired_baseline = self.baseline_transform(out, ctx)
        diagnostics["paired_baseline"] = {
            "n_rows": int(len(paired_baseline)),
            "rows_added_by_splits": int(len(out) - len(data)),
            "note": "baseline rebuilt on the counterfactual's rows so reform "
                    "statistics can pair observations",
        }
        return MethodResult(data=out, diagnostics=diagnostics, baseline=paired_baseline)

    @staticmethod
    def _mask(df: pd.DataFrame, eligible: pd.Series) -> pd.Series:
        # eligible was computed before prepare_cells copied df; reindex safely.
        return eligible.reindex(df.index, fill_value=False)


register(MethodSpec(
    name="lma_labour_alignment",
    summary="Align labour-market states (employment/unemployment) to external "
            "targets and simulate via the LMA add-on.",
    description=(
        "Two-level hierarchical alignment of EUROMOD microdata to external "
        "employment/unemployment targets per population cell (any combination of "
        "input variables, e.g. region x education or age x gender). Individuals are ranked by "
        "logit-predicted transition probabilities (features: age, gender, "
        "disability income, marital-status dummies); fractional targets are hit "
        "exactly by copying the boundary household and splitting its sample "
        "weight. New workers are assumed to take up FULL-TIME employment: 12 "
        "months at the country's own standard week (the model's $lhw constant, "
        "defaulting to 40 hours where it is absent or implausible), paid at "
        "their own predicted hourly wage (yivwg), i.e. "
        "yem_a = hours x 4.3333 x yivwg per month. Entrants with no "
        "predicted wage fall back to 5-nearest-neighbour donor matching (age, "
        "gender, education, marital status, region). The "
        "run activates the LMA add-on (LMA_trans switch) consuming the injected "
        "columns lma/yem_a/yemmy_a/lhw_a. "
        "Metrics come in two forms. LEVELS -- 'employment', 'unemployment' -- are "
        "counts of people, and are what ingested external-model files carry. "
        "RATES -- 'employment_rate', 'participation_rate', 'inactivity_rate' -- are "
        "shares of the cell's working-age population, so a request already phrased "
        "as a rate needs no conversion: on a rate metric 'add' is a percentage-point "
        "change written as a fraction, i.e. inactivity down 3.5pp is "
        "{metric: inactivity_rate, op: add, value: -0.035}. 'inactivity_rate' is the "
        "conventional complement of the participation rate (everyone of working age "
        "neither employed nor unemployed, students and pensioners included), which is "
        "wider than the pool that can actually be moved. A cell may not fix both the "
        "labour force and employment, since that would also fix unemployment. "
        "Methodology is fixed; scenario params "
        "are only: period (which external-model period to apply, required) and "
        "tolerance_pct (reporting threshold, default 5)."),
    channels_consumed=("align",),
    metrics_consumed=("employment", "unemployment",
                      "employment_rate", "participation_rate", "inactivity_rate"),
    cell_variables="Any input variable (deh=3-4, dgn=1, dag=25-34, les=5) and/or 'region'.",
    restructures_rows=True,   # boundary weight-splits copy households
    dataset_requirements=("idhh", "idperson", "dwt", "dag", "dgn", "yem"),
    addon_requirements=((("LMA", "LMA_{cc}"),), (("LMA_trans", True),)),
    min_model_release="J2.54",  # first internal release shipping the LMA_trans extension
    injected_columns=INJECTED,
    params_schema={
        "type": "object",
        "additionalProperties": False,
        "properties": {
            "period": {"type": "string",
                       "description": "External-model period label whose targets to apply. "
                                      "Optional when the shock table has exactly one period."},
            "tolerance_pct": {"type": "number", "default": 5.0,
                              "description": "Reporting threshold for per-cell alignment error."},
        },
    },
    factory=LmaLabourAlignment,
))
