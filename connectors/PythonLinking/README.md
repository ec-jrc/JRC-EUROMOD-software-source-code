# euromod-linking

Connect external economic models to [EUROMOD](https://euromod-web.jrc.ec.europa.eu/) by
transforming its input microdata and run parameters.

A macro model — a CGE model, a forecasting model, a policy scenario written by hand — projects
*changes*: employment by region and education, nominal wages, price levels. EUROMOD simulates
taxes and benefits on a household survey. This package is the layer between them: it takes those
projected changes as data, rewrites the microdata and run parameters so the survey population
matches them, and runs baseline and counterfactual simulations that differ *only* by the shock.

It is a standalone library. It has no dependency on any agent, notebook or UI, and is usable
directly from a script.

## Scope

**This library targets the JRC's EUROMOD model for the 27 EU member states, and only that
model.** It is not a general library for the EUROMOD *software*: models built on the same
engine — SOUTHMOD, SWISSMOD, and national or regional models outside the EU — will not work
with it.

It assumes EUROMOD's own EU-27 conventions throughout: the `ils_udb_*` User Database income
lists, the `les`/`les2` labour-status codings, `drgn1`/`drgn2` read as NUTS regions, variable
names such as `dwt`, `dag`, `deh`, `yem` and `yivwg`, the `$lhw` constant, and the
JRC-maintained LMA add-on. Those conventions are precisely what a different model changes.

## Install

```bash
pip install euromod-linking          # or: pip install -e .
pip install euromod-linking[excel]   # to ingest .xlsx/.xlsm/.xls model output
```

Requires Python 3.11+ and a local EUROMOD model and installation (see the `euromod` connector).
The model is distributed by the JRC and can be requested at
<https://euromod-web.jrc.ec.europa.eu/download-euromod>.

## The canonical shock table

Every external model's output is normalised into one table before anything else happens. Six
columns, one row per shock:

| column | meaning |
|---|---|
| `channel` | how it acts: `align` (move people between states), `scale` (multiply a variable), `constant` (override a model parameter) |
| `metric` | what it acts on: `employment`, `unemployment`, an input variable (`yem`), an income list (`ils_udb_yem`), or a constant name (`$f_cpi`) |
| `group` | which people: a population cell over EUROMOD input variables — `"deh=3-4;dgn=1"`, `""` for everyone |
| `period` | the external model's period label |
| `op` | `set` \| `grow` \| `mult` \| `add` |
| `value` | the number |

Shock tables are content-addressed (`shk_…`), so the same economic scenario always has the same
id regardless of which file or code path produced it.

Population cells are written over *real EUROMOD input variables*, never invented category labels.
`region` is the one named dimension and is NUTS-aware: coarser codes cover their subregions, and
shocks defined at a finer level than the data supports are collapsed to the level it does.

## Two entry points

```python
import pandas as pd
from euromod import Model
from euromod_linking import apply_scenario, run_scenario

system = Model(r"C:\EUROMOD_RELEASES").countries["BE"].systems["BE_2025"]

scenario = {
    "country_code": "BE",
    "system_name": "BE_2025",
    "shocks": {"inline": [
        {"channel": "align", "metric": "employment", "group": "deh=0-3",
         "period": "1", "op": "grow", "value": 0.0655},
    ]},
    "params": {"period": "1"},
}

# Pure transform: returns the counterfactual input, a matched baseline, and diagnostics.
plan = apply_scenario(system, pd.read_csv(data_file, sep="\t"), scenario)

# Convenience: does the above and runs both simulations.
out = run_scenario(system, scenario, input_path=r"C:\EUROMOD_RELEASES\Input")
```

`apply_scenario(..., validate_only=True)` stops before the expensive work and returns what the
scenario *would* do: the per-cell baseline population the shocks resolve against, the resulting
targets, the implied change in people and in percentage points, and feasibility against the pool
of people who can actually move. Check a shock is the size you meant before paying for two
simulations.

External model output in a file is ingested through a declarative mapping spec — a YAML file
under `mappings/`, or a dict — rather than bespoke parsing code:

```python
scenario["shocks"] = {"file": "projections.xlsx", "mapping": "regional_projections"}
```

## Methodologies

The transformation is **never chosen by the caller**. It is resolved from the shock channels and
echoed back, so a scenario cannot silently be handled by a different method than the one that
produced an earlier result. Their code is content-fingerprinted into the scenario id — changing the science invalidates cached results rather than serving stale
ones.

- **`lma_labour_alignment`** (`align`) — two-level hierarchical alignment to external
  employment/unemployment totals per cell. Level 1 sizes the labour force, level 2 splits it into
  employment and unemployment. Who moves is decided by ranked logit propensities, not a random
  draw; targets are hit exactly by splitting the boundary household's weight. New workers are
  given full-time earnings at their own predicted hourly wage. Labour states come from `les`/`les2`
  over working age (18–65), and only genuinely inactive people are recruitable — students,
  pensioners, the long-term sick and conscripts are never moved.
- **`scale_variables`** (`scale`) — cell-level scaling of input variables. An income-list metric
  is expanded extension-aware into its component variables by walking the model's `DefIl`
  definitions, so it stays correct when extensions change what a list contains.

## Diagnostics

Every run reports the baseline it worked from, not just what it achieved: per-cell population and
labour states, the resolved target, the change in people *and* percentage points, feasibility
against the recruitable pool, and transitions as **weighted people** alongside raw row counts.
An alignment grade says only that the target was hit — never that the target was right — so the
baseline is always reported beside it.

## Tests

```bash
pip install -e .[dev]
pytest
```

## Licence

EUPL-1.2.
