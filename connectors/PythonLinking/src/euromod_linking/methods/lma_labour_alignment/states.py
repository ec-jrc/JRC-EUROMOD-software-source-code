"""Labour-market state classification for the LMA alignment method.

The four states partition the population by their role in the alignment:
employed / unemployed (the two states macro targets speak about), inactive
(the recruitment pool for participation entries — domestic tasks and "other"
inactivity, i.e. people who *could* plausibly join the labour force), and
'other' (students, retirees, long-term sick/disabled, conscripts — structurally
out of the labour market and therefore shielded: alignment never moves them,
however large a cell's gap). Without that shield, a big employment target
could "hire" retirees or students, which would be demographically absurd and
would leak pension/education benefit changes into the results.

les2 (from EMSD monthly activity, PL211) is preferred over les because it
separates domestic-tasks inactivity from retirement and disability — exactly
the distinction the recruitment pool needs; the standard les coding is the
fallback approximation.

Fixed methodology constants (les/les2 code sets) — not scenario-configurable.
"""

import pandas as pd

# les2 (EMSD PL211 monthly activity) codes
LES2_CODES = {
    "employed": [1, 2, 3, 4],   # FT/PT employee, FT/PT self-employed
    "unemployed": [5],
    "inactive": [10, 11],       # domestic tasks, other
    "other": [6, 7, 8, 9],      # student, retired, health, military
}

# Standard les codes (fallback when les2 is absent)
LES_CODES = {
    "employed": [2, 3],
    "unemployed": [5],
    "inactive": [7, 9],
    "other": [0, 1, 4, 6, 8],
}

WORKING_AGE = (18, 65)


def les_variable(columns) -> tuple[str, dict]:
    """Pick the labour-status variable: les2 (more detailed) when present."""
    return ("les2", LES2_CODES) if "les2" in columns else ("les", LES_CODES)


def definition(les_var: str | None = None) -> dict:
    """The population definitions this methodology works from, as data.

    Published so a caller can size a shock against the same population the
    alignment will move, instead of reconstructing one from other variables.
    Built from the code sets above, so it cannot drift from the classification.
    """
    codes = LES2_CODES if les_var == "les2" else LES_CODES
    return {
        "labour_status_variable": les_var or "les2 when present, else les",
        "working_age": {"variable": "dag", "min": WORKING_AGE[0], "max": WORKING_AGE[1],
                        "note": "inclusive; people outside it are never moved"},
        "states": {name: sorted(values) for name, values in codes.items()},
        "movable": ["inactive"],
        "notes": [
            "States come from les/les2 — never from employment income. 'yem == 0' is "
            "'no employment income recorded' and includes children, students, pensioners, "
            "the self-employed and the unemployed.",
            "Only the 'inactive' state is recruitable. 'other' (students, retired, "
            "long-term sick, military) is shielded and never moved, however large the target.",
            "Rates are over the cell's working-age population, which the alignment leaves "
            "unchanged — so before/after rates are directly comparable.",
        ],
    }


def classify_labour_status(les: pd.Series, codes: dict) -> pd.DataFrame:
    """Binary indicators employed/unemployed/inactive/other/active.
    'other' is the catch-all for codes outside employed+unemployed+inactive."""
    out = pd.DataFrame(index=les.index)
    out["employed"] = les.isin(codes["employed"]).astype(int)
    out["unemployed"] = les.isin(codes["unemployed"]).astype(int)
    out["inactive"] = les.isin(codes["inactive"]).astype(int)
    out["other"] = (~les.isin(codes["employed"] + codes["unemployed"] + codes["inactive"])).astype(int)
    out["active"] = (out["employed"] + out["unemployed"]).astype(int)
    return out
