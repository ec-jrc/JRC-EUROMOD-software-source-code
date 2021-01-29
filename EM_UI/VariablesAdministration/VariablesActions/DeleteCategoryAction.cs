using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class DeleteCategoryAction : VariablesBaseAction
    {
        AcronymManager _acronymManager = null;
        TreeList _treeAcronyms = null;
        DataGridView _dgvCategories = null;
        VarConfigFacade _varConfigFacade = null;
        bool categStateChanged = false;

        internal DeleteCategoryAction(VariablesForm variablesForm)
        {
            _acronymManager = variablesForm._acronymManager;
            _treeAcronyms = variablesForm.treeAcronyms;
            _dgvCategories = variablesForm.dgvCategories;
            _varConfigFacade = variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (_dgvCategories.SelectedRows.Count == 0)
                return false;

            categStateChanged = _dgvCategories.Rows.Count == _dgvCategories.SelectedRows.Count;

            foreach (DataGridViewRow row in _dgvCategories.SelectedRows)
            {
                VarConfig.CategoryRow categoryRow = row.Tag as VarConfig.CategoryRow;
                categoryRow.Delete();
            }

            _acronymManager.FillCategoriesList(_treeAcronyms.FocusedNode);

            return true;
        }

        internal override bool UpdateVariables()
        {
            return categStateChanged; // by deleting the last category the categorical-state in the variables list is changed
        }
    }
}
