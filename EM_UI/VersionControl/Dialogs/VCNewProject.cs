using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCNewProject : Form
    {
        VCAdministrator _vcAdministrator = null;

        internal class ProjectContent
        {
            internal ProjectContent(string baseProjectPath) { this.baseProjectPath = baseProjectPath; }
            internal ProjectContent(long baseProjectId) { this.baseProjectId = baseProjectId; }
            internal string baseProjectPath = string.Empty; //path as unique identifier of the disk-base-project
            internal long baseProjectId = -1; //id as unique identifier of VC-base-project
            internal string selectedRelease = string.Empty;

            internal List<VersionControlUnitInfo> projectUnits = new List<VersionControlUnitInfo>();
            internal List<bool> selections = new List<bool>(); /*is selected to be part of the new project*/
            internal List<string> selectedYears = new List<string>();
        }
        ProjectContent _projectContent = null; //structure for storing project content (selection of base project)
        string _selectedVersion = null; //Variable for storing the release selected by the user 

        internal VCNewProject(VCAdministrator vcAdministrator)
        {
            _vcAdministrator = vcAdministrator;
            InitializeComponent();
            bool showVC = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.VC_show);

            cmbBaseProjects.Visible = showVC;
            chkProjectOnVC.Visible = showVC;
            cmbSelectVersion.Visible = showVC;
        }

        void VCNewProject_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            bool showVC = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.VC_show);
            if (showVC && _vcAdministrator != null)
            {
                chkProjectOnVC.Checked = true;
                chkProjectOnVC_Click(null, null);
            }
        }

        void btnSelectProjectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { 
                Description = "Please choose the folder to store the new project.", 
                SelectedPath = txtProjectPath.Text 
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            
            txtProjectPath.Text = folderBrowserDialog.SelectedPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtProjectPath.Text))
            { UserInfoHandler.ShowInfo("Please indicate a valid 'Project Path' for storing the new project."); return; }

            if (txtProjectName.Text == string.Empty)
            { UserInfoHandler.ShowInfo("Please indicate a 'Project Name'."); return; }

            string newEuromodFolder = EMPath.AddSlash(txtProjectPath.Text) + txtProjectName.Text;
            if (Directory.Exists(newEuromodFolder) && (Directory.GetFiles(newEuromodFolder).Count() > 0 || Directory.GetDirectories(newEuromodFolder).Count() > 0))
            {UserInfoHandler.ShowError(string.Format("Folder '{0}' exists and is not empty.\n\nThe file structure for the new project requires an empty base-folder.", newEuromodFolder)); return; }

            if (chkProjectOnDisk.Checked && !CountryAdministrator.ContainsEuromodFileStructure(txtBasePath.Text))
            { UserInfoHandler.ShowInfo($"Please indicate a folder containting the {DefGeneral.BRAND_TITLE} file structure as 'Project on Disk' (or check 'No Base Project')."); return; }

            if (chkProjectOnVC.Checked && cmbBaseProjects.SelectedIndex < 0)
            { UserInfoHandler.ShowInfo("Please select a Base Project from the list 'Project on VC' (or check 'No Base Project')."); return; }

            if (chkProjectOnVC.Checked && cmbSelectVersion.SelectedIndex < 0)
            { UserInfoHandler.ShowError("Please select a Version. If no version is available, this project cannot be selected as Base Project."); return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnDefineContent_Click(object sender, EventArgs e)
        {
            if (chkProjectOnDisk.Checked)
            {
                if (!CountryAdministrator.ContainsEuromodFileStructure(txtBasePath.Text))
                {
                    UserInfoHandler.ShowInfo($"Before you can define the content you have to indicate a folder containing the {DefGeneral.BRAND_TITLE} file structure as 'Base Project' (field 'Project on Disk').");
                    return;
                }
                string baseProjectPath = EMPath.AddSlash(txtBasePath.Text).ToLower();
                if (_projectContent != null && _projectContent.baseProjectPath != baseProjectPath)
                    _projectContent = null; //project content was defined before, but for another base-project
                if (_projectContent == null) { if (!AssessDiskBaseProjectContent(baseProjectPath, out _projectContent)) return; }
            }
            else //chkProjectOnVC.Checked = true
            {
                if (cmbBaseProjects.SelectedIndex < 0)
                {
                    UserInfoHandler.ShowInfo("Before you can define the content you have to define a 'Base Project' (select from list 'Project on VC').");
                    return;
                }
                if (cmbSelectVersion.SelectedIndex < 0)
                {
                    UserInfoHandler.ShowInfo("Before you can define the content you have to select a Version for the 'Base Project' (select from list 'Project on VC').");
                    return;
                }

                string currentSelectedVersionIndex = cmbSelectVersion.SelectedItem.ToString();
      
                ProjectNode project = (cmbBaseProjects.Tag as List<ProjectNode>).ElementAt(cmbBaseProjects.SelectedIndex);

                if ((_projectContent != null && _projectContent.baseProjectId != project.Id) || (_projectContent != null && _projectContent.baseProjectId == project.Id && _projectContent.selectedRelease != cmbSelectVersion.SelectedItem.ToString()))
                {
                    _projectContent = null; //project content was defined before, but for another base-project  
                }
                _selectedVersion = currentSelectedVersionIndex; //The selected version is updated

                if (_projectContent == null) { if (!AssessVCBaseProjectContent(out _projectContent)) return; }
            }

            VCProjectContent projectContent = new VCProjectContent(_projectContent);
            if (projectContent.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            projectContent.GetChoices(out _projectContent.selections, out _projectContent.selectedYears);
        }

        bool AssessVCBaseProjectContent(out ProjectContent projectContent)
        {
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            ProjectNode project = (cmbBaseProjects.Tag as List<ProjectNode>).ElementAt(cmbBaseProjects.SelectedIndex);
            ReleaseInfo projectVersion = (cmbSelectVersion.Tag as List<ReleaseInfo>).ElementAt(cmbSelectVersion.SelectedIndex);
            projectContent = new ProjectContent(project.Id);
            List<VersionControlUnitInfo> content;
            string releaseName = string.Empty;
            if (!_vcAdministrator.GetReleaseInfo(project.Id, projectVersion.Name, out content))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage());
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                return false;
            }

            foreach (VersionControlUnitInfo unit in content) { projectContent.projectUnits.Add(unit); projectContent.selections.Add(true); }

            projectContent.selectedRelease = cmbSelectVersion.SelectedItem.ToString();
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            return true;
        }

        static bool AssessDiskBaseProjectContent(string baseProjectPath, out ProjectContent projectContent)
        {
            projectContent = new ProjectContent(baseProjectPath);
            List<VersionControlUnitInfo> content = VCAdministrator.GetContent_LocalProject(baseProjectPath);
            foreach (VersionControlUnitInfo unit in content) projectContent.projectUnits.Add(unit);
            for (int i = 0; i < projectContent.projectUnits.Count; i++) projectContent.selections.Add(true);
            return true;
        }

        void btnSelectBasePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { 
                Description = "Please choose the folder containing the base project.", 
                SelectedPath = string.IsNullOrEmpty(txtBasePath.Text) ? EM_AppContext.FolderEuromodFiles : txtBasePath.Text };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            txtBasePath.Text = folderBrowserDialog.SelectedPath;
        }       

        bool chkBaseProjectChanged(CheckBox changedBox)
        {
            if (!changedBox.Checked) { changedBox.Checked = true; return false; } //don't allow direct unchecking, to make this work like radio-buttons
            if (changedBox == chkProjectOnDisk) chkProjectOnVC.Checked = chkNoBaseProject.Checked = false;
            else if (changedBox == chkProjectOnVC) chkProjectOnDisk.Checked = chkNoBaseProject.Checked = false;
            else chkProjectOnVC.Checked = chkProjectOnDisk.Checked = false;
            txtBasePath.Enabled = changedBox == chkProjectOnDisk;
            btnSelectBasePath.Enabled = changedBox == chkProjectOnDisk;
            btnDefineContent.Enabled = changedBox != chkNoBaseProject;
            cmbBaseProjects.Enabled = changedBox == chkProjectOnVC;
            cmbSelectVersion.Enabled = changedBox == chkProjectOnVC;
            return true;
        }

        void chkProjectOnDisk_Click(object sender, EventArgs e)
        {
            if (!chkBaseProjectChanged(chkProjectOnDisk)) return;
        }

        void chkProjectOnVC_Click(object sender, EventArgs e)
        {
            chkBaseProjectChanged(chkProjectOnVC);

            if (cmbBaseProjects.Items.Count > 0) return; //combo with VC-project is already filled

            //first click on VC-base-project: fill combo with available projects
            if (_vcAdministrator == null) //if not logged in, offer user to do so
            {
                VCAdministrator vcAdministrator = EM_AppContext.Instance.GetVCAdministrator();
                if (!vcAdministrator.HandleButtonLogInOutClicked()) return;
                _vcAdministrator = vcAdministrator;
            }

            List<ProjectNode> projects;
            if (!_vcAdministrator._vcAPI.GetProjectList(out projects, false))
                { UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage()); return; }
            cmbBaseProjects.Tag = projects;
            cmbBaseProjects.Items.Clear();
            int indexCurProject = -1;
            foreach (ProjectNode project in projects)
            {
                int index = cmbBaseProjects.Items.Add(project.Name);
                if (_vcAdministrator._vcAPI.IsVersionControlled() && project.Id == _vcAdministrator._vcAPI.vc_projectInfo.ProjectId) indexCurProject = index;
            }
            if (indexCurProject != -1) cmbBaseProjects.SelectedIndex = indexCurProject; //select currently loaded project (if there is one), as it's intuitive to use it as base
        }

        void chkNoBaseProject_Click(object sender, EventArgs e)
        {
            if (!chkBaseProjectChanged(chkNoBaseProject)) return;
        }

        void cmbBaseProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbSelectVersion.Items.Clear(); cmbSelectVersion.Text = string.Empty;
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            if (cmbBaseProjects.SelectedIndex < 0) return; //should not happen

            List<ReleaseInfo> _releaseInfos;
            ProjectNode selectedProject = (cmbBaseProjects.Tag as List<ProjectNode>).ElementAt(cmbBaseProjects.SelectedIndex);
            if (!_vcAdministrator._vcAPI.GetReleases(selectedProject.Id, out _releaseInfos))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage());
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                return;
            }

            //The name of the project name is automatically populated with the name of the project + selected version
            String projectName = selectedProject.Name;

            cmbSelectVersion.Tag = _releaseInfos;
            foreach (ReleaseInfo commitInfo in _releaseInfos)
                cmbSelectVersion.Items.Add(commitInfo.Name);
            if (cmbSelectVersion.Items.Count > 0)
            {
                cmbSelectVersion.SelectedIndex = 0;
                projectName = projectName + "_" + cmbSelectVersion.SelectedItem;
            }



            txtProjectName.Text = projectName;
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
        }

        internal bool GetInfo(out string projectPath, out string projectName, out bool diskBaseProject, out bool vcBaseProject, out ProjectContent projectContent)
        {
            projectPath = txtProjectPath.Text;
            projectName = txtProjectName.Text;
            diskBaseProject = chkProjectOnDisk.Checked;
            vcBaseProject = chkProjectOnVC.Checked;
            if (_projectContent == null && diskBaseProject) { if (!AssessDiskBaseProjectContent(txtBasePath.Text, out projectContent)) return false; }
            else if (_projectContent == null && vcBaseProject) { if (!AssessVCBaseProjectContent(out projectContent)) return false; }
            else projectContent = _projectContent;
            return true;
        }

        private void cmbSelectVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            String projectName = cmbBaseProjects.SelectedItem + "_" + cmbSelectVersion.SelectedItem;
            txtProjectName.Text = projectName;
        }
    }
}
