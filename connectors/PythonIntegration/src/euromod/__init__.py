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

import os 
import sys

# ### path to the euromod package
MODEL_PATH = os.path.dirname(__file__)
if MODEL_PATH not in sys.path:
    sys.path.insert(0, MODEL_PATH)
del os, sys, MODEL_PATH
    
from core import Model,Country,Policy,System,ReferencePolicy, PolicyInSystem, FunctionInSystem, Parameter, ParameterInSystem, Function, Dataset, DatasetInSystem, Extension
from info import Info
from base import ExtensionSwitch
from utils.euromod_parsing import is_valid_model

__all__ = ["Model",
           "Country",
           "System",
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
           "is_valid_model",
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

