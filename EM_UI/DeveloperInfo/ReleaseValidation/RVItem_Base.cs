using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    abstract class RVItem_Base // base class for validation items (i.e. TabPages)
    {
        internal string tableName = string.Empty; // heading form tabPage
        internal TabPage tabPage = null; // is added in RVForm.AddTables, no need to care about it
        internal DataGridView dataGrid = null; // is also added and formatted in RVForm.AddTables, just needs to be filled in PerformValidation
        internal string problemCountries = string.Empty; // display text for overview table
        private bool showProblemsOnly;

        internal virtual void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            if (dataGrid != null) dataGrid.Rows.Clear();
            problemCountries = string.Empty;
            this.showProblemsOnly = showProblemsOnly;
        }

        // to be called from PerformValidation for a standardised problem-treatment:
        // two error-strings are produced: (A) the text to be displayed in the specific problem-row
        //                                 (B) the text to be displayed in the overview table
        // (A) is returned in 'allCountriesProblems' (B) is written to 'problemCountries'
        // Example for (A) tab 'System Configuration', row 'Missing public system years': "FR: 2011 2014   LV: 2010-2012"
        // Example for (B) "FR LV"
        // the first (boolean) version adds the country's name to allCountriesProblems, i.e. it's enough to say a country has that problem (e.g. does not define a compulsory policy)
        // the second (string) version adds the country's problem to allCountriesProblems, i.e. that problem needs more specification (see example above)
        protected void RegisterProblems(string country, bool countryHasProblems, ref string allCountriesProblems)
        {
            if (!countryHasProblems) return;
            allCountriesProblems += country + ", ";
            if (!problemCountries.Contains(country + ",")) problemCountries += country + ", ";
        }
        protected void RegisterProblems(string country, string oneCountrysProblems, ref string allCountriesProblems)
        {
            if (oneCountrysProblems == string.Empty) return;
            allCountriesProblems += string.Format("{0}: {1}{2}", country, TrimEnd(oneCountrysProblems), Environment.NewLine);
            if (!problemCountries.Contains(country + ",")) problemCountries += country + ", ";
        }

        protected void AddDataGridRow(string validationItem, string problems)
        {
            bool noProblems = problems == string.Empty;
            if (!showProblemsOnly || !noProblems)
                dataGrid.Rows.Add(validationItem,
                                  GetResultImage(noProblems),
                                  noProblems ? "no problems" : TrimEnd(problems));
        }

        internal static string TrimEnd(string toTrim) { return toTrim.TrimEnd(new char[] { ' ', ',', ';', '.', ':', '/', '\\', '-', '|', '\n', '\r', '\t' }); }

        static internal Image GetResultImage(bool ok) { return ok ? Properties.Resources.merge_accept : Properties.Resources.merge_reject; }

        static internal List<CountryConfig.SystemRow> GetPublicSystems(string country)
        {
            return (from sr in CountryAdministrator.GetCountryConfigFacade(country).GetSystemRows()
                    where sr.Private != DefPar.Value.YES select sr).ToList();
        }
    }
}
