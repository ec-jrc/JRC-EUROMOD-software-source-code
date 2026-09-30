"""
Estimates as files: the published baseline, and saving and loading your own.

A table of estimates, as returned by
:func:`~euromod.eu_euromod.calibrate.calibrate_hsv`, has one row per system and
household type. :func:`save_estimates` writes such a table to a JSON file, together
with any notes on how it was made, and :func:`load_estimates` reads it back, so a
model can use estimates without running EUROMOD.

The package ships one such file: the estimates for the 27 EU member states and the
euro area, policy year 2022. ``load_estimates()`` without a path returns them, and
:func:`schedule` turns a row of any table into a :class:`Schedule` that computes
tax rates, taxes and disposable incomes.
"""
import json
import os

import numpy as np
import pandas as pd

from .estimation import hsv, marginal_hsv

__all__ = ["save_estimates", "load_estimates", "schedule", "Schedule"]

_BASELINE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "baseline_2022.json")
_INDEX = ["system", "household_type"]


def save_estimates(estimates, path, **info):
    """
    Save a table of estimates as a JSON file.

    Parameters
    ----------
    estimates : pandas.DataFrame
        Estimates indexed by system and household type, as returned by
        :func:`~euromod.eu_euromod.calibrate.calibrate_hsv`.
    path : :obj:`str`
        File to write.
    **info
        What should be remembered about the estimates: anything that JSON can
        hold, e.g. ``model="J2.0+"``, ``constants={"$tin00_rate9": 0.58}``,
        ``note="top income-tax rates raised by 10 points"``.

    Example
    -------
    >>> estimates = calibrate_hsv(MODEL, DATA, ["PT_2022"], constants=reform)
    >>> save_estimates(estimates, "pt_reform.json", constants=reform)
    """
    table = estimates.reset_index()
    records = table.astype(object).where(table.notna(), None).to_dict("records")
    text = json.dumps({"info": info, "estimates": records}, indent=1)
    with open(path, "w", encoding="utf-8") as file:
        file.write(text)


def load_estimates(path=None):
    """
    Load a table of estimates.

    Parameters
    ----------
    path : :obj:`str`, optional
        A file written by :func:`save_estimates`. Default: the estimates that ship
        with the package, for the 27 EU member states (``"AT_2022"``, ...) and the
        euro area (``"EA"``), policy year 2022, by household type.

    Returns
    -------
    pandas.DataFrame
        Indexed by system and household type, with the columns of
        :func:`~euromod.eu_euromod.calibrate.fit_hsv` and
        :func:`~euromod.eu_euromod.calibrate.fit_diagnostics`. The notes saved with
        the estimates are in ``.attrs``.

    Example
    -------
    >>> baseline = load_estimates()
    >>> baseline.loc[("PT_2022", "all"), ["lambda", "tau"]]
    lambda    0.1925
    tau       0.1631
    >>> baseline.attrs["model"]
    'EUROMOD_RELEASES_J2.0+'
    """
    with open(path or _BASELINE, encoding="utf-8") as file:
        content = json.load(file)
    estimates = pd.DataFrame(content["estimates"]).set_index(_INDEX)
    estimates.attrs.update(content["info"])
    return estimates


class Schedule:
    """
    A fitted tax-benefit function, ready to use.

    Incomes are in multiples of mean gross income, as numbers or arrays: 1.0 is
    mean income. Multiply by the mean gross income of the population you apply it
    to for amounts in currency.

    Parameters
    ----------
    lambda_ : float
        Level: the net tax rate at mean income.
    tau : float
        Progressivity.

    Example
    -------
    >>> portugal = Schedule(0.1925, 0.1631)
    >>> portugal.net_tax_rate(2.0)        # average rate at twice the mean income
    0.2788
    >>> portugal.net_taxes(2.0)           # taxes net of benefits
    0.5576
    >>> portugal.disposable_income(2.0)
    1.4424
    """

    def __init__(self, lambda_, tau):
        self.lambda_, self.tau = float(lambda_), float(tau)

    def net_tax_rate(self, income):
        """Average net tax rate: taxes net of benefits, as a share of gross income."""
        return hsv(income, self.lambda_, self.tau)

    def marginal_rate(self, income):
        """Share of an extra unit of gross income that does not reach disposable income."""
        return marginal_hsv(income, self.lambda_, self.tau)

    def net_taxes(self, income):
        """Taxes net of benefits. Negative when benefits exceed taxes."""
        return np.asarray(income) * self.net_tax_rate(income)

    def disposable_income(self, income):
        """Gross income minus net taxes."""
        return np.asarray(income) - self.net_taxes(income)

    __call__ = net_tax_rate

    def __repr__(self):
        return f"Schedule(lambda_={self.lambda_:.4f}, tau={self.tau:.4f})"


def schedule(estimates, system, household_type="all"):
    """
    The fitted tax-benefit function of one system and household type, ready to use.

    Parameters
    ----------
    estimates : pandas.DataFrame
        A table of estimates, from :func:`load_estimates` or
        :func:`~euromod.eu_euromod.calibrate.calibrate_hsv`.
    system : :obj:`str`
        A system in the table, e.g. ``"PT_2022"`` or ``"EA"``.
    household_type : :obj:`str`
        A household type in the table; default all households.

    Returns
    -------
    Schedule
        Computes the net tax rate, net taxes, disposable income and the marginal
        rate at any gross income. Calling it gives the net tax rate.

    Example
    -------
    >>> portugal = schedule(load_estimates(), "PT_2022")
    >>> portugal.net_tax_rate([0.5, 1.0, 2.0])
    array([0.0958, 0.1925, 0.2788])
    >>> portugal.disposable_income(2.0)
    1.4424
    """
    lambda_, tau = estimates.loc[(system, household_type), ["lambda", "tau"]]
    return Schedule(lambda_, tau)
