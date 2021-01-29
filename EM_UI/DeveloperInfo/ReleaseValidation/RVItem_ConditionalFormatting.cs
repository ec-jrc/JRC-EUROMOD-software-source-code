using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVItem_ConditionalFormatting : RVItem_Base
    {
        class SystemAndBase
        {
            internal SystemAndBase(CountryConfig.SystemRow _system, CountryConfig.SystemRow _baseSystem) { system = _system; baseSystem = _baseSystem; }
            internal CountryConfig.SystemRow system { get; set; }
            internal CountryConfig.SystemRow baseSystem { get; set; }
        }

        internal RVItem_ConditionalFormatting() { tableName = "Conditional Formatting"; }

        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            string privateBase = string.Empty, notPriorBase = string.Empty, missingBase = string.Empty;
            foreach (string country in countries)
            {
                List<SystemAndBase> systemsAndBases = AssessBases(GetPublicSystems(country), country);
                RegisterProblems(country, PrivateBase(systemsAndBases), ref privateBase);
                RegisterProblems(country, NotPriorBase(systemsAndBases), ref notPriorBase);
                RegisterProblems(country, MissingBase(systemsAndBases), ref missingBase);
            }
            AddDataGridRow("Public systems with private base system", privateBase);
            AddDataGridRow("Public systems with base system year not prior to system year", notPriorBase);
            AddDataGridRow("Public systems without base system", missingBase);
        }

        string AddToFaulty(SystemAndBase sab) { return string.Format("{0}(<<{1}), ", sab.system.Name, sab.baseSystem.Name); }

        string PrivateBase(List<SystemAndBase> systemsAndBases)
        {
            string faulty = string.Empty;
            foreach (SystemAndBase systemAndBase in systemsAndBases)
                if (systemAndBase.baseSystem != null && systemAndBase.baseSystem.Private == DefPar.Value.YES)
                    faulty += AddToFaulty(systemAndBase);
            return faulty;
        }

        string NotPriorBase(List<SystemAndBase> systemsAndBases)
        {
            string faulty = string.Empty;
            foreach (SystemAndBase systemAndBase in systemsAndBases)
            {
                if (systemAndBase.baseSystem == null) continue;
                int systemYear = RVItem_SystemConfiguration.GetSystemYear(systemAndBase.system);
                int baseYear = RVItem_SystemConfiguration.GetSystemYear(systemAndBase.baseSystem);
                if (systemYear == -1 || baseYear == -1) continue;
                if (baseYear >= systemYear) faulty += AddToFaulty(systemAndBase);
            }
            return faulty;
        }

        string MissingBase(List<SystemAndBase> systemsAndBases)
        {
            string faulty = string.Empty;
            List<int> sortedSystemYears = RVItem_SystemConfiguration.GetSortedSystemYears((from sab in systemsAndBases select sab.system).ToList());
            foreach (SystemAndBase systemAndBase in systemsAndBases)
            {
                if (systemAndBase.baseSystem != null && systemAndBase.baseSystem.Name != string.Empty) continue; // has a base
                if (sortedSystemYears.Count > 0 && RVItem_SystemConfiguration.GetSystemYear(systemAndBase.system) == sortedSystemYears[0]) continue; // very 1st system
                faulty += systemAndBase.system.Name + ", ";
            }
            return faulty;
        }

        List<SystemAndBase> AssessBases(List<CountryConfig.SystemRow> systems, string country)
        {
            List<SystemAndBase> systemsAndBases = new List<SystemAndBase>();
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
            {
                foreach (CountryConfig.SystemRow system in systems)
                {
                    CountryConfig.SystemRow baseSystem = null;
                    foreach (CountryConfig.ConditionalFormatRow conditionalFormatRow in ccf.GetConditionalFormatRowsOfSystem(system))
                        if (conditionalFormatRow.BaseSystemName != null && conditionalFormatRow.BaseSystemName != string.Empty)
                        { baseSystem = ccf.GetSystemRowByName(conditionalFormatRow.BaseSystemName); break; }
                    systemsAndBases.Add(new SystemAndBase(system, baseSystem));
                }
            }
            return systemsAndBases;
        }
    }
}
