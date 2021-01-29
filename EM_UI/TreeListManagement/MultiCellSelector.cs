using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes.Operations;
using EM_UI.Actions;

namespace EM_UI.TreeListManagement
{
    internal class CellReferenz
    {
        internal TreeListNode Node { get; set; }
        internal TreeListColumn Column { get; set; }

        internal CellReferenz(TreeListNode node, TreeListColumn column) { Node = node; Column = column; }
    }

    internal class CellComparer : IEqualityComparer<CellReferenz>
    {
        public bool Equals(CellReferenz c1, CellReferenz c2)
        {
            return (c1.Node.Id == c2.Node.Id && c1.Column.Caption == c2.Column.Caption);
        }

        public int GetHashCode(CellReferenz cellRef)
        {
            return (cellRef.Column.Caption + cellRef.Node.Id.ToString()).GetHashCode();
        }
    }
        
    internal class MultiCellSelector
    {
        EM_UI_MainForm _mainForm = null;

        List<TreeListNode> _selectedNodes = new List<TreeListNode>();
        List<TreeListColumn> _selectedColumns = new List<TreeListColumn>();

        internal MultiCellSelector(EM_UI_MainForm mainForm) { _mainForm = mainForm; }

        internal List<TreeListNode> GetSelectedNodes(bool focusedNodeIfNoSelection = false, bool focusedNodeIfOutsideSelection = false)
        {
            List<TreeListNode> tln = new List<TreeListNode>();
            tln.AddRange(_selectedNodes);

            if (focusedNodeIfOutsideSelection && //if option focusedNodeIfOutsideSelection is set ...
                _mainForm.treeList.FocusedNode != null &&      //... check if focused node is outside selection ...
                !tln.Contains(_mainForm.treeList.FocusedNode)) //... (i.e. user seems not to refer to selection, as e.g. clicking outside) ...
            {//... assume that user refers to focused node and not to selection
                tln.Clear(); //clear list of nodes, which will be returned
                Clear();     //clear selection
                focusedNodeIfNoSelection = true; //make sure focused node is returned instead of selection
            }

            if (focusedNodeIfNoSelection && tln.Count == 0)
                tln.Add(_mainForm.treeList.FocusedNode); //if nothing is selected, assume that user refers to focused node

            return tln;
        }

        internal List<TreeListColumn> GetSelectedColumns(bool focusedColumnIfNoSelection = false, bool focusedColumnIfOutsideSelection = false)
        {
            List<TreeListColumn> tlc = new List<TreeListColumn>();
            tlc.AddRange(_selectedColumns);

            if (focusedColumnIfOutsideSelection && //if option focusedColumnIfOutsideSelection is set ...
                _mainForm.treeList.FocusedColumn != null &&      //... check if focused column is outside selection ...
                !tlc.Contains(_mainForm.treeList.FocusedColumn)) //... (i.e. user seems not to refer to selection, as e.g. clicking outside) ...
            {//... assume that user refers to focused column and not to selection
                tlc.Clear(); //clear list of columns, which will be returned
                Clear();     //clear selection
                focusedColumnIfNoSelection = true; //make sure focused columns is returned instead of selection
            }

            if (focusedColumnIfNoSelection && tlc.Count == 0)                
                tlc.Add(_mainForm.treeList.FocusedColumn);
            return tlc;
        }

        internal void SelectCells(TreeListNode startNode, TreeListNode endNode, TreeListColumn startColumn, TreeListColumn endColumn)
        {
            try
            {
                _selectedNodes.Clear();
                int startIndex = _mainForm.treeList.GetVisibleIndexByNode(startNode);
                int endIndex = _mainForm.treeList.GetVisibleIndexByNode(endNode);
                for (int index = Math.Min(startIndex, endIndex); index <= Math.Max(startIndex, endIndex); ++index)
                    if (_mainForm.treeList.GetNodeByVisibleIndex(index) != null)
                        _selectedNodes.Add(_mainForm.treeList.GetNodeByVisibleIndex(index));

                _selectedColumns.Clear();

                startIndex = startColumn.VisibleIndex;
                endIndex = endColumn.VisibleIndex;
                for (int index = Math.Min(startIndex, endIndex); index <= Math.Max(startIndex, endIndex); ++index)
                    if (_mainForm.treeList.GetColumnByVisibleIndex(index) != null)
                        _selectedColumns.Add(_mainForm.treeList.GetColumnByVisibleIndex(index));

                _mainForm.treeList.Refresh();
            }
            catch (Exception exception)
            {
                //don't jeopardise stability just because selecting fails
                _selectedNodes.Clear();
                _selectedColumns.Clear();
                Tools.UserInfoHandler.RecordIgnoredException("MultiCellSelector.SelectNodes", exception);
            }
        }

        internal bool HasSelection(bool includeFocusedCell = false)
        {
            if (_selectedColumns.Count > 0 && _selectedNodes.Count > 0)
                return true;
            return (includeFocusedCell && _mainForm.treeList.FocusedNode != null && _mainForm.treeList.FocusedColumn != null);
        }

