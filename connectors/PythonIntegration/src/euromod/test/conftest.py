"""Pytest configuration for the euromod test suite.

Importing the ``euromod`` package eagerly loads the EUROMOD .NET engine. To keep the
tests self-contained (and independent of whatever EUROMOD version happens to be
installed system-wide), default the engine to the DLLs bundled with the package.
An explicitly-set ``EUROMOD_PATH`` still takes precedence.
"""
import os

_LIBS = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "libs")
os.environ.setdefault("EUROMOD_PATH", _LIBS)
