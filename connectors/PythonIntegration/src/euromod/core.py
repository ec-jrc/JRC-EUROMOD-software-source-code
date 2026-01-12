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
import pandas as pd
import polars as pl
import numpy as np
from utils.debug import create_debug_df
from utils._paths import CWD_PATH, DLL_PATH
from base import SystemElement, Euromod_Element, SpineElement
import clr as clr
import System as SystemCs
from utils.clr_array_convert import asNetArray,asNumpyArray
from utils.utils import is_iterable,convert_list_of_str,convert_numeric_columns_to_float64
clr.AddReference(os.path.join(DLL_PATH, "EM_Executable.dll" ))
from EM_Executable import Control
clr.AddReference(os.path.join(DLL_PATH, "EM_XmlHandler.dll" ))
from EM_XmlHandler import CountryInfoHandler,TAGS, ReadCountryOptions,ModelInfoHandler, ReadModelOptions
clr.AddReference(os.path.join(DLL_PATH, "EM_Common.dll" ))
from EM_Common import EMPath
clr.AddReference(os.path.join(DLL_PATH, "EM_Transformer.dll" ))
from EM_Transformer import EM3Global
from container import Container
from typing import Dict, Tuple, Optional, List
from utils.euromod_parsing import evaluate_expression


class Model(Euromod_Element):
    """
    Base class of the Euromod Connector instantiating the microsimulation model 
    EUROMOD.
    
    Parameters
    ----------
    model_path : :obj:`str`
        Path to the EUROMOD project.
        
    Returns
    -------
    Model
        A class containing the EUROMOD base model.
    
    Example
    --------
    >>> from euromod import Model
    >>> mod=Model("C:\\EUROMOD_RELEASES_I6.0+")
    
    .. _Documentation:
        https://github.com/euromod/PythonIntegration/HOWTO.pdf
    """
    
    
    def __init__(self, model_path : str):
        """
            :class:`Model` instance for the tax-benefit model EUROMOD.
        """
        _errors: SystemCs.Collections.Generic.List = SystemCs.Collections.Generic.List[SystemCs.String]()
        self._emPath: EMPath = EMPath(model_path,False)
        if not os.path.exists(self._emPath.GetExtensionsFilePath(False)) & os.path.exists(model_path):
            EM3Global.Transform(self._emPath.GetFolderEuromodFiles(), _errors, True)
        
        self.model_path: str = model_path 
        """: Path to the EUROMOD project."""
        
        ## EM3Translate if needed
        
        
        
        self._modelInfoHandler = ModelInfoHandler(self.model_path)
        self.extensions: Container[Extension] = Container(True)
        """: A :class:`Container` with :class:`Model` extensions."""
        for el in self._modelInfoHandler.GetModelInfo(ReadModelOptions.EXTENSIONS):
            ext = Extension(el.Value,self)
            self.extensions.add(ext.shortName,ext,ext.ID)
        self.countries: Container[Country] = CountryContainer() #: Container with `core.Country` objects
        """: A :class:`Container` with :class:`Country` objects."""
        self._hasMIH: bool = False;
        countries = os.listdir(os.path.join(model_path,'XMLParam','Countries'))
        self._load_country(countries)


    #def __repr__(self):
     #   return f"Model located in {self.model_path}"
                
      
        
    
            
    def _get_extension_info(self,country,system,dataset,ext_short_name):
        """
        Wrapper function to call the GetExtensionSwitchInfo method from C#.
    
        Parameters:
        - country: str
        - system: str
        - dataset: str
        - ext_short_name: str
    
        Returns:
          extensionSwitchInfo: dict
        """
        ret = self._modelInfoHandler.GetExtensionSwitchInfo(country,system,dataset,ext_short_name,None)
        if ret[0] != '':
            raise Exception(ret[0])
        else:
            return dict(ret[1])
        
    def _load_country(self, countries):        

    	### loop over countries to add the country containers
        for country in countries:            
            ### "Country" class is country specific
            self.countries.add(country,self)

    def __getitem__(self, country):
        return self.countries[country]
    
                


