using EM_Common;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EM_UI.VersionControl.Merging
{
    class SwitchablePolicyConfigApplyAdministrator
    {
        MergeAdministrator _mergeAdministrator = null;

        SwitchablePolicyConfigFacade _vcFacLocal = null, _vcFacRemote = null;
        SwitchablePolicyConfig _vcLocal = null, _vcRemote = null;

        SwitchablePolicyConfigMergeForm _mergeForm = null;
        MergeControl _mcSwitchablePolicyConfig = null;

        internal SwitchablePolicyConfigApplyAdministrator(MergeAdministrator mergeAdministrator, SwitchablePolicyConfigMergeForm mergeForm,
                                             SwitchablePolicyConfigFacade vcFacLocal, SwitchablePolicyConfigFacade vcFacRemote)
        {
            _mergeAdministrator = mergeAdministrator;

            _vcFacLocal = vcFacLocal; _vcLocal = _vcFacLocal.GetSwitchablePolicyConfig();
            _vcFacRemote = vcFacRemote; _vcRemote = _vcFacRemote.GetSwitchablePolicyConfig();

            _mergeForm = mergeForm;
            _mcSwitchablePolicyConfig = _mergeForm.GetMergeControlByName(EMPath.EM2_FILE_EXTENSIONS); 

        }

        internal void Apply()
        {
            //apply locally rejected or remotely accepted changes of variables (e.g. monetary-state)
            ApplyChanges(_mcSwitchablePolicyConfig, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, true);
            ApplyChanges(_mcSwitchablePolicyConfig, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, false);

            //remove locally added and rejected or remotely removed and accepted variables
            ApplyRemovals(_mcSwitchablePolicyConfig, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, true);
            ApplyRemovals(_mcSwitchablePolicyConfig, _vcLocal.SwitchablePolicy, _vcLocal.SwitchablePolicy.IDColumn.ColumnName, false);

            //add locally removed and rejected or remotely added and accepted variables
            AddSwitchablePolicyConfig(true); AddSwitchablePolicyConfig(false);
        }

        void ApplyChanges(MergeControl mergeControl, DataTable localDataTable, string idColumnName, bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                //the existence of the component in local/remote/parent country reflects whether it is added (loc/rem), changed (loc/rem), ...
                string whereClause = idColumnName + " = '" + nodeInfo.ID + "'";


                DataRow dataRow = localDataTable.Select(whereClause).First();

                foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    if (!cellInfo.isChanged || (local ? cellInfo.acceptChange : !cellInfo.acceptChange))
                        continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                    MergeControl.CellInfo cellInfoRemote = local ? mergeControl.GetTwinCellInfo(cellInfo) : cellInfo;
                    dataRow.SetField<string>(cellInfo.columnID, cellInfoRemote.text);
                }
            }
        }

        void ApplyRemovals(MergeControl mergeControl, DataTable localDataTable, string idColumnName, bool local)
        {
            foreach (string ID in GetRelevantIDs(mergeControl, local, false))
            {
                string whereClause = idColumnName + " = '" + ID + "'";

                DataRow dataRow = localDataTable.Select(whereClause).First();
                dataRow.Delete();
            }
            _vcFacLocal.GetSwitchablePolicyConfig().AcceptChanges();
        }

        void AddSwitchablePolicyConfig(bool local)
        {
            List<MergeControl.NodeInfo> relevantRows = null;
            MergeControl.NodeInfo currentRelevantNode = null;
            if (!local)
            {
                relevantRows = new List<MergeControl.NodeInfo>();
            }

            foreach (string ID in GetRelevantIDs(_mcSwitchablePolicyConfig, local, true, relevantRows))
            {
                currentRelevantNode = null;
                GlobLocExtensionRow origExtensionRow = ExtensionAndGroupManager.GetGlobalExtension(ID);
                if(origExtensionRow == null && relevantRows != null)
                {

                    foreach(MergeControl.NodeInfo currentRelevantRow in relevantRows)
                    {
                        if (currentRelevantRow.ID.Equals(ID))
                        {
                            currentRelevantNode = currentRelevantRow;
                            break;
                        }
                    }
                }

                ExtensionAndGroupMergeHelper.CopyGlobalExtensionFromAnotherConfig(_vcLocal, origExtensionRow, currentRelevantNode);
            }
                

                
        }

        List<string> GetRelevantIDs(MergeControl mergeControl, bool local, bool add, List<MergeControl.NodeInfo>  relevantRows = null)
        {
            List<string> relevantIDs = new List<string>();
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != (local ? (add ? MergeControl.ChangeType.removed : MergeControl.ChangeType.added)
                                                  : (add ? MergeControl.ChangeType.added : MergeControl.ChangeType.removed)) ||
                    nodeInfo.changeHandling != (local ? MergeControl.ChangeHandling.reject : MergeControl.ChangeHandling.accept))
                    continue; //not relevant, because not a rejected local-remove nor an accepted remote-add
                relevantIDs.Add(nodeInfo.ID);

                if (!local && relevantRows != null)
                {
                    relevantRows.Add(nodeInfo);

                }
            }
            return relevantIDs;
        }

    }
}
