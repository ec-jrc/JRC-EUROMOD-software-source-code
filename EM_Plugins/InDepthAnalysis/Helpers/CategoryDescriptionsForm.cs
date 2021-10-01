using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    internal partial class CategoryDescriptionsForm : Form
    {
        internal CategoryDescriptionsForm(Dictionary<double, string> categoryDescriptions)
        {
            InitializeComponent();
            InDepthAnalysis.SetShowHelp(this, helpProvider);

            if (categoryDescriptions != null)
                foreach (var cd in categoryDescriptions) grid.Rows.Add(cd.Key, cd.Value);
        }

        internal Dictionary<double, string> GetDescriptions()
        {
            Dictionary<double, string> descriptions = new Dictionary<double, string>();
            foreach (DataGridViewRow row in grid.Rows)
                if (CheckRow(row, out double v, out string d, out _) && !descriptions.ContainsKey(v)) descriptions.Add(v, d);
            return descriptions;
        }

        internal static Dictionary<double, string> StringToDic(string descriptions)
        {
            Dictionary<double, string> dic = new Dictionary<double, string>();
            if (string.IsNullOrEmpty(descriptions)) return dic;
            foreach (string description in descriptions.Split(';'))
            {
                string[] d = description.Split('='); if (d.Length != 2) continue;
                if (double.TryParse(d[0], out double val) && !dic.ContainsKey(val)) dic.Add(val, d[1]);
            }
            return dic;
        }

        internal static string DicToString(Dictionary<double, string> descriptions)
        {
            string s = string.Empty; if (descriptions == null) return s;
            foreach (var d in descriptions) s += $"{d.Key}={d.Value};";
            return s.Trim(new char[] { ';' });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int i = grid.Rows.Add(string.Empty, string.Empty);
            grid.CurrentCell = grid.Rows[i].Cells[colValue.Index]; grid.BeginEdit(true);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0) grid.Rows.Remove(grid.SelectedRows[0]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string errors = string.Empty; List<double> values = new List<double>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (CheckRow(row, out double value, out _, out string error))
                {
                    if (values.Contains(value)) errors += $"Double definition of value {value}" + Environment.NewLine;
                    else values.Add(value);
                }
                errors += error;
            }
            if (errors != string.Empty) { MessageBox.Show(errors); return; }
            DialogResult = DialogResult.OK; Close();
        }

        private bool CheckRow(DataGridViewRow row, out double value, out string description, out string error)
        {
            value = 0; error = string.Empty;
            string val = row.Cells[colValue.Index].Value?.ToString();
            description = row.Cells[colDescription.Index].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(val) && string.IsNullOrEmpty(description)) return false;
            if (string.IsNullOrEmpty(val)) { error = $"No value defined for '{description}'"; return false; }
            if (!double.TryParse(val, out value)) { error = $"'{val}' is not a valid (double) value"; return false; }
            return true;
        }
    }
}