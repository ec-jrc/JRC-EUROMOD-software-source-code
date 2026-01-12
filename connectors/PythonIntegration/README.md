# *Euromod Connector*

![python](https://img.shields.io/pypi/pyversions/euromod)
[![Static Badge](https://img.shields.io/badge/0.1.20a-blu?label=euromod&color=blu)](https://pypi.org/project/euromod/)
[![Documentation Status](https://img.shields.io/readthedocs/euromod?label=Read%20the%20Docs&logo=read-the-docs)](http://pythonconnectoreuromod.readthedocs.io/)

## What is _Euromod_?

The Euromod Connector for Python is built to facilitate and simplify the usage of the [EUROMOD](https://euromod-web.jrc.ec.europa.eu "https://euromod-web.jrc.ec.europa.eu") microsimulation model for research and analysis purposes. 

EUROMOD is a tax-benefit microsimulation model for the European Union that enables researchers and policy analysts to calculate, in a comparable manner, the effects of taxes and benefits on household incomes and work incentives for the population of each country and for the EU as a whole. It is a static microsimulation model that applies user-defined tax and benefit policy rules to harmonised microdata on individuals and households, calculates the effects of these rules on household income. 


## Installation 
Install via [PyPi](https://test.pypi.org/project/euromod/)  using _pip_:
```bash
$ pip install euromod
```

### Requirements
The Euromod Connector requires two EUROMOD components: 1) the model (coded policy rules) , and 2) the input microdata with the variables that respect the EUROMOD naming conventions.
For more information, please, read the sections "Model" and "Input microdata" on the [Download Euromod](https://euromod-web.jrc.ec.europa.eu/download-euromod "https://euromod-web.jrc.ec.europa.eu/download-euromod") web page.


## Run simulation

Importing and loading the model:

```python
In[1]: from euromod import Model
In[2]: mod=Model(r"C:\EUROMOD_RELEASES_I6.0+")
```

Loading the dataset using pandas:
```python
In[3]: import pandas as pd
In[4]: data = pd.read_csv(r"C:\EUROMOD_RELEASES_I6.0+\Input\sl_demo_v4.txt",sep="\t")
```

Running a simulation on system 'SL_1996' of the country Simpleland 'SL' (Note: this country model is provided by default with the EUROMOD project):
```python
In[5]: out=mod.countries['SL'].systems['SL_1996'].run(data,'sl_demo_v4')
In[6]: out.outputs[0]
```
```python
Out[6]:
       idhh  idperson  idmother  ...    ils_dispy  il_taxabley  il_bsa_base
0       1.0     101.0       0.0  ...   807.018500      0.00000    807.01850
1       1.0     102.0       0.0  ...     0.000000      0.00000      0.00000
2       1.0     103.0     102.0  ...     0.000000      0.00000      0.00000
3       1.0     104.0     102.0  ...   934.294772    149.54786    149.54786
4       2.0     201.0       0.0  ...  1337.268280   1421.58535   1337.26828
    ...       ...       ...  ...          ...          ...          ...
1255  500.0   50003.0   50002.0  ...     0.000000      0.00000      0.00000
1256  500.0   50004.0   50002.0  ...     0.000000      0.00000      0.00000
1257  500.0   50005.0   50002.0  ...     0.000000      0.00000      0.00000
1258  500.0   50006.0       0.0  ...   839.845300      0.00000    839.84530
1259  500.0   50007.0       0.0  ...     0.000000      0.00000      0.00000

[1260 rows x 43 columns]
```

Checking the policies for country Simpleland 'SL':
```python
In[7]: mod.countries['SL'].policies
```
```python
Out[7]:
0: Uprate_sl            |    DEF: UPRATING FACTORS 
1: ILsDef_sl            |    DEF: STANDARD INCOME CONCEPTS 
2: ILDef_sl             |    DEF: SPECIFIC INCOME CONCEPTS 
3: TUDef_sl             |    DEF: ASSESSMENT UNITS 
4: yem_sl               |    DEF: Minimum Wage 
5: neg_sl               |    DEF: recode negative self-employment income to zer ... 
6: sic_sl               |    SIC: Social Insurance Contributions 
7: bch_sl               |    BEN: Child Benefit 
8: tin_sl               |    TAX: Income Tax 
9: bsa_sl               |    BEN: Social Assistance 
10: output_std_sl       |    DEF: STANDARD OUTPUT INDIVIDUAL LEVEL 
11: output_std_hh_sl    |    DEF: STANDARD OUTPUT HOUSEHOLD LEVEL 
```


Checking the policies for country Simpleland 'SL' in system 'SL_1996':
```python
In[8]: mod.countries['SL'].systems['SL_1996'].policies
```
```python
Out[8]:
0: Uprate_sl             | on        |    DEF: UPRATING FACTORS 
1: ILsDef_sl             | on        |    DEF: STANDARD INCOME CONCEPTS 
2: ILDef_sl              | on        |    DEF: SPECIFIC INCOME CONCEPTS 
3: TUDef_sl              | on        |    DEF: ASSESSMENT UNITS 
4: yem_sl                | on        |    DEF: Minimum Wage 
5: neg_sl                | on        |    DEF: recode negative self-employment income to zer ... 
6: sic_sl                | on        |    SIC: Social Insurance Contributions 
7: bch_sl                | on        |    BEN: Child Benefit 
8: tin_sl                | on        |    TAX: Income Tax 
9: bsa_sl                | on        |    BEN: Social Assistance 
10: output_std_sl        | on        |    DEF: STANDARD OUTPUT INDIVIDUAL LEVEL 
11: output_std_hh_sl     | off       |    DEF: STANDARD OUTPUT HOUSEHOLD LEVEL 
```
 Getting information about a specific policy, e.g. "sic_sl", at a country level:
```python
In[9]: mod.countries['SL'].policies[6]
```
```python
Out[9]:
------------------------------
Policy
------------------------------
	 ID: '20901CF5-0A2A-4BA8-A18A-7092CD6A182D'
	 comment: 'SIC: Social Insurance Contributions'
	 extensions: 0 elements
	 functions: SchedCalc, SchedCalc, SchedCalc
	 name: 'sic_sl'
	 order: '7'
	 private: 'no'
	 spineOrder: '7'
```

 Getting information about a specific policy, e.g. "sic_sl", at a system level:
```python
In[9]: mod.countries['SL'].systems['SL_1996'].policies[6]
```
```python
Out[9]:
------------------------------
PolicyInSystem
------------------------------
	 ID: 'F7E5CACE-CECC-4BB6-9841-A936D097548120901CF5-0A2A-4BA8-A18A-7092CD6A182D'
	 comment: 'SIC: Social Insurance Contributions'
	 extensions: 0 elements
	 functions: SchedCalc, SchedCalc, SchedCalc
	 name: 'sic_sl'
	 order: '7'
	 polID: '20901CF5-0A2A-4BA8-A18A-7092CD6A182D'
	 private: 'no'
	 spineOrder: '7'
	 switch: 'on'
	 sysID: 'F7E5CACE-CECC-4BB6-9841-A936D0975481'
```


## License

&copy; Copyright 2024 European Commission. EUROMOD is licensed under the [EUPL, Version 1.2](https://joinup.ec.europa.eu/software/page/eupl). See the
[documentation](http://pythonconnectoreuromod.readthedocs.io/) for details.

