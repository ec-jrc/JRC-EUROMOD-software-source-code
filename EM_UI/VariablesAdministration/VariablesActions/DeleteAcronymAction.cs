using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class DeleteAcronymAction : VariablesBaseAction
    {
        AcronymManager _acronymManager = null;
        TreeList _treeAcronyms = null;

        internal DeleteAcronymAction(VariablesForm variablesForm)
        {
            _acronymManager = variablesForm._acronymManager;
            _treeAcronyms = variablesForm.treeAcronyms;
        }

        internal override bool Perform()
        {
            if (!AcronymManager.IsAcronymNode(_treeAcronyms.FocusedNode))
                return false;

            VarConfig.AcronymRow acronymRow = _treeAcronyms.FocusedNode.Tag as VarConfig.AcronymRow;

            string usingVariables = _acronymManager.GetVariablesUsingAcronym(acronymRow.Name, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName);
            if (usingVariables != string.Empty)
            {
                if (Tools.UserInfoHandler.GetInfo("Acronym is used by variable(s) " + usingVariables + "\n\nCancel delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;

                List<VarConfig.AcronymRow> usedAcronyms = new List<VarConfig.AcronymRow>();
                usedAcronyms.Add(acronymRow);
                acronymRow.Description = VariablesManager.DESCRIPTION_UNKNOWN; //temporarily rename, to allow for updating automatic labels
                _acronymManager.UpdateAutomaticLabelForSpecificAcronyms(usedAcronyms);
            }
            
            acronymRow.Delete();
            return true;
        }

        internal override bool UpdateVariables()
        {
            return true;
        }

        internal override bool UpdateAcronyms()
        {
            return true;
        }
    }
}
