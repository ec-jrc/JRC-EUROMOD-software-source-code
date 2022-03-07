using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using EM_Common;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Globalization;

namespace EM_Statistics
{
    public static class ExportHandling
    {
        private class ExportPage
        {
            internal string PageName { get; set; }
            internal Color TabColor { get; set; }
            internal string Caption { get; set; }
            internal string SubCaption { get; set; }
            internal string description { get; set; }
            internal string html { get; set; }
            internal List<ExportTable> Tables =  new List<ExportTable>();
        }

        private class ExportTable
        {
            internal string TableName { get; set; }
            internal string Caption { get; set; }
            internal string SubCaption { get; set; }
            internal string description { get; set; }
            internal List<string> ColHeaders { get; set; }
            internal List<string> RowHeaders { get; set; }
            internal List<bool> RowBold { get; set; }
            internal DataTable Content { get; set; }
            internal List<List<string>> NumberFormats = new List<List<string>>();
            internal DisplayResults.DisplayPage.DisplayTable.DisplayGraph Graph { get; set; }
            internal List<List<string>> StringValues = new List<List<string>>(); // contains values of cells which are strings (e.g. info)
        }
        
        private class SheetContent
        {
            internal string SheetName { get; set; }
            internal Color TabColor { get; set; }
            internal string Caption { get; set; }
            internal string SubCaption { get; set; }
            internal List<ExportPage> Pages { get; set; }
        }

        public class Package
        {
            public DisplayResults displayResults = null; // the tables containing the data to display
        }

        /// <summary> each display-page is exported to one Excel-sheet </summary>
        public static bool ExportSinglePackage(DisplayResults displayResults, out string errMsg, out MemoryStream excelStream)
        {
            return Export(new List<DisplayResults>() { displayResults }, true, out errMsg, out excelStream); 
        }

        /// <summary> each package is exported to one Excel-sheet (i.e. the display-pages of each package are arranged one after each other in the sheet) </summary>
        public static bool ExportMultiPackages(List<DisplayResults> allDisplayResults, out string errMsg, out MemoryStream excelStream)
        {
            return Export(allDisplayResults, false, out errMsg, out excelStream);
        }