        internal bool IsCellSelected(CellReferenz cellReference)
        {
            if (!HasSelection())
                return false;
            return _selectedNodes.Contains(cellReference.Node) && _selectedColumns.Contains(cellReference.Column);
        }

        internal TreeListColumn GetColumnIfExactlyOneSelected()
        {
            if (_selectedColumns.Count > 1)
                return null;
            if (_selectedColumns.Count == 1)
                return _selectedColumns[0];
            if (TreeListBuilder.IsSystemColumn(_mainForm.treeList.FocusedColumn))
                return _mainForm.treeList.FocusedColumn;
            return null;
        }

        internal void Clear()
        {
            _selectedNodes.Clear();
            _selectedColumns.Clear();           
        }

        internal void CopyToClipBoard()
        {
            try
            {
                List<TreeListNode > selectedNodes = GetSelectedNodes(true);
                List<TreeListColumn> selectedColumns = GetSelectedColumns(true);
                _mainForm.Cursor = Cursors.WaitCursor;

                //string clipBoardString = string.Empty;
                StringBuilder stringBuilder = new StringBuilder(); //use a StringBuilder instead of a simple string, because otherwise very large copies wont work
                foreach (TreeListNode selectedNode in selectedNodes)
                {
                    string clipBoardString = string.Empty;
                    foreach (TreeListColumn selectedColumn in selectedColumns)
                        clipBoardString += selectedNode.GetValue(selectedColumn) + "\t";
                    if (clipBoardString.EndsWith("\t"))
                        clipBoardString = clipBoardString.Substring(0, clipBoardString.Length - 1);
                    clipBoardString += Environment.NewLine;
                    stringBuilder.Append(clipBoardString);
                }
                string totalClipBoardString = stringBuilder.ToString();
                totalClipBoardString.TrimEnd();
                Clipboard.SetDataObject(totalClipBoardString);
            }
            catch (Exception e)
            {
                EM_UI.Tools.UserInfoHandler.ShowException(e);
            }
            finally
            {
                _mainForm.Cursor = Cursors.Default;
            }
        }

        internal static bool IsClipboardEmpty()
        {
            string clipboardText = string.Empty;
            try
            {
                clipboardText = System.Windows.Forms.Clipboard.GetText();
            }
            catch (Exception exception)
            {
                //assume empty clipboard if call fails
                Tools.UserInfoHandler.RecordIgnoredException("MultiCellSelector.IsClipboardEmpty", exception);
            }

            return (clipboardText == null || clipboardText == string.Empty);
        }

        internal static List<List<string>> GetFromClipBoard()
        {
            List<List<string>> clipboardContent = new List<List<string>>();

            try
            {
                string clipboardText = System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.UnicodeText); //don't know if the text format is necessary, but it seems saver (it does not solve the space-problem below ...)
                
                StringReader stringReader = new StringReader(clipboardText);
                int maxCells = -1;
                for (string line = stringReader.ReadLine(); line != null; line = stringReader.ReadLine())
                {
                    List<string> rowContent = new List<string>();
                    string[] cellContents = line.Split('\t');
                    foreach (string cellContent in cellContents)
                        rowContent.Add(cellContent.Trim()); //the trimming is necessary because otherwise a cell copied from Excel, which contains only a space leads to an XMl-error and destroys the file
                    maxCells = Math.Max(maxCells, rowContent.Count());
                    clipboardContent.Add(rowContent);
                }
                //make the array symmetric, i.e. all rows must have the same number of cells
                for (int iRow = 0; iRow < clipboardContent.Count; ++iRow)
                {
                    if (clipboardContent.ElementAt(iRow).Count < maxCells)
                    {
                        for (int iCell = clipboardContent.ElementAt(iRow).Count; iCell < maxCells; ++iCell)
                            clipboardContent.ElementAt(iRow).Add(string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                EM_UI.Tools.UserInfoHandler.ShowException(e);
            }

            return clipboardContent;
        }

        internal bool DoesSelectionContainPolicyColumn(bool includeFocusedCell = false)
        {
            if (GetSelectedColumns().Contains(_mainForm.GetTreeListBuilder().GetPolicyColumn()))
                return true;
            return includeFocusedCell && _mainForm.treeList.FocusedColumn == _mainForm.GetTreeListBuilder().GetPolicyColumn();
        }

        static internal int GetSelectionTopLevel(List<TreeListNode> selectedNodes, bool firstNodeMustHaveTopLevel)
        {
            int topLevel = int.MaxValue;
            foreach (TreeListNode node in selectedNodes)
                if (node.Level < topLevel)
                    topLevel = node.Level;

            if (firstNodeMustHaveTopLevel && selectedNodes.First().Level != topLevel)
                return -1; //for some functions (e.g. move nodes) a selection of a mixture of parameters, functions and policies does not make sense
                           //e.g. if the first selected node is a function, but policies are selected too, should the policy belonging to the function be moved, though not selected itself?
            return topLevel;
        }
    }
}
