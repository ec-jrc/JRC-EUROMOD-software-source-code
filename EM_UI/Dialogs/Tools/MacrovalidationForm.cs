using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using EM_Common;
using EM_Crypt;
using EM_Statistics;
using EM_Statistics.InDepthAnalysis;
using EM_Statistics.ExternalStatistics;
using EM_Transformer;
using EM_UI;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ExternalStatistics;
using EM_UI.Run;
using EM_XmlHandler;
using EM_Common_Win;

namespace Macrovalidation
{
    public partial class MacrovalidationForm : Form
    {
        List<FilePackageContent> filePackages;

        public MacrovalidationForm()
        {
            // load the template info and let the user select the files
            XML_handling.ParseTemplateInfo(EM_Statistics.Resources.Macrovalidation_xml, out Template.TemplateInfo templateInfo, out ErrorCollector errorCollector, true);
            if (errorCollector.HasErrors()) MessageBox.Show(errorCollector.GetErrorMessage());
            EM_Statistics.StatisticsPresenter.SelectFilesForm selectFilesForm = new EM_Statistics.StatisticsPresenter.SelectFilesForm(templateInfo);
            DialogResult = selectFilesForm.ShowDialog() == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
            if (DialogResult == DialogResult.Cancel) return;
            if (selectFilesForm.filePackages.Count < 1) return;

            filePackages = selectFilesForm.filePackages;
            if (filePackages[0].PathsAlt == null || filePackages[0].PathsAlt.Count < 1) return;
            string country = Path.GetFileName(filePackages[0].PathsAlt.First()).Split('_').First() + "_";
            if (!filePackages[0].PathsAlt.All(file => Path.GetFileName(file).StartsWith(country)))
            {
                MessageBox.Show("You can only select output files from one country.");
                return;
            }

            InitializeComponent();
            Show();
        }

        public static ExeXml.Country GetEM3Country(string emPathFolder, string countryShortName, string systemName)
        {
            EMPath emPath = new EMPath(emPathFolder);
            if (!File.Exists(emPath.GetCountryFilePath(countryShortName, true)))
                throw new Exception($"system info cannot be retrieved: country file {emPath.GetCountryFilePath(countryShortName, true)} does not exist.");

            if (!EM3Country.Transform(emPathFolder, countryShortName, out List<string> errors))
                throw new Exception($"transforming to EM3 failed: {string.Join(";", errors)}");

            string w = string.Empty; Communicator communicator = new Communicator()
            {
                errorAction = new Action<Communicator.ErrorInfo>
                        (ei => { w += $"{Path.GetFileName(emPath.GetFolderOutput())}: {ei.message}" + Environment.NewLine; })
            };

            ExeXml.Country countryContent = ExeXmlReader.ReadCountry(emPath.GetCountryFilePath(countryShortName),
                                                    systemName, null, // null for dataIdentifier
                                                    false, communicator, true); // false for 'ignorePrivate', true for 'readComment'
            return countryContent;
        }




