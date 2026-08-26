"""Shared cell machinery for method methodologies.

Turns shock-table group strings into population cells on the input microdata:
dimension label derivation, region-level collapse, op semantics, and period
resolution. Used by every methodology; lives outside any single method.
"""

import logging

import pandas as pd

from euromod_linking import dimensions as dims_mod
from euromod_linking.methods.base import MethodError

logger = logging.getLogger(__name__)

CELL_COL = "_cell"


def specs_in_shocks(shocks: pd.DataFrame) -> dict[str, list[str]]:
    """Group key -> sorted distinct value specs used across (non-constant) shocks.

    Cells are defined by the shock table itself: the specs a scenario actually
    uses (e.g. deh: ["0-2", "3-4"]) become that variable's categories for this
    run. Nothing is recoded ahead of time, so any input variable works and the
    labels stay readable in the diagnostics."""
    out: dict[str, set[str]] = {}
    for g in shocks.loc[shocks["channel"] != "constant", "group"].unique():
        for k, v in dims_mod.parse_group(g).items():
            out.setdefault(k, set()).add(v)
    return {k: sorted(v) for k, v in sorted(out.items())}


def prepare_cells(df: pd.DataFrame, used: dict[str, list[str]]) -> tuple[pd.DataFrame, dict]:
    """Add per-key label columns (_dim_<key>) + CELL_COL, labelling each row
    with the value spec it satisfies. Returns (df, info)."""
    df = df.copy()
    info: dict = {}
    for key, specs in used.items():
        if key == "region":
            df["_dim_region"] = dims_mod.derive_region(df).astype(str)
            info[key] = {"level": dims_mod.region_column(df.columns) or "country"}
            continue
        if key not in df.columns:
            raise MethodError(f"Group key '{key}' is not a column of the input dataset")
        parsed = []
        for s in specs:
            try:
                parsed.append(dims_mod.parse_value_spec(s))
            except dims_mod.DimensionError as e:
                raise MethodError(f"{key}: {e}")
        # Overlapping specs would put a row in two cells at once — the target
        # totals would then double-count it.
        for i, a in enumerate(parsed):
            for b in parsed[i + 1:]:
                if dims_mod.specs_overlap(a, b):
                    raise MethodError(
                        f"Overlapping value specs for '{key}': {a.raw!r} and {b.raw!r} "
                        "can select the same person")
        labels = pd.Series("", index=df.index)
        for spec in parsed:
            labels = labels.mask(dims_mod.matches(df[key], spec), spec.raw)
        df[f"_dim_{key}"] = labels
        info[key] = {"column": key, "specs": [p.raw for p in parsed]}
    keys = list(used)
    if keys:
        df[CELL_COL] = df[[f"_dim_{k}" for k in keys]].agg(
            lambda r: ";".join(f"{k}={v}" for k, v in zip(keys, r)), axis=1)
    else:
        df[CELL_COL] = ""
    return df, info


def data_region_len(df: pd.DataFrame, used) -> int:
    """Representative code length of the data's region labels (0 = country)."""
    if "region" not in used:
        return 0
    labels = [v for v in df["_dim_region"].unique() if v not in ("", "nan")]
    if not labels:
        return 0
    return int(pd.Series([len(v) for v in labels]).mode().iloc[0])


def collapse_shock_groups(shocks: pd.DataFrame, used, region_len: int,
                          allow_coarser: bool = False,
                          intensive_metrics=frozenset()) -> tuple[pd.DataFrame, list[str]]:
    """Rewrite shock groups to the data's granularity. Region codes finer than
    the data are truncated to region_len (0 -> drop the region key); values are
    aggregated per (metric, collapsed group): grow/mult by mean, set/add by sum.

    Region codes *coarser* than the data are an error by default (count targets
    cannot be disaggregated); with allow_coarser=True they are kept as-is for
    the caller to prefix-match (broadcast semantics — correct for scaling).

    Why mean vs sum: growth rates and multipliers are *intensive* quantities —
    merging two NUTS-2 rates into their NUTS-1 parent takes an average, with
    equal weights, since without subregional population counts in the microdata
    there is no defensible weighting. Levels and absolute deltas (set/add) are
    *extensive* — subregional amounts add up.

    ``intensive_metrics`` names metrics that are intensive whatever the op —
    rates, where a value is a share or a percentage-point change. Without it a
    ``set``/``add`` on a rate would be summed, so two NUTS-2 shocks of -3.5pp
    would become -7pp in their NUTS-1 parent: silently double the intended size.
    """
    warnings: list[str] = []
    rows = []
    for t in shocks.itertuples():
        pairs = dims_mod.parse_group(t.group)
        if "region" in pairs:
            code = pairs["region"]
            if region_len == 0:
                del pairs["region"]
            elif len(code) > region_len:
                pairs["region"] = code[:region_len]
            elif len(code) < region_len and not allow_coarser:
                raise MethodError(
                    f"Shock region '{code}' is coarser than the dataset's region level "
                    f"({region_len}-char codes); cannot disaggregate")
        rows.append({"metric": t.metric, "group": dims_mod.canonical_group(pairs),
                     "op": t.op, "value": t.value})
    out = pd.DataFrame(rows)

    ops = out.groupby(["metric", "group"])["op"].nunique()
    mixed = ops[ops > 1]
    if not mixed.empty:
        raise MethodError(f"Mixed ops within a collapsed cell: {list(mixed.index)[:3]}")

    # Flagged before the groupby: 'metric' is a grouping key, so include_groups=False
    # removes it from the frame _agg sees. Constant within a group either way —
    # metric is the key and op is unique per group (checked just above).
    out["_intensive"] = out["op"].isin(("grow", "mult")) | out["metric"].isin(intensive_metrics)

    def _agg(g: pd.DataFrame) -> pd.Series:
        value = g["value"].mean() if bool(g["_intensive"].iloc[0]) else g["value"].sum()
        return pd.Series({"op": g["op"].iloc[0], "value": value, "n_source": len(g)})

    collapsed = (out.groupby(["metric", "group"], sort=True)
                 .apply(_agg, include_groups=False).reset_index())
    n_merged = int((collapsed["n_source"] > 1).sum())
    if n_merged:
        warnings.append(f"{n_merged} shock cells collapsed to the dataset's regional level "
                        "(growth rates averaged with equal weights)")
    return collapsed, warnings


def apply_op(current: float, op: str, value: float) -> float:
    if op == "grow":
        return current * (1.0 + value)
    if op == "mult":
        return current * value
    if op == "add":
        return current + value
    return value  # set


def resolve_period(shocks: pd.DataFrame, params: dict) -> str:
    """The period whose shocks to apply: params['period'] when given, else the
    table's single distinct period; ambiguity is an error listing the options."""
    period = params.get("period")
    if period is not None and str(period).strip():
        return str(period)
    periods = sorted(shocks["period"].unique())
    if len(periods) == 1:
        return periods[0]
    raise MethodError(f"params.period is required: the shock table has {len(periods)} "
                      f"periods {periods[:12]}")
