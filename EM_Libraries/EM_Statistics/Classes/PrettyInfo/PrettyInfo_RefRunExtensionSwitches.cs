using EM_Common;
using System.Collections.Generic;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefRunExtensionSwitches : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_RUN_EXTENSION_SWITCHES; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                // if no specific ref, try gettijng all refs
                if (resources.refNo < 0)
                {
                    List<string> allRefs = new List<string>();
                    foreach (SystemInfo refSys in resources.reformSystems)
                    {
                        allRefs.Add(refSys.GetSystemName() + ": " + (refSys.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis)
                                    ? origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunExtensionSwitches(fileGenesis))
                                    : origText.Replace(ident, DefPar.Value.NA)));
                    }
                    return string.Join(",", allRefs);
                }
                else    // else get the specific ref
                {
                    return GetRefSys(out SystemInfo refSys, resources)
                                    ? refSys.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis)
                                                           ? origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunExtensionSwitches(fileGenesis))
                                                           : origText.Replace(ident, DefPar.Value.NA)
                                    : origText;
                }
            }
        }
    }
}