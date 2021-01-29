using EM_UI.DataSets;
using System;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class AddCountry_SaveAs_AdaptForm : Form
    {
        internal SaveAsAdaptOptions options = new SaveAsAdaptOptions();

        internal AddCountry_SaveAs_AdaptForm(string cc, SaveAsAdaptOptions _options)
        {
            InitializeComponent();

            // disable checks for adapting global files if they do not contain entries for the original country
            ExchangeRatesConfigFacade ercf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
            chkCloneExRates.Enabled = ercf != null && ercf.HasExchangeRates(cc);
            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false);
            chkCloneHICP.Enabled = hcf != null && hcf.HasHICP(cc);

            options = _options;
            chkAdaptSystemNames.Checked = options.adaptSystemNames;
            chkAdaptPolicyNames.Checked = options.adaptPolicyNames;
            chkAdaptTUNames.Checked = options.adaptTUNames;
            chkAdaptOutputFileNames.Checked = options.adaptOutputFileNames;
            chkAdaptComments.Checked = options.adaptComments;
            chkCloneHICP.Checked = options.cloneHICP == true;
            chkCloneExRates.Checked = options.cloneExRates == true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            options.adaptSystemNames = chkAdaptSystemNames.Checked;
            options.adaptPolicyNames = chkAdaptPolicyNames.Checked;
            options.adaptTUNames = chkAdaptTUNames.Checked;
            options.adaptOutputFileNames = chkAdaptOutputFileNames.Checked;
            options.adaptComments = chkAdaptComments.Checked;

            options.cloneHICP = chkCloneHICP.Checked;
            options.cloneExRates = chkCloneExRates.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal class SaveAsAdaptOptions
    {
        internal bool adaptSystemNames = true;
        internal bool adaptPolicyNames = true;
        internal bool adaptTUNames = true;
        internal bool adaptOutputFileNames = true;
        internal bool adaptComments = false;

        internal bool? cloneHICP = null;
        internal bool? cloneExRates = null;
    }
}
