using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.Actions
{
    internal class MoveNodesAction : BaseAction
    {
        List<TreeListNode> _moveNodes = null;
        TreeListNode _targetNode = null;
        bool _moveAboveTarget = true;

        internal MoveNodesAction(List<TreeListNode> moveNodes, TreeListNode targetNode, bool moveAboveTarget)
        {
            _moveNodes = moveNodes;
            _targetNode = targetNode;
            _moveAboveTarget = moveAboveTarget;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override void PerformAction()
        {
            TreeList treeList = _targetNode.TreeList;
            //-A- first move directly in treeview ...
            int targetIndex = treeList.GetNodeIndex(_targetNode);
            int selectionTopIndex = treeList.GetNodeIndex(_moveNodes.First());
            int selectionBotIndex = treeList.GetNodeIndex(_moveNodes.Last());

            //move up (accomplished by moving the selection above target node - or above target node's next node, if move below target)
            if (targetIndex < selectionTopIndex)
            {
                int index = _moveAboveTarget ? targetIndex : targetIndex + 1;
                foreach (TreeListNode node in _moveNodes)
                    treeList.SetNodeIndex(node, index++);
            }

            //move down (accomplished by moving the nodes between selection and targetNode above selection - including target node itself, if move below target)
            else if (targetIndex > selectionBotIndex)
            {
                List<TreeListNode> shiftUpNodes = new List<TreeListNode>();
                foreach (TreeListNode node in _targetNode.ParentNode == null ? treeList.Nodes : _targetNode.ParentNode.Nodes)
                    if (treeList.GetNodeIndex(node) > selectionBotIndex &&
                        (treeList.GetNodeIndex(node) < targetIndex || !_moveAboveTarget && treeList.GetNodeIndex(node) == targetIndex))
                        shiftUpNodes.Add(node);
                foreach (TreeListNode node in shiftUpNodes)
                    treeList.SetNodeIndex(node, selectionTopIndex++);
            }

            //else: target inside selection: should not happen unless programming error (thus do nothing)

            //-B- then reenact in data: avoids flickering and saves time, compared to reorder in data and update tree, though the latter may be cleaner
            TreeListNodes nodes = _targetNode.ParentNode == null ? treeList.Nodes : _targetNode.ParentNode.Nodes;

            //this takes care of the "hidden" uprating-policy (which is in fact not hidden in the narrow sense, but only exists in data but not in the tree)
            int allowForNotDisplayedPolicy = 0;
            if (_targetNode.ParentNode == null) //moved node is a policy-node
            { //re-order the policies starting with oder of hidden uprating-policy plus 1
                CountryConfig.PolicyRow firstPol = CountryConfigFacade.GetFirstPolicyRow((_targetNode.Tag as BaseTreeListTag).GetDefaultPolicyRow().SystemRow);
                if (firstPol.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name))
                    CountryAdministration.CountryAdministrator.GetCountryConfigFacade(EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName());
                        allowForNotDisplayedPolicy = EM_Helpers.SaveConvertToInt(firstPol.Order) + 1;
            }
            
            foreach (TreeListNode node in nodes)
                (node.Tag as BaseTreeListTag).SetOrder(treeList.GetNodeIndex(node) + allowForNotDisplayedPolicy);
        }
    }
}
