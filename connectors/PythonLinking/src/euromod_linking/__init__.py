"""Link external economic models to EUROMOD.

Turns another model's output — regional employment projections, wage paths,
price developments — into changes to EUROMOD's input microdata and run
parameters, so macro results can be evaluated at household level::

    from euromod import Model
    from euromod_linking import run_scenario

    system = Model(MODEL_PATH)["AT"]["AT_2024"]
    result = run_scenario(system, {
        "country_code": "AT", "system_name": "AT_2024",
        "shocks": {"file": "projections.xlsx", "mapping": my_mapping_spec},
        "params": {"period": "8"},
    }, input_path=INPUT_PATH)

Shocks arrive in one canonical form (the shock table); each external model's
file format is handled by a declarative mapping spec. Each shock channel is
dispatched to the methodology that consumes it rather than chosen by the
caller — a scenario selects data, never modelling — and a table carrying
several channels runs several methods, in their declared stage order.

Scope: this targets the JRC's EUROMOD model for the 27 EU member states, and
only that model — not the EUROMOD *software* in general. Models built on the
same engine (SOUTHMOD, SWISSMOD, national models outside the EU) will not work
with it, because it assumes EUROMOD's own EU-27 conventions: the ``ils_udb_*``
income lists, the ``les``/``les2`` labour-status codings, ``drgn1``/``drgn2``
read as NUTS regions, and the JRC-maintained LMA add-on. The model is
distributed at https://euromod-web.jrc.ec.europa.eu/download-euromod.

Importing this package registers the shipped methodologies, so dispatch always
works.

Everything below is the whole public surface. Anything else is reachable from
its own module (``euromod_linking.compat``, ``euromod_linking.session``, …) but
is not part of the path through the library.
"""

from euromod_linking import methods as _methods  # noqa: F401  (registers methodologies)
from euromod_linking.compat import check_compatibility, compatibility_matrix
from euromod_linking.registry import MethodLookupError, list_specs
from euromod_linking.runner import RunError
from euromod_linking.scenarios import (NoEffectError, ScenarioError, apply_scenario,
                                       run_scenario)
from euromod_linking.shock_table import ShockTableError
from euromod_linking.shock_table import normalize as normalize_shocks

__all__ = [
    # the two entry points
    "apply_scenario", "run_scenario",
    # shocks in, methods available, model support
    "normalize_shocks", "list_specs", "check_compatibility", "compatibility_matrix",
    # what can go wrong
    "ScenarioError", "RunError", "NoEffectError", "MethodLookupError", "ShockTableError",
]

__version__ = "0.3.0"
