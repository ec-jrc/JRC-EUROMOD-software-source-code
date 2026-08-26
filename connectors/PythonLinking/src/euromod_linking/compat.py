"""Method requirements versus what the model actually provides.

A methodology declares what its runs need — ``lma_labour_alignment`` needs the
LMA add-on and the ``LMA_trans`` extension switch — and this module checks that
the model in front of it has them. An unknown add-on is dropped by the engine
with only a console message, so left unchecked both simulations complete
normally and produce identical output, and the failure surfaces at the end as
`NoEffectError`, after paying for two runs. Answering the question first is why
this is called from the ``validate_only`` path.

Two kinds of evidence, deliberately unequal.

Capability — authoritative
--------------------------
Whether the model exposes an add-on and an extension switch is cheap and exact:
``model.addons`` is a directory listing, and `query.system_extension_names`
reads what the system will actually accept. This is what decides. Where the
model cannot be read at all the answer is *undeterminable*, recorded as a note,
never as a failure — "cannot validate" is not "invalid".

Release — advisory
------------------
Which EUROMOD release a model folder holds is not reliably knowable. The
documented marker ``XMLParam/Config/EuromodVersion.txt`` has writer code in the
UI but ships in no real release; the folder name is what the UI itself falls
back to (``EM_AppContext.GetProjectName`` feeding ``<EMVERSION>``), and renaming
the folder erases it. So a detected release only enriches the message — *"the
LMA_trans extension is first shipped in J2.54, this model looks like J2.19"* —
and failing to detect one never blocks anything.

Set ``EUROMOD_SKIP_COMPAT_CHECK=1`` to bypass the check, mirroring the
connector's own ``EUROMOD_SKIP_VERSION_CHECK``.
"""

import logging
import os
import re
from dataclasses import dataclass
from pathlib import Path

from euromod_linking import query, registry
from euromod_linking.session import model_lock

logger = logging.getLogger(__name__)

#: Release identifiers are ``<letter><major>.<minor>`` with an optional trailing
#: ``+`` (``J2.19``, ``I6.0+``), the scheme ``VCAPI.GetNextAutoVersion`` grows
#: from ``A0.0``. Anchored on a non-alphanumeric so ``EUROMOD_MASTER_VERSION_J2.19``
#: matches on ``J2.19`` and not on the ``D_J2`` of some other name.
#:
#: Case-insensitive, because model folders are named by hand and a lowercased
#: one is common; matches are normalised to upper case for display and
#: comparison, so ``..._j2.54`` and ``..._J2.54`` are the same release.
RELEASE_RE = re.compile(r"(?<![A-Za-z0-9])([A-Za-z])(\d+)\.(\d+)(\+?)")

#: Where `model_release` looks, in order of trust. The version file is the
#: documented contract and wins when present; the folder name is what actually
#: carries the release in practice.
RELEASE_SOURCES = ("version-file", "folder-name", "licence-file", "em-log")


@dataclass(frozen=True)
class Requirement:
    """One thing a method needs from the model, and whether it is there.

    ``satisfied`` is tri-state: True, False, or None when the model could not be
    read — a distinction that matters, because treating "cannot read" as "not
    present" would refuse to run against a perfectly good model.
    """

    kind: str                   # "addon" | "extension"
    name: str                   # "LMA" | "LMA_trans"
    system: str | None = None   # the add-on system, e.g. "LMA_BE"
    satisfied: bool | None = None
    detail: str = ""


@dataclass(frozen=True)
class CompatibilityReport:
    """What one method needs, what this model has, and the gap between them.

    ``problems`` are hard failures — the run would not do what was asked.
    ``notes`` are advisory: an undeterminable requirement, or a release that
    looks older than the method's floor.

    Read-only property ``ok`` is True when there are no problems; notes never
    make a report not-ok. This is the field to check.
    """

    method: str
    country_code: str
    system_name: str
    model_release: str | None = None
    release_source: str | None = None
    min_model_release: str | None = None
    requirements: tuple = ()
    problems: tuple = ()
    notes: tuple = ()

    @property
    def ok(self) -> bool:
        """True when nothing blocks the run. Notes do not make a report not-ok."""
        return not self.problems

    def __str__(self):
        head = f"{self.method} on {self.country_code}/{self.system_name}: " + (
            "ok" if self.ok else "NOT ok")
        lines = [head]
        lines += [f"  problem: {p}" for p in self.problems]
        lines += [f"  note:    {n}" for n in self.notes]
        return "\n".join(lines)


