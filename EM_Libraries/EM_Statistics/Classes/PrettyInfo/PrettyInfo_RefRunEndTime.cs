using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefRunEndTime : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_RUN_END_TIME; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return GetRefSys(out SystemInfo refSys, resources)
                                ? refSys.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis)
                                                       ? origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunEndTime(fileGenesis))
                                                       : origText.Replace(ident, DefPar.Value.NA)
                                : origText;
            }
        }
    }
}