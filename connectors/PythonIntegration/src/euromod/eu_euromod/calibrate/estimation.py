"""
The tax-benefit schedule and how it is estimated.

The schedule
------------
A country's tax-benefit system is summarised by the function of Heathcote,
Storesletten and Violante (HSV). For a household with gross income ``y``, measured
in multiples of mean income, disposable income is ``(1 - lambda) * y**(1 - tau)``,
so the average net tax rate is::

    1 - (1 - lambda) * y**(-tau)

``lambda`` is the net tax rate at mean income (the *level*) and ``tau`` measures
how fast the rate rises with income (the *progressivity*; ``tau = 0`` is a flat
rate). Benefits are included, so the rate is negative for households that receive
more than they pay.

The estimator
-------------
Taking logs makes the schedule linear::

    log(disposable) = log(1 - lambda) + (1 - tau) * log(y)

:func:`fit_hsv` estimates it by weighted least squares, with the survey weights
``dwt``. There is a closed-form solution, so nothing is optimised numerically and
the result does not depend on starting values.

To compare a reform with a baseline it is often useful to hold the level fixed and
let only the progressivity move. With ``lambda_fixed`` the intercept is known, and
``tau`` comes from a regression through the origin of
``log(disposable) - log(1 - lambda)`` on ``log(y)``.

Household types are subsets of one sample (:data:`HOUSEHOLD_TYPES`). They share
its income scale, so their estimates can be compared with each other.
"""
import numpy as np
import pandas as pd

from .weighted import gini

__all__ = ["HOUSEHOLD_TYPES", "select_households", "hsv", "marginal_hsv", "fit_hsv",
           "pool", "binned_means", "net_tax_rate_at_mean_income", "fit_diagnostics",
           "to_calibration"]

#: Household types, as conditions on the columns of a household sample.
HOUSEHOLD_TYPES = {
    "all": None,
    "married": "married",
    "single": "not married",
    "single_no_children": "not married and n_children == 0",
    "single_some_children": "not married and n_children >= 1",
    "married_no_children": "married and n_children == 0",
    "married_some_children": "married and n_children >= 1",
}


def select_households(sample, household_type):
    """
    The households of one type.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    household_type : :obj:`str`
        A key of :data:`HOUSEHOLD_TYPES`, or any condition on the sample's
        columns, e.g. ``"hh_size == 1"``.

    Returns
    -------
    pandas.DataFrame
    """
    condition = HOUSEHOLD_TYPES.get(household_type, household_type)
    return sample if condition is None else sample.query(condition)


def hsv(income, lambda_, tau):
    """
    Average net tax rate of the HSV schedule.

    Parameters
    ----------
    income : float or array-like
        Gross income in multiples of mean income.
    lambda_ : float
        Level: the net tax rate at mean income.
    tau : float
        Progressivity.

    Returns
    -------
    float or numpy.ndarray

    Example
    -------
    >>> hsv(np.array([0.5, 1.0, 2.0]), 0.1925, 0.1631)
    array([0.0958, 0.1925, 0.2788])
    """
    return 1 - (1 - lambda_) * np.asarray(income) ** (-tau)


def marginal_hsv(income, lambda_, tau):
    """
    Marginal net tax rate of the HSV schedule: the share of an extra euro of gross
    income that does not reach disposable income.

    Parameters
    ----------
    income, lambda_, tau
        As in :func:`hsv`.

    Returns
    -------
    float or numpy.ndarray
    """
    return 1 - (1 - lambda_) * (1 - tau) * np.asarray(income) ** (-tau)


