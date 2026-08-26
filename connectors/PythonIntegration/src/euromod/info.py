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

import clr as clr
import os
from .utils._paths import CWD_PATH, DLL_PATH
clr.AddReference(os.path.join(DLL_PATH, "EM_XmlHandler.dll" ))
from EM_XmlHandler import CountryInfoHandler


def getInfoInString(info):
    return CountryInfoHandler.GetInfoInString(info)

class Info:
    def __init__(self,info):
        self.info = info

    def __repr__(self):  
        return (getInfoInString(self.info))

    def __getitem__(self,key):
        return self.info[key]
