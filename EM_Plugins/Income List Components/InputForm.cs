using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Income_List_Components
{
    public partial class InputForm : Form
    {
        internal Program Plugin;                    // varialbe that links to the actual plugin

        public InputForm(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;

            // read the starting path from the plugin and pass it to the textbox
            inputPath.Text = Plugin.euromodFilesPath;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Plugin.dataPath = inputPath.Text;
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Please select the EUROMOD Data File folder.";
                folderBrowserDialog.SelectedPath = Plugin.euromodFilesPath;
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    inputPath.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
