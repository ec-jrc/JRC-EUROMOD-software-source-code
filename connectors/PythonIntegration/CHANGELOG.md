# Release notes

<!--next-version-placeholder-->

## v0.4.0 (30/09/2026)

### Feature

- **New opt-in `euromod.eu_euromod` package, for tools that are specific to the
  JRC's model of the 27 EU member states.** The rest of the connector works with
  any model built for the EUROMOD software; the modules in this package rely on
  the EU model's income lists, variable names and system names, and say exactly
  what they need. Like `euromod.calibrate` and `euromod.introspect`, it is not
  imported by `euromod/__init__`.
- **`euromod.eu_euromod.calibrate`: estimate a parametric tax-benefit function
  from a simulation.** It fits the function of Heathcote, Storesletten and
  Violante, `net tax rate = 1 - (1 - lambda) * income**(-tau)`, by weighted least
  squares in logs. The functions need a path to the model, a path to the folder
  with the input data and the name of a system:
  - `prepare_sample()` runs a system and builds the household sample
    (`simulate()` followed by `household_sample()`). A reform is a counterfactual
    system (`"PT_2022_ref"`) or a baseline system with `constants=` overridden.
  - `fit_hsv()` estimates level and progressivity, or progressivity alone with
    `lambda_fixed=`; `calibrate_hsv()` does several systems and household types in
    one call and can add a pooled estimate such as the euro area.
  - `fit_diagnostics()` and the functions of `descriptives` measure the fit and
    describe the sample.
  - `hsv()` evaluates a fitted function with numpy; `to_calibration()` exports it
    as a `euromod.calibrate.Calibration`, which gives a numba-compiled function
    and a JSON file.
  - `load_estimates()` returns the published estimates for the 27 EU member
    states and the euro area (policy year 2022, seven household types), which
    ship with the package; `save_estimates()` and `load_estimates(path)` keep a
    table of your own estimates as a JSON file; `schedule()` turns a row of
    either into a `Schedule` that computes net tax rates, net taxes and
    disposable incomes.

  The module needs numpy and pandas only. See the new documentation page
  "Calibrating a tax-benefit function".
- **New optional extra `euromod[calibrate]`** installs sympy, numba and scipy, which
  `euromod.calibrate` and `to_calibration()` need.

### Fix

- **`Calibration.to_json()` no longer fails when constants were overridden.**
  Constants are keyed by `(name, group)`, which JSON cannot hold as a key, so the
  export raised `TypeError` and left a truncated file behind. They are now written
  as a list of `{name, group, value}` records and read back into the same keys by
  `from_json()`. Files written by earlier versions still load.
- **`run()` rejects a `constantsToOverwrite` key that is not a `(name, group)`
  pair.** A plain string key such as `"$tin_rate"` was taken apart character by
  character and read as the constant `"$"` in group `"t"`, so the override was
  silently not applied. It now raises `TypeError`.

- **`introspect.income_list_components()` finds every income list.** A `DefIl`
  function whose name parameter is spelled `name` rather than `Name` was skipped,
  so in the EU model standard lists such as `ils_dispy` and `ils_taxsim` were
  reported as unknown and were not expanded inside other lists. Parameter names
  are now matched without regard to case. A component marked `n/a`, which is
  not part of the list in that system, is no longer returned as a `+` component.

The bundled EUROMOD engine is unchanged at version 3.8.8.

## v0.3.2 (10/08/2026)

### Fix

- **`run()` no longer accepts an extension switch the model does not know.** An
  unknown extension name was dropped by the engine with only a console message,
  so the simulation completed normally while the behaviour that was asked for
  never happened — baseline and counterfactual came out identical, which reads as
  "the reform has no effect" rather than "the reform never ran". `System.run()`
  now validates `switches=` against the system and raises `ValueError`, naming the
  switches the system does accept. Validation is skipped when no extension names
  can be read from the model, since that is "cannot validate", not "nothing is
  valid".

### Feature

