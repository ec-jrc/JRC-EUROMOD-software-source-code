using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class DeleteVariableAction : VariablesBaseAction
    {
        DataGridView _dgvVariables = null;
        VarConfigFacade _varConfigFacade = null;

        internal DeleteVariableAction(DataGridView dgvVariables, VarConfigFacade varConfigFacade)
        {
            _dgvVariables = dgvVariables;
            _varConfigFacade = varConfigFacade;
        }

        internal override bool Perform()
        {
            if (_dgvVariables.SelectedRows.Count != 1)
                return false;

            int index = _dgvVariables.Rows.IndexOf(_dgvVariables.SelectedRows[0]);
            VarConfig.VariableRow variableRow = _dgvVariables.Rows[index].Tag as VarConfig.VariableRow;

            if (!_varConfigFacade.IsNewRow(variableRow.ID))
                if (Tools.UserInfoHandler.GetInfo("You are deleting a variable that might be used in country implementations. Please consider using the 'clean variables' option to check usage." +
                    Environment.NewLine + Environment.NewLine + "Cancel deleting variable?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;

            _varConfigFacade.DeleteVariable(variableRow);

            _dgvVariables.Rows.Remove(_dgvVariables.SelectedRows[0]);
            return true;
        }
    }
}
