using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        // used in GetPolInfo and GetFunInfo to assess the <Order>
        private static double GetOrder(Dictionary<string, string> props, out string error)
        {
            double order = -1.0; error = null;
            if (!props.ContainsKey(TAGS.ORDER)) error = GenErr("not found");
            else if (!double.TryParse(EM_Helpers.AdaptDecimalSign(props[TAGS.ORDER]), out order)) error = GenErr($"invalid: {props[TAGS.ORDER]}");
            return order;

            string GenErr(string what) { return $"{props.GetOrEmpty(TAGS.POL_ID)}{props.GetOrEmpty(TAGS.FUN_ID)}: <{TAGS.ORDER}> {what}"; }
        }

        // used to find system or dataset by name or id
        private static string GetIdByIdOrName(string identifier, Dictionary<string, Dictionary<string, string>> allSysOrData, bool searchSys)
        {
            if (EM_Helpers.IsGuid(identifier)) // search (data or system) by id
            {
                if (allSysOrData.ContainsKey(identifier)) return identifier;
            }
            else // search (data or system) by name
            {
                string name = identifier.ToLower(); if (name.EndsWith(".txt")) name = name.Substring(0, name.Length - 4);
                foreach (var s in allSysOrData)
                {
                    if (!s.Value.ContainsKey(TAGS.NAME)) continue;
                    string posMatch = s.Value[TAGS.NAME].ToLower(); if (posMatch.EndsWith(".txt")) posMatch = posMatch.Substring(0, name.Length - 4);
                    if (posMatch == name) return s.Key;
                }
                if (!searchSys) return null; // do not throw, because this can be a command-line call with a "not-registered" data
            }
            throw new Exception($"no {(searchSys ? "system" : "dataset")} with identifier {identifier} found");
        }
    }
}
