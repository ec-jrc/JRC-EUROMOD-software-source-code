__license__='''
Copyright 2024 European Commission
*
Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:

*
   https://joinup.ec.europa.eu/software/page/eupl
*

Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the Licence for the specific language governing permissions and limitations under the Licence.
'''

"""Read-only introspection of a system's spine: extensions, constants, income lists.

The model object graph answers "what is defined", but a caller preparing a
:meth:`euromod.System.run` call needs "what is *effective*" — which constants may
be overwritten, which extension names the engine will accept, and which variables
an income list actually resolves to once extension switches are applied. Deriving
that by hand means re-implementing the model's include/exclude rules, which is
how silent wrong answers happen: an extension name the model does not know is
dropped by the engine with only a console message, so a run can appear to succeed
while the behaviour it asked for never happened.

Every function here is read-only and free of simulation side effects. Extension
resolution follows the same rule as :func:`euromod.container.filter`: an element
carrying extension links is included when some link is switched on for inclusion,
or when none is switched on for removal; an element with no links falls back to
its own switch.

This module is not imported by ``euromod/__init__``: importing it is opt-in, like
:mod:`euromod.calibrate`.
"""

import logging
import re

logger = logging.getLogger(__name__)

__all__ = [
    "IncomeListLookupError",
    "iter_real_policies",
    "system_extension_names",
    "system_constant_names",
    "system_constant_params",
    "income_list_components",
    "clear_cache",
]

#: DefIl parameters that configure the list rather than contribute to it.
_IL_SERVICE_PARAMS = {"Name", "Run_Cond", "Output_Var", "TAX_UNIT", "Warn_If_NonPositive"}

_IDENTIFIER = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")


class IncomeListLookupError(KeyError):
    """Raised when an income list is unknown, inactive, or cyclically defined.

    Attributes
    ----------
    available : :obj:`list` [ :obj:`str` ]
        Income list names defined in the system, for error messages.
    """

    def __init__(self, message: str, available: list) -> None:
        super().__init__(message)
        self.available = available

    def __str__(self) -> str:
        return self.args[0]


def _attr(obj, name, default=""):
    """``getattr`` that tolerates the .NET-delegated attribute errors raised by
    lazily-loaded model elements, and strips strings like the XML parser does."""
    try:
        v = getattr(obj, name)
    except Exception:
        return default
    if v is None:
        return default
    return v.strip() if isinstance(v, str) else v


def _int(v):
    try:
        return int(v or 0)
    except (TypeError, ValueError):
        return 0


def iter_real_policies(scope):
    """Yield the policies of a country or system, skipping reference policies.

    Parameters
    ----------
    scope : :class:`euromod.Country` or :class:`euromod.System`
        Element whose ``policies`` container is walked.

    Yields
    ------
    :class:`euromod.Policy`
        Policies excluding :class:`euromod.ReferencePolicy` objects, which carry
        no functions or comment and raise on naive attribute access.
    """
    for p in scope.policies:
        if type(p).__name__ == "ReferencePolicy":
            continue
        yield p


def _ext_links(elem):
    """Extension links of a policy, function or parameter as plain data.

    Returns
    -------
    :obj:`tuple` [ :obj:`tuple` [ :obj:`str`, :obj:`bool` ] ]
        ``(shortName, baseOff)`` pairs; empty when the element is not
        extension-gated.
    """
    out = []
    try:
        for e in (getattr(elem, "extensions", None) or []):
            sn = _attr(e, "shortName")
            if sn:
                out.append((sn, _attr(e, "baseOff") == "true"))
    except Exception:
        pass
    return tuple(out)


def _filter_include(ext_links, cfg):
    """Apply the model's include/exclude rule to one element's extension links.

    Mirrors :func:`euromod.container.filter` for a single element, over plain
    data rather than model objects so the decision can be made without repeated
    interop.

    Parameters
    ----------
    ext_links : :obj:`tuple` [ :obj:`tuple` [ :obj:`str`, :obj:`bool` ] ]
        ``(shortName, baseOff)`` pairs from :func:`_ext_links`.
    cfg : :obj:`dict` [ :obj:`str`, :obj:`bool` ]
        Extension short name -> switched on.

    Returns
    -------
    :obj:`bool` or :obj:`None`
        Whether to include the element, or :obj:`None` when it carries no
        extension links (the caller falls back to the element's own switch).
    """
    if not ext_links:
        return None
    explicit_removal = False
    explicit_inclusion = False
    for short_name, base_off in ext_links:
        if cfg.get(short_name, False):      # extension switched on
            if base_off:
                explicit_removal = True     # marked for removal when on
            else:
                explicit_inclusion = True   # marked for inclusion when on
        elif not base_off:
            # Marked for inclusion when on, but it is off: exclude.
            explicit_removal = True
    return explicit_inclusion or not explicit_removal