class Country(Euromod_Element):
    """Country-specific EUROMOD tax-benefit model.
    
    This class instantiates the EUROMOD tax benefit model for a given country.
    A class instance is automatically generated and stored in the attribute  
    :obj:`countries` of the base class :class:`Model`.
    
    This class contains subclasses of type :class:`System`, :class:`Policy`,
    :class:`Dataset` and :class:`Extension`.
    
    Parameters
    ----------
    country : :obj:`str`
        Name of the country. 
        Must be a two-letter country codes, see the Eurostat `Glossary:Country codes <https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Glossary:Country_codes>`_.
    model : :obj:`Model`
        A class containing the EUROMOD base model.
        
        
    Returns
    -------
    Country
        A class containing the EUROMOD country models.
    
    Example
    --------
    >>> from euromod import Model
    >>> mod=Model("C:\\EUROMOD_RELEASES_I6.0+")
    >>> mod.countries[0]    
    """

    def __init__(self,country: str,model: Model):        
        """Instance of the EUORMOD country-specific tax-benefit model.
        """
        self.name: str = country #: Two-letter country code
        """: Two-letters country code."""
        self.model: Model = model
        """":class:`Model` Returns the base :class:`Model` object."""
        self._hasCIH: bool = False
        self.upratefactors: Container[UprateFactor] | None = None
        """: A :obj:`Container` with :class:`UprateFactor` objects."""
        self.ct_factors: Container[ConsumptionTaxFactor] | None = None
        """: A :obj:`Container` with :class:`ConsumptionTaxFactor` objects."""
        self.systems: Container[System] | None = None #: Container with `core.System` objects
        """: A :obj:`Container` with :class:`System` objects."""
        self.policies: Container[Policy] | None = None #: Container with `core.Policy` objects
        """: A :obj:`Container` with :class:`Policy` objects."""
        self.datasets: Container[Dataset] | None = None #: Container with `core.Dataset` objects
        """: A :obj:`Container` with :class:`Dataset` objects."""
        self.local_extensions: Container[Extension] | None = None #: Container with `core.Extension` objects
        """: A :obj:`Container` with :class:`Extension` objects. These are the local extensions defined for the country."""
        self.extensions: Container[Extension] | None = None #: Container with `core.Extension` objects
        """: A :obj:`Container` with :class:`Extension` objects. These are the local + model extensions defined."""

        
    def _load(self):
        if not self._hasCIH:
            if not Control.TranslateToEM3(self.model.model_path, self.name, SystemCs.Collections.Generic.List[str]()):
                raise Exception("Country XML EM3 Translation failed. Probably provided a non-euromod project as an input-path.")
            self._countryInfoHandler = CountryInfoHandler(self.model.model_path, self.name)
            self._hasCIH = True;
    
    def __getattribute__(self,name):
        if name == "systems" and self.__dict__["systems"] is None:
            self._load()
            self._load_systems()
            return self.systems
        if name == "policies" and self.__dict__["policies"] is None:
            self._load()
            self._load_policies()
            return self.policies
        if name == "datasets" and self.__dict__["datasets"] is None:
            self._load()
            self._load_datasets()
            return self.datasets
        if name == "local_extensions" and self.__dict__["local_extensions"] is None:
            self._load()
            self._load_local_extensions()
            return self.local_extensions
        if name == "extensions" and self.__dict__["extensions"] is None:
            self._load()
            self._load_extensions()
            return self.extensions
        if name == "upratefactors" and self.__dict__["upratefactors"] is None:
            self._load()
            self._load_upratefactors()
            return self.upratefactors
        if name == "ct_factors" and self.__dict__["ct_factors"] is None:
            self._load()
            self._load_consumption_tax_factors()
            return self.ct_factors
        return super().__getattribute__(name)
    
        
    def _load_local_extensions(self):
        self.local_extensions = Container(True)
        for el in self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.LOCAL_EXTENSION):
            ext = Extension(el.Value,self)
            self.local_extensions.add(ext.shortName,ext,ext.ID)
    def _load_extensions(self):
        self.extensions = self.local_extensions + self.model.extensions
        
    
    def _load_policies(self):
        self.policies = Container(True)
        for el in self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.POL):
            pol = Policy(el.Value,self)
            pol.order = self._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_POL,self.systems[-1].ID + pol.ID)["Order"]
            self.policies.add(pol.ID,pol,pol.ID)
        for el in self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.REFPOL):
            ref_pol = ReferencePolicy(el.Value,self)
            self.policies.add(ref_pol.ID,ref_pol,ref_pol.ID)
            self.policies[-1].order = self._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_POL,self.systems[-1].ID + ref_pol.ID)["Order"]
        self.policies.containerList.sort(key=lambda x: int(x.order))
        
        
    def _load_datasets(self):
        self.datasets = Container(True)
        for el in self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.DATA):
            db = Dataset(el.Value,self)
            self.datasets.add(db.name,db,db.ID)
        
    def _load_systems(self):
        self.systems = Container(True)
        systems = self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.SYS)
        for sys in systems:
            self.systems.add(sys.Value["Name"],System(sys.Value,self),sys.Value["ID"])
    def _load_upratefactors(self):
        upratefactors = Container(True)

        upratefactorsList = self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.UPIND)
        for upf in upratefactorsList:
            upratefactors.add(upf.Value["Name"],UprateFactor(upf.Value,self),upf.Value["ID"])
        upratefactorsYearList = self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.UPIND_YEAR)
        for upf_yearCsharp in upratefactorsYearList:
            #if upf_yearCsharp.Value["UpIndID"] in upratefactors.keys():
            upf = upratefactors._get_by_id(upf_yearCsharp.Value["UpIndID"])
            upf.values.add(upf_yearCsharp.Value["Year"],UprateFactorYear(upf_yearCsharp.Value, upf))
            # else:
            #     continue
        self.upratefactors= upratefactors
    def _load_consumption_tax_factors(self):
        consumption_tax_factors = Container(True)
    
        # Retrieve the list of consumption tax factors
        consumption_tax_factors_list = self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.INDTAX)
        for ctf in consumption_tax_factors_list:
            # Create a new ConsumptionTaxFactor instance
            consumption_tax_factor = ConsumptionTaxFactor(ctf.Value, self)
            
            
            # Add the consumption tax factor to the container
            consumption_tax_factors.add(ctf.Value["NameITT"] + "_" + ctf.Value["CoicopCode"], consumption_tax_factor, ctf.Value["ID"])
    
        # Retrieve the list of yearly details for consumption tax factors
        consumption_tax_factors_year_list = self._countryInfoHandler.GetTypeInfo(ReadCountryOptions.INDTAX_YEAR)
        for ctf_year_csharp in consumption_tax_factors_year_list:
            # Check if the consumption tax factor ID exists in the container
            #if ctf_year_csharp.Value["IndTaxID"] in consumption_tax_factors.keys():
            ctf = consumption_tax_factors._get_by_id(ctf_year_csharp.Value["IndTaxID"])
            # Add the year-specific details to the IndTaxInfoContainer of the consumption tax factor
            ctf.values.add(
                ctf_year_csharp.Value["Year"],
                ConsumptionTaxFactorYear(
                    ctf_year_csharp.Value,
                    ctf)
                )
                
            # else:
            #     continue
        # Assign the populated container to an instance variable
        self.ct_factors = consumption_tax_factors
        
        
    def load_data(self, ID_DATASET, PATH_DATA = None):
        """
        Load data as a :class:`pandas.DataFrame` object.

        Parameters
        ----------
            ID_DATASET : :obj:`str` 
                        Name of the dataset excluding extension (Note: must be a `txt` file).   
            PATH_DATA : :obj:`str`, optional
                        Path to the dataset. Default is the folder "PATH_TO_EUROMOD_PROJECT/Input".
        
        Returns
        -------
        pandas.DataFrame 
            Dataset is returned as a :class:`pandas.DataFrame` object.
        """
        if PATH_DATA == None:
            PATH_DATA = os.path.join(self.model.model_path, 'Input')
            
        fname = ID_DATASET + ".txt"    
        df = pd.read_csv(os.path.join(PATH_DATA, fname),sep="\t")
        df.attrs[TAGS.CONFIG_ID_DATA] = ID_DATASET
        df.attrs[TAGS.CONFIG_PATH_DATA] = PATH_DATA
        return df
        
          
                        
    def __getitem__(self, system):
        return self.systems[system]
    
    

    def _short_repr(self):
        return f"Country {self.name}"
    def _container_middle_repr(self):
        return ""
    
    def get_switch_value(self,ext_name : Optional[str] = None,dataset_name : Optional[str] = None,sys_name : Optional[str] = None):
        """
        Get the configuration of the switch.

        Parameters
        ----------
        ext_name : :obj:`str` , optional
            Name of the extension. The default is None.
        dataset_name : :obj:`str` , optional
            Name of the dataset. The default is None.
        sys_name : :obj:`str`, optional
            Name of the system. The default is None.

        Raises
        ------
        KeyError
            Is raised if ext_name, dataset_name or sys_name, but is not configured in the model.

        Returns
        -------
        Container[ExtensionSwitch]
            Object containing information how the switch is configured.
            Note that there is only a value returned if the switch is either explicitly 'off' or 'on'.
            When it's configured as n/a in the model no value will be included.

        """
        patterns = SystemCs.Collections.Generic.List[SystemCs.String]()
        keys = SystemCs.Collections.Generic.List[SystemCs.String]()
        if ext_name is not None:
            try:
                ext_id = self.extensions[ext_name].ID
            except KeyError as e:
                raise KeyError(f"{ext_name} is not a configured extension in this model.") from e
            keys.Add(TAGS.EXTENSION_ID)
            patterns.Add(ext_id)
        if dataset_name is not None:
            try:
                dataset_id = self.datasets[dataset_name].ID
            except KeyError as e:
                raise KeyError(f"{dataset_name} is not a configured dataset in this model.") from e
            keys.Add(TAGS.DATA_ID)
            patterns.Add(dataset_id)
        if sys_name  is not None:
            try:
                sys_id = self.systems[sys_name].ID
            except KeyError as e:
                raise KeyError(f"{sys_name} is not a system in this model.") from e
            keys.Add(TAGS.SYS_ID)
            patterns.Add(sys_id)
        extension_switches = Container()
        
        for i,ext_switch in enumerate(self._countryInfoHandler.GetPiecesOfInfo(ReadCountryOptions.EXTENSION_SWITCH,keys,patterns)):
            extension_switches.add(i, ExtensionSwitch(ext_switch,self))
        return extension_switches
    
    
