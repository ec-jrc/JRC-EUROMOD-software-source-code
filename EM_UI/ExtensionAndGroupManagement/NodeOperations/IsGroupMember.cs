using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.NodeOperations;
using EM_UI.TreeListTags;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class IsGroupMember : IsSpecificBase
    {
        List<string> policyIDs, functionIDs, parameterIDs;

        internal IsGroupMember(CountryConfig countryConfig, string groupName)
        {
            policyIDs = (from pg in countryConfig.LookGroup_Policy where pg.LookGroupRow.Name.ToLower() == groupName.ToLower() select pg.PolicyID).ToList();
            functionIDs = (from fg in countryConfig.LookGroup_Function where fg.LookGroupRow.Name.ToLower() == groupName.ToLower() select fg.FunctionID).ToList();
            parameterIDs = (from pg in countryConfig.LookGroup_Parameter where pg.LookGroupRow.Name.ToLower() == groupName.ToLower() select pg.ParameterID).ToList();
        }

        internal override bool Execute(TreeListNode senderNode)
        {
            BaseTreeListTag tag = senderNode.Tag as BaseTreeListTag;
            if (tag.GetDefaultParameterRow() != null) return parameterIDs.Contains(tag.GetDefaultParameterRow().ID);
            if (tag.GetDefaultFunctionRow() != null) return functionIDs.Contains(tag.GetDefaultFunctionRow().ID);
            return policyIDs.Contains(tag.GetDefaultPolicyRow().ID);
        }
    }
}
