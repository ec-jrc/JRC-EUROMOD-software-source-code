using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionSwitchDefaultValue
    {
        internal string _extensionID;
        internal DataConfig.DBSystemConfigRow _dbSystemConfigRow;
        internal string _defaultValue;

        internal ExtensionSwitchDefaultValue(string extensionID, DataConfig.DBSystemConfigRow dbSystemConfigRow, string defaultValue)
        {
            _extensionID = extensionID;
            _dbSystemConfigRow = dbSystemConfigRow;
            _defaultValue = defaultValue;
        }
    }

    internal class ExtensionSetSwitchesAction : BaseAction
    {
        string _cc = string.Empty;
        bool _actionIsCanceled = false;

        internal ExtensionSetSwitchesAction(string cc)
        {
            _cc = cc;
        }

        internal override bool ShowHiddenSystemsWarning() {  return false; }
        internal override bool ActionIsCanceled() { return _actionIsCanceled; }

        internal override void PerformAction()
        {
            SetExtensionSwitchesForm setSwitchesDialog = new SetExtensionSwitchesForm(_cc);
            if (setSwitchesDialog.ShowDialog() == DialogResult.Cancel) { _actionIsCanceled = true; return; }

            //store the redefined switches in the data-config
            foreach (ExtensionSwitchDefaultValue extensionSwitchDefaultValue in setSwitchesDialog.GetExtensionDefaultSwitches())                   
                ExtensionAndGroupManager.SetExtensionDefaultSwitch(CountryAdministrator.GetDataConfigFacade(_cc).GetDataConfig(),
                    extensionSwitchDefaultValue._dbSystemConfigRow, extensionSwitchDefaultValue._extensionID, extensionSwitchDefaultValue._defaultValue);

            //remove any "relics", i.e. default values of switchable policies, which were deleted
            ExtensionAndGroupManager.ExtensionDefaultSwitches_RemoveRelics(_cc);
        }
    }
}
