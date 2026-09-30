"""
Weighted statistics on arrays: mean, percentile and Gini coefficient.

Every function takes plain arrays of values and survey weights (``dwt`` in EUROMOD
output) and returns a number.
"""
import numpy as np

__all__ = ["weighted_mean", "weighted_percentile", "gini"]


def weighted_mean(values, weights):
    """
    Weighted mean.

    Parameters
    ----------
    values, weights : array-like
        Values and their weights, of equal length.

    Returns
    -------
    float
    """
    values, weights = np.asarray(values, dtype=float), np.asarray(weights, dtype=float)
    return float((values * weights).sum() / weights.sum())


def weighted_percentile(values, weights, percentile):
    """
    Weighted percentile: the smallest value whose cumulative weight reaches the
    requested share of the total weight.

    The result is always one of ``values``; nothing is interpolated.

    Parameters
    ----------
    values, weights : array-like
        Values and their weights, of equal length.
    percentile : float
        Between 0 and 100.

    Returns
    -------
    float
    """
    values, weights = np.asarray(values), np.asarray(weights)
    order = np.argsort(values)
    cumulative = np.cumsum(weights[order])
    position = np.searchsorted(cumulative, cumulative[-1] * percentile / 100)
    return values[order][min(position, len(values) - 1)]


def gini(values, weights):
    """
    Weighted Gini coefficient, from the area under the Lorenz curve (trapezoids).

    Parameters
    ----------
    values, weights : array-like
        Values (e.g. household income) and their weights, of equal length.
        Observations where either is missing are left out.

    Returns
    -------
    float
        0 when everyone has the same value, approaching 1 as it concentrates.
    """
    values, weights = np.asarray(values, dtype=float), np.asarray(weights, dtype=float)
    keep = ~(np.isnan(values) | np.isnan(weights))
    values, weights = values[keep], weights[keep]

    order = np.argsort(values)
    population = np.cumsum(weights[order])
    income = np.cumsum(values[order] * weights[order])
    population, income = population / population[-1], income / income[-1]

    lorenz_area = np.sum((population[1:] - population[:-1])
                         * (income[1:] + income[:-1]) / 2)
    return float(1 - 2 * lorenz_area)
