Calibrating a tax-benefit function
==================================

A tax-benefit system has hundreds of instruments. Macroeconomic models need it in
a compact form: a function from a household's gross income to the taxes it pays,
net of the benefits it receives. `euromod.eu_euromod.calibrate` estimates such a
function from a EUROMOD simulation, for a baseline or for a reform.

```python
from euromod.eu_euromod.calibrate import prepare_sample, fit_hsv

MODEL = r"C:\EUROMOD_RELEASES_J2.0+"      # folder of the model
DATA = r"C:\EUROMOD_DATA"                 # folder with the input datasets

baseline = prepare_sample(MODEL, DATA, "PT_2022")
fit_hsv(baseline)
```
```python
{'lambda': 0.1925, 'tau': 0.1631, 'se_intercept': 0.0015, 'se_lambda': 0.0012,
 'se_tau': 0.0024, 'r2': 0.9565, 'n': 5751}
```

The numbers on this page are from one release of the model and its input data.
Yours will differ with another release.

## How it works

### The function

The function is the one of Heathcote, Storesletten and Violante (2017). For a
household with gross income {math}`y`, measured in multiples of mean income, the
average net tax rate is

```{math}
T(y) = 1 - (1 - \lambda)\, y^{-\tau}
```

- {math}`\lambda` is the **level**: the net tax rate of a household at mean income.
- {math}`\tau` is the **progressivity**: how fast the rate rises with income. With
  {math}`\tau = 0` every household pays the same rate.

Benefits are counted as negative taxes, so the rate is below zero for households
that receive more than they pay.

### The household sample

`prepare_sample` runs a system of the model and turns EUROMOD's individual-level
output into one row per household:

1. The **head** of a household is its main earner: the member with the highest
   earnings (`ils_earns`), the oldest one if earnings are equal, and the one listed
   first in the output if age is equal too.
2. Households are kept when the head is 25 to 60 years old and no member has
   self-employment income (`yse`). The self-employed face different rules, and
   their incomes are measured less reliably.
3. Incomes, taxes and benefits are summed over the members of the household.
   Gross income is original income plus replacement income (`ils_origrepy`);
   disposable income is `ils_dispy`.
4. Households with non-positive gross or disposable income, or negative taxes, are
   dropped.
5. Households below the 2.5th or above the 99th weighted percentile of gross
   income are dropped.
6. Incomes are divided by the weighted mean gross income of the households that
   remain, so that 1.0 is mean income.

Every choice in steps 2, 3 and 5 is an argument of `household_sample`, e.g.
`prepare_sample(MODEL, DATA, "PT_2022", head_age=(25, 64), trim=(1, 99))`.

### The estimator

In logs the function is a straight line,

```{math}
\log(\text{disposable income}) = \log(1 - \lambda) + (1 - \tau) \log(y),
```

which `fit_hsv` estimates by weighted least squares with the survey weights
(`dwt`). The solution is in closed form: nothing is optimised numerically, so the
result does not depend on starting values.

## Using it

### Household types

Household types are subsets of one sample. They keep its income scale, so their
estimates can be compared with each other.

```python
from euromod.eu_euromod.calibrate import HOUSEHOLD_TYPES, select_households

fit_hsv(select_households(baseline, "married"))["tau"]            # 0.1918
fit_hsv(select_households(baseline, "single_no_children"))["tau"] # 0.1309
fit_hsv(select_households(baseline, "hh_size == 1"))              # any condition
```

`HOUSEHOLD_TYPES` holds seven types: `all`, `married`, `single`, and the last two
with and without children. A household is married when its head is (`dms == 2`); a
child is a member aged 18 or under with a parent in the data.

### Several countries, and the euro area

`calibrate_hsv` does everything in one call and returns a table with one row per
system and household type:

