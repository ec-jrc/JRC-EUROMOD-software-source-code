using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    public partial class VCSelectConfigFile : Form
    {
        string _chosenConfigFile = String.Empty;
        public VCSelectConfigFile()
        {
            InitializeComponent();
        }

        private void VCSelectConfigFile_Load(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
            if (varconfigRadio.Checked)
            {
                _chosenConfigFile = "VARCONFIG.xml";
            }
            else if (hicpconfigRadio.Checked)
            {
                _chosenConfigFile = "HICPCONFIG.xml";
            }
            else if(exchangeratesRadio.Checked)
            {
                _chosenConfigFile = "EXCHANGERATESCONFIG.xml";
            }
            else if (switchablepolicyconfigRadio.Checked)
            {
                _chosenConfigFile = "SWITCHABLEPOLICYCONFIG.xml";
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }

        internal void GetChosenConfigFile(out string chosenConfigFile)
        {
            chosenConfigFile = _chosenConfigFile;

        }

    }
}