class ExtensionSwitch(Euromod_Element):
    """A class containing the extension switches of an object.
    
       This class is returned by :func:`~Country.get_switch_value` method and should not 
       be used by the user as a stand alone. 
       
       Returns
       -------
       ExtensionSwitch
           A class with relevant information on the extension switch.
    """
    
    def __init__(self,info,ctry):
        self.parent: Country
        """: The country-specific class."""
        self.value: str = ""
        """: Value of the switch as configured in EUROOMOD. """
        super().__init__(info,ctry)
        self.extension_name: str = ctry.extensions._get_by_id(info[TAGS.EXTENSION_ID]).shortName
        """: Short name of the extension."""
        self.sys_name: str = ctry.systems._get_by_id(info[TAGS.SYS_ID]).name
        """: Name of the applicable system."""
        self.data_name: str = ctry.datasets._get_by_id(info[TAGS.DATA_ID]).name
        """: Name of the applicable dataset."""
    def _container_middle_repr(self):
        ### Potential middle_representation of a string
        return f"{self.value}"
    def _container_begin_repr(self):
        return f"{self.extension_name}, {self.data_name}, {self.sys_name}"
    def _container_end_repr(self):
        return  ""
    def _short_repr(self):
        return f"{self.extension_name}, {self.data_name}, {self.sys_name}"
              
        

class CountryContainer(Container):
    """Container class storing Country objects.
    """
    def add(self,name,model):
        countryObject = Country(name,model)
        self.containerDict[name] = countryObject
        self.containerList.append(countryObject)


class UprateFactorYearsContainer(Container):
    """Container class storing Country objects.
    """
    def add(self,year,upf_year):
        self.containerDict[year] = upf_year
        self.containerList.append(upf_year)
        
class IndTaxInfoContainer(Container):
    """Container class storing Country objects.
    """
    def add(self,id,indTax):
        self.containerDict[id] = indTax
        self.containerList.append(indTax)

class Simulation(Euromod_Element):
    """Object storing the simulation results.
    
    This is a class containing results from the simulation :func:`~System.run` 
    and other related configuration information. 
    
    Returns
    -------
    Simulation
        A class with simulation output.
    """
    
    def __init__(self, out, constantsToOverwrite,polars):
        '''
        A class with results from the simulation :obj:`run`.
        
        Simulation results are stored as :class:`pandas.DataFrame` in the 
        '''  
        self.outputs: Container[pd.DataFrame] = OutputContainer()
        """: A :obj:`Container` with :class:`pandas.DataFrame`-type simulation results. 
            For indexing use an integer or a label from :obj:`output_filenames`."""
        self.output_filenames: list[str] | [] = []
        """ A :obj:`list` of file-names of simulation output."""
        if constantsToOverwrite is None:
            constantsToOverwrite = {}

        if (out.get_Item1()):
            dataDict = dict(out.get_Item2())
            variableNameDict = dict(out.get_Item3())
            for key in dataDict.keys():
                
                clr_arr = dataDict[key]
                temp = asNumpyArray(clr_arr)

                outputvars = list(variableNameDict[key])
                if not polars:
                    self.outputs.add(key, pd.DataFrame(temp, columns=outputvars))
                else:
                    self.outputs.add(key, pl.from_numpy(temp, outputvars))
                    
                self.output_filenames.append(key)

        self.errors: list[str] = [x.message for x in out.Item4]
        """: A :obj:`list` with errors and warnings from the simulation run."""
        
        self.constantsToOverwrite: dict[tuple(str,str),str] = constantsToOverwrite.copy()
        """: A :obj:`dict`-type object with user-defined constants.""" 




