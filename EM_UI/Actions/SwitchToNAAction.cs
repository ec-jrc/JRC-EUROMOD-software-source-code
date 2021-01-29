using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.TreeListTags;

namespace EM_UI.Actions
{
    internal class SwitchToNAAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        TreeListNode _senderNode = null;
        TreeListColumn _senderColumn = null;

        internal SwitchToNAAction(EM_UI_MainForm mainForm, TreeListNode senderNode, TreeListColumn senderColumn)
        {
            _mainForm = mainForm;
            _senderNode = senderNode;
            _senderColumn = senderColumn;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override void PerformAction()
        {
            string systemID = (_senderColumn.Tag as SystemTreeListTag).GetSystemRow().ID;
            (_senderNode.Tag as BaseTreeListTag).SetToNA(systemID);
            _senderNode.SetValue(_senderColumn, DefPar.Value.NA);
            SetChildrenToNA(_senderNode, systemID);
        }

        void SetChildrenToNA(TreeListNode parentNode, string systemID)
        {
            foreach (TreeListNode childNode in parentNode.Nodes)
            {
                (childNode.Tag as BaseTreeListTag).SetToNA(systemID);
                childNode.SetValue(_senderColumn, DefPar.Value.NA);
                if (parentNode.HasChildren)
                    SetChildrenToNA(childNode, systemID);
            }
        }
    }
}
