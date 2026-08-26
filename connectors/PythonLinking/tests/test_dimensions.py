"""Population cells over raw EUROMOD variables: value specs, overlap, region."""

import pandas as pd
import pytest

from euromod_linking import dimensions as dims


class TestParseValueSpec:
    def test_exact(self):
        s = dims.parse_value_spec("3")
        assert (s.kind, s.lo) == ("exact", 3.0)

    def test_range(self):
        s = dims.parse_value_spec("3-4")
        assert (s.kind, s.lo, s.hi) == ("range", 3.0, 4.0)

    def test_range_with_negative_bounds(self):
        s = dims.parse_value_spec("-1-0")
        assert (s.kind, s.lo, s.hi) == ("range", -1.0, 0.0)

    def test_open(self):
        s = dims.parse_value_spec("65+")
        assert (s.kind, s.lo) == ("open", 65.0)

    def test_numeric_set(self):
        s = dims.parse_value_spec("1,3,5")
        assert (s.kind, s.values) == ("set", (1.0, 3.0, 5.0))

    def test_string_value(self):
        s = dims.parse_value_spec("AT1")
        assert (s.kind, s.values) == ("string", ("AT1",))

    def test_reversed_range_rejected(self):
        with pytest.raises(dims.DimensionError, match="reversed"):
            dims.parse_value_spec("9-2")

    def test_empty_rejected(self):
        with pytest.raises(dims.DimensionError):
            dims.parse_value_spec("  ")


class TestMatches:
    @pytest.fixture()
    def series(self):
        return pd.Series([0, 1, 2, 3, 4, 5, 8])

    def test_exact(self, series):
        assert list(dims.matches(series, dims.parse_value_spec("3"))) == \
            [False, False, False, True, False, False, False]

    def test_range_inclusive(self, series):
        got = dims.matches(series, dims.parse_value_spec("3-4"))
        assert list(series[got]) == [3, 4]

    def test_open(self, series):
        got = dims.matches(series, dims.parse_value_spec("5+"))
        assert list(series[got]) == [5, 8]

    def test_set(self, series):
        got = dims.matches(series, dims.parse_value_spec("1,5"))
        assert list(series[got]) == [1, 5]

    def test_negative_range(self):
        s = pd.Series([-1, 0, 1])
        got = dims.matches(s, dims.parse_value_spec("-1-0"))
        assert list(s[got]) == [-1, 0]

    def test_string_column(self):
        s = pd.Series(["AT1", "AT2"])
        got = dims.matches(s, dims.parse_value_spec("AT2"))
        assert list(s[got]) == ["AT2"]

    def test_non_numeric_values_are_skipped(self):
        s = pd.Series(["3", "x", "4"])
        got = dims.matches(s, dims.parse_value_spec("3-4"))
        assert list(got) == [True, False, True]


class TestOverlap:
    def test_ranges_overlap(self):
        assert dims.specs_overlap(dims.parse_value_spec("3-4"), dims.parse_value_spec("4-5"))

    def test_ranges_disjoint(self):
        assert not dims.specs_overlap(dims.parse_value_spec("0-2"), dims.parse_value_spec("3-4"))

    def test_exact_inside_range(self):
        assert dims.specs_overlap(dims.parse_value_spec("3-4"), dims.parse_value_spec("4"))

    def test_open_overlaps_range(self):
        assert dims.specs_overlap(dims.parse_value_spec("5+"), dims.parse_value_spec("3-6"))
        assert not dims.specs_overlap(dims.parse_value_spec("5+"), dims.parse_value_spec("0-2"))

    def test_sets(self):
        assert dims.specs_overlap(dims.parse_value_spec("1,3"), dims.parse_value_spec("3,9"))
        assert not dims.specs_overlap(dims.parse_value_spec("1,3"), dims.parse_value_spec("4,9"))

    def test_strings(self):
        assert dims.specs_overlap(dims.parse_value_spec("AT1"), dims.parse_value_spec("AT1"))
        assert not dims.specs_overlap(dims.parse_value_spec("AT1"), dims.parse_value_spec("AT2"))


class TestRegion:
    def test_finest_column_and_float_codes(self):
        df = pd.DataFrame({"drgn2": [11.0, 12.0], "drgn1": [1, 1]})
        assert list(dims.derive_region(df)) == ["11", "12"]

    def test_fallback_to_drgn1(self):
        assert list(dims.derive_region(pd.DataFrame({"drgn1": [1, 2]}))) == ["1", "2"]

    def test_absent_gives_country_level(self):
        assert list(dims.derive_region(pd.DataFrame({"dag": [1]}))) == [""]

    def test_region_column_preference(self):
        assert dims.region_column(["drgn1", "drgn2"]) == "drgn2"
        assert dims.region_column(["drgn1"]) == "drgn1"
        assert dims.region_column(["dag"]) is None


class TestGroups:
    def test_parse_and_canonical(self):
        assert dims.parse_group("deh=3-4;region=11") == {"deh": "3-4", "region": "11"}
        assert dims.canonical_group("deh=3-4;region=11") == "deh=3-4;region=11"
        assert dims.canonical_group("region=11;deh=3-4") == "deh=3-4;region=11"
        assert dims.parse_group("") == {}

    def test_malformed(self):
        with pytest.raises(dims.DimensionError):
            dims.parse_group("deh")

    def test_repeated_key_rejected(self):
        with pytest.raises(dims.DimensionError, match="repeats"):
            dims.parse_group("deh=3;deh=4")


class TestValidation:
    def test_syntax_ok(self):
        assert dims.validate_group_syntax("deh=3-4;dgn=1") == []
        assert dims.validate_group_syntax("") == []

    def test_retired_dimension_names_explain_replacement(self):
        problems = dims.validate_group_syntax("skill=medium")
        assert problems and "deh=3-4" in problems[0]
        assert dims.validate_group_syntax("gender=F")
        assert dims.validate_group_syntax("age_band=25-34")

    def test_bad_spec_reported(self):
        assert dims.validate_group_syntax("deh=9-2")

    def test_invalid_key(self):
        assert dims.validate_group_syntax("2deh=3")

    def test_columns_checked_separately(self):
        cols = ["deh", "dgn", "dwt", "drgn1"]
        assert dims.validate_group_columns("deh=3-4;dgn=1", cols) == []
        assert dims.validate_group_columns("region=1", cols) == []
        problems = dims.validate_group_columns("dhe=1", cols)
        assert problems and "'dhe'" in problems[0]
        assert "EUROMOD input variables" in problems[0]

    def test_region_without_region_column(self):
        assert dims.validate_group_columns("region=1", ["dag", "dwt"])
