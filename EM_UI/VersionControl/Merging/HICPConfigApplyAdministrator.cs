using EM_Common;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_UI.VersionControl.Merging
{
    class HICPConfigApplyAdministrator
    {
        MergeAdministrator _mergeAdministrator = null;

        HICPConfigFacade _vcFacLocal = null, _vcFacRemote = null;
        HICPConfig _vcLocal = null, _vcRemote = null;

        HICPConfigMergeForm _mergeForm = null;
        MergeControl _mcHICPConfig = null;

        internal HICPConfigApplyAdministrator(MergeAdministrator mergeAdministrator, HICPConfigMergeForm mergeForm,
                                             HICPConfigFacade vcFacLocal, HICPConfigFacade vcFacRemote)
        {
            _mergeAdministrator = mergeAdministrator;

            _vcFacLocal = vcFacLocal; _vcLocal = _vcFacLocal.GetHICPConfig();
            _vcFacRemote = vcFacRemote; _vcRemote = _vcFacRemote.GetHICPConfig();

            _mergeForm = mergeForm;
            _mcHICPConfig = _mergeForm.GetMergeControlByName(EMPath.EM2_FILE_HICP); 

        }

        internal void Apply()
        {
            //apply locally rejected or remotely accepted changes of variables (e.g. monetary-state)
            ApplyChanges(_mcHICPConfig, _vcLocal.HICP, _vcLocal.HICP.CountryColumn.ColumnName, _vcLocal.HICP.YearColumn.ColumnName, true);
            ApplyChanges(_mcHICPConfig, _vcLocal.HICP, _vcLocal.HICP.CountryColumn.ColumnName, _vcLocal.HICP.YearColumn.ColumnName, false);


            //remove locally added and rejected or remotely removed and accepted variables
            ApplyRemovals(_mcHICPConfig, _vcLocal.HICP, _vcLocal.HICP.CountryColumn.ColumnName, _vcLocal.HICP.YearColumn.ColumnName, true);
            ApplyRemovals(_mcHICPConfig, _vcLocal.HICP, _vcLocal.HICP.CountryColumn.ColumnName, _vcLocal.HICP.YearColumn.ColumnName, false);

            //add locally removed and rejected or remotely added and accepted variables
            AddHICP(true); AddHICP(false);
        }

        void ApplyChanges(MergeControl mergeControl, DataTable localDataTable, string idColumnNameCountry, string idColumnNameYear, bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                string country = nodeInfo.ID.Substring(0, nodeInfo.ID.IndexOf("_"));
                string year = nodeInfo.ID.Substring(nodeInfo.ID.IndexOf("_") + 1);

                //the existence of the component in local/remote/parent country reflects whether it is added (loc/rem), changed (loc/rem), ...
                string whereClause = idColumnNameCountry + " = '" + country + "' AND " + idColumnNameYear + " = '" + year + "'";


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

        void ApplyRemovals(MergeControl mergeControl, DataTable localDataTable, string idColumnNameCountry, string idColumnNameYear, bool local)
        {
            foreach (string ID in GetRelevantIDs(mergeControl, local, false))
            {
                string country = ID.Substring(0, ID.IndexOf("_"));
                string year = ID.Substring(ID.IndexOf("_") + 1);

                string whereClause = idColumnNameCountry + " = '" + country + "' AND " + idColumnNameYear + " = '" + year + "'";

                DataRow dataRow = localDataTable.Select(whereClause).First();
                dataRow.Delete();
            }
            _vcFacLocal.GetHICPConfig().AcceptChanges();
        }

        void AddHICP(bool local)
        {
            foreach (string ID in GetRelevantIDs(_mcHICPConfig, local, true))
            {
                string country = ID.Substring(0, ID.IndexOf("_"));
                string year = ID.Substring(ID.IndexOf("_") + 1);

                HICPConfig.HICPRow dataRow = _vcFacRemote.GetHICP(country, year);

                HICPConfigFacade.CopyHICPFromAnotherConfig(_vcLocal, dataRow);

            }

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

    }
}
