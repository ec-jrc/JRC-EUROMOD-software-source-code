using EM_Common;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{

    class RVItem_SystemConfiguration : RVItem_Base
    {
        internal RVItem_SystemConfiguration() { tableName = "System Configuration"; }

        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            string faultySystemNames = string.Empty, missingYear = string.Empty, missingSystems = string.Empty, multipleSystems = string.Empty, noHICPSystems = string.Empty;
            foreach (string country in countries)
            {
                List<CountryConfig.SystemRow> publicSystems = GetPublicSystems(country);
                RegisterProblems(country, GetSystemsNotFollowingNamingRules(publicSystems, country), ref faultySystemNames);
                RegisterProblems(country, GetMissingYear(publicSystems), ref missingYear);
                RegisterProblems(country, GetMissingSystems(publicSystems), ref missingSystems);
                RegisterProblems(country, GetMultipleSystems(publicSystems), ref multipleSystems);
                if (EM_AppContext.Instance.GetHICPConfigFacade(false) != null) // check $HICP only here for the new approach ($HICP stored in global table)
                    RegisterProblems(country, GetNoHICPSystems(publicSystems, country), ref noHICPSystems); // old approach (stored in uprating-factors-policy) is checked with uprating-factors-checks
            }
            AddDataGridRow("Public systems not following naming rules (cc_yyyy)", faultySystemNames);
            AddDataGridRow("Year not defined for public system", missingYear);
            AddDataGridRow("Not unique public system years", multipleSystems);
            AddDataGridRow("Missing public system years", missingSystems);
            if (EM_AppContext.Instance.GetHICPConfigFacade(false) != null) AddDataGridRow("Factor $HICP does not exist", noHICPSystems); // see above
        }

        string GetSystemsNotFollowingNamingRules(List<CountryConfig.SystemRow> publicSystems, string countryName)
        {
            string faulty = string.Empty;
            foreach (CountryConfig.SystemRow system in publicSystems)
            {
                int systemYear = GetSystemYear(system);
                string systemName = system.Name.ToLower();
                if (!systemName.StartsWith(countryName.ToLower()) ||
                    systemName.Length != (countryName + "_yyyy").Length ||
                    systemName[countryName.Length] != '_' ||
                    systemYear < 2000 || systemYear > 2999)
                    faulty += systemName + ", ";
            }
            return faulty;
        }

        string GetMissingYear(List<CountryConfig.SystemRow> publicSystems)
        {
            string faulty = string.Empty;
            foreach (CountryConfig.SystemRow system in publicSystems)
                if (system.Year == null || system.Year == string.Empty) faulty += system.Name + ", ";
            return faulty;
        }
        string GetMultipleSystems(List<CountryConfig.SystemRow> publicSystems)
        {
            string faulty = string.Empty; int ancestor = -1;
            foreach (int systemYear in GetSortedSystemYears(publicSystems))
            {
                string toAdd = systemYear.ToString() + ", ";
                if (ancestor == systemYear && !faulty.Contains(toAdd)) faulty += toAdd;
                ancestor = systemYear;
            }
            return faulty;
        }

        string GetMissingSystems(List<CountryConfig.SystemRow> publicSystems)
        {
            string faulty = string.Empty; int ancestor = -1;
            foreach (int systemYear in GetSortedSystemYears(publicSystems))
            {
                if (ancestor == -1){ ancestor = systemYear; continue; }
                if (ancestor == systemYear) continue; // multiple system years are covered by GetMultipleSystems
                if (ancestor + 1 != systemYear)
                {
                    if (ancestor + 2 == systemYear) faulty += (ancestor + 1).ToString() + ", ";
                    else faulty += string.Format("{0}-{1}, ", ancestor + 1, systemYear - 1);
                }
                ancestor = systemYear;
            }
            return faulty;
        }

        string GetNoHICPSystems(List<CountryConfig.SystemRow> publicSystems, string country)
        {
            string faulty = string.Empty;
            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false); if (hcf == null) return faulty; // should not happen
            foreach (CountryConfig.SystemRow system in publicSystems)
                if (hcf.GetHICP(country, GetSystemYear(system)) == null) faulty += system.Name + ", ";                   
            return faulty;
        }

        static internal int GetSystemYear(CountryConfig.SystemRow system)
        {
            int year;
            return system.Year == null || system.Year == string.Empty || !int.TryParse(system.Year, out year) ? GetSystemYear(system.Name) : year;
        }
        static int GetSystemYear(string systemName) // assume that the 1st occurrence of 4 successive digits starting with 2 is the system year
        {
            List<string> digitSequences = new List<string>();
            string dSeq = string.Empty;
            for (int i = 0; i < systemName.Length; ++i)
            {
                if (EM_Helpers.IsDigit(systemName[i])) dSeq += systemName[i];
                else
                {
                    if (dSeq == string.Empty) continue;
                    digitSequences.Add(dSeq); dSeq = string.Empty;
                }
            }
            if (dSeq != string.Empty) digitSequences.Add(dSeq);

            foreach (string dS in digitSequences) if (dS.Length == 4 && dS[0] == '2') return Convert.ToInt32(dS);
            return -1;
        }

        static internal List<int> GetSortedSystemYears(List<CountryConfig.SystemRow> publicSystems)
        {
            List<int> systemYears = new List<int>();
            foreach (CountryConfig.SystemRow system in publicSystems)
            {
                int year = GetSystemYear(system);
                if (year == -1) continue; // is covered by GetSystemsNotFollowingNamingRules
                systemYears.Add(year);
            }
            systemYears.Sort();
            return systemYears;
        }
    }
}
