namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo
        {
            protected virtual string ident { get; }

            internal virtual string ReplaceText(string origText, PrettyInfoResources resources) { return origText; }
            internal virtual string ReplaceHtml(string origHtml, PrettyInfoResources resources) { return origHtml; }

            protected bool GetRefSys(out SystemInfo refSys, PrettyInfoResources resources)
            {
                refSys = resources.reformSystems != null &&
                         resources.refNo >= 0 && resources.refNo < resources.reformSystems.Count
                       ? resources.reformSystems[resources.refNo]
                       : null;
                return refSys != null;
            }
        }
    }
}
