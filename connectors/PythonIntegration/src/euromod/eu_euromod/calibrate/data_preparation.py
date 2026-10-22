"""
From a EUROMOD run to the household sample the tax-benefit function is fitted on.

Two steps, each a function:

:func:`simulate`
    Runs one system of the EU model on its input dataset and returns EUROMOD's
    individual-level output.

:func:`household_sample`
    Turns that output into one row per household:

    1. The *head* of a household is its main earner: the member with the highest
       earnings (``ils_earns``), the oldest one if earnings are equal. If age is
       equal too, the member listed first in the output.
    2. Households are kept when the head's age is within ``head_age`` and, by
       default, when no member has self-employment income (``yse``). The
       self-employed face different rules and their incomes are measured less
       reliably.
    3. Incomes, taxes and benefits are summed over the members of a household.
    4. Households with non-positive gross income, original income, disposable
       income or income after taxes are dropped. Households whose taxes
       (``ils_taxsim``) are negative in total are kept: refundable tax credits
       are support like a benefit, and dropping their recipients would make the
       sample depend on whether a country pays its support through the tax
       system. ``drop_negative_tax=True`` drops them, as version 0.4 did.
    5. The bottom and top of the gross-income distribution are trimmed at weighted
       percentiles (``trim``).
    6. Incomes are divided by the weighted mean gross income of the households
       that remain, so that 1.0 is mean income. The mean is taken over *all*
       remaining households: subgroups such as married households are selected
       afterwards and share the same scale, which keeps their estimates comparable.

:func:`prepare_sample` does both in one call.

What this module needs from the model
-------------------------------------
Variables ``idhh, dag, dms, dwt, yse, idmother, idfather`` and income lists
``ils_earns, ils_origy, ils_dispy, ils_taxsim, ils_benmt, ils_bennt``, plus the
income list chosen as gross income (default ``ils_origrepy``). A married head is
``dms == 2``. A child is a member aged 18 or under with a parent in the data. A
system name starts with its two-letter country code (``PT_2022``).

Paths are always arguments. Nothing is read from the environment.
"""
import json
import logging
import os

import pandas as pd

from .weighted import weighted_percentile

__all__ = ["simulate", "household_sample", "prepare_sample"]

logger = logging.getLogger(__name__)

_REQUIRED = ["idhh", "dag", "dms", "dwt", "yse", "idmother", "idfather",
             "ils_earns", "ils_origy", "ils_dispy", "ils_taxsim",
             "ils_benmt", "ils_bennt"]

_models = {}          # loaded models by path; loading one is slow


def _model(model_path):
    path = os.path.normpath(model_path)
    if path not in _models:
        from euromod import Model
        _models[path] = Model(path)
    return _models[path]


def _dataset_file(data_path, country, dataset):
    """``<data>/<dataset>.txt`` or ``<data>/<CC>/<dataset>.txt``."""
    for folder in (data_path, os.path.join(data_path, country)):
        path = os.path.join(folder, dataset + ".txt")
        if os.path.exists(path):
            return path
    raise FileNotFoundError(
        f"Dataset '{dataset}.txt' not found in '{data_path}' or its '{country}' subfolder.")


def _run_settings(dataset, constants, addons, switches):
    return {"dataset": dataset,
            "constants": {"|".join(key): value for key, value in sorted(constants.items())},
            "addons": [list(a) if isinstance(a, tuple) else a for a in addons],
            "switches": [list(s) for s in switches]}