class System(Euromod_Element):   
    """A EUROMOD tax-benefit system.
    
    This class represents a EUROMOD tax system.
    Instances of this class are generated when loading the EUROMOD base model.
    These are collected in a :obj:`Container` as attribute `systems` of the :class:`Country`.
    
    Returns
    -------
    System
        A class with country systems.

    Example
    --------
    >>> from euromod import Model
    >>> mod=Model("C:\\EUROMOD_RELEASES_I6.0+")
    >>> mod.countries[0].systems[-1]
    """
    def __init__(self,*arg):
        self.parent: Country
        """: The country-specific class."""
        self.ID: str 
        """Identifier number of the system."""
        self.comment: str 
        """Comment specific to the system."""
        self.currencyOutput: str 
        """Currency of the simulation results."""
        self.currencyParam: str 
        """Currency of the monetary parameters in the system."""
        self.headDefInc: str 
        """Main income definition."""
        self.name: str 
        """Name of the system."""
        self.order: str 
        """System order in the spine."""
        self.private: str 
        """Access type."""
        self.year: str 
        """System year."""
        
        super().__init__(*arg)
        self.datasets: Container[DatasetInSystem] | None = None
        """: A :obj:`Container` of :class:`DatasetInSystem` objects in the system."""
        self.policies: Container[PolicyInSystem] | None = None
        """: A :obj:`Container` of :class:`PolicyInSystem` objects in the system."""
        self.bestmatch_datasets: Container[Dataset] | None = None
        """: A :obj:`Container` with best-match :class:`Dataset` objects in the system."""
    def __getattribute__(self,name):
        if name == 'policies' and self.__dict__["policies"] is None:
            self._load_policies()
            return self.policies
        if name == 'datasets' and self.__dict__["datasets"] is None:
            self._load_datasets()
            return self.datasets
        if name == 'bestmatch_datasets' and self.__dict__["bestmatch_datasets"] is None:
            self._load_bestmatchdatasets()
            return self.bestmatch_datasets
        
        return super().__getattribute__(name)
    def _load_bestmatchdatasets(self):
        self.bestmatch_datasets = Container()
        for x in self.datasets:
            if x.bestMatch == "yes":
                self.bestmatch_datasets.add(x.name,x)
                
    def get_default_extensions(self,dataset):
        """
        

        Parameters
        ----------
        dataset : str
            Name of Dataset.

        Returns
        -------
        extensions : dict[bool,str]
            key is name of the extension, value, is boolean indicating true for on, false for off

        """
        
        extensions = dict()
        for ext in self.parent.extensions:
            ext_info = self.parent.model._get_extension_info(self.parent.name, self.name,dataset,ext.shortName)
            if len(ext_info) == 0:
                extensions[ext.shortName] = False
            else:
                extensions[ext.shortName] = ext_info["Value"] == "on"
        return extensions  
    def _load_datasets(self):
        self.datasets = Container()
        for dataset in self.parent.datasets:
            id = self.ID + dataset.ID
            sysdata = self.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_DATA,id)
            if len(sysdata) > 0:
                self.datasets.add(id,DatasetInSystem(sysdata, id, self, dataset,self))

    def _load_policies(self):
        self.policies = Container()
        for pol in self.parent.policies:
            id = self.ID + pol.ID
            syspol = self.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_POL,id)
            self.policies.add(id,PolicyInSystem(syspol, id, self, pol,self))
    def _get_dataArray(self, df):
        ### check data format
        if type(df) == pd.core.frame.DataFrame:
            ### converting the numpy array to a DotNet/csharp array  
            df = df.select_dtypes(['number'])
            cols = [str(x) for x in df.columns]
            df = df.to_numpy(np.float64).T
        elif type(df) == pl.dataframe.frame.DataFrame:
            df = convert_numeric_columns_to_float64(df)
            cols = [str(x) for x in df.columns]
            df = df.to_numpy().T
        else:   
            raise TypeError("Parameter 'data' must be a pandas.core.frame.DataFrame.")
        dataArr=asNetArray(df)
        
        return (cols,dataArr)
       
        
    def _convert_configsettings(self, configSettings):
        ### check configSettings format
        if type(configSettings) != dict:
            raise TypeError("Parameter 'configSettings' must be dict.")
        ### Creation of csharp dictionary
        configSettingsDict = SystemCs.Collections.Generic.Dictionary[SystemCs.String,SystemCs.String]()
        for key,value in configSettings.items():
            configSettingsDict[SystemCs.String(key) ] = SystemCs.String(value)
        return configSettingsDict
    
    def _get_variables(self, columns):
        #### Initialise Csharp object    
        variables = SystemCs.Collections.Generic.List[SystemCs.String]()
        for col in columns:
            variables.Add(col)
        return variables
    
    def _get_constantsToOverwrite(self, new_constdict):

         ### check configSettings format    
        if new_constdict == None:
            constantsToOverwrite = new_constdict
        else:
            if type(new_constdict) == dict:
                constantsToOverwrite = SystemCs.Collections.Generic.Dictionary[SystemCs.Tuple[SystemCs.String, SystemCs.String],SystemCs.String]()
                for keys,value in new_constdict.items():
                    if not is_iterable(keys):
                        raise TypeError("Parameter 'constantsToOverwrite' must be a dictionary, with an iterable containing the constant name and groupnumber as key and a string as value (Example: {('$f_h_cpi','2022'):'1000'}).")
                    key1 = keys[0]
                    key2 = keys[1] if keys[1] != "" else "-2147483648" 

                        
                    csharpkey = SystemCs.Tuple[SystemCs.String, SystemCs.String](key1, key2)
                    constantsToOverwrite[csharpkey] = value

            else: 
                raise TypeError("Parameter 'constantsToOverwrite' must be a dictionary (Example: {('$f_h_cpi','2022'):'1000'}).")
        
        return constantsToOverwrite
    

                
    def _get_config_settings(self,dataset):
        configsettings = {}
        configsettings[TAGS.CONFIG_PATH_EUROMODFILES] = self.parent.model.model_path
        configsettings[TAGS.CONFIG_PATH_DATA] = ""
        configsettings[TAGS.CONFIG_PATH_OUTPUT] = ""
        configsettings[TAGS.CONFIG_ID_DATA] = dataset
        configsettings[TAGS.CONFIG_COUNTRY] = self.parent.name
        configsettings[TAGS.CONFIG_ID_SYSTEM] = self.name
        return configsettings
    @staticmethod
    def _get_new_output_requested(request_vars,requested_incomelists,requested_vargroups,requested_ilgroups):
        requested_vars_cs = convert_list_of_str(request_vars)
        requested_incomelists_cs = convert_list_of_str(requested_incomelists)
        requested_vargroups_cs = convert_list_of_str(requested_vargroups)
        requested_ilgroups_cs = convert_list_of_str(requested_ilgroups)
        ListCs = SystemCs.Collections.Generic.List[SystemCs.String]
        return SystemCs.Tuple[ListCs,ListCs,ListCs,ListCs](requested_vars_cs,requested_incomelists_cs,requested_vargroups_cs,requested_ilgroups_cs)
        

        
    def run(self,data: pd.DataFrame,dataset_id: str,
            constantsToOverwrite: Optional[Dict[Tuple[str, str], str]] = None,
            verbose: bool = True,outputpath: str = "",
            addons: List[Tuple[str, str]] = [],  switches: List[Tuple[str, bool]] = [],
            nowarnings=False,euro=False,public_components_only=False,
            requested_vars: List[str] = [],requested_incomelists: List[str] = [],
            requested_vargroups: List[str] = [],
            requested_ilgroups: List[str] = [],
            suppress_other_output: bool =False,
            breakfun_id: str = None,) -> Simulation:
        """Run the simulation of a EUROMOD tax-benefit system by passing the pandas or polars dataframe in memory to EUROMOD. Note that string variables will not be passed.
        

        Parameters
        ----------
        data : :class:`pandas.DataFrame`
            input dataframe passed to the EUROMOD model.
        dataset_id : :obj:`str`
            ID of the dataset.
        constantsToOverwrite : :obj:`dict` [ :obj:`tuple` [ :obj:`str`, :obj:`str` ], :obj:`str` ], optional
            A :obj:`dict` with constants to overwrite. Note that the key is a tuple of two strings, for which the first element is the name of the constant and the second is the groupnumber.
            Note that the values must be defined as strings.
            Default is :obj:`None`.
        verbose : :obj:`bool`, optional
            If True then information on the output will be printed. Default is :obj:`True`.
        outputpath : :obj:`str`, optional
            When the output path is provided, there will be anoutput file generated. Default is "".
        addons : :obj:`list` [ :obj:`tuple` [ :obj:`str`, :obj:`str` ]], optional
            List of tuples with addons to be integrated in the spine. The first element of the tuple is the name of the addon
            and the second element is the name of the system in the Addon to be integrated. Default is [].
        switches : :obj:`list` [ :obj:`tuple` [ :obj:`str`, :obj:`bool` ]], optional
            List of tuples with extensions to be switched on or of. The first element of the tuple is the short name of the extension.
            The second element is a boolean Default is [].
        nowarnings : :obj:`bool`, optional
            If True, the warning messages resulting from the simulations will be suppressed. Default is :obj:`False`.
        euro : :obj:`bool`, optional
            If True, the monetary variables will be converted to euro for the simulation. Default value is :obj:`False`.
        public_compoments_only : :obj:`bool`, optional
            If True, the the model will be on with only the public compoments. Default value is :obj:`False`.
        nowarnings : bool, optional
            If True then warnings returned by the model do not get printed. The default is False.
        requested_vars : List[str], optional
            Variables requested in a new separate output. The default is [].
        requested_incomelists : List[str], optional
            Income lists requested in a new separate output. The default is [].
        requested_vargroups : List[str], optional
            Vargroups requested in a new separate output. The default is [].
        requested_ilgroups : List[str], optional
            ilgroups requested in a new separate output. The default is [].
        suppress_other_output : bool, optional
            If True and custom output is specified then DefOutput from the model get suppressed. The default is False.
        
       
        Raises
        ------
        Exception
            Exception when simulation does not finish succesfully, i.e. without errors.

        Returns
        -------
        Simulation 
            A class containing simulation output and error messages.

        Example
        --------
        >>> # Load the dataset
        >>> import pandas as pd
        >>> data = pd.read_csv("C:\\EUROMOD_RELEASES_I6.0+\\Input\\sl_demo_v4.txt",sep="\t")
        >>> # Load EUROMOD
        >>> from euromod import Model
        >>> mod=Model("C:\\EUROMOD_RELEASES_I6.0+")
        >>> # Run simulation
        >>> out=mod.countries['SL'].systems['SL_1996'].run(data,'sl_demo_v4')
        """
 
        
        configSettings = self._get_config_settings(dataset_id)
        configSettings[TAGS.CONFIG_PATH_OUTPUT] = os.path.join(outputpath)
        
        if len(addons) > 0:
            for i,addon in enumerate(addons):
                if not is_iterable(addon):
                    raise(TypeError(str(type(addon)) + " is incorrect type for defining addon"))
                configSettings[TAGS.CONFIG_ADDON + str(i)] = addon[0] + "|" +  addon[1]
        if len(switches) > 0:
            for i,switch in enumerate(switches):
                if not is_iterable(switch):
                    raise(TypeError(str(type(switch)) + " is incorrect type for defining extension switch"))
                status = "on" if switch[1] else "off"
                configSettings[TAGS.CONFIG_EXTENSION_SWITCH + str(i)] = switch[0] + '=' +  status
        
        
        if type(data) == pl.dataframe.frame.DataFrame:
            polars=True
        else:
            polars = False
        ### check for euro boolean
        if euro:
            configSettings[TAGS.CONFIG_FORCE_OUTPUT_EURO] = "yes"
        if public_components_only:
            configSettings[TAGS.CONFIG_IGNORE_PRIVATE] = "yes"


        ### get Csharp objects
        (variables,dataArr) = self._get_dataArray(data)
        configSettings_ = self._convert_configsettings(configSettings)
        constantsToOverwrite_ = self._get_constantsToOverwrite(constantsToOverwrite)      
        variables = self._get_variables(variables)
        if len(requested_vars) + len(requested_incomelists) + len(requested_vargroups) > 0:
            new_requested_output = self._get_new_output_requested(requested_vars,requested_incomelists,requested_vargroups,requested_ilgroups)
        else:
            new_requested_output = None
        
        os.chdir(DLL_PATH)
        ### run system
        out = Control().RunFromPython(configSettings_, dataArr, variables, \
                                      constantsToOverwrite = constantsToOverwrite_,countryInfoHandler = self.parent._countryInfoHandler,suppressOtherOutput = suppress_other_output
                                      ,newOutput=new_requested_output,breakAfterFunId=breakfun_id)
        os.chdir(CWD_PATH)
        sim = Simulation(out, constantsToOverwrite,polars) 
        for error in out.Item4:
            if error.isWarning and verbose:
            	print(f"Warning: {error.message}")
            elif not error.isWarning:
                print(f"Error: {error.message}")
        if out.Item1:
            ### load "Simulations" Container
            if verbose:
                print(f"Simulation for system {self.name} with dataset {dataset_id} finished.")
        else:
            raise Exception(f"Simulation for system {self.name} with dataset {dataset_id} aborted with errors.")
      
        return sim
    
    
    def _short_repr(self):
        return f"{self.name}"
    def _container_middle_repr(self):
        return ""