```python
from euromod.eu_euromod import EURO_AREA_19, eu27_systems
from euromod.eu_euromod.calibrate import calibrate_hsv

table = calibrate_hsv(MODEL, DATA, eu27_systems(2022), pooled={"EA": EURO_AREA_19})
table.loc["PT_2022", ["lambda", "tau", "se_tau", "r2", "n"]]
```
```
                       lambda     tau  se_tau      r2     n
household_type
all                    0.1925  0.1631  0.0024  0.9565  5751
married                0.1783  0.1918  0.0030  0.9663  2616
single                 0.2012  0.1530  0.0036  0.9478  3135
...
```

`pooled` adds an estimate on the pooled sample of a group of countries. Each
country keeps its own income scale and its own survey weights, so it counts in
proportion to its population.

### How well the function fits

```python
from euromod.eu_euromod.calibrate import fit_diagnostics

fit = fit_hsv(baseline)
fit_diagnostics(baseline, fit)
```

| Key | Meaning |
|---|---|
| `net_tax_rate_at_mean` | Observed net tax rate of households at mean income: the data counterpart of `lambda`. |
| `rmse`, `mae` | Errors of the net tax rate, household by household. They mostly reflect the spread of households around the function. |
| `rmse_binned`, `mae_binned` | The same against mean rates in 80 income bins. They measure the shape of the function. |
| `gini_gross`, `gini_disposable` | Observed Gini coefficients. |
| `gini_fitted` | Gini of the disposable income that the fitted function assigns to the same gross incomes. |

### Describing the sample

```python
from euromod.eu_euromod.calibrate import (sample_characteristics, income_shares,
    gini_coefficients, tax_rates_by_quantile, benefit_ratio_by_quantile)

sample_characteristics(baseline)     # households, age of head, share married, ...
income_shares(baseline)              # quintile shares of gross and disposable income
tax_rates_by_quantile(baseline)      # mean net tax rate by decile and household type
benefit_ratio_by_quantile(baseline)  # means-tested benefits over gross income, by decile
```

## Using the estimates

### The published estimates

The estimates for the 27 EU member states and the euro area, policy year 2022,
ship with the package. They need neither the model nor any data:

```python
from euromod.eu_euromod.calibrate import load_estimates, schedule

baseline = load_estimates()          # one row per system and household type
baseline.loc[("PT_2022", "all"), ["lambda", "tau"]]      # 0.1925, 0.1631
baseline.xs("married", level="household_type")["tau"]    # all countries, married households
baseline.attrs                       # model release, policy year, sample definition

portugal = schedule(baseline, "PT_2022")                 # the fitted function, ready to use
portugal.net_tax_rate([0.5, 1.0, 2.0])                   # array([0.0958, 0.1925, 0.2788])
portugal.net_taxes(2.0)                                  # 0.5576: taxes net of benefits
portugal.disposable_income(2.0)                          # 1.4424
portugal.marginal_rate(2.0)                              # 0.3964
schedule(baseline, "PT_2022", "married")                 # the same for one household type
```

Incomes are in multiples of mean gross income: 2.0 is twice the mean, and a
disposable income of 1.4424 is 1.4424 times mean gross income.

The systems are `AT_2022` ... `SK_2022` and `EA`, the pooled sample of the 19
euro-area members of 2022. The columns are those of `fit_hsv` and
`fit_diagnostics`. `schedule` works on any table of estimates, including your own.

### Saving and loading your own

`calibrate_hsv` returns the same kind of table. `save_estimates` writes it to a
JSON file, with any notes you give it, and `load_estimates` reads it back:

```python
from euromod.eu_euromod.calibrate import calibrate_hsv, save_estimates, load_estimates, schedule

reform = {"$tin00_rate9": 0.58}
estimates = calibrate_hsv(MODEL, DATA, ["PT_2022"], constants=reform)
save_estimates(estimates, "pt_reform.json", constants=reform, note="top rate raised to 58%")

estimates = load_estimates("pt_reform.json")    # later, without running EUROMOD
estimates.attrs["note"]                         # 'top rate raised to 58%'
schedule(estimates, "PT_2022").disposable_income(2.0)    # used at once, like the published ones
```

### Evaluating the function

`hsv` evaluates the function with numpy:

