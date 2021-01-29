using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace VCUIAPI
{
    internal class InputBox
    {
        internal static DialogResult Show(string title, string promptText, ref string value, bool trimValue,
                                            bool multiline = false, int width = 400, int height = 100)
        {
            Form inputBoxForm = new Form();
            Label lblPromptText = new Label();
            TextBox txtValue = new TextBox();
            Button btnOk = new Button();
            Button btnCancel = new Button();

            inputBoxForm.Text = title;
            lblPromptText.Text = promptText;
            txtValue.Text = value;

            btnOk.Text = "OK";
            btnCancel.Text = "Cancel";
            btnOk.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;

            int widthButton = 75, heightButton = 23, leftLabel = 9, topLabel = 20, heightLabel = 13, leftText = 12, space = 10;
            lblPromptText.SetBounds(leftLabel, topLabel, width - leftLabel - space, heightLabel);
            txtValue.SetBounds(leftText, lblPromptText.Bottom + space, width - leftText * 2, height - lblPromptText.Bottom - heightButton - 3 * space);
            btnCancel.SetBounds(txtValue.Right - widthButton, height - heightButton - space, widthButton, heightButton);
            btnOk.SetBounds(btnCancel.Left - btnCancel.Width - space, btnCancel.Top, widthButton, heightButton);
            inputBoxForm.ClientSize = new Size(width, height);

            inputBoxForm.Controls.AddRange(new Control[] { lblPromptText, txtValue, btnOk, btnCancel });
            lblPromptText.AutoSize = true;
            txtValue.Anchor = txtValue.Anchor | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            inputBoxForm.FormBorderStyle = FormBorderStyle.Sizable;
            inputBoxForm.StartPosition = FormStartPosition.CenterParent;
            inputBoxForm.MinimizeBox = false;
            inputBoxForm.MaximizeBox = false;
            inputBoxForm.AcceptButton = btnOk;
            inputBoxForm.CancelButton = btnCancel;
            inputBoxForm.ShowIcon = false;
            inputBoxForm.ShowInTaskbar = false;

            if (multiline)
            {
                txtValue.WordWrap = true;
                txtValue.Multiline = true;
            }

            DialogResult dialogResult = inputBoxForm.ShowDialog();
            value = txtValue.Text.Trim();
            if (trimValue == true)
                value = value.Trim();
            return dialogResult;
        }
    }
}
