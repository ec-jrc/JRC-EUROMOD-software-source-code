using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetExtensionInfo(string sysId, string dataId,
                                             Dictionary<string, Dictionary<string, string>> extSwitch,
                                             Dictionary<string, Dictionary<string, string>> localExt,
                                             Dictionary<string, Dictionary<string, string>> extPol,
                                             Dictionary<string, Dictionary<string, string>> extFun,
                                             Dictionary<string, Dictionary<string, string>> extPar,
                                             ExeXml.Country country)
        {
            foreach (Dictionary<string, string> ep in extPol.Values)
            {
                string extId = ep.GetOrEmpty(TAGS.EXTENSION_ID);
                if (!country.extensions.ContainsKey(extId)) country.extensions.Add(extId, new ExeXml.Extension());

                string polId = ep.GetOrEmpty(TAGS.POL_ID);
                string baseOff = ep.GetOrEmpty(TAGS.EXENTSION_BASEOFF);
                country.extensions[extId].polIds.Add(polId, baseOff == "true");
            }

            foreach (Dictionary<string, string> ef in extFun.Values)
            {
                string extId = ef.GetOrEmpty(TAGS.EXTENSION_ID);
                if (!country.extensions.ContainsKey(extId)) country.extensions.Add(extId, new ExeXml.Extension());

                string funId = ef.GetOrEmpty(TAGS.FUN_ID);
                string baseOff = ef.GetOrEmpty(TAGS.EXENTSION_BASEOFF);
                country.extensions[extId].funIds.Add(funId, baseOff == "true");
            }

            foreach (Dictionary<string, string> ep in extPar.Values)
            {
                string extId = ep.GetOrEmpty(TAGS.EXTENSION_ID);
                if (!country.extensions.ContainsKey(extId)) country.extensions.Add(extId, new ExeXml.Extension());

                string parId = ep.GetOrEmpty(TAGS.PAR_ID);
                string baseOff = ep.GetOrEmpty(TAGS.EXENTSION_BASEOFF);
                country.extensions[extId].parIds.Add(parId, baseOff == "true");
            }

            foreach (Dictionary<string, string> es in extSwitch.Values) // loop over <EXTENSION_SWITCHs>
            {
                string extId = es.GetOrEmpty(TAGS.EXTENSION_ID);
                if (!country.extensions.ContainsKey(extId) || es.GetOrEmpty(TAGS.SYS_ID) != sysId || es.GetOrEmpty(TAGS.DATA_ID) != dataId) continue;

                country.extensions[extId].on = DefPar.Value.IsOn(es.GetOrEmpty(TAGS.VALUE));

                if (localExt.ContainsKey(extId)) // the names are only set for local extensions (as they are defined in the country file)
                {                                // they stay empty for global extension (this is in fact only relevant for function AddOn_ExtensionSwitch)
                    localExt[extId].TryGetValue(TAGS.NAME, out country.extensions[extId].localLongName);
                    localExt[extId].TryGetValue(TAGS.SHORT_NAME, out country.extensions[extId].localShortName);
                }
            }
        }
    }
}
