using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    public partial class NumberedSearchResultsForm : Form
    {
        public NumberedSearchResultsForm()
        {
            InitializeComponent();
        }

        public NumberedSearchResultsForm(string t) : this() 
        {
            titleLabel.Text = t;
        }

        internal void setTitle(string t)
        {
            titleLabel.Text = t;
        }

        internal void addResults(List<string> text, List<string> tooltip, List<object> clickData, Func<object, object> callBack)
        {
            resultPanel.Controls.Clear();     // clear any previous results
            for (int i = text.Count()-1; i >= 0; i--)   // add the labels in reverse order as each one is placed on top
            {
                bool dis = clickData[i] == null;     // if row < 0, treat label as disabled 
                NumberedLabel lbl = new NumberedLabel {     // create the label
                    Text = text[i],
                    clickData = clickData[i],
                    Dock = DockStyle.Top, 
                    AutoSize = true, 
                    Cursor = dis ? System.Windows.Forms.Cursors.Default : System.Windows.Forms.Cursors.Hand,
                    ForeColor = dis ? System.Drawing.Color.FromArgb(80,80,80) : System.Drawing.Color.Black
                };
                if (tooltip[i]!=null && tooltip[i]!="") toolTip1.SetToolTip(lbl, tooltip[i]);   // add the tooltip
                if (!dis)
                {
                    lbl.Click += (sender, e) => { callBack(lbl.clickData); };    // on click, call the callBack handler
                    lbl.MouseEnter += (sender, e) => { lbl.ForeColor = System.Drawing.Color.Blue; }; // on hover, paint dark blue
                    lbl.MouseLeave += (sender, e) => { lbl.ForeColor = System.Drawing.Color.Black; }; // on hover, paint dark blue
                }
                resultPanel.Controls.Add(lbl);    // add the label to the scrollable area
            }
        }

        class NumberedLabel : Label     // custom label class that can also hold a row number
        {
            internal object clickData;
        }
    }
}
