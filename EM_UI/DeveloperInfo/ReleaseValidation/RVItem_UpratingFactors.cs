using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    partial class RVItem_UpratingFactors : RVItem_Base
    {
        internal RVItem_UpratingFactors() { tableName = "Uprating Factors"; }

        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            string noHICP = string.Empty, notDollarF = string.Empty, emptyFactors = string.Empty, zeroFactors = string.Empty,
                   noYearForSystem = string.Empty, noYearForData = string.Empty, noIncomeYear = string.Empty;
            foreach (string country in countries)
            {
                CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
                DataConfigFacade dcf = CountryAdministrator.GetDataConfigFacade(country);

                List<CountryConfig.UpratingIndexRow> indices = (from ui in ccf.GetUpratingIndices() select ui).ToList();
                List<CountryConfig.SystemRow> publicSystems = GetPublicSystems(country);
                List<DataConfig.DataBaseRow> datasets = (from d in dcf.GetDataBaseRows() where d.Private != DefPar.Value.YES select d).ToList();

                List<string> indexYears = ccf.GetAllUpratingIndexYears();

                RegisterProblems(country, NotDollarF(indices), ref notDollarF);
                RegisterProblems(country, NoYearForSystem(publicSystems, indexYears), ref noYearForSystem);
                RegisterProblems(country, NoYearForData(datasets, indexYears), ref noYearForData);
                RegisterProblems(country, NoIncomeYear(datasets), ref noIncomeYear);
                RegisterProblems(country, EmptyFactors(ccf, indices, false), ref emptyFactors);
                RegisterProblems(country, EmptyFactors(ccf, indices, true), ref zeroFactors);
            }
            AddDataGridRow("Factors not starting with $f_", notDollarF);
            AddDataGridRow("Systems for which factor-table does not provide year", noYearForSystem);
            AddDataGridRow("Databases for which factor-table does not provide year", noYearForData);
            AddDataGridRow("Databases with undefined income year", noIncomeYear);
            AddDataGridRow("Factors with empty values", emptyFactors);
            AddDataGridRow("Factors with zero values (warning)", zeroFactors);
        }

        string NotDollarF(List<CountryConfig.UpratingIndexRow> indices)
        {
            string faulty = string.Empty;
            foreach (CountryConfig.UpratingIndexRow index in indices)
                if (!index.Reference.ToLower().StartsWith("$f_") && index.Reference.ToLower() != "$hicp") faulty += index.Reference + ", ";
            return faulty;
        }

        string NoYearForSystem(List<CountryConfig.SystemRow> systems, List<string> indexYears)
        {
            string faulty = string.Empty;
            foreach (CountryConfig.SystemRow system in systems)
                if (!indexYears.Contains(RVItem_SystemConfiguration.GetSystemYear(system).ToString())) faulty += system.Name + ", ";
            return faulty;
        }

        string NoYearForData(List<DataConfig.DataBaseRow> datasets, List<string> indexYears)
        {
            string faulty = string.Empty;
            foreach (DataConfig.DataBaseRow dataset in datasets)
            {
                if (dataset.YearInc == null || dataset.YearInc == string.Empty || dataset.YearInc.Length < 4) continue;
                if (!indexYears.Contains(dataset.YearInc)) faulty += string.Format("{0} ({1}), ", dataset.Name, dataset.YearInc);
            }
            return faulty;
        }

        string NoIncomeYear(List<DataConfig.DataBaseRow> datasets)
        {
            string faulty = string.Empty;
            foreach (DataConfig.DataBaseRow dataset in datasets)
            {
                if (dataset.Name.ToLower().Contains("hhot") || dataset.Name.ToLower().Contains("training")) continue;
                if (dataset.YearInc == null || dataset.YearInc == string.Empty ||
                    !EM_Helpers.IsNonNegInteger(dataset.YearInc) || int.Parse(dataset.YearInc) < 1900 || int.Parse(dataset.YearInc) > 2100)
                    faulty += dataset.Name + ", ";
            }
            return faulty;
        }

        string EmptyFactors(CountryConfigFacade ccf, List<CountryConfig.UpratingIndexRow> indices, bool checkZero)
        {
            string faulty = string.Empty;
            foreach (CountryConfig.UpratingIndexRow index in indices)
            {
                string years = string.Empty;
                foreach (var yv in ccf.GetUpratingIndexYearValues(index, true))
                    if ((checkZero && yv.Value == 0) || (!checkZero && yv.Value == -1)) years += yv.Key.ToString() + ", ";
                if (years != string.Empty) faulty += string.Format("{0} ({1}), ", index.Reference, years.TrimEnd(new char[] { ',', ' ' }));
            }
            return faulty;
        }
    }
}
