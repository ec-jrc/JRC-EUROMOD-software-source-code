using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Transformer
{
    /// <summary>
    /// structure to store elements of country- or data-config-XML-files, which are:
    /// systems/policies/functions/parameters/uprating indices/datasets
    /// </summary>
    public class EM2Item
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public long order = -1;
        public string partentId = string.Empty;
        public Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static EM2Item Copy(EM2Item orig, List<EM2Country.SysItem> allSysVals, out List<EM2Country.SysItem> copiedSysVals)
        {
            EM2Item copy = new EM2Item() { id = Guid.NewGuid().ToString(), name = orig.name, order = orig.order, partentId = orig.partentId };
            foreach (var p in orig.properties) copy.properties.Add(p.Key, p.Value);

            List<EM2Country.SysItem> origSysVals = (from sv in allSysVals where sv.itemID == orig.id select sv).ToList();
            copiedSysVals = new List<EM2Country.SysItem>();
            foreach (EM2Country.SysItem origSysVal in origSysVals) // dublicate the values of the policy/function/parameter (order, off/on/value)
            {
                copiedSysVals.Add(new EM2Country.SysItem()
                {
                    sysID = origSysVal.sysID,
                    itemID = copy.id,
                    order = origSysVal.order,
                    value = origSysVal.value
                });
            }

            return copy;
        }
    }

    /// <summary>
    /// structure for XML-properties that cannot be stored in a Dictionary because they are not unique
    /// e.g. ExRates.System, Private.SysId, ...
    /// </summary>
    public class MultiProp
    {
        public string tag;
        public string content;
    }
}
