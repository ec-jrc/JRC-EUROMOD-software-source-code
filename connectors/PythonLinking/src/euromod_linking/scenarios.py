"""Running a linking scenario: validate, transform, (optionally) simulate.

Two entry points:

``apply_scenario(system, data, scenario)``
   The core. Turns external-model shocks into transformed EUROMOD input —
   the counterfactual and, when a methodology restructures rows, a matching
   baseline built on the same rows. Executes nothing.

``run_scenario(system, scenario, input_path=...)``
   Convenience: loads the dataset, applies the scenario and runs both halves
   through ``System.run()``.

Applications that need caching, retries or their own response envelope build on
`apply_scenario` and run the frames themselves; that is why nothing here knows
about caches or run ids.

The API exposes *shocks*, not methodologies: each shock channel in the table is
dispatched to the methodology that consumes it, and the resolved methods are
reported back. A table carrying several channels is handled by several methods,
run one after another in their declared stage order — see
:mod:`euromod_linking.registry` for why the order is theirs and not the
caller's. An optional ``methodology`` pin exists only for exact reproduction or
disambiguation.

Constants semantics:

- scenario ``constants`` are context, applied to BOTH runs, so the delta
  isolates the shock;
- shock records with ``channel: constant`` are part of the shock and apply to
  the COUNTERFACTUAL only (metric = constant name, period = constant group,
  op must be ``set``). A constants-only shock table needs no methodology.

Errors are exceptions (`ScenarioError` carrying `.problems`, `RunError`), not
error dicts — callers that need a machine-readable envelope build it themselves.
"""

import hashlib
import json
import logging

from euromod_linking import dimensions, ingest as ingest_mod, registry, shock_table
from euromod_linking.resources import load_schema
from euromod_linking.runner import RunError, execute, frames_identical, resolve_dataset

logger = logging.getLogger(__name__)


class ScenarioError(ValueError):
    """A scenario cannot be applied. ``.problems`` lists every problem found,
    not just the first, so one call surfaces everything to fix."""

    def __init__(self, problems: list[str]):
        super().__init__(problems[0] if problems else "invalid scenario")
        self.problems = problems


# --- the scenario document ----------------------------------------------------

def _validate_structure(scenario: dict) -> list[str]:
    """JSON-Schema problems with the scenario document ([] = ok)."""
    import jsonschema

    validator = jsonschema.Draft202012Validator(load_schema("scenario.v1.schema.json"))
    problems = []
    for err in sorted(validator.iter_errors(scenario), key=lambda e: list(e.path)):
        loc = "/".join(map(str, err.path)) or "<root>"
        problems.append(f"{loc}: {err.message}")
    return problems


def params_schema(specs) -> dict:
    """The one ``params`` schema a scenario is validated against: the union of
    its methods' schemas.

    ``params`` are scenario semantics — *which* external-model period, a
    reporting threshold — and one scenario has one set of them however many
    methods it runs, so every method reads the same ``period``. The union keeps
    ``additionalProperties: false``, which is what blocks methodology knobs: a
    param no method declares is still an error."""
    properties, required = {}, []
    for spec in specs:
        schema = spec.params_schema or {}
        for name, sub in (schema.get("properties") or {}).items():
            properties.setdefault(name, sub)   # first (earliest stage) wins on a clash
        for name in schema.get("required") or ():
            if name not in required:
                required.append(name)
    out = {"type": "object", "additionalProperties": False, "properties": properties}
    if required:
        out["required"] = required
    return out


def _validate_params(scenario: dict, specs) -> list[str]:
    """Validate scenario['params'] against the methods' merged params_schema."""
    import jsonschema

    problems = []
    validator = jsonschema.Draft202012Validator(params_schema(specs))
    for err in sorted(validator.iter_errors(scenario.get("params") or {}), key=lambda e: list(e.path)):
        loc = "/".join(map(str, err.path)) or "params"
        problems.append(f"params/{loc}: {err.message}")
    return problems


