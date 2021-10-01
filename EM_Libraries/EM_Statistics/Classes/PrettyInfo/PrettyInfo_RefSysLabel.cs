using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefSysLabel : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_SYS_LABEL; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                string replacement = null;
                if (labels != null)
                {
                    List<string> reformLabels = (from l in labels where l.packageKey == resources.packageKey select l.reformSystemLabels).FirstOrDefault();
                    if (reformLabels != null && resources.refNo >= 0 && reformLabels.Count > resources.refNo && !string.IsNullOrEmpty(reformLabels[resources.refNo]))
                        replacement = reformLabels[resources.refNo];
                }
                replacement = replacement ?? (GetRefSys(out SystemInfo refSys, resources) ? EM_Helpers.OutputNameToPretty(refSys.GetFileName()) : ident);
                return origText.Replace(ident, replacement);
            }
        }
    }
}
