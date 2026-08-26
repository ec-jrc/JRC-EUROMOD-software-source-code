"""Nearest-neighbour earnings imputation — the FALLBACK for entrants' wages.

The method's standard assumption is full-time take-up at the person's own
predicted hourly wage (`yivwg`, estimated by the national team for every
working-age person). This module covers the remainder: entrants for whom no
predicted wage exists, because the national wage equation excludes them
(students, the self-employed, pensioners in some countries) or the value is
missing.

Economic intuition
------------------
Someone who enters employment needs a wage before EUROMOD can tax it — but new
entrants have no observed earnings. Statistical matching answers "what would
this person plausibly earn?" with "what observably similar people currently
earn": each entrant is matched to the k=5 most similar continuing workers on
the classic wage-equation observables (age, gender, education, marital status,
region), and their earnings/months/hours are averaged with inverse-distance
weights (closer matches count more). This preserves realistic wage
heterogeneity across entrants — a flat assumption, everyone at the minimum wage
say, would compress the bottom of the earnings distribution and mechanically
overstate redistribution effects.

Donors exclude other entrants (lma==0 filter): imputing from imputed values
would let assumptions compound.

Fixed methodology (matching variables, k=5, inverse-distance weighting).
Deterministic: donors and recipients processed in idperson order; scaler and
KNN are deterministic given pinned input order.
"""

import logging

import numpy as np
import pandas as pd

logger = logging.getLogger(__name__)

N_NEIGHBORS = 5
INCOME_VARS = ("yem", "yemmy", "lhw")
# Preferred matching variables; the subset present in the data is used.
MATCHING_VARS = ("dag", "dgn", "deh", "dms")


def _matching_vars(df: pd.DataFrame, region_label_col: str | None) -> list[str]:
    out = [v for v in MATCHING_VARS if v in df.columns]
    if region_label_col and region_label_col in df.columns:
        out.append(region_label_col)
    return out


def _encode(df: pd.DataFrame, cols: list[str], encodings: dict | None = None):
    """Label-encode non-numeric matching columns (sorted category order)."""
    x = df[cols].copy()
    fit = encodings is None
    encodings = encodings if encodings is not None else {}
    for c in cols:
        if x[c].dtype == object or str(x[c].dtype) == "category":
            # KNN needs numbers. Categories are mapped to integers in sorted
            # order so the encoding is stable across runs; recipients reuse the
            # donor-fitted encoding (fit=False) so both sides live in the same
            # space, with unseen categories pushed far away (-1).
            if fit:
                encodings[c] = {v: i for i, v in enumerate(sorted(x[c].dropna().astype(str).unique()))}
            x[c] = x[c].astype(str).map(encodings[c]).fillna(-1)
        x[c] = pd.to_numeric(x[c], errors="coerce").fillna(0).astype(float)
    return x, encodings


def impute_transition_incomes(df: pd.DataFrame, eligible: pd.Series,
                              region_label_col: str | None,
                              recipients: pd.Index | None = None) -> tuple[pd.DataFrame, dict]:
    """Overwrite yem_a/yemmy_a/lhw_a for `recipients` with donor-matched values.

    `recipients` defaults to every entrant (lma==1). The method normally passes
    only the entrants with no usable predicted wage, since the standard
    assumption is full-time take-up at the person's own yivwg — this matching is
    the fallback for people the wage equation does not cover.
    Returns (df, diagnostics)."""
    from sklearn.neighbors import NearestNeighbors
    from sklearn.preprocessing import StandardScaler

    if recipients is None:
        recipients = df.index[(df["lma"] == 1)]
    diag = {"n_recipients": int(len(recipients))}
    if len(recipients) == 0:
        return df, diag

    # Donor pool: continuing employed workers with actually observed positive
    # earnings, of working age. lma==0 excludes fellow entrants — their
    # incomes are themselves imputed, and imputing from imputations would
    # compound assumptions. Sorted by idperson so KNN tie-breaking (which
    # follows input order) is deterministic.
    donors_mask = (df["new_employed"] == 1) & (df["yem"] > 0) & eligible & (df["lma"] == 0)
    donors = df.loc[donors_mask].sort_values("idperson", kind="mergesort")
    diag["n_donors"] = int(len(donors))
    if len(donors) == 0:
        diag["warning"] = "No donors with positive earnings; flat-rate fallback kept"
        logger.warning("LMA imputation: no donors; keeping flat-rate fallback")
        return df, diag

    cols = _matching_vars(df, region_label_col)
    diag["matching_vars"] = cols
    donors = donors.dropna(subset=[c for c in cols if c in donors.columns])

    # Standardize features so similarity is scale-free: without it, age
    # (range ~50) would dominate a 0/1 gender indicator in the euclidean
    # distance and the "nearest" donor would just be the nearest in age.
    x_donors, encodings = _encode(donors, cols)
    scaler = StandardScaler()
    xd = scaler.fit_transform(x_donors.values)

    # k=5 neighbours balances noise (k=1 would copy a single, possibly odd,
    # donor wage) against bias (large k pulls everyone toward the cell mean).
    k = min(N_NEIGHBORS, len(donors))
    nn = NearestNeighbors(n_neighbors=k, metric="euclidean")
    nn.fit(xd)

    # Recipients are projected into the SAME feature space (donor-fitted
    # scaler + encodings) before the neighbour search.
    rec = df.loc[recipients].sort_values("idperson", kind="mergesort")
    x_rec, _ = _encode(rec, cols, encodings)
    distances, indices = nn.kneighbors(scaler.transform(x_rec.values))

    # Inverse-distance weights: the most similar donors dominate the average;
    # the epsilon guards against division by zero on an exact clone (who then
    # effectively contributes its wage one-to-one).
    w = 1.0 / (distances + 1e-6)
    w = w / w.sum(axis=1, keepdims=True)

    # The three quantities the LMA add-on needs for a new job: annual earnings,
    # months in work, weekly hours — all imputed from the same donors so they
    # stay mutually consistent (a donor's wage comes with that donor's hours).
    target_col = {"yem": "yem_a", "yemmy": "yemmy_a", "lhw": "lhw_a"}
    for var in INCOME_VARS:
        if var not in donors.columns:
            continue
        vals = donors[var].to_numpy()[indices]
        df.loc[rec.index, target_col[var]] = (vals * w).sum(axis=1)

    diag["mean_distance"] = round(float(distances.mean()), 4)
    diag["yem_a_mean"] = round(float(df.loc[rec.index, "yem_a"].mean()), 2)
    return df, diag
