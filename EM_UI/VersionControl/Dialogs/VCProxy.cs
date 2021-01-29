using System;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCProxy : Form
    {
        VCAdministrator _vcAdministrator = null;

        internal VCProxy()
        {
            _vcAdministrator = EM_AppContext.Instance.GetVCAdministrator();
            InitializeComponent();
        }

        private void VCProxy_Load(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            String proxUsername = proxyUsername.Text;
            String proxPassword = proxyPassword.Text;


            _vcAdministrator.setProxyCredentials(proxUsername, proxPassword);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
