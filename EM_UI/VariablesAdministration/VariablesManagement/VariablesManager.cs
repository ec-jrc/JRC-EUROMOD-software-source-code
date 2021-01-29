using DevExpress.XtraBars;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.VariablesAdministration.VariablesActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesManagement
{
    internal class VariablesManager
    {
        VariablesForm _variablesForm;
        DataGridView _dgvVariables = null;
        DataGridView _dgvDescriptions = null;
        VarConfigFacade _varConfigFacade = null;

        internal const string DESCRIPTION_UNKNOWN = "????";

        internal VariablesManager(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _dgvVariables = _variablesForm.dgvVariables;
            _dgvDescriptions = _variablesForm.dgvDescriptions;
            _varConfigFacade = _variablesForm._varConfigFacade;
        }

        internal void FillVariablesList()
        {
            _dgvVariables.SuspendLayout(); //hopefully that enhences speed

            //store which row is selected and which row is the first displayed for restoring after refilling
            string selectedRowID = string.Empty;
            string firstDisplayedRowID = string.Empty;
            StoreRowStates(ref selectedRowID, ref firstDisplayedRowID);
                
            _dgvVariables.Rows.Clear();

            Dictionary<string, bool> isCateg = GetVariablesCategState();
            
            foreach (VarConfig.VariableRow variableRow in _varConfigFacade.GetVariables())
            {
                int index = _dgvVariables.Rows.Add(variableRow.Name, IsMonetary(variableRow), IsHHLevel(variableRow), isCateg[variableRow.ID], variableRow.AutoLabel);
                _dgvVariables.Rows[index].Tag = variableRow;
            }
            _variablesForm.colVariableName.Width = _variablesForm.colVariableName.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            _variablesForm.colMonetary.Width = _variablesForm.colMonetary.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            _variablesForm.colAutomaticLabel.Width = _variablesForm.colAutomaticLabel.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);

            if (_dgvVariables.SortedColumn == null)
                _dgvVariables.Sort(_variablesForm.colVariableName, System.ComponentModel.ListSortDirection.Ascending);
            else
            {
                System.ComponentModel.ListSortDirection sortOrder = System.ComponentModel.ListSortDirection.Ascending;
                if (_dgvVariables.SortOrder == SortOrder.Descending)
                    sortOrder = System.ComponentModel.ListSortDirection.Descending;
                _dgvVariables.Sort(_dgvVariables.SortedColumn, sortOrder);
            }

            _dgvVariables.ResumeLayout();

            RestoreRowStates(selectedRowID, firstDisplayedRowID); //restore selected row and first displayed row
            
            FillCountrySpecificDescriptionList();
        }

        private Dictionary<string, bool> GetVariablesCategState()
        {
            Dictionary<string, List<string>> categAcrosPerType = new Dictionary<string, List<string>>();
            foreach (VarConfig.AcronymRow acroRow in _varConfigFacade._varConfig.Acronym)
            {
                if (acroRow.GetCategoryRows().Count() == 0) continue;
                string varType = acroRow.AcronymLevelRow.AcronymTypeRow.ShortName.ToLower();
                if (!categAcrosPerType.ContainsKey(varType)) categAcrosPerType.Add(varType, new List<string>());
                categAcrosPerType[varType].Add(acroRow.Name.ToLower());
            }

            Dictionary<string, bool> categStates = new Dictionary<string, bool>();
            foreach (VarConfig.VariableRow varRow in _varConfigFacade.GetVariables())
            {
                string varType = GetVariableType(varRow.Name).ToLower();
                bool isCateg = false;
                foreach (string acro in AcronymManager.GetAcronymsUsedByVariable(varRow.Name))
                    if (categAcrosPerType.ContainsKey(varType) && categAcrosPerType[varType].Contains(acro.ToLower()))
                        { isCateg = true; break; }
                categStates.Add(varRow.ID, isCateg);
            }
            return categStates;
        }

        private bool IsCateg(string variableName)
        {
            string varType = GetVariableType(variableName).ToLower();
            foreach (string acroName in AcronymManager.GetAcronymsUsedByVariable(variableName))
            {
                var acro = from a in _varConfigFacade.GetVarConfig().Acronym
                           where a.AcronymLevelRow.AcronymTypeRow.ShortName.ToLower() == varType && a.Name.ToLower() == acroName.ToLower()
                           select a;
                if (acro.Count() > 0 && acro.First().GetCategoryRows().Count() > 0) return true;
            }
            return false;
        }

        void RestoreRowStates(string selectedRowID, string firstDisplayedRowID)
        {
            foreach (DataGridViewRow row in _dgvVariables.Rows)
            {
                VarConfig.VariableRow variableRow = row.Tag as VarConfig.VariableRow;
                if (variableRow.ID == selectedRowID)
                    row.Selected = true;
                if (variableRow.ID == firstDisplayedRowID)
                    _dgvVariables.FirstDisplayedCell = row.Cells[0];
            }
        }

        void StoreRowStates(ref string selectedRowID, ref string firstDisplayedRowID)
        {
            if (_dgvVariables.SelectedRows.Count == 1)
            {
                VarConfig.VariableRow variableRow = _dgvVariables.SelectedRows[0].Tag as VarConfig.VariableRow;
                if (variableRow.RowState == System.Data.DataRowState.Unchanged) //must be checked, because list may be redrawn because of an undo action
                    selectedRowID = variableRow.ID;
            }

            if (_dgvVariables.FirstDisplayedCell != null)
            {
                VarConfig.VariableRow variableRow = _dgvVariables.Rows[_dgvVariables.FirstDisplayedCell.RowIndex].Tag as VarConfig.VariableRow;
                if (variableRow.RowState == System.Data.DataRowState.Unchanged)
                    firstDisplayedRowID = variableRow.ID;
            }
        }

        internal void FillCountrySpecificDescriptionList()
        {
            _dgvDescriptions.Rows.Clear();

            if (_dgvVariables.SelectedRows.Count != 1 || _dgvVariables.SelectedRows[0].Tag == null)
                return;

            VarConfig.VariableRow variableRow = _dgvVariables.SelectedRows[0].Tag as VarConfig.VariableRow;

            foreach (VarConfig.CountryLabelRow countryLabel in variableRow.GetCountryLabelRows())
            {
                int index = _dgvDescriptions.Rows.Add(countryLabel.Country, countryLabel.Label);
                _dgvDescriptions.Rows[index].Tag = countryLabel;
            }

            _variablesForm.colCountry.Width = _variablesForm.colCountry.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            _variablesForm.colCountryDescription.Width = _variablesForm.colCountryDescription.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);

            if (_dgvDescriptions.SortedColumn == null)
                _dgvDescriptions.Sort(_variablesForm.colCountry, System.ComponentModel.ListSortDirection.Ascending);
            else
            {
                System.ComponentModel.ListSortDirection sortOrder = System.ComponentModel.ListSortDirection.Ascending;
                if (_dgvDescriptions.SortOrder == SortOrder.Descending)
                    sortOrder = System.ComponentModel.ListSortDirection.Descending;
                _dgvDescriptions.Sort(_dgvDescriptions.SortedColumn, sortOrder);
            }
        }

        internal void SetFilter()
        {
            bool showSimulated = (bool)_variablesForm.chkSimulated.EditValue;
            bool showData = (bool)_variablesForm.chkData.EditValue;
            bool showMonetary = (bool)_variablesForm.chkMonetary.EditValue;
            bool showNonMonetary = (bool)_variablesForm.chkNonMonetary.EditValue;
            bool showWithCountrySpecificDescriptionOnly = (bool)_variablesForm.chkHasSpecificDescription.EditValue;
            bool showIndLevel = (bool)_variablesForm.chkIndLevel.EditValue;
            bool showHHLevel = (bool)_variablesForm.chkHHLevel.EditValue;
            bool showCategorical = (bool)_variablesForm.chkCategorical.EditValue;
            bool showNonCategorical = (bool)_variablesForm.chkNonCategorical.EditValue;

            //get all countries which have a(ny) country specific description if the respective filter is set
            //(assessing the existence of a country specific description for a certain country and variable in the loop below is way too slow)
            List<string> variablesWithCountrySpecificDescription = new List<string>();
            if (showWithCountrySpecificDescriptionOnly)
            {
                string countryShortName = _variablesForm.cmbCountry.EditValue.ToString();
                if (countryShortName == VariablesForm._anyCountry)
                    countryShortName = string.Empty; //filter for any country specific description (i.e. any description in addition to the automatic label)
                foreach (VarConfig.VariableRow variableRow in _varConfigFacade.GetVariablesWithCountrySpecificDescription(countryShortName))
                    variablesWithCountrySpecificDescription.Add(variableRow.Name.ToLower());
            }

            _dgvVariables.SuspendLayout(); //hopefully that enhences speed

            Dictionary<string, bool> isCateg = GetVariablesCategState();

            foreach (DataGridViewRow dataGridViewRow in _dgvVariables.Rows)
            {
                VarConfig.VariableRow variableRow = dataGridViewRow.Tag as VarConfig.VariableRow;
                string variableName = variableRow.Name.ToLower();

                //variable is visible if its type (benefit, demographic, ...) corresponds to an activated (i.e. checked) type ...
                bool visible = true;
                foreach (BarEditItem filterCheckbox in _variablesForm._typeFilterCheckboxes)
                {
                    if ((bool)filterCheckbox.EditValue == true)
                        continue;
                    string typeShortName = filterCheckbox.Tag.ToString().ToLower();
                    if (typeShortName == DESCRIPTION_UNKNOWN)
                    {
                        if (!_varConfigFacade.GetTypeShortNames(true).Contains(GetVariableType(variableName).ToLower()))
                            visible = false;
                    }
                    else
                    {
                        if (IsVariableOfType(variableName, typeShortName))
                            visible = false;
                    }
                }

                //... however variable must still fulfill other criteria (monetary, simulated, ...)
                if (showWithCountrySpecificDescriptionOnly && !variablesWithCountrySpecificDescription.Contains(variableName.ToLower()))
                    visible = false;
                else if (!showSimulated && IsSimulated(variableName))
                    visible = false;
                else if (!showData && !IsSimulated(variableName))
                    visible = false;
                else if (!showMonetary && IsMonetary(variableRow))
                    visible = false;
                else if (!showNonMonetary && !IsMonetary(variableRow))
                    visible = false;
                else if (!showCategorical && isCateg[variableRow.ID])
                    visible = false;
                else if (!showNonCategorical && !isCateg[variableRow.ID])
                    visible = false;
                else if (!showHHLevel && IsHHLevel(variableRow))
                    visible = false;
                else if (!showIndLevel && !IsHHLevel(variableRow))
                    visible = false;

                dataGridViewRow.Visible = visible;
            }
            
            _dgvVariables.ResumeLayout();
        }

        internal void SelectAllFilters(bool select)
        {
            foreach (BarEditItem filterCheckbox in _variablesForm._typeFilterCheckboxes)
                filterCheckbox.EditValue = select;
            _variablesForm.chkSimulated.EditValue = select;
            _variablesForm.chkData.EditValue = select;
            _variablesForm.chkMonetary.EditValue = select;
            _variablesForm.chkNonMonetary.EditValue = select;
            _variablesForm.chkHasSpecificDescription.EditValue = select;
            _variablesForm.chkIndLevel.EditValue = select;
            _variablesForm.chkHHLevel.EditValue = select;
            _variablesForm.chkNonCategorical.EditValue = select;
            _variablesForm.chkCategorical.EditValue = select;
        }

        internal static bool IsMonetary(VarConfig.VariableRow variableRow)
        {
            return variableRow.Monetary == "1";
        }

        internal static bool IsHHLevel(VarConfig.VariableRow variableRow)
        {
            return variableRow.HHLevel == "1"; // means default is not HH-level
        }

        internal static bool IsSimulated(string variableName)
        {
            return variableName.EndsWith(DefGeneral.POSTFIX_SIMULATED);
        }

        internal static string RemoveSimulatedPostfix(string variableName)
        {
            if (!IsSimulated(variableName))
                return variableName;
            return variableName.Substring(0, variableName.Length - DefGeneral.POSTFIX_SIMULATED.Length);
        }

        internal static string GetVariableType(string variableName)
        {
            if (variableName == string.Empty)
                return string.Empty;
            return variableName.Substring(0, 1);
        }

        internal static bool IsVariableOfType(string variableName, string typeShortName)
        {
            if (variableName == string.Empty)
                return false;
            return typeShortName.ToLower().Contains(variableName.ToLower().Substring(0, 1));
        }

        internal void HandleEndVariableEdit(DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == _variablesForm.dgvVariables.Columns.IndexOf(_variablesForm.colVariableName))
            {
                VarConfig.VariableRow variableRow = _variablesForm.dgvVariables.Rows[e.RowIndex].Tag as VarConfig.VariableRow;
                object val = _variablesForm.dgvVariables[e.ColumnIndex, e.RowIndex].Value;
                string newVariableName = val == null ? string.Empty : val.ToString();

                if (variableRow.Name == newVariableName)
                    return; //do nothing if not actually changed

                string errorText = string.Empty;
                if (!_varConfigFacade.IsNewRow(variableRow.ID))
                    errorText += "You are changing the name of a variable that might be used in country implementations. Please consider using the 'clean variables' option to check usage.\n\n";

                //set automatic label
                string automaticLabel = _variablesForm._acronymManager.GetAutomaticLabel(newVariableName);
                int autoLabelColumnIndex = _variablesForm.dgvVariables.Columns.IndexOf(_variablesForm.colAutomaticLabel);
                _variablesForm.dgvVariables[autoLabelColumnIndex, e.RowIndex].Value = automaticLabel;

                //set whether is categorical or not
                int categColumnIndex = _variablesForm.dgvVariables.Columns.IndexOf(_variablesForm.colCategorical);
                var bkupIsCateg = _variablesForm.dgvVariables[categColumnIndex, e.RowIndex].Value;
                _variablesForm.dgvVariables[categColumnIndex, e.RowIndex].Value = IsCateg(newVariableName);

                //check if valid
                errorText += IsValidVariableName(newVariableName, automaticLabel, e.RowIndex);
                if (errorText != string.Empty)
                {
                    if (Tools.UserInfoHandler.GetInfo(errorText + "Undo change?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _variablesForm.dgvVariables[e.ColumnIndex, e.RowIndex].Value = variableRow.Name;
                        _variablesForm.dgvVariables[categColumnIndex, e.RowIndex].Value = bkupIsCateg;
                        _variablesForm.dgvVariables[autoLabelColumnIndex, e.RowIndex].Value = variableRow.AutoLabel;
                    }
                }
            }

            //write to dataset
            _variablesForm.PerformAction(new ChangeVariableAction(_variablesForm, e));
        }

        internal void HandleValidateVariable(DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 0) return;
            object val = _variablesForm.dgvVariables[e.ColumnIndex, e.RowIndex].Value; string oldName = val == null ? string.Empty : val.ToString();
            string newName = e.FormattedValue == null ? string.Empty : e.FormattedValue.ToString();
            if (oldName == newName) return;
            string error = IsValidVariableName(e.FormattedValue.ToString(), _variablesForm._acronymManager.GetAutomaticLabel(newName), e.RowIndex);
            if (error == string.Empty) return;
            Tools.UserInfoHandler.ShowError(error);
            e.Cancel = true;
        }

        string IsValidVariableName(string variableName, string automaticLabel, int rowIndex)
        {
            if (variableName.Length == 1)
                return "Invalid variable name: too short.\n\n";
            
            if (automaticLabel.Contains(DESCRIPTION_UNKNOWN))
                return "Invalid variable name: usage of not existant acronyms or type.\n\n";

            List<string> usedAcronyms = AcronymManager.GetAcronymsUsedByVariable(variableName);
            int maxLevel = int.MinValue;
            foreach (string acronym in usedAcronyms)
            {
                int acroLevel = _varConfigFacade.GetAcronymLevel(acronym, GetVariableType(variableName));
                if (acroLevel < maxLevel)
                    return "Invalid variable name: wrong order of acronyms.\n\n";
                maxLevel = acroLevel;
            }

            List<string> existingVariables = new List<string>();
            foreach (DataGridViewRow row in _dgvVariables.Rows)
                if (rowIndex != _dgvVariables.Rows.IndexOf(row))
                    existingVariables.Add(row.Cells[_variablesForm.colVariableName.Name].Value.ToString().ToLower());
            if (existingVariables.Contains(variableName.ToLower()))
            {
                int doubleRow = existingVariables.IndexOf(variableName.ToLower()) + 1;
                if (doubleRow > rowIndex)
                    ++doubleRow;
                return "Variable name already exists: see row " + doubleRow.ToString() + ".\n\n";
            }

            return string.Empty;
        }

        internal string CheckForEmptyRows()
        {
            string errList = string.Empty;
            foreach (DataGridViewRow row in _dgvVariables.Rows)
            {
                object val = row.Cells[_variablesForm.colVariableName.Name].Value;
                string variableName = val == null ? string.Empty : val.ToString();
                string strRow = (_dgvVariables.Rows.IndexOf(row) + 1).ToString();
                
                if (variableName == string.Empty)
                    errList += "Empty variable name in row " + strRow + ".\n";
            }
            return errList;
        }

        internal void SearchVariable()
        {
            string searchedVariable = _variablesForm.txtSearchVariable.EditValue == null ? string.Empty : _variablesForm.txtSearchVariable.EditValue.ToString();
            if (searchedVariable == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please enter a variable name to search for in the field above the search button.");
                return;
            }
            
            bool useSearchPattern = searchedVariable.Contains('?') || searchedVariable.Contains('*');
            DataGridViewRow rowToSelect = null;
            List<string> occurrences = new List<string>();
            List<string> occurrenceDescriptions = new List<string>();
            List<object> occurrence_rows = new List<object>();
            foreach (DataGridViewRow row in _dgvVariables.Rows)
            {
                string displayedVariable = row.Cells[_variablesForm.colVariableName.Name].Value.ToString();
                if (EM_Helpers.DoesValueMatchPattern(searchedVariable, displayedVariable))
                {
                    string displayedDescription = row.Cells[_variablesForm.colAutomaticLabel.Name].Value.ToString();
                    
                    occurrence_rows.Add(row.Visible ? (object) row.Index : null);
                    occurrences.Add("Row " + (row.Index + 1).ToString() + ": " + displayedVariable +
                                    (row.Visible == false ? " (hidden!)" : string.Empty));
                    occurrenceDescriptions.Add(displayedDescription);
                    if (rowToSelect == null && row.Visible) //select (below) the first or single not hidden occurrence of the search-pattern
                        rowToSelect = row;
                    if (!useSearchPattern)
                        break; //if no search pattern is used, there cannot be more than one matching variable
                }   
            }

            if (occurrences.Count == 0)
            {
                Tools.UserInfoHandler.ShowError("Variable '" + searchedVariable + "' does not exist.");
                return;
            }
            
            if (rowToSelect != null) //at least one visible match found
            {
                selectRow(_dgvVariables.Rows.IndexOf(rowToSelect));
                if (occurrences.Count == 1)
                    return; //no list of occurrences needs to be displayed if only one match
            }
            string title = occurrences.Count + " variable" + (occurrences.Count > 1 ? "s were" : " was") + " matched:";
            NumberedSearchResultsForm sr = new NumberedSearchResultsForm(title);
            sr.addResults(occurrences, occurrenceDescriptions, occurrence_rows, selectRow);
            sr.ShowDialog(_variablesForm);
        }

        private object selectRow(object clickData)
        {
            int row = (int) clickData;
            if (row >= 0 && row < _dgvVariables.Rows.Count && _dgvVariables.Rows[row].Visible)
            {
                _dgvVariables.CurrentCell = _dgvVariables.Rows[row].Cells[0];
                _dgvVariables.Rows[row].Selected = true;
            }
            return null;
        }
    }
}
