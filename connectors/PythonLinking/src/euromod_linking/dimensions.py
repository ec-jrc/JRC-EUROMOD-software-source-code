"""Population cells: how a shock's ``group`` selects rows of the microdata.

A group key is either a **EUROMOD input variable** (``deh=3-4``, ``dgn=1``,
``dag=25-34``, ``les=5``) or the one named dimension with real structure,
``region``, which resolves to the finest regional column the dataset carries
(drgn2, else drgn1, else country) and understands the NUTS hierarchy.

Expressing cells in the model's own variables is deliberate. Recoded category
labels — an external model's own "low/medium/high skill" classes, say — are
meaningless outside the binding that invented them, cannot be looked up
anywhere, and silently fix an aggregation an analyst may not want. ``deh=3-4`` is
self-describing, verifiable against the dataset, and needs no registry entry —
what a variable means and which coded values it takes are documented by EUROMOD
itself. An external model that publishes its own classes translates them to
variable ranges in its mapping spec, where model-specific semantics belong.

Value specs
-----------
::

    deh=3        exact value
    deh=3-4      inclusive range (``ddi=-1-0`` parses: signs are handled)
    dag=65+      open upper bound
    deh=3,5      set of values

Non-numeric values compare as strings (exact / set only).
"""

import logging
import re
from dataclasses import dataclass
from typing import Mapping

import pandas as pd

logger = logging.getLogger(__name__)


class DimensionError(ValueError):
    pass


# --- the one structural dimension --------------------------------------------

@dataclass(frozen=True)
class Dimension:
    name: str
    column: str
    fallback_columns: tuple[str, ...] = ()
    description: str = ""


REGION = Dimension(
    name="region", column="drgn2", fallback_columns=("drgn1",),
    description="Region at the finest level the dataset carries (drgn2, else drgn1, else "
                "country-wide). NUTS-aware: a coarser shock code covers its subregions, and "
                "finer shock codes collapse to the dataset's level.",
)

_NAMED: dict[str, Dimension] = {REGION.name: REGION}

# Recoded dimension names an external model might reach for, mapped to the
# EUROMOD variables that replace them, so the error can name the alternative.
_RETIRED = {
    "skill": "deh=0-2 (low), deh=3-4 (medium), deh=5-99 (high)",
    "gender": "dgn=1 (male), dgn=0 (female)",
    "age_band": "dag=25-34 (or any range)",
}


def get(name: str) -> Dimension | None:
    return _NAMED.get(name)


def registered() -> dict[str, Dimension]:
    return dict(_NAMED)


def _as_code_str(v) -> str:
    """Region codes may be stored as 12, 12.0 or "12" — normalize to "12"."""
    if pd.isna(v):
        return ""
    if isinstance(v, float) and v.is_integer():
        return str(int(v))
    return str(v).strip()


def region_column(df_columns, dim: Dimension = REGION) -> str | None:
    """Finest available region column ("" -> country level). Deterministic:
    the declared column first, then fallbacks in declared order."""
    for col in (dim.column, *dim.fallback_columns):
        if col in df_columns:
            return col
    return None


def derive_region(df: pd.DataFrame) -> pd.Series:
    """Region code label per row; empty when the dataset has no region column."""
    col = region_column(df.columns)
    if col is None:
        return pd.Series("", index=df.index)
    return df[col].map(_as_code_str)


# --- value specs --------------------------------------------------------------

_RANGE_RE = re.compile(r"^(-?\d+(?:\.\d+)?)\s*-\s*(-?\d+(?:\.\d+)?)$")
_OPEN_RE = re.compile(r"^(-?\d+(?:\.\d+)?)\s*\+$")
_NUM_RE = re.compile(r"^-?\d+(?:\.\d+)?$")


@dataclass(frozen=True)
class ValueSpec:
    """A parsed group value: what rows of one variable it selects."""
    raw: str
    kind: str                       # "exact" | "range" | "open" | "set" | "string"
    lo: float | None = None
    hi: float | None = None
    values: tuple = ()

    def bounds(self) -> tuple[float, float] | None:
        """Numeric interval covered, for overlap detection (None if string-valued)."""
        if self.kind == "range":
            return (self.lo, self.hi)
        if self.kind == "open":
            return (self.lo, float("inf"))
        if self.kind == "exact":
            return (self.lo, self.lo)
        if self.kind == "set" and self.values and all(isinstance(v, float) for v in self.values):
            return (min(self.values), max(self.values))
        return None