def _resolve_shocks(scenario: dict) -> tuple:
    """Resolve scenario['shocks'] to (DataFrame, shock_table_id, summary, warnings).

    Two forms: {inline: [records]} or {file, mapping} (ingested here). The id is
    derived from the canonical records, so re-running either form on the same
    shocks yields the same id without anything being stored. Raises ScenarioError.
    """
    shocks = scenario.get("shocks") or {}
    warnings: list[str] = []

    if "inline" in shocks:
        try:
            df = shock_table.normalize(shocks["inline"])
        except shock_table.ShockTableError as e:
            raise ScenarioError(e.problems)
        summary = shock_table.describe(df, origin="inline")
        return df, summary["shock_table_id"], summary, warnings

    if "file" in shocks:
        try:
            spec = ingest_mod.load_mapping(shocks["mapping"])
            records, warnings = ingest_mod.ingest(
                shocks["file"], spec, country=scenario.get("country_code"))
            df = shock_table.normalize(records)
        except (ingest_mod.IngestError, shock_table.ShockTableError) as e:
            raise ScenarioError(getattr(e, "problems", None) or [str(e)])
        summary = shock_table.describe(df, origin="ingest", file=str(shocks["file"]),
                                       mapping=str(shocks["mapping"]),
                                       dimensions=spec.get("dimensions") or {})
        return df, summary["shock_table_id"], summary, warnings

    raise ScenarioError(["shocks must be one of {inline}, {file, mapping}"])


def _scenario_constants(scenario: dict) -> dict:
    """Scenario constants as the {(name, group): value} form the run takes."""
    out = {}
    for e in scenario.get("constants") or []:
        out[(str(e["name"]).strip(), str(e.get("group") or "").strip())] = str(e["value"])
    return out


def fingerprint(scenario: dict, shock_table_id: str, methodology: str,
                code_fingerprint: str = "") -> str:
    """Canonical scenario hash for a result-cache key. Shocks are represented by
    the table's content id, so inline vs stored vs re-ingested identical shocks
    hit the same cache entry. Excludes the free-text 'name' label.

    ``methodology`` is the run's method reference (``registry.pipeline_name``)
    and ``code_fingerprint`` its ``registry.pipeline_fingerprint``: both carry
    the order the methods ran in, which is part of what the run did."""
    payload = {
        "scenario_version": 1,
        "methodology": methodology,
        # Methodology *code*, not just its name: editing a method must not
        # serve results the previous implementation produced.
        **({"methodology_code": code_fingerprint} if code_fingerprint else {}),
        "cc": str(scenario.get("country_code", "")).upper(),
        "system": scenario.get("system_name", ""),
        "dataset": scenario.get("dataset_name") or "",
        "shocks": shock_table_id,
        "constants": sorted((f"{n}|{g}" if g else n, v)
                            for (n, g), v in _scenario_constants(scenario).items()),
        "params": scenario.get("params") or {},
        "addons": sorted([list(map(str, a)) for a in (scenario.get("addons") or [])]),
        "extensions": sorted([[str(e[0]), bool(e[1])] for e in (scenario.get("extensions") or [])]),
    }
    blob = json.dumps(payload, sort_keys=True, default=str)
    return hashlib.sha256(blob.encode("utf-8")).hexdigest()


# --- running it ---------------------------------------------------------------

class NoEffectError(RuntimeError):
    """The methodology transformed the input but the engine produced identical
    output — the run is invalid.

    Emphatically *not* "the reform has no impact": it means the model never
    acted on the transformation (typically a required add-on or switch is not
    available for this country/system).
    """


# --- pieces of the plan -------------------------------------------------------

