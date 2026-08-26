"""Configure the euromod connector source + DLL path before it is imported.

Opt-in. If ``EUROMOD_CONNECTOR_SRC`` points at a newer connector build (one that
ships the ``Statistics`` module), that directory is prepended to ``sys.path`` so
``import euromod`` resolves to it instead of the installed package. The connector
locates its .NET DLLs (including ``EM_Statistics.dll``) from the ``EUROMOD_PATH``
environment variable — set both in ``.env``; nothing is hard-coded here.

Call :func:`configure` before the first ``import euromod`` (model_session does
this at import time).
"""

import logging
import os
import sys

logger = logging.getLogger(__name__)

_configured = False
_console_hardened = False


def configure() -> None:
    """Idempotently prepend ``EUROMOD_CONNECTOR_SRC`` to ``sys.path`` if valid."""
    global _configured
    if _configured:
        return
    src = os.environ.get("EUROMOD_CONNECTOR_SRC", "").strip()
    if src:
        if os.path.isdir(src):
            if src not in sys.path:
                sys.path.insert(0, src)
            logger.info("euromod: using connector source at %s", src)
        else:
            logger.warning("EUROMOD_CONNECTOR_SRC is set but not a directory: %s", src)
    _configured = True


def harden_dotnet_console() -> None:
    """Detach the .NET Console from the process's console handle.

    The EUROMOD engine prints progress and *also* prints every warning to the
    Windows console (EM_Common.Communicator.ReportError -> Console.WriteLine).
    In processes without a valid console handle — services, detached/background
    launches — that write throws ``IOException: The handle is invalid`` and
    turns a harmless engine warning into a failed simulation.

    Redirecting Console.Out/Error to TextWriter.Null removes the fragile
    channel; nothing is lost: warnings are still delivered through the
    connector's structured result (``simulation.errors``), which
    run_simulation surfaces as ``euromod_warnings``/``euromod_errors``.

    Call after ``import euromod`` (CLR loaded); idempotent, best-effort.
    Set ``EUROMOD_KEEP_CONSOLE=1`` to skip (e.g. when watching engine progress
    interactively).
    """
    global _console_hardened
    if _console_hardened:
        return
    if os.environ.get("EUROMOD_KEEP_CONSOLE", "").strip().lower() in ("1", "true", "yes", "on"):
        _console_hardened = True
        return
    try:
        import System  # available once the connector has loaded the CLR
        System.Console.SetOut(System.IO.TextWriter.Null)
        System.Console.SetError(System.IO.TextWriter.Null)
        logger.info("euromod: .NET console detached (engine warnings surface via simulation results)")
    except Exception:
        logger.debug("euromod: could not detach .NET console", exc_info=True)
    _console_hardened = True
