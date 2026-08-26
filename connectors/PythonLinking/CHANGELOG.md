# Changelog

<!--next-version-placeholder-->

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
