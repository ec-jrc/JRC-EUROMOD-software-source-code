# Population cells

A shock's `group` says which people it applies to. It is a canonical
`"key=value;key=value"` string, and an empty string means everyone:

```
""                    every row
"dgn=1"               men
"deh=3-4"             medium education
"deh=3-4;dgn=1"       men with medium education
"region=BE10"         the Brussels region, and everything under it
```

## Keys are real EUROMOD variables

A key is either a EUROMOD **input variable** — `deh`, `dgn`, `dag`, `les` — or `region`,
the one named dimension with structure of its own.

That restriction is deliberate. Recoded category labels are meaningless outside the binding
that invented them. An external model's own "low / medium / high skill" bands cannot be
looked up anywhere, cannot be verified against a dataset, and silently fix an aggregation the
analyst may not have wanted. `deh=3-4` is self-describing, checkable against the data, and
needs no registry entry to interpret. An external model that publishes its own classes
translates them to variable ranges in its [mapping spec](ingest.md), which is where
model-specific semantics belong.

Category names an external model might reach for are recognised well enough to produce a
pointed error rather than a generic one: `skill` points you at `deh`, `gender` at `dgn`,
`age_band` at `dag`.

## Value specs

```
deh=3         exact value
deh=3-4       inclusive range — ddi=-1-0 parses, signs are handled
dag=65+       open upper bound
deh=3,5       set of values
```

Non-numeric values compare as strings, so only exact matches and sets apply to them.

Groups are canonicalised on their way into the shock table — keys sorted, whitespace
removed — so `"region=12;deh=3-4"` and `"deh=3-4;region=12"` are one cell with one id, not
two.

## Regions are NUTS-aware

`region` resolves to the finest regional column the dataset actually carries: `drgn2` if
present, otherwise `drgn1`, otherwise the whole country. Around that it understands the
NUTS hierarchy in both directions.

A **coarser** shock code covers its subregions: a shock on `region=BE1` applies to everyone
in `BE10`. A **finer** shock code collapses to the level the data supports — if the
external model publishes NUTS-2 rates but the dataset only has NUTS-1, the constituent
rates are averaged rather than the shock being dropped or, worse, matched against nothing.

This is the one place where the package resolves a mismatch instead of refusing it, because
the alternative — requiring every external model to publish at the survey's regional
resolution — would rule out most real pairings.

## Checking a cell before you use it

`validate_group_syntax` checks a group is well formed; `validate_group_columns` checks its
keys exist in a given dataset. The first runs during shock-table normalisation, the second
when the data shows up. `specs_overlap` answers whether two cells intersect, which is how
overlapping `set` shocks are caught.

The cheapest way to see what a cell actually selects is to ask for the plan without paying
for it:

```python
plan = apply_scenario(system, data, scenario, validate_only=True)
plan["diagnostics"]["cell_population"]
```

which reports, per cell, how many people it contains and what their labour states are —
before any transformation runs.