def parse_release(release: str | None):
    """Split ``"J2.54"`` into a sortable ``("J", 2, 54, False)``, or None.

    The last element is the trailing ``+`` of a beta/rolling release, which
    sorts just above the bare version so ``J1.86+`` is not read as older than
    ``J1.86``. None for anything that does not look like a release at all —
    callers treat that as "cannot compare", not as "older"."""
    if not release:
        return None
    m = RELEASE_RE.search(str(release).strip())
    if not m:
        return None
    letter, major, minor, plus = m.groups()
    return (letter.upper(), int(major), int(minor), bool(plus))


def canonical_release(release: str | None) -> str | None:
    """``"..._j2.54"`` -> ``"J2.54"``; unparseable input comes back stripped.

    Every detection source is run through this, so a report shows one spelling
    of a release whether it came from a lowercased folder name, a licence file
    or the change log."""
    parsed = parse_release(release)
    if parsed is None:
        return str(release).strip() if release else None
    letter, major, minor, plus = parsed
    return f"{letter}{major}.{minor}{'+' if plus else ''}"


def release_geq(release: str | None, floor: str | None):
    """Is `release` at least `floor`? None when either cannot be parsed.

    Fails *open* — the caller turns None into a note, not a refusal. This is the
    opposite of ``euromod.utils._version.version_geq``, which gates the engine
    and so fails closed; here the capability probe is the real check and the
    release is only there to write a better message."""
    a, b = parse_release(release), parse_release(floor)
    if a is None or b is None:
        return None
    return a >= b


def _release_from_version_file(root: Path):
    """Line 1 of ``XMLParam/Config/EuromodVersion.txt`` (line 2 is a label)."""
    path = root / "XMLParam" / "Config" / "EuromodVersion.txt"
    try:
        first = path.read_text(encoding="utf-8", errors="replace").splitlines()[0].strip()
    except Exception:
        return None
    return first or None


def _release_from_folder_name(root: Path):
    """The release embedded in the model folder name, e.g.
    ``EUROMOD_MASTER_VERSION_J2.19``. Checks the parent too, because nested
    ``EUROMOD_RELEASES_J0.1+\\EUROMOD_RELEASES_J0.1+`` layouts occur in the wild."""
    for name in (root.name, root.parent.name if root.parent != root else ""):
        m = RELEASE_RE.search(name or "")
        if m:
            return m.group(0)
    return None


def _release_from_licence_file(root: Path):
    """Public releases carry ``EUROMOD_model_licence_J1.86+_beta.txt`` at the
    model root. Masters do not, so this only ever fires for public releases."""
    try:
        for path in sorted(root.glob("EUROMOD_model_licence_*")):
            m = RELEASE_RE.search(path.name)
            if m:
                return m.group(0)
    except Exception:
        pass
    return None


def _release_from_em_log(root: Path):
    """The largest value in the ``Version`` column of ``Log/EM_LOG.xlsx``.

    The only marker that lives in the model's *content* rather than its name, so
    it survives a rename — but the workbook runs to tens of megabytes and tens of
    thousands of rows, which is why this is opt-in. Read the column, never the
    worksheet name: masters still carry stale sheet names from earlier series."""
    path = root / "Log" / "EM_LOG.xlsx"
    if not path.is_file():
        return None
    try:
        import openpyxl
    except ImportError:
        logger.debug("openpyxl not installed; skipping EM_LOG release detection")
        return None
    try:
        wb = openpyxl.load_workbook(path, read_only=True, data_only=True)
        best = None
        for ws in wb.worksheets:
            rows = ws.iter_rows(values_only=True)
            header = next(rows, None) or ()
            try:
                col = [str(h or "").strip().lower() for h in header].index("version")
            except ValueError:
                continue
            for row in rows:
                if col >= len(row):
                    continue
                parsed = parse_release(row[col])
                if parsed and (best is None or parsed > best[0]):
                    best = (parsed, str(row[col]).strip())
        wb.close()
        return best[1] if best else None
    except Exception:
        logger.debug("could not read %s", path, exc_info=True)
        return None


