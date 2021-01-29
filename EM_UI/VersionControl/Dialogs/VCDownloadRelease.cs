using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCDownloadRelease : Form
    {
        VCAdministrator _vcAdministrator = null;
        List<ProjectNode> _projectInfos = new List<ProjectNode>();
        List<ReleaseInfo> _releaseInfos = new List<ReleaseInfo>();

        List<VersionControlUnitInfo> _unitInfos = null;

        List<DownloadAction> _downloadActions = new List<DownloadAction>();

        internal enum DownloadAction { noAction, getReleaseVersion, getMergeSupport };
        internal enum UIReleaseCompare { corresponds, differs, not_available, missing, older, newer };

        internal VCDownloadRelease(VCAdministrator vcAdministrator)
        {
            _vcAdministrator = vcAdministrator;

            InitializeComponent();

            if (!_vcAdministrator._vcAPI.GetProjectList(out _projectInfos, false))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage()); return;
            }

            foreach (ProjectNode projectInfo in _projectInfos)
            {
                cmbProjects.Items.Add(projectInfo.Name);
                if (projectInfo.Id == _vcAdministrator._vcAPI.vc_projectInfo.ProjectId)
                    cmbProjects.Text = projectInfo.Name; //select the current project
            }

            UpdateReleases();
        }

        void UpdateReleases()
        {
            _releaseInfos.Clear(); cmbReleases.Items.Clear(); dgvContent.Rows.Clear(); cmbReleases.Text = string.Empty;

            if (cmbProjects.SelectedIndex < 0) return; //should not happen
            Cursor = Cursors.WaitCursor;
            
            ProjectNode selectedProject = _projectInfos.ElementAt(cmbProjects.SelectedIndex);
            if (!_vcAdministrator._vcAPI.GetReleases(selectedProject.Id, out _releaseInfos))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage()); Cursor = Cursors.Default; return;
            }

            foreach (ReleaseInfo commitInfo in _releaseInfos)
                cmbReleases.Items.Add(commitInfo.Name + " - " + commitInfo.Date.ToShortDateString());

            Cursor = Cursors.Default;
        }

        void VCDownloadRelease_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the folder for the alternative destination.";
            folderBrowserDialog.SelectedPath = txtAlternativeFolder.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtAlternativeFolder.Text = folderBrowserDialog.SelectedPath;
        }

        void btnFullInfo_Click(object sender, EventArgs e)
        {
            if (cmbReleases.SelectedIndex >= 0) (new VCCommitInfo(_releaseInfos.ElementAt(cmbReleases.SelectedIndex))).ShowDialog();
        }

        void cmbProjects_SelectedIndexChanged(object sender, EventArgs e) { UpdateReleases(); }

        bool CheckFieldValidity()
        {
            if (cmbReleases.SelectedIndex < 0) { UserInfoHandler.ShowInfo("Please select a release."); return false; }
            
            _downloadActions.Clear();

            string undefUnits = string.Empty; int undefUnitsCount = 0, actionUnitsCount = 0, mergeCount = 0;
            foreach (var ui in _unitInfos) // _downloadActions needs to be in the same order as _unitInfos ...
            {
                DataGridViewRow row = null; // ... therefore search for the correct row (users can change order sort-column-actions)
                foreach (DataGridViewRow r in dgvContent.Rows) { if ((long) r.Tag == ui.UnitId) { row = r; break; } }

                if (row.Cells[colGetReleaseVersion.Index].ReadOnly)
                    _downloadActions.Add(DownloadAction.noAction);
                else if (EM_Helpers.SaveConvertToBoolean(row.Cells[colKeepUIVersion.Index].Value))
                    _downloadActions.Add(DownloadAction.noAction);
                else if (EM_Helpers.SaveConvertToBoolean(row.Cells[colGetMergeSupport.Index].Value))
                { _downloadActions.Add(DownloadAction.getMergeSupport); ++actionUnitsCount; ++mergeCount; }
                else if (EM_Helpers.SaveConvertToBoolean(row.Cells[colGetReleaseVersion.Index].Value))
                { _downloadActions.Add(DownloadAction.getReleaseVersion); ++actionUnitsCount; }
                else { VCAdministrator.AddUnitToMessage(ref undefUnits, row.Cells[colUnit.Index].Value.ToString()); ++undefUnitsCount; }
            }

            if (undefUnitsCount > 0)
            {
                UserInfoHandler.ShowInfo("Please make a choice for each unit, where user-interface-version and Release-version differ." +
                    Environment.NewLine + string.Format("A choice is missing for unit{0} {1}.", undefUnitsCount > 1 ? "s" : string.Empty, undefUnits));
                return false;
            }

            if (actionUnitsCount == 0)
            {
                UserInfoHandler.ShowInfo("Your choices do not require any download-action." + Environment.NewLine +
                                         "Please change your choices or close the dialog with Cancel.");
                return false;
            }

            if (mergeCount > 0 && txtAlternativeFolder.Text != string.Empty)
            {
                UserInfoHandler.ShowInfo("Downloading to an alternative destination is not possible if 'Merge Support' is requested for any unit.");
                return false;
            }

            if (txtAlternativeFolder.Text != string.Empty && !System.IO.Directory.Exists(txtAlternativeFolder.Text))
            { UserInfoHandler.ShowInfo("Please select an existing folder for the alternative destination."); return false; }

            return true;
        }

        void btnDownload_Click(object sender, EventArgs e)
        {
            if (!CheckFieldValidity()) return;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        bool GetReleaseContent(out List<long> unitIds, out List<string> units, out List<string> types, out List<string> versions, out List<VCDownloadRelease.UIReleaseCompare> stati)
        {
            units = new List<string>(); types = new List<string>(); versions = new List<string>();
            unitIds = new List<long>();
            stati = new List<VCDownloadRelease.UIReleaseCompare>();

            ProjectNode projectInfo = _projectInfos.ElementAt(cmbProjects.SelectedIndex);
            ReleaseInfo releaseInfo = _releaseInfos.ElementAt(cmbReleases.SelectedIndex);

            if (!_vcAdministrator.GetReleaseInfo(projectInfo.Id, releaseInfo.Name, out _unitInfos))
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage());
                return false;
            }
            
            //prepare info for the table that displays differences between UI-version and Release-version
            List<string> listStati; if (!_vcAdministrator.CompareLocalProjectWithOnlineBundle(_unitInfos, out listStati)) return false;
            for (int i = 0; i < _unitInfos.Count; ++i)
            {
                units.Add(_unitInfos[i].Name); //to be displayed in the 'Unit'-column
                types.Add(VCContentControl.UnitTypeToString(_unitInfos[i].UnitType)); //to be displayed in the 'Type'-column
                versions.Add(releaseInfo.Name); //to be displayed in the 'Version'-column
                unitIds.Add(_unitInfos[i].UnitId);

                switch (listStati[i])
                {
                    case VCAdministrator.VC_STATUS_MODIFIED:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.differs);
                        break;
                    case VCAdministrator.VC_STATUS_UPTODATE:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.corresponds);
                        break;
                    case VCAdministrator.VC_STATUS_OLD:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.older);
                        break;
                    case VCAdministrator.VC_STATUS_NEW:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.newer);
                        break;
                    case VCAdministrator.VC_STATUS_NOLOCAL:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.missing);
                        break;
                    default:
                        stati.Add(VCDownloadRelease.UIReleaseCompare.not_available);
                        break;
                }
            }
            return true;
        }

        internal void UpdateReleaseContent()
        {
            dgvContent.Rows.Clear();
            if (cmbReleases.SelectedIndex < 0) return;

            Cursor = Cursors.WaitCursor;
            List<string> units, types, versions; List<UIReleaseCompare> stati; List<long> unitIds;

            if (!GetReleaseContent(out unitIds, out units, out types, out versions, out stati)) { cmbReleases.SelectedIndex = -1; Cursor = Cursors.Default; return; }

            for (int i = 0; i < units.Count; ++i)
            {
                bool downloadDispensable = stati[i] == UIReleaseCompare.corresponds;
                bool missingLocal = stati[i] == UIReleaseCompare.missing;
                
                //the visual part: set the cell style for newly to create checkboxes to background = gray or white, depending on whether they should look like enabled
                colGetMergeSupport.CellTemplate.Style.BackColor = downloadDispensable || _vcAdministrator.VC_UNSUPPORTED_MERGE.Contains(units[i].ToLower()) || missingLocal ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                colGetReleaseVersion.CellTemplate.Style.BackColor = downloadDispensable ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                colKeepUIVersion.CellTemplate.Style.BackColor = downloadDispensable ? System.Drawing.Color.LightGray : System.Drawing.Color.White;

                int indexRow = dgvContent.Rows.Add(units[i], types[i], versions[i], stati[i],
                    !downloadDispensable, false, false); //init download/keep/merge with true/false/false if versions not equal otherwise with false/false/false
                dgvContent.Rows[indexRow].Tag = unitIds[i]; // unitId is necessary to sort the info returned by GetChoices correctly (see CheckFieldValidity)

                // the effective part: set check-boxes to read-only 
                // for "get" and "keep" if versions are equal (i.e. no need to download)
                (dgvContent.Rows[indexRow].Cells[colGetReleaseVersion.Index] as DataGridViewCheckBoxCell).ReadOnly = downloadDispensable;
                (dgvContent.Rows[indexRow].Cells[colKeepUIVersion.Index] as DataGridViewCheckBoxCell).ReadOnly = downloadDispensable;
                // for "merge" if versions are equal, or the Merge Tool does not support this unit, or the local unit is missing
                (dgvContent.Rows[indexRow].Cells[colGetMergeSupport.Index] as DataGridViewCheckBoxCell).ReadOnly = downloadDispensable || _vcAdministrator.VC_UNSUPPORTED_MERGE.Contains(units[i].ToLower()) || missingLocal;

            }

            dgvContent.Focus();

            Cursor = Cursors.Default;
        }

        void btnAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvContent.Rows)
            {
                if (row.Cells[colGetReleaseVersion.Index].ReadOnly) continue;
                row.Cells[colGetReleaseVersion.Index].Value = sender == btnGetAll;
                row.Cells[colGetMergeSupport.Index].Value = sender == btnMergeAll;
                row.Cells[colKeepUIVersion.Index].Value = sender == btnKeepAll;
            }
        }

        void cmbReleases_SelectedIndexChanged(object sender, EventArgs e) { UpdateReleaseContent(); }

        void dgvContent_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex != colGetMergeSupport.Index && e.ColumnIndex != colGetReleaseVersion.Index && e.ColumnIndex != colKeepUIVersion.Index) return;

            DataGridViewCellCollection cells = dgvContent.Rows[e.RowIndex].Cells;
            if (EM_Helpers.SaveConvertToBoolean(cells[e.ColumnIndex].Value)) return;

            //if any action (get, keep, merge) is set for a unit, no other action can be set as well, thus untick the other two check-boxes (but only if the clicked cell is not read-only
            if (!(dgvContent.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell).ReadOnly)
            {
                if (e.ColumnIndex == colGetMergeSupport.Index) cells[colKeepUIVersion.Index].Value = cells[colGetReleaseVersion.Index].Value = false;
                if (e.ColumnIndex == colKeepUIVersion.Index) cells[colGetMergeSupport.Index].Value = cells[colGetReleaseVersion.Index].Value = false;
                if (e.ColumnIndex == colGetReleaseVersion.Index) cells[colKeepUIVersion.Index].Value = cells[colGetMergeSupport.Index].Value = false;
            }
        }

        internal void GetChoices(out long projectId, out ReleaseInfo releaseInfo,
                                 out List<VersionControlUnitInfo> unitInfos,
                                 out List<VCDownloadRelease.DownloadAction> downloadActions, out string alternativePath)
        {
            projectId = _projectInfos.ElementAt(cmbProjects.SelectedIndex).Id;
            releaseInfo = _releaseInfos.ElementAt(cmbReleases.SelectedIndex);
            unitInfos = _unitInfos;
            downloadActions = _downloadActions;
            alternativePath = txtAlternativeFolder.Text;
        }

    }
}
