# scale_variables

Cell-level arithmetic on numeric input variables: wage changes, hours shocks, any income
component adjustment. It consumes the `scale` channel.

## How it works

### The price and income channel

A macro model projects nominal paths — wage growth by region, an hours adjustment, a change in
investment income. This method applies them **heterogeneously across population cells**,
which is the entire point of pushing them through a microsimulation.

A uniform +3% wage change does not have a uniform effect on disposable income. Tax brackets,
benefit withdrawals and means tests bite differently along the distribution, so the same
percentage at the top and the bottom produces quite different net outcomes. EUROMOD supplies
exactly that pass-through when the scaled inputs are simulated.

### What a shock names

The `metric` is an **input variable** — `yem`, `lhw`, `yiy` — an **EUROMOD income list** such
as `ils_udb_yem`, or the **descriptive name** of one: `"employment income"`.

Income lists matter because an economic concept like "employment income" is not one variable.
Scaling the list scales every component the model itself counts under that concept, so the
shock stays consistent with the model's own accounting. Expansion is **extension-aware**: it
walks the model's `DefIl` definitions at run time, so a list stays correct when an extension
changes what it contains.

Lists accept `mult` and `grow` only. A proportional factor distributes exactly over a sum; an
absolute `add` or `set` on an aggregate has no unique per-component allocation, so it is
rejected rather than resolved arbitrarily.

### Naming a concept instead of a list

`ils_udb_yem` is the model's name for employment income, not anybody's. So a `scale` metric may
also be written as the concept:

```python
{"channel": "scale", "metric": "employment income", "group": "deh=3-4",
 "period": "1", "op": "grow", "value": 0.03}
```

The names come from the model itself — each list's own `DefIl` comment, taken as the majority
spelling across the 27 country files — plus the other comments the model uses for the same list
elsewhere and the short forms an analyst is likely to type. Matching ignores case, underscores,
hyphens and punctuation, so `Employment income`, `employment_income` and `EMPLOYMENT INCOME` are
one name. The table below lists every accepted spelling.

Resolution happens during **normalisation**, the way group keys are canonicalised there, and it
has three consequences worth knowing:

- The shock table always holds `ils_udb_yem`, whichever spelling produced it. So the content id
  identifies the *scenario*, not how it was written — `"employment income"` and `ils_udb_yem`
  give the same `shk_` id and hit the same cache entry.
- Writing both spellings in one table is caught as a duplicate shock, not applied twice.
- A misspelt concept fails at normalisation, naming the closest matches, **before** a model is
  loaded — rather than surfacing later as "not a column of the input dataset".

A metric with no whitespace is never touched: `yem` stays the raw variable, and only the raw
variable. Concept names are the way to reach the list.

### The income concepts you can name

These are EUROMOD's User Database output lists. The **names** are standardised across the EU-27
countries; their **membership** is not — it is country- and extension-specific, which is why a
list is resolved against the live model at run time rather than tabulated here. A name a given
system does not define fails with the list of names it does: the nine benefit lists are defined
in 25 of the 27 countries, the rest in all 27.

The table is generated from the package's own catalogue, so the names and the accepted spellings
here are the ones the code will accept.

```{eval-rst}
.. income-lists::
```

### Simulated components, and why they are skipped

A variable whose name ends in `_s` is a **simulated output**: EUROMOD computes it during the
run from the input microdata and the policy rules. It is not a column of the input, so there
is nothing to scale — and even if there were, the engine would overwrite it on the next run.

This is the difference between the groups above. Market-income lists resolve to *data-reported*
variables: across all 27 countries `ils_udb_yiy`, `ypr`, `ypp`, `ypt`, `yot`, `kfbcc` and `xmp`
resolve to no simulated component at all, and `yem` and `yse` to three apiece against 26 and 23
reported ones. `ils_udb_yem` is employment income as the survey recorded it, and scaling it
changes what the model is given.

Benefit and tax lists resolve largely to what the model *produces*. `ils_udb_tis` is tax on
income and social security contributions, and carries **one** reported component against 121
simulated ones — so scaling it moves almost nothing, whatever the factor.

The package refuses to pretend otherwise, in three steps:

1. **The split is reported, not hidden.** Expansion compares the list's components against the
   input's own columns and returns `{"scaled": [...], "skipped_not_in_input": [...]}`. What you
   see is what will move.
2. **A partial list warns.** `ils_udb_bun` resolves to `bun`, `bun_s`, `byr` and more — a
   mixture. Scaling it shifts the recorded part while the simulated part is recomputed from
   unchanged rules, which is almost never what was meant, so the warning names every skipped
   component and says so.
3. **A fully simulated list is rejected.** If nothing a list resolves to is in the input,
   the scenario fails rather than running a shock that provably does nothing.

The same split is computed by `preview()` and by `apply()`, from the same columns, so the
validation report and the run cannot disagree about what was scaled.

