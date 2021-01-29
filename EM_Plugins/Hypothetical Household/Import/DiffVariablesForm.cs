using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class DiffVariablesForm : Form
    {
        internal DiffVariablesForm(string varName, string masterProject, string importProject,
                                   List<ImportVariablesForm.Difference> differences, Program plugin)
        {
            InitializeComponent();

            labelVarName.Text = varName;
            colMasterProject.HeaderCell.Value = masterProject;
            colImportProject.HeaderCell.Value = importProject;

            foreach (ImportVariablesForm.Difference difference in differences)
            {
                string masterVal = difference.masterVal, importVal = difference.importVal;
                if (difference.description.Contains("DefaultValue")) // get a nicer display for the default value
                {
                    masterVal = GetDefaultValue_DisplayText(masterVal, plugin);
                    importVal = GetDefaultValue_DisplayText(importVal, plugin);
                }
                grid.Rows.Add(difference.description, masterVal, importVal);
            }
        }

        private string GetDefaultValue_DisplayText(string defaultVal, Program plugin)
        {
            string displayText = NumericEditor.GetDisplayText(defaultVal, true, plugin);
            return displayText == string.Empty ? defaultVal : displayText; // if the DefaultValue is in fact no Range, but just a number
        }                                                                  // NumericEditor.GetDisplayText returns an empty string

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
