"""What every linking method is handed and what it returns.

A method implements ``apply`` and ``check_dataset``, plus an optional
``preview``, and must be deterministic: same input data, same shocks, same
params gives bit-identical output.
"""

from dataclasses import dataclass, field

import pandas as pd


class MethodError(ValueError):
    """Raised by a method for domain failures (infeasible targets, bad state)."""


@dataclass(frozen=True)
class MethodContext:
    # Which model, country, system and dataset the run is against. A method
    # needing anything further from the model — an income list's membership, a
    # country's full-time week — looks it up itself through `euromod_linking.query`
    # using these, and keeps that knowledge in its own module. The context
    # carries no method-specific fields, so adding a method never widens it.
    country_code: str
    system_name: str
    dataset_name: str | None = None
    # Extension switches the scenario turns on or off. Model lookups are
    # extension-aware, so a method must pass these through when it resolves one.
    extensions: list | None = None


@dataclass
class MethodResult:
    data: pd.DataFrame                   # transformed input (counterfactual)
    diagnostics: dict = field(default_factory=dict)
    constants: dict | None = None        # optional {(name, group): value} the method emits
    # The *same rows* with the shock not applied. Methods that restructure the
    # microdata (household weight splits) must supply this: statistics that
    # compare a baseline with a reform pair the two runs observation by
    # observation — fixed poverty line, baseline-defined decile groups — so the
    # two frames have to share one row axis. Emitting it here means one
    # alignment produces both runs. None => the unmodified input is already a
    # valid, row-aligned baseline.
    baseline: pd.DataFrame | None = None


# The method interface, written out because it is a contract rather than a base
# class — a method is any object with these callables:
#
#   apply(data, shocks, params, ctx) -> MethodResult
#       Produce the counterfactual input from the shocks. Deterministic.
#
#   check_dataset(columns, shocks) -> list[str]
#       Dataset-aware validation problems ([] = ok), from the column list alone,
#       before any expensive work.
#
#   preview(data, shocks, params, ctx) -> dict        (optional)
#       What apply() would target, without doing the work: the per-cell baseline
#       population, the resulting targets, the implied change, feasibility
#       warnings. Must reuse apply()'s own target construction — a preview
#       computed a second way can disagree with the run it previews — and must
#       not fit models or align, since it runs on the validation path.
