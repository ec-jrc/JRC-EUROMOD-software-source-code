"""Connector-backed read helpers for a country's policy structure.

Two-stage design so extension exploration is cheap:

1. `_build_raw_country` materialises the connector object graph for a country
   ONCE (the expensive first-touch interop), extracting every policy/constant
   definition plus its extension links (shortName, baseOff) as plain data.
   Cached per country.
2. `_resolve` builds a CountryIndex from that raw data for a given
   (dataset, extensions) configuration in pure Python — no connector interop.
   Duplicate definitions are resolved *switch- and extension-aware*: a per-system
   switch config (get_switch_value baseline for a dataset + caller overrides)
   drives the same include/exclude logic as euromod.container.filter; the
   effective value is the last non-'n/a' among active definitions in spine order
   (fallback: last non-'n/a' overall).

Both raw and resolved results are cached; the model is read-only at runtime so
caches stay valid for the process lifetime.

`system_extension_names`, `system_constant_names`, `system_constant_params` and
`income_list_components` are kept here rather than taken from `euromod`, so the
package works against every released version of the connector.
"""

import logging
from dataclasses import dataclass, field

from euromod_linking.session import get_country, model_lock

logger = logging.getLogger(__name__)


@dataclass
class ConstantInfo:
    name: str
    group: str
    comment: str
    policy: str


@dataclass
class PolicyInfo:
    name: str
    comment: str
    type: str
    order: int
    constants: list[str] = field(default_factory=list)


@dataclass
class SystemInfo:
    name: str
    year: str
    policy_switches: dict[str, str] = field(default_factory=dict)
    constant_values: dict[str, str] = field(default_factory=dict)


@dataclass
class CountryIndex:
    code: str
    policies: dict[str, PolicyInfo] = field(default_factory=dict)
    systems: dict[str, SystemInfo] = field(default_factory=dict)
    constants: dict[str, ConstantInfo] = field(default_factory=dict)


def _attr(obj, name, default=""):
    """getattr that swallows the connector's .NET-delegated AttributeErrors.
    Strips strings to match the XML parser's .strip() behaviour."""
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
    """Yield policies of a Country or System, skipping ReferencePolicy objects
    (which lack .functions/.comment and crash naive iteration/find())."""
    for p in scope.policies:
        if type(p).__name__ == "ReferencePolicy":
            continue
        yield p


_IL_SERVICE_PARAMS = {"Name", "Run_Cond", "Output_Var", "TAX_UNIT", "Warn_If_NonPositive"}


