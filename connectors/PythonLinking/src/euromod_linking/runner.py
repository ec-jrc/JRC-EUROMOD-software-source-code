"""Executing a EUROMOD simulation on transformed input.

Plain functions returning plain values. The library's job is to *produce*
transformed microdata (`apply_scenario`); running it is one call to
``System.run()``. An application that wants caching, retries or a run registry
writes that around `apply_scenario` rather than plugging a strategy in here.

Dataset resolution and argument normalisation live here because they are model
knowledge: which .txt backs a dataset name, and how grouped constants must be
shaped for ``constantsToOverwrite``.

Paths are always arguments — a library must not silently resolve which model it
runs against from the environment.
"""

import logging
from pathlib import Path

logger = logging.getLogger(__name__)


class RunError(RuntimeError):
    """A simulation could not be executed (bad dataset, engine failure, ...)."""


# --- dataset resolution -------------------------------------------------------

def dataset_file(dataset_name: str, cc: str, input_path: str) -> Path | None:
    """Locate a dataset .txt, supporting a flat repository (all files in one
    directory) or a per-country subdirectory layout."""
    base = Path(input_path)
    for p in (base / f"{dataset_name}.txt", base / cc / f"{dataset_name}.txt"):
        if p.exists():
            return p
    return None


def bestmatch_dataset_name(system) -> str | None:
    """The connector's own best-match dataset for a system (authoritative)."""
    try:
        for d in system.bestmatch_datasets:
            n = getattr(d, "name", None)
            if n:
                return str(n).strip()
    except Exception:
        pass
    return None


def fallback_candidates(cc: str, input_path: str) -> list[Path]:
    """All CC_*.txt datasets, newest name first."""
    base = Path(input_path)
    flat = sorted(base.glob(f"{cc}_*.txt"), reverse=True)
    if flat:
        return flat
    sub = base / cc
    return sorted(sub.glob(f"{cc}_*.txt"), reverse=True) if sub.is_dir() else []


def resolve_dataset(system, country_code: str, dataset_name: str | None, input_path: str):
    """Resolve which dataset file to run.

    Explicit name wins; otherwise the system's best match, and if that file is
    not on disk, the newest available CC_* dataset — flagged, never silent.
    Returns (path, resolved_name, bestmatch_name, used_bestmatch).
    """
    cc = country_code.upper()
    if dataset_name:
        p = dataset_file(dataset_name, cc, input_path)
        if p is None:
            raise FileNotFoundError(f"Dataset '{dataset_name}' not found under {input_path}")
        return p, dataset_name, None, False

    best = bestmatch_dataset_name(system)
    if best:
        p = dataset_file(best, cc, input_path)
        if p is not None:
            return p, best, best, True

    candidates = fallback_candidates(cc, input_path)
    if not candidates:
        raise FileNotFoundError(
            f"No datasets found for country '{cc}' under {input_path}"
            + (f" (best match '{best}' is not available)" if best else ""))
    return candidates[0], candidates[0].stem, best, False


def normalize_constants(constants) -> dict:
    """Normalise constants to the ``{(name, group): value}`` shape ``run()`` wants.

    Accepts ``{"$name": v}``, ``{"$name|group": v}`` (split on the last ``|``) or
    a list of ``{"name", "group"?, "value"}``. Uprating factors are grouped by
    year, e.g. ``("$f_cpi", "2023")``.
    """
    if not constants:
        return {}
    items = []
    if isinstance(constants, dict):
        for k, v in constants.items():
            name, sep, group = str(k).rpartition("|")
            if not sep:
                name, group = str(k), ""
            items.append((name, group, v))
    elif isinstance(constants, list):
        for e in constants:
            if not isinstance(e, dict) or "name" not in e or "value" not in e:
                raise RunError(f"Invalid constants entry {e!r}: expected "
                               "{'name', 'group'?, 'value'}")
            items.append((str(e["name"]), str(e.get("group") or ""), e["value"]))
    else:
        raise RunError("constants must be a dict or a list of {name, group?, value}")

    out = {}
    for name, group, v in items:
        name = name.strip()
        if not name:
            raise RunError("Constant name must be non-empty")
        out[(name, str(group).strip())] = str(v)
    return out


# --- execution ----------------------------------------------------------------

def execute(system, data, *, country_code: str, input_path: str,
            dataset_name: str | None = None, constants=None, addons=None,
            extensions=None):
    """Run one simulation and return its output DataFrame.

    Raises RunError if the engine fails.
    """
    from euromod_linking.session import model_lock

    cc = country_code.upper()
    with model_lock:
        _, resolved, _, _ = resolve_dataset(system, cc, dataset_name, input_path)

        kwargs = {}
        norm = normalize_constants(constants)
        if norm:
            kwargs["constantsToOverwrite"] = dict(norm)
        if addons:
            kwargs["addons"] = [tuple(a) if isinstance(a, (list, tuple)) else a
                                for a in addons]
        if extensions:
            kwargs["switches"] = [(str(e[0]), bool(e[1])) for e in extensions]

        try:
            out = system.run(data, resolved, **kwargs)
        except Exception as e:
            raise RunError(f"{type(e).__name__}: {e}") from e

    return out.outputs[0]


def frames_identical(a, b) -> bool:
    """Are two simulation outputs the same to the last cent?

    Used to detect a scenario that transformed the input but had no effect on
    the results — an invalid run, not 'the reform has no impact'.
    """
    if a is None or b is None or a.shape != b.shape:
        return False
    try:
        return bool(a.equals(b))
    except Exception:
        logger.debug("output comparison failed", exc_info=True)
        return False
