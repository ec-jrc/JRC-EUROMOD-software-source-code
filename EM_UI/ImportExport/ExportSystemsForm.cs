using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    public partial class ExportSystemsForm : Form
    {
        internal List<string> _selectedSystems = null;
        internal string _selectedPath = string.Empty;
        internal bool _deleteExportedSystems = false;

        internal ExportSystemsForm(List<string> systemNames)
        {
            InitializeComponent();

            lstSystems.Items.Clear();
            foreach (string systemName in systemNames)
                lstSystems.Items.Add(systemName);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (lstSystems.CheckedItems.Count == 0)
            {
                Tools.UserInfoHandler.ShowError("Please select at least one system.");
                return;
            }
            _selectedSystems = new List<string>();
            foreach (object selectedSystem in lstSystems.CheckedItems)
                _selectedSystems.Add(selectedSystem.ToString());

            if (txtExportFolder.Text == string.Empty || !System.IO.Directory.Exists(txtExportFolder.Text))
            {
                Tools.UserInfoHandler.ShowError("Please select a valid export path.");
                return;
            }
            _selectedPath = EMPath.AddSlash(txtExportFolder.Text);

            if (radExportAndDelete.Checked)
                _deleteExportedSystems = true;

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnSelectExportFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = EM_AppContext.FolderEuromodFiles;
            folderBrowserDialog.Description = "Please select the export folder";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            txtExportFolder.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
