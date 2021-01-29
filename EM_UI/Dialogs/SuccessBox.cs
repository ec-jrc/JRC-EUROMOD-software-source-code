using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    //very primitive success-box, just to make it more obvious that a success is reported
    //could perhaps be extended to serve as a common adaptable message box or even unify the various boxes (InputBox, InfoBox, SuccessBox, OptionalWarningsForm)
    internal partial class SuccessBox : Form
    {
        internal SuccessBox(string message)
        {
            InitializeComponent();

            int preWidth = lblMessage.Width;
            int preHeight = lblMessage.Height;

            lblMessage.Text = message;

            this.Width += lblMessage.Width - preWidth;
            this.Height = Math.Max(this.Height, this.Height + lblMessage.Height - preHeight);

            btnOK.Top = Math.Max(btnOK.Top, lblMessage.Top + lblMessage.Height + 15);
            btnOK.Left = this.Left + (this.Width - btnOK.Width) / 2;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
