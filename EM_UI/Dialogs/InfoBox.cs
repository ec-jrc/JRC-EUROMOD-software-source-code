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
    //class could be extended to have more selectable features (like buttons, etc.)
    internal partial class InfoBox : Form
    {
        internal InfoBox()
        {
            InitializeComponent();
        }

        internal void Show(string info, string caption = "", int startPositionX = -1, int startPositionY = -1, bool readOnly = false)
        {
            txtInfo.ReadOnly = readOnly;
            txtInfo.Text = info;
            //txtInfo.AutoSize = true; //does not work unfortunately

            this.Text = caption;

            if (startPositionX != -1)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Left = startPositionX;
                this.Top = startPositionY;
            }

            this.ShowDialog();
        }

        void btnOKForClosing_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