def _active(base_switch, ext_links, cfg):
    """Whether an element is active: extension links decide when present,
    otherwise the element's own ``switch``."""
    include = _filter_include(ext_links, cfg)
    return (base_switch == "on") if include is None else include


def _il_component_name(name):
    """Whether a DefIl parameter name denotes a component (a variable or nested
    income list) rather than a service parameter such as ``TAX_UNIT``."""
    if name in _IL_SERVICE_PARAMS or name.startswith("#"):
        return False
    return bool(_IDENTIFIER.fullmatch(name))


def _bestmatch_dataset(system):
    """Name of the system's best-matching dataset, or :obj:`None`."""
    for attr in ("bestmatch_datasets", "datasets"):
        try:
            for d in getattr(system, attr):
                name = _attr(d, "name")
                if name:
                    return name
        except Exception:
            continue
    return None


# --- raw materialisation ------------------------------------------------------
# Walking the spine is the expensive first-touch interop, so each system is
# extracted once into plain data and every later question is answered from that.
# The model is read-only at run time, so the cache stays valid for the process.
_raw_cache: dict = {}


def _system_key(system):
    return (_attr(_attr(system, "parent", None), "name"), _attr(system, "name"))


def _switch_defaults(system):
    """Per-dataset extension defaults: ``{dataset_name: {extension: on}}``."""
    defaults: dict = {}
    country = _attr(system, "parent", None)
    if country is None:
        return defaults
    try:
        for sw in country.get_switch_value(sys_name=_attr(system, "name")):
            dataset = _attr(sw, "data_name")
            extension = _attr(sw, "extension_name")
            if dataset and extension:
                defaults.setdefault(dataset, {})[extension] = _attr(sw, "value") == "on"
    except Exception:
        logger.debug("could not read switch defaults for %s", _attr(system, "name"),
                     exc_info=True)
    return defaults


def _extract(system):
    """Extract one system's spine into plain data (see :func:`_system_raw`)."""
    try:
        dataset_names = {_attr(d, "name") for d in system.datasets}
    except Exception:
        dataset_names = set()

    policies = []
    const_occ: dict = {}
    il_occ: dict = {}
    for pol in iter_real_policies(system):
        pol_name = _attr(pol, "name")
        if not pol_name:
            continue
        spine_order = _int(_attr(pol, "spineOrder", 0))
        policies.append({
            "name": pol_name, "switch": _attr(pol, "switch"), "ext": _ext_links(pol),
            "spineOrder": spine_order, "order": _int(_attr(pol, "order", 0)),
            "comment": _attr(pol, "comment"), "type": _attr(pol, "type"),
        })
        for fun in (_attr(pol, "functions", None) or []):
            fun_name = _attr(fun, "name")
            if fun_name not in ("DefConst", "DefIl"):
                continue
            fun_ext = _ext_links(fun)
            fun_switch = _attr(fun, "switch")
            fun_order = _int(_attr(fun, "order", 0))
            if fun_name == "DefConst":
                for par in (_attr(fun, "parameters", None) or []):
                    par_name = _attr(par, "name")
                    if not (isinstance(par_name, str) and par_name.startswith("$")):
                        continue
                    const_occ.setdefault(par_name, []).append({
                        "key": (spine_order, fun_order, _int(_attr(par, "order", 0))),
                        "fun_ext": fun_ext, "fun_switch": fun_switch,
                        "value": _attr(par, "value"), "policy": pol_name,
                        "group": str(_attr(par, "group")), "comment": _attr(par, "comment"),
                    })
            else:
                il_name = None
                components = []
                for par in (_attr(fun, "parameters", None) or []):
                    par_name = _attr(par, "name")
                    if not isinstance(par_name, str) or not par_name:
                        continue
                    if par_name == "Name":
                        il_name = str(_attr(par, "value")).strip()
                    elif (_il_component_name(par_name)
                          and str(_attr(par, "private")).lower() != "yes"):
                        components.append((par_name, str(_attr(par, "value")).strip(),
                                           _ext_links(par)))
                if il_name:
                    il_occ.setdefault(il_name, []).append({
                        "key": (spine_order, fun_order), "fun_ext": fun_ext,
                        "fun_switch": fun_switch, "policy": pol_name,
                        "components": components,
                    })

    return {
        "name": _attr(system, "name"), "year": str(_attr(system, "year")),
        "dataset_names": dataset_names, "bestmatch": _bestmatch_dataset(system),
        "switch_defaults": _switch_defaults(system),
        "policies": policies, "const_occ": const_occ, "il_occ": il_occ,
    }


