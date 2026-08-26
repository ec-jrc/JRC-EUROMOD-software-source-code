# Model compatibility

A methodology needs things from the model. `lma_labour_alignment` needs the **LMA add-on**
and the **`LMA_trans` extension switch**; without them the alignment it performs on the
microdata has nothing to act on.

The engine does not complain about either. An unknown extension switch is dropped with a
console message, and an add-on that is not there simply is not applied. Left unchecked, both
simulations complete normally, produce identical output, and the scenario fails at the end
with `NoEffectError` — after paying for two full runs.

`check_compatibility` asks the question first.

```python
from euromod_linking import check_compatibility

report = check_compatibility(system, "lma_labour_alignment")
print(report.ok)
for r in report.requirements:
    print(f"{r.kind:10} {r.name:12} satisfied={r.satisfied}  ({r.detail})")
```

```text
True
addon      LMA          satisfied=True  (present)
extension  LMA_trans    satisfied=True  (accepted by this system)
```

The same check runs inside `apply_scenario`, including under `validate_only=True`, so a
scenario that cannot work is rejected before anything expensive happens. The report is on
the result under `plan["compatibility"]`.

## Reading a failure

Requirements are listed separately because they fail for different reasons and have
different fixes.

```text
lma_labour_alignment on BE/BE_2001: NOT ok
  problem: lma_labour_alignment needs add-on system LMA_BE, but no system of the LMA
           add-on applies to BE_2001.
  note:    This model looks like J2.54 (from folder-name), at or above the J2.54 floor
           for lma_labour_alignment — so the release is not what is missing.
    addon      LMA         satisfied=False  (no add-on system LMA_BE applies to BE_2001)
    extension  LMA_trans   satisfied=True   (accepted by this system)
```

The extension is accepted, so the model is new enough; what is missing is an add-on system
covering this particular base system. **Add-on applicability is per system**, matched against
the add-on's own `AddOn_Applic` patterns — an add-on can be installed and still not cover
older systems in the same country. Those two lines together say the fix is a different
system, not a different model.

A model that predates the extension entirely reads the other way round:

```text
  problem: lma_labour_alignment needs the LMA_trans extension, which BE/BE_2025 does not
           accept. An unknown switch is silently ignored by the engine, so the run would
           appear to succeed without applying it.
  note:    lma_labour_alignment needs EUROMOD release J2.54 or later; this model looks
           like J2.19 (from folder-name).
```

## Capability decides; the release explains

The two kinds of evidence are deliberately unequal.

**Capability is authoritative.** Whether the model ships an add-on is a directory listing,
and whether a system accepts an extension switch is read from the model itself. Both are
cheap and exact, and both are what actually determine whether a run will work.

**The release is advisory.** Which EUROMOD release a folder holds is not reliably knowable.
The documented marker `XMLParam/Config/EuromodVersion.txt` has writer code in the UI but
ships in no real release. The folder name is what the UI itself falls back to — and a user
who renames the folder erases it. So a detected release only ever improves the message; it
never blocks a run, and failing to detect one is not a failure.

`model_release()` tries, in order:

| Source | Note |
|---|---|
| `XMLParam/Config/EuromodVersion.txt` | The documented contract. Authoritative when present — which, in practice, it is not. |
| The model folder name | `EUROMOD_MASTER_VERSION_J2.19`. What actually carries the release today. Checks the parent too, for nested layouts. |
| `EUROMOD_model_licence_*.txt` at the model root | Public releases only. |
| `Log/EM_LOG.xlsx`, column `Version` | The only marker in the model's *content*, so it survives a rename — but the workbook runs to tens of megabytes, so it is opt-in via `deep=True`. |

Releases sort by letter, then major, then minor, so `J2.7` is older than `J2.19` and
`I6.0+` is older than `J1.0`. A trailing `+` marks a rolling build and sorts just above the
bare version, so `J1.86+` is not read as older than `J1.86`. Anything unparseable compares
as `None` — "cannot compare", never "too old".

## Three-valued by design

Every requirement is satisfied, unsatisfied, or **undeterminable**. Only the middle one
produces a problem; the third produces a note and leaves the report `ok`.

That distinction is the whole safety property. Collapsing "the model could not be read" into
"the requirement is not met" would refuse to run against a perfectly good model, which is a
worse failure than the late one this check replaces. If you never loaded a model at all,
the check reports that it verified nothing and blocks nothing.

Set `EUROMOD_SKIP_COMPAT_CHECK=1` to disable it entirely.

## Surveying a model

`compatibility_matrix` answers "what can I actually run here" without writing a scenario:

```python
from euromod import Model
from euromod_linking import compatibility_matrix

model = Model(r"C:\EUROMOD_RELEASES")
df = compatibility_matrix(model, "BE")
df[["system", "methodology", "ok", "model_release", "min_model_release"]]
```

```text
 system            methodology     ok model_release min_model_release
BE_2001 lma_labour_alignment  False         J2.54             J2.54
BE_2001      scale_variables   True         J2.54
BE_2019 lma_labour_alignment  False         J2.54             J2.54
BE_2019      scale_variables   True         J2.54
BE_2020 lma_labour_alignment   True         J2.54             J2.54
BE_2020      scale_variables   True         J2.54
BE_2025 lma_labour_alignment   True         J2.54             J2.54
BE_2025      scale_variables   True         J2.54
```

One model, one release, and the answer still differs by system — which is the point of asking
per system rather than per model. Pass a country code unless you mean otherwise: walking every
country of a full model means loading every country.

## Declaring a floor

A methodology states its earliest supported release on its `MethodSpec`:

```python
register(MethodSpec(
    name="lma_labour_alignment",
    ...
    addon_requirements=((("LMA", "LMA_{cc}"),), (("LMA_trans", True),)),
    min_model_release="J2.54",  # first internal release shipping the LMA_trans extension
))
```

`{cc}` in an add-on requirement is resolved against the scenario's country, so
`"LMA_{cc}"` becomes `LMA_BE` and is checked against the add-on systems that actually apply
to the base system.
