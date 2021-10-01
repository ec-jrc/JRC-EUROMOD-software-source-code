using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public partial class TemplateApi
    {
        public const string MARKER_CURRENT_CONTENT = "°CurrentContent°";
        public const string MARKER_REPLACE = "°Replace:";
        public const string MARKER_BY = "°°By:";
        public const string MARKER_END = "°";

        /// <summary>
        /// allows for advanced replacement of strings in template (titles, descriptions, formulaStrings, ...) by api-provided content
        /// example: string in template: poa + bsa + OTHER_BEN - tin - OTHER_TAX
        ///          string in api: °CurrentContent° °Replace:OTHER_BEN°°By:bch + bed° °Replace:OTHER_TAX°°By:tscee - tscer°
        ///          result: poa + bsa + bch + bed - tin - tscee - tscer
        /// rules:
        /// - any occurence of °CurrentContent° is replaced by templateString (before any other changes)
        /// - there can be several °Replace:x°°By:y°-structures (spaces between are allowed, but no other text)
        /// - °Replace:x°°By:y°-structures must be indicated at the end of apiString (any text after leads to an error-message)
        /// </summary>
        /// <param name="templateString">string as defined in template (or by a prior api-change)</param>
        /// <param name="apiString">string as defined by ApiMerge-function</param>
        /// <param name="keep">
        /// true: prioritises templateString: i.e. templateString is replaced by apiString only if templateString is null or empty
        /// false: prioritises apiString: i.e. templateString is replaced by apiString, unless apiString is null or empty
        /// </param>
        /// <param name="errorCollector"></param>
        internal static void Merge(ref string templateString, string apiString, bool keep, ErrorCollector errorCollector)
        {
            if (!string.IsNullOrEmpty(apiString) && (string.IsNullOrEmpty(templateString) || !keep))
            {
                apiString = apiString.Replace(MARKER_CURRENT_CONTENT, templateString ?? string.Empty);

                int iRepBy = apiString.IndexOf(MARKER_REPLACE);
                if (iRepBy >= 0)
                {
                    string apiPattern = apiString.Substring(iRepBy); apiString = apiString.Substring(0, iRepBy);

                    List<Tuple<string, string>> replaceBy = new List<Tuple<string, string>>();
                    while (apiPattern.StartsWith(MARKER_REPLACE))
                    {
                        CutFront(ref apiPattern, MARKER_REPLACE);
                        int iBy = apiPattern.IndexOf(MARKER_BY); if (iBy < 0) { AddError(); break; }
                        string replace = apiPattern.Substring(0, iBy);
                        CutFront(ref apiPattern, replace + MARKER_BY);
                        int iEnd = apiPattern.IndexOf(MARKER_END); if (iEnd < 0) { AddError(); break; }
                        string by = apiPattern.Substring(0, iEnd);
                        CutFront(ref apiPattern, by + MARKER_END);
                        replaceBy.Add(new Tuple<string, string>(replace, by));
                    }
                    if (apiPattern.Trim() != string.Empty) AddError();

                    foreach (Tuple<string, string> rb in replaceBy) apiString = apiString.Replace(rb.Item1, rb.Item2);

                    void CutFront(ref string s, string cut) { s = s.Length == cut.Length ? string.Empty : s.Substring(cut.Length).TrimStart(); }
                    void AddError() { errorCollector.AddError($"TemplateApi Merge: invalid {MARKER_REPLACE}x{MARKER_BY}y{MARKER_END} structure found: {apiPattern}"); }
                }

                templateString = apiString;
            }
        }

        internal static void Merge<T>(ref T templateItem, T apiItem, bool keep, ErrorCollector errorCollector)
        {
            if (apiItem != null && (templateItem == null || !keep)) templateItem = apiItem;
        }

        internal static void Merge<T>(ref T templateItem, T apiItem, T defaultValue, bool keep, ErrorCollector errorCollector) // for not null-able types
        {
            if (!apiItem.Equals(defaultValue) && (templateItem.Equals(defaultValue) || !keep)) templateItem = apiItem;
        }

        internal static void MergeParameters(ref List<Template.Parameter> templateParameters, List<Template.Parameter> apiParameters, bool keep, ErrorCollector errorCollector)
        {
            // note: mergedParameters will, irrespectively from keep, include all parameters which exist in either api or template, as unnecessary parameters do no harm
            List<Template.Parameter> mergedParameters = new List<Template.Parameter>(), doubleParameters = new List<Template.Parameter>();
            foreach (Template.Parameter tPar in templateParameters) // add parameters which exist in template version only, or in both versions
            {
                Template.Parameter aPar = (from p in apiParameters // note: includes unnamed parameters (as for example exist for CreateEquivalized)
                                           where (p.name ?? string.Empty).ToLower() == (tPar.name ?? string.Empty).ToLower()
                                           select p).FirstOrDefault();
                if (aPar == null) mergedParameters.Add(tPar);
                else { doubleParameters.Add(aPar); mergedParameters.Add(keep ? tPar : aPar); }
            }
            foreach (Template.Parameter aPar in apiParameters) // add parameters which exist in api-version only
            {
                if (!doubleParameters.Contains(aPar)) mergedParameters.Add(aPar);
            }
            templateParameters = mergedParameters;
        }
    }
}
