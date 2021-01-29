using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class DeleteAcronymTypeAction : VariablesBaseAction
    {
        AcronymManager _acronymManager = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        internal DeleteAcronymTypeAction(VariablesForm variablesForm)
        {
            _acronymManager = variablesForm._acronymManager;
            _treeAcronyms = variablesForm.treeAcronyms;
            _varConfigFacade = variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (!AcronymManager.IsTypeNode(_treeAcronyms.FocusedNode))
                return false;

            VarConfig.AcronymTypeRow typeRow = _treeAcronyms.FocusedNode.Tag as VarConfig.AcronymTypeRow;

            string usage = string.Empty;
            List<VarConfig.AcronymRow> usedAcronymRows = new List<VarConfig.AcronymRow>();
            foreach (VarConfig.AcronymRow acronymRow in _varConfigFacade.GetAcronymsOfType(typeRow.ShortName))
            {
                string usingVariables = _acronymManager.GetVariablesUsingAcronym(acronymRow.Name, typeRow.ShortName);
                if (usingVariables != string.Empty)
                {
                    usedAcronymRows.Add(acronymRow);
                    usage += acronymRow.Name + " used by " + usingVariables + Environment.NewLine;
                }
            }

            if (usage != string.Empty)
            {
                if (usage.Length > 2000) //probably this will happen only per accident, if a user tries to delete types like tax or benefit/pension (MessageBox is then too large for the screen)
                    usage = usage.Substring(0, 2000) + Environment.NewLine + Environment.NewLine + "etc., etc., etc.";

                if (Tools.UserInfoHandler.GetInfo("The following acronyms of this type are used:" + Environment.NewLine + usage +
                            Environment.NewLine + Environment.NewLine + "Cancel delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;

                foreach (VarConfig.AcronymRow usedAcronymRow in usedAcronymRows)
                    usedAcronymRow.Description = VariablesManager.DESCRIPTION_UNKNOWN; //temporarily rename, to allow for updating automatic labels
                typeRow.LongName = VariablesManager.DESCRIPTION_UNKNOWN;
                _acronymManager.UpdateAutomaticLabelForSpecificAcronyms(usedAcronymRows);
            }

            typeRow.Delete();

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

        internal override bool UpdateFilterCheckboxes()
        {
            return true;
        }
    }
}