class OutputContainer(Container):
    def add(self,name,data):
        self.containerDict[name] = data
        self.containerList.append(data)
    def __repr__(self):
        s= ""
        for i,el in enumerate(self.containerList):
            s += f"{i}: {repr(el)}\n"
        return s
    
        
class PolicyContainer(Container):
    def add(self,id,policy):
        self.containerDict[id] = policy
        self.containerList.append(policy)

class FunctionContainer(Container):     
    def add(self,id,function):
        self.containerDict[id] = function
        self.containerList.append(function)
        

class UprateFactorYear(Euromod_Element):
    def __init__(self,*args):
        self.ID: str
        """: Identifier UprateFactor"""
        self.value: str
        """: Value of uprating factor """
        self.year: str
        """: Year of uprating factor """
        self.parent: UprateFactor
        super().__init__(*args)
    def _short_repr(self):
        return f"{self.year: self.value}" 
    def _container_repr(self):
        return f"{self.year}"
    def _container_middle_repr(self):
        ### Potential middle_representation of a string
        return f"{self.value}"
    def _container_begin_repr(self):
        return f"{self.parent.name} ({self.year})"
    def _container_end_repr(self):
        return  ""

class UprateFactor(Euromod_Element):
    def __init__(self,*args):
        self.ID: str
        """: Identifier UprateFactor"""
        self.name: str
        """: Name of uprating factor """
        self.description: str
        """: Description of uprating factor """
        self.comment: str
        """: Comment of uprating factor """
        self.values: UprateFactorYearsContainer = UprateFactorYearsContainer()
        """: Container of the uprating values for the respective years """
        self.parent: Country
        super().__init__(*args) 
    
        
