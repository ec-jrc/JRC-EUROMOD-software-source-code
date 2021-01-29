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
    internal class ChangeParameterGroupAction : BaseAction
    {
        TreeListNode _senderNode = null;
        string _newGroup = string.Empty;
        bool _changeInTree = false;
        TreeListColumn _groupColumn = null;

        internal ChangeParameterGroupAction(TreeListNode senderNode, string newGroup, bool changeInTree = false, TreeListColumn groupColumn = null)
        {
            _senderNode = senderNode;
            _newGroup = newGroup;
            _changeInTree = changeInTree;
            _groupColumn = groupColumn;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override void PerformAction()
        {
            foreach (CountryConfig.ParameterRow parameterRow in (_senderNode.Tag as ParameterTreeListTag).GetParameterRows())
                parameterRow.Group = _newGroup;
            if (_changeInTree)
                _senderNode.SetValue(_groupColumn, _newGroup);
        }
    }
}
