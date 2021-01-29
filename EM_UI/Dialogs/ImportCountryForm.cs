using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ImportCountryForm : Form
    {
        internal ImportCountryForm()
        {
            InitializeComponent();
        }

        internal string GetImportCountryFolder() { return txtCountryFolder.Text; }
        internal string GetCountryShortName() { return txtShortName.Text; }
        internal string GetFlagPathAndFileName() { return txtFlag.Text; }
        internal bool GetAdaptGlobal() { return chkAdaptGlobal.Checked; }

        void ImportCountryForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtCountryFolder.Text))
            {
                UserInfoHandler.ShowError("Import Country Folder does not exist.");
                return;
            }

            if (Directory.Exists(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + txtShortName.Text))
            {
                UserInfoHandler.ShowError("Country folder '" + txtShortName.Text + "' already exists at '" + EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) +  "'.");
                return;
            }

            if (txtFlag.Text != string.Empty && !CountryAdministrator.IsValidFlagFilePath(txtFlag.Text))
                return;

            //out-commented as it does not really make sense to not allow for longer country-short-names
            //if (txtShortName.Text.Length != 2)
            //{
            //    if (UserInfoHandler.GetInfo("Short Name is supposed to have two characters. Do you want to correct?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //        return;
            //}

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCountryFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the Country folder";
            folderBrowserDialog.SelectedPath = EM_AppContext.FolderEuromodFiles;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtCountryFolder.Text = folderBrowserDialog.SelectedPath;
        }

        void txtCountryFolder_TextChanged(object sender, EventArgs e)
        {
            txtShortName.Text = Directory.Exists(txtCountryFolder.Text) ? new DirectoryInfo(txtCountryFolder.Text).Name : string.Empty;
        }

        void btnFlag_Click(object sender, EventArgs e)
        {
            txtFlag.Text = CountryAdministrator.ShowFlagSelectDialog(txtFlag.Text);
        }
    }
}
