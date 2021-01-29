using EM_Common;
using EM_UI.DataSets;
using System;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ConfigureCountryForm : Form
    {
        CountryConfig.CountryRow _countryRow = null;

        void ConfigureCountryForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            //outcommented, mainly because it is not true for add-ons, but also as there is no obvious reason to check for this
            //if (txtShortName.Text.Length != 2)
            //{
            //    if (Tools.UserInfoHandler.GetInfo("Short Name is supposed to have two characters. Do you want to correct?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //        return;
            //}

            if (_countryRow.ShortName != txtShortName.Text)
                if (!OptionalWarningsManager.Show(OptionalWarningsManager._changeCountryShortNameWarning))
                    return;

            _countryRow.Name = txtLongName.Text;
            //_countryRow.ShortName = txtShortName.Text; //country short name cannot be changed (anymore) as this leads to crashes (see County.cs function CheckForCorrespondenceOfCountryShortName)
            _countryRow.Private = chkPrivate.Checked ? DefPar.Value.YES : DefPar.Value.NO;

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal ConfigureCountryForm(string countryShortName, CountryConfig.CountryRow countryRow)
        {
            InitializeComponent();

            _countryRow = countryRow;
            labelCountry.Text = countryShortName;

            txtLongName.Text = countryRow.Name;
            txtShortName.Text = countryRow.ShortName;

            chkPrivate.Checked = countryRow.Private == DefPar.Value.YES;
        }
    }
}
