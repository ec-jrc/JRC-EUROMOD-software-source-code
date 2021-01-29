using EM_Common;
using EM_UI.DataSets;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeCategoryAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        DataGridView _dgvCategories = null;
        DataGridViewCellEventArgs _eventArgs;

        internal ChangeCategoryAction(VariablesForm variablesForm, DataGridViewCellEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _dgvCategories = _variablesForm.dgvCategories;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            DataGridViewRow changedRow = _dgvCategories.Rows[_eventArgs.RowIndex];
            VarConfig.CategoryRow categoryRow = changedRow.Tag as VarConfig.CategoryRow;
            string newValue = changedRow.Cells[_eventArgs.ColumnIndex].Value.ToString();

            if (_eventArgs.ColumnIndex == _dgvCategories.Columns.IndexOf(_variablesForm.colCategoryValue))
            {
                if (categoryRow.Value == newValue)
                    return false; //only change if different

                if (!EM_Helpers.IsNumeric(newValue))
                {
                    if (Tools.UserInfoHandler.GetInfo(newValue + " is not numeric.\n\nUndo change?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _variablesForm._acronymManager.FillCategoriesList(_variablesForm.treeAcronyms.FocusedNode);
                        return false;
                    }
                }

                categoryRow.Value = newValue;
            }
            else if (_eventArgs.ColumnIndex == _dgvCategories.Columns.IndexOf(_variablesForm.colCategoryDescription))
            {
                if (categoryRow.Description == newValue)
                    return false; //only change if different

                categoryRow.Description = newValue;
            }
            
            return true;
        }
    }
}
