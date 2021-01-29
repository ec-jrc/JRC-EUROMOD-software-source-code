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
    internal partial class OptionalWarningsForm : Form
    {
        void OptionalWarningsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal OptionalWarningsForm(string Warning, bool showCancelButton = true)
        {
            InitializeComponent();

            int preWidth = lblWarning.Width;
            int preHeight = lblWarning.Height;

            lblWarning.Text = Warning;

            this.Width += lblWarning.Width - preWidth;
            this.Height = Math.Max(this.Height, this.Height + lblWarning.Height - preHeight);

            if (!showCancelButton)
            {
                btnCancel.Hide();
                btnOK.Left = btnOK.Left + (btnCancel.Left - btnOK.Left) / 2;
            }
        }

        internal bool Reshow()
        {
            return !chkDoNotReshowWarning.Checked;
        }
    }
}