def _system_raw(system):
    """Plain-data view of a system's spine, materialised once and cached."""
    key = _system_key(system)
    if key not in _raw_cache:
        _raw_cache[key] = _extract(system)
    return _raw_cache[key]


def clear_cache() -> None:
    """Drop the cached spine extractions.

    Only needed when a model is reloaded from changed files within one process;
    the model is otherwise read-only at run time.
    """
    _raw_cache.clear()


def _switch_config(raw, dataset=None, extensions=None):
    """Effective extension configuration: dataset defaults plus caller overrides."""
    names = raw["dataset_names"]
    chosen = dataset if (dataset and dataset in names) else raw["bestmatch"]
    cfg = dict(raw["switch_defaults"].get(chosen, {}))
    for name, value in (extensions or []):
        cfg[str(name)] = bool(value)
    return cfg


def _pol_active_map(raw, cfg):
    """Policy name -> active under this configuration (any active occurrence)."""
    out: dict = {}
    for p in raw["policies"]:
        active = _active(p["switch"], p["ext"], cfg)
        out[p["name"]] = out.get(p["name"], False) or active
    return out


def _sign_mult(a, b):
    return "+" if (a == "-") == (b == "-") else "-"


def _resolve_income_list(raw, cfg, list_name, _seen=frozenset()):
    """Resolve an income list's effective components from extracted spine data.

    Extension-aware at every level: the defining policy, the ``DefIl`` function
    and each component parameter must be active. Duplicate definitions resolve in
    spine order (last active wins, as for constants); nested ``ils_``/``il_``
    components expand recursively with signs multiplied through.

    Parameters
    ----------
    raw : :obj:`dict`
        Extracted system data from :func:`_system_raw`.
    cfg : :obj:`dict` [ :obj:`str`, :obj:`bool` ]
        Extension configuration from :func:`_switch_config`.
    list_name : :obj:`str`
        Income list to resolve, e.g. ``"ils_dispy"``.

    Returns
    -------
    :obj:`list` [ :obj:`tuple` [ :obj:`str`, :obj:`str` ] ]
        ``(variable, sign)`` pairs in definition order, deduplicated.

    Raises
    ------
    IncomeListLookupError
        If the list is unknown, inactive under this configuration, or cyclic.
    """
    occurrences = raw.get("il_occ", {}).get(list_name)
    if not occurrences:
        raise IncomeListLookupError(
            "Unknown income list {!r} in system {!r}".format(list_name, raw.get("name")),
            sorted(raw.get("il_occ", {})))
    if list_name in _seen:
        raise IncomeListLookupError(
            "Cyclic income-list definition at {!r}".format(list_name),
            sorted(raw.get("il_occ", {})))

    pol_active = _pol_active_map(raw, cfg)
    chosen = None
    for occ in sorted(occurrences, key=lambda o: o["key"]):
        if (pol_active.get(occ["policy"], False)
                and _active(occ["fun_switch"], occ["fun_ext"], cfg)):
            chosen = occ
    if chosen is None:
        raise IncomeListLookupError(
            "Income list {!r} is not active under this dataset/extension "
            "configuration".format(list_name), sorted(raw.get("il_occ", {})))

    out = []
    seen_vars = set()
    for name, sign, par_ext in chosen["components"]:
        if _filter_include(par_ext, cfg) is False:
            continue
        sign = sign if sign in ("+", "-") else "+"
        if name.startswith(("ils_", "il_")) and name in raw.get("il_occ", {}):
            for sub_name, sub_sign in _resolve_income_list(
                    raw, cfg, name, _seen | {list_name}):
                if sub_name not in seen_vars:
                    seen_vars.add(sub_name)
                    out.append((sub_name, _sign_mult(sign, sub_sign)))
        elif name not in seen_vars:
            seen_vars.add(name)
            out.append((name, sign))
    return out


