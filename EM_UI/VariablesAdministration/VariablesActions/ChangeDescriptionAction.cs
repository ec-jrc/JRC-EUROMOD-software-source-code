using EM_UI.DataSets;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeDescriptionAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        DataGridView _dgvDescriptions = null;
        VarConfigFacade _varConfigFacade = null;
        DataGridViewCellEventArgs _eventArgs = null;

        internal ChangeDescriptionAction(VariablesForm variablesForm, DataGridViewCellEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _dgvDescriptions = _variablesForm.dgvDescriptions;
            _varConfigFacade = _variablesForm._varConfigFacade;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            DataGridViewRow changedRow = _dgvDescriptions.Rows[_eventArgs.RowIndex];
            VarConfig.CountryLabelRow countryLabelRow = changedRow.Tag as VarConfig.CountryLabelRow;

            string changedVal = changedRow.Cells[_eventArgs.ColumnIndex].Value == null ? string.Empty :
                                changedRow.Cells[_eventArgs.ColumnIndex].Value.ToString();
            if (countryLabelRow.Label != changedVal)
            {//only (change and) return true if actually changed
                countryLabelRow.Label = changedVal;
                return true;
            }
            return false;
        }
    }
}
