"""
Copyright 2024 European Commission

Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:
   https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software distributed
under the Licence is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied. See the Licence for the
specific language governing permissions and limitations under the Licence.

Software-version compatibility for the EUROMOD Python connector.

The connector talks to a EUROMOD engine (a set of .NET DLLs) that lives either in
an external EUROMOD installation or in the ``libs`` folder bundled with this
package. This module decides *which* engine to load, so that the connector never
silently runs against an engine older than it was built for:

* If the resolved external engine is at least :data:`REQUIRED_SOFTWARE_VERSION`,
  it is used.
* Otherwise (older, or its version cannot be determined) the connector falls back
  to the DLLs bundled with the package, emitting a warning.

The software version is the ``UI_VERSION`` constant inside ``EM_Common.dll``
(``EM_Common.DefGeneral.UI_VERSION``). It is not stamped on the DLL's file
version, so it can only be read by loading the assembly. To read it *without*
polluting the process's CLR (which cannot unload an assembly to swap in a
different engine afterwards) the probe runs in a throw-away subprocess. Any
failure of the probe is treated as "unknown version" and triggers the bundled
fallback, so the mechanism is safe by construction.
"""

import os
import sys
import subprocess
import warnings

#: Minimum EUROMOD software version this connector requires. Keep in lock-step
#: with the DLLs shipped in ``libs`` (bundled must be >= required) and, ideally,
#: with the Stata connector's floor.
REQUIRED_SOFTWARE_VERSION = "3.8.8"

#: Set env var ``EUROMOD_SKIP_VERSION_CHECK=1`` to bypass probing entirely and use
#: the resolved path as-is (useful for development against a known engine).
_SKIP_ENV = "EUROMOD_SKIP_VERSION_CHECK"


def parse(version):
    """Return ``(major, minor, patch)`` ints, or ``None`` if unparseable."""
    if not version:
        return None
    parts = str(version).strip().split(".")
    if len(parts) < 3:
        return None
    try:
        return tuple(int(parts[i]) for i in range(3))
    except (ValueError, TypeError):
        return None


def version_geq(a, b):
    """``True`` if version ``a`` >= version ``b``. Fails closed: an unparseable
    operand yields ``False`` (mirrors the Stata connector's ``version_geq``)."""
    pa, pb = parse(a), parse(b)
    if pa is None or pb is None:
        return False
    return pa >= pb


def read_software_version(dll_dir):
    """Read ``EM_Common.DefGeneral.UI_VERSION`` from the ``EM_Common.dll`` in
    *dll_dir* without loading it into this process. Returns the version string,
    or ``None`` on any failure (missing DLL, load error, timeout, ...)."""
    if not dll_dir:
        return None
    em_common = os.path.join(dll_dir, "EM_Common.dll")
    if not os.path.isfile(em_common):
        return None
    # Read the const in a throw-away subprocess so we never load EM_Common into
    # this process (which would prevent swapping to a different engine).
    child = (
        "import os, sys, clr;"
        "clr.AddReference(os.path.join(sys.argv[1], 'EM_Common.dll'));"
        "from EM_Common import DefGeneral;"
        "sys.stdout.write(str(DefGeneral.UI_VERSION))"
    )
    try:
        proc = subprocess.run(
            [sys.executable, "-c", child, dll_dir],
            capture_output=True, text=True, timeout=120,
        )
    except (OSError, subprocess.SubprocessError):
        return None
    if proc.returncode != 0:
        return None
    out = proc.stdout.strip()
    return out if parse(out) is not None else None


def _same_path(a, b):
    try:
        return os.path.normcase(os.path.realpath(a)) == os.path.normcase(os.path.realpath(b))
    except (OSError, ValueError):
        return False


def resolve_engine(candidate_dir, bundled_dir, is_explicit=False):
    """Decide which engine directory to load.

    Parameters
    ----------
    candidate_dir : str
        The externally-resolved engine directory (env override or a system
        install), or the bundled directory itself when nothing external exists.
    bundled_dir : str
        The package's ``libs`` folder (the fallback engine shipped with the
        connector).
    is_explicit : bool
        Whether *candidate_dir* came from an explicit ``EUROMOD_PATH`` override
        (affects the wording of the fallback warning).

    Returns
    -------
    (path, version, used_fallback) : tuple[str, str | None, bool]
    """
    if os.environ.get(_SKIP_ENV):
        return candidate_dir, read_software_version(candidate_dir), False

    # The candidate already is the bundled engine: use it, but surface a strong
    # warning if the shipped DLLs are themselves older than required (a packaging
    # / release problem). We warn rather than crash so the connector still runs.
    if _same_path(candidate_dir, bundled_dir):
        ver = read_software_version(bundled_dir)
        if ver is None:
            warnings.warn(
                "[euromod] Could not determine the version of the bundled EUROMOD "
                "engine in '%s'. Proceeding, but compatibility cannot be verified."
                % bundled_dir, RuntimeWarning, stacklevel=2)
        elif not version_geq(ver, REQUIRED_SOFTWARE_VERSION):
            warnings.warn(
                "[euromod] The bundled EUROMOD engine is version %s, older than the "
                "required %s. This is a packaging issue with the euromod package; "
                "please update it." % (ver, REQUIRED_SOFTWARE_VERSION),
                RuntimeWarning, stacklevel=2)
        return bundled_dir, ver, False

    # External candidate: probe it and fall back to bundled if too old/unreadable.
    ver = read_software_version(candidate_dir)
    if ver is not None and version_geq(ver, REQUIRED_SOFTWARE_VERSION):
        return candidate_dir, ver, False

    detected = ver if ver else "an undeterminable version"
    source = "EUROMOD_PATH" if is_explicit else "the EUROMOD installation"
    warnings.warn(
        "[euromod] The EUROMOD engine at '%s' (%s) is older than the required "
        "software version %s, or its version could not be determined. Falling back "
        "to the engine bundled with the euromod package ('%s'). Update your EUROMOD "
        "software (or %s) to a version >= %s to use it instead."
        % (candidate_dir, detected, REQUIRED_SOFTWARE_VERSION, bundled_dir,
           source, REQUIRED_SOFTWARE_VERSION),
        RuntimeWarning, stacklevel=2)

    bver = read_software_version(bundled_dir)
    if bver is None or not version_geq(bver, REQUIRED_SOFTWARE_VERSION):
        warnings.warn(
            "[euromod] The bundled fallback engine is %s, which is also older than "
            "the required %s. Please update the euromod package."
            % (bver if bver else "of an unknown version", REQUIRED_SOFTWARE_VERSION),
            RuntimeWarning, stacklevel=2)
    return bundled_dir, bver, True
