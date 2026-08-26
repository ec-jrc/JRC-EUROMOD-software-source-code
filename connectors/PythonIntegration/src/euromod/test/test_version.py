"""Unit tests for the software-version compatibility logic (utils._version).

These are pure-logic tests: they monkeypatch the version probe, so no real
EUROMOD engine / pythonnet is needed (mirrors the CLR-mocking approach in
test_statistics.py).
"""
import pytest

from ..utils import _version
from ..utils._version import parse, version_geq, resolve_engine, REQUIRED_SOFTWARE_VERSION


# --- parse / version_geq -----------------------------------------------------

def test_parse_valid():
    assert parse("3.8.7") == (3, 8, 7)
    assert parse("10.0.1") == (10, 0, 1)


@pytest.mark.parametrize("bad", ["3.8", "x.y.z", "3.8.x", "", None])
def test_parse_invalid(bad):
    assert parse(bad) is None


def test_version_geq_true():
    assert version_geq("3.8.7", "3.8.7")
    assert version_geq("3.9.0", "3.8.7")
    assert version_geq("4.0.0", "3.8.7")


def test_version_geq_false():
    assert not version_geq("3.8.6", "3.8.7")
    assert not version_geq("3.7.10", "3.8.7")


@pytest.mark.parametrize("a,b", [("3.8", "3.8.7"), ("abc", "3.8.7"), ("3.8.7", "")])
def test_version_geq_fails_closed_on_malformed(a, b):
    assert version_geq(a, b) is False


# --- resolve_engine decision table -------------------------------------------

def _patch(monkeypatch, mapping):
    monkeypatch.setattr(_version, "read_software_version", lambda d: mapping.get(d))


def test_external_compatible_used(monkeypatch):
    req = REQUIRED_SOFTWARE_VERSION
    _patch(monkeypatch, {"ext": req, "bundled": req})
    path, ver, fb = resolve_engine("ext", "bundled")
    assert (path, ver, fb) == ("ext", req, False)


def test_external_newer_used(monkeypatch):
    _patch(monkeypatch, {"ext": "4.1.0", "bundled": "3.8.7"})
    path, ver, fb = resolve_engine("ext", "bundled")
    assert path == "ext" and fb is False


def test_external_too_old_falls_back(monkeypatch):
    _patch(monkeypatch, {"ext": "3.8.2", "bundled": "3.8.7"})
    with pytest.warns(RuntimeWarning):
        path, ver, fb = resolve_engine("ext", "bundled")
    assert (path, ver, fb) == ("bundled", "3.8.7", True)


def test_external_unreadable_falls_back(monkeypatch):
    _patch(monkeypatch, {"ext": None, "bundled": "3.8.7"})
    with pytest.warns(RuntimeWarning):
        path, ver, fb = resolve_engine("ext", "bundled")
    assert (path, ver, fb) == ("bundled", "3.8.7", True)


def test_bundled_selfcheck_ok_no_warning(monkeypatch, recwarn):
    req = REQUIRED_SOFTWARE_VERSION
    _patch(monkeypatch, {"bundled": req})
    path, ver, fb = resolve_engine("bundled", "bundled")
    assert (path, ver, fb) == ("bundled", req, False)
    assert len(recwarn) == 0


def test_bundled_selfcheck_too_old_warns(monkeypatch):
    _patch(monkeypatch, {"bundled": "3.8.2"})
    with pytest.warns(RuntimeWarning, match="packaging"):
        path, ver, fb = resolve_engine("bundled", "bundled")
    assert (path, fb) == ("bundled", False)


def test_both_old_falls_back_and_double_warns(monkeypatch):
    _patch(monkeypatch, {"ext": "3.8.0", "bundled": "3.8.2"})
    with pytest.warns(RuntimeWarning):
        path, ver, fb = resolve_engine("ext", "bundled")
    assert (path, ver, fb) == ("bundled", "3.8.2", True)


def test_skip_env_bypasses_probe(monkeypatch):
    monkeypatch.setenv(_version._SKIP_ENV, "1")
    _patch(monkeypatch, {"ext": "1.0.0", "bundled": "3.8.7"})
    path, ver, fb = resolve_engine("ext", "bundled")
    assert (path, fb) == ("ext", False)


def test_required_version_is_semver():
    assert parse(REQUIRED_SOFTWARE_VERSION) is not None
