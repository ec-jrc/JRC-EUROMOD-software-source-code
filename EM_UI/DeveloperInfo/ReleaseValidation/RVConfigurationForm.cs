using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    public partial class RVConfigurationForm : Form
    {
        internal const short PERFORM_VALIDATION = 0;
        internal const short CREATE_INFO_FILE = 1;
        internal const short COMPARE_VERSIONS = 2;
        internal short selectedFunction = -1;
        internal bool showProblemsOnly = false;

        internal List<string> validationItems = null;
        internal List<string> countries = null;
        internal string infoFilePath = string.Empty;
        internal string compareCountryFolder = string.Empty;

        public RVConfigurationForm(List<string> validationItems)
        {
            InitializeComponent();

            foreach (Country country in CountryAdministrator.GetCountries()) lstCountries.Items.Add(country._shortName);
            btnAllNo_Click(btnAllCountries);

            foreach (string validationItem in validationItems) lstValidationItems.Items.Add(validationItem);
            btnAllNo_Click(btnAllItems);

            txtInfoOutputFolder.Text = EM_AppContext.FolderOutput;
            txtInfoOutputFile.Text = "ReleaseInfo.xlsx";
            txtDataFolder.Text = EM_AppContext.FolderInput;
        }

        void btnValidate_Click(object sender, EventArgs e)
        {
            validationItems = new List<string>(); countries = new List<string>();
            foreach (string validationItem in lstValidationItems.CheckedItems) validationItems.Add(validationItem);
            if (validationItems.Count == 0) { UserInfoHandler.ShowInfo("Please select at least one validation item."); return; }
            foreach (string country in lstCountries.CheckedItems) countries.Add(country);
            if (countries.Count == 0) { UserInfoHandler.ShowInfo("Please select at least one country."); return; }

            selectedFunction = PERFORM_VALIDATION;
            showProblemsOnly = chkShowProblemsOnly.Checked;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close(); // it is important to close this modal dialog otherwise it gets parent of the non-modal dialog showing validation results
                     // with the consequence that the non-modality of the result-dialog would be useless, as this modal dialog does not
                     // allow accessing the UI and if it's closed the non-modal dialog is closed too
                     // explicitly making the main-form parent of the non-modal dialog does not work either, because for whatever reason
                     // one cannot close the main-form anymore once the validation result dialog was started
        }

        void btnCreateInfoFile_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtInfoOutputFolder.Text))
                { UserInfoHandler.ShowError("Please select an existing Output Folder."); return; }
            if (txtInfoOutputFile.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                { UserInfoHandler.ShowError("Output File contains invalid characters."); return; }
            if (!txtInfoOutputFile.Text.ToLower().EndsWith(".xlsx")) txtInfoOutputFile.Text += ".xlsx";
            infoFilePath = Path.Combine(txtInfoOutputFolder.Text, txtInfoOutputFile.Text);
            if (File.Exists(infoFilePath) &&
                UserInfoHandler.GetInfo("Should the existing file be overwritten?", MessageBoxButtons.YesNo) == DialogResult.No) return;
            selectedFunction = CREATE_INFO_FILE;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        void btnSelectInfoOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Please choose the output folder for the info file";
            if (Directory.Exists(txtInfoOutputFolder.Text)) folderBrowser.SelectedPath = txtInfoOutputFolder.Text;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) txtInfoOutputFolder.Text = folderBrowser.SelectedPath;
        }

        void btnAllNo_Click(object sender, EventArgs e = null)
        {
            CheckedListBox box = (sender as Button).Name == btnAllCountries.Name || (sender as Button).Name == btnNoCountries.Name ? lstCountries : lstValidationItems;
            for (int i = 0; i < box.Items.Count; ++i) box.SetItemChecked(i, (sender as Button).Name == btnAllCountries.Name || (sender as Button).Name == btnAllItems.Name);
        }

        void btnCompareVersions_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtFolderVersion.Text)) { UserInfoHandler.ShowError("Please select an existing folder for the version to compare with."); return; }

            compareCountryFolder = EMPath.AddSlash(txtFolderVersion.Text) + EMPath.Folder_Countries_withoutPath();
            if (!Directory.Exists(compareCountryFolder)) { UserInfoHandler.ShowError("Folder '" + txtFolderVersion.Text + "' does not contain folder 'Countries'."); return; }
            
            selectedFunction = COMPARE_VERSIONS;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        void btnSelectFolderVersion_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Please choose the folder containing the version to compare with";
            if (Directory.Exists(txtFolderVersion.Text)) folderBrowser.SelectedPath = txtFolderVersion.Text;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) txtFolderVersion.Text = folderBrowser.SelectedPath;
        }

        private void btnSelectInputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Please choose the folder containing the data files";
            if (Directory.Exists(txtDataFolder.Text)) folderBrowser.SelectedPath = txtDataFolder.Text;
            if (folderBrowser.ShowDialog() == DialogResult.OK) txtDataFolder.Text = folderBrowser.SelectedPath;
        }

        private void btnAddCustomData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = true;
            if (Directory.Exists(txtDataFolder.Text)) openFileDialog.InitialDirectory = txtDataFolder.Text;

            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                foreach (string f in openFileDialog.FileNames) if (!listCustomData.Items.Contains(f)) listCustomData.Items.Add(f);
        }

        private void btnDeleteCustomData_Click(object sender = null, EventArgs e = null)
        {
            List<object> delItems = new List<object>();
            foreach (object d in listCustomData.SelectedItems) delItems.Add(d);
            foreach (object item in delItems) listCustomData.Items.Remove(item);
        }

        private void listCustomData_KeyUp(object sender, KeyEventArgs e) { if (e.KeyCode != Keys.Delete) return; btnDeleteCustomData_Click(); }

        private void btnCheckHHLevel_Click(object sender, EventArgs e)
        {
            List<string> customData = new List<string>(); foreach (string c in listCustomData.Items) customData.Add(c);
            new RVHHLevelVarForm(txtDataFolder.Text, txtDataNamePattern.Text, customData).ShowDialog();
        }
    }
}
