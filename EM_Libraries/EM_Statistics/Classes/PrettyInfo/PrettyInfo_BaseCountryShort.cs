using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseCountryShort : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_COUNTRY_SHORT; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return origText.Replace(ident, resources.baseSystems[0].GetSystemName().Contains("_")
                                               ? resources.baseSystems[0].GetSystemName().Split('_')[0].ToUpper() : string.Empty);
            }
        }
    }
}
