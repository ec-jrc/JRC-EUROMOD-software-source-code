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
    public partial class PolicyEffectsChooseMode : Form
    {
        public PolicyEffectsChooseMode()
        {
            InitializeComponent();
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Yes;
        }

        private void btnBasic_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.No;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://linkprotect.cudasvc.com/url?a=http%3a%2f%2fdoi.org%2f10.1111%2fobes.12354&c=E,1,orbQNECEbOPJkDpBQRqHimdqSKeR3yLc9dX3s0n_ro0eDXvbXcDuWo9X_hXGqom7NmlaEyiOPUddpRHGqlylmxF8fww-7Kllaqmhq11uIJbgh6Z7cDk8TAoG&typo=1");
        }
    }
}
