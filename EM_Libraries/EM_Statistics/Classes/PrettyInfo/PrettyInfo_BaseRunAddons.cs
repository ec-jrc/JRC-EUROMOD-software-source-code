using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseRunAddons : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_RUN_ADDONS; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.baseSystem.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis) ?
                    origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunAddons(fileGenesis)) : origText.Replace(ident, DefPar.Value.NA);
            }
        }
    }
}

