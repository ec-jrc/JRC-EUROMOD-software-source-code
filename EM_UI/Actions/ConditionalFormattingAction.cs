using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using EM_UI.DataSets;
using EM_UI.Dialogs;

namespace EM_UI.Actions
{
    internal class ConditionalFormattingAction : BaseAction
    {
        CountryConfigFacade _countryConfigFacade = null;
        string _countryShortName = string.Empty;
        bool _actionIsCanceled = false;
        ConditionalFormattingForm _conditionalFormattingForm = null;

        internal ConditionalFormattingAction(string countryShortName, CountryConfigFacade countryConfigFacade)
        {
            _countryShortName = countryShortName;
            _countryConfigFacade = countryConfigFacade;
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
            _conditionalFormattingForm = new ConditionalFormattingForm(_countryShortName, _countryConfigFacade);
            if (_conditionalFormattingForm.ShowDialog() == DialogResult.Cancel)
                _actionIsCanceled = true;
        }
       
        internal List<KeyValuePair<string, string>> GetSystemsToExpand()
        {
            return _conditionalFormattingForm._systemsToExpand;
        }
    }
}
