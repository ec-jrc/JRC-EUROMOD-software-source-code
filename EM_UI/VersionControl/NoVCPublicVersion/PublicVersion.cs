using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VersionControl
{
    internal partial class PublicVersion
    {
        static string _publicVersionPath = string.Empty;
        static string _publicVersionNumber = string.Empty;

        static internal void Generate()
        {
            //get the necessary information from the user (path and version number)
            PublicVersionForm publicVersionForm = new PublicVersionForm();
            if (publicVersionForm.ShowDialog() == DialogResult.Cancel)
                return;
            _publicVersionPath = EMPath.AddSlash(publicVersionForm.GetPath());
            _publicVersionNumber = publicVersionForm.GetVersionNumber();

            ProgressIndicator progressIndicator = new ProgressIndicator(Generate_BackgroundEventHandler, //the handler passed to the progress indicator will do the work (see below)
                                                                        "Generating Public Version");
            if (progressIndicator.ShowDialog() == System.Windows.Forms.DialogResult.OK) //regular termination, i.e user did not cancel the procedure
                UserInfoHandler.ShowSuccess("Pulic version successfully stored at '" + _publicVersionPath + ".'" + Environment.NewLine +
                                         "Change user interface's current project (via the menu item 'Open Project') to check the result.");
        }

        static void Generate_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            //assess the name of the new EuromodFiles-folder in accordance to the version number
            DirectoryInfo sourceFolder = new DirectoryInfo(EM_AppContext.FolderEuromodFiles);
            string folderEMF = "EuromodFiles_" + _publicVersionNumber;
            if (!EM_Helpers.IsValidFileName(folderEMF))
            {
                UserInfoHandler.ShowInfo(folderEMF + " is not a valid folder name. Please change the version number.");
                e.Cancel = true; return;
            }

            //first copy the whole EuromodFiles folder to the respective path
            if (!XCopy.Folder(EM_AppContext.FolderEuromodFiles, _publicVersionPath, folderEMF)) { e.Cancel = true; return; }

            string fullPublicPath = _publicVersionPath + EMPath.AddSlash(folderEMF);

            //then adapt the copy
            string folderCountries = 
                EMPath.AddSlash( //at the new path assess the folder that contains the files (usually EuromodFiles)
                EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles).Replace(EM_AppContext.FolderEuromodFiles, fullPublicPath));
            try
            {
                List<Country> countries = CountryAdministrator.GetCountries();

                //remove private systems, policies and datasets of each country
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                    Country country = countries[i];
                    CountryConfigFacade countryConfigFacade = country.GetCountryConfigFacade(true, folderCountries + country._shortName);
                    DataConfigFacade dataConfigFacade = country.GetDataConfigFacade(true, folderCountries + country._shortName);

                    //assess which systems, policies and datasets are private
                    List<CountryConfig.SystemRow> privateSystems = new List<CountryConfig.SystemRow>(); //systems
                    List<CountryConfig.PolicyRow> privatePolicies = new List<CountryConfig.PolicyRow>(); //policies
                    List<CountryConfig.FunctionRow> privateFunctions = new List<CountryConfig.FunctionRow>(); //functions
                    List<CountryConfig.ParameterRow> privateParameters = new List<CountryConfig.ParameterRow>(); //parameters
                    List<string> privateSystemIDs = new List<string>(); //necessary for afterwards identifying database-connections of private systems
                    foreach (CountryConfig.SystemRow system in countryConfigFacade.GetSystemRows())
                    {
                        if (system.Private.ToLower() == DefPar.Value.YES.ToLower())
                        {
                            privateSystems.Add(system);
                            privateSystemIDs.Add(system.ID);
                        }
                        else
                        {
                            foreach (CountryConfig.PolicyRow policy in system.GetPolicyRows())
                            {
                                if (policy.Private == DefPar.Value.YES)
                                    privatePolicies.Add(policy);
                                else
                                {
                                    if (policy.PrivateComment != null && policy.PrivateComment != string.Empty)
                                        policy.PrivateComment = string.Empty; //remove private policy-comment if there is any
                                    foreach (CountryConfig.FunctionRow function in policy.GetFunctionRows())
                                    {
                                        if (function.Private == DefPar.Value.YES)
                                            privateFunctions.Add(function);
                                        else
                                        {
                                            if (function.PrivateComment != null && function.PrivateComment != string.Empty)
                                                function.PrivateComment = string.Empty; //remove private function-comment if there is any
                                            foreach (CountryConfig.ParameterRow parameter in function.GetParameterRows())
                                            {
                                                if (parameter.Private == DefPar.Value.YES)
                                                    privateParameters.Add(parameter);
                                                else if (parameter.PrivateComment != null && parameter.PrivateComment != string.Empty)
                                                    parameter.PrivateComment = string.Empty; //remove private parameter-comment if there is any
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    List<DataConfig.DataBaseRow> privateDataSets = new List<DataConfig.DataBaseRow>(); //datasets
                    List<DataConfig.DBSystemConfigRow> privateDBSystemConfigs = new List<DataConfig.DBSystemConfigRow>(); //database-connections of private systems
                    foreach (DataConfig.DataBaseRow dataSet in dataConfigFacade.GetDataBaseRows())
                    {
                        if (dataSet.Private.ToLower() == DefPar.Value.YES.ToLower())
                            privateDataSets.Add(dataSet);
                        else
                        {
                            foreach (DataConfig.DBSystemConfigRow dbSystemConfig in dataConfigFacade.GetDBSystemConfigRows(dataSet.ID))
                            {
                                if (privateSystemIDs.Contains(dbSystemConfig.SystemID))
                                    privateDBSystemConfigs.Add(dbSystemConfig);
                            }
                        }
                    }

                    //remove user-set node colors
                    countryConfigFacade.RemoveAllNodeColors();

                    //restore or install default base-system-colouring
                    countryConfigFacade.setAutomaticConditionalFormatting(true);

                    //remove private systems
                    if (countryConfigFacade.GetCountryRow().Private == DefPar.Value.YES || //if country is private or
                        privateSystems.Count == countryConfigFacade.GetSystemRows().Count) //there are no systems left, delete country
                    {
                        Directory.Delete(folderCountries + country._shortName, true);
                        country.SetCountryConfigFacade(null);
                        country.SetDataConfigFacade(null);
                        continue;
                    }
                    else //otherwise delete private systems
                    {
                        foreach (CountryConfig.SystemRow privateSystem in privateSystems)
                            privateSystem.Delete();
                    }

                    //remove private parameters
                    foreach (CountryConfig.ParameterRow privateParameter in privateParameters)
                        privateParameter.Delete();

                    //remove private functions
                    foreach (CountryConfig.FunctionRow privateFunction in privateFunctions)
                        privateFunction.Delete();

                    //remove private policies
                    foreach (CountryConfig.PolicyRow privatePolicy in privatePolicies)
                        privatePolicy.Delete();

                    //remove private datasets
                    foreach (DataConfig.DataBaseRow privateDataSet in privateDataSets)
                        privateDataSet.Delete();

                    //remove database-connections of private systems
                    foreach (DataConfig.DBSystemConfigRow privateDBSystemConfig in privateDBSystemConfigs)
                        privateDBSystemConfig.Delete();

                    country.WriteXML(folderCountries + country._shortName);
                    country.SetCountryConfigFacade(null);
                    country.SetDataConfigFacade(null);

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 80.0));
                }
                
                //remove private add-ons
                string folderAddOns = EMPath.AddSlash( //at the new path assess the folder that contains the files (usually EuromodFiles)
                EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles).Replace(EM_AppContext.FolderEuromodFiles, fullPublicPath));
                foreach (Country addOn in CountryAdministrator.GetAddOns())
                {
                    bool oldStyle = CountryAdministrator.ConsiderOldAddOnFileStructure(true);
                    CountryConfigFacade addOnConfigFacade = addOn.GetCountryConfigFacade(true, folderAddOns + (oldStyle ? string.Empty : addOn._shortName));
                    if (addOnConfigFacade.GetCountryRow().Private != DefPar.Value.YES)
                        continue;
                    if (oldStyle)
                        File.Delete(folderAddOns + addOn._shortName + ".xml");
                    else
                        Directory.Delete(folderAddOns + addOn._shortName, true);
                    addOn.SetCountryConfigFacade(null);
                }

                // remove the "other" column from the variables file
                string pathVarConfig = new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true).Replace(EM_AppContext.FolderEuromodFiles, fullPublicPath);
                VarConfigFacade vcf = new VarConfigFacade(pathVarConfig);
                if (vcf.LoadVarConfig())
                {
                    foreach (VarConfig.CountryLabelRow r in
                        from l in vcf._varConfig.CountryLabel where l.Country.ToLower() == "other" select l)
                        r.Delete();
                    vcf.Commit(); vcf.WriteXML(pathVarConfig);
                }

                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                //change version number
                string txtVersionPath = EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "EuromodVersion.txt";
                txtVersionPath = txtVersionPath.Replace(EM_AppContext.FolderEuromodFiles, fullPublicPath);
                using (StreamWriter versionFile = new StreamWriter(txtVersionPath))
                {
                    versionFile.WriteLine(_publicVersionNumber);
                    versionFile.WriteLine("PUBLIC VERSION");
                }

                //remove private rows from log file
                string logFile = new EMPath(EM_AppContext.FolderEuromodFiles).GetEmLogFilePath(); // determine the path of the em_log-file in the public folder
                logFile = logFile.Replace(EM_AppContext.FolderEuromodFiles, fullPublicPath);
                backgroundWorker.ReportProgress(100);
                if (File.Exists(logFile)) AdaptLogFile(logFile);

                //take care to not have any "xx_in_use.txt" files in the release
                try
                {
                    foreach (string inUseFile in Directory.GetFiles(fullPublicPath, "*_in_use.txt", SearchOption.AllDirectories))
                        File.Delete(inUseFile);
                }
                catch (Exception exception)
                {
                    //do nothing if this fails
                    UserInfoHandler.RecordIgnoredException("PublicVersion.Generate_BackgroundEventHandler", exception);
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                e.Cancel = true; //stop the process and allow progress indicator to set dialog result to Cancel
            }
        }

        internal static void CleanLogFile()
        {
            string logFile = new EMPath(EM_AppContext.FolderEuromodFiles).GetEmLogFilePath();
            
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();
            if (mainForm != null) mainForm.Cursor = Cursors.WaitCursor;
            string error = AdaptLogFile(logFile);
            if (mainForm != null) mainForm.Cursor = Cursors.Default;

            if (error == string.Empty) UserInfoHandler.ShowSuccess("Successfully cleaned " + logFile + ".");
            else UserInfoHandler.ShowError("Failed to clean " + logFile + ":" + Environment.NewLine + error);
        }

        private static string AdaptLogFile(string logFile)
        {

            try
            {
                // search for the worksheet to adapt
                ExcelPackage excel = new ExcelPackage(new FileInfo(logFile));
                ExcelWorksheet worksheet = null;
                foreach (ExcelWorksheet ws in excel.Workbook.Worksheets)
                    if (ws.Name.ToLower().Contains("current")) { worksheet = ws; break; }
                if (worksheet == null) return "Worksheet 'current' not found.";

                // delete any other worksheets
                for (int w = excel.Workbook.Worksheets.Count; w > 0; --w)
                    if (worksheet.Name != excel.Workbook.Worksheets[w].Name) excel.Workbook.Worksheets.Delete(w);

                // search for the column that indicates whether changes are public (probably headed "Public change: 1=yes, 0=no")
                int pubCol = -1;
                for (int c = 1; c < 100; ++c)
                    if (worksheet.Cells[1, c].Value != null &&
                        worksheet.Cells[1, c].Value.ToString().ToLower().Contains("public change")) { pubCol = c; break; }
                if (pubCol == -1) return "Column 'public change' not found in worksheet 'current'.";

                // delete rows which contain non-public changes
                List<int> rowsToDelete = new List<int>(); int emptyCount = 0;
                for (int r = 2; r < 100000; ++r)
                {
                    if (worksheet.Cells[r, 1].Value == null || worksheet.Cells[r, 1].Value.ToString() == string.Empty) ++emptyCount;
                    if (emptyCount >= 5) break; // assuming there are not 5 empty rows after each other but this is actually the end of the list
                    if (worksheet.Cells[r, pubCol].Value == null || worksheet.Cells[r, pubCol].Value.ToString().Trim() != "1") rowsToDelete.Add(r);
                }

                for (int d = rowsToDelete.Count-1; d >= 0; --d) worksheet.DeleteRow(rowsToDelete[d]);

                // finally delete the column containing the public-information
                worksheet.DeleteColumn(pubCol);

                // rename from e.g. "F2.30-current" to something neutral
                worksheet.Name = Path.GetFileNameWithoutExtension(EMPath.FILE_EMLOG);

                // move focus to top
                worksheet.Select("A2");

                excel.Save();
                excel.Dispose();
                return string.Empty;
            }
            catch (Exception exception) { return exception.Message; }
        }
    }
}