def simulate(model_path, data_path, system, *, dataset=None, constants=None,
             addons=(), switches=(), cache_dir=None):
    """
    Run one system of the EU model and return EUROMOD's individual-level output.

    Parameters
    ----------
    model_path : :obj:`str`
        Folder of the EUROMOD model.
    data_path : :obj:`str`
        Folder with the input datasets (``<dataset>.txt``, directly in the folder
        or in one subfolder per country).
    system : :obj:`str`
        Name of the system, e.g. ``"PT_2022"``, or of a counterfactual system
        defined in the model, e.g. ``"PT_2022_ref"``.
    dataset : :obj:`str`, optional
        Name of the input dataset. Default is the system's best-match dataset. A
        baseline and a counterfactual are only comparable on the same dataset, so
        pass it when the counterfactual system has no best match of its own.
    constants : :obj:`dict`, optional
        Model constants to override for this run, e.g. ``{"$tin00_rate9": 0.58}``.
        For a constant defined per group, use the pair as key:
        ``{("$f_cpi", "2023"): 1.02}``.
    addons, switches : list, optional
        Passed to :meth:`euromod.System.run`.
    cache_dir : :obj:`str`, optional
        Folder in which to keep the output and reuse it on the next call. Use one
        folder per scenario: beside each output a small ``.json`` records the
        dataset, constants, add-ons and switches that produced it, and asking for
        the same system with different ones is an error.

    Returns
    -------
    pandas.DataFrame
        One row per individual.

    Example
    -------
    >>> output = simulate(MODEL, DATA, "PT_2022")
    >>> reform = simulate(MODEL, DATA, "PT_2022", constants={"$tin00_rate9": 0.58})
    """
    country = system[:2]
    constants = {(key, "") if isinstance(key, str) else tuple(key): str(value)
                 for key, value in (constants or {}).items()}

    if cache_dir:
        cached, record = (os.path.join(cache_dir, system + ext) for ext in (".pkl", ".json"))
        if os.path.exists(cached) and os.path.exists(record):
            with open(record, encoding="utf-8") as file:
                stored = json.load(file)
            asked = _run_settings(dataset or stored["dataset"], constants, addons, switches)
            if stored != asked:
                raise ValueError(
                    f"'{cached}' was produced with different settings ({stored}) than "
                    f"requested ({asked}). Use another cache_dir for this scenario.")
            return pd.read_pickle(cached)

    countries = _model(model_path).countries
    try:
        model_system = countries[country].systems[system]
    except Exception:
        available = [s.name for s in countries[country].systems]
        raise ValueError(f"System '{system}' not found. Systems of {country}: {available}") from None

    if dataset is None:
        best_match = model_system.bestmatch_datasets
        if len(best_match) == 0:
            raise ValueError(f"System '{system}' has no best-match dataset; pass dataset=.")
        dataset = best_match[0].name

    data = pd.read_csv(_dataset_file(data_path, country, dataset), sep="\t")
    output = model_system.run(data, dataset, constantsToOverwrite=constants,
                              addons=list(addons), switches=list(switches),
                              verbose=False).outputs[0]

    if cache_dir:
        os.makedirs(cache_dir, exist_ok=True)
        output.to_pickle(cached)
        with open(record, "w", encoding="utf-8") as file:
            json.dump(_run_settings(dataset, constants, addons, switches), file, indent=2)
    return output


