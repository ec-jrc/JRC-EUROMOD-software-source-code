"""
Calibrating a parametric tax-benefit function on the EU model.

A tax-benefit system has many instruments. Macroeconomic models need it in a
compact form: a function from a household's gross income to the taxes it pays net
of the benefits it receives. This package estimates such a function from a EUROMOD
simulation, for a baseline or for a reform.

It works in two steps, one module each:

:mod:`~euromod.eu_euromod.calibrate.data_preparation`
    Runs a system of the model and builds the household sample: one row per
    household, with gross and disposable income in multiples of mean income.

:mod:`~euromod.eu_euromod.calibrate.estimation`
    Fits the function of Heathcote, Storesletten and Violante,
    ``net tax rate = 1 - (1 - lambda) * income**(-tau)``, and measures how well it
    fits.

:mod:`~euromod.eu_euromod.calibrate.descriptives` describes the sample,
:mod:`~euromod.eu_euromod.calibrate.weighted` holds the weighted statistics, and
:mod:`~euromod.eu_euromod.calibrate.estimates` saves and loads estimates. The
estimates for the 27 EU member states and the euro area (policy year 2022) ship
with the package::

    from euromod.eu_euromod.calibrate import load_estimates, schedule

    portugal = schedule(load_estimates(), "PT_2022")
    portugal.net_tax_rate(1.0)            # 0.1925, the rate at mean income
    portugal.disposable_income(2.0)       # 1.4424, in multiples of mean income

The functions need a path to the model and a path to the folder with the input
data, and the name of a system::

    from euromod.eu_euromod.calibrate import prepare_sample, fit_hsv

    baseline = prepare_sample(MODEL, DATA, "PT_2022")
    fit_hsv(baseline)                     # {'lambda': 0.1925, 'tau': 0.1631, ...}

A reform is a counterfactual system of the model, or a baseline system with some
constants overridden::

    reform = prepare_sample(MODEL, DATA, "PT_2022_ref")
    reform = prepare_sample(MODEL, DATA, "PT_2022", constants={"$tin00_rate9": 0.58})

This package is independent of :mod:`euromod.calibrate`, the connector's general
engine for fitting arbitrary functional forms. :func:`to_calibration` turns a
result of this package into an object of that engine.
"""
import pandas as pd

from .data_preparation import household_sample, prepare_sample, simulate
from .descriptives import (benefit_ratio_by_quantile, family_composition, gini_coefficients,
                           income_shares, sample_characteristics, tax_rates_by_quantile)
from .estimates import Schedule, load_estimates, save_estimates, schedule
from .estimation import (HOUSEHOLD_TYPES, binned_means, fit_diagnostics, fit_hsv, hsv,
                         marginal_hsv, net_tax_rate_at_mean_income, pool, select_households,
                         to_calibration)

__all__ = ["simulate", "household_sample", "prepare_sample",
           "HOUSEHOLD_TYPES", "select_households", "hsv", "marginal_hsv", "fit_hsv", "pool",
           "binned_means", "net_tax_rate_at_mean_income", "fit_diagnostics", "to_calibration",
           "sample_characteristics", "income_shares", "gini_coefficients",
           "tax_rates_by_quantile", "benefit_ratio_by_quantile", "family_composition",
           "save_estimates", "load_estimates", "schedule", "Schedule", "calibrate_hsv"]


def calibrate_hsv(model_path, data_path, systems, *, household_types=HOUSEHOLD_TYPES,
                  pooled=None, lambda_fixed=None, constants=None, cache_dir=None,
                  **sample_options):
    """
    Estimate the HSV schedule for several systems and household types in one call.

    Parameters
    ----------
    model_path : :obj:`str`
        Folder of the EUROMOD model.
    data_path : :obj:`str`
        Folder with the input datasets.
    systems : list of :obj:`str`
        System names, e.g. ``["PT_2022"]`` or
        :func:`euromod.eu_euromod.eu27_systems` ``(2022)``.
    household_types : :obj:`dict`
        Conditions by name; default :data:`HOUSEHOLD_TYPES`.
    pooled : :obj:`dict`, optional
        Groups of countries to estimate on a pooled sample as well, e.g.
        ``{"EA": EURO_AREA_19}``. Every country of a group must be among
        ``systems``.
    lambda_fixed : float, optional
        Hold the level at this value and estimate only ``tau``.
    constants, cache_dir
        As in :func:`simulate`. ``constants`` apply to every system.
    **sample_options
        As in :func:`household_sample`.

    Returns
    -------
    pandas.DataFrame
        One row per system (or pooled group) and household type, with the columns
        of :func:`fit_hsv` and :func:`fit_diagnostics`. Keep it with
        :func:`save_estimates`.

    Example
    -------
    >>> from euromod.eu_euromod import EURO_AREA_19, eu27_systems
    >>> table = calibrate_hsv(MODEL, DATA, eu27_systems(2022), pooled={"EA": EURO_AREA_19})
    >>> table.loc[("PT_2022", "all"), ["lambda", "tau"]]
    """
    samples = {system: prepare_sample(model_path, data_path, system, constants=constants,
                                      cache_dir=cache_dir, **sample_options)
               for system in systems}

    for name, countries in (pooled or {}).items():
        members = {system: samples[system] for system in systems if system[:2] in countries}
        absent = sorted(set(countries) - {system[:2] for system in members})
        if absent:
            raise ValueError(f"Pooled group '{name}' needs systems for {absent}.")
        samples[name] = pool(members)

    rows = []
    for name, sample in samples.items():
        for household_type, condition in household_types.items():
            households = sample if condition is None else sample.query(condition)
            fit = fit_hsv(households, lambda_fixed=lambda_fixed)
            rows.append({"system": name, "household_type": household_type,
                         **fit, **fit_diagnostics(households, fit)})
    return pd.DataFrame(rows).set_index(["system", "household_type"])
