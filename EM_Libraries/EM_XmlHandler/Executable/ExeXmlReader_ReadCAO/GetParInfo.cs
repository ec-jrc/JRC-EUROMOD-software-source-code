using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetParInfo(string sysId,
                                       Dictionary<string, Dictionary<string, string>> pars,
                                       Dictionary<string, Dictionary<string, string>> sysPars,
                                       bool ignorePrivate,
                                       ExeXml.CAO cao, out List<string> errors,
                                       bool readComment = false)
        {
            errors = new List<string>();

            foreach (Dictionary<string, string> sp in sysPars.Values) // loop over <SYS_PARs>
            {
                if (sp.ContainsKey(TAGS.SYS_ID) && sp[TAGS.SYS_ID] == sysId) // consider only <SYS_PAR> with the appropriate <SYS_ID>
                {
                    ExeXml.Par par = new ExeXml.Par();
                    string parId = sp.GetOrEmpty(TAGS.PAR_ID);

                    if (parId == string.Empty) continue;

                    if (!pars.ContainsKey(parId))
                    { errors.Add($"No <{TAGS.PAR}> corresponds to <{TAGS.SYS_PAR}> with <{TAGS.PAR_ID}> {parId}"); continue; }

                    if (ignorePrivate)
                    {
                        string priv = pars[parId].GetOrEmpty(TAGS.PRIVATE);
                        if (priv != null && priv == DefPar.Value.YES) continue; // ignore if private
                    }

                    //par.val = EM_Helpers.AdaptDecimalSign(XmlHelpers.RemoveCData(sp.GetOrEmpty(TAGS.VALUE)));
                    par.val = XmlHelpers.RemoveCData(sp.GetOrEmpty(TAGS.VALUE));
                    if (par.val == DefPar.Value.NA) continue; // ignore n/a
                    par.order = sp.GetOrEmpty(TAGS.ORDER); // get <Order> for error messages (otherwise irrelevant for executable)

                    par.Name = pars[parId].GetOrEmpty(TAGS.NAME); // get Name, Group and FunId from <PARs>
                    if (readComment) par.comment = XmlHelpers.RemoveCData(pars[parId].GetOrEmpty(TAGS.COMMENT));
                    string funId = pars[parId].GetOrEmpty(TAGS.FUN_ID);
                    par.Group = pars[parId].GetOrEmpty(TAGS.GROUP);

                    par.val = allowInnerWhitespace.Contains(par.Name.ToLower()) ? par.val.ToLower().Trim()
                                                                                : EM_Helpers.RemoveWhitespace(par.val.ToLower());

                    // assign the parameter to the corresponding function
                    foreach (var pol in cao.pols)          // note that 'TryAdd' is actually necessary for parameters of
                        if (pol.Value.funs.ContainsKey(funId)) // reference policies, because the policies actually contain the
                            pol.Value.funs[funId].pars.TryAdd(parId, par); // same functions, not just copies of the same functions
                }
            }
        }

        private static List<string> _allowInnerWhitespace = null;
        private static List<string> allowInnerWhitespace
        {
            get
            {
                if (_allowInnerWhitespace == null) _allowInnerWhitespace = new List<string>()
                {
                    DefPar.CallProgramme.Programme.ToLower(),
                    DefPar.CallProgramme.Argument.ToLower(),
                    DefPar.CallProgramme.Path.ToLower(),
                    DefPar.DefInput.path.ToLower(),
                    DefPar.DefInput.file.ToLower(),
                    DefPar.AddOn_ExtensionSwitch.Extension_Name.ToLower()
                };
                return _allowInnerWhitespace;
            }
        }

    }
}
