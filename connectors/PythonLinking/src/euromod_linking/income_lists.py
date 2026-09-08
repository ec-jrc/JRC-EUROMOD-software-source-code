"""The standardised ``ils_udb_*`` income lists, and the descriptive names for them.

A ``scale`` shock names what it moves. That can be a raw input variable
(``yem``), an income list (``ils_udb_yem``), or — because neither of those is
how anyone says it out loud — the concept itself: ``"employment income"``.

Why the list and not the variable
---------------------------------
"Employment income" as an economic concept is not one column. The model's own
``ils_udb_yem`` covers every component EUROMOD counts under that concept, which
is country- and extension-specific: 26 distinct data-reported components across
the EU-27, and a different subset in each country. Naming the list keeps a shock
consistent with the model's own accounting instead of a guess about which raw
variables belong to it.

So the catalogue below is a *naming* aid only. Membership is never tabulated
here — it is resolved against the live model at run time by
:func:`euromod_linking.query.income_list_components`, and a name a given system
does not define fails with the names it does.

Where the names come from
-------------------------
``label`` is the list's own ``DefIl`` comment in the country XML, taken as the
majority spelling across the EU-27 country files. ``aliases`` are the other
comments the model itself uses for the same list in other countries, plus the
short forms an analyst is likely to type. Both are accepted; matching ignores
case, underscores, hyphens and punctuation, so ``"Employment income"``,
``employment_income`` and ``EMPLOYMENT INCOME`` are one name.

Resolution happens in :func:`euromod_linking.shock_table.normalize`, the way
group keys are canonicalised there, so the canonical table always holds the
``ils_udb_*`` name. That is what keeps the content id a property of the
*scenario* rather than of how it was spelled.

What is worth scaling
---------------------
``group`` splits the catalogue by what its components actually are, counted over
all 27 country files:

``market income``
    Data-reported throughout — ``ils_udb_yiy``, ``ypr``, ``ypp``, ``ypt``,
    ``yot``, ``kfbcc`` and ``xmp`` resolve to *no* simulated component in any
    country, and ``yem``/``yse`` to three apiece against 26 and 23 reported
    ones. Scaling these changes what the model is given, which is the point.

``benefits and taxes``
    Mixed to overwhelmingly simulated: ``ils_udb_tis`` is one reported component
    against 121 simulated ones. EUROMOD recomputes a simulated component from
    the rules during the run, so scaling it changes nothing and it is reported
    as ``skipped_not_in_input``. To change what a benefit or tax pays out, shock
    its policy parameters through the ``constant`` channel instead.

``aggregate``
    ``ils_udb_yds`` is disposable income: it holds no variables of its own, only
    the other twenty lists, taxes included. Scaling it fans out over all of
    them at once, which is almost never a shock anyone meant to write.
"""

from __future__ import annotations

import difflib
import re
from dataclasses import dataclass

#: Render order for the groups. Market income first: those are the lists a
#: scale shock should normally reach for.
GROUPS = ("market income", "benefits and taxes", "aggregate")

#: Prose for each group, used by the docs directive and by ``catalogue()``.
GROUP_NOTES = {
    "market income": "Data-reported. Scaling these changes what the model is given.",
    "benefits and taxes": "Largely simulated: EUROMOD recomputes these from the "
                          "rules, so scaling them mostly does nothing. Shock the "
                          "policy parameters through the 'constant' channel instead.",
    "aggregate": "Built from the other lists rather than from variables of its own. "
                 "Scaling it fans out over all of them at once.",
}


@dataclass(frozen=True)
class IncomeList:
    """One standardised ``ils_udb_*`` list and the names that reach it."""

    name: str
    label: str
    group: str
    aliases: tuple[str, ...] = ()
    note: str = ""

    @property
    def scalable(self) -> bool:
        """Whether scaling this list is a meaningful shock. See module docstring."""
        return self.group == "market income"

    @property
    def accepted(self) -> tuple[str, ...]:
        """Every descriptive spelling that resolves to this list, label first."""
        return (self.label,) + self.aliases


