"""Method requirements vs the model: release detection, the comparator, and
the capability probe that decides.

The distinction under test throughout is three-valued. A requirement is met,
unmet, or *undeterminable*, and only the middle one may block a run — collapsing
the third into the second would refuse to run against a model that is merely
unreadable, which is worse than the late failure this check replaces.
"""

import pytest

from euromod_linking import compat, registry


# --- fakes: the connector object graph compat.py walks -----------------------

class FakeContainer:
    """Stands in for euromod.container.Container — compat only calls keys()/[]."""

    def __init__(self, mapping):
        self._d = dict(mapping)

    def keys(self):
        return self._d.keys()

    def __getitem__(self, key):
        return self._d[key]


class FakeAddon:
    def __init__(self, name, applicable):
        self.name = name
        self._applicable = dict(applicable)   # base system -> [addon system names]

    def get_applicable_systems(self, base_system):
        return FakeContainer({n: object() for n in self._applicable.get(base_system, [])})


class FakeModel:
    def __init__(self, model_path="", addons=None):
        self.model_path = model_path
        self.addons = FakeContainer(addons or {})


class FakeCountry:
    def __init__(self, name, model):
        self.name = name
        self.model = model


class FakeSystem:
    def __init__(self, name, parent):
        self.name = name
        self.parent = parent


def make_system(*, model_path="", addons=None, cc="BE", system_name="BE_2025"):
    return FakeSystem(system_name, FakeCountry(cc, FakeModel(model_path, addons)))


LMA = "lma_labour_alignment"
SCALE = "scale_variables"

#: Read the floor from the registry rather than repeating it: it is a claim about
#: EUROMOD releases and will move again, and a test suite that hardcodes it fails
#: for the wrong reason when it does.
FLOOR = registry.resolve(LMA).min_model_release
AT_FLOOR = rf"C:\Models\EUROMOD_MASTER_VERSION_{FLOOR}"
BELOW_FLOOR = r"C:\Models\EUROMOD_MASTER_VERSION_A0.1"
ABOVE_FLOOR = r"C:\Models\EUROMOD_MASTER_VERSION_Z9.99"


def lma_ready(model_path=AT_FLOOR):
    """A system whose model ships everything lma_labour_alignment asks for."""
    return make_system(model_path=model_path,
                       addons={"LMA": FakeAddon("LMA", {"BE_2025": ["LMA_BE"]})})


@pytest.fixture()
def extensions(monkeypatch):
    """Control what query.system_extension_names reports, without a model."""

    def setter(names):
        monkeypatch.setattr(compat.query, "system_extension_names",
                            lambda cc, system_name=None: set(names))

    setter({"LMA_trans", "BTA", "TCA"})
    return setter


# --- the comparator ----------------------------------------------------------

class TestParseRelease:
    def test_splits_into_sortable_parts(self):
        assert compat.parse_release("J2.53") == ("J", 2, 53, False)
        assert compat.parse_release("I6.0+") == ("I", 6, 0, True)

    def test_found_inside_a_folder_name(self):
        assert compat.parse_release("EUROMOD_MASTER_VERSION_J2.19") == ("J", 2, 19, False)

    def test_case_insensitive(self):
        """Model folders are named by hand and lowercased ones occur, so the
        same release must not read as undetectable depending on its spelling."""
        assert compat.parse_release("euromod_master_version_j2.54") == ("J", 2, 54, False)
        assert compat.release_geq("euromod_master_version_j2.54", "J2.53") is True

    @pytest.mark.parametrize("value", ["", None, "nonsense", "3.8.8", "2.19"])
    def test_unparseable_is_none(self, value):
        """None means 'cannot compare'. It must never be read as 'old'."""
        assert compat.parse_release(value) is None


