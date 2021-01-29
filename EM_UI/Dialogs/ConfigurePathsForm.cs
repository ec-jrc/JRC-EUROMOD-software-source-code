using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ConfigurePathsForm : Form
    {
        List<string> _pathsUserSettings = null;

        void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            bool reload = false;
            if (EMPath.AddSlash(EM_AppContext.Instance.GetUserSettingsAdministrator().Get().EuromodFolder).ToLower() != EMPath.AddSlash(cmbEuromodFolder.Text).ToLower())
            {
                if (!EM_AppContext.Instance.CloseAnythingOpen()) return; // if there are any countries/add-ons/variables open, try to close them
                reload = true;
            }

            if (!Directory.Exists(cmbEuromodFolder.Text))
            {
                UserInfoHandler.ShowError(cmbEuromodFolder.Text + " is not a valid path.");
                return;
            }

            if (!Directory.Exists(EMPath.AddSlash(cmbEuromodFolder.Text) + EMPath.Folder_Countries_withoutPath()))
            {
                if (UserInfoHandler.GetInfo($"'{cmbEuromodFolder.Text}' does not (yet) contain the {DefGeneral.BRAND_TITLE} file structure."
                     + Environment.NewLine + Environment.NewLine + "Do you want to change the 'Project Folder'?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return;
            }

            if (reload)
            {
                ReloadUserSettings();
                CountryAdministration.CountryAdministrator.ClearCountryList();
                EM_AppContext.Instance.SetBrand(); // allow UI to show another look, i.e. present a brand alternative to EUROMOD
                EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //only the empty main form is open and must be updated
                EM_AppContext.Instance.UpdateMainFormCaption(); //set title (of single open mainform) to "EUROMOD Version (Path)"
                EM_AppContext.Instance.UnloadVarConfigFacade();
                EM_AppContext.Instance.UnloadHICPConfigFacade();
                EM_AppContext.Instance.UnloadExchangeRatesConfigFacade();
                EM_AppContext.Instance.UnloadSwitchablePolicyConfigFacade();
            }

            Close();
        }

        void ReloadUserSettings()
        {
            string pathUserSettings = string.Empty;
            string pathNewProject = EMPath.AddSlash(cmbEuromodFolder.Text).ToLower();
            if (cmbEuromodFolder.Items.Contains(pathNewProject)) //loading a project with existing user-settings
                pathUserSettings = _pathsUserSettings.ElementAt(cmbEuromodFolder.Items.IndexOf(pathNewProject));
            else //loading a 'lose' project, i.e. file-structure on disk, but no user settings available yet
                pathUserSettings = UserSettingsAdministrator.GenerateProjectSettings(pathNewProject);

            EM_AppContext.Instance.StoreViewSettings();
            EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings);
            EM_AppContext.Instance.InitViewKeeper();
        }

        void ConfigurePathsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            cmbEuromodFolder.Text = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().EuromodFolder;
            foreach (string projectPath in UserSettingsAdministrator.GetAvailableProjectPaths(out _pathsUserSettings))
                cmbEuromodFolder.Items.Add(EMPath.AddSlash(projectPath).ToLower());
        }     

        void btnEuromod_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = $"Please choose the Project Folder";
            folderBrowserDialog.SelectedPath = EM_AppContext.FolderEuromodFiles;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                cmbEuromodFolder.Text = folderBrowserDialog.SelectedPath;
        }

        internal ConfigurePathsForm()
        {
            InitializeComponent();
        }
    }
}
