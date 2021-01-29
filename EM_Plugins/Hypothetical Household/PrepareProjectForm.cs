using System;
using System.IO;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class PrepareProjectForm : Form
    {
        private string projectPath = null;

        public PrepareProjectForm(string heading)
        {
            InitializeComponent();
            Name = heading;
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                Description = "Please choose the folder that should contain the HHOT project",
                SelectedPath = txtProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtProjectPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtProjectPath.Text) || string.IsNullOrEmpty(txtProjectName.Text))
                {
                    MessageBox.Show("Please indicate a valid Project Path and Name.");
                    return;
                }
                if (Directory.Exists(Path.Combine(txtProjectPath.Text, txtProjectName.Text)))
                {
                    MessageBox.Show("Please indicate a valid Project Path and Name. Project must not yet exist.");
                    return;
                }
                projectPath = Path.Combine(txtProjectPath.Text, txtProjectName.Text);
                Directory.CreateDirectory(projectPath);
            }
            catch(Exception exception)
            {
                MessageBox.Show("Failed to prepare HHOT project:" + Environment.NewLine + exception.Message);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        internal string GetProjectPath()
        {
            return projectPath;
        }
    }
}
