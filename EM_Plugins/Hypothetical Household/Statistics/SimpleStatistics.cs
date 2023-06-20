using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    class SimpleStatistics
    {
        private const string USER_VARIABLE_HHTYPE_SELECTED = "hhtype_selected";
        private const string USER_VARIABLE_HHTYPE_DESCRIPTION = "hhtype_description";

        private const string TEMPLATE_NAME_BUDGET_CONSTRAINTS = "hhot_budgetConstraints_template.xml";
        private const string TEMPLATE_NAME_BARS_PER_COUNTRYYEAR = "hhot_barsPerCountryYear_template.xml";
        private const string TEMPLATE_NAME_BARS_PER_HHTYPE = "hhot_barsPerHHType_template.xml";

        internal class HHTypes_Files
        {
            internal string hhTypeID; internal string hhTypeName = string.Empty;
            internal List<string> files = new List<string>(); // list of output files containing the HH-type
        }

        internal static void BudgetConstraints(Dictionary<int, string> hhTypes = null, List<string> outputFiles = null)
        {
            try
            {
                List<HHTypes_Files> hhTypes_files = new List<HHTypes_Files>();
                if (hhTypes == null) // call from Stastics-Menu: let user select files
                {                    // and find out which HH-types are contained in which files
                    if (!MenuCall_GetSelection(TEMPLATE_NAME_BUDGET_CONSTRAINTS, out hhTypes_files))
                        return;
                }
                else // call from Wizard: the (single) HH-type is contained in each file
                {    // put parameter-content into Dictionary, to allow for common further treatment
                    hhTypes_files.Add(new HHTypes_Files() { hhTypeID = hhTypes.First().Key.ToString(), hhTypeName = hhTypes.First().Value, files = outputFiles });
                }

                List<FilePackageContent> filePackages = new List<FilePackageContent>();
                List<Template.TemplateInfo.UserVariable> userVariables = new List<Template.TemplateInfo.UserVariable>();

                foreach (HHTypes_Files hhtf in hhTypes_files)
                {
                    foreach (string file in hhtf.files)
                    {
                        FilePackageContent fpc = new FilePackageContent() { PathBase = file };
                        userVariables.Add(new Template.TemplateInfo.UserVariable()
                        {
                            name = USER_VARIABLE_HHTYPE_SELECTED,
                            description = hhtf.hhTypeName,
                            value = hhtf.hhTypeID,
                            packageKey = fpc.Key
                        });
                        filePackages.Add(fpc);
                    }
                }

                StartPresenter(TEMPLATE_NAME_BUDGET_CONSTRAINTS, filePackages, userVariables);
            }
            catch (Exception e) { MessageBox.Show("Failed to generate Statistics." + Environment.NewLine + e.Message); }
        }

        internal static void BreakDownCountryYear(Dictionary<int, string> hhTypes = null, List<string> outputFiles = null)
        {
            try
            {
                List<HHTypes_Files> hhTypes_files = new List<HHTypes_Files>();
                if (hhTypes == null) // call from Stastics-Menu: let user select files
                {                    // and find out which HH-types are contained in which files
                    if (!MenuCall_GetSelection(TEMPLATE_NAME_BARS_PER_COUNTRYYEAR, out hhTypes_files))
                        return;
                }
                else // call from Wizard: all files contain all hh-types
                {    // put parameter-content into Dictionary, to allow for common further treatment
                    foreach (var hhType in hhTypes)
                        hhTypes_files.Add(new HHTypes_Files() { hhTypeID = hhType.Key.ToString(), hhTypeName = hhType.Value, files = outputFiles });
                }

                List<FilePackageContent> filePackages = new List<FilePackageContent>();
                List<Template.TemplateInfo.UserVariable> userVariables = new List<Template.TemplateInfo.UserVariable>();
                foreach (HHTypes_Files hhtf in hhTypes_files)
                {
                    FilePackageContent fpc = new FilePackageContent();
                    foreach (string file in hhtf.files) fpc.PathsAlt.Add(file);
                    userVariables.Add(new Template.TemplateInfo.UserVariable()
                    {
                        name = USER_VARIABLE_HHTYPE_SELECTED,
                        description = hhtf.hhTypeName,
                        value = hhtf.hhTypeID,
                        packageKey = fpc.Key
                    });
                    filePackages.Add(fpc);
                }

                StartPresenter(TEMPLATE_NAME_BARS_PER_COUNTRYYEAR, filePackages, userVariables);
            }
            catch (Exception e) { MessageBox.Show("Failed to generate Statistics." + Environment.NewLine + e.Message); }
            
        }

        internal static void BreakDownHHTypes(Dictionary<int, string> hhTypes = null, List<string> outputFiles = null)
        {
            try
            {
                if (hhTypes == null) // call from Stastics-Menu: let user select files
                {                    // and get all HH-types in any file
                    if (!MenuCall_GetSelection(TEMPLATE_NAME_BARS_PER_HHTYPE, out List<HHTypes_Files> hhTypes_files))
                        return;
                    // for this template we only require a (unique) list of output-files and descriptions for any HH-type
                    hhTypes = new Dictionary<int, string>(); outputFiles = new List<string>();
                    foreach (HHTypes_Files hhtf in hhTypes_files)
                    {
                        hhTypes.Add(int.Parse(hhtf.hhTypeID), hhtf.hhTypeName);
                        foreach (string file in hhtf.files) if (!outputFiles.Contains(file)) outputFiles.Add(file);
                    }
                }

                Dictionary<string, string> hhTypeDescription = new Dictionary<string, string>();

                List<FilePackageContent> filePackages = new List<FilePackageContent>();
                foreach (string of in outputFiles) filePackages.Add(new FilePackageContent() { PathBase = of });

                foreach (var i in hhTypes) hhTypeDescription.Add(i.Key.ToString(), i.Value);
                List<Template.TemplateInfo.UserVariable> userVariable = new List<Template.TemplateInfo.UserVariable>()
                { new Template.TemplateInfo.UserVariable() { name = USER_VARIABLE_HHTYPE_DESCRIPTION, forEachValueDescription = hhTypeDescription } };

                StartPresenter(TEMPLATE_NAME_BARS_PER_HHTYPE, filePackages, userVariable);
            }
            catch (Exception e) { MessageBox.Show("Failed to generate Statistics." + Environment.NewLine + e.Message); }
        }

        private static bool MenuCall_GetSelection(string templateName, out List<HHTypes_Files> hhTypes_files)
        {
            hhTypes_files = new List<HHTypes_Files>();
            if (!XML_handling.ParseTemplateInfo(Path.Combine(Program.getHHOTfolder(), templateName), out Template.TemplateInfo templateInfo, out ErrorCollector errorCollector))
            {
                MessageBox.Show(errorCollector.GetErrorMessage());
                return false;
            }
            try
            {
                // first let user select files
                // and find out which HH-types the selected files contain:
                // i.e. to each distinct HH-type found in any of the files assign a list of files that contain it
                SelectFilesForm selectFilesForm = new SelectFilesForm(templateInfo);
                if (selectFilesForm.ShowDialog() == DialogResult.Cancel) return false;

                // then let user select HH-types
                if (templateName == TEMPLATE_NAME_BARS_PER_HHTYPE)
                    hhTypes_files = selectFilesForm.hhTypes_files; // breakdown per HH-type does not require user-selection of HH-types
                else
                {
                    SelectHHTypesForm selectHHTypesForm = new SelectHHTypesForm(templateInfo,
                        (from hhtf in selectFilesForm.hhTypes_files select hhtf.hhTypeName).ToList()); // the (rather stupid) dialog gets just the names of the HH-types ...
                    if (selectHHTypesForm.ShowDialog() == DialogResult.Cancel) return false;
                    foreach (int index in selectHHTypesForm.selectedHHTypesIndices) // ... and returns the inidces of the selected HH-types ...
                        hhTypes_files.Add(selectFilesForm.hhTypes_files[index]); // ... which are used to copy from the SelectFilesForm-structure to the result of this function

                    // warn if a selected file is dropped, because it does not contain any of the selected hh-types
                    string notCovered = Environment.NewLine;
                    foreach (string selectedFile in GetPlainFileList(selectFilesForm.hhTypes_files))
                        if (!GetPlainFileList(hhTypes_files).Contains(selectedFile)) notCovered += Path.GetFileName(selectedFile) + Environment.NewLine;                       
                    if (notCovered.Trim() != string.Empty && MessageBox.Show($"The following file(s) do not contain any of the selected HH-types and are therefore neglected: {notCovered.TrimEnd()}",
                        string.Empty, MessageBoxButtons.OKCancel) == DialogResult.Cancel) return false;
                }
                return true;
            }
            catch (Exception e) { MessageBox.Show(e.Message); return false; }
        }

        private static List<string> GetPlainFileList(List<HHTypes_Files> hhTypes_files)
        {
            List<string> plainList = new List<string>();
            foreach (HHTypes_Files hhtf in hhTypes_files)
                foreach (string file in hhtf.files)
                    if (!plainList.Contains(file)) plainList.Add(file);
            return plainList;
        }

        private static void StartPresenter(string templateName, List<FilePackageContent> filePackages, List<Template.TemplateInfo.UserVariable> userVariables)
        {
            Dictionary<string, object> args = new Dictionary<string, object>()
            {
                { "templatename", Path.Combine(Program.getHHOTfolder(), templateName) },
                { "userinput", userVariables },
                { "FilePackages", filePackages }
            };
            EM_Statistics.StatisticsPresenter.StatisticsPresenter.Run(args);
        }
    }
}