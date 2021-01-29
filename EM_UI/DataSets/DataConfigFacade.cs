using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_UI.DataSets
{
    internal class DataConfigFacade
    {
        DataConfig _dataConfig = null;

        internal DataConfigFacade() { } //standard constructor

        internal DataConfigFacade(DataConfig dataConfig) //special constructor to copy a country (see RunMainForm)
        {
            _dataConfig = dataConfig.Copy() as DataConfig;
        }

        internal void WriteXml(string filePath, string fileName, bool saveWithLineBreaks = true)
        {
            Stream fileStream = new FileStream(EMPath.AddSlash(filePath) + fileName, FileMode.Create);
            using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream,
                DefGeneral.DEFAULT_ENCODING, CountryConfigFacade._cDataElements, saveWithLineBreaks))
                _dataConfig.WriteXml(xmlWriter);
        }

        internal DataConfig ReadXml(string filePath, string fileName)
        {
            _dataConfig = new DataConfig();
            using (StreamReader streamReader = new StreamReader(EMPath.AddSlash(filePath) + fileName, DefGeneral.DEFAULT_ENCODING))                                                                
                _dataConfig.ReadXml(streamReader);
            _dataConfig.AcceptChanges();
            return _dataConfig;
        }

        internal static void AddDataConfigAndGenerateXML(string filePath, string fileName, bool saveWithLineBreaks = true)
        {
            DataConfig dataConfig = new DataConfig();
            Stream fileStream = new FileStream(EMPath.AddSlash(filePath) + fileName, FileMode.Create);
            using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING,
                                                    CountryConfigFacade._cDataElements, saveWithLineBreaks))
                dataConfig.WriteXml(xmlWriter);
        }

        internal DataConfig GetDataConfig()
        {
            return _dataConfig;
        }

        internal List<DataConfig.DataBaseRow> GetDataBaseRows()
        {
            return (from dataBase in _dataConfig.DataBase select dataBase).ToList();
        }

        internal DataConfig.DataBaseRow GetDataBaseRow(string ID)
        {
            List<DataConfig.DataBaseRow> dataBaseRows = (from dataBase in GetDataBaseRows() where dataBase.ID == ID select dataBase).ToList<DataConfig.DataBaseRow>();
            return (dataBaseRows.Count == 1) ? dataBaseRows.First() : null;
        }

        internal DataConfig.DataBaseRow GetDataBaseRowByDataFile(string filePath, string fileName)
        {
            List<DataConfig.DataBaseRow> dataBaseRows = (from dataBase in _dataConfig.DataBase
                                                         where EMPath.AddSlash(dataBase.FilePath).ToLower() == EMPath.AddSlash(filePath).ToLower() &&
                                                               AddExtensionToFileName(dataBase.Name, "txt").ToLower() == AddExtensionToFileName(fileName, "txt").ToLower()
                                                         select dataBase).ToList();
            if (dataBaseRows.Count > 0)
                return dataBaseRows.First();
            return null;

            string AddExtensionToFileName(string fn, string extension)
            {
                if (extension.Count() > 0 && !extension.StartsWith(".")) extension = "." + extension;
                return fn.Count() >= extension.Count() && fn.ToLower().EndsWith(extension.ToLower()) ? fn : fn + extension;
            }
        }

        internal List<DataConfig.DBSystemConfigRow> GetDBSystemConfigRows(string dataBaseID)
        {
            return (from dbSystemConfig in _dataConfig.DBSystemConfig where dbSystemConfig.DataBaseID == dataBaseID select dbSystemConfig).ToList();
        }

        internal DataConfig.DBSystemConfigRow GetDBSystemConfigRow(string dataBaseID, string systemID)
        {
            List<DataConfig.DBSystemConfigRow> dbSystemConfigRows = (from dbSystemConfig in _dataConfig.DBSystemConfig
                                                                     where dbSystemConfig.DataBaseID == dataBaseID && dbSystemConfig.SystemID == systemID
                                                                     select dbSystemConfig).ToList<DataConfig.DBSystemConfigRow>();
            return (dbSystemConfigRows.Count == 1) ? dbSystemConfigRows.First() : null;
        }

        internal DataConfig.DataBaseRow AddDataBaseRow(string fileName, string filePath)
        {
            return _dataConfig.DataBase.AddDataBaseRow(Guid.NewGuid().ToString(), fileName,
                                                        string.Empty, //Comment
                                                        string.Empty, //YearCollection
                                                        string.Empty, //YearInc
                                                        DefPar.Value.EURO,
                                                        filePath, ".",
                                                        DefPar.Value.NO, //Private
                                                        DefPar.Value.NO, //UseCommonDefault
                                                        DefPar.Value.NO, //ReadXVariables
                                                        string.Empty,    //ListStringOutVar
                                                        string.Empty);   //IndirectTaxTableYear
        }

        internal DataConfig.DBSystemConfigRow AddDBSystemConfigRow(string systemID, string sysName, DataConfig.DataBaseRow dataBaseRow)
        {
            return _dataConfig.DBSystemConfig.AddDBSystemConfigRow(systemID, sysName, dataBaseRow,
                                                                    string.Empty, //UseDefault
                                                                    DefPar.Value.NO, //UseCommonDefault: parameter transferred to database-level, can be deleted once not used in any cc_DataConfig.xml anymore
                                                                    string.Empty, //Uprate
                                                                    DefPar.Value.NO); //BestMatch
        }

        static internal DataConfig.DataBaseRow CopyDataBaseRowFromAnotherCountry(DataConfig dataConfig, DataConfig.DataBaseRow originalDataBaseRow, bool keepID = false)
        {
            return dataConfig.DataBase.AddDataBaseRow(keepID ? originalDataBaseRow.ID : Guid.NewGuid().ToString(),
                                                        originalDataBaseRow.Name,
                                                        originalDataBaseRow.Comment,
                                                        originalDataBaseRow.YearCollection,
                                                        originalDataBaseRow.YearInc,
                                                        originalDataBaseRow.Currency,
                                                        originalDataBaseRow.FilePath,
                                                        originalDataBaseRow.DecimalSign,
                                                        originalDataBaseRow.Private,
                                                        originalDataBaseRow.UseCommonDefault,
                                                        originalDataBaseRow.ReadXVariables,
                                                        originalDataBaseRow.ListStringOutVar,
                                                        originalDataBaseRow.IndirectTaxTableYear);
        }

        static internal DataConfig.DBSystemConfigRow OvertakeDBSystemConfigRowFromAnotherCountry(DataConfig dataConfig,
                                                     DataConfig.DBSystemConfigRow originalDBSystemConfigRow,
                                                     DataConfig.DataBaseRow newDataBaseRow, CountryConfig.SystemRow newSystemRow, bool overtakeExtensionSwitches = true)
        {
            DataConfig.DBSystemConfigRow dbSystemConfigRow = dataConfig.DBSystemConfig.AddDBSystemConfigRow(newSystemRow.ID, newSystemRow.Name, newDataBaseRow,
                                                                    originalDBSystemConfigRow.UseDefault,
                                                                    originalDBSystemConfigRow.UseCommonDefault,
                                                                    originalDBSystemConfigRow.Uprate,
                                                                    originalDBSystemConfigRow.BestMatch);
            if (overtakeExtensionSwitches)
                foreach (DataConfig.PolicySwitchRow extensionSwitchRow in originalDBSystemConfigRow.GetPolicySwitchRows())
                    dataConfig.PolicySwitch.AddPolicySwitchRow(extensionSwitchRow.SwitchablePolicyID, newSystemRow.ID, newDataBaseRow.ID, extensionSwitchRow.Value);

            return dbSystemConfigRow;
        }

        internal void DeleteDBSystemConfigRows(string systemID)
        {
            foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in
                                                (from dbSystemConfig in _dataConfig.DBSystemConfig
                                                 where dbSystemConfig.SystemID == systemID
                                                 select dbSystemConfig))
                dbSystemConfigRow.Delete();
        }

        internal void RenameSystemInDBSystemConfigRows(string systemID, string newName)
        {
            foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in
                                                (from dbSystemConfig in _dataConfig.DBSystemConfig
                                                 where dbSystemConfig.SystemID == systemID
                                                 select dbSystemConfig))
                dbSystemConfigRow.SystemName = newName;
        }

        internal void CopyDBSystemConfigRows(CountryConfig.SystemRow originalSystemRow, CountryConfig.SystemRow copySystemRow)
        {
            foreach (DataConfig.DBSystemConfigRow originalDBSystemConfigRow in
                                                (from dbSystemConfig in _dataConfig.DBSystemConfig
                                                 where dbSystemConfig.SystemID == originalSystemRow.ID
                                                 select dbSystemConfig).ToList())
            {
                DataConfig.DBSystemConfigRow copyDBSystemConfigRow = _dataConfig.DBSystemConfig.AddDBSystemConfigRow(copySystemRow.ID, copySystemRow.Name, originalDBSystemConfigRow.DataBaseRow,
                                                                originalDBSystemConfigRow.UseDefault, originalDBSystemConfigRow.UseCommonDefault, originalDBSystemConfigRow.Uprate, //parameter transferred to database-level, can be deleted once not used in any cc_DataConfig.xml anymore
                                                                originalDBSystemConfigRow.BestMatch);

                foreach (DataConfig.PolicySwitchRow originalExtensionSwitchRow in originalDBSystemConfigRow.GetPolicySwitchRows())
                    _dataConfig.PolicySwitch.AddPolicySwitchRow(originalExtensionSwitchRow.SwitchablePolicyID,
                                copySystemRow.ID, originalExtensionSwitchRow.DataBaseID, originalExtensionSwitchRow.Value);
            }
        }

        internal void RegisterWithUndoManager(ADOUndoManager undoMananger)
        {
            undoMananger.AddDataSet(_dataConfig);
        }

        internal List<string> GetSystemsNamesDistinctAndOrdered(string countryShortName)
        {
            CountryConfigFacade ccF = CountryAdministration.CountryAdministrator.GetCountryConfigFacade(countryShortName);
            Dictionary<string, CountryConfig.SystemRow> systemRows = new Dictionary<string, CountryConfig.SystemRow>();
            foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in _dataConfig.DBSystemConfig.Rows)
            {
                if (systemRows.ContainsKey(dbSystemConfigRow.SystemID)) continue;
                CountryConfig.SystemRow systemRow = ccF.GetSystemRowByID(dbSystemConfigRow.SystemID);
                systemRows.Add(systemRow.ID, systemRow);
            }
            return (from s in systemRows.Values orderby long.Parse(s.Order) select s.Name).ToList();
        }

        internal List<DataConfig.DBSystemConfigRow> GetDBSystemConfigRowsBySystem(string systemName)
        {
            return (from dbSystemConfig in _dataConfig.DBSystemConfig
                    where dbSystemConfig.SystemName.ToLower() == systemName.ToLower()
                    select dbSystemConfig).ToList();
        }
    }
}
