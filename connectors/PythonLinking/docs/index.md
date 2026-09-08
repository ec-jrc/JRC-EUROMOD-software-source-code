# euromod-linking

Connect external economic models to [EUROMOD](https://euromod-web.jrc.ec.europa.eu/) by
transforming its input microdata and run parameters.

A macro model — a CGE model, a forecasting model, a policy scenario written by hand —
projects *changes*: employment by region and education, nominal wages, price levels. EUROMOD
simulates taxes and benefits on a household survey. This package is the layer between them.
It takes those projected changes as data, rewrites the microdata and run parameters so the
survey population matches them, and runs baseline and counterfactual simulations that differ
*only* by the shock.

It is a standalone library, with no dependency on any agent, notebook or UI.

:::{important}
**This library targets the JRC's EUROMOD model for the 27 EU member states, and only that
model.** It is not a general library for the EUROMOD *software*.

Other models are built on the same engine — SOUTHMOD, SWISSMOD, and national or regional
models outside the EU — and this package will not work with them. It is written against
EUROMOD's own EU-27 conventions throughout, and those conventions are what a different model
changes.
:::

## Install

```bash
pip install euromod-linking          # or: pip install -e .
pip install euromod-linking[excel]   # to ingest .xlsx/.xlsm/.xls model output
```

Python 3.11+, plus a local EUROMOD model and installation. Loading and running the model is the
[euromod connector](https://ec-jrc.github.io/JRC-EUROMOD-software-source-code/)'s job — this
package sits on top of it, and the connector's documentation is published alongside this site.

The model itself — the country parameter files and the software — is distributed by the JRC
and can be requested at
[euromod-web.jrc.ec.europa.eu/download-euromod](https://euromod-web.jrc.ec.europa.eu/download-euromod).
Point `MODEL_PATH` at the model folder you unpack; everything here resolves against that
installation rather than against anything shipped in this package.

## What is assumed about the model

Every one of these is a EUROMOD EU-27 convention, and each is where a non-JRC model would
break:

`ils_udb_*` income lists
: Naming for the [income concepts](methods/scale-variables.md) a `scale` shock can address.
  These are EUROMOD's User Database output lists, standardised across the EU-27 countries.

`les` / `les2` labour status
: The code sets that decide who is employed, unemployed, inactive or shielded. `les2` follows
  the EU-SILC/EMSD monthly-activity coding.

`drgn1` / `drgn2` regions
: The `region` dimension resolves to these columns and reads their values as **NUTS** codes,
  so a coarser code covers its subregions.

`$lhw`, `dwt`, `idhh`, `idperson`, `dag`, `dgn`, `deh`, `yem`, `yivwg`
: Constant and input-variable names taken as given, not discovered.

The LMA add-on and the `LMA_trans` extension
: Required by [`lma_labour_alignment`](methods/lma-labour-alignment.md), and maintained as
  part of the JRC model. See [Model compatibility](concepts/compatibility.md) for how their
  presence is checked before a run.

Population cells and the shock table are the portable parts — they are written over whatever
input variables a dataset has. The **methods** are where the EU-27 assumptions live.

## The shape of it

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

# Pure transform: the counterfactual input, a matched baseline, and diagnostics.
plan = apply_scenario(system, pd.read_csv(data_file, sep="\t"), scenario)

# Convenience: does the above and runs both simulations.
out = run_scenario(system, scenario, input_path=r"C:\EUROMOD_RELEASES\Input")
```

Four ideas carry the whole design, and the concept pages below take them in order: every
external model's output becomes one **shock table**; the people a shock applies to are named
by a **population cell** written over real EUROMOD variables; a **scenario document** binds
shocks to a country and system; and the **methodology** that does the transformation is
resolved from the shocks rather than chosen by the caller.

## Contents

```{toctree}
:maxdepth: 2
:caption: Concepts

concepts/shock-table.md
concepts/population-cells.md
concepts/scenarios.md
concepts/ingest.md
concepts/compatibility.md
```

```{toctree}
:maxdepth: 2
:caption: Methods

methods/index.md
methods/scale-variables.md
methods/lma-labour-alignment.md
methods/align-with-scaling.md
```

```{toctree}
:maxdepth: 1
:caption: Guides

notebooks/getstarted.ipynb
notebooks/examples.ipynb
```

```{toctree}
:maxdepth: 1
:caption: Reference

autoapi/index.rst
license.md
```
