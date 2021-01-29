using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ContextMenu
{
    internal partial class FunctionContextMenu : UserControl
    {
        EM_UI_MainForm _mainForm = null;

        ToolStripMenuItem _mniAddFunctionBefore = null;
        ToolStripMenuItem _mniAddFunctionAfter = null;
        const string _menuItemCaption_AddFunction = "Add Function ";
        const string _menuItemCaption_AddFunctionBefore = "Before";
        const string _menuItemCaption_AddFunctionAfter = "After";

        const string _menuItemCaption_PolicyFunctions = "Policy Functions";
        const string _menuItemCaption_SystemFunctions = "System Functions";
        const string _menuItemCaption_SpecialFunctions = "Special Functions";
        const string _menuItemCaption_AddOnFunctions = "Add-On Functions";
        const string _menuItemCaption_OtherSystemFunctions = "Other System Functions";

        internal ToolStripMenuItem BuildAddFunctionMenu(ContextMenuStrip parentMenu, string caption, bool addAsFirstItem, TreeListNode senderNode)
        {
            //remove any existing 'Add Function ...' menu item
            foreach (ToolStripItem deleteMenuItem in parentMenu.Items) 
            {
                if (deleteMenuItem.Text == caption)
                {
                    parentMenu.Items.Remove(deleteMenuItem);
                    break;
                }
            }

            //build the 'Add Function ...' menu item itself
            ToolStripMenuItem mainMenuItem = new ToolStripMenuItem(caption);
            if (addAsFirstItem) //'Add Function Before' and 'Add Function After' are the first entries of the function context menu
                parentMenu.Items.Insert(0, mainMenuItem);
            else //'Add Function' is the last entry of the policy context menu
                parentMenu.Items.Add(mainMenuItem);

            //get function names and descriptions (Dictionary key = name, Dictionary value = description)
            string policyName = (senderNode.Tag as BaseTreeListTag).GetPolicyName();
            Dictionary<string, string> menuItemNamesAndDescriptions = GetAddFunctionMenuItems(policyName);

            //build sub menu items
            ToolStripMenuItem actDropDown = mainMenuItem;
            foreach (string menuItemCaption in menuItemNamesAndDescriptions.Keys)
            {
                if (menuItemCaption.Substring(0, 1) == "#") //# indicates that a new dropdown menu item should be generated (e.g. 'System Functions')
                {
                    actDropDown = new ToolStripMenuItem(menuItemNamesAndDescriptions[menuItemCaption]);
                    mainMenuItem.DropDownItems.Add(actDropDown);
                }
                else
                {
                    ToolStripItem addMenuItem = actDropDown.DropDownItems.Add(menuItemCaption, null, new EventHandler(mniAddFunction_Click));

                    //tag informs the event handler where the new function should be added
                    if (senderNode.ParentNode == null || caption.ToLower().Contains(_menuItemCaption_AddFunctionBefore.ToLower()))
                        //'Add Function' from policy node (add as last function) or 'Add Function Before' from function node (add before node)
                        addMenuItem.Tag = senderNode;
                    else
                    {   //'Add Function After' from function node
                        if (senderNode.NextNode != null)
                            //add before next node
                            addMenuItem.Tag = senderNode.NextNode;
                        else
                            //add as last node
                            addMenuItem.Tag = senderNode.ParentNode;
                    }

                    addMenuItem.ToolTipText = menuItemNamesAndDescriptions[menuItemCaption];
                }
            }

            return mainMenuItem;
        }

        Dictionary<string, string> GetAddFunctionMenuItems(string policyName)
        {
            //assess if policy is a system policy or a regular policy
            //string kind = FunctionConfigFacade.functionTypePolicy;
            DefinitionAdmin.Fun.KIND kind = DefinitionAdmin.Fun.KIND.POLICY;

            foreach (string systemPolicyName in DefPol.GetSystemPolicies())
                if (policyName.ToLower().Contains(systemPolicyName.ToLower()))
                    kind = DefinitionAdmin.Fun.KIND.SYSTEM;

            //assess if policy has (a) related function(s), e.g. policy 'ILDef_cc' <-> function 'DefIL'
            Dictionary<string, string> menuItemNamesAndDescriptions = DefinitionAdmin.GetPolRelatedFuns(policyName);
            if (menuItemNamesAndDescriptions.Count > 0)
                menuItemNamesAndDescriptions.Add("#1", _menuItemCaption_OtherSystemFunctions); //# marks a drop-down menu, the number is just because dictionaries don't accept equal keys

            //get 'main functions', i.e. policy functions for regular policies and system functions for system functions
            //(for system policies with related functions the latter appear under the header 'other system functions')
            Dictionary<string, string> functionsOfKind = DefinitionAdmin.GetFunNamesAndDesc(kind);
            foreach (string functionOfKind in functionsOfKind.Keys)
                if (!menuItemNamesAndDescriptions.ContainsKey(functionOfKind))
                    menuItemNamesAndDescriptions.Add(functionOfKind, functionsOfKind[functionOfKind]);

            //now get 'subordered functions', i.e. system functions for regular policies and policy functions for system policies
            if (kind == DefinitionAdmin.Fun.KIND.POLICY)
            {
                menuItemNamesAndDescriptions.Add("#2", _menuItemCaption_SystemFunctions);
                kind = DefinitionAdmin.Fun.KIND.SYSTEM;
            }
            else
            {
                menuItemNamesAndDescriptions.Add("#2", _menuItemCaption_PolicyFunctions);
                kind = DefinitionAdmin.Fun.KIND.POLICY;
            }
            functionsOfKind = DefinitionAdmin.GetFunNamesAndDesc(kind);
            foreach (string functionOfKind in functionsOfKind.Keys)
                menuItemNamesAndDescriptions.Add(functionOfKind, functionsOfKind[functionOfKind]);

            //finally get special functions ...
            menuItemNamesAndDescriptions.Add("#3", _menuItemCaption_SpecialFunctions);
            functionsOfKind = DefinitionAdmin.GetFunNamesAndDesc(DefinitionAdmin.Fun.KIND.SPECIAL);
            foreach (string functionOfKind in functionsOfKind.Keys)
                menuItemNamesAndDescriptions.Add(functionOfKind, functionsOfKind[functionOfKind]);

            //... and add-on-functions if window shows an add-on and the policy is the ao_control_-policy (i.e. the only policy that can contain these functions)
            if (_mainForm._isAddOn && policyName.ToLower().StartsWith(ExeXml.AddOn.POL_AO_CONTROL.ToLower()))
            {
                menuItemNamesAndDescriptions.Add("#4", _menuItemCaption_AddOnFunctions);
                functionsOfKind = DefinitionAdmin.GetFunNamesAndDesc(DefinitionAdmin.Fun.KIND.ADDON);
                foreach (string functionOfKind in functionsOfKind.Keys)
                    menuItemNamesAndDescriptions.Add(functionOfKind, functionsOfKind[functionOfKind]);
            }

            return menuItemNamesAndDescriptions;
        }

        void GreyState()
        {
            GreyState(GetFunctionNode(_mainForm.treeList.FocusedNode));
        }

        void GreyState(TreeListNode senderNode)
        {
            if (senderNode == null)
                return;

            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            bool isParameterNode = treeListTag.GetDefaultParameterRow() != null;
            mniDeleteParameter.Enabled = isParameterNode;
            mniDeleteFunction.ShortcutKeys = isParameterNode ? System.Windows.Forms.Keys.None : System.Windows.Forms.Keys.Delete;
            mniDeleteParameter.ShortcutKeys = isParameterNode ? System.Windows.Forms.Keys.Delete : System.Windows.Forms.Keys.None;
            mniMoveFunctionDown.Enabled = senderNode.NextNode != null;
            mniMoveFunctionUp.Enabled = senderNode.PrevNode != null;
            mniPasteFunctionAfter.Enabled = mniPasteFunctionBefore.Enabled = EM_AppContext.Instance.GetPasteFunctionAction() != null;
            mniPrivate.Enabled = !_mainForm._isAddOn;
            //the following is in fact not really correct (a reasonable copy of the selection is only possible if the function-name-cell lies within the selection and the selection contains editable cells)
            //it's aim is just to indicate that it is not possible to copy the values of the function with this menu (unless they are selected)
            mniCopyValues.Enabled = _mainForm.GetMultiCellSelector().HasSelection();
            toolStripSeparator10.Enabled = !MultiCellSelector.IsClipboardEmpty();
            mniGroups.Enabled = !_mainForm._isAddOn;
            mniExtensions.Enabled = !_mainForm._isAddOn;
        }

        internal FunctionContextMenu(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        //provide context menu with appropriate 'Add Function ...' menu item, depending on the node sending the right mouse click
        internal ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {
            TreeListNode aSender = senderNode;
            if (senderNode.NextNode == null)
                aSender = senderNode.ParentNode; //'Add Function After' the last node is the same as 'Add Function' from a policy node
                                                 //(which always adds the function at the end)
            _mniAddFunctionAfter = BuildAddFunctionMenu(mnuFunction, _menuItemCaption_AddFunction + _menuItemCaption_AddFunctionAfter, true, aSender);
            _mniAddFunctionBefore = BuildAddFunctionMenu(mnuFunction, _menuItemCaption_AddFunction + _menuItemCaption_AddFunctionBefore, true, senderNode);

            GreyState(senderNode);
            return mnuFunction;
        }      

        #region ActionHandlers

        TreeListNode GetFunctionNode(TreeListNode parameterOrFunctionNode)
        {   //get function node if menu was opened from a parameter node
            BaseTreeListTag treeListTag = parameterOrFunctionNode.Tag as BaseTreeListTag;
            if (treeListTag.GetDefaultParameterRow() == null)
                return parameterOrFunctionNode;
            else
                return parameterOrFunctionNode.ParentNode;
        }

        void mniAddFunction_Click(object senderMenuItem, EventArgs e)
        {
            TreeListNode senderNode = (senderMenuItem as ToolStripMenuItem).Tag as TreeListNode;

            //add the function
            AddFunctionAction action = new AddFunctionAction(_mainForm, senderNode, senderMenuItem.ToString(), _mainForm.GetTreeListManager().GetIDsOfHiddenSystems());
            _mainForm.PerformAction(action, false);

            //after redrawing treeview expand whatever is necessary to make the new function visible
            _mainForm.GetTreeListBuilder().ExpandSpecificNode(action.GetAddedFunctionsIDs());
        }

        void mniPasteFunctionAfter_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPasteFunctionAfter.Enabled == false)
                return;
            EM_AppContext.Instance.GetPasteFunctionAction().SetPasteInfo(GetFunctionNode(_mainForm.treeList.FocusedNode), this._mainForm,
                                                                false, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems());
            _mainForm.PerformAction(EM_AppContext.Instance.GetPasteFunctionAction(), false);
        }

        void mniShowAddParameterForm_Click(object senderMenuItem, EventArgs e)
        {
            EM_AppContext.Instance.GetAddParameterForm().Show();
            EM_AppContext.Instance.GetAddParameterForm().UpdateContent(_mainForm.treeList.FocusedNode);
        }

        internal void mniMoveFunctionUp_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            _mainForm.GetTreeListManager().HandleMoveNodes(true);
        }

        internal void mniMoveFunctionDown_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            _mainForm.GetTreeListManager().HandleMoveNodes(false);
        }

        internal void mniDeleteFunction_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (!mniDeleteFunction.Enabled)
                return;

            //focus the function if the menu was called from a parameter node (of this function)
            TreeListNode functionNode = GetFunctionNode(_mainForm.treeList.FocusedNode);
            _mainForm.treeList.FocusedNode = functionNode;

            //assure that a possible selection of the MultiCellSelector does not refer to something completely different than the function clicked
            if (_mainForm.GetMultiCellSelector().HasSelection())
            {
                bool clearMultiCellSelector = false;
                if (!_mainForm.GetMultiCellSelector().GetSelectedNodes().Contains(functionNode) || //if the selection does not contain the clicked function ...
                    !_mainForm.GetMultiCellSelector().DoesSelectionContainPolicyColumn()) //... or the selection does not contain the policy column (i.e. some parameter values or comments are selected) ...
                    clearMultiCellSelector = true;
                else
                {//... or the selection contains something different from sibiling functions (i.e. other functions within the function's policy) and parameters of the clicked function ...
                    foreach (TreeListNode selectedNode in _mainForm.GetMultiCellSelector().GetSelectedNodes())
                    {
                        if (selectedNode.ParentNode != null && selectedNode.ParentNode == functionNode)
                            continue; //a parameter of the clicked function, that's fine
                        if (selectedNode.ParentNode != null && selectedNode.ParentNode == functionNode.ParentNode)
                            continue; //a sibling function, that's fine too
                        if (selectedNode.ParentNode != null && selectedNode.ParentNode.ParentNode != null && selectedNode.ParentNode.ParentNode == functionNode.ParentNode && //(i.e. parameter within the same policy)
                            _mainForm.GetMultiCellSelector().GetSelectedNodes().Contains(selectedNode.ParentNode))
                            continue; //a parameter of a sibling function that's within the selection, that's fine too
                        clearMultiCellSelector = true;
                        break;
                    }
                }
                if (clearMultiCellSelector)
                    _mainForm.ClearCellSelection(); //... just clear the selection, and thus make the TreeListManager delete the clicked (and therefore focused) function
            }

            _mainForm.GetTreeListManager().HandleDelete(true);
        }

        void mniCopyIdentifier_Click(object sender, EventArgs e)
        {
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            treeListTag.CopyIdentfierToClipboard();
        }

        void mniCopySymbolicIdentifier_Click(object sender, EventArgs e)
        {
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            treeListTag.CopySymbolicIdentfierToClipboard();
        }

        internal void mniDeleteParameter_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (!mniDeleteParameter.Enabled)
                return;

            //assure that a possible selection of the MultiCellSelector does not refer to something completely different than the parameter clicked
            if (_mainForm.GetMultiCellSelector().HasSelection())
            {
                TreeListNode parameterNode = _mainForm.treeList.FocusedNode; 
                bool clearMultiCellSelector = false;
                if (!_mainForm.GetMultiCellSelector().GetSelectedNodes().Contains(parameterNode) || //if the selection does not contain the clicked parameter ...
                    !_mainForm.GetMultiCellSelector().DoesSelectionContainPolicyColumn()) //... or the selection does not contain the policy column (i.e. some parameter values or comments are selected) ...
                    clearMultiCellSelector = true;
                else //... or the selection contains something different from sibiling parameters (i.e. other parameters within the parameter's function) ...
                {
                    foreach (TreeListNode selectedNode in _mainForm.GetMultiCellSelector().GetSelectedNodes())
                    {
                        if (selectedNode.ParentNode != null && selectedNode.ParentNode == parameterNode.ParentNode)
                            continue; //a sibling parameter, that's fine
                        clearMultiCellSelector = true;
                        break;
                    }
                }
                if (clearMultiCellSelector)
                    _mainForm.ClearCellSelection(); //... just clear the selection, and thus make the TreeListManager delete the clicked (and therefore focused) parameter
            }

            _mainForm.GetTreeListManager().HandleDelete(true);
        }

        internal void mniCopyFunction_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniCopyFunction.Enabled)
                _mainForm.GetTreeListManager().HandleCopyFunctions();
        }

        internal void mniPasteFunctionBefore_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPasteFunctionBefore.Enabled == false)
                return;
            EM_AppContext.Instance.GetPasteFunctionAction().SetPasteInfo(GetFunctionNode(_mainForm.treeList.FocusedNode), _mainForm,
                                                                true, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems());
            _mainForm.PerformAction(EM_AppContext.Instance.GetPasteFunctionAction(), false);
        }

        internal void mniCollapseAllFunctions_Click(object sender, EventArgs e)
        {
            GreyState();
            if (mniCollapseAllFunctions.Enabled == false)
                return;

            if (_mainForm.GetMultiCellSelector() != null && _mainForm.GetMultiCellSelector().HasSelection()) // if multiple cells are selected
            {
                _mainForm.GetTreeListManager().expandCollapseNodes(_mainForm.GetMultiCellSelector().GetSelectedNodes(), false, true, true);
            }
            else    // if only one cell is selected/focused
            {
                TreeListNode fn = _mainForm.treeList.FocusedNode;
                if (fn != null) _mainForm.GetTreeListManager().expandCollapseNodes(fn, false, true, true);
            }
/*            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            bool isParameterNode = treeListTag.GetDefaultParameterRow() != null;
            TreeListNode policyNode = isParameterNode ? _mainForm.treeList.FocusedNode.ParentNode.ParentNode : _mainForm.treeList.FocusedNode.ParentNode;

            foreach (TreeListNode focusedNode in policyNode.Nodes)
                focusedNode.Expanded = false;
            policyNode.Expanded = false;
*/        }

        internal void mniExpandAllFunctions_Click(object sender, EventArgs e)
        {
            GreyState();
            if (mniExpandAllFunctions.Enabled == false)
                return;

            if (_mainForm.GetMultiCellSelector() != null && _mainForm.GetMultiCellSelector().HasSelection()) // if multiple cells are selected
            {
                _mainForm.GetTreeListManager().expandCollapseNodes(_mainForm.GetMultiCellSelector().GetSelectedNodes(), true, true, true);
            }
            else    // if only one cell is selected/focused
            {
                TreeListNode fn = _mainForm.treeList.FocusedNode;
                if (fn != null) _mainForm.GetTreeListManager().expandCollapseNodes(fn, true, true, true);
            }

/*            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            bool isParameterNode = treeListTag.GetDefaultParameterRow() != null;

            TreeListNode policyNode = isParameterNode ? _mainForm.treeList.FocusedNode.ParentNode.ParentNode : _mainForm.treeList.FocusedNode.ParentNode; 
            policyNode.ExpandAll();
*/        }

        void mnuFunction_Opening(object sender, CancelEventArgs e)
        {
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            bool visible = (treeListTag.GetFunctionName().ToLower() == DefFun.DefIl.ToLower());
            mniDescriptionAsComment.Visible = visible; //this menu item is only available for the function DefIL
            toolStripSeparator_DefIL.Visible = visible;
        }

        void mniDescriptionAsComment_Click(object sender, EventArgs e)
        {
            //a(ny) system is necessary to find out which incomelists (via DefIL), constants (via DefConst) and variables (via DefVar) are defined and to gather their descriptions
            TreeListColumn anySystemColumn = null;
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (TreeListBuilder.IsSystemColumn(column))
                {
                    anySystemColumn = column;
                    break;
                }
            }
            if (anySystemColumn == null || anySystemColumn.Tag == null)
                return;

            _mainForm.Cursor = Cursors.WaitCursor;

            Dictionary<string, string> descriptionsILVarConst = new Dictionary<string, string>(); //gather incomelists and their description in a dictionary to be applied below
            SystemTreeListTag systemTag = anySystemColumn.Tag as SystemTreeListTag;
            foreach (DataSets.CountryConfig.ParameterRow ilParameterRow in systemTag.GetParameterRowsILs())
                if (!descriptionsILVarConst.Keys.Contains(ilParameterRow.Value.ToLower()))
                    descriptionsILVarConst.Add(ilParameterRow.Value.ToLower(), systemTag.GetILTUComment(ilParameterRow));
            foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsConstants())
                if (!descriptionsILVarConst.Keys.Contains(parameterRow.Name.ToLower()))
                    descriptionsILVarConst.Add(parameterRow.Name.ToLower(), parameterRow.Comment);
            foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsDefVariables())
                if (!descriptionsILVarConst.Keys.Contains(parameterRow.Name.ToLower()))
                    descriptionsILVarConst.Add(parameterRow.Name.ToLower(), parameterRow.Comment);

            //the necessary comment-changes must be gathered in an action-group (to allow for a common undo)
            ActionGroup actionGroup = new ActionGroup();

            //loop over the parameters of the DefIL-function to put the respective descriptions of its components in the comment column
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            TreeListNode functionNode = treeListTag.GetDefaultParameterRow() != null ? _mainForm.treeList.FocusedNode.ParentNode : _mainForm.treeList.FocusedNode;
            foreach (TreeListNode parameterNode in functionNode.Nodes)
            {
                CountryConfig.ParameterRow parameterRow = (parameterNode.Tag as ParameterTreeListTag).GetDefaultParameterRow();
                string description = string.Empty;
                if (descriptionsILVarConst.Keys.Contains(parameterRow.Name.ToLower())) //component is an incomelist, variable or constant
                    description = descriptionsILVarConst[parameterRow.Name.ToLower()];
                else //component is a variable (defined in the variables description file)
                    description = EM_AppContext.Instance.GetVarConfigFacade().GetDescriptionOfVariable(parameterRow.Name, _mainForm.GetCountryShortName());
                if (description != string.Empty)
                    actionGroup.AddAction(new ChangeParameterCommentAction(parameterNode, description, true, _mainForm.GetTreeListBuilder().GetCommentColumn()));
                //else: 'name' parameter, no description or unknown component
            }

            if (actionGroup.GetActionsCount() > 0)
                _mainForm.PerformAction(actionGroup, false);
            
            _mainForm.Cursor = Cursors.Default;
        }

        void mniCopyValues_Click(object sender, EventArgs e)
        {
            _mainForm.treeList.FocusedColumn = _mainForm.GetTreeListBuilder().GetPolicyColumn();
            _mainForm.GetMultiCellSelector().CopyToClipBoard();
        }

        void mniPasteValues_Click(object sender, EventArgs e)
        {
            _mainForm.treeList.FocusedColumn = _mainForm.GetTreeListBuilder().GetPolicyColumn();
            _mainForm.PerformAction(new PasteMultiValuesAction(_mainForm), false);
        }

        void mniPrivate_Click(object sender, EventArgs e)
        {
            SetSelectionPrivate(_mainForm);
        }

        internal static void SetSelectionPrivate(EM_UI_MainForm mainForm)
        {
            List<TreeListNode> selectedNodes = new List<TreeListNode>();
            TreeListNode focusedNode = mainForm.treeList.FocusedNode;
            if (mainForm.GetMultiCellSelector().HasSelection() && (focusedNode == null || mainForm.GetMultiCellSelector().GetSelectedNodes().Contains(focusedNode)))
                selectedNodes.AddRange(mainForm.GetMultiCellSelector().GetSelectedNodes());
            else selectedNodes.Add(focusedNode);
            mainForm.PerformAction(new SetSelectionPrivateStateAction(selectedNodes), false);
        }

        #endregion ActionHandlers

        #region groups_extensions

        void mniExtensionOrGroup_DropDownOpening(object sender, System.EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            mi.DropDownItems.Clear(); // replace the dummy-drop-down-menu with the relevant groups/extensions
            foreach (KeyValuePair<string, Image> ge in ExtensionAndGroupMenuManager.GetRelevantMenuItems(_mainForm.GetCountryShortName(), mi.Name))
                mi.DropDownItems.Add(ge.Key, ge.Value, mniExtensionOrGroup_ExtensionOrGroupSelected);
        }

        void mniExtensionOrGroup_ExtensionOrGroupSelected(object sender, System.EventArgs e)
        {
            ExtensionAndGroupMenuManager.MenuItemClicked(_mainForm, sender.ToString()); // sender = group/extension-name
        }

        #endregion groups_extensions
    }
}
