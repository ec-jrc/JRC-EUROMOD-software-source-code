"""scale_variables — cell-level scaling of numeric input variables.

Consumes the ``scale`` channel: nominal wage changes, hours shocks, any income
component adjustment. metric = an input variable (e.g. ``yem``), an EUROMOD
income list (e.g. ``ils_udb_yem``), or the descriptive name of one (e.g.
``"employment income"``, resolved to ``ils_udb_yem`` during normalisation — see
:mod:`euromod_linking.income_lists`). A list is expanded extension-aware into
its component input variables against the live model.

Economic intuition
------------------
This is the price/income channel of model linking: a macro model projects
nominal paths (wage growth by region/sector-proxy, hours adjustments) and the
shock applies them *heterogeneously* across population cells — which is the
entire point of pushing them through a microsimulation. A uniform +3% wage
change has non-uniform disposable-income effects because tax brackets,
benefit withdrawals and means tests bite differently along the distribution;
EUROMOD supplies exactly that pass-through when the scaled inputs are
simulated against unchanged (or separately shocked) policy parameters.

Income lists matter because "employment income" as an economic concept is not
one variable: scaling the list scales every component the model itself counts
under that concept (extension-aware: an extension can add or remove
components), keeping the shock consistent with the model's own accounting.
Lists accept only mult/grow: a proportional factor distributes exactly over a
sum, while an absolute add/set on an aggregate has no unique per-component
allocation.

Purely arithmetic and deterministic: rows are matched per shock by its own
group keys (subset matching on derived dimension labels), ops apply in sorted
(metric, group) order, no rows are added or removed, and the baseline is the
untransformed run. Overlapping ``set`` shocks on one metric are rejected
(order-dependent); ``mult``/``grow`` compose commutatively.
"""

from euromod_linking import income_lists as _catalogue

#: The standardised ``ils_udb_*`` income lists as ``{group: {name: label}}``,
#: published so a caller shocking an economic concept ("investment income") can
#: name the model's own aggregate for it instead of guessing which raw variables
#: belong to it — or, better, write the concept itself and let
#: :mod:`euromod_linking.income_lists` resolve it.
#:
#: A view over :data:`euromod_linking.income_lists.CATALOGUE`, which carries the
#: labels, the accepted spellings and the per-list notes. Defined there rather
#: than here because ``shock_table`` canonicalises a descriptive metric during
#: normalisation and must not import a method to do it.
INCOME_LISTS = {
    group: {e.name: e.label for e in _catalogue.CATALOGUE if e.group == group}
    for group in _catalogue.GROUPS
}


def income_lists() -> dict:
    """The ``ils_udb_*`` catalogue as data: what each list covers, what it
    answers to, and the usage notes. See :mod:`euromod_linking.income_lists`."""
    cat = _catalogue.catalogue()
    return {
        "lists": {k: dict(v) for k, v in INCOME_LISTS.items()},
        "groups": cat["groups"],
        "aliases": cat["aliases"],
        "notes": [
            "A metric may be an input variable, an income list, or a descriptive "
            "name for one ('employment income' -> ils_udb_yem). Descriptive names "
            "are resolved during normalisation, ignoring case, underscores, hyphens "
            "and punctuation, so the shock table always holds the ils_udb_* name and "
            "the content id does not depend on the spelling. See 'aliases'.",
            "Naming is standardised across countries; membership is country- and "
            "extension-specific and is resolved against the live model, so an unknown "
            "name fails with the names that system does define. The nine benefit lists "
            "are defined in 25 of the 27 countries, the rest in all 27.",
            "Scaling a list scales every component the model counts under that concept, "
            "which keeps the shock consistent with EUROMOD's own accounting. Prefer it "
            "over naming raw variables when the shock is about an economic concept.",
            "Lists accept 'mult' and 'grow' only: a proportional factor distributes "
            "exactly over a sum, an absolute 'add'/'set' on an aggregate does not.",
            "Benefit and tax lists are built largely from simulated components that "
            "EUROMOD recomputes, so scaling them mostly has no effect: those components "
            "are reported as 'skipped_not_in_input', and a list with none in the input "
            "at all is rejected. ils_udb_tis carries one reported component against 121 "
            "simulated ones. Where a list carries both, scaling shifts only the reported "
            "part — rarely what is meant. To change what a benefit or tax pays out, "
            "shock its policy parameters through the 'constant' channel.",
            "ils_udb_yds is disposable income and holds no variables of its own, only "
            "the other twenty lists including taxes, so scaling it fans out over all of "
            "them at once. Name a component list instead.",
        ],
    }