#: The 21 ``ils_udb_*`` lists the EUROMOD EU-27 model defines. Labels and most
#: aliases are the model's own ``DefIl`` comments; see the module docstring.
CATALOGUE: tuple[IncomeList, ...] = (
    # ---- market income: data-reported, and what a scale shock should name ----
    IncomeList("ils_udb_yem", "Employment income", "market income",
               ("earnings", "wages and salaries", "employee income",
                "UDB SILC harmonised employment income")),
    IncomeList("ils_udb_yse", "Self-employment income", "market income",
               ("business income", "UDB SILC harmonised self-employment income")),
    IncomeList("ils_udb_yiy", "Investment income", "market income",
               ("capital income", "income from capital",
                "interests, dividends, income from capital investments",
                # The model's own comment carries this typo; accept it as written.
                "interests, dividents, income from capital investments",
                "UDB SILC harmonised investment income")),
    IncomeList("ils_udb_ypr", "Income from rental of property or land", "market income",
               ("property income", "rental income", "income from property",
                "income from rent", "UDB SILC harmonised income from rent")),
    IncomeList("ils_udb_ypp", "Pensions from individual private plans", "market income",
               ("private pensions", "private pension income",
                "income from private pension",
                "UDB SILC harmonised pension from individual private plans")),
    IncomeList("ils_udb_ypt", "Private transfers (received)", "market income",
               ("household transfers received",
                "inter-household cash transfers received",
                "UDB SILC harmonised cash transfers received"),
               note="Received, so an inflow. The paid side is ils_udb_xmp."),
    IncomeList("ils_udb_yot", "Income received by people aged under 16", "market income",
               ("income of children under 16", "children income",
                "UDB SILC harmonised income received by people aged under 16")),
    IncomeList("ils_udb_kfbcc", "Company car", "market income",
               ("company car benefit", "in-kind transfers", "in-kind benefits",
                "UDB SILC harmonised company car")),
    IncomeList("ils_udb_xmp", "Private transfers (paid)", "market income",
               ("maintenance payments", "maintenance paid",
                "inter-household cash transfers paid",
                "UDB SILC harmonised cash transfers paid"),
               note="Paid, so an outflow: growing it lowers disposable income. "
                    "The received side is ils_udb_ypt."),

    # ---- benefits and taxes: largely simulated, see the module docstring ----
    IncomeList("ils_udb_bun", "Unemployment benefits", "benefits and taxes",
               ("UDB SILC harmonised unemployment benefits",)),
    IncomeList("ils_udb_bsa", "Social assistance and social exclusion benefits",
               "benefits and taxes",
               ("social assistance", "social exclusion benefits",
                "UDB SILC harmonised social assistance/exclusion benefits")),
    IncomeList("ils_udb_bho", "Housing benefits", "benefits and taxes",
               ("housing allowances", "UDB SILC harmonised housing allowances")),
    IncomeList("ils_udb_bhl", "Health and sickness benefits", "benefits and taxes",
               ("sickness benefits", "health benefits",
                "UDB SILC harmonised sickness benefits")),
    IncomeList("ils_udb_bed", "Education benefits", "benefits and taxes",
               ("education allowances", "educational allowances",
                "UDB SILC harmonised education related allowances")),
    IncomeList("ils_udb_bfa", "Family benefits", "benefits and taxes",
               ("child benefits", "family and children allowances",
                "UDB SILC harmonised family children related allowances")),
    IncomeList("ils_udb_boa", "Old-age benefits", "benefits and taxes",
               ("old-age pensions", "public pensions",
                "UDB SILC harmonised old-age benefits")),
    IncomeList("ils_udb_bsu", "Survivor benefits", "benefits and taxes",
               ("survivors benefits", "UDB SILC harmonised survivor benefits")),
    IncomeList("ils_udb_bdi", "Disability benefits", "benefits and taxes",
               ("UDB SILC harmonised disability benefits",)),
    IncomeList("ils_udb_tis", "Tax on income and social security contributions",
               "benefits and taxes",
               ("income tax and social insurance contributions",
                "income taxes and social contributions",
                "UDB SILC harmonised tax on income and social contributions"),
               note="1 reported component against 121 simulated ones across the "
                    "EU-27: scaling it moves almost nothing."),
    IncomeList("ils_udb_tpr", "Taxes on wealth", "benefits and taxes",
               ("property tax", "wealth taxes", "regular taxes on wealth",
                "UDB SILC harmonised tax on wealth")),

    # ---- aggregate ----
    IncomeList("ils_udb_yds", "Disposable income", "aggregate",
               ("household disposable income", "UDB SILC disposable income"),
               note="Holds no variables of its own — it sums the other twenty "
                    "lists, taxes included. Name a component list instead."),
)

