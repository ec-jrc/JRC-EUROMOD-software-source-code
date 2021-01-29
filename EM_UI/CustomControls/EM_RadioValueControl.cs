using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;

namespace EM_UI.CustomControls
{
    public partial class EM_RadioValueControl : UserControl, IEditValue
    {
        public EM_RadioValueControl()
        {
            InitializeComponent();
        }

        public object EditValue
        {
            get
            {
                return (rbDefault.Checked ? "D" : "C") + numCustomValue.Value;
            }
            set
            {
                if (value != null && value.ToString() != "")
                {
                    rbCustom.Checked = value.ToString()[0] == 'C';
                    rbDefault.Checked = value.ToString()[0] == 'D';
                    numCustomValue.Value = int.Parse(value.ToString().Substring(1));
                }
            }
        }

        public event EventHandler EditValueChanged;

        internal void setCustomValue(int val)
        {
            numCustomValue.Value = val;
        }

        internal void setIsCustom(bool isCustom)
        {
            rbCustom.Checked = isCustom;
            rbDefault.Checked = !isCustom;
            numCustomValue.Enabled = rbCustom.Checked;
        }

        public int getValue()
        {
            return rbDefault.Checked ? int.MinValue : (int)numCustomValue.Value;
        }

        private void rbDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (EditValueChanged != null)
                EditValueChanged(this, EventArgs.Empty);
        }

        private void rbCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (EditValueChanged != null)
                EditValueChanged(this, EventArgs.Empty);
        }

        private void numCustomValue_ValueChanged(object sender, EventArgs e)
        {
            if (EditValueChanged != null)
                EditValueChanged(this, EventArgs.Empty);
        }
    }

}