# --- public API ---------------------------------------------------------------
def system_extension_names(system) -> set:
    """Extension short names the model accepts in ``run(switches=...)``.

    A switch the model does not know is silently dropped by the engine, which
    only reports "An error occurred during the processing of the
    ExtensionSwitches" to the console: the simulation then completes normally
    while the behaviour the caller asked for never happened. Validate against
    this set before running.

    Parameters
    ----------
    system : :class:`euromod.System`
        System whose country and dataset defaults are inspected.

    Returns
    -------
    :obj:`set` [ :obj:`str` ]
        Short names from the country's extensions (local plus model-wide, i.e.
        everything declared in ``Config/SWITCHABLEPOLICYCONFIG.xml``, including
        add-on extensions) unioned with the per-dataset switch defaults observed
        for this system. Empty when neither can be read, which callers should
        treat as "cannot validate" rather than "nothing is valid".

    Example
    --------
    >>> from euromod import Model
    >>> from euromod.introspect import system_extension_names
    >>> mod = Model("C:\\EUROMOD_RELEASES_I6.0+")
    >>> "LMA_trans" in system_extension_names(mod.countries['BE'].systems['BE_2025'])
    True
    """
    names = set()
    country = _attr(system, "parent", None)
    if country is not None:
        try:
            for ext in (getattr(country, "extensions", None) or []):
                short_name = _attr(ext, "shortName")
                if short_name:
                    names.add(short_name)
        except Exception:
            logger.debug("could not read country extensions", exc_info=True)
    for ds_cfg in _system_raw(system)["switch_defaults"].values():
        names.update(ds_cfg)
    return names


def system_constant_names(system) -> set:
    """``$``-prefixed constants defined via ``DefConst`` in a system.

    Parameters
    ----------
    system : :class:`euromod.System`
        System to inspect.

    Returns
    -------
    :obj:`set` [ :obj:`str` ]
        Constant names, extension-independent (every occurrence, whether or not
        it is active under a particular configuration).
    """
    return {name for name, occ in _system_raw(system)["const_occ"].items() if occ}


def system_constant_params(system) -> dict:
    """All ``$``-prefixed parameters overridable via ``run(constantsToOverwrite=...)``,
    mapped to the groups they are defined with.

    ``constantsToOverwrite`` is keyed by ``(name, group)``, and uprating factors
    such as ``$f_cpi`` are parameters of the ``Uprate`` function keyed by
    year-group rather than ``DefConst`` entries — so validating an override
    against :func:`system_constant_names` alone would reject them. This walks
    every function.

    Parameters
    ----------
    system : :class:`euromod.System`
        System to inspect.

    Returns
    -------
    :obj:`dict` [ :obj:`str`, :obj:`set` [ :obj:`str` ] ]
        Parameter name -> the groups it is defined with (``''`` when ungrouped).

    Example
    --------
    >>> from euromod.introspect import system_constant_params
    >>> params = system_constant_params(sys)          # doctest: +SKIP
    >>> sorted(params["$f_cpi"])[:3]                  # doctest: +SKIP
    ['2021', '2022', '2023']
    """
    params: dict = {}
    for pol in iter_real_policies(system):
        for fun in (_attr(pol, "functions", None) or []):
            for par in (_attr(fun, "parameters", None) or []):
                name = _attr(par, "name")
                if isinstance(name, str) and name.startswith("$"):
                    params.setdefault(name, set()).add(str(_attr(par, "group")))
    return params


def income_list_components(system, list_name, dataset=None, extensions=None) -> list:
    """Effective components of an income list under a dataset and extension set.

    Income list membership is not fixed: extensions add and remove components,
    and a list may be redefined further down the spine. Expanding a list by
    reading one ``DefIl`` definition therefore gives the wrong variables as soon
    as an extension is switched.

    Parameters
    ----------
    system : :class:`euromod.System`
        System whose spine defines the list.
    list_name : :obj:`str`
        Income list name, e.g. ``"ils_dispy"``.
    dataset : :obj:`str`, optional
        Dataset whose switch defaults apply. Default is the system's best match.
    extensions : :obj:`list` [ :obj:`tuple` [ :obj:`str`, :obj:`bool` ] ], optional
        Switch overrides on top of the dataset defaults, in the form taken by
        :meth:`euromod.System.run`. Default is :obj:`None`.

    Returns
    -------
    :obj:`list` [ :obj:`tuple` [ :obj:`str`, :obj:`str` ] ]
        ``(variable, sign)`` pairs, where sign is ``'+'`` or ``'-'``. Nested
        income lists are expanded recursively with signs multiplied through.

    Raises
    ------
    IncomeListLookupError
        If the list is unknown, inactive under this configuration, or cyclic.

    Example
    --------
    >>> from euromod.introspect import income_list_components
    >>> income_list_components(sys, "ils_dispy")               # doctest: +SKIP
    [('yem', '+'), ('yse', '+'), ('tin', '-'), ...]
    """
    raw = _system_raw(system)
    return _resolve_income_list(raw, _switch_config(raw, dataset, extensions), list_name)
