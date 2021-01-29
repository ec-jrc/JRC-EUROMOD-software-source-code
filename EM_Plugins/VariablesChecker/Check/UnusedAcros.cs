using System.Collections.Generic;
using System.Linq;

namespace VariablesChecker
{
    internal partial class Check
    {
        internal static string UnusedAcros(List<string> print)
        {
            List<string> varNames = (from v in VariablesChecker.varData.Variable select v.Name.ToLower().Trim()).ToList();
            foreach (VarConfig.AcronymRow acro in VariablesChecker.varData.Acronym)
            {
                string acroType = acro.AcronymLevelRow.AcronymTypeRow.ShortName.ToLower();
                string acroName = acro.Name.ToLower().Trim();
                bool used = false;
                foreach (string varName in varNames)
                {
                    if (varName == string.Empty || !acroType.Contains(varName[0])) continue;
                    if (GetVarAcros(varName).Contains(acroName)) { used = true; break; }
                }
                if (!used) print.Add(string.Format("{0}\t{1}\tin {2} - {3}",
                                     acro.Name, acro.Description, acro.AcronymLevelRow.AcronymTypeRow.LongName.ToUpper(), acro.AcronymLevelRow.Name));
            }
            return print.Count == 0 ? "No usused acronyms found" : print.Count + " usused acronyms found";
        }
    }
}
