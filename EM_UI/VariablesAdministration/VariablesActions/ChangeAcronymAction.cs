using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeAcronymAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        AcronymManager _acronymManager = null;
        VarConfigFacade _varConfigFacade = null;
        CellValueChangedEventArgs _eventArgs = null;
        bool _updateVariables = true;

        internal ChangeAcronymAction(VariablesForm variablesForm, CellValueChangedEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _acronymManager = variablesForm._acronymManager;
            _varConfigFacade = _variablesForm._varConfigFacade;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            string newValue = _eventArgs.Value.ToString();

            VarConfig.AcronymRow acronymRow = _eventArgs.Node.Tag as VarConfig.AcronymRow;

            List<KeyValuePair<string, string>> updateLabelAcronyms = new List<KeyValuePair<string, string>>();

            //change of acronym-description
            if (_eventArgs.Column.Name == _variablesForm.colAcronymDescription.Name)
            {
                if (newValue == acronymRow.Description)
                    return false; //only change if different

                acronymRow.Description = newValue;
                updateLabelAcronyms.Add(new KeyValuePair<string, string>(acronymRow.Name, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName));
            }

            //change of acronym itself
            else
            {
                if (newValue == acronymRow.Name)
                    return false; //only change if different

                //check for usage
                string usingVariables = _acronymManager.GetVariablesUsingAcronym(acronymRow.Name, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName);
                if (usingVariables != string.Empty)
                {
                    if (Tools.UserInfoHandler.GetInfo("Acronym is used by variable(s) " + usingVariables + "\n\nUndo change?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _updateVariables = false;
                        return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables)
                    }
                }
            
                //check for correctness
                if (newValue.Length != 2)
                {
                    Tools.UserInfoHandler.ShowError("Acronym must have two characters. Please correct.");
                    _updateVariables = false;
                    return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables)
                }
                if (newValue.Contains('#'))
                {
                    Tools.UserInfoHandler.ShowError("Character # is reserved for footnotes and therefore cannot be used for acronyms. Please correct.");
                    _updateVariables = false;
                    return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables)
                }

                string acroType = _eventArgs.Node.ParentNode.ParentNode.GetDisplayText(_variablesForm.colAcronym);
                foreach (VarConfig.AcronymRow silblingRow in _varConfigFacade.GetAcronymsOfType(acroType))
                    if (silblingRow.Name.ToLower() == newValue.ToLower())
                    {
                        Tools.UserInfoHandler.ShowError("Acronym already exists (see level " + silblingRow.AcronymLevelRow.Name + "). Please correct.");
                        _updateVariables = false;
                        return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables)
                    }

                //care for updating automatic labels of variables concerned
                updateLabelAcronyms.Add(new KeyValuePair<string, string>(newValue, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName)); //variables already using the new acro-name now get a valid auto-label 
                if (usingVariables != string.Empty)
                    updateLabelAcronyms.Add(new KeyValuePair<string, string>(acronymRow.Name, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName)); //variables, which used the old acro-name get an invalid auto-label 

                //finally reflect change in datarow
                acronymRow.Name = newValue;
            }

            //update automatic label of variables concerned
            _variablesForm._acronymManager.UpdateAutomaticLabelForSpecificAcronyms(updateLabelAcronyms);

            return true;
        }

        internal override bool UpdateVariables()
        {
            return _updateVariables;
        }

        internal override bool UpdateAcronyms()
        {
            return true;
        }
    }
}
