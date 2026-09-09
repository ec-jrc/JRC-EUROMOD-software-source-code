"""Method registry — the named linkage methodologies.

A *method* is a reviewed implementation that turns external-model shocks (the
canonical shock table) into a transformed EUROMOD input DataFrame plus run
parameters. Methodology lives in code; a scenario can only select a method and
supply scenario-semantics params validated against the method's own
params_schema.

Each method consumes one kind of shock — a *channel* — and dispatch is per
channel: a shock table carrying several channels is handled by several methods,
one each, run in **stage** order. A method's stage says what kind of
transformation it is, and that is what fixes the order when methods share a
scenario. There is no method for a combination of channels; the combination is
a property of the scenario, and the scenario engine composes the methods.

Methods carry no version number. What guards against an edit to the modelling
silently serving results computed by earlier code is `code_fingerprint`, which
hashes a method's own source into the scenario fingerprint — a content-derived
identity that cannot be forgotten the way a hand-maintained version integer can.
"""

from dataclasses import dataclass, field
from typing import Callable, Iterable


class MethodLookupError(KeyError):
    def __init__(self, message: str, available: list[str]):
        super().__init__(message)
        self.available = available

    def __str__(self):
        return self.args[0]


# --- stages -------------------------------------------------------------------
#
# The order methods run in when a scenario carries several channels. A stage is
# a property of what a method *does to the input*, not of any pairing with
# another method, so a new method picks one stage and composes with every other
# method without a composite being written for each pair.
#
# Values first, then people. The values a method changes — wages, hours, income
# components — are the environment the later transitions happen in:
# lma_labour_alignment pays a new worker their own predicted hourly wage yivwg,
# an input variable, so scaling first is what makes an entrant enter at
# counterfactual wages. Moving people first and scaling after would leave
# entrants on baseline wages, because their earnings land in yem_a, which a
# scale shock does not reach.
#
# Methods sharing a stage are assumed to commute; within a stage they run in
# name order, which must not matter.
STAGE_VALUES = 10   # arithmetic on what the input records; rows and people unchanged
STAGE_PEOPLE = 20   # who is in which state: transitions, household weight splits
STAGE_WEIGHTS = 30  # reserved: reweighting the whole sample to external totals


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
    stage: int = STAGE_VALUES              # when it runs relative to other methods, see above
    restructures_rows: bool = False        # True when apply() adds/removes rows (e.g. household
                                           # weight splits) — the baseline is then rebuilt on the
                                           # same rows so the two runs stay observation-paired
    preview_by_applying: bool = False      # apply() is cheap enough to run on the validation
                                           # path, so later stages preview against its output
                                           # rather than against the untouched input
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


# --- dispatch -----------------------------------------------------------------

def resolve_for_channel(channel: str, metrics: set[str] = frozenset()) -> MethodSpec:
    """Dispatch for one channel: the unique registered methodology consuming it
    (and these metrics, where the spec constrains them).

    Users never select a methodology — it is resolved from the shocks and
    echoed in the response; an explicit scenario pin exists only for
    reproduction and for the day two methodologies claim the same channel."""
    candidates = [spec for spec in _REGISTRY.values()
                  if channel in spec.channels_consumed
                  and (not spec.metrics_consumed or set(metrics) <= set(spec.metrics_consumed))]
    if not candidates:
        supported = sorted({c for s in _REGISTRY.values() for c in s.channels_consumed})
        raise MethodLookupError(
            f"No methodology handles shock channel {channel!r}"
            + (f" with metrics {sorted(metrics)}" if metrics else "")
            + f". Supported channels: {supported}",
            available_names())
    if len(candidates) > 1:
        names = sorted(s.name for s in candidates)
        raise MethodLookupError(
            f"Multiple methodologies handle channel {channel!r}: {names}. "
            "Pin one via the scenario's 'methodology' field.", names)
    return candidates[0]


def resolve_for_channels(channels_metrics: dict[str, set[str]]) -> list[MethodSpec]:
    """Dispatch for a whole shock table: ``{channel: metrics}`` -> the methods
    that handle it, one per channel, in the order they run.

    Each channel dispatches on its own, so a mistyped metric on one channel is
    refused here, by name, whatever else the table carries."""
    specs = [resolve_for_channel(ch, metrics) for ch, metrics in sorted(channels_metrics.items())]
    return pipeline(specs)


def pipeline(specs: Iterable[MethodSpec]) -> list[MethodSpec]:
    """The given methods in the order they run: by stage, then name; each once.

    Refuses two methods claiming one channel — a scenario in which the same
    shock would be applied twice, or in which which of them applies depends on
    iteration order."""
    unique: dict[str, MethodSpec] = {}
    for spec in specs:
        unique.setdefault(spec.name, spec)
    ordered = sorted(unique.values(), key=lambda s: (s.stage, s.name))
    claimed: dict[str, str] = {}
    for spec in ordered:
        for ch in spec.channels_consumed:
            if ch in claimed and claimed[ch] != spec.name:
                raise MethodLookupError(
                    f"Methods {claimed[ch]!r} and {spec.name!r} both consume channel {ch!r}; "
                    "a scenario cannot run both.", [claimed[ch], spec.name])
            claimed[ch] = spec.name
    return ordered


def resolve_pipeline(reference: str) -> list[MethodSpec]:
    """The methods a scenario's ``methodology`` pin names, in run order.

    A pin is a single name, or several joined with ``+`` — the form
    ``pipeline_name`` produces and results echo, so a pinned reproduction can be
    pasted back from an earlier response."""
    names = [n.strip() for n in str(reference or "").split("+") if n.strip()]
    if not names:
        raise MethodLookupError("Empty methodology reference", available_names())
    return pipeline(resolve(n) for n in names)


def pipeline_name(specs: Iterable[MethodSpec]) -> str:
    """The reference for a run's methods: ``scale_variables+lma_labour_alignment``.

    In run order, so the name says what happened first. Accepted back as a pin."""
    return "+".join(s.name for s in specs)


# --- fingerprints -------------------------------------------------------------

_code_fp_cache: dict[str, str] = {}


def code_fingerprint(spec: MethodSpec) -> str:
    """Content hash of the methodology's own source.

    Results are cached on the scenario fingerprint, but a scenario document does
    not change when the *methodology* does — so without this, editing a
    method's modelling silently serves results computed by the earlier code.
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


def pipeline_fingerprint(specs: Iterable[MethodSpec]) -> str:
    """One fingerprint for every method a run uses, in run order.

    The order is part of what a run does, so it is part of the hash: the same
    two methods in the other order would be a different transformation. Empty
    when there are no methods (a constants-only scenario)."""
    import hashlib

    specs = list(specs)
    if not specs:
        return ""
    if len(specs) == 1:
        return code_fingerprint(specs[0])
    h = hashlib.sha256()
    for spec in specs:
        h.update(spec.name.encode())
        h.update(code_fingerprint(spec).encode())
    return h.hexdigest()[:12]
