using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseTaxSysPretty : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_TAX_SYS_PRETTY; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return origText.Replace(ident, EM_Helpers.OutputNameToTaxPretty(resources.baseSystem.GetFileName()));
            }
        }
    }
}