def fit_hsv(sample, *, lambda_fixed=None):
    """
    Estimate the HSV schedule on a household sample.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample with ``income_norm``, ``disposable_norm`` and ``dwt``.
    lambda_fixed : float, optional
        Hold the level at this value and estimate only ``tau``.

    Returns
    -------
    dict
        ``lambda``, ``tau``, ``se_tau`` and ``n``, plus

        - ``se_intercept`` (standard error of ``log(1 - lambda)``), ``se_lambda``
          and ``r2`` when both parameters are estimated;
        - ``r2_uncentered`` when ``lambda`` is fixed.

        Standard errors are the classical ones of weighted least squares.

    Example
    -------
    >>> fit = fit_hsv(sample)
    >>> fit["lambda"], fit["tau"]
    (0.1925, 0.1631)
    >>> fit_hsv(select_households(sample, "married"))["tau"]
    0.1918
    """
    x = np.log(sample["income_norm"].to_numpy(dtype=float))
    y = np.log(sample["disposable_norm"].to_numpy(dtype=float))
    w = sample["dwt"].to_numpy(dtype=float)
    n = int(x.size)

    if lambda_fixed is not None:
        y = y - np.log(1 - lambda_fixed)
        slope = (w * x * y).sum() / (w * x * x).sum()
        squared_residuals = (w * (y - slope * x) ** 2).sum()
        return {"lambda": float(lambda_fixed), "tau": float(1 - slope),
                "se_tau": float(np.sqrt(squared_residuals / (n - 1) / (w * x * x).sum())),
                "r2_uncentered": float(1 - squared_residuals / (w * y * y).sum()),
                "n": n}

    x_mean, y_mean = np.average(x, weights=w), np.average(y, weights=w)
    slope = (w * (x - x_mean) * (y - y_mean)).sum() / (w * (x - x_mean) ** 2).sum()
    intercept = y_mean - slope * x_mean
    squared_residuals = (w * (y - intercept - slope * x) ** 2).sum()

    design = np.column_stack([np.sqrt(w), x * np.sqrt(w)])
    covariance = squared_residuals / (n - 2) * np.linalg.inv(design.T @ design)
    se_intercept, se_slope = np.sqrt(np.diag(covariance))

    lambda_ = 1 - np.exp(intercept)
    return {"lambda": float(lambda_), "tau": float(1 - slope),
            "se_intercept": float(se_intercept),
            "se_lambda": float((1 - lambda_) * se_intercept),
            "se_tau": float(se_slope),
            "r2": float(1 - squared_residuals / (w * (y - y_mean) ** 2).sum()),
            "n": n}


def pool(samples):
    """
    Pool the household samples of several countries, e.g. into a euro-area sample.

    Each country keeps its own income scale (1.0 is that country's mean income)
    and its own survey weights, so a country counts in proportion to its
    population.

    Parameters
    ----------
    samples : dict
        Household samples by system name, e.g. ``{"AT_2022": ..., "BE_2022": ...}``.

    Returns
    -------
    pandas.DataFrame
        All households, with a ``country`` column.
    """
    return pd.concat([sample.assign(country=name[:2]) for name, sample in samples.items()],
                     ignore_index=True)


def binned_means(sample, bins=80):
    """
    Mean income and mean net tax rate in equally wide income bins.

    Used to judge the shape of the fitted schedule without the scatter of
    individual households.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    bins : :obj:`int`
        Number of bins between the lowest and highest ``income_norm``.

    Returns
    -------
    pandas.DataFrame
        One row per non-empty bin: ``income_norm`` and ``net_tax_rate`` (plain
        means over the households in the bin), ``dwt_sum`` (their total weight)
        and ``n`` (their number).
    """
    groups = sample.groupby(pd.cut(sample["income_norm"], bins=bins), observed=False)
    return groups.agg(income_norm=("income_norm", "mean"),
                      net_tax_rate=("net_tax_rate", "mean"),
                      dwt_sum=("dwt", "sum"),
                      n=("dwt", "size")).dropna()


def net_tax_rate_at_mean_income(sample, band=0.05):
    """
    Observed net tax rate of households at mean income.

    A weighted straight line is fitted through the net tax rates of households
    whose income is within ``band`` of the mean, and read off at the mean. This is
    the data counterpart of ``lambda``.

    Parameters
    ----------
    sample : pandas.DataFrame
        A household sample.
    band : float
        Half-width of the income window around 1.0.

    Returns
    -------
    float
        If no household falls in the window, the rate of the closest household.
    """
    near = sample[sample["income_norm"].between(1 - band, 1 + band)]
    near = near[["income_norm", "net_tax_rate", "dwt"]].dropna()
    if len(near) == 0:
        closest = (sample["income_norm"] - 1).abs().idxmin()
        return float(sample.loc[closest, "net_tax_rate"])

    w, y = near["dwt"].to_numpy(), near["net_tax_rate"].to_numpy()
    x = near["income_norm"].to_numpy() - 1
    sw, sx, sxx, sy, sxy = w.sum(), (w * x).sum(), (w * x * x).sum(), (w * y).sum(), (w * x * y).sum()
    determinant = sw * sxx - sx * sx
    return float(sy / sw if determinant == 0 else (sy * sxx - sx * sxy) / determinant)