import logging

import pandas as pd

from euromod_linking import dimensions as dims_mod
from euromod_linking.methods import cells
from euromod_linking.methods.base import MethodContext, MethodError, MethodResult
from euromod_linking.registry import MethodSpec, register

logger = logging.getLogger(__name__)


#: Which ops commute with which. Two shocks on the same people and the same
#: metric are only safe together when they share a family: mult and grow both
#: multiply, add composes with add. Across families order decides the answer —
#: ``(x + a) * m`` is not ``x * m + a`` — and ``set`` overwrites, so it commutes
#: with nothing, not even another ``set``.
OP_FAMILY = {"mult": "proportional", "grow": "proportional",
             "add": "additive", "set": "absolute"}


def _is_income_list(metric: str) -> bool:
    return metric.startswith(("ils_", "il_"))


def resolve_income_lists(shocks: pd.DataFrame, ctx: MethodContext) -> dict:
    """``ils_*`` metrics -> their component input variables, from the live model.

    Extension-aware: which variables a list covers depends on which extensions
    are switched on, so this is resolved against the model rather than tabulated
    here. Returns {} when no shock names a list."""
    from euromod_linking.query import IncomeListLookupError, income_list_components

    metrics = sorted({t.metric for t in shocks.itertuples() if _is_income_list(t.metric)})
    if not metrics:
        return {}
    problems, out = [], {}
    for m in metrics:
        try:
            out[m] = income_list_components(ctx.country_code, ctx.system_name, m,
                                            dataset=ctx.dataset_name,
                                            extensions=ctx.extensions)
        except IncomeListLookupError as e:
            problems.append(f"Income list {m!r} could not be resolved: {e}")
        except Exception as e:
            # Reaching the model can fail for reasons that are not about this
            # list at all (no model loaded, country unreadable). Report it as a
            # failure to resolve rather than letting a session error escape a
            # method call.
            problems.append(f"Income list {m!r} could not be resolved against the model: {e}")
    if problems:
        raise MethodError("; ".join(problems))
    return out


def expansion_report(expansions: dict, columns) -> tuple[dict, list]:
    """Which components of each income list will actually be scaled, and a
    warning naming the ones that will not.

    A variable ending in ``_s`` is a *simulated output*: EUROMOD computes it from
    the microdata during the run, so it is not a column of the input and cannot
    be scaled. Reporting a list's full membership would promise a shock the run
    will not deliver — ``ils_udb_bun`` resolves to five components of which only
    two are input variables. The split is computed from the input's own columns
    and used for both the preview and the run, so the two cannot disagree.
    """
    report, warnings = {}, []
    for m, comps in expansions.items():
        scaled = [v for v, _sign in comps if v in columns]
        skipped = [v for v, _sign in comps if v not in columns]
        report[m] = {"scaled": scaled, "skipped_not_in_input": skipped}
        if skipped:
            warnings.append(
                f"Income list {m!r}: {len(skipped)} of {len(comps)} components are not in "
                f"the input dataset, so they will NOT be scaled — they are simulated "
                f"outputs EUROMOD recomputes from the inputs, or absent from this dataset: "
                f"{skipped}. Only {scaled} will change. To alter what a benefit or tax pays "
                "out, shock its policy parameters through the 'constant' channel instead.")
    return report, warnings


