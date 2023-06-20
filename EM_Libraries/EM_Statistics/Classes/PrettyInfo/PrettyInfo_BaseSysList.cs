using System.Linq;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseSysList : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_SYS_LIST; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.baseSystems != null && resources.baseSystems.Count > 0 ?
                       origText.Replace(ident, string.Join(", ", from r in resources.baseSystems select r.GetSystemName()))
                       : origText;
            }
        }
    }
}
