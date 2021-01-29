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
    internal partial class AddBookmarkForm : Form
    {
        void AddBookmarkForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal AddBookmarkForm()
        {
            InitializeComponent();
            txtBMName.Focus();
        }
    }
}
