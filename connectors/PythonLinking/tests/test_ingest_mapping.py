"""Ingest through a shipped mapping spec (mock workbook fixture)."""

import pytest

from euromod_linking import ingest, shock_table

SPEC = "regional_projections"


class TestIngest:
    def test_full_ingest(self, projections_excel):
        records, warnings = ingest.ingest(str(projections_excel), SPEC, country="AT")
        df = shock_table.normalize(records)
        # 3 AT regions x 3 education bands x 10 periods x 2 sheets
        assert len(df) == 180
        assert set(df["channel"]) == {"align"}
        assert set(df["metric"]) == {"employment", "unemployment"}
        assert set(df["op"]) == {"grow"}
        assert set(df["period"]) == {str(p) for p in range(1, 11)}
        # NUTS prefix stripped, canonical group ordering, class translated at ingest
        assert "deh=0-2;region=11" in set(df["group"])
        # DE rows filtered with a warning
        assert any("other countries" in w for w in warnings)
        assert not any("DE" in g for g in df["group"])

    def test_specific_value(self, projections_excel):
        records, _ = ingest.ingest(str(projections_excel), SPEC, country="AT")
        df = shock_table.normalize(records)
        # employment, AT11 (i=0), 'low' (j=0), period 3: 0.02 + 0.01*3 + 0 = 0.05
        row = df[(df["metric"] == "employment") & (df["group"] == "deh=0-2;region=11")
                 & (df["period"] == "3")]
        assert len(row) == 1
        assert row["value"].iloc[0] == pytest.approx(0.05)

    def test_country_with_no_rows(self, projections_excel):
        with pytest.raises(ingest.IngestError, match="No shock records"):
            ingest.ingest(str(projections_excel), SPEC, country="FR")

    def test_missing_file(self):
        with pytest.raises(ingest.IngestError, match="File not found"):
            ingest.ingest("nope.xlsx", SPEC)

    def test_unknown_mapping(self, projections_excel):
        with pytest.raises(ingest.IngestError, match="Unknown mapping"):
            ingest.ingest(str(projections_excel), "does_not_exist")

    def test_missing_sheet(self, tmp_path, projections_excel):
        import pandas as pd
        bad = tmp_path / "bad.xlsx"
        pd.DataFrame({"a": [1]}).to_excel(bad, sheet_name="wrong", index=False)
        with pytest.raises(ingest.IngestError, match="not readable"):
            ingest.ingest(str(bad), SPEC, country="AT")

    def test_disallowed_class_value(self, tmp_path):
        """A class the spec does not declare is rejected, not passed through: an
        unrecognised label would otherwise become a population cell nobody meant."""
        import pandas as pd
        bad = tmp_path / "badclass.xlsx"
        df = pd.DataFrame([{"c0": "employment", "c1": "AT11", "c2": "unknown_band", 1: 0.02}])
        with pd.ExcelWriter(bad, engine="openpyxl") as xl:
            df.to_excel(xl, sheet_name="employment", index=False)
            df.to_excel(xl, sheet_name="unemployment", index=False)
        with pytest.raises(ingest.IngestError, match="not in allowed"):
            ingest.ingest(str(bad), SPEC, country="AT")


class TestMappings:
    def test_list_mappings(self):
        names = [m["name"] for m in ingest.list_mappings()]
        assert SPEC in names

    def test_mapping_schema_enforced(self):
        with pytest.raises(ingest.IngestError, match="invalid"):
            ingest.load_mapping({"mapping_version": 1, "name": "x"})
