using EM_Common;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseRunExtensionSwitches : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_RUN_EXTENSION_SWITCHES; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return resources.baseSystem.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis) ?
                    origText.Replace(ident, PrettyInfoFileGenesis.GetPrettyRunExtensionSwitches(fileGenesis)) : origText.Replace(ident, DefPar.Value.NA);
            }
        }
    }
}


