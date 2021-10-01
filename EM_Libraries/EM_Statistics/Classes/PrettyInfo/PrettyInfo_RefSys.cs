namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_RefSys : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_REF_SYS; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return GetRefSys(out SystemInfo refSys, resources) ? origText.Replace(ident, refSys.GetSystemName()) : origText;
            }
        }
    }
}
