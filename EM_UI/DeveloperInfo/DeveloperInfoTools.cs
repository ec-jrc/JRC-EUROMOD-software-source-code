using EM_Common;
using EM_UI.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo
{
    internal class DeveloperInfoTools
    {
        internal static bool ExportListView(ListView listView, string exportFolder, string exportFile)
        {
            return ExportView(exportFolder, exportFile, null, listView);
        }

        internal static bool ExportDataGridView(DataGridView dataGridView, string exportFolder, string exportFile)
        {
            return ExportView(exportFolder, exportFile, dataGridView);
        }

        static bool ExportView(string exportFolder, string exportFile, DataGridView dataGridView = null, ListView listView = null)
        {
            try
            {
                string exportPath = EMPath.AddSlash(exportFolder);
                if (!Directory.Exists(exportPath))
                {
                    UserInfoHandler.ShowError("'" + exportFolder + "' is not a valid path.");
                    return false;
                }

                if (exportFile.Length > 3 && exportFile.Substring(exportFile.Length - 4, 1) != ".")
                    exportFile += ".txt";
                string exportFileFullName = EMPath.AddSlash(exportFolder) + exportFile;
                FileInfo fileInfo = new FileInfo(exportFileFullName); //to throw an exception if invalid name (e.g. using characters like *?)

                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(exportFileFullName);

                if (dataGridView != null)
                {
                    if (dataGridView.ColumnHeadersVisible)
                    {
                        string line = (dataGridView.RowHeadersVisible) ? "\t" : string.Empty;
                        foreach (DataGridViewColumn column in dataGridView.Columns)
                            line += ((column.HeaderCell.Value == null) ? string.Empty : column.HeaderCell.Value.ToString()) + "\t";
                        line = line.TrimEnd();
                        streamWriter.WriteLine(line);
                    }
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        string line = string.Empty;
                        if (dataGridView.RowHeadersVisible)
                            line = ((row.HeaderCell.Value == null) ? string.Empty : row.HeaderCell.Value.ToString()) + "\t";
                        foreach (DataGridViewColumn column in dataGridView.Columns)
                            line += ((row.Cells[column.Index].Value == null) ? string.Empty : row.Cells[column.Index].Value.ToString()) + "\t";
                        line = line.TrimEnd();
                        streamWriter.WriteLine(line);
                    }
                }
                else if (listView != null)
                {
                    string line = string.Empty;
                    foreach (ColumnHeader columnHeader in listView.Columns)
                        line += columnHeader.Text + "\t";
                    line = line.TrimEnd();
                    streamWriter.WriteLine(line);

                    foreach (ListViewItem row in listView.Items)
                    {
                        line = string.Empty;
                        foreach (ListViewItem.ListViewSubItem column in row.SubItems)
                            line += column.Text + "\t";
                        line = line.TrimEnd();
                        streamWriter.WriteLine(line);
                    }
                }

                streamWriter.Close();

                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return false;
            }
        }
    }
}
