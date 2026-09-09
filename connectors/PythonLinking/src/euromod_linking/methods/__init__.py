"""Method implementations.

Importing this package registers every shipped methodology in the registry.
The package __init__ imports it explicitly, so `import euromod_linking` always leaves
the registry populated — a caller that reaches straight for the orchestration
function must not silently get "no methodology handles channel ...".

Each method consumes one shock channel and declares the *stage* it runs in;
the scenario engine composes them, so there is no method to register for a
combination of channels.
"""

# Registered methods — one import per method package:
from euromod_linking.methods import lma_labour_alignment  # noqa: F401
from euromod_linking.methods import scale_variables  # noqa: F401
