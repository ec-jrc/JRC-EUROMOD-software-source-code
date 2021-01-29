using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.NodeOperations;
using EM_UI.TreeListTags;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class IsExtensionMember : IsSpecificBase
    {
        List<string> policyIDs = new List<string>(), functionIDs = new List<string>(), parameterIDs = new List<string>();

        internal IsExtensionMember(CountryConfig countryConfig, DataConfig dataConfig, string extensionName, bool onOnly)
        {
            List<GlobLocExtensionRow> extensions = ExtensionAndGroupManager.GetExtensions((countryConfig.Country.Rows[0] as CountryConfig.CountryRow).ShortName);
            string extensionID = (from e in extensions where e.Name == extensionName select e.ID).FirstOrDefault(); if (extensionID == null) return;

            policyIDs = (from pe in countryConfig.Extension_Policy where pe.ExtensionID == extensionID && (!onOnly || !pe.BaseOff) select pe.PolicyID).ToList();
            functionIDs = (from fe in countryConfig.Extension_Function where fe.ExtensionID == extensionID && (!onOnly || !fe.BaseOff) select fe.FunctionID).ToList();
            parameterIDs = (from pe in countryConfig.Extension_Parameter where pe.ExtensionID == extensionID && (!onOnly || !pe.BaseOff) select pe.ParameterID).ToList();
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
