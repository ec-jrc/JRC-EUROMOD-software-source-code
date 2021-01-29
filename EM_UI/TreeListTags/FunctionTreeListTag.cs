using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
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
    internal class FunctionTreeListTag : BaseTreeListTag
    {
        Dictionary<string, CountryConfig.FunctionRow> _functionRows = new Dictionary<string, CountryConfig.FunctionRow>(); //contains one function data-row per system

        internal FunctionTreeListTag(EM_UI_MainForm mainform) : base(mainform) { }

        internal void AddFunctionRowOfSystem(string systemID, CountryConfig.FunctionRow functionRow)
        {
            _functionRows.Add(systemID, functionRow);
        }

        internal List<CountryConfig.FunctionRow> GetFunctionRows()
        {
            return _functionRows.Values.ToList();
        }

        internal Dictionary<string, CountryConfig.FunctionRow> GetFunctionDictionary()
        {
            return _functionRows;
        }

        internal CountryConfig.FunctionRow GetFunctionRowOfSystem(string systemID)
        {
            return _functionRows[systemID];
        }

        //descriptions of override functions see BaseTreeListTag

        internal override CountryConfig.PolicyRow GetDefaultPolicyRow()
        {
            return GetDefaultFunctionRow().PolicyRow;
        }

        internal override CountryConfig.FunctionRow GetDefaultFunctionRow()
        {
            return _functionRows.Values.FirstOrDefault<CountryConfig.FunctionRow>();
        }

        internal override int GetOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultFunctionRow().Order);
        }

        internal override int GetPolicyOrder()
        {
            return EM_Helpers.SaveConvertToInt(GetDefaultFunctionRow().PolicyRow.Order);
        }

        internal override int GetFunctionOrder()
        {
            return GetOrder();
        }

        internal override string GetPolicyName()
        {
            return GetDefaultFunctionRow().PolicyRow.Name;
        }

        internal override string GetFunctionName()
        {
            return GetDefaultFunctionRow().Name;
        }

        internal override string GetID(string systemID)
        {
            return _functionRows[systemID].ID;
        }

        internal override string GetDefaultID()
        {
            return GetDefaultFunctionRow().ID;
        }

        internal override string SaveGetDefaultID(string dummyID = "")
        {
            return (_functionRows.Count == 0) ? dummyID : GetDefaultID();
        }

        internal override List<string> GetIDsWithinAllSystems()
        {
            List<string> ids = new List<string>();
            foreach (CountryConfig.FunctionRow functionRow in _functionRows.Values)
                ids.Add(functionRow.ID);
            return ids;
        }

        internal override string GetPrivateComment()
        {
            string privateComment = GetDefaultFunctionRow().PrivateComment;
            return privateComment == null ? string.Empty : privateComment;
        }

        internal override void SetOrder(int order)
        {
            foreach (CountryConfig.FunctionRow functionRow in _functionRows.Values)
                functionRow.Order = order.ToString();
        }

        internal override void SetComment(string comment, bool isPrivate = false)
        {
            foreach (CountryConfig.FunctionRow functionRow in _functionRows.Values)
            {
                if (isPrivate)
                    functionRow.PrivateComment = comment;
                else
                    functionRow.Comment = comment;
            }
        }

        internal override void SetToNA(string systemID = "")
        {
            foreach (string functionSystemID in _functionRows.Keys)
                if (systemID == string.Empty || systemID == functionSystemID)
                    _functionRows[functionSystemID].Switch = DefPar.Value.NA;
        }

        internal override void CopySymbolicIdentfierToClipboard()
        {
            System.Windows.Forms.Clipboard.SetText(ImportExportHelper.GetSymbolicID(GetDefaultFunctionRow()));
        }

        internal override System.Drawing.Color GetBackColor()
        {
            //return System.Drawing.Color.FromArgb(230, 230, 250);
            return System.Drawing.Color.FromArgb(234, 244, 253);
        }

        internal override ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {
            return _mainForm.GetFunctionContextMenu().GetContextMenu(senderNode);
        }

        internal override RepositoryItem GetEditor(TreeListNode senderNode, TreeListColumn senderColumn)
        {
            try
            {
                //assess whether the policy is switchable for the specific system, i.e. is controlled via the switches-dialog and the run-tool
                if (ExtensionAndGroupManager.ShowExtensionSwitchEditor(_functionRows[(senderColumn.Tag as SystemTreeListTag).GetSystemRow().ID]))
                    return GetSwitchEditor();
            }
            catch (Exception exception) { Tools.UserInfoHandler.RecordIgnoredException("FunctionTreeListTag.GetEditor", exception); }
            return _mainForm.OnOffToggleEditor;
        }

        internal override void StoreChangedValue(string newValue, CountryConfig.SystemRow systemRow)
        {
            _functionRows[systemRow.ID].Switch = newValue;
        }

        internal override string GetFunctionSpecifier(GetPreviewTextEventArgs eventArgs)
        {
            string specifier = string.Empty;

            //run over parameters of the function to find the "specifier parameter", e.g. 'name' for DefIL and DefTU
            foreach (TreeListNode parameterNode in eventArgs.Node.Nodes)
            {
                string parameterName = (parameterNode.Tag as ParameterTreeListTag).GetParameterName();
                if (!IsFunctionSpecifier(GetFunctionName(), parameterName, out bool isSingleSpecifier)) continue;

                //specifier found: analyse content
                List<string> foundValues = new List<string>();
                foreach (TreeListColumn column in _mainForm.GetTreeListBuilder().GetSystemColums())
                {
                    string parameterValue = parameterNode.GetDisplayText(column);
                    if (parameterValue == DefPar.Value.NA || foundValues.Contains(parameterValue.ToLower()))
                        continue; //goto next value if this is n/a or already listed (e.g. same 'name' for all or several systems)

                    specifier = (specifier == string.Empty) ? parameterName + ": "
                        : specifier + "; "; //if parameter with multiple occurrence (e.g. 'dataset') or different value for systems, just append (e.g. dataset: uk_2008*; uk_2009*)
                                            //(remark: it is not foreseen that a function has two different specifiers, as currently they only possible application for this would be Sys and SysNa for AddOn_Applic)
                    specifier += parameterValue;
                    foundValues.Add(parameterValue.ToLower());
                }

                if (isSingleSpecifier) break; //for e.g. 'name' of DefIL, DefTU we can stop here, as it can only appear once, however for e.g. 'dataset' of Uprate, SetDefault we need to continue as multiple occurrences are possible
            }

            return specifier;
        }

        static bool IsFunctionSpecifier(string functionName, string parameterName, out bool isSingleSpecifier)
        {
            Dictionary<string, Tuple<string, bool>> funSpec = new Dictionary<string, Tuple<string, bool>>()
            {
                { DefFun.DefIl.ToLower(), new Tuple<string, bool>(DefPar.DefIl.Name, true) },
                { DefFun.DefTu.ToLower(), new Tuple<string, bool>(DefPar.DefTu.Name, true) },
                { DefFun.UpdateTu.ToLower(), new Tuple<string, bool>(DefPar.UpdateTu.Name, true) },
                { DefFun.DefOutput.ToLower(), new Tuple<string, bool>(DefPar.DefOutput.File, true) },
                { DefFun.Uprate.ToLower(), new Tuple<string, bool>(DefPar.Uprate.Dataset, false) },
                { DefFun.SetDefault.ToLower(), new Tuple<string, bool>(DefPar.SetDefault.Dataset, false) },
                { DefFun.Elig.ToLower(), new Tuple<string, bool>(DefPar.Elig.Elig_Cond, true) },
                { DefFun.Allocate.ToLower(), new Tuple<string, bool>(DefPar.Allocate.Share, true) },
                { DefFun.Loop.ToLower(), new Tuple<string, bool>(DefPar.Loop.Loop_Id, true) },
                { DefFun.UnitLoop.ToLower(), new Tuple<string, bool>(DefPar.UnitLoop.Loop_Id, true) },
                { DefFun.Store.ToLower(), new Tuple<string, bool>("post*", true) },
                { DefFun.Restore.ToLower(), new Tuple<string, bool>("post*", true) },
                { DefFun.DropUnit.ToLower(), new Tuple<string, bool>(DefPar.DropUnit.Drop_Cond, true) },
                { DefFun.KeepUnit.ToLower(), new Tuple<string, bool>(DefPar.KeepUnit.Keep_Cond, true) },
                { DefFun.CallProgramme.ToLower(), new Tuple<string, bool>(DefPar.CallProgramme.Programme, true) },
                { DefFun.AddOn_Applic.ToLower(), new Tuple<string, bool>(DefPar.AddOn_Applic.Sys, false) },
                { DefFun.AddOn_Pol.ToLower(), new Tuple<string, bool>(DefPar.AddOn_Pol.Pol_Name, true) },
                { DefFun.DefInput.ToLower(), new Tuple<string, bool>(DefPar.DefInput.file, true) },
                { "default", new Tuple<string, bool>("output*var", true) } //output_var and output_add_var are default if function has no specific specifier
            };

            Tuple<string, bool> specifyingParameter = funSpec.ContainsKey(functionName.ToLower()) ? funSpec[functionName.ToLower()] : funSpec["default"];
            isSingleSpecifier = true;
            if (!EM_Helpers.DoesValueMatchPattern(specifyingParameter.Item1, parameterName)) return false;
            isSingleSpecifier = specifyingParameter.Item2;
            return true;
        }

        internal override string GetValue(string systemID)
        {
            return _functionRows[systemID].Switch;
        }

        internal override bool IsPrivate()
        {
            string isPrivate = GetDefaultFunctionRow().Private;
            return (isPrivate == DefPar.Value.YES);
        }

        internal override int GetSpecialNodeColor()
        {
            CountryConfig.FunctionRow functionRow = GetDefaultFunctionRow();
            if (functionRow == null || functionRow.IsNull(CountryConfigFacade._columnName_Color))
                return DefPar.Value.NO_COLOR;
            return functionRow.Color;
        }

        internal override void SetSpecialNodeColor(int argbColor)
        {
            foreach (CountryConfig.FunctionRow functionRow in _functionRows.Values)
                functionRow.Color = argbColor;
        }
    }
}
