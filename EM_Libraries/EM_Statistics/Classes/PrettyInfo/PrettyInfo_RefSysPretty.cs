using EM_Common;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefSysPretty : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_SYS_PRETTY; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return GetRefSys(out SystemInfo refSys, resources) ? origText.Replace(ident, EM_Helpers.OutputNameToPretty(refSys.GetSystemName())) : origText;
            }
        }
    }
}
