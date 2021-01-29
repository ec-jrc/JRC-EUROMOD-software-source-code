using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;
using EM_UI.TreeListManagement;
using EM_UI.NodeOperations;
using EM_UI.Actions;

namespace EM_UI.Dialogs
{
    internal partial class FindReplaceForm : Form
    {
        const string _messageNoWildcards = "Please note that wildcards (*?) do not work in replace mode.";

        List<KeyValuePair<TreeListColumn, TreeListNode>> _foundCells = null;
        int _changesStatusAtLastSearch = -1;
        string _searchCriteriaAtLastSearch = string.Empty;
        List<TreeListNode> _selectedNodesAtLastSearch = null;
        List<TreeListColumn> _selectedColumnsAtLastSearch = null;
        EM_UI_MainForm _mainFormAtLastSearch = null;

        void FindReplaceForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            if (EM_AppContext.Instance.IsPublicVersion())
                chkIncludePrivateComments.Hide();
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void FindReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //don't close just hide
            Hide();
        }

        void SetMode(bool modeReplace)
        {
            if (btnReplace.Text.EndsWith(" ...") && modeReplace)
                btnReplace.Text = btnReplace.Text.Substring(0, btnReplace.Text.Length - 4);
            if (!btnReplace.Text.EndsWith(" ...") && !modeReplace)
                btnReplace.Text += " ...";
            labReplace.Visible = modeReplace;
            btnReplaceAll.Visible = modeReplace;
            cboReplaceBy.Visible = modeReplace;
            chkIncludePrivateComments.Visible = !modeReplace;
            chkIncludePrivateComments.Checked = modeReplace ? false : chkIncludePrivateComments.Checked;
        }

        void btnReplace_Click(object sender, EventArgs e)
        {
            //not yet in find-replace-mode mode: switch from find-mode to find-replace-mode
            if (!btnReplaceAll.Visible)
            {
                SetMode(true);
                return;
            }

            if (IsSearchFieldEmpty())
                return;

            //already in find-replace-mode mode
            string cellValue = null;
            if (GetActiveMainForm().treeList.FocusedNode != null && GetActiveMainForm().treeList.FocusedColumn != null)
                cellValue = GetActiveMainForm().treeList.FocusedNode.GetValue(GetActiveMainForm().treeList.FocusedColumn).ToString();
            if (cellValue == null || !cellValue.ToLower().Contains(cboFind.Text.ToLower()))
            {
                string message = "Focused cell does not match. Try 'Search Next/Previous' to find the next match.";
                if (cboFind.Text.IndexOfAny(new char[] {'?','*'}) >= 0)
                    message += Environment.NewLine + Environment.NewLine + _messageNoWildcards;
                Tools.UserInfoHandler.ShowError(message);
                return;
            }

            cellValue = GetReplacement(cellValue);
            if (TreeListBuilder.IsCommentColumn(GetActiveMainForm().treeList.FocusedColumn))          
                GetActiveMainForm().PerformAction(new ChangeParameterCommentAction(GetActiveMainForm().treeList.FocusedNode, cellValue,
                                                            true, GetActiveMainForm().GetTreeListBuilder().GetCommentColumn()), false);
            else if (TreeListBuilder.IsSystemColumn(GetActiveMainForm().treeList.FocusedColumn))
                GetActiveMainForm().PerformAction(new ChangeParameterValueAction(GetActiveMainForm().treeList.FocusedNode,
                                                            GetActiveMainForm().treeList.FocusedColumn, cellValue, true), false);
            else if (TreeListBuilder.IsPolicyColumn(GetActiveMainForm().treeList.FocusedColumn) && GetActiveMainForm().treeList.FocusedNode.Tag != null && 
                (GetActiveMainForm().treeList.FocusedNode.Tag as BaseTreeListTag).IsPolicyColumnEditable())
                GetActiveMainForm().PerformAction(new ChangeParameterNameAction(GetActiveMainForm().treeList.FocusedNode, cellValue,
                                                            true, GetActiveMainForm().GetTreeListBuilder().GetPolicyColumn()), false);
            else
            {
                Tools.UserInfoHandler.ShowError("Replace can only change system columns and the comment column.");
                return;
            }
            
            btnFindNext_Click(null, null);
        }

