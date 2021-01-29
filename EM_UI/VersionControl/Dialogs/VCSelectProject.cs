using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.Tools;
using EM_UI.CountryAdministration;
using EM_UI.Validate;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCSelectProject : Form
    {
        int _lastCheckedItem = -1;

        internal VCSelectProject(VCAPI vcApi, string caption, bool checkAdminRights)
        {
            InitializeComponent();

            this.Text = caption;

            List<ProjectNode> projectInfos;
            List<ProjectNode> filteredProjectInfo = new List<ProjectNode>();
            
            // If you cannot get the list of projects, then no point in opening this form
            if (!vcApi.GetProjectList(out projectInfos, false)) 
            { 
                UserInfoHandler.ShowError(vcApi.GetErrorMessage()); 
                DialogResult = DialogResult.Cancel;
                Load += (s, e) => Close();
                return; 
            }

            foreach (ProjectNode projectInfo in projectInfos)
            {
                if (!checkAdminRights || (checkAdminRights && vcApi.HasCurrentUserProjectAdminRight(projectInfo.Id)))
                {
                   lstProjects.Items.Add(string.Format("{0} {1}", projectInfo.Id, projectInfo.Name));
                   filteredProjectInfo.Add(projectInfo);
                }
                
            }

            if(projectInfos.Count > 0 && lstProjects.Items.Count == 0) { UserInfoHandler.ShowError("You need to have administrator rights in a project to remove it. No projects available!"); return; }
            lstProjects.Tag = filteredProjectInfo;
            if (filteredProjectInfo.Count > 0) {lstProjects.SelectedIndex = 0; lstProjects.Focus(); }
        }

        void VCLinkToProject_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void lstProjects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue != CheckState.Checked && _lastCheckedItem != -1) lstProjects.SetItemChecked(_lastCheckedItem, false);
            _lastCheckedItem = e.Index;
        }

        internal ProjectNode GetSelectedProject()
        {
            if (lstProjects.CheckedIndices.Count == 0) return null;
            return (lstProjects.Tag as List<ProjectNode>).ElementAt(lstProjects.CheckedIndices[0]);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (GetSelectedProject() == null)
            {
                UserInfoHandler.ShowInfo("Please select a project.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
