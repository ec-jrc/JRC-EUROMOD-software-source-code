using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using EM_Common;
using EM_Common_Win;
using EM_Statistics;

namespace EM_Statistics.StatisticsPresenter
{
    public partial class SelectDefaultTemplateComboForm : Form
    {
        internal Template.TemplateInfo templateInfo = null;
        internal List<FilePackageContent> filePackages = null;
        internal List<Template.TemplateInfo.UserVariable> userInput = null;
        internal Template template = null;
        private string templatePath = "";

        private string givenTemplateName = null;
        private Template givenTemplate = null;
        private List<FilePackageContent> givenFilePackages = null;
        private List<Template.TemplateInfo.UserVariable> givenUserInput = null;

        public SelectDefaultTemplateComboForm(string templateName = null, Template template = null,
                                  List<FilePackageContent> filePackages = null,
                                  List<Template.TemplateInfo.UserVariable> userInput = null)
        {
            InitializeComponent();

            // overtake whatever the caller provides - the dialog (and the sub-dialogs it calls) will only ask for the still missing
            givenTemplateName = templateName;
            givenTemplate = template;
            givenFilePackages = filePackages;
            givenUserInput = userInput;
        }

        private void SelectDefaultTemplateComboForm_Load(object sender, EventArgs e)
        {
            if (givenTemplateName != null || givenTemplate != null) btnOK_Click(null, null);

            // Preload default templates into combobox
            if (Directory.Exists(EnvironmentInfo.GetUserSelectableTemplateFolder()))
            {
                // Try reading only the brand templates
                string[] allTemplates = Directory.GetFiles(EnvironmentInfo.GetUserSelectableTemplateFolder(), DefGeneral.BRAND_NAME + "*.xml");
                // If no brand-specific template is found, load the default EUROMOD templates
                if (allTemplates.Length == 0) allTemplates = Directory.GetFiles(EnvironmentInfo.GetUserSelectableTemplateFolder(), DefGeneral.BRAND_NAME_DEFAULT + "*.xml");

                // try to retieve last selected template from session info
                TSDictionary pluginSessionInfo = UISessionInfo.GetSessionUserSettings(StatisticsPresenter.USER_SETTINGS_ID);
                string lastSelectedTemplate = pluginSessionInfo != null && pluginSessionInfo.ContainsKey(StatisticsPresenter.LAST_SELECTED_TEMPLATE)
                    ? pluginSessionInfo.GetItem<string>(StatisticsPresenter.LAST_SELECTED_TEMPLATE) : null;

                int selectedIndex = 0;
                foreach (string templatePath in allTemplates)
                {
                    if (XML_handling.ParseTemplateInfo(templatePath, out Template.TemplateInfo templateInfo, out ErrorCollector errorCollector))
                    {
                        comboBox1.Items.Add(new TemplateItem(templateInfo.name, templatePath));
                        if (templateInfo.name == lastSelectedTemplate) selectedIndex = comboBox1.Items.Count - 1;
                    }
                }
                if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = selectedIndex;
            }
            else
            {
                MessageBox.Show("The default template folder was not found!");
            }
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            if (!LoadTemplateInfo()) return;

            Visible = false;

            if (givenFilePackages == null) SelectFilePackages();
            else { filePackages = givenFilePackages; DialogResult = DialogResult.OK; }

            if (givenUserInput != null) userInput = givenUserInput;
            else if (DialogResult != DialogResult.Cancel && templateInfo.GetUserVariables().Count != 0)
            {
                TakeUserInput takeUserInput = new TakeUserInput(templateInfo);
                takeUserInput.ShowDialog();
                userInput = takeUserInput.userVariables;
                DialogResult = takeUserInput.DialogResult;
            }

            // if all done, fully parse the selected template
            if (DialogResult == DialogResult.OK)
            {
                if (givenTemplate != null)
                    template = givenTemplate;
                else if (templatePath != "")
                {
                    if (!XML_handling.ParseTemplate(templatePath, out template, out ErrorCollector errorCollector))
                        DialogResult = DialogResult.Cancel;
                    if (errorCollector.HasErrors()) MessageBox.Show(errorCollector.GetErrorMessage());
                }
                else
                {
                    MessageBox.Show("Error! no template or templatepath given!");
                    DialogResult = DialogResult.Cancel;
                }
                if (template != null) template.info = templateInfo;
            }

            if (sender != null && template != null && template.info != null && template.info.name != null)
            {
                // save selected template in session info
                TSDictionary pluginSessionInfo = new TSDictionary();
                pluginSessionInfo.SetItem(StatisticsPresenter.LAST_SELECTED_TEMPLATE, template.info.name);
                UISessionInfo.SetSessionUserSettings(StatisticsPresenter.USER_SETTINGS_ID, pluginSessionInfo);
            }

            Close();
        }

