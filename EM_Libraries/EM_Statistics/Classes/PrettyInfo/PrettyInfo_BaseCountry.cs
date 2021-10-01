using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseCountry : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_COUNTRY; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return origText.Replace(ident, EM_Helpers.ShortCountryToFullCountry(resources.baseSystem.GetSystemName().Length >= 2
                                               ? resources.baseSystem.GetSystemName().Substring(0, 2).ToUpper() : string.Empty));
            }
        }
    }
}
