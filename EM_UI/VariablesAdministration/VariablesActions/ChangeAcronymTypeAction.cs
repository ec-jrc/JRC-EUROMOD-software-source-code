using DevExpress.XtraTreeList;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class ChangeAcronymTypeAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        AcronymManager _acronymManager = null;
        VarConfigFacade _varConfigFacade = null;
        CellValueChangedEventArgs _eventArgs = null;
        bool _updateVariables = true;
        bool _updateFilterCheckboxes = true;

        internal ChangeAcronymTypeAction(VariablesForm variablesForm, CellValueChangedEventArgs eventArgs)
        {
            _variablesForm = variablesForm;
            _acronymManager = variablesForm._acronymManager;
            _varConfigFacade = _variablesForm._varConfigFacade;
            _eventArgs = eventArgs;
        }

        internal override bool Perform()
        {
            string newValue = _eventArgs.Value.ToString();

            VarConfig.AcronymTypeRow typeRow = _eventArgs.Node.Tag as VarConfig.AcronymTypeRow;

            string oldTypeName = string.Empty;

            //change of type-description (long name)
            if (_eventArgs.Column.Name == _variablesForm.colAcronymDescription.Name)
            {
                if (newValue == typeRow.LongName)
                    return false; //only change if different

                if (!DoChangeIfUsed(typeRow.ShortName, newValue))
                    return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables or filter-checkboxes)

                typeRow.LongName = newValue;
            }

            //change of type's short name
            else
            {
                if (newValue == typeRow.ShortName)
                    return false; //only change if different

                if (!DoChangeIfUsed(typeRow.ShortName, newValue))
                    return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables or filter-checkboxes)

                oldTypeName = typeRow.ShortName;

                //check for correctness
                if (_varConfigFacade.GetTypeShortNames().Contains(newValue.ToLower()))
                {
                    Tools.UserInfoHandler.ShowError("Acronym type already exists. Please choose another short name.");
                    _updateVariables = false;
                    _updateFilterCheckboxes = false;
                    return true; //return true to allow for redraw, i.e. show the old value still stored in the datarow (but only update acronyms, no need to update variables or filter-checkboxes)
                }

                //finally reflect change in datarow
                typeRow.ShortName = newValue;
            }

            //update automatic label of variables concerned
            List<KeyValuePair<string, string>> updateLabelAcronyms = new List<KeyValuePair<string, string>>();
            foreach (VarConfig.AcronymLevelRow levelRow in typeRow.GetAcronymLevelRows())
                foreach (VarConfig.AcronymRow acroRow in levelRow.GetAcronymRows())
                    if (acroRow.Name != string.Empty)
                    {
                        updateLabelAcronyms.Add(new KeyValuePair<string, string>(acroRow.Name, typeRow.ShortName)); //variables already using the new acro-type now get a valid auto-label
                        if (oldTypeName != string.Empty)
                            updateLabelAcronyms.Add(new KeyValuePair<string, string>(acroRow.Name, oldTypeName)); //variables, which used the old acro-type get an invalid auto-label 
                    }
            _variablesForm._acronymManager.UpdateAutomaticLabelForSpecificAcronyms(updateLabelAcronyms);

            return true;
        }

        bool DoChangeIfUsed(string typeShortName, string newValue)
        {
            string usage = string.Empty;
            foreach (VarConfig.AcronymRow acronymRow in _varConfigFacade.GetAcronymsOfType(typeShortName))
            {
                string usingVariables = _acronymManager.GetVariablesUsingAcronym(acronymRow.Name, typeShortName);
                if (usingVariables != string.Empty)
                    usage += acronymRow.Name + " used by " + usingVariables + "\n";
            }
            if (usage != string.Empty)
            {
                if (usage.Length > 2000) //probably this will happen only per accident, if a user tries to change types like tax or benefit/pension (MessageBox is then too large for the screen)
                    usage = usage.Substring(0, 2000) + "\n\netc., etc., etc.";

                if (Tools.UserInfoHandler.GetInfo("The following acronyms of this type are used:\n" + usage + "\n\nUndo change?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _updateVariables = false;
                    _updateFilterCheckboxes = false;
                    return false;
                }
            }
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

        internal override bool UpdateFilterCheckboxes()
        {
            return _updateFilterCheckboxes;
        }
    }
}
