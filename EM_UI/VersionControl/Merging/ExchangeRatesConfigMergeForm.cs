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
    internal partial class ExchangeRatesConfigMergeForm : MergeFormBase
    {
        internal const string EXCHANGERATESCONFIG = "EXCHANGERATESCONFIG";

        MergeAdministrator _mergeAdministrator = null;
        MergeControl _mcExchangeRatesConfig = new MergeControl();


        internal ExchangeRatesConfigMergeForm(MergeAdministrator mergeAdministrator)
        {
            _mergeAdministrator = mergeAdministrator;

            InitializeComponent();

            SetPositionMergeControl(_mcExchangeRatesConfig, tabExchangeRatesConfig);
        }

        void VariablesMergeForm_Load(object sender, EventArgs e) { string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath; }

        void btnClose_Click(object sender, EventArgs e) { this.Close(); }

        void btnSave_Click(object sender, EventArgs e) { _mergeAdministrator.WriteInfoToInfoFiles(); }

        void btnApply_Click(object sender, EventArgs e) { if (_mergeAdministrator.ApplyAcceptedChanges()) this.Close(); }

        internal override bool HasDifferences() { return !_mcExchangeRatesConfig.IsEmpty(); }

        internal override void LoadMergeControl(string mcName, List<string> levelInfo = null) { GetMergeControlByName(mcName).LoadInfo(mcName, levelInfo, _mergeAdministrator._pathLocalVersion, _mergeAdministrator._pathRemoteVersion); }

        internal override MergeControl GetMergeControlByName(string mcName)
        {
            return _mcExchangeRatesConfig;
        }
    }
}
