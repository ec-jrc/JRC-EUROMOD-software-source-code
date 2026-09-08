"""Canonical shock table: validation, canonicalization, deterministic ids, storage."""

import pytest

from euromod_linking import shock_table


def rec(**kw):
    base = {"channel": "align", "metric": "employment", "group": "deh=3-4",
            "period": "1", "op": "grow", "value": 0.02}
    base.update(kw)
    return base


class TestNormalize:
    def test_valid_records(self):
        df = shock_table.normalize([rec(), rec(metric="unemployment", value=-0.01)])
        assert len(df) == 2
        assert list(df.columns) == list(shock_table.COLUMNS)

    def test_group_canonicalized(self):
        df = shock_table.normalize([rec(group="deh=3-4;region=11"),
                                    rec(group="region=12;deh=3-4")])
        assert set(df["group"]) == {"deh=3-4;region=11", "deh=3-4;region=12"}

    def test_bad_channel_op_value(self):
        with pytest.raises(shock_table.ShockTableError) as e:
            shock_table.normalize([rec(channel="nope"), rec(op="nope"),
                                   rec(value=float("nan")), rec(value="x")])
        assert len(e.value.problems) == 4

    def test_group_syntax_validated(self):
        """normalize() checks group SYNTAX; whether a key is a real dataset
        column is a dataset-aware check done later by run_scenario."""
        with pytest.raises(shock_table.ShockTableError):
            shock_table.normalize([rec(group="planet")])          # no '='
        with pytest.raises(shock_table.ShockTableError):
            shock_table.normalize([rec(group="deh=9-2")])         # reversed range
        with pytest.raises(shock_table.ShockTableError):
            shock_table.normalize([rec(group="skill=medium")])    # a model's own recoding
        # An unknown-but-well-formed variable passes here by design.
        assert len(shock_table.normalize([rec(group="planet=3")])) == 1

    def test_constant_channel_skips_dimension_check(self):
        df = shock_table.normalize([rec(channel="constant", metric="$f_cpi",
                                        group="", period="2023", op="set", value=1.05)])
        assert len(df) == 1

    def test_duplicates_rejected(self):
        with pytest.raises(shock_table.ShockTableError):
            shock_table.normalize([rec(), rec(value=0.5)])

    def test_empty_rejected(self):
        with pytest.raises(shock_table.ShockTableError):
            shock_table.normalize([])


class TestContentId:
    def test_deterministic_and_order_independent(self):
        a = shock_table.normalize([rec(), rec(metric="unemployment")])
        b = shock_table.normalize([rec(metric="unemployment"), rec()])
        assert shock_table.content_id(a) == shock_table.content_id(b)
        assert shock_table.content_id(a).startswith("shk_")

    def test_source_excluded_from_id(self):
        a = shock_table.normalize([rec(source="file_a.xlsx#employment")])
        b = shock_table.normalize([rec(source="file_b.xlsx#employment")])
        assert shock_table.content_id(a) == shock_table.content_id(b)

    def test_value_changes_id(self):
        a = shock_table.normalize([rec(value=0.02)])
        b = shock_table.normalize([rec(value=0.03)])
        assert shock_table.content_id(a) != shock_table.content_id(b)


class TestDescribe:
    def test_carries_the_id_the_summary_and_the_origin(self):
        df = shock_table.normalize([rec()])
        d = shock_table.describe(df, origin="test")
        assert d["shock_table_id"] == shock_table.content_id(df)
        assert d["n_shocks"] == 1
        assert d["origin"] == "test"

    def test_nothing_is_written_to_disk(self, tmp_path, monkeypatch):
        """Normalising a table is a pure transform. A library that wrote files as
        a side effect of describing data would litter whatever directory it ran
        in — which is why the store this replaced was removed."""
        monkeypatch.chdir(tmp_path)
        df = shock_table.normalize([rec()])
        shock_table.describe(df, origin="test")
        shock_table.content_id(df)
        assert list(tmp_path.iterdir()) == []


class TestDescriptiveMetricNames:
    """A scale metric may name the economic concept rather than the model's code
    for it. Resolving that here, beside the group, is what keeps the content id a
    property of the scenario rather than of how somebody spelled it."""

    def scale(self, metric, **kw):
        return rec(channel="scale", metric=metric, op="grow", value=0.03, **kw)

    def test_concept_is_canonicalised_to_the_model_name(self):
        df = shock_table.normalize([self.scale("employment income")])
        assert df["metric"][0] == "ils_udb_yem"

    @pytest.mark.parametrize("spelling", [
        "employment income", "Employment income", "EMPLOYMENT INCOME",
        "employment_income", "Employment-Income", "  employment   income  ",
    ])
    def test_matching_ignores_case_separators_and_padding(self, spelling):
        df = shock_table.normalize([self.scale(spelling)])
        assert df["metric"][0] == "ils_udb_yem"

    def test_punctuation_in_the_model_label_is_not_load_bearing(self):
        """The model writes 'Private transfers (paid)'; nobody types the brackets."""
        for spelling in ("Private transfers (paid)", "private transfers paid",
                         "maintenance payments"):
            df = shock_table.normalize([self.scale(spelling)])
            assert df["metric"][0] == "ils_udb_xmp", spelling

    def test_both_spellings_give_the_same_content_id(self):
        a = shock_table.normalize([self.scale("employment income")])
        b = shock_table.normalize([self.scale("ils_udb_yem")])
        assert shock_table.content_id(a) == shock_table.content_id(b)

    def test_both_spellings_in_one_table_are_a_duplicate(self):
        """Otherwise the same shock applies twice, compounding silently."""
        with pytest.raises(shock_table.ShockTableError) as e:
            shock_table.normalize([self.scale("employment income"),
                                   self.scale("ils_udb_yem")])
        assert "duplicate" in e.value.problems[0]
        assert "ils_udb_yem" in e.value.problems[0]

    def test_unknown_concept_fails_here_naming_close_matches(self):
        """A misspelt concept must not survive to become 'not a column of the
        input dataset' after a model has been loaded."""
        with pytest.raises(shock_table.ShockTableError) as e:
            shock_table.normalize([self.scale("employmnt income")])
        problem = e.value.problems[0]
        assert "not a known income concept" in problem
        assert "ils_udb_yem" in problem

    def test_a_bare_variable_name_is_left_alone(self):
        """yem is the raw variable and stays it; only concept names reach the list."""
        assert shock_table.normalize([self.scale("yem")])["metric"][0] == "yem"

    def test_other_channels_are_untouched(self):
        df = shock_table.normalize([rec(metric="employment"),
                                    rec(channel="constant", metric="$f_cpi",
                                        group="", period="2023", op="set", value=1.02)])
        assert set(df["metric"]) == {"employment", "$f_cpi"}
