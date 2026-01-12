# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import sys
import re
sys.path.insert(0,r"C:\Users\serruha\source\EUROMOD Development\connectors\PythonIntegration\src")
import euromod as em
mod = em.Model("C:\\Users\serruha\\Downloads\\EUROMOD_RELEASES_J0.1+\\EUROMOD_RELEASES_J0.1+")
import os
repository = r"C:\Users\serruha\DATA\2022 datasets"
from sympy.parsing.sympy_parser import parse_expr
count_footnote = 0
def preparse(string):
    # Define a dictionary mapping symbols to their operations
    operators = [
        (r"(?<![=|<|>|!])(=)(?!=)", "=="),
        (r"\&{2}", "&"),
        (r"\|{2}", "|"),
        (r"(!)(?!=)","~")
        ]
    for (pattern,repl ) in operators:
        string = re.sub(pattern,repl,string)
    operations = {
        '#y': "/12",
        '#d': "/365*12",
        '#m': '',
        # Add more symbols and operations here as needed
    }
    
    # Define a regex pattern that matches any of the keys in the operations dictionary
    pattern = r'(\d+(\.\d+)?)(\s*#\w)'
    
    # Define a replacement function
    def replacement(match):
        number = match.group(1)  # The number part of the match
        symbol = match.group(3)  # The symbol part of the match (e.g., #y or #d)
        if symbol.lstrip() in operations.keys():
            return f"({number}{operations[symbol.lstrip()]})"
        else:
           raise NotImplementedError(f"{symbol} not implemented")
        # You can add more conditions for other symbols if necessary
    
    footnote_pattern = r'(\w+)(#\d+)'
    def footnote_replacement(match):
        global count_footnote
        repl = f"footnote_place_holder{count_footnote}"
        count_footnote += 1
        return repl
        
    string = re.sub(footnote_pattern,footnote_replacement,string)
    
    
    string = string.replace('$',"CONST_")
    adjust_pattern = r"(?!\()([^\&|\|]+)(?!\))(\&|\|)"
    def adjust_logic_parsing(match):
        expression = match.group(1) 
        logic_operator = match.group(2)
        return f"({expression}) {logic_operator}"
    string = re.sub(adjust_pattern,adjust_logic_parsing,string)
    adjust_pattern2 = r"(\&|\|)(?!\()([^\&|\|]+)(?![\)|&|\|])"
    def adjust_logic_parsing2(match):
        expression = match.group(2) 
        logic_operator = match.group(1)
        return f"{logic_operator}({expression}) "
    string = re.sub(adjust_pattern2,adjust_logic_parsing2,string)
    # Perform the substitution with the replacement function
    return re.sub(pattern, replacement, string)
vars_to_consider = ["ddi","dcz","deh"]
vars_to_consider = ["ddi"]
exceptions = []

var = "ddi"
country = mod.countries[3]
print(f"Processing {country.name}")
pars = country.systems[-1].policies.find("functions.parameters.value",var,True)
if len(pars) == 0:
    print(f"{country.name} has no policy referring to the variable {var}")
else:
    for par in pars:
        try:
            print(parse_expr(preparse(f"{par.value} & ({var} == )"),evaluate=False).simplify())
        except Exception as e:
            print(f"{par.spineOrder}{par.name}: {par.value}")
            exceptions.append(f"{par.spineOrder}{par.name}: {par.value}, \n {e}")
                    



 
    
    
    

get_available_data()

df = pd.read_csv(os.path.join(repository,"ES_2021_b1.txt"),sep='\t')
t = time.time()
mod["ES"]["ES_2022"].run(df,"ES_2021_b1")
print(time.time() - t)