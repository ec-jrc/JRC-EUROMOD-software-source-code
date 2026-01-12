# -*- coding: utf-8 -*-
"""
Created on Thu Sep  4 15:07:25 2025

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
import pandas as pd


def create_debug(df,nth_person=0 ,var=None,values=[]):
    """
    Copies a DataFrame len(values) times and shifts the specified ID columns by a large constant to make them unique.
    
    Parameters:
    - df: The original pandas DataFrame.
    - N: The number of times to copy the DataFrame.
    - input_ids: The list of column names that need to have unique IDs.
    - shift_constant: The constant used to shift the ID values.
    
    Returns:
    - A new DataFrame with N copies of the original, with unique IDs.
    """

    
    input_ids = ['idhh','idperson','idfather'	,'idmother','idpartner','idorighh','idorigperson']
    copies = []
    shift_constant = int(df.idperson.max()) +1
    for i in range(len(values)):
        # Create a copy of the original DataFrame
        df_copy = df.copy()

        # Shift the ID columns by a constant times the copy index to ensure uniqueness
        for id_col in input_ids:
            if id_col in df_copy.columns:
                df_copy[id_col] += shift_constant * i*(df_copy[id_col] > 0)
        df_copy.loc[nth_person,var] = values[i]
        # Append the modified copy to the list
        copies.append(df_copy)

    # Concatenate all the copies into a single DataFrame
    result_df = pd.concat(copies, ignore_index=True)

    return result_df



model_path= r"R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\15 - Release checks\2025Q1\model\EUROMOD_MASTER_VERSION_J0.51"
mod = em.Model(model_path)
repository = r"R:\B2\04 - EUROMOD JRC\01 - Repository\03 - Datasets\All data\All countries"
dataset = mod["HU"][-1].bestmatch_datasets[0].name
df = pd.read_csv(os.path.join(repository,dataset + ".txt"),sep="\t")

constantsToOverwrite = {("$tin_rate","1"): "20%"}

mod["CZ"]
mod["CZ"]["CZ_2023"].datasets
dataset = "CZ_2023_hhot"
dataset_path = os.path.join(repository, dataset + ".txt") # Get the path of the dataset
df = pd.read_csv(os.path.join(repository, dataset + ".txt"),sep="\t") #load the dataset to a panda dataset
debug_df = df.loc[df["sft_h"] == "Single"]
debug_df = create_debug(debug_df,0,"yem",[x for x in range(0,10000,50)])
sim = mod["CZ"]["CZ_2023"].run(debug_df,dataset,breakfun_id="990F36A7-79B5-4C5C-B353-125B953B87BD")
df_output = sim.outputs[0]

#create a graph that is plotting 



