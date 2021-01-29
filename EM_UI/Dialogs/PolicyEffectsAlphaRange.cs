using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    public partial class PolicyEffectsAlphaRange : Form
    {
        const int ALPHA_RANGE_MAX = 500, ALPHA_RANGE_MAX_SHOW = 50; // for warnings/errors with respect to the range chosen by the user

        double valStart = 0, valEnd = 0, valStep = 0;

        public PolicyEffectsAlphaRange()
        {
            InitializeComponent();
        }

        public void GetStartEndStep(out double start, out double end, out double step)
        {
            start = valStart; end = valEnd; step = valStep;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        public static bool GetRangeValues(out List<double> rangeValues, double start, double end, double step, bool showResults = false)
        {
            rangeValues = new List<double>();

            if (start > end) { UserInfoHandler.ShowError("Start must not be higher than end (" + start.ToString() + ">" + end.ToString() + ")"); return false; }

            int absMaxCount = showResults ? ALPHA_RANGE_MAX_SHOW // template allows for 50+2 (the 2 are for CPI+MII,
                                          : ALPHA_RANGE_MAX;     // thus in fact it'll be 52 if they are not selected)
            for (double d = start; d <= end && rangeValues.Count <= ALPHA_RANGE_MAX + 1; d += step)
            {
                if (d == 0)
                {
                    if (UserInfoHandler.GetInfo("The range contains zero." + Environment.NewLine + "Should zero be skipped?",
                        MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel) return false;
                }
                else rangeValues.Add(d);
            }

            if (rangeValues.Count == 0) { UserInfoHandler.ShowError("The range contains no numbers."); return false; }
            if (rangeValues.Count <= ALPHA_RANGE_MAX_SHOW) return true; // consider 50 as 'reasonable' (?) and do not issue a warning

            // if template is not shown, allow for 500, but still warn if more than 50; if template is shown 50 is max, as the template does not allow for more (see above)
            string cnt = rangeValues.Count <= ALPHA_RANGE_MAX + 1 ? rangeValues.Count.ToString() : "too many";
            string message = "The range contains " + cnt + " numbers." + Environment.NewLine
                           + "Allowed are " + ALPHA_RANGE_MAX_SHOW + " for 'Run Only' and " + ALPHA_RANGE_MAX + " for 'Run & Show Results'.";
            if (rangeValues.Count > absMaxCount) { UserInfoHandler.ShowError(message); return false; }
            return UserInfoHandler.GetInfo(message + Environment.NewLine + "Do you want to continue?", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateTextBox(txtStart, out valStart)) return;
            if (!ValidateTextBox(txtEnd, out valEnd)) return;
            if (!ValidateTextBox(txtStep, out valStep)) return;

            List<double> dummy;
            if (!GetRangeValues(out dummy, valStart, valEnd, valStep)) return;

            DialogResult = DialogResult.OK; Close();
        }

        private bool ValidateTextBox(TextBox tb, out double val)
        {
            if (string.IsNullOrEmpty(tb.Text) || !EM_Helpers.TryConvertToDouble(tb.Text, out val) || tb.Text.Contains(','))
            {
                UserInfoHandler.ShowError(tb.Name.Replace("txt", string.Empty) + " must be a valid number.");
                val = 0; return false;
            }
            return true;
        }
    }
}