        string GetReplacement(string cellValue)
        {
            if (checkEntireCell.Checked)
                cellValue = cboReplaceBy.Text;
            else
            {
                if (chkMatchCase.Checked)
                    cellValue = cellValue.Replace(cboFind.Text, cboReplaceBy.Text);
                else
                {
                    //cellValue = cellValue.Replace(cboFind.Text, cboFind.Text.ToLower());
                    //cellValue = cellValue.Replace(cboFind.Text.ToLower(), cboReplaceBy.Text); //replaced by the next line, as Replace would not replace GROSS by gross
                    cellValue = Regex.Replace(cellValue, Regex.Escape(cboFind.Text), cboReplaceBy.Text, RegexOptions.IgnoreCase);
                }
            }
            return cellValue;
        }

        void btnFindPrevious_Click(object sender, EventArgs e) { FindNextPrev(false); }
        void btnFindNext_Click(object sender, EventArgs e) { FindNextPrev(true); }
        void FindNextPrev(bool findNext)
        {
            if (IsSearchFieldEmpty())
                return;

            //put the search/replace string into the combo-box (if not already in) to have a 'history' of searches
            AddToHistory(cboFind);
            AddToHistory(cboReplaceBy);

            //find the matching cells
            //remark: finding all cells is probably faster than finding the next cell by using the functions which delivers the visible index (i.e. not an internal index, but what the user sees)
            if (!FindMatches())
                return; //no matches found (respective notification is displayed in FindMatches)

            //sort the list of matching cells with respect of sort-order (next/previous) as well as sort-by (column/row)
            List<List<int>> sortedMatchInfo = new List<List<int>>();
            for (int iUnsorted = 0; iUnsorted < _foundCells.Count; ++iUnsorted)
            {
                KeyValuePair<TreeListColumn, TreeListNode> cellToSortIn = _foundCells.ElementAt(iUnsorted);
                int colToSortIn = GetActiveMainForm().treeList.Columns.IndexOf(cellToSortIn.Key);
                int rowToSortIn = TreeListManager.GetNodeVirtualRow(cellToSortIn.Value);
            
                List<int> matchInfoToSortIn = new List<int>();
                matchInfoToSortIn.Add(iUnsorted);
                matchInfoToSortIn.Add(colToSortIn);
                matchInfoToSortIn.Add(rowToSortIn);

                int i = 0;
                for (; i < sortedMatchInfo.Count; ++i)
                {
                    int indexYetIn = sortedMatchInfo.ElementAt(i).ElementAt(0);
                    int colYetIn = sortedMatchInfo.ElementAt(i).ElementAt(1);
                    int rowYetIn = sortedMatchInfo.ElementAt(i).ElementAt(2);
                    if (findNext && radSearchByColumns.Checked)
                        if (colYetIn > colToSortIn || (colYetIn == colToSortIn && rowYetIn > rowToSortIn))
                            break;
                    if (findNext && radSearchByRows.Checked)
                        if (rowYetIn > rowToSortIn || (rowYetIn == rowToSortIn && colYetIn > colToSortIn))
                            break;
                    if (!findNext && radSearchByColumns.Checked)
                        if (colYetIn < colToSortIn || (colYetIn == colToSortIn && rowYetIn < rowToSortIn))
                            break;
                    if (!findNext && radSearchByRows.Checked)
                        if (rowYetIn < rowToSortIn || (rowYetIn == rowToSortIn && colYetIn < colToSortIn))
                            break;
                }
                sortedMatchInfo.Insert(i, matchInfoToSortIn);
            }

            //assess the visible indices of the currently focused cell, search it in the list to assess the next/previous cell
            int index = findNext ? 0 : sortedMatchInfo.Count - 1; //focus first/last matching cell if no match or no focus
            if (GetActiveMainForm().treeList.FocusedNode != null && GetActiveMainForm().treeList.FocusedColumn != null)
            {
                int focusedRow = TreeListManager.GetNodeVirtualRow(GetActiveMainForm().treeList.FocusedNode);
                int focusedColumn = GetActiveMainForm().treeList.Columns.IndexOf(GetActiveMainForm().treeList.FocusedColumn);

                int Criterion1 = radSearchByColumns.Checked ? 1 : 2;
                int Criterion2 = radSearchByColumns.Checked ? 2 : 1;
                int focusedCriterion1 = radSearchByColumns.Checked ? focusedColumn : focusedRow;
                int focusedCriterion2 = radSearchByColumns.Checked ? focusedRow : focusedColumn;
                int prevNextConverter = findNext ? 1 : -1;

                for (int i = 0; i < sortedMatchInfo.Count; ++i)
                {
                    int matchInfoIndex = sortedMatchInfo.ElementAt(i).ElementAt(0);
                    int matchInfoCriterion1 = sortedMatchInfo.ElementAt(i).ElementAt(Criterion1);
                    int matchInfoCriterion2 = sortedMatchInfo.ElementAt(i).ElementAt(Criterion2);

                    if (matchInfoCriterion1 * prevNextConverter > focusedCriterion1 * prevNextConverter ||
                        (matchInfoCriterion1 == focusedCriterion1 && matchInfoCriterion2 * prevNextConverter > focusedCriterion2 * prevNextConverter))
                    {
                        index = matchInfoIndex;
                        break;
                    }
                }
            }

            //focus next/previous matching cell or first/last if no next/previous match found
            TreeListNode node = _foundCells.ElementAt(index).Value;
            TreeListColumn column = _foundCells.ElementAt(index).Key;

            //expand the parent nodes to make the match visible
            for (TreeListNode parentNode = node.ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
                parentNode.Expanded = true;
            node.TreeList.FocusedNode = node;
            node.TreeList.FocusedColumn = column;
        }

        bool FindMatches()
        {
            //if combo-box 'Search In' is not visible (replace mode) search in values and comments otherwise follow user's directions
            bool searchInNames = (radSearchInPolicyColumn.Checked || radSearchInAllColumns.Checked);
            bool searchInValues = (radSeachInSystemColumns.Checked || radSearchInAllColumns.Checked);
            bool searchInComments = (radSearchInCommentColumn.Checked || radSearchInAllColumns.Checked);

            List<TreeListColumn> searchColumns = new List<TreeListColumn>();
            if (searchInNames)
                searchColumns.Add(GetActiveMainForm().GetTreeListBuilder().GetPolicyColumn());
            if (searchInValues)
                searchColumns.AddRange(GetActiveMainForm().GetTreeListBuilder().GetSystemColums());
            if (searchInComments)
                searchColumns.Add(GetActiveMainForm().GetTreeListBuilder().GetCommentColumn());

            string regularExpression = cboFind.Text;
            if (!checkEntireCell.Checked && !chkMatchExactWord.Checked)
            {
                if (!regularExpression.StartsWith("*"))
                    regularExpression = "*" + regularExpression;
                if (!regularExpression.EndsWith("*"))
                    regularExpression += "*";
            }

            //check wheter search criteria or search base (treelist current state, selection) has changed to assess whether search has to be renewed ...
            //... did search criteria change ...
            string searchCriteria = "°" + regularExpression + "°" + chkMatchCase.Checked.ToString() + "°" + radSearchInVisibleCells.Checked.ToString() + "°"
                                    + radSearchInSelectedCells.Checked.ToString() + "°" + chkIncludePrivateComments.Checked.ToString() + "°" + chkMatchExactWord.Checked.ToString() + "°"
                                    + searchInNames.ToString() + searchInValues.ToString() + searchInComments.ToString() + "°";
            //... was treelist changed since last search ...
            int changeStatus = GetActiveMainForm().GetChangesStatus();
            //... was selection changed since last search ...
            List<TreeListNode> selectedNodes = null;
            List<TreeListColumn> selectedColumns = null;
            if (radSearchInSelectedCells.Checked)
            {
                selectedNodes = GetActiveMainForm().GetMultiCellSelector().GetSelectedNodes();
                selectedColumns = GetActiveMainForm().GetMultiCellSelector().GetSelectedColumns();
            }

            //perform search if first search-call or if criteria have changed
            if (_foundCells == null || changeStatus != _changesStatusAtLastSearch || searchCriteria != _searchCriteriaAtLastSearch ||
                _mainFormAtLastSearch != GetActiveMainForm() || HasSelectionChanged(selectedNodes, selectedColumns))
            {
                if (_foundCells == null)
                    _foundCells = new List<KeyValuePair<TreeListColumn, TreeListNode>>();
                _foundCells.Clear();

                GetActiveMainForm().Cursor = Cursors.WaitCursor;
                foreach (TreeListColumn searchColumn in searchColumns)
                {
                    if (selectedColumns != null && !selectedColumns.Contains(searchColumn))
                        continue;
                    DoesNodeMatchPatterns matchFinder = new DoesNodeMatchPatterns(regularExpression, searchColumn, chkMatchCase.Checked,
                                                                                  radSearchInVisibleCells.Checked, selectedNodes, chkIncludePrivateComments.Checked, chkMatchExactWord.Checked);
                    TreatSpecificNodes treater = new TreatSpecificNodes(matchFinder, null, false);
                    GetActiveMainForm().treeList.NodesIterator.DoOperation(treater);
                    foreach (TreeListNode node in treater.GetSpecificNodes())
                        _foundCells.Add(new KeyValuePair<TreeListColumn, TreeListNode>(searchColumn, node));
                }
                GetActiveMainForm().Cursor = Cursors.Default;
            }

            if (_foundCells.Count == 0)
                Tools.UserInfoHandler.ShowError("No match found.");

            _selectedNodesAtLastSearch = selectedNodes;
            _selectedColumnsAtLastSearch = selectedColumns;
            _changesStatusAtLastSearch = changeStatus;
            _searchCriteriaAtLastSearch = searchCriteria;
            _mainFormAtLastSearch = GetActiveMainForm();
            return _foundCells.Count > 0;
        }

        bool HasSelectionChanged(List<TreeListNode> selectedNodes, List<TreeListColumn> selectedColumns)
        {
            bool nodesEqual = true;
            bool columnsEqual = true;
            if ((selectedNodes == null && _selectedNodesAtLastSearch != null) || (selectedNodes != null && _selectedNodesAtLastSearch == null))
                nodesEqual = false;
            else
            {
                if (selectedNodes != null && _selectedNodesAtLastSearch != null)
                {
                    if (_selectedNodesAtLastSearch.Count != selectedNodes.Count)
                        nodesEqual = false;
                    else
                    {
                        if (selectedNodes.Count > 0 &&
                            (selectedNodes.First() != _selectedNodesAtLastSearch.First() || selectedNodes.Last() != _selectedNodesAtLastSearch.Last()))
                            nodesEqual = false;
                    }
                }
            }
            if ((selectedColumns == null && _selectedColumnsAtLastSearch != null) || (selectedColumns != null && _selectedColumnsAtLastSearch == null))
                columnsEqual = false;
            else
            {
                if (selectedColumns != null && _selectedColumnsAtLastSearch != null)
                {
                    if (_selectedColumnsAtLastSearch.Count != selectedColumns.Count)
                        columnsEqual = false;
                    else
                    {
                        if (selectedColumns.Count > 0 &&
                            (selectedColumns.First() != _selectedColumnsAtLastSearch.First() || selectedColumns.Last() != _selectedColumnsAtLastSearch.Last()))
                            columnsEqual = false;
                    }
                }
            }

            return !nodesEqual | !columnsEqual;
        }

        void AddToHistory(ComboBox comboBox)
        {
            if (comboBox.Text == string.Empty)
                return;
            if (!comboBox.Items.Contains(comboBox.Text))
                comboBox.Items.Add(comboBox.Text);
        }

        void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (IsSearchFieldEmpty())
                return;

            if (cboFind.Text.IndexOfAny(new char[] { '?', '*' }) >= 0)
            {
                if (Tools.UserInfoHandler.GetInfo(_messageNoWildcards, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            if (!FindMatches())
                return;

            ActionGroup actionGroup = new ActionGroup();
            foreach (KeyValuePair<TreeListColumn, TreeListNode> foundCell in _foundCells)
            {
                string currentValue = foundCell.Value.GetValue(foundCell.Key).ToString();

                if (TreeListBuilder.IsCommentColumn(foundCell.Key))
                    actionGroup.AddAction(new ChangeParameterCommentAction(foundCell.Value, GetReplacement(currentValue), true,
                                                                            GetActiveMainForm().GetTreeListBuilder().GetCommentColumn()));
                else if (TreeListBuilder.IsSystemColumn(foundCell.Key))
                    actionGroup.AddAction(new ChangeParameterValueAction(foundCell.Value, foundCell.Key, GetReplacement(currentValue), true));
                else if (TreeListBuilder.IsPolicyColumn(foundCell.Key) && foundCell.Value.Tag != null && (foundCell.Value.Tag as BaseTreeListTag).IsPolicyColumnEditable())
                    actionGroup.AddAction(new ChangeParameterNameAction(foundCell.Value, GetReplacement(currentValue), true,
                                                                            GetActiveMainForm().GetTreeListBuilder().GetPolicyColumn()));
            }
            GetActiveMainForm().PerformAction(actionGroup, false);
        }

        internal FindReplaceForm()
        {
            InitializeComponent();
        }

        internal void Show(bool Replace)
        {
            SetMode(Replace);
            Show();
            cboFind.Focus();
        }

        EM_UI_MainForm GetActiveMainForm()
        {
            return EM_AppContext.Instance.GetActiveCountryMainForm();
        }

        bool IsSearchFieldEmpty()
        {
            if (cboFind.Text != string.Empty)
                return false;
            EM_UI.Tools.UserInfoHandler.ShowError("Please specify a search pattern.");
            return true;
        }

    }
}
