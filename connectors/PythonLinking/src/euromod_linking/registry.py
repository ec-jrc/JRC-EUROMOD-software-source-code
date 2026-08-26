"""Method registry — the named linkage methodologies.

A *method* is a reviewed implementation that turns external-model shocks (the
canonical shock table) into a transformed EUROMOD input DataFrame plus run
parameters. Methodology lives in code; a scenario can only select a method and
supply scenario-semantics params validated against the method's own
params_schema.

Methods carry no version number. What guards against an edit to the science
silently serving results computed by earlier code is `code_fingerprint`, which
hashes a method's own source into the scenario fingerprint — a content-derived
identity that cannot be forgotten the way a hand-maintained version integer can.
"""

from dataclasses import dataclass, field
from typing import Callable


class MethodLookupError(KeyError):
    def __init__(self, message: str, available: list[str]):
        super().__init__(message)
        self.available = available

    def __str__(self):
        return self.args[0]


@dataclass(frozen=True)
class MethodSpec:
    """A methodology's published contract: what it consumes, what it needs, and
    what it does to the input.

    ``name`` is how a methodology is referred to everywhere else — in dispatch,
    in a scenario's ``methodology`` pin, and in results.
    """

    name: str                              # e.g. "lma_labour_alignment"
    summary: str                           # one-liner for the method list
    description: str                       # full methodology description
    channels_consumed: tuple[str, ...]     # shock channels this method understands
    metrics_consumed: tuple[str, ...]      # metrics within those channels
    cell_variables: str                    # human description of what can define target cells
    dataset_requirements: tuple[str, ...]  # method's own required input columns
    addon_requirements: tuple              # (addon_entries, switch_entries) applied to both runs
    injected_columns: tuple[str, ...]      # numeric columns the method adds to the input
    params_schema: dict                    # JSON Schema for scenario "params" (additionalProperties: false)
    restructures_rows: bool = False        # True when apply() adds/removes rows (e.g. household
                                           # weight splits) — the baseline is then rebuilt on the
                                           # same rows so the two runs stay observation-paired
    min_model_release: str | None = None   # earliest EUROMOD release shipping what this method
                                           # needs, e.g. "J2.54" — advisory only, see compat.py
    factory: Callable = field(repr=False, default=None)  # () -> method instance


_REGISTRY: dict[str, MethodSpec] = {}


def register(spec: MethodSpec) -> MethodSpec:
    _REGISTRY[spec.name] = spec
    return spec


def available_names() -> list[str]:
    return sorted(_REGISTRY)


def resolve(name: str) -> MethodSpec:
    """The method registered under `name`."""
    name = str(name or "").strip()
    if name in _REGISTRY:
        return _REGISTRY[name]
    base, sep, _ = name.partition("@")
    if sep and base.strip() in _REGISTRY:
        raise MethodLookupError(
            f"Unknown method {name!r}. Methods are named without a version — "
            f"use {base.strip()!r}.", available_names())
    raise MethodLookupError(f"Unknown method {name!r}", available_names())


def list_specs() -> list[MethodSpec]:
    return [_REGISTRY[k] for k in sorted(_REGISTRY)]


_code_fp_cache: dict[str, str] = {}


def code_fingerprint(spec: MethodSpec) -> str:
    """Content hash of the methodology's own source.

    Results are cached on the scenario fingerprint, but a scenario document does
    not change when the *methodology* does — so without this, editing a
    method's science silently serves results computed by the earlier code.
    Hashing the implementation makes any code change invalidate its cached runs,
    the same content-addressed discipline used for shock tables and scores.
    Empty string if the source cannot be read, which leaves caching keyed on the
    methodology name alone."""
    if spec.name in _code_fp_cache:
        return _code_fp_cache[spec.name]
    import hashlib
    import inspect
    from pathlib import Path

    fp = ""
    try:
        path = Path(inspect.getfile(spec.factory))
        files = sorted(path.parent.rglob("*.py")) if path.name == "__init__.py" else [path]
        h = hashlib.sha256()
        for f in files:
            h.update(f.name.encode())
            h.update(f.read_bytes())
        fp = h.hexdigest()[:12]
    except Exception:
        pass
    _code_fp_cache[spec.name] = fp
    return fp


def resolve_for_channels(channels: set[str], metrics: set[str]) -> MethodSpec:
    """Dispatch: the unique registered methodology covering the given shock
    channels (and metrics, where the spec constrains them). Users never select
    a methodology — it is resolved from the shocks and echoed in the response;
    an explicit scenario pin exists only for reproduction and for the day two
    methodologies claim the same channel."""
    candidates = [spec for spec in _REGISTRY.values()
                  if channels <= set(spec.channels_consumed)
                  and (not spec.metrics_consumed or metrics <= set(spec.metrics_consumed))]
    if not candidates:
        supported = sorted({c for s in _REGISTRY.values() for c in s.channels_consumed})
        raise MethodLookupError(
            f"No methodology handles shock channels {sorted(channels)}"
            + (f" with metrics {sorted(metrics)}" if metrics else "")
            + f". Supported channels: {supported}",
            available_names())
    if len(candidates) > 1:
        names = sorted(s.name for s in candidates)
        raise MethodLookupError(
            f"Multiple methodologies handle channels {sorted(channels)}: {names}. "
            "Pin one via the scenario's 'methodology' field.", names)
    return candidates[0]
