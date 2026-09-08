# align_with_scaling

Income scaling and labour-market alignment in one scenario. It consumes the `scale` **and**
`align` channels together, so a macro model projecting both a nominal path and a labour-market
path goes through EUROMOD as one shock table rather than two chained runs.

## How it works

### Composition, not reimplementation

Both halves are the registered methods themselves — [`scale_variables`](scale-variables.md)
and [`lma_labour_alignment`](lma-labour-alignment.md) — called in turn. Neither changes
behaviour by being composed: income-list expansion, cell collapsing, the op-commutativity
rules, targets, scoring, weight splits and entrant earnings all behave exactly as they do
alone, and each half's diagnostics come back unflattened under `scale` and `align`.

### The order is fixed, and it is not yours

**Scale first, then align.**

That is a modelling decision, so it lives in code. A shock table is sorted by
`(channel, metric, group, period)` and carries no order of its own — the record order you write
is discarded at normalisation, which is what keeps the content id a property of the scenario
rather than of how it was typed. Reading the economics off alphabetical channel names, or off
which sheet a mapping spec happens to list first, would be exactly the order-dependence that
`scale_variables` already [refuses between two overlapping shocks](scale-variables.md).

Why this order and not the other: the wage structure is the environment the labour-market
transitions happen in. `lma_labour_alignment` pays a new worker their own predicted hourly wage
`yivwg` — an **input variable** — so scaling first is what makes an entrant enter at
counterfactual wages. Aligning first and scaling after would leave entrants on baseline wages,
because their earnings land in `yem_a`, which a `yem` or `ils_udb_yem` shock does not reach.
The scale half would silently miss precisely the people the align half created.

### What the baseline is

The **untouched input**. This method sets up "both shocks against neither".

`MethodResult.baseline` is the alignment's paired baseline — the same rows, split households
included, with every transition off — and it therefore carries the scaling. That is what
`restructures_rows` promises: its job is to keep the two frames row-aligned so a fixed poverty
line or baseline-defined deciles mean something, not to undo the scaling.

If you want the scaling treated as *context* rather than as part of the shock, put it in the
scenario's `constants`, which are applied to **both** runs, or run the two scenarios separately.

## Using it

Write both channels in one table. Nothing else changes:

```python
scenario = {
    "country_code": "BE",
    "system_name": "BE_2025",
    "shocks": {"inline": [
        {"channel": "scale", "metric": "employment income", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.03},
        {"channel": "align", "metric": "employment", "group": "deh=3-4",
         "period": "1", "op": "grow", "value": 0.05},
    ]},
    "params": {"period": "1", "tolerance_pct": 5.0},
}

plan = apply_scenario(system, data, scenario)
plan["methodology"]          # 'align_with_scaling' — resolved, not chosen
plan["diagnostics"]["order"] # ['scale', 'align']
```

Writing the `align` record first changes nothing — that is the point.

### Reading the diagnostics

```python
plan["diagnostics"]
# {'order': ['scale', 'align'],
#  'scale': {'period': '1', 'applied': [...], 'n_shocks_applied': 1,
#            'income_list_expansions': {...}, 'warnings': [...]},
#  'align': {'targets': [...], 'grades': {...}, 'transitions': {...},
#            'cells': [...], 'weight_totals': {...}, ...},
#  'warnings': [...]}
```

Each half's keys are exactly what it reports on its own, so anything you already read from
`scale_variables` or `lma_labour_alignment` is in the same place one level down. The top-level
`warnings` is the concatenation of both.

### Previewing

```python
plan = apply_scenario(system, data, scenario, validate_only=True)
plan["diagnostics"]["align"]["cells"]
```

One caveat: `validate_only` is no longer free here. The preview runs the **scaling for real** —
the arithmetic pass only, no logit fitting and no alignment — because the alignment has to be
sized against the frame it will actually see. A preview that reported cell populations and
feasibility for *unscaled* data would disagree with the run it previews.

### Running both halves

```python
out = run_scenario(system, scenario, input_path=INPUT_DIR, paired_baseline=True)
out["baseline_output"], out["counterfactual_output"]
```

`paired_baseline=True` matters here, as it does for `lma_labour_alignment` alone: the alignment
splits households, so the two frames only pair row for row when the baseline is the one the
method built.

## When not to use it

If your table carries only one channel, dispatch gives you the dedicated method and this one
never appears — that is deliberate, not a fallback. And if you want the two shocks in a
different order, or the scaling as context, chain two scenarios instead:

```python
plan_a = apply_scenario(system, data, wage_scenario)
plan_b = apply_scenario(system, plan_a["counterfactual"], alignment_scenario)
```

which makes the order and the baseline yours to state.

## What it needs

The union of the two halves: the alignment's LMA add-on and `LMA_trans` switch, its dataset
columns, and its minimum model release. Check before running:

```python
from euromod_linking import check_compatibility

check_compatibility(system, "align_with_scaling")
```

```{eval-rst}
.. method-reference:: align_with_scaling
   :no-title:
   :no-description:
```
