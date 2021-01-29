using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.VersionControl
{
    public partial class PublicVersionForm : Form
    {
        private string path = string.Empty;
        private string versionNumber = string.Empty;

        public PublicVersionForm()
        {
            InitializeComponent();
        }

        void PublicVersionForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal string GetPath()
        {
            return path;
        }
        
        internal string GetVersionNumber()
        {
            return versionNumber;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please select a folder for storing the Public Release.";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtPath.Text == string.Empty || !Directory.Exists(txtPath.Text)) { Tools.UserInfoHandler.ShowError("Please specify a valid path."); return; }
            path = txtPath.Text;

            if (txtVersionNumber.Text == string.Empty) { Tools.UserInfoHandler.ShowError("Please specify a Version Number."); return; }
            versionNumber = txtVersionNumber.Text;
            
            DialogResult = DialogResult.OK;
        }
    }
}
