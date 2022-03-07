using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefTaxSysPretty : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REFORM_TAX_SYS_PRETTY; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.reformSystems.Count > 0 ? origText.Replace(ident, EM_Helpers.OutputNameToTaxPretty(resources.reformSystems[0].GetFileName())): origText;
            }
        }
    }
}