```python
import numpy as np
from euromod.eu_euromod.calibrate import hsv, marginal_hsv

incomes = np.array([0.5, 1.0, 2.0])             # multiples of mean income
hsv(incomes, fit["lambda"], fit["tau"])           # array([0.0958, 0.1925, 0.2788])
marginal_hsv(incomes, fit["lambda"], fit["tau"])  # array([0.2433, 0.3242, 0.3964])
```

`to_calibration` turns a fit into a `Calibration` of the connector's general
calibration engine. It can be saved as JSON and gives a numba-compiled function,
which another compiled function can call:

```python
import numba
from euromod.eu_euromod.calibrate import to_calibration

cal = to_calibration(fit, system="PT_2022")
cal(incomes)                                    # array([0.0958, 0.1925, 0.2788])
cal.to_json("PT_2022_hsv.json")

schedule = cal.numba_function                   # schedule(income, lambda_, tau)

@numba.njit
def tax_bill(incomes, lambda_, tau):
    total = 0.0
    for y in incomes:
        total += y * schedule(y, lambda_, tau)
    return total
```

On arrays numpy and numba are equally fast. The compiled function matters when
the schedule is called inside a model's own compiled loop.

## Reproducing a reform

A reform is either a counterfactual system defined in the model, or a baseline
system with some constants overridden. Name a different system, or pass the
constants:

```python
reform = prepare_sample(MODEL, DATA, "PT_2022_ref")

reform = prepare_sample(MODEL, DATA, "PT_2022",
                        constants={"$tin00_rate6": 0.47, "$tin00_rate7": 0.535,
                                   "$tin00_rate8": 0.55, "$tin00_rate9": 0.58})
```

To see what a reform does to progressivity, hold the level fixed and let only
`tau` move:

```python
fit_hsv(baseline, lambda_fixed=0.192)["tau"]    # 0.1629
fit_hsv(reform, lambda_fixed=0.192)["tau"]      # 0.1733
```

This example raises the four highest income-tax rates of Portugal by ten
percentage points. With `lambda_fixed`, `tau` comes from a regression through the
origin of `log(disposable income) - log(1 - lambda)` on `log(y)`.

Three things to keep in mind:

- A baseline and a reform are only comparable on the same input dataset. A
  counterfactual system without a best-match dataset of its own needs
  `dataset="..."`.
- A constant that is defined per group takes the pair as key:
  `constants={("$f_cpi", "2023"): 1.02}`.
- The reform sample is normalised by its own mean income. A reform that changes
  gross incomes therefore also moves the income scale.

## What it needs

**Paths.** The folder of the model and the folder with the input datasets. A
dataset is looked up as `<data>/<name>.txt` and as `<data>/<CC>/<name>.txt`.
Paths are always arguments: nothing is read from the environment.

**From the model.**

| | |
|---|---|
| Variables | `idhh`, `dag`, `dms`, `dwt`, `yse`, `idmother`, `idfather` |
| Income lists | `ils_earns`, `ils_origy`, `ils_origrepy`, `ils_dispy`, `ils_taxsim`, `ils_benmt`, `ils_bennt` |
| Conventions | `dms == 2` is married; a system name starts with its two-letter country code |

If the output of a run lacks one of these, `household_sample` stops and names it.

**Packages.** numpy and pandas. Only `to_calibration` needs more: install
`euromod[calibrate]` for sympy and numba.

### Keeping simulation output

A run takes from seconds to minutes. With `cache_dir` the output is kept and
reused on the next call:

```python
baseline = prepare_sample(MODEL, DATA, "PT_2022", cache_dir="runs/baseline")
reform = prepare_sample(MODEL, DATA, "PT_2022", constants=REFORM, cache_dir="runs/reform")
```

Use one folder per scenario. Beside each output a small `.json` file records the
dataset, constants, add-ons and switches that produced it. Asking for the same
system with other settings is an error, so a stale file cannot pass for a reform.
The files hold household microdata: keep them where that data is allowed to be.

## Reference

Heathcote, J., Storesletten, K. and Violante, G. L. (2017). Optimal tax
progressivity: an analytical framework. *Quarterly Journal of Economics*, 132(4),
1693-1754.