class TestReleaseGeq:
    def test_minor_then_major_then_letter(self):
        assert compat.release_geq("J2.53", "J2.19") is True
        assert compat.release_geq("J2.19", "J2.53") is False
        assert compat.release_geq("J2.7", "J2.19") is False     # numeric, not lexical
        assert compat.release_geq("J3.0", "J2.53") is True
        assert compat.release_geq("I6.0+", "J1.0") is False     # letter dominates
        assert compat.release_geq("J1.0", "I6.0+") is True

    def test_equal_release_meets_its_own_floor(self):
        assert compat.release_geq("J2.53", "J2.53") is True

    def test_plus_sorts_just_above_the_bare_version(self):
        """J1.86+ is a rolling build of J1.86 — not older than it."""
        assert compat.release_geq("J1.86+", "J1.86") is True
        assert compat.release_geq("J1.86", "J1.86+") is False

    def test_fails_open_when_either_side_is_unparseable(self):
        assert compat.release_geq("mymodel", "J2.53") is None
        assert compat.release_geq("J2.53", None) is None


# --- release detection -------------------------------------------------------

class TestCanonicalRelease:
    def test_normalises_case_and_strips_surroundings(self):
        assert compat.canonical_release("euromod_master_version_j2.54") == "J2.54"
        assert compat.canonical_release("EUROMOD_RELEASES_I6.0+") == "I6.0+"

    def test_unparseable_comes_back_stripped(self):
        assert compat.canonical_release("  3.2.1  ") == "3.2.1"
        assert compat.canonical_release(None) is None


class TestModelRelease:
    def test_reads_the_folder_name(self, tmp_path):
        root = tmp_path / "EUROMOD_MASTER_VERSION_J2.19"
        root.mkdir()
        assert compat.model_release(str(root)) == ("J2.19", "folder-name")

    def test_lowercase_folder_reports_the_canonical_release(self, tmp_path):
        root = tmp_path / "euromod_master_version_j2.54"
        root.mkdir()
        assert compat.model_release(str(root)) == ("J2.54", "folder-name")

    def test_reads_the_parent_when_nested(self, tmp_path):
        """EUROMOD_RELEASES_J0.1+\\EUROMOD_RELEASES_J0.1+ layouts occur in the wild."""
        root = tmp_path / "EUROMOD_RELEASES_J1.0+" / "model"
        root.mkdir(parents=True)
        assert compat.model_release(str(root)) == ("J1.0+", "folder-name")

    def test_version_file_wins_over_the_folder_name(self, tmp_path):
        """The folder name is a heuristic; the version file is the documented
        contract, so a renamed folder must not override it."""
        root = tmp_path / "EUROMOD_MASTER_VERSION_J2.19"
        (root / "XMLParam" / "Config").mkdir(parents=True)
        (root / "XMLParam" / "Config" / "EuromodVersion.txt").write_text(
            "J2.53\nPUBLIC VERSION\n", encoding="utf-8")
        assert compat.model_release(str(root)) == ("J2.53", "version-file")

    def test_licence_file_when_the_folder_name_says_nothing(self, tmp_path):
        root = tmp_path / "model"
        root.mkdir()
        (root / "EUROMOD_model_licence_J1.86+_beta.txt").write_text("x", encoding="utf-8")
        assert compat.model_release(str(root)) == ("J1.86+", "licence-file")

    def test_renamed_folder_is_undetectable_not_old(self, tmp_path):
        """A user who renames the model erases the release. That is 'unknown',
        and the caller must not turn it into a refusal."""
        root = tmp_path / "my model copy"
        root.mkdir()
        assert compat.model_release(str(root)) == (None, None)

    def test_empty_path(self):
        assert compat.model_release("") == (None, None)

    def test_em_log_is_opt_in(self, tmp_path, monkeypatch):
        """The change log is tens of megabytes, so it is never read by default."""
        root = tmp_path / "model"
        (root / "Log").mkdir(parents=True)
        calls = []
        monkeypatch.setattr(compat, "_release_from_em_log",
                            lambda r: calls.append(r) or "J9.99")
        assert compat.model_release(str(root)) == (None, None)
        assert calls == []
        assert compat.model_release(str(root), deep=True) == ("J9.99", "em-log")


# --- the capability probe ----------------------------------------------------

