using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.Validate;

namespace EM_UI.Dialogs
{
    internal partial class ComponentSearchForm : Form
    {
        void ComponentSearchForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (txtSearchByIdentifier.Text == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please specify the identifier.");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        internal ComponentSearchForm()
        {
            InitializeComponent();
        }

        internal string GetIdentifier() { return txtSearchByIdentifier.Text; }

        private void txtSearchByIdentifier_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                btnOK_Click(null, null);
            }
        }
    }
}
