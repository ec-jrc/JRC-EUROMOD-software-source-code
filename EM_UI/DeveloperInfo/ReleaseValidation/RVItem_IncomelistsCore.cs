using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVItem_IncomelistsCore : RVItem_Base
    {
        private class ILInfo
        {
            internal bool LabelEmpty;
            internal List<CountryConfig.ParameterRow> Components = new List<CountryConfig.ParameterRow>();
        }

        private List<string> coreILs = new List<string>() { "ils_earns", "ils_origy", "ils_origrepy", "ils_pen", "ils_sicct",
                                                            "ils_sicer", "ils_sicee", "ils_sicse", "ils_sicot", "ils_sicdy",
                                                            "ils_ben", "ils_benmt", "ils_bennt", "ils_bensim", "ils_tax", "ils_taxsim", "ils_dispy" };
        private List<string> nonCoreILs = new List<string>() { "ils_udb_yem", "ils_udb_yse", "ils_udb_yiy", "ils_udb_ypp", "ils_udb_ypr", "ils_udb_yot",
                                                               "ils_udb_ypt", "ils_udb_xmp", "ils_udb_kfbcc", "ils_udb_tpr", "ils_udb_tis", "ils_udb_boa",
                                                               "ils_udb_bsu", "ils_udb_bdi", "ils_udb_bun", "ils_udb_bhl", "ils_udb_bed", "ils_udb_bsa",
                                                               "ils_udb_bfa", "ils_udb_bho", "ils_udb_yds" };
        private bool core;

        internal RVItem_IncomelistsCore(bool core) { tableName = "Incomelists " + (core ? "(core)" : "(non-core)"); this.core = core; }

        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            string notStartingWithIls = string.Empty, compulsoryMissing = string.Empty, noLabel = string.Empty, noComponents = string.Empty,
                   defDispy = string.Empty, defBen = string.Empty, defSicdy = string.Empty, defYds = string.Empty, diffBenB1 = string.Empty, diffBenB2 = string.Empty;
            foreach (string country in countries)
            {
                Dictionary<string, Dictionary<string, ILInfo>> ilInfo = AssessILInfo(country, core); //=Dic<IL-name, Dic<system-name, ILInfo>>
                RegisterProblems(country, NotStartingWithIls(ilInfo), ref notStartingWithIls);
                RegisterProblems(country, CompulsoryMissing(ilInfo, country, core), ref compulsoryMissing); // use core as parameter as such it could go into the same tab, if this makes more sense (calling function twice)
                RegisterProblems(country, NoLabel(ilInfo, country, core), ref noLabel);
                RegisterProblems(country, NoComponents(ilInfo, core), ref noComponents);
                RegisterProblems(country, DiffBenBx(ilInfo, "ils_b1"), ref diffBenB1);
                RegisterProblems(country, DiffBenBx(ilInfo, "ils_b2"), ref diffBenB2);

                List<string> posComp = new List<string>() { "ils_origy", "ils_ben" };
                List<string> negComp = new List<string>() { "ils_tax", "ils_sicdy" };
                if (core) RegisterProblems(country, FaultyDefinition(ilInfo, "ils_dispy", posComp, negComp), ref defDispy);

                posComp = new List<string>() { "ils_pen", "ils_benmt", "ils_bennt" }; negComp.Clear();
                if (core) RegisterProblems(country, FaultyDefinition(ilInfo, "ils_ben", posComp, negComp), ref defBen);

                posComp = new List<string>() { "ils_sicee", "ils_sicse", "ils_sicot" }; negComp.Clear();
                if (core) RegisterProblems(country, FaultyDefinition(ilInfo, "ils_sicdy", posComp, negComp), ref defSicdy);

                posComp = new List<string>() { "ils_udb_yem", "ils_udb_yse", "ils_udb_yiy", "ils_udb_ypp", "ils_udb_ypr", "ils_udb_yot",
                                               "ils_udb_ypt", "ils_udb_kfbcc", "ils_udb_boa", "ils_udb_bsu", "ils_udb_bdi", "ils_udb_bun",
                                               "ils_udb_bhl", "ils_udb_bed", "ils_udb_bsa", "ils_udb_bfa", "ils_udb_bho" };
                negComp = new List<string>() { "ils_udb_xmp", "ils_udb_tpr", "ils_udb_tis" };
                if (!core) RegisterProblems(country, FaultyDefinition(ilInfo, "ils_udb_yds", posComp, negComp), ref defYds);
            }
            if (core) AddDataGridRow("IL in policy '" + DefPol.SPECIAL_POL_ILSDEF + "' not starting with 'ils_'", notStartingWithIls);
            AddDataGridRow("Compulsory IL missing", compulsoryMissing);
            AddDataGridRow("IL without label", noLabel);
            AddDataGridRow("Empty IL", noComponents);
            if (core) AddDataGridRow("Incorrect definition of 'ils_dispy'", defDispy);
            if (core) AddDataGridRow("Incorrect definition of 'ils_ben'", defBen);
            if (core) AddDataGridRow("Incorrect definition of 'ils_sicdy'", defSicdy);
            if (core) AddDataGridRow("ils_ben different from ils_b1", diffBenB1);
            if (core) AddDataGridRow("ils_ben different from ils_b2", diffBenB2);
            if (!core) AddDataGridRow("Incorrect definition of 'ils_udb_yds'", defYds);
        }

        string NotStartingWithIls(Dictionary<string, Dictionary<string, ILInfo>> ilInfos)
        {
            string faulty = string.Empty;
            foreach (string ilName in ilInfos.Keys) if (!ilName.ToLower().StartsWith("ils_")) faulty += ilName + ", ";
            return faulty;
        }

        string NoComponents(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, bool localCore)
        {
            string faulty = string.Empty;
            foreach (var il in ilInfos)
                if (il.Value.Count > 0 && il.Value.First().Value.Components.Count == 0)
                    if ((localCore && coreILs.Contains(il.Key)) || (!localCore && !coreILs.Contains(il.Key))) // report any empty non-core-IL in the non-core-tab
                    faulty += il.Key + ", ";
            return faulty;
        }

        string CompulsoryMissing(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, string country, bool localCore)
        {
            string faulty = string.Empty;
            foreach (string compIL in localCore ? coreILs : nonCoreILs)
            {
                if (!ilInfos.ContainsKey(compIL)) faulty += compIL + ", ";
                else if (ilInfos[compIL].Count < GetPublicSystems(country).Count)
                { // il exists, but not for all public systems
                    faulty += compIL + " (";
                    foreach (CountryConfig.SystemRow publicSystem in GetPublicSystems(country))
                        if (!ilInfos[compIL].ContainsKey(publicSystem.Name.ToLower())) faulty += publicSystem.Name + ",";
                    faulty = TrimEnd(faulty); faulty += ("), ");
                }
            }
            return faulty;
        }

        string DiffBenBx(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, string ilsBName)
        {
            const string ILS_BEN = "ils_ben";
            string faulty = string.Empty;
            
            if (!ilInfos.ContainsKey(ILS_BEN)) return faulty; // missing ils_ben is handled in check for compulsory ils

            List<string> bxNames = new List<string>(); // gather the names of all ils_b?*
            foreach (string ilName in ilInfos.Keys) if (ilName.StartsWith(ilsBName)) bxNames.Add(ilName);

            // This is a hard-coded request! "ils_ben = ils_b2* + ils_b1_bun" (where ils_b1_bun exists)
            if (ilsBName == "ils_b2") bxNames.Add("ils_b1_bun");
            // This is also a hard-coded request! "ils_ben = ils_b1* - ils_b1_bcb" (where ils_b1_bcb exists)
            if (ilsBName == "ils_b1") bxNames.Remove("ils_b1_bcb");

            string avoidDoubleDiff = string.Empty; 
            foreach (string sysName in ilInfos[ILS_BEN].Keys) // loop over systems (which ought to be the same for ils_ben and ils_b?*)
            {
                Dictionary<string, double> contentBen =   // get the "real" content of ils_ben
                    ResolveIL(ilInfos, ILS_BEN, sysName); // i.e. bch_s + 0.5 bsa_s + ... (that's nonsense, just illustrates the content of the dictionary)

                Dictionary<string, double> contentBx = new Dictionary<string,double>(); // get the "real" content of all ils_b?*
                foreach (string bxName in bxNames)                                      // (by merging ils_b?_bun, ils_b?_bsa, etc.)
                    MergeDics(ref contentBx, ResolveIL(ilInfos, bxName, sysName));      // could be simplyfied if developers decide for a ils_b? (i.e. a summary of all ils_b?*)

                string diff = string.Empty; // compare components of ils_ben and merged ils_b?*
                foreach (var compBen in contentBen) // first check if all components of ils_ben exist in one of the ils_b?*, and if the add-factor is equal
                    if (!contentBx.ContainsKey(compBen.Key) || contentBx[compBen.Key] != compBen.Value) diff += compBen.Key + " ";
                foreach (var compBx in contentBx) // then check if the ils_b?* contain components which are not contained in ils_ben
                    if (!contentBen.ContainsKey(compBx.Key)) diff += compBx.Key + " ";
                if (diff != string.Empty)
                {
                    if (diff == avoidDoubleDiff) faulty += string.Format("{0} (ditto), ", sysName);
                    else faulty += string.Format("{0} ({1}), ", sysName, diff.TrimEnd());
                    avoidDoubleDiff = diff; // that just serves an attempt to avoid repeating the differences for all systems
                }
            }
            return faulty;
        }

        void MergeDics(ref Dictionary<string, double> orDic, Dictionary<string, double> mergeDic)
        {
            foreach (var m in mergeDic)
            {
                if (!orDic.ContainsKey(m.Key)) orDic.Add(m.Key, m.Value);
                else orDic[m.Key] += m.Value;
            }
        }

        Dictionary<string, double> ResolveIL(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, string ilName, string sysName)
        {
            Dictionary<string, double> content = new Dictionary<string, double>();
            if (!(ilInfos.ContainsKey(ilName) && ilInfos[ilName].ContainsKey(sysName))) return content;   // if this il does not exist for this system, skip it (return empty)! 
            foreach (CountryConfig.ParameterRow comp in ilInfos[ilName][sysName].Components)
            {
                string varILName = comp.Name.ToLower();
                if (!ilInfos.ContainsKey(varILName)) // if the component is a variable: add to content
                {
                    string strVal = comp.Value;
                    double factor = strVal.StartsWith("-") ? -1.0 : 1.0;
                    strVal = strVal.Replace("+", "").Replace("-", "");
                    if (strVal != string.Empty) { double f; if (double.TryParse(strVal, out f)) factor *= f; };
                    content.Add(varILName, factor);
                }
                else // if the component is an incomelist: recursive call to add the whole content
                    MergeDics(ref content, ResolveIL(ilInfos, varILName, sysName));
            }
            return content;
        }

        string NoLabel(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, string country, bool localCore)
        {
            string faulty = string.Empty;
            foreach (var info in ilInfos)
            {
                string ilName = info.Key; Dictionary<string /* system */, ILInfo> ilInfo = info.Value;
                if ((localCore && coreILs.Contains(ilName)) || (!localCore && !coreILs.Contains(ilName))) // report any non-core-IL without label in the non-core-tab
                    if (ilInfo.Count > 0 && ilInfo.First().Value.LabelEmpty) faulty += ilName + ", ";
            }
            return faulty;
        }

        string FaultyDefinition(Dictionary<string, Dictionary<string, ILInfo>> ilInfos, string il, List<string> posComp, List<string> negComp)
        {
            if (!ilInfos.ContainsKey(il)) return string.Empty; // il does not exist - is handled by other check

            // check for missing or wrong components
            string faulty1 = string.Empty;
            for (string posNeg = "+"; ; posNeg = "-")
            {
                foreach (string comp in posNeg == "+" ? posComp : negComp)
                {
                    string sFaulty = string.Empty; int nFaulty = 0;
                    foreach (string sys in ilInfos[il].Keys)
                    {
                        bool good = false;
                        foreach (CountryConfig.ParameterRow par in ilInfos[il][sys].Components)
                            if (par.Name.ToLower() == comp && par.Value == posNeg) { good = true; break; }
                        if (good == true) continue;
                        sFaulty += sys + ","; ++nFaulty;
                    }
                    if (nFaulty == 0) continue;
                    if (nFaulty == ilInfos[il].Count) faulty1 += comp + ", ";
                    else faulty1 += string.Format("{0} ({1}), ", comp, TrimEnd(sFaulty));
                }
                if (posNeg == "-") break;
            }
            if (faulty1 != string.Empty) faulty1 = "missing/wrong components: " + faulty1;

            // check for superfluous components
            Dictionary<string, List<string>> superfluousList = new Dictionary<string, List<string>>();
            foreach (string sys in ilInfos[il].Keys)
            {
                foreach (CountryConfig.ParameterRow par in ilInfos[il][sys].Components)
                {
                    string parName = par.Name.ToLower();
                    if (posComp.Contains(parName) || negComp.Contains(parName)) continue;
                    if (!superfluousList.ContainsKey(parName)) superfluousList.Add(parName, new List<string>());
                    superfluousList[parName].Add(sys);
                }
            }
            string faulty2 = string.Empty;
            foreach (var superfluous in superfluousList)
            {
                string superfluousComp = superfluous.Key; // make \r \n visible (unnecessary and potentially dangerous stuff)
                superfluousComp = superfluousComp.Replace("\r", "\\r").Replace("\n", "\\n");
                faulty2 += superfluousComp;
                if (superfluous.Value.Count == ilInfos[il].Count) faulty2 += ", ";
                else
                {
                    string sFaulty = string.Empty; foreach (string sys in superfluous.Value)  sFaulty += sys + ",";
                    faulty2 += " (" + TrimEnd(sFaulty) + "), ";
                }
            }
            if (faulty2 != string.Empty) faulty2 = "superfluous components: " + faulty2;

            return TrimEnd(faulty1) + (faulty1 != string.Empty && faulty2 != string.Empty ? "; " : "") + TrimEnd(faulty2);
        }
        
        private Dictionary<string, Dictionary<string, ILInfo>> AssessILInfo(string country, bool core)
        {
            Dictionary<string, Dictionary<string, ILInfo>> ilInfos = new Dictionary<string, Dictionary<string, ILInfo>>();
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);

            foreach (CountryConfig.SystemRow system in GetPublicSystems(country))
            {
                // UDB (aka non-core) ILS are kept in a separate policy
                CountryConfig.PolicyRow ilPol = ccf.GetPolicyRowByName(system.ID, core ? DefPol.SPECIAL_POL_ILSDEF : DefPol.SPECIAL_POL_ILSUDBDEF);
                if (ilPol == null || ilPol.Switch != DefPar.Value.ON || ilPol.Private == DefPar.Value.YES) continue;
                foreach (CountryConfig.FunctionRow ilFunc in ccf.GetFunctionRowsByPolicyIDAndFunctionName(ilPol.ID, DefFun.DefIl))
                {
                    if (ilFunc.Switch != DefPar.Value.ON || ilFunc.Private == DefPar.Value.YES) continue;
                    ILInfo ilInfo = new ILInfo() { LabelEmpty = ilFunc.Comment == string.Empty };
                    string ilName = string.Empty;
                    
                    foreach (CountryConfig.ParameterRow ilPar in ilFunc.GetParameterRows())
                    {
                        if (ilPar.Name.ToLower() == DefPar.DefIl.Name.ToLower()) { ilName = ilPar.Value.ToLower(); continue; }
                        if (ilPar.Value == DefPar.Value.NA || ilPar.Private == DefPar.Value.YES) continue;
                        ilInfo.Components.Add(ilPar);
                    }
                    if (ilName == string.Empty) continue; // rather unlikely

                    if (!ilInfos.ContainsKey(ilName)) ilInfos.Add(ilName, new Dictionary<string, ILInfo>());
                    if (!ilInfos[ilName].ContainsKey(system.Name.ToLower())) // this is a security-request: ilName should be unique per system
                        ilInfos[ilName].Add(system.Name.ToLower(), ilInfo);
                }
            }
            return ilInfos;
        }
    }
}
