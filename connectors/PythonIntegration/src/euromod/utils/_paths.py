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

### path to the current user's working directory
CWD_PATH = os.getcwd()

### path to the euromod package (used to locate the bundled engine below)
EUROMOD_PATH = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))

### path to the root folder where euromod package is installed
ROOT_PATH =  os.path.dirname(EUROMOD_PATH)
### engine bundled with the package (the fallback engine)
BUNDLED_PATH = os.path.join(EUROMOD_PATH, "libs")

### resolve the candidate engine directory: explicit env override > system install > bundled
ENV_DLL_PATH = os.getenv('EUROMOD_PATH', "")
_SYSTEM_INSTALL = r"C:\Program Files\EUROMOD\EUROMOD"
if ENV_DLL_PATH != "":
    _candidate, _explicit = ENV_DLL_PATH, True
elif os.path.exists(_SYSTEM_INSTALL):
    _candidate, _explicit = _SYSTEM_INSTALL, False
else:
    _candidate, _explicit = BUNDLED_PATH, False

### Use the candidate engine if it is compatible (>= required software version),
### otherwise fall back to the bundled engine. See utils._version.resolve_engine.
from ._version import resolve_engine, REQUIRED_SOFTWARE_VERSION
DLL_PATH, SOFTWARE_VERSION, USING_BUNDLED_FALLBACK = resolve_engine(
    _candidate, BUNDLED_PATH, is_explicit=_explicit)
