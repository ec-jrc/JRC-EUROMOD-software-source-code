using EM_Common_Win;
using System;
using System.IO;
using System.Windows.Forms;

namespace Web_Statistics
{
    public partial class InputForm : Form
    {
        internal Program Plugin;                    // varialbe that links to the actual plugin

        public InputForm(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;

            // Try reading the starting path from the plugin and pass it to the textbox
            object path = "";
            inputPathEUROMOD.Text = UISessionInfo.GetEuromodFilesFolder();
            if (Plugin.settings.TryGetValue("EuromodFolder", out path))
            {
                inputPathEUROMOD.Text = path.ToString();
 //               inputPathSTATA.Text = Path.Combine(Path.GetDirectoryName(path.ToString()), "Excel_folders");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!checkPaths()) return;
            Plugin.inputPathEUROMOD = inputPathEUROMOD.Text.Trim();
            Plugin.inputPathSTATA = inputPathSTATA.Text.Trim();
            Plugin.outputPath = outputPath.Text.Trim();
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private bool checkPaths()
        {
            if (inputPathEUROMOD.Text.Trim() == "")
            {
                MessageBox.Show("You must specify where the EUROMOD files are stored.");
                return false;
            }
            if (!Directory.Exists(inputPathEUROMOD.Text.Trim()))
            {
                MessageBox.Show("The folder you specified for the EUROMOD files does not exist.");
                return false;
            }
            if (inputPathSTATA.Text.Trim() == "")
            {
                MessageBox.Show("You must specify where the STATA files are stored.");
                return false;
            }
            if (!Directory.Exists(inputPathSTATA.Text.Trim()))
            {
                MessageBox.Show("The folder you specified for the STATA files does not exist.");
                return false;
            }
            if (outputPath.Text.Trim() == "")
            {
                MessageBox.Show("You must specify the name of an existing Excel template for the output file.");
                return false;
            }
//            if (!File.Exists(outputPath.Text.Trim()))
 //           {
  //              MessageBox.Show("The output file you specified does not exist.");
   //             return false;
    //        }
            return true;
        }

        private void btnOpenFolderEUROMOD_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (inputPathEUROMOD.Text.Trim() != "" && Directory.Exists(inputPathEUROMOD.Text.Trim()))
                fbd.SelectedPath = inputPathEUROMOD.Text.Trim();
            fbd.ShowDialog();
            inputPathEUROMOD.Text = fbd.SelectedPath;
        }

        private void btnOpenFolderSTATA_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (inputPathSTATA.Text.Trim() != "" && Directory.Exists(inputPathSTATA.Text.Trim()))
                fbd.SelectedPath = inputPathSTATA.Text.Trim();
            fbd.ShowDialog();
            inputPathSTATA.Text = fbd.SelectedPath;
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {   
            SaveFileDialog sfd = new SaveFileDialog();
            if (outputPath.Text.Trim() != "" && File.Exists(outputPath.Text.Trim()))
                sfd.FileName = outputPath.Text.Trim();
            sfd.Filter = "Excel files|*.xls";
            sfd.DefaultExt = "*.xls";
            sfd.ShowDialog();
            outputPath.Text = sfd.FileName;
        }
    }
}
