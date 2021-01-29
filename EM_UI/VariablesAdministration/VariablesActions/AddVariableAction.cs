using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class AddVariableAction : VariablesBaseAction
    {
        DataGridView _dgvVariables = null;
        VarConfigFacade _varConfigFacade = null;

        internal AddVariableAction(DataGridView dgvVariables, VarConfigFacade varConfigFacade)
        {
            _dgvVariables = dgvVariables;
            _varConfigFacade = varConfigFacade;
        }

        internal override bool Perform()
        {
            VarConfig.VariableRow variableRow = _varConfigFacade.AddVariable();

            int index = 0;
            if (_dgvVariables.SelectedRows.Count == 1)
                index = _dgvVariables.Rows.IndexOf(_dgvVariables.SelectedRows[0]) + 1;
            _dgvVariables.Rows.Insert(index, variableRow.Name, VariablesManager.IsMonetary(variableRow),
                                      VariablesManager.IsHHLevel(variableRow), false, variableRow.AutoLabel);
            _dgvVariables.Rows[index].Tag = variableRow;
            _dgvVariables.Rows[index].Cells[0].Selected = true; //set input focus to just added row (selecting row instead of cell does not work)
            _dgvVariables.Select(); //grid view must be selected too, otherwise another list may have the input focus
            return true;
        }
    }
}