def parse_value_spec(spec: str) -> ValueSpec:
    """Parse a group value into a ValueSpec. Raises DimensionError."""
    s = str(spec).strip()
    if not s:
        raise DimensionError("Empty value spec")
    if "," in s:
        parts = [p.strip() for p in s.split(",") if p.strip()]
        if not parts:
            raise DimensionError(f"Empty value set {spec!r}")
        if all(_NUM_RE.match(p) for p in parts):
            return ValueSpec(s, "set", values=tuple(float(p) for p in parts))
        return ValueSpec(s, "set", values=tuple(parts))
    m = _RANGE_RE.match(s)
    if m:
        lo, hi = float(m.group(1)), float(m.group(2))
        if lo > hi:
            raise DimensionError(f"Range {spec!r} has its bounds reversed")
        return ValueSpec(s, "range", lo=lo, hi=hi)
    m = _OPEN_RE.match(s)
    if m:
        return ValueSpec(s, "open", lo=float(m.group(1)))
    if _NUM_RE.match(s):
        return ValueSpec(s, "exact", lo=float(s))
    return ValueSpec(s, "string", values=(s,))


def matches(series: pd.Series, spec: ValueSpec) -> pd.Series:
    """Boolean mask: which rows of `series` the spec selects."""
    if spec.kind == "string":
        return series.map(_as_code_str) == spec.values[0]
    numeric = pd.to_numeric(series, errors="coerce")
    if spec.kind == "exact":
        return numeric == spec.lo
    if spec.kind == "range":
        return numeric.between(spec.lo, spec.hi)
    if spec.kind == "open":
        return numeric >= spec.lo
    if spec.kind == "set":
        if all(isinstance(v, float) for v in spec.values):
            return numeric.isin(spec.values)
        return series.map(_as_code_str).isin([str(v) for v in spec.values])
    raise DimensionError(f"Unknown value-spec kind {spec.kind!r}")


def specs_overlap(a: ValueSpec, b: ValueSpec) -> bool:
    """True when two specs can select the same row — ambiguous cell membership."""
    if a.kind == "string" or b.kind == "string":
        return bool(set(a.values) & set(b.values))
    if a.kind == "set" or b.kind == "set":
        # Conservative: a set overlaps anything whose interval contains a member.
        sets, other = (a, b) if a.kind == "set" else (b, a)
        ob = other.bounds()
        if ob is None:
            return False
        if other.kind == "set":
            return bool(set(a.values) & set(b.values))
        return any(ob[0] <= v <= ob[1] for v in sets.values)
    ab, bb = a.bounds(), b.bounds()
    if ab is None or bb is None:
        return False
    return ab[0] <= bb[1] and bb[0] <= ab[1]


# --- group strings ------------------------------------------------------------

def parse_group(group: str) -> dict[str, str]:
    """Parse a canonical group string "k=v;k2=v2" into a dict ("" -> {})."""
    out: dict[str, str] = {}
    for part in str(group or "").split(";"):
        part = part.strip()
        if not part:
            continue
        k, sep, v = part.partition("=")
        k = k.strip()
        if not sep or not k:
            raise DimensionError(f"Malformed group component {part!r} (expected key=value)")
        if k in out:
            raise DimensionError(f"Group {group!r} repeats key {k!r}")
        out[k] = v.strip()
    return out


def canonical_group(pairs: Mapping[str, str] | str) -> str:
    """Canonical form: keys sorted, "k=v;k2=v2"."""
    if isinstance(pairs, str):
        pairs = parse_group(pairs)
    return ";".join(f"{k}={pairs[k]}" for k in sorted(pairs))


_KEY_RE = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")


def validate_group_syntax(group: str) -> list[str]:
    """Syntax-only problems with a group string ([] = ok).

    Deliberately does NOT check that a key is a real dataset column — that is a
    dataset-aware check (see validate_group_columns), applied once the dataset
    is known."""
    problems = []
    try:
        pairs = parse_group(group)
    except DimensionError as e:
        return [str(e)]
    for k, v in pairs.items():
        if not _KEY_RE.match(k):
            problems.append(f"Invalid group key {k!r} in {group!r}: expected an input-variable "
                            "name (or 'region')")
            continue
        if k in _RETIRED:
            problems.append(f"Group key {k!r} is no longer supported — use the EUROMOD variable "
                            f"directly, e.g. {_RETIRED[k]}")
            continue
        try:
            parse_value_spec(v)
        except DimensionError as e:
            problems.append(f"Group {group!r}: {e}")
    return problems


def validate_group_columns(group: str, columns) -> list[str]:
    """Dataset-aware problems: every key must be `region` or a dataset column."""
    problems = []
    for k in parse_group(group):
        if k == "region":
            if region_column(columns) is None:
                problems.append("Group uses 'region' but the dataset has no drgn2/drgn1 column")
        elif k not in columns:
            problems.append(f"Group key {k!r} is not a column of the input dataset. "
                            "Population cells are written over the dataset's own "
                            "EUROMOD input variables (deh, dgn, dag, les, ...)")
    return problems
