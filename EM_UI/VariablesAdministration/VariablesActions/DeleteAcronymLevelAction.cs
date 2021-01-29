using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class DeleteAcronymLevelAction : VariablesBaseAction
    {
        AcronymManager _acronymManager = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        internal DeleteAcronymLevelAction(VariablesForm variablesForm)
        {
            _acronymManager = variablesForm._acronymManager;
            _treeAcronyms = variablesForm.treeAcronyms;
            _varConfigFacade = variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (!AcronymManager.IsLevelNode(_treeAcronyms.FocusedNode))
                return false;

            VarConfig.AcronymLevelRow levelRow = _treeAcronyms.FocusedNode.Tag as VarConfig.AcronymLevelRow;

            string usage = string.Empty;
            List<VarConfig.AcronymRow> usedAcronymRows = new List<VarConfig.AcronymRow>();
            foreach (VarConfig.AcronymRow acronymRow in _varConfigFacade.GetAcronymsOfType(levelRow.AcronymTypeRow.ShortName))
            {
                if (levelRow.Name.ToLower() != acronymRow.AcronymLevelRow.Name.ToLower())
                    continue;
                string usingVariables = _acronymManager.GetVariablesUsingAcronym(acronymRow.Name, levelRow.AcronymTypeRow.ShortName);
                if (usingVariables != string.Empty)
                {
                    usedAcronymRows.Add(acronymRow);
                    usage += acronymRow.Name + " used by " + usingVariables + "\n";
                }
            }

            if (usage != string.Empty)
            {
                if (Tools.UserInfoHandler.GetInfo("The following acronyms of this level are used:\n" + usage + "\nCancel delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;

                foreach (VarConfig.AcronymRow usedAcronymRow in usedAcronymRows)
                    usedAcronymRow.Description = VariablesManager.DESCRIPTION_UNKNOWN; //temporarily rename, to allow for updating automatic labels
                _acronymManager.UpdateAutomaticLabelForSpecificAcronyms(usedAcronymRows);
            }

            levelRow.Delete();

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
