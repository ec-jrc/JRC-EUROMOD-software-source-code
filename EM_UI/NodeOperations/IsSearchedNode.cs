using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;

namespace EM_UI.NodeOperations
{
    internal class IsSearchedNode : IsSpecificBase
    {
        List<string> _systemIDs = null;
        string _ID = string.Empty;

        internal IsSearchedNode(List<string> systemIDs, string ID)
        {
            _systemIDs = systemIDs;
            _ID = ID;
        }

        internal override bool Execute(TreeListNode node)
        {
            BaseTreeListTag tag = node.Tag as BaseTreeListTag;
            foreach (string systemID in _systemIDs)
            {
                if (tag.GetID(systemID).ToLower() == _ID.ToLower())
                    return true;
            }
            return false;
        }
    }
}
