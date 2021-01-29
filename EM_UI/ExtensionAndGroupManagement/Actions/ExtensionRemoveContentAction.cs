using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionRemoveContentAction : BaseAction
    {
        string cc, extensionName;
        EM_UI_MainForm mainForm;
        List<CountryConfig.PolicyRow> selectedPolRows = new List<CountryConfig.PolicyRow>();
        List<CountryConfig.FunctionRow> selectedFunRows = new List<CountryConfig.FunctionRow>();

        internal ExtensionRemoveContentAction(string _cc, EM_UI_MainForm _mainForm, string _extensionName)
        {
            cc = _cc;  extensionName = _extensionName; mainForm = _mainForm;
        }

        internal override void PerformAction()
        {
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();
            ExtensionAndGroupMenuManager.GetSelectionPolFunPar(cc, out selectedPolRows, out selectedFunRows, out List<CountryConfig.ParameterRow> parRows);
            string extensionID = (from e in ExtensionAndGroupManager.GetExtensions(cc) where e.Name == extensionName select e.ID).FirstOrDefault(); if (extensionID == null) return;

            List<CountryConfig.Extension_PolicyRow> delPolicyRows = new List<CountryConfig.Extension_PolicyRow>();
            List<CountryConfig.Extension_FunctionRow> delFunctionRows = new List<CountryConfig.Extension_FunctionRow>();
            List<CountryConfig.Extension_ParameterRow> delParameterRows = new List<CountryConfig.Extension_ParameterRow>();

            foreach (CountryConfig.PolicyRow policyRow in selectedPolRows)
            {
                selectedFunRows.AddRange(policyRow.GetFunctionRows()); // to make sure that all functions of the policy are removed as well - note: the AddContent-function
                                                               // does not remove a single function if its parent-policy is added (later)
                foreach (CountryConfig.Extension_PolicyRow del in from pe in countryConfig.Extension_Policy where pe.ExtensionID == extensionID & pe.PolicyID == policyRow.ID select pe)
                    if (!delPolicyRows.Contains(del)) delPolicyRows.Add(del); // theoretically (but stupidly) this can be 2 (off and on)
            }
            foreach (CountryConfig.FunctionRow functionRow in selectedFunRows)
            {
                parRows.AddRange(functionRow.GetParameterRows()); // see above
                foreach (CountryConfig.Extension_FunctionRow del in from fe in countryConfig.Extension_Function where fe.ExtensionID == extensionID & fe.FunctionID == functionRow.ID select fe)
                    if (!delFunctionRows.Contains(del)) delFunctionRows.Add(del);
            }
            foreach (CountryConfig.ParameterRow parameterRow in parRows)
            {
                foreach (CountryConfig.Extension_ParameterRow del in from pe in countryConfig.Extension_Parameter where pe.ExtensionID == extensionID & pe.ParameterID == parameterRow.ID select pe)
                    if (!delParameterRows.Contains(del)) delParameterRows.Add(del);
            }
            for (int i = delPolicyRows.Count - 1; i >= 0; --i) delPolicyRows[i].Delete();
            for (int i = delFunctionRows.Count - 1; i >= 0; --i) delFunctionRows[i].Delete();
            for (int i = delParameterRows.Count - 1; i >= 0; --i) delParameterRows[i].Delete();
        }

        internal override void DoAfterCommitWork() // adapt in switches treelist directly, as a full update takes too long
        {
            foreach (CountryConfig.PolicyRow polRow in selectedPolRows)
            {
                SetSwitch(polRow.ID, polRow.SystemID, false, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(polRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(polRow));
                foreach (CountryConfig.FunctionRow funRow in polRow.GetFunctionRows())
                    SetSwitch(funRow.ID, polRow.SystemID, true, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(funRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(funRow));
            }
            foreach (CountryConfig.FunctionRow funRow in selectedFunRows)
                SetSwitch(funRow.ID, funRow.PolicyRow.SystemID, true, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(funRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(funRow));

            void SetSwitch(string rowID, string sysID, bool isFun, string origSwitch, bool changeOldStyleXmlSwitchToToggle)
            {
                KeyValuePair<TreeListNode, TreeListColumn> cell = mainForm.GetTreeListBuilder().GetCellByDataRow(rowID, sysID, isFun);
                if (cell.Key != null && cell.Value != null) cell.Key.SetValue(cell.Value,
                    changeOldStyleXmlSwitchToToggle && origSwitch == DefPar.Value.SWITCH ? DefPar.Value.TOGGLE : origSwitch);
            }
        }
    }
}