class ScaleVariables:
    """See module docstring. Instances are stateless between calls."""

    def check_dataset(self, columns, shocks: pd.DataFrame) -> list[str]:
        problems = []
        scale = shocks[shocks["channel"] == "scale"]
        if scale.empty:
            problems.append("Shock table contains no 'scale' channel records")
        for m in sorted(scale["metric"].unique()):
            # Income lists are resolved in preview()/apply(), which have the ctx
            # this needs to reach the model; plain metrics must be dataset columns.
            if not _is_income_list(m) and m not in columns:
                problems.append(f"Variable {m!r} is not a column of the input dataset")
        for g in sorted(scale["group"].unique()):
            problems.extend(dims_mod.validate_group_columns(g, columns))
        return problems

    def preview(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
                ctx: MethodContext) -> dict:
        """Which income-list components this scenario would actually scale.

        Resolved the same way apply() resolves them, from the same columns, so
        the preview and the run cannot disagree."""
        scale = shocks[shocks["channel"] == "scale"]
        shocks_p = scale[scale["period"] == cells.resolve_period(scale, params)]
        report, warnings = expansion_report(
            resolve_income_lists(shocks_p, ctx), list(data.columns))
        return {"income_list_expansions": report, "warnings": warnings}

    def apply(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
              ctx: MethodContext) -> MethodResult:
        scale = shocks[shocks["channel"] == "scale"]
        period = cells.resolve_period(scale, params)
        shocks_p = scale[scale["period"] == period]
        if shocks_p.empty:
            available = sorted(scale["period"].unique())
            raise MethodError(f"No scale shocks for period {period!r}; available: {available}")

        original_columns = list(data.columns)
        warnings: list[str] = []

        # --- income-list expansion ---------------------------------------------
        expansions = resolve_income_lists(shocks_p, ctx)
        il_report, il_warnings = expansion_report(expansions, original_columns)
        warnings.extend(il_warnings)
        expanded_rows = []
        for t in shocks_p.itertuples():
            if _is_income_list(t.metric):
                comps = expansions[t.metric]
                if t.op not in ("mult", "grow"):
                    raise MethodError(f"Income-list metric {t.metric!r} only supports "
                                      f"'mult'/'grow' ops (got {t.op!r})")
                present = il_report[t.metric]["scaled"]
                if not present:
                    raise MethodError(f"Income list {t.metric!r} has no components in the "
                                      "input dataset")
                for v in present:
                    expanded_rows.append({"channel": "scale", "metric": v, "group": t.group,
                                          "period": t.period, "op": t.op, "value": t.value,
                                          "_from": t.metric})
            else:
                expanded_rows.append({"channel": "scale", "metric": t.metric, "group": t.group,
                                      "period": t.period, "op": t.op, "value": t.value,
                                      "_from": ""})
        shocks_x = pd.DataFrame(expanded_rows)

        # --- cells + regional collapse ----------------------------------------
        used = cells.specs_in_shocks(shocks_x)
        df, dim_info = cells.prepare_cells(data, used)
        region_len = cells.data_region_len(df, used)
        # A variable's expansion source is per-metric (survives group collapse).
        origin = {m: f for m, f in zip(shocks_x["metric"], shocks_x["_from"]) if f}
        # allow_coarser: a NUTS-1 shock broadcasts to its NUTS-2 subregions via
        # prefix matching below — well-defined for scaling (not for count targets).
        collapsed, cwarnings = cells.collapse_shock_groups(
            shocks_x.drop(columns=["_from"]), used, region_len, allow_coarser=True)
        warnings.extend(cwarnings)

        # --- apply in sorted (metric, group) order ----------------------------
        applied = []
        # Rows already touched on each metric, kept per commuting family so the
        # overlap check below can tell "compounds harmlessly" from "depends on
        # which cell sorted first".
        family_masks: dict[tuple[str, str], pd.Series] = {}
        for t in collapsed.itertuples():
            if t.metric not in df.columns:
                raise MethodError(f"Variable {t.metric!r} is not a column of the input dataset")
            pairs = dims_mod.parse_group(t.group)
            mask = pd.Series(True, index=df.index)
            for k, v in pairs.items():
                if k == "region":
                    # NUTS codes are hierarchical: prefix match broadcasts a
                    # coarser shock over its subregions (exact codes still match).
                    mask &= df["_dim_region"].str.startswith(v)
                else:
                    mask &= df[f"_dim_{k}"] == v
            n_rows = int(mask.sum())
            if n_rows == 0:
                warnings.append(f"Shock cell {t.group!r} ({t.metric}) matches no one in the data")

            # Overlap policy: two shocks may touch the same person on the same
            # metric only when their ops commute, or the result depends on which
            # cell sorted first — an ordering no declarative shock table should
            # encode. mult/grow commute with each other (both multiply) and add
            # commutes with add; across families they do not, since
            # (x+a)*m != x*m+a. 'set' commutes with nothing, not even itself.
            family = OP_FAMILY[t.op]
            for (metric, prev_family), prev_mask in family_masks.items():
                if metric != t.metric or not bool((prev_mask & mask).any()):
                    continue
                if prev_family == family and family != "absolute":
                    continue                       # same family: compounds, order-free
                raise MethodError(
                    f"Shock on {t.metric!r} for cell {t.group!r} ({t.op!r}) overlaps an earlier "
                    f"{prev_family} shock on the same people. Those ops do not commute, so the "
                    f"result would depend on the order the cells happen to sort in. Use "
                    f"non-overlapping cells, or express both shocks with ops of the same kind.")
            key = (t.metric, family)
            family_masks[key] = (family_masks[key] | mask) if key in family_masks else mask

            # Integer-typed variables (counts, coded amounts) must widen to
            # float before a fractional factor lands on them.
            if not pd.api.types.is_float_dtype(df[t.metric]):
                df[t.metric] = df[t.metric].astype(float)
            # Weighted aggregate before/after = the macro size of this shock
            # (e.g. the total wage-bill change it injects) — reported in the
            # diagnostics so the caller can sanity-check against the macro
            # model's own aggregate.
            before = float((df.loc[mask, t.metric] * df.loc[mask, "dwt"]).sum())
            value = float(t.value)
            if t.op == "mult":
                df.loc[mask, t.metric] *= value
            elif t.op == "grow":
                df.loc[mask, t.metric] *= (1.0 + value)
            elif t.op == "add":
                df.loc[mask, t.metric] += value
            else:  # set
                df.loc[mask, t.metric] = value
            after = float((df.loc[mask, t.metric] * df.loc[mask, "dwt"]).sum())

            entry = {"metric": t.metric, "group": t.group, "op": t.op,
                     "value": round(value, 8), "n_rows": n_rows,
                     "weighted_sum_before": round(before, 2),
                     "weighted_sum_after": round(after, 2)}
            src = origin.get(t.metric, "")
            if src:
                entry["expanded_from"] = src
            applied.append(entry)

        diagnostics = {
            "period": period,
            "dimensions": dim_info,
            "applied": applied,
            "n_shocks_applied": len(applied),
            "income_list_expansions": il_report,
            "warnings": warnings,
        }
        out = df[original_columns]
        return MethodResult(data=out, diagnostics=diagnostics)


