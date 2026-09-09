# Several shocks in one scenario

A macro model rarely projects one thing. It gives a wage path *and* a labour-market path,
and both belong in one scenario: the household-level answer to "what does this projection
mean" is the answer to both together, not the sum of two separate answers.

So a shock table may carry several channels, and nothing about the scenario document changes
when it does:

```python
scenario = {
    "country_code": "BE",
    "system_name": "BE_2025",
    "shocks": {"inline": [
        {"channel": "scale", "metric": "employment income", "group": "deh=0-3",
         "period": "1", "op": "grow", "value": 0.03},
        {"channel": "align", "metric": "inactivity_rate", "group": "deh=0-3",
         "period": "1", "op": "add", "value": -0.035},
    ]},
    "params": {"period": "1"},
}

plan = apply_scenario(system, data, scenario)
plan["methodology"]   # 'scale_variables+lma_labour_alignment'
plan["methods"]       # ['scale_variables', 'lma_labour_alignment']
```

## One channel, one method

Dispatch is **per channel**. Each channel in the table resolves to the method that consumes
it, independently of every other channel, and the scenario runs those methods one after
another. There is no method for a *combination* of channels — the combination is a property
of the scenario, and the engine composes the methods.

That is a deliberate shape. A method written for a pair of channels would need to be written
again for every other pair, and once more for each triple; and it would be a candidate for
any table carrying only one of its channels, so dispatch would have to explain why the
dedicated method still wins. Per-channel dispatch has neither problem. It also keeps a
mistake local: a mistyped `align` metric is refused by name at dispatch, whatever else the
table carries, rather than surfacing later as a complaint about some other channel.

## The order is the methods', not yours

Each method declares a **stage**, and methods run in stage order — a property of what the
method *does to the input*, so it holds for every combination without being stated for each:

`STAGE_VALUES` — `scale_variables`
: Arithmetic on what the input records. Rows and people are unchanged.

`STAGE_PEOPLE` — `lma_labour_alignment`
: Who is in which state: transitions, household weight splits.

**Values first, then people.** The values a method changes are the environment the later
transitions happen in. `lma_labour_alignment` pays a new worker their own predicted hourly
wage `yivwg` — an **input variable** — so scaling first is what makes an entrant enter at
counterfactual wages. Moving people first and scaling after would leave entrants on baseline
wages, because their earnings land in `yem_a`, which a `yem` or `ils_udb_yem` shock does not
reach: the scaling would silently miss precisely the people the alignment created.

The order you *write* the records in never matters. A shock table is sorted by
`(channel, metric, group, period)` and carries no order of its own — which is what keeps the
content id a property of the scenario rather than of how it was typed.

Methods sharing a stage are assumed to commute; within a stage they run in name order, and
that order must not matter.

## What each method sees

Each method is handed only the records of the channels it consumes, and runs on the frame the
previous method produced. `check_dataset`, income-list expansion, cell collapsing, the
op-commutativity rules, targets, scoring, weight splits and entrant earnings all behave exactly
as they do when the method runs alone — composition adds nothing to a method and takes nothing
from it.

## Params and run arguments

One scenario has one set of `params`, so every method reads the same `period`. The document is
validated against the **union** of the methods' `params_schema`, still with
`additionalProperties: false`: `tolerance_pct` is accepted when `lma_labour_alignment` runs and
refused when it does not, and a param no method declares is an error.

The add-ons and extension switches of a run are likewise the union of what the methods need.

## What the baseline is

The **untouched input**: the scenario sets up "all shocks against none".

`plan["baseline"]` — the *paired* baseline — is built by the last method that restructured
rows, on the frame it received. With scaling before alignment it therefore carries the scaling:
its job is to keep the two frames row-aligned so a fixed poverty line or baseline-defined
decile groups mean something, not to undo the earlier stages. Run it with `paired_baseline=True`
when a baseline-vs-reform statistics template will pair observations.

If you want a shock treated as *context* rather than as part of the experiment, the scenario's
`constants` are applied to both runs; for anything else, run two scenarios.

## Reading the diagnostics

One method: its diagnostics as they are, flat, exactly as documented for that method.

Several: each method's diagnostics **unflattened under its name**, with `order` naming the
sequence and `stages` saying which channels each one took:

```python
plan["diagnostics"]
# {'order': ['scale_variables', 'lma_labour_alignment'],
#  'stages': [{'method': 'scale_variables', 'stage': 10, 'channels': ['scale']},
#             {'method': 'lma_labour_alignment', 'stage': 20, 'channels': ['align']}],
#  'scale_variables': {'applied': [...], 'income_list_expansions': {...}, ...},
#  'lma_labour_alignment': {'targets': [...], 'grades': {...}, 'transitions': {...}, ...},
#  'warnings': [...],
#  'cell_population': {...}}
```

Anything you already read from a method on its own is in the same place one level down. The
top-level `warnings` is the concatenation of every method's.

## Previewing

`apply_scenario(..., validate_only=True)` previews each method in turn. A later stage is
previewed against what the earlier ones would hand it, where an earlier method declares that
its `apply()` is cheap enough to run on the validation path (`preview_by_applying`, which
`scale_variables` does): a preview that sized an alignment against the *unscaled* frame could
disagree with the run it previews. Nothing is fitted or aligned in a preview.

## Reproducing a run

The result's `methodology` is the methods in run order, joined with `+`, and it is accepted
back as the scenario's `methodology` pin:

```python
scenario["methodology"] = "scale_variables+lma_labour_alignment"
```

A pin must cover every channel in the table. The [scenario fingerprint](scenarios.md) folds
in every method's `code_fingerprint` *and their order*, so editing either method — or a change
of stage — invalidates a cached result rather than serving one computed by the earlier code.

## When not to combine

If you want the shocks in a different order, or one of them as context, chain two scenarios
and feed the first counterfactual to the second:

```python
plan_a = apply_scenario(system, data, wage_scenario)
plan_b = apply_scenario(system, plan_a["counterfactual"], alignment_scenario)
```

which makes the order and the baseline yours to state.
