# lma_labour_alignment

Aligns the survey population to external employment and unemployment targets per population
cell, then drives the resulting transitions through EUROMOD's LMA add-on. It consumes the
`align` channel.

## How it works

### The problem it solves

A macro model says employment in a region rises 6.5%. A household survey has people in states
— employed, unemployed, inactive — with sample weights. Somebody has to decide **which**
people change state, because the tax-benefit consequences depend entirely on who: a
long-term inactive person entering work loses different benefits than an unemployed person
finding a job, and gains different earnings.

This method makes that decision explicitly and reports what it decided.

### Two levels, in that order

Alignment happens in two stages, mirroring the standard labour-supply decomposition: first the
**participation** margin (in or out of the labour force), then the **employment** margin
(employed versus unemployed, given participation).

```
labour force  = employment target + unemployment target   (level 1)
unemployment  = its own target within the labour force    (level 2)
employment    = labour force - unemployment               (residual)
```

Employment is never adjusted directly. Aligning in this order makes the macro accounting
identities hold exactly at every cell, and makes every implied transition interpretable:
inactive → active is participation entry, active-non-unemployed → unemployed is job loss,
unemployed → employed is job finding.

Aligning employment and unemployment separately would hit both targets while implying an
impossible participation path — someone moving straight from inactivity into employment being
counted as an unemployment exit.

### Who moves

Four logit models rank candidates. Each fits the cross-sectional probability of currently
being in a state on standard labour-supply determinants — age, gender, disability income (a
strong participation barrier), marital status — and the fitted probability is used as a
**propensity ranking**. Someone who looks like the currently employed is treated as the most
likely marginal entrant into employment.

This is deliberately an *ordering device*, not a causal transition model. Only ranks matter,
coefficients are never interpreted, and the same fit is reused for every period so period
comparisons differ only in their targets.

Ranking rather than drawing at random is what makes the distributional results meaningful:
transitions concentrate on plausible individuals, so "who gains income, whose benefits change"
reflects something other than a random number generator.

The fit depends only on the observed population, never on shocks or targets, so scores are
cached on a content hash of the input columns. A ten-period sweep pays for one set of fits,
not ten.

### Hitting a target exactly

Macro targets are continuous; people are discrete. After selecting whole individuals down the
ranking, the residual gap is almost always a *fraction* of the next candidate's weight — 40%
of the 5,000 people that person represents.

Rather than overshoot or undershoot, the boundary **household** is split: one branch carrying
40% of the weight where the person transitions, one copy carrying 60% where they do not.
Population totals are unchanged — weight is partitioned, never created. Households are split
rather than individuals because EUROMOD assesses taxes and benefits at household level.

This is why the method *restructures rows*, and therefore why it returns a matching baseline
built on the same rows rather than letting you run the untouched input. The two halves stay
observation-paired.

### What a new worker earns

Someone entering employment needs a wage before EUROMOD can tax it, but new entrants have no
observed earnings.

The standard assumption is full-time take-up at the person's **own predicted hourly wage**
(`yivwg`), twelve months a year. Weekly hours come from the country's own `$lhw` constant —
full-time is 35 hours in one country and 42 in another, so a hardcoded 40 would misstate
entrants' earnings. Where the model does not define `$lhw`, or defines it implausibly, the
method falls back to 40.

Entrants with no predicted wage — the national wage equation excludes some groups — are
matched to the five most similar continuing workers on age, gender, education, marital status
and region, and their earnings, months and hours averaged with inverse-distance weights.
Donors exclude other entrants, so imputed values never feed further imputation. Matching
rather than assigning a flat value preserves realistic wage heterogeneity; a flat assumption
would compress the bottom of the earnings distribution and mechanically overstate
redistribution.

### Who is never moved

Labour states come from `les2` where the dataset has it, `les` otherwise, over working age
18–65 inclusive. `les2` is preferred because it separates domestic-tasks inactivity from
retirement and disability, which is exactly the distinction the recruitment pool needs.

Only the **inactive** state is recruitable. Students, retirees, the long-term sick and
conscripts are classified separately and shielded — alignment never moves them, however large
a cell's gap. Without that shield a large employment target could "hire" retirees, which is
demographically absurd and would leak pension and education benefit changes into the results.

States come from `les`/`les2`, never from employment income. `yem == 0` means "no employment
income recorded" and includes children, students, pensioners, the self-employed and the
unemployed.

You can read these definitions as data, so a shock can be sized against the same population
the alignment will move:

```python
from euromod_linking.methods.lma_labour_alignment import states

states.definition("les2")
```

### Levels and rates

