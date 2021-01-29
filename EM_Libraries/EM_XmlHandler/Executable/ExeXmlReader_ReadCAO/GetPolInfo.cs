using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetPolInfo(string sysId,
                                       Dictionary<string, Dictionary<string, string>> pols,
                                       Dictionary<string, Dictionary<string, string>> sysPols,
                                       Dictionary<string, Dictionary<string, string>> refPols,
                                       bool ignorePrivate,
                                       ExeXml.CAO cao, out List<string> errors, bool readComment = false)
        {
            errors = new List<string>();

            foreach (Dictionary<string, string> sp in sysPols.Values) // loop over <SYS_POLs>
            {
                if (sp.ContainsKey(TAGS.SYS_ID) && sp[TAGS.SYS_ID] == sysId) // consider only <SYS_POL> with the appropriate <SYS_ID>
                {
                    ExeXml.Pol pol = new ExeXml.Pol();
                    string polId = sp.GetOrEmpty(TAGS.POL_ID);

                    bool? on = DefPar.Value.IsOn(sp.GetOrEmpty(TAGS.SWITCH));
                    if (on == null) continue; // ignore if switched to n/a
                    pol.on = on == true;

                    pol.order = GetOrder(sp, out string orderError);
                    if (orderError != null) errors.Add(orderError);

                    if (polId == string.Empty) continue;

                    string rPolId = polId; // reference policies: the info must come from the referenced pol, but the id from the reference 
                    if (refPols.ContainsKey(polId)) rPolId = refPols[polId].GetOrEmpty(TAGS.REFPOL_ID);
                    if (pols.ContainsKey(rPolId))
                    {
                        pol.name = pols[rPolId].GetOrEmpty(TAGS.NAME); // get policy-name from <POLs>
                        if (readComment) pol.comment = XmlHelpers.RemoveCData(pols[rPolId].GetOrEmpty(TAGS.COMMENT));

                        if (ignorePrivate)
                        {
                            string priv = pols[rPolId].GetOrEmpty(TAGS.PRIVATE);
                            if (priv != null && priv == DefPar.Value.YES) continue; // ignore if private
                        }
                    }

                    cao.pols.Add(polId, pol);
                }
            }
        }
    }
}
