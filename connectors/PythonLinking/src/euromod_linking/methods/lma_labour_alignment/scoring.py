"""Transition scoring: four logit models ranking who transitions first.

Economic intuition
------------------
The macro model says *how many* people change state per cell; these logits
decide *who*. Each model fits the cross-sectional probability of currently
being in a state (employed, unemployed, active, unemployed-given-active) on
standard labour-supply determinants — age, gender, disability income (yds, a
strong participation barrier), marital-status dummies. The fitted probability
is used as a *propensity ranking*: someone who looks like the currently
employed is assumed the most likely marginal entrant into employment, and vice
versa. Selecting movers by this ranking concentrates transitions on plausible
individuals, which is what makes the downstream distributional results (who
gains income, whose benefits change) meaningful.

This is deliberately an ordering device, not a causal transition model: only
the ranks matter, coefficients are never interpreted, and the same fit is
reused for every period so period comparisons differ only in targets.

Fixed methodology (feature set, model form) — not scenario-configurable.
Deterministic: fixed feature ordering, sorted dummy columns; statsmodels'
Newton solver is deterministic given identical inputs.

Caching
-------
The fit depends only on the *observed* population — features and current
labour states — and never on shocks, targets, cells or periods. So the same
four models are refitted identically for every scenario on a dataset: a
10-period sweep would pay for 40 logit fits that all produce the same numbers.
The scores are therefore cached, keyed on a content hash of the exact input
columns used (plus SCORING_VERSION, so a change of feature set or model form
invalidates every stored score). Content addressing means the cache cannot go
stale: different data — even the same file re-generated — hashes differently
and refits. In-memory for the process, and optionally persisted to disk
(EUROMOD_SCORE_CACHE_DIR) so a new process does not refit.
"""

import hashlib
import logging
import os
import threading
from pathlib import Path

import numpy as np
import pandas as pd

logger = logging.getLogger(__name__)

PROB_COLUMNS = ("prob_employed", "prob_unemployed", "prob_active", "prob_unemp_if_active")

# Bump when the feature set, model form or scoring semantics change — this is
# what stops previously cached scores from being served by newer code.
SCORING_VERSION = 1

# Columns the fit actually reads (features + outcomes). The fingerprint covers
# exactly these, so unrelated dataset differences never cause a refit.
_FEATURE_SOURCE_COLS = ("dag", "dgn", "yds", "dms")
_OUTCOME_COLS = ("employed", "unemployed", "active")

_cache: dict[str, pd.DataFrame] = {}
_cache_lock = threading.Lock()


def cache_enabled() -> bool:
    return os.environ.get("EUROMOD_SCORE_CACHE", "1").strip().lower() not in (
        "0", "false", "no", "off", "")


def _cache_dir() -> Path:
    d = os.environ.get("EUROMOD_SCORE_CACHE_DIR", "").strip() or os.path.join(
        os.getcwd(), ".score_cache")
    p = Path(d)
    p.mkdir(parents=True, exist_ok=True)
    return p


def fingerprint(df: pd.DataFrame) -> str | None:
    """Content hash of the scoring inputs (index + feature/outcome values).

    Includes the index so a hit guarantees the cached scores align row-for-row
    with the caller's frame. Returns None if the frame cannot be hashed, in
    which case the caller simply refits."""
    from pandas.util import hash_pandas_object

    cols = [c for c in (*_FEATURE_SOURCE_COLS, *_OUTCOME_COLS) if c in df.columns]
    try:
        row_hashes = hash_pandas_object(df[cols], index=True).values
    except Exception:
        logger.debug("scoring: could not fingerprint inputs; caching skipped", exc_info=True)
        return None
    h = hashlib.sha256()
    h.update(f"v{SCORING_VERSION}|{','.join(cols)}|{len(df)}|".encode())
    h.update(row_hashes.tobytes())
    return h.hexdigest()[:16]


def _cache_get(key: str) -> pd.DataFrame | None:
    with _cache_lock:
        if key in _cache:
            return _cache[key]
    pkl = _cache_dir() / f"{key}.pkl"
    if pkl.exists():
        try:
            probs = pd.read_pickle(pkl)
            with _cache_lock:
                _cache[key] = probs
            return probs
        except Exception:
            logger.exception("score_cache read failed for %s", key)
    return None


def _cache_put(key: str, probs: pd.DataFrame) -> None:
    with _cache_lock:
        _cache[key] = probs
    try:
        probs.to_pickle(_cache_dir() / f"{key}.pkl")
    except Exception:
        logger.exception("score_cache write failed for %s", key)


