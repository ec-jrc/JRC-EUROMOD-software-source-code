using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Tools;
using System.Collections.Generic;

namespace EM_UI.NodeOperations
{
    internal class DoesNodeMatchPatterns : IsSpecificBase
    {
        string _patterns = string.Empty;
        TreeListColumn _column = null;
        bool _matchCase = false;
        bool _mustBeExpanded = false;
        bool _matchWord = false;
        List<TreeListNode> _mustBeWithinNodes = null;
        bool _includePrivateComments;

        internal DoesNodeMatchPatterns(string patterns, TreeListColumn column, bool matchCase = false, bool mustBeExpanded = false,  
                                       List<TreeListNode> mustBeWithinNodes = null, bool includePrivateComments = false, bool matchWord = false)
        {
            _patterns = patterns;
            _column = column;
            _matchCase = matchCase;
            _mustBeExpanded = mustBeExpanded;
            _mustBeWithinNodes = mustBeWithinNodes;
            _includePrivateComments = includePrivateComments;
            _matchWord = matchWord;
        }

        bool IsFullyExpanded(TreeListNode node)
        {
            if (node.ParentNode == null)
                return true; //policy node is always visible
            if (!node.ParentNode.Expanded)
                return false; //function node is only visible if parent policy node is expanded
            return IsFullyExpanded(node.ParentNode); //parameter node is only visible if parent policy and function nodes are expanded
        }

        internal override bool Execute(TreeListNode senderNode)
        {
            if (!senderNode.Visible || !_column.Visible) //do not take into account cells which are hidden due to row-hiding or columns moved to the hidden-system-box
                return false;                            //(not visible cells cannot be focused and thus are a e.g. a problem with search next)

            if (_mustBeExpanded && !IsFullyExpanded(senderNode))
                return false;

            if (_mustBeWithinNodes != null && !_mustBeWithinNodes.Contains(senderNode))
                return false;

            object cellValue = senderNode.GetValue(_column);
            if (cellValue == null)
                return false;
            foreach (string pattern in ConditionalFormattingHelper.GetFormatConditionPatterns(_patterns))
            {
                if (EM_Helpers.DoesValueMatchPattern(pattern, cellValue.ToString(), _matchCase, _matchWord))
                    return true;

                if (_includePrivateComments && TreeListManagement.TreeListBuilder.IsCommentColumn(_column))
                {
                    TreeListTags.BaseTreeListTag tag = senderNode.Tag as TreeListTags.BaseTreeListTag;
                    if (tag != null && EM_Helpers.DoesValueMatchPattern(pattern, tag.GetPrivateComment(), _matchCase, _matchWord))
                        return true;
                }
            }

            return false;
        }

    }
}
