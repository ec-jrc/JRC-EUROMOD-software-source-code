"""Tests for euromod.introspect.

The extension include/exclude rule and the income-list walk are pure functions
over plain data, so they are tested against hand-built structures without a
model or the CLR. The one test that needs a real model is marked ``live``.
"""
import os

import pytest

from euromod import introspect
from euromod.introspect import IncomeListLookupError, _filter_include, _active


class TestFilterInclude:
    """Mirrors container.filter: an element carrying extension links is included
    when some link is on-for-inclusion, or when none is on-for-removal."""

    def test_no_links_defers_to_the_elements_own_switch(self):
        assert _filter_include((), {"EXT": True}) is None
        assert _active("on", (), {}) is True
        assert _active("off", (), {}) is False

    def test_included_when_its_extension_is_on(self):
        assert _filter_include((("EXT", False),), {"EXT": True}) is True

    def test_excluded_when_its_extension_is_off(self):
        """Marked for inclusion when on, but it is off."""
        assert _filter_include((("EXT", False),), {"EXT": False}) is False

    def test_base_off_removes_when_its_extension_is_on(self):
        assert _filter_include((("EXT", True),), {"EXT": True}) is False

    def test_base_off_keeps_when_its_extension_is_off(self):
        assert _filter_include((("EXT", True),), {"EXT": False}) is True

    def test_explicit_inclusion_beats_removal(self):
        links = (("A", False), ("B", True))
        assert _filter_include(links, {"A": True, "B": True}) is True

    def test_unknown_extension_counts_as_off(self):
        assert _filter_include((("EXT", False),), {}) is False


def _raw(il_occ, policies=None):
    """Minimal extracted-system shape for the income-list resolver."""
    return {"name": "XX_2024", "il_occ": il_occ,
            "policies": policies or [{"name": "P", "switch": "on", "ext": ()}]}


def _occ(components, policy="P", switch="on", ext=(), key=(1, 1)):
    return {"key": key, "fun_ext": ext, "fun_switch": switch,
            "policy": policy, "components": components}


class TestResolveIncomeList:
    def test_returns_components_with_signs_in_definition_order(self):
        raw = _raw({"ils_x": [_occ([("yem", "+", ()), ("tin", "-", ())])]})
        assert introspect._resolve_income_list(raw, {}, "ils_x") == [("yem", "+"), ("tin", "-")]

    def test_nested_lists_expand_with_signs_multiplied(self):
        raw = _raw({
            "ils_inner": [_occ([("a", "+", ()), ("b", "-", ())])],
            "ils_outer": [_occ([("ils_inner", "-", ()), ("c", "+", ())])],
        })
        # -(a - b) + c  ->  a negative, b positive, c positive
        assert introspect._resolve_income_list(raw, {}, "ils_outer") == [
            ("a", "-"), ("b", "+"), ("c", "+")]

    def test_component_excluded_by_its_own_extension_link(self):
        raw = _raw({"ils_x": [_occ([("yem", "+", ()), ("bonus", "+", (("EXT", False),))])]})
        assert introspect._resolve_income_list(raw, {"EXT": False}, "ils_x") == [("yem", "+")]
        assert introspect._resolve_income_list(raw, {"EXT": True}, "ils_x") == [
            ("yem", "+"), ("bonus", "+")]

    def test_last_active_definition_wins_in_spine_order(self):
        raw = _raw({"ils_x": [_occ([("early", "+", ())], key=(1, 1)),
                              _occ([("late", "+", ())], key=(2, 1))]})
        assert introspect._resolve_income_list(raw, {}, "ils_x") == [("late", "+")]

    def test_an_inactive_redefinition_does_not_win(self):
        raw = _raw({"ils_x": [_occ([("kept", "+", ())], key=(1, 1)),
                              _occ([("gated", "+", ())], key=(2, 1), ext=(("EXT", False),))]})
        assert introspect._resolve_income_list(raw, {"EXT": False}, "ils_x") == [("kept", "+")]

    def test_duplicate_components_are_deduped(self):
        raw = _raw({"ils_x": [_occ([("yem", "+", ()), ("yem", "-", ())])]})
        assert introspect._resolve_income_list(raw, {}, "ils_x") == [("yem", "+")]

    def test_unknown_list_reports_what_exists(self):
        raw = _raw({"ils_x": [_occ([("a", "+", ())])]})
        with pytest.raises(IncomeListLookupError) as e:
            introspect._resolve_income_list(raw, {}, "ils_nope")
        assert e.value.available == ["ils_x"]

    def test_inactive_list_is_distinguished_from_a_missing_one(self):
        raw = _raw({"ils_x": [_occ([("a", "+", ())], ext=(("EXT", False),))]})
        with pytest.raises(IncomeListLookupError, match="not active"):
            introspect._resolve_income_list(raw, {"EXT": False}, "ils_x")

    def test_cycles_raise_rather_than_recurse_forever(self):
        raw = _raw({"ils_a": [_occ([("ils_b", "+", ())])],
                    "ils_b": [_occ([("ils_a", "+", ())])]})
        with pytest.raises(IncomeListLookupError, match="Cyclic"):
            introspect._resolve_income_list(raw, {}, "ils_a")

    def test_component_of_an_inactive_policy_is_not_used(self):
        raw = _raw({"ils_x": [_occ([("a", "+", ())], policy="OFF")]},
                   policies=[{"name": "OFF", "switch": "off", "ext": ()}])
        with pytest.raises(IncomeListLookupError, match="not active"):
            introspect._resolve_income_list(raw, {}, "ils_x")


