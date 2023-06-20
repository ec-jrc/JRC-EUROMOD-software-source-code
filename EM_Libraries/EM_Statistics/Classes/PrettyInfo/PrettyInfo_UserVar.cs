namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfo_UserVar: PrettyInfo
        {
            protected override string ident { get => PRETTY_INFO_USER_VAR; }

            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                string newText = origText;
                while (newText.IndexOf("[@") > -1)
                {
                    int start = newText.IndexOf("[@");
                    int end = newText.IndexOf("]", start) + 1;
                    string userVar = newText.Substring(start, end - start);
                    newText = newText.Substring(0, start) + 
                              EM_TemplateCalculator.GetRealVariableName(userVar, resources.templateInfo, true, resources.packageKey, resources.refNo) +
                              newText.Substring(end);
                }
                return newText;
            }
        }
    }
}