        private void GetExternalStatistics(FilePackageContent filePackageContent, Dictionary<string, Dictionary<string, List<string>>> yearsPerCountry, out Dictionary<string, string> amounts, out Dictionary<string, string> ben, out Dictionary<string, string> level, out Dictionary<string, string> source, out Dictionary<string, string> comments, out Dictionary<string, string> destination, out Dictionary<string, Dictionary<string, string>> inequality, out Dictionary<string, Dictionary<string, string>> poverty)
        {
            // Initialize the dictionaries 
            // The key of each dictionary will be: incomelist_varname_year, and the value will be the value
            amounts = new Dictionary<string, string>();
            ben = new Dictionary<string, string>();
            level = new Dictionary<string, string>();
            source = new Dictionary<string, string>();
            comments = new Dictionary<string, string>();
            destination = new Dictionary<string, string>();
            inequality = new Dictionary<string, Dictionary<string, string>>();
            poverty = new Dictionary<string, Dictionary<string, string>>();

            string _selectedCountry = yearsPerCountry.Keys.First();
            List<string> _selectedYears = yearsPerCountry[_selectedCountry].Keys.ToList();
            _selectedYears.Sort();
            CountryConfigFacade _countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(yearsPerCountry.Keys.First(), true);
            Dictionary<string, ExternalStatisticAggregate> xmlIncomeListComponents = ExternalStatisticUtil.GetCountryXMLIncomeListComponents(_selectedCountry, _selectedYears);
            ExternalStatistic storedExternalStatistic = ExternalStatisticUtil.GetStoredExternalStatistics(_countryConfigFacade, xmlIncomeListComponents, _selectedYears);

            foreach (KeyValuePair<string, ExternalStatisticAggregate> agg in storedExternalStatistic.Aggregate)
            {
                ExternalStatisticAggregate singleAggr = agg.Value;
                string key = singleAggr.IncomeList + (string.IsNullOrEmpty(singleAggr.Name) ? "" : "_" + singleAggr.Name);
                Dictionary<string, ExternalStatisticAggregateValues> yearValues = singleAggr.YearValues;

                foreach (string year in _selectedYears)
                {
                    if (yearValues.ContainsKey(year))
                    {
                        string yearKey = key + "_" + year;
                        amounts.Add(yearKey, yearValues[year].Amount);
                        ben.Add(yearKey, yearValues[year].Beneficiares);
                        level.Add(yearKey, yearValues[year].Level);
                    }
                }

                source.Add(key, singleAggr.Source);
                comments.Add(key, singleAggr.Comment);
                destination.Add(key, singleAggr.Destination);
            }

            foreach (ExternalStatisticDistributional agg in storedExternalStatistic.Distributional.Values)
            {
                Dictionary<string, string> vals = new Dictionary<string, string>() { { "description", agg.Description }, { "source", agg.Source }, { "comments", agg.Comment } };
                foreach (string year in _selectedYears)
                {
                    if (agg.YearValues.ContainsKey(year))
                    {
                        vals.Add(year, agg.YearValues[year]);
                    }
                }
                if (agg.TableName == ExternalStatisticsForm.DISTRIBUTIONAL_INEQUALITY)
                    inequality.Add(agg.Name, vals);
                else if (agg.TableName == ExternalStatisticsForm.DISTRIBUTIONAL_POVERTY)
                    poverty.Add(agg.Name, vals);
                else
                    MessageBox.Show("ERROR! Invalid distributional table found.");
            }
        }

