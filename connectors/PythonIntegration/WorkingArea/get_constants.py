# -*- coding: utf-8 -*-
"""
Created on Thu Aug 21 14:03:07 2025

@author: serruha
"""

import sys

sys.path.insert(0,r"C:\Users\serruha\source\EUROMOD\connectors\PythonIntegration\src")
constants = dict()
def get_by_spine_order(country_model,spine_order,sysIndex,constant_name):
    (pol_idx,fun_idx,par_idx) = spine_order.split('.')
    sysYear = country_model[sysIndex].year
    value = country_model[sysIndex].policies[int(pol_idx)-1].functions[int(fun_idx) -1].parameters[int(par_idx)-1].value
    if value in country_model.ct_factors.keys():
        if sysYear in country_model.ct_factors[value].values.keys():
            value = country_model.ct_factors[value].values[sysYear].value
    if value in constants:   
        value = constants[value][sysYear]
    if not constant_name in constants:
        constants[constant_name] = dict()
    constants[constant_name][sysYear] = value
        
        
    return value

from openpyxl import Workbook
import euromod as em
import os
model_path= r"R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\15 - Release checks\2025Q1\model\EuromodFiles_J0.52+"
mod = em.Model(model_path)
wb = Workbook()

for country in mod.countries[0:2]:
    pars = country.policies.find("functions.name","^DefConst",True ).find("parameters.name",r"^\$",True)
    print(f"Potential constants for {country.name}")
    data= []
    wb.create_sheet(country.name)
    wb[country.name].append(["Order in Spine","Name"] + [f"Value {x.year}" for x in country.systems] + ["Comment"])

    for par in pars:
        data = [par.spineOrder,par.name] + [get_by_spine_order(country,par.spineOrder,-len(country.systems) + i,par.name) for i in  range(0,len(country.systems))] + [par.comment]
        wb[country.name].append(data)
   

wb.save(os.path.join("c:/users/serruha","constants.xlsx"))


