using EM_Common;
using EM_UI.Tools;
using EM_UI.UpratingIndices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_UI.DataSets
{
    internal class CountryConfigFacade
    {
        CountryConfig _countryConfig = null;

        const int _GUID_Length = 36;

        internal const string _columnName_Color = "Color";
        
        internal CountryConfigFacade() { } //standard constructor

        internal CountryConfigFacade(string countryShortName, string countryLongName = "") //constructor allowing for generating a CountryConfigFacade with an empty country
        {
            _countryConfig = new CountryConfig();
            _countryConfig.Country.AddCountryRow(Guid.NewGuid().ToString(), countryLongName, countryShortName, DefPar.Value.NO);
        }

        #region definitions

        class CompareParameterRowsByName : IEqualityComparer<CountryConfig.ParameterRow>
        {
            public bool Equals(CountryConfig.ParameterRow parameterRow1, CountryConfig.ParameterRow parameterRow2) { return parameterRow1.Name == parameterRow2.Name; }
            public int GetHashCode(CountryConfig.ParameterRow parameterRow) { return parameterRow.ID.GetHashCode(); }
        }
       
        const string _typeCountryRow = "CountryRow";
        const string _typeSystemRow = "SystemRow";
        const string _typePolicyRow = "PolicyRow";
        const string _typeFunctionRow = "FunctionRow";
        const string _typeParameterRow = "ParameterRow";

        internal static readonly string[] _cDataElements =  new string[]{ "Comment", "Value" };

        #endregion definitions

        #region xml_read_write

        internal CountryConfigFacade(CountryConfig countryConfig) //special constructor to copy a country (see RunMainForm)
        {
            _countryConfig = countryConfig.Copy() as CountryConfig;
        }

        internal CountryConfig ReadXml(string filePath, string fileName)
        {
            _countryConfig = new CountryConfig();

            using (StreamReader streamReader = new StreamReader(EMPath.AddSlash(filePath) + fileName, DefGeneral.DEFAULT_ENCODING))
            {
                _countryConfig.ReadXml(streamReader);
            }

            _countryConfig.AcceptChanges();

            MigrateUpratingIndices();

            return _countryConfig;
        }

        internal void WriteXml(string filePath, string fileName, bool saveWithLineBreaks = true)
        {
            Stream fileStream = new FileStream(EMPath.AddSlash(filePath) + fileName, FileMode.Create);
            using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING,
                                                                         CountryConfigFacade._cDataElements, saveWithLineBreaks))
                _countryConfig.WriteXml(xmlWriter);
        }

        #endregion xml_read_write

        #region system_functions

        internal CountryConfig.SystemRow AddFirstSystemRow(string systemName) //for adding very first system
        {
            return _countryConfig.System.AddSystemRow(Guid.NewGuid().ToString(),
                systemName,
                _countryConfig.Country.First(),
                DefPar.Value.EURO, //Output
                DefPar.Value.EURO, //Param
                "1",
                DefVarName.ILSORIGY,
                DefPar.Value.NO, //Private
                "1", //Order
                string.Empty, //Year
                string.Empty); //Comment
        }

        internal static CountryConfig.SystemRow CopySystemRow(string copySystemName, CountryConfig.SystemRow originalSystemRow, bool addAtTail = false)
        {
            CountryConfig countryConfig = originalSystemRow.Table.DataSet as CountryConfig;
            string order = (!addAtTail) ? ShiftRowsUp(originalSystemRow, false) //if not define otherwise (add at tail) add after base system
                           : GetMaxOrderPlus1AsString(countryConfig.Country.First());
            
            CountryConfig.SystemRow copySystemRow = countryConfig.System.AddSystemRow(Guid.NewGuid().ToString(), copySystemName,
                originalSystemRow.CountryRow, originalSystemRow.CurrencyOutput, originalSystemRow.CurrencyParam,
                originalSystemRow.ExchangeRateEuro, originalSystemRow.HeadDefInc, originalSystemRow.Private, order, originalSystemRow.Year, originalSystemRow.Comment);

            CopySystemContent(originalSystemRow, copySystemRow);

            return copySystemRow;
        }

        static void CopySystemContent(CountryConfig.SystemRow originalSystemRow, CountryConfig.SystemRow copySystemRow, bool keepIDs = false)
        {
            //this rather complex procedure takes care that reference policies don't point to the policy in the original system,
            //but (correctly) to the respective policy in the copied system; without doing this the procedure is a two liner, which is in effect carried out if keepIDs=true
            Dictionary<string, CountryConfig.PolicyRow> policyMatches = new Dictionary<string, CountryConfig.PolicyRow>();
            foreach (CountryConfig.PolicyRow policyRow in originalSystemRow.GetPolicyRows())
            {
                CountryConfig.PolicyRow copyPolicyRow = CopyPolicyRowToNewSystem(policyRow, copySystemRow, keepIDs);
                if (!keepIDs)
                    policyMatches.Add(policyRow.ID, copyPolicyRow); //store the original policy's id and the copied policy's id in a table ...
            }

            if (keepIDs)
                return; //no restoring of references necessary, as they are the same and can be simply copied in CopyPolicyRowToNewSystem

            //... to afterwards restore references (this needs to be done afterwards, as a reference may also point to a subsequent policy)
            foreach (CountryConfig.PolicyRow policyRow in originalSystemRow.GetPolicyRows())
            {
                if (policyRow.ReferencePolID == null || policyRow.ReferencePolID == string.Empty)
                    continue;
                policyMatches[policyRow.ID].ReferencePolID = policyMatches[policyRow.ReferencePolID].ID;
            }

            //if ids are not kept all ids used by parameter-identifiers must be replaced by their correspondents in the copied system
            if (!keepIDs)
            {
                foreach (CountryConfig.ParameterRow parameterRow in
                    (from pR in (copySystemRow.Table.DataSet as CountryConfig).Parameter where pR.FunctionRow.PolicyRow.SystemID == copySystemRow.ID select pR))
                {
                    if (parameterRow.Value.Length != _GUID_Length) //try to identify GUIDs by their length
                        continue;

                    CountryConfig originalCountryConfig = originalSystemRow.Table.DataSet as CountryConfig;
                    System.Data.DataRow originalRow = GetRowByID(originalCountryConfig, parameterRow.Value); //get component in original system

                    if (originalRow == null)
                        continue; //maybe it wasn't a GUID, but just had the same length

                    System.Data.DataRow copyRow = GetRowWithSameOrder(originalRow, copySystemRow); //identify correspondent component by its order (i.e. use the fact that copied and original system have the same construction)
                    if (copyRow == null)
                        continue; //should not happen

                    parameterRow.Value = GetRowID(copyRow);
                }
            }
        }

        internal CountryConfig GetCountryConfig()
        {
            return _countryConfig;
        }

        internal static CountryConfig.SystemRow TransferSystemIdentically(CountryConfig.SystemRow originalSystemRow, CountryConfig copyCountryConfig)
        {
            CountryConfig.SystemRow copySystemRow = CopySystemRowToAnotherCountry(originalSystemRow, originalSystemRow.Name, copyCountryConfig, false, false);
            copySystemRow.ID = originalSystemRow.ID;
            return copySystemRow;
        }

        internal static CountryConfig.SystemRow CopySystemRowToAnotherCountry(CountryConfig.SystemRow originalSystemRow, string copySystemName,
                                                    CountryConfig copyCountryConfig, bool keepIDs = false, bool copyContent = true)
        {
            //if keepIDs=true only the system ID is new, policy-, function- and parameter-ids are kept
            //this is used (for add-ons) for correct identification of the components to which parameter-identifieres refer (in functions ChangeParam, loops, etc.)
            //note however that such a system cannot be stored with the original system, as identical ids would prevent correct identification of parent/child rows
                                
            string order = GetMaxOrderPlus1AsString(copyCountryConfig.Country.First());
            CountryConfig.SystemRow copySystemRow = copyCountryConfig.System.AddSystemRow(Guid.NewGuid().ToString(), copySystemName,
                copyCountryConfig.Country.First(), originalSystemRow.CurrencyOutput, originalSystemRow.CurrencyParam,
                originalSystemRow.ExchangeRateEuro, originalSystemRow.HeadDefInc, originalSystemRow.Private, order, originalSystemRow.Year, originalSystemRow.Comment);

            if (copyContent)
                CopySystemContent(originalSystemRow, copySystemRow, keepIDs);

            return copySystemRow;
        }

        internal List<CountryConfig.SystemRow> GetSystemRows()
        {
            return (from system in _countryConfig.System select system).ToList();
        }

        internal CountryConfig.SystemRow GetSystemRowByName(string systemName)
        {
            List<CountryConfig.SystemRow> systemRows = (from system in _countryConfig.System
                                                        where system.Name.ToLower() == systemName.ToLower() select system).ToList();
            return (systemRows.Count == 1) ? systemRows.First() : null;
        }

        internal CountryConfig.SystemRow GetSystemRowByID(string systemID)
        {
            List<CountryConfig.SystemRow> systemRows = (from system in _countryConfig.System where system.ID == systemID select system).ToList();
            return (systemRows.Count == 1) ? systemRows.First() : null;
        }

        internal List<CountryConfig.SystemRow> GetSystemRowsOrdered()
        {
            return (from system in _countryConfig.System select system).OrderBy(system => long.Parse(system.Order)).ToList();
        }

        //returns all incomelists (defFunctionName=DefIL, defFunctionSpecialParameterName=name),
        //constants (defFunctionName=DefConst, defFunctionSpecialParameterName=const_name), etc. of a specific system
        internal static void GetDefFunctionInformation(CountryConfig.SystemRow systemRow,
                                                                ref List<CountryConfig.ParameterRow> defFunctionSpecialParameterRows,
                                                                string defFunctionName,
                                                                string defFunctionSpecialParameterName)
        {
            if (defFunctionSpecialParameterRows != null)
                return;

            CountryConfig countryConfig = systemRow.Table.DataSet as CountryConfig;

            bool defVar = defFunctionName.ToLower() == DefFun.DefVar.ToLower();
            bool defConst = defFunctionName.ToLower() == DefFun.DefConst.ToLower();
            var query = (from parameter in countryConfig.Parameter
                             join function in countryConfig.Function on parameter.FunctionID equals function.ID
                             join policy in countryConfig.Policy on function.PolicyID equals policy.ID
                             where function.Name.ToLower() == defFunctionName.ToLower() &&
                                    function.Switch.ToLower() == DefPar.Value.ON &&
                                    //functions DefVar and DefConst do not have a special parameter (like 'name')
                                    //instead all parameters are variable- respectively constant-definitions, except ... see exclusion below
                                    (defVar || defConst || parameter.Name.ToLower() == defFunctionSpecialParameterName.ToLower()) &&
                                    parameter.Value != DefPar.Value.NA &&
                                    policy.SystemID == systemRow.ID
                             select parameter).Distinct(new CompareParameterRowsByName());
            defFunctionSpecialParameterRows = query.ToList<CountryConfig.ParameterRow>();

            //exclude the DefVar/DefConst-parameters 'var_monetary', 'var_dataset', 'var_systemyear', 'const_dataset', 'const_systemyear' from the list
            for (int i = defFunctionSpecialParameterRows.Count - 1; i >= 0; --i)
            {
                string parameterName = defFunctionSpecialParameterRows.ElementAt(i).Name.ToLower();
                if (parameterName == DefPar.DefVar.Var_Monetary.ToLower() ||
                    parameterName == DefPar.DefVar.Var_Dataset.ToLower() ||
                    parameterName == DefPar.DefVar.Var_SystemYear.ToLower() ||
                    parameterName == DefPar.DefConst.Const_Dataset.ToLower() ||
                    parameterName == DefPar.DefConst.Const_SystemYear.ToLower())
                    defFunctionSpecialParameterRows.RemoveAt(i);
            }
        }

        internal void CopySystemFormatting(CountryConfig.SystemRow originalSystemRow, CountryConfig.SystemRow copySystemRow)
        {
            foreach (CountryConfig.ConditionalFormatRow condtionalFormatRow in GetConditionalFormatRowsOfSystem(originalSystemRow))
            {
                if (condtionalFormatRow.BaseSystemName == string.Empty)
                    //standard conditional formatting
                    AddConditionalFormat_SystemsRow(condtionalFormatRow, copySystemRow);
                else
                {//formatting marking the differences between of the system to its base system
                    CountryConfig.ConditionalFormatRow compareBaseConditionalFormatting = AddConditionalFormatRow(condtionalFormatRow.BackColor, condtionalFormatRow.ForeColor, string.Empty, originalSystemRow.Name);
                    AddConditionalFormat_SystemsRow(compareBaseConditionalFormatting, copySystemRow);
                }
            }
        }

        internal CountryConfig.SystemDataTable GetSystemDataTable()
        {
            return _countryConfig.System;
        }

        internal static void DeleteSystemRow(CountryConfig.SystemRow systemRow)
        {
            CountryConfig countryConfig = systemRow.Table.DataSet as CountryConfig;
            string systemName = systemRow.Name;

            systemRow.Delete();

            try
            {
                //find the conditional formats which need to be removed ...
                List<CountryConfig.ConditionalFormatRow> conditionalFormatsToRemove = new List<CountryConfig.ConditionalFormatRow>();
                foreach (CountryConfig.ConditionalFormatRow conditionalFormatRow in countryConfig.ConditionalFormat)
                {
                    //... because they only refer to this system
                    int usedInGeneral = (from allCFSystemRows in countryConfig.ConditionalFormat_Systems
                                         where allCFSystemRows.ConditionalFormatID == conditionalFormatRow.ID
                                         select allCFSystemRows).ToList().Count;
                    int usedBySystem = (from allCFSystemRows in countryConfig.ConditionalFormat_Systems
                                        where allCFSystemRows.ConditionalFormatID == conditionalFormatRow.ID
                                              && allCFSystemRows.SystemName.ToLower() == systemName.ToLower()
                                        select allCFSystemRows).ToList().Count;
                    if (usedBySystem == usedInGeneral)
                        conditionalFormatsToRemove.Add(conditionalFormatRow);

                    //... or use it as base system
                    if (conditionalFormatRow.BaseSystemName.ToLower() == systemName.ToLower())
                        conditionalFormatsToRemove.Add(conditionalFormatRow);
                }
                foreach (CountryConfig.ConditionalFormatRow conditionalFormatToRemove in conditionalFormatsToRemove)
                    conditionalFormatToRemove.Delete();
            }
            catch (Exception e) { UserInfoHandler.RecordIgnoredException("CountryConfigFacade.DeleteSystemRow", e); } //do nothing, to not jeopardise removal of system
        }

        internal void RenameSystemInConditionalFormats(string oldSystemName, string newSystemName)
        {
            foreach (CountryConfig.ConditionalFormatRow conditionalFormatRow in
                    (from cfr in _countryConfig.ConditionalFormat
                     where cfr.BaseSystemName.ToLower() == oldSystemName.ToLower()
                     select cfr))
                conditionalFormatRow.BaseSystemName = newSystemName;
            foreach (CountryConfig.ConditionalFormat_SystemsRow conditionalFormat_SystemsRow in
                    (from cfsr in _countryConfig.ConditionalFormat_Systems
                     where cfsr.SystemName.ToLower() == oldSystemName.ToLower()
                     select cfsr))
                conditionalFormat_SystemsRow.SystemName = newSystemName;
        }

        #endregion system_functions

        #region policy_functions

        internal static CountryConfig.PolicyRow CopyPolicyRowToNewSystem(CountryConfig.PolicyRow originalPolicyRow, CountryConfig.SystemRow newSystemRow,
                                                                bool keepIDs = false, bool copyContent = true)
        {//used in CopySystemRow
            CountryConfig countryConfig = newSystemRow.Table.DataSet as CountryConfig;
            string guid = keepIDs ? originalPolicyRow.ID : Guid.NewGuid().ToString();
            string referencePolID = keepIDs ? originalPolicyRow.ReferencePolID : null; //if ids are not kept, the id doesn't make any sense as it refers to the original system (see CopySystemContent for handling)
            CountryConfig.PolicyRow copyPolicyRow = countryConfig.Policy.AddPolicyRow(guid, newSystemRow, referencePolID,
                            originalPolicyRow.Name, originalPolicyRow.Type, originalPolicyRow.Comment, originalPolicyRow.PrivateComment,
                            originalPolicyRow.Order, originalPolicyRow.Switch, originalPolicyRow.Private, DefPar.Value.NO_COLOR);
            if (copyContent)
                foreach (CountryConfig.FunctionRow functionRow in originalPolicyRow.GetFunctionRows())
                    CopyFunctionRowAtTailOrUseOriginalOrder(functionRow, copyPolicyRow,
                                    false, //don't copy to end, but use order in originalPolicyRow 
                                    false, //SwitchOff
                                    keepIDs);
            return copyPolicyRow;
        }

        internal CountryConfig.PolicyRow AddFirstPolicyRow(string policyName, string policyType, CountryConfig.SystemRow systemRow, string switchSetting)
        {
            return _countryConfig.Policy.AddPolicyRow(Guid.NewGuid().ToString(), systemRow,
                                                        null, //ReferencePolID
                                                        policyName, policyType,
                                                        !string.IsNullOrEmpty(policyType) ? $"{policyType.ToUpper()}:" : string.Empty, //Comment
                                                        string.Empty, //PrivateComment
                                                        "1", //Order
                                                        switchSetting,
                                                        DefPar.Value.NO, //Private
                                                        DefPar.Value.NO_COLOR);
        }

        internal static CountryConfig.PolicyRow AddPolicyRow(string policyName, string policyType,
                                                                CountryConfig.PolicyRow neighbourPolicyRow, bool addBeforeNeighbour, string switchSetting)
        {
            CountryConfig countryConfig = neighbourPolicyRow.Table.DataSet as CountryConfig;
            string order = ShiftRowsUp(neighbourPolicyRow, addBeforeNeighbour);
            return countryConfig.Policy.AddPolicyRow(Guid.NewGuid().ToString(), neighbourPolicyRow.SystemRow,
                                            null, //ReferencePolID
                                            policyName, policyType,
                                            !string.IsNullOrEmpty(policyType) ? $"{policyType.ToUpper()}:" : string.Empty, //Comment
                                            string.Empty, //PrivateComment
                                            order, switchSetting,
                                            DefPar.Value.NO, //Private
                                            DefPar.Value.NO_COLOR);
        }

        internal static CountryConfig.PolicyRow CopyPolicyRow(CountryConfig.PolicyRow originalPolicyRow, string policyName,
                                                CountryConfig.PolicyRow neighbourPolicyRow, bool copyBeforeNeighbour, bool switchNA, bool keepIDs = false)
        {
            CountryConfig countryConfig = neighbourPolicyRow.Table.DataSet as CountryConfig;
            string order = CountryConfigFacade.ShiftRowsUp(neighbourPolicyRow, copyBeforeNeighbour);
            string policySwitch = switchNA ? DefPar.Value.NA : originalPolicyRow.Switch;
            string guid = keepIDs ? originalPolicyRow.ID : Guid.NewGuid().ToString();
            CountryConfig.PolicyRow copyPolicyRow = countryConfig.Policy.AddPolicyRow(guid, neighbourPolicyRow.SystemRow,
                            null, //ReferencePolID
                            policyName, originalPolicyRow.Type, originalPolicyRow.Comment, originalPolicyRow.PrivateComment, order, policySwitch, originalPolicyRow.Private, DefPar.Value.NO_COLOR);
            foreach (CountryConfig.FunctionRow functionRow in originalPolicyRow.GetFunctionRows())
                CopyFunctionRowAtTailOrUseOriginalOrder(functionRow, copyPolicyRow,
                                false, //don't copy to end, but use order in originalPolicyRow 
                                switchNA, keepIDs);
            return copyPolicyRow;
        }

        internal static CountryConfig.PolicyRow AddReferencePolicyRow(CountryConfig.PolicyRow originalPolicyRow,
                                                                        CountryConfig.PolicyRow neighbourPolicyRow, bool copyBeforeNeighbour, bool switchNA)
        {
            CountryConfig countryConfig = neighbourPolicyRow.Table.DataSet as CountryConfig;
            string order = CountryConfigFacade.ShiftRowsUp(neighbourPolicyRow, copyBeforeNeighbour);
            string policySwitch = switchNA ? DefPar.Value.NA : originalPolicyRow.Switch;
            CountryConfig.PolicyRow referencePolicyRow = countryConfig.Policy.AddPolicyRow(Guid.NewGuid().ToString(), neighbourPolicyRow.SystemRow,
                                                                        originalPolicyRow.ID, //ReferencePolID
                                                                        string.Empty, //Name
                                                                        string.Empty, //Type
                                                                        originalPolicyRow.Comment, originalPolicyRow.PrivateComment, order, policySwitch,
                                                                        DefPar.Value.NO, //Private
                                                                        DefPar.Value.NO_COLOR);
            return referencePolicyRow;
        }

        internal CountryConfig.PolicyRow GetPolicyRowByID(string ID)
        {
            List<CountryConfig.PolicyRow> policyRows = (from policy in _countryConfig.Policy where policy.ID == ID select policy).ToList();
            return (policyRows.Count == 1) ? policyRows.First() : null;
        }

        internal CountryConfig.PolicyRow GetPolicyRowByName(string systemID, string policyName)
        {
            List<CountryConfig.PolicyRow> policyRows = (from policy in _countryConfig.Policy
                where policy.SystemID == systemID &&
                policy.Name.ToLower().StartsWith(policyName.ToLower()) //StartsWith because function is called with policy name like "output_std", i.e. without country shortcut
                select policy).ToList();

            if (policyRows.Count == 1)
                return policyRows.First();
            
            if (policyRows.Count == 0)
                return null;

            //if more than one match, select the best fitting
            int minDif = int.MaxValue;
            int indexBestMatch = 0;
            for (int index = 0; index < policyRows.Count; ++index)
            {
                if (policyRows.ElementAt(index).Switch == DefPar.Value.NA)
                    continue;
                int dif = string.Compare(policyRows.ElementAt(index).Name, policyName, true);
                if (dif < minDif)
                {
                    indexBestMatch = index;
                    minDif = dif;
                }
            }
            return policyRows.ElementAt(indexBestMatch);
        }

        internal List<CountryConfig.PolicyRow> GetPolicyRows()
        {
            return _countryConfig.Policy.ToList();
        }

        internal CountryConfig.PolicyRow GetPolicyRowByFullName(string systemID, string policyName)
        {
            List<CountryConfig.PolicyRow> policyRows = (from policy in _countryConfig.Policy
                                                        where policy.SystemID == systemID && policy.Name.ToLower() == policyName.ToLower()
                                                        select policy).ToList();
            if (policyRows.Count == 1)
                return policyRows.First();
            return null;
        }

        internal CountryConfig.PolicyRow GetPolicyRowByOrder(string order, string systemID)
        {
            List<CountryConfig.PolicyRow> policyRows = (from policy in _countryConfig.Policy
                                                        where policy.Order == order &&
                                                              policy.SystemID == systemID
                                                        select policy).ToList();
            return policyRows.Count != 1 ? null : policyRows.First();
        }

        internal CountryConfig.PolicyRow GetPolicyRowByNameOrderAndSystemID(string policyName, string policyOrder, string systemID)
        {
            List<CountryConfig.PolicyRow> policyRows = (from policy in _countryConfig.Policy
                                                        where policy.Name.ToLower() == policyName.ToLower() &&
                                                              policy.Order == policyOrder &&
                                                              policy.SystemID == systemID
                                                        select policy).ToList();
            if (policyRows.Count != 1)
                return null;
            return policyRows.First();
        }

        //function returns policies ordered and only one of all systems (where the system is arbitrary)
        //this considers the fact that each system must contain each policy (set to n/a or at least switched off if not used)
        internal List<CountryConfig.PolicyRow> GetPolicyRowsOrderedAndDistinct()
        {
            List<CountryConfig.PolicyRow> policyRows = new List<CountryConfig.PolicyRow>();

            List<string> alreadyIn = new List<string>();
            foreach (CountryConfig.PolicyRow policyRow in (from policy in _countryConfig.Policy select (policy)).OrderBy(policy => long.Parse(policy.Order)).ToList())
            {
                string ident = policyRow.Name.ToLower() + policyRow.Order;
                if (alreadyIn.Contains(ident)) continue;
                policyRows.Add(policyRow);
                alreadyIn.Add(ident);
            }

            return policyRows;
        }

        internal static void SetPolicyToNA(CountryConfig.PolicyRow policyRow)
        {
            policyRow.Switch = DefPar.Value.NA;
            foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                SetFunctionToNA(functionRow);
        }

        internal static bool IsReferencePolicy(CountryConfig.PolicyRow policyRow)
        {
            return (from potRef in policyRow.SystemRow.GetPolicyRows()
                    where potRef.ReferencePolID == policyRow.ID select potRef).Count() > 0;
        }

        #endregion policy_functions

        #region function_functions

        internal static CountryConfig.FunctionRow AddFunctionRowAtTail(string functionName, CountryConfig.PolicyRow policyRow, string switchSetting)
        {//add function at the end of policy
            CountryConfig countryConfig = policyRow.Table.DataSet as CountryConfig;
            string order = GetMaxOrderPlus1AsString(policyRow);
            return countryConfig.Function.AddFunctionRow(Guid.NewGuid().ToString(), policyRow, functionName,
                                                          string.Empty, string.Empty, //Comment and PrivateComment
                                                          order, switchSetting,
                                                          DefPar.Value.NO, //Private
                                                          DefPar.Value.NO_COLOR);
        }

        internal static CountryConfig.FunctionRow AddFunctionRow(string functionName,
                                                                 CountryConfig.FunctionRow neighbourFunctionRow, bool addBeforeNeighbour, string switchSetting) //, bool switchOn)
        {//add function at a specific place in policy
            CountryConfig countryConfig = neighbourFunctionRow.Table.DataSet as CountryConfig;
            string order = ShiftRowsUp(neighbourFunctionRow, addBeforeNeighbour);
            return countryConfig.Function.AddFunctionRow(Guid.NewGuid().ToString(), neighbourFunctionRow.PolicyRow, functionName,
                                                string.Empty, string.Empty, //Comment and PrivateComment
                                                order, switchSetting,
                                                DefPar.Value.NO, //Private
                                                DefPar.Value.NO_COLOR);
        }
        
        internal static CountryConfig.FunctionRow CopyFunctionRowAtTailOrUseOriginalOrder(CountryConfig.FunctionRow originalFunctionRow,
                                                  CountryConfig.PolicyRow copyPolicyRow, bool copyAtTail, bool switchNA, bool keepIDs = false, bool copyContent = true)
        {//copy function to the end of policy
            CountryConfig countryConfig = copyPolicyRow.Table.DataSet as CountryConfig;
            string order = copyAtTail ? GetMaxOrderPlus1AsString(copyPolicyRow) : originalFunctionRow.Order;
            string functionSwitch = switchNA ? DefPar.Value.NA : originalFunctionRow.Switch;
            string guid = keepIDs ? originalFunctionRow.ID : Guid.NewGuid().ToString();
            CountryConfig.FunctionRow copyFunction = countryConfig.Function.AddFunctionRow(guid, copyPolicyRow,
                originalFunctionRow.Name, originalFunctionRow.Comment, originalFunctionRow.PrivateComment, order, functionSwitch, originalFunctionRow.Private, DefPar.Value.NO_COLOR);
            if (copyContent)
                foreach (CountryConfig.ParameterRow parameter in originalFunctionRow.GetParameterRows())
                    CopyParameterRow(parameter, copyFunction, switchNA, keepIDs);
            return copyFunction;
        }

        internal static CountryConfig.FunctionRow CopyFunctionRow(CountryConfig.FunctionRow originalFunctionRow,
                                                    CountryConfig.FunctionRow neighbourFunctionRow, bool copyBeforeNeighbour, bool switchNA, bool keepIDs = false)
        {//copy function to a specific place in policy
            CountryConfig countryConfig = neighbourFunctionRow.Table.DataSet as CountryConfig;
            string order = ShiftRowsUp(neighbourFunctionRow, copyBeforeNeighbour);
            string functionSwitch = switchNA ? DefPar.Value.NA : originalFunctionRow.Switch;
            string guid = keepIDs ? originalFunctionRow.ID : Guid.NewGuid().ToString();
            CountryConfig.FunctionRow copyFunctionRow = countryConfig.Function.AddFunctionRow(guid, neighbourFunctionRow.PolicyRow,
                originalFunctionRow.Name, originalFunctionRow.Comment, originalFunctionRow.PrivateComment, order, functionSwitch, originalFunctionRow.Private, DefPar.Value.NO_COLOR);
            foreach (CountryConfig.ParameterRow parameter in originalFunctionRow.GetParameterRows())
                CopyParameterRow(parameter, copyFunctionRow, switchNA, keepIDs);
            return copyFunctionRow;
        }

        internal List<CountryConfig.FunctionRow> GetFunctionRowsOfPolicyOrdered(string policyID)
        {
            return (from function in _countryConfig.Function
                    where function.PolicyID == policyID
                    select function).OrderBy(function => long.Parse(function.Order)).ToList();
        }

        internal List<CountryConfig.FunctionRow> GetFunctionRowsByPolicyIDAndFunctionName(string policyID, string functionName)
        {
            return (from function in _countryConfig.Function
                    where function.PolicyID == policyID && function.Name.ToLower() == functionName.ToLower()
                    select function).OrderBy(function => long.Parse(function.Order)).ToList();
        }

        internal List<CountryConfig.FunctionRow> GetFunctionRowsBySystemIDAndFunctionName(string systemID, string functionName)
        {
            return (from function in _countryConfig.Function
                    where function.PolicyRow.SystemID == systemID && function.Name.ToLower() == functionName.ToLower()
                    select function).OrderBy(function => long.Parse(function.Order)).ToList();
        }

        internal CountryConfig.FunctionRow GetFunctionRowByID(string ID)
        {
            List<CountryConfig.FunctionRow> functionRows = (from function in _countryConfig.Function where function.ID == ID select function).ToList();
            if (functionRows.Count != 1)
                return null;
            return functionRows.First();
        }

        internal CountryConfig.FunctionRow GetFunctionRowByOrder(string policyOrder, string functionOrder, string systemID)
        {
            List<CountryConfig.FunctionRow> functionRows = (from function in _countryConfig.Function
                        where function.PolicyRow.SystemID == systemID
                           && function.PolicyRow.Order == policyOrder
                           && function.Order == functionOrder
                        select function).ToList();
            return functionRows.Count != 1 ? null : functionRows.First();
        }

        internal CountryConfig.FunctionRow GetFunctionRowByPolicyNameAndOrder(string systemID, string policyName, string order)
        {
            try
            {
                CountryConfig.FunctionRow func = (from function in _countryConfig.Function
                                                  where function.PolicyRow.SystemID == systemID
                                                     && function.PolicyRow.Name.ToLower() == policyName.ToLower()
                                                     && function.Order == order
                                                     && function.Switch != DefPar.Value.NA           //policy name is not really a unique identifier, therefore it's possible to find more than one row
                                                     && function.PolicyRow.Switch != DefPar.Value.NA //as this is most likely true because of system imports, only one found item should be not n/a
                                                  select function).FirstOrDefault();
                if (func == null)
                {
                    func = (from function in _countryConfig.Function
                            where function.PolicyRow.SystemID == systemID
                                   && function.PolicyRow.Name.ToLower() == policyName.ToLower()
                                   && function.Order == order
                            select function).FirstOrDefault();
                }
                return func;
            }
            catch (Exception e) { UserInfoHandler.RecordIgnoredException("CountryConfigFacade.GetFunctionRowByPolicyNameAndOrder", e); return null; }
        }

        internal static int GetMaxFootnoteUsedByFunction(CountryConfig.FunctionRow functionRow, string footnoteName)
        {
            int maxFootnote = 0;
            foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
            {
                if (parameterRow.Name.ToLower() != footnoteName.ToLower())
                    continue; //not the same footnote
                if (EM_Helpers.SaveConvertToInt(parameterRow.Group) > maxFootnote)
                    maxFootnote = EM_Helpers.SaveConvertToInt(parameterRow.Group);
            }
            return maxFootnote;
        }

        internal static void SetFunctionToNA(CountryConfig.FunctionRow functionRow)
        {
            functionRow.Switch = DefPar.Value.NA;
            foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                parameterRow.Value = DefPar.Value.NA;
        }

        #endregion function_functions

        #region parameter_functions

        internal static CountryConfig.ParameterRow CopyParameterRow(CountryConfig.ParameterRow originalParameterRow, CountryConfig.FunctionRow functionRow, bool setToNA, bool keepIDs = false,
                CountryConfig.ParameterRow neighbourParameterRow = null, bool addBeforeNeighbour = true, bool copyAtTail = false)
        {
            CountryConfig countryConfig = functionRow.Table.DataSet as CountryConfig;
            string parameterValue = setToNA ? DefPar.Value.NA : originalParameterRow.Value;
            string guid = keepIDs ? originalParameterRow.ID : Guid.NewGuid().ToString();
            string order = originalParameterRow.Order;
            if (neighbourParameterRow != null)
                order = ShiftRowsUp(neighbourParameterRow, addBeforeNeighbour);
            if (copyAtTail)
                order = GetMaxOrderPlus1AsString(functionRow);
            return countryConfig.Parameter.AddParameterRow(guid, functionRow, originalParameterRow.Name,
                    originalParameterRow.Comment, originalParameterRow.PrivateComment, parameterValue, order, originalParameterRow.ValueType, originalParameterRow.Group, DefPar.Value.NO_COLOR, originalParameterRow.Private);
        }

        internal static CountryConfig.ParameterRow AddParameterRowAtTail(CountryConfig.FunctionRow functionRow,
                                                                         string parName, DefinitionAdmin.Par parDef, string group = "")
        {
            CountryConfig countryConfig = functionRow.Table.DataSet as CountryConfig;
            if (group == string.Empty) group = IsGroupPar(functionRow.Name, parName) ? "1" : string.Empty;
            return countryConfig.Parameter.AddParameterRow(Guid.NewGuid().ToString(), functionRow, parName,
                                                string.Empty, string.Empty, //Comment and PrivateComment
                                                DefPar.Value.NA, GetMaxOrderPlus1AsString(functionRow),
                                                DefinitionAdmin.ParTypeToString(parDef.valueType), group, DefPar.Value.NO_COLOR,
                                                string.Empty); //Private
        }

        internal static CountryConfig.ParameterRow AddParameterRow(CountryConfig.ParameterRow neighbourParameterRow, bool addBeforeNeighbour,
                                                                   string parName, DefinitionAdmin.Par parDef, string group = "")
        {
            CountryConfig countryConfig = neighbourParameterRow.Table.DataSet as CountryConfig;
            string order = ShiftRowsUp(neighbourParameterRow, addBeforeNeighbour);
            if (group == string.Empty) group = IsGroupPar(neighbourParameterRow.FunctionRow.Name, parName) ? "1" : string.Empty;
            return countryConfig.Parameter.AddParameterRow(Guid.NewGuid().ToString(), neighbourParameterRow.FunctionRow, parName,
                                                string.Empty, string.Empty, //Comment and PrivateComment
                                                DefPar.Value.NA, order, DefinitionAdmin.ParTypeToString(parDef.valueType), group, DefPar.Value.NO_COLOR,
                                                string.Empty); //Private
        }

        private static bool IsGroupPar(string funName, string parName)
        {
            DefinitionAdmin.Fun fun = DefinitionAdmin.GetFunDefinition(funName, false); if (fun == null) return false;
            return fun.GetGroupParDef(parName, out string group) != null;
        }

        internal static CountryConfig.ParameterRow GetParameterRowByOrderAndName(CountryConfig.FunctionRow functionRow, string parameterOrder, string parameterName)
        {
            foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
            {
                if (parameterRow.Order == parameterOrder)
                {
                    if (parameterRow.Name.ToLower() == parameterName.ToLower())
                        return parameterRow;
                    return null;
                }
            }
            return null;
        }

        internal CountryConfig.ParameterRow GetParameterRowByName(string functionID, string parameterName)
        {
            List<CountryConfig.ParameterRow> parameterRows = (from parameter in _countryConfig.Parameter
                                                              where parameter.FunctionID == functionID && parameter.Name.ToLower() == parameterName.ToLower()
                                                              select parameter).ToList();
            if (parameterRows.Count == 1)
                return parameterRows.First();
            return null;
        }

        internal List<CountryConfig.ParameterRow> GetParameterRowsByName(string functionID, string parameterName)
        {
            return (from parameter in _countryConfig.Parameter
                    where parameter.FunctionID == functionID && parameter.Name.ToLower() == parameterName.ToLower()
                    select parameter).ToList();
        } 

        internal CountryConfig.ParameterRow GetParameterRowByID(string parameterID)
        {
            List<CountryConfig.ParameterRow> parameterRows = (from parameter in _countryConfig.Parameter
                                                              where parameter.ID == parameterID
                                                              select parameter).ToList();
            if (parameterRows.Count == 1)
                return parameterRows.First();
            return null;
        }

        internal CountryConfig.ParameterRow GetParameterRowByOrder(string functionID, string order)
        {
            List<CountryConfig.ParameterRow> parameterRows = (from parameter in _countryConfig.Parameter
                                                              where parameter.FunctionID == functionID && parameter.Order == order
                                                              select parameter).ToList();
            if (parameterRows.Count == 1)
                return parameterRows.First();
            return null;
        }

        internal CountryConfig.ParameterRow GetParameterRowByOrder(string policyOrder, string functionOrder, string parameterOrder, string systemID)
        {
            List<CountryConfig.ParameterRow> parameterRows = (from par in _countryConfig.Parameter
                                                              where par.Order == parameterOrder &&
                                                                    par.FunctionRow.Order == functionOrder &&
                                                                    par.FunctionRow.PolicyRow.Order == policyOrder &&
                                                                    par.FunctionRow.PolicyRow.SystemRow.ID == systemID
                                                              select par).ToList();
            return (parameterRows.Count() == 1) ? parameterRows.First() : null;
        }

        internal List<CountryConfig.ParameterRow> GetParameterRowsOfFunctionOrdered(CountryConfig.FunctionRow functionRow)
        {
            return (from parameter in functionRow.GetParameterRows()
                    select parameter).OrderBy(parameter => long.Parse(parameter.Order)).ToList();
        }

        internal static bool DoesParameterExist(CountryConfig.FunctionRow functionRow, string parameterName, string group = "")
        {
            foreach (CountryConfig.ParameterRow countryConfigParameterRow in functionRow.GetParameterRows())
            {
                if (countryConfigParameterRow.Name.ToLower() == parameterName.ToLower() && countryConfigParameterRow.Group == group)
                    return true;
                //also take account of conflict parameters (output_var/output_add_var, comp_perTU/comp_perElig)
                foreach (string conflict in DefinitionAdmin.GetParDefinition(functionRow.Name, parameterName).substitutes)
                {
                    if (countryConfigParameterRow.Name.ToLower() == conflict.ToLower() && countryConfigParameterRow.Group == group)
                        return true;
                }
            }
            return false;
        }

        internal List<string> GetParameterValuesPossiblyContainingVariables()
        {
            //variable can also be used in policy column (functions Uprate, SetDefault, DefIL, DefConst, etc.)
            //add parameter name if a respective function is concerned
            List<string> functionsWithEditableNameParameter = new List<string>();
            foreach (var f in DefinitionAdmin.GetFunNamesAndDesc(DefinitionAdmin.Fun.KIND.ALL))
                if (DefinitionAdmin.GetParDefinition(f.Key, DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER), false) != null
                    || DefinitionAdmin.GetParDefinition(f.Key, DefPar.PAR_TYPE.PLACEHOLDER.ToString().ToUpper(), false) != null)
                    functionsWithEditableNameParameter.Add(f.Key.ToLower());

            List<string> parameterValues = new List<string>();
            foreach (CountryConfig.ParameterRow countryConfigParameterRow in _countryConfig.Parameter)
            {
                //return the values of all parameters which possibly contain variables: i.e. formulas, conditions and variable/incomelist-parameters
                if (countryConfigParameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.CONDITION).ToLower() ||
                    countryConfigParameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.FORMULA).ToLower() ||
                    countryConfigParameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VARorIL).ToLower() ||
                    countryConfigParameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VAR).ToLower() ||
                    countryConfigParameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.TEXT).ToLower())
                    parameterValues.Add(countryConfigParameterRow.Value);

                if (functionsWithEditableNameParameter.Contains(countryConfigParameterRow.FunctionRow.Name.ToLower()))
                    parameterValues.Add(countryConfigParameterRow.Name);
            }

            return parameterValues;
        }

        #endregion parameter_functions

        #region general_functions

        internal string GetCountryShortName()
        {
            return _countryConfig.Country.First().ShortName;
        }
        internal static string GetCountryShortName(System.Data.DataRow row)
        {
            while (row.GetType().Name != _typeCountryRow)
                row = GetSuperRow(row);
            return (row as CountryConfig.CountryRow).ShortName;
        }

        internal string GetCountryLongName()
        {
            if (_countryConfig.Country.Count > 0)
                return _countryConfig.Country.First().Name;
            return string.Empty;
        }
        internal static string GetCountryLongName(System.Data.DataRow row)
        {
            while (row.GetType().Name != _typeCountryRow)
                row = GetSuperRow(row);
            return (row as CountryConfig.CountryRow).Name;
        }

        internal static string ShiftRowsUp(System.Data.DataRow row, bool toInsertRowBefore)
        {
            int originalOrder = EM_Helpers.SaveConvertToInt(GetOrderAsString(row));
            int freeOrder = int.MaxValue;
            foreach (System.Data.DataRow subRow in GetSubRows(GetSuperRow(row)))
            {
                int order = EM_Helpers.SaveConvertToInt(GetOrderAsString(subRow));
                if (order > originalOrder || (toInsertRowBefore && order == originalOrder))
                {
                    SetOrder(subRow, order + 1);
                    if (order < freeOrder)
                        freeOrder = order;
                }
            }
            if (freeOrder == int.MaxValue)
                freeOrder = originalOrder + 1;
            return Convert.ToString(freeOrder);
        }

        internal static string GetMaxOrderPlus1AsString(System.Data.DataRow row) { return Convert.ToString(GetMaxOrderPlus1AsInt(row)); }
        internal static int GetMaxOrderPlus1AsInt(System.Data.DataRow row) { return GetMaxOrderAsInt(row) + 1; }
        internal static string GetMaxOrderAsString(System.Data.DataRow row) { return Convert.ToString(GetMaxOrderAsInt(row)); }
        internal static int GetMaxOrderAsInt(System.Data.DataRow row)
        {
            int maxOrder = int.MinValue;
            foreach (System.Data.DataRow subRow in GetSubRows(row))
            {
                if (GetOrderAsInt(subRow) > maxOrder)
                    maxOrder = GetOrderAsInt(subRow);
            }
            if (maxOrder == int.MinValue)
                return 0;
            return maxOrder;
        }

        internal static System.Data.DataRow[] GetSubRows(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeCountryRow:
                    return (row as CountryConfig.CountryRow).GetSystemRows();
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).GetPolicyRows();
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).GetFunctionRows();
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).GetParameterRows();
            }
            return null;
        }

        internal static System.Data.DataRow GetSuperRow(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).CountryRow;
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).SystemRow;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).PolicyRow;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).FunctionRow;
            }
            return null;
        }

        internal static int GetOrderAsInt(System.Data.DataRow row) { return EM_Helpers.SaveConvertToInt(GetOrderAsString(row)); }
        internal static string GetOrderAsString(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).Order;
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).Order;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).Order;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).Order;
            }
            return string.Empty;
        }

        internal static string GetRowID(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).ID;
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).ID;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).ID;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).ID;
            }
            return string.Empty;
        }

        internal static void SetOrder(System.Data.DataRow row, int order) { SetOrder(row, Convert.ToString(order)); }
        internal static void SetOrder(System.Data.DataRow row, string order)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    (row as CountryConfig.SystemRow).Order = order; break;
                case _typePolicyRow:
                    (row as CountryConfig.PolicyRow).Order = order; break;
                case _typeFunctionRow:
                    (row as CountryConfig.FunctionRow).Order = order; break;
                case _typeParameterRow:
                    (row as CountryConfig.ParameterRow).Order = order; break;
            }
        }

        internal static string GetRowName(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).Name;
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).Name;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).Name;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).Name;
            }
            return string.Empty;
        }

        internal static void SetRowName(System.Data.DataRow row, string name)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    (row as CountryConfig.SystemRow).Name = name; break;
                case _typePolicyRow:
                    (row as CountryConfig.PolicyRow).Name = name; break;
                case _typeFunctionRow:
                    (row as CountryConfig.FunctionRow).Name = name; break;
                case _typeParameterRow:
                    (row as CountryConfig.ParameterRow).Name = name; break;
            }
        }

        internal static System.Data.DataRow GetRowByID(CountryConfig countryConfig, string ID)
        {
            var query1 = (from ele in countryConfig.System where ele.ID == ID select ele);
            if (query1.Count() > 0)
                return query1.First();
            var query2 = (from ele in countryConfig.Policy where ele.ID == ID select ele);
            if (query2.Count() > 0)
                return query2.First();
            var query3 = (from ele in countryConfig.Function where ele.ID == ID select ele);
            if (query3.Count() > 0)
                return query3.First();
            var query4 = (from ele in countryConfig.Parameter where ele.ID == ID select ele);
            if (query4.Count() > 0)
                return query4.First();
            return null;
        }

        internal static bool IsIdentifier(CountryConfig.SystemRow systemRow, string potentialIdentifier)
        {
            //first try to check (by length) if identifier is actually a GUID (for performance reasons)
            if (potentialIdentifier.Length != _GUID_Length)
                return false;

            //then check if the system actually contains a policy, function or parameter with this ID
            if ((from ele in systemRow.GetPolicyRows()
                 where ele.ID == potentialIdentifier
                 select ele).Count() > 0)
                return true;
            CountryConfig countryConfig = systemRow.Table.DataSet as CountryConfig;
            if ((from ele in countryConfig.Function
                 where ele.PolicyRow.SystemID == systemRow.ID && ele.ID == potentialIdentifier
                 select ele).Count() > 0)
                return true;
            if ((from ele in countryConfig.Parameter
                 where ele.FunctionRow.PolicyRow.SystemID == systemRow.ID && ele.ID == potentialIdentifier
                 select ele).Count() > 0)
                return true;
            return false;
        }

        static System.Data.DataRow GetRowWithSameOrder(System.Data.DataRow originalRow, CountryConfig.SystemRow copySystemRow)
        {
            List<string> orders = new List<string>();
            for (System.Data.DataRow row = originalRow; row.GetType().Name != _typeSystemRow; row = GetSuperRow(row))
                orders.Add(GetOrderAsString(row));

            CountryConfig copyCountryConfig = copySystemRow.Table.DataSet as CountryConfig;
            List <System.Data.DataRow> foundRows = null;
            switch (orders.Count)
            {
                case 1:
                    foundRows = (from policy in copySystemRow.GetPolicyRows()
                                             where policy.Order == orders.ElementAt(0)
                                             select policy).ToList<System.Data.DataRow>();
                    break;
                case 2:
                    foundRows = (from function in copyCountryConfig.Function
                                 where function.Order == orders.ElementAt(0) &&
                                       function.PolicyRow.Order == orders.ElementAt(1) &&
                                       function.PolicyRow.SystemRow.Order == copySystemRow.Order
                                 select function).ToList<System.Data.DataRow>();
                    break;
                case 3:
                    foundRows = (from parameter in copyCountryConfig.Parameter
                                                where parameter.Order == orders.ElementAt(0) &&
                                                      parameter.FunctionRow.Order == orders.ElementAt(1) &&
                                                      parameter.FunctionRow.PolicyRow.Order == orders.ElementAt(2) &&
                                                      parameter.FunctionRow.PolicyRow.SystemRow.Order == copySystemRow.Order
                                                select parameter).ToList<System.Data.DataRow>();
                    break;
            }

            return foundRows.Count == 1 ? foundRows.First() : null;
        }

        internal static void SetRowToNA(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typePolicyRow:
                    SetPolicyToNA(row as CountryConfig.PolicyRow);
                    break;
                case _typeFunctionRow:
                    SetFunctionToNA(row as CountryConfig.FunctionRow);
                    break;
                case _typeParameterRow:
                    (row as CountryConfig.ParameterRow).Value = DefPar.Value.NA;
                    break;
            }
        }

        internal static System.Data.DataRow GetTwinRow(System.Data.DataRow row, string systemID)
        {
            CountryConfig countryConfig = row.Table.DataSet as CountryConfig;

            CountryConfig.PolicyRow policyRow = null;
            CountryConfig.FunctionRow functionRow = null;
            CountryConfig.ParameterRow parameterRow = null;

            switch (row.GetType().Name)
            {
                case _typePolicyRow:
                    policyRow = row as CountryConfig.PolicyRow;
                    break;
                case _typeFunctionRow:
                    functionRow = row as CountryConfig.FunctionRow;
                    policyRow = functionRow.PolicyRow; //the (parent) policy row is necessary to improve performance (see below)
                    break;
                case _typeParameterRow:
                    parameterRow = row as CountryConfig.ParameterRow;
                    functionRow = parameterRow.FunctionRow; //the (parent) function and policy rows are necessary to improve performance (see below)
                    policyRow = functionRow.PolicyRow;
                    break;
            }

            //first assess the twin policy ...
            List<CountryConfig.PolicyRow> policyRows = (from policy in countryConfig.Policy
                    where policy.Order == policyRow.Order &&
                          policy.SystemID == systemID
                    select policy).ToList();
            if (policyRows.Count == 0)
                return null; //should not happen
            if (functionRow == null)
                return policyRows.First(); //... if the reference was a policy the task is accomplished

            //... then assess the twin function, but only look within the functions of the respective policy, otherwise things get slow ...
            List<CountryConfig.FunctionRow> functionRows = (from function in policyRows.First().GetFunctionRows()
                    where function.Order == functionRow.Order &&
                          function.PolicyRow.Order == functionRow.PolicyRow.Order &&
                          function.PolicyRow.SystemID == systemID
                    select function).ToList();
            if (functionRows.Count == 0)
                return null; //should not happen
            if (parameterRow == null)
                return functionRows.First(); //... if the reference was a function the task is accomplished

            //... finally assess the twin parameter, but only look within the parameters of the respective function, otherwise things get VERY slow ...
            List<CountryConfig.ParameterRow> parameterRows = (from parameter in functionRows.First().GetParameterRows()
                    where parameter.Order == parameterRow.Order &&
                          parameter.FunctionRow.Order == parameterRow.FunctionRow.Order &&
                          parameter.FunctionRow.PolicyRow.Order == parameterRow.FunctionRow.PolicyRow.Order &&
                          parameter.FunctionRow.PolicyRow.SystemID == systemID
                    select parameter).ToList();

            if (parameterRows.Count == 0)
                return null; //should not happen
            return parameterRows.First();
        }

        internal static System.Data.DataRow GetLastSibling(System.Data.DataRow row)
        {
            int maxOrder = int.MinValue;
            System.Data.DataRow lastChild = null;
            foreach (System.Data.DataRow subRow in GetSubRows(GetSuperRow(row)))
            {
                int order = GetOrderAsInt(subRow);
                if (order > maxOrder)
                {
                    maxOrder = order;
                    lastChild = subRow;
                }
            }
            return lastChild;
        }

        internal static System.Data.DataRow CopyPolicyOrFunction(System.Data.DataRow originalRow, System.Data.DataRow neighbourRow = null, bool insertBefore = true,
                                                                 System.Data.DataRow parentRow = null, bool setToNA = false)
        {
            System.Data.DataRow copiedRow = null;
            switch (originalRow.GetType().Name)
            {
                case _typePolicyRow:
                    //note: does not allow for empty neighbourRow, i.e. function is not supposed to be used for copying into completely empty system
                    copiedRow = CopyPolicyRow(originalRow as CountryConfig.PolicyRow, (originalRow as CountryConfig.PolicyRow).Name,
                                neighbourRow as CountryConfig.PolicyRow, insertBefore, (originalRow as CountryConfig.PolicyRow).Switch == DefPar.Value.NA);
                    break;
                case _typeFunctionRow:
                    if (neighbourRow != null)
                        copiedRow = CopyFunctionRow(originalRow as CountryConfig.FunctionRow, neighbourRow as CountryConfig.FunctionRow, insertBefore,
                                    (originalRow as CountryConfig.FunctionRow).Switch == DefPar.Value.OFF);
                    else
                        copiedRow = CopyFunctionRowAtTailOrUseOriginalOrder(originalRow as CountryConfig.FunctionRow, parentRow as CountryConfig.PolicyRow,
                                    false, //copyAtTail
                                    (originalRow as CountryConfig.FunctionRow).Switch == DefPar.Value.OFF);
                    break;
            }
            if (copiedRow != null && setToNA)
                SetRowToNA(copiedRow);
            return copiedRow;
        }

        internal void RegisterWithUndoManager(ADOUndoManager undoMananger)
        {
            undoMananger.AddDataSet(_countryConfig);
        }

        internal static bool IsNA(System.Data.DataRow row)
        {
            string naParameter = string.Empty;
            switch (row.GetType().Name)
            {
                case _typePolicyRow:
                    naParameter = (row as CountryConfig.PolicyRow).Switch;
                    break;
                case _typeFunctionRow:
                    naParameter = (row as CountryConfig.FunctionRow).Switch;
                    break;
                case _typeParameterRow:
                    naParameter = (row as CountryConfig.ParameterRow).Value;
                    break;
            }
            return naParameter == DefPar.Value.NA;
        }

        internal bool IsNAInAllSystems(System.Data.DataRow row, List<CountryConfig.SystemRow> relevantSystems = null)
        {
            List<string> relevantSystemIDs = new List<string>();
            if (relevantSystems != null)
                relevantSystemIDs = (from system in relevantSystems select system.ID).ToList();
            foreach (CountryConfig.SystemRow systemRow in _countryConfig.System)
            {
                if (!relevantSystemIDs.Contains(systemRow.ID))
                    continue;
                System.Data.DataRow twinRow = GetTwinRow(row, systemRow.ID);
                if (!IsNA(twinRow))
                    return false;
            }
            return true;
        }

        internal List<System.Data.DataRow> GetTwinRows(System.Data.DataRow row, List<CountryConfig.SystemRow> relevantSystems = null)
        {
            List<System.Data.DataRow> twinRows = new List<System.Data.DataRow>();
            List<string> relevantSystemIDs = new List<string>();
            if (relevantSystems != null)
                relevantSystemIDs = (from system in relevantSystems select system.ID).ToList();

            foreach (CountryConfig.SystemRow systemRow in _countryConfig.System)
            {
                if (!relevantSystemIDs.Contains(systemRow.ID))
                    continue;
                twinRows.Add(GetTwinRow(row, systemRow.ID));
            }

            return twinRows;
        }

        internal static bool IsPolicyRow(System.Data.DataRow row)
        {
            return row.GetType().Name == _typePolicyRow;
        }

        internal static bool IsFunctionRow(System.Data.DataRow row)
        {
            return row.GetType().Name == _typeFunctionRow;
        }

        internal static bool IsParameterRow(System.Data.DataRow row)
        {
            return row.GetType().Name == _typeParameterRow;
        }

        internal static CountryConfig.SystemRow GetSystemRow(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).SystemRow;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).PolicyRow.SystemRow;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).FunctionRow.PolicyRow.SystemRow;
            }
            return null;
        }

        internal static System.Data.DataRow GetParentRow(System.Data.DataRow row)
        {
            switch (row.GetType().Name)
            {
                case _typeSystemRow:
                    return (row as CountryConfig.SystemRow).CountryRow;
                case _typePolicyRow:
                    return (row as CountryConfig.PolicyRow).SystemRow;
                case _typeFunctionRow:
                    return (row as CountryConfig.FunctionRow).PolicyRow;
                case _typeParameterRow:
                    return (row as CountryConfig.ParameterRow).FunctionRow;
            }
            return null;
        }

        internal static List<System.Data.DataRow> GetChildRows(System.Data.DataRow row)
        {
            List<System.Data.DataRow> childRows = new List<System.Data.DataRow>();
            switch (row.GetType().Name)
            {
                case _typeCountryRow:
                    foreach (CountryConfig.SystemRow systemRow in (row as CountryConfig.CountryRow).GetSystemRows())
                        childRows.Add(systemRow);
                    break;
                case _typeSystemRow:
                    foreach (CountryConfig.PolicyRow policyRow in (row as CountryConfig.SystemRow).GetPolicyRows())
                        childRows.Add(policyRow);
                    break;
                case _typePolicyRow:
                    foreach (CountryConfig.FunctionRow functionRow in (row as CountryConfig.PolicyRow).GetFunctionRows())
                        childRows.Add(functionRow);
                    break;
                case _typeFunctionRow:
                    foreach (CountryConfig.ParameterRow parameterRow in (row as CountryConfig.FunctionRow).GetParameterRows())
                        childRows.Add(parameterRow);
                    break;
            }
            return childRows;
        }

        internal static CountryConfig.PolicyRow GetPrePolicyRow(CountryConfig.PolicyRow policyRow) { return GetPreRow(policyRow) as CountryConfig.PolicyRow; }
        internal static CountryConfig.FunctionRow GetPreFunctionRow(CountryConfig.FunctionRow functionRow) { return GetPreRow(functionRow) as CountryConfig.FunctionRow; }
        internal static CountryConfig.ParameterRow GetPreParameterRow(CountryConfig.ParameterRow parameterRow) { return GetPreRow(parameterRow) as CountryConfig.ParameterRow; }
        internal static System.Data.DataRow GetPreRow(System.Data.DataRow row)
        {
            System.Data.DataRow preRow = null;
            foreach (System.Data.DataRow siblingRow in GetChildRows(GetParentRow(row)))
            {
                if (GetOrderAsInt(siblingRow) < GetOrderAsInt(row) &&
                    (preRow == null || GetOrderAsInt(preRow) < GetOrderAsInt(siblingRow)))
                    preRow = siblingRow;
            }
            return preRow;
        }

        internal static CountryConfig.PolicyRow GetPostPolicyRow(CountryConfig.PolicyRow policyRow) { return GetPostRow(policyRow) as CountryConfig.PolicyRow; }
        internal static CountryConfig.FunctionRow GetPostFunctionRow(CountryConfig.FunctionRow functionRow) { return GetPostRow(functionRow) as CountryConfig.FunctionRow; }
        internal static CountryConfig.ParameterRow GetPostParameterRow(CountryConfig.ParameterRow parameterRow) { return GetPostRow(parameterRow) as CountryConfig.ParameterRow; }
        internal static System.Data.DataRow GetPostRow(System.Data.DataRow row)
        {
            System.Data.DataRow postRow = null;
            foreach (System.Data.DataRow siblingRow in GetChildRows(GetParentRow(row)))
            {
                if (GetOrderAsInt(siblingRow) > GetOrderAsInt(row) &&
                    (postRow == null || GetOrderAsInt(postRow) > GetOrderAsInt(siblingRow)))
                    postRow = siblingRow;
            }
            return postRow;
        }

        internal static CountryConfig.PolicyRow GetFirstPolicyRow(CountryConfig.SystemRow parentRow) { return GetFirstRow(parentRow) as CountryConfig.PolicyRow; }
        internal static CountryConfig.FunctionRow GetFirstFunctionRow(CountryConfig.PolicyRow parentRow) { return GetFirstRow(parentRow) as CountryConfig.FunctionRow; }
        internal static CountryConfig.ParameterRow GetFirstParameterRow(CountryConfig.FunctionRow parentRow) { return GetFirstRow(parentRow) as CountryConfig.ParameterRow; }
        internal static System.Data.DataRow GetFirstRow(System.Data.DataRow parentRow)
        {
            System.Data.DataRow firstRow = null;
            foreach (System.Data.DataRow row in GetChildRows(parentRow))
            {
                if (firstRow == null || GetOrderAsInt(firstRow) > GetOrderAsInt(row))
                    firstRow = row;
            }
            return firstRow;
        }

        internal static CountryConfig.PolicyRow GetLastPolicyRow(CountryConfig.SystemRow parentRow) { return GetLastRow(parentRow) as CountryConfig.PolicyRow; }
        internal static CountryConfig.FunctionRow GetLastFunctionRow(CountryConfig.PolicyRow parentRow) { return GetLastRow(parentRow) as CountryConfig.FunctionRow; }
        internal static CountryConfig.ParameterRow GetLastParameterRow(CountryConfig.FunctionRow parentRow) { return GetLastRow(parentRow) as CountryConfig.ParameterRow; }
        internal static System.Data.DataRow GetLastRow(System.Data.DataRow parentRow)
        {
            System.Data.DataRow lastRow = null;
            foreach (System.Data.DataRow row in GetChildRows(parentRow))
            {
                if (lastRow == null || GetOrderAsInt(lastRow) < GetOrderAsInt(row))
                    lastRow = row;
            }
            return lastRow;
        }

        internal void RemoveAllNodeColors()
        {
            foreach (CountryConfig.PolicyRow policyRow in (from row in _countryConfig.Policy
                                                           where !row.IsNull(_columnName_Color) && row.Color != DefPar.Value.NO_COLOR
                                                           select row))
                policyRow.Color = DefPar.Value.NO_COLOR;
            foreach (CountryConfig.FunctionRow functionRow in (from row in _countryConfig.Function
                                                           where !row.IsNull(_columnName_Color) && row.Color != DefPar.Value.NO_COLOR
                                                           select row))
                functionRow.Color = DefPar.Value.NO_COLOR;
            foreach (CountryConfig.ParameterRow parameterRow in (from row in _countryConfig.Parameter
                                                               where !row.IsNull(_columnName_Color) && row.Color != DefPar.Value.NO_COLOR
                                                               select row))
                parameterRow.Color = DefPar.Value.NO_COLOR;
        }

        internal static void StraightenChildOrder(System.Data.DataRow parentRow)
        {
            SortedList<int, System.Data.DataRow> sortedChildren = new SortedList<int, System.Data.DataRow>();
            foreach (System.Data.DataRow childRow in GetChildRows(parentRow))
                sortedChildren.Add(GetOrderAsInt(childRow), childRow);
            foreach (System.Data.DataRow sortedChild in sortedChildren.Values)
                SetOrder(sortedChild, sortedChildren.IndexOfValue(sortedChild));
        }

        #endregion general_functions

        #region country_functions

        internal static void AddCountryRowAndGenerateXML(string countryLongName, string countryShortName, string storagePath)
        {
            CountryConfig countryConfig = new CountryConfig();
            countryConfig.Country.AddCountryRow(Guid.NewGuid().ToString(), countryLongName, countryShortName, DefPar.Value.NO);
            Stream fileStream = new FileStream(storagePath + countryShortName + ".xml", FileMode.Create);
            using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING, CountryConfigFacade._cDataElements, false))
                countryConfig.WriteXml(xmlWriter);
        }

        internal CountryConfig.CountryRow GetCountryRow()
        {
            List<CountryConfig.CountryRow> helperList = (from country in _countryConfig.Country select country).ToList();
            if (helperList.Count > 0)
                return helperList.First();
            return null;
        }

        #endregion country_functions

        #region conditional_format_functions

        internal List<CountryConfig.ConditionalFormatRow> GetConditionalFormatRowsOfSystem(CountryConfig.SystemRow systemRow)
        {
            return (from conditionalFormat in _countryConfig.ConditionalFormat
                    join conditionalFormatSystems in _countryConfig.ConditionalFormat_Systems
                                                  on conditionalFormat.ID equals conditionalFormatSystems.ConditionalFormatID
                    where conditionalFormatSystems.SystemName == systemRow.Name
                    select conditionalFormat).ToList();
        }

        internal List<CountryConfig.ConditionalFormatRow> GetConditionalFormatRows()
        {
            return (from conditionalFormat in _countryConfig.ConditionalFormat select conditionalFormat).ToList();
        }

        internal CountryConfig.ConditionalFormatRow GetConditionalFormatRowByID(string ID)
        {
            var query = from conditionalFormat in _countryConfig.ConditionalFormat where conditionalFormat.ID == ID select conditionalFormat;
            return (query.Count() == 1) ? query.First() : null;
        }

        internal void DeleteConditionalFormatRow(CountryConfig.ConditionalFormatRow conditionalFormatRow)
        {
            _countryConfig.ConditionalFormat.Rows.Remove(conditionalFormatRow);
        }

        internal CountryConfig.ConditionalFormatRow AddConditionalFormatRow(string backColor, string foreColor, string condition, string baseSystemName)
        {
            return _countryConfig.ConditionalFormat.AddConditionalFormatRow(Guid.NewGuid().ToString(), backColor, foreColor, condition, baseSystemName);
        }

        internal static void CopyConditionalFormatRowToAnotherCountry(CountryConfig countryConfig, CountryConfig.ConditionalFormatRow originalRow)
        {
            countryConfig.ConditionalFormat.ImportRow(originalRow);
            foreach (CountryConfig.ConditionalFormat_SystemsRow condFormatSysRow in originalRow.GetConditionalFormat_SystemsRows())
                countryConfig.ConditionalFormat_Systems.ImportRow(condFormatSysRow);
        }

        internal CountryConfig.ConditionalFormat_SystemsRow AddConditionalFormat_SystemsRow(CountryConfig.ConditionalFormatRow conditionalFormatRow, CountryConfig.SystemRow systemRow)
        {
            return _countryConfig.ConditionalFormat_Systems.AddConditionalFormat_SystemsRow(conditionalFormatRow, systemRow);
        }

        internal void setAutomaticConditionalFormatting(bool _resetBase)
        {
            // first get all system names
            List<CountryConfig.SystemRow> allSystemRows = GetSystemRowsOrdered();   // make sure that you get them ordered 

            foreach (CountryConfig.SystemRow sr in allSystemRows)
            {
                string baseSystemName = string.Empty;
                CountryConfig.ConditionalFormatRow conditionalFormatRow = null;
                // First make sure that this system does not already have a base system
                bool hasBase = false;
                foreach (CountryConfig.ConditionalFormat_SystemsRow cf_sr in sr.GetConditionalFormat_SystemsRows())
                {
                    if (cf_sr.ConditionalFormatRow != null && cf_sr.ConditionalFormatRow.Condition == string.Empty && cf_sr.ConditionalFormatRow.BaseSystemName != string.Empty)
                    {
                        conditionalFormatRow = cf_sr.ConditionalFormatRow;  // a base system was found
                        hasBase = true;
                        break;
                    }
                }
                if (!hasBase || _resetBase)    // if there is no base system, or if we need to reset it, then analyze the system name and try to match the best base system
                {
                    System.Text.RegularExpressions.Regex standard_name = new 
                    System.Text.RegularExpressions.Regex(@"(\w+)_(\d{4,4})(.+)?");   // match any number of letters for the country name + "_" + four digits for the year + optionally one or more characters after that
                    System.Text.RegularExpressions.Match match = standard_name.Match(sr.Name);
                    if (match.Success)
                    {
                        if (match.Groups[3].Value == string.Empty)    // mached standard name (e.g. DE_2007)
                        {
                            bool found = false;
                            int iy = 0;
                            while (!found && iy++ < 10)
                            {     // search up to 10 years before current system
                                int yr = int.Parse(match.Groups[2].Value) - iy;
                                baseSystemName = match.Groups[1].Value + "_" + yr;
                                if (allSystemRows.Find(x => x.Name == baseSystemName) != null) found = true;
                            }
                            if (!found) baseSystemName = string.Empty;
                        }
                        else    // mached expanded name (e.g. DE_2007_xxxxxxx)
                        {
                            baseSystemName = match.Groups[1].Value + "_" + match.Groups[2].Value;
                            if (allSystemRows.Find(x => x.Name == baseSystemName) == null) baseSystemName = string.Empty;
                        }
                    }
                    if (baseSystemName != string.Empty)     // if baseSystemName is not empty, then a match was found and we need to add it as a base system!
                    {
                        if (hasBase && _resetBase) conditionalFormatRow.Delete();   // if there already was a base system, delete it
                        conditionalFormatRow = AddConditionalFormatRow(             // then create the new base system
                            ConditionalFormattingHelper.GetDisplayTextFromColor(EM_UI.Dialogs.ConditionalFormattingForm._defaultColorSystemDifferences),
                            ConditionalFormattingHelper._noSpecialColor,
                            string.Empty, baseSystemName);
                        AddConditionalFormat_SystemsRow(conditionalFormatRow, sr);  // finally add the new base system
                    }
                }
            }
        }

        #endregion conditional_format_functions

        #region uprating_index_functions

        internal List<CountryConfig.UpratingIndexRow> GetUpratingIndices() { return _countryConfig.UpratingIndex.ToList(); }

        internal CountryConfig.UpratingIndexRow GetUpratingIndex(string reference)
        {
            List<CountryConfig.UpratingIndexRow> uis = (from ui in _countryConfig.UpratingIndex
                                                        where ui.Reference.ToLower() == reference.ToLower()
                                                        select ui).ToList();
            return uis.Count == 0 ? null : uis[0];
        }

        internal Dictionary<int, double> GetUpratingIndexYearValues(CountryConfig.UpratingIndexRow index, bool includeEmpty = false)
        {
            Dictionary<int, double> yearValues = new Dictionary<int, double>();
            foreach (string yv in index.YearValues.Split(UpratingIndices.UpratingIndicesForm._separator))
            {
                if (string.IsNullOrEmpty(yv)) continue;
                int year; double value;
                bool valEmpty = !EM_Helpers.TryConvertToDouble(yv.Substring(5), out value);
                if (int.TryParse(yv.Substring(0, 4), out year) && !yearValues.ContainsKey(year) && (!valEmpty || includeEmpty))
                    yearValues.Add(year, valEmpty ? -1 : value);
            }
            return yearValues;
        }

        internal List<string> GetAllUpratingIndexYears()
        {
            List<string> years = new List<string>();
            foreach (CountryConfig.UpratingIndexRow index in _countryConfig.UpratingIndex)
                foreach(int year in GetUpratingIndexYearValues(index).Keys)
                    if (!years.Contains(year.ToString())) years.Add(year.ToString());
            return years;
        }

        internal void UpdateUpratingIndices(List<Tuple<string, string, string, string>> indices)
        {
            foreach (var upInd in indices)
            {
                List<CountryConfig.UpratingIndexRow> ind = (from i in _countryConfig.UpratingIndex where i.Reference.ToLower() == upInd.Item2.ToLower() select i).ToList();
                if (ind.Count == 0) _countryConfig.UpratingIndex.AddUpratingIndexRow(Guid.NewGuid().ToString(), upInd.Item1, upInd.Item2, upInd.Item3, upInd.Item4);
                else { ind[0].Description = upInd.Item1; ind[0].Reference = upInd.Item2; ind[0].Comment = upInd.Item3; ind[0].YearValues = upInd.Item4; }
            }
            List<string> refs = (from i in indices select i.Item2.ToLower()).ToList();
            for (int i = _countryConfig.UpratingIndex.Count - 1; i >= 0; --i)
                if (!refs.Contains(_countryConfig.UpratingIndex[i].Reference.ToLower())) _countryConfig.UpratingIndex.ElementAt(i).Delete();
        }

        void MigrateUpratingIndices()
        {
            try
            {
                List<CountryConfig.PolicyRow> pols = (from p in _countryConfig.Policy where p.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name) select p).ToList();
                if (pols.Count == 0) return;

                List<CountryConfig.ParameterRow> parameterRows = new List<CountryConfig.ParameterRow>();
                // get the (single, switched-off) DefVar function, which contains the raw indices
                CountryConfig.FunctionRow functionIndices = GetFunctionRowsByPolicyIDAndFunctionName(pols.First().ID, DefFun.DefVar).First();
                parameterRows = functionIndices.GetParameterRows().ToList();

                // first parameter-row of the function contains the available years (as name) in the form "2007°2008°...°2013°2014"
                if (parameterRows.ElementAt(0).Name == string.Empty) return; // no years available
                List<string> years = parameterRows.ElementAt(0).Name.Split(UpratingIndicesForm._separator).ToList();

                // each other parameter-row of the function contains information for one index in the form "index-description°index-name°factor for 2007°...°factor for 2014°comment",
                //                                                                                     e.g."Consumer Price Index°$f_cpi°100.0°105.3°...°149.7°152.1°Source: National Statistic"
                for (int i = 1; i < parameterRows.Count; ++i)
                {
                    string info = parameterRows.ElementAt(i).Name;
                    List<string> infoSplit = new List<string>(info.Split(UpratingIndicesForm._separator));
                    if (infoSplit.Count != years.Count + 3) continue; // should not happen

                    CountryConfig.UpratingIndexRow upInd = _countryConfig.UpratingIndex.AddUpratingIndexRow(Guid.NewGuid().ToString(),
                                                            infoSplit[0], infoSplit[1], infoSplit[infoSplit.Count - 1].Replace("§$&", "#"), string.Empty);
                    for (int y = 0; y < years.Count; ++y)
                        upInd.YearValues += years[y] + UpratingIndicesForm._separatorYear + infoSplit[y + 2] + UpratingIndicesForm._separator;
                    upInd.YearValues = upInd.YearValues.TrimEnd(new char[] { UpratingIndicesForm._separator });
                }

                for (int p = pols.Count - 1; p >= 0; --p) pols[p].Delete();

                _countryConfig.AcceptChanges();
            }
            catch (Exception) { _countryConfig.RejectChanges(); }
        }

        #endregion uprating_index_functions
    }
}
