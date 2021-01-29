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
    internal partial class AssignSystemsForm : Form
    {
        const string _labelNotAssigned = "Not Assigned";
        Dictionary<string, string> _systemAssignments = new Dictionary<string, string>();

        void AssignSystemsForm_Load(object sender, EventArgs e)
        {
            helpProvider.HelpNamespace = EM_UI.Tools.PathsHelper.File_EUROMODHelp_chm;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            _systemAssignments.Clear();
            foreach (ListViewItem listViewItem in lvSystemsPasteCountry.Items)
                if (listViewItem.SubItems[1].Tag.ToString() != _labelNotAssigned)
                    _systemAssignments.Add(listViewItem.SubItems[0].Tag.ToString(), listViewItem.SubItems[1].Tag.ToString());
            DialogResult = DialogResult.OK;
            Close();
        }
       
        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnAssign_Click(object sender, EventArgs e)
        {
            if (lvSystemsCopyCountry.SelectedItems.Count == 0 || lvSystemsPasteCountry.SelectedItems.Count == 0)
                return;
            lvSystemsPasteCountry.SelectedItems[0].SubItems[1].Text = lvSystemsCopyCountry.SelectedItems[0].Text;
            lvSystemsPasteCountry.SelectedItems[0].SubItems[1].Tag = lvSystemsCopyCountry.SelectedItems[0].Tag;
        }

        internal AssignSystemsForm(string pasteCountryShortName, Dictionary<string, string> pasteCountrySystems_NamesAndIDs,
                                   string copyCountryShortName, Dictionary<string, string> copyCountrySystems_NamesAndIDs)
        {
            InitializeComponent();
            txCopyCountry.Text = copyCountryShortName;
            txPasteCountry.Text = pasteCountryShortName + " <- " + copyCountryShortName;
            ListViewItem listViewItem = null;
            foreach (string pasteCountrySystemName in pasteCountrySystems_NamesAndIDs.Keys)
            {
                listViewItem = new ListViewItem(new string[] { pasteCountrySystemName, _labelNotAssigned });
                listViewItem.SubItems[0].Tag = pasteCountrySystems_NamesAndIDs[pasteCountrySystemName];
                listViewItem.SubItems[1].Tag = _labelNotAssigned;
                lvSystemsPasteCountry.Items.Add(listViewItem);
            }
            foreach (string copyCountrySystemName in copyCountrySystems_NamesAndIDs.Keys)
            {
                listViewItem = new ListViewItem(new string[] { copyCountrySystemName });
                listViewItem.Tag = copyCountrySystems_NamesAndIDs[copyCountrySystemName];
                lvSystemsCopyCountry.Items.Add(listViewItem);
            }
            listViewItem = new ListViewItem(new string[] { _labelNotAssigned });
            listViewItem.Tag = _labelNotAssigned;
            listViewItem.ToolTipText = "'" + _labelNotAssigned + "' effects that copied policy/function is switched off for this system";
            lvSystemsCopyCountry.Items.Add(listViewItem);
        }

        internal Dictionary<string, string> GetSystemAssignment()
        {
            return _systemAssignments;
        }
    }
}
