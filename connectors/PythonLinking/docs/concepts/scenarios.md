# Scenario documents

A scenario binds shocks to a country and system. It is a plain dict, validated against a
JSON Schema shipped with the package (`schemas/scenario.v1.schema.json`), and it is
deliberately thin: it selects *data*, never modelling.

```python
scenario = {
    "country_code": "BE",
    "system_name": "BE_2025",
    "shocks": {"inline": [
        {"channel": "align", "metric": "employment", "group": "deh=0-3",
         "period": "1", "op": "grow", "value": 0.0655},
    ]},
    "params": {"period": "1"},
}
```

## Where shocks come from

`shocks` takes exactly one of two forms:

`{"inline": [...]}`
: Records written out in the document, as above.

`{"file": "projections.xlsx", "mapping": "regional_projections"}`
: An external model's output file, read through a declarative [mapping spec](ingest.md).

Both are reproducible: the resulting table's id is derived from its contents, so the same
shocks give the same `shock_table_id` whichever form supplied them. The result reports that
id under `plan["shock_table_id"]`.

## Two kinds of constants, and the difference matters

This is the part that is easy to get wrong, so the package refuses the ambiguous case.

**Scenario `constants`** are *context*. They apply to **both** runs:

```python
scenario["constants"] = [{"name": "$f_cpi", "group": "2023", "value": 1.058}]
```

Use them for the things that are true of the world you are simulating in, not of the
reform: an uprating factor, a price level. Because both halves get them, the difference
between the runs still isolates the shock.

**Shock records with `channel: constant`** are *part of the shock*. They apply to the
**counterfactual only** — `metric` is the constant name, `period` is the constant group,
and `op` must be `set`.

A constant that appears in both places is rejected rather than silently resolved, since the
two readings give opposite answers about what the baseline is.

A scenario whose shocks are *all* `channel: constant` needs no methodology at all: the input
microdata is untouched and the two runs differ only in their overrides.

## params

`params` carries scenario-semantics parameters, validated against the resolved
methodology's own `params_schema` with `additionalProperties: false` — so a typo is an
error, not a silently ignored setting. Every shipped methodology takes `period`, selecting
which of the external model's periods to apply; `lma_labour_alignment` also takes
`tolerance_pct`. The full list per methodology is in the
[Methods](../methods/index.md) section. When a scenario runs
[several methods](composition.md), it is validated against the union of their schemas — one
scenario has one set of params, and every method reads the same `period`.

A constants-only scenario takes no params.

## methodology

There is a `methodology` field, and you should normally leave it out. It exists to pin a
run for exact reproduction, or to disambiguate on the day two methodologies claim the same
channel. Under normal use each shock channel is resolved to its method and the result echoes
the reference back to you — `lma_labour_alignment`, or
`scale_variables+lma_labour_alignment` when several ran, in run order — see
[Methods](../methods/index.md) for why. A pin takes the same form and must cover every channel
in the table.

## Validating cheaply

`check_scenario(scenario)` validates everything that needs neither the model nor the data:
structure, shock resolution, methodology dispatch, declared params, and the methodology's
contract. It raises `ScenarioError` carrying `.problems`, a list of every problem found
rather than the first.

```python
from euromod_linking.scenarios import check_scenario

try:
    check_scenario(scenario)
except ScenarioError as e:
    for problem in e.problems:
        print(problem)
```

The next step up is `apply_scenario(..., validate_only=True)`, which additionally consults
the model and the data — cell populations, income-list expansion, model
[compatibility](compatibility.md), and the methodology's own preview of what it would
target — and still transforms nothing. That preview is what makes a mis-sized shock visible
while it is still cheap to fix.

## Fingerprints

`scenario.fingerprint()` reduces a scenario to a canonical hash. It folds in the shock
table's content id *and* the resolved methods' `pipeline_fingerprint` — a hash of each
method's own source, in run order. So editing the modelling, or a change in which methods run
or in which order, invalidates cached results instead of serving answers computed by a
previous version of it.
