using System.Collections.Generic;

namespace VariablesChecker
{
    internal partial class Check
    {
        internal static string AcroTwins(List<string> print)
        {
            Dictionary<string, List<string>> sortedNames = new Dictionary<string, List<string>>();

            foreach (var v in VariablesChecker.varData.Variable)
            {
                string sortedName = string.Empty, name = v.Name.ToLower().Trim();
                if (name == string.Empty) continue; // unlikely
                sortedName += name[0];
                foreach (string acro in GetVarAcros(name, true)) sortedName += acro;
                if (sortedName.Length != name.Length) continue; // should be handled in the checks for invalid names
                
                if (!sortedNames.ContainsKey(sortedName)) sortedNames.Add(sortedName, new List<string>());
                sortedNames[sortedName].Add(v.Name);
            }

            foreach (var s in sortedNames)
            {
                if (s.Value.Count <= 1) continue;
                string twin = string.Empty;
                foreach (string v in s.Value) twin += v + " ";
                print.Add(twin.TrimEnd());
            }
            return (print.Count == 0) ? "No acro-twins found" : print.Count.ToString() + " acro-twins found";
        }
    }
}
