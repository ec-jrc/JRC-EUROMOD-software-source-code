using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.Dialogs.Tools
{
    public partial class ChooseStatistics : Form
    {
        public enum SelectionType { None, StatisticsPresenter, InDepthAnalysis }
        internal SelectionType selection = SelectionType.None; 

        public ChooseStatistics()
        {
            InitializeComponent();
        }

        private void btnStatisticsPresenter_Click(object sender, EventArgs e)
        {
            Close();
            selection = SelectionType.StatisticsPresenter;
        }

        private void btnInDepth_Click(object sender, EventArgs e)
        {
            Close();
            selection = SelectionType.InDepthAnalysis;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
