using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.ExtensionAndGroupManagement;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.TreeListTags
{
    internal abstract class BaseTreeListTag
    {
        protected EM_UI_MainForm _mainForm = null;

        internal BaseTreeListTag(EM_UI_MainForm mainForm) { _mainForm = mainForm; }

        //default means that no system (i.e column) is required - simply takes a random (usually the first) system
        internal virtual CountryConfig.PolicyRow GetDefaultPolicyRow() { return null; } //if policy: policy's data-row, if function: data-row of policy the function belongs to, if parameter: data-row of policy the parameter belongs to
        internal virtual CountryConfig.FunctionRow GetDefaultFunctionRow() { return null; } //if policy: null, if function: function's data-row, if parameter: data-row of function the parameter belongs to
        internal virtual CountryConfig.ParameterRow GetDefaultParameterRow() { return null; } //if policy: null, if function: null, if parameter: parameter's data-row

        internal abstract int GetOrder(); //get policy's order within spine / function's order within policy / parameter's order within function, note: must be equal for all systems
        internal abstract int GetPolicyOrder(); //if policy: policy's order, if function: order of policy the function belongs to, if parameter: order of policy the parameter belongs to
        internal virtual int GetFunctionOrder() { return -1; } //if policy: -1, if function: function's order, if parameter: order of function the parameter belongs to
        internal virtual int GetParameterOrder() { return -1; } //if policy: -1, if function: -1, if parameter: parameter's order
        internal abstract string GetValue(string systemID); //if policy or function: get the value of respectiv switch, if paraemter: get respective value

        internal abstract string GetPolicyName(); //if policy: policy's name, if function: name of policy the function belongs to, if parameter: name of policy the parameter belongs to, note: must be equal for all systems
        internal virtual string GetFunctionName() { return null; } //if policy: null, if function: function's name, if parameter: name of function the parameter belongs to
        internal virtual string GetParameterName() { return null; } //if policy: null, if function: null, if parameter: parameter's name

        internal abstract string GetID(string systemID); //get policy's / function's / parameter's ID, note: ID is not!!! equal for all systems
        internal abstract string GetDefaultID(); //default means simply taking a random (usually the first) system, advantage: no indication of a system is required
        internal abstract string SaveGetDefaultID(string dummyID = ""); //same as above, but does not crash if there is no system
        internal abstract List<string> GetIDsWithinAllSystems();

        internal abstract string GetPrivateComment();

        internal abstract void CopySymbolicIdentfierToClipboard(); //copy the symbolic identifier of a policy (=policy name), function (e.g. yse_#3) or parameter (e.g. yse_#3.4) to the clipboard
        internal virtual void CopyIdentfierToClipboard() //copy the real (system specific) identifier of a policy, function or parameter to the clipboard
        {
            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(_mainForm.GetCountryShortName(), null, true);
            selectSystemsForm.SetSingleSelectionMode();
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;
            if (selectSystemsForm.IsAllSystemsSelected())
            {
                string clipboardText = string.Empty;
                foreach (TreeListColumn column in (from col in _mainForm.GetTreeListBuilder().GetSystemColums()
                                                   where col.VisibleIndex >= 0 select col).OrderBy(col => col.VisibleIndex))
                    clipboardText += GetID((column.Tag as SystemTreeListTag).GetSystemRow().ID) + "\t"; ;
                clipboardText = clipboardText.TrimEnd('\t');
                System.Windows.Forms.Clipboard.SetText(clipboardText);
            }
            else if (selectSystemsForm.GetSelectedSystemRows().Count > 0)
                System.Windows.Forms.Clipboard.SetText(GetID(selectSystemsForm.GetSelectedSystemRows().First().ID));
        }

        internal abstract void SetOrder(int order); //set policy's order within spine / function's order within policy / parameter's order within function, note: must be equal for all systems
        internal abstract void SetComment(string comment, bool isPrivate = false); //set policy's / function's / parameter's (private) comment, note: there is only one comment for all systems
        internal virtual void SetPrivateComment(string comment) { SetComment(comment, true); } //just for symmetrie with GetPrivateComment (could in fact directly use SetComment)

        internal abstract void SetToNA(string systemID = ""); //policy / function: set switch to n/a, parameter: set value to n/a; for indicated system or all systems if 'systemID' empty

        internal virtual System.Drawing.Color GetBackColor() { return System.Drawing.Color.White; }
        internal virtual System.Drawing.Font GetFont(System.Drawing.Font originalFont) { return originalFont; }

        internal virtual ContextMenuStrip GetContextMenu(TreeListNode senderNode) { return null; } //policy / funtion: return respective menu (no context menu for parameter)

        internal virtual bool IsGroupColumnEditable() { return false; } //parameter returns true
        internal virtual bool IsPolicyColumnEditable() { return false; } //parameter returns true for [Placeholder]-parameters in e.g. DefConst, DefIL, ...
        internal virtual bool ShowOrderChangeWarning() { return true; } //parameter returns false (warning about changing policy/function order)

        internal abstract RepositoryItem GetEditor(TreeListNode senderNode, TreeListColumn senderColumn); //policy / function: return combo-box-editor with on/off/..., parameter: return editor specific to parameter type
        internal virtual List<int> GetTypeSpecificIntelliItems(SystemTreeListTag systemTag) { return null; } //parameter returns intelli-items specific to parameter type (e.g. only variables for output_var)
        internal virtual bool ValidateEditorInput(string value, TreeListNode node, TreeListColumn column, ref string errorText) { return true; } //parameter: some basic checking if user input corresponds with type of parameter

        //handling for function and policies, parameter has its own handling (described there)
        internal virtual void HandleValueChanged(TreeListNode node, TreeListColumn column, string value)
        {
            if (value == TreeListManagement.TreeListManager.NA_ALL_COMPONENTS)
                return; //'n/a (all components)' has a special handling (see HandleSwitchToNAallComponents)
            _mainForm.PerformAction(new ChangeParameterValueAction(node, column, value), false, false); //set switch of function/policy
        }

        //this function needs to be called earlier than simple switch-changes (to on/off/toggle/n/a)
        //these are called on the occasion of the CellValueChanged-event, for which the user has to leave the cell (click somewhere else) to trigger the event
        //'n/a (all components)' however needs immediate reaction and is therefore on the occasion of the OnOffToggleEditor-EditValueChanged-event
        internal virtual void HandleSwitchToNAallComponents(TreeListNode node, TreeListColumn column)
        {
            //set switch of function/policy and all appendant functions/parameters of the policy/function to n/a
            _mainForm.PerformAction(new SwitchToNAAction(_mainForm, node, column), false);
        }

        internal abstract void StoreChangedValue(string newValue, CountryConfig.SystemRow systemRow); //overtake user changes to data-row (this function is called by the Actions started by HandleValueChanged)

        internal virtual string GetFunctionSpecifier(DevExpress.XtraTreeList.GetPreviewTextEventArgs eventArgs) { return string.Empty; }

        internal virtual bool IsPrivate() { return false; }

        internal abstract void SetSpecialNodeColor(int argbColor);
        internal abstract int GetSpecialNodeColor();

        static DevExpress.XtraEditors.Repository.RepositoryItemTextEdit _switchEditor = null;

        internal virtual DevExpress.XtraEditors.Repository.RepositoryItemTextEdit GetSwitchEditor()
        {
            if (_switchEditor == null)
            {
                _switchEditor = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
                _switchEditor.ReadOnly = true;
            }
            return _switchEditor;
        }

        // avoid that user's paste something else than on/off/etc. into policy- and function-switches
        internal bool IsPermittedPasteValue(string value, string systemID)
        {
            if (GetDefaultParameterRow() != null) return true; // no check for parameters
            if ((GetDefaultFunctionRow() != null && ExtensionAndGroupManager.ShowExtensionSwitchEditor(GetDefaultFunctionRow())) ||
                (GetDefaultPolicyRow() != null && ExtensionAndGroupManager.ShowExtensionSwitchEditor(GetDefaultPolicyRow())))
                return false; // if switch='switch' the cell is not-editable
            return (value.ToLower() == DefPar.Value.ON || value.ToLower() == DefPar.Value.OFF ||
                    value.ToLower() == DefPar.Value.TOGGLE || value.ToLower() == DefPar.Value.NA);
        }
    }
}

