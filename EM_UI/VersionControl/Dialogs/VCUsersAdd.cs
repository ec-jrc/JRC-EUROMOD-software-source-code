using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCUsersAdd : Form
    {
        internal VCUsersAdd(List<string> users)
        {
            InitializeComponent();

            foreach (string user in users) listUsers.Items.Add(user);
        }

        void VCUsersAdd_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < listUsers.Items.Count; ++index) listUsers.SetItemChecked(index, true);
        }

        void btnClearSelect_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < listUsers.Items.Count; ++index) listUsers.SetItemChecked(index, false);
        }

        internal List<int> GetSelectedIndices()
        {
            List<int> checkedIndices = new List<int>(); 
            foreach (int index in listUsers.CheckedIndices) checkedIndices.Add(index);
            return checkedIndices;
        }
    }
}
