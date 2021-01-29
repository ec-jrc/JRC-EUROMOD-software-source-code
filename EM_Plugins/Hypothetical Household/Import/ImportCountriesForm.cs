using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class ImportCountriesForm : Form
    {
        public ImportCountriesForm(List<string> countries)
        {
            InitializeComponent();

            foreach (string c in countries) listCountries.Items.Add(c, true);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        internal void GetResult(out List<string> accepted, out List<string> notAccepted)
        {
            accepted = new List<string>(); notAccepted = new List<string>();
            foreach (var item in listCountries.Items)
            {
                if (listCountries.CheckedItems.Contains(item)) accepted.Add(item.ToString());
                else notAccepted.Add(item.ToString());
            }
        }
    }
}
