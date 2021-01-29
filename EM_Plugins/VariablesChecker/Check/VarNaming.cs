using EM_Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VariablesChecker
{
    internal partial class Check
    {
        private static object printLock = new object();

        internal static string VarNaming(List<string> print)
        {
            Parallel.ForEach(VariablesChecker.varData.Variable, v =>
            {
                string vName = v.Name.ToLower().Trim();
                if (vName == string.Empty) { lock(printLock) print.Add("variable without a name (ID " + v.ID + ")"); return; } // only for formal correctness

                if (vName.StartsWith("id")) return;

                string vType = vName.Substring(0, 1);
                var aT = from t in VariablesChecker.varData.AcronymType
                         where t.ShortName.ToLower().Contains(vType) // Contains instead of = because we have b/p
                         select t;
                if (aT == null || aT.Count() == 0) { lock (printLock) print.Add(string.Format("{0}\tunknown type {1}\t{2}", v.Name, v.Name[0], v.ItemArray[3])); return; }

                VarConfig.AcronymTypeRow acroType = aT.First();

                List<string> mainAcros = new List<string>();
                var aL = from l in VariablesChecker.varData.AcronymLevel where l.Name.ToLower() == "main" & l.AcronymTypeRow.ShortName.ToLower() == vType select l;
                if (aL != null && aL.Count() > 0) mainAcros = (from a in aL.First().GetAcronymRows() select a.Name.ToLower()).ToList();

                if (vName.EndsWith(DefGeneral.POSTFIX_SIMULATED)) vName = vName.Substring(0, vName.Length - 2);
                int maxLevel = -1; bool good = true; List<string> vAcros = GetVarAcros(vName);
                foreach (string vAcro in vAcros)
                {
                    var ac = from a in VariablesChecker.varData.Acronym
                             where a.Name.ToLower() == vAcro & a.AcronymLevelRow.TypeID == acroType.ID
                             select a;
                    if (ac == null || ac.Count() == 0) { lock (printLock) print.Add(string.Format("{0}\tunknown acronym {1}\t{2}", v.Name, vAcro, v.ItemArray[3])); good = false; break; }
                    int level = ac.First().AcronymLevelRow.Index;
                    if (level < maxLevel) { lock (printLock) print.Add(string.Format("{0}\tinvalid order of acronyms\t{1}", v.Name, v.ItemArray[3])); good = false; break; }
                    maxLevel = level;

                    if (mainAcros.Contains(vAcro) && vAcros.IndexOf(vAcro) != 0)
                    {
                        lock (printLock) print.Add(string.Format("{0}\tacronyms of the Main-level must immediately follow after the type ({1}), this is violated by {2}\t{3}",
                                                                 v.Name, v.Name[0], vAcro, v.ItemArray[3]));
                        good = false; break;
                    }
                }
                if (!good) return;

                if (vName.Length != vAcros.Count * 2 + 1) lock (printLock) print.Add(string.Format("{0}\tinvalid 1-character acronym {1}\t{2}", v.Name, v.Name.Last(), v.ItemArray[3]));
            });

            return (print.Count == 0) ? "No variable naming rule violations found" : print.Count.ToString() + " variable naming rule violations found";
        }
    }
}