register(MethodSpec(
    name="scale_variables",
    summary="Scale numeric input variables (wages, hours, income components) "
            "per population cell — multiply, grow, add or set.",
    description=(
        "Cell-level arithmetic adjustment of EUROMOD input variables: each "
        "'scale' shock names a variable (e.g. yem) or an EUROMOD income list "
        "(e.g. ils_udb_yem — expanded extension-aware into its component input "
        "variables; mult/grow only) and applies mult (x value), grow "
        "(x (1+value)), add (+value) or set (=value) to every person in its "
        "cell. Cells combine any registered dimensions; shocks of different "
        "granularity may coexist and compose in sorted order (overlapping "
        "'set' is rejected as ambiguous). NUTS-2 shock regions collapse to the "
        "dataset's regional level with growth rates averaged. No rows are "
        "added or removed; the baseline is the untransformed run. Scenario "
        "params: period (optional when the shock table has one period)."),
    channels_consumed=("scale",),
    metrics_consumed=(),  # open: any input variable or income list
    cell_variables="Any input variable (deh=3-4, dgn=1, dag=25-34) and/or 'region'.",
    dataset_requirements=("idhh", "idperson", "dwt"),
    addon_requirements=((), ()),
    injected_columns=(),
    params_schema={
        "type": "object",
        "additionalProperties": False,
        "properties": {
            "period": {"type": "string",
                       "description": "External-model period label whose shocks to apply. "
                                      "Optional when the shock table has exactly one period."},
        },
    },
    factory=ScaleVariables,
))
