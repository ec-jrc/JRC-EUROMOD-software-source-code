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
    internal class ExtensionAddContentAction : BaseAction
    {
        string cc, extensionName;
        bool on;
        EM_UI_MainForm mainForm;

        internal ExtensionAddContentAction(string _cc, EM_UI_MainForm _mainForm, string _extensionName, bool _on)
        {
            cc = _cc; mainForm = _mainForm; extensionName = _extensionName; on = _on;
        }

        internal override void PerformAction()
        {
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();
            ExtensionAndGroupMenuManager.GetSelectionPolFunPar(cc, out List<CountryConfig.PolicyRow> polRows, out List<CountryConfig.FunctionRow> funRows, out List<CountryConfig.ParameterRow> parRows);

            string extensionID = (from e in ExtensionAndGroupManager.GetExtensions(cc) where e.Name == extensionName select e.ID).FirstOrDefault(); if (extensionID == null) return;
            bool baseOff = !on;
            foreach (CountryConfig.PolicyRow polRow in polRows)
                if (!(from pe in countryConfig.Extension_Policy where pe.PolicyID == polRow.ID && pe.ExtensionID == extensionID && pe.BaseOff == baseOff select pe).Any())
                {
                    countryConfig.Extension_Policy.AddExtension_PolicyRow(extensionID, polRow, baseOff); // make sure to not add twice (and thus crash)
                    if (polRow.Switch == DefPar.Value.NA) polRow.Switch = DefPar.Value.OFF; // if set to n/a the xml-reader would ignore the policy, before extensions are taken into account
                }
            foreach (CountryConfig.FunctionRow funRow in funRows)
                if (!(from fe in countryConfig.Extension_Function where fe.FunctionID == funRow.ID && fe.ExtensionID == extensionID && fe.BaseOff == baseOff select fe).Any())
                {
                    countryConfig.Extension_Function.AddExtension_FunctionRow(extensionID, funRow, baseOff);
                    if (funRow.Switch == DefPar.Value.NA) funRow.Switch = DefPar.Value.OFF;
                }
            foreach (CountryConfig.ParameterRow parRow in parRows)
                if (!(from pe in countryConfig.Extension_Parameter where pe.ParameterID == parRow.ID &&  pe.ExtensionID == extensionID &&  pe.BaseOff == baseOff select pe).Any())
                    countryConfig.Extension_Parameter.AddExtension_ParameterRow(extensionID, parRow, baseOff);

            if (!on) return;

            // adapt switches in treelist directly, as a full update takes too long
            foreach (CountryConfig.PolicyRow polRow in polRows)
            {
                SetSwitch(polRow.ID, polRow.SystemID, false, DefPar.Value.SWITCH);
                // for functions, which are part of an extension-policy one still needs to show off and n/a instead of switch (see GetExtensionAdaptedSwitch)
                foreach (CountryConfig.FunctionRow funRow in polRow.GetFunctionRows())
                    SetSwitch(funRow.ID, polRow.SystemID, true, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(funRow));
            }
            foreach (CountryConfig.FunctionRow funRow in funRows) SetSwitch(funRow.ID, funRow.PolicyRow.SystemID, true, DefPar.Value.SWITCH);

            void SetSwitch(string rowID, string sysID, bool isFun, string switchValue)
            {
                KeyValuePair<TreeListNode, TreeListColumn> cell = mainForm.GetTreeListBuilder().GetCellByDataRow(rowID, sysID, isFun);
                if (cell.Key != null && cell.Value != null) cell.Key.SetValue(cell.Value, switchValue);
            }
        }
    }
}