def model_release(model_path: str, *, deep: bool = False):
    """Best-effort ``(release, source)`` for a model folder, e.g.
    ``("J2.19", "folder-name")``.

    Tries the documented version file, then the folder name, then a public
    release's licence file; with ``deep=True`` it finally parses the change log,
    which is slow but survives a renamed folder. ``(None, None)`` means the
    release could not be determined — which callers must treat as unknown, never
    as too old."""
    if not model_path:
        return (None, None)
    root = Path(model_path)
    finders = [("version-file", _release_from_version_file),
               ("folder-name", _release_from_folder_name),
               ("licence-file", _release_from_licence_file)]
    if deep:
        finders.append(("em-log", _release_from_em_log))
    for source, finder in finders:
        try:
            found = finder(root)
        except Exception:
            logger.debug("release detection via %s failed", source, exc_info=True)
            continue
        if found:
            return (canonical_release(found), source)
    return (None, None)


def _skip_requested() -> bool:
    return os.environ.get("EUROMOD_SKIP_COMPAT_CHECK", "").strip().lower() in ("1", "true", "yes", "on")


def _entry_names(entry, cc: str):
    """An add-on requirement is ``("LMA", "LMA_{cc}")`` or a bare ``"LMA"``;
    return ``(addon_name, addon_system_or_None)`` with ``{cc}`` resolved — the
    same formatting `scenarios.run_arguments` applies before the run."""
    parts = list(entry) if isinstance(entry, (list, tuple)) else [entry]
    names = [str(p).format(cc=cc) for p in parts]
    return names[0], (names[1] if len(names) > 1 else None)


def _known_addons(model):
    """Add-on names the model ships, or None when they cannot be listed."""
    try:
        return {str(k) for k in model.addons.keys()}
    except Exception:
        logger.debug("could not list model add-ons", exc_info=True)
        return None


def _addon_applies(model, addon_name: str, addon_system: str | None, system_name: str):
    """Does `addon_system` apply to `system_name`? None when undeterminable.

    Loading an add-on triggers an EM3 translation of its XML, so this is the
    expensive part of the check and runs only once the add-on is known to be
    present."""
    if not addon_system or not system_name:
        return None
    try:
        applicable = model.addons[addon_name].get_applicable_systems(system_name)
        names = {str(k) for k in applicable.keys()}
    except Exception:
        logger.debug("could not resolve applicable systems of add-on %s", addon_name, exc_info=True)
        return None
    return addon_system in names


