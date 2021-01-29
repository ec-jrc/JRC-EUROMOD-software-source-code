using EM_Common;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StatisticsPresenter
{
    class StatisticsPresenter : PiInterface
    {
        internal const string USER_SETTINGS_ID = "StatisticsPresenter";
        internal const string BASE_OUTPUT_FOLDER = "BaseOutputFolder"; // retained, project specific user setting, stored in UI-user-settings
        internal const string REFORM_OUTPUT_FOLDER = "ReformOutputFolder"; // retained, project specific user setting, stored in UI-user-settings
        internal const string LAST_SELECTED_TEMPLATE = "LastSelectedTemplate"; // session user setting

        public override string GetTitle()
        {
            return "Statistics Presenter";
        }

        public override string GetDescription()
        {
            return "This Plug-in allows selecting and presenting EUROMOD Statistics.";
        }

        public override string GetFullFileName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public override bool IsWebApplicable()
        {
            return false;
        }

        public override void Run(Dictionary<string, object> arguments = null)
        {
            try
            {
                EM_TemplateCalculator.WarmUp(); // warms up the library in a parallel thread to avoid a 2 second delay when running the first dynamic calculation

                string templateName = null;
                Template template = null;
                List<FilePackageContent> filePackages = null;
                List<Template.TemplateInfo.UserVariable> userInput = null;

                if (arguments != null)
                {
                    string errors = string.Empty;
                    foreach (var argument in arguments)
                    {
                        if (argument.Key.ToLower() == "templatename") templateName = GetArgument<string>(argument, ref errors);
                        else if (argument.Key.ToLower() == "template") template = GetArgument<Template>(argument, ref errors);
                        else if (argument.Key.ToLower() == "filepackages") filePackages = GetArgument<List<FilePackageContent>>(argument, ref errors);
                        else if (argument.Key.ToLower() == "userinput") userInput = GetArgument<List<Template.TemplateInfo.UserVariable>>(argument, ref errors);
                        else errors += "Unknown argument: " + argument.Key + Environment.NewLine;
                    }

                    if (errors != string.Empty)
                    {
                        MessageBox.Show(GetTitle() + " was called with invalid argument(s):" + Environment.NewLine +
                            "Valid arguments are:" + Environment.NewLine +
                            "TemplateName (string)" + Environment.NewLine +
                            "Template (EMS_Template)" + Environment.NewLine +
                            "FilePackages (List<EMS_FilePackageContent>)" + Environment.NewLine +
                            "UserInput (List<EMS_UserInput>)" + Environment.NewLine +
                            "RunTimeCaptions (List<EMS_RunTimeCaption>)" + Environment.NewLine +
                            Environment.NewLine + "Error(s):" + Environment.NewLine + errors);
                        return;
                    }
                }
                else
                {
                    // if there is only one template in the UserSelectableTemplates folder then open this by default, otherwise let user chose
                    var templates = Directory.EnumerateFiles(EnvironmentInfo.GetUserSelectableTemplateFolder(), "*.xml");
                    if (templates.Count() == 1) templateName = templates.First();
                }

                SelectDefaultTemplateComboForm form = new SelectDefaultTemplateComboForm(templateName, template, filePackages, userInput);
                if (form.ShowDialog() == DialogResult.Cancel) return;

                PresenterForm presenter = new PresenterForm(form.template, form.filePackages, form.userInput);
                presenter.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Failed to run plug-in '{GetTitle()}':{Environment.NewLine}{exception.Message}");
            }

            T GetArgument<T>(KeyValuePair<string, object> argument, ref string errors)
            {
                if (argument.Value is T == true) return (T)argument.Value;
                errors += "Type of argument '" + argument.Key + "' is invalid." + Environment.NewLine;
                return default(T);
            }
        }
    }
}
