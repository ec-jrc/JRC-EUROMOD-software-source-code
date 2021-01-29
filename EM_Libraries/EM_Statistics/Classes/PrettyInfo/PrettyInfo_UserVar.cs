namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfo_UserVar: PrettyInfo
        {
            internal override string ReplaceText(string origText, PrettyInfoResources resources)
            {
                string newText = origText;
                while (newText.IndexOf("[@") > -1)
                {
                    int start = newText.IndexOf("[@");
                    int end = newText.IndexOf("]", start) + 1;
                    string userVar = newText.Substring(start, end - start);
                    int oneBasedRefNo = resources.refNo < 0 ? -1 : resources.refNo + 1; // GetRealVariableName requires 1-based reference-number
                    newText = newText.Substring(0, start) + 
                              EM_TemplateCalculator.GetRealVariableName(userVar, resources.templateInfo, true, resources.packageKey, oneBasedRefNo) +
                              newText.Substring(end);
                }
                return newText;
            }
        }
    }
}
