using System;
using System.Windows.Forms;

namespace StatisticsPresenter
{
    public partial class ShowInfoForm : Form
    {
        public ShowInfoForm() : this("Info", "No info found!") { }
        
        public ShowInfoForm(string caption, string content)
        {
            InitializeComponent();
            Text = caption;
            content = content.Trim();
            if (content.StartsWith("{\\rtf1", StringComparison.Ordinal))
                richTextBox1.Rtf = content;
            else
                richTextBox1.Text = content;
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
