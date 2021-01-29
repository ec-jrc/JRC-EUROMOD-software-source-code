using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetFunInfo(string sysId,
                                       Dictionary<string, Dictionary<string, string>> funs,
                                       Dictionary<string, Dictionary<string, string>> sysFuns,
                                       Dictionary<string, Dictionary<string, string>> refPols,
                                       bool ignorePrivate,
                                       ExeXml.CAO cao, out List<string> errors,
                                       bool readComment = false)
        {
            errors = new List<string>();

            foreach (Dictionary<string, string> sf in sysFuns.Values) // loop over <SYS_FUNs>
            {
                if (sf.ContainsKey(TAGS.SYS_ID) && sf[TAGS.SYS_ID] == sysId) // consider only <SYS_FUN> with the appropriate <SYS_ID>
                {
                    ExeXml.Fun fun = new ExeXml.Fun();
                    string funId = sf.GetOrEmpty(TAGS.FUN_ID);

                    if (funId == string.Empty) continue;

                    if (!funs.ContainsKey(funId))
                    { errors.Add($"No <{TAGS.FUN}> corresponds to <{TAGS.SYS_FUN}> with <{TAGS.FUN_ID}> {funId}"); continue; }

                    if (ignorePrivate)
                    {
                        string priv = funs[funId].GetOrEmpty(TAGS.PRIVATE);
                        if (priv != null && priv == DefPar.Value.YES) continue; // ignore if private
                    }

                    fun.Name = funs[funId].GetOrEmpty(TAGS.NAME); // get Name and PolId from <FUNs>
                    if (readComment) fun.comment = XmlHelpers.RemoveCData(funs[funId].GetOrEmpty(TAGS.COMMENT));
                    string polId = funs[funId].GetOrEmpty(TAGS.POL_ID);

                    bool? on = DefPar.Value.IsOn(sf.GetOrEmpty(TAGS.SWITCH));
                    if (on == null &&                              // ignore if switched to n/a ...
                        fun.Name != DefFun.AddOn_Applic) continue; // ... except the AddOn_Applic-function of policy AO_control_*
                    fun.on = on == true;

                    fun.order = GetOrder(sf, out string orderError);
                    if (orderError != null) errors.Add(orderError);

                    // assign the function to the corresponding policy (respectively policies - considering reference policies)
                    foreach (var pol in cao.pols)
                    {
                        if (pol.Key == polId ||
                            (refPols.ContainsKey(pol.Key) && refPols[pol.Key].GetOrEmpty(TAGS.REFPOL_ID) == polId))
                            pol.Value.funs.TryAdd(funId, fun);
                    }
                }
            }
        }
    }
}
