using EM_XmlHandler;
using System.Collections.Generic;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        /// <summary> performs all EM2->EM3 adaptations for the exchange-rates global file </summary>
        /// <in_out param name="exRates"> content of file as read by EM2Global.Read </param>
        internal void AdaptExRates(ref List<List<MultiProp>> exRates)
        {
            List<List<MultiProp>> newExRates = new List<List<MultiProp>>();
            foreach (List<MultiProp> rate in exRates)
            {
                List<MultiProp> newRate = new List<MultiProp>();
                foreach (var prop in rate)
                {   // transform from EM2: <ValidFor>bg_2006, bg_2007, bg_2008</ValidFor> (1 property)
                    // to EM3: <System>bg_2006</System>,<System>bg_2007</System>,<System>bg_2008</System> (n properties for n systems)
                    if (prop.tag == EM2TAGS.VALID_FOR)
                        foreach (string sys in prop.content.Split(',')) // consider to use system-id instead of system-name
                            newRate.Add(new MultiProp() { tag = EM_XmlHandler.TAGS.SYS_NAME, content = sys.Trim() });
                    else newRate.Add(prop);
                }
                newExRates.Add(newRate);
            }
            exRates = newExRates;
        }

        /// <summary> performs all EM2->EM3 adaptations for the (old) SwitchablePolicyConfig (new Extensions) file </summary>
        /// <in_out param name="switches"> content of file as read by EM2Global.Read </param>
        internal void AdaptExtensions(ref List<List<MultiProp>> polSwitches)
        {
            List<List<MultiProp>> extensions = new List<List<MultiProp>>();
            foreach (List<MultiProp> sw in polSwitches)
            {   
                List<MultiProp> ext = new List<MultiProp>();
                foreach (var prop in sw)
                {   // transform from EM2 "pattern" to EM3 "short-name", e.g. NamePattern=BTA_?? -> ShortName=BTA
                    // reason: in future this will not be used as a pattern to identify the switchable policy in the countries
                    // but just as a short-name for e.g. display in the run-tool
                    // note: automatic removal of wildcards is outcommeted, as it is confusing and should be done be developers manually
                    // also see "explanation wrt new handling of policy switches" in Explanations.cs
                    if (prop.tag == EM2TAGS.NAME_PATTERN)
                        ext.Add(new MultiProp()
                        {
                            tag = EM_XmlHandler.TAGS.SHORT_NAME,
                            content = XmlHelpers.RemoveCData(prop.content) //.Replace("_??", string.Empty).Replace("_*", string.Empty)
                        });
                    else ext.Add(new MultiProp() // other properties (ID, ...) unchanged, except from a little cosmectic change ...
                        {
                            tag = prop.tag == EM2TAGS.LONG_NAME ? EM_XmlHandler.TAGS.NAME : prop.tag, // ... LongName -> Name
                            content = prop.content
                        });
                }
                extensions.Add(ext);
            }
            polSwitches = extensions;
        }
    }
}
