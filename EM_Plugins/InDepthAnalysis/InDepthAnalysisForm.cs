using EM_Common;
using EM_Common_Win;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    internal partial class InDepthAnalysisForm : Form
    {
        private readonly Template.TemplateInfo templateInfo = null;
        private readonly List<ISettings> settingsDialogs = new List<ISettings>();
        private Settings currentSettings = new Settings();
        private readonly EMVarInfo emVarInfo = new EMVarInfo();
        private MergedDatasetHandler mergedDatasetHandler = new MergedDatasetHandler();
        private string currentSettingsPath = string.Empty;

        private const string IN_DEPTH_SESSION_INFO = "IN_DEPTH_SESSION_INFO";
        private const string IN_DEPTH_ACKNOWLEDGEMENT = "The In-depth Analysis plugin was developed as a part of the project " +
            "\"Support for the Microsimulation Hub of the Council of Economic Advisors in Greece\", " +
            "funded by the European Commission’s Structural Reform Support Programme and implemented by the " +
            "Joint Research Centre through an administrative arrangement with DG REFORM. All parties agreed to " +
            "incorporate the plugin to EUROMOD, which implies the public distribution of its compiled version under " +
            "the EUROMOD End-user licence agreement and its source code under the European Union Public Licence (EUPL-1.2) " +
            "or a later version of this licence.";

        internal InDepthAnalysisForm()
        {
            try
            {
                InitializeComponent();
                InitTreeAndDialogs();
                InDepthAnalysis.SetShowHelp(this, helpProvider);

                bool success = XML_handling.ParseTemplate(Properties.Resources.InDepthAnalysis, out Template template, out ErrorCollector errorCollector, true);
                if (errorCollector.HasErrors()) MessageBox.Show(errorCollector.GetErrorMessage());
                if (!success) return;

                templateInfo = template.info;

                // "update" to default settings and possibly to baselines & reforms selected in a previous In-depth-session of the current UI-session
                TSDictionary pluginSessionInfo = UISessionInfo.GetSessionUserSettings(IN_DEPTH_SESSION_INFO);
                if (pluginSessionInfo != null && pluginSessionInfo.ContainsKey(IN_DEPTH_SESSION_INFO))
                    currentSettings = pluginSessionInfo.GetItem<Settings>(IN_DEPTH_SESSION_INFO);
                UpdateForNewSettings();
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void InitTreeAndDialogs()
        {
            treeCategories.Nodes.Clear();

            string nameMainNode = SettingsFiscal.pageName_Fiscal;
            settingsDialogs.Add(new SettingsFiscal());
            int iMainNode = treeCategories.Nodes.Add(new TreeNode("1. Fiscal") { Name = nameMainNode });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("1.1. Revenue-expenditure") { Name = SettingsFiscal.tableName_RevenueExpenditure });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("1.2. Number of taxpayers/beneficiaries") { Name = SettingsFiscal.tableName_FiscalTaxpayersBeneficiaries });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("1.3. Disaggregated concepts") { Name = SettingsFiscal.tableName_DisaggregatedConcepts });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("1.4. Disaggregated concepts, number of units") { Name = SettingsFiscal.tableName_DisaggregatedConceptsUnits });

            nameMainNode = SettingsDistributional.pageName_Distributional;
            settingsDialogs.Add(new SettingsDistributional());
            iMainNode = treeCategories.Nodes.Add(new TreeNode("2. Distributional") { Name = nameMainNode });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.1. Taxpayers-beneficiaries") { Name = SettingsDistributional.tableName_DistributionalTaxpayersBeneficiaries });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.2. Total tax or benefit") { Name = SettingsDistributional.tableName_TotalTaxOrBenefit });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.3. Mean tax or benefit") { Name = SettingsDistributional.tableName_MeanTaxOrBenefit });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.4. Average tax burden") { Name = SettingsDistributional.tableName_AverageTaxBurden });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.5. Mean disposable income") { Name = SettingsDistributional.tableName_MeanDisposableIncome });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.6. Mean eq. disp. income") { Name = SettingsDistributional.tableName_MeanEqDispIncome });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.7. Winners") { Name = SettingsDistributional.tableName_Winners });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("2.8. Losers") { Name = SettingsDistributional.tableName_Losers });

            nameMainNode = SettingsInequalityAndPoverty.pageName_InequalityAndPoverty;
            settingsDialogs.Add(new SettingsInequalityAndPoverty());
            iMainNode = treeCategories.Nodes.Add(new TreeNode("3. Inequality and Poverty") { Name = nameMainNode });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.1. Inequality") { Name = SettingsInequalityAndPoverty.tableName_Inequality });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.2. Progressivity") { Name = SettingsInequalityAndPoverty.tableName_Progressivity });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.3. AROP rates") { Name = SettingsInequalityAndPoverty.tableName_AropRates });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.4. AROP gap") { Name = SettingsInequalityAndPoverty.tableName_AropGap });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.5. AROP rates by type") { Name = SettingsInequalityAndPoverty.tableName_AropRatesByType });
            treeCategories.Nodes[iMainNode].Nodes.Add(new TreeNode("3.6. AROP gap by type") { Name = SettingsInequalityAndPoverty.tableName_AropGapByType });

            treeCategories.ExpandAll();
        }

        void UpdateForNewSettings()
        {
            if (currentSettings == null) currentSettings = new Settings();

            radioCompareWithBaseline.Checked = currentSettings.compareWithBaseline;
            radioCompareWithPrevious.Checked = !currentSettings.compareWithBaseline;
            
            txtPathEuromodFiles.Text = currentSettings.pathEuromodFiles;
            txtPathMergedDataset.Text = currentSettings.pathMergedDataset;
            chkSaveMergedDataset.Checked = currentSettings.saveMergedDataset;

            foreach (TreeNode mainCategory in treeCategories.Nodes)
            {
                mainCategory.Checked = !currentSettings.inactiveTablesAndPages.Contains(mainCategory.Name);
                foreach (TreeNode subCategory in mainCategory.Nodes)
                    subCategory.Checked = !currentSettings.inactiveTablesAndPages.Contains(subCategory.Name);
            }

            gridBaselines.Rows.Clear(); gridReforms.Rows.Clear();
            foreach (BaselineReformPackage baselineReformPackage in currentSettings.baselineReformPackages)
            {
                int baseRow = gridBaselines.Rows.Add(
                    File.Exists(baselineReformPackage.baseline.filePath) ? Properties.Resources.existing : Properties.Resources.missing,
                        Path.GetFileName(baselineReformPackage.baseline.filePath), baselineReformPackage.baseline.label);
                gridBaselines.Rows[baseRow].Cells[colBaselines.Index].ToolTipText = baselineReformPackage.baseline.filePath;
                gridBaselines.Rows[baseRow].Tag = baselineReformPackage;

                foreach (BaselineReformPackage.BaselineOrReform reform in baselineReformPackage.reforms)
                {
                    int refRow = gridReforms.Rows.Add(
                    File.Exists(reform.filePath) ? Properties.Resources.existing : Properties.Resources.missing,
                        Path.GetFileName(reform.filePath), reform.label);
                    gridReforms.Rows[refRow].Cells[colBaselines.Index].ToolTipText = reform.filePath;
                    gridReforms.Rows[refRow].Tag = new Tuple<BaselineReformPackage, int>(baselineReformPackage, baselineReformPackage.reforms.IndexOf(reform));
                }
            }

            if (UISessionInfo.GetActiveMainForm() != null) UISessionInfo.GetActiveMainForm().Cursor = Cursors.WaitCursor; // the tool may not yet be visible
            Cursor = Cursors.WaitCursor;
            currentSettings.UpdateBaselineReformInfo(out List<string> errors);
            if (errors.Any()) MessageBox.Show(string.Join(Environment.NewLine, errors));
            Cursor = Cursors.Default;
            if (UISessionInfo.GetActiveMainForm() != null) UISessionInfo.GetActiveMainForm().Cursor = Cursors.Default;

            foreach (ISettings settingsDialog in settingsDialogs) settingsDialog.UpdateSettings(currentSettings);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                bool hasValidPackages = currentSettings.GetFilePackages(out List<FilePackageContent> filePackages, out List<string> errors);
                if ((errors.Any() && MessageBox.Show(string.Join(Environment.NewLine, errors), null,
                    hasValidPackages ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK) == DialogResult.Cancel) || !hasValidPackages) return;

                // reload template to avoid any remainders from previous runs (but error-checking is not necessary anymore)
                XML_handling.ParseTemplate(Properties.Resources.InDepthAnalysis, out Template template, out _, true);
                ErrorCollector errorCollector = new ErrorCollector();
                TemplateApi templateApi = new TemplateApi(template, errorCollector); templateApi.SetEndUserFriendlyActionErrorMode();

                if (string.IsNullOrEmpty(currentSettings.pathEuromodFiles) || !emVarInfo.GetNonMonetaryVariables(currentSettings, currentSettings.pathEuromodFiles, out List<string> nonMonetaryVariables))
                    templateApi.SetNonMonetaryVariablesCreationMode(true); // "auto" non-monetary-variables specification, i.e. just variables starting with d or l
                else templateApi.SetNonMonetaryVariablesCreationMode(true, new List<string>(), nonMonetaryVariables); // selective non-monetary-variables specification, using info from variables file

                bool anythingChecked = false;
                foreach (TreeNode mainCategory in treeCategories.Nodes)
                {
                    foreach (TreeNode subCategory in mainCategory.Nodes)
                    {
                        if (!subCategory.Checked)
                            templateApi.ModifyTable(new Template.Page.Table() { name = subCategory.Name, active = false }, mainCategory.Name);
                    }
                    if (!mainCategory.Checked) templateApi.ModifyPage(new Template.Page() { name = mainCategory.Name, active = false });
                    else anythingChecked = true;
                }
                if (!anythingChecked && !currentSettings.saveMergedDataset) { MessageBox.Show("Please select at least one table or tick the option \"Save merged dataset at…\""); return; }

                if (anythingChecked)
                {
                    List<Template.TemplateInfo.UserVariable> systemSpecificVars = new List<Template.TemplateInfo.UserVariable>();
                    foreach (ISettings settingsDialog in settingsDialogs)
                    {
                        settingsDialog.ModifyTemplate(templateApi, out List<Template.TemplateInfo.UserVariable> ssv);
                        if (ssv != null) systemSpecificVars.AddRange(ssv);
                    }
                    currentSettings.WriteMetadata(templateApi);

                    List<PrettyInfoProvider.PackageLabels> labels = new List<PrettyInfoProvider.PackageLabels>();
                    foreach (FilePackageContent fp in filePackages)
                    {
                        BaselineReformPackage brp = (from p in currentSettings.baselineReformPackages
                                                     where EMPath.IsSamePath(p.baseline.filePath, fp.PathBase)
                                                     select p).FirstOrDefault();
                        if (brp != null)
                        {
                            PrettyInfoProvider.PackageLabels packageLabels = new PrettyInfoProvider.PackageLabels() {
                                packageKey = fp.Key, baseSystemLabel = brp.baseline.label };
                            foreach (var reform in brp.reforms) packageLabels.reformSystemLabels.Add(reform.label);
                            labels.Add(packageLabels);
                        }
                    }
                    templateApi.SetLabels(labels);

                    if (errorCollector.HasErrors()) MessageBox.Show(errorCollector.GetErrorMessage());

                    if (!currentSettings.compareWithBaseline)
                    {
                        errorCollector.Clear();
                        ModifyCompareColumns(templateApi);
                        if (errorCollector.HasErrors()) MessageBox.Show("!!! DEBUG ERROR MESSAGE - JUST FOR CHECKING !!!\n\n" + errorCollector.GetErrorMessage());
                    }

                    PiLoader.GetPlugIn("Statistics Presenter").Run(new Dictionary<string, object> {
                        { "template", templateApi.GetTemplate() },
                        { "FilePackages", filePackages },
                        { "userinput", systemSpecificVars } } );
                }

                if (chkSaveMergedDataset.Checked) mergedDatasetHandler.SaveMergedDataset(this, txtPathMergedDataset.Text, filePackages);
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void ModifyCompareColumns(TemplateApi templateApi)
        {
            foreach (TreeNode mainCategory in treeCategories.Nodes)
            {
                if (!mainCategory.Checked) continue;
                foreach (TreeNode subCategory in mainCategory.Nodes)
                {
                    if (!subCategory.Checked) continue;

                    templateApi.ReplaceInTitles(replace: "<br>(Baseline)", by: "",
                        pageNamePattern: mainCategory.Name, tableNamePattern: subCategory.Name, columnNamePattern: "*");
                    templateApi.ReplaceInTitles(replace: "Baseline", by: "previous",
                        pageNamePattern: mainCategory.Name, tableNamePattern: subCategory.Name, reformColumnNamePattern: "*");

                    foreach (var rb in new[] { new { replace = "Baseline", by = "Reform~Baseline" },
                                               new { replace = "BaselineShare", by = "ReformShare~BaselineShare" },
                                               new { replace = "BaselineTotal", by = "ReformTotal~BaselineTotal" }})
                        templateApi.ReplaceInCellActionFormulas(
                            replace: $"{HardDefinitions.FormulaParameter.BASE_COL}{rb.replace}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}",
                            by: $"{HardDefinitions.FormulaParameter.REF_COL_PRE}{rb.by}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}",
                            pageNamePattern: mainCategory.Name, tableNamePattern: subCategory.Name,
                            reformColumnNamePattern: "*", reformCellNamePattern: "*");
                }
            }
        }

        internal void ReportException(Exception exception)
        {
            MessageBox.Show($"In-depth Analysis: unexpected error.{Environment.NewLine}{exception}");
            Cursor = Cursors.Default; // undo any WaitCursor setting
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("You will lose any unsaved changes. Are you sure you want to start a new project?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            currentSettings = new Settings() { baselineReformPackages = new List<BaselineReformPackage>() };
            SetCurrentSettingsPath(string.Empty);
            UpdateForNewSettings();
        }

        private void miLoadSettings_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "XML files|*.xml", CheckPathExists = true, CheckFileExists = true, AddExtension = true, Multiselect = false, };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                currentSettings = Settings.FromXml(ofd.FileName, currentSettings?.baselineReformPackages);
                SetCurrentSettingsPath(ofd.FileName);
                UpdateForNewSettings();
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void SetCurrentSettingsPath(string settingsPath = "")
        {
            currentSettingsPath = settingsPath;
            string title = "In-depth Analysis";
            if (!string.IsNullOrEmpty(currentSettingsPath))
                title += " - " + currentSettingsPath;
            Text = title;
        }

        private void miSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(currentSettingsPath))
                    saveAsToolStripMenuItem_Click(null, null);
                else
                    currentSettings.ToXml(currentSettingsPath);
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { DefaultExt = ".xml", Filter = "XML files|*.xml" };
            if (sfd.ShowDialog() == DialogResult.Cancel) return;
            SetCurrentSettingsPath(sfd.FileName);
            currentSettings.ToXml(currentSettingsPath);
        }

        private void miResetSettings_Click(object sender, EventArgs e)
        {
            DialogResult storeBrbps = DialogResult.Yes;
            if (MessageBox.Show("All personalised settings will be reset to default values, continue?", string.Empty,
                            MessageBoxButtons.YesNo) == DialogResult.No) return;
            if (currentSettings.baselineReformPackages.Any())
            {
                storeBrbps = MessageBox.Show("Should baselines & reforms be reset too?", string.Empty, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (storeBrbps == DialogResult.Cancel) return;
            }

            currentSettings = new Settings() { baselineReformPackages =
                storeBrbps == DialogResult.No ? currentSettings.baselineReformPackages : new List<BaselineReformPackage>() };
            UpdateForNewSettings();
        }

        private void miFiscalSettings_Click(object sender, EventArgs e)
        {
            (from d in settingsDialogs where d.GetType() == typeof(SettingsFiscal) select d).First().ShowDialog();
        }

        private void miDistributionalSettings_Click(object sender, EventArgs e)
        {
            (from d in settingsDialogs where d.GetType() == typeof(SettingsDistributional) select d).First().ShowDialog();
        }

        private void miInequalityPovertySettings_Click(object sender, EventArgs e)
        {
            (from d in settingsDialogs where d.GetType() == typeof(SettingsInequalityAndPoverty) select d).First().ShowDialog();
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radioCompareWith_CheckedChanged(object sender, EventArgs e)
        {
            currentSettings.compareWithBaseline = radioCompareWithBaseline.Checked;
        }

        private void treeCategories_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent == null) foreach (TreeNode subNode in e.Node.Nodes) { subNode.Checked = e.Node.Checked; UpdateInactive(subNode); }
            else if (e.Node.Checked && !e.Node.Parent.Checked) { e.Node.Parent.Checked = true; UpdateInactive(e.Node.Parent); }
            else
            {
                bool isChecked = false;
                foreach (TreeNode subNode in e.Node.Parent.Nodes) if (subNode.Checked) isChecked = true;
                e.Node.Parent.Checked = isChecked;
                UpdateInactive(e.Node.Parent);
            }
            UpdateInactive(e.Node);
            
            void UpdateInactive(TreeNode node)
            {
                if (node.Checked) currentSettings.inactiveTablesAndPages.Remove(node.Name);
                else currentSettings.inactiveTablesAndPages.AddUnique(node.Name);
            }
        }

        private void btnSelectBaselinesAndReforms_Click(object sender, EventArgs e)
        {
            try
            {
                SelectBaselinesReforms form = new SelectBaselinesReforms(currentSettings, (from rv in templateInfo.requiredVariables select rv.readVar).ToList());
                if (form.ShowDialog() != DialogResult.OK) return;

                Dictionary<string, string> lablels = StoreLabels();
                currentSettings.pathBaselineFiles = Path.GetDirectoryName(form.filePackages[0].PathBase);
                currentSettings.pathReformFiles = Path.GetDirectoryName(form.filePackages[0].PathsAlt[0]);
                currentSettings.baselineReformPackages.Clear();

                foreach (FilePackageContent filePackage in form.filePackages)
                    currentSettings.baselineReformPackages.Add(new BaselineReformPackage(filePackage));
                RestoreLabels(lablels);

                UpdateForNewSettings();
            }
            catch (Exception exception) { ReportException(exception); }

            Dictionary<string, string> StoreLabels()
            {
                Dictionary<string, string> labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (BaselineReformPackage brp in currentSettings.baselineReformPackages)
                {
                    labels.TryAdd("base_" + brp.baseline.filePath, brp.baseline.label);
                    foreach (var reform in brp.reforms) labels.Add(reform.filePath, reform.label);
                }
                return labels;
            }
            void RestoreLabels(Dictionary<string, string> labels)
            {
                foreach (BaselineReformPackage brp in currentSettings.baselineReformPackages)
                {
                    if (labels.ContainsKey("base_" + brp.baseline.filePath)) brp.baseline.label = labels["base_" + brp.baseline.filePath];
                    foreach (var reform in brp.reforms) if (labels.ContainsKey(reform.filePath)) reform.label = labels[reform.filePath];
                }
            }
        }

        private void txtPathEuromodFiles_Validated(object sender, EventArgs e)
        {
            if (EMPath.IsSamePath(txtPathEuromodFiles.Text, currentSettings.pathEuromodFiles)) return;
            try
            {
                currentSettings.pathEuromodFiles = txtPathEuromodFiles.Text;
                currentSettings.UpdateBaselineReformInfo(out List<string> errors);
                if (errors.Any()) MessageBox.Show(string.Join(Environment.NewLine, errors));
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void btnSelPathEuromod_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { SelectedPath = txtPathEuromodFiles.Text };
                if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel) return;

                currentSettings.pathEuromodFiles = folderBrowserDialog.SelectedPath;

                //If no baselineReform packages have been selected yet, the path is also updated.
                if (currentSettings.baselineReformPackages == null || !currentSettings.baselineReformPackages.Any())
                {
                    EMPath emPath = new EMPath(currentSettings.pathEuromodFiles);
                    string newOutputFolder = emPath.GetFolderOutput();
                    currentSettings.pathBaselineFiles = newOutputFolder;
                    currentSettings.pathReformFiles = newOutputFolder;
                }

                if (!EMPath.IsSamePath(txtPathEuromodFiles.Text, currentSettings.pathEuromodFiles))
                {
                    currentSettings.UpdateBaselineReformInfo(out List<string> errors);
                    if (errors.Any()) MessageBox.Show(string.Join(Environment.NewLine, errors));
                }
                txtPathEuromodFiles.Text = currentSettings.pathEuromodFiles;
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void btnRstPathEuromod_Click(object sender, EventArgs e)
        {
            try { 
                currentSettings.ResetPathEuromodFilesFolder();

                if (!EMPath.IsSamePath(txtPathEuromodFiles.Text, currentSettings.pathEuromodFiles))
                {
                    //If no baselineReform packages have been selected yet, the path is also updated.
                    if (currentSettings.baselineReformPackages == null || !currentSettings.baselineReformPackages.Any())
                    {
                        EMPath emPath = new EMPath(currentSettings.pathEuromodFiles);
                        string newOutputFolder = emPath.GetFolderOutput();
                        currentSettings.pathBaselineFiles = newOutputFolder;
                        currentSettings.pathReformFiles = newOutputFolder;
                    }

                    currentSettings.UpdateBaselineReformInfo(out List<string> errors);
                    if (errors.Any()) MessageBox.Show(string.Join(Environment.NewLine, errors));
                }

                txtPathEuromodFiles.Text = currentSettings.pathEuromodFiles;

            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void btnSelPathMergedDataset_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { SelectedPath = txtPathMergedDataset.Text };
                if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel) return;
                txtPathMergedDataset.Text = currentSettings.pathMergedDataset = folderBrowserDialog.SelectedPath;
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void btnRstPathMergedDataset_Click(object sender, EventArgs e)
        {
            try
            {
                currentSettings.ResetPathMergedDataset();
                txtPathMergedDataset.Text = currentSettings.pathMergedDataset;
            }
            catch (Exception exception) { ReportException(exception); }
        }

        private void txtPathMergedDataset_Validated(object sender, EventArgs e)
        {
            currentSettings.pathMergedDataset = txtPathMergedDataset.Text;
        }

        private void chkSaveMergedDataset_CheckedChanged(object sender, EventArgs e)
        {
            currentSettings.saveMergedDataset = chkSaveMergedDataset.Checked;
        }

        private void gridBaselines_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colBaselineLabel.Index || e.RowIndex < 0) return;
            BaselineReformPackage brpRow = gridBaselines.Rows[e.RowIndex].Tag as BaselineReformPackage;
            foreach (BaselineReformPackage brp in currentSettings.baselineReformPackages)
                if (brp == brpRow) brp.baseline.label = gridBaselines.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
        }

        private void gridReforms_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colReformsLabel.Index || e.RowIndex < 0) return;
            Tuple<BaselineReformPackage, int> tpl = gridReforms.Rows[e.RowIndex].Tag as Tuple<BaselineReformPackage, int>;
            foreach (BaselineReformPackage brp in currentSettings.baselineReformPackages)
                if (brp == tpl.Item1) brp.reforms[tpl.Item2].label = gridReforms.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
        }

        private void InDepthAnalysisForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mergedDatasetHandler.StopAllActions();

                // store selected baselines & reforms for reloading, if the In-depth-plugin is opened again in the current UI-session
                TSDictionary pluginSessionInfo = new TSDictionary();
                pluginSessionInfo.SetItem(IN_DEPTH_SESSION_INFO, currentSettings);
                UISessionInfo.SetSessionUserSettings(IN_DEPTH_SESSION_INFO, pluginSessionInfo);
            }
            catch { }
        }

        private void miHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, InDepthAnalysis.HelpPath);
        }

        private void miHelpAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"In-depth Analysis v{Settings.VERSION}. "+ System.Environment.NewLine + System.Environment.NewLine +  IN_DEPTH_ACKNOWLEDGEMENT, "About");
        }

        private void treeCategories_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node != null) // avoid blue node background colour, if text is selected
            {
                var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
                e.Graphics.FillRectangle(new SolidBrush(treeCategories.BackColor), e.Node.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, treeCategories.ForeColor, TextFormatFlags.GlyphOverhangPadding);
            }
        }

    }
}
