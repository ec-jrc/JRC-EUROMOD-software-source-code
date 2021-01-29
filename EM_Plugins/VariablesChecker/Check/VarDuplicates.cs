using System.Collections.Generic;

namespace VariablesChecker
{
    internal partial class Check
    {
        internal static string VarDuplicates(List<string> print)
        {
            List<string> read = new List<string>();
            Dictionary<string, int> dupl = new Dictionary<string, int>();

            foreach (var v in VariablesChecker.varData.Variable)
            {
                string name = v.Name.ToLower().Trim();
                if (read.Contains(name))
                {
                    if (!dupl.ContainsKey(name)) dupl.Add(name, 1);
                    dupl[name]++;
                }
                else read.Add(name);
            }

            if (dupl.Count == 0) return "No duplicate variables found";
            
            foreach (var d in dupl) print.Add(d.Key + (d.Value == 2 ? string.Empty : " (" + d.Value.ToString() + ")"));
            return dupl.Count.ToString() + " duplicate variables found";
        }
    }
}
