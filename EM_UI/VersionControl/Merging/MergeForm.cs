using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Merging
{
    internal partial class MergeForm : MergeFormBase
    {
        internal const string SYSTEMS = "SYSTEMS";
        internal const string DATASETS = "DATASETS";
        internal const string SPINE = "SPINE";
        internal const string POLICY_SWITCHES = "POLICY SWITCHES";
        internal const string CONDITIONAL_FORMATTING = "CONDITIONAL FORMATTING";
        internal const string UPRATING_INDICES = "UPRATING_INDICES";
        internal const string INDIRECT_TAXES = "INDIRECT_TAXES";
        internal const string EXTERNAL_STATISTICS = "EXTERNAL_STATISTICS";
        internal const string EXTENSIONS = "EXTENSIONS";
        internal const string EXT_SWITCHES = "EXT_SWITCHES";
        internal const string LOOK_GROUPS = "LOOK_GROUPS";
        internal const string SEPARATOR = "<SEPARATOR>";

        internal const string YEAR_VALUES = "YearValues";
        public static List<string> ALL_COLUMN_NAMES = new List<string> { "Description", "Reference", "YearValues", "Comment", "Category", "Source", "Destination", "TableName" };

        const bool IMAGE_ACCEPT_LOADED = true;

        MergeAdministrator _mergeAdministrator = null;
        MergeControl _mcSystems = new MergeControl();
        MergeControl _mcSpine = new MergeControl();
        MergeControl _mcData = new MergeControl();
        MergeControl _mcCondFormat = new MergeControl();
        MergeControl _mcUpratingIndices = new MergeControl();
        MergeControl _mcIndirectTaxes = new MergeControl();
        MergeControl _mcExternalStatistics = new MergeControl();
        MergeControl _mcExtensions = new MergeControl();
        MergeControl _mcExtSwitches = new MergeControl();
        MergeControl _mcLookGroups = new MergeControl();

        void MergeForm_Load(object sender, EventArgs e) { string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath; }

        internal MergeForm(MergeAdministrator mergeAdministrator)
        {
            _mergeAdministrator = mergeAdministrator;

            InitializeComponent();

            SetPositionMergeControl(_mcSystems, tabSystem);
            SetPositionMergeControl(_mcSpine, tabSpine);
            SetPositionMergeControl(_mcData, tabData);
            SetPositionMergeControl(_mcCondFormat, tabCondForm);
            SetPositionMergeControl(_mcUpratingIndices, tabUpratingIndices);
            SetPositionMergeControl(_mcIndirectTaxes, tabIndirectTaxes);
            SetPositionMergeControl(_mcExternalStatistics, tabExternalStatistics);
            SetPositionMergeControl(_mcExtensions, tabExtensions);
            SetPositionMergeControl(_mcExtSwitches, tabExtSwitches);
            SetPositionMergeControl(_mcLookGroups, tabLookGroups);

        }

        internal override bool HasDifferences()
        {
            return !_mcSystems.IsEmpty() || !_mcSpine.IsEmpty() || !_mcData.IsEmpty() || !_mcCondFormat.IsEmpty() ||
                   !_mcUpratingIndices.IsEmpty() || !_mcIndirectTaxes.IsEmpty() || !_mcExternalStatistics.IsEmpty() || 
                   !_mcExtensions.IsEmpty() || !_mcExtSwitches.IsEmpty() || !_mcLookGroups.IsEmpty()
                   //|| picCountryNameChangedLocal.Visible || picCountryNameChangedRemote.Visible; //would prefer to use this over the conditions below, but obviously the dialog is not yet updated and visible is always false
                   || txtNameLocal.Text != txtNameRemote.Text || chkCountryPrivateLocal.Checked != chkCountryPrivateRemote.Checked;
        }

        internal void SetInfoCountryControls(string nameLocal, string nameRemote, 
                                             bool privateLocal, bool privateRemote,
                                             bool nameChangedLocal, bool nameChangedRemote,
                                             bool privateChangedLocal, bool privateChangedRemote,
                                             bool nameAcceptLocal = true, bool nameAcceptRemote = true,
                                             bool privateAcceptLocal = true, bool privateAcceptRemote = true)
        {
            labCountry.Text = nameLocal;
            txtNameLocal.Text = nameLocal;
            picCountryNameChangedLocal.Visible = picCountryNameAcceptLocal.Visible = nameChangedLocal;
            picCountryNameAcceptLocal.Image = nameAcceptLocal ? EM_UI.Properties.Resources.merge_accept : EM_UI.Properties.Resources.merge_reject;
            picCountryNameAcceptLocal.Tag = nameAcceptLocal;

            txtNameRemote.Text = nameRemote;
            picCountryNameChangedRemote.Visible = picCountryNameAcceptRemote.Visible = nameChangedRemote;
            picCountryNameAcceptRemote.Image = nameAcceptRemote ? EM_UI.Properties.Resources.merge_accept : EM_UI.Properties.Resources.merge_reject;
            picCountryNameAcceptRemote.Tag = nameAcceptRemote;

            chkCountryPrivateLocal.Checked = privateLocal;
            picCountryPrivateChangedLocal.Visible = picCountryPrivateAcceptLocal.Visible = privateChangedLocal;
            picCountryPrivateAcceptLocal.Image = privateAcceptLocal ? EM_UI.Properties.Resources.merge_accept : EM_UI.Properties.Resources.merge_reject;
            picCountryPrivateAcceptLocal.Tag = privateAcceptLocal;

            chkCountryPrivateRemote.Checked = privateRemote;
            picCountryPrivateChangedRemote.Visible = picCountryPrivateAcceptRemote.Visible = privateChangedRemote;
            picCountryPrivateAcceptRemote.Image = privateAcceptRemote ? EM_UI.Properties.Resources.merge_accept : EM_UI.Properties.Resources.merge_reject;
            picCountryPrivateAcceptRemote.Tag = privateAcceptRemote;
        }

        internal void GetInfoCountryControls(out string nameLocal, out string nameRemote,
                                             out bool privateLocal, out bool privateRemote,
                                             out bool nameChangedLocal, out bool nameChangedRemote, out bool privateChangedLocal, out bool privateChangedRemote,
                                             out bool nameAcceptLocal, out bool nameAcceptRemote, out bool privateAcceptLocal, out bool privateAcceptRemote)
        {
            nameLocal = txtNameLocal.Text;
            nameChangedLocal = picCountryNameAcceptLocal.Visible;
            nameAcceptLocal = Convert.ToBoolean(picCountryNameAcceptLocal.Tag) == IMAGE_ACCEPT_LOADED;

            nameRemote = txtNameRemote.Text;
            nameChangedRemote = picCountryNameAcceptRemote.Visible;
            nameAcceptRemote = Convert.ToBoolean(picCountryNameAcceptRemote.Tag) == IMAGE_ACCEPT_LOADED;

            privateLocal = chkCountryPrivateLocal.Checked;
            privateChangedLocal = picCountryPrivateAcceptLocal.Visible;
            privateAcceptLocal = Convert.ToBoolean(picCountryPrivateAcceptLocal.Tag) == IMAGE_ACCEPT_LOADED;

            privateRemote = chkCountryPrivateRemote.Checked;
            privateChangedRemote = picCountryPrivateAcceptRemote.Visible;
            privateAcceptRemote = Convert.ToBoolean(picCountryPrivateAcceptRemote.Tag) == IMAGE_ACCEPT_LOADED;
        }

        internal string StoreCountryControls()
        {
            string nameLocal, nameRemote;
            bool privateLocal, privateRemote;
            bool nameChangedLocal, nameChangedRemote, privateChangedLocal, privateChangedRemote;
            bool nameAcceptLocal, nameAcceptRemote, privateAcceptLocal, privateAcceptRemote;
            GetInfoCountryControls(out nameLocal, out nameRemote,
                                   out privateLocal, out privateRemote,
                                   out nameChangedLocal, out nameChangedRemote, out privateChangedLocal, out privateChangedRemote,
                                   out nameAcceptLocal, out nameAcceptRemote, out privateAcceptLocal, out privateAcceptRemote);
            return nameLocal + SEPARATOR + nameRemote + SEPARATOR + 
                privateLocal + SEPARATOR + privateRemote + SEPARATOR +
                nameChangedLocal + SEPARATOR + nameChangedRemote + SEPARATOR +
                privateChangedLocal + SEPARATOR + privateChangedRemote + SEPARATOR + 
                nameAcceptLocal + SEPARATOR + nameAcceptRemote + SEPARATOR +
                privateAcceptLocal + SEPARATOR + privateAcceptRemote;
        }

        internal void RestoreCountryControls(string content)
        {
            string[] elements = content.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
            if (elements.Count() != 12)
                return;
            SetInfoCountryControls(elements[0], elements[1],
                                   elements[2].ToLower() == "true", elements[3].ToLower() == "true",
                                   elements[4].ToLower() == "true", elements[5].ToLower() == "true",
                                   elements[6].ToLower() == "true", elements[7].ToLower() == "true",
                                   elements[8].ToLower() == "true", elements[9].ToLower() == "true",
                                   elements[10].ToLower() == "true", elements[11].ToLower() == "true");
        }

        internal override MergeControl GetMergeControlByName(string mcName)
        {
            switch (mcName)
            {
                case SYSTEMS: return _mcSystems;
                case DATASETS: return _mcData;
                case SPINE: return _mcSpine;
                case CONDITIONAL_FORMATTING: return _mcCondFormat;
                case UPRATING_INDICES: return _mcUpratingIndices;
                case INDIRECT_TAXES: return _mcIndirectTaxes;
                case EXTERNAL_STATISTICS: return _mcExternalStatistics;
                case EXTENSIONS: return _mcExtensions;
                case EXT_SWITCHES: return _mcExtSwitches;
                case LOOK_GROUPS: return _mcLookGroups;
            }
            return null;
        }

        internal override void LoadMergeControl(string mcName, List<string> levelInfo = null)
        {
            GetMergeControlByName(mcName).LoadInfo(mcName, levelInfo, _mergeAdministrator._pathLocalVersion, _mergeAdministrator._pathRemoteVersion);

            if (mcName == SPINE && GetMergeControlByName(mcName).GetNodeInfoLocal().Count == 0)
                btnApply.Enabled = btnSave.Enabled = false; //if there are no elements (most likely because there are no systems with matching ids)
        }

        void btnClose_Click(object sender, EventArgs e) { this.Close(); }

        void btnSave_Click(object sender, EventArgs e) { _mergeAdministrator.WriteInfoToInfoFiles(); }

        void btnApply_Click(object sender, EventArgs e) { if (_mergeAdministrator.ApplyAcceptedChanges()) this.Close(); }

        void miAccept_Click(object sender, EventArgs e) { AcceptReject_CountryNameOrPrivate(sender, true); }
        void miReject_Click(object sender, EventArgs e) { AcceptReject_CountryNameOrPrivate(sender, false); }
        void AcceptReject_CountryNameOrPrivate(object sender, bool accept)
        {
            if (sender == null) return;
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;
            ContextMenuStrip menu = menuItem.Owner as ContextMenuStrip;
            if (menu == null || menu.SourceControl == null) return;
            PictureBox picBox = menu.SourceControl as PictureBox;
            if (picBox == null)
                return;
            picBox.Image = accept ? EM_UI.Properties.Resources.merge_accept : EM_UI.Properties.Resources.merge_reject;
            picBox.Tag = accept;
        }

        void MergeForm_Shown(object sender, System.EventArgs e)
        {
            if (tabControl.TabPages.Count < 5) return;
            if (!_mcSystems.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[0];
            else if (!_mcSpine.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[1];
            else if (!_mcData.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[2];
            else if (!_mcCondFormat.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[3];
            else if (!_mcUpratingIndices.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[4];
            else if (!_mcIndirectTaxes.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[5];
            else if (!_mcExternalStatistics.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[6];
            else if (!_mcExtensions.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[7];
            else if (!_mcExtSwitches.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[8];
            else if (!_mcLookGroups.IsEmpty()) tabControl.SelectedTab = tabControl.TabPages[9];
            else tabControl.SelectedTab = tabControl.TabPages[0];
        }
    }
}
