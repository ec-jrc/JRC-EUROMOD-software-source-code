# Release notes

<!--next-version-placeholder-->

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
