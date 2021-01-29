using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    internal partial class SelectHHTypesForm : Form
    {
        private Template.TemplateInfo templateInfo = null;
        internal List<int> selectedHHTypesIndices = null;

        public SelectHHTypesForm(Template.TemplateInfo _templateInfo, List<string> hhTypes)
        {
            InitializeComponent();

            templateInfo = _templateInfo;
            labCaption.Text = templateInfo.name;

            foreach (string hhType in hhTypes) listHHTypes.Items.Add(hhType);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listHHTypes.CheckedIndices.Count == 0) { MessageBox.Show("Please select at least one Household Type"); return; }

            selectedHHTypesIndices = new List<int>();
            foreach (int index in listHHTypes.CheckedIndices) selectedHHTypesIndices.Add(index);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
