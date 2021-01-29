using EM_Common;
using EM_UI.DataSets;
using EM_UI.GlobalAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ConfigureSystemsForm : Form
    {
        internal const string DEFAULT_EXCHANGE_RATE = "1";

        void ConfigureSystemsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!CheckSystemYear()) return;
            bool haveChanges = false;

            foreach (DataGridViewRow dataGridViewRow in dgvSystems.Rows)
            {
                CountryConfig.SystemRow systemRow = dataGridViewRow.Tag as CountryConfig.SystemRow;
                if (systemRow.CurrencyParam != dataGridViewRow.Cells[colCurrencyParameters.Name].Value.ToString())
                {
                    systemRow.CurrencyParam = dataGridViewRow.Cells[colCurrencyParameters.Name].Value.ToString();
                    haveChanges = true;
                }
                if (systemRow.CurrencyOutput != dataGridViewRow.Cells[colCurrencyOutput.Name].Value.ToString())
                {
                    systemRow.CurrencyOutput = dataGridViewRow.Cells[colCurrencyOutput.Name].Value.ToString();
                    haveChanges = true;
                }
                if (EM_Helpers.SaveConvertToBoolean(dataGridViewRow.Cells[colPrivate.Name].Value) == true)
                {
                    if (systemRow.Private != DefPar.Value.YES)
                    {
                        systemRow.Private = DefPar.Value.YES;
                        haveChanges = true;
                    }
                }
                else
                {
                    if (systemRow.Private != DefPar.Value.NO)
                    {
                        systemRow.Private = DefPar.Value.NO;
                        haveChanges = true;
                    }
                }
                if (systemRow.HeadDefInc != dataGridViewRow.Cells[colHeadDefInc.Name].Value.ToString())
                {
                    systemRow.HeadDefInc = dataGridViewRow.Cells[colHeadDefInc.Name].Value.ToString();
                    haveChanges = true;
                }
                if (systemRow.Year != dataGridViewRow.Cells[colYear.Name].Value.ToString())
                {
                    systemRow.Year = dataGridViewRow.Cells[colYear.Name].Value.ToString();
                    haveChanges = true;
                }
                string comment = dataGridViewRow.Cells[colComment.Name].Value == null ? string.Empty : dataGridViewRow.Cells[colComment.Name].Value.ToString();
                if (systemRow.Comment != comment)
                {
                    systemRow.Comment = comment;
                    haveChanges = true;
                }
            }
            DialogResult = haveChanges == true ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal ConfigureSystemsForm(string countryShortName, CountryConfig.SystemDataTable systemTable)
        {
            InitializeComponent();

            lblCountry.Text = countryShortName;
            colCurrencyOutput.Items.Add(DefPar.Value.EURO);
            colCurrencyOutput.Items.Add(DefPar.Value.NATIONAL);
            colCurrencyParameters.Items.Add(DefPar.Value.EURO);
            colCurrencyParameters.Items.Add(DefPar.Value.NATIONAL);

            Dictionary<string, string> globalRates = SyncGlobalExRatesIn(countryShortName, systemTable);

            foreach (CountryConfig.SystemRow systemRow in systemTable.Rows)
            {
                //add available incomelists to combo-box for HeadDefInc, i.e. the combo contains all incomelists not just the one for the system, as there is only one definition of combo-content for all systems
                List<CountryConfig.ParameterRow> ilNameParameterRows = null;
                CountryConfigFacade.GetDefFunctionInformation(systemRow, ref ilNameParameterRows, DefFun.DefIl, DefPar.DefIl.Name);
                foreach (CountryConfig.ParameterRow ilNameParameterRow in ilNameParameterRows)
                    if (!colHeadDefInc.Items.Contains(ilNameParameterRow.Value)) //avoid multiple input of incomelist
                        colHeadDefInc.Items.Add(ilNameParameterRow.Value);
                if (!colHeadDefInc.Items.Contains(systemRow.HeadDefInc))
                    colHeadDefInc.Items.Add(systemRow.HeadDefInc); //if currently set HeadDefInc is not a valid incomelist still add it, otherwise the grid shows error messages

                string currencyOutput = (systemRow.CurrencyOutput.ToLower() == DefPar.Value.NATIONAL.ToLower()) ? DefPar.Value.NATIONAL : DefPar.Value.EURO;
                string currencyParam = (systemRow.CurrencyParam.ToLower() == DefPar.Value.NATIONAL.ToLower()) ? DefPar.Value.NATIONAL : DefPar.Value.EURO;
                bool isPrivate = (systemRow.Private.ToLower() == DefPar.Value.YES) ? true : false;
                string exRate = globalRates.ContainsKey(systemRow.Name.ToLower()) ? globalRates[systemRow.Name.ToLower()] : DEFAULT_EXCHANGE_RATE;
                int index = dgvSystems.Rows.Add(systemRow.Name, exRate, systemRow.Year, currencyParam, currencyOutput, isPrivate, systemRow.HeadDefInc, systemRow.Comment);
                dgvSystems.Rows[index].Tag = systemRow;
            }
        }

        Dictionary<string, string> SyncGlobalExRatesIn(string countryShortName, CountryConfig.SystemDataTable systemTable)
        {
            Dictionary<string, string> diffRates = new Dictionary<string, string>();
            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
            List<ExchangeRatesConfig.ExchangeRatesRow> globalExRates = (excf != null) ? excf.GetExchangeRates(countryShortName) : new List<ExchangeRatesConfig.ExchangeRatesRow>();
//            List<ExchangeRatesConfig.ExchangeRatesRow> globalExRates = new List<ExchangeRatesConfig.ExchangeRatesRow>();
            if (globalExRates.Count == 0) return diffRates;

            foreach (CountryConfig.SystemRow systemRow in systemTable.Rows)
            {
                string globalExRate = DEFAULT_EXCHANGE_RATE;  // default value for missing exchange rates
                foreach (ExchangeRatesConfig.ExchangeRatesRow ger in globalExRates)
                {
                    List<string> vF = ExchangeRate.ValidForToList(ger.ValidFor);
                    if (vF.Contains(systemRow.Name.ToLower())) { globalExRate = ExchangeRate.DefaultRate(ger).ToString(); break; }
                }
                diffRates.Add(systemRow.Name.ToLower(), globalExRate);
            }

            return diffRates;
        }

        bool CheckSystemYear()
        {
            foreach (DataGridViewRow dataGridViewRow in dgvSystems.Rows)
            {
                if (dataGridViewRow.Cells[colYear.Name].Value == null) dataGridViewRow.Cells[colYear.Name].Value = string.Empty;
                if (dataGridViewRow.Cells[colYear.Name].Value.ToString() == string.Empty) continue;
                int year;
                if (!int.TryParse(dataGridViewRow.Cells[colYear.Name].Value.ToString(), out year) || year < 1900 || year > 2100)
                    { UserInfoHandler.ShowError("Please indicate a valid year for system " + dataGridViewRow.Cells[colSystem.Name].Value.ToString() + "."); return false; }
            }
            return true;
        }
    }
}
