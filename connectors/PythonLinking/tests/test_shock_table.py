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
