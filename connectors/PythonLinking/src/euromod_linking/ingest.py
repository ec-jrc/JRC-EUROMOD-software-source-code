"""Universal shock ingest: (external model output file, mapping spec) ->
canonical shock-table records.

Mapping specs are declarative, repo-versioned YAML files in
``euromod_linking/mappings/``, validated against ``mapping.v1.schema.json``.
Anything a spec cannot express becomes a new *named transform* here — mappings
stay data, code stays reviewed.
"""

import logging
import re
from pathlib import Path

from euromod_linking.resources import load_mapping_text, load_schema, mapping_names

logger = logging.getLogger(__name__)


class IngestError(ValueError):
    pass


def _schema(name: str) -> dict:
    return load_schema(name)


def list_mappings() -> list[dict]:
    """Available mapping specs: [{name, description, file}]."""
    out = []
    for name in mapping_names():
        try:
            spec = load_mapping(name)
            out.append({"name": spec["name"], "description": spec["description"],
                        "file": f"{name}.yaml"})
        except Exception as e:
            out.append({"name": name, "error": str(e), "file": f"{name}.yaml"})
    return out


def load_mapping(name_or_spec) -> dict:
    """Load + schema-validate a mapping spec by name (mappings/{name}.yaml)
    or as an already-parsed dict."""
    import jsonschema
    import yaml

    if isinstance(name_or_spec, dict):
        spec = name_or_spec
    else:
        name = str(name_or_spec).strip()
        if not re.fullmatch(r"[a-z0-9_]+", name):
            raise IngestError(f"Invalid mapping name {name!r}")
        text = load_mapping_text(name)
        if text is None:
            raise IngestError(f"Unknown mapping {name!r}. Available: {mapping_names()}")
        spec = yaml.safe_load(text)
    try:
        jsonschema.validate(spec, _schema("mapping.v1.schema.json"))
    except jsonschema.ValidationError as e:
        raise IngestError(f"Mapping spec invalid at {'/'.join(map(str, e.path)) or '<root>'}: {e.message}")
    return spec


# --- named transforms ---------------------------------------------------------

def _t_identity(v: str) -> str:
    return v


def _t_strip(v: str) -> str:
    return v.strip()


_NUTS_RE = re.compile(r"^([A-Z]{2})(.*)$")


def _t_nuts_code(v: str) -> str:
    """NUTS code -> sub-country part: 'AT12' -> '12', 'AT1' -> '1', 'AT' -> ''."""
    m = _NUTS_RE.match(v.strip().upper())
    if not m:
        raise IngestError(f"Value {v!r} is not a NUTS code (expected 2-letter country prefix)")
    return m.group(2)


def _nuts_country(v: str) -> str:
    m = _NUTS_RE.match(str(v).strip().upper())
    return m.group(1) if m else ""


_TRANSFORMS = {"identity": _t_identity, "strip": _t_strip, "nuts_code": _t_nuts_code}


# --- reading ------------------------------------------------------------------

def _resolve_column(df, role: str, spec_cols: dict):
    entry = (spec_cols or {}).get(role)
    if entry is None:
        raise IngestError(f"Mapping does not declare a column for role '{role}'")
    if "name" in entry:
        if entry["name"] not in df.columns:
            raise IngestError(f"Column {entry['name']!r} (role '{role}') not found in file")
        return entry["name"]
    pos = int(entry["position"])
    if pos >= len(df.columns):
        raise IngestError(f"Column position {pos} (role '{role}') out of range ({len(df.columns)} columns)")
    return df.columns[pos]


def _period_columns(df, periods_spec: dict) -> list[tuple[str, object]]:
    """[(period_label, column)] for numbered_columns mode."""
    lo, hi = periods_spec.get("range", [1, 10])
    out = []
    for col in df.columns:
        try:
            f = float(str(col).strip())
            n = int(f) if f.is_integer() else None
        except (TypeError, ValueError):
            n = None
        if n is not None and lo <= n <= hi:
            out.append((str(n), col))
    if not out:
        raise IngestError(f"No period columns with headers {lo}..{hi} found (columns: {list(df.columns)[:8]}...)")
    return out


