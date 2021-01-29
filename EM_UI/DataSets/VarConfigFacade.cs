using EM_Common;
using EM_UI.Tools;
using EM_UI.VariablesAdministration.VariablesManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DataSets
{
    internal class VarConfigFacade
    {
        Dictionary<string, string> _variables_NamesAndAutoLabels = null;
        Dictionary<string, Dictionary<string, string>> _variables_NamesAndDescriptions_ByCountry = null;

        List<string> _guidsOfNewRows = new List<string>();

        string _pathVarConfig = string.Empty;

        const string _noDescription = "-";

        internal VarConfig _varConfig = null;

        static readonly string[] _cDataElements = new string[] { "LongName", "ShortName", "Name", "Description", "AutoLabel", "Label", "NamePattern" };

        internal VarConfigFacade(string pathVarConfig)
        {
            _pathVarConfig = pathVarConfig;
        }

        internal VarConfigFacade()
        {
            _pathVarConfig = new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true);
            LoadVarConfig();
        }

        internal bool LoadVarConfig()
        {
            try
            {
                if (_varConfig == null)
                {
                    _varConfig = new VarConfig();
                    using (StreamReader streamReader = new StreamReader(_pathVarConfig, DefGeneral.DEFAULT_ENCODING))
                        _varConfig.ReadXml(streamReader);
                    _varConfig.AcceptChanges();
                }
                return true;
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
                return false;
            }
        }

        internal VarConfig GetVarConfig() { return _varConfig; }

        internal void WriteXML(string alternativePath = "")
        {
            try
            {
                if (alternativePath == string.Empty) alternativePath = new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true);
                Stream fileStream = new FileStream(alternativePath, FileMode.Create);
                using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING, _cDataElements))
                    _varConfig.WriteXml(xmlWriter);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        internal bool IsVariableName(string Name)
        {
            return ((from variable in _varConfig.Variable where variable.Name.ToLower() == Name.ToLower() select variable).ToList().Count > 0);
        }

        internal List<VarConfig.VariableRow> GetVariables()
        {
            return (from variable in _varConfig.Variable select variable).ToList();
        }

        internal List<VarConfig.VariableRow> GetVariablesSortedByName()
        {
            return (from variable in _varConfig.Variable select variable).OrderBy(variable => variable.Name).ToList();
        }

        Dictionary<string, string> GetVariables_NamesAndAutoLabels()
        {
            if (_variables_NamesAndAutoLabels == null)
            {
                _variables_NamesAndAutoLabels = new Dictionary<string, string>();
                foreach (VarConfig.VariableRow variable in _varConfig.Variable.Rows)
                    _variables_NamesAndAutoLabels.Add(variable.Name, variable.AutoLabel);
            }
            return _variables_NamesAndAutoLabels;
        }

        internal List<string> GetVariables_Names()
        {
            return GetVariables_NamesAndAutoLabels().Keys.ToList();
        }

        internal List<VarConfig.VariableRow> GetVariablesWithCountrySpecificDescription(string countryShortName = "")
        {
            return (from countryLabel in _varConfig.CountryLabel //select names and labels for this country
                    where (countryShortName == string.Empty || countryLabel.Country.ToLower() == countryShortName.ToLower())
                        & countryLabel.Label != string.Empty
                        & countryLabel.Label != _noDescription
                    select countryLabel.VariableRow).ToList();
        }

        internal Dictionary<string, string> GetVariables_NamesAndDescriptions(string countryShortName = "")
        {//store variables and country specific descriptions once assessed to speed up the process (currently variables are not changed by the UI)
            GetVariables_NamesAndAutoLabels(); //first get auto labels, as they are needed if there is no country specific description
            
            if (countryShortName == string.Empty)
                return _variables_NamesAndAutoLabels;

            if (_variables_NamesAndDescriptions_ByCountry == null)
                _variables_NamesAndDescriptions_ByCountry = new Dictionary<string, Dictionary<string, string>>(); //function called for the first time

            Dictionary<string, string> countryVariables_NamesAndDescriptions = null;
            if (_variables_NamesAndDescriptions_ByCountry.Keys.Contains(countryShortName.ToLower()))
                countryVariables_NamesAndDescriptions = _variables_NamesAndDescriptions_ByCountry[countryShortName.ToLower()]; //not first call for this country: return stored list
            else //first call for this country
            {
                List<VarConfig.CountryLabelRow> countryLabelRows = (from countryLabel in _varConfig.CountryLabel //select names and labels for this country
                                                                    where countryLabel.Country.ToLower() == countryShortName.ToLower()
                                                                    select countryLabel).ToList();
                if (countryLabelRows.Count == 0)
                    countryVariables_NamesAndDescriptions = _variables_NamesAndAutoLabels; //there are no country specific variables descriptions
                                                                                           //(happens if country is added with new interface, as long as variable administration is not yet integrated)
                else
                {
                    countryVariables_NamesAndDescriptions = new Dictionary<string, string>();
                    foreach (VarConfig.CountryLabelRow countryLabelRow in countryLabelRows)
                    {
                        string description = string.Empty;
                        if (countryLabelRow.Label != string.Empty && countryLabelRow.Label != _noDescription)
                            description = countryLabelRow.Label;
                        else //if no country specific label use auto label
                        {
                            if (_variables_NamesAndAutoLabels.Keys.Contains(countryLabelRow.VariableRow.Name))
                                description = _variables_NamesAndAutoLabels[countryLabelRow.VariableRow.Name];
                        }
                        if (!countryVariables_NamesAndDescriptions.ContainsKey(countryLabelRow.VariableRow.Name)) countryVariables_NamesAndDescriptions.Add(countryLabelRow.VariableRow.Name, description);
                    }
                }
                if (!_variables_NamesAndDescriptions_ByCountry.ContainsKey(countryShortName.ToLower())) _variables_NamesAndDescriptions_ByCountry.Add(countryShortName.ToLower(), countryVariables_NamesAndDescriptions);
            }              

            return countryVariables_NamesAndDescriptions;
        }

        internal string GetDescriptionOfVariable(string variableName, string countryShortName = "")
        {
            List<string> description = null;
            if (countryShortName != string.Empty)
            {
                description = (from countryLabel in _varConfig.CountryLabel
                               where countryLabel.VariableRow.Name.ToLower() == variableName.ToLower() && countryLabel.Country.ToLower() == countryShortName.ToLower()
                               select countryLabel.Label).ToList();
                if (description.Count > 0 && description.First() != _noDescription)
                    return description.First(); //if country specific description available return ...
                //... else return autolabel
            }
            description = (from variable in _varConfig.Variable where variable.Name.ToLower() == variableName.ToLower() select variable.AutoLabel).ToList();
            if (description.Count > 0)
                return description.First();
            return string.Empty;
        }

        internal void RefreshVariables_NamesAndDescriptions()
        {
            _variables_NamesAndAutoLabels = null;
            _variables_NamesAndDescriptions_ByCountry = null;
        }

        internal VarConfig.VariableRow GetVariableByID(string ID)
        {
            var query = from v in _varConfig.Variable where v.ID == ID select v;
            return query.Count() == 1 ? query.First() : null;
        }

        internal VarConfig.VariableRow GetVariableByName(string name)
        {
            var query = from v in _varConfig.Variable where v.Name == name select v;
            return query.Count() > 0 ? query.First() : null;
        }

        internal VarConfig.CountryLabelRow GetCountryLabelByID(string ID)
        {
            var query = from cl in _varConfig.CountryLabel where cl.ID == ID select cl;
            return query.Count() == 1 ? query.First() : null;
        }

        internal VarConfig.AcronymTypeRow GetAcronymTypeByID(string ID)
        {
            var query = from act in _varConfig.AcronymType where act.ID == ID select act;
            return query.Count() == 1 ? query.First() : null;
        }

        internal VarConfig.AcronymLevelRow GetAcronymLevelByID(string ID)
        {
            var query = from acl in _varConfig.AcronymLevel where acl.ID == ID select acl;
            return query.Count() == 1 ? query.First() : null;
        }

        internal VarConfig.AcronymRow GetAcronymByID(string ID)
        {
            var query = from ac in _varConfig.Acronym where ac.ID == ID select ac;
            return query.Count() == 1 ? query.First() : null;
        }

        internal VarConfig.CategoryRow GetCategoryByKey(string acroID, string value)
        {
            var query = from cat in _varConfig.Category where cat.AcronymID == acroID && cat.Value == value select cat;
            return query.Count() == 1 ? query.First() : null;
        }

        internal void AddCountrySpecificDescriptions(string countryShortName)
        {
            if ((from countryLabel in _varConfig.CountryLabel
                 where countryLabel.Country.ToLower() == countryShortName.ToLower()
                 select countryLabel).ToList().Count > 0)
                return; //country specific descriptions already exist

            foreach (VarConfig.VariableRow variableRow in _varConfig.Variable.Rows) //add an empty ("-") description for the country to each variable
                _varConfig.CountryLabel.AddCountryLabelRow(Guid.NewGuid().ToString(), variableRow, countryShortName, _noDescription);
            Commit();
        }

        internal void DeleteCountrySpecificDescriptions(string countryShortName)
        {
            foreach (VarConfig.CountryLabelRow countryLabelRow in (from countryLabel in _varConfig.CountryLabel
                                                                   where countryLabel.Country.ToLower() == countryShortName.ToLower()
                                                                   select countryLabel).ToList())
                countryLabelRow.Delete();
            Commit();
        }

        internal bool SetCountrySpecificDescription(VarConfig.VariableRow variableRow, string countryShortName, string description)
        {
            List<VarConfig.CountryLabelRow> countryLabelRows = (from countryLabel in variableRow.GetCountryLabelRows()
                                                               where countryLabel.Country.ToLower() == countryShortName.ToLower()
                                                               select countryLabel).ToList();
            if (countryLabelRows.Count != 1)
                return false; //country specific descriptions does not exist (or is not unique, which shouldn't happen)

            countryLabelRows.ElementAt(0).Label = description;
            return true;
        }

        internal void CopyCountryLabelRow(VarConfig.CountryLabelRow originalDescription, VarConfig.VariableRow parentVariable)
        {
            _varConfig.CountryLabel.AddCountryLabelRow(originalDescription.ID, parentVariable, originalDescription.Country, originalDescription.Label);
        }

        internal List<VarConfig.AcronymTypeRow> GetAllAcronyms()
        {
            return (from acronymType in _varConfig.AcronymType select acronymType).ToList();
        }

        internal int GetAcronymsCount()
        {
            return _varConfig.Acronym.Count();
        }

        internal List<VarConfig.AcronymTypeRow> GetAcronymTypesSortedByShortName()
        {
            return (from acronymType in _varConfig.AcronymType select acronymType).OrderBy(acronymType => acronymType.ShortName).ToList();
        }

        internal List<string> GetTypeShortNames(bool toLower = false)
        {
            List<string> acroTypes = new List<string>();

            foreach (VarConfig.AcronymTypeRow typeRow in _varConfig.AcronymType.Rows)
            {
                string shortName = typeRow.ShortName;
                if (toLower)
                    shortName = shortName.ToLower();
                for (int i = 0; i < shortName.Length; ++i)
                    acroTypes.Add(shortName.Substring(i, 1)); //takes account of e.g. B/P
                //add not really existing identfier for id-variables (idperson, idhh, ...)
                acroTypes.Add(toLower ? AcronymManager.IDENTIFIER_TYPE.ToLower() : AcronymManager.IDENTIFIER_TYPE);
            }
            
            return acroTypes;
        }

        internal List<VarConfig.AcronymRow> GetAcronymsOfType(string typeShortName)
        {
            return (from acronym in _varConfig.Acronym
                    where acronym.AcronymLevelRow.AcronymTypeRow.ShortName.ToLower().Contains(typeShortName.ToLower())
                    select acronym).ToList();
        }

        internal List<VarConfig.AcronymRow> GetAcronymsOfLevelSortedByName(VarConfig.AcronymLevelRow levelRow)
        {
            return (from acronym in levelRow.GetAcronymRows() select acronym).OrderBy(acronym => acronym.Name).ToList();
        }

        internal string GetTypeDescription(string typeShortName)
        {
            List<string> listLongName = (from acroType in _varConfig.AcronymType
                    where acroType.ShortName.ToLower().Contains(typeShortName.ToLower())
                    select acroType.LongName).ToList<string>();
            if (listLongName.Count != 1)
                return VariablesManager.DESCRIPTION_UNKNOWN;
            return listLongName.First();
        }

        internal string GetAcronymDescription(string typeShortName, string acronym)
        {
            string description = VariablesManager.DESCRIPTION_UNKNOWN;
            foreach (VarConfig.AcronymRow acronymRow in GetAcronymsOfType(typeShortName))
            {
                if (acronymRow.Name.ToLower() == acronym)
                {
                    description = acronymRow.Description;
                    break;
                }
            }
            return description;
        }

        internal VarConfig.VariableRow AddVariable(string name = "", string monetary = "1", string hhLevel = "0", string autoLabel = "")
        {
            string guid = Guid.NewGuid().ToString();
            _guidsOfNewRows.Add(guid);

            VarConfig.VariableRow variableRow = _varConfig.Variable.AddVariableRow(guid, name, monetary, hhLevel, autoLabel);

            foreach (string country in GetCountriesShortNames())
                _varConfig.CountryLabel.AddCountryLabelRow(Guid.NewGuid().ToString(), variableRow, country, "-");

            return variableRow;
        }

        internal static VarConfig.VariableRow CopyVariableFromAnotherConfig(VarConfig varConfig, VarConfig.VariableRow originalVar)
        {
            VarConfig.VariableRow variableRow = varConfig.Variable.AddVariableRow(originalVar.ID, originalVar.Name, originalVar.Monetary, originalVar.HHLevel, originalVar.AutoLabel);

            foreach (VarConfig.CountryLabelRow originalLabel in originalVar.GetCountryLabelRows())
                varConfig.CountryLabel.AddCountryLabelRow(originalLabel.ID, variableRow, originalLabel.Country, originalLabel.Label);

            return variableRow;
        }

        internal List<string> GetCountriesShortNames(bool toLower = false)
        {
            return (from countryLabel in _varConfig.CountryLabel
                    select (toLower ? countryLabel.Country.ToLower() : countryLabel.Country)).Distinct().ToList();
        }

        internal void DeleteVariable(VarConfig.VariableRow variableRow)
        {
            variableRow.Delete();
        }

        internal int GetAcronymLevel(string acronym, string type)
        {
            List<VarConfig.AcronymRow> acronymRows = (from acronymRow in _varConfig.Acronym
                                                      where acronymRow.Name.ToLower() == acronym.ToLower()
                                                      && acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName.ToLower().Contains(type.ToLower())
                                                      select acronymRow).ToList();
            if (acronymRows.Count == 0)
                return int.MinValue; //should not happen

            return acronymRows.First().AcronymLevelRow.Index;
        }

        internal List<VarConfig.AcronymLevelRow> GetAcronymLevelsSortedByIndex(VarConfig.AcronymTypeRow parentRow)
        {
            return (from acronymLevelRow in _varConfig.AcronymLevel
                    where acronymLevelRow.TypeID == parentRow.ID
                    select acronymLevelRow).OrderBy(acronymLevelRow => acronymLevelRow.Index).ToList();
        }

        internal List<VarConfig.AcronymLevelRow> GetAcronymLevelsSortedByName(VarConfig.AcronymTypeRow parentRow)
        {
            return (from acronymLevelRow in _varConfig.AcronymLevel
                    where acronymLevelRow.TypeID == parentRow.ID
                    select acronymLevelRow).OrderBy(acronymLevelRow => acronymLevelRow.Name).ToList();
        }

        internal VarConfig.AcronymTypeRow AddAcronymTypeRow(string longName = "", string shortName = "")
        {
            string guid = Guid.NewGuid().ToString();
            _guidsOfNewRows.Add(guid);
            return _varConfig.AcronymType.AddAcronymTypeRow(guid, longName, shortName);
        }

        internal void CopyAcronymTypeRow(VarConfig.AcronymTypeRow originalType)
        {
            _varConfig.AcronymType.AddAcronymTypeRow(originalType.ID, originalType.LongName, originalType.ShortName);
        }

        internal VarConfig.AcronymLevelRow AddAcronymLevelRow(VarConfig.AcronymTypeRow parentRow, VarConfig.AcronymLevelRow addAfterRow = null, string name = "")
        {
            int indexAfter = 0;
            if (addAfterRow != null)
                indexAfter = addAfterRow.Index;

            foreach (VarConfig.AcronymLevelRow acronymLevelRow in parentRow.GetAcronymLevelRows())
                if (acronymLevelRow.Index > indexAfter)
                    ++acronymLevelRow.Index;

            string guid = Guid.NewGuid().ToString();
            _guidsOfNewRows.Add(guid);
            return _varConfig.AcronymLevel.AddAcronymLevelRow(guid, indexAfter + 1, parentRow, name);
        }

        internal void CopyAcronymLevelRow(VarConfig.AcronymLevelRow originalLevel, VarConfig.AcronymTypeRow parentType)
        {
            _varConfig.AcronymLevel.AddAcronymLevelRow(originalLevel.ID, originalLevel.Index, parentType, originalLevel.Name);
        }

        internal VarConfig.AcronymRow AddAcronymRow(VarConfig.AcronymLevelRow parentRow, string acronym = "", string description = "")
        {
            string guid = Guid.NewGuid().ToString();
            _guidsOfNewRows.Add(guid);
            return _varConfig.Acronym.AddAcronymRow(guid, parentRow, acronym, description);
        }

        internal void CopyAcronymRow(VarConfig.AcronymRow originalAcronym, VarConfig.AcronymLevelRow parentLevel)
        {
            _varConfig.Acronym.AddAcronymRow(originalAcronym.ID, parentLevel, originalAcronym.Name, originalAcronym.Description);
        }

        internal VarConfig.CategoryRow AddCategoryRow(VarConfig.AcronymRow parentRow, string value = "", string description = "")
        {
            return _varConfig.Category.AddCategoryRow(parentRow, value, description);
        }

        internal void DeleteCategoryRows(VarConfig.AcronymRow acronymRow)
        {
            List<VarConfig.CategoryRow> categoryRows = (from categoryRow in acronymRow.GetCategoryRows() select categoryRow).ToList();
            for (int index = categoryRows.Count() - 1; index >= 0; --index)
                categoryRows.ElementAt(index).Delete();
        }

        internal void CopyCategoryRow(VarConfig.CategoryRow originalCategory, VarConfig.AcronymRow parentAcronym)
        {
            _varConfig.Category.AddCategoryRow(parentAcronym, originalCategory.Value, originalCategory.Description);
        }

        internal void ClearNewRowsList()
        {
            _guidsOfNewRows.Clear();
        }

        internal bool IsNewRow(string guid)
        {//variables and acronyms which were created within the current session of the variables-administration are treated differntly at certain occasions
         //(e.g. new variables can be deleted or name-changed without warning, the name of acronyms and acronym-types can only be changed for new ones, ...)
            return _guidsOfNewRows.Contains(guid);
        }

        internal void Commit()
        {
            _varConfig.AcceptChanges();
        }

        internal List<VarConfig.SwitchablePolicyRow> GetSwitchablePolicies()
        {
            return _varConfig.SwitchablePolicy.ToList<VarConfig.SwitchablePolicyRow>();
        }

        internal VarConfig.SwitchablePolicyRow AddSwitchablePolicy(string namePattern = "", string longName = "")
        {
            return _varConfig.SwitchablePolicy.AddSwitchablePolicyRow(Guid.NewGuid().ToString(), namePattern, longName);
        }

        internal void RejectSwitchablePolicyChanges()
        {
            _varConfig.SwitchablePolicy.RejectChanges();
        }

        internal VarConfig.SwitchablePolicyRow GetSwitchablePolicy(string switchablePolicyID)
        {
            List<VarConfig.SwitchablePolicyRow> listSwitchablePolicy = (from swp in _varConfig.SwitchablePolicy where swp.ID == switchablePolicyID select swp).ToList<VarConfig.SwitchablePolicyRow>();
            return listSwitchablePolicy.Count == 1 ? listSwitchablePolicy.First() : null;
        }

        internal static VarConfig.SwitchablePolicyRow CopySwitchPolicyFromAnotherConfig(VarConfig varConfig, VarConfig.SwitchablePolicyRow originalSP)
        {
            return varConfig.SwitchablePolicy.AddSwitchablePolicyRow(originalSP.ID, originalSP.NamePattern, originalSP.LongName);
        }
    }
}
