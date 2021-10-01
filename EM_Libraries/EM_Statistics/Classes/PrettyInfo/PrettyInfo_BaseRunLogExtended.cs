using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseRunLogExtended : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_RUNLOG_EXTENDED; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.baseSystem.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis) ?
                    origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunLogExtended(fileGenesis)) : origText.Replace(ident, DefPar.Value.NA);
            }
        }
    }
}
