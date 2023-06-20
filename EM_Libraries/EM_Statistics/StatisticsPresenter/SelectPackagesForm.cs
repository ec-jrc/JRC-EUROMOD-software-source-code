using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_Statistics.StatisticsPresenter
{
    public partial class SelectPackagesForm : Form
    {
        internal Template.TemplateInfo templateInfo = null;
        internal List<FilePackageContent> filePackages = null;
        List<int> selectedIndices = new List<int>();    // store the order of selection for Alts

        public SelectPackagesForm(Template.TemplateInfo templateInfo)
        {
            InitializeComponent();
            this.templateInfo = templateInfo;
            labCaption.Text = templateInfo.name;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //int min, max; template.FilePackageDefinition.GetMinMaxNumberOfPackages(out min, out max, template.TemplateType);
            int selCount = listPackages.Items.Count;
            if (selCount < templateInfo.minFiles || selCount > templateInfo.maxFiles)
            {
                if (templateInfo.minFiles == templateInfo.maxFiles) MessageBox.Show(string.Format("Please select {0} file packages.", templateInfo.maxFiles));
                else if (templateInfo.maxFiles == int.MaxValue) MessageBox.Show(string.Format("Please select at least {0} file packages.", templateInfo.minFiles));
                else MessageBox.Show(string.Format("Please select {0} to {1} file packages.", templateInfo.minFiles, templateInfo.maxFiles));
                return;
            }

            filePackages = new List<FilePackageContent>();
    //            foreach (var item in selectedIndices.Select(i => listPackages.Items[i])) filePackages.Add((item as FilePackage_ListItem).FilePackage);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
/*            if (template.templateType == EMS_TEMPLATE_TYPE.BASE_ALT)
            {
                SelectBaseAltsForm form = new SelectBaseAltsForm(template);
                if (form.ShowDialog() == DialogResult.Cancel) return;
                listPackages.Items.Add(new FilePackage_ListItem(template.TemplateType) { FilePackage = form.filePackages[0] });
            }
            else if (template.TemplateType == EMS_TEMPLATE_TYPE.MULTI_ALT)
            {
                SelectFilesForm form = new SelectFilesForm(template);
                if (form.ShowDialog() == DialogResult.Cancel) return;
                listPackages.Items.Add(new FilePackage_ListItem(template.TemplateType) { FilePackage = form.filePackages[0] });
            }
*/        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            for (int i = listPackages.SelectedItems.Count - 1; i >= 0; --i)
                listPackages.Items.Remove(listPackages.SelectedItems[i]);
        }

        private void listPackages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) buttonDelete_Click(null, null);
        }

        /*
        private class FilePackage_ListItem
        {
            internal FilePackage_ListItem(HardDefinitions.TemplateType templateType) { template.templateType = templateType; }

            internal FilePackageContent FilePackage { get { return filePackage; } set { if (value != null) filePackage = value; } }
            private FilePackageContent filePackage = new FilePackageContent();

            public override string ToString()
            {
                if (template.templateType == HardDefinitions.TemplateType.BaselineReform)
                    return string.Format("'{0}' + ...", filePackage.PathBase);
                else if (template.templateType == HardDefinitions.TemplateType.Multi)
                    return filePackage.PathsAlt.Count == 0 ? "Empty Package?" : string.Format("'{0}' + ...", filePackage.PathsAlt[0]);
                else return string.Format("Unkown template type '{0}'.", template.templateType);
            }
        }
        */

        private void listPackages_SizeChanged(object sender, EventArgs e)
        {
            lblMultiSelect.Top = listPackages.Top + listPackages.Height;
        }

        private void listPackages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listPackages.SelectedIndex > -1)
            {
                selectedIndices.AddRange(listPackages.SelectedIndices.OfType<int>()
                                                 .Except(selectedIndices));
                selectedIndices.RemoveRange(0, selectedIndices.Count - listPackages.SelectedItems.Count);
            }
        }
    }
}
