using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.ImportExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.TreeListTags
{
    internal class PolicyTreeListTag : BaseTreeListTag
    {
        Dictionary<string, CountryConfig.PolicyRow> _policyRows = new Dictionary<string, CountryConfig.PolicyRow>(); //contains one policy per for each system

        internal PolicyTreeListTag(EM_UI_MainForm mainForm) : base(mainForm) { }

        internal void AddPolicyRowOfSystem(string systemID, CountryConfig.PolicyRow policyRow)
        {
            _policyRows.Add(systemID, policyRow);
        }

        internal List<CountryConfig.PolicyRow> GetPolicyRows()
        {
            return _policyRows.Values.ToList();
        }

        internal CountryConfig.PolicyRow GetPolicyRowOfSystem(string systemID)
        {
            return _policyRows[systemID];
        }

        internal static int GetStateImageIndex(CountryConfig.PolicyRow policyRow)
        {
            if (policyRow.ReferencePolID == null || policyRow.ReferencePolID == string.Empty)
                return policyRow.Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_POL : DefGeneral.IMAGE_IND_POL;
            else
                return policyRow.Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_REF : DefGeneral.IMAGE_IND_REF;
        }

        internal static bool IsReferencePolicy(TreeListNode node)
        {
            return node.StateImageIndex == DefGeneral.IMAGE_IND_REF || node.StateImageIndex == DefGeneral.IMAGE_IND_PRIV_REF;
        }

        //descriptions of override functions see BaseTreeListTag

        internal override CountryConfig.PolicyRow GetDefaultPolicyRow()
        {
            return _policyRows.Values.FirstOrDefault<CountryConfig.PolicyRow>();
        }

        internal override int GetOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultPolicyRow().Order);
        }

        internal override int GetPolicyOrder()
        {
            return GetOrder();
        }

        internal override string GetPolicyName()
        {
            return GetDefaultPolicyRow().Name;
        }

        internal override string GetID(string systemID)
        {
            return _policyRows[systemID].ID;
        }

        internal override string GetDefaultID()
        {
            return GetDefaultPolicyRow().ID;
        }

        internal override string SaveGetDefaultID(string dummyID = "")
        {
            return (_policyRows.Count == 0) ? dummyID : GetDefaultID();
        }

        internal override List<string> GetIDsWithinAllSystems()
        {
            List<string> ids = new List<string>();
            foreach (CountryConfig.PolicyRow policyRow in _policyRows.Values)
                ids.Add(policyRow.ID);
            return ids;
        }

        internal override string GetPrivateComment()
        {
            string privateComment = GetDefaultPolicyRow().PrivateComment;
            return privateComment == null ? string.Empty : privateComment;
        }

        internal override void SetOrder(int order)
        {
            foreach (CountryConfig.PolicyRow policyRow in _policyRows.Values)
                policyRow.Order = order.ToString();
        }

        internal override void SetComment(string comment, bool isPrivate = false)
        {
            foreach (CountryConfig.PolicyRow policyRow in _policyRows.Values)
            {
                if (isPrivate)
                    policyRow.PrivateComment = comment;
                else
                    policyRow.Comment = comment;
            }   
        }

        internal override void SetToNA(string systemID = "")
        {
            foreach (string policySystemID in _policyRows.Keys)
                if (systemID == string.Empty || systemID == policySystemID)
                    _policyRows[policySystemID].Switch = DefPar.Value.NA;
        }

        internal override void CopySymbolicIdentfierToClipboard()
        {
            System.Windows.Forms.Clipboard.SetText(ImportExportHelper.GetSymbolicID(GetDefaultPolicyRow()));
        }

        internal override System.Drawing.Color GetBackColor()
        {
            return System.Drawing.Color.FromArgb(234, 244, 253);
        }

        internal override System.Drawing.Font GetFont(System.Drawing.Font originalFont)
        {
            originalFont = new System.Drawing.Font(originalFont, System.Drawing.FontStyle.Bold);
            return originalFont;
        }

        internal override ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {
            return _mainForm.GetPolicyContextMenu().GetContextMenu(senderNode);
        }

        internal override RepositoryItem GetEditor(TreeListNode senderNode, TreeListColumn senderColumn)
        {
            try
            {
                //assess whether the policy is switchable for the specific system, i.e. is controlled via the switches-dialog and the run-tool
                if (ExtensionAndGroupManager.ShowExtensionSwitchEditor(_policyRows[(senderColumn.Tag as SystemTreeListTag).GetSystemRow().ID]))
                    return GetSwitchEditor();
            }
            catch (Exception exception) { Tools.UserInfoHandler.RecordIgnoredException("PolicyTreeListTag.GetEditor", exception); }
            return _mainForm.OnOffToggleEditor; //if no return the (standard) combobox, which allows selecting the switch (on, off, ...)
        }

        internal override void StoreChangedValue(string newValue, CountryConfig.SystemRow systemRow)
        {
            _policyRows[systemRow.ID].Switch = newValue;
        }

        internal override bool IsPrivate()
        {
            string isPrivate = GetDefaultPolicyRow().Private;
            return (isPrivate == DefPar.Value.YES);
        }

        internal override string GetValue(string systemID)
        {
            return _policyRows[systemID].Switch;
        }

        internal override int GetSpecialNodeColor()
        {
            CountryConfig.PolicyRow policyRow = GetDefaultPolicyRow();
            if (policyRow == null || policyRow.IsNull(CountryConfigFacade._columnName_Color))
                return DefPar.Value.NO_COLOR;
            return policyRow.Color;
        }

        internal override void SetSpecialNodeColor(int argbColor)
        {
            foreach (CountryConfig.PolicyRow policyRow in _policyRows.Values)
                policyRow.Color = argbColor;
        }
    }
}
