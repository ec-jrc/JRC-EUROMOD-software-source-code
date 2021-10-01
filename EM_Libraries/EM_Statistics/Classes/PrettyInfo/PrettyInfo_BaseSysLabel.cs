using EM_Common;
using System.Linq;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseSysLabel : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_SYS_LABEL; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                string label = labels == null ? null : (from l in labels where l.packageKey == resources.packageKey select l.baseSystemLabel).FirstOrDefault();
                string replacement = string.IsNullOrEmpty(label) ? EM_Helpers.OutputNameToPretty(resources.baseSystem.GetFileName()) : label;
                return origText.Replace(ident, replacement);
            }
        }
    }
}