def household_sample(output, *, income="ils_origrepy", disposable="ils_dispy",
                     head_age=(25, 60), trim=(2.5, 99), drop_self_employed=True,
                     drop_negative_tax=False):
    """
    Build the household sample from EUROMOD's individual-level output.

    The steps are described at the top of this module.

    Parameters
    ----------
    output : pandas.DataFrame
        Individual-level output of a EUROMOD run, e.g. from :func:`simulate`.
    income : :obj:`str`
        Income list used as gross income. Default is original income plus
        replacement income, ``ils_origrepy``.
    disposable : :obj:`str`
        ``"ils_dispy"``, or ``"ils_dispy_sicee"`` for disposable income before
        employee social insurance contributions (``ils_dispy + ils_sicee``).
    head_age : (int, int)
        Youngest and oldest age of the head, both included.
    trim : (float, float)
        Weighted percentiles of household gross income below and above which
        households are dropped.
    drop_self_employed : :obj:`bool`
        Drop households in which any member has self-employment income.
    drop_negative_tax : :obj:`bool`
        Drop households whose taxes (``ils_taxsim``, summed over the members) are
        negative. Off by default; version 0.4 always dropped them.

    Returns
    -------
    pandas.DataFrame
        One row per household, sorted by ``idhh``, with

        - ``dwt``, ``hh_size``, ``head_age``, ``dms``, ``married``, ``n_children``;
        - the household totals of the income lists (``ils_origrepy``,
          ``ils_dispy``, ``ils_benmt``, ..., and ``ils_ben`` and ``ils_sicee`` when
          the output has them) and ``posttax_income`` (gross income minus
          ``ils_taxsim``);
        - ``income_norm`` and ``disposable_norm``: gross and disposable income
          divided by mean gross income;
        - ``net_tax_rate = 1 - disposable / income`` (taxes net of benefits) and
          ``tax_rate = 1 - posttax_income / income`` (taxes only).

        ``sample.attrs["mean_income"]`` holds the mean used for normalising.

    Example
    -------
    >>> sample = household_sample(simulate(MODEL, DATA, "PT_2022"))
    >>> married = sample.query("married")
    """
    needed = _REQUIRED + [income] + (["ils_sicee"] if disposable == "ils_dispy_sicee" else [])
    missing = [name for name in needed if name not in output.columns]
    if missing:
        raise ValueError(f"The EUROMOD output lacks {missing}. This module needs the "
                         "variables and income lists of the JRC EU model.")

    df = output.copy()
    df["posttax_income"] = df[income] - df["ils_taxsim"]
    if disposable == "ils_dispy_sicee":
        df["ils_dispy_sicee"] = df["ils_dispy"] + df["ils_sicee"]
    logger.info("input: %d individuals, %d households", len(df), df["idhh"].nunique())

    # Head = main earner. The sort is stable, so full ties go to the member listed first.
    earners = df[df["ils_earns"] >= 0]
    heads = (earners.sort_values(["ils_earns", "dag"], ascending=[False, False])
                    .groupby("idhh").head(1))
    in_age = heads.loc[heads["dag"].between(*head_age), "idhh"]
    df = df[df["idhh"].isin(in_age)]
    logger.info("head aged %d-%d: %d households", *head_age, df["idhh"].nunique())

    if drop_self_employed:
        self_employed = df.loc[df["yse"] != 0, "idhh"].unique()
        df = df[~df["idhh"].isin(self_employed)]
        logger.info("without self-employment income: %d households", df["idhh"].nunique())

    totals = [income, "ils_origy", "ils_dispy", "ils_taxsim", "ils_benmt", "ils_bennt",
              "posttax_income"]
    totals += [name for name in ("ils_ben", "ils_sicee", "ils_dispy_sicee") if name in df.columns]
    totals = list(dict.fromkeys(totals))

    is_child = (df["dag"] <= 18) & ((df["idmother"] != 0) | (df["idfather"] != 0))
    members = df.groupby("idhh")
    hh = members[totals].sum()
    hh["dwt"] = members["dwt"].first()
    hh["hh_size"] = members.size()
    hh["n_children"] = is_child.groupby(df["idhh"]).sum()

    hh = hh[(hh[income] > 0) & (hh["ils_origy"] > 0) & (hh["ils_dispy"] > 0)
            & (hh["posttax_income"] > 0)]
    if drop_negative_tax:
        hh = hh[hh["ils_taxsim"] >= 0]
    low, high = (weighted_percentile(hh[income].to_numpy(), hh["dwt"].to_numpy(), p)
                 for p in trim)
    hh = hh[(hh[income] >= low) & (hh[income] <= high)].copy()
    logger.info("positive incomes, trimmed to [%.2f, %.2f]: %d households", low, high, len(hh))

    heads = heads.set_index("idhh")
    hh["head_age"] = heads["dag"]
    hh["dms"] = heads["dms"]
    hh["married"] = hh["dms"] == 2

    mean_income = (hh[income] * hh["dwt"] / hh["dwt"].sum()).sum()
    hh["income_norm"] = hh[income] / mean_income
    hh["disposable_norm"] = hh[disposable] / mean_income
    hh["net_tax_rate"] = 1 - hh[disposable] / hh[income]
    hh["tax_rate"] = 1 - hh["posttax_income"] / hh[income]

    hh = hh.reset_index()
    hh.attrs["mean_income"] = float(mean_income)
    return hh


def prepare_sample(model_path, data_path, system, *, dataset=None, constants=None,
                   addons=(), switches=(), cache_dir=None, **sample_options):
    """
    Run a system and build its household sample: :func:`simulate` followed by
    :func:`household_sample`.

    Parameters
    ----------
    model_path, data_path, system, dataset, constants, addons, switches, cache_dir
        As in :func:`simulate`.
    **sample_options
        ``income``, ``disposable``, ``head_age``, ``trim``, ``drop_self_employed``,
        ``drop_negative_tax``, as in :func:`household_sample`.

    Returns
    -------
    pandas.DataFrame
        One row per household.

    Example
    -------
    >>> baseline = prepare_sample(MODEL, DATA, "PT_2022")
    >>> reform = prepare_sample(MODEL, DATA, "PT_2022_ref")
    """
    output = simulate(model_path, data_path, system, dataset=dataset, constants=constants,
                      addons=addons, switches=switches, cache_dir=cache_dir)
    return household_sample(output, **sample_options)
