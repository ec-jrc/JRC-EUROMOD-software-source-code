using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptSysDataProp(EM2Data.SysDataItem sysDataItem)
        {
            sysDataItem.properties.Remove(EM2TAGS.USE_COMMON_DEFAULT);  // moved to data-level
            sysDataItem.properties.Remove(EM2TAGS.UPRATE);              // out-dated
            sysDataItem.properties.Remove(EM2TAGS.USE_DEFAULT);         // not used
        }

        // called for each extension-switch, see "explanation wrt new handling of policy switches" in Explanations.cs
        private void AdaptExtensionSwitchProp(EM2Data.PolSwitchItem sw,
                                              List<List<MultiProp>> extensionInfo,  // info from global extensions file
                                              Dictionary<string, EM2Item> policies, // all polices of the country
                                              out bool remove,
                                              EM2Country.Content ctryContent)
        {           
            sw.value = XmlHelpers.RemoveCData(sw.value); // e.g. <![CDATA[off]]> -> off (CDATA is just unnecessary)
            remove = sw.value == DefPar.Value.NA; // remove the (local) switch if set to n/a
            if (remove) return;

            // if "old style": generate a content for the extension:
            // get id(s) of policy/ies in the country that match(es) the pattern of the global switch policy
            // e.g. get id of Hungarian policy BTA_hu if sw.switchPolID points to global policy-switch with pattern 'BTA_??'
            foreach (string polID in GetExtensionPolIDs(sw.switchPolID, extensionInfo, policies))
                if ((from ep in ctryContent.extensionPol
                     where ep.Item1 == sw.switchPolID && ep.Item2 == polID
                     select ep).Count() == 0) // avoid double input, e.g. already transfered to "new style", but still the short-name (aka name-pattern) is BTA_??
                    ctryContent.extensionPol.Add(new System.Tuple<string, string, string>(sw.switchPolID, polID, "false"));
        }
    }
}
