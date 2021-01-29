using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace EM_UI.GlobalAdministration
{
    internal class GlobalAdministrator
    {
        internal static void ShowHICPDialog()
        {
            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false); // false: do not create if not existent yet (only if user demands below)
            List<HICPConfig.HICPRow> hicps = null;
            if (hcf != null) hicps = hcf.GetHICPs();
            if ((hicps == null || hicps.Count == 0) && GenerateHICPTable(out hicps) == DialogResult.Cancel) return;

            HICPForm hicpForm = new HICPForm(hicps);
            if (hicpForm.ShowDialog() == DialogResult.Cancel) return;
            
            hcf = EM_AppContext.Instance.GetHICPConfigFacade(true); // true: if user actually decided to generate HICPs (and thus GlobalConfig) from scratch (unlikely case)
            if (hcf != null && hcf.RefreshHICPs(hicpForm.updatedHICPs)) hcf.WriteXML();
        }

        private static DialogResult GenerateHICPTable(out List<HICPConfig.HICPRow> hicps)
        {
            DialogResult choice = UserInfoHandler.GetInfo("The HICP Table does not yet exist. Do you want to automatically generate it by extracting HICPs from country tables?",
                                                          MessageBoxButtons.YesNoCancel);
            hicps = choice == DialogResult.Cancel ? null : new List<HICPConfig.HICPRow>();
            if (choice != DialogResult.Yes) return choice; // if user's choice was No (or even on error in assessing HICPs below) show the empty HICP table (i.e. hicps != null)

            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(true); if (hcf == null) return DialogResult.Cancel; // bad error - shouldn't happen
            try
            {
                ProgressIndicator progressIndicator = new ProgressIndicator(GetCountryHICPs_BackgroundEventHandler, "Assessing Country Info ...");
                if (progressIndicator.ShowDialog() != System.Windows.Forms.DialogResult.OK) return choice; // user cancelled the generation - show empty HICP table

                List<Tuple<string, int, double, string>> hicpInfo = progressIndicator.Result as List<Tuple<string, int, double, string>>;
                if (hicpInfo == null) // an exception was thrown while loading countries
                {
                    UserInfoHandler.ShowError(string.Format("Generating HICP Table failed with the following error:{0}{1}", Environment.NewLine, progressIndicator.Result.ToString()));
                    return choice; // show empty HICP table
                }
                if (hcf.RefreshHICPs(hicpInfo) && hcf.WriteXML()) hicps = hcf.GetHICPs();
                return choice;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return choice; }
        }

        static void GetCountryHICPs_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            try
            {
                List<Tuple<string, int, double, string>> hicps = new List<Tuple<string,int,double,string>>();
                List<Country> countries = CountryAdministrator.GetCountries();
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Result = null; e.Cancel = true; return; }

                    CountryConfigFacade ccf = countries[i].GetCountryConfigFacade(); if (ccf == null) continue;
                    CountryConfig.UpratingIndexRow hicp = ccf.GetUpratingIndex("$HICP"); if (hicp == null) continue;
                    // put parameter values into structure that can be stored in HICP table
                    List<Tuple<string, int, double, string>> hicpsCC = new List<Tuple<string, int, double, string>>();
                    foreach (var yv in ccf.GetUpratingIndexYearValues(hicp))
                        hicpsCC.Add(new Tuple<string, int, double, string>(countries[i]._shortName, yv.Key, yv.Value, hicp.Comment));
                    hicps.AddRange(hicpsCC);

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }
                e.Result = hicps;
            }
            catch (Exception exception) { e.Result = exception.Message; backgroundWorker.ReportProgress(100); }
        }

        internal static void ShowExchangeRatesDialog()
        {
            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(true); // true: create if not existent yet
            List<ExchangeRatesConfig.ExchangeRatesRow> exRates = null;
            if (excf != null) exRates = excf.GetExchangeRates();

            ExchangeRatesForm exRatesForm = new ExchangeRatesForm(exRates);
            if (exRatesForm.ShowDialog() == DialogResult.Cancel) return;

            if (excf != null && excf.RefreshExchangeRates(exRatesForm.updatedExRates)) excf.WriteXML();
        }

        private static List<ExchangeRate> RemoveEuroCountries(List<ExchangeRate> ratesCountry)
        {
            List<ExchangeRate> adaptedList = new List<ExchangeRate>();
            foreach (ExchangeRate rateCountry in ratesCountry) if (rateCountry.DefaultRate() != 1) adaptedList.Add(rateCountry);
            return adaptedList;
        }

        static void GetCountryExchangeRates_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            try
            {
                List<ExchangeRate> exRates = new List<ExchangeRate>();
                List<Country> countries = CountryAdministrator.GetCountries();
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Result = null; e.Cancel = true; return; }

                    CountryConfigFacade ccf = countries[i].GetCountryConfigFacade(); if (ccf == null) continue;
                    Dictionary<double, string> sysRates = new Dictionary<double, string>();
                    foreach (CountryConfig.SystemRow system in ccf.GetSystemRows())
                    {
                        if (system.IsExchangeRateEuroNull()) continue; // rather unlikely
                        double rate; if (!double.TryParse(system.ExchangeRateEuro, out rate)) continue; // also rather unlikely
                        if (sysRates.ContainsKey(rate)) sysRates[rate] = ExchangeRate.AddToValidFor(sysRates[rate], system.Name);
                        else sysRates.Add(rate, system.Name.ToLower());
                    }
                    foreach (var sr in sysRates)
                    {
                        ExchangeRate exRatesCY = new ExchangeRate() { Country = countries[i]._shortName, June30 = sr.Key, ValidFor = sr.Value };
                        exRates.Add(exRatesCY);
                    }
                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }
                e.Result = exRates;
            }
            catch (Exception exception) { e.Result = exception.Message; backgroundWorker.ReportProgress(100); }
        }
    }
}
