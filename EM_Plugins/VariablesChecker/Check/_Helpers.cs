using System.Collections.Generic;

namespace VariablesChecker
{
    internal partial class Check
    {
        private static List<string> GetVarAcros(string varName, bool sort = false)
        {
            List<string> acros = new List<string>();
            for (int i = 1; i < varName.Length - 1; i += 2)
                acros.Add(varName.Substring(i, 2));
            if (sort) acros.Sort();
            return acros;
        }
    }
}
