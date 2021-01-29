using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;

namespace EM_UI.NodeOperations
{
    internal class IsNodeValueDifferent : IsSpecificBase
    {
        TreeListColumn _column1;
        TreeListColumn _column2;
        ImportExport.ImportByIDDiscrepancies _importByIDDiscrepancies;
        internal int _countImportByIDDiscrepancies { get; private set; }

        internal IsNodeValueDifferent(TreeListColumn column1, TreeListColumn column2,
                                      ImportExport.ImportByIDDiscrepancies importByIDDiscrepancies = null)
        {
            _column1 = column1;
            _column2 = column2;
            _importByIDDiscrepancies = importByIDDiscrepancies; //only relevant for comparing versions - show the little red/green squares in the group column, which hint at differences to compare-systems
        }

        internal override bool Execute(TreeListNode node)
        {
            object nodeValue1 = node.GetValue(_column1);
            object nodeValue2 = node.GetValue(_column2);
            if (nodeValue1 == null || nodeValue2 == null)
                return false;

            bool isDifferent = nodeValue1.ToString().ToLower() != nodeValue2.ToString().ToLower();

            TreeListTags.BaseTreeListTag tag = node.Tag as TreeListTags.BaseTreeListTag;
            if (_importByIDDiscrepancies != null && tag != null)
                if (_importByIDDiscrepancies.GetDiscrepancy(tag.GetIDsWithinAllSystems()) != string.Empty)
                {
                    if (!isDifferent)
                        ++_countImportByIDDiscrepancies; //assess count of differences only due to the little red/green squares
                    isDifferent = true;
                }
            return isDifferent;
        }
    }
}
