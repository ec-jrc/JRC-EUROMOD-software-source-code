using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Linq;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class AddCategoryAction : VariablesBaseAction
    {
        AcronymManager _acronymManager = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;
        bool categStateChanged = false;

        internal AddCategoryAction(VariablesForm variablesForm)
        {
            _acronymManager = variablesForm._acronymManager;
            _treeAcronyms = variablesForm.treeAcronyms;
            _varConfigFacade = variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (_treeAcronyms.FocusedNode == null || !AcronymManager.IsAcronymNode(_treeAcronyms.FocusedNode))
                return false;

            VarConfig.AcronymRow acronymRow = _treeAcronyms.FocusedNode.Tag as VarConfig.AcronymRow;

            categStateChanged = acronymRow.GetCategoryRows().Count() == 0;

            _varConfigFacade.AddCategoryRow(acronymRow);

            _acronymManager.FillCategoriesList(_treeAcronyms.FocusedNode);
                        
            return true;
        }

        internal override bool UpdateVariables()
        {
            return categStateChanged; // by adding a first category, the categorical-state in the variables list is changed
        }
    }
}
