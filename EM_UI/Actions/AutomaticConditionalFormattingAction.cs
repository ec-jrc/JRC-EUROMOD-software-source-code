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
    internal class AutomaticConditionalFormattingAction : BaseAction
    {
        CountryConfigFacade _countryConfigFacade = null;
        bool _resetFormatting = false;

        internal AutomaticConditionalFormattingAction(CountryConfigFacade countryConfigFacade, bool resetFormatting = false)
        {
            _countryConfigFacade = countryConfigFacade;
            _resetFormatting = resetFormatting;
        }

        internal override void PerformAction()
        {
            _countryConfigFacade.setAutomaticConditionalFormatting(_resetFormatting);
        }
    }
}
