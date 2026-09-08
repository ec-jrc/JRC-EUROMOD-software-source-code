"""align_with_scaling — income scaling and labour-market alignment in one scenario.

Consumes the ``scale`` and ``align`` channels together, so a macro model that
projects both a nominal path and a labour-market path can be pushed through
EUROMOD as one shock table instead of two chained runs.

The order is fixed
------------------
**Scale first, then align.** That is a modelling decision, so it lives here in
code rather than being read off the scenario: a shock table is sorted by
``(channel, metric, group, period)`` and carries no order of its own, and the
record order a caller happens to write is thrown away at normalisation to keep
the content id a property of the scenario. Inferring the economics from
alphabetical channel names, or from which sheet a mapping spec lists first,
would be exactly the order-dependence that ``scale_variables`` already refuses
between two overlapping shocks.

Why this order and not the other. The wage structure is the environment the
labour-market transitions happen in. ``lma_labour_alignment`` pays a new worker
their own predicted hourly wage ``yivwg`` — an *input variable* — so scaling
first is what makes an entrant enter at counterfactual wages. Aligning first and
scaling after would leave entrants on baseline wages, because their earnings
land in ``yem_a``, which a ``yem`` or ``ils_udb_yem`` shock does not touch: the
scale half would silently miss precisely the people the align half created.

What the baseline is
--------------------
The **untouched input**. This method sets up "both shocks against neither".

``MethodResult.baseline`` is the alignment's paired baseline — the same rows,
split households included, with every transition off — and it therefore carries
the scaling. That is deliberate and it is what ``restructures_rows`` promises:
its job is to keep the two frames row-aligned so a fixed poverty line or
baseline-defined deciles mean something, not to undo the scaling. A caller who
wants the scaling treated as *context* rather than as part of the shock should
put it in the scenario's ``constants`` (applied to both runs) or run the two
scenarios separately.

Composition, not reimplementation
---------------------------------
Both halves are the registered methods themselves, called in turn. Neither
contract is restated here: ``check_dataset`` delegates, the diagnostics are
reported under ``scale`` and ``align`` unflattened, and ``composes`` puts both
delegates' source into this method's ``code_fingerprint`` so editing either one
invalidates a cached run of this one.
"""

import pandas as pd

from euromod_linking.methods.base import MethodContext, MethodError, MethodResult
from euromod_linking.methods.lma_labour_alignment import INJECTED, LmaLabourAlignment
from euromod_linking.methods.scale_variables import ScaleVariables
from euromod_linking.registry import MethodSpec, register

#: The fixed order. See the module docstring for why it is not a scenario input.
ORDER = ("scale", "align")


def _warnings(diagnostics: dict) -> list:
    w = diagnostics.get("warnings") or []
    return list(w) if isinstance(w, (list, tuple)) else [w]


