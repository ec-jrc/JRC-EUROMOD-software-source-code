using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class NumericEditor : UserControl
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        private const string TYPE_VALUE = "Value";
        private const string TYPE_REFERENCE = "Reference";
        private const char SEPARATOR = '|';
        readonly bool isRange;

        public NumericEditor(Program _plugin, bool _isRange = true)
        {
            InitializeComponent();
            Location = new Point(0, 0);
            Plugin = _plugin;
            isRange = _isRange;
            lookUpEditReference.Properties.DataSource = Plugin.referenceTablesData.ReferenceTables;
            checkEndingValue.Visible = isRange;
            numericUpDownEndingValue.Visible = isRange;
            numericUpDownStep.Visible = isRange;
            labelStep.Visible = isRange;
            Height = isRange?140:90;
            labelStart.Text = isRange ? "Starting Value:" : "Value:";
            Enter += NumericEditor_Enter;
        }

        private void NumericEditor_Enter(object sender, EventArgs e)
        {
            numericUpDownStartingValue.Select(0, numericUpDownStartingValue.Value.ToString().Length);
        }

        public string EditValue
        {
            set
            {
                if (value == null || value == "" || !value.Contains(SEPARATOR))
                {
                    setEmpty();
                }
                else
                {
                    try
                    {
                        string[] vals = value.Split(SEPARATOR);
                        radioButtonValue.Checked = vals[0] == TYPE_VALUE;
                        radioButtonReference.Checked = vals[0] == TYPE_REFERENCE;
                        labelPercStart.Visible = vals[0] == TYPE_REFERENCE;
                        labelPercEnd.Visible = isRange && (vals[0] == TYPE_REFERENCE);
                        labelPercStep.Visible = isRange && (vals[0] == TYPE_REFERENCE);
                        lookUpEditReference.Enabled = vals[0] == TYPE_REFERENCE;
                        lookUpEditReference.EditValue = vals[1];
                        numericUpDownStartingValue.Text = vals[2];
                        if (isRange && vals.Length > 3)
                        {
                            try
                            {
                                checkEndingValue.Checked = bool.Parse(vals[3]);
                                numericUpDownEndingValue.Enabled = checkEndingValue.Checked;
                                numericUpDownStep.Enabled = checkEndingValue.Checked;
                                numericUpDownEndingValue.Text = vals[4];
                                numericUpDownStep.Text = vals[5];
                            }
                            catch
                            {
                                // half way there, so it was probalby a Numeric that turned into NumericRange
                                checkEndingValue.Checked = false;
                                numericUpDownEndingValue.Enabled = false;
                                numericUpDownStep.Enabled = false;
                                numericUpDownEndingValue.Text = "";
                                numericUpDownStep.Text = "";
                            }
                        }
                    }
                    catch
                    {
                        // in case of conversion error. This can happen for example if you edit the Advanced Variables and change the VariableType
                        setEmpty();
                    }
                }
            }
            get
            {
                try
                {
                    string val = "";
                    val += (radioButtonValue.Checked ? TYPE_VALUE : TYPE_REFERENCE) + SEPARATOR;    // vals[0]
                    val += lookUpEditReference.EditValue.ToString() + SEPARATOR;                    // vals[1]
                    val += numericUpDownStartingValue.Text;                                         // vals[2]
                    if (isRange)
                    {
                        val += SEPARATOR;
                        val += checkEndingValue.Checked.ToString() + SEPARATOR;                         // vals[3]
                        val += numericUpDownEndingValue.Text + SEPARATOR;                               // vals[4]
                        val += numericUpDownStep.Text;                                                  // vals[5]
                    }
                    return val;
                }
                catch
                {
                    return "";
                }
            }
        }

        private void setEmpty()
        {
            radioButtonValue.Checked = true;
            radioButtonReference.Checked = false;
            lookUpEditReference.Enabled = false;
            lookUpEditReference.EditValue = (Plugin.referenceTablesData.ReferenceTables.Rows.Count > 0 ? Plugin.referenceTablesData.ReferenceTables.Rows[0]["TableName"].ToString() : "0");
            checkEndingValue.Checked = false;
            numericUpDownEndingValue.Enabled = false;
            numericUpDownStep.Enabled = false;
            numericUpDownStartingValue.Text = "";
            numericUpDownEndingValue.Text = "";
            numericUpDownStep.Text = "";
            labelPercStart.Visible = false;
            labelPercEnd.Visible = false;
            labelPercStep.Visible = false;
        }

        public void setSingleValue(string x)
        {
            setEmpty();
            numericUpDownStartingValue.Text = x;
        }

        internal string getDisplayText()
        {
            return GetDisplayText(EditValue, isRange, Plugin);
        }

        public static string GetDisplayText(object value, bool isRange, Program Plugin)
        {
            if (value == null || value.ToString() == "" || !value.ToString().Contains(SEPARATOR))
            {
                return "";
            }
            else
            {
                try
                {
                    string[] vals = value.ToString().Split(SEPARATOR);
                    bool isRef = vals[0] == TYPE_REFERENCE;
                    string per = isRef ? "%" : "";
                    string val = vals[2] + per;
                    if (isRange && vals.Length > 3)
                    {
                        if (vals[3] == true.ToString()) val += "-" + vals[4] + per;
                    }
                    if (isRef)
                    {
                        string tableName = "N/A";
                        DataRow[] r = Plugin.referenceTablesData.ReferenceTables.Select("TableName='" + vals[1] + "'");
                        if (r.Length == 1) tableName = r[0].Field<string>("TableDescription");
                        val += " of " + tableName;
                    }
                    return val;
                }
                catch
                {
                    // in case of conversion error, just return "". This can happen for example if you edit the Advanced Variables and change the VariableType
                    return "";
                }
            }
        }

        private void checkEndingValue_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox c = sender as CheckBox;
            numericUpDownEndingValue.Enabled = c.Checked;
            numericUpDownStep.Enabled = c.Checked;
        }

        private void radioButtonValue_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton r = sender as RadioButton;
            lookUpEditReference.Enabled = !r.Checked;
            labelPercStart.Visible = !r.Checked;
            labelPercEnd.Visible = isRange && !r.Checked;
            labelPercStep.Visible = isRange && !r.Checked;
        }

        private void radioButtonReference_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton r = sender as RadioButton;
            lookUpEditReference.Enabled = r.Checked;
            labelPercStart.Visible = r.Checked;
            labelPercEnd.Visible = isRange && r.Checked;
            labelPercStep.Visible = isRange && r.Checked;
        }

        internal bool isRangedValue()
        {
            return checkEndingValue.Checked;
        }

        internal double getStartingValue()
        {
            return (double)numericUpDownStartingValue.Value;
        }

        internal double getEndingValue()
        {
            return (double)numericUpDownEndingValue.Value;
        }

        internal double getStepValue()
        {
            return (double)numericUpDownStep.Value;
        }

        internal bool isReference()
        {
            return radioButtonReference.Checked;
        }

        internal string getReferenceTable()
        {
            return lookUpEditReference.EditValue.ToString();
        }

        public class CustomNumericUpDown : NumericUpDown
        {
            //Override this to format the displayed text
            protected override void UpdateEditText()
            {
                Text = Value.ToString("0." + new string('#', DecimalPlaces));
            }
        }
    }
}
