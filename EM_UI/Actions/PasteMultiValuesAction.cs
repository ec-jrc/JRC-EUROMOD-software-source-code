using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListTags;
using EM_UI.TreeListManagement;
using EM_UI.DataSets;

namespace EM_UI.Actions
{
    internal class PasteMultiValuesAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;

        internal PasteMultiValuesAction(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
        }

        internal override bool ClearMultiSelector()
        {
            return false;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override void PerformAction()
        {
            List<List<string>> clipboardConent = MultiCellSelector.GetFromClipBoard();

            //one single cell copied: spread its content to the whole selected region
            if (clipboardConent.Count == 1 && clipboardConent.ElementAt(0).Count == 1)
            {
                foreach (TreeListNode node in _mainForm.GetMultiCellSelector().GetSelectedNodes(true))
                    foreach (TreeListColumn column in _mainForm.GetMultiCellSelector().GetSelectedColumns(true))
                        PasteIntoCell(node, column, clipboardConent.ElementAt(0).ElementAt(0));
                return;
            }

            //more than one single cell copied:
            //first get a possible selection; if there isn't any get the focused node; also get the focused node if it is outside the selection (this is accomplished by the parameters of GetSelected...)
            List<TreeListNode> selectedNodes = _mainForm.GetMultiCellSelector().GetSelectedNodes(true, true);
            List<TreeListColumn> selectedColumns = _mainForm.GetMultiCellSelector().GetSelectedColumns(true, true);
                        
            int focusedNodeVisibleIndex = (selectedColumns.Count != 1 || selectedNodes.Count != 1) ? -1 :
                                        _mainForm.treeList.GetVisibleIndexByNode(selectedNodes.ElementAt(0));
            int focusedColumnVisibleIndex = (selectedColumns.Count != 1 || selectedNodes.Count != 1) ? -1 :
                                        selectedColumns.ElementAt(0).VisibleIndex;

            //then loop over the clipboard content
            int nodeIndex = 0;
            foreach (List<string> rowContent in clipboardConent) //loop over rows in clipboard
            {
                TreeListNode node = null;
                if (focusedNodeVisibleIndex == -1) //there is a selection: only paste those values, which fit into the selection
                {
                    if (nodeIndex >= selectedNodes.Count)
                       break;
                    node = selectedNodes.ElementAt(nodeIndex);
                }
                else //there is no selection: paste, starting from the focused node
                {
                    node = _mainForm.treeList.GetNodeByVisibleIndex(focusedNodeVisibleIndex + nodeIndex);
                    if (node == null)
                        break;
                }
                ++nodeIndex;
                
                int columnIndex = 0;
                foreach (string cellContent in rowContent) //loop over columns in clipboard
                {
                    TreeListColumn column = null;
                    if (focusedColumnVisibleIndex == -1) //there is a selection: only paste those values, which fit into the selection
                    {
                        if (columnIndex >= selectedColumns.Count)
                            break;
                        column = selectedColumns.ElementAt(columnIndex);
                    }
                    else //there is no selection: paste, starting from the focused column
                    {
                        column = _mainForm.treeList.GetColumnByVisibleIndex(focusedColumnVisibleIndex + columnIndex);
                        if (column == null)
                            break;
                    }
                    ++columnIndex;

                    PasteIntoCell(node, column, cellContent);
                }
            }
        }

        void PasteIntoCell(TreeListNode node, TreeListColumn column, string value)
        {
            //account for the destination of the value to paste, i.e. to which column it should be pasted ...
            //... system column: i.e. either a parameter value or a switch of a policy or function
            if (TreeListBuilder.IsSystemColumn(column))
            {
                BaseTreeListTag treeListTag = (node.Tag as BaseTreeListTag);
                DataSets.CountryConfig.SystemRow systemRow = (column.Tag as SystemTreeListTag).GetSystemRow();
                if (!treeListTag.IsPermittedPasteValue(value, systemRow.ID)) return; // e.g. trying to paste 'grumml' into a policy-switch
                treeListTag.StoreChangedValue(value, systemRow);
                TreeListManager.UpdateIntelliAndTUBoxInfo((node.Tag as BaseTreeListTag).GetFunctionName(), column);
            }
            //... comment column: i.e. a comment (to a parameter, function or policy)
            else if (TreeListBuilder.IsCommentColumn(column))
                (node.Tag as BaseTreeListTag).SetComment(value);
            //... policy column: ...
            else if (TreeListBuilder.IsPolicyColumn(column))
            {   //... an editable policy column belongs to a parameter and contains e.g. a component of an incomelist, the name of a constant/variable, etc.
                if ((node.Tag as BaseTreeListTag).IsPolicyColumnEditable())
                {
                    foreach (CountryConfig.ParameterRow parameterRow in (node.Tag as ParameterTreeListTag).GetParameterRows())
                        parameterRow.Name = value;
                }
                else return; //no action if a policy-, function- or parameter-name
            }
            //... group column:
            else if (TreeListBuilder.IsGroupColumn(column))
            {
                //... an editable group column belongs to a parameter
                if ((node.Tag as BaseTreeListTag).IsGroupColumnEditable())
                {
                    foreach (CountryConfig.ParameterRow parameterRow in (node.Tag as ParameterTreeListTag).GetParameterRows())
                        parameterRow.Group = value;
                }
                else return; //group column of policies and functions is not editable
            }
            else return; //does not happen

            node.SetValue(column, value); //update nodes here to avoid having to redraw the tree (to enhance performance)
        }
    }
}
