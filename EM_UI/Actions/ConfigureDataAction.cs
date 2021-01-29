using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.Actions
{
    internal class ConfigDataAction : BaseAction
    {
        DataConfigFacade _dataConfigFacade = null;
        string _countryShortName = string.Empty;
        bool _actionIsCanceled = false;
        bool _datasetAdded = false;

        internal ConfigDataAction(string countryShortName, DataConfigFacade dataConfigFacade)
        {
            _countryShortName = countryShortName;
            _dataConfigFacade = dataConfigFacade;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal bool DatasetAdded()
        {
            return _datasetAdded;
        }

        internal override void PerformAction()
        {
            ConfigureDataForm configureDataForm = new ConfigureDataForm(_countryShortName);

            //firstly provide dialog with the information it needs to display (i.e. info on datasets and available system-dataset combinations) ...
            foreach (CountryConfig.SystemRow systemRow in CountryAdministrator.GetCountryConfigFacade(_countryShortName).GetSystemRows().OrderBy(s => long.Parse(s.Order)))
                configureDataForm._systemInfo.Add(systemRow.ID, systemRow.Name);

            foreach (DataConfig.DataBaseRow dataBaseRow in _dataConfigFacade.GetDataBaseRows())
            {
                ConfigureDataForm.RowTag rowTag = new ConfigureDataForm.RowTag();
                rowTag.ID = dataBaseRow.ID;
                rowTag.Name = dataBaseRow.Name;
                rowTag.Comment = dataBaseRow.Comment;
                rowTag.YearCollection = dataBaseRow.YearCollection;
                rowTag.YearInc = dataBaseRow.YearInc;
                rowTag.Currency = dataBaseRow.Currency;
                rowTag.FilePath = dataBaseRow.FilePath;
                rowTag.DecimalSign = dataBaseRow.DecimalSign;
                rowTag.Private = dataBaseRow.Private;
                rowTag.UseCommonDefault = dataBaseRow.UseCommonDefault;
                rowTag.ReadXVariables = string.IsNullOrEmpty(dataBaseRow.ReadXVariables) ? DefPar.Value.NO : dataBaseRow.ReadXVariables;
                rowTag.IsCommonDefaultNull = dataBaseRow.IsUseCommonDefaultNull();
                rowTag.ListStringOutVar = dataBaseRow.ListStringOutVar;
                rowTag.IndirectTaxTableYear = dataBaseRow.IndirectTaxTableYear;

                foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in _dataConfigFacade.GetDBSystemConfigRows(dataBaseRow.ID))
                {
                    ConfigureDataForm.CellTag cellTag = new ConfigureDataForm.CellTag();
                    cellTag.SystemID = dbSystemConfigRow.SystemID;
                    cellTag.UseDefault = dbSystemConfigRow.UseDefault;
                    cellTag.UseCommonDefault = dbSystemConfigRow.UseCommonDefault;
                    cellTag.Uprate = dbSystemConfigRow.Uprate;
                    cellTag.BestMatch = dbSystemConfigRow.BestMatch;
                    rowTag.CellTags.Add(dbSystemConfigRow.SystemID, cellTag);
                }
                configureDataForm._dataBaseInfo.Add(rowTag);
            }

            //... then show the dialog ...
            if (configureDataForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                _actionIsCanceled = true;
                return;
            }

            //... finally store the modified information
            List<DataConfig.DataBaseRow> dataBaseRowsToDelete = new List<DataConfig.DataBaseRow>();
            List<DataConfig.DBSystemConfigRow> dbSystemConfigRowsToDelete = new List<DataConfig.DBSystemConfigRow>();

            foreach (ConfigureDataForm.RowTag dataBaseInfo in configureDataForm._dataBaseInfo)
            {
                DataConfig.DataBaseRow dataBaseRow = _dataConfigFacade.GetDataBaseRow(dataBaseInfo.ID);
                if (dataBaseInfo.ChangeState == ConfigureDataForm.ChangeStates.removed)
                {
                    dataBaseRowsToDelete.Add(dataBaseRow);
                    continue;
                }

                if (dataBaseInfo.ChangeState == ConfigureDataForm.ChangeStates.added)
                {
                    dataBaseRow = _dataConfigFacade.AddDataBaseRow(dataBaseInfo.Name, dataBaseInfo.FilePath);
                    _datasetAdded = true;
                }

                if (dataBaseInfo.ChangeState != ConfigureDataForm.ChangeStates.unchanged)
                {
                    dataBaseRow.ID = dataBaseInfo.ID;
                    dataBaseRow.Name = dataBaseInfo.Name;
                    dataBaseRow.Comment = dataBaseInfo.Comment;
                    dataBaseRow.YearCollection = dataBaseInfo.YearCollection;
                    dataBaseRow.YearInc = dataBaseInfo.YearInc;
                    dataBaseRow.Currency = dataBaseInfo.Currency;
                    dataBaseRow.FilePath = dataBaseInfo.FilePath;
                    dataBaseRow.DecimalSign = dataBaseInfo.DecimalSign;
                    dataBaseRow.Private = dataBaseInfo.Private;
                    dataBaseRow.UseCommonDefault = dataBaseInfo.UseCommonDefault;
                    dataBaseRow.ReadXVariables = dataBaseInfo.ReadXVariables;
                    dataBaseRow.ListStringOutVar = dataBaseInfo.ListStringOutVar;
                    dataBaseRow.IndirectTaxTableYear = dataBaseInfo.IndirectTaxTableYear;
                }

                foreach (ConfigureDataForm.CellTag dbSystemCombination in dataBaseInfo.CellTags.Values)
                {
                    DataConfig.DBSystemConfigRow dbSystemConfigRow = _dataConfigFacade.GetDBSystemConfigRow(dataBaseInfo.ID, dbSystemCombination.SystemID);
                    if (dbSystemCombination.ChangeState == ConfigureDataForm.ChangeStates.removed)
                    {
                        dbSystemConfigRowsToDelete.Add(dbSystemConfigRow);
                        continue;
                    }

                    if (dbSystemCombination.ChangeState == ConfigureDataForm.ChangeStates.added)
                        dbSystemConfigRow = _dataConfigFacade.AddDBSystemConfigRow(dbSystemCombination.SystemID,
                            CountryAdministrator.GetCountryConfigFacade(_countryShortName).GetSystemRowByID(dbSystemCombination.SystemID).Name,
                            dataBaseRow);

                    if (dbSystemCombination.ChangeState != ConfigureDataForm.ChangeStates.unchanged)
                    {
                        dbSystemConfigRow.SystemID = dbSystemCombination.SystemID;
                        dbSystemConfigRow.UseDefault = dbSystemCombination.UseDefault;
                        dbSystemConfigRow.UseCommonDefault = dbSystemCombination.UseCommonDefault;
                        dbSystemConfigRow.Uprate = dbSystemCombination.Uprate;
                        dbSystemConfigRow.BestMatch = dbSystemCombination.BestMatch;
                    }
                }
            }

            foreach (DataConfig.DataBaseRow dataBaseRowToDelete in dataBaseRowsToDelete)
                dataBaseRowToDelete.Delete();
            foreach (DataConfig.DBSystemConfigRow dbSystemConfigRowToDelete in dbSystemConfigRowsToDelete)
                dbSystemConfigRowToDelete.Delete();
        }
    }
}
