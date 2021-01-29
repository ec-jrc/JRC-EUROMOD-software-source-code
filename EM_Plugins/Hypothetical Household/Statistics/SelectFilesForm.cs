using EM_Common_Win;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    internal partial class SelectFilesForm : Form
    {
        private Template.TemplateInfo templateInfo = null;
        private List<string> requiredVariables = null;

        internal List<SimpleStatistics.HHTypes_Files> hhTypes_files = null;

        public SelectFilesForm(Template.TemplateInfo _templateInfo)
        {
            InitializeComponent();

            templateInfo = _templateInfo;
            requiredVariables = templateInfo.GetRequiredVariables();
            requiredVariables.Add(DataGenerator.HHTYPE_NAME);
            labCaption.Text = templateInfo.name;
            textPath.Text = UISessionInfo.GetOutputFolder();
        }

        private void SelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { SelectedPath = textPath.Text, Description = "Please select the folder containing the files." };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void textPath_TextChanged(object sender, EventArgs e)
        {
            listFiles.Items.Clear();
            if (Directory.Exists(textPath.Text))
                foreach (string filePath in Directory.GetFiles(textPath.Text, "*.txt"))
                    if (IsValidInput(filePath)) listFiles.Items.Add(Path.GetFileName(filePath));
        }

        private bool IsValidInput(string filePath)
        {
            string[] headers = null;
            try
            {
                string firstLine = File.ReadLines(filePath).FirstOrDefault();
                if (firstLine == null || firstLine.Length < 1) return false;
                headers = firstLine.ToLower().Split('\t');
            }
            catch (IOException) { return false; } // if the file is locked (e.g. open in Excel) ignore it
            catch (Exception ex) { MessageBox.Show($"{filePath}: unexpected error: '{ex.Message}'"); return false; }

            foreach (string rv in requiredVariables) if (!headers.Contains(rv.ToLower())) return false;
            return true;
        }

        private void lblMissingFiles_Clicked(object sender, EventArgs e)
        {
            MessageBox.Show("Files need to be tab-delimited and contain at least the following columns: " + Environment.NewLine + string.Join(Environment.NewLine, requiredVariables));
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listFiles.CheckedItems.Count == 0) { MessageBox.Show("Please select at least one file"); return; }

            // remark: it would maybe be more "tidy" to do the following in the SimpleStatistics-class, but then we cannot show a wait-cursor
            Cursor = Cursors.WaitCursor;
            string errors = Environment.NewLine;
            Dictionary<string, SimpleStatistics.HHTypes_Files> hhtfs = new Dictionary<string, SimpleStatistics.HHTypes_Files>();
            foreach (string fileName in listFiles.CheckedItems)
            {
                try
                {
                    string filePath = Path.Combine(textPath.Text, fileName);
                    var lines = File.ReadLines(filePath);
                    List<string> headers = lines.First().ToLower().Split('\t').ToList();
                    int indexHHTypeID = headers.IndexOf(DataGenerator.HHTYPE_ID), indexHHTypeName = headers.IndexOf(DataGenerator.HHTYPE_NAME);
                    string curHHTypeID = null;
                    foreach (string line in lines.Skip(1))
                    {
                        string[] vars = line.Split('\t');
                        if (curHHTypeID != vars[indexHHTypeID])
                        {
                            curHHTypeID = vars[indexHHTypeID];
                            if (!hhtfs.ContainsKey(curHHTypeID)) hhtfs.Add(curHHTypeID,
                                new SimpleStatistics.HHTypes_Files() { hhTypeID = curHHTypeID, hhTypeName = vars[indexHHTypeName] });
                            if (!hhtfs[curHHTypeID].files.Contains(filePath)) hhtfs[curHHTypeID].files.Add(filePath);
                        }
                    }
                }
                catch (Exception exception) { errors += $"{fileName}: {exception.Message}" + Environment.NewLine; }
            }
            hhTypes_files = hhtfs.Values.ToList();
            Cursor = Cursors.Default;

            if (errors.Trim() != string.Empty && MessageBox.Show($"The following file(s) showed errors on analysing their content:{errors.TrimEnd()}",
                                                                 string.Empty, MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
