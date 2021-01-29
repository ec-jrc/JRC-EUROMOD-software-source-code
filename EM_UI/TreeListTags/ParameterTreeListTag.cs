using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.Editor;
using EM_UI.ImportExport;
using EM_UI.Validate;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.TreeListTags
{
    internal class ParameterTreeListTag : BaseTreeListTag
    {
        enum PolicyColumnEditable { no_idea, yes, no }; //performance optimisation Aug 13

        Dictionary<string, CountryConfig.ParameterRow> _parameterRows = new Dictionary<string, CountryConfig.ParameterRow>(); //contains one parameter data-row per system
        
        //performance optimisation Aug 13
        PolicyColumnEditable _policyColumnEditable = PolicyColumnEditable.no_idea;

        private RepositoryItemComboBox _categoriesEditor = null;
 
        internal ParameterTreeListTag(EM_UI_MainForm mainForm) : base(mainForm) { }

        RepositoryItemComboBox GetCategoriesEditor()
        {
            if (_categoriesEditor != null)
                return _categoriesEditor;

            _categoriesEditor = new RepositoryItemComboBox();
            _categoriesEditor.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            _categoriesEditor.CloseUpKey = new DevExpress.Utils.KeyShortcut(Keys.Alt | Keys.Down);
            _categoriesEditor.Items.AddRange(DefinitionAdmin.GetParDefinition(GetFunctionName(), GetParameterName()).categValues);
            _categoriesEditor.Items.Add(DefPar.Value.NA);
            _categoriesEditor.MouseWheel += new MouseEventHandler(_categoriesEditor_MouseWheel);
            return _categoriesEditor;
        }

        void _categoriesEditor_MouseWheel(object sender, MouseEventArgs e)
        {
            _mainForm.treeList.Focus(); //set focus to treeList, to avoid changing value of list
        }
        
        internal List<CountryConfig.ParameterRow> GetParameterRows()
        {
            return _parameterRows.Values.ToList();
        }

        internal void AddParameterRowOfSystem(string systemID, CountryConfig.ParameterRow parameterRow)
        {
            _parameterRows.Add(systemID, parameterRow);
        }

        internal CountryConfig.ParameterRow GetParameterRowOfSystem(string systemID)
        {
            return _parameterRows[systemID];
        }
        
        //descriptions of override functions see BaseTreeListTag

        internal override CountryConfig.PolicyRow GetDefaultPolicyRow()
        {
            return GetDefaultFunctionRow().PolicyRow;
        }
        
        internal override CountryConfig.FunctionRow GetDefaultFunctionRow()
        {
            return GetDefaultParameterRow().FunctionRow;
        }

        internal override CountryConfig.ParameterRow GetDefaultParameterRow()
        {
            return _parameterRows.Values.FirstOrDefault<CountryConfig.ParameterRow>();
        }

        internal override int GetOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultParameterRow().Order);
        }

        internal override int GetPolicyOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultParameterRow().FunctionRow.PolicyRow.Order);
        }

        internal override int GetFunctionOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultParameterRow().FunctionRow.Order);
        }

        internal override int GetParameterOrder()
        {
            return GetOrder();
        }

        internal override string GetPolicyName()
        {
            return GetDefaultParameterRow().FunctionRow.PolicyRow.Name;
        }

        internal override string GetFunctionName()
        {
            return GetDefaultParameterRow().FunctionRow.Name;
        }

        internal override string GetParameterName()
        {
            return GetDefaultParameterRow().Name;
        }

        internal override string GetID(string systemID)
        {
            return _parameterRows.ContainsKey(systemID) ? _parameterRows[systemID].ID : "";
        }

        internal override string GetDefaultID()
        {
            return GetDefaultParameterRow().ID;
        }

        internal override string SaveGetDefaultID(string dummyID = "")
        {
            return (_parameterRows.Count == 0) ? dummyID : GetDefaultID();
        }

        internal override List<string> GetIDsWithinAllSystems()
        {
            List<string> ids = new List<string>();
            foreach (CountryConfig.ParameterRow parameterRow in _parameterRows.Values)
                ids.Add(parameterRow.ID);
            return ids;
        }

        internal override string GetPrivateComment()
        {
            string privateComment = GetDefaultParameterRow().PrivateComment;
            return privateComment == null ? string.Empty : privateComment;
        }

        internal override void SetOrder(int order)
        {
            foreach (CountryConfig.ParameterRow parameterRow in _parameterRows.Values)
                parameterRow.Order = order.ToString();
        }

        internal override void SetComment(string comment, bool isPrivate = false)
        {
            foreach (CountryConfig.ParameterRow parameterRow in _parameterRows.Values)
            {
                if (isPrivate)
                    parameterRow.PrivateComment = comment;
                else
                    parameterRow.Comment = comment;
            }
        }

        internal override void SetToNA(string systemID = "")
        {
            foreach (string parameterSystemID in _parameterRows.Keys)
                if (systemID == string.Empty || systemID == parameterSystemID)
                    _parameterRows[parameterSystemID].Value = DefPar.Value.NA;
        }

        internal override void CopySymbolicIdentfierToClipboard()
        {
            System.Windows.Forms.Clipboard.SetText(ImportExportHelper.GetSymbolicID(GetDefaultParameterRow()));
        }

        internal override ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {   //show function context menu also when a parameter is clicked, because it's more convenient and the menu is anyway rather a function/parameter menu
            return _mainForm.GetFunctionContextMenu().GetContextMenu(senderNode.ParentNode); 
        }

        internal override bool IsGroupColumnEditable()
        {
            return true;
        }

        internal override bool IsPolicyColumnEditable()
        {
            if (_policyColumnEditable == PolicyColumnEditable.no_idea) //performance optimisation Aug 13: store editability instead of assessing each time
            {
                if (DefinitionAdmin.GetParDefinition(GetFunctionName(), GetParameterName(), false) != null)
                    _policyColumnEditable = PolicyColumnEditable.no; // a concrete parameter was found (e.g. ArithOp/Formula)
                else _policyColumnEditable = PolicyColumnEditable.yes;
            }
            return _policyColumnEditable == PolicyColumnEditable.yes;
        }

        internal override bool ShowOrderChangeWarning()
        {
            return false;
        }

        internal override bool ValidateEditorInput(string Value, TreeListNode Node, TreeListColumn Column, ref string errorText)
        {
            return ParameterValidation.ValidateEditorInput(Value, Node, Column, ref errorText);
        }

        internal override List<int> GetTypeSpecificIntelliItems(SystemTreeListTag systemTag)
        {
            List<int> specficIntelliItems = null;
            string valType = _parameterRows[systemTag.GetSystemRow().ID].ValueType.ToLower();
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VARorIL).ToLower())
            {
                specficIntelliItems = new List<int>();
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsStandardVar);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsDefVar);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsDefConst);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsDefIL);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsUpRateFactor);
            }
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VAR).ToLower())
            {
                specficIntelliItems = new List<int>();
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsStandardVar);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsDefVar);
                specficIntelliItems.Add(FormulaEditorManager._intelliContainsDefConst);
                if (GetDefaultParameterRow().Name.ToLower() != DefPar.Common.Output_Var.ToLower() &&
                    GetDefaultParameterRow().Name.ToLower() != DefPar.Common.Output_Add_Var.ToLower())
                    specficIntelliItems.Add(FormulaEditorManager._intelliContainsUpRateFactor);
            }

            if (IsDefTUMember()) specficIntelliItems = new List<int>() { FormulaEditorManager._intelliContainsDefTUMembers };

            if (IsUprateFactor()) specficIntelliItems = new List<int>() { FormulaEditorManager._intelliContainsUpRateFactor };

            return specficIntelliItems;
        }

        private bool IsUprateFactor()
        {
            return GetDefaultFunctionRow().Name == DefFun.Uprate &&
                  (GetDefaultParameterRow().Name.ToLower() == DefPar.Uprate.Def_Factor.ToLower() || IsPolicyColumnEditable());
        }

        private bool IsDefTUMember()
        {
            return GetFunctionName().ToLower() == DefFun.DefTu.ToLower() &&
                   GetParameterName().ToLower() == DefPar.DefTu.Members.ToLower();
        }

        internal override RepositoryItem GetEditor(TreeListNode senderNode, TreeListColumn senderColumn)
        {
            RepositoryItem editor = null;

            SystemTreeListTag systemTag = senderColumn.Tag as SystemTreeListTag;
            if (systemTag == null)
                return null;
            if (!_parameterRows.ContainsKey(systemTag.GetSystemRow().ID))
                return null;

            string valType = _parameterRows[systemTag.GetSystemRow().ID].ValueType.ToLower();
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.BOOLEAN).ToLower())
                editor = _mainForm.YesNoEditorCombo;
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.FORMULA).ToLower())
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.CONDITION).ToLower())
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.CATEG).ToLower())
                editor = GetCategoriesEditor();
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.IL).ToLower())
                editor = systemTag.GetILEditor();
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VAR).ToLower())
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.TU).ToLower())
                editor = systemTag.GetTUEditor();
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.TEXT).ToLower())
                if (IsUprateFactor() || IsDefTUMember()) // to allow for suggesting uprating-factors resp. DefTU/Members via intelliSense
                    editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);
            if (valType == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.VARorIL).ToLower())
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);

            //allows free editing or add-on parameters, otherwise e.g. taxunits could not be set, as they may be defined in an other country and not in the add-on-system
            if (_mainForm._isAddOn)
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);
            //special handling for IlArithOp/ILName: showing ils would not offer the ils generated by IlArithOp/Out_ILName
            if (GetFunctionName().ToLower() == DefFun.IlArithOp.ToLower() && GetParameterName().ToLower() == DefPar.IlArithOp.ILName.ToLower())
                editor = _mainForm.GetFormulaEditorManager().CreateEditor(senderNode, senderColumn);

            return editor;
        }

        internal override void StoreChangedValue(string newValue, CountryConfig.SystemRow systemRow)
        {
            _parameterRows[systemRow.ID].Value = newValue;
        }

        internal override void HandleValueChanged(TreeListNode node, TreeListColumn column, string value)
        {
            //check if a change value of a formula or condition requires to add new footnotes (e.g. user selected amount#x1 in itelli -> #x_amount needs to be added)
            Dictionary<KeyValuePair<string, string>, DefinitionAdmin.Par> footnoteParametersToAdd = ParameterValidation.GetFootnoteParametersToAdd(node, ref value);
            ActionGroup editActions = new ActionGroup();
            foreach (KeyValuePair<string, string> footnoteParameterToAdd in footnoteParametersToAdd.Keys)
                editActions.AddAction(new AddParameterAction(node, footnoteParameterToAdd.Key, footnoteParametersToAdd[footnoteParameterToAdd], footnoteParameterToAdd.Value));

            editActions.AddAction(new ChangeParameterValueAction(node, column, value)); //change formula/condition itself

            _mainForm.PerformAction(editActions, //peform all necessary changes
                footnoteParametersToAdd.Count > 0, //update nodes only if parameters added
                false, //no update of columns
                node.ParentNode); //performance optimisation Aug 13: update only the concerned function instead of the whole tree
        }

        internal override string GetValue(string systemID)
        {
            return _parameterRows[systemID].Value;
        }

        internal override bool IsPrivate()
        {
            return GetDefaultParameterRow().Private == DefPar.Value.YES;
        }

        internal override int GetSpecialNodeColor()
        {
            CountryConfig.ParameterRow parameterRow = GetDefaultParameterRow();
            if (parameterRow == null || parameterRow.IsNull(CountryConfigFacade._columnName_Color))
                return DefPar.Value.NO_COLOR;
            return parameterRow.Color;
        }

        internal override void SetSpecialNodeColor(int argbColor)
        {
            foreach (CountryConfig.ParameterRow parameterRow in _parameterRows.Values)
                parameterRow.Color = argbColor;
        }
    }
}
