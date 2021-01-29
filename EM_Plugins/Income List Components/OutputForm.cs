using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Income_List_Components
{
    public partial class OutputForm : Form
    {
        internal Program Plugin;                        // varialbe that links to the actual plugin
        internal DataTable resultData = new DataTable();   // holds the displayed results

        public OutputForm(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
        }

        private void OutputForm_Shown(object sender, EventArgs e)
        {
            labelControl2.Visible = true;
            labelControl2.BringToFront();
            btnClose.Visible = false;
            Refresh();
            Plugin.prepareIncomeListInfo();
            calculateResults();
            labelControl2.Visible = false;
            btnClose.Visible = true;
        }

        private void calculateResults()
        {
            resultData.Columns.Add("country", typeof(string));
            resultData.Columns.Add("year", typeof(string));
            resultData.Columns.Add("name", typeof(string));
            resultData.Columns.Add("description", typeof(string));
            resultData.Columns.Add("components", typeof(string));
            resultData.Columns.Add("components description", typeof(string));

            for (int r = 0; r < Plugin.chkData.Rows.Count; r++)
            {
                foreach (DataColumn c in Plugin.chkData.Columns)
                {
                    if (Plugin.chkData.Rows[r].Field<bool>(c))
                    {
                        string country = Plugin.countries[r].shortname;
                        string year = c.ColumnName;
                        foreach (EM_IncomeList il in Plugin.countries[r].systems[c.ColumnName].incomelists.Values)
                        {
                            if (il.name.Substring(0, 3).ToLower() == "ils")   // only output default income lists
                            {
                                resultData.Rows.Add(new object[] {
                                    country,
                                    year,
                                    il.name,
                                    il.description,
                                    il.shortComponentList,
                                    il.longComponentList
                                });
                            }
                        }
                    }
                }
            }

            gridResults.DataSource = resultData;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
