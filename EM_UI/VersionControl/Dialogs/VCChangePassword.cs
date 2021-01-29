using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.Tools;
using EM_UI.Validate;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCChangePassword : Form
    {

        internal VCChangePassword(string oldPassword, string forgotPasswordQuestion, string forgotPasswordAnswer)
        {
            InitializeComponent();

            txtPassword.Text = txtRetypePassword.Text = oldPassword;
            txtForgotPasswordQuestion.Text = forgotPasswordQuestion;
            txtForgotPasswordAnswer.Text = forgotPasswordAnswer;
        }

        void VCChangePassword_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == txtRetypePassword.Text) { DialogResult = DialogResult.OK; Close(); return; }
            UserInfoHandler.ShowError("Password does not correspond with retyped password. Please correct.");
        }

        internal void GetNewSettings(out string password, out string forgotPasswordQuestion, out string forgotPasswordAnswer)
        {
            password = txtPassword.Text;
            forgotPasswordQuestion = txtForgotPasswordQuestion.Text;
            forgotPasswordAnswer = txtForgotPasswordAnswer.Text;
        }
    }
}
