using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.ContextMenu
{
    internal partial class PolicyContextMenu : UserControl
    {
        EM_UI_MainForm _mainForm = null;
        string _clickedMainMenu = string.Empty;
        const string _menuItemCaption_AddFunc = "Add Function";
        ToolStripMenuItem _mniAddFunction = null;

        void GreyState()
        {
            GreyState(_mainForm.treeList.FocusedNode);
        }

        void GreyState(TreeListNode senderNode)
        {
            if (senderNode == null)
                return;
            bool singlePolicyView = (_mainForm != null && _mainForm.GetTreeListBuilder() != null && _mainForm.GetTreeListBuilder().SinglePolicyView);
            mniMovePolicyDown.Enabled = senderNode.NextNode != null && !singlePolicyView;
            mniMovePolicyUp.Enabled = senderNode.PrevNode != null && !singlePolicyView;
            mniPastePolicyAfter.Enabled = mniPastePolicyBefore.Enabled = EM_AppContext.Instance.GetPastePolicyAction() != null && !singlePolicyView;
            mniPasteReferenceBefore.Enabled = mniPasteReferenceAfter.Enabled = !_mainForm._isAddOn &&
                EM_AppContext.Instance.GetPastePolicyAction() != null &&
                EM_AppContext.Instance.GetPastePolicyAction().GetCopyCountryShortName() == _mainForm.GetCountryShortName() &&
                !singlePolicyView;
            mniPasteFunction.Enabled = EM_AppContext.Instance.GetPasteFunctionAction() != null && !PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniCopyPolicy.Enabled = !PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniRenamePolicy.Enabled = !PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniChangePolicyType.Enabled = !PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniPrivate.Enabled = !_mainForm._isAddOn;
            mniGoToReferredPolicy.Enabled = PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniAddPolicyAfter.Enabled = !singlePolicyView;
            mniAddPolicyBefore.Enabled = !singlePolicyView;
            mniDeletePolicy.Enabled = !singlePolicyView;
            //the following is in fact not really correct (a reasonable copy of the selection is only possible if the policy-name-cell lies within the selection and the selection contains editable cells)
            //it's aim is just to indicate that it is not possible to copy the values of the policy with this menu (unless they are selected)
            mniCopyValues.Enabled = _mainForm.GetMultiCellSelector().HasSelection();
            mniPasteValues.Enabled = !MultiCellSelector.IsClipboardEmpty();
            if (_mniAddFunction == null)
                return; 
            _mniAddFunction.Enabled = !PolicyTreeListTag.IsReferencePolicy(senderNode);
            mniGroups.Enabled = !_mainForm._isAddOn;
            mniExtensions.Enabled = !_mainForm._isAddOn;
        }

        internal ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {
            _mniAddFunction = _mainForm.GetFunctionContextMenu().BuildAddFunctionMenu(_mainForm.GetPolicyContextMenu().mnuPolicy, _menuItemCaption_AddFunc,
                                                                                    false, senderNode);
            GreyState(senderNode);
            return mnuPolicy;
        }

        internal PolicyContextMenu(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        #region EventHandlers

        void mniAddPolicyBefore_DropDownOpened(object senderMenuItem, EventArgs e)
        {
            _clickedMainMenu = mniAddPolicyBefore.Name; //MainMenu_AddBefore; //store the main menu because the sub menu (when activated) doesn't give this information
                                                        //(concretely: one knows that something should happen with a benefi/tax/etc., but not what)
        }

        void mniAddPolicyAfter_DropDownOpened(object senderMenuItem, EventArgs e)
        {
            _clickedMainMenu = mniAddPolicyAfter.Name;
        }

        void mniChangePolicyType_DropDownOpened(object senderMenuItem, EventArgs e)
        {
            _clickedMainMenu = mniChangePolicyType.Name;
        }

        void mnuPolicyTypes_Click(object senderMenuItem, EventArgs e)
        {
            string policyType = senderMenuItem.ToString(); //benefit, tax, sic, etc.

            if (_clickedMainMenu == mniAddPolicyBefore.Name) //ActMainMenu is set in PolMenI_xxx_DropDownOpened
                _mainForm.PerformAction(new AddPolicyAction(_mainForm, _mainForm.treeList.FocusedNode, policyType, true, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems()), false);
            else if (_clickedMainMenu == mniAddPolicyAfter.Name)
                _mainForm.PerformAction(new AddPolicyAction(_mainForm, _mainForm.treeList.FocusedNode, policyType, false, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems()), false);
            else if (_clickedMainMenu == mniChangePolicyType.Name)
                    _mainForm.PerformAction(new ChangePolicyTypeAction(_mainForm.treeList.FocusedNode, policyType));
            _clickedMainMenu = string.Empty;
        }

        void mniCopyIdentifier_Click(object sender, EventArgs e)
        {
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            if (treeListTag != null)
                treeListTag.CopyIdentfierToClipboard();
        }

        void mniCopySymbolicIdentifier_Click(object sender, EventArgs e)
        {
            BaseTreeListTag treeListTag = _mainForm.treeList.FocusedNode.Tag as BaseTreeListTag;
            if (treeListTag != null)
                treeListTag.CopySymbolicIdentfierToClipboard();
        }

        void mniPrivate_Click(object sender, EventArgs e)
        {
            GreyState();
            if (!_mainForm._isAddOn) FunctionContextMenu.SetSelectionPrivate(_mainForm);
        }

        private void mniGoToReferredPolicy_Click(object sender, EventArgs e)
        {
            GreyState();
            if (mniGoToReferredPolicy.Enabled) _mainForm.GetTreeListManager().HandleF3();
        }

        void mniCollapseAllFunctions_Click(object senderMenuItem, EventArgs e)
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

//            foreach (TreeListNode focusedNode in _mainForm.treeList.FocusedNode.Nodes)
//                focusedNode.Expanded = false;
//            _mainForm.treeList.FocusedNode.Expanded = false;
        }

        void mniExpandAllFunctions_Click(object senderMenuItem, EventArgs e)
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

//            _mainForm.treeList.FocusedNode.ExpandAll();
        }

        void mniRenamePolicy_Click(object senderMenuItem, EventArgs e)
        {
            GreyState(); 
            if (mniRenamePolicy.Enabled)
                _mainForm.PerformAction(new RenamePolicyAction(_mainForm, _mainForm.treeList.FocusedNode, _mainForm.GetCountryShortName()), false);
        }

        internal void mniMovePolicyUp_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            _mainForm.GetTreeListManager().HandleMoveNodes(true);
        }

        internal void mniMovePolicyDown_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            _mainForm.GetTreeListManager().HandleMoveNodes(false);
        }

        internal void mniDeletePolicy_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (!mniDeletePolicy.Enabled)
                return;

            //assure that a possible selection of the MultiCellSelector does not refer to something completely different than the policy clicked
            if (_mainForm.GetMultiCellSelector().HasSelection())
            {
                if (!_mainForm.GetMultiCellSelector().GetSelectedNodes().Contains(_mainForm.treeList.FocusedNode) || //if the selection does not contain the clicked policy ...
                    !_mainForm.GetMultiCellSelector().DoesSelectionContainPolicyColumn()) //... or the selection does not contain the policy column (i.e. some parameter values or comments are selected) ...
                    _mainForm.ClearCellSelection(); //... just clear the selection, and thus make the TreeListManager delete the clicked (and therefore focused) policy
            }
            _mainForm.GetTreeListManager().HandleDelete(true);
        }

        internal void mniCopyPolicy_Click(object senderMenuItem, EventArgs e)
        {
            GreyState(); 
            if (mniCopyPolicy.Enabled)
                EM_AppContext.Instance.SetPastePolicyAction(new PastePolicyAction(_mainForm.treeList.FocusedNode, _mainForm));
        }

        internal void mniPastePolicyBefore_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPastePolicyBefore.Enabled == false)
                return;
            EM_AppContext.Instance.GetPastePolicyAction().SetPasteInfo(_mainForm.treeList.FocusedNode, this._mainForm,
                                                                    true, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems(), false);
            _mainForm.PerformAction(EM_AppContext.Instance.GetPastePolicyAction(), false);
        }

        void mniPastePolicyAfter_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPastePolicyAfter.Enabled == false)
                return;
            EM_AppContext.Instance.GetPastePolicyAction().SetPasteInfo(_mainForm.treeList.FocusedNode, this._mainForm,
                                                                    false, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems(), false);
            _mainForm.PerformAction(EM_AppContext.Instance.GetPastePolicyAction(), false);
        }

        void mniPasteFunction_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPasteFunction.Enabled == false)
                return;
            EM_AppContext.Instance.GetPasteFunctionAction().SetPasteInfo(_mainForm.treeList.FocusedNode, this._mainForm,
                                                                false, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems());
            _mainForm.PerformAction(EM_AppContext.Instance.GetPasteFunctionAction(), false);
        }

        void mniPasteReferenceBefore_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPasteReferenceBefore.Enabled == false)
                return;
            EM_AppContext.Instance.GetPastePolicyAction().SetPasteInfo(_mainForm.treeList.FocusedNode, this._mainForm,
                                                                    true, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems(), true);
            _mainForm.PerformAction(EM_AppContext.Instance.GetPastePolicyAction(), false);
        }

        void mniPasteReferenceAfter_Click(object senderMenuItem, EventArgs e)
        {
            GreyState();
            if (mniPasteReferenceAfter.Enabled == false)
                return;
            EM_AppContext.Instance.GetPastePolicyAction().SetPasteInfo(_mainForm.treeList.FocusedNode, this._mainForm,
                                                                    false, _mainForm.GetTreeListManager().GetIDsOfHiddenSystems(), true);
            _mainForm.PerformAction(EM_AppContext.Instance.GetPastePolicyAction(), false);
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

        #endregion EventHandlers

        #region groups_extensions

        void mniExtensionOrGroup_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            mi.DropDownItems.Clear(); // replace the dummy-drop-down-menu with the relevant groups
            foreach (KeyValuePair<string, Image> ge in ExtensionAndGroupMenuManager.GetRelevantMenuItems(_mainForm.GetCountryShortName(), mi.Name))
                mi.DropDownItems.Add(ge.Key, ge.Value, mniExtensionOrGroup_ExtensionOrGroupSelected);
        }

        void mniExtensionOrGroup_ExtensionOrGroupSelected(object sender, EventArgs e)
        {
            ExtensionAndGroupMenuManager.MenuItemClicked(_mainForm, sender.ToString()); // sender = group/extension-name
        }

        #endregion groups_extensions
    }
}
