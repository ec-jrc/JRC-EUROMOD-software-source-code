using EM_Common;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.GlobalAdministration;
using EM_UI.Tools;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.CountryAdministration
{
    internal partial class CountryAdministrator
    {
        static Dictionary<string, Country> _countries = null;

        static int _flagImageWidth = 30;
        static int _flagImageHeight = 20;
        static int _addOnImageWidth = 32;
        static int _addOnImageHeight = 32;

        static string _imageExtension = ".png";

        static internal Country GetCountry(string countryShortName)
        {
            if (!_countries.Keys.Contains(countryShortName.ToLower()))
                return null;
            return _countries[countryShortName.ToLower()];
        }

        internal static void ClearCountryList() { if (_countries != null) _countries.Clear(); }

        internal static List<Country> GetCountries(bool refresh = false) { return GetCountriesOrAddOns(false, refresh); }
        internal static List<Country> GetAddOns(bool refresh = false) { return GetCountriesOrAddOns(true, refresh); }
        static List<Country> GetCountriesOrAddOns(bool addOn, bool refresh = false)
        {
            if (_countries == null || refresh)
            {
                if (_countries == null)
                    _countries = new Dictionary<string, Country>();
                //_countries.Clear(); //outcommented, because this is a rather unsave approach: all not saved changes in any open country would be lost
                try
                {
                    //load countries
                    LoadCountriesOrAddOns(false);
                    //load add-ons
                    _oldAddOnFileStructure = IsOldAddOnFileStructure(); //check if add-ons are already stored in folders or still as single files directly in the add-ons-folder
                    if (ConsiderOldAddOnFileStructure())
                        LoadAddOns(); //different method for loading add-ons only necessary if "old style storage" ...
                    else
                        LoadCountriesOrAddOns(true); //... otherwise use same procedure as for loading countries
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception);
                }
            }
            return (from country in _countries where country.Value._isAddOn == addOn select country.Value).OrderBy(country => country._shortName).ToList<Country>();
        }

        static void LoadCountriesOrAddOns(bool addOns)
        {
            if (!Directory.Exists(addOns ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)))
                return; //may happen if user cancels the Open Project dialog

            DirectoryInfo folderContainingCountryFolders = new DirectoryInfo(addOns ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles));
            foreach (DirectoryInfo countryFolder in folderContainingCountryFolders.GetDirectories())
            {
                if ((countryFolder.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue; //avoid showing the .svn folder
                
                if (_countries.Keys.Contains(countryFolder.Name.ToLower()))
                    continue; //country already in countries list

                Country country = new Country();
                country._shortName = countryFolder.Name;
                string countryFilePath = EMPath.AddSlash(countryFolder.FullName);

                country._flag = GetFlag(country._shortName, addOns);
                country._isAddOn = addOns;

                _countries.Add(country._shortName.ToLower(), country);
            }
        }

        internal static void AddCountry(bool addOn = false)
        {
            AddCountryForm addCountryForm = new AddCountryForm(addOn);
            if (addCountryForm.ShowDialog() == DialogResult.Cancel)
                return;

            if (AddCountryOrAddOn(addCountryForm.GetCountryShortName(), addCountryForm.GetCountryLongName(), addCountryForm.GetFlagPathAndFileName(), addOn) == null)
                return;

            string cao = addOn ? "add-on" : "country";
            UserInfoHandler.ShowSuccess("'" + addCountryForm.GetCountryLongName() + "' successfully added!" + Environment.NewLine +
                "Go to " + cao + " gallery to load the new  " + cao + ".");
        }

        internal static void ImportCountry()
        {
            ImportCountryForm importCountryForm = new ImportCountryForm();
            if (importCountryForm.ShowDialog() == DialogResult.Cancel)
                return;

            try
            {
                //get info assesd via the dialog: short name of the country and where from to copy it into the countries folder
                string newShortName = importCountryForm.GetCountryShortName();
                string importCountryFolder = importCountryForm.GetImportCountryFolder();
                string oldShortName = new DirectoryInfo(importCountryFolder).Name;

                //create a country, which will allow for checking if import folder contains the necessary files, add the country to the country-administration and change the countries short-name in the xml-file
                Country country = new Country();
                country._shortName = oldShortName;
                country._isAddOn = false;

                //check whether country and data-configuration files exist in the import folder
                if (!File.Exists(EMPath.AddSlash(importCountryFolder) + country.GetCountryFileName()))
                {
                    UserInfoHandler.ShowError("Import folder '" + importCountryFolder + "' does not contain the file '" + country.GetCountryFileName() + "'.");
                    return;
                }
                if (!File.Exists(EMPath.AddSlash(importCountryFolder) + country.GetDataConfigFileName()))
                {
                    UserInfoHandler.ShowError("Import folder '" + importCountryFolder + "' does not contain the file '" + country.GetDataConfigFileName() + "'.");
                    return;
                }

                //copy the country folder to import and its content into the country folder and rename the files
                string newCountryFolder = EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + newShortName;
                Directory.CreateDirectory(newCountryFolder);
                foreach (FileInfo file in new DirectoryInfo(importCountryFolder).GetFiles())
                {
                    string fileName = file.Name.ToLower();
                    if (fileName.StartsWith(oldShortName.ToLower()))
                        fileName = newShortName + fileName.Substring(oldShortName.Length); //rename e.g. at.xml to a2.xml and at_dataconfig.xml to a2_dataconfig.xml (and perhaps at.png to a2.png)
                    file.CopyTo(EMPath.AddSlash(newCountryFolder) + fileName);
                }
                country._shortName = newShortName;

                //if another flag than that of the import country was indicated (and exists) copy it into country folder (overwritting the import country's flag)
                if (importCountryForm.GetFlagPathAndFileName() != string.Empty && File.Exists(importCountryForm.GetFlagPathAndFileName()))
                    File.Copy(importCountryForm.GetFlagPathAndFileName(), GetFlagPath(country._shortName, false) + country._shortName + _imageExtension, true);
                country._flag = GetFlag(country._shortName, false);
                
                //load the country file to correct the country's short name
                CountryConfigFacade countryConfigFacade = country.GetCountryConfigFacade(true, string.Empty,
                                        false); //function corrects country's short name if inconsistent (the 'false' stands for do not issue warning)

                //remark: do not add column for country specific description to variables file, as importing countries usually is not permanent

                //add country to country-administration
                _countries.Add(country._shortName.ToLower(), country);

                EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //update galleries of all loaded countries to show the imported country's flag

                // adapt global files for HICP and Exchangerate, if user requests
                if (importCountryForm.GetAdaptGlobal())
                {
                    CloneHICP(country._shortName, oldShortName, importCountryFolder);
                    Dictionary<string, string> sysNames = new Dictionary<string, string>(); // for import this could be just a list of system-names, but for SaveAs (which also uses the function) the system-names in the copy may be different
                    foreach (CountryConfig.SystemRow sys in countryConfigFacade.GetSystemRows()) sysNames.Add(sys.Name, sys.Name);
                    CloneExchangeRates(sysNames, country._shortName, oldShortName, importCountryFolder);
                }

                UserInfoHandler.ShowSuccess("'" + countryConfigFacade.GetCountryLongName() + "' successfully imported!" + Environment.NewLine + "Go to country gallery to load the country.");
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal static Country AddCountryOrAddOn(string shortName, string longName, string flagPathAndFileName = "", bool isAddOn = false)
        {
            DirectoryInfo storageFolder = isAddOn ? new DirectoryInfo(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)) : new DirectoryInfo(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles));
            Country country = null;
            try
            {
                if (!ConsiderOldAddOnFileStructure(isAddOn))
                    storageFolder.CreateSubdirectory(shortName); //create folder for new country

                country = new Country();
                country._shortName = shortName;
                if (flagPathAndFileName != string.Empty) //if user indicated an image for the flag/symol
                {
                    string flagFile = GetFlagPath(shortName, isAddOn) + shortName + CountryAdministrator._imageExtension;
                    if (!File.Exists(flagFile))
                        File.Copy(flagPathAndFileName, flagFile, true); //copy flag into country/add-on-folder (or in flag directory if old style add-on storage)
                }
                country._flag = GetFlag(shortName, isAddOn);
                country._isAddOn = isAddOn;
                _countries.Add(country._shortName.ToLower(), country);

                string storagePath = isAddOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles);
                if (!ConsiderOldAddOnFileStructure(isAddOn))
                    storagePath += shortName + "\\";
                CountryConfigFacade.AddCountryRowAndGenerateXML(longName, shortName, storagePath); //generalte cc.xml
                if (!isAddOn)
                {
                    DataConfigFacade.AddDataConfigAndGenerateXML(country.GetPath(), country.GetDataConfigFileName()); //generate cc_DataConfig.xml
                    EM_AppContext.Instance.GetVarConfigFacade().AddCountrySpecificDescriptions(shortName); //add (empty) country specific descriptions to VarConfig
                    EM_AppContext.Instance.GetVarConfigFacade().WriteXML();
                }
                else AddAOControl(country); //create the policy ao_control_xx for storing the add-on definitions

                EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //update galleries of all loaded countries
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return null;
            }
            return country;
        }

        private static void AddAOControl(Country addOn)
        {
            CountryConfigFacade addOnCountryConfigFacade = addOn.GetCountryConfigFacade();

            //create the policy for storing the add-on definitions: to be filled with functions AddOnApplic, AddOn_Pol, AddOn_Func and AddOn_Par
            CountryConfig.SystemRow addOnSystem = addOnCountryConfigFacade.AddFirstSystemRow(addOn._shortName);
            CountryConfig.PolicyRow addOnDefPolicy = addOnCountryConfigFacade.AddFirstPolicyRow(ExeXml.AddOn.POL_AO_CONTROL + addOn._shortName,
                                                     string.Empty, addOnSystem, DefPar.Value.ON);
            CountryConfig.FunctionRow addOnApplicFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.AddOn_Applic,
                                                            addOnDefPolicy, DefPar.Value.ON); //add the AddOnApplic function ...
            CountryConfig.ParameterRow addOnApplicParameter = CountryConfigFacade.AddParameterRowAtTail(addOnApplicFunction,
                DefPar.AddOn_Applic.Sys, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Applic, DefPar.AddOn_Applic.Sys));
            addOn.WriteXML();
        }

        internal static void DeleteCountry(bool addOn = false)
        {
            List<string> countriesOrAddOns = addOn ? (from country in GetAddOns() select country._shortName).ToList() : (from country in GetCountries() select country._shortName).ToList();
            DeleteCountryForm deleteCountryForm = new DeleteCountryForm(countriesOrAddOns);
            if (addOn)
                deleteCountryForm.Text = "Delete Add-Ons";
            if (deleteCountryForm.ShowDialog() == DialogResult.Cancel)
                return;

            try
            {
                foreach (string countryShortName in deleteCountryForm.GetSelectedCountries())
                {
                    if (!ConsiderOldAddOnFileStructure(addOn))
                    {
                        DirectoryInfo countryFolder = new DirectoryInfo((addOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + countryShortName);
                        countryFolder.Delete(true);
                        if (!addOn)
                        {
                            EM_AppContext.Instance.GetVarConfigFacade().DeleteCountrySpecificDescriptions(countryShortName); //remove country specific descriptions from VarConfig
                            EM_AppContext.Instance.GetVarConfigFacade().WriteXML();
                        }
                    }
                    else
                    {
                        File.Delete(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + countryShortName + ".xml");
                    }

                    if (GetCountry(countryShortName) != null)
                        _countries.Remove(countryShortName.ToLower());
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }

            if (deleteCountryForm.GetAdaptGlobal()) { AdaptHICP(); AdaptExchangeRates(); }

            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //update galleries of all loaded countries

            UserInfoHandler.ShowInfo(string.Join(", ", deleteCountryForm.GetSelectedCountries()) + " deleted.");

            void AdaptHICP()
            {
                try
                {
                    HICPConfigFacade hicpConfigFacade = EM_AppContext.Instance.GetHICPConfigFacade(false); if (hicpConfigFacade == null) return;
                    HICPConfig hicpConfig = hicpConfigFacade.GetHICPConfig();
                    foreach (HICPConfig.HICPRow delRow in from hicp in hicpConfig.HICP 
                             where deleteCountryForm.GetSelectedCountries().Contains(hicp.Country, StringComparer.OrdinalIgnoreCase)
                             select hicp) delRow.Delete();
                    hicpConfig.AcceptChanges();
                    hicpConfigFacade.WriteXML();
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception, "A problem occured in adapting the HICP global file. Please check manually whether settings are correct.", false);
                }
            }

            void AdaptExchangeRates()
            {
                try
                {
                    ExchangeRatesConfigFacade exRatesConfigFacade = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false); if (exRatesConfigFacade == null) return;
                    ExchangeRatesConfig exRatesConfig = exRatesConfigFacade.GetExchangeRatesConfig();
                    foreach (ExchangeRatesConfig.ExchangeRatesRow delRow in from exr in exRatesConfig.ExchangeRates
                             where deleteCountryForm.GetSelectedCountries().Contains(exr.Country, StringComparer.OrdinalIgnoreCase)
                             select exr) delRow.Delete();
                    exRatesConfig.AcceptChanges();
                    exRatesConfigFacade.WriteXML();
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception, "A problem occured in adapting the exchange rates global file. Please check manually whether settings are correct.", false);
                }
            }
        }

        internal static void SaveAs(string origShortName, string copyShortName, string copyLongName, string flagPathAndFileName,
                                    SaveAsAdaptOptions saveAsAdaptOptions)
        {
            Country originalCountry = GetCountry(origShortName);

            DirectoryInfo storageFolder = originalCountry._isAddOn ? new DirectoryInfo(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)) : new DirectoryInfo(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles));
            try
            {
                if (!ConsiderOldAddOnFileStructure(originalCountry._isAddOn))
                    storageFolder.CreateSubdirectory(copyShortName); //create folder for copied country

                Country copiedCountry = new Country();
                copiedCountry._shortName = copyShortName;
                copiedCountry._longName = copyLongName;
                copiedCountry._isAddOn = originalCountry._isAddOn;

                CountryConfigFacade countryConfigFacade = originalCountry.GetCountryConfigFacade();
                DataConfigFacade dataConfigFacade = originalCountry.GetDataConfigFacade();
                countryConfigFacade.GetCountryRow().ShortName = copiedCountry._shortName;
                countryConfigFacade.GetCountryRow().Name = copiedCountry._longName;

                copiedCountry.SetCountryConfigFacade(countryConfigFacade);
                copiedCountry.SetDataConfigFacade(dataConfigFacade);
                originalCountry.SetCountryConfigFacade(null);
                originalCountry.SetDataConfigFacade(null);

                //if another flag than that of the original country was indicated (and exists) copy it into country folder
                string flagPath = GetFlagPath(copiedCountry._shortName, copiedCountry._isAddOn) + copiedCountry._shortName + _imageExtension;
                if (flagPathAndFileName != string.Empty && File.Exists(flagPathAndFileName))
                    File.Copy(flagPathAndFileName, flagPath, true);
                else //otherwise copy the original country's flag
                    originalCountry._flag.Save(flagPath);
                copiedCountry._flag = GetFlag(copiedCountry._shortName, copiedCountry._isAddOn);

                // replace country-shortname in parameter-files (in prinicple replace *or_* by *cp_* and *_or* by *_cp*)
                Dictionary<string, string> ocSysNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (CountryConfig.SystemRow sys in countryConfigFacade.GetSystemRows()) ocSysNames.Add(sys.Name, sys.Name);
                if (saveAsAdaptOptions != null) AdaptContent(countryConfigFacade, dataConfigFacade, ref ocSysNames);

                //save country file and data-config file in new folder
                copiedCountry.WriteXML();
                _countries.Add(copiedCountry._shortName.ToLower(), copiedCountry);

                // copy HICP and ExchangeRates, if the original country has respective entries in these global files
                if (saveAsAdaptOptions != null && saveAsAdaptOptions.cloneHICP == true) CloneHICP(copyShortName, origShortName);
                if (saveAsAdaptOptions != null && saveAsAdaptOptions.cloneExRates == true) CloneExchangeRates(ocSysNames, copyShortName, origShortName);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return;
            }

            void AdaptContent(CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade, ref Dictionary<string, string> ocSysNames)
            {
                string or_ = $"{origShortName}_", _or = $"_{origShortName}", cp_ = $"{copyShortName}_", _cp = $"_{copyShortName}";
                foreach (CountryConfig.SystemRow sys in countryConfigFacade.GetSystemRows())
                {
                    // adapt system names: e.g. OR_2020 -> CP_2020, also adapt in db-system-rows
                    if (saveAsAdaptOptions.adaptSystemNames) { string origSysName = sys.Name; AdaptSystemName(sys); ocSysNames[origSysName] = sys.Name; }
                    // adapt system comment
                    if (saveAsAdaptOptions.adaptComments) sys.Comment = AdaptComment(sys.Comment);

                    // adapt all TU names (parameter DefTU/Name) and gather for replacement in parameters (see AdaptTUParameter)
                    Dictionary<string, string> tuNames = (saveAsAdaptOptions.adaptTUNames) ? AdaptTUNames(sys) : new Dictionary<string, string>();

                    foreach (CountryConfig.PolicyRow pol in sys.GetPolicyRows())
                    {
                        // adapt policy names: e.g. uprate_or -> uprate_cp
                        if (saveAsAdaptOptions.adaptPolicyNames) AdaptPolicyName(pol);
                        // adapt policy comment and private comment
                        if (saveAsAdaptOptions.adaptComments) { pol.Comment = AdaptComment(pol.Comment); pol.PrivateComment = AdaptComment(pol.PrivateComment); }

                        foreach (CountryConfig.FunctionRow fun in pol.GetFunctionRows())
                        {
                            // adapt function comment and private comment
                            if (saveAsAdaptOptions.adaptComments) { fun.Comment = AdaptComment(fun.Comment); fun.PrivateComment = AdaptComment(fun.PrivateComment); }

                            foreach (CountryConfig.ParameterRow par in fun.GetParameterRows())
                            {
                                // adapt output file names: e.g. or_2020_std -> cp_2020_std
                                if (saveAsAdaptOptions.adaptOutputFileNames) AdaptOutputFilenames(par);
                                // adapt parameter values using taxunits, e.g. TAX_UNIT, #_Level, UnitInfo_TU of DefOutput, ...
                                if (saveAsAdaptOptions.adaptTUNames) AdaptTUParameter(par, tuNames);
                                // adapt parameter comment and private comment
                                if (saveAsAdaptOptions.adaptComments) { par.Comment = AdaptComment(par.Comment); par.PrivateComment = AdaptComment(par.PrivateComment); }
                            }
                        }
                    }
                }

                void AdaptSystemName(CountryConfig.SystemRow sys)
                {
                    if (sys.Name.StartsWith(or_, StringComparison.OrdinalIgnoreCase))
                    {
                        string origName = sys.Name; sys.Name = cp_ + sys.Name.Substring(or_.Length);
                        foreach (DataConfig.DBSystemConfigRow dbSys in from ds in dataConfigFacade.GetDataConfig().DBSystemConfig where ds.SystemID == sys.ID select ds)
                            dbSys.SystemName = sys.Name;
                        foreach (CountryConfig.ConditionalFormat_SystemsRow cfsRow in from cfs in countryConfigFacade.GetCountryConfig().ConditionalFormat_Systems where cfs.SystemName.ToLower() == origName.ToLower() select cfs)
                            cfsRow.SystemName = sys.Name;
                        foreach (CountryConfig.ConditionalFormatRow cfRow in from cf in countryConfigFacade.GetCountryConfig().ConditionalFormat where cf.BaseSystemName.ToLower() == origName.ToLower() select cf)
                            cfRow.BaseSystemName = sys.Name;
                    }
                }

                void AdaptPolicyName(CountryConfig.PolicyRow pol)
                {
                    if (pol.Name.EndsWith(_or, StringComparison.OrdinalIgnoreCase))
                        pol.Name = pol.Name.Substring(0, pol.Name.Length - _or.Length) + _cp.ToLower();
                }

                void AdaptOutputFilenames(CountryConfig.ParameterRow par)
                {
                    if (TrimLow(par.FunctionRow.Name) != TrimLow(DefFun.DefOutput) || TrimLow(par.Name) != TrimLow(DefPar.DefOutput.File)) return;
                    if (par.Value.StartsWith(or_, StringComparison.OrdinalIgnoreCase))
                        par.Value = cp_.ToLower() + par.Value.Substring(or_.Length);
                }

                Dictionary<string, string> AdaptTUNames(CountryConfig.SystemRow sys)
                {
                    Dictionary<string, string> tuNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (CountryConfig.ParameterRow tuNamePar in from p in countryConfigFacade.GetCountryConfig().Parameter
                                                                     where p.FunctionRow.PolicyRow.SystemID == sys.ID &&
                                                                           TrimLow(p.FunctionRow.Name) == TrimLow(DefFun.DefTu) &&
                                                                           TrimLow(p.Name) == TrimLow(DefPar.DefTu.Name) select p)
                    {
                        string origName = tuNamePar.Value.Trim(); // only trim, but preserve upper/lower
                        if (!origName.EndsWith(_or, StringComparison.OrdinalIgnoreCase)) continue;
                        // change in parameter file
                        tuNamePar.Value = origName.Substring(0, origName.Length - _or.Length) + _cp.ToLower();
                        // add to Dictionary for later replacing parameters, which use the TU
                        tuNames.TryAdd(origName, tuNamePar.Value); // e.g. key=tu_FamBen_or, value=tu_FamBen_cp
                    }
                    return tuNames;
                }

                void AdaptTUParameter(CountryConfig.ParameterRow par, Dictionary<string, string> tuNames)
                {
                    if (TrimLow(par.Name) == TrimLow(DefPar.Common.TAX_UNIT) || 
                        TrimLow(par.Name) == TrimLow(DefPar.Footnote.Level) ||
                        (TrimLow(par.FunctionRow.Name) == TrimLow(DefFun.DefOutput) && TrimLow(par.Name) == TrimLow(DefPar.DefOutput.UnitInfo_TU)) ||
                        (TrimLow(par.FunctionRow.Name) == TrimLow(DefFun.UnitLoop) && TrimLow(par.Name) == TrimLow(DefPar.UnitLoop.Elig_Unit)) ||
                        (TrimLow(par.FunctionRow.Name) == TrimLow(DefFun.UpdateTu) && TrimLow(par.Name) == TrimLow(DefPar.UpdateTu.Name)) ||
                        (TrimLow(par.FunctionRow.Name) == TrimLow(DefFun.Store) && (TrimLow(par.Name) == TrimLow(DefPar.Store.Level) ||
                                                                                    TrimLow(par.Name) == TrimLow(DefPar.Store.Var_Level) || 
                                                                                    TrimLow(par.Name) == TrimLow(DefPar.Store.IL_Level))))
                        // Dictionary-key is not case sensitive, but trim still has to be done (see AdaptTUNames)
                        if (tuNames.ContainsKey(par.Value.Trim())) par.Value = tuNames[par.Value.Trim()];
                }

                string AdaptComment(string comment)
                {
                    if (string.IsNullOrEmpty(comment)) return comment;
                    for (int n = 0; n < 2; ++n)
                    {
                        string or = n == 0 ? _or : or_; string cp = n == 0 ? _cp : cp_;
                        // a case insensitive Replace (using RegEx seems to risky, as one needs to care about reserved chars)
                        for (int i = comment.IndexOf(or, StringComparison.OrdinalIgnoreCase); i >= 0;
                            i = comment.IndexOf(or, i + cp.Length, StringComparison.OrdinalIgnoreCase))
                        {
                            string matched_or = comment.Substring(i, or.Length);
                            comment = comment.Substring(0, i) + (matched_or.Equals(matched_or.ToLower()) ? cp.ToLower() : cp) + comment.Substring(i + or.Length);
                        }
                    }
                    return comment;
                }

                string TrimLow(string s) { return string.IsNullOrEmpty(s) ? string.Empty : s.ToLower().Trim(); }
            }
        }

        internal static CountryConfigFacade GetCountryConfigFacade(string countryShortName, bool enforceReading = false)
        {
            Country country = GetCountry(countryShortName);
            if (country != null)
                return country.GetCountryConfigFacade(false);
            return null;
        }

        internal static DataConfigFacade GetDataConfigFacade(string countryShortName, bool enforceReading = false)
        {
            Country country = GetCountry(countryShortName);
            if (country != null)
                return country.GetDataConfigFacade(enforceReading);
            return null;
        }

        internal static void WriteXML(string countryShortName, string alternativePath = "", string alternativeName = "")
        {
            Country country = GetCountry(countryShortName);
            if (country != null)
                country.WriteXML(alternativePath, alternativeName);
        }

        internal static string GetCountryFileName(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            if (country != null)
                return country.GetCountryFileName();
            return string.Empty;
        }

        internal static string GetDataFileName(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            if (country != null)
                return country.GetDataConfigFileName();
            return string.Empty;
        }

        internal static string GetCountryPath(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            return (country == null) ? string.Empty : country.GetPath();
        }

        internal static bool IsAddOn(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            return country != null && country._isAddOn;
        }

        internal static bool DoesCountryExist(string shortName)
        {
            return GetCountry(shortName) != null;
        }

        internal static int GetImageWidth(bool addOn)
        {
            return addOn ? _addOnImageWidth : _flagImageWidth;
        }

        internal static int GetImageHeight(bool addOn)
        {
            return addOn ? _addOnImageHeight : _flagImageHeight;
        }

        internal static Country GetCopyOfCountry(string countryShortName)
        {
            Country originalCountry = GetCountry(countryShortName);
            if (originalCountry == null)
                return null;

            Country copiedCountry = new Country();
            copiedCountry._shortName = countryShortName;
            copiedCountry.SetCountryConfigFacade(new CountryConfigFacade(GetCountryConfigFacade(countryShortName).GetCountryConfig()));
            copiedCountry.SetDataConfigFacade(new DataConfigFacade(GetDataConfigFacade(countryShortName).GetDataConfig()));

            return copiedCountry;
        }

        internal static void ResetConfigFacades(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            if (country == null)
                return;

            country.SetCountryConfigFacade(null);
            country.SetDataConfigFacade(null);
        }

        //TODO: this function should not be necessary !!! it allows, e.g. to remove countries from the gallery which do not exist any more
        //      it's (preliminary) purpose is to handle the problems caused by the outcommented "_countries.Clear();" in GetCountriesOrAddOns
        //      but this problem should be solved, respectively the respective comment reviewed
        internal static void RemoveCountryFromList(string countryShortName)
        {
            if (GetCountry(countryShortName) != null)
                _countries.Remove(countryShortName.ToLower());
        }

        internal static bool SaveWithLineBreaks(string path, string countryShortName)
        {
            try
            {
                string countryFolderPath = EMPath.AddSlash(path) + countryShortName;
                Directory.CreateDirectory(countryFolderPath);

                Country country = GetCountry(countryShortName);
                country.GetCountryConfigFacade();
                country.GetDataConfigFacade();
                country.WriteXML(countryFolderPath, string.Empty, true);
                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return false;
            }
        }

        internal static bool SaveAsTabDelimitedTextFile(string path, string countryShortName)
        {
            try
            {
                CountryConfigFacade countryConfigFacade = GetCountryConfigFacade(countryShortName);

                List<CountryConfig.SystemRow> systemRows = countryConfigFacade.GetSystemRowsOrdered();
                if (systemRows.Count == 0)
                    return true;

                System.IO.StreamWriter txtWriter = new System.IO.StreamWriter(EMPath.AddSlash(path) + countryShortName + ".txt");

                string headerLine = "TYPE\tNAME\tGROUP";
                foreach (CountryConfig.SystemRow systemRow in systemRows)
                    headerLine += "\t" + systemRow.Name;
                headerLine += "\tCOMMENT";
                txtWriter.WriteLine(headerLine);

                List<CountryConfig.PolicyRow> policyRows_sys1 = (from policy in systemRows[0].GetPolicyRows() select policy).OrderBy(parameter => long.Parse(parameter.Order)).ToList();
                foreach (CountryConfig.PolicyRow policyRow_sys1 in policyRows_sys1)
                {
                    string policyLine;
                    if (policyRow_sys1.ReferencePolID == null || policyRow_sys1.ReferencePolID == string.Empty)
                        policyLine = "POLICY\t" + policyRow_sys1.Name + "\t";
                    else
                        policyLine = "POLICY (REF)\t" + countryConfigFacade.GetPolicyRowByID(policyRow_sys1.ReferencePolID).Name + "\t";
                    foreach (CountryConfig.SystemRow systemRow in systemRows)
                        policyLine += "\t" + (CountryConfigFacade.GetTwinRow(policyRow_sys1, systemRow.ID) as CountryConfig.PolicyRow).Switch;
                    policyLine += "\t" + policyRow_sys1.Comment;
                    txtWriter.WriteLine(policyLine);

                    List<CountryConfig.FunctionRow> functionRows_sys1 = (from function in policyRow_sys1.GetFunctionRows() select function).OrderBy(parameter => long.Parse(parameter.Order)).ToList();
                    foreach (CountryConfig.FunctionRow functionRow_sys1 in functionRows_sys1)
                    {
                        string functionLine = "FUNCTION\t" + functionRow_sys1.Name + "\t";
                        foreach (CountryConfig.SystemRow systemRow in systemRows)
                            functionLine += "\t" + (CountryConfigFacade.GetTwinRow(functionRow_sys1, systemRow.ID) as CountryConfig.FunctionRow).Switch;
                        functionLine += "\t" + functionRow_sys1.Comment;
                        txtWriter.WriteLine(functionLine);

                        List<CountryConfig.ParameterRow> parameterRows_sys1 = (from parameter in functionRow_sys1.GetParameterRows() select parameter).OrderBy(parameter => long.Parse(parameter.Order)).ToList();
                        foreach (CountryConfig.ParameterRow parameterRow_sys1 in parameterRows_sys1)
                        {
                            string parameterLine = "Parameter" + "\t" + parameterRow_sys1.Name + "\t" + parameterRow_sys1.Group;
                            foreach (CountryConfig.SystemRow systemRow in systemRows)
                                parameterLine += "\t" + (CountryConfigFacade.GetTwinRow(parameterRow_sys1, systemRow.ID) as CountryConfig.ParameterRow).Value;
                            parameterLine += "\t" + parameterRow_sys1.Comment;
                            txtWriter.WriteLine(parameterLine);
                        }
                    }
                }

                txtWriter.Close();
                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return false;
            }
        }

        internal static Image GetFlag(string countryShortName)
        {
            Country country = GetCountry(countryShortName);
            return (country != null) ? country._flag : null;
        }

        static Image GetImageFromFile(string imageFilePath, Image replacementImage)
        { //this function does, other than Image.FromFile, not lock the image, which would not allow for deleting the image (in DeleteCountry)
            try
            {
                if (File.Exists(imageFilePath))
                    using (FileStream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(stream);
                    }
            }
            catch //catches if file does not exist or is no image
            {
            }
            return replacementImage;
        }

        internal static string ShowFlagSelectDialog(string currentFlagFilePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "portable network graphic (*.png)|*.png";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            return (openFileDialog.ShowDialog() != DialogResult.Cancel) ? openFileDialog.FileName : currentFlagFilePath;
        }

        internal static bool IsValidFlagFilePath(string flagFilePath)
        {
            if (File.Exists(flagFilePath) && Path.GetExtension(flagFilePath) == _imageExtension)
                return true;
            UserInfoHandler.ShowError("'" + flagFilePath + "' is not a valid path to a picture for the country's flag. (Note: only " + _imageExtension + "-files are allowed.)");
            return false;  
        }

        internal static bool ContainsEuromodFileStructure(string folder)
        {
            return Directory.Exists(Path.Combine(folder, EMPath.Folder_Countries_withoutPath()));
        }

        internal static string GenerateEuromodFileStructure(string path, string euromodFolderName = "EuromodFiles")
        {
            string newEuromodFolder = EMPath.AddSlash(path) + euromodFolderName;
            try
            {
                if (Directory.Exists(newEuromodFolder) && (Directory.GetFiles(newEuromodFolder).Count() > 0 || Directory.GetDirectories(newEuromodFolder).Count() > 0))
                {
                    UserInfoHandler.ShowError(string.Format("Folder '{0}' exists and is not empty.\n\nThe file structure for the new project requires an empty base-folder.", newEuromodFolder));
                    return string.Empty;
                }
                if (!(Directory.Exists(newEuromodFolder))) Directory.CreateDirectory(newEuromodFolder);
                if (!(Directory.Exists(EMPath.AddSlash(newEuromodFolder) + "Input")))
                    Directory.CreateDirectory(EMPath.AddSlash(newEuromodFolder) + "Input");
                if (!(Directory.Exists(EMPath.AddSlash(newEuromodFolder) + "Output")))
                    Directory.CreateDirectory(EMPath.AddSlash(newEuromodFolder) + "Output");
                if (!(Directory.Exists(EMPath.AddSlash(newEuromodFolder) + "Log")))
                    Directory.CreateDirectory(EMPath.AddSlash(newEuromodFolder) + "Log");
                EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
                if (!(Directory.Exists(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles), newEuromodFolder))))
                    Directory.CreateDirectory(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles), newEuromodFolder));
                if (!(Directory.Exists(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles), newEuromodFolder))))
                    Directory.CreateDirectory(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles), newEuromodFolder));
                if (!(Directory.Exists(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles), newEuromodFolder))))
                    Directory.CreateDirectory(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles), newEuromodFolder));
                if (!(Directory.Exists(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), newEuromodFolder))))
                    Directory.CreateDirectory(emPath.Folder_AtAlternativeEMPath(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), newEuromodFolder));
                /*string newImageFolder = emPath.Folder_AtAlternativeEMPath(new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages(), newEuromodFolder);
                if (!(Directory.Exists(newImageFolder))) Directory.CreateDirectory(newImageFolder);
                if (Directory.Exists(new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages()))
                    foreach (string imageFile in Directory.GetFiles(new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages()))
                        File.Copy(imageFile, newImageFolder + new FileInfo(imageFile).Name);*/
                return newEuromodFolder;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                if (Directory.Exists(newEuromodFolder)) Directory.Delete(newEuromodFolder, true);
                return string.Empty;
            }
        }

        internal static bool CopyCountry(string country, string fromFolder, string toFolder, out string errorMessage) { return CopyCAO(country, fromFolder, toFolder, false, out errorMessage); }
        internal static bool CopyAddOn(string addOn, string fromFolder, string toFolder, out string errorMessage) { return CopyCAO(addOn, fromFolder, toFolder, true, out errorMessage); }
        static bool CopyCAO(string shortName, string fromFolder, string toFolder, bool addOn, out string errorMessage)
        {
            try
            {
                EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
                string fromCAOFolder = emPath.Folder_AtAlternativeEMPath((addOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + shortName, fromFolder);
                string toCAOFolder = emPath.Folder_AtAlternativeEMPath((addOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + shortName, toFolder);
                if (!Directory.Exists(toCAOFolder)) Directory.CreateDirectory(toCAOFolder);
                bool success = Country.CopyXMLFiles(shortName, out errorMessage, fromCAOFolder, toCAOFolder, addOn ? 1 : 0);
                File.Copy(EMPath.AddSlash(fromCAOFolder) + shortName + _imageExtension, EMPath.AddSlash(toCAOFolder) + shortName + _imageExtension);
                return success;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        internal static bool CopyConfig(string config, string fromFolder, string toFolder, out string errorMessage)
        {
            try
            {
                EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
                File.Copy(emPath.File_AtAlternativeEMPath(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + config + ".xml", fromFolder),
                          emPath.File_AtAlternativeEMPath(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles)  + config + ".xml", toFolder));
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        private static void CloneHICP(string copyCountryShortName, string sourceCountryShortName, string sourceCountryFolder = null)
        {
            try
            {
                HICPConfigFacade copyHICPConfigFacade = EM_AppContext.Instance.GetHICPConfigFacade(false); if (copyHICPConfigFacade == null) return;
                HICPConfig copyHICPConfig = copyHICPConfigFacade.GetHICPConfig();

                HICPConfig sourceHICPConfig = copyHICPConfig; // for 'SaveAs' HICPs are cloned from the source country in the same HICP-file
                if (sourceCountryFolder != null) // for 'Import' HICPs are (possibly) cloned from a HICP-file in another EM-model
                {
                    HICPConfigFacade sourceHICPConfigFacade = GetExternalHICPConfigFacade(sourceCountryFolder); if (sourceHICPConfigFacade == null) return;
                    sourceHICPConfig = sourceHICPConfigFacade.GetHICPConfig();
                }

                List<Tuple<int, double, string>> clonedHICPs = new List<Tuple<int, double, string>>();
                foreach (HICPConfig.HICPRow hicp in sourceHICPConfig.HICP.Where(h => h.Country.ToLower() == sourceCountryShortName.ToLower()))
                    clonedHICPs.Add(new Tuple<int, double, string>(hicp.Year, hicp.Value, hicp.Comment));
                if (!clonedHICPs.Any()) return;

                foreach (Tuple<int, double, string> clonedHICP in clonedHICPs)
                {
                    if ((from hicp in copyHICPConfig.HICP. // make sure that country/year is unique: the HICP-file may already contain a HICP for this combination, in this case keep it
                         Where(h => h.Country.ToLower() == copyCountryShortName.ToLower() && h.Year == clonedHICP.Item1) select hicp).Any()) continue;
                    copyHICPConfig.HICP.AddHICPRow(copyCountryShortName, clonedHICP.Item1, clonedHICP.Item2, clonedHICP.Item3);
                }
                copyHICPConfigFacade.WriteXML();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "A problem occured in adapting the HICP global file. Please check manually whether settings are correct.", false);
            }
        }

        private static void CloneExchangeRates(Dictionary<string, string> copySourceSysNames, string copyCountryShortName, string sourceCountryShortName, string sourceCountryFolder = null)
        {
            try
            {
                ExchangeRatesConfigFacade copyExRatesConfigFacade = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
                ExchangeRatesConfig copyExRatesConfig = copyExRatesConfigFacade.GetExchangeRatesConfig();

                ExchangeRatesConfigFacade sourceExRatesConfigFacade = copyExRatesConfigFacade; // for 'SaveAs' rates are cloned from the source country in the same exchange-rates-file
                if (sourceCountryFolder != null) // for 'Import' rates are (possibly) cloned from an exchange-rate-file in another EM-model
                {
                    sourceExRatesConfigFacade = GetExternalExchangeRatesConfigFacade(sourceCountryFolder); if (sourceExRatesConfigFacade == null) return;
                }

                List<Tuple<double, double, double, double, string, string>> clonedExRates = new List<Tuple<double, double, double, double, string, string>>();
                foreach (ExchangeRatesConfig.ExchangeRatesRow exRate in sourceExRatesConfigFacade.GetExchangeRates(sourceCountryShortName))
                {
                    // adapt ValidFor: take care of system-names that were adapted from original short name to copy short name (e.g. OR_2020 -> CP_2020)
                    // (in principle that's only necessary vor 'SaveAs', as 'Import' does not (yet) offer this option)
                    string validFor = string.Empty;
                    foreach (string orSys in ExchangeRate.ValidForToList(exRate.ValidFor))
                        validFor = ExchangeRate.AddToValidFor(validFor, copySourceSysNames.ContainsKey(orSys) ? copySourceSysNames[orSys] : orSys);
                    clonedExRates.Add(new Tuple<double, double, double, double, string, string>(exRate.June30, exRate.YearAverage, exRate.FirstSemester, exRate.SecondSemester, exRate.Default, validFor));
                }

                // make sure that country/system is unique: the exchange-rate-file may already contain a rate for this combination, in this case keep it
                List<Tuple<double, double, double, double, string, string>> cleanedClonedExRates = new List<Tuple<double, double, double, double, string, string>>();
                foreach (Tuple<double, double, double, double, string, string> clonedExRate in clonedExRates)
                {
                    string cleanedValidFor = clonedExRate.Item6;
                    foreach (string sysName in ExchangeRate.ValidForToList(clonedExRate.Item6))
                    {
                        if ((from exr in copyExRatesConfigFacade.GetExchangeRates(copyCountryShortName) where ExchangeRate.ValidForToList(exr.ValidFor).Contains(sysName) select exr).Any())
                            cleanedValidFor = ExchangeRate.RemoveFromValidFor(cleanedValidFor, sysName);
                    }
                    if (cleanedValidFor.Trim() != string.Empty)
                        cleanedClonedExRates.Add(new Tuple<double, double, double, double, string, string>(clonedExRate.Item1, clonedExRate.Item2, clonedExRate.Item3, clonedExRate.Item4, clonedExRate.Item5, cleanedValidFor));
                }
                if (!cleanedClonedExRates.Any()) return;

                foreach (Tuple<double, double, double, double, string, string> clonedExRate in cleanedClonedExRates)
                    copyExRatesConfig.ExchangeRates.AddExchangeRatesRow(copyCountryShortName, clonedExRate.Item1, clonedExRate.Item2, clonedExRate.Item3, clonedExRate.Item4, clonedExRate.Item5, clonedExRate.Item6);
                copyExRatesConfigFacade.WriteXML();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "A problem occured in adapting the exchange rates global file. Please check manually whether settings are correct.", false);
            }
        }

        private static string GetEMPathFromCountryPath(string countryPath)
        {
            string ccPath = new EMPath(string.Empty).GetFolderCountries(true); // XMLParam\Countries
            int i = countryPath.ToLower().IndexOf(ccPath.ToLower()); 
            return i < 0 ? null : countryPath.Substring(0, i);
        }

        internal static HICPConfigFacade GetExternalHICPConfigFacade(string countryPath)
        {
            string emPath = GetEMPathFromCountryPath(countryPath);
            HICPConfigFacade hicpCF = new HICPConfigFacade(new EMPath(emPath).GetHICPFilePath(true));
            if (hicpCF == null) return null; hicpCF.LoadHICPConfig(); return hicpCF;
        }

        internal static ExchangeRatesConfigFacade GetExternalExchangeRatesConfigFacade(string countryPath)
        {
            string emPath = GetEMPathFromCountryPath(countryPath);
            ExchangeRatesConfigFacade exRateCF = new ExchangeRatesConfigFacade(new EMPath(emPath).GetExRatesFilePath(true));
            if (exRateCF == null) return null; exRateCF.LoadExchangeRatesConfig(); return exRateCF;
        }
    }
}
