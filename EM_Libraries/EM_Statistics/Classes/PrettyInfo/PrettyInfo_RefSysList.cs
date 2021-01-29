using System.Linq;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefSysList : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_SYS_LIST; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.reformSystems != null && resources.reformSystems.Any()
                     ? origText.Replace(ident, string.Join(", ", from r in resources.reformSystems select r.GetSystemName()))
                     : origText;
            }
        }
    }
}
