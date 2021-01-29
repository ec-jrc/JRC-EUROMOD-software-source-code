using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EM_UI.EDEM
{
    internal class MigrateUpratingIndices
    {
        internal static void Migrate()
        {
            ProgressIndicator progressIndicator = new ProgressIndicator(Migrate_BackgroundEventHandler, "Migrating Uprating Indices ...");
            progressIndicator.ShowDialog();
        }

        private static void Migrate_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            try
            {
                List<Country> countries = CountryAdministrator.GetCountries();
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Result = null; e.Cancel = true; return; }

                    CountryConfigFacade ccf = countries[i].GetCountryConfigFacade(); if (ccf == null) continue;
                    countries[i].WriteXML();

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }
            }
            catch (Exception exception) { e.Result = exception.Message; backgroundWorker.ReportProgress(100); }
        }
    }
}
