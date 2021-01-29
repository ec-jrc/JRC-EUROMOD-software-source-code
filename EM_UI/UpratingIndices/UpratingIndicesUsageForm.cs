using EM_Common;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.UpratingIndices
{
    internal partial class UpratingIndicesUsageForm : Form
    {
        internal const char separator = '°';

        CountryConfigFacade _countryConfigFacade = null;
        List<string> _indices;
        Dictionary<int, CountryConfig.SystemRow> _stdSystems = new Dictionary<int, CountryConfig.SystemRow>();

        void UpratingIndicesUsageForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal UpratingIndicesUsageForm(List<string> indices, CountryConfigFacade countryConfigFacade)
        {
            InitializeComponent();

            _countryConfigFacade = countryConfigFacade;
            _indices = indices;

            int maxYear = Convert.ToInt32(nudStartYear.Minimum);
            int minYear = Convert.ToInt32(nudEndYear.Maximum);
            foreach (CountryConfig.SystemRow system in _countryConfigFacade.GetSystemRows())
            {
                int systemYear;
                string countryName = _countryConfigFacade.GetCountryShortName();
                if (system.Name.Length != countryName.Length + 5 || !system.Name.ToLower().StartsWith(countryName.ToLower() + "_") ||
                    !int.TryParse(system.Name.Substring(system.Name.Length - 4), out systemYear) ||
                    systemYear < Convert.ToInt32(nudStartYear.Minimum) || systemYear > Convert.ToInt32(nudStartYear.Maximum)) continue;
                _stdSystems.Add(systemYear, system);
                if (systemYear > maxYear) maxYear = systemYear;
                if (systemYear < minYear) minYear = systemYear;
            }

            if (_stdSystems.Count == 0) { minYear = Convert.ToInt32(nudStartYear.Minimum); maxYear = Convert.ToInt32(nudEndYear.Maximum); }
            nudStartYear.Minimum = nudEndYear.Minimum = nudStartYear.Value = minYear;
            nudStartYear.Maximum = nudEndYear.Maximum = nudEndYear.Value = maxYear;

            UpdateContent();
        }

        void UpdateContent()
        {
            dgvIndices.Rows.Clear();

            //gather all uprating functions (funcUprate) of the selected systems
            Dictionary<int, List<CountryConfig.FunctionRow>> funcUprates_byYear = new Dictionary<int, List<CountryConfig.FunctionRow>>();
            foreach (int year in _stdSystems.Keys)
            {
                if (year < nudStartYear.Value || year > nudEndYear.Value) continue;
                List<CountryConfig.FunctionRow> funcUprates = new List<CountryConfig.FunctionRow>();
                funcUprates.AddRange(_countryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(_stdSystems[year].ID, DefFun.Uprate));
                funcUprates_byYear.Add(year, funcUprates);
            }

            //gather the variables, which are uprated (i.e. parameters like yem=$HCPI)
            Dictionary<int, List<CountryConfig.ParameterRow>> uprateParameters_byYear = new Dictionary<int, List<CountryConfig.ParameterRow>>();
            foreach (int year in funcUprates_byYear.Keys)
            {
                List<CountryConfig.ParameterRow> uprateParameters = new List<CountryConfig.ParameterRow>();
                foreach (CountryConfig.FunctionRow funcUprate in funcUprates_byYear[year])
                {
                    if (funcUprate.Switch == DefPar.Value.OFF ||
                        funcUprate.Switch == DefPar.Value.NA) continue;
                    //gather all variables which are uprated by the funcUprate
                    //no need to remove special parameters (e.g. dataset) as they will drop out below (their value will not match any factor-name)
                    foreach (CountryConfig.ParameterRow par in funcUprate.GetParameterRows()) uprateParameters.Add(par);
                }
                uprateParameters_byYear.Add(year, uprateParameters);
            }

            //run over indices and check if they are used in any of the parameters gathered above
            foreach (string index in _indices)
            {
                string[] indexParts = index.Split(separator);
                string description = indexParts[0], reference = indexParts[1].ToLower(), comment = indexParts[2];
                Dictionary<string, List<int>> usage_byYear = new Dictionary<string, List<int>>();

                foreach (int year in uprateParameters_byYear.Keys)
                {
                    foreach (CountryConfig.ParameterRow uprateParameter in uprateParameters_byYear[year])
                    {
                        if (uprateParameter.Value.ToLower() != reference) continue;
                        List<int> years = new List<int>();
                        string parName = uprateParameter.Name.ToLower();
                        if (!usage_byYear.Keys.Contains(parName)) usage_byYear.Add(parName, years);
                        usage_byYear[parName].Add(year);
                    }
                }

                string outUsage = string.Empty, sep = ", ";
                foreach (string usage in usage_byYear.Keys)
                {
                    if (usage_byYear[usage].Count == uprateParameters_byYear.Count) outUsage += usage + sep;
                    else
                    {
                        outUsage += usage + " (";
                        foreach (int year in usage_byYear[usage]) outUsage += year.ToString() + sep;
                        if (outUsage.EndsWith(sep)) outUsage = outUsage.Substring(0, outUsage.Length - sep.Length);
                        outUsage += ")" + sep;
                    }
                }
                if (outUsage.EndsWith(sep)) outUsage = outUsage.Substring(0, outUsage.Length - sep.Length);

                dgvIndices.Rows.Add(description, reference, outUsage, comment);
            }
        }

        void btnExtSearch_Click(object sender, EventArgs e) { (new ComponentUseForm(EM_AppContext.Instance.GetActiveCountryMainForm())).ShowDialog(); }
        void nudStartYear_ValueChanged(object sender, EventArgs e) { UpdateContent(); }
        void nudEndYear_ValueChanged(object sender, EventArgs e) { UpdateContent(); }
    }
}
