"""Method method implementations.

Importing this package registers every shipped methodology in the registry.
The package __init__ imports it explicitly, so `import euromod_linking` always leaves
the registry populated — a caller that reaches straight for the orchestration
function must not silently get "no methodology handles channel ...".
"""

# Registered methods — one import per method package:
from euromod_linking.methods import lma_labour_alignment  # noqa: F401
from euromod_linking.methods import scale_variables  # noqa: F401

# Composing methods last: their module imports the delegates they register over.
from euromod_linking.methods import align_with_scaling  # noqa: F401
