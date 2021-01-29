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
    class ExchangeRatesConfigApplyAdministrator
    {
        MergeAdministrator _mergeAdministrator = null;

        ExchangeRatesConfigFacade _vcFacLocal = null, _vcFacRemote = null;
        ExchangeRatesConfig _vcLocal = null, _vcRemote = null;

        ExchangeRatesConfigMergeForm _mergeForm = null;
        MergeControl _mcExchangeRatesConfig = null;

        internal ExchangeRatesConfigApplyAdministrator(MergeAdministrator mergeAdministrator, ExchangeRatesConfigMergeForm mergeForm,
                                             ExchangeRatesConfigFacade vcFacLocal, ExchangeRatesConfigFacade vcFacRemote)
        {
            _mergeAdministrator = mergeAdministrator;

            _vcFacLocal = vcFacLocal; _vcLocal = _vcFacLocal.GetExchangeRatesConfig();
            _vcFacRemote = vcFacRemote; _vcRemote = _vcFacRemote.GetExchangeRatesConfig();

            _mergeForm = mergeForm;
            _mcExchangeRatesConfig = _mergeForm.GetMergeControlByName(EMPath.EM2_FILE_EXRATES); 

        }

        internal void Apply()
        {
            //apply locally rejected or remotely accepted changes of variables (e.g. monetary-state)
            ApplyChanges(_mcExchangeRatesConfig, _vcLocal.ExchangeRates, _vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, true);
            ApplyChanges(_mcExchangeRatesConfig, _vcLocal.ExchangeRates, _vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, false);


            //remove locally added and rejected or remotely removed and accepted variables
            ApplyRemovals(_mcExchangeRatesConfig, _vcLocal.ExchangeRates, _vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, true);
            ApplyRemovals(_mcExchangeRatesConfig, _vcLocal.ExchangeRates, _vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, false);

            //add locally removed and rejected or remotely added and accepted variables
            AddExchangeRates(_vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, true); AddExchangeRates(_vcLocal.ExchangeRates.CountryColumn.ColumnName, _vcLocal.ExchangeRates.ValidForColumn.ColumnName, false);
        }

        void ApplyChanges(MergeControl mergeControl, DataTable localDataTable, string idColumnNameID1, string idColumnNameID2, bool local)
        {
            foreach (MergeControl.NodeInfo nodeInfo in local ? mergeControl.GetNodeInfoLocal() : mergeControl.GetNodeInfoRemote())
            {
                if (nodeInfo.changeType != MergeControl.ChangeType.changed ||
                    nodeInfo.changeHandling == (local ? MergeControl.ChangeHandling.accept : MergeControl.ChangeHandling.reject))
                    continue; //not relevant, because neither changed nor locally accepted nor remotely rejected

                string country = nodeInfo.ID.Substring(0, nodeInfo.ID.IndexOf("_"));
                string validFor = nodeInfo.ID.Substring(nodeInfo.ID.IndexOf("_") + 1);

                //the existence of the component in local/remote/parent country reflects whether it is added (loc/rem), changed (loc/rem), ...
                string whereClause = idColumnNameID1 + " = '" + country + "' AND " + idColumnNameID2 + " = '" + validFor + "'";


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

        void ApplyRemovals(MergeControl mergeControl, DataTable localDataTable, string idColumnNameID1, string idColumnNameID2, bool local)
        {
            foreach (string ID in GetRelevantIDs(mergeControl, local, false))
            {
                string country = ID.Substring(0, ID.IndexOf("_"));
                string validFor = ID.Substring(ID.IndexOf("_") + 1);

                string whereClause = idColumnNameID1 + " = '" + country + "' AND " + idColumnNameID2 + " = '" + validFor + "'";

                DataRow dataRow = localDataTable.Select(whereClause).First();
                dataRow.Delete();
            }
            _vcFacLocal.GetExchangeRatesConfig().AcceptChanges();
        }

        void AddExchangeRates(string idColumnNameID1, string idColumnNameID2, bool local)
        {
            foreach (string ID in GetRelevantIDs(_mcExchangeRatesConfig, local, true))
            {
                string country = ID.Substring(0, ID.IndexOf("_"));
                string validFor = ID.Substring(ID.IndexOf("_") + 1);

                string whereClause = idColumnNameID1 + " = '" + country + "' AND " + idColumnNameID2 + " = '" + validFor + "'";

                ExchangeRatesConfig.ExchangeRatesRow dataRow = (ExchangeRatesConfig.ExchangeRatesRow) _vcRemote.ExchangeRates.Select(whereClause).First();

                ExchangeRatesConfigFacade.CopyExchangeRatesFromAnotherConfig(_vcLocal, dataRow);

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
