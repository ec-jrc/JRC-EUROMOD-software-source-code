using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListTags;
using EM_UI.DataSets;

namespace EM_UI.Actions
{
    internal class ChangeParameterCommentAction : BaseAction
    {
        TreeListNode _senderNode = null;
        string _newComment = null;
        bool _changeInTree = false;
        TreeListColumn _commentColumn = null;

        internal ChangeParameterCommentAction(TreeListNode senderNode, string newComment, bool changeInTree = false, TreeListColumn commentColumn = null)
        {
            _senderNode = senderNode;
            _newComment = newComment;
            _changeInTree = changeInTree;
            _commentColumn = commentColumn;
        }

        internal override void PerformAction()
        {
            (_senderNode.Tag as BaseTreeListTag).SetComment(_newComment);
            if (_changeInTree)
                _senderNode.SetValue(_commentColumn, _newComment);
        }
    }

}
