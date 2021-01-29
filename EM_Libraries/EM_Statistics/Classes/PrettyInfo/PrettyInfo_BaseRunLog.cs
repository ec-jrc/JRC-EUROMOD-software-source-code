using EM_Common;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseRunLog : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_RUNLOG; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.baseSystem.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis) ?
                    origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunLog(fileGenesis)) : origText.Replace(ident, DefPar.Value.NA);
            }
        }
    }
}
