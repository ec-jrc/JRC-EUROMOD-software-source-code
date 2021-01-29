using EM_Common;
using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace VariablesChecker
{
    internal partial class Check
    {
        private static object varListLock = new object();

        private class CountryParValue
        {
            internal string formula = string.Empty; // value of parameter or (editable) policy-column
            internal bool isNa = false;             // true if parent policy- and/or function is set to n/a
        }

        private class VarUsePattern
        {
            internal string useCountries = string.Empty;
            internal string naCountries = string.Empty;
            internal string noLabelCountries = string.Empty;
            internal string autoLabel = string.Empty;
        }

        internal static string VarUsage(List<string> print, BackgroundWorker bkWorker)
        {
            Dictionary<string, VarUsePattern> varUsages = new Dictionary<string, VarUsePattern>();
            foreach (VarConfig.VariableRow v in VariablesChecker.varData.Variable) varUsages.Add(v.Name, new VarUsePattern() { autoLabel = v.AutoLabel });

            string ccFolderPath = Path.Combine(UISessionInfo.GetEuromodFilesFolder(), @"XMLParam\Countries");
            DirectoryInfo[] ccFolders = new DirectoryInfo(ccFolderPath).GetDirectories(); double done = 0; bool canceled = false;

            Parallel.ForEach(ccFolders, ccFolder =>
            {
                if (bkWorker.CancellationPending) { canceled = true; return; }

                List<CountryParValue> parValues; string shortName;
                if (!ReadCountry(Path.Combine(ccFolderPath, ccFolder.Name, ccFolder.Name + ".xml"), out parValues, out shortName)) return;

                List<string> usedVar = new List<string>(), naVar = new List<string>(), noLabelVar = new List<string>();
                foreach (VarConfig.VariableRow v in VariablesChecker.varData.Variable)
                {
                    if (bkWorker.CancellationPending) { canceled = true; return; }
                    bool? used = false;
                    foreach (CountryParValue parValue in parValues)
                    {
                        if (UsedInFormula(v.Name.Trim().ToLower(), parValue.formula))
                        {
                            if (parValue.isNa) used = null; // means used as n/a - note: no break, i.e. can still be overwritten by true
                            else { used = true; break; }
                        }
                    }
                    if (used == false) continue;
                    if (used == true) usedVar.Add(v.Name); else naVar.Add(v.Name);
                    var ccDesc = (from l in v.GetCountryLabelRows() where l.Country.ToLower().Trim() == shortName.ToLower().Trim() select l);
                    if (ccDesc.Count() == 0 || ccDesc.First().Label.Trim() == string.Empty || ccDesc.First().Label.Trim() == "-") noLabelVar.Add(v.Name);
                }

                lock(varListLock)
                {
                    foreach (string v in usedVar) varUsages[v].useCountries += shortName + " ";
                    foreach (string v in naVar) varUsages[v].naCountries += shortName + " ";
                    foreach (string v in noLabelVar) varUsages[v].noLabelCountries += shortName + " ";
                    bkWorker.ReportProgress((int)Math.Round(++done / ccFolders.Count() * 100));
                }
            });

            if (canceled) return "Checking variable usage canceled";

            int unused = 0;
            print.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", "Variable", "Used in", "Used in n/a only", "No description for", "Auto-description"));
            foreach (var vu in varUsages)
            {
                if (vu.Value.useCountries == string.Empty && vu.Value.naCountries == string.Empty) { vu.Value.useCountries = "not used"; ++unused; }
                print.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                    vu.Key, vu.Value.useCountries.TrimEnd(), vu.Value.naCountries.TrimEnd(), vu.Value.noLabelCountries.TrimEnd(), vu.Value.autoLabel));
            }

            return (unused == 0) ? "No unused variables found" : unused + " unused variables found";
        }

        private static bool ReadCountry(string path, out List<CountryParValue> parValues, out string shortName)
        {
            parValues = new List<CountryParValue>(); shortName = string.Empty;
            if (!File.Exists(path)) return false;

            List<string> keepUnique = new List<string>();
            using (StreamReader sr = new StreamReader(path, DefGeneral.DEFAULT_ENCODING))
            using (XmlReader mainReader = XmlReader.Create(sr))
            {
                // *** READ COUNTRY ***
                mainReader.ReadToDescendant("Country");
                XmlReader countryReader = mainReader.ReadSubtree(); countryReader.ReadToDescendant("ID");
                do
                {
                    if (countryReader.NodeType != XmlNodeType.Element) continue;
                    if (countryReader.Name == "ShortName") shortName = countryReader.ReadInnerXml().ToUpper();
                    if (countryReader.Name != "System") continue;

                    // *** READ SYSTEM ***
                    XmlReader sysReader = countryReader.ReadSubtree(); sysReader.ReadToDescendant("ID");
                    do
                    {
                        if (sysReader.NodeType != XmlNodeType.Element && sysReader.Name != "Policy") continue;

                        // *** READ POLICY ***
                        bool isPolNA = false;
                        XmlReader polReader = sysReader.ReadSubtree(); polReader.ReadToDescendant("ID");
                        do
                        {
                            if (polReader.NodeType != XmlNodeType.Element) continue;
                            if (polReader.Name == "Switch" && polReader.ReadInnerXml() == "n/a") isPolNA = true;
                            if (polReader.Name != "Function") continue;

                            // *** READ FUNCTION ***
                            bool isFunNA = false; string funName = string.Empty;
                            XmlReader funReader = polReader.ReadSubtree(); funReader.ReadToDescendant("ID");
                            do
                            {
                                if (funReader.NodeType != XmlNodeType.Element) continue;
                                if (funReader.Name == "Name") funName = funReader.ReadInnerXml().ToLower().Trim();
                                if (funReader.Name == "Switch" && funReader.ReadInnerXml() == "n/a") isFunNA = true;
                                if (funReader.Name != "Parameter") continue;

                                // *** READ PARAMETER *** (see descriptions for policy above)
                                string parName = string.Empty, parValue = string.Empty;
                                XmlReader parReader = funReader.ReadSubtree(); parReader.ReadToDescendant("ID");
                                do
                                {
                                    if (parReader.NodeType != XmlNodeType.Element) continue;
                                    if (parReader.Name == "Name") parName = parReader.ReadInnerXml();
                                    if (parReader.Name == "Value") parValue = RemoveCData(parReader.ReadInnerXml());
                                } while (parReader.Read());

                                // *** STORE PARAMETER ***
                                // try a bit of optimisation by not storing formulas for each system where equal, etc.
                                string pVal = parValue.Trim().ToLower();
                                if (funName == "uprate") pVal = parName.Trim().ToLower(); // for Uprate value in policy-column (i.e. Name) is relevant
                                bool isNA = isPolNA || isFunNA;
                                string kuVal = pVal + (isNA ? "[ISNA]" : string.Empty); // make sure that formula is not classified as n/a if it appears in several systems (n/a and not n/a)
                                if (funName != "defil" && pVal.Length >= 3 && !keepUnique.Contains(kuVal)) // DefIL is no candidate (pol-col: IL-name, sys-col: +/-)
                                {                                                                          // varialbes have at least three characters (type + 1 acro)
                                    keepUnique.Add(kuVal);
                                    parValues.Add(new CountryParValue() { formula = pVal, isNa = isNA });
                                }
                                if (funName == "setdefault") // for SetDefault value in policy-column (i.e. Name) as well as value in system-columns is relevant
                                {
                                    pVal = parName.Trim().ToLower(); kuVal = pVal + (isNA ? "[ISNA]" : string.Empty);
                                    if (pVal.Length >= 3 && !keepUnique.Contains(kuVal))
                                    {
                                        keepUnique.Add(kuVal);
                                        parValues.Add(new CountryParValue() { formula = pVal, isNa = isNA });
                                    }
                                }

                            } while (funReader.Read()); funReader.Close();
                        } while (polReader.Read()); polReader.Close();
                    } while (sysReader.Read()); sysReader.Close();
                } while (countryReader.Read()); countryReader.Close();
            }
            return true;
        }

        private static string RemoveCData(string s)
        {
            string pre = "<![CDATA[", post = "]]>";
            return s.StartsWith(pre) && s.EndsWith(post) ? s.Substring(pre.Length, s.Length - pre.Length - post.Length) : s;
        }

        internal static bool UsedInFormula(string variable, string formula)
        {
            if (!formula.Contains(variable)) return false;
            if (formula == variable) return true;

            string delim = " +-/*(){}^%\\<>!";
            for (int index = 0; ; index += variable.Length)
            {
                index = formula.IndexOf(variable, index);
                if (index == -1) break;

                int left = index - 1, right = index + variable.Length;
                if ((left < 0 || delim.Contains(formula[left])) &&
                    (right >= formula.Length || delim.Contains(formula[right]))) return true;
            }

            return false;
        }
    }
}
