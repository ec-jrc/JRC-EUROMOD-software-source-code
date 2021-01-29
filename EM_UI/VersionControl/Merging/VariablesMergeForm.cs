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
    internal partial class VariablesMergeForm : MergeFormBase
    {
        internal const string VARIABLES = "VARIABLES";
        internal const string ACRONYMS = "ACRONYMS";
        internal const string COUNTRY_LABELS = "COUNTRY_LABELS";
        internal const string SWITCHABLE_POLICIES = "SWITCHABLE_POLICIES";

        MergeAdministrator _mergeAdministrator = null;
        MergeControl _mcVariables = new MergeControl();
        MergeControl _mcAcronyms = new MergeControl();
        MergeControl _mcCountryLabels = new MergeControl();
        MergeControl _mcSwitchablePolicies = new MergeControl();


        internal VariablesMergeForm(MergeAdministrator mergeAdministrator)
        {
            _mergeAdministrator = mergeAdministrator;

            InitializeComponent();

            SetPositionMergeControl(_mcVariables, tabVariables);
            SetPositionMergeControl(_mcAcronyms, tabAcronyms);
            SetPositionMergeControl(_mcSwitchablePolicies, tabSwitchablePolicies);
            SetPositionMergeControl(_mcCountryLabels, tabCountryLabels);
        }

        void VariablesMergeForm_Load(object sender, EventArgs e) { string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath; }

        void btnClose_Click(object sender, EventArgs e) { this.Close(); }

        void btnSave_Click(object sender, EventArgs e) { _mergeAdministrator.WriteInfoToInfoFiles(); }

        void btnApply_Click(object sender, EventArgs e) { if (_mergeAdministrator.ApplyAcceptedChanges()) this.Close(); }

        internal override bool HasDifferences() { return !_mcVariables.IsEmpty() || !_mcAcronyms.IsEmpty() || !_mcCountryLabels.IsEmpty() || !_mcSwitchablePolicies.IsEmpty(); }

        internal override void LoadMergeControl(string mcName, List<string> levelInfo = null) { GetMergeControlByName(mcName).LoadInfo(mcName, levelInfo, _mergeAdministrator._pathLocalVersion, _mergeAdministrator._pathRemoteVersion); }

        internal override MergeControl GetMergeControlByName(string mcName)
        {
            switch (mcName)
            {
                case VARIABLES: return _mcVariables;
                case ACRONYMS: return _mcAcronyms;
                case SWITCHABLE_POLICIES: return _mcSwitchablePolicies;
                case COUNTRY_LABELS: return _mcCountryLabels;
            }
            return null;
        }
    }
}
