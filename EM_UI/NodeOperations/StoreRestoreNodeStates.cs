using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Nodes.Operations;
using EM_UI.TreeListTags;

namespace EM_UI.NodeOperations
{
    internal class StoreRestoreNodeStates : TreeListOperation
    {
        List<string> _expandedNodesIDs = null;
        List<string> _selectedNodesIDs = null;
        List<string> _hiddenNodesIDs = null;
        string _focusedNodeID = null;
        string _focusedNodeID_alternative = null;
        int _focusedColumnIndex = -1;
        bool _storeMode = true;

        void StoreStates(TreeListNode senderNode)
        {
            try
            {
                BaseTreeListTag senderNodeTag = senderNode.Tag as BaseTreeListTag; //not that using string senderNodeID = (senderNodeTag as BaseTreeListTag).GetDefaultID()
                                        //is not a good idea, because it slows the process down by many catches if the data-row stored in the tag does not exist any more
                if (senderNode.Expanded)
                    _expandedNodesIDs.Add(senderNodeTag.GetDefaultID());
                if (senderNode.Selected)
                    _selectedNodesIDs.Add(senderNodeTag.GetDefaultID());
                if (senderNode.Visible == false)
                    _hiddenNodesIDs.Add(senderNodeTag.GetDefaultID());
                if (senderNode.Focused)
                {
                    _focusedNodeID = senderNodeTag.GetDefaultID();
                    _focusedColumnIndex = senderNode.TreeList.Columns.IndexOf(senderNode.TreeList.FocusedColumn); //also store the focused column
                    _focusedNodeID_alternative = GetFocusedNodeID_alternative(senderNode);
                }
            }
            catch (Exception e)
            {
                //do not jeopardise tree-list building, because node state cannot be restored (e.g. nodes point at deleted data-rows)
                Tools.UserInfoHandler.RecordIgnoredException("StoreRestoreNodeStates.StoreStates", e);
            }
        }

        void RestoreStates(TreeListNode senderNode)
        {
            try
            {
                string senderNodeID = (senderNode.Tag as BaseTreeListTag).GetDefaultID();
                if (_expandedNodesIDs.Contains(senderNodeID))
                    senderNode.Expanded = true;
                if (_selectedNodesIDs.Contains(senderNodeID))
                    senderNode.Selected = true;
                senderNode.Visible = !(_hiddenNodesIDs.Contains(senderNodeID));

                if (_focusedNodeID == senderNodeID)
                {
                    senderNode.TreeList.SetFocusedNode(senderNode);
                    _focusedNodeID = string.Empty; //mark as found

                    if (_focusedColumnIndex != -1 && _focusedColumnIndex < senderNode.TreeList.Columns.Count) //also restore the focused column
                        senderNode.TreeList.FocusedColumn = senderNode.TreeList.Columns[_focusedColumnIndex];
                }
                if (_focusedNodeID_alternative == senderNodeID && _focusedNodeID != string.Empty) //2nd condition: focused node already found, no need for alternative
                    senderNode.TreeList.SetFocusedNode(senderNode);
            }
            catch (Exception e)
            {
                //do not jeopardise tree-list building, because node state cannot be restored (e.g. nodes point at deleted data-rows)
                Tools.UserInfoHandler.RecordIgnoredException("StoreRestoreNodeStates.RestoreStates", e);
            }
        }

        string GetFocusedNodeID_alternative(TreeListNode focusedNode)
        {   //get an alternative node to focus, if the currently focused node was deleted
            TreeListNode alternativeNode = null;

            if (focusedNode.NextNode != null)
                alternativeNode = focusedNode.NextNode;
            else if (focusedNode.PrevNode != null)
                alternativeNode = focusedNode.PrevNode;
            else
                alternativeNode = focusedNode.ParentNode;

            if (alternativeNode == null)
                return null;
            return (alternativeNode.Tag as BaseTreeListTag).GetDefaultID();
        }

        void ClearStores()
        {
            _expandedNodesIDs.Clear();
            _selectedNodesIDs.Clear();
            _hiddenNodesIDs.Clear();
            _focusedNodeID = null;
            _focusedNodeID_alternative = null;
            _storeMode = true;
        }

        internal StoreRestoreNodeStates() : base()
        {
            _expandedNodesIDs = new List<string>();
            _selectedNodesIDs = new List<string>();
            _hiddenNodesIDs = new List<string>();
        }

        public override void Execute(TreeListNode senderNode)
        {
            if (_storeMode)
                StoreStates(senderNode);
            else
                RestoreStates(senderNode);
        }

        public override void FinalizeOperation()
        {
            base.FinalizeOperation();
            if (!_storeMode)
                ClearStores();
            else
                _storeMode = !_storeMode;
        }

    }
}