        private static bool Export(List<DisplayResults> allDisplayResults, bool splitPackageIntoSheets,
                                                        out string errMsg, out MemoryStream excelStream)
        {
            errMsg = string.Empty;
            excelStream = null;
            try
            {
                if (allDisplayResults == null || allDisplayResults.Count == 0) { errMsg = "Nothing to export."; return false; }

                HardDefinitions.ExportDescriptionMode descriptionMode = allDisplayResults.First().info.exportDescriptionMode;
                List<SheetContent> sheetsContent = new List<SheetContent>();
                if (!splitPackageIntoSheets)
                {
                    foreach (DisplayResults displayResults in allDisplayResults)
                    {
                        if (displayResults == null || displayResults.displayPages == null || displayResults.displayPages.Count == 0) continue;
                        List<ExportPage> expPages = GetPackagePages(displayResults);
                        SheetContent sheetContent = new SheetContent
                        {
                            Caption = displayResults.info.title,
                            SubCaption = displayResults.info.subtitle,
                            SheetName = displayResults.info.button,
                            Pages = expPages
                        };
                        sheetsContent.Add(sheetContent);
                    }
                }
                else // this option is only called with a single package (see ExportDisplayPages above)
                {
                    DisplayResults displayResults = allDisplayResults[0];
                    if (displayResults == null || displayResults.displayPages == null || displayResults.displayPages.Count == 0) { errMsg = "Nothing to export."; return false; };
                    List<ExportPage> expPages = GetPackagePages(displayResults);
                    foreach (ExportPage expPage in expPages)
                    {
                        SheetContent sheetContent = new SheetContent
                        {
                            SheetName = expPage.PageName,
                            TabColor = expPage.TabColor,
                            Caption = displayResults.info.title,
                            SubCaption = displayResults.info.subtitle,
                            Pages = new List<ExportPage>() { expPage }
                        };
                        sheetsContent.Add(sheetContent);
                    }
                }

                if (sheetsContent.Count == 0) { errMsg = "Nothing to export."; return false; };
                ExcelPackage excel = new ExcelPackage();
                foreach (SheetContent sheetContent in sheetsContent)
                {
                    string sheetName = VerifyUniqueSheetName(excel.Workbook.Worksheets, sheetContent.SheetName);
                    var workSheet = excel.Workbook.Worksheets.Add(sheetName);
                    if (!sheetContent.TabColor.Equals(Color.Empty)) workSheet.TabColor = sheetContent.TabColor;
                    int chartCount = 0;

                    int rPos = 1;
                    workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                    workSheet.Cells[rPos, 1].Style.Font.Size = 16;
                    workSheet.Cells[rPos, 1].Style.Font.Color.SetColor(Color.DarkBlue);
                    workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(sheetContent.Caption));
                    if (!string.IsNullOrEmpty(sheetContent.SubCaption))
                    {
                        workSheet.Cells[rPos, 1].Style.Font.Color.SetColor(Color.DarkBlue);
                        workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                        workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(sheetContent.SubCaption));
                    }
                    ++rPos;

                    foreach (ExportPage exportPage in sheetContent.Pages)
                    {
                        if (!string.IsNullOrEmpty(exportPage.Caption))
                        {
                            workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                            workSheet.Cells[rPos, 1].Style.Font.Size = 14;
                            workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportPage.Caption));
                            ++rPos;
                        }
                        if (!string.IsNullOrEmpty(exportPage.SubCaption))
                        {
                            workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                            workSheet.Cells[rPos, 1].Style.Font.Size = 13;
                            workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportPage.SubCaption));
                            ++rPos;
                        }
                        if (!string.IsNullOrEmpty(exportPage.html))
                        {
                            workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportPage.html));
                        }
                        foreach (ExportTable exportTable in exportPage.Tables)
                        {
                            if (!string.IsNullOrEmpty(exportTable.Caption))
                            {
                                workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                                workSheet.Cells[rPos, 1].Style.Font.Size = 12;
                                workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportTable.Caption));
                            }
                            if (!string.IsNullOrEmpty(exportTable.SubCaption))
                            {
                                workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                                workSheet.Cells[rPos++, 1].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportTable.SubCaption));
                            }
                            for (int ch = 0; ch < exportTable.ColHeaders.Count; ++ch)
                            {
                                workSheet.Cells[rPos, ch + 2].Style.Font.Italic = true;
                                workSheet.Cells[rPos, ch + 2].Style.Font.Bold = true;
                                workSheet.DefaultColWidth = 40;
                                workSheet.Cells[rPos, ch + 2].Value = ReplaceBr(EM_Helpers.StripHTMLSpecialCharacters(exportTable.ColHeaders[ch]));

                            }
                            for (int rh = 1; rh <= exportTable.RowHeaders.Count; ++rh)
                            {
                                workSheet.Cells[rPos + rh, 1].Style.Font.Italic = true;

                                if (exportTable.RowBold[rh - 1])
                                {
                                    workSheet.Cells[rPos + rh, 1].Style.Font.Bold = true;
                                }
                                workSheet.Cells[rPos + rh, 1].Value = exportTable.RowHeaders[rh - 1];
                            }
                            ExcelRangeBase pastedTable = workSheet.Cells[++rPos, 2].LoadFromDataTable(exportTable.Content, false);
                            if (pastedTable != null) // may be null if for example Content does not have any rows (just column-headers)
                            {
                                for (int c = pastedTable.Start.Column; c <= pastedTable.End.Column; c++)
                                {
                                    List<string> nf = exportTable.NumberFormats[c - pastedTable.Start.Column];
                                    List<string> sv = exportTable.StringValues[c - pastedTable.Start.Column];
                                    for (int r = pastedTable.Start.Row; r <= pastedTable.End.Row; r++)
                                    {
                                        if (sv[r - pastedTable.Start.Row] == null)
                                            workSheet.Cells[r, c].Style.Numberformat.Format = nf[r - pastedTable.Start.Row];
                                        else
                                            workSheet.Cells[r, c].Value = sv[r - pastedTable.Start.Row];
                                        if (exportTable.RowBold[r - pastedTable.Start.Row])
                                        {
                                            workSheet.Cells[r, c].Style.Font.Bold = true;
                                        }
                                    }


                                }

                                if (exportTable.Graph != null && exportTable.Graph.allSeries.Count > 0)
                                {
                                    List<eChartType> chartTypes = GetAllChartTypes(exportTable.Graph);
                                    ExcelChart baseChart = (ExcelChart)workSheet.Drawings.AddChart("myChart" + (++chartCount), chartTypes[0]);

                                    Dictionary<eChartType, ExcelChart> allAreas = new Dictionary<eChartType, ExcelChart>
                                {
                                    { chartTypes[0], baseChart }
                                };
                                    for (int i = 1; i < chartTypes.Count; i++) allAreas.Add(chartTypes[i], baseChart.PlotArea.ChartTypes.Add(chartTypes[i]));
                                    baseChart.SetSize(900, 500);
                                    baseChart.SetPosition(pastedTable.Start.Row, 0, pastedTable.End.Column, 50);
                                    baseChart.Title.Text = exportTable.Graph.title;
                                    baseChart.Axis[0].LabelPosition = eTickLabelPosition.Low;
                                    if (exportTable.Graph.seriesInRows)
                                    {
                                        int labelPos = pastedTable.Start.Row - 1;
                                        if (!string.IsNullOrEmpty(exportTable.Graph.axisX.valuesFrom))
                                        {
                                            for (int r = pastedTable.Start.Row; r <= pastedTable.End.Row; r++)
                                                if (workSheet.Cells[r, pastedTable.Start.Column - 1].Value.ToString() == exportTable.Graph.axisX.valuesFrom) labelPos = r;
                                        }
                                        ExcelRange axisLabels = workSheet.Cells[labelPos, pastedTable.Start.Column, labelPos, pastedTable.End.Column];
                                        for (int r = pastedTable.Start.Row; r <= pastedTable.End.Row; r++)
                                        {
                                            string seriesName = workSheet.Cells[r, pastedTable.Start.Column - 1].Value.ToString();
                                            DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series s = exportTable.Graph.allSeries.Where(x => x.name == seriesName).First();
                                            if (s.visible)
                                            {
                                                ExcelChartSerie serie = allAreas[GetChartType(s)].Series.Add(workSheet.Cells[r, pastedTable.Start.Column, r, pastedTable.End.Column], axisLabels);
                                                SetSerieDetails(s, serie);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int labelPos = pastedTable.Start.Column - 1;
                                        if (!string.IsNullOrEmpty(exportTable.Graph.axisX.valuesFrom))
                                        {
                                            for (int c = pastedTable.Start.Column; c <= pastedTable.End.Column; c++)
                                                if (workSheet.Cells[pastedTable.Start.Row - 1, c].Value.ToString() == exportTable.Graph.axisX.valuesFrom) labelPos = c;
                                        }
                                        ExcelRange axisLabels = workSheet.Cells[pastedTable.Start.Row, labelPos, pastedTable.End.Row, labelPos];
                                        for (int c = pastedTable.Start.Column; c <= pastedTable.End.Column; c++)
                                        {
                                            string seriesName = workSheet.Cells[pastedTable.Start.Row - 1, c].Value.ToString();
                                            DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series s = exportTable.Graph.allSeries.Where(x => x.name == seriesName).First();
                                            if (s.visible)
                                            {
                                                ExcelChartSerie serie = allAreas[GetChartType(s)].Series.Add(workSheet.Cells[pastedTable.Start.Row, c, pastedTable.End.Row, c], axisLabels);
                                                SetSerieDetails(s, serie);
                                            }
                                        }
                                    }
                                }
                                rPos += exportTable.RowHeaders.Count;
                                if (descriptionMode == HardDefinitions.ExportDescriptionMode.InSheets) InsertDescription(workSheet, ref rPos, exportTable.description);
                                rPos++; // Keep a space between tables
                            }
                        }
                        if (descriptionMode == HardDefinitions.ExportDescriptionMode.InSheets) InsertDescription(workSheet, ref rPos, exportPage.description);
                        rPos++; // Keep an extra space between pages
                    }
                }
                AddDescriptionPage(excel, allDisplayResults[0], descriptionMode);
                

                // todo: not yet clear whether the file-selection should be in the presenter or the library
                excelStream = new MemoryStream();
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                excel.SaveAs(excelStream);
                excel.Dispose();
                return true;
            }
            catch (Exception exception) { errMsg = exception.Message; return false; }
        }

        private static void AddDescriptionPage(ExcelPackage excel, DisplayResults displayResults, HardDefinitions.ExportDescriptionMode descriptionMode)
        {
            try
            {
                if (descriptionMode == HardDefinitions.ExportDescriptionMode.No || 
                    displayResults == null || displayResults.displayPages == null || displayResults.displayPages.Count == 0) return;

                // check if sheet is necessary: not necessary, if no general description ...
                if (displayResults.info == null || string.IsNullOrEmpty(displayResults.info.description))
                {
                    if (descriptionMode == HardDefinitions.ExportDescriptionMode.InSheets) return; // ...  and other descriptions are included in content-sheets ...
                    bool noDescriptions = true; // ... or no descriptions at all
                    foreach (DisplayResults.DisplayPage page in displayResults.displayPages)
                        if (!string.IsNullOrEmpty(page.description) || (from t in page.displayTables where !string.IsNullOrEmpty(t.description) select t).Any())
                            { noDescriptions = false; break; }
                    if (noDescriptions) return;
                }

                string sheetName = VerifyUniqueSheetName(excel.Workbook.Worksheets, "Descriptions");
                var workSheet = excel.Workbook.Worksheets.Add(sheetName);
                excel.Workbook.Worksheets.MoveToStart(workSheet.Name);

                int rPos = 1;
                workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                workSheet.Cells[rPos, 1].Style.Font.Size = 16;
                workSheet.Cells[rPos, 1].Style.Font.Color.SetColor(Color.DarkBlue);
                workSheet.Cells[rPos++, 1].Value = "Descriptions";

                if (displayResults.info != null) InsertDescription(workSheet, ref rPos, displayResults.info.description, "General Description");
                if (descriptionMode == HardDefinitions.ExportDescriptionMode.InSheets) return;

                foreach (DisplayResults.DisplayPage page in displayResults.displayPages)
                {
                    InsertDescription(workSheet, ref rPos, page.description, $"Page '{page.name}'");
                    foreach (DisplayResults.DisplayPage.DisplayTable table in page.displayTables)
                        InsertDescription(workSheet, ref rPos, table.description, $"Page '{page.name}' Table '{table.name}'"); ;
                }
            }
            catch { } // do not jeopardise Export by faulty description-generation
        }

        private static void InsertDescription(ExcelWorksheet workSheet, ref int rPos, string description, string header = "Description")
        {
            try
            {
                if (string.IsNullOrEmpty(description)) return;
                workSheet.Cells[rPos, 1].Style.Font.Bold = true;
                workSheet.Cells[rPos++, 1].Value = header;

                foreach (string p in Regex.Split(description, "</p>")) // todo: improve ...
                {
                    string para = p.Replace("\n", " ") // for e.g. "... income from\nother sources ..."
                                   .Replace("&nbsp;", " ");
                    para = Regex.Replace(para, "<[^>]*>", " "); // removes <x ...> and </x> (at least I think so)
                    while (para.Contains("  ")) para = para.Replace("  ", " "); // gets rid of multiple blanks
                    while (para.Contains("\n\n")) para = para.Replace("\n\n", "\n"); // gets rid of multiple blank lines
                    para = para.Trim();
                    if (para != string.Empty)
                    {
                        workSheet.Cells[rPos, 1].Style.Numberformat.Format = "@"; // format as text
                        workSheet.Cells[rPos++, 1].Value = para;
                    }
                }
                ++rPos; // add an empty row after the description
            }
            catch { } // do not jeopardise Export by faulty description-generation
        }

        private static void SetSerieDetails(DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series s, ExcelChartSerie serie)
        {
            serie.Header = s.name;
            if (!string.IsNullOrEmpty(s.colour))
            {
                Color sc = s.colour[0] == '#' ? Color.FromArgb(int.Parse("FF" + s.colour.Substring(1), System.Globalization.NumberStyles.HexNumber)) : Color.FromName(s.colour);
                serie.Fill.Color = sc;
                if (GetChartType(s) == eChartType.Line) serie.Border.Fill.Color = sc;
                if (serie is ExcelScatterChartSerie)
                {
                    ExcelScatterChartSerie se = (serie as ExcelScatterChartSerie);
                    se.Marker = GetMarkerStyle(s);
                    se.MarkerSize = s.size;
                    se.MarkerColor = sc;
                }
            }

        }

        private static List<eChartType> GetAllChartTypes(DisplayResults.DisplayPage.DisplayTable.DisplayGraph graph)
        {
            List<eChartType> types = new List<eChartType>();
            foreach (DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series serie in graph.allSeries) if (!types.Contains(GetChartType(serie))) types.Add(GetChartType(serie));
            return types;
        }

        private static eChartType GetChartType(DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series s)
        {
            switch (s.type.ToLower())
            {
                case "stackedcolumn": return eChartType.ColumnStacked;
                case "columnclustered": return eChartType.ColumnClustered;
                case "barstacked": return eChartType.BarStacked;
                case "barclustered": return eChartType.BarClustered;
                case "point": return eChartType.XYScatter;
                case "line": return eChartType.Line;
            }
            return eChartType.Line;
        }

        private static eMarkerStyle GetMarkerStyle(DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series s)
        {
            switch (s.markerStyle.ToLower())
            {
                case "circle": return eMarkerStyle.Circle;
                case "cross": return eMarkerStyle.X;
                case "diamond": return eMarkerStyle.Diamond;
                case "square": return eMarkerStyle.Square;
                case "triangle": return eMarkerStyle.Triangle;
                case "star4": return eMarkerStyle.Star;
                case "star5": return eMarkerStyle.Star;
                case "star6": return eMarkerStyle.Star;
                case "star10": return eMarkerStyle.Star;
            }
            return eMarkerStyle.Square;
        }

        private static string VerifyUniqueSheetName(ExcelWorksheets excelWorksheets, string sheetName)
        {
            if (sheetName.Length > 30) sheetName = sheetName.Substring(0, 30); // Excel does not allow for names with more than 30 characters
            List<string> sheetNames = new List<string>(); sheetName = sheetName.ToLower(); int i = 0;
            foreach (ExcelWorksheet s in excelWorksheets) sheetNames.Add(s.Name.ToLower());
            while(sheetNames.Contains(sheetName)) sheetName = (++i).ToString() + sheetName.Substring(i.ToString().Length);
            return sheetName;
        }

        private static List<ExportPage> GetPackagePages(DisplayResults displayResults)
        {
            List<ExportPage> exportPages = new List<ExportPage>();
            foreach (var dp in displayResults.displayPages)
                exportPages.Add(GetPageContent(dp, dp.name));
            return exportPages;
        }
        
        private static ExportPage GetPageContent(DisplayResults.DisplayPage displayPage, string tableName)
        {
            ExportPage exportPage = new ExportPage();

            exportPage.Caption = displayPage.title;
            exportPage.SubCaption = displayPage.subtitle;
            exportPage.description = displayPage.description;
            exportPage.PageName = displayPage.button.title;
            exportPage.TabColor = ParseColor(displayPage.button.backgroundColour);

            foreach (DisplayResults.DisplayPage.DisplayTable displayTable in displayPage.displayTables)
                exportPage.Tables.Add(GetTableContent(displayTable, tableName));

            return exportPage;
        }

        private static Color ParseColor(string color)
        {
            if (String.IsNullOrEmpty(color)) return Color.Empty;
            Color c = Color.Empty;
            if (color.StartsWith("#") && color.Length == 7)
            {
                int red, green, blue;
                if (int.TryParse(color.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out red) 
                    && int.TryParse(color.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out green) 
                    && int.TryParse(color.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out blue))
                    c = Color.FromArgb(red, green, blue);
            }
            else
            {
                c = Color.FromName(color);
            }
            return c;
        }

        private static ExportTable GetTableContent(DisplayResults.DisplayPage.DisplayTable displayTable, string tableName)
        {
            ExportTable exportTable = new ExportTable();

            if (displayTable == null) return exportTable;

            exportTable.Caption = displayTable.title;
            exportTable.SubCaption = displayTable.subtitle;
            exportTable.description = displayTable.description;

            exportTable.TableName = tableName;
            exportTable.Content = new DataTable();
            exportTable.ColHeaders = new List<string>();
            exportTable.RowHeaders = new List<string>();
            exportTable.RowBold = new List<bool>();

            exportTable.NumberFormats.Clear();
            exportTable.StringValues.Clear();

            foreach (DisplayResults.DisplayPage.DisplayTable.DisplayColumn col in displayTable.columns)
            {
                string colHeader = ReplaceBr(col.title);
                exportTable.ColHeaders.Add(colHeader);
                exportTable.Content.Columns.Add(displayTable.columns.IndexOf(col).ToString(), typeof(double));
                exportTable.NumberFormats.Add(new List<string>());
                exportTable.StringValues.Add(new List<string>());
            }

            for (int r = 0; r < displayTable.rows.Count; r++)
            {
                DisplayResults.DisplayPage.DisplayTable.DisplayRow row = displayTable.rows[r];
                string rowHeader = ReplaceBr(row.title);
                exportTable.RowHeaders.Add(rowHeader);
                exportTable.RowBold.Add(row.strong);

                object[] cellValues = new object[exportTable.Content.Columns.Count]; int c = 0;
                foreach (DisplayResults.DisplayPage.DisplayTable.DisplayCell cell in displayTable.cells[r])
                {
                    exportTable.NumberFormats[c].Add(CSharpFormatToExcelFormat(cell.stringFormat));
                    exportTable.StringValues[c].Add(cell.isStringValue ? ReplaceBr(cell.displayValue) : null);
                    cellValues[c++] = cell.value;
                }
                exportTable.Content.Rows.Add(cellValues);
            }
            if (displayTable.graph != null) exportTable.Graph = displayTable.graph;

            return exportTable;
        }

        private static string CSharpFormatToExcelFormat(string stringFormat) // todo: improve this adhoc implementation
        {
            if (stringFormat == null) return string.Empty;
            if (!stringFormat.Contains("P")) return stringFormat;
            if (!int.TryParse(stringFormat.Substring(stringFormat.IndexOf("P") + 1), out int dec) || dec == 0) return "#,##0%";
            else stringFormat = "#,##0."; for (int d = 1; d < dec; ++d) stringFormat += "#"; stringFormat += "0%"; return stringFormat;
        }

        private static string ReplaceBr(string s) { return s.Replace("<br/>", " ").Replace("<br />", " ").Replace("<br>", " "); }
     }
}