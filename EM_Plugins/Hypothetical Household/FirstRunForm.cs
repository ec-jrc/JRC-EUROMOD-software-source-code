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
    public partial class FirstRunForm : Form
    {
        private bool generateExample = false;

        public FirstRunForm()
        {
            InitializeComponent();
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                Description = "Please choose the HHOT project folder",
                SelectedPath = txtProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtProjectPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProjectPath.Text) || !Directory.Exists(txtProjectPath.Text))
            {
                MessageBox.Show("Please indicate a valid Project Path.");
                return;
            }
            generateExample = false;
            DialogResult = DialogResult.OK;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            generateExample = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        internal void GetInfo(out string projectPath, out bool generateExample)
        {
            projectPath = txtProjectPath.Text;
            generateExample = this.generateExample;
        }
    }
}