def check_compatibility(system, method, *, deep_release: bool = False) -> CompatibilityReport:
    """Can this methodology actually run against this model and system?

    `method` is a reference like ``"lma_labour_alignment"`` or an
    `MethodSpec`. `system` is a live connector System, from which the country,
    the model and the model path are reached.

    Checks each declared add-on and extension switch against the model, and
    compares the detected release against the method's ``min_model_release``.
    Only the first of those can produce a problem; the release only ever adds a
    note explaining one. With ``deep_release`` the change log is parsed too, at
    the cost of reading a large workbook."""
    spec = method if isinstance(method, registry.MethodSpec) else registry.resolve(method)
    system_name = str(query._attr(system, "name") or "")
    country = getattr(system, "parent", None)
    cc = str(query._attr(country, "name") or "").upper()
    model = getattr(country, "model", None)
    model_path = str(query._attr(model, "model_path") or "")

    release, source = model_release(model_path, deep=deep_release)
    addon_entries, switch_entries = (spec.addon_requirements or ((), ()))
    reqs: list[Requirement] = []
    problems: list[str] = []
    notes: list[str] = []

    with model_lock:
        known_addons = _known_addons(model) if model is not None else None
        for entry in addon_entries:
            addon_name, addon_system = _entry_names(entry, cc)
            if known_addons is None:
                reqs.append(Requirement("addon", addon_name, addon_system, None,
                                        "could not list the model's add-ons"))
                notes.append(f"Could not verify the {addon_name} add-on: the model's add-on "
                             "folder could not be listed.")
                continue
            if addon_name not in known_addons:
                reqs.append(Requirement("addon", addon_name, addon_system, False,
                                        f"model ships {sorted(known_addons)}"))
                problems.append(f"{spec.name} needs the {addon_name} add-on, which this model does "
                                f"not ship (it has: {', '.join(sorted(known_addons)) or 'none'}).")
                continue
            applies = _addon_applies(model, addon_name, addon_system, system_name)
            if applies is False:
                reqs.append(Requirement("addon", addon_name, addon_system, False,
                                        f"no add-on system {addon_system} applies to {system_name}"))
                problems.append(f"{spec.name} needs add-on system {addon_system}, but no system of "
                                f"the {addon_name} add-on applies to {system_name}.")
            elif applies is None and addon_system:
                reqs.append(Requirement("addon", addon_name, addon_system, None,
                                        "add-on present; applicability could not be resolved"))
                notes.append(f"The {addon_name} add-on is present but its applicability to "
                             f"{system_name} could not be resolved.")
            else:
                reqs.append(Requirement("addon", addon_name, addon_system, True, "present"))

        known_ext = None
        if switch_entries and cc:
            try:
                known_ext = query.system_extension_names(cc, system_name or None)
            except Exception:
                logger.debug("could not read extension names for %s/%s", cc, system_name, exc_info=True)

    for entry in switch_entries:
        ext_name = str(entry[0] if isinstance(entry, (list, tuple)) else entry).format(cc=cc)
        if not known_ext:
            reqs.append(Requirement("extension", ext_name, None, None,
                                    "the model's extension names could not be read"))
            notes.append(f"Could not verify the {ext_name} extension: this model's extension "
                         "names could not be read.")
            continue
        if ext_name in known_ext:
            reqs.append(Requirement("extension", ext_name, None, True, "accepted by this system"))
        else:
            reqs.append(Requirement("extension", ext_name, None, False,
                                    f"system accepts {len(known_ext)} other extensions"))
            problems.append(f"{spec.name} needs the {ext_name} extension, which {cc}/{system_name} "
                            "does not accept. An unknown switch is silently ignored by the engine, "
                            "so the run would appear to succeed without applying it.")

    floor = spec.min_model_release
    if floor:
        seen = f"{release} (from {source})" if release else None
        new_enough = release_geq(release, floor)
        if new_enough is False:
            notes.append(f"{spec.name} needs EUROMOD release {floor} or later; this model looks "
                         f"like {seen}.")
        elif new_enough is None:
            notes.append(f"{spec.name} needs EUROMOD release {floor} or later. This model's "
                         "release could not be determined, so that was not checked.")
        elif problems:
            # Say only what is certain. The release rules out "upgrade the model",
            # but it cannot distinguish an add-on that was never installed from one
            # that is installed and simply does not cover this system.
            notes.append(f"This model looks like {seen}, at or above the {floor} floor for "
                         f"{spec.name} — so the release is not what is missing.")

    return CompatibilityReport(
        method=spec.name, country_code=cc, system_name=system_name,
        model_release=release, release_source=source, min_model_release=floor,
        requirements=tuple(reqs), problems=tuple(problems), notes=tuple(notes))


def check_all(system, *, deep_release: bool = False) -> list[CompatibilityReport]:
    """A report per registered methodology, in registry order."""
    return [check_compatibility(system, spec, deep_release=deep_release)
            for spec in registry.list_specs()]


def compatibility_matrix(model, country_code: str | None = None, *,
                         deep_release: bool = False):
    """Which methodologies this model supports, as a DataFrame.

    One row per (country, system, methodology) with ``ok``, the detected
    release, and the problems — the answer to "what can I actually run here"
    without writing a scenario first. Restrict with `country_code`, since
    walking every country of a full model means loading every country."""
    import pandas as pd

    codes = [country_code.upper()] if country_code else [str(k) for k in model.countries.keys()]
    rows = []
    for cc in codes:
        try:
            country = model[cc]
            systems = list(country.systems)
        except Exception:
            logger.debug("could not read systems of %s", cc, exc_info=True)
            continue
        for system in systems:
            for report in check_all(system, deep_release=deep_release):
                rows.append({
                    "country": cc,
                    "system": report.system_name,
                    "methodology": report.method,
                    "ok": report.ok,
                    "model_release": report.model_release or "",
                    "min_model_release": report.min_model_release or "",
                    "problems": " ".join(report.problems),
                    "notes": " ".join(report.notes),
                })
    return pd.DataFrame(rows, columns=["country", "system", "methodology", "ok", "model_release",
                                       "min_model_release", "problems", "notes"])
