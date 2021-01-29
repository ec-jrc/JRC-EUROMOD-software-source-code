using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.Tools;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCAdminContent : Form
    {
        VCAdministrator _vcAdministrator = null;

        internal VCAdminContent(VCAdministrator vcAdministrator, bool hasAdminRight)
        {
            _vcAdministrator = vcAdministrator;
            
            InitializeComponent();

            if (!hasAdminRight) btnUploadUnits.Enabled = btnRemoveUnits.Enabled = false;

            FillUnitList();
        }

        void VCAdminContent_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnUploadUnits_Click(object sender, EventArgs e) { if (_vcAdministrator.HandleButtonUploadUnits()) FillUnitList(); }
        void btnDownloadUnits_Click(object sender, EventArgs e) { if (_vcAdministrator.HandleButtonDownloadUnits()) FillUnitList(); }
        void btnRemoveUnits_Click(object sender, EventArgs e) { if (_vcAdministrator.HandleButtonRemoveUnits()) FillUnitList(); }
        
        void FillUnitList()
        {
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.WaitCursor;
            lvUnits.Items.Clear();
            List<VersionControlUnitInfo> units;
            if (!_vcAdministrator._vcAPI.GetLocalUnits(out units) || units == null)
            {
                UserInfoHandler.ShowError(_vcAdministrator._vcAPI.GetErrorMessage());
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.Default;
                return;
            }
            foreach (VersionControlUnitInfo unitInfo in units)
                lvUnits.Items.Add(string.Format("{0} ({1})", unitInfo.Name, VCContentControl.UnitTypeToString(unitInfo.UnitType)));
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.Default;
        }
    }
}
