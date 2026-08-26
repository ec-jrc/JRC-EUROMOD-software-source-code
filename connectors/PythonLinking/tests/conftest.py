"""Shared fixtures: synthetic microdata and a mock external-model workbook."""

import numpy as np
import pandas as pd
import pytest


@pytest.fixture(autouse=True)
def _isolated_stores(tmp_path, monkeypatch):
    """Keep the score cache out of the repo during tests.

    The score cache is the only thing the package persists; shock tables are
    content-identified and never written."""
    monkeypatch.setenv("EUROMOD_SCORE_CACHE_DIR", str(tmp_path / "score_cache"))
    from euromod_linking.methods.lma_labour_alignment import scoring
    scoring.clear_cache()  # no score reuse across tests (each gets a fresh tmp dir)
    yield


@pytest.fixture(scope="session")
def be_system_raw():
    """Extracted spine of a real system, for tests that must not drift from the
    model. Skipped unless EUROMOD_MODEL_PATH points at an installed model."""
    import os

    path = os.environ.get("EUROMOD_MODEL_PATH", "").strip()
    if not path:
        pytest.skip("EUROMOD_MODEL_PATH not set")
    from euromod_linking import query
    from euromod_linking.session import get_model

    get_model(path)
    raw = query._get_raw("BE")
    return next(s for s in raw["systems"] if s["name"] == "BE_2025")


@pytest.fixture()
def synthetic_microdata() -> pd.DataFrame:
    """~40 households / ~110 persons with intact household structure.

    Engineered so alignment cells have plenty of employed/unemployed/inactive
    candidates in every region x skill x gender x age_band combination that the
    tests shock. Weights vary by household so fractional targets force
    boundary splits.
    """
    rng = np.random.default_rng(42)
    rows = []
    idperson = 0
    for hh in range(1, 41):
        size = int(rng.integers(1, 5))
        dwt = float(rng.integers(10, 101))
        drgn1 = int(rng.integers(1, 3))  # regions "1" and "2"
        adults = []
        for m in range(size):
            idperson += 1
            is_child = m >= 2 and rng.random() < 0.5
            if is_child:
                dag = int(rng.integers(2, 16))
                les2 = 0
                deh = 0
                yem = 0.0
            else:
                dag = int(rng.integers(20, 63))
                les2 = int(rng.choice([1, 3, 5, 10, 7], p=[0.40, 0.15, 0.15, 0.20, 0.10]))
                deh = int(rng.integers(1, 7))
                yem = float(rng.integers(1500, 4000)) if les2 in (1, 2, 3, 4) else 0.0
                adults.append(idperson)
            rows.append({
                "idhh": hh, "idperson": idperson,
                "idfather": 0, "idmother": 0, "idpartner": 0,
                "dag": dag, "dgn": int(rng.integers(0, 2)), "dms": int(rng.integers(1, 4)),
                "deh": deh, "dwt": dwt, "les2": les2,
                "yem": yem, "yemmy": 12.0 if yem > 0 else 0.0,
                "lhw": float(rng.integers(35, 41)) if yem > 0 else 0.0,
                "yds": 0.0, "drgn1": drgn1, "yivwg": 12.0,
            })
        if len(adults) >= 2:  # link the first two adults as partners
            a, b = adults[0], adults[1]
            for r in rows:
                if r["idperson"] == a:
                    r["idpartner"] = b
                if r["idperson"] == b:
                    r["idpartner"] = a
    return pd.DataFrame(rows)


@pytest.fixture()
def projections_excel(tmp_path):
    """Tiny workbook in the shipped spec's layout: sheets employment/unemployment,
    positional columns (status, NUTS-2 region code, education class), numeric
    headers 1..10 holding decimal growth rates. Includes another country's rows,
    which ingest filters out."""
    path = tmp_path / "projections.xlsx"
    regions = ["AT11", "AT12", "AT21", "DE11"]
    classes = ["low", "medium", "high"]

    def sheet(status: str, base: float) -> pd.DataFrame:
        rows = []
        for i, region in enumerate(regions):
            for j, education in enumerate(classes):
                row = {"c0": status, "c1": region, "c2": education}
                for p in range(1, 11):
                    row[p] = round(base + 0.01 * p + 0.001 * (i + j), 4)
                rows.append(row)
        return pd.DataFrame(rows)

    with pd.ExcelWriter(path, engine="openpyxl") as xl:
        sheet("employment", 0.02).to_excel(xl, sheet_name="employment", index=False)
        sheet("unemployment", -0.01).to_excel(xl, sheet_name="unemployment", index=False)
    return path