def _split_constant_shocks(shocks):
    """Separate ``channel=constant`` records into {(name, group): value}.

    Returns (population_shocks, constant_overrides). Raises ScenarioError when a
    constant shock is relative — only ``set`` is meaningful for a parameter.
    """
    problems, overrides = [], {}
    for t in shocks[shocks["channel"] == "constant"].itertuples():
        if t.op != "set":
            problems.append(f"Constant shock {t.metric!r}: op must be 'set' (got {t.op!r}; "
                            "relative constant changes are not supported)")
            continue
        overrides[(t.metric, str(t.period))] = str(t.value)
    if problems:
        raise ScenarioError(problems)
    return shocks[shocks["channel"] != "constant"], overrides


def _resolve_methods(scenario: dict, population_shocks) -> list:
    """The pinned methods, else one per shock channel by dispatch, in run
    order. [] when the scenario is constants-only."""
    pin = scenario.get("methodology")
    if pin:
        return registry.resolve_pipeline(pin)
    if population_shocks.empty:
        return []
    by_channel = {str(ch): set(sub["metric"].unique())
                  for ch, sub in population_shocks.groupby("channel")}
    return registry.resolve_for_channels(by_channel)


def own_shocks(population_shocks, spec):
    """The records a method is handed: those of the channels it consumes."""
    return population_shocks[population_shocks["channel"].isin(spec.channels_consumed)]


def _check_contract(specs, population_shocks) -> list[str]:
    """Shocks vs the methods' declared contracts (a safety net for pinned
    references — dispatch already guarantees channels and metrics match)."""
    problems = []
    consumed = {ch for s in specs for ch in s.channels_consumed}
    for ch in sorted(set(population_shocks["channel"].unique()) - consumed):
        problems.append(f"No pinned methodology consumes channel '{ch}' "
                        f"(pinned: {[s.name for s in specs]})")
    for spec in specs:
        own = own_shocks(population_shocks, spec)
        if own.empty:
            problems.append(f"Methodology {spec.name} consumes channels "
                            f"{sorted(spec.channels_consumed)}, but the shock table carries none")
            continue
        for m in sorted(own["metric"].unique()):
            if spec.metrics_consumed and m not in spec.metrics_consumed:
                problems.append(f"Methodology {spec.name} does not consume metric '{m}' "
                                f"(consumes: {sorted(spec.metrics_consumed)})")
    return sorted(set(problems))


def run_arguments(scenario: dict, specs) -> tuple[list, list]:
    """Add-ons and extension switches the run needs: every method's own
    requirements (with ``{cc}`` resolved) plus anything the scenario adds."""
    cc = str(scenario["country_code"]).upper()
    addons: list = []
    switches: list = []
    for spec in specs or ():
        addon_entries, switch_entries = (spec.addon_requirements or ((), ()))
        for a in addon_entries:
            entry = ([str(x).format(cc=cc) for x in a] if isinstance(a, (list, tuple))
                     else str(a).format(cc=cc))
            if entry not in addons:
                addons.append(entry)
        for s in switch_entries:
            entry = [str(s[0]).format(cc=cc), bool(s[1])]
            if entry not in switches:
                switches.append(entry)
    addons += list(scenario.get("addons") or [])
    switches += [[str(e[0]), bool(e[1])] for e in (scenario.get("extensions") or [])]
    return addons, switches


def _compatibility(system, spec):
    """The methodology's requirements checked against the model, or None.

    None means the check could not run at all — an unreadable model, or the
    caller setting ``EUROMOD_SKIP_COMPAT_CHECK``. That is deliberately not a
    failure: this check exists to turn a late `NoEffectError` into an early
    `ScenarioError`, and it must never become a new way for a working scenario
    to be refused."""
    from euromod_linking import compat

    if compat._skip_requested():
        return None
    try:
        return compat.check_compatibility(system, spec)
    except Exception:
        logger.debug("compatibility check unavailable for %s", spec.name, exc_info=True)
        return None


