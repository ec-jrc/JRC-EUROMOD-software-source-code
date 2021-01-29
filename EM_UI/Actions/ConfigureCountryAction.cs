using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;
using EM_UI.Dialogs;

namespace EM_UI.Actions
{
    internal class ConfigCountryAction : BaseAction
    {
        CountryConfig.CountryRow _countryRow = null;
        string _countryShortName = string.Empty;
        bool _actionIsCanceled = false;

        internal ConfigCountryAction(string countryShortName, CountryConfig.CountryRow countryRow)
        {
            _countryShortName = countryShortName;
            _countryRow = countryRow;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            ConfigureCountryForm configureCountryForm = new ConfigureCountryForm(_countryShortName, _countryRow);
            if (configureCountryForm.ShowDialog() == DialogResult.Cancel)
                _actionIsCanceled = true;
        }
    }
}