        private void SelectFilePackages()
        {
            // TODO what is this? 
            // temporarily change MULTI_ALT to BASE_ALT if a reference scenario ought to be defined - is changed back (and further treated) after user choice
//            bool refDef = template.TemplateType == EMS_TEMPLATE_TYPE.MULTI_ALT && template.FilePackageDefinition.SpecifyReference;
//            if (refDef) template.TemplateType = EMS_TEMPLATE_TYPE.BASE_ALT;

            if (templateInfo.templateType == HardDefinitions.TemplateType.Default ||
               (templateInfo.templateType == HardDefinitions.TemplateType.Multi))
            { // (simple) selection-dialog, which allows for selecting (one or more) files
                SelectFilesForm form = new SelectFilesForm(templateInfo);
                DialogResult = form.ShowDialog() == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
                filePackages = form.filePackages;
            }
            else if (templateInfo.templateType == HardDefinitions.TemplateType.BaselineReform)
            { // dialog allowing to select one base and several alternatives
                SelectBaseAltsForm form = new SelectBaseAltsForm(templateInfo);
                DialogResult = form.ShowDialog() == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
                filePackages = form.filePackages;
            }

            

            /*
            if (refDef) // MULTI_ALT with a reference scenario: add the "base", i.e. the reference scenario, as the first alternative
            {           // this is what the library needs to make EMS_Template.REFERENCE_ALT work
                foreach (EMS_FilePackageContent filePackage in filePackages)
                {
                    filePackage.PathsAlt.Insert(0, filePackage.PathBase);
                    filePackage.PathBase = string.Empty;
                    template.TemplateType = EMS_TEMPLATE_TYPE.MULTI_ALT;
                }
            }
             */
        }

        private bool LoadTemplateInfo()
        {
            if (givenTemplate != null) { templateInfo = givenTemplate.info; return true; } // nothing to do, if an other caller than the UI already provided a loaded template

            if (givenTemplateName != null) templatePath = givenTemplateName;    // another caller than the UI provided the name of the template to load
            else templatePath = (comboBox1.SelectedItem as TemplateItem).Value; // UI-call

            if (XML_handling.ParseTemplateInfo(templatePath, out templateInfo, out ErrorCollector errorCollector)) return true;

            MessageBox.Show(errorCollector.GetErrorMessage());
            if (givenTemplateName != null)
            {
                DialogResult = DialogResult.Cancel;
                Close(); // terminate the whole selection process if an other caller than the UI provided rubbish input
            }
            return false;
        }

        private void btnDescription_Click(object sender, EventArgs e)
        {
            if (!LoadTemplateInfo()) return;
            if (templateInfo.description == string.Empty) MessageBox.Show("No description available for this template.");
            else
            {
                ShowInfoForm showInfo = new ShowInfoForm(templateInfo.name, templateInfo.description);
                showInfo.ShowDialog(this);
            }
        }

        private class TemplateItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }

            public TemplateItem(string title, string pathTemplate)
            {
                Text = title;
                Value = pathTemplate;
            }
        }
    }
}