class ConsumptionTaxFactorYear(Euromod_Element):
    def __init__(self, *args):
        self.indTaxID: str
        """: Identifier for the individual tax in this year"""
        
        self.year: str
        """: Year associated with this tax factor"""
        
        self.value: str
        """: Tax factor value for the specified year"""
        
        self.parent: ConsumptionTaxFactor
        super().__init__(*args)
    def _short_repr(self):
        return f"{self.year}" 
    def _container_repr(self):
        return f"{self.year}"
    def _container_middle_repr(self):
        ### Potential middle_representation of a string
        return self.value
    def _container_begin_repr(self):
        return f"{self.parent.name} ({self.year})"
    def _container_end_repr(self):
        return  ""

class ConsumptionTaxFactor(Euromod_Element):
    def __init__(self, *args):
        self.ID: str
        """: Identifier for ConsumptionTaxFactor"""
        
        self.name: str
        """: Name of the consumption tax factor"""
        
        self.nameITT: str
        """: ITT Name of the consumption tax factor"""
        
        self.paramType: str
        """: Parameter type, e.g., VAT Rates"""
        
        self.unit: str
        """: Unit of the consumption tax factor, e.g., percentage"""
        
        self.coicopCode: str
        """: COICOP code related to consumption tax"""
        
        self.coicopVersion: str
        """: Version of the COICOP code"""
        
        self.coicopLabel: str
        """: Label for the COICOP code"""
        
        self.coicopCodeVersion: str
        """: Combined COICOP code and version"""
        
        self.comment: str
        """: Comment regarding the consumption tax factor"""
        
        self.values: IndTaxInfoContainer = IndTaxInfoContainer()
        """: Container for the consumption tax values for respective years"""
        
        self.parent: Country
        
        super().__init__(*args)




class Dataset(Euromod_Element):
    """Dataset available in a country model.
    
    This class contains the relevant information about a dataset.
    
    Returns
    -------
    Dataset
        A class with the country-specific dataset.
        
    """
    _objectType = ReadCountryOptions.DATA

    def _short_repr(self):
        return f"{self.name}"
    def _container_middle_repr(self):
        return ""
    def __init__(self,*args): 
        self.parent: Country
        """: The country-specific class."""
        self.ID: str 
        """: Dataset identifier number."""
        self.name: str 
        """: Name of the dataset."""
        self.yearCollection: str 
        """: Year of the dataset collection."""
        self.yearInc: str 
        """: Reference year for the income variables."""
        
        self.coicopVersion: str = ""
        """: COICOP  version."""
        self.comment: str = ""
        """: Comment  about the dataset."""
        self.currency: str = ""
        """: Currency of the monetary values in the dataset."""
        self.decimalSign: str = ""
        """: Decimal sign"""
        self.private: str = "no"
        """: Access type."""
        self.readXVariables: str = "no"
        """: Read variables."""
        self.useCommonDefault: str = "no"
        """: Use default."""
        self.system_elements: Container = Container()
        super().__init__(*args)      
        
        

class Policy(SpineElement):
    """Policy rules modeled in a country.
    
    Returns
    -------
    Policy
        A class with the country-specific policies.
        
    """
    _objectType = ReadCountryOptions.POL
    _extensionType = ReadCountryOptions.EXTENSION_POL
    def _load_functions(self):
        self.functions = FunctionContainer()
        functions = self.parent._countryInfoHandler.GetPiecesOfInfo(ReadCountryOptions.FUN,TAGS.POL_ID,self.ID)
        for fun in functions:
            self.functions.add(fun["ID"] ,Function(fun,self))
            self.functions[-1].order = self.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_FUN,self.parent.systems[0].ID + fun["ID"])["Order"]
        
        self.functions.containerList.sort(key=lambda x: int(x.order))

    def _container_middle_repr(self):
        ext = self._get_extension_repr()
        
        return f"{ext}"
    def _container_end_repr(self):
        
        comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        
        return f"{comment}"
    
    def __getattribute__(self, name):
        if name == "extensions" and self.__dict__["extensions"] is None:
            self._linkToExtensions()
            return self.extensions
        if name == "functions" and self.__dict__["functions"] is None:
            self._load_functions()
            return self.functions
        return super().__getattribute__(name)

    def __init__(self,*arg):
        self.parent: Country
        """: The country-specific class."""
        self.private: str = "no"
        """: Access type. Default is 'no'."""
        super().__init__(*arg)
        self.functions: Container[Function] | None = None
        """: A :obj:`Container` of policy-specific :class:`Function` objects."""
        self.extensions: Container[Extension] | None = None
        """: A :obj:`Container` of policy-specific :class:`Extension` objects."""
        
        self.ID: str 
        """Identifier number of the policy."""
        self.comment: str 
        """Comment specific to the policy."""
        self.name: str 
        """Name of the policy."""
        self.order: str 
        """Order of the policy in the specific spine."""
        self.spineOrder: str 
        """Order of the policy in the spine."""
    
    