class TestCheckCompatibility:
    def test_everything_present_is_ok(self, extensions):
        report = compat.check_compatibility(lma_ready(), LMA)
        assert report.ok and not report.problems
        assert {r.name for r in report.requirements} == {"LMA", "LMA_trans"}
        assert all(r.satisfied is True for r in report.requirements)

    def test_method_without_requirements_is_always_ok(self):
        report = compat.check_compatibility(make_system(), SCALE)
        assert report.ok and not report.requirements and not report.notes

    def test_missing_addon_is_a_problem_naming_what_is_there(self, extensions):
        system = make_system(model_path=r"C:\Models\EUROMOD_MASTER_VERSION_J2.19",
                             addons={"MTR": FakeAddon("MTR", {})})
        report = compat.check_compatibility(system, LMA)
        assert not report.ok
        assert any("LMA add-on" in p and "MTR" in p for p in report.problems)

    def test_addon_present_but_not_applicable_to_this_system(self, extensions):
        """The add-on ships, but no LMA_BE system matches BE_2025 — the
        {cc}-resolved system name is the thing being checked here."""
        system = make_system(model_path="", addons={"LMA": FakeAddon("LMA", {"AT_2024": ["LMA_AT"]})})
        report = compat.check_compatibility(system, LMA)
        assert not report.ok
        assert any("LMA_BE" in p for p in report.problems)

    def test_missing_extension_is_a_problem(self, extensions):
        extensions({"BTA", "TCA"})
        report = compat.check_compatibility(lma_ready(), LMA)
        assert not report.ok
        assert any("LMA_trans" in p and "silently ignored" in p for p in report.problems)

    def test_unreadable_extensions_are_a_note_not_a_problem(self, extensions):
        """An empty set means the model could not be read. 'Cannot validate' is
        not 'invalid' — this must stay ok."""
        extensions(set())
        report = compat.check_compatibility(lma_ready(), LMA)
        assert report.ok
        assert any("Could not verify" in n and "LMA_trans" in n for n in report.notes)

    def test_unlistable_addons_are_a_note_not_a_problem(self, extensions):
        system = make_system()
        system.parent.model = None
        report = compat.check_compatibility(system, LMA)
        assert report.ok
        assert any("Could not verify" in n for n in report.notes)

    def test_accepts_a_spec_object_as_well_as_a_ref(self, extensions):
        spec = registry.resolve(LMA)
        assert compat.check_compatibility(lma_ready(), spec).method == LMA

    def test_reports_the_country_and_system_it_checked(self, extensions):
        report = compat.check_compatibility(lma_ready(), LMA)
        assert (report.country_code, report.system_name) == ("BE", "BE_2025")


class TestReleaseFloor:
    def test_below_the_floor_is_a_note_not_a_problem(self, extensions):
        """Capability decides; the release only explains. A model that somehow
        has LMA_trans while looking old still runs."""
        report = compat.check_compatibility(lma_ready(BELOW_FLOOR), LMA)
        assert report.ok
        assert report.model_release == "A0.1"
        assert any(FLOOR in n and "A0.1" in n for n in report.notes)

    def test_at_or_above_the_floor_adds_no_note(self, extensions):
        assert compat.check_compatibility(lma_ready(AT_FLOOR), LMA).notes == ()
        assert compat.check_compatibility(lma_ready(ABOVE_FLOOR), LMA).notes == ()

    def test_undetectable_release_says_so(self, extensions):
        report = compat.check_compatibility(lma_ready(r"C:\Models\my copy"), LMA)
        assert report.ok
        assert any("could not be determined" in n for n in report.notes)

    def test_old_release_explains_a_capability_failure(self, extensions):
        """The whole point of tracking the floor: the error says 'upgrade',
        not just 'missing'."""
        extensions(set())
        system = make_system(model_path=BELOW_FLOOR,
                             addons={"MTR": FakeAddon("MTR", {})})
        report = compat.check_compatibility(system, LMA)
        assert not report.ok
        assert any(FLOOR in n and "A0.1" in n for n in report.notes)

    def test_new_release_rules_out_upgrading_without_overclaiming(self, extensions):
        """A model above the floor tells you upgrading will not help. It does not
        tell you *why* the requirement is unmet — an add-on can be installed and
        still not cover the system in hand — so the note must not guess."""
        extensions({"BTA"})
        report = compat.check_compatibility(lma_ready(ABOVE_FLOOR), LMA)
        assert not report.ok
        assert any("the release is not what is missing" in n for n in report.notes)


