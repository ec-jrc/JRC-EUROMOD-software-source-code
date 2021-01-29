using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    internal partial class RVCompareVersionsForm : Form
    {
        internal RVCompareVersionsForm(string compareCountryFolder)
        {
            InitializeComponent();

            ProgressIndicator progressIndicator = new ProgressIndicator(GetCountryInfo_BackgroundEventHandler, "Comparing ...",
                                                                        EMPath.AddSlash(compareCountryFolder));
            if (progressIndicator.ShowDialog() != System.Windows.Forms.DialogResult.OK) return; // user pressed Cancel

            foreach (var c in progressIndicator.Result as Dictionary<string, List<string>>)
                dataGrid.Rows.Add(c.Key, c.Value[0], c.Value[1], c.Value[2], c.Value[3], c.Value[4], c.Value[5]);
        }

        static void GetCountryInfo_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Result = null; e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            try
            {
                double i = 0; Dictionary<string, List<string>> cmpInfo = new Dictionary<string, List<string>>(); 
                foreach (Country cLoc in CountryAdministrator.GetCountries())
                {
                    backgroundWorker.ReportProgress(Convert.ToInt32(i / CountryAdministrator.GetCountries().Count * 100.0)); ++i;

                    CountryConfigFacade ccfLoc = cLoc.GetCountryConfigFacade(); if (ccfLoc == null) continue;
                    DataConfigFacade dcfLoc = cLoc.GetDataConfigFacade(); if (dcfLoc == null) continue;
                    
                    string cmpCountryFolder = e.Argument.ToString() + cLoc._shortName; if (!Directory.Exists(cmpCountryFolder)) continue;
                    Country cCmp = new Country(); cCmp._shortName = cLoc._shortName;
                    CountryConfigFacade ccfCmp = cCmp.GetCountryConfigFacade(true, cmpCountryFolder); if (ccfCmp == null) continue;
                    DataConfigFacade dcfCmp = cCmp.GetDataConfigFacade(true, cmpCountryFolder); if (dcfCmp == null) continue;

                    string addData = string.Empty, addSys = string.Empty, addPol = string.Empty, delData = string.Empty, delSys = string.Empty, delPol = string.Empty;
                    List<string> cmpData = new List<string>(), cmpSys = new List<string>(), cmpPol = new List<string>();

                    foreach (DataConfig.DataBaseRow d in dcfCmp.GetDataBaseRows()) cmpData.Add(d.Name.EndsWith(".txt") ? d.Name.ToLower() : (d.Name + ".txt").ToLower());
                    foreach (CountryConfig.SystemRow s in ccfCmp.GetSystemRows()) cmpSys.Add(s.Name.ToLower());
                    if (cmpSys.Count > 0)
                        foreach (CountryConfig.PolicyRow p in ccfCmp.GetSystemRows().First().GetPolicyRows())
                            if (!p.Name.ToLower().StartsWith(UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name)) cmpPol.Add(p.Name.ToLower());

                    foreach (DataConfig.DataBaseRow d in dcfLoc.GetDataBaseRows())
                    {
                        string dataName = d.Name.EndsWith(".txt") ? d.Name.ToLower() : (d.Name + ".txt").ToLower();
                        if (cmpData.Contains(dataName)) cmpData.Remove(dataName); else addData += dataName + ", ";
                    }
                    foreach (string d in cmpData) if (d != string.Empty) delData += d + ", ";

                    foreach (CountryConfig.SystemRow s in ccfLoc.GetSystemRows())
                    {
                        if (cmpSys.Contains(s.Name.ToLower())) cmpSys.Remove(s.Name.ToLower()); else addSys += s.Name.ToLower() + ", ";
                    }
                    foreach (string s in cmpSys) if (s != string.Empty) delSys += s + ", ";

                    if (ccfLoc.GetSystemRows().Count > 0)
                        foreach (CountryConfig.PolicyRow p in ccfLoc.GetSystemRows().First().GetPolicyRows())
                        {
                            if (p.Name.ToLower().StartsWith(UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name)) continue;
                            if (cmpPol.Contains(p.Name.ToLower())) cmpPol.Remove(p.Name.ToLower()); else addPol += p.Name.ToLower() + ", ";
                        }
                    foreach (string p in cmpPol) if (p != string.Empty) delPol += p + ", ";

                    char[] t = new char[] { ',', ' ' };
                    cmpInfo.Add(cLoc._shortName, new List<string>() { addData.TrimEnd(t), delData.TrimEnd(t), addSys.TrimEnd(t), delSys.TrimEnd(t), addPol.TrimEnd(t), delPol.TrimEnd(t) });
                }

                backgroundWorker.ReportProgress(100);
                e.Result = cmpInfo;
            }
            catch (Exception exception) { e.Result = exception.Message; backgroundWorker.ReportProgress(100); }
        }
    }
}