class ReferencePolicy(SpineElement):
    """Object storing the reference policies.
    
    Returns
    -------
    ReferencePolicy
        A class with the country-specific reference policies.
        
    """
    _objectType = ReadCountryOptions.REFPOL
    def __init__(self,info,parent):
        self.parent: Country
        """The country-specific class."""
        super().__init__(info,parent) #take the parent constructor
        #get name of the reference policy using RefPolID
        self.name: str = self.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.POL,self.refPolID)["Name"]
        """: Name of the reference policy."""
        self.extensions: Container[Extension] | None = None
        """: A :obj:`Container` of reference policy-specific :class:`Extension` objects."""


    def _short_repr(self):
        return f"Reference Policy: {self.name}"
    def _container_middle_repr(self):
        return "Reference Policy"
    def _container_begin_repr(self):
        return f"Reference Policy: {self.name}"
    def __getattribute__(self, name):
        if name == "extensions" and self.__dict__["extensions"] is None:
            self._linkToExtensions()
            return self.extensions
        return super().__getattribute__(name)

           

class Function(SpineElement):
    """Functions implemented in a country policy.
    
    Returns
    -------
    Function
        A class with country-specific function.
    """
    _objectType = ReadCountryOptions.FUN
    _extensionType = ReadCountryOptions.EXTENSION_FUN

    def _short_repr(self):
        ext = self._get_extension_repr()
        return f"{self.name}{ext}"
    def _container_middle_repr(self):
        ext = self._get_extension_repr()
        return ext
    def _container_end_repr(self):
        comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        return  comment
    
    def _load_parameters(self):
        self.parameters = Container()
        parameters = self.parent.parent._countryInfoHandler.GetPiecesOfInfo(ReadCountryOptions.PAR,TAGS.FUN_ID,self.ID) #Returns an Iterable of Csharp Dictionary<String,String>
        for par in parameters:
            self.parameters.add(par["ID"] ,Parameter(par,self))
            self.parameters[-1].order = self.parent.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_PAR,self.parent.parent.systems[0].ID + par["ID"])["Order"]
        self.parameters.containerList.sort(key=lambda x: int(x.order))
    
    def __getattribute__(self, name):
        if name == "extensions" and self.__dict__["extensions"] is None:
            self._linkToExtensions()
            return self.extensions
        if name == "parameters" and self.__dict__["parameters"] is None:
            self._load_parameters()
            return self.parameters
        return super().__getattribute__(name)
    def __init__(self,*arg):
        self.parent: Policy
        """The class of the country-specific policy."""
        super().__init__(*arg)
        self.parameters: Container[Parameter] | None = None
        """: A :obj:`Container` of :class:`Parameter` objects in a country."""
        self.extensions: Container[Extension] | None = None
        """: A :obj:`Container` of :class:`Extension` objects in a country."""
        
        self.ID: str 
        """Identifier number of the function."""
        self.comment: str
        """Comment specific to the function."""
        self.name: str
        """Name of the function."""
        self.order: str
        """Order of the function in the specific spine."""
        self.polID: str
        """Identifier number of the reference policy."""
        self.private: str 
        """Access type."""
        self.spineOrder: str
        """Order of the function in the spine."""        


class Parameter(SpineElement):
    """Parameters set up in a function.
    
    Returns
    -------
    Parameter
        A class with country-specific parameter.
    """
    _objectType = ReadCountryOptions.PAR
    _extensionType = ReadCountryOptions.EXTENSION_PAR
    def _container_middle_repr(self):
        ext = self._get_extension_repr()
        return f"{ext}"
    def _container_end_repr(self):
        comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        return  f"{comment}"
    def __getattr__(self,name):
        if name == "extensions":
            self._linkToExtensions()
            return self.extensions
        raise AttributeError();   
    def __getattribute__(self, name):
        if name == "extensions" and self.__dict__["extensions"] is None:
            self._linkToExtensions()
            return self.extensions

        return super().__getattribute__(name)
    def __init__(self,*arg):
        self.parent: Function
        """The class of the country-specific function."""
        self.group: str = ""
        """str: Parameter group value."""
        super().__init__(*arg)
        self.extensions: Container[Extension] | None = None
        """: A :obj:`Container` with :class:`Extension` objects."""
        
        self.ID: str 
        """Identifier number of the parameter."""
        self.comment: str 
        """Comment specific to the parameter."""
        self.funID: str 
        """Identifier number of the reference function at country level."""
        self.name: str 
        """Name of the parameter."""
        self.order: str 
        """Order of the parameter in the specific spine."""
        self.spineOrder: str 
        """Order of the parameter in the spine."""


