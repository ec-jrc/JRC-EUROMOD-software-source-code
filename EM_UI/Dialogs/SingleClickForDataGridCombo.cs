using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal class SingleClickForDataGridCombo
    {
        // this changes the annoying behaviour of DataGridView combo-boxes, which as a default require clicking twice to show the drop-down
        internal static void HandleDataGridViewMouseDown(object sender, MouseEventArgs e)
        {
            DataGridView dataGridView = sender as DataGridView; if (dataGridView == null) return;
            DataGridView.HitTestInfo info = dataGridView.HitTest(e.X, e.Y); if (info == null) return;
            if (info.Type != DataGridViewHitTestType.Cell || info.ColumnIndex < 0) return;
            if (dataGridView.Columns[info.ColumnIndex] as DataGridViewComboBoxColumn == null) return; // is not a combo-box column
            dataGridView.CurrentCell = dataGridView.Rows[info.RowIndex].Cells[info.ColumnIndex];
        }
    }
}
