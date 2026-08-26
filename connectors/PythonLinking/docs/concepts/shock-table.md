# The shock table

Every external model says something different. One publishes employment growth by region
and education class; another publishes a wage path; an analyst writes down "unemployment
falls 2 points in the north". Before any of that can drive a EUROMOD run it is normalised
into one table. Everything downstream — dispatch, validation, caching, reproducibility —
reads that table and nothing else.

One tidy record per shock, eight columns:

```{eval-rst}
:``channel``: Which EUROMOD lever the shock pulls. ``align`` moves people between states,
   ``scale`` multiplies an input variable, ``constant`` overrides a model parameter.
   (``reweight`` and ``inject`` are reserved; no methodology consumes them yet.) The
   channel is what selects the methodology — see :doc:`/methods/index`.
:``metric``: What the lever acts on, interpreted per channel: ``employment`` or
   ``unemployment`` for ``align``, an input variable (``yem``) or an income list
   (``ils_udb_yem``) for ``scale``, a constant name (``$f_cpi``) for ``constant``.
:``group``: Which people. A population cell over real EUROMOD input variables,
   ``"deh=3-4;dgn=1"``, or ``""`` for everyone. See :doc:`population-cells`.
:``period``: The external model's own period label, as a string. A shock table usually
   carries a whole path; the scenario's ``params.period`` picks the one to apply.
:``op``: How the value applies — ``set``, ``grow``, ``mult`` or ``add``.
:``value``: The number.
:``unit``: Optional, documentation only.
:``source``: Optional provenance — file, sheet, row — carried through so a number in a
   diagnostic can be traced back to the cell it came from.
```

## The four operations

`set` writes the value, `add` adds it, `mult` multiplies by it, and `grow` multiplies by
`(1 + value)` — so a 6.55% employment increase is `{"op": "grow", "value": 0.0655}`.

The distinction between `grow` and `add` matters most on rate metrics, where a request is
already expressed as a share. There, `add` is a percentage-point change written as a
fraction: inactivity down 3.5pp is `{"metric": "inactivity_rate", "op": "add", "value": -0.035}`.

Two `set` shocks whose cells overlap are rejected rather than resolved, because there is no
defensible order in which to apply them. Overlapping `mult`/`grow`/`add` shocks compose, in
sorted order.

## Content-addressed

`normalize()` validates and canonicalises records, then sorts them by
`(channel, metric, group, period)`. The sorted, canonical form is hashed into an id of the
form `shk_` plus twelve hex digits:

```python
from euromod_linking import normalize_shocks
from euromod_linking.shock_table import content_id, summarize

records = [
    {"channel": "align", "metric": "employment", "group": "deh=0-3",
     "period": "1", "op": "grow", "value": 0.0655},
]
table = normalize_shocks(records)
content_id(table)      # 'shk_...' — the same for any input that means the same thing
```

Because the id is derived from content rather than assigned on arrival, the same economic
scenario always has the same id no matter which file, mapping spec or code path produced
it. Two runs that claim to be the same scenario can be checked, and a result cache cannot
serve one scenario's answer for another's question.

Group syntax is validated here, but *not* whether a key is a real column of your dataset —
that check needs the data and happens later, in `apply_scenario`. A well-formed
`planet=3` passes normalisation and fails when it meets the microdata.

## Nothing is stored

A table is a DataFrame you hold; the id is computed from it on demand. The package writes
no files and keeps no registry, so normalising shocks has no side effects and leaves nothing
behind in your working directory.

That is enough for the job the id does. Reproducibility comes from the id being *derived*
from the content: two tables that mean the same thing hash the same whether or not either
was ever saved. Wanting to keep a table is an ordinary data-handling question — hold the
DataFrame, or write it wherever the rest of your analysis lives.

`content_id()` gives the id, `summarize()` renders a table compactly for a diagnostic, and
`describe()` combines the two into the form that appears on a scenario result.
