using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    public partial class ShowLicenceForm : Form
    {
        public ShowLicenceForm()
        {
            InitializeComponent();
            string licencePath = string.Empty;
            EM_AppContext.Instance.GetLicencePath(out licencePath);

            licenceTextBox.LoadFile(licencePath, RichTextBoxStreamType.RichText);
            licenceTextBox.SelectAll();
            licenceTextBox.SelectionIndent += 15;
            licenceTextBox.SelectionRightIndent += 15;
            licenceTextBox.DeselectAll();
            licenceTextBox.SelectionBackColor = licenceTextBox.BackColor;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}