        private void MacrovalidationForm_Shown(object sender, EventArgs e)
        {
            Refresh();  // make sure the form is displayed properly
            Cursor.Current = Cursors.WaitCursor;

            XML_handling.ParseTemplate(EM_Statistics.Resources.Macrovalidation_xml, out Template template, out ErrorCollector errorCollector, true);
            if (errorCollector.HasErrors()) MessageBox.Show(errorCollector.GetErrorMessage());
            TemplateApi templateApi = new TemplateApi(template, errorCollector); templateApi.SetEndUserFriendlyActionErrorMode();

            if (filePackages.Count < 0) return;

            FilePackageContent filePackageContent = filePackages[0];
            if (filePackageContent.PathsAlt.Count < 1) return;
            Dictionary<string, Dictionary<string, List<string>>> yearsPerCountry = new Dictionary<string, Dictionary<string, List<string>>>();
            List<string> systems = new List<string>();
            foreach (string filenameAndPath in filePackageContent.PathsAlt)
            {
                string sysName = Path.GetFileNameWithoutExtension(filenameAndPath);
                if (sysName.EndsWith("_std")) sysName = sysName.Substring(0, sysName.Length - 4);
                string[] fileInfo = sysName.Split('_');
                if (fileInfo.Length < 2) continue;
                if (!yearsPerCountry.ContainsKey(fileInfo[0])) yearsPerCountry.Add(fileInfo[0], new Dictionary<string, List<string>>());
                if (!yearsPerCountry[fileInfo[0]].ContainsKey(fileInfo[1])) yearsPerCountry[fileInfo[0]].Add(fileInfo[1], new List<string>());
                yearsPerCountry[fileInfo[0]][fileInfo[1]].Add(filenameAndPath);
                systems.Add(sysName);
            }
            string country = yearsPerCountry.Keys.First();
            List<string> years = yearsPerCountry[country].Keys.ToList();

            GetExternalStatistics(filePackages[0], yearsPerCountry, out Dictionary<string, string> amounts, out Dictionary<string, string> ben, out Dictionary<string, string> level,
                                                out Dictionary<string, string> source, out Dictionary<string, string> comments, out Dictionary<string, string> destination,
                                                out Dictionary<string, Dictionary<string, string>> inequality, out Dictionary<string, Dictionary<string, string>> poverty);

            Dictionary<string, Dictionary<string, SettingsMacrovalidation.ILVarSummary>> allIncomelists = new Dictionary<string, Dictionary<string, SettingsMacrovalidation.ILVarSummary>>();
            int reform = 0;
            foreach (string system in systems)
            {
                Dictionary<string, ExternalStatisticsComponent> componentsIncomeList = ExternalStatisticUtil.ReadIncomeListComponentsFromXMLFile(country, new List<string>() { system }, true, false);
                Cursor.Current = Cursors.WaitCursor;
                foreach (ExternalStatisticsComponent esc in componentsIncomeList.Values)
                {
                    foreach (KeyValuePair<string, Dictionary<string, IlVarInfo>> keyValuePair in esc.fiscalIls)
                    {
                        string il = keyValuePair.Key;
                        if (!allIncomelists.ContainsKey(il)) allIncomelists.Add(il, new Dictionary<string, SettingsMacrovalidation.ILVarSummary>());
                        foreach (string var in keyValuePair.Value.Keys)
                        {
                            if (!allIncomelists[il].ContainsKey(var))
                            {
                                SettingsMacrovalidation.ILVarSummary ivi = new SettingsMacrovalidation.ILVarSummary()
                                {
                                    varName = var,
                                    description = keyValuePair.Value[var].description
                                };
                                string varKey = il + (string.IsNullOrEmpty(var) ? "" : "_" + var);
                                if (destination.ContainsKey(varKey)) ivi.destination = destination[varKey];
                                // all previous years it did not exist, but it exists in this one, so add values for previous years too
                                for (int i = 0; i < (reform); i++)
                                {
                                    ivi.exists.Add(false);
                                    ivi.reformsSubstract.Add(null);
                                }
                                ivi.exists.Add(true);
                                ivi.reformsSubstract.Add(keyValuePair.Value[var].subtract);
                                allIncomelists[il].Add(var, ivi);
                            }
                            else
                            {
                                allIncomelists[il][var].exists.Add(true);
                                allIncomelists[il][var].reformsSubstract.Add(keyValuePair.Value[var].subtract);
                            }
                        }
                    }
                }
                reform++;
                // finally, there might have been some missing varialbes in this year, that existed before
                // add values for them too. make sure you do this *after* increasing "reform"!
                allIncomelists.All(il => il.Value.All(v => {
                    if (v.Value.exists.Count < reform) v.Value.exists.Add(false);
                    if (v.Value.reformsSubstract.Count < reform) v.Value.reformsSubstract.Add(null);
                    return true;
                }));

            }

            SettingsMacrovalidation settingsmacrovalidation = new SettingsMacrovalidation();


            //settingsmacrovalidation.getSystemNamesAndYears(yearsPerCountry);
            settingsmacrovalidation.ModifyTemplate(templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars, yearsPerCountry.First().Value, allIncomelists,
                amounts, ben, level, source, comments, destination, inequality, poverty);

            EM_Statistics.StatisticsPresenter.StatisticsPresenter.Run(new Dictionary<string, object> {
                            { "template", templateApi.GetTemplate() },
                            { "FilePackages", filePackages },
                            { "userinput", systemSpecificVars } });

            Cursor.Current = Cursors.Default;
            Close();
        }
    }
}
