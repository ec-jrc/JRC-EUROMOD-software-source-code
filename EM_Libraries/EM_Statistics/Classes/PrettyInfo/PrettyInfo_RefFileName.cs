namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefFileName : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_FILENAME; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return GetRefSys(out SystemInfo refSys, resources) ? origText.Replace(ident, refSys.GetFileName()) : origText;
            }
        }
    }
}
