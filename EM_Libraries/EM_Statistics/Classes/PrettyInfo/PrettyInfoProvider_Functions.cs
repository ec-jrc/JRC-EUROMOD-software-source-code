using System;
using System.Collections.Generic;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        internal static string GetPrettyText(Template.TemplateInfo templateInfo, string origText,
                                             List<SystemInfo> baseSystems, List<SystemInfo> reformSystems = null,
                                             string packageKey = null, int refNo = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(origText)) return string.Empty;
                
                PrettyInfoResources resources = new PrettyInfoResources(templateInfo, baseSystems, reformSystems, packageKey, refNo);

                string newText = origText;
                foreach (PrettyInfo pi in allPrettyInfos) if (newText.Contains(pi.getId())) newText = pi.ReplaceText(newText, resources);
                return newText;
            }
            catch { return origText; }
        }

        internal static string GetPrettyHtml(Template.TemplateInfo templateInfo, string origText,
                                             SystemInfo baseSystem, List<SystemInfo> reformSystems = null,
                                             string packageKey = null, int refNo = -1)
        {
            throw new NotImplementedException();

            // is the HTML generated here and origText is really unformatted text or is it actually origHtml?

            //if (string.IsNullOrEmpty(origText)) return string.Empty;
            //PrettyInfoResources resources = new PrettyInfoResources(templateInfo, baseSystem, reformSystems, packageKey, refNo);
            //foreach (PrettyInfo pi in allPrettyInfos) origText = pi.ReplaceHtml(origText, resources);
            //return origText;
        }
    }
}