def fit_diagnostics(sample, fit, bins=80):
    """
    How well a fitted schedule describes a sample.

    Parameters
    ----------
    sample : pandas.DataFrame
        The household sample the schedule was fitted on.
    fit : dict
        Result of :func:`fit_hsv`.
    bins : :obj:`int`
        Number of income bins for the binned errors.

    Returns
    -------
    dict
        - ``net_tax_rate_at_mean``: see :func:`net_tax_rate_at_mean_income`.
        - ``rmse``, ``mae``: weighted errors of the net tax rate, household by
          household. Dominated by the spread of households around the schedule.
        - ``rmse_binned``, ``mae_binned``: the same against :func:`binned_means`,
          weighted by bin population. These measure the shape of the schedule.
        - ``gini_gross``, ``gini_disposable``: observed Gini coefficients.
        - ``gini_fitted``: Gini of the disposable income the fitted schedule
          assigns to the same gross incomes. The gap to ``gini_disposable`` is the
          redistribution the two-parameter schedule does not capture.
    """
    lambda_, tau = fit["lambda"], fit["tau"]
    income = sample["income_norm"].to_numpy(dtype=float)
    w = sample["dwt"].to_numpy(dtype=float)
    error = sample["net_tax_rate"].to_numpy(dtype=float) - hsv(income, lambda_, tau)

    binned = binned_means(sample, bins)
    binned_w = binned["dwt_sum"].to_numpy()
    binned_error = binned["net_tax_rate"].to_numpy() - hsv(binned["income_norm"].to_numpy(), lambda_, tau)

    return {
        "net_tax_rate_at_mean": net_tax_rate_at_mean_income(sample),
        "rmse": float(np.sqrt((w * error ** 2).sum() / w.sum())),
        "mae": float((w * np.abs(error)).sum() / w.sum()),
        "rmse_binned": float(np.sqrt((binned_w * binned_error ** 2).sum() / binned_w.sum())),
        "mae_binned": float((binned_w * np.abs(binned_error)).sum() / binned_w.sum()),
        "gini_gross": gini(income, w),
        "gini_disposable": gini(sample["disposable_norm"].to_numpy(dtype=float), w),
        "gini_fitted": gini((1 - lambda_) * income ** (1 - tau), w),
    }


def to_calibration(fits, *, system, dataset="", model_version="", constants=None):
    """
    Export fitted schedules as a :class:`euromod.calibrate.Calibration`.

    The ``Calibration`` evaluates the schedule with a numba-compiled function, which
    can be called from other compiled code, and can be saved to and loaded from
    JSON. It needs ``sympy`` and ``numba`` (``pip install euromod[calibrate]``).

    Parameters
    ----------
    fits : dict
        One result of :func:`fit_hsv`, or several by household type:
        ``{"all": fit, "married": fit_married}``.
    system : :obj:`str`
        Name of the system the fits belong to.
    dataset, model_version : :obj:`str`, optional
        Recorded in the calibration.
    constants : :obj:`dict`, optional
        Constants that were overridden in the run, as passed to
        :func:`~euromod.eu_euromod.calibrate.simulate`.

    Returns
    -------
    euromod.calibrate.Calibration
        With one group per household type.

    Example
    -------
    >>> cal = to_calibration(fit_hsv(sample), system="PT_2022")
    >>> cal(np.array([0.5, 1.0, 2.0]))
    array([0.0958, 0.1925, 0.2788])
    >>> cal.to_json("PT_2022_hsv.json")
    >>> schedule = cal.numba_function          # schedule(income, lambda_t, tau)
    """
    import sympy
    from euromod.calibrate import Calibration

    if "tau" in fits:
        fits = {"all": fits}
    conditions = {name: HOUSEHOLD_TYPES.get(name, name) for name in fits}
    constants = {(key, "") if isinstance(key, str) else tuple(key): str(value)
                 for key, value in (constants or {}).items()}

    return Calibration(
        model_version=model_version, system=system, dataset_id=dataset,
        functional_form=sympy.sympify("1 - (1 - lambda_t) * income_norm**(-tau)"),
        data_vars=["income_norm"], parameter_vars=["lambda_t", "tau"],
        estimates={name: {"lambda_t": fit["lambda"], "tau": fit["tau"]}
                   for name, fit in fits.items()},
        target_variable="net_tax_rate", function_type="household",
        group_filters={name: "SELECT * FROM self"
                       + (f" WHERE {condition.replace('==', '=')}" if condition else "")
                       for name, condition in conditions.items()},
        constantsToOverwrite=constants,
        variable_labels={"income_norm": "Gross income in multiples of mean income",
                         "net_tax_rate": "Average net tax rate",
                         "lambda_t": "Level", "tau": "Progressivity"})
