__license__='''
Copyright 2024 European Commission
*
Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:

*
   https://joinup.ec.europa.eu/software/page/eupl
*

Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the Licence for the specific language governing permissions and limitations under the Licence.
'''

from .core import Model,Country,Addon,Policy,System,AddonSystem,ReferencePolicy, PolicyInSystem, FunctionInSystem, Parameter, ParameterInSystem, Function, Dataset, DatasetInSystem, Extension
from .info import Info
from .base import ExtensionSwitch
from .utils.euromod_parsing import is_valid_model
# Software-version compatibility info, resolved at import (see utils._version).
from .utils._paths import SOFTWARE_VERSION as software_version
from .utils._paths import USING_BUNDLED_FALLBACK as using_bundled_fallback
from .utils._version import REQUIRED_SOFTWARE_VERSION as required_software_version
try:
    from .statistics import Statistics, StatisticsResult
except ImportError as _stats_err:
    import warnings as _warnings
    _warnings.warn(
        f"[euromod] Statistics module unavailable: {_stats_err}\n"
        "  The euromod package will work normally, but statistics functionality is disabled.\n"
        "  Ensure EM_Statistics.dll is placed in the libs folder or EUROMOD installation directory.",
        ImportWarning,
        stacklevel=1,
    )
    Statistics = None
    StatisticsResult = None

__all__ = ["Model",
           "Country",
           "Addon",
           "System",
           "AddonSystem",
           "Dataset",
           "DatasetInSystem",
           "Policy",
           "PolicyInSystem",
           "ReferencePolicy",
           "Function",
           "FunctionInSystem",
           "Parameter",
           "ParameterInSystem",
           "Extension",
           "ExtensionSwitch",
           "Statistics",
           "StatisticsResult",
           "is_valid_model",
           "software_version",
           "required_software_version",
           "using_bundled_fallback",
           "__version__",
           "__doc__"]

# module level doc-string
__doc__ = """
Euromod - a Python library for the microsimulation model EUROMOD.
===========================================================================

**euromod** is a Python package that runs the microsimulation model EUROMOD
provided by the European Commission - JRC. 

*This package requires the EUROMOD software and model to be installed 
on your device. For more information, please visit:
    https://euromod-web.jrc.ec.europa.eu/download-euromod

EUROMOD:
-------------
is a tax-benefit microsimulation model for the European Union that enables 
researchers and policy analysts to calculate, in a comparable manner, 
the effects of taxes and benefits on household incomes and work incentives 
for the population of each country and for the EU as a whole.

Originally maintained, developed and managed by the Institute for Social and 
Economic Research (ISER) of the University of Essex, since 2021 EUROMOD 
is maintained, developed and managed by the Joint Research Centre (JRC) 
of the European Commission, in collaboration with Eurostat 
and national teams from the EU countries. 
===========================================================================
"""

# with open(os.path.join(MODEL_PATH,'VERSION.txt')) as f:
#     __version__ = f.readlines()[0] 
def _get_version() -> str:
    from importlib.metadata import version

    return version(__name__)


__version__ = _get_version()