def cell_population(data, population_shocks) -> dict:
    """Weighted people matched by each distinct shock cell.

    Surfacing this before a run is what turns a mistyped cell (``deh=34``) into
    a visible zero rather than a shock that silently hits nobody.
    """
    import pandas as pd

    counts = {}
    for group in sorted(g for g in population_shocks["group"].unique() if g):
        mask = pd.Series(True, index=data.index)
        for k, v in dimensions.parse_group(group).items():
            series = dimensions.derive_region(data) if k == "region" else data.get(k)
            if series is None:
                mask = None
                break
            mask &= (series.str.startswith(v) if k == "region"
                     else dimensions.matches(series, dimensions.parse_value_spec(v)))
        if mask is None:
            continue
        counts[group] = {"n_rows": int(mask.sum()),
                         "n_weighted_all_ages": round(float(data.loc[mask, "dwt"].sum()), 1)}
    return counts


def _stages(specs) -> list[dict]:
    """What ran, in order — the run's own account of its composition."""
    return [{"method": s.name, "stage": s.stage, "channels": sorted(s.channels_consumed)}
            for s in specs]


def _nest(specs, parts: dict) -> dict:
    """Per-method diagnostics as the plan reports them.

    One method: its diagnostics as they are, flat — the common case, and what
    every reader of a single-method run expects. Several: each method's
    diagnostics UNFLATTENED under its name, with ``order`` naming the sequence
    and ``stages`` saying which channels each one took, so anything you read
    from a method on its own is in the same place one level down."""
    if len(specs) == 1:
        return dict(parts[specs[0].name])
    out = {"order": [s.name for s in specs], "stages": _stages(specs)}
    for spec in specs:
        out[spec.name] = dict(parts[spec.name])
    out["warnings"] = _merge_warnings(*(parts[s.name].get("warnings") for s in specs))
    return out


# --- the core transform -------------------------------------------------------

def check_scenario(scenario: dict) -> dict:
    """Validate everything that does not need the model or the data.

    Structure, shock resolution, methodology dispatch, declared params and the
    methods' contracts. Separated so a caller can reject a malformed scenario
    without paying to load a model. Raises ScenarioError with `.problems`;
    returns the resolved pieces for `apply_scenario`.
    """
    problems = _validate_structure(scenario)
    if problems:
        raise ScenarioError(problems)

    shocks, shock_id, summary, warnings = _resolve_shocks(scenario)
    population_shocks, shock_constants = _split_constant_shocks(shocks)
    specs = _resolve_methods(scenario, population_shocks)   # MethodLookupError propagates

    context_constants = _scenario_constants(scenario)
    clash = sorted(set(context_constants) & set(shock_constants))
    if clash:
        raise ScenarioError([
            f"Constants appear both as scenario context and as shocks: {clash}. Context "
            "constants apply to both runs; shock constants only to the counterfactual."])

    if not specs:
        if scenario.get("params"):
            raise ScenarioError(["params require a methodology; a constants-only "
                                 "scenario takes none"])
    else:
        problems = _validate_params(scenario, specs)
        problems += _check_contract(specs, population_shocks)
        if problems:
            raise ScenarioError(problems)

    addons, extensions = run_arguments(scenario, specs)
    return {
        "specs": specs,
        "methodology": registry.pipeline_name(specs) if specs else "constants-only",
        "methods": [s.name for s in specs],
        "stages": _stages(specs),
        "population_shocks": population_shocks,
        "shock_constants": shock_constants,
        "context_constants": context_constants,
        "constants": {**context_constants, **shock_constants},
        "shock_table_id": shock_id, "shocks": summary,
        "addons": addons, "extensions": extensions,
        "warnings": list(warnings),
        "counterfactual": None, "baseline": None, "diagnostics": {},
        "compatibility": None,   # filled by apply_scenario, which has the system
    }


