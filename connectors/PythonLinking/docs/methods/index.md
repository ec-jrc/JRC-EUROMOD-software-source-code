# Methods

A method — a *methodology* — is a reviewed implementation that turns shocks into
transformed EUROMOD input plus run parameters. It is the modelling of the linkage: which people
move, how they are ranked, what a new worker earns, what a growth rate multiplies.

Each one lives in its own module under `euromod_linking.methods`, consumes one shock channel,
and is referred to everywhere by its plain name.

Two ship with the package:

[`scale_variables`](scale-variables.md)
: Cell-level arithmetic on input variables and income lists. Multiply, grow, add or set.
  Consumes `scale` shocks.

[`lma_labour_alignment`](lma-labour-alignment.md)
: Two-level hierarchical alignment of the population to external employment and unemployment
  targets. Consumes `align` shocks.

A scenario may carry both channels; the engine then runs both methods, scaling first. That
composition is a property of the scenario, not a third method — see
[Several shocks in one scenario](../concepts/composition.md).

## The caller never picks one

Dispatch is from the shock table's channels and metrics, not from a field in the scenario. An
`align` shock on `employment` resolves to `lma_labour_alignment`; a `scale` shock resolves
to `scale_variables`; a table carrying both resolves to both. The resolved reference comes
back in the result.

The reason is that the alternative is silently wrong. If the caller named the methodology, the
same scenario could be handled by a different method than the one that produced an earlier
result, and nothing in either output would say so. Resolving from the shocks means a scenario
that dispatches differently is a scenario that *is* different.

A consequence follows. Because the scenario never names the method, nothing in the document
changes when the method itself changes — so the guard against a stale result has to live
somewhere other than the scenario. `registry.code_fingerprint` hashes a method's own source
into the [scenario fingerprint](../concepts/scenarios.md), so editing an implementation
invalidates cached results rather than serving answers computed by the earlier code. When
several methods run, every one's fingerprint is folded in, together with their order.

Methods deliberately carry **no version number**. A content-derived identity cannot be
forgotten the way a hand-maintained version integer can, and the integer would have to be
remembered on exactly the occasion it is easiest to overlook: a small correction to the
modelling that changes the numbers.

Dispatch is per channel, so a method is only ever a candidate for the channel it consumes. If
two methods claim the *same* channel, dispatch raises `MethodLookupError` listing both, and
the scenario's `methodology` field is how you choose. That is the field's only purpose,
alongside reproducing an old run exactly.

## What a method declares

Each publishes its contract as a `MethodSpec`: the channel and metrics it consumes, the
**stage** it runs in, the input columns it requires, the add-ons and extension switches its
runs need, the columns it injects, whether it restructures rows, and its `params_schema`.
That contract is what [scenario validation](../concepts/scenarios.md) and the
[compatibility check](../concepts/compatibility.md) test against.

Read it at runtime:

```python
from euromod_linking import list_specs

for spec in list_specs():
    print(spec.name, "—", spec.summary)
    print("  consumes:", spec.channels_consumed, "at stage", spec.stage)
    print("  needs   :", spec.dataset_requirements)
```

## Declared contracts

Generated from the registry, so it cannot drift from the code it describes.

```{eval-rst}
.. method-reference::
   :no-description:
```
