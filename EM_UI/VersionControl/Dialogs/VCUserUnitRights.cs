using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCUserUnitRights : Form
    {
        VCAPI _vcAPI = null;
        long _userId = 0;
        VCAPI.VC_ACCESS_RIGHTS _defaultRight = VCAPI.VC_ACCESS_RIGHTS.NONE;
        Dictionary<long /*unit-id*/, VCAPI.VC_ACCESS_RIGHTS /*unit-right*/> _initialRights = null;

        internal VCUserUnitRights(VCAPI vcAPI, UserInfo userInfo, VCAPI.VC_ACCESS_RIGHTS defaultRight, Dictionary<long, VCAPI.VC_ACCESS_RIGHTS> initialRights)
        {
            InitializeComponent();

            _userId = userInfo.userId;
            _vcAPI = vcAPI;
            _defaultRight = defaultRight;
            _initialRights = initialRights;

            this.Text = this.Text + userInfo.username;

            List<VersionControlUnitInfo> units;
            if (!_vcAPI.GetRemoteUnits(_vcAPI.vc_projectInfo.ProjectId, out units))
            {
                UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                return;
            }

            if (defaultRight == VCAPI.VC_ACCESS_RIGHTS.UPLOAD) colWrite.HeaderText = btnAllWrite.Text = "Default"; 
            if (defaultRight == VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD) colRead.HeaderText = btnAllRead.Text = "Default";
            if (defaultRight == VCAPI.VC_ACCESS_RIGHTS.NONE) colNone.HeaderText = btnAllNone.Text = "Default";

            foreach (VersionControlUnitInfo unit in units)
            {
                long unitId = unit.UnitId;
                VCAPI.VC_ACCESS_RIGHTS right = (_initialRights != null && _initialRights.ContainsKey(unitId)) ? _initialRights[unitId] : defaultRight;
                int index = dgvUnits.Rows.Add(unit.Name, unit.UnitType,
                            right == VCAPI.VC_ACCESS_RIGHTS.UPLOAD,
                            right == VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD,
                            right != VCAPI.VC_ACCESS_RIGHTS.UPLOAD && right != VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD);
                dgvUnits.Rows[index].Tag = unit.UnitId;
                dgvUnits.Rows[index].Cells[colWrite.Index].ReadOnly = //to handle ticking manually (see dgvUnits_CellContentClick)
                    dgvUnits.Rows[index].Cells[colRead.Index].ReadOnly = dgvUnits.Rows[index].Cells[colNone.Index].ReadOnly = true;
            }
        }

        VCAPI.VC_ACCESS_RIGHTS GetRightSetting(DataGridViewRow row)
        {
            if (EM_Helpers.SaveConvertToBoolean(row.Cells[colWrite.Index].Value)) return VCAPI.VC_ACCESS_RIGHTS.UPLOAD;
            if (EM_Helpers.SaveConvertToBoolean(row.Cells[colRead.Index].Value)) return VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD;
            if (EM_Helpers.SaveConvertToBoolean(row.Cells[colNone.Index].Value)) return VCAPI.VC_ACCESS_RIGHTS.NONE;
            return _defaultRight; //should not happen
        }

        void VCUserUnitRights_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void dgvUnits_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colWrite.Index && e.ColumnIndex != colRead.Index && e.ColumnIndex != colNone.Index) return;
            //the check-boxes cannot get unchecked by ticking them, i.e. ticking a checked box changes nothing
            //however ticking a not checked box, checks this box and unchecks the other two
            DataGridViewCellCollection cells = dgvUnits.Rows[e.RowIndex].Cells;
            cells[colWrite.Index].Value = colWrite.Index == e.ColumnIndex;
            cells[colRead.Index].Value = colRead.Index == e.ColumnIndex;
            cells[colNone.Index].Value = colNone.Index == e.ColumnIndex;
        }

        void btnAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvUnits.Rows)
            {
                row.Cells[colWrite.Index].Value = sender == btnAllWrite;
                row.Cells[colRead.Index].Value = sender == btnAllRead;
                row.Cells[colNone.Index].Value = sender == btnAllNone;
            }
        }

        internal Dictionary<long, VCAPI.VC_ACCESS_RIGHTS> GetChangedRights()
        {
            Dictionary<long, VCAPI.VC_ACCESS_RIGHTS> rights = new Dictionary<long, VCAPI.VC_ACCESS_RIGHTS>();
            foreach (DataGridViewRow row in dgvUnits.Rows)
            {
                VCAPI.VC_ACCESS_RIGHTS right = GetRightSetting(row);
                int unitId = Convert.ToInt32(row.Tag);
                if (right != _defaultRight) rights.Add(unitId, right);
            }
            return rights;
        }
    }
}