def _il_component_name(name: str) -> bool:
    """True when a DefIl parameter name looks like a component (variable or
    nested income list), not a service parameter."""
    import re
    if name in _IL_SERVICE_PARAMS or name.startswith("#"):
        return False
    return bool(re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", name))


def _ext_links(elem):
    """Extension links of a policy/function as plain data: ((shortName, baseOff_bool), ...)."""
    out = []
    try:
        for e in (getattr(elem, "extensions", None) or []):
            sn = _attr(e, "shortName")
            if sn:
                out.append((sn, _attr(e, "baseOff") == "true"))
    except Exception:
        pass
    return tuple(out)


def _bestmatch_dataset(system):
    for attr in ("bestmatch_datasets", "datasets"):
        try:
            for d in getattr(system, attr):
                n = _attr(d, "name")
                if n:
                    return n
        except Exception:
            continue
    return None


# --- Stage 1: one-time raw materialisation (cached per country) --------------
def _build_raw_country(country_code: str) -> dict:
    cc = country_code.upper()
    country = get_country(cc)
    systems = []

    for system in country.systems:
        sysname = _attr(system, "name")
        # dataset names + best-match, and per-(dataset,extension) switch defaults
        try:
            ds_names = {_attr(d, "name") for d in system.datasets}
        except Exception:
            ds_names = set()
        switch_defaults: dict = {}
        try:
            for sw in country.get_switch_value(sys_name=sysname):
                ds = _attr(sw, "data_name")
                en = _attr(sw, "extension_name")
                if ds and en:
                    switch_defaults.setdefault(ds, {})[en] = _attr(sw, "value") == "on"
        except Exception:
            logger.debug("get_switch_value failed for %s", sysname, exc_info=True)

        policies = []
        const_occ: dict[str, list[dict]] = {}
        il_occ: dict[str, list[dict]] = {}
        for pol in iter_real_policies(system):
            pname = _attr(pol, "name")
            if not pname:
                continue
            pso = _int(_attr(pol, "spineOrder", 0))
            policies.append({
                "name": pname, "switch": _attr(pol, "switch"), "ext": _ext_links(pol),
                "spineOrder": pso, "order": _int(_attr(pol, "order", 0)),
                "comment": _attr(pol, "comment"), "type": _attr(pol, "type"),
            })
            for fun in (_attr(pol, "functions", None) or []):
                fname = _attr(fun, "name")
                if fname == "DefConst":
                    fext = _ext_links(fun)
                    fsw = _attr(fun, "switch")
                    fo = _int(_attr(fun, "order", 0))
                    for par in (_attr(fun, "parameters", None) or []):
                        cname = _attr(par, "name")
                        if not (isinstance(cname, str) and cname.startswith("$")):
                            continue
                        const_occ.setdefault(cname, []).append({
                            "key": (pso, fo, _int(_attr(par, "order", 0))),
                            "fun_ext": fext, "fun_switch": fsw, "value": _attr(par, "value"),
                            "policy": pname, "group": str(_attr(par, "group")), "comment": _attr(par, "comment"),
                        })
                elif fname == "DefIl":
                    fext = _ext_links(fun)
                    fsw = _attr(fun, "switch")
                    fo = _int(_attr(fun, "order", 0))
                    il_name = None
                    components: list = []
                    for par in (_attr(fun, "parameters", None) or []):
                        cname = _attr(par, "name")
                        if not isinstance(cname, str) or not cname:
                            continue
                        if cname == "Name":
                            il_name = str(_attr(par, "value")).strip()
                        elif _il_component_name(cname) and str(_attr(par, "private")).lower() != "yes":
                            components.append((cname, str(_attr(par, "value")).strip(),
                                               _ext_links(par)))
                    if il_name:
                        il_occ.setdefault(il_name, []).append({
                            "key": (pso, fo), "fun_ext": fext, "fun_switch": fsw,
                            "policy": pname, "components": components,
                        })

        systems.append({
            "name": sysname, "year": str(_attr(system, "year")),
            "dataset_names": ds_names, "bestmatch": _bestmatch_dataset(system),
            "switch_defaults": switch_defaults, "policies": policies,
            "const_occ": const_occ, "il_occ": il_occ,
        })

    return {"code": cc, "systems": systems}


# --- Stage 2: pure-Python resolution per (dataset, extensions) ----------------
def _filter_include(ext_links, cfg):
    """Pure-Python port of euromod.container.filter for one element.
    Returns None when the element has no extension links (caller uses base switch)."""
    if not ext_links:
        return None
    explicit_removal = False
    explicit_inclusion = False
    for sn, base_off in ext_links:
        if cfg.get(sn, False):          # extension on
            if base_off:
                explicit_removal = True
            else:
                explicit_inclusion = True
        elif not base_off:              # extension off, but element is inclusion-when-on
            explicit_removal = True
    return explicit_inclusion or not explicit_removal


def _active(base_switch, ext_links, cfg):
    """Extension-gated element: filter decides. Un-gated: base switch decides."""
    inc = _filter_include(ext_links, cfg)
    return (base_switch == "on") if inc is None else inc


def _resolve(raw: dict, dataset, extensions) -> CountryIndex:
    idx = CountryIndex(code=raw["code"])
    overrides = [(str(n), bool(v)) for n, v in (extensions or [])]

    for rsys in raw["systems"]:
        si = SystemInfo(name=rsys["name"], year=rsys["year"])
        ds = dataset if (dataset and dataset in rsys["dataset_names"]) else rsys["bestmatch"]
        cfg = dict(rsys["switch_defaults"].get(ds, {}))
        for n, v in overrides:
            cfg[n] = v

        switch_occ: dict[str, list[str]] = {}
        pol_active: dict[str, bool] = {}
        for p in rsys["policies"]:
            act = _active(p["switch"], p["ext"], cfg)
            if p["name"] not in idx.policies:
                idx.policies[p["name"]] = PolicyInfo(
                    name=p["name"], comment=p["comment"], type=p["type"], order=p["order"])
            switch_occ.setdefault(p["name"], []).append("on" if act else p["switch"])
            pol_active[p["name"]] = pol_active.get(p["name"], False) or act

        for pname, sws in switch_occ.items():
            if "on" in sws:
                si.policy_switches[pname] = "on"
            elif "off" in sws:
                si.policy_switches[pname] = "off"
            else:
                si.policy_switches[pname] = sws[-1] if sws else "n/a"

        for cname, occs in rsys["const_occ"].items():
            resolved = []
            for o in occs:
                fun_ok = _active(o["fun_switch"], o["fun_ext"], cfg)
                active = pol_active.get(o["policy"], False) and fun_ok
                resolved.append((o["key"], active, o["value"], o["policy"], o["group"], o["comment"]))
                pol = idx.policies.get(o["policy"])
                if pol is not None and cname not in pol.constants:
                    pol.constants.append(cname)
            resolved.sort(key=lambda t: t[0])
            act = [t for t in resolved if t[1] and t[2] != "n/a"]
            pool = act if act else [t for t in resolved if t[2] != "n/a"]
            chosen = pool[-1] if pool else resolved[-1]
            si.constant_values[cname] = chosen[2]
            if cname not in idx.constants:
                idx.constants[cname] = ConstantInfo(
                    name=cname, group=chosen[4], comment=chosen[5], policy=chosen[3])

        idx.systems[si.name] = si
    return idx


# --- caches + public API ------------------------------------------------------
_raw_cache: dict = {}
_index_cache: dict = {}


def _get_raw(cc: str) -> dict:
    if cc not in _raw_cache:
        _raw_cache[cc] = _build_raw_country(cc)
    return _raw_cache[cc]


def build_country_index(country_code: str, dataset=None, extensions=None) -> CountryIndex:
    """Materialise (or reuse) the country's raw data and resolve it for one config."""
    with model_lock:
        return _resolve(_get_raw(country_code.upper()), dataset, extensions)


def get_country_index(country_code: str, dataset=None, extensions=None) -> CountryIndex | None:
    cc = country_code.upper()
    key = (cc, dataset, tuple(sorted((str(n), bool(v)) for n, v in (extensions or []))))
    with model_lock:
        if key in _index_cache:
            return _index_cache[key]
        try:
            idx = _resolve(_get_raw(cc), dataset, extensions)
        except Exception:
            logger.exception("Failed to build connector index for %s", cc)
            return None
        _index_cache[key] = idx
        logger.info("Indexed %s (dataset=%s, ext=%s): %d policies, %d systems, %d constants",
                    cc, dataset, key[2], len(idx.policies), len(idx.systems), len(idx.constants))
        return idx


def clear_caches():
    with model_lock:
        _raw_cache.clear()
        _index_cache.clear()


def system_constant_names(system) -> set[str]:
    """$-prefixed constant names defined (via DefConst) in a live System object.
    Extension-independent; used to validate constants_to_overwrite."""
    names: set[str] = set()
    for pol in iter_real_policies(system):
        for fun in (_attr(pol, "functions", None) or []):
            if _attr(fun, "name") != "DefConst":
                continue
            for par in (_attr(fun, "parameters", None) or []):
                n = _attr(par, "name")
                if isinstance(n, str) and n.startswith("$"):
                    names.add(n)
    return names


class IncomeListLookupError(KeyError):
    def __init__(self, message: str, available: list[str]):
        super().__init__(message)
        self.available = available

    def __str__(self):
        return self.args[0]


def _pol_active_map(rsys: dict, cfg: dict) -> dict[str, bool]:
    """Policy name -> active under this extension config (any active occurrence)."""
    out: dict[str, bool] = {}
    for p in rsys["policies"]:
        act = _active(p["switch"], p["ext"], cfg)
        out[p["name"]] = out.get(p["name"], False) or act
    return out


def _sign_mult(a: str, b: str) -> str:
    return "+" if (a == "-") == (b == "-") else "-"


def resolve_income_list(rsys: dict, cfg: dict, list_name: str,
                        _seen: frozenset = frozenset()) -> list[tuple[str, str]]:
    """Pure resolution of an income list's effective components under one
    (dataset, extensions) switch config, from a system's raw data.

    Extension-aware at every level: the defining policy, the DefIl function,
    and each component parameter must be active/included. Duplicate DefIl
    definitions resolve in spine order (last active wins, the constants rule);
    nested ils_*/il_* components expand recursively, cycle-safe, with signs
    multiplied through. Deterministic; components in definition order, deduped.
    """
    occs = rsys.get("il_occ", {}).get(list_name)
    if not occs:
        raise IncomeListLookupError(
            f"Unknown income list {list_name!r} in system {rsys.get('name')!r}",
            sorted(rsys.get("il_occ", {})))
    if list_name in _seen:
        raise IncomeListLookupError(
            f"Cyclic income-list definition at {list_name!r}", sorted(rsys.get("il_occ", {})))
    pol_active = _pol_active_map(rsys, cfg)

    chosen = None
    for occ in sorted(occs, key=lambda o: o["key"]):
        if pol_active.get(occ["policy"], False) and _active(occ["fun_switch"], occ["fun_ext"], cfg):
            chosen = occ
    if chosen is None:
        raise IncomeListLookupError(
            f"Income list {list_name!r} is not active under this dataset/extension "
            "configuration", sorted(rsys.get("il_occ", {})))

    out: list[tuple[str, str]] = []
    seen_vars: set[str] = set()
    for cname, sign, par_ext in chosen["components"]:
        inc = _filter_include(par_ext, cfg)
        if inc is False:
            continue
        sign = sign if sign in ("+", "-") else "+"
        if cname.startswith(("ils_", "il_")) and cname in rsys.get("il_occ", {}):
            for sub_name, sub_sign in resolve_income_list(
                    rsys, cfg, cname, _seen | {list_name}):
                if sub_name not in seen_vars:
                    seen_vars.add(sub_name)
                    out.append((sub_name, _sign_mult(sign, sub_sign)))
        else:
            if cname not in seen_vars:
                seen_vars.add(cname)
                out.append((cname, sign))
    return out


def system_extension_names(country_code: str, system_name: str | None = None) -> set[str]:
    """Extension short names the model accepts in run(switches=...).

    A switch the model does not know is silently dropped by the engine (it only
    mutters "An error occurred during the processing of the ExtensionSwitches"),
    so a scenario can appear to run while the behaviour it asked for never
    happened. Callers validate against this first.

    Sources, unioned: the country's own extensions (country.extensions merges
    local + model-wide, i.e. everything declared in
    Config/SWITCHABLEPOLICYCONFIG.xml — including add-on extensions such as
    LMA_trans) and the per-dataset switch defaults actually observed for the
    system. Returns an empty set if neither can be read, which callers treat as
    "cannot validate" rather than "nothing is valid"."""
    cc = country_code.upper()
    names: set[str] = set()
    with model_lock:
        try:
            country = get_country(cc)
            for ext in (getattr(country, "extensions", None) or []):
                sn = _attr(ext, "shortName")
                if sn:
                    names.add(sn)
        except Exception:
            logger.debug("could not read extensions for %s", cc, exc_info=True)
        raw = _get_raw(cc)
    for rsys in raw["systems"]:
        if system_name and rsys["name"] != system_name:
            continue
        for ds_cfg in rsys["switch_defaults"].values():
            names.update(ds_cfg)
    return names


def income_list_components(country_code: str, system_name: str, list_name: str,
                           dataset=None, extensions=None) -> list[tuple[str, str]]:
    """Effective (variable, sign) components of an income list in a live system,
    resolved for the given (dataset, extensions) configuration."""
    with model_lock:
        raw = _get_raw(country_code.upper())
    rsys = next((s for s in raw["systems"] if s["name"] == system_name), None)
    if rsys is None:
        raise IncomeListLookupError(
            f"Unknown system {system_name!r} for {country_code.upper()}",
            [s["name"] for s in raw["systems"]])
    ds = dataset if (dataset and dataset in rsys["dataset_names"]) else rsys["bestmatch"]
    cfg = dict(rsys["switch_defaults"].get(ds, {}))
    for n, v in (extensions or []):
        cfg[str(n)] = bool(v)
    return resolve_income_list(rsys, cfg, list_name)


def system_constant_params(system) -> dict[str, set[str]]:
    """All $-prefixed parameter names overridable via run(constantsToOverwrite=...),
    mapped to the set of groups they are defined with ('' = ungrouped).

    Walks every function, not just DefConst: uprating factors (e.g. $f_cpi) are
    parameters of the Uprate function keyed by year-group, and must validate as
    ("$f_cpi", "2023")-style overrides."""
    params: dict[str, set[str]] = {}
    for pol in iter_real_policies(system):
        for fun in (_attr(pol, "functions", None) or []):
            for par in (_attr(fun, "parameters", None) or []):
                n = _attr(par, "name")
                if isinstance(n, str) and n.startswith("$"):
                    params.setdefault(n, set()).add(str(_attr(par, "group")))
    return params