def apply_scenario(system, data, scenario: dict, *, dataset_name: str | None = None,
                   validate_only: bool = False) -> dict:
    """Transform input microdata according to a scenario. Executes nothing.

    Returns a dict with::

      counterfactual  the transformed input
      baseline        the matching baseline (same rows) when a methodology
                      restructures rows, else None — run the untouched `data`
      diagnostics     the methods' account of what they did: flat for one
                      method, nested under each method's name (with ``order``)
                      for several
      methodology     the resolved reference, e.g. "lma_labour_alignment" or
                      "scale_variables+lma_labour_alignment" (run order)
      methods, stages what ran, and in which order
      constants       {(name, group): value} for the counterfactual run
      context_constants  ... applied to BOTH runs
      addons, extensions   what the runs must activate
      compatibility   what each method needs from the model and whether this
                      model has it: a list of CompatibilityReport, one per
                      method (None entries when the model could not be inspected)

    Several methods run in stage order, each on the frame the previous one
    produced. The paired ``baseline`` is the one built by the last method that
    restructured rows, on the frame it received — so it carries every earlier
    stage's changes, which is what keeps it row-aligned with the counterfactual.

    Raises ScenarioError (with .problems) when the scenario cannot be applied.
    """
    from euromod_linking.methods.base import MethodContext, MethodError
    from euromod_linking.session import adopt

    # Read helpers below (income-list expansion, the country's full-time week)
    # resolve the model from the process session. Take it from the System the
    # caller already handed us, so building the model yourself is enough.
    adopt(system)

    result = check_scenario(scenario)
    specs = result.pop("specs")
    population_shocks = result.pop("population_shocks")
    shock_constants = result["shock_constants"]

    cc = str(scenario["country_code"]).upper()
    system_name = scenario["system_name"]

    if not specs:
        # Constants-only: the input is unchanged; the two runs differ in overrides.
        result["counterfactual"] = data
        result["compatibility"] = []
        return result

    # A problem is prefixed with the method it comes from only when there are
    # several to tell apart; a single method's messages stay as that method
    # wrote them.
    def tag(spec, message):
        return f"[{spec.name}] {message}" if len(specs) > 1 else message

    problems = []
    columns = list(data.columns)
    steps = []   # (spec, method instance, the records it gets)
    for spec in specs:
        missing = [c for c in spec.dataset_requirements if c not in columns]
        if missing:
            problems.append(tag(spec, f"Dataset lacks required columns {missing}"))
        method = spec.factory()
        own = own_shocks(population_shocks, spec)
        problems += [tag(spec, p) for p in method.check_dataset(columns, own)]
        steps.append((spec, method, own))

    # The model side of the contract. A missing add-on or extension switch is
    # dropped by the engine without failing, so without this the scenario runs
    # both simulations and only then raises NoEffectError. Checking here means
    # validate_only catches it too, before anything expensive happens.
    result["compatibility"] = [_compatibility(system, spec) for spec in specs]
    for report in result["compatibility"]:
        if report is not None:
            problems += list(report.problems)

    if problems:
        raise ScenarioError(problems)

    ctx = MethodContext(country_code=cc, system_name=system_name,
                        dataset_name=dataset_name,
                        extensions=scenario.get("extensions"))
    params = scenario.get("params") or {}

    if validate_only:
        # Everything above is validation; the transform below is the expensive
        # part (it runs the whole alignment), so stop here — but not before
        # asking each method what it would target. That preview is what makes
        # a mis-sized shock visible while it is still cheap to fix.
        #
        # A later stage previews against what the earlier ones would hand it,
        # where an earlier method says its apply() is cheap enough to run here
        # (preview_by_applying). A preview that sized an alignment against the
        # untouched input could disagree with the run it previews.
        frame = data
        parts = {}
        for spec, method, own in steps:
            preview = getattr(method, "preview", None)
            try:
                parts[spec.name] = dict(preview(frame, own, params, ctx)) if callable(preview) else {}
                if spec.preview_by_applying:
                    frame = method.apply(frame, own, params, ctx).data
            except MethodError as e:
                raise ScenarioError([f"[{spec.name}] {e}"])
        diagnostics = _nest(specs, parts)
        diagnostics["cell_population"] = cell_population(data, population_shocks)
        result["diagnostics"] = diagnostics
        result["warnings"] = _merge_warnings(result["warnings"], diagnostics.get("warnings"))
        return result

    frame, baseline, parts = data, None, {}
    for spec, method, own in steps:
        try:
            applied = method.apply(frame, own, params, ctx)
        except MethodError as e:
            raise ScenarioError([f"[{spec.name}] {e}"])
        frame = applied.data
        if applied.baseline is not None:
            # Built on the frame this method received, so it carries the
            # earlier stages and pairs row for row with what comes out.
            baseline = applied.baseline
        parts[spec.name] = dict(applied.diagnostics or {})

    diagnostics = _nest(specs, parts)
    diagnostics["cell_population"] = cell_population(data, population_shocks)
    if shock_constants:
        diagnostics["constant_shocks_applied"] = {
            f"{n}|{g}" if g else n: v for (n, g), v in sorted(shock_constants.items())}

    result["counterfactual"] = frame
    result["baseline"] = baseline
    result["diagnostics"] = diagnostics
    # The methods' warnings are the ones about the shock itself ("moves 0.035
    # people", "consumes 60% of the pool"). Kept in diagnostics for existing
    # readers, but surfaced at top level too — a caller scanning `warnings`
    # must not miss them.
    result["warnings"] = _merge_warnings(result["warnings"], diagnostics.get("warnings"))
    return result