- **New opt-in `euromod.introspect` module: read-only introspection of a system's
  spine.** It answers "what is *effective*" rather than "what is defined", so a
  caller preparing a `run()` call does not have to re-implement the model's
  extension include/exclude rules by hand:
  - `system_extension_names()` — the extension names `run(switches=...)` accepts.
  - `system_constant_params()` — every `$`-parameter overridable via
    `run(constantsToOverwrite=...)`, mapped to the groups it is defined with, so
    year-keyed uprating factors validate as well as plain `DefConst` constants;
    `system_constant_names()` for the `DefConst` names alone.
  - `income_list_components()` — the variables an income list actually resolves
    to under a given dataset and extension set, following extension switches and
    redefinitions down the spine (raises `IncomeListLookupError` when the list is
    unknown, inactive or cyclic).
  - `iter_real_policies()` to walk a country or system skipping reference
    policies, and `clear_cache()` for the rare case of reloading a model in one
    process.

  The module is not imported by `euromod/__init__` — `import euromod.introspect`
  explicitly, as with `euromod.calibrate`. Spine data is extracted once per system
  and cached, so repeated questions cost no further interop.
- **`utils.harden_dotnet_console()`**, for embedding the connector in a process
  without a valid console handle (a service, a detached launch, some notebook
  kernels). There, the engine writing a warning to the console raises
  `IOException` and turns a harmless warning into a failed simulation; this
  detaches the .NET console so it cannot. Warnings still arrive through the
  structured simulation result. Call it yourself before running; opt back out with
  `EUROMOD_KEEP_CONSOLE=1`.

The bundled EUROMOD engine is unchanged at version 3.8.8.

## v0.3.1 (21/07/2026)

### Fix

- **No more module-name clashes with your own project.** The connector used to add
  its own directory to `sys.path`, which made its submodules importable under bare
  names (`utils`, `core`, `base`, `info`, `container`, `statistics`, `debug`). In a
  project that had its own `utils.py`, `import euromod` failed with
  `ModuleNotFoundError: ... 'utils' is not a package`; in the other import order
  euromod's modules shadowed the host project's — and the standard library's
  `statistics`. All internal imports are now relative and both `sys.path`
  injections are gone, so importing `euromod` no longer touches the global module
  namespace. `euromod.core`, `euromod.utils`, ... remain importable as before; only
  the leaked bare names are gone.
- **Statistics now works with a system-wide EUROMOD installation.** Engine
  auto-detection now uses `C:\Program Files\EUROMOD\EUROMOD` — the complete
  installation, which ships `EM_Statistics.dll` — instead of the incomplete
  `...\Executable` folder that left `Statistics` unavailable.
- Removed the dead, shadowed top-level `calibrate.py`.

## v0.3.0 (17/07/2026)

### Feature

- **Statistics.** New `Statistics` and `StatisticsResult` classes to compute
  EUROMOD statistics from a template against one or more simulations, with access
  to the resulting pages, tables, columns and rows, and export via
  `StatisticsResult.to_excel()`.
- **Statistics runs on Linux and macOS too.** The bundled statistics engine is a
  cross-platform (netstandard2.0) compute-only build, so statistics calculation is
  no longer Windows-only. Excel export (`to_excel()`) still requires a full Windows
  EUROMOD installation and raises a clear error otherwise.
- **Software-version compatibility with automatic fallback.** The connector now
  declares the EUROMOD software version it needs and checks the installed engine
  *before* loading it. If that engine is older than required — or its version
  cannot be determined — the connector falls back to the engine bundled with the
  package and warns, instead of failing obscurely part-way through a run. New
  attributes: `euromod.software_version`, `euromod.required_software_version`,
  `euromod.using_bundled_fallback` and `Model.software_version`.
- **Add-ons are now exposed as navigable objects on the model.** `Model.addons` is a
  container of `Addon` objects that can be explored like a `Country` (systems,
  policies, functions, parameters). Add-on systems are represented by the new
  `AddonSystem` class, which exposes their applicability (`applies_to_patterns`,
  `not_applicable_patterns`, `is_applicable()`); `Addon.get_applicable_systems()`
  returns the add-on systems that apply to a given base system. These objects are a
  read-only inspection view — use the `addons=` argument of `System.run()` to apply
  an add-on in a simulation.
- **Add-ons can be given by name only.** The add-on system no longer has to be
  spelled out; it is resolved automatically, with a clear error when the name is
  ambiguous.
- New `is_valid_model()` helper to check whether a path is a EUROMOD project.
- The bundled EUROMOD engine was updated to version 3.8.8.

## v0.1.21a (12/09/2024)


### Feature

- Information with respect to Extension Switches can now be retrieved
- Better formatting of information
