using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVItem_DatabaseConfiguration : RVItem_Base
    {
        enum BestMatchCheckType { Year_NotEqual_Collection, Income_Greater_Year, UseCommonDefault, NoHICP };

        internal class SystemAndData
        {
            internal SystemAndData(CountryConfig.SystemRow _system) { system = _system; allData = new List<DataConfig.DBSystemConfigRow>(); bestMatches = new List<DataConfig.DBSystemConfigRow>(); }
            internal CountryConfig.SystemRow system { get; set; }
            internal List<DataConfig.DBSystemConfigRow> allData { get; set; }
            internal List<DataConfig.DBSystemConfigRow> bestMatches { get; set; } // note: this is a subgroup of allData
            // note: only "publicly used" system-data-combinations are read into this structure (see function AssessData)
            // i.e. public systems and the (public) data that can be used by them
        }

        internal RVItem_DatabaseConfiguration() { tableName = "Database Configuration"; }

        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            string noBestMatch = string.Empty, multiBestMatches = string.Empty, hypoBestMatch = string.Empty, notPriorBestMatch = string.Empty;
            string yearNotEqualCollection = string.Empty, incomeGreaterYear = string.Empty, useCommonDefault = string.Empty, noHICP = string.Empty;
            foreach (string country in countries)
            {
                List<SystemAndData> systemsAndData = AssessData(GetPublicSystems(country), country);
                RegisterProblems(country, NoBestMatch(systemsAndData), ref noBestMatch);
                RegisterProblems(country, MultiBestMatches(systemsAndData), ref multiBestMatches);
                RegisterProblems(country, HypoBestMatch(systemsAndData), ref hypoBestMatch);
                RegisterProblems(country, NotPriorBestMatch(systemsAndData), ref notPriorBestMatch);
                RegisterProblems(country, BestMatchCheck(systemsAndData, BestMatchCheckType.Year_NotEqual_Collection), ref yearNotEqualCollection);
                RegisterProblems(country, BestMatchCheck(systemsAndData, BestMatchCheckType.Income_Greater_Year), ref incomeGreaterYear);
                RegisterProblems(country, BestMatchCheck(systemsAndData, BestMatchCheckType.UseCommonDefault), ref useCommonDefault);
                if (EM_AppContext.Instance.GetHICPConfigFacade(false) != null) // check $HICP only here for the new approach ($HICP stored in global table)
                    RegisterProblems(country, BestMatchCheck(systemsAndData, BestMatchCheckType.NoHICP, country), ref noHICP); // old approach (stored in uprating-factors-policy) is checked with uprating-factors-checks
            }
            AddDataGridRow("Public systems without best-matching public data", noBestMatch);
            AddDataGridRow("Public systems with non-unique best-matching public data", multiBestMatches);
            AddDataGridRow("Public systems with training- or hypo-data as best-match", hypoBestMatch);
            AddDataGridRow("Public systems with income year of best-matching data not prior or equal to system year", notPriorBestMatch);
            AddDataGridRow("Best-matching data with year <> collection year", yearNotEqualCollection);
            AddDataGridRow("Best-matching data with income year > collection year", incomeGreaterYear);
            AddDataGridRow("Best-matching data with 'Use Common Default' ticked", useCommonDefault);
            if (EM_AppContext.Instance.GetHICPConfigFacade(false) != null) AddDataGridRow("Best-matching data with $HICP not defined", noHICP);
        }

        string NoBestMatch(List<SystemAndData> systemsAndData)
        {
            string faulty = string.Empty;
            foreach (SystemAndData systemAndData in systemsAndData)
                if (systemAndData.bestMatches.Count == 0) faulty += systemAndData.system.Name + ", ";
            return faulty;
        }

        string MultiBestMatches(List<SystemAndData> systemsAndData)
        {
            string faulty = string.Empty;
            foreach (SystemAndData systemAndData in systemsAndData)
            {
                if (systemAndData.bestMatches.Count <= 1) continue;
                string bm = string.Empty;
                foreach (DataConfig.DBSystemConfigRow dbs in systemAndData.bestMatches) bm += dbs.DataBaseRow.Name + " ";
                faulty += string.Format("{0} ({1}), ", systemAndData.system.Name, TrimEnd(bm));
            }
            return faulty;
        }

        string HypoBestMatch(List<SystemAndData> systemsAndData)
        {
            string faulty = string.Empty;
            foreach (SystemAndData systemAndData in systemsAndData)
            {
                string bm = string.Empty;
                foreach (DataConfig.DBSystemConfigRow dbs in systemAndData.bestMatches) 
                    if (dbs.DataBaseRow.Name.ToLower().StartsWith("hypo") || dbs.DataBaseRow.Name.ToLower().StartsWith("training"))
                        bm += dbs.DataBaseRow.Name + " ";
                if (bm != string.Empty) faulty += string.Format("{0} ({1}), ", systemAndData.system.Name, TrimEnd(bm));
            }
            return faulty;
        }

        string NotPriorBestMatch(List<SystemAndData> systemsAndData)
        {
            string faulty = string.Empty;
            foreach (SystemAndData systemAndData in systemsAndData)
            {
                if (systemAndData.bestMatches.Count != 1) continue; // only consider unique best-matches for simplicity
                int systemYear = RVItem_SystemConfiguration.GetSystemYear(systemAndData.system);
                int dataYear = EM_Helpers.SaveConvertToInt(systemAndData.bestMatches[0].DataBaseRow.YearInc);
                if (systemYear <= 0 || dataYear <= 0 || systemYear >= dataYear) continue;
                faulty += string.Format("{0} ({1}<{2}), ", systemAndData.system.Name, systemYear, dataYear);
            }
            return faulty;
        }

        string BestMatchCheck(List<SystemAndData> systemsAndData, BestMatchCheckType check, string country = "")
        {
            string faulty = string.Empty; List<string> done = new List<string>();
            foreach (SystemAndData systemAndData in systemsAndData)
            {
                if (systemAndData.bestMatches.Count != 1) continue; // only consider unique best-matches for simplicity
                DataConfig.DataBaseRow data = systemAndData.bestMatches[0].DataBaseRow;
                if (done.Contains(data.Name.ToLower())) continue; // data can be best-match of several systems (just need to check once)
                switch (check)
                {
                    case BestMatchCheckType.Year_NotEqual_Collection:
                        string cy = data.YearCollection.Trim() == string.Empty ? "-" : data.YearCollection.Trim();
                        if (!data.Name.Contains(cy)) faulty += string.Format("{0} ({1}), ", data.Name, cy);
                        break;
                    case BestMatchCheckType.Income_Greater_Year:
                        int incYear, colYear;
                        if (int.TryParse(data.YearInc, out incYear) && int.TryParse(data.YearCollection, out colYear) && incYear > colYear)
                            faulty += string.Format("{0} ({1}>{2}), ", data.Name, incYear, colYear);
                        break;
                    case BestMatchCheckType.UseCommonDefault:
                        if (data.UseCommonDefault == DefPar.Value.YES) faulty += data.Name + ", ";
                        break;
                    case BestMatchCheckType.NoHICP:
                        HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false); if (hcf == null) return string.Empty; // should not happen
                        int year;
                        if (!int.TryParse(data.YearInc, out year) || hcf.GetHICP(country, year) == null) faulty += data.Name + ", ";
                        break;
                }
                done.Add(data.Name.ToLower());
            }
            return faulty;
        }

        internal static List<SystemAndData> AssessData(List<CountryConfig.SystemRow> systems, string country)
        {
            List<SystemAndData> systemsAndData = new List<SystemAndData>();
            DataConfigFacade dcf = CountryAdministrator.GetDataConfigFacade(country);
            foreach (CountryConfig.SystemRow sys in systems)
            {
                SystemAndData sad = new SystemAndData(sys);
                foreach (DataConfig.DBSystemConfigRow dbs in dcf.GetDBSystemConfigRowsBySystem(sys.Name))
                {
                    if (dbs.DataBaseRow.Private == DefPar.Value.YES) continue; // do not take-up private data into this structure !!!
                    sad.allData.Add(dbs);
                    if (dbs.BestMatch == DefPar.Value.YES) sad.bestMatches.Add(dbs);
                }
                systemsAndData.Add(sad);
            }
            return systemsAndData;
        }
    }
}
