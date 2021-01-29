using System.Collections.Generic;

namespace VariablesChecker
{
    internal partial class Check
    {
        internal static string AcroDuplicates(List<string> print)
        {
            int counter = 0;
            foreach (var aType in VariablesChecker.varData.AcronymType)
            {
                Dictionary<string, List<string>> adups = new Dictionary<string, List<string>>();

                foreach (var aLevel in aType.GetAcronymLevelRows())
                    foreach (var acro in aLevel.GetAcronymRows())
                    {
                        if (!adups.ContainsKey(acro.Name.ToLower())) adups.Add(acro.Name.ToLower(), new List<string>());
                        adups[acro.Name.ToLower()].Add(aLevel.Name);
                    }

                foreach (var adup in adups)
                {
                    if (adup.Value.Count <= 1) continue;
                    ++counter;
                    string dup = $"{aType.LongName.ToUpper()} {adup.Key.ToUpper()}: in ";
                    foreach (string aLevel in adup.Value) dup += $"'{aLevel}' and ";
                    if (dup.EndsWith(" and ")) dup = dup.Substring(0, dup.Length - " and ".Length);
                    print.Add(dup.TrimEnd());
                }
            }

            return (counter == 0) ? "No duplicate acros found" : counter.ToString() + " duplicate acros found";
        }
    }
}
