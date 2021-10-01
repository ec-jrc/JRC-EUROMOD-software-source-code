using EM_Common;
using EM_UI.Tools;
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
using VCUIAPI;
using static EM_UI.VersionControl.Dialogs.VCNewProject;

namespace EM_UI.VersionControl.Dialogs
{
    public partial class VCDownloadLatestBundle : Form
    {
        VCAdministrator _vcAdministrator = null;
        List<ProjectNode> _projectInfos = new List<ProjectNode>();
        long _projectId;
        long _releaseId;
        ProjectNode _projectNode = null;
        ReleaseInfo _releaseInfo = null;
        ProjectContent _projectContent = null;
        string _bundleName;

        internal VCDownloadLatestBundle(VCAdministrator vcAdministrator)
        {
            _vcAdministrator = vcAdministrator;
            InitializeComponent();

            if (!_vcAdministrator._vcAPI.GetProjectList(out _projectInfos, false))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage()); return;
            }

            List<VersionControlUnitInfo> units;
            
            foreach (ProjectNode projectInfo in _projectInfos)
            {
                if (projectInfo.Id == _vcAdministrator._vcAPI.vc_projectInfo.ProjectId)

                {
                    _projectNode = projectInfo;
                    textProject.Text = projectInfo.Name; //select the current project
                    _projectId = projectInfo.Id;


                    if (!_vcAdministrator._vcAPI.GetLatestReleaseUnitInfo(projectInfo.Id, out _releaseInfo))
                    {
                        UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage()); Cursor = Cursors.Default; return;
                    }

                    if (_releaseInfo != null)
                    {
                        _releaseId = _releaseInfo.Id;
                        _bundleName = _releaseInfo.Name;
                        txtBundle.Text = _releaseInfo.Name + " - " + _releaseInfo.Date.ToShortDateString();  
                    }

                    AssessVCBaseProjectContent(out _projectContent);
                    units = _projectContent.projectUnits;

                    foreach (VersionControlUnitInfo unit in units)
                    {
                        ListViewItem item = listUnits.Items.Add(string.Format("{0} ({1})", unit.Name, VCContentControl.UnitTypeToString(unit.UnitType)));
                        item.ToolTipText = unit.Name;
                        item.Checked = true;
                        item.Tag = unit;                     
                        if(unit.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG)
                            {
                            item.ForeColor = SystemColors.GrayText;
                            item.BackColor = SystemColors.InactiveBorder;
                        }
                    }
                }

                

            }

            
        }




        private void btnDownload_Click(object sender, EventArgs e)
        {
            
            if (txtDestinationFolder.Text.Equals(String.Empty)){ UserInfoHandler.ShowInfo("Please indicate a valid 'Project Path' for dowloading the latest online bundle."); return; }
            else if (!Directory.Exists(txtDestinationFolder.Text)){ UserInfoHandler.ShowInfo("Please indicate an existing 'Project Path' for storing the latest online bundle."); return; }

            string newEuromodFolder = EMPath.AddSlash(txtDestinationFolder.Text) + textProject.Text + "_" + _bundleName;
            if (Directory.Exists(newEuromodFolder) && (Directory.GetFiles(newEuromodFolder).Count() > 0 || Directory.GetDirectories(newEuromodFolder).Count() > 0))
            { UserInfoHandler.ShowError(string.Format("Folder '{0}' exists and is not empty.\n\nThe file structure for the new project requires an empty base-folder.", newEuromodFolder)); return; }

            _projectContent.selections = GetChoices();
            DialogResult = System.Windows.Forms.DialogResult.OK;

            Close();
        }

        internal void GetInfo(out string projectPath, out string projectName, out long projectId, out string releaseName, out long releaseId, out ProjectContent projectContent)
        {
            projectPath = txtDestinationFolder.Text;
            projectName = textProject.Text;
            projectId = _projectId;
            releaseId = _releaseId;
            releaseName = _bundleName;
            projectContent = _projectContent;

        }

        internal List<bool> GetChoices()
        {
            List<bool> choice = new List<bool>();
            foreach (ListViewItem item in listUnits.Items) choice.Add(item.Checked);
            return choice;
        }

        bool AssessVCBaseProjectContent(out ProjectContent projectContent)
        {
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            ProjectNode project = _projectNode;
            ReleaseInfo projectVersion = _releaseInfo;
            projectContent = new ProjectContent(project.Id);
            List<VersionControlUnitInfo> content;

            if (!_vcAdministrator.GetReleaseInfo(project.Id, projectVersion.Name, out content))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage());
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                return false;
            }

            foreach (VersionControlUnitInfo unit in content) projectContent.projectUnits.Add(unit);

            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            return true;
        }


        void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the folder to download the latest bundle.";
            folderBrowserDialog.SelectedPath = txtDestinationFolder.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtDestinationFolder.Text = folderBrowserDialog.SelectedPath;
        }


        void btnSel_Click(bool sel) {
            foreach (ListViewItem item in listUnits.Items)
            {
                if (!sel)
                {
                    VersionControlUnitInfo checkedUnit = (VersionControlUnitInfo)item.Tag;

                    if (checkedUnit.UnitType != VCAPI.VC_FOLDER_TYPE.CONFIG)
                    {
                        item.Checked = sel;
                    }
                }
                else
                {
                    item.Checked = sel;
                }
                
            }
        }

        private void btnSel_Click(object sender, EventArgs e)
        {
            btnSel_Click(true);
        }

        private void btnUnsel_Click(object sender, EventArgs e)
        {
            btnSel_Click(false);
        }

        private void txtDestinationFolder_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtDestinationFolder.Text))
            {
                txtBundlePath.Text = txtDestinationFolder.Text + "\\" + textProject.Text + "_" + _bundleName;
            }
            else
            {
                txtBundlePath.Text = "";
            }
        }

        private void listUnits_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            if (item != null && !item.Checked && item.ToolTipText == string.Empty)
            {
                    VersionControlUnitInfo checkedUnit = (VersionControlUnitInfo)item.Tag;

                    if (checkedUnit.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG)
                    {
                        item.Checked = true;
                        UserInfoHandler.ShowInfo("Configuration units are needed for the model to work properly. They" +
                            " cannot be unticked.");
                    }

            }
            else
            {
                item.ToolTipText = string.Empty;
            }

        }

    }
}
