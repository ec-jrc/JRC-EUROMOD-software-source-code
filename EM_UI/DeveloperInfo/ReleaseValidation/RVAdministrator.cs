using EM_UI.CountryAdministration;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVAdministrator
    {
        static RVForm _rvForm = null;
        static RVForm rvForm { get { if (_rvForm == null) _rvForm = new RVForm(); EM_AppContext.Instance.AddTopMostWindow(_rvForm); return _rvForm; } }

        static internal void ShowDialog()
        {
            RVConfigurationForm rvConfigurationForm = new RVConfigurationForm(rvForm.GetValidationItemNames());
            if (rvConfigurationForm.ShowDialog() == DialogResult.Cancel) return;

            // loading Country- and DataConfigFacades requires reading all not yet read country-XML-files, as this is time-consuming use progress-bar
            List<string> loadedCountries = null;
            if (rvConfigurationForm.selectedFunction != RVConfigurationForm.COMPARE_VERSIONS)
                { loadedCountries = LoadCountryConfigFacades(rvConfigurationForm); if (loadedCountries == null) return; }

            // note that it is important to close the RVConfigurationForm before starting the non-modal RVForm, otherwise it disappears with closing the configuration form (see RVConfigurationForm.btnValidate_Click)
            if (rvConfigurationForm.selectedFunction == RVConfigurationForm.PERFORM_VALIDATION)
                rvForm.PerformValidation(rvConfigurationForm.validationItems, loadedCountries, rvConfigurationForm.showProblemsOnly);
            if (rvConfigurationForm.selectedFunction == RVConfigurationForm.CREATE_INFO_FILE)
                (new RVExcelInfo()).GenerateInfo(rvConfigurationForm.infoFilePath, loadedCountries);
            if (rvConfigurationForm.selectedFunction == RVConfigurationForm.COMPARE_VERSIONS)
                (new RVCompareVersionsForm(rvConfigurationForm.compareCountryFolder)).ShowDialog();
        }

        static List<string> LoadCountryConfigFacades(RVConfigurationForm rvConfigurationForm)
        {
            // if validation is to be performed: load the selected countries only; if info-file is to be generated: load all countries
            List<string> countries = rvConfigurationForm.selectedFunction == RVConfigurationForm.CREATE_INFO_FILE
                ? (from c in CountryAdministrator.GetCountries() select c._shortName).ToList() : rvConfigurationForm.countries;

            //the handler passed to the progress indicator will do the work (see below)
            ProgressIndicator progressIndicator = new ProgressIndicator(GetCountryInfo_BackgroundEventHandler, "Assessing Country Info ...", countries);
            if (progressIndicator.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null; // user pressed Cancel

            List<string> result = progressIndicator.Result as List<string>;
            if (result == null) // an exception was thrown while loading countries
            {
                UserInfoHandler.ShowError(string.Format("Validation failed with the following error:{0}{1}", Environment.NewLine, progressIndicator.Result.ToString()));
                return null;
            }
            return result; // overtake the perhaps now shorter list of countries (due to non-ability to load a country)
        }

        static void GetCountryInfo_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Result = null; e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            try // this is just to read the country- and data-XML-files, not to do anything with them or store the ConfigFacades
            {   // the CountryAdministrator will keept them in memory and thus quick-access by the RVTable-derived classes is possible
                List<string> countries = e.Argument as List<string>;
                List<int> faultyCountries = new List<int>();
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (CountryAdministrator.GetCountryConfigFacade(countries[i]) == null ||
                        CountryAdministrator.GetDataConfigFacade(countries[i]) == null) faultyCountries.Add(i);
                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }
                for (int f = faultyCountries.Count - 1; f >= 0; --f) countries.RemoveAt(f);
                e.Result = countries;
            }
            catch (Exception exception) { e.Result = exception.Message; backgroundWorker.ReportProgress(100); }
        }
    }
}
