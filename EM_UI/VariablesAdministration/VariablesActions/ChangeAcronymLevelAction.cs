using DevExpress.XtraTreeList;
using EM_UI.DataSets;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeAcronymLevelAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        VarConfigFacade _varConfigFacade = null;
        CellValueChangedEventArgs _eventArgs = null;

        internal ChangeAcronymLevelAction(VariablesForm variablesForm, CellValueChangedEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _varConfigFacade = _variablesForm._varConfigFacade;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            string newValue = _eventArgs.Value.ToString();

            VarConfig.AcronymLevelRow levelRow = _eventArgs.Node.Tag as VarConfig.AcronymLevelRow;

            if (newValue == levelRow.Name)
                return false; //only change if different

            levelRow.Name = newValue;

            return true;
        }
    }
}
