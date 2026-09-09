# Changelog

<!--next-version-placeholder-->

## v0.3.0 (09/09/2026)

- **Composition moves from a method into the engine.** A shock table carrying several
  channels no longer dispatches to a composite method; each channel dispatches to its own
  method and `apply_scenario` runs them in **stage** order. `MethodSpec.stage` says what kind
  of transformation a method is (`STAGE_VALUES` for `scale_variables`, `STAGE_PEOPLE` for
  `lma_labour_alignment`), so the order holds for every combination without a method being
  written for each pair - and dispatch is per channel, so a mistyped `align` metric is
  refused by name whatever else the table carries. `align_with_scaling` and
  `MethodSpec.composes` are removed. See the new *Several shocks in one scenario* page.
- **Diagnostics of a multi-method run nest under each method's name**, with `order` and
  `stages`; a single-method run stays flat. `plan["methodology"]` is the methods in run order
  joined with `+` (`scale_variables+lma_labour_alignment`) and is accepted back as a pin;
  `plan["methods"]` and `plan["stages"]` say what ran. `plan["compatibility"]` is a list, one
  report per method.
- **`params` are validated against the union of the methods' schemas**, so `tolerance_pct`
  is accepted whenever `lma_labour_alignment` runs and refused otherwise.
- **Previews see what the run will hand them.** A method may declare `preview_by_applying`
  (as `scale_variables` does) so a later stage's preview is sized against its output.
- Registry API: `resolve_for_channel`, `resolve_for_channels({channel: metrics})`,
  `pipeline`, `resolve_pipeline`, `pipeline_name`, `pipeline_fingerprint`;
  `check_scenario()` returns `specs` (a list) instead of `spec`.

## v0.2.0 (08/09/2026)

- **Shocks in one scenario can now carry both `scale` and `align` channels.** A new
  composing method, `align_with_scaling`, dispatches for a table holding both and runs the
  two registered methods in a fixed order - scaling first, then alignment - so new workers
  enter at counterfactual wages via `yivwg`. Neither half is reimplemented; their diagnostics
  come back unflattened under `scale` and `align`. The order is methodology, not a scenario
  input: record order is discarded at normalisation, as before.
- **Dispatch prefers the most specific method.** A composing method declares every channel
  it can take, which made it a candidate for each channel alone; resolution now narrows to
  the fewest declared channels, so a `scale`-only table still gets `scale_variables`. Genuine
  ambiguity is still an error.
- **`MethodSpec.composes`.** A method that delegates names its delegates, and their source
  is folded into its `code_fingerprint`, so editing `scale_variables` invalidates cached
  runs of the composite too.
- **Descriptive names for income lists.** A `scale` metric may name the economic concept -
  `"employment income"` - instead of `ils_udb_yem`. Names come from the model's own `DefIl`
  comments across the 27 country files, plus the short forms an analyst would type; matching
  ignores case, separators and punctuation. Resolution happens at normalisation, so the
  canonical table always holds the `ils_udb_*` name and the content id does not depend on
  spelling. A misspelt concept fails at normalisation naming the closest matches. The
  catalogue lives in the new `euromod_linking.income_lists` module; the accepted spellings
  are generated into the `scale_variables` docs from it.
- **Income-list catalogue corrected against the model.** `ils_udb_yds` was described as
  total market income and filed as scalable; the model defines it as disposable income, an
  aggregate of all twenty other lists including taxes, and it now sits in an `aggregate`
  group. `ils_udb_yot` is income of people under 16 (not "other income") and `ils_udb_tpr`
  is taxes on wealth. `ils_udb_kfbcc` (company car) and `ils_udb_xmp` (private transfers
  paid) were missing and are both data-reported. The live test now asserts the catalogue is
  exactly the set the model defines.
- Fixed a `KeyError` in the documented `income_lists()` example.

## v0.1.0 (25/08/2026)

First release.

Turns an external economic model's projected changes into transformed EUROMOD input
microdata and run parameters, so macro results can be evaluated at household level.

- **The shock table.** One canonical eight-column format every external model's output is
  normalised into, identified by a content hash so the same economic scenario always has the
  same id. Channels: `align`, `scale`, `constant` (`reweight` and `inject` are reserved).
- **Population cells** written over real EUROMOD input variables, never invented category
  labels. `region` is NUTS-aware: coarser codes cover their subregions, and finer codes
  collapse to the level the dataset supports.
- **Two entry points.** `apply_scenario` transforms input and executes nothing;
  `run_scenario` additionally runs baseline and counterfactual. `validate_only=True` reports
  what a scenario would do before paying for two simulations.
- **Two linking methods**, resolved from the shock channels rather than chosen by the caller:
  `lma_labour_alignment` (two-level hierarchical alignment to employment and unemployment
  targets, driving the LMA add-on) and `scale_variables` (cell-level arithmetic on input
  variables and extension-aware income lists).
- **Model compatibility checking.** `check_compatibility` and `compatibility_matrix` verify a
  method's required add-ons and extension switches against the model before a run, so a
  missing `LMA_trans` fails during validation rather than after two simulations.
- **Declarative ingest.** External model output is read through mapping specs — YAML or a
  dict — rather than bespoke parsing code.

Requires Python 3.11+ and a local EUROMOD model, which is distributed by the JRC at
<https://euromod-web.jrc.ec.europa.eu/download-euromod>. This targets the EUROMOD model for
the 27 EU member states; models built on the same engine but with different conventions
(SOUTHMOD, SWISSMOD, national models outside the EU) are out of scope.
