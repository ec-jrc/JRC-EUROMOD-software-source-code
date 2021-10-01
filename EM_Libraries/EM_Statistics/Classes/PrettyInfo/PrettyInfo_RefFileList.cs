using System.Linq;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefFileList : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_FILE_LIST; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.reformSystems != null && resources.reformSystems.Any()
                     ? origText.Replace(ident, string.Join(", ", from r in resources.reformSystems select r.GetFileName()))
                     : origText;
            }
        }
    }
}
