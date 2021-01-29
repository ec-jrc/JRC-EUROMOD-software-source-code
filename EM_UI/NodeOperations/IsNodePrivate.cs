using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;

namespace EM_UI.NodeOperations
{
    internal class IsNodePrivate : IsSpecificBase
    {
        internal override bool Execute(TreeListNode senderNode)
        {
            return (senderNode.Tag as BaseTreeListTag).IsPrivate();
        }
    }
}
