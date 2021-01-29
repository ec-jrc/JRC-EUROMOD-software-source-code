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
    internal partial class VCMultiUpDownload : Form
    {
        VCAdministrator _vcAdministrator = null;
        bool _upload = true;
        string nextVersion = string.Empty;
        List<VersionControlUnitInfo> successfulUploads = null;

        internal VCMultiUpDownload(VCAdministrator vcAdministrator, bool upload, bool selectAll = false, string nextAutoVersion = "")
        {
            _vcAdministrator = vcAdministrator;
            _upload = upload;
            nextVersion = nextAutoVersion;
            InitializeComponent();
            txtVersion.Text = nextAutoVersion;

            vcContent.Init(_vcAdministrator, true, !upload, false);
            btnUpDownload.Text = _upload ? "Upload" : "Download";
            this.Text = _upload ? "Version Control - Upload Bundle" : "Version Control Download";
        }

        void VCMultiUpDownload_Load(object sender, EventArgs e)
        {
            string helpPath; 
            EM_AppContext.Instance.GetHelpPath(out helpPath); 
            helpProvider.HelpNamespace = helpPath;

            vcContent.AssessStati(false, true);
            btnIncludeAll_Click(null, null);
            chkVersion_CheckedChanged(null, null);
        }

        void btnIncludeAll_Click(object sender, EventArgs e) { vcContent.checkInclude(true); }
        void btnClearInclude_Click(object sender, EventArgs e) { vcContent.checkInclude(false); }

        bool checkComments()
        {
            return true; // vcContent.checkComments();
        }

        internal string GetVersion()
        {
            if (chkVersion.Checked)
                return txtVersion.Text.Trim();
            else
            {
                return nextVersion;
            }
        }
        
        void btnUpDownload_Click(object sender, EventArgs e)
        {

            if (!vcContent.AssessStati(true)) return;

            List<VersionControlUnitInfo> unitInfos; List<string> stati; 
            if (_upload)
            {
                if (!vcContent.GetUploadInfo(out unitInfos, out stati)) return;
                successfulUploads = _vcAdministrator.MultiUpload(unitInfos, stati, GetVersion());
                if (successfulUploads.Count == 0) return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }


        private void chkVersion_CheckedChanged(object sender, EventArgs e)
        {
            txtVersion.Enabled = chkVersion.Checked;
        }

        internal List<VersionControlUnitInfo> GetSuccessfulUploads()
        {
            return successfulUploads;
        }
    }
}
