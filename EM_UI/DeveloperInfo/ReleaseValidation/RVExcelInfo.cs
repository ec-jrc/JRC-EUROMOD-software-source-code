using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet;
using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.Tools;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVExcelInfo
    {
        class RVCountryInfo
        {
            internal RVCountryInfo(string _country) { country = _country; }
            internal string country;
            internal Dictionary<int, bool> systemYearInfo = new Dictionary<int, bool>(); // key: system-year, value: is private
            internal Dictionary<string, bool> dataInfo = new Dictionary<string, bool>(); // key: data-name, value: is private
            internal List<string> privatePolicies = new List<string>();
            internal List<string> privateFunctions = new List<string>();
            internal List<string> privateParameters = new List<string>();
            internal bool hasTrainingData = false;
            internal bool isTrainingPublic = false;
            internal bool hasHypoData = false;
            internal bool mtrImplemented = false;
            internal bool lmaImplemented = false;
            internal bool nrrImplemented = false;
            internal Dictionary<string, List<string>> dataNA = // key: public data name,
                 new Dictionary<string, List<string>>();       // value: public systems for which data is not available
            internal Dictionary<string, List<string>> switchInfoOn = // key: extension-id
                 new Dictionary<string, List<string>>();             // value: list of DB-Sys-Comb (e.g. AT_2008_A6:AT_2009,AT_2010; AT_2007_A1:AT_2009,AT_2010, ...)
            internal Dictionary<string, List<string>> switchInfoOff = new Dictionary<string, List<string>>(); // as above
            internal Dictionary<string, List<string>> extensionContent = // key: extension-id
                 new Dictionary<string, List<string>>();                 // value: list of pol/fun/par which belong to the extension with info whether they are base-on or base-off [and whether they are private], e.g. 69 TCO_el: on (private); 1.1.28 SetDefault_el/SetDefault/bfapl_s: on
            internal Dictionary<int, string> bestMatchInfo = new Dictionary<int, string>(); // key: system-year, value: best-matching data
            internal Dictionary<string, string> idOrderInfo = new Dictionary<string, string>(); // key pol/fun/par-id, value: pol/fun/par-order
        }

        List<RVCountryInfo> rvCountryInfo = null;
        List<int> systemYears = null; List<int> systemYearsPublic = null;
        List<string> countries = null;
        Dictionary<string, int> globalExtensions = new Dictionary<string, int>(); // key: extension id, value: index of column in Excel-sheet
        Dictionary<string, int> countriesExtensions = new Dictionary<string, int>(); // key: extension id, value: index of column in Excel-sheet
        Dictionary<string, int> allExtensions = new Dictionary<string, int>(); // key: extension id, value: index of column in Excel-sheet

        List<Tuple<string, string, bool>> countriesExtensionInfo = new List<Tuple<string, string, bool>>(); // item1: extension id, item2: extension long name

        const string AVAILABLE = "√";
        const string NOT_AVAILABLE = "x";

        SpreadsheetControl spreadsheetControl = null; IWorkbook workbook = null;
        Worksheet sheetRelease = null, sheetYears = null, sheetDatasets = null, sheetPrivate = null, sheetAddOns = null,
            sheetGlobalExtensions = null, sheetCountriesExtensions = null, sheetExtensionsContent = null, sheetBestMatches = null;
        int colFirstYear = 1, colDataPublic = -1, colData = -1, colTrainingPublic = -1, colTraining = -1, colHypo = -1,
            colDataNA = -1, colPrivData = -1, colPrivSys = -1, colPrivPol = -1, colPrivFun = -1, colPrivPar = -1, colMTR = -1, colLMA = -1, colNRR = -1;

        internal void GenerateInfo(string outputFilePath, List<string> _countries)
        {
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm(); if (mainForm != null) mainForm.Cursor = Cursors.WaitCursor;
            try
            {
                countries = _countries;
                AssessCountryInfo();
                InitExcelFile(outputFilePath);
                FillExcelFile();
                spreadsheetControl.SaveDocument();
                UserInfoHandler.ShowSuccess(string.Format("Successfully generated Release info file '{0}'.", outputFilePath)); // perhaps open ???
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, string.Format("Failed to generate Release info file '{0}'.", outputFilePath), false);
            }
            finally { if (mainForm != null) mainForm.Cursor = Cursors.Default; }
        }

        void AssessCountryInfo()
        {
            rvCountryInfo = new List<RVCountryInfo>();
            systemYears = new List<int>(); systemYearsPublic = new List<int>();

            CountryConfigFacade ccfMTR = CountryAdministrator.GetCountryConfigFacade("MTR");
            CountryConfigFacade ccfLMA = CountryAdministrator.GetCountryConfigFacade("LMA");
            CountryConfigFacade ccfNRR = CountryAdministrator.GetCountryConfigFacade("NRR");

            foreach (string country in countries)
            {
                CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
                DataConfigFacade dcf = CountryAdministrator.GetDataConfigFacade(country);

                RVCountryInfo ccInfo = new RVCountryInfo(country);
                List<int> ccSystemYearsPublic = new List<int>();

                foreach (CountryConfig.SystemRow system in CountryAdministrator.GetCountryConfigFacade(country).GetSystemRows())
                {
                    int year = RVItem_SystemConfiguration.GetSystemYear(system); if (year == -1) continue;
                    bool isPrivate = system.Private == DefPar.Value.YES;
                    if (!ccInfo.systemYearInfo.ContainsKey(year)) ccInfo.systemYearInfo.Add(year, isPrivate);
                    else if (ccInfo.systemYearInfo[year] == true) ccInfo.systemYearInfo[year] = isPrivate; // if there is a public and a private system for the year, let the public dominate
                    if (!systemYears.Contains(year)) systemYears.Add(year);
                    if (!isPrivate) { ccSystemYearsPublic.Add(year); if (!systemYearsPublic.Contains(year)) systemYearsPublic.Add(year); }

                    List<DataConfig.DBSystemConfigRow> bm = (from d in dcf.GetDataConfig().DBSystemConfig
                                                             where d.SystemID == system.ID & d.BestMatch == DefPar.Value.YES
                                                             select d).ToList();
                    if (bm.Count > 0 && !ccInfo.bestMatchInfo.ContainsKey(year)) ccInfo.bestMatchInfo.Add(year, bm.First().DataBaseRow.Name);
                }

                //ccInfo.privateComponents = (from p in ccf.GetPolicyRowsOrderedAndDistinct()
                //                          where p.Private == DefPar.Value.YES select p.Name).ToList();
                int oPol = 0;
                foreach (CountryConfig.PolicyRow pol in ccf.GetPolicyRowsOrderedAndDistinct())
                {
                    ++oPol; int oFun = 0; string privFun = string.Empty;
                    ccInfo.idOrderInfo.Add(pol.ID, $"{oPol}");
                    if (pol.Private == DefPar.Value.YES)
                        { ccInfo.privatePolicies.Add(string.Format("{0} {1}", oPol, pol.Name)); continue; }
                    foreach (CountryConfig.FunctionRow fun in (from f in pol.GetFunctionRows() select f).OrderBy(f => long.Parse(f.Order)))
                    {
                        ++oFun; int oPar = 0; string privPar = string.Empty;
                        ccInfo.idOrderInfo.Add(fun.ID, $"{oPol}.{oFun}");
                        if (fun.Private == DefPar.Value.YES)
                            { privFun += string.Format("{0} {1} ", oFun, fun.Name); continue; }
                        
                        foreach (CountryConfig.ParameterRow par in (from p in fun.GetParameterRows() select p).OrderBy(p => long.Parse(p.Order)))
                        {
                            ++oPar;
                            ccInfo.idOrderInfo.Add(par.ID, $"{oPol}.{oFun}.{oPar}");
                            if (par.Private == DefPar.Value.YES)
                                { privPar += string.Format("{0} {1} ", oPar, par.Name); continue; }
                        }
                        if (privPar != string.Empty) ccInfo.privateParameters.Add(string.Format("{0}.{1} {2}/{3}/...: {4}",
                            oPol.ToString(), oFun.ToString(), pol.Name, fun.Name, privPar));
                    }
                    if (privFun != string.Empty) ccInfo.privateFunctions.Add(string.Format("{0} {1}/...: {2}",
                        oPol, pol.Name, privFun));
                }

                foreach (DataConfig.DataBaseRow data in dcf.GetDataBaseRows())
                {
                    if (ccInfo.dataInfo.ContainsKey(data.Name)) continue;
                    bool isPrivate = data.Private == DefPar.Value.YES;
                    ccInfo.dataInfo.Add(data.Name, isPrivate);
                    if (data.Name.ToLower().StartsWith("training")) { ccInfo.hasTrainingData = true; ccInfo.isTrainingPublic = !isPrivate; }
                    if (data.Name.ToLower().StartsWith("hypo")) ccInfo.hasHypoData = true;

                    if (isPrivate) continue;
                    ccInfo.dataNA.Add(data.Name, new List<string>());

                    if (!data.Name.ToLower().Contains("hhot"))
                    {
                        foreach (int systemYearPublic in ccSystemYearsPublic)
                        {
                            bool isAvailable = false;
                            foreach (DataConfig.DBSystemConfigRow dbs in dcf.GetDBSystemConfigRows(data.ID))
                            {
                                CountryConfig.SystemRow systemRow = CountryAdministrator.GetCountryConfigFacade(country).GetSystemRowByName(dbs.SystemName);
                                if (RVItem_SystemConfiguration.GetSystemYear(systemRow) == systemYearPublic) { isAvailable = true; break; }
                            }
                            if (!isAvailable) ccInfo.dataNA[data.Name].Add(systemYearPublic.ToString());
                        }
                    }
                }

                foreach (string extId in (from s in dcf.GetDataConfig().PolicySwitch select s.SwitchablePolicyID).Distinct())
                {
                    AssessExtensionInfo(ccf, dcf, extId, DefPar.Value.ON, ref ccInfo.switchInfoOn);
                    AssessExtensionInfo(ccf, dcf, extId, DefPar.Value.OFF, ref ccInfo.switchInfoOff);
                }

                AssessExtensionContent(ccf, dcf, ccInfo.idOrderInfo, ref ccInfo.extensionContent);

                foreach (DataConfig.ExtensionRow er in dcf.GetDataConfig().Extension) ;
                foreach (GlobLocExtensionRow er in ExtensionAndGroupManager.GetLocalExtensions(ccInfo.country))
                    countriesExtensionInfo.Add(new Tuple<string, string, bool>(er.ID, er.Name, IsWholeContentPrivate(ccf.GetCountryConfig(), er.ID)));

                ccInfo.mtrImplemented = AddOnImplemented(ccfMTR, country, "mtr");
                ccInfo.lmaImplemented = AddOnImplemented(ccfLMA, country, "lma");
                ccInfo.nrrImplemented = AddOnImplemented(ccfNRR, country, "nrr");

                rvCountryInfo.Add(ccInfo);
            }
            systemYears.Sort(); systemYearsPublic.Sort();
        }

        private void AssessExtensionContent(CountryConfigFacade ccf, DataConfigFacade dcf,
                                            Dictionary<string, string> idOrderInfo, ref Dictionary<string, List<string>> extensionContent)
        {
            extensionContent.Clear();
            foreach (string extId in (from s in dcf.GetDataConfig().PolicySwitch select s.SwitchablePolicyID).Distinct())
            {
                List<string> content = new List<string>();
                foreach (CountryConfig.Extension_PolicyRow extPolRow in from e in ccf.GetCountryConfig().Extension_Policy where e.ExtensionID == extId select e)
                {
                    if (extPolRow.PolicyRow.SystemRow != ccf.GetSystemRows().First()) continue;
                    string order = idOrderInfo.ContainsKey(extPolRow.PolicyID) ? idOrderInfo[extPolRow.PolicyID] : string.Empty;
                    string name = string.IsNullOrEmpty(extPolRow.PolicyRow.ReferencePolID) ? extPolRow.PolicyRow.Name : ccf.GetPolicyRowByID(extPolRow.PolicyRow.ReferencePolID).Name;
                    string description = $"{order} {name}: {(extPolRow.BaseOff ? DefPar.Value.OFF : DefPar.Value.ON)}";
                    if (extPolRow.PolicyRow.Private == DefPar.Value.YES) description += " (private)";
                    content.Add(description);
                }
                foreach (CountryConfig.Extension_FunctionRow extFunRow in from e in ccf.GetCountryConfig().Extension_Function where e.ExtensionID == extId select e)
                {
                    if (extFunRow.FunctionRow.PolicyRow.SystemRow != ccf.GetSystemRows().First()) continue;
                    string order = idOrderInfo.ContainsKey(extFunRow.FunctionID) ? idOrderInfo[extFunRow.FunctionID] : string.Empty;
                    string description = $"{order} {extFunRow.FunctionRow.Name}: {(extFunRow.BaseOff ? DefPar.Value.OFF : DefPar.Value.ON)}";
                    if (extFunRow.FunctionRow.Private == DefPar.Value.YES) description += " (private)";
                    content.Add(description);
                }
                foreach (CountryConfig.Extension_ParameterRow extParRow in from e in ccf.GetCountryConfig().Extension_Parameter where e.ExtensionID == extId select e)
                {
                    if (extParRow.ParameterRow.FunctionRow.PolicyRow.SystemRow != ccf.GetSystemRows().First()) continue;
                    string order = idOrderInfo.ContainsKey(extParRow.ParameterID) ? idOrderInfo[extParRow.ParameterID] : string.Empty;
                    string description = $"{order} {extParRow.ParameterRow.Name}: {(extParRow.BaseOff ? DefPar.Value.OFF : DefPar.Value.ON)}";
                    if (extParRow.ParameterRow.Private == DefPar.Value.YES) description += " (private)";
                    content.Add(description);
                }
                if (content.Any()) extensionContent.Add(extId, content);
            }
        }

        private bool IsWholeContentPrivate(CountryConfig countryConfig, string ID)
        {
            foreach (CountryConfig.PolicyRow polRow in from ep in countryConfig.Extension_Policy where ep.ExtensionID == ID && !ep.BaseOff select ep.PolicyRow)
                if (polRow.Private != DefPar.Value.YES) return false;
            foreach (CountryConfig.FunctionRow funRow in from ef in countryConfig.Extension_Function where ef.ExtensionID == ID && !ef.BaseOff select ef.FunctionRow)
                if (funRow.Private != DefPar.Value.YES) return false;
            foreach (CountryConfig.ParameterRow parRow in from ep in countryConfig.Extension_Parameter where ep.ExtensionID == ID && !ep.BaseOff select ep.ParameterRow)
                if (parRow.Private != DefPar.Value.YES) return false;
            return true;
        }

        private static void AssessExtensionInfo(CountryConfigFacade ccf, DataConfigFacade dcf,
                                                string extensionId, string offOn, ref Dictionary<string, List<string>> switchInfo)
        {
            Dictionary<string, List<string>> dataSwitches = new Dictionary<string, List<string>>();
            foreach (DataConfig.PolicySwitchRow polSwitch in from e in dcf.GetDataConfig().PolicySwitch where e.SwitchablePolicyID == extensionId select e)
            {
                if (polSwitch.Value != offOn) continue;
                if (!dataSwitches.ContainsKey(polSwitch.DataBaseID)) dataSwitches.Add(polSwitch.DataBaseID, new List<string>());
                dataSwitches[polSwitch.DataBaseID].Add(polSwitch.SystemID);
            }
            foreach (var dataSwitch in dataSwitches)
            {
                DataConfig.DataBaseRow dataRow = dcf.GetDataBaseRow(dataSwitch.Key); if (dataRow == null) continue;
                string sumDataSwitches = dataRow.Name + ":";
                foreach (string sysId in dataSwitch.Value)
                {
                    CountryConfig.SystemRow sysRow = ccf.GetSystemRowByID(sysId); if (sysRow == null) continue;
                    sumDataSwitches += sysRow.Name + ",";
                }
                if (!switchInfo.ContainsKey(extensionId)) switchInfo.Add(extensionId, new List<string>());
                switchInfo[extensionId].Add(RVItem_Base.TrimEnd(sumDataSwitches));
            }
        }

        bool AddOnImplemented(CountryConfigFacade ccfAO, string country, string prefix)
        {
            if (ccfAO == null) return false;
            if (ccfAO.GetSystemRowByName(prefix + "_" + country) != null) return true;

            return (from p in ccfAO.GetCountryConfig().Parameter
                    where p.FunctionRow.PolicyRow.SystemRow.Name.ToLower() == prefix.ToLower() & // note: this system (i.e. LMA) does not exist for LMA
                          p.FunctionRow.PolicyRow.Name.ToLower().StartsWith(ExeXml.AddOn.POL_AO_CONTROL.ToLower()) &
                          p.FunctionRow.Name.ToLower() == DefFun.AddOn_Applic.ToLower() &
                          p.Name.ToLower() == DefPar.AddOn_Applic.Sys.ToLower() &
                          p.Value.ToLower().StartsWith(country.ToLower() + "_")
                    select p).ToList().Count > 0;
        }

        void InitExcelFile(string outputFilePath)
        {
            // copy the template (stored in resources) to the selected path
            File.WriteAllBytes(outputFilePath, Properties.Resources.ReleaseInfo1);
            spreadsheetControl = new SpreadsheetControl();
            if (!spreadsheetControl.LoadDocument(outputFilePath)) throw new Exception("Failed to load template Excel file.");
            workbook = spreadsheetControl.Document;

            string errMess = "Copying template failed - template does not have the expected format.";
            if (workbook.Worksheets.Count != 9) throw new Exception(errMess);
            sheetRelease = workbook.Worksheets[0];
            sheetYears = workbook.Worksheets[1];
            sheetDatasets = workbook.Worksheets[2];
            sheetPrivate = workbook.Worksheets[3];
            sheetAddOns = workbook.Worksheets[4];
            sheetGlobalExtensions = workbook.Worksheets[5];
            sheetCountriesExtensions = workbook.Worksheets[6];
            sheetExtensionsContent = workbook.Worksheets[7];
            sheetBestMatches = workbook.Worksheets[8];

            InsertYearColumns(sheetRelease, systemYearsPublic, out colDataPublic);
            int dummy; InsertYearColumns(sheetYears, systemYears, out dummy);
            InsertYearColumns(sheetBestMatches, systemYears, out dummy);

            InsertExtensionsColumns(true, false);
            InsertExtensionsColumns(false, true);
            InsertExtensionsColumns(true, true);

            colTrainingPublic = colDataPublic + 1;
            colData = 1; colTraining = 2; colHypo = 3; colDataNA = 4;
            colPrivData = 1; colPrivSys = 2; colPrivPol = 3; colPrivFun = 4; colPrivPar = 5; 
            colMTR = 1; colLMA = 2; colNRR = 3;
        }

        void InsertExtensionsColumns(bool global, bool countrySpecific)
        {
            Worksheet sheetExtensions = global & countrySpecific ? sheetExtensionsContent : global ? sheetGlobalExtensions : sheetCountriesExtensions;
            Dictionary<string, int> extensions = global & countrySpecific ? allExtensions : global ? globalExtensions : countriesExtensions;

            int colSwitches = 1;
            List<Tuple<string, string>> ext = new List<Tuple<string, string>>();
            if (global)
            {
                foreach (GlobLocExtensionRow gle in ExtensionAndGroupManager.GetGlobalExtensions()) ext.Add(new Tuple<string, string>(gle.ID, gle.Name));
            }
            if (countrySpecific)
            {
                foreach(Tuple<string, string, bool> e in countriesExtensionInfo)
                ext.Add(new Tuple<string, string>(e.Item1, $"{e.Item2}{(e.Item3 ? " (whole content private)" : string.Empty)}"));
            }

            for (int c = 0; c < ext.Count - 2; ++c)
            {
                int cIns = colSwitches + 1 + c;
                sheetExtensions.Columns[cIns].Insert(InsertCellsMode.ShiftCellsRight);
                sheetExtensions.Columns[cIns].CopyFrom(sheetExtensions.Columns[colSwitches]); // to overtake the format
            }
            int cLabel = -1;
            for (int c = 0; c < ext.Count; ++c)
            {
                cLabel = colSwitches + c;
                sheetExtensions[1, cLabel].Value = ext[c].Item2;
                extensions.Add(ext[c].Item1, cLabel);
            }
        }

        void InsertYearColumns(Worksheet sheet, List<int> years, out int colData)
        {
            for (int c = 0; c < years.Count - 2; ++c)
            {
                int cIns = colFirstYear + 1 + c;
                sheet.Columns[cIns].Insert(InsertCellsMode.ShiftCellsRight);
                sheet.Columns[cIns].CopyFrom(sheet.Columns[colFirstYear]); // to overtake the format
            }
            int cLabel = -1;
            for (int c = 0; c < years.Count; ++c)
            {
                cLabel = colFirstYear + c;
                sheet[1, cLabel].Value = years[c];
            }
            colData = cLabel + 1;
        }

        void FillExcelFile()
        {
            int row = 2;
            foreach (RVCountryInfo ccInfo in rvCountryInfo)
            {
                // fill country name in all sheets
                for (int i = 0; i < workbook.Worksheets.Count; ++i)
                    workbook.Worksheets[i].Cells[row, 0].Value = ccInfo.country;

                // fill year info in release-, years, and best-matches-sheets
                int col = colFirstYear;
                foreach (int y in systemYears)
                {
                    sheetYears.Cells[row, col].Value = ccInfo.systemYearInfo.ContainsKey(y) ? AVAILABLE : NOT_AVAILABLE;
                    sheetBestMatches.Cells[row, col].Value = ccInfo.bestMatchInfo.ContainsKey(y) ? ccInfo.bestMatchInfo[y] : string.Empty;
                    ++col;
                }
                col = colFirstYear;
                foreach (int y in systemYearsPublic)
                {
                    sheetRelease.Cells[row, col].Value = ccInfo.systemYearInfo.ContainsKey(y) && !ccInfo.systemYearInfo[y] ? AVAILABLE : NOT_AVAILABLE;
                    ++col;
                }

                // fill data info in release- and datasets-sheets
                string allData = string.Empty, pubData = string.Empty, privData = string.Empty, allDataNA = string.Empty;
                foreach (string dataName in ccInfo.dataInfo.Keys)
                {
                    if (!dataName.ToLower().StartsWith("training") && !dataName.ToLower().StartsWith("hypo")) allData += dataName + ", ";
                    if (ccInfo.dataInfo[dataName]) privData += dataName + ", ";
                    else if (!dataName.ToLower().StartsWith("training")) pubData += dataName + ", ";
                }
                sheetDatasets.Cells[row, colData].Value = RVItem_Base.TrimEnd(allData);
                sheetRelease.Cells[row, colDataPublic].Value = RVItem_Base.TrimEnd(pubData);

                sheetDatasets.Cells[row, colTraining].Value = ccInfo.hasTrainingData ? AVAILABLE : NOT_AVAILABLE;
                sheetDatasets.Cells[row, colHypo].Value = ccInfo.hasHypoData ? AVAILABLE : NOT_AVAILABLE;
                sheetRelease.Cells[row, colTrainingPublic].Value = ccInfo.isTrainingPublic ? AVAILABLE : NOT_AVAILABLE;

                foreach (var dataNA in ccInfo.dataNA)
                {
                    if (dataNA.Value.Count == 0) continue;
                    allDataNA += dataNA.Key + ": ";
                    foreach (string sys in dataNA.Value) allDataNA += sys + ",";
                    allDataNA = RVItem_Base.TrimEnd(allDataNA) + Environment.NewLine;
                }
                sheetDatasets.Cells[row, colDataNA].Value = RVItem_Base.TrimEnd(allDataNA);

                // fill private info in private-sheet
                string privSys = string.Empty;
                foreach (int y in ccInfo.systemYearInfo.Keys) if (ccInfo.systemYearInfo[y]) privSys += y + ", ";
                string privPol = string.Empty, privFun = string.Empty, privPar = string.Empty;
                foreach (string p in ccInfo.privatePolicies) privPol += p + Environment.NewLine;
                foreach (string f in ccInfo.privateFunctions) privFun += f + Environment.NewLine;
                foreach (string p in ccInfo.privateParameters) privPar += p + Environment.NewLine;
                sheetPrivate.Cells[row, colPrivData].Value = privData == string.Empty ? "-" : RVItem_Base.TrimEnd(privData);
                sheetPrivate.Cells[row, colPrivSys].Value = privSys == string.Empty ? "-" : RVItem_Base.TrimEnd(privSys);
                sheetPrivate.Cells[row, colPrivPol].Value = privPol == string.Empty ? "-" : RVItem_Base.TrimEnd(privPol);
                sheetPrivate.Cells[row, colPrivFun].Value = privFun == string.Empty ? "-" : RVItem_Base.TrimEnd(privFun);
                sheetPrivate.Cells[row, colPrivPar].Value = privPar == string.Empty ? "-" : RVItem_Base.TrimEnd(privPar);

                // fill add-on info in addons-sheet
                sheetAddOns.Cells[row, colMTR].Value = ccInfo.mtrImplemented ? AVAILABLE : NOT_AVAILABLE;
                sheetAddOns.Cells[row, colLMA].Value = ccInfo.lmaImplemented ? AVAILABLE : NOT_AVAILABLE;
                sheetAddOns.Cells[row, colNRR].Value = ccInfo.nrrImplemented ? AVAILABLE : NOT_AVAILABLE;

                // fill extension info in extension-sheets
                for (bool global = true; ; global = false)
                {
                    for (bool on = true; ; on = false)
                    {
                        Dictionary<string, int> extensions = global ? globalExtensions : countriesExtensions;
                        Worksheet sheetExtensions = global ? sheetGlobalExtensions : sheetCountriesExtensions;
                        foreach (var switchInfo in on ? ccInfo.switchInfoOn : ccInfo.switchInfoOff)
                        {
                            if (!extensions.ContainsKey(switchInfo.Key)) continue;
                            int iCol = extensions[switchInfo.Key];
                            string dbSysSwitches = (on ? "default-switch = on:" : Environment.NewLine + "default-switch = off:") + Environment.NewLine;
                            foreach (var dbSysSwitch in switchInfo.Value) dbSysSwitches += dbSysSwitch + Environment.NewLine;
                            sheetExtensions.Cells[row, iCol].Value += RVItem_Base.TrimEnd(dbSysSwitches);
                        }
                        if (on == false) break;
                    }
                    if (global == false) break;
                }

                // fill extensions-content info in extensions-content-sheet
                foreach (var extension in allExtensions)
                    if (ccInfo.extensionContent.ContainsKey(extension.Key))
                        sheetExtensionsContent.Cells[row, extension.Value].Value = string.Join(Environment.NewLine, ccInfo.extensionContent[extension.Key]);

                ++row;
            }
        }
    }
}
