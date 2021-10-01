using EM_Common;
using EM_Common_Win;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StatisticsPresenter
{
    public partial class SelectBaseAltsForm : Form
    {
        internal Template.TemplateInfo templateInfo = null;
        public List<FilePackageContent> filePackages = null;
        List<int> selectedIndices = new List<int>();    // store the order of selection for Alts

        public SelectBaseAltsForm(Template.TemplateInfo templateInfo, string outputFolder = null)
        {
            InitializeComponent();

            this.templateInfo = templateInfo;

            labCaption.Text = templateInfo.name;

            // try to retieve selected paths from UI-user-settings, if not successful set to UI-output-folder
            if (outputFolder == null)
            {
                string storedBaseOutputFolder = UISessionInfo.GetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER);
                textBasePath.Text = string.IsNullOrEmpty(storedBaseOutputFolder) ? UISessionInfo.GetOutputFolder() : storedBaseOutputFolder;
                string storedAltOutputFolder = UISessionInfo.GetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.REFORM_OUTPUT_FOLDER);
                textAltPath.Text = string.IsNullOrEmpty(storedAltOutputFolder) ? textBasePath.Text : storedAltOutputFolder;
            }
            else textBasePath.Text = textAltPath.Text = outputFolder;

            //template.FilePackageDefinition.GetMinMaxNumberOfAlternatives(out minAlts, out maxAlts, template.TemplateType);
            if (templateInfo.maxFiles == 1)
            {
                listAlt.SelectionMode = SelectionMode.One;
                lblMultiSelect.Visible = false;
                lblOrderRetained.Visible = false;
            }
            else listAlt.SelectionMode = SelectionMode.MultiExtended;
        }

        private void btnSelectBasePath_Click(object sender, EventArgs e) { SelectPath(false); }
        private void btnSelectAltPath_Click(object sender, EventArgs e) { SelectPath(true); }
        private void SelectPath(bool alt)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = alt ? textAltPath.Text : textBasePath.Text;
            folderBrowserDialog.Description = string.Format("Please select the folder containing the {0}.",
                alt ? "alternative-scenario(s) file(s)" : "base-scenario file");
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (alt) textAltPath.Text = folderBrowserDialog.SelectedPath; else textBasePath.Text = folderBrowserDialog.SelectedPath;
            if (MessageBox.Show(string.Format("Should the path for the {0} be changed as well?",
                        alt ? "base scenario" : "alternative scenarios"), string.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
                if (alt) textBasePath.Text = textAltPath.Text; else textAltPath.Text = textBasePath.Text;
        }

        private void textBasePath_TextChanged(object sender, EventArgs e) { FillFileList(false); }
        private void textAltPath_TextChanged(object sender, EventArgs e) { FillFileList(true); }

        private void FillFileList(bool alt)
        {
            ListBox box = alt ? listAlt : listBase;
            string filePattern = "*.txt"; //TODO alt ? template.FilePackageDefinition.AltFileNamePattern : template.FilePackageDefinition.BaseFileNamePattern;
            string path = alt ? textAltPath.Text : textBasePath.Text;

            box.Items.Clear();
            if (Directory.Exists(path))
                foreach (string filePath in Directory.GetFiles(path, filePattern))
                    if (SelectFilesForm.isValidInput(filePath, templateInfo)) box.Items.Add((new FileInfo(filePath)).Name);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBase.SelectedIndex < 0)
                { MessageBox.Show("Please select a file for the base scenario."); return; }

            int selCount = listAlt.SelectedIndices.Count;
            if (selCount < templateInfo.minFiles || selCount > templateInfo.maxFiles)
            {
                if (templateInfo.maxFiles == 1) MessageBox.Show("Please select a file for the alternative scenario."); // is the same as selCount=0 (as list is in single-selection mode)
                else if (templateInfo.minFiles == templateInfo.maxFiles) MessageBox.Show(string.Format("Please select {0} files for the alternative scenarios.", templateInfo.maxFiles));
                else if (templateInfo.maxFiles == int.MaxValue) MessageBox.Show(string.Format("Please select at least {0} files for the alternative scenarios.", templateInfo.minFiles));
                else MessageBox.Show(string.Format("Please select {0} to {1} files for the alternative scenarios.", templateInfo.minFiles, templateInfo.maxFiles));
                return;
            }

            FilePackageContent filePackage = new FilePackageContent()
                { PathBase = Path.Combine(textBasePath.Text, listBase.SelectedItem.ToString()) };
            foreach (string altFile in selectedIndices.Select(i => listAlt.Items[i]))  // get alt selections in order
                filePackage.PathsAlt.Add(Path.Combine(textAltPath.Text, altFile));

            // in fact the dialog always returns exactly one file-package, the list is just for consistency-reasons with SelectFilesForm and SelectTemplateForm
            filePackages = new List<FilePackageContent>() { filePackage };

            // save selected paths in UI-user-settings (see description in SelectFileForm.cs )
            if (!EMPath.IsSamePath(textBasePath.Text.ToLower(), UISessionInfo.GetOutputFolder().ToLower()))
                UISessionInfo.SetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER, textBasePath.Text);
            else UISessionInfo.RemoveRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.BASE_OUTPUT_FOLDER);
            if (!EMPath.IsSamePath(textAltPath.Text.ToLower(), UISessionInfo.GetOutputFolder().ToLower()))
                UISessionInfo.SetRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.REFORM_OUTPUT_FOLDER, textAltPath.Text);
            else UISessionInfo.RemoveRetainedUserSetting(StatisticsPresenter.USER_SETTINGS_ID, StatisticsPresenter.REFORM_OUTPUT_FOLDER);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void lblMissingFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This template requires tab-delimited text files that contain at least the following columns: " + Environment.NewLine + string.Join(Environment.NewLine, templateInfo.GetRequiredVariables()));
        }

        private void listAlt_SizeChanged(object sender, EventArgs e)
        {
            lblMultiSelect.Top = listAlt.Top + listAlt.Height;
            lblMultiSelect.Left = listAlt.Left + listAlt.Width - lblMultiSelect.Width;
            lblOrderRetained.Top = lblMultiSelect.Top + lblMultiSelect.Height;
            lblOrderRetained.Left = lblMultiSelect.Left + lblMultiSelect.Width - lblMultiSelect.Width;
        }

        private void listAlt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listAlt.SelectedIndex > -1)
            {
                selectedIndices.AddRange(listAlt.SelectedIndices.OfType<int>()
                                                 .Except(selectedIndices));
                List<int> removeItems = new List<int>();
                foreach (int x in selectedIndices) if (!listAlt.SelectedIndices.Contains(x)) removeItems.Add(x);
                foreach (int x in removeItems) selectedIndices.Remove(x);
            }
            listAlt.Refresh();    // to make sure that the DrawItem is called after the index chagned
        }

        private void listAlt_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            if (listAlt.SelectedIndices.Contains(e.Index) && selectedIndices.Contains(e.Index))
                e.Graphics.DrawString("{" + selectedIndices.IndexOf(e.Index) + "} " + listAlt.Items[e.Index], e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            else
                e.Graphics.DrawString(listAlt.Items[e.Index].ToString(), e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            e.DrawFocusRectangle();
        }
    }
}
