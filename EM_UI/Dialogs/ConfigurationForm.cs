using EM_Common;
using EM_UI.Tools;
using EM_UI.TreeListManagement;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ConfigurationForm : Form
    {
        void ConfigurationForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal ConfigurationForm()
        {
            InitializeComponent();

            InitialiseAutoSave();
            InitialiseWarnings();
            InitialiseGeneral();
            InitialiseVersionControl();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!StoreAutoSave()) return;
            if (!StoreVersionControl()) return;
            StoreWarnings();
            StoreGeneral();
            EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(true);
            ViewKeeper.SetKeepMode(chkStoreViewSettings.Checked);

            DialogResult = DialogResult.Cancel;
            Close();
        }


        void InitialiseAutoSave()
        {
            if (EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval <= 0)
            {
                chkAutosave.Checked = false;
                txtAutosaveInterval.Text = string.Empty;
            }
            else
            {
                chkAutosave.Checked = true;
                txtAutosaveInterval.Text = Convert.ToString(Convert.ToInt32(EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval / 60000));
            }
        }

        void InitialiseWarnings()
        {
            Dictionary<string, bool> optionalWarnings = OptionalWarningsManager.GetOptionalWarnings();
            foreach (string optionalWarning in optionalWarnings.Keys)
            {
                int index = lstWarnings.Items.Add(optionalWarning);
                lstWarnings.SetItemChecked(index, optionalWarnings[optionalWarning]);
            }
        }

        void InitialiseGeneral()
        {
            txtOutputFolder.Text = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OutputFolder;
            txtInputFolder.Text = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().InputFolder;
            string closeWithLastMainform = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().CloseInterfaceWithLastMainform;
            chkCloseWithLastCountry.Checked = closeWithLastMainform == string.Empty || closeWithLastMainform == DefPar.Value.YES;
            chkStoreViewSettings.Checked = ViewKeeper.GetKeepMode();
        }

        void InitialiseVersionControl()
        {
            txtProjectName.Text = EM_AppContext.Instance.GetProjectName();
            chkIsVersionControlled.Checked = EM_AppContext.Instance.GetVCAdministrator().IsProjectVersionControlled();
        }

        void StoreGeneral()
        {
            EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OutputFolder = txtOutputFolder.Text;
            EM_AppContext.Instance.GetUserSettingsAdministrator().Get().InputFolder = txtInputFolder.Text;
            EM_AppContext.Instance.GetUserSettingsAdministrator().Get().CloseInterfaceWithLastMainform = chkCloseWithLastCountry.Checked ?
                                                        DefPar.Value.YES : DefPar.Value.NO;
        }

        bool StoreAutoSave()
        {
            if (chkAutosave.Checked && txtAutosaveInterval.Text != string.Empty)
            {
                if (!EM_Helpers.IsNonNegInteger(txtAutosaveInterval.Text))
                {
                    Tools.UserInfoHandler.ShowError("Please insert an integer number for the Autosave Interval.");
                    return false;
                }
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval = EM_Helpers.SaveConvertToInt(txtAutosaveInterval.Text) * 60000;
            }
            else
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval = -1;
            return true;
        }

        void StoreWarnings()
        {
            string optionalWarnings = string.Empty;
            for (int index = 0; index < lstWarnings.Items.Count; ++index)
            {
                if (lstWarnings.GetItemChecked(index))
                    optionalWarnings += '1';
                else
                    optionalWarnings += '0';
            }
            OptionalWarningsManager.SetOptionalWarnings(optionalWarnings);
        }

        bool StoreVersionControl()
        {
            EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCLogInAtProjectLoad = chkLogInAtProjectLoad.Checked;
            return true;
        }

        void btnSelectOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the Output folder";
            folderBrowserDialog.SelectedPath = EM_AppContext.FolderOutput;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtOutputFolder.Text = folderBrowserDialog.SelectedPath;
        }

        void btnSelectInputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the Input folder";
            folderBrowserDialog.SelectedPath = EM_AppContext.FolderInput;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtInputFolder.Text = folderBrowserDialog.SelectedPath;
        }

        void btnSetStandardPaths_Click(object sender, EventArgs e)
        {
            txtOutputFolder.Text = EM_AppContext.FolderEuromodFiles + "output";
            txtInputFolder.Text = EM_AppContext.FolderEuromodFiles + "input";
            txtProjectName.Text = EM_AppContext.Instance.GetProjectName();
        }
    }
}
