using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common_Win;
using EM_UI.Actions;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System;
using System.Windows.Forms;

namespace EM_UI.ContextMenu
{
    internal partial class CellContextMenu : UserControl
    {
        EM_UI_MainForm _mainForm = null;
        TreeListNode _senderNode = null;
        TreeListColumn _senderColumn = null;

        internal CellContextMenu(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        internal ContextMenuStrip GetContextMenu(TreeListNode senderNode, TreeListColumn senderColumn)
        {
            //private comments are not available in public versions
            mniPrivateComment.Visible = !EM_AppContext.Instance.IsPublicVersion();
            separator.Visible = !EM_AppContext.Instance.IsPublicVersion();

            //disable paste in (the rather unlikely) case that there is nothing on the clipboard
            mniPasteValues.Enabled = !MultiCellSelector.IsClipboardEmpty();

            _senderNode = senderNode;
            _senderColumn = senderColumn;

            return mnuCell;
        }

        void mniPrivateComment_Click(object sender, EventArgs e)
        {
            if (_senderNode == null || _senderNode.Tag == null)
                return;
            BaseTreeListTag nodeTag = _senderNode.Tag as BaseTreeListTag;
            if (nodeTag == null)
                return;

            string existingPrivateComment = nodeTag.GetPrivateComment();
            if (UserInput.Get("Insert/Edit Private Comment (for '" + _senderNode.GetDisplayText(_mainForm.GetTreeListBuilder().GetPolicyColumn()) + "')",
                               out existingPrivateComment, existingPrivateComment, true) != DialogResult.Cancel)
                nodeTag.SetPrivateComment(existingPrivateComment);
        }

        void mniCopyValues_Click(object sender, EventArgs e)
        {
            if (_senderColumn == null || _senderNode == null)
                return;
            _mainForm.treeList.FocusedNode = _senderNode;
            _mainForm.treeList.FocusedColumn = _senderColumn;
            _mainForm.GetMultiCellSelector().CopyToClipBoard();
        }

        void mniPasteValues_Click(object sender, EventArgs e)
        {
            if (_senderColumn == null || _senderNode == null)
                return;
            _mainForm.treeList.FocusedNode = _senderNode;
            _mainForm.treeList.FocusedColumn = _senderColumn;
            _mainForm.PerformAction(new PasteMultiValuesAction(_mainForm), false);
        }
    }
}
