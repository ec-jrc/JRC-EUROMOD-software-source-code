using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;
using EM_UI.Dialogs;

namespace EM_UI.Actions
{
    internal class ConfigSystemsAction : BaseAction
    {
        CountryConfig.SystemDataTable _systemDataTable = null;
        string _countryShortName = string.Empty;
        bool _actionIsCanceled = false;

        internal ConfigSystemsAction(string countryShortName, CountryConfig.SystemDataTable systemDataTable)
        { 
            _countryShortName = countryShortName;
            _systemDataTable = systemDataTable;
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
            ConfigureSystemsForm configureSystemsForm = new ConfigureSystemsForm(_countryShortName, _systemDataTable);
            if (configureSystemsForm.ShowDialog() == DialogResult.Cancel)
                _actionIsCanceled = true;
        }
    }
}
