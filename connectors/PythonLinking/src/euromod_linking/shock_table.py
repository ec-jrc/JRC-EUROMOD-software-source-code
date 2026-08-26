"""Canonical shock table — the interchange format every external-model shock
must be expressed in before it can drive a EUROMOD scenario.

One tidy record per shock::

    channel  align|constant|scale|reweight|inject   which EUROMOD lever
    metric   e.g. "employment", "$f_cpi", "yem"      channel-specific referent
    group    canonical "key=value;..." string        population cell ("" = all)
    period   external-model period label (str)
    op       set|grow|mult|add                       value semantics
    value    number
    unit     optional, documentation only
    source   optional provenance (file/sheet/row)

A table's identity is its content: `content_id` hashes the canonical records
into ``shk_`` + 12 hex digits, so the same economic scenario always has the same
id whatever file or code path produced it. Nothing is stored — the id is derived
on demand, and the table itself is a DataFrame the caller holds.
"""

import hashlib
import json
import logging

import pandas as pd

from euromod_linking import dimensions

logger = logging.getLogger(__name__)

CHANNELS = ("align", "constant", "scale", "reweight", "inject")
OPS = ("set", "grow", "mult", "add")
COLUMNS = ("channel", "metric", "group", "period", "op", "value", "unit", "source")


class ShockTableError(ValueError):
    def __init__(self, problems: list[str]):
        super().__init__("; ".join(problems[:5]) + (f" (+{len(problems)-5} more)" if len(problems) > 5 else ""))
        self.problems = problems


def normalize(records: list[dict]) -> pd.DataFrame:
    """Validate + canonicalise raw records into the canonical DataFrame,
    sorted by (channel, metric, group, period). Raises ShockTableError."""
    if not isinstance(records, list) or not records:
        raise ShockTableError(["shocks must be a non-empty list of records"])
    problems: list[str] = []
    rows = []
    for i, r in enumerate(records):
        if not isinstance(r, dict):
            problems.append(f"record {i}: not an object")
            continue
        channel = str(r.get("channel", "")).strip()
        metric = str(r.get("metric", "")).strip()
        op = str(r.get("op", "")).strip()
        period = str(r.get("period", "")).strip()
        if channel not in CHANNELS:
            problems.append(f"record {i}: channel {channel!r} not in {CHANNELS}")
        if not metric:
            problems.append(f"record {i}: metric is required")
        if op not in OPS:
            problems.append(f"record {i}: op {op!r} not in {OPS}")
        try:
            value = float(r.get("value"))
            if not (value == value and abs(value) != float("inf")):  # NaN/inf
                raise ValueError
        except (TypeError, ValueError):
            problems.append(f"record {i}: value {r.get('value')!r} is not a finite number")
            value = float("nan")
        group = ""
        try:
            group = dimensions.canonical_group(str(r.get("group", "") or ""))
        except dimensions.DimensionError as e:
            problems.append(f"record {i}: {e}")
        if channel != "constant":  # constants address parameters, not population cells
            # Syntax only here: whether a key is a real dataset column can only
            # be judged once the dataset is known (run_scenario's dataset-aware
            # stage does that, and reports how many people each cell matched).
            for p in dimensions.validate_group_syntax(group):
                problems.append(f"record {i}: {p}")
        rows.append({
            "channel": channel, "metric": metric, "group": group, "period": period,
            "op": op, "value": value,
            "unit": str(r.get("unit", "") or ""), "source": str(r.get("source", "") or ""),
        })
    if problems:
        raise ShockTableError(problems)
    df = pd.DataFrame(rows, columns=list(COLUMNS))
    df = df.sort_values(["channel", "metric", "group", "period"], kind="mergesort").reset_index(drop=True)
    dup = df.duplicated(["channel", "metric", "group", "period"], keep=False)
    if dup.any():
        d = df[dup][["channel", "metric", "group", "period"]].drop_duplicates().head(3)
        raise ShockTableError([f"duplicate shock for {t.channel}/{t.metric}/{t.group!r}/period {t.period}"
                               for t in d.itertuples()])
    return df


def content_id(df: pd.DataFrame) -> str:
    """Deterministic id from shock content (unit/source excluded, so identical
    shocks from different files dedupe)."""
    core = df[["channel", "metric", "group", "period", "op", "value"]]
    blob = json.dumps(core.values.tolist(), sort_keys=False, default=str)
    return "shk_" + hashlib.sha256(blob.encode("utf-8")).hexdigest()[:12]


def summarize(df: pd.DataFrame) -> dict:
    groups = sorted(df["group"].unique())
    return {
        "n_shocks": int(len(df)),
        "channels": sorted(df["channel"].unique()),
        "metrics": sorted(df["metric"].unique()),
        "periods": sorted(df["period"].unique()),
        "n_groups": len(groups),
        "groups_sample": groups[:10],
    }


def describe(df: pd.DataFrame, **extra) -> dict:
    """The id and summary of a table, as it appears in a scenario result."""
    return {"shock_table_id": content_id(df), **summarize(df), **extra}