class PolicyInSystem(SystemElement):
    """Policy rules modeled in a system.
    
    Returns
    -------
    PolicyInSystem
        A class with system-specific policies.
    """
    _objectType = ReadCountryOptions.SYS_POL
    def __init__(self,*arg):
        self.parent: Country
        """The country-specific class."""
        super().__init__(*arg)
        self.functions: Container[FunctionInSystem] | None = None
        """: A :obj:`Container` with :class:`FunctionInSystem` objects specific to the system"""

        self.private: str 
        """: Access type. Default is 'no'."""
        self.extensions: Container[Extension]
        """: A :obj:`Container` of policy-specific :class:`Extension` objects."""
        self.ID: str
        """Identifier number of the policy."""
        self.comment: str
        """Comment specific to the policy."""
        self.name: str 
        """Name of the policy."""
        self.order: str 
        """Order of the policy in the specific spine."""
        self.spineOrder: str 
        """Order of the policy in the spine."""
        self.polID: str 
        """Identifier number of the reference policy at country level."""
        self.sysID: str 
        """Identifier number of the reference system."""
        self.switch: str 
        """Policy switch action."""

    def _container_middle_repr(self):
        ext = self._get_extension_repr()
        return f"{self.switch}{ext}" 
    def _container_end_repr(self):
        if type(self.parentTypeObject) == Policy:
            comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        else:
            comment = ""
        return  f"{comment}"
    def __getattribute__(self, name):
        if name == "functions" and self.__dict__["functions"] is None:
            self._load_functions()
            return self.functions
        return super().__getattribute__(name)

        
    def _load_functions(self):
        self.functions = FunctionContainer()
        sys = self.parentSystem
        for fun in self.parentTypeObject.functions:
            id = sys.ID + fun.ID
            sysfun = self.parentSystem.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_FUN,id)
            self.functions.add(id,FunctionInSystem(sysfun, id, sys, fun,self))
            
        
class ParameterInSystem(SystemElement):
    """Parameters set up in a function for a specific system.
    
    Returns
    -------
    ParameterInSystem
        A class with the system-specific function parameter.
    """
    parent: Function
    """The class of the country-specific function."""
    group: str 
    """Parameter group number."""
    extensions: Container #list # Container[Extension]
    """: A :obj:`Container` with :class:`Extension` objects."""
    ID: str
    """Identifier number of the parameter."""
    comment: str
    """Comment specific to the parameter."""
    funID: str 
    """Identifier number of the reference function at country level."""
    name: str 
    """Name of the parameter."""
    order: str 
    """Order of the parameter in the specific spine."""
    spineOrder: str 
    """Order of the parameter in the spine."""
    parID: str 
    """Identifier number of the reference parameter at country level."""
    sysID: str 
    """Identifier number of the reference system."""
    value: str 
    """Value of the parameter."""
    
    _extensionType = ReadCountryOptions.EXTENSION_PAR
    _ctryOption = ReadCountryOptions.SYS_PAR

    def _short_repr(self):
        return f"{self.parentTypeObject.name}" 
    def _container_middle_repr(self):
        return f"{self.value}" 
    def _container_end_repr(self):
        comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        return  f"{comment}"
    
    def evaluate(self,var_dict=dict()):
        return evaluate_expression(self.value,var_dict)
class DatasetInSystem(SystemElement):
    """Datasets available in a system model.
    
    Returns
    -------
    DatasetInSystem
        A class with the system-specific dataset.
    """
    parent: Country
    """The country specific class."""
    ID: str 
    """: Dataset identifier number."""
    bestMatch: str 
    """: If yes, the current dataset is a best match for the specific system."""
    coicopVersion: str 
    """: COICOP  version."""
    comment: str 
    """: Comment  about the dataset."""
    currency: str 
    """: Currency of the monetary values in the dataset."""
    dataID: str 
    """: Identifier number of the reference dataset at the country level."""
    decimalSign: str 
    """: Decimal sign"""
    name: str 
    """: Name of the dataset."""
    private: str 
    """: Access type."""
    readXVariables: str 
    """: Read variables."""
    sysID: str 
    """: Identifier number of the reference system."""
    useCommonDefault: str 
    """: Use default."""
    yearCollection: str 
    """: Year of the dataset collection."""
    yearInc: str 
    """: Reference year for the income variables."""
        
    _ctryOption = ReadCountryOptions.SYS_DATA
    def _container_middle_repr(self):
        if self.bestMatch == "yes":
            return  "best match"
        else:
            return ""
    
class FunctionInSystem(SystemElement):
    """Functions implemented in a policy for a specific system.
    
    Returns
    -------
    FunctionInSystem
        A class with the system-specific function.
    """
    _ctryOption = ReadCountryOptions.SYS_FUN 
    def __init__(self,*arg):
        self.parent: Policy
        """The class of the country-specific policy."""
        super().__init__(*arg)
        self.parameters: Container[ParameterInSystem] | None = None
        """: A :obj:`Container` with :class:`ParameterInSystem` objects specific to a function."""
        
        
        self.ID: str
        """Identifier number of the function."""
        self.comment: str
        """Comment specific to the function."""
        self.funID: str 
        """Identifier number of the reference function at country level."""
        self.name: str 
        """Name of the function."""
        self.order: str 
        """Order of the function in the specific spine."""
        self.polID: str
        """Identifier number of the reference policy."""
        self.private: str 
        """Access type."""
        self.spineOrder: str
        """Order of the function in the spine."""
        self.switch: str 
        """: Policy switch action."""
        self.sysID: str
        """: Identifier number of the reference policy."""
        self.extensions: Container[Extension] 
        """: A :obj:`Container` of :class:`Extension` objects in a system."""

    
    def _container_middle_repr(self):
        ext = self._get_extension_repr()
        return f"{self.switch}{ext}" 
    def _container_end_repr(self):
        comment = self.comment if len(self.comment) < 50 else self.comment[:50] + " ..."
        return  f"{comment}"
    def __getattribute__(self, name):
        if name == "parameters" and self.__dict__["parameters"] is None:
            self._load_parameters()
            return self.parameters
        return super().__getattribute__(name)
    def _load_parameters(self):
       self.parameters = Container()
       sys = self.parentSystem
       for par in self.parentTypeObject.parameters:
           id = sys.ID + par.ID
           syspar = self.parentSystem.parent._countryInfoHandler.GetPieceOfInfo(ReadCountryOptions.SYS_PAR,id)
           self.parameters.add(id,ParameterInSystem(syspar, id, sys, par,self))
           
    

class Extension(Euromod_Element):
    """EUROMOD extensions. 
    
    Returns
    -------
    Extension
        A class with the model extensions.
        
    """
    def __init__(self,*arg):
        self.parent: Model
        """The model base class."""
        self.name: str = None
        """Long name of the extension."""
        self.shortName: str  = None
        """Short name of the extension."""
        super().__init__(*arg)
    _objectType = ReadModelOptions.EXTENSIONS
    #def __repr__(self):
     #   return f"Extension: {self.name}" 

    def _short_repr(self):
        return f"{self.shortName}" 
    def _container_middle_repr(self):
        return  ""

    
    


    
