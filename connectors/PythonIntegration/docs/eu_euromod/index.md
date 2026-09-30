Tools for the EU model
======================

The connector works with any model built for the EUROMOD software. The package
`euromod.eu_euromod` is different: its modules are written for one model, the
model of the 27 EU member states maintained by the Joint Research Centre.

They rely on that model's conventions: its income lists (`ils_dispy`,
`ils_origrepy`, ...), its variable names (`dwt`, `dms`, `yse`, ...) and its system
names (`PT_2022`). On a model that does not follow these conventions they do not
work, and each module states exactly what it needs.

The package is not loaded by `import euromod`. Import what you need:

```python
from euromod.eu_euromod import EU27, EURO_AREA_19, eu27_systems
from euromod.eu_euromod.calibrate import prepare_sample, fit_hsv
```

`EU27` and `EURO_AREA_19` are tuples of country codes, and `eu27_systems(2022)`
returns the baseline system names `["AT_2022", "BE_2022", ..., "SK_2022"]`.

```{toctree}
:maxdepth: 1

calibrate.md
```