class TestCheckAllAndMatrix:
    def test_check_all_covers_every_registered_methodology(self, extensions):
        reports = compat.check_all(lma_ready())
        assert {r.method for r in reports} == {s.name for s in registry.list_specs()}

    def test_matrix_has_a_row_per_system_and_methodology(self, extensions):
        model = FakeModel(AT_FLOOR,
                          {"LMA": FakeAddon("LMA", {"BE_2025": ["LMA_BE"]})})
        country = FakeCountry("BE", model)
        country.systems = [FakeSystem("BE_2025", country), FakeSystem("BE_2024", country)]
        model.countries = FakeContainer({"BE": country})
        model.__class__.__getitem__ = lambda self, cc: country

        df = compat.compatibility_matrix(model, "BE")
        assert len(df) == 2 * len(registry.list_specs())
        assert set(df["system"]) == {"BE_2025", "BE_2024"}
        assert df.loc[df["methodology"] == SCALE, "ok"].all()


class TestReportRendering:
    def test_str_lists_problems_and_notes(self, extensions):
        extensions(set())
        report = compat.check_compatibility(
            make_system(model_path=r"C:\Models\EUROMOD_MASTER_VERSION_J2.19"), LMA)
        text = str(report)
        assert "NOT ok" in text and "problem:" in text and "note:" in text


# --- the scenarios.py wiring -------------------------------------------------

class TestScenarioWiring:
    def test_skip_env_var_disables_the_check(self, monkeypatch, extensions):
        from euromod_linking import scenarios

        monkeypatch.setenv("EUROMOD_SKIP_COMPAT_CHECK", "1")
        assert scenarios._compatibility(make_system(), registry.resolve(LMA)) is None

    def test_an_uninspectable_system_blocks_nothing(self):
        """A caller who never loaded a model must still be able to validate a
        scenario. The check reports that it could verify nothing, and adds no
        problems — it must not become a new way for a working scenario to fail."""
        from euromod_linking import scenarios

        report = scenarios._compatibility(object(), registry.resolve(LMA))
        assert report.ok and not report.problems
        assert report.notes
        assert all(r.satisfied is None for r in report.requirements)

    def test_check_scenario_always_carries_the_key(self):
        from euromod_linking import scenarios

        result = scenarios.check_scenario({
            "country_code": "BE", "system_name": "BE_2025",
            "shocks": {"inline": [{"channel": "constant", "metric": "$f_cpi",
                                   "period": "", "op": "set", "value": 1.02}]},
        })
        assert result["compatibility"] is None


# --- against a real model ----------------------------------------------------

@pytest.mark.live
class TestAgainstARealModel:
    """Needs EUROMOD_MODEL_PATH. Deselected by default (addopts = -m 'not live');
    run with `pytest -m live`."""

    @pytest.fixture(scope="class")
    def system(self):
        import os

        path = os.environ.get("EUROMOD_MODEL_PATH", "").strip()
        if not path:
            pytest.skip("EUROMOD_MODEL_PATH not set")
        from euromod_linking.session import get_model

        return get_model(path)["BE"]["BE_2025"]

    def test_every_methodology_yields_a_coherent_report(self, system):
        """Not 'every methodology passes' — whether LMA is available depends on
        the model's release. What must hold is that a failure names the thing
        that is missing, so the message is actionable."""
        for report in compat.check_all(system):
            assert report.country_code == "BE"
            assert report.system_name == "BE_2025"
            spec = registry.resolve(report.method)
            required = {n for entry in (spec.addon_requirements or ((), ()))[0]
                        for n in ([entry] if isinstance(entry, str) else entry)}
            required |= {e[0] if isinstance(e, (list, tuple)) else e
                         for e in (spec.addon_requirements or ((), ()))[1]}
            for problem in report.problems:
                assert any(name.format(cc="BE") in problem for name in required), problem

    def test_scale_variables_is_supported_by_any_model(self, system):
        """It asks nothing of the model, so a failure here means the probe
        itself is broken rather than the model being unsuitable."""
        assert compat.check_compatibility(system, SCALE).ok

    def test_release_is_detected_for_a_conventionally_named_model(self, system):
        report = compat.check_compatibility(system, LMA)
        if report.model_release is None:
            pytest.skip("model folder carries no release marker")
        assert compat.parse_release(report.model_release) is not None