class TestServiceParameters:
    def test_defil_service_parameters_are_not_components(self):
        for name in ("Name", "TAX_UNIT", "Run_Cond", "Output_Var", "Warn_If_NonPositive"):
            assert introspect._il_component_name(name) is False

    def test_comment_rows_are_not_components(self):
        assert introspect._il_component_name("#comment") is False

    def test_variable_names_are_components(self):
        for name in ("yem", "ils_origy", "_x1"):
            assert introspect._il_component_name(name) is True


@pytest.mark.live
class TestAgainstARealModel:
    """Needs EUROMOD_MODEL_PATH. The switch-name check is the one that matters:
    an unknown extension is silently dropped by the engine, so a run can look
    successful while doing nothing."""

    @pytest.fixture(scope="class")
    @classmethod
    def system(cls):
        path = os.environ.get("EUROMOD_MODEL_PATH", "").strip()
        if not path:
            pytest.skip("EUROMOD_MODEL_PATH not set")
        from euromod import Model
        country = Model(path).countries["BE"]
        return country.systems["BE_2025"]

    def test_extension_names_include_the_addon_switches(self, system):
        names = introspect.system_extension_names(system)
        assert names, "no extension names read from the model"
        assert "LMA_trans" in names, sorted(names)

    def test_constant_params_are_a_superset_of_defconst_names(self, system):
        """The point of system_constant_params: it walks every function, not
        just DefConst, because constantsToOverwrite is keyed by (name, group)
        and uprating factors are group-keyed parameters of other functions.
        Asserted structurally -- constant names are country-specific."""
        params = introspect.system_constant_params(system)
        assert params
        assert introspect.system_constant_names(system) <= set(params)
        assert [n for n, groups in params.items() if groups - {""}], \
            "expected at least one group-keyed parameter"

    def test_income_list_resolves_to_variables(self, system):
        raw = introspect._system_raw(system)
        name = next(n for n in sorted(raw["il_occ"]) if n.startswith("ils_udb_"))
        comps = introspect.income_list_components(system, name)
        assert comps and all(s in ("+", "-") for _, s in comps), (name, comps)

    def test_unknown_income_list_lists_what_exists(self, system):
        with pytest.raises(IncomeListLookupError) as e:
            introspect.income_list_components(system, "ils_not_a_real_list")
        assert e.value.available, "error should name the lists that do exist"

    def test_run_rejects_an_unknown_switch(self, system):
        import pandas as pd
        with pytest.raises(ValueError, match="Unknown extension switch"):
            system.run(pd.DataFrame({"idhh": [1]}), "whatever",
                       switches=[("NOT_A_REAL_EXTENSION", True)])