class AlignWithScaling:
    """See module docstring. Instances are stateless between calls."""

    def __init__(self):
        self.scale = ScaleVariables()
        self.align = LmaLabourAlignment()

    def _split(self, shocks: pd.DataFrame):
        """The two halves, in the order they run."""
        return (shocks[shocks["channel"] == "scale"],
                shocks[shocks["channel"] == "align"])

    def check_dataset(self, columns, shocks: pd.DataFrame) -> list[str]:
        scale, align = self._split(shocks)
        problems = []
        # Both halves are required. A table carrying one channel dispatches to
        # the dedicated method (registry prefers the fewest declared channels),
        # so getting here with one half empty means a pinned methodology — say
        # what to pin instead rather than running a degenerate composition.
        if scale.empty:
            problems.append("align_with_scaling needs at least one 'scale' shock; "
                            "for alignment alone use lma_labour_alignment")
        if align.empty:
            problems.append("align_with_scaling needs at least one 'align' shock; "
                            "for scaling alone use scale_variables")
        if problems:
            return problems
        return (self.scale.check_dataset(columns, scale)
                + self.align.check_dataset(columns, align))

    def preview(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
                ctx: MethodContext) -> dict:
        """What each half would do, the alignment previewed against the scaled frame.

        The scaling really runs here — it is the arithmetic pass only, no logit
        fitting and no alignment — because a preview that reported the alignment
        against *unscaled* data would disagree with the run it previews about
        the very cells it is meant to size."""
        scale, align = self._split(shocks)
        scaled = self.scale.apply(data, scale, params, ctx)
        align_preview = self.align.preview(scaled.data, align, params, ctx)
        return {
            "order": list(ORDER),
            "scale": self.scale.preview(data, scale, params, ctx),
            "align": align_preview,
            "warnings": _warnings(scaled.diagnostics) + _warnings(align_preview),
        }

    def apply(self, data: pd.DataFrame, shocks: pd.DataFrame, params: dict,
              ctx: MethodContext) -> MethodResult:
        scale, align = self._split(shocks)

        scaled = self.scale.apply(data, scale, params, ctx)
        aligned = self.align.apply(scaled.data, align, params, ctx)

        if aligned.baseline is None:  # pragma: no cover - the align half always pairs
            raise MethodError(
                "lma_labour_alignment returned no paired baseline; align_with_scaling "
                "restructures rows and cannot pair the two runs without one")

        constants = {**(scaled.constants or {}), **(aligned.constants or {})}
        return MethodResult(
            data=aligned.data,
            baseline=aligned.baseline,
            constants=constants or None,
            diagnostics={
                "order": list(ORDER),
                "scale": scaled.diagnostics,
                "align": aligned.diagnostics,
                "warnings": _warnings(scaled.diagnostics) + _warnings(aligned.diagnostics),
            },
        )


register(MethodSpec(
    name="align_with_scaling",
    summary="Scale income variables and align labour-market states in one "
            "scenario — scaling first, then alignment.",
    description=(
        "Composition of scale_variables and lma_labour_alignment in a fixed "
        "order: every 'scale' shock is applied first, then the 'align' targets "
        "are hit against the scaled frame. The order is methodology and is not "
        "expressible in a scenario. Scaling first is what makes new workers "
        "enter at counterfactual wages, since lma_labour_alignment pays them "
        "their own predicted hourly wage yivwg, an input variable; aligning "
        "first would leave entrants on baseline wages because their earnings "
        "land in yem_a, which a scale shock does not reach. Each half keeps its "
        "own contract: dataset checks, income-list expansion, cell collapsing, "
        "op-commutativity rules, targets, scoring, weight splits and entrant "
        "earnings all behave exactly as they do alone, and their diagnostics "
        "are reported unflattened under 'scale' and 'align'. The baseline is "
        "the untouched input; the paired baseline carries the scaling, so that "
        "the two simulated frames stay row-aligned. To treat scaling as context "
        "applied to both runs instead, use scenario constants. Scenario params: "
        "period (required when the table has several) and tolerance_pct."),
    channels_consumed=("align", "scale"),
    # Open, unlike lma_labour_alignment's five: the scale half accepts any input
    # variable or income list, so no single tuple constrains both. The align
    # half still checks its own metrics in check_dataset.
    metrics_consumed=(),
    cell_variables="Any input variable (deh=3-4, dgn=1, dag=25-34, les=5) and/or 'region'.",
    restructures_rows=True,   # inherited from the alignment half's weight splits
    dataset_requirements=("idhh", "idperson", "dwt", "dag", "dgn", "yem"),
    addon_requirements=((("LMA", "LMA_{cc}"),), (("LMA_trans", True),)),
    min_model_release="J2.54",
    injected_columns=INJECTED,
    composes=("scale_variables", "lma_labour_alignment"),
    params_schema={
        "type": "object",
        "additionalProperties": False,
        "properties": {
            "period": {"type": "string",
                       "description": "External-model period label whose shocks and targets to "
                                      "apply, for both halves. Optional when the shock table "
                                      "has exactly one period."},
            "tolerance_pct": {"type": "number",
                              "description": "Alignment reporting threshold in percent, "
                                             "default 5. Passed to the align half."},
        },
    },
    factory=AlignWithScaling,
))
