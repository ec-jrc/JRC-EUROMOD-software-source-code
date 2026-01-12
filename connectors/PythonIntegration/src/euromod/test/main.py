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

import pandas as pd
import os

from core import Model


def simpleland():
    """
    Run simulation with default parameters for Simpleland.
    
    Example
    --------
    >>> from euromod import test
    >>> test.simpleland()
    """
    
    
    print('\n\nPlease provide the full path to the EUROMOD model root folder:\n')
    PATH_EUROMODFILES = input()
    PATH_EUROMODFILES = PATH_EUROMODFILES[1:-1] if PATH_EUROMODFILES[0]=="'" or PATH_EUROMODFILES[0]== '"' else PATH_EUROMODFILES
    
        
    ID_DATASET = 'sl_demo_v4.txt'
    PATH_DATA = os.path.join(PATH_EUROMODFILES,'Input',ID_DATASET)
    COUNTRY = 'SL'
    ID_SYSTEM = 'SL_1996'
        
    # load model
    mod =Model(PATH_EUROMODFILES)
    
    # display available countries in euromod
    # mod.countries
    # mod[COUNTRY].name
    
    # load country
    # mod[COUNTRY].load()
    # mod.countries[COUNTRY].load()
    
    
    print('\n',mod.countries[COUNTRY])
    print('     long name: Simpleland')
    print('--------------------')
    for sysobj in mod[COUNTRY].systems:
        print('System: ', sysobj.name)
        print('Best-match datasets:', [sysobj.bestmatch_datasets[i].name for i in range(len(sysobj.bestmatch_datasets))])
        print('Currency of parameters:', sysobj.currencyParam)
    
    
    # data=mod[COUNTRY].load_data(ID_DATASET, PATH_DATA = PATH_DATA)
    data = pd.read_csv(PATH_DATA,sep="\t")
    
    print('\nTesting simulation with default options...')
    out=mod[COUNTRY][ID_SYSTEM].run(data,ID_DATASET)
    print('\noutput:')
    print(out.outputs[0])
    
    print('\nSimulation successful!')
    
    
    # print('Testing simulation run with constantsToOverwrite...')
    # out = mod[COUNTRY][ID_SYSTEM].run(data,ID_DATASET,constantsToOverwrite = {("$f_h_cpi","2022"):'10000'})
    
    
    # print('Testing simulation run with addons...')
    # out = mod[COUNTRY][ID_SYSTEM].run(data,ID_DATASET,addons = [("LMA","LMA_"+COUNTRY)])
    
    # print('Testing simulation run with switches...')
    # out = mod[COUNTRY][ID_SYSTEM].run(data,ID_DATASET,switches = [("BTA",True)])

# if __name__ == "__main__":
#     simpleland()
