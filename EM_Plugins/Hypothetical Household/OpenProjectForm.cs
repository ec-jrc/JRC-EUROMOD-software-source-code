using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class OpenProjectForm : Form
    {
        private string projectPath = null;
        public OpenProjectForm()
        {
            InitializeComponent();
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                Description = "Please choose the folder that contains the HHOT project",
                SelectedPath = txtProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtProjectPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtProjectPath.Text))
                {
                    MessageBox.Show("Please indicate a valid Project Path and Name.");
                    return;
                }
                if (!Directory.Exists(txtProjectPath.Text))
                {
                    MessageBox.Show("Please indicate a valid Project Path and Name. Project must not yet exist.");
                    return;
                }

                projectPath = txtProjectPath.Text;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to open HHOT project:" + Environment.NewLine + exception.Message);
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
