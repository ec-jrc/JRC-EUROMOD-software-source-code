# -*- coding: utf-8 -*-
"""
Created on Thu Oct 17 11:52:58 2024

@author: serruha
"""

from ._paths import  DLL_PATH
import os
import clr
clr.AddReference(os.path.join(DLL_PATH, "EM_Common.dll" ))
from EM_Common import Calculator,EMPath
import System as SystemCs

def evaluate_expression(expr_string,var_dict=dict()):
    calc = Calculator()
    expr = calc.CompileExpression(expr_string )
    necessary_vars = list(dict(calc._varExprCleaned).keys())
    
    lacking_var = set(necessary_vars) - set(var_dict.keys())
    if len(lacking_var) > 0:
        raise Exception(f"Following variables should be provided: {lacking_var}")
    if len(var_dict) > 0 :
        varindex = SystemCs.Collections.Generic.Dictionary[SystemCs.String,SystemCs.Int32]()
        varList = []
        for i,el in enumerate(var_dict.items()):
            varindex[el[0]] = i
            varList.append(el[1])
        varArray = SystemCs.Array[SystemCs.Double](varList)
        calc.SetVarIndex(varindex)
    else:
        varArray = SystemCs.Array[SystemCs.Double]([])
   
    return expr.ToDouble(varArray)


def is_valid_model(path):
    if type(path) != str:
        return False
    return EMPath(path,False).isValidModel()    
    