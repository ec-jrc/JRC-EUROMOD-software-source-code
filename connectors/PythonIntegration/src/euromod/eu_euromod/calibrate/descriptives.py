"""
Statistics that describe a household sample.

All functions take a household sample and return a small table. Quantiles are
quantiles of gross income over the whole sample, with survey weights; percentages
are on a 0-100 scale.
"""
import numpy as np
import pandas as pd

from .estimation import HOUSEHOLD_TYPES
from .weighted import gini, weighted_mean, weighted_percentile

__all__ = ["sample_characteristics", "income_shares", "gini_coefficients",
           "tax_rates_by_quantile", "benefit_ratio_by_quantile", "family_composition"]


def _quantiles(values, weights, n):
    """Label every household Q1..Qn by weighted quantile of ``values``."""
    edges = [weighted_percentile(values.to_numpy(), weights.to_numpy(), 100 * i / n)
             for i in range(n + 1)]
    return pd.cut(values, bins=edges, labels=[f"Q{i + 1}" for i in range(n)],
                  include_lowest=True, duplicates="drop")


def _by_quantile(sample, quantile, column):
    """Weighted mean of ``column`` within each quantile."""
    return (sample[column] * sample["dwt"]).groupby(quantile, observed=True).sum() \
        / sample["dwt"].groupby(quantile, observed=True).sum()


def sample_characteristics(sample):
    """
    Size and composition of the sample.

    Returns
    -------
    dict
        ``n`` (households), ``head_age`` (mean age of the head), ``share_married``
        (share of households with a married head), ``share_children`` (share with
        at least one child), ``children_per_family`` (mean number of children
        among households that have any) and ``hh_size`` (mean household size).
        Shares are between 0 and 1. All means use survey weights.
    """
    w = sample["dwt"]
    with_children = sample["n_children"] >= 1
    return {
        "n": len(sample),
        "head_age": weighted_mean(sample["head_age"], w),
        "share_married": weighted_mean(sample["married"], w),
        "share_children": weighted_mean(with_children, w),
        "children_per_family": weighted_mean(sample.loc[with_children, "n_children"],
                                             w[with_children]),
        "hh_size": weighted_mean(sample["hh_size"], w),
    }


def income_shares(sample, n=5):
    """
    Share of total income held by each quantile, for gross and disposable income.

    Each column ranks households by its own income: gross shares by gross income,
    disposable shares by disposable income.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    n : :obj:`int`
        Number of quantiles (5 for quintiles).

    Returns
    -------
    pandas.DataFrame
        Rows ``Q1..Qn``; columns ``gross`` and ``disposable``, in percent.
    """
    shares = {}
    for name, column in (("gross", "income_norm"), ("disposable", "disposable_norm")):
        income = sample[column] * sample["dwt"]
        quantile = _quantiles(sample[column], sample["dwt"], n)
        shares[name] = income.groupby(quantile, observed=True).sum() / income.sum() * 100
    return pd.DataFrame(shares)


def gini_coefficients(sample):
    """
    Gini coefficients of gross and disposable household income.

    Returns
    -------
    dict
        ``gross`` and ``disposable``.
    """
    w = sample["dwt"].to_numpy()
    return {"gross": gini(sample["income_norm"].to_numpy(), w),
            "disposable": gini(sample["disposable_norm"].to_numpy(), w)}


def tax_rates_by_quantile(sample, n=10, household_types=HOUSEHOLD_TYPES):
    """
    Mean net tax rate by income quantile, for each household type.

    The quantiles are those of the whole sample, so a column compares households
    of one type with all households at the same income.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    n : :obj:`int`
        Number of quantiles (10 for deciles).
    household_types : :obj:`dict`
        Conditions by name; default :data:`HOUSEHOLD_TYPES`.

    Returns
    -------
    pandas.DataFrame
        Rows ``Q1..Qn``; one column per household type, in percent.
    """
    quantile = _quantiles(sample["income_norm"], sample["dwt"], n)
    rates = {}
    for name, condition in household_types.items():
        households = sample if condition is None else sample.query(condition)
        rates[name] = _by_quantile(households, quantile.loc[households.index], "net_tax_rate") * 100
    return pd.DataFrame(rates)


def benefit_ratio_by_quantile(sample, n=10, benefit="ils_benmt", income="ils_origrepy"):
    """
    Benefits as a share of gross income, by income quantile.

    The ratio is computed for each household and then averaged, so it describes
    the typical household in a quantile rather than the quantile's totals.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    n : :obj:`int`
        Number of quantiles.
    benefit : :obj:`str`
        Household total to take the ratio of; default means-tested benefits.
    income : :obj:`str`
        Household gross income the sample was built with.

    Returns
    -------
    pandas.DataFrame
        Rows ``Q1..Qn`` and ``all``; columns ``ratio`` (mean ratio, in percent)
        and ``weight`` (total survey weight, for combining quantiles).
    """
    sample = sample.assign(_ratio=sample[benefit] / sample[income])
    quantile = _quantiles(sample["income_norm"], sample["dwt"], n)
    table = pd.DataFrame({"ratio": _by_quantile(sample, quantile, "_ratio") * 100,
                          "weight": sample["dwt"].groupby(quantile, observed=True).sum()})
    table.loc["all"] = [weighted_mean(sample["_ratio"], sample["dwt"]) * 100,
                        sample["dwt"].sum()]
    return table


def family_composition(sample):
    """
    How marriage and children go together in the sample.

    Returns
    -------
    dict
        Shares between 0 and 1: ``children_among_single`` and
        ``children_among_married`` (households with children, among those with an
        unmarried or a married head), ``single_among_children`` and
        ``married_among_children`` (the reverse).
    """
    w = sample["dwt"]
    married = sample["married"].astype(bool)
    children = sample["n_children"] >= 1
    return {
        "children_among_single": weighted_mean(children[~married], w[~married]),
        "children_among_married": weighted_mean(children[married], w[married]),
        "single_among_children": weighted_mean(~married[children], w[children]),
        "married_among_children": weighted_mean(married[children], w[children]),
    }
