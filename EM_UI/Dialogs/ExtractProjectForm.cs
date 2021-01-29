using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    public partial class ExtractProjectForm : Form
    {
        private Dictionary<string, List<string>> ccSystems = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private string projectPath = string.Empty;

        public ExtractProjectForm()
        {
            InitializeComponent();

            foreach (Country c in CountryAdministrator.GetCountries()) lstCountries.Items.Add(c._shortName);
            FillInYears();
        }

        private void FillInYears() { lstSystems.Items.Clear(); for (int y = 2000; y <= 2050; ++y) lstSystems.Items.Add(y.ToString()); }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lstCountries.SelectedItems.Count == 0 && lstSystems.SelectedItems.Count == 0)
                { UserInfoHandler.ShowInfo("Please select the countries and/or systems you want to include into the project."); return; }

            if (txtProjectName.Text == string.Empty || txtProjectPath.Text == string.Empty)
                { UserInfoHandler.ShowError("Please select a valid Project Name and/or Project Path."); return; }
            projectPath = EMPath.AddSlash(EMPath.AddSlash(txtProjectPath.Text) + txtProjectName.Text);
            if (!EM_Helpers.IsValidFileName(projectPath)) { UserInfoHandler.ShowInfo(projectPath + " is not a valid folder name for the new project."); return; }

            Cursor = Cursors.WaitCursor; bool undo = false;
            try
            {
                // first copy the whole EuromodFiles folder to the respective path, to then adapt the copy
                if (!XCopy.Folder(EM_AppContext.FolderEuromodFiles, txtProjectPath.Text, txtProjectName.Text)) { Cursor = Cursors.Default; return; }
                undo = true;

                // delete all unnecessary files and folders (but do not report or stop if any of this fails)
                EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
                DeleteFolder(ReplacePath(emPath.GetFolderLog()));
                ClearFolder(ReplacePath(EM_AppContext.FolderOutput));
                ClearFolder(ReplacePath(emPath.GetFolderTemp()));
                DeleteFile(ReplacePath(Path.Combine(emPath.GetFolderConfig(true), "VersionControl.xml")));
                
                string folderCountries = ReplacePath(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles));
                List<string> selCountries = new List<string>(); foreach (var item in lstCountries.SelectedItems) selCountries.Add(item.ToString().ToLower());
                ClearFolder(folderCountries, selCountries);

                // delete all unnecessary systems
                List<string> selSystems = null;
                if (lstSystems.SelectedItems.Count > 0 && lstSystems.SelectedItems.Count != lstSystems.Items.Count)
                    { selSystems = new List<string>(); foreach (var item in lstSystems.SelectedItems) selSystems.Add(item.ToString().ToLower()); }
                foreach (string cc in selCountries)
                {
                    DeleteFile(EMPath.AddSlash(projectPath + cc) + cc + "_in_use.txt");

                    if (selSystems == null) continue; // if all system/years are selected or nothing is selected, assume that user does not want to "reduce" systems 

                    Country country = new Country(cc);
                    CountryConfigFacade ccf = country.GetCountryConfigFacade(true, folderCountries + country._shortName);
                    DataConfigFacade dcf = country.GetDataConfigFacade(true, folderCountries + country._shortName);

                    List<CountryConfig.SystemRow> delSystems = new List<CountryConfig.SystemRow>();
                    foreach (CountryConfig.SystemRow system in ccf.GetSystemRows())
                    {
                        if (radShowSystems.Checked) { if (!selSystems.Contains(system.Name.ToLower())) delSystems.Add(system); }
                        else
                        {
                            string systemYear = system.Year == null || system.Year == string.Empty ? EM_Helpers.ExtractSystemYear(system.Name) : system.Year;
                            if (!selSystems.Contains(systemYear)) delSystems.Add(system);
                        }
                    }

                    List<DataConfig.DBSystemConfigRow> delDBSysCons = new List<DataConfig.DBSystemConfigRow>();
                    List<string> delSystemIds = (from d in delSystems select d.ID).ToList();
                    foreach (DataConfig.DataBaseRow dataSet in dcf.GetDataBaseRows())
                    {
                        foreach (DataConfig.DBSystemConfigRow dbSystemConfig in dcf.GetDBSystemConfigRows(dataSet.ID))
                            { if (delSystemIds.Contains(dbSystemConfig.SystemID)) delDBSysCons.Add(dbSystemConfig); }
                    }

                    foreach (CountryConfig.SystemRow delSystem in delSystems) delSystem.Delete();
                    foreach (DataConfig.DBSystemConfigRow delDBSysCon in delDBSysCons) delDBSysCon.Delete();

                    country.WriteXML(folderCountries + country._shortName);
                }
                UserInfoHandler.ShowSuccess("Successfully created project folder " + projectPath + ".");
                Close();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowError(exception.Message);
                if (undo) { try { if (Directory.Exists(projectPath)) Directory.Delete(projectPath, true); } catch { } }
            }
            Cursor = Cursors.Default;
        }

        private string ReplacePath(string emSubFolder)
        {
            return EMPath.AddSlash(emSubFolder.Replace(EM_AppContext.FolderEuromodFiles, projectPath));
        }

        private void DeleteFile(string fileName) { try { if (File.Exists(fileName)) File.Delete(fileName); } catch { } }
        private void DeleteFolder(string folderName) { try { if (Directory.Exists(folderName)) Directory.Delete(folderName, true); } catch { } }
        private void ClearFolder(string folderName, List<string> keep = null)
        {
            try
            {
                if (!Directory.Exists(folderName)) return;
                if (keep == null) keep = new List<string>();
                DirectoryInfo folder = new DirectoryInfo(folderName);
                foreach (FileInfo file in folder.GetFiles()) { if (!keep.Contains(file.Name.ToLower())) file.Delete(); }
                foreach (DirectoryInfo subFolder in folder.GetDirectories()) { if (!keep.Contains(subFolder.Name.ToLower())) subFolder.Delete(true); }
            }
            catch (Exception exception)
            {
                if (keep.Count > 0) UserInfoHandler.ShowException(exception, "Failure in deleting not selected countries", false);
            }
        }

        private void lstCountries_SelectedIndexChanged(object sender = null, EventArgs e = null)
        {
            if (radShowYears.Checked) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                lstSystems.Items.Clear();
                foreach (string c in lstCountries.SelectedItems)
                {
                    if (!ccSystems.ContainsKey(c))
                        ccSystems.Add(c, (from s in CountryAdministrator.GetCountryConfigFacade(c).GetSystemRowsOrdered() select s.Name).ToList());
                    foreach (string s in ccSystems[c]) lstSystems.Items.Add(s);
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowError(exception.Message); }
            Cursor = Cursors.Default;
        }

        void radShow_CheckedChanged(object sender, EventArgs e)
        {
            if (radShowSystems.Checked) lstCountries_SelectedIndexChanged();
            else FillInYears();
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please select a folder for storing the Project.";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtProjectPath.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