**To change what a benefit or tax pays out, shock its policy parameters through the
`constant` channel instead.** That changes the rules the engine applies, which is the thing
that actually determines a simulated amount. Scaling the output of a calculation cannot
change the calculation.

### How shocks compose

Rows are matched per shock by its own group keys. Shocks of different granularity may coexist:
a national wage shock and a regional one both apply to someone caught by both, and their
effects compound.

Mixing ops in one table is fine. The rule is about **overlap**, and it is one line: two shocks
may touch the same person on the same metric only when their ops commute.

`mult` and `grow` — *proportional*
: Commute with each other. Both multiply, so `x × m × (1+g)` is the same either way round.

`add` — *additive*
: Commutes with `add`. Does **not** commute with `mult`/`grow`: `(x + a) × m` is not
  `x × m + a`.

`set` — *absolute*
: Commutes with nothing, not even another `set` — the later one would simply overwrite the
  earlier.

An overlap across families is **rejected**, naming both shocks. Without that rule the answer
would depend on which cell sorted first alphabetically, which is no basis for an economic
result. Non-overlapping cells may use any ops they like.

Region shocks finer than the dataset collapse to the level it supports, with growth rates
averaged. See [population cells](../concepts/population-cells.md).

### What it does not do

No rows are added or removed and no weights change, so the baseline is simply the untouched
input. The transform is purely arithmetic and deterministic — the same inputs always give the
same output.

## Using it

### A scenario

```python
import pandas as pd
from euromod_linking import apply_scenario
from euromod import Model

system = Model(MODEL_PATH)["BE"]["BE_2025"]
data = pd.read_csv(data_file, sep="\t")

scenario = {
    "country_code": "BE",
    "system_name": "BE_2025",
    "shocks": {"inline": [
        # Employment income up 3% for medium education.
        {"channel": "scale", "metric": "yem", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.03},
        # Investment income down 1% everywhere.
        {"channel": "scale", "metric": "yiy", "group": "",
         "period": "1", "op": "grow", "value": -0.01},
    ]},
    "params": {"period": "1"},
}

plan = apply_scenario(system, data, scenario)
counterfactual = plan["counterfactual"]
```

`plan["methodology"]` reads back `"scale_variables"` — the dispatch that was resolved, not
one you asked for.

### Scaling an income list

```python
scenario["shocks"] = {"inline": [
    {"channel": "scale", "metric": "ils_udb_yem", "group": "region=21",
     "period": "1", "op": "grow", "value": 0.05},
]}

plan = apply_scenario(system, data, scenario, validate_only=True)
plan["diagnostics"]["income_list_expansions"]
# {'ils_udb_yem': {'scaled': ['yem'], 'skipped_not_in_input': []}}
```

The expansion report is worth reading before a full run: it names exactly which input
variables the list resolved to for this country and system, and which components were skipped
because the model simulates them.

The same shock can be written as the concept, which normalises to exactly the same table:

```python
scenario["shocks"] = {"inline": [
    {"channel": "scale", "metric": "employment income", "group": "region=21",
     "period": "1", "op": "grow", "value": 0.05},
]}

plan = apply_scenario(system, data, scenario, validate_only=True)
plan["shocks"]["metrics"]
# ['ils_udb_yem']
```

To see the catalogue of standardised lists, what each covers and every spelling that reaches it:

```python
from euromod_linking.methods.scale_variables import income_lists

income_lists()["lists"]["market income"]
# {'ils_udb_yem': 'Employment income', 'ils_udb_yse': 'Self-employment income', ...}

income_lists()["aliases"]["ils_udb_yem"]
# ['Employment income', 'earnings', 'wages and salaries', 'employee income', ...]
```

`income_lists()["groups"]` carries the same thing with the per-list notes — which lists are
worth scaling, and why `ils_udb_yds` is not one of them.

### Checking the size of a shock first

```python
plan = apply_scenario(system, data, scenario, validate_only=True)
plan["diagnostics"]["cell_population"]
# {'deh=3-4': {'n_rows': 5460, 'n_weighted_all_ages': 8813907.8}}
```

That reports how many people each cell actually contains, so a group that matches nobody — a
typo, or a variable this dataset does not carry — is visible before anything is transformed.

### Running both halves

```python
from euromod_linking import run_scenario

out = run_scenario(system, scenario, input_path=INPUT_DIR)
out["baseline_output"], out["counterfactual_output"]
```

Because this method leaves the row structure alone, the baseline is the untransformed input
and the two outputs are directly comparable row for row.

## What it needs

Nothing from the model beyond the system itself — no add-ons, no extension switches — so it
runs against any EUROMOD release. Its dataset requirements are just the identifiers and the
weight.

```{eval-rst}
.. method-reference:: scale_variables
   :no-title:
   :no-description:
```
