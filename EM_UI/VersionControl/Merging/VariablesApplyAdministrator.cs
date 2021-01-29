using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EM_UI.VersionControl.Merging
{
    internal class VariablesApplyAdministrator
    {
        MergeAdministrator _mergeAdministrator = null;

        VarConfigFacade _vcFacLocal = null, _vcFacRemote = null;
        VarConfig _vcLocal = null, _vcRemote = null;

        VariablesMergeForm _mergeForm = null;
        MergeControl _mcVariables = null;
        MergeControl _mcAcronyms = null;
        MergeControl _mcCountryLabels = null;
        MergeControl _mcSwitchablePolicies = null;

        const short LEVEL_INVALID = 0;
        const short LEVEL_ACROTYPE = 1;
        const short LEVEL_ACROLEVEL = 2;
        const short LEVEL_ACRO = 3;
        const short LEVEL_ACROCAT = 4;

        internal VariablesApplyAdministrator(MergeAdministrator mergeAdministrator, VariablesMergeForm mergeForm,
                                             VarConfigFacade vcFacLocal, VarConfigFacade vcFacRemote)
        {
            _mergeAdministrator = mergeAdministrator;

            _vcFacLocal = vcFacLocal; _vcLocal = _vcFacLocal.GetVarConfig();
            _vcFacRemote = vcFacRemote; _vcRemote = _vcFacRemote.GetVarConfig();

            _mergeForm = mergeForm;
            _mcVariables = _mergeForm.GetMergeControlByName(VariablesMergeForm.VARIABLES);
            _mcAcronyms = _mergeForm.GetMergeControlByName(VariablesMergeForm.ACRONYMS);
            _mcCountryLabels = _mergeForm.GetMergeControlByName(VariablesMergeForm.COUNTRY_LABELS);
            _mcSwitchablePolicies = _mergeForm.GetMergeControlByName(VariablesMergeForm.SWITCHABLE_POLICIES);
        }

        internal void Apply()
        {
            //apply locally rejected or remotely accepted changes of variables (e.g. monetary-state)
            ApplyChanges(_mcVariables, _vcLocal.Variable, _vcLocal.Variable.IDColumn.ColumnName, true);
            ApplyChanges(_mcVariables, _vcLocal.Variable, _vcLocal.Variable.IDColumn.ColumnName, false);

            //remove locally added and rejected or remotely removed and accepted variables
            ApplyRemovals(_mcVariables, _vcLocal.Variable, _vcLocal.Variable.IDColumn.ColumnName, true);
            ApplyRemovals(_mcVariables, _vcLocal.Variable, _vcLocal.Variable.IDColumn.ColumnName, false);

            //add locally removed and rejected or remotely added and accepted variables
            AddVariables(true); AddVariables(false);

            //apply locally rejected or remotely accepted changes of country-specific descriptions
            ApplyChanges(_mcCountryLabels, _vcLocal.CountryLabel, _vcLocal.CountryLabel.IDColumn.ColumnName, true);
            ApplyChanges(_mcCountryLabels, _vcLocal.CountryLabel, _vcLocal.CountryLabel.IDColumn.ColumnName, false);
            
            //add locally removed and rejected or remotely added and accepted labels of a whole country
            AddLabelsWholeCountry(true); AddLabelsWholeCountry(false);

            //remove locally added and rejected or remotely removed and accepted labels of a whole country
            RemoveLabelsWholeCountry(true); RemoveLabelsWholeCountry(false);

            //apply locally rejected or remotely accepted changes of switchable policies
            //ApplyChanges(_mcSwitchablePolicies, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, true);
            //ApplyChanges(_mcSwitchablePolicies, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, false);

            //remove locally added and rejected or remotely removed and accepted switchable policies
            //ApplyRemovals(_mcSwitchablePolicies, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, true);
            //ApplyRemovals(_mcSwitchablePolicies, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, false);

            //add locally removed and rejected or remotely added and accepted switchable policies
            //AddSwitchPolicies(true); AddSwitchPolicies(false);

            //apply locally rejected or remotely accepted changes of acronyms  (-types, -levels and categories)
            ChangeAcros(true); ChangeAcros(false);

            //remove locally added and rejected or remotely removed and accepted acronyms (-types, -levels and categories)
            RemoveAcros(true); RemoveAcros(false);

            //add locally removed and rejected or remotely added and accepted acronyms (-types, -levels and categories)
            AddAcros(true); AddAcros(false);
        }

        List<string> GetRelevantIDs(MergeControl mergeControl, bool local, bool add)
        {
            List<string> relevantIDs = new List<string>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (local ? (add ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added)
                                                  : (add ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed)) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-remove nor an accepted remote-add
                relevantIDs.Add(nodeInfo.ID);
            }
            return relevantIDs;
        }

        void ApplyRemovals(MergeControl mergeControl, DataTable localDataTable, string idColumnName, bool local)
        {
            foreach (string ID in GetRelevantIDs(mergeControl, local, false))
            {
                DataRow dataRow = localDataTable.Select(idColumnName + " = '" + ID + "'").First();
                dataRow.Delete();
            }
            _vcFacLocal.GetVarConfig().AcceptChanges();
        }

        short AssessAcroLevel(string ID, bool local, out DataRow dataRow, List<string> allIDs = null)
        {
            VarConfigFacade _vcFac = local ? _vcFacLocal : _vcFacRemote;
            dataRow = _vcFac.GetAcronymTypeByID(ID);
            if (dataRow != null) return LEVEL_ACROTYPE;

            VarConfig.AcronymLevelRow acroLevel = _vcFac.GetAcronymLevelByID(ID);
            if (acroLevel != null)
            {
                if (allIDs != null && allIDs.Contains(acroLevel.AcronymTypeRow.ID)) return LEVEL_INVALID; //the level is already taken into account, by the action on type-level (e.g. deleted via the type instead of individually)
                dataRow = acroLevel; return LEVEL_ACROLEVEL;
            }
            VarConfig.AcronymRow acro = _vcFac.GetAcronymByID(ID);
            if (acro != null)
            {
                if (allIDs != null && (allIDs.Contains(acro.AcronymLevelRow.ID) ||
                                       allIDs.Contains(acro.AcronymLevelRow.AcronymTypeRow.ID))) return LEVEL_INVALID; //see comment above
                dataRow = acro; return LEVEL_ACRO;
            }
            if (!ID.Contains("#")) return LEVEL_INVALID;
            string catValue = ID.Split(new string[] { "#" }, StringSplitOptions.None)[0];
            string acroID = ID.Split(new string[]{"#"}, StringSplitOptions.None)[1];
            VarConfig.CategoryRow categ = _vcFac.GetCategoryByKey(acroID, catValue);
            if (categ != null)
            {
                if (allIDs != null && (allIDs.Contains(categ.AcronymRow.ID) ||
                                       allIDs.Contains(categ.AcronymRow.AcronymLevelRow.ID) ||
                                       allIDs.Contains(categ.AcronymRow.AcronymLevelRow.AcronymTypeRow.ID))) return LEVEL_INVALID; //see comment above
                dataRow = categ; return LEVEL_ACROCAT;
            }
            dataRow = null; return LEVEL_INVALID;
        }

        void ChangeAcros(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcAcronyms.GetNodeInfoLocal() : _mcAcronyms.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                const string NOT_CHANGED = "NOT_CHANGED";
                MergeControl.CellInfo cellInfo = nodeInfo.cellInfo.First();
                MergeControl.CellInfo cellInfoRemote = local ? _mcAcronyms.GetTwinCellInfo(cellInfo) : cellInfo;
                string changedName = (cellInfo.isChanged && (local ? !cellInfo.acceptChange : cellInfo.acceptChange)) //if name is changed and remotely accepted or locally rejected 
                                     ? cellInfoRemote.text : NOT_CHANGED; //change to remote name (otherwise mark not changed)
                cellInfo = nodeInfo.cellInfo.Last();
                cellInfoRemote = local ? _mcAcronyms.GetTwinCellInfo(cellInfo) : cellInfo;
                string changedDesc = (cellInfo.isChanged && (local ? !cellInfo.acceptChange : cellInfo.acceptChange)) //if description is changed and remotely accepted or locally rejected
                                     ? cellInfoRemote.text : NOT_CHANGED; //change to remote description (otherwise mark not changed)
                DataRow dataRow;
                switch (AssessAcroLevel(nodeInfo.ID, true, out dataRow))
                {
                    case LEVEL_INVALID: continue; //should not happen

                    case LEVEL_ACROTYPE:
                        VarConfig.AcronymTypeRow acroType = dataRow as VarConfig.AcronymTypeRow;
                        if (changedName != NOT_CHANGED) acroType.ShortName = changedName;
                        if (changedDesc != NOT_CHANGED) acroType.LongName = changedDesc;
                        break;

                    case LEVEL_ACROLEVEL:
                        VarConfig.AcronymLevelRow acroLevel = dataRow as VarConfig.AcronymLevelRow;
                        if (changedName != NOT_CHANGED) acroLevel.Index = Convert.ToInt32(changedName); //probably not relevant
                        if (changedDesc != NOT_CHANGED) acroLevel.Name = changedDesc;
                        break;

                    case LEVEL_ACRO:
                        VarConfig.AcronymRow acro = dataRow as VarConfig.AcronymRow;
                        if (changedName != NOT_CHANGED) acro.Name = changedName;
                        if (changedDesc != NOT_CHANGED) acro.Description = changedDesc;
                        break;

                    case LEVEL_ACROCAT:
                        VarConfig.CategoryRow categ = dataRow as VarConfig.CategoryRow;
                        if (changedName != NOT_CHANGED) categ.Value = changedName;
                        if (changedDesc != NOT_CHANGED) categ.Description = changedDesc;
                        break;
                }
            }
        }

        void AddAcros(bool local)
        {
            List<string> addIDs = GetRelevantIDs(_mcAcronyms, local, true);
            foreach (string ID in addIDs)
            {
                DataRow dataRow; 
                switch (AssessAcroLevel(ID, false, out dataRow))
                {
                    case LEVEL_INVALID: continue; //should not happen
                    
                    case LEVEL_ACROTYPE:
                        VarConfig.AcronymTypeRow acroType = dataRow as VarConfig.AcronymTypeRow;
                        _vcFacLocal.CopyAcronymTypeRow(acroType); //note: this copies without content (i.e. without included levels, their acronyms and their categories (which may have been refused))
                        break;
                    
                    case LEVEL_ACROLEVEL:
                        VarConfig.AcronymLevelRow acroLevel = dataRow as VarConfig.AcronymLevelRow;
                        VarConfig.AcronymTypeRow parentType = _vcFacLocal.GetAcronymTypeByID(acroLevel.AcronymTypeRow.ID);
                        if (parentType != null) //possible though non-sense (see comment below)
                            _vcFacLocal.CopyAcronymLevelRow(acroLevel, parentType); //see note above (i.e. included acronyms and their categories are not copied)
                        break;
                    
                    case LEVEL_ACRO:
                        VarConfig.AcronymRow acro = dataRow as VarConfig.AcronymRow;
                        VarConfig.AcronymLevelRow parentLevel = _vcFacLocal.GetAcronymLevelByID(acro.AcronymLevelRow.ID);
                        if (parentLevel != null) //possible though non-sense (see comment below)
                            _vcFacLocal.CopyAcronymRow(acro, parentLevel); //see note above (i.e. included categories are not copied)
                        break;
                    
                    case LEVEL_ACROCAT:
                        VarConfig.CategoryRow categ = dataRow as VarConfig.CategoryRow;
                        VarConfig.AcronymRow parentAcro = _vcFacLocal.GetAcronymByID(categ.AcronymRow.ID);
                        if (parentAcro != null) //may happen if (though non-sense) copying acro was refused while copying category was accepted
                            _vcFacLocal.CopyCategoryRow(categ, parentAcro);
                        break;
                }
            }

            _vcFacLocal.GetVarConfig().AcceptChanges();
        }

        void RemoveAcros(bool local)
        {
            List<string> removeIDs = GetRelevantIDs(_mcAcronyms, local, false);
            foreach (string ID in removeIDs)
            {
                DataRow dataRow; if (AssessAcroLevel(ID, true, out dataRow, removeIDs) != LEVEL_INVALID) dataRow.Delete();
                _vcFacLocal.GetVarConfig().AcceptChanges();
            }          
        }

        void AddVariables(bool local)
        {
            foreach (string ID in GetRelevantIDs(_mcVariables, local, true))
                VarConfigFacade.CopyVariableFromAnotherConfig(_vcLocal, _vcFacRemote.GetVariableByID(ID));
        }

        //void AddSwitchPolicies(bool local)
        //{
        //    foreach (string ID in GetRelevantIDs(_mcSwitchablePolicies, local, true))
        //        SwitchablePolicyConfigFacade.CopySwitchPolicyFromAnotherConfig(_vcLocal, EM_AppContext.Instance.GetSwitchablePolicyConfigFacade().GetSwitchablePolicy(ID));
        //}

        void ApplyChanges(MergeControl mergeControl, DataTable localDataTable, string idColumnName, bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                DataRow dataRow = localDataTable.Select(idColumnName + " = '" + nodeInfo.ID + "'").First();

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange))
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? mergeControl.GetTwinCellInfo(cellInfo) : cellInfo;
                    dataRow.SetField<string>(cellInfo.columnID, cellInfoRemote.text);
                }
            }
        }

        void AddLabelsWholeCountry(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcCountryLabels.GetNodeInfoLocal() : _mcCountryLabels.GetNodeInfoRemote())
            {
                if (!nodeInfo.ID.StartsWith(MergeAdministrator.IDENTIFIER_ADD_DEL_COUNTRY_LABELS) ||
                    nodeInfo.changeType != (local ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                continue;

                string countryShortName = nodeInfo.ID.Substring(MergeAdministrator.IDENTIFIER_ADD_DEL_COUNTRY_LABELS.Length).ToLower();
                foreach (VarConfig.CountryLabelRow remoteLabel in (from lab in _vcRemote.CountryLabel where lab.Country.ToLower() == countryShortName select lab))
                {
                    VarConfig.VariableRow localParentVariable = _vcFacLocal.GetVariableByID(remoteLabel.VariableID);
                    if (localParentVariable != null) _vcFacLocal.CopyCountryLabelRow(remoteLabel, localParentVariable);
                }
            }
        }

        void RemoveLabelsWholeCountry(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcCountryLabels.GetNodeInfoLocal() : _mcCountryLabels.GetNodeInfoRemote())
            {
                if (!nodeInfo.ID.StartsWith(MergeAdministrator.IDENTIFIER_ADD_DEL_COUNTRY_LABELS) ||
                    nodeInfo.changeType != (local ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue;

                string countryShortName = nodeInfo.ID.Substring(MergeAdministrator.IDENTIFIER_ADD_DEL_COUNTRY_LABELS.Length);
                _vcFacLocal.DeleteCountrySpecificDescriptions(countryShortName);
            }
        }
    }
}
