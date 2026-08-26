"""Shared, long-lived EUROMOD connector session.

Owns the single process-wide euromod.Model object and the lock that serializes
all connector access (reads and simulation runs). No business logic and no
mutation helpers — the model is used read-only at runtime; the only simulation
overrides go through System.run() kwargs, which do not mutate the model.
"""

import os
import threading

# The model path is always an argument: a library must not silently resolve
# which model it runs against from the environment. The first caller decides,
# and later callers reuse that loaded model.

# The euromod.Model object is not thread-safe; one reentrant lock serializes
# every connector access. RLock because accessors nest (get_system -> get_country
# -> get_model) and tools hold the lock while calling the accessors.
model_lock = threading.RLock()

_model = None
_loaded_path: str | None = None


class ModelLookupError(Exception):
    """Raised when a country/system is not found. ``.available`` lists the names
    that would have worked, so a caller can report them."""

    def __init__(self, message: str, available: list | None = None):
        super().__init__(message)
        self.available = available or []


def get_model(model_path: str | None = None):
    """Return the cached euromod.Model, loading it once per process.

    Pass `model_path` to load (or switch to) a model; omit it to reuse the one
    already loaded, which is how callers deep in a call chain avoid threading
    the path through every function. Loading is expensive (.NET + the whole
    model graph), so a genuinely different path is the only thing that reloads.
    """
    global _model, _loaded_path
    with model_lock:
        if model_path:
            norm = os.path.normpath(model_path)  # no spurious reloads on Windows separators
            if _model is None or _loaded_path != norm:
                from euromod import Model  # lazy: keep module import .NET-free
                from euromod_linking.env import harden_dotnet_console
                harden_dotnet_console()  # engine warnings must not kill consoleless processes
                _model = Model(norm)
                _loaded_path = norm
        elif _model is None:
            raise ModelLookupError(
                "No EUROMOD model loaded. Pass model_path (the model root directory) "
                "on the first call.")
        return _model


def adopt(system) -> bool:
    """Make the model behind `system` the one this process reads from.

    Callers hand `apply_scenario` a live System they built themselves; the read
    helpers underneath resolve the model from here. Adopting the caller's own
    model closes that gap, so nobody has to load the model twice or through a
    particular door for income-list expansion to work.

    Returns True when a model was adopted. Silent False when `system` is not a
    live connector object — the caller may have loaded a model already, and this
    is a convenience, not a precondition."""
    global _model, _loaded_path
    model = getattr(getattr(system, "parent", None), "model", None)
    path = getattr(model, "model_path", None)
    if model is None or not path:
        return False
    with model_lock:
        norm = os.path.normpath(str(path))
        if _model is not model:
            _model, _loaded_path = model, norm
    return True


def get_country(country_code: str, model_path: str | None = None):
    """Return the live Country object, or raise ModelLookupError(available=[...])."""
    cc = country_code.upper()
    with model_lock:
        mod = get_model(model_path)
        try:
            return mod[cc]
        except Exception:
            available = list(mod.countries.keys()) if hasattr(mod.countries, "keys") else [str(c) for c in mod.countries]
            raise ModelLookupError(f"Country '{cc}' not found", available)


def get_system(country_code: str, system_name: str, model_path: str | None = None):
    """Return the live System object, or raise ModelLookupError(available=[...])."""
    cc = country_code.upper()
    with model_lock:
        country = get_country(cc, model_path)
        try:
            return country[system_name]
        except Exception:
            available = list(country.systems.keys()) if hasattr(country.systems, "keys") else [str(s) for s in country.systems]
            raise ModelLookupError(f"System '{system_name}' not found for {cc}", available)
