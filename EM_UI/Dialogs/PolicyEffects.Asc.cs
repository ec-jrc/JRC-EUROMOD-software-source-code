using EM_Common;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    partial class PolicyEffects
    {
        private void goAsc()
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add("templatename", Path.Combine(EnvironmentInfo.GetSystemTemplateFolder(), "PET_template.xml"));
 
                // template is of type BASE_ALT and allows for multiple packages
                // we need one or two packages per country: one if one dataset is selected, two if both datasets are selected
                // each package hase one base and as many alternatives as alphas (for Custom, CPI, MII)

                List<FilePackageContent> filePackages = new List<FilePackageContent>();
                List<Template.TemplateInfo.UserVariable> userVariables = new List<Template.TemplateInfo.UserVariable>();

                int indexPackage = 0;
                foreach (var runCountry in runCountries)
                {
                    string countryName = runCountry.Key;
                    DataRow policyTableRow = policyDataTable.Select("Country='" + countryName + "'")[0];
                    foreach (var data in new[] { new { radio = checkRadioData1, sysPrim = runCountry.Value.Item1, sysSec = runCountry.Value.Item2, index = "1" },
                                                 new { radio = checkRadioData2, sysPrim = runCountry.Value.Item2, sysSec = runCountry.Value.Item1, index = "2" } })
                    {
                        if (data.radio.Checked || checkRadioDataBoth.Checked)
                        {
                            FilePackageContent filePackage = new FilePackageContent();
                            filePackage.PathBase = textBoxOutputPath.Text + data.sysPrim + "_std.txt";
                            int indexAlt = 0;
                            string dataName = policyDataTable.Select("Country='" + countryName + "'").First().Field<string>("Data" + data.index); // used for table-caption
                            if (checkBoxAlphaFIX.Checked)
                            {
                                foreach (double a in alphaFIXValues)
                                {
                                    filePackage.PathsAlt.Add(textBoxOutputPath.Text + data.sysSec + "_on_" + policyTableRow["Data" + data.index.ToString()].ToString().ToLower() + "_a" + GetAlphaFIXId(a) + "_std.txt");
                                    ++indexAlt;
                                    CalculateFormulaConsts(userVariables, filePackage.Key, indexAlt, data.index == "1",
                                                           alphaValues[countryName + "_fix" + GetAlphaFIXId(a)], dataName, "custom");
                                }
                            }
                            if (checkBoxAlphaCPI.Checked)
                            {
                                filePackage.PathsAlt.Add(textBoxOutputPath.Text + data.sysSec + "_on_" + policyTableRow["Data" + data.index.ToString()].ToString().ToLower() + "_cpi_std.txt");
                                ++indexAlt;
                                CalculateFormulaConsts(userVariables, filePackage.Key, indexAlt, data.index == "1",
                                                       alphaValues[countryName + "_cpi"], dataName, "CPI");
                            }
                            if (checkBoxAlphaMII.Checked)
                            {
                                filePackage.PathsAlt.Add(textBoxOutputPath.Text + data.sysSec + "_on_" + policyTableRow["Data" + data.index.ToString()].ToString().ToLower() + "_mii_std.txt");
                                ++indexAlt;
                                CalculateFormulaConsts(userVariables, filePackage.Key, indexAlt, data.index == "1",
                                                       alphaValues[countryName + "_mii"], dataName, "MII");
                            }

                            // make sure all files exist, otherwise do not add the package (probably an error occurred or the run was canceled)
                            bool allOk = File.Exists(filePackage.PathBase);
                            foreach (string pa in filePackage.PathsAlt) if (!File.Exists(pa)) { allOk = false; break; }
                            if (allOk)
                            {
                                userVariables.Add(new Template.TemplateInfo.UserVariable() // user variable is used for generating the caption for the buttons below the tables
                                {
                                    packageKey = filePackage.Key,
                                    name = "BottomButtonCaption",
                                    description = string.Format("Data{0}", data.index),
                                    inputType = HardDefinitions.UserInputType.Numeric, value = "1",
                                });
                                filePackages.Add(filePackage); ++indexPackage;
                            }
                        }
                    }
                }

                userVariables.Add(new Template.TemplateInfo.UserVariable() // user variable is used for generating the main title of the presenter
                {
                    name = "MainCaption",
                    description = string.Format("The policy effects in {0} - {1}", comboBox1.Text, comboBox2.Text),
                    inputType = HardDefinitions.UserInputType.Numeric, value = "1"
                });

                arguments.Add("FilePackages", filePackages);
                arguments.Add("userinput", userVariables);
                PiInterface statisticsPresenter = PiLoader.GetPlugIn("Statistics Presenter");
                statisticsPresenter.Run(arguments);
            }
            catch (Exception exception) { MessageBox.Show(exception.Message); }
        }

        private void CalculateFormulaConsts(List<Template.TemplateInfo.UserVariable> formulaConstants, string filePackageKey,
                                            int indexAlt, bool data1, double alpha,
                                            string dataNameForCaption, string alphaForCaption)
        {   // the 4 formulas (see PolicyEffects.cs calculateStatistics) can be written as an extension of the basic formula (C - B) / Bdpi:
            // (const1 * (const2 * C - B) * cb_bc_converter) / Bdpi
            // (the cb_bc_converter takes the values 1 or -1 to switch between C-B and B-C)
            double const1 = 1;
            double const2 = 1;
            int cb_bc_converter = 1;
            if (showFull)
            {
                if (data1)
                {
                    const1 = (2 + alpha) / 3.0;
                    if (!checkRadioMarket.Checked) const2 = 1 / alpha;
                }
                else
                {
                    cb_bc_converter = -1;
                    const1 = (1 / 3.0) * ((1 / alpha) + 2);
                    if (!checkRadioMarket.Checked) const2 = alpha;
                }
            }

            foreach (var uv in new[]
            { 
                // user variables for the 3 constants used in the template
                new { name = "Const1", value = const1.ToString(), description = string.Empty },
                new { name = "Const2", value = const2.ToString(), description = string.Empty },
                new { name = "Const3", value = cb_bc_converter.ToString(), description = string.Empty },
                // user variables for generating the table caption and the caption for the buttons above the tables
                new { name = "TableCaption", value = "1", description = string.Format("{0} [{1}] on dataset {2}", alphaForCaption, alpha, dataNameForCaption) },
                new { name = "TopButtonCaption", value = "1", description = string.Format("alpha={0} (Data{1})", alphaForCaption, data1 ? "1" : "2") }
            })
                formulaConstants.Add(new Template.TemplateInfo.UserVariable()
                {
                    packageKey = filePackageKey,
                    reformNumber = indexAlt,
                    name = uv.name,
                    description = uv.description,
                    value = uv.value,
                    inputType = HardDefinitions.UserInputType.Numeric
                });
        }
    }
}
