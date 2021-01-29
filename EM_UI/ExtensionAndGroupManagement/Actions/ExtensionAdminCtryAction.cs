using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionAdminCtryAction : BaseAction
    {
        string cc;
        EM_UI_MainForm mainForm;
        bool actionIsCanceled = false;

        List<CountryConfig.PolicyRow> delExPolRows = new List<CountryConfig.PolicyRow>();
        List<CountryConfig.FunctionRow> delExFunRows = new List<CountryConfig.FunctionRow>();

        internal ExtensionAdminCtryAction(string _cc, EM_UI_MainForm _mainForm)
        {
            cc = _cc; mainForm = _mainForm;
        }

        internal override bool ActionIsCanceled()
        {
            return actionIsCanceled;
        }

        internal override void PerformAction()
        {
            DataConfig dataConfig = CountryAdministrator.GetDataConfigFacade(cc).GetDataConfig();
            List<ExtensionOrGroup> extensions = new List<ExtensionOrGroup>();
            foreach (DataConfig.ExtensionRow ext in from e in dataConfig.Extension select e) extensions.Add(new ExtensionOrGroup(ext));

            using (AdminExtensionsOrGroupsForm adminDialog = new AdminExtensionsOrGroupsForm("Administrate Country Specific Extensions", extensions))
            {
                if (adminDialog.ShowDialog() == DialogResult.Cancel) { actionIsCanceled = true; return; }

                foreach (ExtensionOrGroup addEx in adminDialog.added)
                    dataConfig.Extension.AddExtensionRow(Guid.NewGuid().ToString(), addEx.name, addEx.shortName, addEx.look.ToXml());

                foreach (ExtensionOrGroup changeEx in adminDialog.changed)
                {
                    DataConfig.ExtensionRow exRow = (from e in dataConfig.Extension where e.ID == changeEx.id select e).First();
                    exRow.Name = changeEx.name; exRow.ShortName = changeEx.shortName; exRow.Look = changeEx.look.ToXml();
                }

                List<DataConfig.ExtensionRow> ex = (from e in dataConfig.Extension select e).ToList();
                for (int i = ex.Count - 1; i >= 0; --i)
                    if (adminDialog.deletedIds.Contains(ex[i].ID)) ex[i].Delete();

                // prepare for DoAfterCommitWork (updating tree)
                CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();
                foreach (string polID in from ep in countryConfig.Extension_Policy where adminDialog.deletedIds.Contains(ep.ExtensionID) select ep.PolicyID)
                    delExPolRows.Add((from p in countryConfig.Policy where p.ID == polID select p).FirstOrDefault());
                foreach (string funID in from ef in countryConfig.Extension_Function where adminDialog.deletedIds.Contains(ef.ExtensionID) select ef.FunctionID)
                    delExFunRows.Add((from f in countryConfig.Function where f.ID == funID select f).FirstOrDefault());

                // deleteExtensionContent
                List<CountryConfig.Extension_PolicyRow> delPolicyRows = new List<CountryConfig.Extension_PolicyRow>();
                List<CountryConfig.Extension_FunctionRow> delFunctionRows = new List<CountryConfig.Extension_FunctionRow>();
                List<CountryConfig.Extension_ParameterRow> delParameterRows = new List<CountryConfig.Extension_ParameterRow>();
                foreach (string extensionID in adminDialog.deletedIds)
                {
                    delPolicyRows.AddRange((from pe in countryConfig.Extension_Policy where pe.ExtensionID == extensionID select pe).ToList());
                    delFunctionRows.AddRange((from fe in countryConfig.Extension_Function where fe.ExtensionID == extensionID select fe).ToList());
                    delParameterRows.AddRange((from pe in countryConfig.Extension_Parameter where pe.ExtensionID == extensionID select pe).ToList());
                }
                for (int i = delPolicyRows.Count - 1; i >= 0; --i) delPolicyRows[i].Delete();
                for (int i = delFunctionRows.Count - 1; i >= 0; --i) delFunctionRows[i].Delete();
                for (int i = delParameterRows.Count - 1; i >= 0; --i) delParameterRows[i].Delete();

                
            }
        }

        internal override void DoAfterCommitWork() // adapt spine switches for deleted extensions (directly in tree, as a full update takes too long)
        {
            foreach (CountryConfig.PolicyRow polRow in delExPolRows)
            {
                if (polRow == null) continue;
                SetSwitch(polRow.ID, polRow.SystemID, false, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(polRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(polRow));
                foreach (CountryConfig.FunctionRow funRow in polRow.GetFunctionRows())
                    SetSwitch(funRow.ID, polRow.SystemID, true, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(funRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(funRow));
            }
            foreach (CountryConfig.FunctionRow funRow in delExFunRows)
            {
                if (funRow == null) continue;
                SetSwitch(funRow.ID, funRow.PolicyRow.SystemID, true, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(funRow), !ExtensionAndGroupManager.ShowExtensionSwitchEditor(funRow));
            }

            void SetSwitch(string rowID, string sysID, bool isFun, string origSwitch, bool changeOldStyleXmlSwitchToToggle)
            {
                KeyValuePair<TreeListNode, TreeListColumn> cell = mainForm.GetTreeListBuilder().GetCellByDataRow(rowID, sysID, isFun);
                if (cell.Key != null && cell.Value != null) cell.Key.SetValue(cell.Value,
                    changeOldStyleXmlSwitchToToggle && origSwitch == DefPar.Value.SWITCH ? DefPar.Value.TOGGLE : origSwitch);
            }
        }
    }
}
