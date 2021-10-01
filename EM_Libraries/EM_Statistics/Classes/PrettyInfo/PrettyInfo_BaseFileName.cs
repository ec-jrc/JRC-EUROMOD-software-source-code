namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_BaseFileName : PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_BASE_FILENAME; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                return origText.Replace(ident, resources.baseSystem.GetFileName());
            }
        }
    }
}
