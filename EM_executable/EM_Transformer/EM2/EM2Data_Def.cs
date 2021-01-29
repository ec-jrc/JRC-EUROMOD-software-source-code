using System;
using System.Collections.Generic;

namespace EM_Transformer
{
    /// <summary>
    /// class for reading an EM2 country-dataconfig-XML-file into an EM2Data.Content structure (see below)
    /// note the comment in EM2Country_Def concerning class-responsibility for EM2->EM3 "content"- vs. "structure"-adaptation
    /// </summary>
    public partial class EM2Data
    {
        /// <summary> the complete content of a EM2 country-dataconfig-XML-file </summary>
        public class Content
        {
            // datset-properties:
            // dic-key = id
            // dic-value (EM2Item) = id (redundant), name, other properties (yearinc, currency, commondefault, ...)
            public Dictionary<string, EM2Item> dataSets = new Dictionary<string, EM2Item>();

            // dataset-system combination properties: sys-id, data-id, other properties (bestmatch, ...)
            public List<SysDataItem> sysData = new List<SysDataItem>();
            public List<PolSwitchItem> policySwitches = new List<PolSwitchItem>();
            public List<EM2Item> localExtensions = new List<EM2Item>();
        }

        /// <summary> structure to store dataset-system combination properties </summary>
        public class SysDataItem
        {
            public string sysID = string.Empty;
            public string dataID = string.Empty;
            public Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public string sysName = string.Empty; // will actually be dropped (i.e. not written by EM3Country.Write)
        }

        /// <summary> structure to store the PolicySwitch-elements of the country-dataconfig-XML-file </summary>
        public class PolSwitchItem
        {
            public string switchPolID = string.Empty; // the id in the global switch-file
            public string sysID = string.Empty;
            public string dataID = string.Empty;
            public string value = string.Empty;       // on / off / n/a
        }
    }
}
