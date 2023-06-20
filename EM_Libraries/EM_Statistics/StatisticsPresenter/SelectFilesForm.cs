using EM_Common;
using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_Statistics.StatisticsPresenter
{
    public partial class SelectFilesForm : Form
    {
        internal Template.TemplateInfo templateInfo = null;
        public List<FilePackageContent> filePackages = null;
        readonly List<int> selectedIndices = new List<int>();    // store the order of selection for Alts

        public SelectFilesForm(Template.TemplateInfo templateInfo)
        {
            InitializeComponent();

            this.templateInfo = templateInfo;

            labCaption.Text = templateInfo.name;

            // try to retieve selected path from UI-user-settings, if not successful set to UI-output-folder
            string storedBaseOutputFolder = UISessionInfo.GetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER);
            textPath.Text = string.IsNullOrEmpty(storedBaseOutputFolder) ? UISessionInfo.GetOutputFolder() : storedBaseOutputFolder;

            if (templateInfo.maxFiles == 1)
            {
                listFiles.SelectionMode = SelectionMode.One;
                lblMultiSelect.Visible = false;
                lblOrderRetained.Visible = false;
            }
            else listFiles.SelectionMode = SelectionMode.MultiExtended;
        }

        private void SelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { SelectedPath = textPath.Text, Description = "Please select the folder containing the files." };
            if (Path.GetFileName(folderBrowserDialog.SelectedPath).Contains("*") || Path.GetFileName(folderBrowserDialog.SelectedPath).Contains("?")) folderBrowserDialog.SelectedPath = Path.GetDirectoryName(folderBrowserDialog.SelectedPath);
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void textPath_TextChanged(object sender, EventArgs e)
        {
            string filePattern = "*.txt";
            string path = textPath.Text;

            if (Path.GetFileName(path).Contains("*") || Path.GetFileName(path).Contains("?"))
            {
                filePattern = Path.GetFileName(path) + ".txt";
                path = Path.GetDirectoryName(path);
            }

            listFiles.Items.Clear();
            if (Directory.Exists(path))
                foreach (string filePath in Directory.GetFiles(path, filePattern))
                    if (isValidInput(filePath, templateInfo)) listFiles.Items.Add((new FileInfo(filePath)).Name);
            btnNoSuitableFiles.Visible = listFiles.Items.Count == 0;
            lblMissingFiles.Visible = listFiles.Items.Count > 0;
        }

        private void list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listFiles.SelectionMode == SelectionMode.One)
                listFiles.SelectedIndex = listFiles.IndexFromPoint(e.Location);
            btnOK_Click(null, null);
        }

        internal static bool isValidInput(string filePath, Template.TemplateInfo template)
        {
            // Check if this is a valid output file containing all the required fields for this template
            string[] headers = null;
            try
            {
                string firstLine = File.ReadLines(filePath).FirstOrDefault();
                if (firstLine == null || firstLine.Length < 1) return false;    // ignore empty files
                headers = firstLine.ToLower().Split('\t'); // get the header line from the file - assume that the file is Tab-separated and first row is headers
            }
            catch (IOException)
            {
                return false;   // if the file is locked (e.g. open in Excel) ignore it
            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + " has returned the unexpected error: '" + ex.Message + "'");
                return false;   // unexpected error
            }
            string missing = string.Empty;
            // check all required fields to see if they exist as headers
            foreach (string rf in template.GetRequiredVariables())
                if (!headers.Contains(rf.ToLower())) missing += Environment.NewLine + rf;
            // if not, return false and keep an error message 
            if (missing != string.Empty) missing = "The following variables are missing from \"" + filePath + "\":" + missing;
            return missing == string.Empty;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int selCount = listFiles.SelectedIndices.Count;
            if (selCount < templateInfo.minFiles || selCount > templateInfo.maxFiles)
            {
                if (templateInfo.maxFiles == 1) MessageBox.Show("Please select a file."); // is the same as selCount=0 (as list is in single-selection mode)
                else if (templateInfo.minFiles == templateInfo.maxFiles) MessageBox.Show(string.Format("Please select {0} files.", templateInfo.maxFiles));
                else if (templateInfo.maxFiles == int.MaxValue) MessageBox.Show(string.Format("Please select at least {0} files.", templateInfo.minFiles));
                else MessageBox.Show(string.Format("Please select {0} to {1} files.", templateInfo.minFiles, templateInfo.maxFiles));
                return;
            }

            filePackages = new List<FilePackageContent>();
            foreach (var file in selectedIndices.Select(i => listFiles.Items[i]))
            {
                string fileFullPath = Path.Combine(textPath.Text, file.ToString());
                if (templateInfo.templateType == HardDefinitions.TemplateType.Default)
                    filePackages.Add(new FilePackageContent() { PathBase = fileFullPath });
                else // MULTI_ALT
                {
                    if (filePackages.Count == 0) filePackages.Add(new FilePackageContent());
                    filePackages[0].PathsAlt.Add(fileFullPath);
                }
            }

            // save selected paths in UI-user-settings (see EM_UI.PlugInService.SessionInfo.cs, about why not using an own settings-file)
            if (!EMPath.IsSamePath(textPath.Text.ToLower(), UISessionInfo.GetOutputFolder().ToLower())) // if the path was changed: save in user-settings ...
                UISessionInfo.SetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER, textPath.Text);
            else // ... if path corresponds with default-output-folder: remove from user-settings, to avoid that it will remain, if user changes the default-path in project settings
                UISessionInfo.RemoveRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnNoSuitableFiles_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This template requires tab-delimited text files that contain at least the following columns: " + Environment.NewLine + string.Join(Environment.NewLine, templateInfo.GetRequiredVariables()));
        }

        private void lblMissingFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This template requires tab-delimited text files that contain at least the following columns: " + Environment.NewLine + string.Join(Environment.NewLine, templateInfo.GetRequiredVariables()));
        }

        private void listFiles_SizeChanged(object sender, EventArgs e)
        {
            btnNoSuitableFiles.Width = listFiles.Width;
            btnNoSuitableFiles.Top = listFiles.Top + listFiles.Height - btnNoSuitableFiles.Height;
            lblMultiSelect.Top = listFiles.Top + listFiles.Height;
            lblMultiSelect.Left = listFiles.Left + listFiles.Width - lblMultiSelect.Width;
            lblOrderRetained.Top = lblMultiSelect.Top + lblMultiSelect.Height;
            lblOrderRetained.Left = lblMultiSelect.Left + lblMultiSelect.Width - lblMultiSelect.Width;
        }

        private void listFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listFiles.SelectedIndex > -1)
            {
                selectedIndices.AddRange(listFiles.SelectedIndices.OfType<int>()
                                                 .Except(selectedIndices));
                List<int> removeItems = new List<int>();
                foreach (int x in selectedIndices) if (!listFiles.SelectedIndices.Contains(x)) removeItems.Add(x);
                foreach (int x in removeItems) selectedIndices.Remove(x);
            }
            listFiles.Refresh();    // to make sure that the DrawItem is called after the index chagned
        }

        private void listFiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            if (listFiles.SelectedIndices.Contains(e.Index) && selectedIndices.Contains(e.Index))
                e.Graphics.DrawString("{" + selectedIndices.IndexOf(e.Index) + "} " + listFiles.Items[e.Index], e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            else
                e.Graphics.DrawString(listFiles.Items[e.Index].ToString(), e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            e.DrawFocusRectangle();
        }
    }
}
