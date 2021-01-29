using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCCommitInfo : Form
    {

        internal VCCommitInfo(ReleaseInfo info)
        {
            InitializeComponent();

            txtAuthor.Text = info.Author;
            txtDate.Text = info.Date.ToString();
            txtInfo.Text = info.Message;
        }

        void VCCommitInfo_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
