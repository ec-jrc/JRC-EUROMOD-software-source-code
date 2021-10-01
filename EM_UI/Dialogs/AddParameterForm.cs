using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.Tools;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using EM_UI.Validate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class AddParameterForm : Form
    {
        CountryConfig.FunctionRow _displayedFunctionRow = null;
        TreeListNode _focusedNode = null;
        bool _inApplyAction = false;

        class AddParameterTag //structure for saving information in gridview-row-tags
        {
            internal AddParameterTag() { _parameterNode = null; _parDef = null; _parName = string.Empty; }
            internal string _parName;
            internal DefinitionAdmin.Par _parDef; //definition of parameter
            internal DefinitionAdmin.ParGroup _parGroup;
            internal TreeListNode _parameterNode; //necessary for "substitute" parameters
        };

        void AddParameterForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void ClearContent()
        {
            dgvParameter.Rows.Clear();
            foreach (Control control in Controls)
                control.Enabled = (_displayedFunctionRow != null);
            btnClose.Enabled = true;
        }

        List<DataGridViewRow> GetSelectedRows()
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow dataGridViewRow in dgvParameter.Rows)
            {
                if ((bool)(dataGridViewRow.Cells[colAdd.Index].Value) == true)
                    selectedRows.Add(dataGridViewRow);
            }
            return selectedRows;
        }

        void btnApply_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> selectedRows = GetSelectedRows();
            if (selectedRows.Count == 0)
                return;

            ActionGroup actionGroup = new ActionGroup();
            List<KeyValuePair<string, string>> conflictParameters = new List<KeyValuePair<string, string>>(); //helper list for dedecting conflicts à la comp_perElig[grp1]/comp_perTU[grp1]
            foreach (DataGridViewRow selectedRow in selectedRows)
            {
                string countAsString = selectedRow.Cells[colCount.Index].Value == null ? "0" : selectedRow.Cells[colCount.Index].Value.ToString();
                uint count = (countAsString != string.Empty && EM_Helpers.IsNonNegInteger(countAsString)) ? Convert.ToUInt32(countAsString) : 1;
                var gon = selectedRow.Cells[colGroupNo.Index].Value; string groupOrNo = gon == null? string.Empty : gon.ToString();
                string initialGroupOrNo = groupOrNo;
                string substitute = selectedRow.Cells[colReplaces.Index].Value.ToString();

                if (substitute != string.Empty)
                {
                    actionGroup.AddAction(new ChangeParameterNameAction((selectedRow.Tag as AddParameterTag)._parameterNode,
                                                                        selectedRow.Cells[colParameter.Index].Value.ToString()));
                    continue;
                }

                for (uint index = 0; index < count; ++index)
                {
                    DefinitionAdmin.Par parDef = (selectedRow.Tag as AddParameterTag)._parDef;
                    DefinitionAdmin.ParGroup parGroup = (selectedRow.Tag as AddParameterTag)._parGroup;
                    string parName = (selectedRow.Tag as AddParameterTag)._parName;

                    if (parDef.maxCount == 1 && (parGroup == null || parGroup.maxCount == 1) && count > 1) count = 1; //if parameter is "single" make sure it is only added once!

                    if (groupOrNo != string.Empty) //group- or footnote-parameter
                    {
                        if (EM_Helpers.IsNonNegInteger(groupOrNo))
                            groupOrNo = (EM_Helpers.SaveConvertToInt(initialGroupOrNo) + index).ToString();

                        if (CountryConfigFacade.DoesParameterExist(_displayedFunctionRow, parName, groupOrNo))
                        {//may happen if user changes group or footnote number manually
                            UserInfoHandler.ShowError($"Parameter {parName} with Grp/No {groupOrNo} already exists. " + 
                                                       "Please consider Grp/No and/or whether there is a conflict parameter.");
                            return;
                        }
                    }
                    if (parDef.substitutes.Count > 0)
                    {
                        foreach (string conflictPar in parDef.substitutes)
                         {
                            if (conflictParameters.Contains(new KeyValuePair<string, string>(conflictPar.ToLower(), groupOrNo)))
                            {//trying to add e.g. comp_perElig and comp_perTU with same group or output_var and output_add_var
                                string groupMessage = groupOrNo != string.Empty ? " with identical group " + groupOrNo : string.Empty;
                                UserInfoHandler.ShowError($"Cannot add parameter {parName} and parameter {conflictPar} {groupMessage}, as they are subsitudes.");
                                return;
                            }
                        }
                        conflictParameters.Add(new KeyValuePair<string, string>(parName.ToLower(), groupOrNo));
                    }

                    if ((_focusedNode.Tag as BaseTreeListTag).GetDefaultParameterRow() == null)
                        //if the dialog was opened via the function context menu, parameters are added at the end of the function
                        //thus they can be added simply one after the other, without changing the order ...
                        actionGroup.AddAction(new AddParameterAction(_focusedNode, parName, parDef, groupOrNo));
                    else
                        //if the dialog was opened via the parameter context menu, parameters are added after the selected parameter
                        //thus they have to be added in reverted order to keep the given order
                        actionGroup.InsertAction(0, new AddParameterAction(_focusedNode, parName, parDef, groupOrNo));
                }
            }

            _inApplyAction = true;

            TreeListNode functionNode = (_focusedNode.Tag as BaseTreeListTag).GetDefaultParameterRow() == null ? _focusedNode : _focusedNode.ParentNode;
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetCountryMainForm(_displayedFunctionRow.PolicyRow.SystemRow.CountryRow.ShortName);
            mainForm.PerformAction(actionGroup,
                true, false,
                functionNode); //performance optimisation Aug 13: update only the concerned function instead of the whole tree

            _inApplyAction = false;
            
            UpdateContent(functionNode);
        }

        void AddParamForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //don't close just hide
            Hide();
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void gridParam_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != colGroupNo.Index && e.ColumnIndex != colCount.Index)
                return;
            
            string value = e.FormattedValue.ToString();
            if (value == string.Empty)
                return;
            
            //check if group and count are non-negative integers (exception: DefIL allows for decimals for parameters + and -)
            string errorText = string.Empty;
            string parameterName = e.ColumnIndex == colCount.Index ? string.Empty : dgvParameter.Rows[e.RowIndex].Cells[colParameter.Index].Value.ToString();
            if (!ParameterValidation.ValidateGroupInput(value, parameterName, ref errorText))
            {
                Tools.UserInfoHandler.ShowError(errorText);
                e.Cancel = true;
            }
            else
            {
                if (dgvParameter.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue != e.FormattedValue)
                    dgvParameter.Rows[e.RowIndex].Cells[colAdd.Index].Value = true; //automatically select param if group or count is changed
            }
        }

        void checkShowCommon_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideRows();
        }

        void checkShowFootnote_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideRows();
        }

        void ShowHideRows()
        {
            foreach (DataGridViewRow dataGridViewRow in dgvParameter.Rows)
            {
                bool isFootnote = (dataGridViewRow.Tag as AddParameterTag)._parDef.isFootnote;
                bool isCommon = (dataGridViewRow.Tag as AddParameterTag)._parDef.isCommon;

                dataGridViewRow.Visible = true;
                if ((isFootnote && chkShowFootnoteParameters.Checked == false) || (isCommon && chkShowCommonParameters.Checked == false))
                    dataGridViewRow.Visible = false;
            }
        }

        void gridParam_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnApply_Click(null, null);
                e.Handled = true;
            }
            if (e.KeyCode == Keys.F5 || e.KeyCode == Keys.F6)
            {
                EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();
                mainForm.GetTreeListManager().HandleFKeyDown(e.KeyCode == Keys.F5, this);
            }
        }

        void btnHelpDescription_Click(object sender, EventArgs e)
        {
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();
            mainForm.GetTreeListManager().HandleFKeyDown(true, this);
        }

        void btnHelpSummary_Click(object sender, EventArgs e)
        {
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();
            mainForm.GetTreeListManager().HandleFKeyDown(false, this);
        }

        internal AddParameterForm()
        {
            InitializeComponent();
        }

        internal void UpdateContent(TreeListNode focusedNode)
        {
            try
            {
                if (_inApplyAction)
                    return;

                _displayedFunctionRow = null;
                _focusedNode = focusedNode; //needed to know where to put the parameters within the function

                if (_focusedNode != null && _focusedNode.Tag != null)
                    _displayedFunctionRow = (_focusedNode.Tag as BaseTreeListTag).GetDefaultFunctionRow(); //selected function or function where selected parameter is part of (null if policy selelcted)

                ClearContent(); //empty controls and activate/deactivate corresponding to whether a function is selected in the tree or not

                if (_displayedFunctionRow == null)
                    return; //no function selected in tree

                lblFunction.Text = _displayedFunctionRow.Name + " (order: " + _displayedFunctionRow.Order + ") in policy " + _displayedFunctionRow.PolicyRow.Name;

                Dictionary<string, string> groupList = new Dictionary<string, string>(); //helper list for group parameters

                foreach (AddParameterTag apt in GetFullParList(_displayedFunctionRow.Name))
                {
                    string parName = apt._parName; DefinitionAdmin.Par parDef = apt._parDef;
                    string groupName = apt._parGroup == null ? string.Empty : apt._parGroup.groupName;
                    string defValue = apt._parDef.defaultValue == null ? "" : apt._parDef.defaultValue.ToString();
                    bool isGroup = apt._parGroup != null; //parameter belongs/doesn't belong to a group; does: comp_Cond, comp_perTU
                    bool isSingle = parDef.maxCount == 1; //parameter can/cannot be used more than once; can: DefOuput/Var, cannot: TAX_UNIT
                    bool isFootnote = parDef.isFootnote; //parameter is/isn't a footnote parameter; is: #_UpLim, #_Amount
                    bool isConflict = parDef.substitutes.Count > 0; //parameter has/hasn't a Substitute; has: Output_Var / Output_Add_Var
                    bool isIn = false; //parameter isn't ...
                    foreach (CountryConfig.ParameterRow countryConfigParameterRow in _displayedFunctionRow.GetParameterRows())
                    {
                        if (countryConfigParameterRow.Name.ToLower() == parName.ToLower())
                        {
                            isIn = true; //.. is already used in the function
                            break;
                        }
                    }

                    string subsitude = string.Empty;
                    string groupOrNo = string.Empty;
                    string count = string.Empty;

                    if (isSingle && !isGroup && !isFootnote && isIn)
                        continue; //parameter is already used (and multi use is not possible)

                    if (!isSingle || isGroup || isFootnote)
                        count = "1"; //as default multiple useable parameter is added once

                    if (isGroup) //group parameter, e.g. comp_Cond, comp_perTU
                    {
                        //propse the likely group identifier, i.e. the next available number
                        if (groupList.Keys.Contains(groupName.ToLower()))
                            groupOrNo = groupList[groupName.ToLower()]; //for all group parameters of the same group the same group identifier is proposed
                        else
                        {
                            int maxGroup = GetMaxGroupUsedByFunction(_displayedFunctionRow, groupName) + 1;
                            groupOrNo = maxGroup.ToString();
                            groupList.Add(groupName.ToLower(), groupOrNo);
                        }
                    }

                    if (isFootnote) //footnote parameter, e.g. #_UpLim, #_Amount
                    {
                        //propse the likely footnote identifier, i.e. the next available number
                        groupOrNo = (CountryConfigFacade.GetMaxFootnoteUsedByFunction(_displayedFunctionRow, parName) + 1).ToString();
                    }

                    if (isConflict && !isGroup)
                    {//if e.g. output_var is already used, still show output_add_var, but put out_var in substitute column
                        TreeListNode functionNode = ((focusedNode.Tag as BaseTreeListTag).GetDefaultParameterRow() == null) ? focusedNode : focusedNode.ParentNode;

                        foreach (TreeListNode parameterNode in functionNode.Nodes)
                        {
                            string parameterName = parameterNode.GetDisplayText(TreeListBuilder._policyColumnName);
                            foreach (string conflictPar in parDef.substitutes)
                                if (parameterName.ToLower() == conflictPar.ToLower())
                                {
                                    subsitude = parameterName;
                                    apt._parameterNode = parameterNode;
                                    break;
                                }
                            if (subsitude != string.Empty)
                                break;
                        }
                    }

                    int index = dgvParameter.Rows.Add(false, parName, subsitude, groupOrNo, count, defValue, parDef.description);
                    dgvParameter.Rows[index].Tag = apt;
                }

                ShowHideRows();
                dgvParameter.Select();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        private int GetMaxGroupUsedByFunction(CountryConfig.FunctionRow functionRow, string groupName)
        {
            int maxGroup = 0; List<string> potentialGroupPars = new List<string>();
            DefinitionAdmin.Fun fun = DefinitionAdmin.GetFunDefinition(functionRow.Name, false);
            if (fun != null) // find all parameters belonging to this group, e.g. functionRow.Name=BenCalc, groupName=Comp_X:
            {                // find: Comp_PerTU, Comp_PerElig, Comp_Cond, Comp_LowLim, Comp_UpLim
                var pgs = from pg in fun.GetGroupParList() where pg.groupName.ToLower() == groupName.ToLower() select pg;
                if (pgs.Count() > 0) // this should always be fulfilled (i.e. the function has such a group)
                {
                    foreach (var p in pgs.First().par)
                    {
                        potentialGroupPars.Add(p.Key.ToLower());
                        foreach (string s in p.Value.substitutes) potentialGroupPars.Add(s);
                    }
                }
            }
            foreach (CountryConfig.ParameterRow countryConfigParameterRow in functionRow.GetParameterRows())
            {
                if (countryConfigParameterRow.Group == string.Empty || !EM_Helpers.IsNonNegInteger(countryConfigParameterRow.Group))
                    continue; //no group parameter
                if (!potentialGroupPars.Contains(countryConfigParameterRow.Name.ToLower()) && !IsPlaceholderSibling(countryConfigParameterRow, potentialGroupPars))
                    continue; //no sibling
                if (EM_Helpers.SaveConvertToInt(countryConfigParameterRow.Group) > maxGroup)
                    maxGroup = EM_Helpers.SaveConvertToInt(countryConfigParameterRow.Group);
            }
            return maxGroup;
        }

        private bool IsPlaceholderSibling(CountryConfig.ParameterRow parRow, List<string> potentialGroupPars)
        {
            if (!potentialGroupPars.Contains(DefPar.PAR_TYPE.PLACEHOLDER.ToString().ToLower())) return false;
            return DefinitionAdmin.GetParDefinition(parRow.FunctionRow.Name, parRow.Name, false) == null;
        }

        private static List<AddParameterTag> GetFullParList(string funName)
        {
            List<AddParameterTag> pars = new List<AddParameterTag>();
            DefinitionAdmin.Fun fun = DefinitionAdmin.GetFunDefinition(funName, false); if (fun == null) return pars;

            List<AddParameterTag> commonPars = new List<AddParameterTag>(), footnotePars = new List<AddParameterTag>();
            foreach (var p in fun.GetParList()) AddPar(p);
            foreach (var pg in fun.GetGroupParList()) foreach (var p in pg.par) AddPar(p, pg);
            pars.AddRange(commonPars); pars.AddRange(footnotePars); // to get parameters in order: fun/common/footnotes
            return pars;

            void AddPar(KeyValuePair<string, DefinitionAdmin.Par> p, DefinitionAdmin.ParGroup pg = null)
            {
                if (p.Key.ToLower() == DefPar.DefConst.Const_Monetary.ToLower()) return; // Const_Monetary is not allowed, but needs to exist in library, because it is used in some countries (e.g. el, ie)
                string parName = p.Key.ToUpper() == DefPar.PAR_TYPE.PLACEHOLDER.ToString().ToUpper() ? DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER) : p.Key;
                AddParameterTag apt = new AddParameterTag() { _parName = parName, _parDef = p.Value, _parGroup = pg };
                if (p.Value.isFootnote) footnotePars.Add(apt); else if (p.Value.isCommon) commonPars.Add(apt); else pars.Add(apt);
            }
        }
    }
}
