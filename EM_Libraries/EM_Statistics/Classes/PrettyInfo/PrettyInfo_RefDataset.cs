using EM_Common;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefDataset : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_DATASET; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return GetRefSys(out SystemInfo refSys, resources)
                               ? refSys.GetFileGenesis(out SystemInfo.FileGenesis fileGenesis) 
                                                      ? origText.Replace(ident, fileGenesis.runInfo.databaseName)
                                                      : origText.Replace(ident, DefPar.Value.NA)
                               : origText;
            }
        }
    }
}
