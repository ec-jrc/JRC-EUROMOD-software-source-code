using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;

namespace EM_UI.NodeOperations
{
    internal class GetNodeByID : IsSpecificBase
    {
        List<string> _nodeIDs = new List<string>();

        internal GetNodeByID(List<string> nodeIDs)
        {   //either provide the IDs of all systems ...
            _nodeIDs.AddRange(nodeIDs);
        }
        
        internal GetNodeByID(string nodeDefaultID)
        {   //... or provide the default ID (the one delivered by getDefaultID)
            _nodeIDs.Add(nodeDefaultID);
        }

        internal override bool Execute(TreeListNode senderNode)
        {
            BaseTreeListTag tag = senderNode.Tag as BaseTreeListTag;
            return _nodeIDs.Contains(tag.GetDefaultID());
        }

    }
}
