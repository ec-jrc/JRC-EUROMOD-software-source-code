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

### path to the current user's working directory
CWD_PATH = os.getcwd()

### path to the euromod package
EUROMOD_PATH = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
if EUROMOD_PATH not in sys.path:
    sys.path.insert(0, EUROMOD_PATH)

### path to the root folder where euromod package is installed
ROOT_PATH =  os.path.dirname(EUROMOD_PATH)
# if ROOT_PATH not in sys.path:
#     sys.path.insert(0, ROOT_PATH)
if os.path.exists(r"C:\Program Files\EUROMOD\Executable"):
    DLL_PATH = "C:\Program Files\EUROMOD\Executable"
else:
    DLL_PATH = os.path.join(EUROMOD_PATH, "libs")
ENV_DLL_PATH = os.getenv('EUROMOD_PATH', "")
if ENV_DLL_PATH != "":
    print(f"Using EUROMOD as defined in {ENV_DLL_PATH}")
    DLL_PATH = ENV_DLL_PATH
#DLL_PATH = r"C:\Program Files\EUROMOD\Executable"
