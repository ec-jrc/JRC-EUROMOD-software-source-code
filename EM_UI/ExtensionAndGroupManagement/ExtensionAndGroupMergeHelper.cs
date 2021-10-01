using EM_UI.DataSets;
using EM_UI.VersionControl.Merging;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionAndGroupMergeHelper
    {
        internal static void GetLookGroupContent(CountryConfigFacade countryConfigFacade, string groupID,
            out List<string> policyIDs, out List<string> functionIDs, out List<string> parameterIDs)
        {
            CountryConfig countryConfig = countryConfigFacade.GetCountryConfig();
            policyIDs = (from pg in countryConfig.LookGroup_Policy where pg.LookGroupRow.ID.ToLower() == groupID.ToLower() select pg.PolicyID).ToList();
            functionIDs = (from fg in countryConfig.LookGroup_Function where fg.LookGroupRow.ID.ToLower() == groupID.ToLower() select fg.FunctionID).ToList();
            parameterIDs = (from pg in countryConfig.LookGroup_Parameter where pg.LookGroupRow.ID.ToLower() == groupID.ToLower() select pg.ParameterID).ToList();
        }

        internal static void GetExtensionContent(CountryConfigFacade countryConfigFacade, string extensionID,
            out Dictionary<string, bool> policyInfo, out Dictionary<string, bool> functionInfo, out Dictionary<string, bool> parameterInfo)
        {
            CountryConfig countryConfig = countryConfigFacade.GetCountryConfig();
            policyInfo = new Dictionary<string, bool>(); functionInfo = new Dictionary<string, bool>(); parameterInfo = new Dictionary<string, bool>();
            foreach (var pe in countryConfig.Extension_Policy)
                if (pe.ExtensionID == extensionID) policyInfo.Add(pe.PolicyID, pe.BaseOff);
            foreach (var fe in countryConfig.Extension_Function)
                if (fe.ExtensionID == extensionID) functionInfo.Add(fe.FunctionID, fe.BaseOff);
            foreach (var pe in countryConfig.Extension_Parameter)
                if (pe.ExtensionID == extensionID) parameterInfo.Add(pe.ParameterID, pe.BaseOff);
        }

        internal static void CopyGlobalExtensionFromAnotherConfig(SwitchablePolicyConfig switchablePolicyConfig, GlobLocExtensionRow origExtensionRow, MergeControl.NodeInfo node = null)
        {
            if(origExtensionRow != null)
            {
                switchablePolicyConfig.SwitchablePolicy.AddSwitchablePolicyRow(origExtensionRow.ID, origExtensionRow.ShortName, origExtensionRow.Name, origExtensionRow.Look);

            }
            else
            {
                switchablePolicyConfig.SwitchablePolicy.AddSwitchablePolicyRow(node.ID, node.cellInfo[0].text, node.cellInfo[1].text, node.cellInfo[2].text);
            }
        }

        internal static List<DataConfig.ExtensionRow> GetLocalExtensions(DataConfigFacade dataConfigFacade)
        {
            return (from e in dataConfigFacade.GetDataConfig().Extension select e).ToList();
        }

        internal static SwitchablePolicyConfig.SwitchablePolicyRow GetGlobalExtension(SwitchablePolicyConfigFacade switchablePolicyConfigFacade, string extensionID)
        {
            return (from e in switchablePolicyConfigFacade.GetSwitchablePolicyConfig().SwitchablePolicy where e.ID == extensionID select e).FirstOrDefault();
        }
    }
}
