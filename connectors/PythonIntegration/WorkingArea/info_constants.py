# -*- coding: utf-8 -*-
"""
Created on Thu Aug 21 13:56:59 2025

@author: serruha
"""

# -*- coding: utf-8 -*-
"""
Created on Wed Oct  9 15:40:51 2024

@author: serruha
"""


import sys
import os
sys.path.insert(0,r"C:\Users\serruha\source\EUROMOD\connectors\PythonIntegration\src")
os.environ["EUROMOD_PATH"] = r"C:\Users\serruha\source\EUROMOD\em_executable\EM_Executable\bin\x64\Release\netstandard2.0"
import clr as clr
from euromod.utils._paths import CWD_PATH, DLL_PATH
clr.AddReference(os.path.join(DLL_PATH, "EM_XmlHandler.dll" ))
from EM_XmlHandler import CountryInfoHandler,TAGS, ReadCountryOptions,ModelInfoHandler, ReadModelOptions, ExeXmlReader
from openpyxl import Workbook
import euromod as em
import os
model_path= r"R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\15 - Release checks\2025Q1\model\EUROMOD_MASTER_VERSION_J0.51"
mod = em.Model(model_path)
wb = Workbook()

for country in mod.countries[0:1]:
    pars = country.systems[-1].policies.find("functions.name","^(?!DefConst)",True ).find("parameters.value",r"\W\d+.?\d+#",True)
    pars2 = country.systems[-1].policies.find("functions.name","^(?!DefConst)",True ).find("parameters.value",r"\W\d{1,}\.\d{1,}",True)
    pars3 = country.systems[-1].policies.find("functions.name","^(?!DefConst)",True ).find("parameters.value",r"\W\d{3,}",True)
    all_pars = set(pars)  | set(pars2) | set(pars3)
    print(f"Potential constants for {country.name}")
    data= []
    wb.create_sheet(country.name)
    wb[country.name].append(["Order in Spine","Name Parameter","Value Parameter","Policy","Comment"])
    all_pars_list = list(all_pars)
    all_pars_list.sort(key=lambda x: x.spineOrder)
    for par in all_pars_list:
        if not par.parent.name in ["DefConst","DefVar"]:
            print(f"{par.spineOrder} : {par.name} : {par.value}")
            data = [par.spineOrder,par.name,par.value,par.parent.parent.name,par.comment]
            wb[country.name].append(data)
   

wb.save(os.path.join(model_path,"potentialConstants.xlsx"))

