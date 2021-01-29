using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeVariableAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        DataGridView _dgvVariables = null;
        VarConfigFacade _varConfigFacade = null;
        DataGridViewCellEventArgs _eventArgs = null;

        internal ChangeVariableAction(VariablesForm variablesForm, DataGridViewCellEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _dgvVariables = _variablesForm.dgvVariables;
            _varConfigFacade = _variablesForm._varConfigFacade;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            DataGridViewRow changedRow = _dgvVariables.Rows[_eventArgs.RowIndex];
            VarConfig.VariableRow variableRow = changedRow.Tag as VarConfig.VariableRow;

            if (_eventArgs.ColumnIndex == _dgvVariables.Columns.IndexOf(_variablesForm.colVariableName))
            {//variables name edited
                if (variableRow.Name != changedRow.Cells[_eventArgs.ColumnIndex].Value.ToString())
                {//only (change and) return true if actually changed
                    variableRow.Name = changedRow.Cells[_eventArgs.ColumnIndex].Value.ToString();
                    int columnIndexAutoLabel = _variablesForm.dgvVariables.Columns.IndexOf(_variablesForm.colAutomaticLabel);
                    variableRow.AutoLabel = changedRow.Cells[columnIndexAutoLabel].Value.ToString();
                    return true;
                }
            }

            if (_eventArgs.ColumnIndex == _dgvVariables.Columns.IndexOf(_variablesForm.colMonetary))
            {//checkbox monetary edited
                bool newValue_IsMonetary = (bool)changedRow.Cells[_eventArgs.ColumnIndex].Value;
                if (VariablesManager.IsMonetary(variableRow) && !newValue_IsMonetary)
                {//current value monetary, new value non-monetary
                    variableRow.Monetary = "0";
                    return true;
                }
                if (!VariablesManager.IsMonetary(variableRow) && newValue_IsMonetary)
                {//current value non-monetary, new value monetary
                    variableRow.Monetary = "1";
                    return true;
                }
            }

            if (_eventArgs.ColumnIndex == _dgvVariables.Columns.IndexOf(_variablesForm.colHHLevel))
            {//checkbox HH Level edited
                bool newValue_IsHHLevel = (bool)changedRow.Cells[_eventArgs.ColumnIndex].Value;
                if (VariablesManager.IsHHLevel(variableRow) && !newValue_IsHHLevel)
                {//current value hh-level, new value ind-level
                    variableRow.HHLevel = "0";
                    return true;
                }
                if (!VariablesManager.IsHHLevel(variableRow) && newValue_IsHHLevel)
                {//current value ind-level, new value hh-level
                    variableRow.HHLevel = "1";
                    return true;
                }
            }

            return false;
        }
    }
}