def _merge_warnings(*lists) -> list:
    """Concatenate warning lists, dropping duplicates, preserving order."""
    out, seen = [], set()
    for items in lists:
        for w in items or []:
            if w not in seen:
                seen.add(w)
                out.append(w)
    return out


# --- convenience: apply and run -----------------------------------------------

def run_scenario(system, scenario: dict, *, input_path: str,
                 paired_baseline: bool = False) -> dict:
    """Apply a scenario and run both halves through ``System.run()``.

    ``paired_baseline`` runs the baseline on the counterfactual's own rows, so
    the two outputs are observation-paired — needed by any baseline-vs-reform
    comparison that works row by row, such as one applying a fixed poverty line
    or baseline-defined decile groups.

    Returns the apply_scenario dict with ``baseline_output`` and
    ``counterfactual_output`` frames added. Raises NoEffectError when the
    methodology changed people but the outputs are identical.
    """
    import pandas as pd

    cc = str(scenario["country_code"]).upper()
    system_name = scenario["system_name"]
    data_file, dataset_used, _, _ = resolve_dataset(
        system, cc, scenario.get("dataset_name"), input_path)
    data = pd.read_csv(data_file, sep="\t")

    plan = apply_scenario(system, data, scenario, dataset_name=dataset_used)
    plan["dataset_used"] = dataset_used

    baseline_input = plan["baseline"] if (paired_baseline and plan["baseline"] is not None) else data
    run_args = dict(country_code=cc, input_path=input_path, dataset_name=dataset_used,
                    addons=plan["addons"] or None, extensions=plan["extensions"] or None)

    plan["baseline_output"] = execute(system, baseline_input,
                                      constants=_as_list(plan["context_constants"]), **run_args)
    plan["counterfactual_output"] = execute(system, plan["counterfactual"],
                                            constants=_as_list(plan["constants"]), **run_args)

    # Did the transform change the input at all? Ask the data, not the method:
    # reading a method's diagnostics would mean knowing each method's private
    # keys here, and a shape this did not recognise would silently answer "no"
    # and disarm the guard.
    if (not frames_identical(baseline_input, plan["counterfactual"])
            and frames_identical(plan["baseline_output"], plan["counterfactual_output"])):
        raise NoEffectError(
            f"[{plan['methodology']}] transformed the input but the simulation output is "
            "identical to the baseline: the engine did not act on it. Check that the "
            f"required add-ons {plan['addons']} and switches {plan['extensions']} exist "
            "for this country/system.")
    return plan


def _as_list(constants: dict) -> list | None:
    return [{"name": n, "group": g, "value": v}
            for (n, g), v in sorted(constants.items())] or None
