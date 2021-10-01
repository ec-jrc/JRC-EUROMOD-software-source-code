using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.VariablesAdministration.VariablesActions;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesManagement
{
    internal class AcronymManager
    {
        VariablesForm _variablesForm = null;
        TreeList _treeAcronyms = null;
        DataGridView _dgvCategories = null;
        VarConfigFacade _varConfigFacade = null;

        string _variableInEditModus = string.Empty;

        const string AUTOMATICLABEL_SIMULATED = "simulated";
        const string AUTOMATICLABEL_SEPARATOR = " : ";
        internal const string IDENTIFIER_TYPE = "ID";

        void treeAcronyms_DrawHandler(object sender, GetCustomNodeCellStyleEventArgs e)
        {
            if (e.Column != _variablesForm.colAcronym)
                return;

            if (_variableInEditModus == string.Empty)
                e.Appearance.BackColor = Color.Transparent;
            else
            {//if user just edits a variable: highlight the variables used by that variable
                if (IsAcronymOfVariableInEditModus(e.Node))
                    e.Appearance.BackColor = Color.Tomato;
            }
        }

        internal static bool IsAcronymOfVariable(string variable, string acro, string acroType)
        {
            variable = variable.ToLower();
            acro = acro.ToLower();
            
            if (variable.Contains(acro)) //first a rough test ...
            {//... if passed a more accurate test
                if (VariablesManager.IsVariableOfType(variable, acroType))
                {
                    if (GetAcronymsUsedByVariable(variable).Contains(acro))
                        return true;
                }
            }

            return false;
        }

        bool IsAcronymOfVariableInEditModus(TreeListNode acroNode)
        {//does the acronym of this node belong to the variable, which is just edited?
            if (_variableInEditModus == string.Empty || !IsAcronymNode(acroNode))
                return false;
            return IsAcronymOfVariable(_variableInEditModus, 
                                        acroNode.GetDisplayText(_variablesForm.colAcronym), //acronym
                                        (acroNode.Tag as VarConfig.AcronymRow).AcronymLevelRow.AcronymTypeRow.ShortName); //acronym tpye
        }

        internal AcronymManager(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _treeAcronyms = _variablesForm.treeAcronyms;
            _dgvCategories = _variablesForm.dgvCategories;
            _varConfigFacade = _variablesForm._varConfigFacade;
        }

        internal void FillAcronymsList()
        {
            _treeAcronyms.BeginUnboundLoad();

            //store which nodes are expanded, the focused node and the first visible node
            List<string> expandedNodesIDs = new List<string>();
            string focusedNodeID = string.Empty;
            string topVisibleNodeID = string.Empty;
            StoreNodeStates(_treeAcronyms, ref expandedNodesIDs, ref focusedNodeID, ref topVisibleNodeID);
            
            //refill acronym list
            _treeAcronyms.Nodes.Clear();
            foreach (VarConfig.AcronymTypeRow acroTypeRow in _varConfigFacade.GetAllAcronyms())
            {
                //add types (benefit, tax, demographic, ...)
                TreeListNode typeNode = _treeAcronyms.AppendNode(null, null);
                typeNode.SetValue(_variablesForm.colAcronymDescription, acroTypeRow.LongName.ToUpper());
                typeNode.SetValue(_variablesForm.colAcronym, acroTypeRow.ShortName.ToUpper());
                typeNode.Tag = acroTypeRow;

                //add levels (main, status, other, ...): need to be sorted by index, therefore do not use acroTypeRow.GetAcronymLevelRows()
                foreach (VarConfig.AcronymLevelRow acroLevelRow in _varConfigFacade.GetAcronymLevelsSortedByIndex(acroTypeRow))
                {
                    TreeListNode levelNode = _treeAcronyms.AppendNode(null, typeNode);
                    levelNode.SetValue(_variablesForm.colAcronymDescription, acroLevelRow.Name);
                    //levelNode.SetValue(_variablesForm.colAcronym, ???);  //there is no short name or acronym
                    levelNode.Tag = acroLevelRow;

                    //add acronyms
                    foreach (VarConfig.AcronymRow acroRow in acroLevelRow.GetAcronymRows())
                    {
                        TreeListNode acroNode = _treeAcronyms.AppendNode(null, levelNode);
                        acroNode.SetValue(_variablesForm.colAcronymDescription, acroRow.Description);
                        acroNode.SetValue(_variablesForm.colAcronym, acroRow.Name.ToUpper());
                        acroNode.Tag = acroRow;
                    }
                }
            }

            //restore collapse/expanded, focused and first visible node states
            RestoreNodeStates(_treeAcronyms, expandedNodesIDs, focusedNodeID, topVisibleNodeID);

            _variablesForm.colAcronym.BestFit();
            _variablesForm.colAcronymDescription.BestFit();

            _treeAcronyms.EndUnboundLoad();

            _treeAcronyms.NodeCellStyle += new GetCustomNodeCellStyleEventHandler(treeAcronyms_DrawHandler);
        }

        internal static void RestoreNodeStates(TreeList treeAcronyms, List<string> expandedNodesIDs, string focusedNodeID, string topVisibleNodeID)
        {
            foreach (TreeListNode typeNode in treeAcronyms.Nodes)
            {
                string typeID = (typeNode.Tag as VarConfig.AcronymTypeRow).ID;
                if (expandedNodesIDs.Contains(typeID))
                    typeNode.Expanded = true;
                if (focusedNodeID == typeID)
                    treeAcronyms.FocusedNode = typeNode;
                if (topVisibleNodeID == typeID)
                    treeAcronyms.TopVisibleNodeIndex = treeAcronyms.GetVisibleIndexByNode(typeNode);

                foreach (TreeListNode levelNode in typeNode.Nodes)
                {
                    string levelID = (levelNode.Tag as VarConfig.AcronymLevelRow).ID;
                    if (expandedNodesIDs.Contains(levelID))
                        levelNode.Expanded = true;
                    if (focusedNodeID == levelID)
                        treeAcronyms.FocusedNode = levelNode;
                    if (topVisibleNodeID == levelID)
                        treeAcronyms.TopVisibleNodeIndex = treeAcronyms.GetVisibleIndexByNode(levelNode);

                    foreach (TreeListNode acroNode in levelNode.Nodes)
                    {
                        string acroID = (acroNode.Tag as VarConfig.AcronymRow).ID;
                        if (focusedNodeID == acroID)
                            treeAcronyms.FocusedNode = acroNode;
                        if (topVisibleNodeID == acroID)
                            treeAcronyms.TopVisibleNodeIndex = treeAcronyms.GetVisibleIndexByNode(acroNode);
                    }
                }
            }
        }

        internal static void StoreNodeStates(TreeList treeAcronyms, ref List<string> expandedNodesIDs, ref string focusedNodeID, ref string topVisibleNodeID)
        {
            foreach (TreeListNode typeNode in treeAcronyms.Nodes)
            {
                VarConfig.AcronymTypeRow typeRow = typeNode.Tag as VarConfig.AcronymTypeRow;
                if (typeRow.RowState != System.Data.DataRowState.Unchanged)
                    continue; //happens if tree is redrawn because of an undo action
                string typeID = typeRow.ID;
                if (typeNode.Expanded == true)
                    expandedNodesIDs.Add(typeID);
                if (typeNode.Focused == true)
                    focusedNodeID = typeID;
                if (treeAcronyms.GetVisibleIndexByNode(typeNode) == treeAcronyms.TopVisibleNodeIndex)
                    topVisibleNodeID = typeID;

                foreach (TreeListNode levelNode in typeNode.Nodes)
                {
                    VarConfig.AcronymLevelRow levelRow = levelNode.Tag as VarConfig.AcronymLevelRow;
                    if (levelRow.RowState != System.Data.DataRowState.Unchanged)
                        continue;
                    string levelID = levelRow.ID;
                    if (levelNode.Expanded == true)
                        expandedNodesIDs.Add(levelID);
                    if (levelNode.Focused == true)
                        focusedNodeID = levelID;
                    if (treeAcronyms.GetVisibleIndexByNode(levelNode) == treeAcronyms.TopVisibleNodeIndex)
                        topVisibleNodeID = levelID;

                    foreach (TreeListNode acroNode in levelNode.Nodes)
                    {
                        VarConfig.AcronymRow acroRow = acroNode.Tag as VarConfig.AcronymRow;
                        if (acroRow.RowState != System.Data.DataRowState.Unchanged)
                            continue;
                        string acroID = acroRow.ID;
                        if (acroNode.Focused == true)
                            focusedNodeID = acroID;
                        if (treeAcronyms.GetVisibleIndexByNode(acroNode) == treeAcronyms.TopVisibleNodeIndex)
                            topVisibleNodeID = acroID;
                    }
                }
            }
        }

        internal void FillCategoriesList(TreeListNode focusedNode)
        {
            _dgvCategories.Rows.Clear();

            if (!IsAcronymNode(focusedNode))
                return;

            VarConfig.AcronymRow acronymRow = focusedNode.Tag as VarConfig.AcronymRow;
            foreach (VarConfig.CategoryRow categoryRow in acronymRow.GetCategoryRows())
            {
                int index = _dgvCategories.Rows.Add(categoryRow.Value, categoryRow.Description);
                _dgvCategories.Rows[index].Tag = categoryRow;
            }

            _variablesForm.colCategoryValue.Width = _variablesForm.colCategoryValue.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            _variablesForm.colCategoryDescription.Width = _variablesForm.colCategoryDescription.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);

            if (_dgvCategories.SortedColumn == null)
                _dgvCategories.Sort(_variablesForm.colCategoryValue, System.ComponentModel.ListSortDirection.Ascending);
            else
            {
                System.ComponentModel.ListSortDirection sortOrder = System.ComponentModel.ListSortDirection.Ascending;
                if (_dgvCategories.SortOrder == SortOrder.Descending)
                    sortOrder = System.ComponentModel.ListSortDirection.Descending;
                _dgvCategories.Sort(_dgvCategories.SortedColumn, sortOrder);
            }
        }

        internal void HandleBeginVariableEdit(DataGridViewCellCancelEventArgs e)
        {
            object val = _variablesForm.dgvVariables[e.ColumnIndex, e.RowIndex].Value; if (val == null) return;
            _variableInEditModus = val.ToString().ToLower();

            //to make acros visible, which are used by the variable, which is just edited, expand the respective nodes
            //(the acros are highlighted in treeAcronyms_DrawHandler)
            TreeListNode firstMarkedAcro = null;
            foreach (TreeListNode typeNode in _treeAcronyms.Nodes)
                foreach (TreeListNode levelNode in typeNode.Nodes)
                    foreach (TreeListNode acroNode in levelNode.Nodes)
                    {
                        if (!IsAcronymOfVariableInEditModus(acroNode))
                            continue;
                        acroNode.Expanded = true;
                        levelNode.Expanded = true;
                        typeNode.Expanded = true;
                        if (firstMarkedAcro == null)
                            firstMarkedAcro = acroNode;
                    }

            if (firstMarkedAcro != null) //scroll to the level node of the first marked acronym (seems more reasonable than the type node, if there is a lot expanded)
                _treeAcronyms.TopVisibleNodeIndex = _treeAcronyms.GetVisibleIndexByNode(firstMarkedAcro.ParentNode);

            _treeAcronyms.Refresh();
        }

        internal void HandleEndVariableEdit(DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != _variablesForm.dgvVariables.Columns.IndexOf(_variablesForm.colVariableName))
                return;

            //switch off highlighting of acros, which are part of the just edited variable
            _variableInEditModus = string.Empty;
            _treeAcronyms.Refresh();
        }

        internal string GetAutomaticLabel(string variableName, string currentLabel = "")
        {
            if (variableName.ToLower().StartsWith(IDENTIFIER_TYPE.ToLower()))
                return currentLabel; //id's (e.g. idhh, idperson) do not follow the rules, therefore don't change
            string variableType = VariablesManager.GetVariableType(variableName);
            string AutomaticLabel = _varConfigFacade.GetTypeDescription(variableType);
            foreach (string acronym in GetAcronymsUsedByVariable(variableName))
                AutomaticLabel += AUTOMATICLABEL_SEPARATOR + _varConfigFacade.GetAcronymDescription(variableType, acronym);
            if (VariablesManager.IsSimulated(variableName))
                AutomaticLabel += AUTOMATICLABEL_SEPARATOR + AUTOMATICLABEL_SIMULATED;
            return AutomaticLabel;
        }

        internal bool UpdateAutomaticLabel()
        {
            _variablesForm.Cursor = Cursors.WaitCursor;

            bool anyUpdates = false;
            foreach (DataGridViewRow row in _variablesForm.dgvVariables.Rows)
            {
                VarConfig.VariableRow variableRow = row.Tag as VarConfig.VariableRow;
                string updatedLabel = GetAutomaticLabel(variableRow.Name, variableRow.AutoLabel);
                if (updatedLabel.ToLower() != variableRow.AutoLabel.ToLower())
                {
                    variableRow.AutoLabel = updatedLabel;
                    anyUpdates = true;
                }
            }

            _variablesForm.Cursor = Cursors.Default;
            return anyUpdates;
        }

        internal void UpdateAutomaticLabelForSpecificAcronyms(List<VarConfig.AcronymRow> acronymRows)
        {
            List<KeyValuePair<string, string>> acronymsWithType = new List<KeyValuePair<string, string>>();
            foreach (VarConfig.AcronymRow acronymRow in acronymRows)
                acronymsWithType.Add(new KeyValuePair<string, string>(acronymRow.Name, acronymRow.AcronymLevelRow.AcronymTypeRow.ShortName));
            UpdateAutomaticLabelForSpecificAcronyms(acronymsWithType);
        }

        internal void UpdateAutomaticLabelForSpecificAcronyms(List<KeyValuePair<string, string>> acronymsWithType)
        {
            foreach (DataGridViewRow row in _variablesForm.dgvVariables.Rows)
            {
                VarConfig.VariableRow variableRow = row.Tag as VarConfig.VariableRow;
                foreach (KeyValuePair<string, string> acronymWithType in acronymsWithType)
                    if (acronymWithType.Value.ToLower().Contains(VariablesManager.GetVariableType(variableRow.Name).ToLower()) &&
                        GetAcronymsUsedByVariable(variableRow.Name).Contains(acronymWithType.Key.ToLower()))
                        variableRow.AutoLabel = GetAutomaticLabel(variableRow.Name);
            }
        }

        internal string GetVariablesUsingAcronym(string acronym, string acronymType)
        {
            string usingVariables = string.Empty;
            foreach (DataGridViewRow row in _variablesForm.dgvVariables.Rows)
            {
                VarConfig.VariableRow variableRow = row.Tag as VarConfig.VariableRow;
                if (acronymType.ToLower().Contains(VariablesManager.GetVariableType(variableRow.Name).ToLower())  &&
                    GetAcronymsUsedByVariable(variableRow.Name).Contains(acronym.ToLower()))
                    usingVariables += variableRow.Name + " ";
            }
            return usingVariables;
        }

        internal static List<string> GetAcronymsUsedByVariable(string variableName)
        {
            List<string> acronymList = new List<string>();
            variableName = VariablesManager.RemoveSimulatedPostfix(variableName).ToLower();
            for (int pos = 1; pos < variableName.Length - 1; pos += 2)
                acronymList.Add(variableName.Substring(pos, 2));
            if (variableName.Length > 0 && variableName.Length % 2 == 0) //correct variable name must have an odd length (type + acros with 2 characters)
                acronymList.Add(variableName.Substring(variableName.Length-1)); //add the incorrect (1-character) acro to produce an invalid description in the automatic label
            return acronymList;
        }

        internal static bool IsTypeNode(TreeListNode node)
        {
            if (node == null || node.Tag == null)
                return false;
            VarConfig.AcronymTypeRow typeRow = node.Tag as VarConfig.AcronymTypeRow;
            return typeRow != null;
        }

        internal static bool IsLevelNode(TreeListNode node)
        {
            if (node == null || node.Tag == null)
                return false;
            VarConfig.AcronymLevelRow levelRow = node.Tag as VarConfig.AcronymLevelRow;
            return levelRow != null;
        }

        internal static bool IsAcronymNode(TreeListNode node)
        {
            if (node == null || node.Tag == null)
                return false;
            VarConfig.AcronymRow acronymRow = node.Tag as VarConfig.AcronymRow;
            return acronymRow != null;
        }

        internal bool IsCellReadOnly(System.ComponentModel.CancelEventArgs e)
        {   //level-node has no acronym or short-name, therefore this column cannot be edited
            return _treeAcronyms.FocusedNode != null && IsLevelNode(_treeAcronyms.FocusedNode) && _treeAcronyms.FocusedColumn.Name == _variablesForm.colAcronym.Name;
        }

        internal void HandleChangeAcronymAction(CellValueChangedEventArgs eventArgs)
        {
            if (AcronymManager.IsTypeNode(eventArgs.Node))
                _variablesForm.PerformAction(new ChangeAcronymTypeAction(_variablesForm, eventArgs));
            else if (AcronymManager.IsLevelNode(eventArgs.Node))
                _variablesForm.PerformAction(new ChangeAcronymLevelAction(_variablesForm, eventArgs));
            else if (AcronymManager.IsAcronymNode(eventArgs.Node))
                _variablesForm.PerformAction(new ChangeAcronymAction(_variablesForm, eventArgs));
        }

        internal string CheckForEmptyRows()
        {
            string errList = string.Empty;
            string empty = "??";

            foreach (VarConfig.AcronymTypeRow typeRow in _varConfigFacade.GetAllAcronyms())
            {
                string whereToFindType = "Type '";
                if (typeRow.LongName.Trim() != string.Empty)
                    whereToFindType += typeRow.LongName.Trim();
                else if (typeRow.ShortName.Trim() != string.Empty)
                    whereToFindType += typeRow.ShortName.Trim();
                else
                    whereToFindType += empty;
                whereToFindType += "'";

                if (typeRow.ShortName.Trim() == string.Empty || typeRow.LongName.Trim() == string.Empty)
                    errList += whereToFindType + ": missing description and/or acronym.\n";

                foreach (VarConfig.AcronymLevelRow levelRow in _varConfigFacade.GetAcronymLevelsSortedByIndex(typeRow))
                {
                    string whereToFindLevel = whereToFindType + ", level '";
                    if (levelRow.Name.Trim() != string.Empty)
                        whereToFindLevel += levelRow.Name.Trim();
                    else
                        whereToFindLevel += empty;
                    whereToFindLevel += "'";

                    if (levelRow.Name.Trim() == string.Empty)
                        errList += whereToFindLevel + ": missing description.\n";

                    foreach (VarConfig.AcronymRow acronymRow in levelRow.GetAcronymRows())
                    {
                        string whereToFindAcronym = whereToFindLevel + ", acronym '";
                        if (acronymRow.Description.Trim() != string.Empty)
                            whereToFindAcronym += acronymRow.Description.Trim();
                        else if (acronymRow.Name.Trim() != string.Empty)
                            whereToFindAcronym += acronymRow.Name.Trim();
                        else
                            whereToFindAcronym += empty;
                        whereToFindAcronym += "'";

                        if (acronymRow.Description.Trim() == string.Empty || acronymRow.Name.Trim() == string.Empty)
                            errList += whereToFindAcronym + ": missing description and/or acronym.\n";

                        foreach (VarConfig.CategoryRow categoryRow in acronymRow.GetCategoryRows())
                        {
                            string whereToFindCategory = whereToFindAcronym + ", category '";
                            if (categoryRow.Description.Trim() != string.Empty)
                                whereToFindCategory += categoryRow.Description.Trim();
                            else if (categoryRow.Value.Trim() != string.Empty)
                                whereToFindCategory += categoryRow.Value.Trim();
                            else
                                whereToFindCategory += empty;
                            whereToFindCategory += "'";

                            if (categoryRow.Description.Trim() == string.Empty || categoryRow.Value.Trim() == string.Empty)
                                errList += whereToFindCategory + ": missing description and/or value.\n";
                        }
                    }
                }
            }

            return errList;
        }

        internal void HandleEnterKey(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == Keys.Enter && !keyEventArgs.Control && !keyEventArgs.Shift && !keyEventArgs.Alt)
            {
                keyEventArgs.Handled = false;
                keyEventArgs.SuppressKeyPress = false;

                TreeListNode node = _treeAcronyms.FocusedNode;
                if (node == null)
                    return;

                if (node.Nodes.Count > 0 && node.Expanded) //node has visible child-nodes: select first child
                {
                    _treeAcronyms.FocusedNode = node.Nodes.FirstNode;
                    return;
                }
                while (true)
                {
                    if (node.NextNode != null) //node is not the last on its level: select next
                    {
                        _treeAcronyms.FocusedNode = node.NextNode;
                        return;
                    }
                    if (node.ParentNode != null)
                        node = node.ParentNode; //node is last on its level, but not the very last: move one level up and try again
                    else
                        return; //node is the very last
                }
            }
        }

        /*
        internal void SearchAcronym()
        {
            string searchedType = _variablesForm.cmbAcronymType.EditValue == null ? string.Empty : _variablesForm.cmbAcronymType.EditValue.ToString().ToLower();
            string searchedAcronym = _variablesForm.txtSearchAcronym.EditValue == null ? string.Empty : _variablesForm.txtSearchAcronym.EditValue.ToString().ToLower().Trim();

            if (searchedAcronym == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please enter an acronym to search for in the field left of the search buttons.");
                return;
            }

            List<string> occurrences = new List<string>();
            List<string> occurrenceDescriptions = new List<string>();
            List<object> occurrence_nodes = new List<object>();
            foreach (TreeListNode typeNode in _treeAcronyms.Nodes)
            {
                if (searchedType != VariablesForm._allAcronymTypes.ToLower() &&
                    typeNode.GetDisplayText(_variablesForm.colAcronymDescription).ToLower() != searchedType)
                    continue;
                foreach (TreeListNode levelNode in typeNode.Nodes)
                    foreach (TreeListNode acroNode in levelNode.Nodes)
                    {
                        if (acroNode.GetDisplayText(_variablesForm.colAcronym).ToLower() == searchedAcronym)
                        {
                            if (occurrences.Count == 0) //select the single or first occurrence
                            {
                                acroNode.Expanded = true;
                                levelNode.Expanded = true;
                                typeNode.Expanded = true;
                                _treeAcronyms.TopVisibleNodeIndex = _treeAcronyms.GetVisibleIndexByNode(acroNode);
                                acroNode.Selected = true;
                            
                                if (searchedType != VariablesForm._allAcronymTypes.ToLower())
                                    return; //if a specific type was selected, there cannot be further occurrences - thus we are done
                            }

                            //gather occurrences to check below if there is more than one
                            occurrenceDescriptions.Add("TYPE: " + typeNode.GetDisplayText(_variablesForm.colAcronymDescription) +
                                           ", LEVEL: " + levelNode.GetDisplayText(_variablesForm.colAcronymDescription) +
                                           ", DESCRIPTION: " + acroNode.GetDisplayText(_variablesForm.colAcronymDescription));
                            occurrences.Add(acroNode.GetDisplayText(_variablesForm.colAcronymDescription));
                            occurrence_nodes.Add(new List<TreeListNode>() { acroNode, levelNode, typeNode });
                        }
                    }
            }
            if (occurrences.Count == 0)
                Tools.UserInfoHandler.ShowError("Acronym '" + searchedAcronym + "' does not exist.");
            else if (occurrences.Count == 1)
                return; //the single occurrence was selected in the if-part above, nothing else to do
            else
            { //show info about several occurrences (can only occur if search included all acronym types)
                string title = occurrences.Count + " acronym" + (occurrences.Count > 1 ? "s were" : " was") + " matched:";
                NumberedSearchResultsForm sr = new NumberedSearchResultsForm(title);
                sr.addResults(occurrences, occurrenceDescriptions, occurrence_nodes, selectNode);
                sr.ShowDialog(_variablesForm);
            }
        }

        internal void SearchAcronymsByDescription()
        {
            string searchedType = _variablesForm.cmbAcronymType.EditValue == null ? string.Empty : _variablesForm.cmbAcronymType.EditValue.ToString().ToLower();
            string searchedDescription = _variablesForm.txtSearchAcronym.EditValue == null ? string.Empty : _variablesForm.txtSearchAcronym.EditValue.ToString().ToLower().Trim();

            if (searchedDescription == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please enter a description to search for in the field left of the search buttons.");
                return;
            }

            string foundMatches = string.Empty;
            foreach (VarConfig.AcronymRow acronymRow in _varConfigFacade._varConfig.Acronym)
            {
                if (searchedType != VariablesForm._allAcronymTypes.ToLower() &&
                    searchedType != acronymRow.AcronymLevelRow.AcronymTypeRow.LongName.ToLower())
                    continue;
                if (EM_Helpers.DoesValueMatchPattern(searchedDescription, acronymRow.Description))
                    foundMatches += "> " + acronymRow.Name +
                        ": TYPE: " + acronymRow.AcronymLevelRow.AcronymTypeRow.LongName +
                        ", LEVEL: " + acronymRow.AcronymLevelRow.Name +
                        ", DESCRIPTION: " + acronymRow.Description + Environment.NewLine;
            }

            if (foundMatches == string.Empty)
                Tools.UserInfoHandler.ShowInfo("No acronym with a respective description found.");
            else
                Tools.UserInfoHandler.ShowInfo(foundMatches);
        }
        /**/

        internal void SearchAcronymsByNameOrDescription(bool useName)
        {
            string searchedType = _variablesForm.cmbAcronymType.EditValue == null ? string.Empty : _variablesForm.cmbAcronymType.EditValue.ToString().ToLower();
            string searchedText = _variablesForm.txtSearchAcronym.EditValue == null ? string.Empty : _variablesForm.txtSearchAcronym.EditValue.ToString().ToLower().Trim();

            if (searchedText == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please enter "+(useName?"an acronym":"a description")+" to search for in the field left of the search buttons.");
                return;
            }

            List<string> occurrences = new List<string>();
            List<string> occurrenceDescriptions = new List<string>();
            List<object> occurrence_nodes = new List<object>();
            foreach (TreeListNode typeNode in _treeAcronyms.Nodes)
            {
                if (searchedType != VariablesForm._allAcronymTypes.ToLower() &&
                    typeNode.GetDisplayText(_variablesForm.colAcronymDescription).ToLower() != searchedType)
                    continue;

                // first try to match the Type
                if ((useName && typeNode.GetDisplayText(_variablesForm.colAcronym).ToLower() == searchedText) ||        // search for acronym name
                    (!useName && EM_Helpers.DoesValueMatchPattern(searchedText, typeNode.GetDisplayText(_variablesForm.colAcronymDescription).ToLower())))   // or search for acronym description
                {
                    if (occurrences.Count == 0) //select the single or first occurrence
                    {
                        selectNode(new List<TreeListNode>() { typeNode });

                        if (useName & searchedType != VariablesForm._allAcronymTypes.ToLower())
                            return; //if a specific type was selected, and we are searching for name, there cannot be further occurrences - thus we are done
                    }

                    //gather occurrences to check below if there is more than one
                    occurrenceDescriptions.Add("TYPE: " + typeNode.GetDisplayText(_variablesForm.colAcronymDescription));
                    occurrences.Add(typeNode.GetDisplayText(_variablesForm.colAcronymDescription));
                    occurrence_nodes.Add(new List<TreeListNode>() { typeNode });
                }
                foreach (TreeListNode levelNode in typeNode.Nodes)
                {
                    // then try to match the level
                    if (!useName && EM_Helpers.DoesValueMatchPattern(searchedText, levelNode.GetDisplayText(_variablesForm.colAcronymDescription).ToLower()))   // only search for description
                    {
                        if (occurrences.Count == 0) //select the single or first occurrence
                        {
                            selectNode(new List<TreeListNode>() { levelNode, typeNode });

                            if (useName & searchedType != VariablesForm._allAcronymTypes.ToLower())
                                return; //if a specific type was selected, and we are searching for name, there cannot be further occurrences - thus we are done
                        }

                        //gather occurrences to check below if there is more than one
                        occurrenceDescriptions.Add("TYPE: " + typeNode.GetDisplayText(_variablesForm.colAcronymDescription) +
                           ", LEVEL: " + levelNode.GetDisplayText(_variablesForm.colAcronymDescription));
                        occurrences.Add(levelNode.GetDisplayText(_variablesForm.colAcronymDescription));
                        occurrence_nodes.Add(new List<TreeListNode>() { levelNode, typeNode });
                    }

                    foreach (TreeListNode acroNode in levelNode.Nodes)
                    {
                        // finally try to match the acronyms
                        if ((useName && acroNode.GetDisplayText(_variablesForm.colAcronym).ToLower() == searchedText) ||        // search for acronym name
                            (!useName && EM_Helpers.DoesValueMatchPattern(searchedText, acroNode.GetDisplayText(_variablesForm.colAcronymDescription).ToLower())))   // or search for acronym description
                        {
                            if (occurrences.Count == 0) //select the single or first occurrence
                            {
                                selectNode(new List<TreeListNode>() { acroNode, levelNode, typeNode });

                                if (useName & searchedType != VariablesForm._allAcronymTypes.ToLower())
                                    return; //if a specific type was selected, and we are searching for name, there cannot be further occurrences - thus we are done
                            }

                            //gather occurrences to check below if there is more than one
                            occurrenceDescriptions.Add("TYPE: " + typeNode.GetDisplayText(_variablesForm.colAcronymDescription) +
                                           ", LEVEL: " + levelNode.GetDisplayText(_variablesForm.colAcronymDescription) +
                                           ", DESCRIPTION: " + acroNode.GetDisplayText(_variablesForm.colAcronymDescription));
                            occurrences.Add(acroNode.GetDisplayText(_variablesForm.colAcronymDescription));
                            occurrence_nodes.Add(new List<TreeListNode>() { acroNode, levelNode, typeNode });
                        }
                    }
                }
            }
            if (occurrences.Count == 0)
            {
                if (useName) Tools.UserInfoHandler.ShowError("Acronym '" + searchedText + "' does not exist.");
                else Tools.UserInfoHandler.ShowInfo("No acronym with a respective description found.");
            }
            else if (occurrences.Count == 1)
                return; //the single occurrence was selected in the if-part above, nothing else to do
            else
            { //show info about several occurrences (can only occur if search included all acronym types)
                string title = occurrences.Count + " acronym" + (occurrences.Count > 1 ? "s were" : " was") + " matched:";
                NumberedSearchResultsForm sr = new NumberedSearchResultsForm(title);
                sr.addResults(occurrences, occurrenceDescriptions, occurrence_nodes, selectNode);
                sr.ShowDialog(_variablesForm);
            }
        }

        private object selectNode(object clickData)
        {
            if (clickData != null)
            {
                List<TreeListNode> nodes = (List<TreeListNode>)clickData;
                foreach (TreeListNode n in nodes) n.Expanded = true;
                _treeAcronyms.TopVisibleNodeIndex = _treeAcronyms.GetVisibleIndexByNode(nodes[0]);
                nodes[0].Selected = true;
            }
            return null;
        }
    }
}
