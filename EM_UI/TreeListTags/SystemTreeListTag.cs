using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using EM_Common;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.TreeListTags
{
    internal class SystemTreeListTag
    {
        CountryConfig.SystemRow _systemRow = null;

        RepositoryItemComboBox _TUEditor = null;
        RepositoryItemComboBox _ILEditor = null;

        List<CountryConfig.ParameterRow> _parameterRowsTUs = null;
        List<CountryConfig.ParameterRow> _parameterRowsILs = null;
        List<CountryConfig.ParameterRow> _parameterRowsConstants = null;
        List<CountryConfig.ParameterRow> _parameterRowsDefVariables = null;

        //performance optimisation Aug 13: introduced functions FillTU_Editor and FillIL_Editor instead of directly calling FillTUorILEditor
        void FillTU_Editor() { if (_TUEditor == null) FillTUorILEditor(ref _TUEditor, GetParameterRowsTUs()); }
        void FillIL_Editor() { if (_ILEditor == null) FillTUorILEditor(ref _ILEditor, GetParameterRowsILs()); }
        void FillTUorILEditor(ref RepositoryItemComboBox editor, List<CountryConfig.ParameterRow> parameterRowsTUsorILs)
        {
            try
            {
                if (editor == null)
                {
                    editor = new RepositoryItemComboBox();
                    editor.CloseUpKey = new DevExpress.Utils.KeyShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Down));
                    editor.DrawItem += new DevExpress.XtraEditors.ListBoxDrawItemEventHandler(Editor_DrawItem);
                    editor.Closed += new DevExpress.XtraEditors.Controls.ClosedEventHandler(Editor_Closed);
                    editor.MouseWheel += new MouseEventHandler(Editor_MouseWheel);

                    foreach (CountryConfig.ParameterRow parameterRow in parameterRowsTUsorILs)
                    {
                        editor.Items.Add(parameterRow.Value);
                    }
                    editor.Items.Add(DefPar.Value.NA);
                    editor.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        void Editor_MouseWheel(object sender, MouseEventArgs e)
        {
            EM_UI_MainForm mainForm = (sender as ComboBoxEdit).Parent.Parent as EM_UI_MainForm;
            mainForm.treeList.Focus(); //set focus to treeList, to avoid changing value of list
        }

        //in this method a tooltip for the items of the combo is drawn
        void Editor_DrawItem(object sender, DevExpress.XtraEditors.ListBoxDrawItemEventArgs e)
        {
            ComboBoxEdit cboEditor = sender as ComboBoxEdit;
            EM_UI_MainForm mainForm = cboEditor.Parent.Parent as EM_UI_MainForm;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                //search for the Comment, it should go into the tooltip
                var parameterRowComment = from parameterRow in GetParameterRowsILs() where parameterRow.Value.Equals(e.Item.ToString()) select parameterRow;
                if (parameterRowComment.Count() == 0)
                    parameterRowComment = from parameterRow in GetParameterRowsTUs() where parameterRow.Value.Equals(e.Item.ToString()) select parameterRow;
                if (parameterRowComment.Count() == 1)
                {
                    mainForm.lblComboboxEditorToolTip.Text = GetILTUComment(parameterRowComment.First());
                    mainForm.lblComboboxEditorToolTip.Visible = true;

                    int y = mainForm.treeList.Location.Y + cboEditor.Location.Y + e.Bounds.Bottom + 5;
                    int x = cboEditor.Location.X + e.Bounds.Right + 25;

                    mainForm.lblComboboxEditorToolTip.Location = new Point(x, y);
                    mainForm.lblComboboxEditorToolTip.BringToFront();
                }
                else
                    mainForm.lblComboboxEditorToolTip.Visible = false;
            }
        }

        //close the ItemTooltip as well
        void Editor_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {
            ComboBoxEdit cboEditor = sender as ComboBoxEdit;
            EM_UI_MainForm mainForm = cboEditor.Parent.Parent as EM_UI_MainForm;
            mainForm.lblComboboxEditorToolTip.Visible = false;
        }

        internal SystemTreeListTag(CountryConfig.SystemRow systemRow)
        {
            _systemRow = systemRow;
        }

        internal CountryConfig.SystemRow GetSystemRow()
        {
            return _systemRow;
        }

        internal int GetSystemOrder()
        {
            return EM_Helpers.SaveConvertToInt(_systemRow.Order);
        }

        internal void SetSystemOrder(int Order)
        {
            _systemRow.Order = Convert.ToString(Order);
        }

        internal RepositoryItemComboBox GetTUEditor()
        {
            FillTU_Editor();
            return _TUEditor;
        }

        internal RepositoryItemComboBox GetILEditor()
        {
            FillIL_Editor();
            return _ILEditor;
        }

        //called if treeview is repainted because of a change, but columns are not repainted
        //otherwise editors would not reflect newly added items (TUs, ILs, etc.)
        internal void UpdateEditors()
        {
            _TUEditor = null;
            _ILEditor = null;
            
            //do not update the information displayed by the editors (i.e. taxunits, incomelists) on each update as this causes severe performance problems
            //instead the update is done in the ChangeParameterAction (and restricted to the concerned system and Def-function)
            //_parameterRowsTUs = null;
            //_parameterRowsILs = null;
            //_parameterRowsConstants = null;
            //_parameterRowsDefVariables = null;
        }

        //called by ChangeParameterAction and SpreadParameterValueAction (via TreeListManager)
        //to enforce updating of the respective info if functions DefTU, DefIL, DefVar or DefConst are changed
        internal void UpdateTUInfo() { _parameterRowsTUs = null; }
        internal void UpdateILInfo() { _parameterRowsILs = null; }
        internal void UpdateConstantsInfo() { _parameterRowsConstants = null; }
        internal void UpdateVariablesInfo() { _parameterRowsDefVariables = null; }

        internal List<DataSets.CountryConfig.ParameterRow> GetParameterRowsTUs()
        {
            CountryConfigFacade.GetDefFunctionInformation(_systemRow, ref _parameterRowsTUs, DefFun.DefTu, DefPar.DefTu.Name);
            return _parameterRowsTUs;
        }

        internal List<DataSets.CountryConfig.ParameterRow> GetParameterRowsILs()
        {
            CountryConfigFacade.GetDefFunctionInformation(_systemRow, ref _parameterRowsILs, DefFun.DefIl, DefPar.DefIl.Name);
            return _parameterRowsILs;
        }

        internal List<DataSets.CountryConfig.ParameterRow> GetParameterRowsConstants()
        {
            CountryConfigFacade.GetDefFunctionInformation(_systemRow, ref _parameterRowsConstants, DefFun.DefConst, DefPar.DefConst.EM2_Const_Name);
            return _parameterRowsConstants;
        }

        internal List<DataSets.CountryConfig.ParameterRow> GetParameterRowsDefVariables()
        {
            CountryConfigFacade.GetDefFunctionInformation(_systemRow, ref _parameterRowsDefVariables, DefFun.DefVar, DefPar.DefVar.EM2_Var_Name);
            return _parameterRowsDefVariables;
        }

        //either use the comment alongside the name-parameter or the function's comment
        internal string GetILTUComment(CountryConfig.ParameterRow parameterRowILTU)
        {
            if (parameterRowILTU.Comment != null && parameterRowILTU.Comment != string.Empty)
                return parameterRowILTU.Comment;
            return parameterRowILTU.FunctionRow.Comment;
        }

    }
}
