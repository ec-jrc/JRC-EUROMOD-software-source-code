using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet;
using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_UI.VersionControl.Merging
{
    internal class ChangeLogAdministrator
    {
        // a note on speed: this is quite slow, but this seems to be due to loading the Spreadsheet library
        // most of the time is taken after the first two lines and it gets much faster if one calls the function again
        internal static List<string> GetChanges(string releaseVersion)
        {
            try
            {
                SpreadsheetControl emLog = new SpreadsheetControl();
                if (!emLog.LoadDocument(new EMPath(EM_AppContext.FolderEuromodFiles).GetEmLogFilePath())) return new List<string>();
               
                // find the relevant worksheet
                Worksheet log = null;
                foreach (Worksheet ws in emLog.Document.Worksheets) // currently the name is 'F2.30-current'
                    if (ws.Name.ToLower().Contains("current")) { log = ws; break; };
                if (log == null) return new List<string>();

                // collect the column-indices of all columns between 'Version' and 'Change', i.e. 'File', 'Policy', etc.
                List<int> colInfo = new List<int>(); int colVersion = -1;
                for (int c = 0; c < 100; ++c)
                {
                    if (GetCellValue(log, 0, c) == "version") { colVersion = c; continue; }
                    if (colVersion >= 0) colInfo.Add(c);
                    if (GetCellValue(log, 0, c) == "change") break;
                }
                if (colVersion < 0) return new List<string>();

                // find the rows that belong to the release-version and compose change-report
                int startRow = -1; List<string> changes = new List<string>();
                for (int r = 1; r < 100000; ++r) // currently there are about 5000 rows
                {
                    bool belongsToVersion = GetCellValue(log, r, colVersion) == releaseVersion.ToLower().Trim();
                    if (belongsToVersion) { if (startRow == -1) startRow = r; } // start collecting if column Version contains the release version
                    else { if (startRow != -1) break; } // finished collecting, if not - usually column Version should be empty

                    if (!belongsToVersion || GetCellValue(log, r, colVersion + 1).StartsWith("merged with"))
                        continue; // skip the row 'Merged with version xxx and saved as EuromodFiles_yyy' (which is not a change)

                    // compose change-rows like 'File: DE.XML; Policy: tco_de; System: all; Change: Update tco_de 2015-2017'
                    string change = string.Empty;
                    foreach (int c in colInfo)
                    {
                        string info = GetCellValue(log, r, c, false); if (info == string.Empty) continue;
                        change += GetCellValue(log, 0, c, false) + ": " + info + "; ";
                    }
                    changes.Add(change.Trim(new char[]{ ' ', ';'}));
                }
                return changes;
            }
            catch { return new List<string>(); }
        }

        private static string GetCellValue(Worksheet w, int r, int c, bool toLower = true)
        {
            if (w[r, c].Value == null) return string.Empty;
            string cellVal = w[r, c].Value.TextValue; if (cellVal == null) return string.Empty;
            return toLower ? cellVal = cellVal.Trim().ToLower() : cellVal = cellVal.Trim();
        }
    }
}
