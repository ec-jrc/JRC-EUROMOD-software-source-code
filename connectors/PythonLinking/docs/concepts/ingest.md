# Ingesting external model output

An external model's results arrive as a file — usually a spreadsheet with a layout nobody
chose deliberately: a couple of unnamed leading columns, one sheet per quantity, one column
per projection period. Turning that into shock records is exactly the kind of work that
accretes bespoke parsing functions, one per model, each subtly different and none of them
reviewed as carefully as the modelling they feed.

So it is data instead. A **mapping spec** describes the file's layout declaratively. It is
validated against `mapping.v1.schema.json`, and it can be a YAML file or a plain dict.

```python
from euromod_linking.ingest import ingest

records, warnings = ingest("projections.xlsx", spec, country="BE")
```

The rule that keeps this honest: **mappings stay data, code stays reviewed.** Anything a spec
cannot express becomes a new *named transform* in `ingest.py`, added deliberately — not an
escape hatch that lets arbitrary logic live in YAML.

## A spec, part by part

Take a workbook with two sheets, `employment` and `unemployment`. Each row is a region and an
education class; each numbered column is a projection period holding a growth rate against
baseline.

The `reader` block says where rows come from and what they mean. Each sheet declares the
[channel and metric](shock-table.md) its rows produce:

```yaml
mapping_version: 1
name: regional_projections
description: Regional employment and unemployment projections by education class.

reader:
  format: excel
  sheets:
    - {sheet: employment,   channel: align, metric: employment}
    - {sheet: unemployment, channel: align, metric: unemployment}
```

`columns` names the columns the spec will refer to — by position, since the leading columns
carry no usable headers. `periods` says the period labels are numeric column headers:

```yaml
columns:
  region_code: {position: 1}
  education:   {position: 2}

periods:
  mode: numbered_columns
  range: [1, 10]

value_semantics:
  op: grow          # decimal growth vs baseline: target = baseline * (1 + value)
  unit: rate
```

`group` builds the [population cell](population-cells.md) for each row, and it is where the
interesting decision lives:

```yaml
group:
  region:
    from: region_code
    transform: nuts_code    # "BE21" -> region=21
  deh:
    from: education
    allowed: [low, medium, high]
    values:
      low:    "0-2"         # up to lower secondary
      medium: "3-4"         # upper / post secondary
      high:   "5-99"        # tertiary
```

An external model that publishes its own categories — skill bands, sectors, household types —
translates them **here, at the model boundary**. Everything downstream speaks plain EUROMOD
variables: `deh=3-4`, not the external label. That is what makes the resulting shock table
readable by someone who has never seen the source model.

Two keys do that translation. `values` maps an external class to a
[value spec](population-cells.md); `as` emits the result under a different key, when the
external column's name is not the EUROMOD variable's name. `allowed` rejects a class the spec
does not know rather than passing it through silently.

Finally, `filters` drops rows that carry nothing usable:

```yaml
filters:
  - column: region_code
    not_null: true
```

The `country` argument to `ingest` keeps only rows for the country being simulated, which
matters when one file covers several. Rows it drops are reported in the returned warnings, not
discarded quietly.

## Using a spec

Pass a dict directly, which is the quickest way to work while the layout is still moving:

```python
from euromod_linking.ingest import ingest, load_mapping

spec = {
    "mapping_version": 1,
    "name": "regional_projections",
    "description": "Regional employment projections by education class.",
    "reader": {"format": "excel",
               "sheets": [{"sheet": "employment", "channel": "align",
                           "metric": "employment"}]},
    "columns": {"region_code": {"position": 1}, "education": {"position": 2}},
    "periods": {"mode": "numbered_columns", "range": [1, 3]},
    "value_semantics": {"op": "grow", "unit": "rate"},
    "group": {"region": {"from": "region_code", "transform": "nuts_code"},
              "deh": {"from": "education",
                      "values": {"low": "0-2", "medium": "3-4", "high": "5-99"}}},
}

load_mapping(spec)                      # validate on its own; raises IngestError
records, warnings = ingest("projections.xlsx", spec, country="BE")
```

Once the layout has settled, save it as a YAML file under `euromod_linking/mappings/` and
refer to it by name — from Python, or from a scenario, so the file is read as part of applying
the scenario rather than beforehand:

```python
scenario["shocks"] = {"file": "projections.xlsx", "mapping": "regional_projections"}
```

`list_mappings()` reports the specs shipped with the package, and `load_mapping(name)`
validates one by name. `resources.py` reads shipped data through `importlib.resources`, so
specs keep working from a zipped wheel.
