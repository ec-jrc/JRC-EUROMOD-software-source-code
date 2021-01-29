using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.ViewInfo;
using EM_Common;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.NodeOperations;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using EM_UI.Validate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.TreeListManagement
{
    internal class TreeListManager
    {
        CountryConfigFacade _countryConfigFacade = null;
        EM_UI_MainForm _mainForm = null;
        ComponentUseForm _componentUseForm = null;
        const string _descriptionHelpFileName = "EM_FC_FunctionName.htm";
        const string _summaryHelpFileName = "EM_FC_Sum_FunctionName.htm";

        Dictionary<string, string> multiPage = new Dictionary<string, string>()
        {
            {"store", "Store_Restore"},
            {"restore", "Store_Restore"},
            {"min", "Min_Max"},
            {"max", "Min_Max"},
            {"loop", "Loop_UnitLoop"},
            {"unitloop", "Loop_UnitLoop"},
            {"dropunit", "DropUnit_KeepUnit"},
            {"keepunit", "DropUnit_KeepUnit"},
            {"defvar", "DefVar_DefConst"},
            {"defconst", "DefVar_DefConst"},
            {"deftu", "DefTU_UpdateTU"},
            {"updatetu", "DefTU_UpdateTU"},
            {"addon_par", "AddOn_Applic_AddOn_Pol_AddOn_Func_AddOn_Par"},
            {"addon_func", "AddOn_Applic_AddOn_Pol_AddOn_Func_AddOn_Par"},
            {"addon_pol", "AddOn_Applic_AddOn_Pol_AddOn_Func_AddOn_Par"},
            {"addon_applic", "AddOn_Applic_AddOn_Pol_AddOn_Func_AddOn_Par"}
        };

        internal TreeListManager(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade)
        {
            _mainForm = mainForm;
            _countryConfigFacade = countryConfigFacade;
        }

        internal void HandleFKeyDown(bool isF5, Control parentWindow)
        {
            if (_mainForm.treeList.FocusedNode == null)
                return;

            string functionName = (_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).GetFunctionName();
            if (functionName == null) return;
            functionName = functionName.Trim().ToLower();
            if (isF5 && multiPage.ContainsKey(functionName)) functionName = multiPage[functionName];
            string helpFileName = string.Empty;
            if (isF5)
                helpFileName = _descriptionHelpFileName.Replace("FunctionName", functionName);
            else
                helpFileName = _summaryHelpFileName.Replace("FunctionName", functionName);
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath);
            Help.ShowHelp(parentWindow, helpPath, helpFileName);
        }

        internal void HandleF3()
        {
            try
            {
                if (_mainForm.treeList.FocusedNode == null || !PolicyTreeListTag.IsReferencePolicy(_mainForm.treeList.FocusedNode)) return;
                string polId = (_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).GetDefaultPolicyRow().ReferencePolID;
                foreach (TreeListNode node in _mainForm.treeList.Nodes)
                    if ((node.Tag as BaseTreeListTag).GetIDsWithinAllSystems().Contains(polId))
                        { _mainForm.treeList.FocusedNode = node; break; }
            }
            catch { }
        }

        internal void HandleKeyDown(KeyEventArgs keyEventArgs, CountryConfigFacade countryConfigFacade, ADOUndoManager undoManager)
        {
            //store information about keyboard-state as in functions other than key-handlers one has no information which keys are currently down
            _mainForm._currentKeyState = keyEventArgs;

            //undo any existing selection if escape was pressed
            if (keyEventArgs.KeyCode == Keys.Escape && _mainForm.GetMultiCellSelector() != null)
            {
                _mainForm.ClearCellSelection();
                _mainForm.treeList.Refresh();
                return;
            }

            // handle Ctrl+Left to move to Parent Node if current Node is already collapsed
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Left)
            {
                TreeListNode n = _mainForm.treeList.FocusedNode;
                if (n != null && n.Expanded == false && n.ParentNode != null)   // if the current Node is collapsed and has a parent
                {
                    _mainForm.treeList.FocusedNode = n.ParentNode;  // move focus to the Node's parent
                    keyEventArgs.Handled = true;
                    return;
                }
            }

            // handle the numpad +/- so that the default XtraTree handling does not take over. Our expand/collapse handling occurs in KeyPress
            if (keyEventArgs.KeyCode == Keys.Subtract || keyEventArgs.KeyCode == Keys.Add)
            {
                keyEventArgs.Handled = true;
                return;
            }

            //handle ctrl-C / ctrl-insert (copy) and ctrl-V / shift-insert (paste)
            if (_mainForm.GetMultiCellSelector() != null && _mainForm.GetMultiCellSelector().HasSelection(true))
            {
                if (keyEventArgs.Control && (keyEventArgs.KeyCode == Keys.C || keyEventArgs.KeyCode == Keys.Insert))
                {
                    _mainForm.GetMultiCellSelector().CopyToClipBoard();
                    return;
                }
                if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.V) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Insert))
                {
                    _mainForm.PerformAction(new PasteMultiValuesAction(_mainForm), false);
                    return;
                }
            }

            //handle ctrl-Z (undo)
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Z)   // Undo
            {
                if (!undoManager.HasChanges())
                    return;
                _mainForm.PerformAction(new UndoAction(undoManager), true, true);
                return;
            }

            //handle ctrl-Y (redo)
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Y)
            {
                if (!undoManager.CanRedo())
                    return;
                _mainForm.PerformAction(new RedoAction(undoManager), true, true);
                return;
            }

            //handle ctrl-F (find) and ctrl-H (replace)
            if (keyEventArgs.Control && (keyEventArgs.KeyCode == Keys.F || keyEventArgs.KeyCode == Keys.H) && countryConfigFacade != null)
                EM_AppContext.Instance.GetFindReplaceForm().Show(keyEventArgs.KeyCode == Keys.H);

            //show add-parameter-form on ctrl-A
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.A)
            {
                if (countryConfigFacade != null)
                {
                    EM_AppContext.Instance.GetAddParameterForm().Show();
                    EM_AppContext.Instance.GetAddParameterForm().UpdateContent(_mainForm.treeList.FocusedNode);
                }
            }

            //F5, F6: show help on selected function
            if (keyEventArgs.KeyCode == Keys.F5 || keyEventArgs.KeyCode == Keys.F6)
                HandleFKeyDown(keyEventArgs.KeyCode == Keys.F5, _mainForm);

            //F3: if reference policy, go to referred policy
            if (keyEventArgs.KeyCode == Keys.F3) HandleF3();

            if (keyEventArgs.KeyCode == Keys.F5 || keyEventArgs.KeyCode == Keys.F7)
                HandleShowSystemDiscrepancyInfo();

            //F2: open for editing without selecting text
            if (keyEventArgs.KeyCode == Keys.F2 && _mainForm.treeList.FocusedNode != null)
            {
                _mainForm.treeList.ShowEditor();                // try to show the editor
                if (_mainForm.treeList.ActiveEditor != null)    // if editor exists, deselect the text
                    _mainForm.treeList.ActiveEditor.DeselectAll();
                keyEventArgs.Handled = true;                    // avoid handling this twice
                return;
            }

            if (_mainForm.treeList.FocusedNode == null)
                return;

            //if not a special key, pass to command-processor
            keyEventArgs.SuppressKeyPress = ProcessKey(keyEventArgs);
        }

        internal void HandleKeyPress(KeyPressEventArgs keyEventArgs)
        {
            // Handle +/- to expand/collapse one or more Nodes. Capture both the NumPad +/- and the standard keyboard +/-.
            if (keyEventArgs.KeyChar == '-' || keyEventArgs.KeyChar == '+' || keyEventArgs.KeyChar == '/' || keyEventArgs.KeyChar == '*')
            {
                bool expand = (keyEventArgs.KeyChar == '+' || keyEventArgs.KeyChar == '*');  // decide if we are expanding or collapsing
                bool deep = (keyEventArgs.KeyChar == '/' || keyEventArgs.KeyChar == '*');  // decide if we are going deep

                if (_mainForm.GetMultiCellSelector() != null && _mainForm.GetMultiCellSelector().HasSelection()) // if multiple cells are selected
                {
                    expandCollapseNodes(_mainForm.GetMultiCellSelector().GetSelectedNodes(), expand, deep);
                }
                else    // if only one cell is selected/focused
                {
                    TreeListNode fn = _mainForm.treeList.FocusedNode;
                    if (fn != null)
                    {
                        if (fn.Tag is EM_UI.TreeListTags.ParameterTreeListTag) return;  // if this is a parameter, then just type "+" etc
                        expandCollapseNodes(fn, expand, deep);
                    }
                }
                keyEventArgs.Handled = true;   // either way, do not further handle this keypress (to avoid opening the cell editor)
                return;
            }
        }

        internal void HandleKeyUp(KeyEventArgs keyEventArgs)
        {
            _mainForm._currentKeyState = null; //release information about keyboard-state (see HandleKeyDown)

            //handle ctrl-S (save)
            if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.S)
            {
                if (_mainForm._isReadOnly)
                    UserInfoHandler.ShowInfo((_mainForm._isAddOn ? "Add-on" : "Country") + " is in read-only mode.");
                else
                {
                    _mainForm.WriteXml();
                    _mainForm.SetButtonGreyState();
                }
                return;
            }

            //handle enter
            if (keyEventArgs.KeyCode == Keys.Enter && !keyEventArgs.Control && !keyEventArgs.Shift && !keyEventArgs.Alt)
            {
                keyEventArgs.Handled = false;
                keyEventArgs.SuppressKeyPress = false;

                TreeListNode node = _mainForm.treeList.FocusedNode;
                if (node == null)
                    return;

                if (node.Nodes.Count > 0 && node.Expanded) //node has visible child-nodes: select first child
                {
                    _mainForm.treeList.FocusedNode = node.Nodes.FirstNode;
                    return;
                }
                while (true)
                {
                    if (node.NextNode != null) //node is not the last on its level: select next
                    {
                        _mainForm.treeList.FocusedNode = node.NextNode;
                        return;
                    }
                    if (node.ParentNode != null)
                        node = node.ParentNode; //node is last on its level, but not the very last: move one level up and try again
                    else
                        return; //node is the very last
                }
            }
        }

        internal void HandleMouseDown(MouseEventArgs mouseEventArgs, Point mainFormMousePosition, Keys mainFormModifierKeys)
        {
            bool focusCell = false;
            // Check if user clicked on a border and focus on the correct cell
            TreeListHitInfo hitInfo = _mainForm.treeList.CalcHitInfo(mouseEventArgs.Location);

            if (hitInfo.Column == null) // if the user clicked exactly on a border (horizontal of vertical)
            {
                focusCell = true;
                hitInfo = _mainForm.treeList.CalcHitInfo(new Point(mouseEventArgs.X - 1, mouseEventArgs.Y - 1));
                if (hitInfo.Column == null) // if the user clicked on the horizontal border and exactly one pixel from the vertical border
                {
                    hitInfo = _mainForm.treeList.CalcHitInfo(new Point(mouseEventArgs.X, mouseEventArgs.Y - 1));
                    if (hitInfo.Column == null) // if the user clicked on the vertical border and exactly one pixel from the horizontal border
                    {
                        hitInfo = _mainForm.treeList.CalcHitInfo(new Point(mouseEventArgs.X - 1, mouseEventArgs.Y));
                    }
                }
            }
            if (((mouseEventArgs.Button == MouseButtons.Right || focusCell)
                && hitInfo.Node != null //happens if column-header is clicked
                && hitInfo.Column != null))
            {
                _mainForm.treeList.FocusedNode = hitInfo.Node;
                _mainForm.treeList.FocusedColumn = hitInfo.Column;
            }
        }

        internal void HandleMouseUp(MouseEventArgs mouseEventArgs, Point mainFormMousePosition, Keys mainFormModifierKeys)
        {
            Point treeListMousePosition = _mainForm.treeList.PointToClient(mainFormMousePosition);
            TreeListHitInfo mouseHitInfo = _mainForm.treeList.CalcHitInfo(treeListMousePosition);

            //if user left-clicked in a policy row which is referencing another one
            bool stopActions = false;
            if (mouseEventArgs.Button == MouseButtons.Left && mainFormModifierKeys == Keys.None && mouseHitInfo.HitInfoType == HitInfoType.StateImage && PolicyTreeListTag.IsReferencePolicy(mouseHitInfo.Node))
            {
                stopActions = true;
                HandleF3();
            }

            if (mouseEventArgs.Button == MouseButtons.Left && mainFormModifierKeys == Keys.None && !stopActions)
            {
                //user left-clicked in the policy-column or on a policy/function-symbol
                if ((mouseHitInfo.HitInfoType == HitInfoType.Cell && TreeListBuilder.IsPolicyColumn(mouseHitInfo.Column))
                    || mouseHitInfo.HitInfoType == HitInfoType.StateImage)
                {//provide a dialog that asks whether a parameter should be replaced by its alias, e.g. Comp_perTU by Com_perElig or Output_Var by Output_Add_Var
                    if (mouseHitInfo.Node.Tag == null)
                        return;
                    CountryConfig.ParameterRow parameterRow = (mouseHitInfo.Node.Tag as BaseTreeListTag).GetDefaultParameterRow();
                    if (parameterRow == null)
                        return; //node does not refer to a parameter row (but to a policy or function row)

                    //currently ignore that a parameter might have more than one conflict parameter (e.g. Round_To, Round_Up, Round_Down), 
                    //which would require something like a menu for choice (may be reflected if there is demand)
                    DefinitionAdmin.Par defPar = DefinitionAdmin.GetParDefinition(parameterRow.FunctionRow.Name, parameterRow.Name);
                    if (defPar.substitutes.Count() == 0)
                        return; //parameter does not have an alias

                    if (UserInfoHandler.GetInfo($"Replace '{parameterRow.Name}' by '{defPar.substitutes.First()}'?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return;
                    _mainForm.PerformAction(new ChangeParameterNameAction(mouseHitInfo.Node, defPar.substitutes.First()), true);
                }

                //if in single-policy-view, show the menu that allows to select a policy
                else if (mouseHitInfo.HitInfoType == HitInfoType.Column && TreeListBuilder.IsPolicyColumn(mouseHitInfo.Column))
                    _mainForm.GetTreeListBuilder().ShowPolicySelectMenu(mainFormMousePosition);
            }

            if (!stopActions && mouseEventArgs.Button == MouseButtons.Right && mainFormModifierKeys == Keys.None && _mainForm.treeList.State == TreeListState.Regular)
            {
                ContextMenuStrip contextMenu = null;
                //user clicked on a row-number
                if (mouseHitInfo.HitInfoType == HitInfoType.RowIndicator)
                    contextMenu = _mainForm.GetRowContextMenu().GetContextMenu(mouseHitInfo.Node); //show context-menu which allows to hide/show rows

                //user clicked in the policy column or on a policy/function-symbol
                if ((mouseHitInfo.HitInfoType == HitInfoType.Cell && TreeListBuilder.IsPolicyColumn(mouseHitInfo.Column))
                    || mouseHitInfo.HitInfoType == HitInfoType.StateImage)
                {
                    _mainForm.treeList.FocusedNode = mouseHitInfo.Node; //show the respective context-menu (i.e. policy- or function-menu)
                    contextMenu = (mouseHitInfo.Node.Tag as BaseTreeListTag).GetContextMenu(mouseHitInfo.Node);
                }

                //user clicked system-, comment- or group column
                if (mouseHitInfo.HitInfoType == HitInfoType.Cell && mouseHitInfo.Node.Tag != null && 
                        (TreeListBuilder.IsGroupColumn(mouseHitInfo.Column) ||
                         TreeListBuilder.IsSystemColumn(mouseHitInfo.Column) ||
                         TreeListBuilder.IsCommentColumn(mouseHitInfo.Column)))
                {
                    contextMenu = _mainForm.GetCellContextMenu().GetContextMenu(mouseHitInfo.Node, mouseHitInfo.Column);
                }

                if (contextMenu != null)
                    contextMenu.Show(mainFormMousePosition);
            }
        }

        internal void HandleShowSystemDiscrepancyInfo()
        {   //user pressed F7 in a group-column-cell that shows a little red square, indicating info produced by compare versions (or import system by id)
            if (_mainForm._importByIDDiscrepancies != null &&
                        _mainForm.treeList.FocusedColumn != null && TreeListBuilder.IsGroupColumn(_mainForm.treeList.FocusedColumn) &&
                        _mainForm.treeList.FocusedNode != null && _mainForm.treeList.FocusedNode.Tag != null)
            {
                BaseTreeListTag tag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
                if (tag != null)
                {
                    string discrepancy = _mainForm._importByIDDiscrepancies.GetDiscrepancy(tag.GetIDsWithinAllSystems());
                    if (discrepancy != string.Empty)
                    {   //show an infobox, which allows for more permanent watching and copying the text
                        List<string> systemIDs = (from sys in _countryConfigFacade.GetSystemRows() select sys.ID).ToList();
                        (new InfoBox()).Show(discrepancy, _mainForm._importByIDDiscrepancies.GetIdentificationInfo(systemIDs)); 
                    }
                }
            }
        }

        bool IsNodeVisibleOnScreen(TreeListNode node) //not (yet) used function, however may be useful at some time
        {
            int visibleIndex = node.TreeList.GetVisibleIndexByNode(node);
            return visibleIndex >= node.TreeList.TopVisibleNodeIndex &&
                visibleIndex < node.TreeList.TopVisibleNodeIndex + node.TreeList.ViewInfo.VisibleRowCount;
        }

        TreeListNode GetBottomVisibleNode()
        {
            RowInfo rowInfo = _mainForm.treeList.ViewInfo.RowsInfo.Rows[_mainForm.treeList.ViewInfo.RowsInfo.Rows.Count - 1] as RowInfo;
            return rowInfo == null ? null : rowInfo.Node;
        }

        TreeListNode GetTopVisibleNode()
        {
            RowInfo rowInfo = _mainForm.treeList.ViewInfo.RowsInfo.Rows[0] as RowInfo;
            return rowInfo == null ? null : rowInfo.Node;
        }

        internal ColumnInfo GetInfo_RightmostFixedColumnLeft()
        {//i.e. the group column - just to keep it more general
            for (int index = _mainForm.treeList.ViewInfo.ColumnsInfo.Columns.Count - 1; index >= 0; --index)
            {
                ColumnInfo columnInfo = _mainForm.treeList.ViewInfo.ColumnsInfo.Columns[index] as ColumnInfo;
                if (columnInfo != null && columnInfo.Column != null && columnInfo.Column.Visible == true && columnInfo.Column.Fixed == FixedStyle.Left)
                    return columnInfo;
            }
            return null; //the programme should never reach this row
        }

        ColumnInfo GetInfo_LeftmostFixedColumnRight()
        {//i.e. the comments column - just to keep it more general
            for (int index = 0; index < _mainForm.treeList.ViewInfo.ColumnsInfo.Columns.Count; ++index)
            {
                ColumnInfo columnInfo = _mainForm.treeList.ViewInfo.ColumnsInfo.Columns[index] as ColumnInfo;
                if (columnInfo != null && columnInfo.Column != null && columnInfo.Column.Visible == true && columnInfo.Column.Fixed == FixedStyle.Right)
                    return columnInfo;
            }
            return null; //the programme should never reach this row
        }

        List<TreeListColumn> GetNonFixedVisibleColumns()
        {//get all system columns which are actually visible, i.e. not in the column chooser and not hidden by exceeding the scroll area
            TreeListViewInfo viewInfo = _mainForm.treeList.ViewInfo;
            int leftEdge = GetInfo_RightmostFixedColumnLeft().Bounds.X + GetInfo_RightmostFixedColumnLeft().Bounds.Width + viewInfo.BorderSize.Width;
            int rightEdge = GetInfo_LeftmostFixedColumnRight().Bounds.X - viewInfo.BorderSize.Width;

            List<TreeListColumn> nonFixedVisibleColumns = new List<TreeListColumn>();

            for (int index = 0; index < viewInfo.ColumnsInfo.Columns.Count; ++index)
            {
                ColumnInfo columnInfo = viewInfo.ColumnsInfo.Columns[index] as ColumnInfo;
                if (columnInfo != null && columnInfo.Column != null && columnInfo.Column.Visible && columnInfo.Column.Fixed == FixedStyle.None &&
                    columnInfo.Bounds.X + columnInfo.Bounds.Width > leftEdge && columnInfo.Bounds.X < rightEdge)
                        nonFixedVisibleColumns.Add(columnInfo.Column);
            }
            return nonFixedVisibleColumns;
        }

        TreeListColumn GetRightmostNonFixedVisibleColumn()
        {//i.e. the column left of the comments column, which is not hidden by exceeding the scroll area
            return GetNonFixedVisibleColumns().Count > 0 ? GetNonFixedVisibleColumns().ElementAt(GetNonFixedVisibleColumns().Count - 1) : null;
        }

        TreeListColumn GetLeftmostNonFixedVisibleColumn()
        {//i.e. the column right of the group column, which is not hidden by exceeding the scroll area
            return GetNonFixedVisibleColumns().Count > 0 ? GetNonFixedVisibleColumns().ElementAt(0) : null;
        }

        internal void HandleColumnMenuShowing(PopupMenuShowingEventArgs eventArgs)
        {
            _mainForm.GetColumnContextMenu().ShowContextMenu(eventArgs);
        }

        internal void CreateRespectiveEditor(GetCustomNodeCellEditEventArgs eventArgs)
        {
            if (eventArgs.Node.Tag == null)
                return;

            RepositoryItem editor = null;

            //comment-column: show memo-editor
            if (TreeListBuilder.IsCommentColumn(eventArgs.Column))
            {
                editor = new RepositoryItemMemoEdit();
                (editor as RepositoryItemMemoEdit).AcceptsReturn = false;
            }

            else if (TreeListBuilder.IsPolicyColumn(eventArgs.Column))
            {
                if ((eventArgs.Node.Tag as BaseTreeListTag).IsPolicyColumnEditable())
                    editor = _mainForm.GetFormulaEditorManager().CreateEditor(eventArgs.Node, eventArgs.Column);
            }

            //system-column: show editor in accordance to the type of the cell-value (formula, taxunit, switch, etc.)
            //or policy-column: usually not editable, except in special cases (e.g. variables in Uprate, SetDefault, ...)
            else if (TreeListBuilder.IsSystemColumn(eventArgs.Column))
            {
                editor = (eventArgs.Node.Tag as BaseTreeListTag).GetEditor(eventArgs.Node, eventArgs.Column);
            }

            if (editor != null)
                eventArgs.RepositoryItem = editor;
        }

        internal void HandleShowingEditor(CancelEventArgs cancelEventArgs)
        {
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt && (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                cancelEventArgs.Cancel = true;
                return; //to avoid opening of editor when the user actually wants to select cells
            }

            if (_mainForm.treeList.FocusedColumn == null || _mainForm.treeList.FocusedNode == null)
                return;

            if (TreeListBuilder.IsGroupColumn(_mainForm.treeList.FocusedColumn) && !(_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).IsGroupColumnEditable())
                cancelEventArgs.Cancel = true; //only parameter-nodes allow editing group-column

            if (TreeListBuilder.IsPolicyColumn(_mainForm.treeList.FocusedColumn) && !(_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).IsPolicyColumnEditable())
                cancelEventArgs.Cancel = true; //only for some special functions' parameters policy-column may be edited (e.g. ILDef, Uprate, SetDefault, ...)
        }

        internal void ValidateEditorInput(DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs validateEventArgs)
        {
            if (_mainForm.GetFormulaEditorManager().isStillEditing())
                return;

            string errorText = string.Empty;
            if (TreeListBuilder.IsSystemColumn(_mainForm.treeList.FocusedColumn))
                validateEventArgs.Valid = (_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).ValidateEditorInput(validateEventArgs.Value.ToString(),
                    _mainForm.treeList.FocusedNode, _mainForm.treeList.FocusedColumn, ref errorText);
            if (TreeListBuilder.IsGroupColumn(_mainForm.treeList.FocusedColumn))
                validateEventArgs.Valid = ParameterValidation.ValidateGroupInput(validateEventArgs.Value.ToString(),
                    _mainForm.treeList.FocusedNode.GetDisplayText(_mainForm.treeList.Columns.ColumnByName(TreeListBuilder._policyColumnName)), ref errorText);
            validateEventArgs.ErrorText = errorText;

            validateEventArgs.Valid = true; //validation is switched off as currently not reliable enough - may hamper inserting valid input, which is wrongly qualified as invalid
        }

        internal void HandleCellValueChanged(string newValue, TreeListColumn column, TreeListNode node)
        {
            if (IsFormulaEditing())
                return;
            if (TreeListBuilder.IsSystemColumn(column))
                (node.Tag as BaseTreeListTag).HandleValueChanged(node, column, newValue);
            else if (TreeListBuilder.IsGroupColumn(column))
                _mainForm.PerformAction(new ChangeParameterGroupAction(node, newValue), false);
            else if (TreeListBuilder.IsCommentColumn(column))
                _mainForm.PerformAction(new ChangeParameterCommentAction(node, newValue), false);
            else if (TreeListBuilder.IsPolicyColumn(column))
                _mainForm.PerformAction(new ChangeParameterNameAction(node, newValue), false);
        }

        internal const string NA_ALL_COMPONENTS = "n/a all components";

        internal void HandleSwitchToNAallComponents()
        {//see description of function BaseTreeListTag.HandleSwitchToNAallComponents for explanation
            if (_mainForm.treeList.FocusedColumn == null ||
                _mainForm.treeList.FocusedNode == null || _mainForm.treeList.FocusedNode.Tag == null ||
                _mainForm.treeList.ActiveEditor == null || _mainForm.treeList.ActiveEditor.EditValue == null ||
                _mainForm.treeList.ActiveEditor.EditValue.ToString() != NA_ALL_COMPONENTS)
                return;
            (_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).HandleSwitchToNAallComponents(_mainForm.treeList.FocusedNode, _mainForm.treeList.FocusedColumn);
        }

        internal void StopFormulaEditing()
        {
            _mainForm.GetFormulaEditorManager().StopIntelliList();
        }

        internal bool IsFormulaEditing()
        {
            return _mainForm.GetFormulaEditorManager().isStillEditing();
        }

        internal void HandleDragDrop(DragEventArgs dragEventArgs)
        {
            Point point = _mainForm.treeList.PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
            TreeListNode targetNode = _mainForm.treeList.CalcHitInfo(point).Node;
            Rectangle nodeBounds = _mainForm.treeList.CalcHitInfo(point).Bounds;
            bool moveAboveTarget;
            if (_mainForm.treeList.GetNodeIndex(targetNode) < _mainForm.treeList.GetNodeIndex(_mainForm.treeList.FocusedNode))
                moveAboveTarget = point.Y < nodeBounds.Y + Math.Round(nodeBounds.Height / 3.0) + 1;  // if moving above focused, top 1/3 counts as "move above target"
            else
                moveAboveTarget = point.Y < nodeBounds.Y + Math.Round(2 * nodeBounds.Height / 3.0) + 1;   // if moving below focused, top 2/3 counts as "move above target"

            HandleMoveNodes(moveAboveTarget, targetNode);

            dragEventArgs.Effect = DragDropEffects.None; //suppress default behaviour
            return;
        }

        internal void DrawNodeRowNumber(CustomDrawNodeIndicatorEventArgs eventArgs)
        {
            IndicatorObjectInfoArgs indicatorObjectInfoArgs = eventArgs.ObjectArgs as IndicatorObjectInfoArgs;
            indicatorObjectInfoArgs.DisplayText = GetNodeRowNumber(eventArgs.Node, eventArgs.Node.ParentNode);
            eventArgs.ImageIndex = -1;

            List<KeyValuePair<TreeListNode, TreeListNode>> rangesOfHiddenNodes = new List<KeyValuePair<TreeListNode, TreeListNode>>();
            TreeListManager.GetRangesOfHiddenNodes(_mainForm.treeList.Nodes, ref rangesOfHiddenNodes);

            bool foundHiddenNode = false;
            bool isLastNode = false;
            bool parentExpanded = true;
            int nextNodeToHiddenNodeId = -1;
            int prevNodeToHiddenNodeId = -1;
            //int previousNodeToHiddenNodeId = -1;
            int currentNodeId = -1;
            foreach (KeyValuePair<TreeListNode, TreeListNode> hiddenNodeDict in rangesOfHiddenNodes)
            {
                TreeListNode hiddenNode = hiddenNodeDict.Key;

                if (hiddenNode.NextVisibleNode != null)
                {
                    nextNodeToHiddenNodeId = GetNodeVirtualRow(hiddenNode.NextVisibleNode);
                    currentNodeId = GetNodeVirtualRow(eventArgs.Node);

                    if ((nextNodeToHiddenNodeId == currentNodeId))
                    {
                        foundHiddenNode = true;

                        //Check if the parent is expanded or not
                        if(hiddenNode.ParentNode != null)
                        {
                            parentExpanded = hiddenNode.ParentNode.Expanded;
                        }

                        break;
                    }
                }else if(hiddenNode.PrevVisibleNode != null)
                {
                    prevNodeToHiddenNodeId = GetNodeVirtualRow(hiddenNode.PrevVisibleNode);
                    currentNodeId = GetNodeVirtualRow(eventArgs.Node);

                    if ((prevNodeToHiddenNodeId == currentNodeId))
                    {
                        foundHiddenNode = true;
                        isLastNode = true;
                        break;
                    }
                    
                    break;
                }
                
            }

            // if the node is focused or part of an extension or group or adjacent to a hidden row we need custom drawing
            // i.e. drawn with golden background if focused and/or with little markers if part of extension or group
            List<ExtensionOrGroup> egs = new List<ExtensionOrGroup>();
            egs = ExtensionAndGroupManager.GetLookGroupDrawInfo(_mainForm.GetCountryShortName(), eventArgs.Node);
            egs.AddRange(ExtensionAndGroupManager.GetExtensionDrawInfo(_mainForm.GetCountryShortName(), eventArgs.Node));

            if (eventArgs.Node != eventArgs.Node.TreeList.FocusedNode && !egs.Any() && !foundHiddenNode) return;

            Color borderColor = Color.FromArgb(202, 203, 211), origFillColor = Color.FromArgb(245, 246, 247);     // ideally one would get these colours from eventArgs.Appearance,
            Color fillColor = eventArgs.Node == eventArgs.Node.TreeList.FocusedNode ? Color.Gold : origFillColor; // but the respective variables are wrong or not filled
            Rectangle r = new Rectangle(eventArgs.Bounds.X, eventArgs.Bounds.Y, eventArgs.Bounds.Width - 1, eventArgs.Bounds.Height - 1);
            eventArgs.Graphics.FillRectangle(new SolidBrush(fillColor), r);
            int rightPos = 1; foreach (ExtensionOrGroup eg in egs) eg.look.DoTreePainting(eventArgs, ref rightPos, eg.style); // draw extension/group-markers
            eventArgs.Graphics.DrawLine(new Pen(new SolidBrush(borderColor)), new Point(r.Left, r.Bottom), new Point(r.Right, r.Bottom)); // draw border
            eventArgs.Graphics.DrawLine(new Pen(new SolidBrush(borderColor)), new Point(r.Right, r.Top), new Point(r.Right, r.Bottom));


            if (foundHiddenNode && parentExpanded && !isLastNode)
            {
                eventArgs.Graphics.DrawLine(new Pen(new SolidBrush(Color.DarkRed)), new Point(r.Right, r.Top), new Point(r.Left, r.Top));
            }
            else if (foundHiddenNode && !parentExpanded && !isLastNode)
            {
                eventArgs.Graphics.DrawLine(new Pen(new SolidBrush(Color.Orange)), new Point(r.Right, r.Top), new Point(r.Left, r.Top));
            }
            else if(foundHiddenNode && isLastNode)
            {
                eventArgs.Graphics.DrawLine(new Pen(new SolidBrush(Color.DarkRed)), new Point(r.Right, r.Bottom), new Point(r.Left, r.Bottom));
            }

            eventArgs.Graphics.DrawString(" " + indicatorObjectInfoArgs.DisplayText, eventArgs.Appearance.Font, new SolidBrush(Color.Black), eventArgs.Bounds,
                                          new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center }); // draw text, i.e.row-number
            eventArgs.Handled = true;
        }

        internal static string GetNodeRowNumber(TreeListNode node, TreeListNode parentNode)
        {
                if (node.ParentNode == null)
                    return (node.TreeList.Nodes.IndexOf(node) + 1).ToString();
                return GetNodeRowNumber(node.ParentNode, node.ParentNode.ParentNode) + "." + (parentNode.Nodes.IndexOf(node) + 1).ToString();
        }

        //returns a "row-number" in the form pppfffaaa, where ppp represents the policy's index, fff the function's index and aaa the parameter's index
        internal static int GetNodeVirtualRow(TreeListNode node)
        {
            if (node.ParentNode == null) //policy node: return e.g 3000000 for the 3rd policy (3.000.000)
                return (node.TreeList.Nodes.IndexOf(node) + 1) * 1000 * 1000;
            if (node.ParentNode.ParentNode == null) //function node: return e.g 3012000 for the 12th function in 3rd policy (3.012.000)
                return (node.TreeList.Nodes.IndexOf(node.ParentNode) + 1) * 1000 * 1000 + (node.ParentNode.Nodes.IndexOf(node) + 1) * 1000;
            //parameter node: return e.g. 1008022 for the 22th parameter in 8th function of 1st policy
            return (node.TreeList.Nodes.IndexOf(node.ParentNode.ParentNode) + 1) * 1000 * 1000 + (node.ParentNode.ParentNode.Nodes.IndexOf(node.ParentNode) + 1) * 1000
                    + node.ParentNode.Nodes.IndexOf(node) + 1;
        }

        internal static List<TreeListNode> GetSiblingNodesBefore(TreeListNode node) { return GetSiblingNodes(node, true, false); }
        internal static List<TreeListNode> GetSiblingNodesAfter(TreeListNode node) { return GetSiblingNodes(node, false, true); }
        internal static List<TreeListNode> GetSiblingNodes(TreeListNode node, bool beforeOnly = false, bool afterOnly = false)
        {
            List<TreeListNode> siblingNodesConcerned = new List<TreeListNode>();

            //if there is no parent node, the node's index and the sibling nodes are assessed via the treelist itself
            TreeListNodes siblingNodes = node.ParentNode == null ? node.TreeList.Nodes : node.ParentNode.Nodes;
            int nodeIndex = node.ParentNode == null ? node.TreeList.Nodes.IndexOf(node) : node.ParentNode.Nodes.IndexOf(node);

            foreach (TreeListNode siblingNode in siblingNodes)
            {
                int siblingNodeIndex = node.ParentNode == null ? node.TreeList.Nodes.IndexOf(siblingNode) : node.ParentNode.Nodes.IndexOf(siblingNode);
                if (nodeIndex == siblingNodeIndex)
                    continue;
                if (afterOnly && siblingNodeIndex < nodeIndex)
                    continue;
                if (beforeOnly && siblingNodeIndex > nodeIndex)
                    continue;
                siblingNodesConcerned.Add(siblingNode);
            }

            return siblingNodesConcerned;
        }

        internal static void GetRangesOfHiddenNodes(TreeListNodes nodesToCheck, ref List<KeyValuePair<TreeListNode, TreeListNode>> rangesOfHiddenNodes)
        {
            TreeListNode fromNode = null;
            TreeListNode toNode = null;

            foreach (TreeListNode nodeToCheck in nodesToCheck)
            {
                if (nodeToCheck.Visible == true)
                {
                    if (fromNode != null) //node is the first visible after a (range of) hidden node(s)
                    {
                        rangesOfHiddenNodes.Add(new KeyValuePair<TreeListNode, TreeListNode>(fromNode, toNode)); //insert the range of hidden nodes into the list
                        fromNode = null;
                    }
                    if (nodeToCheck.HasChildren)
                        GetRangesOfHiddenNodes(nodeToCheck.Nodes, ref rangesOfHiddenNodes); //collect hidden sub-nodes of the (visible) node
                }
                else
                {
                    if (fromNode == null)
                        fromNode = nodeToCheck;
                    toNode = nodeToCheck; //always assume that hidden node is the last of the range (is overwritten if the following node is hidden too)
                }
            }
            if (fromNode != null) //if the last node is hidden the insert above would not be called
                rangesOfHiddenNodes.Add(new KeyValuePair<TreeListNode, TreeListNode>(fromNode, toNode));

            //note that, to keep things simple, this ignores hidden sub-nodes of hidden nodes;
            //therefore, to stay consistant, the unhide mechanism (UnhideNodeRange) unhides all sub-nodes of a nodes to unhide
        }

        internal void UnhideNodeRange(TreeListNode fromNode, TreeListNode toNode)
        {
            TreeListNodes siblingNodes = (fromNode.ParentNode == null) ? fromNode.TreeList.Nodes : fromNode.ParentNode.Nodes;

            int fromIndex = siblingNodes.IndexOf(fromNode);
            int toIndex = siblingNodes.IndexOf(toNode);

            foreach (TreeListNode siblingNode in siblingNodes)
            {
                int siblingIndex = siblingNodes.IndexOf(siblingNode);
                if (siblingIndex >= fromIndex && siblingIndex <= toIndex)
                {
                    siblingNode.Visible = true;
                    if (siblingNode.HasChildren)
                        UnhideNodeRange(siblingNode.FirstNode, siblingNode.LastNode); //unhide all sub-nodes (see note in GetRangesOfHiddenNodes)
                }
            }
        }

        internal TreeListNode GetSpecifiedNode(string ID)
        {
            return FocusSpecifiedNode(ID, false, false);
        }

        internal TreeListNode FocusSpecifiedNode(string ID, bool focus = true, bool promptIfNotFound = true)
        {
            IsSearchedNode searcher = new IsSearchedNode(_mainForm.GetTreeListBuilder().GetSystemIDs(), ID);
            TreatSpecificNodes treater = new TreatSpecificNodes(searcher, null, focus);
            _mainForm.treeList.NodesIterator.DoOperation(treater);

            int nNodes = treater.GetSpecificNodes().Count;
            if (nNodes > 0)
            {
                if (focus)
                    _mainForm.treeList.FocusedNode = treater.GetSpecificNodes().First();
                return treater.GetSpecificNodes().First();
            }

            if (promptIfNotFound)
                Tools.UserInfoHandler.ShowError("No corresponding component found.");
            return null;
        }

        internal List<string> GetIDsOfHiddenSystems()
        {
            List<string> idsOfHiddenSystems = new List<string>();
            try
            {
                foreach (TreeListColumn column in _mainForm.treeList.Columns)
                    if (column.Visible == false)
                        idsOfHiddenSystems.Add((column.Tag as SystemTreeListTag).GetSystemRow().ID);
            }
            catch (Exception exception)
            {
                //do nothing to prevent any action from failing (e.g. export systems may try to access systems which have DataRowState.Deleted)
                UserInfoHandler.RecordIgnoredException("TreeListManager.GetIDsOfHiddenSystems", exception);
            }
            return idsOfHiddenSystems;
        }

        internal void AddSystem()
        {
            if (CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()).GetSystemRows().Count == 0)
            {       // if this is the first system, just add it
                _mainForm.PerformAction(new CopySystemAction(
                    CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()), _mainForm._isAddOn), true, true);
            }
            else    // otherwise you need to copy it from an existing one
            {
                SelectSystemsForm selectSystemsForm = new SelectSystemsForm(_mainForm.GetCountryShortName());
                selectSystemsForm.SetSingleSelectionMode();
                selectSystemsForm.SetCaption("Select Base System");
                if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                    return;
                CountryConfig.SystemRow systemRow = selectSystemsForm.GetSelectedSystemRows().ElementAt(0);
                TreeListColumn baseColumn = _mainForm.treeList.Columns.ColumnByName(systemRow.Name);

                _mainForm.PerformAction(new CopySystemAction(baseColumn,
                                                                CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()),
                                                                CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName())), true, true);
            }
        }

        internal void DeleteSystems()
        {
            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(_mainForm.GetCountryShortName());
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;
            List<string> selectedSystemIDs = (from sR in selectSystemsForm.GetSelectedSystemRows() select sR.ID).ToList();
            if (selectedSystemIDs.Count == 0)
                return;
            List<TreeListColumn> columnsToDelete = new List<TreeListColumn>();
            foreach (TreeListColumn column in _mainForm.GetTreeListBuilder().GetSystemColums())
            {
                if (selectedSystemIDs.Contains((column.Tag as SystemTreeListTag).GetSystemRow().ID))
                    columnsToDelete.Add(column);
            }

            if (Tools.UserInfoHandler.GetInfo("Are you sure you want to delete the selected system(s)?", MessageBoxButtons.YesNo) == DialogResult.No) return;

            // this is a rather unclean thing to do and works only if one suspends system-formatting and crashed in debug-mode
            // there is too much happening ... removing conditional-formats, etc.
            foreach (TreeListColumn column in columnsToDelete)
            {
                bool update = column == columnsToDelete.Last();
                _mainForm.SuspendSystemFormatting(!update);
                _mainForm.PerformAction(new DeleteSystemAction(
                    column, CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName()), false), update, update);
            }
        }

        internal void ShowComponentSearchForm()
        {
            ComponentSearchForm componentSearchForm = new ComponentSearchForm();
            if (componentSearchForm.ShowDialog() == DialogResult.Cancel)
                return;

            FocusSpecifiedNode(componentSearchForm.GetIdentifier());
        }

        internal void ShowComponentUseForm()
        {
            if (_componentUseForm == null)
                _componentUseForm = new ComponentUseForm(_mainForm);
            _componentUseForm.ShowDialog();
        }

        static Dictionary<string, string> GetFunctionsExistingFootnoteParameters(TreeListNode functionNode, string systemName, bool getAmountParameters)
        {
            Dictionary<string, string> footnoteParameters = new Dictionary<string, string>();
            foreach (TreeListNode node in functionNode.Nodes)
            {
                string parameterName = node.GetDisplayText(TreeListBuilder._policyColumnName);
                string parameterGroup = node.GetDisplayText(TreeListBuilder._groupColumnName);
                if (parameterName.StartsWith("#") &&
                    ((getAmountParameters && parameterName.ToLower().Contains(DefPar.Value.AMOUNT.ToLower())) ||
                    (!getAmountParameters && !parameterName.ToLower().Contains(DefPar.Value.AMOUNT.ToLower()))))
                {
                    if (getAmountParameters) //reverse #_Amount to Amount#x as required by the formula
                        parameterName = DefPar.Value.AMOUNT + "#" + parameterGroup;
                    else //e.g. #_Level -> #_Level1
                        parameterName = parameterName + parameterGroup;
                    footnoteParameters.Add(parameterName, "(" + node.GetValue(systemName) + ") " + node.GetDisplayText(TreeListBuilder._commentColumnName));
                    string test = "(" + node.GetValue(systemName) + ") " + node.GetDisplayText(TreeListBuilder._commentColumnName);

                }
            }
            return footnoteParameters;
        }

        internal static Dictionary<string, string> GetFunctionsExistingFootnoteParameters(TreeListNode functionNode, string systemName)
        {//get all existing #x_ parameters of the function, e.g. #1_Level, #1_UpLim, etc., plus their value and description
            return GetFunctionsExistingFootnoteParameters(functionNode, systemName, false);
        }

        internal static Dictionary<string, string> GetFunctionsExistingAmountParameters(TreeListNode functionNode, string systemName)
        {//get all existing #x_Amount parameters of the function plus their value and description
            return GetFunctionsExistingFootnoteParameters(functionNode, systemName, true);
        }

        internal static int GetNextAvailableFootnoteCounter(TreeListNode functionNode)
        {
            int maxFootnote = int.MinValue;
            foreach (TreeListNode node in functionNode.Nodes)
            {
                string parameterName = node.GetDisplayText(TreeListBuilder._policyColumnName);              
                int footnote = -1;
                if (DefinitionAdmin.GetParDefinition(functionNode.GetDisplayText(TreeListBuilder._policyColumnName), parameterName).isFootnote)
                    footnote = EM_Helpers.SaveConvertToInt(node.GetDisplayText(TreeListBuilder._groupColumnName));
                if (footnote < 0)
                    continue;
                if (footnote > maxFootnote)
                    maxFootnote = footnote;
            }
            return (maxFootnote == int.MinValue) ? 1 : maxFootnote + 1;
        }

        internal string GetCountryShortName()
        {
            return _mainForm.GetCountryShortName();
        }

        internal string WhatIsWorkedOn(TreeListNode focusedNode)
        {
            if (focusedNode == null || focusedNode.Tag == null)
                return string.Empty;

            string policyName = (focusedNode.Tag as BaseTreeListTag).GetPolicyName();
            if (policyName == null)
                return string.Empty;

            string functionName = (focusedNode.Tag as BaseTreeListTag).GetFunctionName();
            if (functionName == null)
                return policyName;
            else
                return policyName + " - " + functionName;
        }

        internal void HandleDelete(bool byContextMenu)
        {
            if (byContextMenu //if the function is activated by the delete-item of the (policy or function) context menu ...
                || _mainForm.GetMultiCellSelector().DoesSelectionContainPolicyColumn(true)) //... or (activated by delete-button) the policy column is part of the selection ...
                HandleDeleteComponents(); //... delete the whole rows (i.e. selected policies, functions, parameters) ...
            else
                HandleDeleteContents(); //... otherwise just delete the content of the selected cells
        }

        void HandleDeleteComponents()
        {
            ActionGroup actionGroup = new ActionGroup();
            bool selectionContainsPolicies = false;
            bool selectionContainsCompulsoryPolicies = false;
            bool selectionContainsFunctions = false;

            //process the nodes bottom-up, to avoid deleting e.g. a parameter, which was already deleted in the course of deleting its function
            TreeListNode lastNodeToDelete = null;
            List<TreeListNode> selectedNodes = _mainForm.GetMultiCellSelector().GetSelectedNodes(true);
            for (int index = selectedNodes.Count - 1; index >= 0; --index)
            {
                TreeListNode selectedNode = selectedNodes.ElementAt(index);
                BaseTreeListTag tag = selectedNode.Tag as BaseTreeListTag;
                if (tag.GetDefaultParameterRow() != null)
                {
                    actionGroup.AddAction(new DeleteParameterAction(selectedNode));
                }
                else if (tag.GetDefaultFunctionRow() != null)
                {
                    actionGroup.AddAction(new DeleteFunctionAction(selectedNode));
                    if (selectionContainsFunctions == false)
                    {
                        lastNodeToDelete = null;
                        selectionContainsFunctions = true;
                    }
                }
                else
                {
                    actionGroup.AddAction(new DeletePolicyAction(selectedNode));
                    if (selectionContainsPolicies == false)
                    {
                        lastNodeToDelete = null;
                        selectionContainsPolicies = true;
                    }
                    foreach (string compulsoryPolicyName in DefPol.GetCompulsoryPolicies())
                    {
                        if (tag.GetPolicyName().ToLower().Contains(compulsoryPolicyName.ToLower()))
                        {
                            selectionContainsCompulsoryPolicies = true;
                            break;
                        }
                    }
                }
                if (lastNodeToDelete == null)
                    lastNodeToDelete = selectedNode; //to set the focus, see below
            }

            //pose security question subject to the 'highest level' component included in the selection
            if (selectionContainsPolicies)
            {
                string warningCompulsoryPolicy = selectionContainsCompulsoryPolicies ? "You are about to delete (a) compulsory policy/ies!\n\n" : string.Empty;
                if (Tools.UserInfoHandler.GetInfo(warningCompulsoryPolicy + "Are you sure you want to delete the policy/ies?", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            else if (selectionContainsFunctions) //no policies selected, ask whether functions should be deleted (unless warning is switched off)
            {
                if (!OptionalWarningsManager.Show(OptionalWarningsManager._deleteFunctionWarning))
                    return;
            }
            else //no policies and no functions selected, ask whether parameters should be deleted (unless warning is switched off)
            {
                if (!OptionalWarningsManager.Show(OptionalWarningsManager._deleteParameterWarning))
                    return;
            }

            //performance optimisation Aug 13: if only parameters are deleted update only the function instead of the whole tree
            //not necessary anymore, the tree is not updated anymore (but just the nodes deleted)
            //TreeListNode functionNode = null;
            //if (!selectionContainsPolicies && !selectionContainsFunctions && selectedNodes.Count > 0)
            //    functionNode = selectedNodes.ElementAt(0).ParentNode;

            //focus the next node after delete-area before performing action in order to avoid jumping to the very first node after the delete because the focused node was deleted
            //also, it is essential to have a focused node, as many functions within the UI rely on a focused node
            TreeListNode newFocusedNode = null;
            for (TreeListNode node = lastNodeToDelete; node != null; node = node.ParentNode)
            {
                if (node.NextNode == null)
                    continue; //if the last parameter/function is within the delete-area, focus the next function/policy
                newFocusedNode = node.NextNode;
                break;
            }
            if (newFocusedNode == null) //this happens only if the very last (visible) node is deleted within a selection of nodes ...
                if (_mainForm.treeList.Nodes.Count > 0) //... therefore, for this rare case, seems not necessary to do something more complicated (than selecting the first node)
                    _mainForm.treeList.FocusedNode = _mainForm.treeList.Nodes.FirstNode; 
            _mainForm.treeList.FocusedNode = newFocusedNode;

            _mainForm.PerformAction(actionGroup, false);
        }

        bool ProcessKey(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Up)
                _mainForm.GetTreeListManager().HandleMoveNodes(true);
            else if (e.Control && e.KeyCode == Keys.Down)
                _mainForm.GetTreeListManager().HandleMoveNodes(false);
            else if (e.KeyCode == Keys.Delete)
                _mainForm.GetTreeListManager().HandleDelete(false);
//            else if (e.KeyCode == Keys.Oemplus)
//                _mainForm.treeList.FocusedNode.Expanded = true;
//            else if (e.KeyCode == Keys.OemMinus)
//                _mainForm.treeList.FocusedNode.Expanded = false;
            else if (e.Alt && e.KeyCode == Keys.S)
                _mainForm.PerformAction(new SpreadParameterValueAction(_mainForm), false);
            else if (e.Alt && e.KeyCode == Keys.H)
                EM_UI.ContextMenu.RowContextMenu.HideRow(_mainForm.treeList.FocusedNode);
            else if (e.Alt && e.KeyCode == Keys.O)
                EM_UI.ContextMenu.RowContextMenu.HideAllOtherRows(_mainForm.treeList.FocusedNode);
            else
                return false;
            return true;
        }

        void HandleDeleteContents()
        {
            //'delete' the content of the selected cells, i.e. set to n/a
            List<TreeListColumn> selectedColumns = _mainForm.GetMultiCellSelector().GetSelectedColumns(true);
            List<TreeListNode> selectedNodes = _mainForm.GetMultiCellSelector().GetSelectedNodes(true);

            ActionGroup actionGroup = new ActionGroup();
            foreach (TreeListColumn selectedColumn in selectedColumns)
            {
                foreach (TreeListNode selectedNode in selectedNodes)
                {
                    if (TreeListBuilder.IsGroupColumn(selectedColumn)
                        && (selectedNode.Tag as BaseTreeListTag).GetDefaultParameterRow() != null) //group column can only be changed for parameter nodes
                        actionGroup.AddAction(new ChangeParameterGroupAction(selectedNode, string.Empty, true, selectedColumn));
                    else if (TreeListBuilder.IsSystemColumn(selectedColumn))
                        actionGroup.AddAction(new ChangeParameterValueAction(selectedNode, selectedColumn, string.Empty, true));
                    else if (TreeListBuilder.IsCommentColumn(selectedColumn))
                        actionGroup.AddAction(new ChangeParameterCommentAction(selectedNode, string.Empty, true, selectedColumn));
                    //if policy column is selected pressing delete leads to deleting the whole row not just the content (see above)
                }
            }
            _mainForm.PerformAction(actionGroup, false);
        }

        internal bool HandleMoveNodes(bool moveAboveTarget, TreeListNode targetNode = null)
        {//called via "key-moving" (ctrl-up/down), "contextmenu-moving" (move policies/funcions up/down) and drag-and-drop
            //get information about the nodes to move
            List<TreeListNode> moveNodes = _mainForm.GetMultiCellSelector().GetSelectedNodes(true, true);
            int highestSelectedLevel = MultiCellSelector.GetSelectionTopLevel(moveNodes, true);
            if (targetNode == null) //if called via context menu or shift-key: there is no target node, but the selected node(s) is/are to be moved up or down
            {                       //thus target node is next node / previous node of selection
                if (moveAboveTarget)
                    targetNode = moveNodes.First().PrevNode;
                else //moveDown: the selection may consist of e.g. functions and their parameters, thus the last selected node would be a parameter node
                {    //          but we need the next node of the function of this last selected parameter (also see comment in MultiCellSelector.GetSelectionTopLevel)
                    foreach (TreeListNode node in moveNodes.Reverse<TreeListNode>())
                        if (node.Level == highestSelectedLevel)
                        {
                            targetNode = node.NextNode;
                            break;
                        }
                }
            }
            else    // if called via drag & drop, confirm that the move is valid
            {
                if (moveAboveTarget && targetNode.PrevNode == moveNodes.Last()) return false;       // trying to move just after the last selected (no move)
                if (!moveAboveTarget && targetNode.NextNode == moveNodes.First()) return false;     // trying to move just before the first selected (no move)
                if (moveNodes.Contains(targetNode)) return false;       // trying to move within selection (no move)
            }

            //check if movement is possible, if not return false ...
            //... check if there is anything to move
            if (moveNodes.Count == 0 || targetNode == null)
                return false;
            //... find out whether policies, functions or parameters are to be moved ...
            if (highestSelectedLevel < 0) //... more than one level can be selected, i.e. policies and functions, but in a meaningful way (see function) ...
                return false;             //... thus refuse moving if no sensible selection
            //... level of moved nodes must be equal to target node level, e.g. policies cannot be moved below a function, parameters not above a policy, etc. ...
            if (highestSelectedLevel != targetNode.Level)
                return false;
            //... cannot move parameters from one function to another respectively functions from one policy to another ...
            if (moveNodes.First().ParentNode != targetNode.ParentNode)
                return false;

            //... ask whether to really move polcies or functions
            if ((targetNode.Tag as BaseTreeListTag).ShowOrderChangeWarning() &&
                !OptionalWarningsManager.Show(OptionalWarningsManager._changeOrderOfPolicyFunctionWarning))
                return false;

            //... some preparation: make sure that we have only top-level-nodes (e.g.) no parameter nodes in functions are to be moved)
            //                      make sure that we do not skip hidden nodes
            int topIndex = _mainForm.treeList.GetNodeIndex(moveNodes.First());
            int index = moveNodes.Count - 1;
            for (; index >= 0; --index)
                if (moveNodes.ElementAt(index).Level == highestSelectedLevel)
                    break;
            int botIndex = _mainForm.treeList.GetNodeIndex(moveNodes.ElementAt(index));
            moveNodes.Clear();
            foreach (TreeListNode node in targetNode.ParentNode == null ? _mainForm.treeList.Nodes : targetNode.ParentNode.Nodes)
                if (_mainForm.treeList.GetNodeIndex(node) >= topIndex && _mainForm.treeList.GetNodeIndex(node) <= botIndex)
                    moveNodes.Add(node);
            //... cannot move selection before/after a node that is part of the selection
            if (_mainForm.treeList.GetNodeIndex(targetNode) >= topIndex && _mainForm.treeList.GetNodeIndex(targetNode) <= botIndex)
                return false;

            //movement is ok, thus perform
            _mainForm.PerformAction(new MoveNodesAction(moveNodes, targetNode, moveAboveTarget), false);

            return true;
        }

        // This function will expand or collapse a single node
        internal void expandCollapseNodes(TreeListNode node, bool expand = true, bool deep = false, bool doRoot = false)
        {
            if (doRoot)      // If doRoot, find the policy that corresponds to this node
            {
                while (node.ParentNode != null) node = node.ParentNode;
            }
            if (node.HasChildren)   // if there is something to expand/collapse
            {
                if (!deep) node.Expanded = expand;  // if expanding/collapsing only this node
                else // if expanding/collapsing this node and all its children
                {
                    if (expand) node.ExpandAll();     // this is much faster than manually expanding everything
                    else
                    {   // manually collapse this node and all its children
                        node.Expanded = expand;
                        foreach (TreeListNode n in node.Nodes)  
                        {
                            if (n.HasChildren) n.Expanded = expand; 
                        }
                    }
                }
            }
        }

        // This function will expand or collapse multiple nodes
        internal void expandCollapseNodes(List<TreeListNode> nodeList, bool expand = true, bool deep = false, bool doRoot = false)
        {
            foreach (TreeListNode n in nodeList)
            {
                if (n.HasChildren) expandCollapseNodes(n, expand, deep, doRoot);
            }
        }

        //enforce updating of the info provided by combo-boxes and Intelli-sense if Def-functions are changed
        internal void UpdateIntelliAndTUBoxInfo() { foreach (TreeListColumn col in _mainForm.treeList.Columns) UpdateIntelliAndTUBoxInfo(string.Empty, col); }
        internal static void UpdateIntelliAndTUBoxInfo(string functionName, TreeListColumn column)
        {
            try
            {
                SystemTreeListTag tag = (column.Tag == null) ? null : (column.Tag as SystemTreeListTag);
                if (functionName == null || tag == null)
                    return;
                functionName = functionName.ToLower();
                if (functionName == string.Empty || functionName == DefFun.DefTu.ToLower())
                    (column.Tag as SystemTreeListTag).UpdateTUInfo();
                if (functionName == string.Empty || functionName == DefFun.DefIl.ToLower())
                    (column.Tag as SystemTreeListTag).UpdateILInfo();
                if (functionName == string.Empty || functionName == DefFun.DefConst.ToLower())
                    (column.Tag as SystemTreeListTag).UpdateConstantsInfo();
                if (functionName == string.Empty || functionName == DefFun.DefVar.ToLower())
                    (column.Tag as SystemTreeListTag).UpdateVariablesInfo();
            }
            catch (Exception exception)
            {
                UserInfoHandler.RecordIgnoredException("TreeListManager.UpdateIntelliAndTUBoxInfo", exception);
            }
        }

        internal void HandleCopyFunctions()
        {
            try
            {
                List<TreeListNode> copyNodes = new List<TreeListNode>();

                MultiCellSelector multiCellSelector = _mainForm.GetMultiCellSelector();
                if (multiCellSelector.HasSelection() &&
                    multiCellSelector.DoesSelectionContainPolicyColumn()) //ignore any selection if it does not include the policy column
                {
                    List<TreeListNode> selectedNodes = multiCellSelector.GetSelectedNodes();
                    int selectionLevel = MultiCellSelector.GetSelectionTopLevel(selectedNodes, true);
                    if (selectionLevel == 1) //one or more functions are selected and the top selected node is a function node (the "true" in the function above)
                    {
                        foreach (TreeListNode selectedNode in selectedNodes)
                            if (selectedNode.Tag as ParameterTreeListTag == null) //gather the function nodes (skip the paramter nodes)
                                copyNodes.Add(selectedNode);
                    }
                    else //something invalid is selected
                    {
                        if (!OptionalWarningsManager.Show(OptionalWarningsManager._invalidPasteFunctionsWarning))
                            return;
                    }
                }

                if (copyNodes.Count == 0)
                {
                    if ((_mainForm.treeList.FocusedNode.Tag as BaseTreeListTag).GetParameterName() != null)
                        copyNodes.Add(_mainForm.treeList.FocusedNode.ParentNode); //menu was called from parameter node
                    else
                        copyNodes.Add(_mainForm.treeList.FocusedNode); //menu was called from function node
                }

                EM_AppContext.Instance.SetPasteFunctionAction(new PasteFunctionAction(copyNodes, _mainForm));
            }
            catch (Exception exception)
            {
                EM_UI.Tools.UserInfoHandler.ShowException(exception);
            }
        }

        internal void HandleConditionalFormatting()
        {
            ConditionalFormattingAction action = new ConditionalFormattingAction(_mainForm.GetCountryShortName(), _countryConfigFacade);
            _mainForm.PerformAction(action, false);

            //expanding of differences between systems must be done after the action, because PerformAction stores the expand-states before the action and restores it afterwards
            if (action.ActionIsCanceled() == true)
                return;
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;
                foreach (KeyValuePair<string, string> systemToExpand in action.GetSystemsToExpand())
                {
                    IsNodeValueDifferent isNodeValueDifferent = new IsNodeValueDifferent(_mainForm.GetTreeListBuilder().GetSystemColumnByName(systemToExpand.Key),
                                                                                         _mainForm.GetTreeListBuilder().GetSystemColumnByName(systemToExpand.Value),
                                                                                         _mainForm._importByIDDiscrepancies);
                    TreatSpecificNodes treatSpecificNodes = new TreatSpecificNodes(isNodeValueDifferent, null, true); //the only "treatment" is expanding of differences
                    _mainForm.treeList.NodesIterator.DoOperation(treatSpecificNodes);
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
            finally
            {
                _mainForm.Cursor = Cursors.Default;
            }
        }

        internal void CheckForUnhandledChange()
        {   //this is called before saving to ensure that the most recent change is saved too; this is necessary because the treelist obviously does
            //not get the CellValueChanged-notification if the editor is still open and the treelist looses focus because the save button is pressed
            try
            {
                string displayedValue = _mainForm.treeList.FocusedNode.GetDisplayText(_mainForm.treeList.FocusedColumn);
                if (_mainForm.treeList.ActiveEditor != null)    //if there is an open editor, check if its value is saved
                {
                    string editorValue = _mainForm.treeList.ActiveEditor.EditValue.ToString();
                    if (editorValue != displayedValue)
                        HandleCellValueChanged(editorValue, _mainForm.treeList.FocusedColumn, _mainForm.treeList.FocusedNode);
                }
            }
            catch (Exception exception)
            {
                //do nothing if this does not work (thus, amongst others, no cecking of null-values is necessary)
                UserInfoHandler.RecordIgnoredException("TreeListManager.CheckForUnhandledChange", exception);
            }
        }

        internal void HandleExpandPrivate()
        {
            _mainForm.treeList.NodesIterator.DoOperation(new TreatSpecificNodes(
                      isSpecific: new IsNodePrivate(), treatment: null,
                      visualiseTreated: true, expandTreated: true, setVisibleTreated: true));
        }
    }
}