Metrics come in two forms. **Levels** — `employment`, `unemployment` — are counts of people.
**Rates** — `employment_rate`, `participation_rate`, `inactivity_rate` — are shares of the
cell's working-age population.

Rates exist so a request already phrased as a rate needs no conversion. Deriving a growth rate
by hand from some other population count is the classic way a shock ends up the wrong size. On
a rate metric, `add` is a percentage-point change written as a fraction: inactivity down 3.5pp
is `{"metric": "inactivity_rate", "op": "add", "value": -0.035}`.

The denominator is the cell's working-age population, which alignment leaves unchanged, so
before and after rates are directly comparable.

Note that `inactivity_rate` is the conventional complement of the participation rate —
everyone of working age who is neither employed nor unemployed, students and pensioners
included. That is wider than the pool the method can actually move, which is a feasibility
question rather than a definitional one, and the diagnostics report it as such.

A cell may not fix both the labour force and employment, since that would also fix
unemployment. Such a scenario is rejected.

### Anchoring on the survey's own baseline

A growth-rate shock becomes `target = current weighted count × (1 + rate)`. Only the external
model's projected *change* is imposed. Any level disagreement between the macro model's
employment stock and the weighted survey count — different definitions, different reference
periods — is deliberately left alone rather than being silently corrected into the microdata.

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
        {"channel": "align", "metric": "employment", "group": "region=21;deh=3-4",
         "period": "1", "op": "grow", "value": 0.0655},
        {"channel": "align", "metric": "unemployment", "group": "region=21;deh=3-4",
         "period": "1", "op": "grow", "value": -0.02},
    ]},
    "params": {"period": "1", "tolerance_pct": 5.0},
}
```

### Check the model supports it first

This method needs the LMA add-on and the `LMA_trans` extension switch, which older EUROMOD
releases do not ship. Ask before running:

```python
from euromod_linking import check_compatibility

print(check_compatibility(system, "lma_labour_alignment"))
```

An unmet requirement is also caught inside `apply_scenario`, including under
`validate_only=True`, so a scenario that cannot work is rejected before anything expensive
happens. See [Model compatibility](../concepts/compatibility.md).

### Look at the plan before paying for it

```python
plan = apply_scenario(system, data, scenario, validate_only=True)
plan["diagnostics"]["cell_population"]
```

This stops before the alignment itself and reports what the shocks resolve to: the per-cell
baseline population and its labour states, the resulting targets, the implied change in people
and in percentage points, and feasibility against the pool of people who can actually move.
Checking that a shock is the size you meant costs nothing here and two simulations later.

### Apply it

```python
plan = apply_scenario(system, data, scenario)

plan["counterfactual"]   # transformed input
plan["baseline"]         # matched baseline on the same rows — run this, not `data`
plan["diagnostics"]
```

Because weight splitting adds rows, `plan["baseline"]` is **not** the input you passed in.
Run the returned baseline, or the two simulations will not be observation-paired.

### Reading the diagnostics

```python
d = plan["diagnostics"]

d["transitions"]   # weighted people AND raw rows, per transition type
d["targets"]       # target vs achieved, per cell
d["grades"]        # per-cell alignment grade
d["cells"]         # the baseline each cell was measured from
d["unfillable_gaps"]        # targets the recruitable pool could not reach
d["n_cells_over_tolerance_pct"]
```

Two things about this report are deliberate.

Transitions are reported as **weighted people alongside raw row counts**, because they answer
different questions and are easy to confuse. Rows tell you whether a result rests on a handful
of observations; people is the population quantity that belongs in an answer. After weight
splitting a single row can stand for a fraction of a person, so the weighted count is the one
that matches the target.

Every run reports **the baseline it worked from**, not just what it achieved. An alignment
grade says only that the target was hit — never that the target was right — so the baseline
population sits beside it. A shock that was mis-sized produces a perfect grade against a
target that was wrong, and only the baseline reveals that.

### Running both halves

```python
from euromod_linking import run_scenario

out = run_scenario(system, scenario, input_path=INPUT_DIR)
```

The baseline runs **with the LMA add-on active** but with no transitions flagged, rather than
with the add-on switched off. Both runs then pass through an identical policy configuration,
so the difference between them isolates the labour-market transitions and cannot pick up
incidental differences from toggling the add-on itself.

If the transform changed people but the two outputs are identical, `run_scenario` raises
`NoEffectError`. That is not "the reform has no impact" — it means the engine never acted on
the transformation.

## What it needs

The LMA add-on with its country system, and the `LMA_trans` extension switch. It injects four
columns the add-on consumes: `lma`, `yem_a`, `yemmy_a`, `lhw_a`.

```{eval-rst}
.. method-reference:: lma_labour_alignment
   :no-title:
   :no-description:
```