BY_NAME: dict[str, IncomeList] = {e.name: e for e in CATALOGUE}


def _key(text: str) -> str:
    """Matching key: lowercase, punctuation and separators collapsed to spaces.

    So ``Employment income``, ``employment_income``, ``EMPLOYMENT-INCOME`` and
    ``Private transfers (received)`` each reduce to one thing. Nothing an
    EUROMOD variable name can be reduces to a multi-word key, which is what
    makes it safe to treat a whitespace-bearing metric as a concept name."""
    return " ".join(re.sub(r"[^a-z0-9]+", " ", str(text).lower()).split())


def _build_index() -> dict[str, str]:
    """Alias key -> canonical name. A collision is a packaging error, not a
    runtime one: two lists claiming one name would make resolution arbitrary,
    so it fails at import where a test will see it."""
    index: dict[str, str] = {}
    for entry in CATALOGUE:
        for spelling in (entry.name,) + entry.accepted:
            k = _key(spelling)
            if k in index and index[k] != entry.name:
                raise RuntimeError(
                    f"income-list alias {spelling!r} claimed by both "
                    f"{index[k]} and {entry.name}")
            index[k] = entry.name
    return index


_INDEX: dict[str, str] = _build_index()


def names() -> list[str]:
    """The catalogued ``ils_udb_*`` names, in catalogue order."""
    return [e.name for e in CATALOGUE]


def canonical_metric(metric: str) -> str | None:
    """The ``ils_udb_*`` name a descriptive metric denotes, or None.

    Returns None for anything not in the catalogue — a raw variable name, an
    ``align`` metric, a list this catalogue does not carry."""
    return _INDEX.get(_key(metric))


def suggestions(metric: str, n: int = 3) -> list[str]:
    """The catalogued names closest to what was written, for an error message."""
    close = difflib.get_close_matches(_key(metric), list(_INDEX), n=n, cutoff=0.6)
    seen, out = set(), []
    for k in close:
        name = _INDEX[k]
        if name not in seen:
            seen.add(name)
            out.append(f"{BY_NAME[name].label!r} ({name})")
    return out


def resolve_metric(metric: str) -> tuple[str, str | None]:
    """``(canonical metric, problem)`` for a ``scale`` metric.

    A catalogued spelling resolves to its ``ils_udb_*`` name. Anything else is
    returned unchanged — *except* a metric carrying whitespace, which no EUROMOD
    variable or income list name ever does: whoever wrote it meant a concept and
    misspelled it, and saying so here is cheaper than a later "not a column of
    the input dataset"."""
    name = canonical_metric(metric)
    if name is not None:
        return name, None
    text = str(metric).strip()
    if re.search(r"\s", text):
        hint = suggestions(text)
        detail = (f"Closest: {', '.join(hint)}." if hint
                  else f"Accepted names: {', '.join(e.label for e in CATALOGUE)}.")
        return text, (f"metric {text!r} is not a known income concept. {detail} "
                      "A metric is an input variable, an income list, or one of "
                      "the descriptive names for a list")
    return text, None


def catalogue() -> dict:
    """The catalogue as plain data: what each list covers and what it answers to."""
    return {
        "groups": {
            g: {
                "note": GROUP_NOTES[g],
                "lists": {
                    e.name: {"label": e.label, "scalable": e.scalable,
                             "accepted": list(e.accepted), "note": e.note}
                    for e in CATALOGUE if e.group == g
                },
            }
            for g in GROUPS
        },
        "aliases": {e.name: list(e.accepted) for e in CATALOGUE},
    }