def _apply_filters(df, filters: list, spec_cols: dict):
    for f in filters or []:
        col = _resolve_column(df, f["column"], spec_cols) if f["column"] in (spec_cols or {}) else f["column"]
        if f.get("not_null"):
            df = df[df[col].notna()]
        if "equals" in f:
            df = df[df[col] == f["equals"]]
    return df


def ingest(file_path: str, mapping, country: str | None = None) -> tuple[list[dict], list[str]]:
    """Read an external model output file through a mapping spec.

    Returns (records, warnings). ``country`` filters rows to one country when a
    group derivation uses the nuts_code transform, for files covering several.
    Raises IngestError on structural problems.
    """
    import pandas as pd

    spec = load_mapping(mapping)
    path = Path(file_path)
    if not path.exists():
        raise IngestError(f"File not found: {file_path}")

    fmt = spec["reader"]["format"]
    spec_cols = spec.get("columns", {})
    group_spec = spec.get("group", {})
    op = spec["value_semantics"]["op"]
    unit = spec["value_semantics"].get("unit", "")
    cc = (country or "").strip().upper() or None

    records: list[dict] = []
    warnings: list[str] = []

    for sheet_spec in spec["reader"]["sheets"]:
        sheet = sheet_spec["sheet"]
        if fmt == "excel":
            try:
                df = pd.read_excel(path, sheet_name=sheet)
            except ValueError as e:
                raise IngestError(f"Sheet {sheet!r} not readable from {path.name}: {e}")
        else:
            df = pd.read_csv(path)

        df = _apply_filters(df, spec.get("filters"), spec_cols)

        pcols = _period_columns(df, spec.get("periods", {})) \
            if spec.get("periods", {}).get("mode", "numbered_columns") == "numbered_columns" \
            else [("", spec["periods"]["column"])]

        # Pre-resolve group role columns once per sheet.
        gcols = {key: _resolve_column(df, g["from"], spec_cols) for key, g in group_spec.items()}
        # `as:` renames the emitted key (external label -> EUROMOD variable);
        # `values:` translates the external model's own classes into value specs
        # (a "medium skill" class -> deh 3-4, say). Both keep model-specific
        # recodings inside the mapping, so shock tables speak EUROMOD variables only.
        gkeys = {key: g.get("as", key) for key, g in group_spec.items()}

        n_skipped_country = 0
        for _, row in df.iterrows():
            pairs = {}
            bad = False
            for key in sorted(group_spec):
                g = group_spec[key]
                raw = row[gcols[key]]
                if pd.isna(raw):
                    bad = True
                    break
                raw = str(raw)
                if g.get("transform") == "nuts_code" and cc and _nuts_country(raw) != cc:
                    n_skipped_country += 1
                    bad = True
                    break
                val = _TRANSFORMS[g.get("transform", "identity")](raw)
                allowed = g.get("allowed")
                if allowed and val not in allowed:
                    raise IngestError(
                        f"Sheet {sheet!r}: value {val!r} for '{key}' not in allowed {allowed}")
                values = g.get("values")
                if values:
                    if val not in values:
                        raise IngestError(
                            f"Sheet {sheet!r}: value {val!r} for '{key}' has no translation "
                            f"(mapping declares {sorted(values)})")
                    val = str(values[val])
                pairs[gkeys[key]] = val
            if bad:
                continue
            group = ";".join(f"{k}={pairs[k]}" for k in sorted(pairs))
            for period, pcol in pcols:
                v = row[pcol]
                if pd.isna(v):
                    continue
                records.append({
                    "channel": sheet_spec["channel"], "metric": sheet_spec["metric"],
                    "group": group, "period": period, "op": op, "value": float(v),
                    "unit": unit, "source": f"{path.name}#{sheet}",
                })
        if n_skipped_country and cc:
            warnings.append(f"Sheet {sheet!r}: {n_skipped_country} rows for other countries skipped (kept {cc})")

    if not records:
        raise IngestError(f"No shock records produced from {path.name}"
                          + (f" for country {cc}" if cc else ""))
    return records, warnings
