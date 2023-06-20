using EM_Common;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EM_UI.VersionControl.Merging
{
    internal class ApplyAdministrator
    {
        internal class Sequencer
        {
            CountryConfigFacade _ccFacLocal = null, _ccFacRemote = null;
            CountryConfig _ccLocal = null, _ccRemote = null;
            Dictionary<string, SortedList<int, string>> components = new Dictionary<string,SortedList<int,string>>();

            internal Sequencer(CountryConfigFacade ccFacLocal, CountryConfigFacade ccFacRemote) { _ccFacLocal = ccFacLocal; _ccFacRemote = ccFacRemote; _ccLocal = _ccFacLocal.GetCountryConfig(); _ccRemote = _ccFacRemote.GetCountryConfig(); }

            internal void TakeComponent(string parentID, List<DataRow> siblings)
            {
                if (components.ContainsKey(parentID))
                    return; //already listed for having to be reordered
                SortedList<int, string> infoOriginalOrder = new SortedList<int, string>();
                foreach (DataRow sibling in siblings)
                    infoOriginalOrder.Add(CountryConfigFacade.GetOrderAsInt(sibling), CountryConfigFacade.GetRowID(sibling));
                components.Add(parentID, infoOriginalOrder);
            }

            internal void PerformSequencing()
            {
                foreach (string parentID in components.Keys)
                    SequenceComponent(parentID, components[parentID]);
                components.Clear(); //after performing the sequencing delete the (anyway useless) info, thus the Sequencer can be used again
            }

            void SequenceComponent(string parentID, SortedList<int, string> infoOriginalOrder)
            {
                //first gather the components that were temporarily equiped with oder 10.00x and thus need to be put to their correct place
                List<string> toSequenceIDs = new List<string>();
                DataRow parentRow = CountryConfigFacade.GetRowByID(_ccLocal, parentID);
                foreach (DataRow child in CountryConfigFacade.GetChildRows(parentRow))
                {
                    int order = CountryConfigFacade.GetOrderAsInt(child);
                    if (order >= FIRST_TEMPORDER)
                        toSequenceIDs.Add(CountryConfigFacade.GetRowID(child));
                }

                //then run over the original order (where these components were taken from: if added remote order, if changed to remote order: local order)
                //the purpose of doing this in the original order is to avoid searching for a predecessor which is not yet sorted in (but has to be within this process)
                List<string> preIDs = new List<string>();
                foreach (string childID in infoOriginalOrder.Values)
                {
                    preIDs.Insert(0, childID); //gather the predecessors - in original order - of a component that needs putting in place, which are used below to asses where the component belongs to
                    if (!toSequenceIDs.Contains(childID))
                         continue;

                    //component that needs putting in place found
                    DataRow toSequenceRow = CountryConfigFacade.GetRowByID(_ccLocal, childID);
                    
                    //try to find a predecessor, which also exists in the new "family" of the component
                    DataRow preRow = null;
                    foreach (string preID in preIDs)
                    {
                        if (preID == childID) continue; //don't take toSequenceRow itself as predecessor
                        preRow = CountryConfigFacade.GetRowByID(_ccLocal, preID); if (preRow != null) break;
                    }

                    //gather information about the component over all systems, because order must be changed in parallel
                    List<DataRow> twinRows = new List<DataRow>();
                    List<DataRow> twinParentRows = new List<DataRow>();
                    List<DataRow> twinPreRows = new List<DataRow>();
                    foreach (CountryConfig.SystemRow system in _ccLocal.System)
                    {
                        DataRow twinRow = CountryConfigFacade.GetTwinRow(toSequenceRow, system.ID);
                        twinRows.Add(twinRow);
                        twinParentRows.Add(CountryConfigFacade.GetParentRow(twinRow));
                        if (preRow != null) twinPreRows.Add(CountryConfigFacade.GetTwinRow(preRow, system.ID));
                    }
                    
                    //sort the component in where it belongs to - do this for all systems
                    for (int index = 0; index < twinRows.Count; ++index)
                    {
                        DataRow twinRow = twinRows[index];
                        string order = (preRow != null) ? CountryConfigFacade.ShiftRowsUp(twinPreRows[index], false)
                                                    : CountryConfigFacade.ShiftRowsUp(CountryConfigFacade.GetFirstRow(twinParentRows[index]), true);
                        CountryConfigFacade.SetOrder(twinRow, order);
                    }

                    //get rid of the 1000x-numbers by numbering the components 0-1-2-3-etc.
                    foreach (DataRow twinParentRow in twinParentRows)
                        CountryConfigFacade.StraightenChildOrder(twinParentRow);
                }
            }
        }

        MergeAdministrator _mergeAdministrator = null;

        CountryConfigFacade _ccFacLocal = null, _ccFacRemote = null;
        CountryConfig _ccLocal = null, _ccRemote = null;
        DataConfigFacade _dcFacLocal = null, _dcFacRemote = null;
        DataConfig _dcLocal = null, _dcRemote = null;
        bool _isAddOn = false;
        SwitchablePolicyConfigFacade _vcFacSwitchablePolicyLocal = null, _vcFacSwitchablePolicyRemote = null;

        MergeForm _mergeForm = null;
        MergeControl _mcSystems = null;
        MergeControl _mcSpine = null;
        MergeControl _mcData = null;
        MergeControl _mcCondFormat = null;
        MergeControl _mcUpratingFactors = null;
        MergeControl _mcIndirectTaxes = null;
        MergeControl _mcExternalStatistics = null;
        MergeControl _mcExtensions = null;
        MergeControl _mcExtSwitches = null;
        MergeControl _mcLookGroups = null;

        Sequencer _spineSequencer = null;
        Sequencer _policySequencer = null;
        Sequencer _functionSequencer = null;

        const short POLICY_CHANGE = 0;
        const short FUNCTION_CHANGE = 1;
        const short PARAMETER_CHANGE = 2;
        const int FIRST_TEMPORDER = 10000;
        int _tempOrder = FIRST_TEMPORDER;

        const short DATA_CHANGE = 0;
        const short DBSYS_CHANGE = 1;

        const short EXTENSION_CHANGE = 0;
        const short EXTENSION_CONTENT_CHANGE = 1;
        const short LOOKGROUPS_CHANGE = 0;
        const short LOOKGROUPS_CONTENT_CHANGE = 1;

        internal ApplyAdministrator(MergeAdministrator mergeAdministrator, MergeForm mergeForm,
            CountryConfigFacade ccFacLocal, DataConfigFacade dcFacLocal,
            CountryConfigFacade ccFacRemote, DataConfigFacade dcFacRemote, SwitchablePolicyConfigFacade vcFacSwitchablePolicyLocal,
            SwitchablePolicyConfigFacade vcFacSwitchablePolicyRemote)
        {
            _mergeAdministrator = mergeAdministrator;
            
            _ccFacLocal = ccFacLocal; _ccLocal = _ccFacLocal.GetCountryConfig();
            _dcFacLocal = dcFacLocal; _isAddOn = _dcFacLocal == null; _dcLocal = _isAddOn ? null : _dcFacLocal.GetDataConfig();
            _ccFacRemote = ccFacRemote; _ccRemote = _ccFacRemote.GetCountryConfig();
            _dcFacRemote = dcFacRemote; _dcRemote = _isAddOn ? null : _dcFacRemote.GetDataConfig();
            _vcFacSwitchablePolicyLocal = vcFacSwitchablePolicyLocal; _vcFacSwitchablePolicyRemote = vcFacSwitchablePolicyRemote;

            _spineSequencer = new Sequencer(ccFacLocal, ccFacRemote);
            _policySequencer = new Sequencer(ccFacLocal, ccFacRemote);
            _functionSequencer = new Sequencer(ccFacLocal, ccFacRemote);

            _mergeForm = mergeForm;
            _mcSystems = _mergeForm.GetMergeControlByName(MergeForm.SYSTEMS);
            _mcSpine = _mergeForm.GetMergeControlByName(MergeForm.SPINE);
            _mcData = _mergeForm.GetMergeControlByName(MergeForm.DATASETS);
            _mcCondFormat = _mergeForm.GetMergeControlByName(MergeForm.CONDITIONAL_FORMATTING);
            _mcUpratingFactors = _mergeForm.GetMergeControlByName(MergeForm.UPRATING_INDICES);
            _mcIndirectTaxes = _mergeForm.GetMergeControlByName(MergeForm.INDIRECT_TAXES);
            _mcExternalStatistics = _mergeForm.GetMergeControlByName(MergeForm.EXTERNAL_STATISTICS);
            _mcExtensions = _mergeForm.GetMergeControlByName(MergeForm.EXTENSIONS);
            _mcExtSwitches = _mergeForm.GetMergeControlByName(MergeForm.EXT_SWITCHES);
            _mcLookGroups = _mergeForm.GetMergeControlByName(MergeForm.LOOK_GROUPS);
        }

        internal void Apply()
        {

            //apply locally rejected or remotely accepted pure value changes, i.e. parameter-values and policy/function-switches (elements available in both, local and remote)
            ChangeValues(true); ChangeValues(false);

            //remove locally added and rejected or remotely removed and accepted policies, functions and parameters
            RemoveComponents(true); RemoveComponents(false);

            //remove locally added and rejected or remotely removed and accepted systems
            RemoveSystems(true); RemoveSystems(false);

            //add locally removed and rejected or remotely added and accepted systems
            AddSystems(true); AddSystems(false);

            //apply locally rejected or remotely accepted changes in system settings (currency, exchange-rate, etc.)
            ChangeSystemSettings(true); ChangeSystemSettings(false);

            //add locally removed and rejected or remotely added and accepted policies, functions and parameters
            AddComponents(true); AddComponents(false);

            _spineSequencer.PerformSequencing();    //note that it is necessary to do the re-ordering here and below again
            _policySequencer.PerformSequencing();   //because here it is based on the remote original-order and below on the local original-order
            _functionSequencer.PerformSequencing(); //for the Sequencer it is necessary to work on one original-order
                                                    //also note that the Sequencer deletes its info (given via TakeComponent) after having performed (i.e. after PerformSequencing)

            //apply locally rejected or remotely accepted changes concerning all systems (name, comment, etc.)
            ChangeSettings(true); ChangeSettings(false);


            //adapt sequence if locally rejected and reorder all components which need reordering
            AdaptSequence();
            _spineSequencer.PerformSequencing();
            _policySequencer.PerformSequencing();
            _functionSequencer.PerformSequencing();


            if (!_isAddOn)
            {

                //remove locally added and rejected or remotely removed and accepted datasets, db-sys-combinations and extension-values
                RemoveDataComponents(true); RemoveDataComponents(false);

                //add locally removed and rejected or remotely added and accepted datasets, db-sys-combinations and extension-values
                AddDataComponents(true); AddDataComponents(false);

                //apply locally rejected or remotely accepted changes concerning datasets, db-sys-combinations and extension-values
                ChangeDataSettings(true); ChangeDataSettings(false);

                //remove/add/change locally added and rejected or remotely removed and accepted local extensions and extension-contents
                RemoveExtensionInfo(true); RemoveExtensionInfo(false);
                AddExtensionInfo(true); AddExtensionInfo(false);
                ChangeExtensionInfo(true); ChangeExtensionInfo(false);

                //remove/add/change locally rejected or remotely accepted changes of extension switches
                RemoveExtensionSwitches(true); RemoveExtensionSwitches(false);
                AddExtensionSwitches(true); AddExtensionSwitches(false);
                ChangeExtensionSwitches(true); ChangeExtensionSwitches(false);
                _ccLocal.AcceptChanges(); _dcLocal.AcceptChanges();

                //remove/add/change locally added and rejected or remotely removed and accepted local groups
                RemoveLookGroupsInfo(true); RemoveLookGroupsInfo(false); 
                AddLookGroupsInfo(true); AddLookGroupsInfo(false);
                ChangeLookGroupsInfo(true); ChangeLookGroupsInfo(false);
            }

            //remove locally added and rejected or remotely removed and accepted conditional formatting
            RemoveCondFormat(true); RemoveCondFormat(false);

            //add locally removed and rejected or remotely added and accepted conditional formatting
            AddCondFormat(true); AddCondFormat(false);

            //apply locally rejected or remotely accepted changes concerning conditional formatting
            ChangeCondFormat(true); ChangeCondFormat(false);

            //remove locally added and rejected or remotely removed and accepted uprating indices
            RemoveUpratingIndices();

            //add locally removed and rejected or remotely added and accepted uprating indices
            AddUpratingIndices();

            //apply locally rejected or remotely accepted changes concerning uprating indices
            //ChangeUpratingIndices(true); ChangeUpratingIndices(false);
            ChangeUpratingIndices();

            //remove locally added and rejected or remotely removed and accepted indirect taxes
            RemoveIndirectTaxes();

            //add locally removed and rejected or remotely added and accepted indirect taxes
            AddIndirectTaxes();

            //apply locally rejected or remotely accepted changes concerning indirect taxes
            //ChangeIndirectTaxes(true); ChangeIndirectTaxes(false);
            ChangeIndirectTaxes();

            //remove locally added and rejected or remotely removed and accepted external statistics
            RemoveExternalStatistics();

            //add locally removed and rejected or remotely added and accepted external statistics
            AddExternalStatistics();

            //apply locally rejected or remotely accepted changes concerning external statistics
            //ChangeExternalStatistics(true); ChangeExternalStatistics(false);
            ChangeExternalStatistics();

            //apply locally rejected or remotely accepted changes in country-long-name and private-setting
            ChangeCountrySettings();
        }

        void ChangeValues(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSpine.GetNodeInfoLocal() : _mcSpine.GetNodeInfoRemote())
            {

                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange) || cellInfo.ID == string.Empty)
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected, or not a value-change (but e.g. a name- or comment-change)

                    MergeControl.CellInfo cellInfoRemote = local ? _mcSpine.GetTwinCellInfo(cellInfo) : cellInfo;
                    switch (nodeInfo.node.Level) //replace local value/switch by remote value/switch
                    {
                        case POLICY_CHANGE:
                            CountryConfig.PolicyRow policyRow = _ccFacLocal.GetPolicyRowByID(cellInfo.ID);
                            policyRow.Switch = cellInfoRemote.text;
                            break;
                        case FUNCTION_CHANGE:
                            CountryConfig.FunctionRow functionRow = _ccFacLocal.GetFunctionRowByID(cellInfo.ID);
                            functionRow.Switch = cellInfoRemote.text;
                            break;
                        case PARAMETER_CHANGE:
                            CountryConfig.ParameterRow parameterRow = _ccFacLocal.GetParameterRowByID(cellInfo.ID);
                            parameterRow.Value = cellInfoRemote.text;
                            break;
                    }
                }
            }
        }

        void RemoveComponents(bool local)
        {
            List<CountryConfig.PolicyRow> defaultPolicies = new List<DataSets.CountryConfig.PolicyRow>();
            List<CountryConfig.FunctionRow> defaultFunctions = new List<DataSets.CountryConfig.FunctionRow>();
            List<CountryConfig.ParameterRow> defaultParameters = new List<DataSets.CountryConfig.ParameterRow>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSpine.GetNodeInfoLocal() : _mcSpine.GetNodeInfoRemote())
            {

                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-add nor an accepted remote-remove

                //gather all compenents which need to be deleted, at first in the default-system
                switch (nodeInfo.node.Level)
                {
                    case POLICY_CHANGE: defaultPolicies.Add(_ccFacLocal.GetPolicyRowByID(nodeInfo.ID)); break;
                    case FUNCTION_CHANGE: defaultFunctions.Add(_ccFacLocal.GetFunctionRowByID(nodeInfo.ID)); break;
                    case PARAMETER_CHANGE: defaultParameters.Add(_ccFacLocal.GetParameterRowByID(nodeInfo.ID)); break;
                }
            }

            //then gather all compenents which need to be deleted, but now in all systems
            List<CountryConfig.PolicyRow> allSysPolicies = new List<DataSets.CountryConfig.PolicyRow>();
            List<CountryConfig.FunctionRow> allSysFunctions = new List<DataSets.CountryConfig.FunctionRow>();
            List<CountryConfig.ParameterRow> allSysParameters = new List<DataSets.CountryConfig.ParameterRow>();
            foreach (CountryConfig.SystemRow system in _ccFacLocal.GetSystemRows())
            {
                foreach (CountryConfig.PolicyRow defaultPol in defaultPolicies)
                    allSysPolicies.Add(_ccFacLocal.GetPolicyRowByOrder(defaultPol.Order, system.ID));
                foreach (CountryConfig.FunctionRow defaultFunc in defaultFunctions)
                    allSysFunctions.Add(_ccFacLocal.GetFunctionRowByOrder(defaultFunc.PolicyRow.Order, defaultFunc.Order, system.ID));
                foreach (CountryConfig.ParameterRow defaultPar in defaultParameters)
                    allSysParameters.Add(_ccFacLocal.GetParameterRowByOrder(defaultPar.FunctionRow.PolicyRow.Order, defaultPar.FunctionRow.Order, defaultPar.Order, system.ID));
            }
                
            //first remove all parameters (start with most nested element, to avoid deleting parameters of already deleted functions)
            foreach (CountryConfig.ParameterRow remPar in allSysParameters) remPar.Delete();
            foreach (CountryConfig.FunctionRow remFunc in allSysFunctions) remFunc.Delete(); //then remove functions
            foreach (CountryConfig.PolicyRow remPol in allSysPolicies) remPol.Delete(); //then remove policies

            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void RemoveSystems(bool local)
        {
            List<CountryConfig.SystemRow> remSys = new List<CountryConfig.SystemRow>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSystems.GetNodeInfoLocal() : _mcSystems.GetNodeInfoRemote())
            {

                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-add nor an accepted remote-remove
                remSys.Add(_ccFacLocal.GetSystemRowByID(nodeInfo.ID));
            }

            foreach (CountryConfig.SystemRow system in remSys)
                system.Delete();
            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void AddSystems(bool local)
        {
            List<CountryConfig.SystemRow> systemsToAdd = new List<CountryConfig.SystemRow>();
            string idMatchSystem = string.Empty;
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSystems.GetNodeInfoLocal() : _mcSystems.GetNodeInfoRemote())
            {

                MergeControl.NodeInfo nodeInfoTwin = _mcSystems.GetNodeInfoByID(nodeInfo.ID, !local);
                if (idMatchSystem == string.Empty &&
                    (nodeInfo.changeType == MergeControl.ChangeType.none || nodeInfo.changeType == MergeControl.ChangeType.changed) &&
                    (nodeInfoTwin.changeType == MergeControl.ChangeType.none || nodeInfoTwin.changeType == MergeControl.ChangeType.changed))
                    idMatchSystem = nodeInfo.ID; //need a system that exists in both, local and remote, to match the components (policies/functions/parameters)

                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-remove nor an accepted remote-add
                systemsToAdd.Add(_ccFacRemote.GetSystemRowByID(nodeInfo.ID));
            }

            CountryConfig.SystemRow matchSystemRemote = _ccFacRemote.GetSystemRowByID(idMatchSystem);
            CountryConfig.SystemRow matchSystemLocal = _ccFacLocal.GetSystemRowByID(idMatchSystem);

            foreach (CountryConfig.SystemRow systemToAdd in systemsToAdd)
            {
                //copy the system 1:1 (same id, ...) from remote to local, initially without content (policies/functions/parameters) ...
                CountryConfig.SystemRow addedSystem = CountryConfigFacade.TransferSystemIdentically(systemToAdd, _ccLocal);

                //... to copy the content bit by bit and therewith adapt it to the local spine
                foreach (CountryConfig.PolicyRow matchPolicyLocal in matchSystemLocal.GetPolicyRows())
                {
                    CountryConfig.PolicyRow policyToAdd = null;
                    bool notAvailableRemote = false;

                    //(a) link the two match-systems: search for remote policy with same id as local policy (LOC/SYS1/POL_cc --find--> REM/SYS1/POL_cc)
                    CountryConfig.PolicyRow matchPolicyRemote = _ccFacRemote.GetPolicyRowByID(matchPolicyLocal.ID);
                    if (matchPolicyRemote == null) //is ok to happen, if this policy never existed remote, i.e. is new in local and may be added with AddComponents later on
                    {                              //may also happen for inconsisteny reasons, I guess (system has match, but not all components have)
                        policyToAdd = matchPolicyLocal;
                        notAvailableRemote = true;
                    }
                    else
                    {
                        //(b) link the remote match-policy with its twin in the system to add (REM/SYS1/POL_cc --find--> REM/SYSadd/POL_cc)
                        policyToAdd = _ccFacRemote.GetPolicyRowByOrder(matchPolicyRemote.Order, systemToAdd.ID);
                        if (policyToAdd == null) //should not happen actually
                        {
                            policyToAdd = matchPolicyLocal;
                            notAvailableRemote = true;
                        }
                    }

                    //(c) copy the found policy to the added local system (REM/SYSadd/POL_cc --copy--> LOC/SYSadd/POL_cc)
                    //    if the policy does not exist in remote (see if == null in (a) and (b)) just copy the local match-policy and set it to n/a
                    CountryConfig.PolicyRow addedPolicy = CountryConfigFacade.CopyPolicyRowToNewSystem(policyToAdd, addedSystem,
                                                            !notAvailableRemote, //keepIDs: usually id is kept (policy has the same id as remote), exception: no remote policy was found
                                                            false);              //copyContent: do not automatically copy functions and parameters - they are copied individually in what follows
                    addedPolicy.Order = matchPolicyLocal.Order; //we need the local order and not the remote order
                    addedPolicy.Name = matchPolicyLocal.Name; //this is because some functions (namely CountryConfigFacade.GetPolicyRowByNameOrderAndSystemID) refer to the policies name (actually all settings (i.e. not system specific stuff, like comments) should be equal for all systems, but the others are not as critical)
                    if (notAvailableRemote) addedPolicy.Switch = DefPar.Value.NA;

                    // if the copied policy is part of an extension, the new policy also must be made part of the extension
                    List<CountryConfig.Extension_PolicyRow> epToCopy = new List<CountryConfig.Extension_PolicyRow>();
                    foreach (var ep in from e in _ccLocal.Extension_Policy where e.PolicyID == matchPolicyLocal.ID select e) epToCopy.Add(ep);
                    foreach (var ep in epToCopy) _ccLocal.Extension_Policy.AddExtension_PolicyRow(ep.ExtensionID, addedPolicy, ep.BaseOff);

                    foreach (CountryConfig.FunctionRow matchFunctionLocal in matchPolicyLocal.GetFunctionRows())
                    {
                        CountryConfig.FunctionRow functionToAdd = null;
                        notAvailableRemote = false;

                        //(a) as above
                        CountryConfig.FunctionRow matchFunctionRemote = _ccFacRemote.GetFunctionRowByID(matchFunctionLocal.ID);
                        if (matchFunctionRemote == null) //is ok to happen, ... see above
                        {
                            functionToAdd = matchFunctionLocal;
                            notAvailableRemote = true;
                        }
                        else
                        {
                            //(b) as above
                            functionToAdd = _ccFacRemote.GetFunctionRowByOrder(matchPolicyRemote.Order, matchFunctionRemote.Order, systemToAdd.ID);
                            if (functionToAdd == null) //should not happen actually
                            {
                                functionToAdd = matchFunctionLocal;
                                notAvailableRemote = true;
                            }
                        }

                        //(c) as above
                        CountryConfig.FunctionRow addedFunction = CountryConfigFacade.CopyFunctionRowAtTailOrUseOriginalOrder(
                                                            functionToAdd, addedPolicy,
                                                            true,               //copyAtTail
                                                            notAvailableRemote, //switchNA
                                                            !notAvailableRemote,//keepIDs
                                                            false);             //copyContent
                        addedFunction.Order = matchFunctionLocal.Order;

                        // if the copied function is part of an extension, the new function also must be made part of the extension
                        List<CountryConfig.Extension_FunctionRow> efToCopy = new List<CountryConfig.Extension_FunctionRow>();
                        foreach (var ef in from e in _ccLocal.Extension_Function where e.FunctionID == matchFunctionLocal.ID select e) efToCopy.Add(ef);
                        foreach (var ef in efToCopy) _ccLocal.Extension_Function.AddExtension_FunctionRow(ef.ExtensionID, addedFunction, ef.BaseOff);

                        foreach (CountryConfig.ParameterRow matchParameterLocal in matchFunctionLocal.GetParameterRows())
                        {
                            CountryConfig.ParameterRow parameterToAdd = null;
                            notAvailableRemote = false;

                            //(a) as above
                            CountryConfig.ParameterRow matchParameterRemote = _ccFacRemote.GetParameterRowByID(matchParameterLocal.ID);
                            if (matchParameterRemote == null) //is ok to happen, ... see above
                            {
                                parameterToAdd = matchParameterLocal;
                                notAvailableRemote = true;
                            }
                            else
                            {
                                //(b) as above
                                parameterToAdd = _ccFacRemote.GetParameterRowByOrder(functionToAdd.ID, matchParameterRemote.Order);
                                if (parameterToAdd == null) //should not happen actually
                                {
                                    parameterToAdd = matchParameterLocal;
                                    notAvailableRemote = true;
                                }
                            }

                            //(c) as above
                            CountryConfig.ParameterRow addedParameter = CountryConfigFacade.CopyParameterRow(parameterToAdd, addedFunction,
                                                                notAvailableRemote,  //setToNA
                                                                !notAvailableRemote, //keepIDs
                                                                null,                //neighbourParameterRow
                                                                true,                //addBeforeNeighbour
                                                                false);              //copyAtTail
                            addedParameter.Order = matchParameterLocal.Order;

                            // if the copied parameter is part of an extension, the new parameter also must be made part of the extension
                            List<CountryConfig.Extension_ParameterRow> eparToCopy = new List<CountryConfig.Extension_ParameterRow>();
                            foreach (var ep in from e in _ccLocal.Extension_Parameter where e.ParameterID == matchParameterLocal.ID select e) eparToCopy.Add(ep);
                            foreach (var ep in eparToCopy) _ccLocal.Extension_Parameter.AddExtension_ParameterRow(ep.ExtensionID, addedParameter, ep.BaseOff);
                        }
                    }
                }
            }
        }

        void AddComponents(bool local)
        {
            //gather the policies, functions and parameters which need to be added
            List<CountryConfig.PolicyRow> policiesToAdd = new List<CountryConfig.PolicyRow>();
            List<CountryConfig.FunctionRow> functionsToAdd = new List<CountryConfig.FunctionRow>();
            List<CountryConfig.ParameterRow> parametersToAdd = new List<CountryConfig.ParameterRow>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSpine.GetNodeInfoLocal() : _mcSpine.GetNodeInfoRemote())
            {

                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-remove nor an accepted remote-add

                switch (nodeInfo.node.Level)
                {
                    case POLICY_CHANGE: policiesToAdd.Add(_ccFacRemote.GetPolicyRowByID(nodeInfo.ID)); break;
                    case FUNCTION_CHANGE: functionsToAdd.Add(_ccFacRemote.GetFunctionRowByID(nodeInfo.ID)); break;
                    case PARAMETER_CHANGE: parametersToAdd.Add(_ccFacRemote.GetParameterRowByID(nodeInfo.ID)); break;
                }
            }

            //note: the restoring of locally deleted components is solely based on getting it from remote, i.e. parent is not involved
            //one other possibility would be to restore from parent: not doing this can be justified by possible changes in remote meanwhile
            //the other (optimal) possibility would be to restore preferably from remote and, if not possible, from parent: not doing this is only justified by risking a bend in the programmer's brain

            //LOOP OVER ALL SYSTEMS to add the policies, functions and parameters
            Dictionary<string, CountryConfig.PolicyRow> policiesWithNewGuids = new Dictionary<string, CountryConfig.PolicyRow>();
            Dictionary<string, CountryConfig.FunctionRow> functionsWithNewGuids = new Dictionary<string, CountryConfig.FunctionRow>();
            int tempOrder = -1;
            foreach (CountryConfig.SystemRow systemLocal in _ccLocal.System) //copy the components 
            {
                tempOrder = _tempOrder;

                //LOOP OVER ALL NEW POLICIES
                foreach (CountryConfig.PolicyRow polInDefaultSys in policiesToAdd)
                {
                    bool notAvailableRemote = false;
                    CountryConfig.PolicyRow policyToAdd = _ccFacRemote.GetPolicyRowByOrder(polInDefaultSys.Order, systemLocal.ID); //note: it's actually the remote system, but system.ID is equal in remote and local
                    if (policyToAdd == null) //is ok to happen if the system does not exist remotely
                    {
                        policyToAdd = polInDefaultSys;
                        notAvailableRemote = true;
                    }

                    CountryConfig.PolicyRow addedPolicy = CountryConfigFacade.CopyPolicyRowToNewSystem(policyToAdd, systemLocal,
                        !notAvailableRemote, //keepIDs: usually id is kept (policy has the same id as remote), exception: no remote policy was found (e.g. because system does not exist)
                        false);              //copyContent: do not automatically copy functions and parameters, because (part of) the nested functions/parameters could be rejected
                    addedPolicy.Order = (tempOrder++).ToString(); //use a temporary order (which needs to just to be equal for all systems), to be corrected with other oder-adaptions
                    if (notAvailableRemote)
                    {
                        addedPolicy.Switch = DefPar.Value.NA;
                        policiesWithNewGuids.Add( //need to keep record of policies not available remotely (for this system) as it's likely that
                            systemLocal.ID + polInDefaultSys.ID, addedPolicy); //they are needed to add their child-functions (see below -x-)
                    }

                    if (systemLocal.ID == polInDefaultSys.SystemID) //arrange for reordering the spine, to put the new policies to their correct place
                        _spineSequencer.TakeComponent(systemLocal.ID, polInDefaultSys.SystemRow.GetPolicyRows().ToList<DataRow>());
                }

                //LOOP OVER ALL NEW FUNCTIONS
                foreach (CountryConfig.FunctionRow funcInDefaultSys in functionsToAdd)
                {

                    bool notAvailableRemote = false;
                    CountryConfig.FunctionRow functionToAdd = _ccFacRemote.GetFunctionRowByOrder(funcInDefaultSys.PolicyRow.Order, funcInDefaultSys.Order,
                                                                            systemLocal.ID); //note: it's actually the remote system, but system.ID is equal in remote and local);
                    CountryConfig.PolicyRow parentPolicy = null;
                    if (functionToAdd != null)
                    {
                        parentPolicy = _ccFacLocal.GetPolicyRowByID(functionToAdd.PolicyID);
                        if (parentPolicy == null) //the user (nonsensically) accepts adding the function, but rejects adding the parent-policy
                            continue; //ignore in this case
                    }
                    else //is ok to happen if the system does not exist remotely
                    {
                        functionToAdd = funcInDefaultSys;
                        notAvailableRemote = true;

                        //3 possibilities: the new function ... (!remember!: the (locally) new function does exist remotely, but not for this system, as the system does not exist remotely)
                        //... is part of an existing policy (e.g. a new function Uprate is added to existing policy Uprate_cc)
                        CountryConfig.PolicyRow parentInDefaultSys = _ccFacLocal.GetPolicyRowByID(funcInDefaultSys.PolicyID);
                        if (parentInDefaultSys != null && EM_Helpers.SaveConvertToInt(parentInDefaultSys.Order) < FIRST_TEMPORDER)                      
                            parentPolicy = _ccFacLocal.GetPolicyRowByOrder(parentInDefaultSys.Order, systemLocal.ID);
                        else
                        {   //... is part of a new policy (e.g. the function Elig is part of the new policy BrandNew_cc)
                            if (policiesWithNewGuids.ContainsKey(systemLocal.ID + funcInDefaultSys.PolicyRow.ID))
                                parentPolicy = policiesWithNewGuids[systemLocal.ID + funcInDefaultSys.PolicyRow.ID]; //(see above -x-)
                            //... is part of a new policy, and the user (nonsensically) accepts adding the function, but rejects adding the parent-policy
                            else continue; //ignore in this case 
                        }
                    }

                    CountryConfig.FunctionRow addedFunction = CountryConfigFacade.CopyFunctionRowAtTailOrUseOriginalOrder(
                                                            functionToAdd, parentPolicy,
                                                            true,               //copyAtTail
                                                            notAvailableRemote, //switchNA
                                                            !notAvailableRemote,//keepIDs
                                                            false);             //copyContent
                    addedFunction.Order = (tempOrder++).ToString(); //temporary order: see above

                    if (notAvailableRemote)
                    {
                        addedFunction.Switch = DefPar.Value.NA;
                        functionsWithNewGuids.Add( //need to keep record ... see above
                            systemLocal.ID + funcInDefaultSys.ID, addedFunction);
                    }

                    if (systemLocal.ID == funcInDefaultSys.PolicyRow.SystemID) //arrange for reordering the parent-policy, to put the new functions to their correct place
                        _policySequencer.TakeComponent(parentPolicy.ID, funcInDefaultSys.PolicyRow.GetFunctionRows().ToList<DataRow>());
                }

                //LOOP OVER ALL NEW PARAMETERS
                foreach (CountryConfig.ParameterRow parInDefaultSys in parametersToAdd)
                {

                    bool notAvailableRemote = false;
                    CountryConfig.FunctionRow parentFunction = null;

                    CountryConfig.ParameterRow parameterToAdd = _ccFacRemote.GetParameterRowByOrder(parInDefaultSys.FunctionRow.PolicyRow.Order,
                        parInDefaultSys.FunctionRow.Order, parInDefaultSys.Order, systemLocal.ID);  //note: it's actually the remote system, but system.ID is equal in remote and local

                    if (parameterToAdd != null)
                    {
                        parentFunction = _ccFacLocal.GetFunctionRowByID(parameterToAdd.FunctionID);
                        if (parentFunction == null) //the user (nonsensically) accepts adding the parameter, but rejects adding the parent-policy or -function
                            continue; //ignore in this case
                    }
                    else //is ok to happen if the system does not exist remotely
                    {
                        parameterToAdd = parInDefaultSys;
                        notAvailableRemote = true;

                        //3 possibilities: see above ...
                        //parentFunction = _ccFacLocal.GetFunctionRowByOrder(parInDefaultSys.FunctionRow.PolicyRow.Order, parInDefaultSys.FunctionRow.Order, systemLocal.ID);
                        CountryConfig.FunctionRow parentInDefaultSys = _ccFacLocal.GetFunctionRowByID(parInDefaultSys.FunctionID);
                        if (parentInDefaultSys != null && EM_Helpers.SaveConvertToInt(parentInDefaultSys.Order) < FIRST_TEMPORDER)
                            parentFunction = _ccFacLocal.GetFunctionRowByOrder(parentInDefaultSys.PolicyRow.Order, parentInDefaultSys.Order, systemLocal.ID);
                        else
                        {
                            if (functionsWithNewGuids.ContainsKey(systemLocal.ID + parInDefaultSys.FunctionRow.ID))
                                parentFunction = functionsWithNewGuids[systemLocal.ID + parInDefaultSys.FunctionRow.ID];
                            else continue;
                        }
                    }

                    CountryConfig.ParameterRow addedParameter = CountryConfigFacade.CopyParameterRow(parameterToAdd, parentFunction,
                                                                notAvailableRemote,  //setToNA
                                                                !notAvailableRemote, //keepIDs
                                                                null,                //neighbourParameterRow
                                                                true,                //addBeforeNeighbour
                                                                false);              //copyAtTail
                    addedParameter.Order = (tempOrder++).ToString(); //temporary order: see above

                    if (systemLocal.ID == parInDefaultSys.FunctionRow.PolicyRow.SystemID) //arrange for reordering the parent-policy, to put the new functions to their correct place
                        _functionSequencer.TakeComponent(parentFunction.ID, parInDefaultSys.FunctionRow.GetParameterRows().ToList<DataRow>());
                }
            }
            _tempOrder = tempOrder; //this function is called for local and remote, therefore do not start at 10.000 again for the second call, but continue with a not yet used number
        }

        void ChangeSystemSettings(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSystems.GetNodeInfoLocal() : _mcSystems.GetNodeInfoRemote())
            {

                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                CountryConfig.SystemRow system = _ccFacLocal.GetSystemRowByID(nodeInfo.ID);

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange))
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected
                    
                    MergeControl.CellInfo cellInfoRemote = local ? _mcSystems.GetTwinCellInfo(cellInfo) : cellInfo;
                    system[cellInfo.columnID] = cellInfoRemote.text;
                }
            }
        }

        void ChangeSettings(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcSpine.GetNodeInfoLocal() : _mcSpine.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange) || cellInfo.ID != string.Empty)
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected, or not a setting-change (but a value-change)

                    MergeControl.CellInfo cellInfoRemote = local ? _mcSpine.GetTwinCellInfo(cellInfo) : cellInfo;

                    CountryConfig.PolicyRow polInDefaultSys = null;
                    CountryConfig.FunctionRow funcInDefaultSys = null;
                    CountryConfig.ParameterRow parInDefaultSys = null;

                    switch (nodeInfo.node.Level)
                    {
                        case POLICY_CHANGE: polInDefaultSys = _ccFacLocal.GetPolicyRowByID(nodeInfo.ID); break;
                        case FUNCTION_CHANGE: funcInDefaultSys = _ccFacLocal.GetFunctionRowByID(nodeInfo.ID); break;
                        case PARAMETER_CHANGE: parInDefaultSys = _ccFacLocal.GetParameterRowByID(nodeInfo.ID); break;
                    }

                    foreach (CountryConfig.SystemRow system in _ccLocal.System)
                        switch (nodeInfo.node.Level)
                        {
                            case POLICY_CHANGE: _ccFacLocal.GetPolicyRowByOrder(polInDefaultSys.Order, system.ID)[cellInfo.columnID] = cellInfoRemote.text; break;
                            case FUNCTION_CHANGE: _ccFacLocal.GetFunctionRowByOrder(funcInDefaultSys.PolicyRow.Order, funcInDefaultSys.Order, system.ID)[cellInfo.columnID] = cellInfoRemote.text; break;
                            case PARAMETER_CHANGE: _ccFacLocal.GetParameterRowByOrder(parInDefaultSys.FunctionRow.PolicyRow.Order, parInDefaultSys.FunctionRow.Order, parInDefaultSys.Order, system.ID)[cellInfo.columnID] = cellInfoRemote.text; break;
                        }
                }
            }
        }

        void AdaptSequence()
        {
            List<string> parentIDs = new List<string>();
            if (_mcSpine._sequenceInfo != null && _mcSpine._sequenceInfo.isChanged && _mcSpine._sequenceInfo.acceptChange)
            {
                DataRow firstNodeDataRow = CountryConfigFacade.GetRowByID(_ccLocal, _mcSpine.GetNodeInfoLocal().First().ID);
                parentIDs.Add(CountryConfigFacade.GetSystemRow(firstNodeDataRow).ID); //the spine is going to be reordered (the parentID is the id of a (n arbitrary) system that exists as well in local as in remote)
            }
            foreach (MergeControl.NodeInfo nodeInfo in _mcSpine.GetNodeInfoLocal())
                if (nodeInfo.subNodesSequenceInfo != null && nodeInfo.subNodesSequenceInfo.isChanged && nodeInfo.subNodesSequenceInfo.acceptChange)
                    parentIDs.Add(nodeInfo.ID); //a policy or function is going to be reordered

            //fill the Sequencer with info on the local order before the re-ordering, so that it can afterwards place components which do not exist remotely
            foreach (string parentID in parentIDs)
            {
                DataRow parentLocal = CountryConfigFacade.GetRowByID(_ccLocal, parentID);
                Sequencer sequencer = CountryConfigFacade.IsPolicyRow(parentLocal) ? _policySequencer
                            : CountryConfigFacade.IsFunctionRow(parentLocal) ? _functionSequencer : _spineSequencer;
                sequencer.TakeComponent(parentID, CountryConfigFacade.GetChildRows(parentLocal));
            }

            //exchange the local order with the remote order for components which exist in local as well as in remote
            foreach (string parentID in parentIDs)
            {
                DataRow parentLocal = CountryConfigFacade.GetRowByID(_ccLocal, parentID);

                // twins have to be assessed in this (pre-)extra-loop, because they are found by order, which is changing in the main-loop
                Dictionary<DataRow, List<DataRow>> childTwinsLocal = new Dictionary<DataRow, List<DataRow>>();
                foreach (DataRow childLocal in CountryConfigFacade.GetChildRows(parentLocal))
                {
                    List<DataRow> twinsLocal = new List<DataRow>();
                    foreach (CountryConfig.SystemRow system in _ccLocal.System)
                        twinsLocal.Add(CountryConfigFacade.GetTwinRow(childLocal, system.ID));
                    childTwinsLocal.Add(childLocal, twinsLocal);
                }
                foreach (DataRow childLocal in CountryConfigFacade.GetChildRows(parentLocal)) //this loop refers to the default-system (a system existing as well in local as in remote)
                {
                    //gather the twin components in all systems, as the reordering must of course be parallel
                    List<DataRow> twinsLocal = childTwinsLocal[childLocal];

                    //look if there is a component in remote with the same id ...
                    DataRow childRemote = CountryConfigFacade.GetRowByID(_ccRemote, CountryConfigFacade.GetRowID(childLocal));
                    string order = string.Empty;
                    if (childRemote != null) //... if yes, use this remote twin's order
                        order = CountryConfigFacade.GetOrderAsString(childRemote);
                    else //... if no, use a temporary order - the Sequencer will care for re-integration afterwards (based on the original local order)
                        order = (_tempOrder++).ToString();

                    foreach (DataRow twin in twinsLocal) //apply the assessed order on the component in all systems
                        CountryConfigFacade.SetOrder(twin, order);
                }
            }
        }

        void GatherDataComponentsToTreat(bool takeFromLocalData,
            bool localChanges, MergeControl.ChangeType relevantChangeTypeLocal, MergeControl.ChangeType relevantChangeTypeRemote,
            out List<DataConfig.DataBaseRow> data, out List<DataConfig.DBSystemConfigRow> dbSys)
        {
            data = new List<DataConfig.DataBaseRow>();
            dbSys = new List<DataConfig.DBSystemConfigRow>();

            foreach (MergeControl.NodeInfo nodeInfo in localChanges ? _mcData.GetNodeInfoLocal() : _mcData.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (localChanges ? relevantChangeTypeLocal : relevantChangeTypeRemote) ||
                    nodeInfo.changeHandling != (localChanges ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-add nor an accepted remote-remove

                DataConfigFacade dcFac = takeFromLocalData ? _dcFacLocal : _dcFacRemote;

                switch (nodeInfo.node.Level)
                {
                    case DATA_CHANGE: data.Add(dcFac.GetDataBaseRow(nodeInfo.ID)); break;
                    case DBSYS_CHANGE: dbSys.Add(GetDBSysFromNodeInfoID(dcFac, nodeInfo.ID)); break;
                }
            }
        }

        DataConfig.DBSystemConfigRow GetDBSysFromNodeInfoID(DataConfigFacade dcFac, string nodeInfoID)
        {
            List<string> IDs = MergeAdministrator.DisassembleNodeInfoID(nodeInfoID);
            return (IDs.Count() == 2) ? dcFac.GetDBSystemConfigRow(IDs[1], IDs[0]) : null; //must be sysID#dataID
        }

        void RemoveDataComponents(bool local)
        {
            //gather the datasets, db-sys-combinations and policy-switch-values which need to be deleted
            List<DataConfig.DataBaseRow> remData; List<DataConfig.DBSystemConfigRow> remDBSys;
            GatherDataComponentsToTreat(true, local, MergeControl.ChangeType.added, MergeControl.ChangeType.removed, out remData, out remDBSys);

            foreach (DataConfig.DBSystemConfigRow dbSys in remDBSys)
                dbSys.Delete();

            foreach (DataConfig.DataBaseRow data in remData)
                data.Delete();

            _dcLocal.AcceptChanges();
        }

        void AddDataComponents(bool local)
        {
            //gather the datasets, db-sys-combinations and policy-switch-values which need to be added
            List<DataConfig.DataBaseRow> addData; List<DataConfig.DBSystemConfigRow> addDBSys;
            GatherDataComponentsToTreat(false, local, MergeControl.ChangeType.removed, MergeControl.ChangeType.added, out addData, out addDBSys);

            foreach (DataConfig.DataBaseRow data in addData)
                DataConfigFacade.CopyDataBaseRowFromAnotherCountry(_dcLocal, data, true);
            _dcLocal.AcceptChanges();

            foreach (DataConfig.DBSystemConfigRow dbSys in addDBSys)
            {
                DataConfig.DataBaseRow data = _dcFacLocal.GetDataBaseRow(dbSys.DataBaseID);
                CountryConfig.SystemRow system = _ccFacLocal.GetSystemRowByID(dbSys.SystemID);
                if (data != null && system != null) DataConfigFacade.OvertakeDBSystemConfigRowFromAnotherCountry(_dcLocal, dbSys, data, system, false);
            }
            _dcLocal.AcceptChanges();
        }

        void ChangeDataSettings(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcData.GetNodeInfoLocal() : _mcData.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || local ? cellInfo.acceptChange : !cellInfo.acceptChange)
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? _mcData.GetTwinCellInfo(cellInfo) : cellInfo;

                    switch (nodeInfo.node.Level)
                    {
                        case DATA_CHANGE:
                            DataConfig.DataBaseRow data = _dcFacLocal.GetDataBaseRow(nodeInfo.ID);
                            if (data != null) data[cellInfo.columnID] = cellInfoRemote.text;
                            break;

                        case DBSYS_CHANGE: 
                            DataConfig.DBSystemConfigRow dbSys = GetDBSysFromNodeInfoID(_dcFacLocal, nodeInfo.ID);
                            if (dbSys != null) dbSys[cellInfo.columnID] = cellInfoRemote.text;
                            break;
                    }
                }
            }
        }

        void RemoveCondFormat(bool local)
        {
            List<CountryConfig.ConditionalFormatRow> remCondFormat = new List<CountryConfig.ConditionalFormatRow>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcCondFormat.GetNodeInfoLocal() : _mcCondFormat.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-add nor an accepted remote-remove
                remCondFormat.Add(_ccFacLocal.GetConditionalFormatRowByID(nodeInfo.ID));
            }

            foreach (CountryConfig.ConditionalFormatRow condFormat in remCondFormat)
                condFormat.Delete();
            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void AddCondFormat(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcCondFormat.GetNodeInfoLocal() : _mcCondFormat.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (local ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-remove nor an accepted remote-add

                CountryConfigFacade.CopyConditionalFormatRowToAnotherCountry(_ccLocal, _ccFacRemote.GetConditionalFormatRowByID(nodeInfo.ID));
            }               
        }

        void ChangeCondFormat(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcCondFormat.GetNodeInfoLocal() : _mcCondFormat.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                CountryConfig.ConditionalFormatRow condFormat = _ccFacLocal.GetConditionalFormatRowByID(nodeInfo.ID);

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange))
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? _mcCondFormat.GetTwinCellInfo(cellInfo) : cellInfo;
                    condFormat[cellInfo.columnID] = cellInfoRemote.text;
                }
            }
        }

        /**
         * UPRATING INDICES
         */
        void RemoveUpratingIndices()
        {
            List<CountryConfig.UpratingIndexRow> remUI = new List<CountryConfig.UpratingIndexRow>();
            foreach (MergeControl.NodeInfo nodeInfo in _mcUpratingFactors.GetNodeInfoLocal(MergeForm.UPRATING_INDICES))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.removed && nodeInfo.changeHandling == MergeControl.ChangeHandling.accept)
                    remUI.Add((from ui in _ccFacLocal.GetUpratingIndices() where ui.ID == nodeInfo.ID select ui).First());
            }
            foreach (CountryConfig.UpratingIndexRow upratingIndex in remUI)
                upratingIndex.Delete();
            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void AddUpratingIndices()
        {
            foreach (MergeControl.NodeInfo nodeInfo in _mcUpratingFactors.GetNodeInfoLocal(MergeForm.UPRATING_INDICES))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.added && nodeInfo.changeHandling== MergeControl.ChangeHandling.accept)
                {
                    CountryConfig.UpratingIndexRow addUI = (from ui in _ccFacRemote.GetUpratingIndices() where ui.ID == nodeInfo.ID select ui).First();
                    _ccFacLocal.GetCountryConfig().UpratingIndex.AddUpratingIndexRow(addUI.ID, addUI.Description, addUI.Reference, addUI.Comment, addUI.YearValues);

                }

            }
        }

        void ChangeUpratingIndices()
        {
            foreach (MergeControl.NodeInfo nodeInfo in  _mcUpratingFactors.GetNodeInfoLocal(MergeForm.UPRATING_INDICES))
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed) { continue; }
                CountryConfig.UpratingIndexRow upratingIndex = (from ui in _ccFacLocal.GetUpratingIndices() where ui.ID == nodeInfo.ID select ui).First();
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged) { continue; }
                    upratingIndex[cellInfo.columnID] = cellInfo.text;

                }
            }
        }

        /**
         * INDIRECT TAXES
         */
        void RemoveIndirectTaxes()
        {
            List<CountryConfig.IndirectTaxRow> remUI = new List<CountryConfig.IndirectTaxRow>();
            foreach (MergeControl.NodeInfo nodeInfo in _mcIndirectTaxes.GetNodeInfoLocal(MergeForm.INDIRECT_TAXES))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.removed && nodeInfo.changeHandling == MergeControl.ChangeHandling.accept)
                    remUI.Add((from ui in _ccFacLocal.GetIndirectTaxes() where ui.ID == nodeInfo.ID select ui).First());
            }
            foreach (CountryConfig.IndirectTaxRow ittIndex in remUI)
                ittIndex.Delete();
            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void AddIndirectTaxes()
        {
            foreach (MergeControl.NodeInfo nodeInfo in _mcIndirectTaxes.GetNodeInfoLocal(MergeForm.INDIRECT_TAXES))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.added && nodeInfo.changeHandling== MergeControl.ChangeHandling.accept)
                {
                    CountryConfig.IndirectTaxRow addUI = (from ui in _ccFacRemote.GetIndirectTaxes() where ui.ID == nodeInfo.ID select ui).FirstOrDefault();
                    if (addUI != null)
                        _ccFacLocal.GetCountryConfig().IndirectTax.AddIndirectTaxRow(addUI.ID, addUI.Reference, addUI.Comment, addUI.YearValues);
                }
            }
        }

        void ChangeIndirectTaxes()
        {
            foreach (MergeControl.NodeInfo nodeInfo in _mcIndirectTaxes.GetNodeInfoLocal(MergeForm.INDIRECT_TAXES))
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed) { continue; }
                CountryConfig.IndirectTaxRow indirectTax = (from ui in _ccFacLocal.GetIndirectTaxes() where ui.ID == nodeInfo.ID select ui).First();
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged) { continue; }
                    indirectTax[cellInfo.columnID] = cellInfo.text;
                }
            }
        }

        /**
         * EXTERNAL STATISTICS
         */
        void RemoveExternalStatistics()
        {
            List<CountryConfig.ExternalStatisticRow> remUI = new List<CountryConfig.ExternalStatisticRow>();
            foreach (MergeControl.NodeInfo nodeInfo in _mcExternalStatistics.GetNodeInfoLocal(MergeForm.EXTERNAL_STATISTICS))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.removed && nodeInfo.changeHandling == MergeControl.ChangeHandling.accept)
                    remUI.Add((from ui in _ccFacLocal.GetExternalStatistics() where ui.ID == nodeInfo.ID select ui).First());
            }
            foreach (CountryConfig.ExternalStatisticRow ittIndex in remUI)
                ittIndex.Delete();
            _ccFacLocal.GetCountryConfig().AcceptChanges();
        }

        void AddExternalStatistics()
        {
            foreach (MergeControl.NodeInfo nodeInfo in _mcExternalStatistics.GetNodeInfoLocal(MergeForm.EXTERNAL_STATISTICS))
            {
                if (nodeInfo.changeType == MergeControl.ChangeType.added && nodeInfo.changeHandling == MergeControl.ChangeHandling.accept)
                {
                    CountryConfig.ExternalStatisticRow addES = (from es in _ccFacRemote.GetExternalStatistics() where es.ID == nodeInfo.ID select es).FirstOrDefault();
                    if (addES != null)
                        _ccFacLocal.GetCountryConfig().ExternalStatistic.AddExternalStatisticRow(addES.ID, addES.Category, addES.Reference, addES.Description, addES.YearValues, addES.Comment, addES.Source, addES.TableName, addES.Destination);
                }
            }
        }

        void ChangeExternalStatistics()
        {
            foreach (MergeControl.NodeInfo nodeInfo in _mcExternalStatistics.GetNodeInfoLocal(MergeForm.EXTERNAL_STATISTICS))
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed) { continue; }
                CountryConfig.ExternalStatisticRow exStat = (from ui in _ccFacLocal.GetExternalStatistics() where ui.ID == nodeInfo.ID select ui).FirstOrDefault();
                if (exStat == null) continue;
                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged) { continue; }
                    exStat[cellInfo.columnID] = cellInfo.text;
                }
            }
        }


        void ChangeCountrySettings()
        {
            string nameLocal, nameRemote;
            bool privateLocal, privateRemote;
            bool nameChangedLocal, nameChangedRemote, privateChangedLocal, privateChangedRemote;
            bool nameAcceptLocal, nameAcceptRemote, privateAcceptLocal, privateAcceptRemote;
            _mergeForm.GetInfoCountryControls(out nameLocal, out nameRemote,
                                   out privateLocal, out privateRemote,
                                   out nameChangedLocal, out nameChangedRemote, out privateChangedLocal, out privateChangedRemote,
                                   out nameAcceptLocal, out nameAcceptRemote, out privateAcceptLocal, out privateAcceptRemote);

            if ((nameChangedLocal && !nameAcceptLocal) || (nameChangedRemote && nameAcceptRemote))
                _ccLocal.Country.First().Name = nameRemote;
            if ((privateChangedLocal && !privateAcceptLocal) || (privateChangedRemote && privateAcceptRemote))
                _ccLocal.Country.First().Private = privateRemote ? DefPar.Value.YES : DefPar.Value.NO;
        }

        void GatherExtensionInfoToTreat(bool takeFromLocalData,
            bool localChanges, MergeControl.ChangeType relevantChangeTypeLocal, MergeControl.ChangeType relevantChangeTypeRemote,
            out List<DataConfig.ExtensionRow> extensions, out List<KeyValuePair<string, string>> extContent, out List<ExtensionOrGroup> globalExtension)
        {
            extensions = new List<DataConfig.ExtensionRow>();
            extContent = new List<KeyValuePair<string, string>>();
            globalExtension = new List<ExtensionOrGroup>();

            foreach (MergeControl.NodeInfo nodeInfo in localChanges ? _mcExtensions.GetNodeInfoLocal() : _mcExtensions.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (localChanges ? relevantChangeTypeLocal : relevantChangeTypeRemote) ||
                    nodeInfo.changeHandling != (localChanges ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; // not relevant, because not a rejected local-add nor an accepted remote-remove

                DataConfigFacade dcFac = takeFromLocalData ? _dcFacLocal : _dcFacRemote;
                CountryConfigFacade ccFac = takeFromLocalData ? _ccFacLocal : _ccFacRemote;
                SwitchablePolicyConfigFacade spcFac = takeFromLocalData ? _vcFacSwitchablePolicyLocal : _vcFacSwitchablePolicyRemote;

                if (nodeInfo.node.Level == EXTENSION_CHANGE)
                {
                    try
                    {
                        //This will work if the extension is local
                        //If the extension is global, it will throw an exception
                        extensions.Add((from e in ExtensionAndGroupMergeHelper.GetLocalExtensions(dcFac) where e.ID == nodeInfo.ID select e).First()); 
                    }
                    catch (Exception exception)
                    {
                        string msg = exception.Message;

                        //If the extension is global
                        //We get the information from the SwitchableConfigPolicyFile
                        var extension = ExtensionAndGroupMergeHelper.GetGlobalExtension(spcFac, nodeInfo.ID);
                        if(extension != null)
                        {
                            globalExtension.Add(new ExtensionOrGroup(extension));
                        }
                        
                    }
                }
                else if(nodeInfo.node.Level == EXTENSION_CONTENT_CHANGE)
                {
                    extContent.Add(new KeyValuePair<string, string>(nodeInfo.parentID, nodeInfo.ID)); 
                }

            }
        }

        void RemoveExtensionInfo(bool local)
        {
            List<DataConfig.ExtensionRow> extensions; List<KeyValuePair<string, string>> extContent;
            List<ExtensionOrGroup> globalExtension;
            GatherExtensionInfoToTreat(true, local, MergeControl.ChangeType.added, MergeControl.ChangeType.removed, out extensions, out extContent, out globalExtension);

            List<DataRow> toDel = new List<DataRow>();
            foreach (var ec in extContent)
                foreach (DataRow r in ExtMerge_GetTwinRows(ec.Key, ec.Value)) toDel.Add(r);
            foreach (DataRow r in toDel) r.Delete();

            foreach (DataConfig.ExtensionRow e in extensions) e.Delete();
            _dcLocal.AcceptChanges();

            
            if (_vcFacSwitchablePolicyLocal != null)
            {
                List<string> deletedGlobalExtensionIds = new List<string>();
                foreach(ExtensionOrGroup deletedExtension in globalExtension)
                {
                    deletedGlobalExtensionIds.Add(deletedExtension.id);
                }

                List<SwitchablePolicyConfig.SwitchablePolicyRow> ex = (from swp in _vcFacSwitchablePolicyLocal.GetSwitchablePolicyConfig().SwitchablePolicy select swp).ToList();
                for (int i = ex.Count - 1; i >= 0; --i)
                    if (deletedGlobalExtensionIds.Contains(ex[i].ID)) ex[i].Delete();

                _vcFacSwitchablePolicyLocal.AcceptChanges();
            }
        }

        void AddExtensionInfo(bool local)
        {
            List<DataConfig.ExtensionRow> extensions; List<KeyValuePair<string, string>> extContent;
            List<ExtensionOrGroup> globalExtension;
            GatherExtensionInfoToTreat(false, local, MergeControl.ChangeType.removed, MergeControl.ChangeType.added, out extensions, out extContent, out globalExtension);

            foreach (DataConfig.ExtensionRow addExt in extensions)
                _dcFacLocal.GetDataConfig().Extension.AddExtensionRow(addExt.ID, addExt.Name, addExt.ShortName, addExt.Look);
            _dcLocal.AcceptChanges();

            
            if (_vcFacSwitchablePolicyLocal != null)
            {
                foreach (ExtensionOrGroup addExt in globalExtension)
                    if (_vcFacSwitchablePolicyLocal.GetSwitchablePolicyConfig().SwitchablePolicy.FindByID(addExt.id) == null)
                        _vcFacSwitchablePolicyLocal.GetSwitchablePolicyConfig().SwitchablePolicy.AddSwitchablePolicyRow(addExt.id, addExt.name, addExt.shortName, addExt.look.ToXml());
                _vcFacSwitchablePolicyLocal.AcceptChanges();
            }

            foreach (var addContent in extContent)
            {
                string extID = addContent.Key, polFunPar = addContent.Value;
                string preFix = polFunPar.Substring(0, MergeAdministrator.MERGEINFO_POLID.Length);
                string polFunParID = polFunPar.Substring(MergeAdministrator.MERGEINFO_POLID.Length, polFunPar.LastIndexOf(MergeAdministrator.MERGEINFO_EXTID) - MergeAdministrator.MERGEINFO_EXTID.Length);

                switch (preFix)
                {
                    case MergeAdministrator.MERGEINFO_POLID:
                        CountryConfig.PolicyRow rPol = _ccFacLocal.GetPolicyRowByID(polFunParID); if (rPol == null) continue;
                        bool polBaseOff = (from e in _ccRemote.Extension_Policy where e.ExtensionID == extID && e.PolicyID == polFunParID select e.BaseOff).First();
                        foreach (CountryConfig.PolicyRow twinPolRow in (from p in _ccLocal.Policy where p.Order == rPol.Order select p))
                            _ccLocal.Extension_Policy.AddExtension_PolicyRow(extID, twinPolRow, polBaseOff);
                        break;
                    case MergeAdministrator.MERGEINFO_FUNID:
                        CountryConfig.FunctionRow rFun = _ccFacLocal.GetFunctionRowByID(polFunParID); if (rFun == null) continue;
                        bool funBaseOff = (from e in _ccRemote.Extension_Function where e.ExtensionID == extID && e.FunctionID == polFunParID select e.BaseOff).First();
                        foreach (CountryConfig.FunctionRow twinFunRow in (from f in _ccLocal.Function where f.PolicyRow.Order == rFun.PolicyRow.Order & f.Order == rFun.Order select f))
                            _ccLocal.Extension_Function.AddExtension_FunctionRow(extID, twinFunRow, funBaseOff);
                        break;
                    case MergeAdministrator.MERGEINFO_PARID:
                        CountryConfig.ParameterRow rPar = _ccFacLocal.GetParameterRowByID(polFunParID); if (rPar == null) continue;
                        bool parBaseOff = (from e in _ccRemote.Extension_Parameter where e.ExtensionID == extID && e.ParameterID == polFunParID select e.BaseOff).First();
                        foreach (CountryConfig.ParameterRow twinParRow in (from p in _ccLocal.Parameter where p.FunctionRow.PolicyRow.Order == rPar.FunctionRow.PolicyRow.Order & p.FunctionRow.Order == rPar.FunctionRow.Order & p.Order == rPar.Order select p))
                            _ccLocal.Extension_Parameter.AddExtension_ParameterRow(extID, twinParRow, parBaseOff);
                        break;
                }
            }
            _ccLocal.AcceptChanges();
        }

        void ChangeExtensionInfo(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcExtensions.GetNodeInfoLocal() : _mcExtensions.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || local ? cellInfo.acceptChange : !cellInfo.acceptChange)
                        continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? _mcExtensions.GetTwinCellInfo(cellInfo) : cellInfo;
                    switch (nodeInfo.node.Level)
                    {
                        case EXTENSION_CHANGE:
                            try {
                                DataConfig.ExtensionRow extRow = (from e in ExtensionAndGroupMergeHelper.GetLocalExtensions(_dcFacLocal) where e.ID == nodeInfo.ID select e).First();
                                switch (cellInfo.columnID)
                                {
                                    case MergeAdministrator.MERGECOL_EXT_NAME: extRow.Name = cellInfoRemote.text; break;
                                    case MergeAdministrator.MERGECOL_EXT_SHORT_BASEOFF: extRow.ShortName = cellInfoRemote.text; break;
                                    case MergeAdministrator.MERGECOL_EXT_LOOK: extRow.Look = cellInfoRemote.text; break;
                                }
                            }
                            catch (Exception e) //If the extension is global
                            {
                                string message = e.Message;
                                
                                if (_vcFacSwitchablePolicyLocal != null)
                                {
                                    var rowExt = ExtensionAndGroupMergeHelper.GetGlobalExtension(_vcFacSwitchablePolicyLocal, nodeInfo.ID);

                                    if (rowExt != null)
                                    {
                                        ExtensionOrGroup row = new ExtensionOrGroup(rowExt);

                                        if (row != null)
                                        {
                                            switch (cellInfo.columnID)
                                            {
                                                case MergeAdministrator.MERGECOL_EXT_NAME: ChangeExtensionRow(_vcFacSwitchablePolicyLocal, nodeInfo.ID, cellInfoRemote.text, row.shortName, row.look); break;
                                                case MergeAdministrator.MERGECOL_EXT_SHORT_BASEOFF: ChangeExtensionRow(_vcFacSwitchablePolicyLocal, nodeInfo.ID, row.name, cellInfoRemote.text, row.look); break;
                                                case MergeAdministrator.MERGECOL_EXT_LOOK:
                                                    {
                                                        //For the look is a bit more complex, we need to get the remote ExtensionOrGroup
                                                        var rowRemoteExt = ExtensionAndGroupMergeHelper.GetGlobalExtension(_vcFacSwitchablePolicyRemote, nodeInfo.ID);
                                                        ExtensionOrGroup remoteExtension = null;
                                                        if (rowRemoteExt != null)
                                                        {
                                                            remoteExtension = new ExtensionOrGroup(rowRemoteExt);
                                                        }
                                                        ChangeExtensionRow(_vcFacSwitchablePolicyLocal, nodeInfo.ID, row.name, row.shortName, remoteExtension.look);
                                                        break;
                                                    }

                                            }
                                        }
                                    }
                                }

                                void ChangeExtensionRow(SwitchablePolicyConfigFacade configFacade, string id, string name, string shortName, LookDef look)
                                {
                                    SwitchablePolicyConfig config = configFacade.GetSwitchablePolicyConfig();
                                    SwitchablePolicyConfig.SwitchablePolicyRow exRow = (from lg in config.SwitchablePolicy where lg.ID == id select lg).First();
                                    exRow.LongName = name; exRow.NamePattern = shortName; exRow.Look = look.ToXml();
                                }
                            }
                            break;
                        case EXTENSION_CONTENT_CHANGE: // must be change in BaseOff
                            foreach (var r in ExtMerge_GetTwinRows(nodeInfo.parentID, nodeInfo.ID))
                                r["BaseOff"] = Convert.ToBoolean(cellInfoRemote.text);
                            break;
                    }
                }
            }
        }

        void GatherLookGroupsInfoToTreat(bool takeFromLocalData,
            bool localChanges, MergeControl.ChangeType relevantChangeTypeLocal, MergeControl.ChangeType relevantChangeTypeRemote,
            out List<CountryConfig.LookGroupRow> lookGroups, out List<KeyValuePair<string, string>> lookGroupsContent)
        {
            lookGroups = new List<CountryConfig.LookGroupRow>();
            lookGroupsContent = new List<KeyValuePair<string, string>>();

            foreach (MergeControl.NodeInfo nodeInfo in localChanges ? _mcLookGroups.GetNodeInfoLocal() : _mcLookGroups.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (localChanges ? relevantChangeTypeLocal : relevantChangeTypeRemote) ||
                    nodeInfo.changeHandling != (localChanges ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; // not relevant, because not a rejected local-add nor an accepted remote-remove
                CountryConfigFacade ccFac = takeFromLocalData ? _ccFacLocal : _ccFacRemote;

                switch (nodeInfo.node.Level)
                {
                    case LOOKGROUPS_CHANGE: lookGroups.Add((from e in from lg in ccFac.GetCountryConfig().LookGroup select lg
                                                            where e.ID == nodeInfo.ID select e).First()); break;
                    case LOOKGROUPS_CONTENT_CHANGE: lookGroupsContent.Add(new KeyValuePair<string, string>(nodeInfo.parentID, nodeInfo.ID)); break;
                }
            }
        }


        List<DataRow> LookGroupsMerge_GetTwinRows(string extID, string polFunPar)
        { // as we stored only the ID of one pol/fun/par (i.e. of an arbitrary system), we need to find all Extension_XXXRows of all systems
            List<DataRow> twinRows = new List<DataRow>();
            string preFix = polFunPar.Substring(0, MergeAdministrator.MERGEINFO_GROUP_POLID.Length);
            string polFunParID = polFunPar.Substring(MergeAdministrator.MERGEINFO_GROUP_POLID.Length, polFunPar.LastIndexOf(MergeAdministrator.MERGEINFO_GROUPID) - MergeAdministrator.MERGEINFO_GROUPID.Length);

            switch (preFix)
            {
                case MergeAdministrator.MERGEINFO_POLID:
                    CountryConfig.PolicyRow rPol = _ccFacLocal.GetPolicyRowByID(polFunParID); if (rPol == null) return twinRows;
                    foreach (CountryConfig.LookGroup_PolicyRow ep in (from e in _ccLocal.LookGroup_Policy where e.LookGroupID == extID select e))
                    {
                        CountryConfig.PolicyRow trPol = _ccFacLocal.GetPolicyRowByID(ep.PolicyID);
                        if (trPol != null && trPol.Order == rPol.Order) twinRows.Add(ep);
                    }
                    break;
                case MergeAdministrator.MERGEINFO_FUNID:
                    CountryConfig.FunctionRow rFun = _ccFacLocal.GetFunctionRowByID(polFunParID); if (rFun == null) return twinRows;
                    foreach (CountryConfig.LookGroup_FunctionRow ef in (from e in _ccLocal.LookGroup_Function where e.LookGroupID == extID select e))
                    {
                        CountryConfig.FunctionRow trFun = _ccFacLocal.GetFunctionRowByID(ef.FunctionID);
                        if (trFun != null && trFun.PolicyRow.Order == rFun.PolicyRow.Order && trFun.Order == rFun.Order) twinRows.Add(ef);
                    }
                    break;
                case MergeAdministrator.MERGEINFO_PARID:
                    CountryConfig.ParameterRow rPar = _ccFacLocal.GetParameterRowByID(polFunParID); if (rPar == null) return twinRows;
                    foreach (CountryConfig.LookGroup_ParameterRow ep in (from e in _ccLocal.LookGroup_Parameter where e.LookGroupID == extID select e))
                    {
                        CountryConfig.ParameterRow trPar = _ccFacLocal.GetParameterRowByID(ep.ParameterID);
                        if (trPar != null && trPar.FunctionRow.PolicyRow.Order == rPar.FunctionRow.PolicyRow.Order &&
                            trPar.FunctionRow.Order == rPar.FunctionRow.Order && trPar.Order == rPar.Order) twinRows.Add(ep);
                    }
                    break;
            }
            return twinRows;
        }

        void RemoveLookGroupsInfo(bool local)
        {
            List<CountryConfig.LookGroupRow> lookGroups; List<KeyValuePair<string, string>> lookGroupsContent;
            GatherLookGroupsInfoToTreat(true, local, MergeControl.ChangeType.added, MergeControl.ChangeType.removed, out lookGroups, out lookGroupsContent);

            List<DataRow> toDel = new List<DataRow>();
            foreach (var ec in lookGroupsContent)
                foreach (DataRow r in LookGroupsMerge_GetTwinRows(ec.Key, ec.Value)) toDel.Add(r);
            foreach (DataRow r in toDel) r.Delete();

            foreach (CountryConfig.LookGroupRow e in lookGroups) e.Delete();

            _ccLocal.AcceptChanges();
        }

        void AddLookGroupsInfo(bool local)
        {
            List<CountryConfig.LookGroupRow> lookGroups; List<KeyValuePair<string, string>> lookGroupsContent;
            GatherLookGroupsInfoToTreat(false, local, MergeControl.ChangeType.removed, MergeControl.ChangeType.added, out lookGroups, out lookGroupsContent);

            foreach (CountryConfig.LookGroupRow addLookGroups in lookGroups)
                _ccFacLocal.GetCountryConfig().LookGroup.AddLookGroupRow(addLookGroups.ID, addLookGroups.Name, addLookGroups.ShortName, addLookGroups.Look);
            _ccLocal.AcceptChanges();

            foreach (var addContent in lookGroupsContent)
            {
                string lookGroupsID = addContent.Key, polFunPar = addContent.Value;
                string preFix = polFunPar.Substring(0, MergeAdministrator.MERGEINFO_GROUP_POLID.Length);
                string polFunParID = polFunPar.Substring(MergeAdministrator.MERGEINFO_GROUP_POLID.Length, polFunPar.LastIndexOf(MergeAdministrator.MERGEINFO_GROUPID) - MergeAdministrator.MERGEINFO_GROUPID.Length);

                CountryConfig.LookGroupRow groupRow = _ccFacLocal.GetCountryConfig().LookGroup.FindByID(lookGroupsID);
                switch (preFix)
                {
                    case MergeAdministrator.MERGEINFO_GROUP_POLID:
                        CountryConfig.PolicyRow rPol = _ccFacLocal.GetPolicyRowByID(polFunParID); if (rPol == null) continue;
                        foreach (CountryConfig.PolicyRow twinPolRow in (from p in _ccLocal.Policy where p.Order == rPol.Order select p))
                            _ccLocal.LookGroup_Policy.AddLookGroup_PolicyRow(groupRow, twinPolRow);
                        break;
                    case MergeAdministrator.MERGEINFO_FUNID:
                        CountryConfig.FunctionRow rFun = _ccFacLocal.GetFunctionRowByID(polFunParID); if (rFun == null) continue;
                        foreach (CountryConfig.FunctionRow twinFunRow in (from f in _ccLocal.Function where f.PolicyRow.Order == rFun.PolicyRow.Order & f.Order == rFun.Order select f))
                            _ccLocal.LookGroup_Function.AddLookGroup_FunctionRow(groupRow, twinFunRow);
                            
                        break;
                    case MergeAdministrator.MERGEINFO_PARID:
                        CountryConfig.ParameterRow rPar = _ccFacLocal.GetParameterRowByID(polFunParID); if (rPar == null) continue;
                        foreach (CountryConfig.ParameterRow twinParRow in (from p in _ccLocal.Parameter where p.FunctionRow.PolicyRow.Order == rPar.FunctionRow.PolicyRow.Order & p.FunctionRow.Order == rPar.FunctionRow.Order & p.Order == rPar.Order select p))
                            _ccLocal.LookGroup_Parameter.AddLookGroup_ParameterRow(groupRow, twinParRow);
                        break;
                }
            }
            _ccLocal.AcceptChanges();
        }

        void ChangeLookGroupsInfo(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcLookGroups.GetNodeInfoLocal() : _mcLookGroups.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || local ? cellInfo.acceptChange : !cellInfo.acceptChange)
                        continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? _mcLookGroups.GetTwinCellInfo(cellInfo) : cellInfo;
                    switch (nodeInfo.node.Level)
                    {
                        case LOOKGROUPS_CHANGE:
                            CountryConfig.LookGroupRow groupRow = (from e in from lg in _ccFacLocal.GetCountryConfig().LookGroup select lg
                                                                   where e.ID == nodeInfo.ID select e).First();
                            switch (cellInfo.columnID)
                            {
                                case MergeAdministrator.MERGECOL_GROUP_NAME: groupRow.Name = cellInfoRemote.text; break;
                                case MergeAdministrator.MERGECOL_GROUP_SHORT_NAME: groupRow.ShortName = cellInfoRemote.text; break;
                                case MergeAdministrator.MERGECOL_GROUP_LOOK: groupRow.Look = cellInfoRemote.text; break;
                            }
                            break;
                        case LOOKGROUPS_CONTENT_CHANGE: // must be change in BaseOff
                            //foreach (var r in ExtMerge_GetTwinRows(nodeInfo.parentID, nodeInfo.ID))
                              //  r["BaseOff"] = Convert.ToBoolean(cellInfoRemote.text);
                            break;
                    }
                }
            }
        }

        List<DataRow> ExtMerge_GetTwinRows(string extID, string polFunPar)
        { // as we stored only the ID of one pol/fun/par (i.e. of an arbitrary system), we need to find all Extension_XXXRows of all systems
            List<DataRow> twinRows = new List<DataRow>();
            string preFix = polFunPar.Substring(0, MergeAdministrator.MERGEINFO_POLID.Length);
            //int polFunParIDLen = polFunPar.LastIndexOf(MergeAdministrator.MERGEINFO_EXTID);
            string polFunParID = polFunPar.Substring(MergeAdministrator.MERGEINFO_POLID.Length, polFunPar.LastIndexOf(MergeAdministrator.MERGEINFO_EXTID) - MergeAdministrator.MERGEINFO_EXTID.Length);

            switch (preFix)
            {
                case MergeAdministrator.MERGEINFO_POLID:
                    CountryConfig.PolicyRow rPol = _ccFacLocal.GetPolicyRowByID(polFunParID); if (rPol == null) return twinRows;
                    foreach (CountryConfig.Extension_PolicyRow ep in (from e in _ccLocal.Extension_Policy where e.ExtensionID == extID select e))
                    {
                        CountryConfig.PolicyRow trPol = _ccFacLocal.GetPolicyRowByID(ep.PolicyID);
                        if (trPol != null && trPol.Order == rPol.Order) twinRows.Add(ep);
                    }
                    break;
                case MergeAdministrator.MERGEINFO_FUNID:
                    CountryConfig.FunctionRow rFun = _ccFacLocal.GetFunctionRowByID(polFunParID); if (rFun == null) return twinRows;
                    foreach (CountryConfig.Extension_FunctionRow ef in (from e in _ccLocal.Extension_Function where e.ExtensionID == extID select e))
                    {
                        CountryConfig.FunctionRow trFun = _ccFacLocal.GetFunctionRowByID(ef.FunctionID);
                        if (trFun != null && trFun.PolicyRow.Order == rFun.PolicyRow.Order && trFun.Order == rFun.Order) twinRows.Add(ef);
                    }
                    break;
                case MergeAdministrator.MERGEINFO_PARID:
                    CountryConfig.ParameterRow rPar = _ccFacLocal.GetParameterRowByID(polFunParID); if (rPar == null) return twinRows;
                    foreach (CountryConfig.Extension_ParameterRow ep in (from e in _ccLocal.Extension_Parameter where e.ExtensionID == extID select e))
                    {
                        CountryConfig.ParameterRow trPar = _ccFacLocal.GetParameterRowByID(ep.ParameterID);
                        if (trPar != null && trPar.FunctionRow.PolicyRow.Order == rPar.FunctionRow.PolicyRow.Order &&
                            trPar.FunctionRow.Order == rPar.FunctionRow.Order && trPar.Order == rPar.Order) twinRows.Add(ep);
                    }
                    break;
            }
            return twinRows;
        }

        void GatherExtensionSwitchesToTreat(bool takeFromLocalData,
            bool localChanges, MergeControl.ChangeType relevantChangeTypeLocal, MergeControl.ChangeType relevantChangeTypeRemote,
            out List<DataConfig.PolicySwitchRow> extSwitches)
        {
            extSwitches = new List<DataConfig.PolicySwitchRow>();

            foreach (MergeControl.NodeInfo nodeInfo in localChanges ? _mcExtSwitches.GetNodeInfoLocal() : _mcExtSwitches.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (localChanges ? relevantChangeTypeLocal : relevantChangeTypeRemote) ||
                    nodeInfo.changeHandling != (localChanges ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; // not relevant, because not a rejected local-add nor an accepted remote-remove

                DataConfig dc = takeFromLocalData ? _dcLocal : _dcRemote;

                string[] ids = nodeInfo.ID.Split('|'); if (ids.Count() != 3) continue;
                string extID = ids[0], sysID = ids[1], dbID = ids[2];
                var ps = (from p in dc.PolicySwitch where p.SwitchablePolicyID == extID & p.SystemID == sysID & p.DataBaseID == dbID select p).FirstOrDefault();
                if (ps != null) extSwitches.Add(ps);
            }
        }

        void RemoveExtensionSwitches(bool local)
        {
            List<DataConfig.PolicySwitchRow> extSwitches;
            GatherExtensionSwitchesToTreat(true, local, MergeControl.ChangeType.added, MergeControl.ChangeType.removed, out extSwitches);

            foreach (DataConfig.PolicySwitchRow ps in extSwitches) ps.Delete();
            _dcLocal.AcceptChanges();
        }

        void AddExtensionSwitches(bool local)
        {
            List<DataConfig.PolicySwitchRow> extSwitches;
            GatherExtensionSwitchesToTreat(false, local, MergeControl.ChangeType.removed, MergeControl.ChangeType.added, out extSwitches);

            foreach (DataConfig.PolicySwitchRow ps in extSwitches)
            {
                // only add if actually all components exist
                if ((ExtensionAndGroupManager.GetGlobalExtension(ps.SwitchablePolicyID) == null &&
                    (from e in _dcLocal.Extension where e.ID == ps.SwitchablePolicyID select e).Count() == 0) ||
                    (from dbs in _dcLocal.DBSystemConfig where dbs.DataBaseID == ps.DataBaseID & dbs.SystemID == ps.SystemID select dbs).Count() == 0)
                    continue;
                DataConfig.PolicySwitchRow existing = (from d in _dcLocal.PolicySwitch 
                                                       where d.SwitchablePolicyID == ps.SwitchablePolicyID &
                                                             d.SystemID == ps.SystemID & d.DataBaseID == ps.DataBaseID
                                                       select d).FirstOrDefault(); // could exist, if value is n/a
                if (existing != null) existing.Value = ps.Value;
                else _dcLocal.PolicySwitch.AddPolicySwitchRow(ps.SwitchablePolicyID, ps.SystemID, ps.DataBaseID, ps.Value);
            }
            _dcLocal.AcceptChanges();
        }

        void ChangeExtensionSwitches(bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? _mcExtSwitches.GetNodeInfoLocal() : _mcExtSwitches.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || local ? cellInfo.acceptChange : !cellInfo.acceptChange)
                        continue; // not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? _mcExtensions.GetTwinCellInfo(cellInfo) : cellInfo;

                    string[] ids = nodeInfo.ID.Split('|'); if (ids.Count() != 3) continue;
                    string extID = ids[0], sysID = ids[1], dbID = ids[2];
                    DataConfig.DBSystemConfigRow dbSys = (from dbs in _dcLocal.DBSystemConfig where dbs.DataBaseID == dbID & dbs.SystemID == sysID select dbs).FirstOrDefault();
                    if (dbSys != null) ExtensionAndGroupManager.SetExtensionDefaultSwitch(_dcFacLocal.GetDataConfig(), dbSys, extID, cellInfoRemote.text);
                }
            }
            _dcLocal.AcceptChanges();
        }
    }   
}