def clear_cache() -> None:
    """Drop in-memory scores (disk entries are content-addressed and harmless)."""
    with _cache_lock:
        _cache.clear()


def estimate_probabilities(df: pd.DataFrame, use_cache: bool = True) -> tuple[pd.DataFrame, dict]:
    """Add PROB_COLUMNS to df (rows = the alignment population, with binary
    status columns employed/unemployed/active already present).

    Returns (scored_df, info) where info reports whether the scores came from
    the cache. Identical inputs are served without refitting."""
    key = fingerprint(df) if (use_cache and cache_enabled()) else None
    if key is not None:
        cached = _cache_get(key)
        if cached is not None:
            logger.info("scoring: cache hit %s (%d rows, no refit)", key, len(df))
            out = df.copy()
            for col in PROB_COLUMNS:
                out[col] = cached[col]
            return out, {"cached": True, "key": key, "n_rows": int(len(df))}

    out = _fit_probabilities(df)
    if key is not None:
        _cache_put(key, out[list(PROB_COLUMNS)])
        logger.info("scoring: fitted and cached %s (%d rows)", key, len(df))
    return out, {"cached": False, "key": key, "n_rows": int(len(df))}


def _fit_probabilities(df: pd.DataFrame) -> pd.DataFrame:
    """The actual estimation — see module docstring. Pure function of df."""
    import statsmodels.api as sm

    df = df.copy()
    # Feature block: the standard observable labour-supply determinants.
    #   dag      age (life-cycle participation profile)
    #   dgn_ind  gender indicator (participation/employment gaps)
    #   yds      disability income — receiving it is a strong barrier to
    #            (re-)entering work, so it pushes the activity score down
    #   dms_*    marital-status dummies (household specialisation effects)
    feats = pd.DataFrame(index=df.index)
    feats["dag"] = pd.to_numeric(df["dag"], errors="coerce").fillna(0)
    feats["dgn_ind"] = (pd.to_numeric(df["dgn"], errors="coerce").fillna(0) == 1).astype(int)
    if "yds" in df.columns:
        feats["yds"] = pd.to_numeric(df["yds"], errors="coerce").fillna(0)
    if "dms" in df.columns:
        dms = pd.to_numeric(df["dms"], errors="coerce").fillna(0)
        if dms.nunique() > 1:
            # drop_first avoids the dummy trap; sorted columns keep the design
            # matrix (and thus the fit) identical across runs.
            dummies = pd.get_dummies(dms, prefix="dms", drop_first=True).astype(float)
            feats = pd.concat([feats, dummies[sorted(dummies.columns)]], axis=1)

    # Zero-variance features (e.g. yds all 0) make the Hessian singular.
    feats = feats.loc[:, feats.nunique() > 1]
    X = sm.add_constant(feats.astype(float), has_constant="add")

    def _fit(y: pd.Series, X_: pd.DataFrame, label: str) -> pd.Series:
        # Degenerate outcomes (all 0 or all 1) can occur in small cells: use the
        # observed rate as a constant score rather than failing.
        if y.nunique() < 2:
            logger.warning("logit '%s' degenerate (single outcome class); using constant score", label)
            return pd.Series(float(y.mean()), index=y.index)
        try:
            model = sm.Logit(y.astype(float), X_).fit(disp=0)
            return pd.Series(model.predict(X_), index=y.index)
        except Exception as e:  # separation / singular Hessian on odd samples
            logger.warning("logit '%s' failed (%s); using constant score", label, e)
            return pd.Series(float(y.mean()), index=y.index)

    # Four separate propensities, one per margin the alignment moves people
    # across. prob_active ranks participation entries/exits (level 1);
    # prob_unemployed ranks job losses/findings (level 2).
    df["prob_employed"] = _fit(df["employed"], X, "employed")
    df["prob_unemployed"] = _fit(df["unemployed"], X, "unemployed")
    df["prob_active"] = _fit(df["active"], X, "active")

    # Conditional model, fitted on the active subsample only: unemployment
    # risk GIVEN participation — kept for future refinements of level 2.
    active_idx = df.index[df["active"] == 1]
    df["prob_unemp_if_active"] = np.nan
    if len(active_idx):
        df.loc[active_idx, "prob_unemp_if_active"] = _fit(
            df.loc[active_idx, "unemployed"], X.loc[active_idx], "unemp|active")
    return df
