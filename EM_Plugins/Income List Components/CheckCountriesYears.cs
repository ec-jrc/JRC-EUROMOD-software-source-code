using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.Diagnostics;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Income_List_Components
{
    /*
     * The Input form is responsible for allowing the user to select which Countries/Systems should be processed.
     * It is also responsible for checking these files exist, for reading them and passing the data to the plugin.
     */
    public partial class CheckCountriesYears : Form
    {
        const int columnHeaderHeight = 40;
        const int rowHeaderWidth = 60;
        internal Program Plugin;                    // varialbe that links to the actual plugin
        readonly List<bool> columnsCheckBoxState = new List<bool>();
        readonly List<bool> rowsCheckBoxState = new List<bool>();
        bool allCheckBoxState = false;
        readonly RepositoryItemCheckEdit edit = new RepositoryItemCheckEdit();
        readonly List<string> countryShortNames = new List<string>();
        readonly Dictionary<string, Dictionary<string, bool>> comboExists = new Dictionary<string, Dictionary<string, bool>>();

        private readonly bool timing = false;

        public CheckCountriesYears(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
        }

        private void readCountrySystemInfo(string euromod_path)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Plugin.chkData = new DataTable();
            Plugin.countries = new List<EM_Country>();

            string countries_path = Path.Combine(euromod_path, "XMLParam", "Countries");
            if (!Directory.Exists(countries_path)) return;

            // First get all the country folders and the respective country XMLs
            string[] countryFolderPaths = Directory.GetDirectories(countries_path);
            List<string> countryXMLPaths = new List<string>();
            foreach (string cfp in countryFolderPaths)
            {
                string tmp_path = Path.Combine(cfp, Path.GetFileName(cfp)) + ".xml";
                if (File.Exists(tmp_path))
                {
                    countryXMLPaths.Add(tmp_path);
                    countryShortNames.Add(Path.GetFileName(cfp).ToUpper());
                }
            }

            List<string> years = new List<string>();
            // Then read the different years in each XML
            foreach (string xmlPath in countryXMLPaths)
            {
                EM_Country country = new EM_Country();
                Dictionary<string, bool> dict = new Dictionary<string, bool>();
                
                using (XmlReader xmlReader = XmlReader.Create(xmlPath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                {
                    xmlReader.ReadToDescendant("Country");
                    using (XmlReader countryReader = xmlReader.ReadSubtree())
                    {
                        countryReader.ReadToDescendant("Name");
                        country.name = countryReader.ReadElementContentAsString();
                        countryReader.ReadToNextSibling("ShortName");
                        country.shortname = countryReader.ReadElementContentAsString().ToUpper();
                        while (countryReader.ReadToNextSibling("System"))
                        {
                            using (XmlReader systemReader = countryReader.ReadSubtree())
                            {
                                EM_System system = new EM_System();
                                systemReader.ReadToDescendant("Name");
                                system.name = systemReader.ReadElementContentAsString().ToUpper();
                                systemReader.ReadToNextSibling("Year");
                                system.year = systemReader.ReadElementContentAsString();
                                if (system.year != "" && !dict.ContainsKey(system.year)) 
                                {
                                    dict.Add(system.year, true);
                                    if (!years.Contains(system.year)) years.Add(system.year);
                                    while (systemReader.ReadToNextSibling("Policy"))
                                    {
                                        using (XmlReader policyReader = systemReader.ReadSubtree())
                                        {
                                            policyReader.ReadToDescendant("Name");
                                            string policyName = policyReader.ReadElementContentAsString();
                                            policyReader.ReadToNextSibling("Switch");
                                            if (policyReader.ReadElementContentAsString().ToUpper() == "ON")
                                            {
                                                while (policyReader.ReadToNextSibling("Function"))
                                                {
                                                    using (XmlReader functionReader = policyReader.ReadSubtree())
                                                    {
                                                        functionReader.ReadToDescendant("Name");
                                                        if (functionReader.ReadElementContentAsString().ToUpper() == "DEFIL")
                                                        {
                                                            EM_IncomeList il = new EM_IncomeList() { policy = policyName };
                                                            functionReader.ReadToNextSibling("Comment");
                                                            il.description = functionReader.ReadElementContentAsString();
                                                            functionReader.ReadToNextSibling("Switch");
                                                            if (functionReader.ReadElementContentAsString().ToUpper() == "ON")
                                                            {
                                                                while (functionReader.ReadToNextSibling("Parameter"))
                                                                {
                                                                    if (il.name == "ils_pen")
                                                                    {
                                                                        // il.name = "ils_pen";
                                                                    }
                                                                    using (XmlReader parameterReader = functionReader.ReadSubtree())
                                                                    {
                                                                        parameterReader.ReadToDescendant("Name");
                                                                        string parName = functionReader.ReadElementContentAsString();
                                                                        if (parName.ToUpper() == "NAME")
                                                                        {
                                                                            parameterReader.ReadToNextSibling("Value");
                                                                            il.name = functionReader.ReadElementContentAsString();
                                                                        }
                                                                        else
                                                                        {
                                                                            parameterReader.ReadToNextSibling("Comment");
                                                                            string comment = parameterReader.ReadElementContentAsString();
                                                                            parameterReader.ReadToNextSibling("Value");
                                                                            string addIt = parameterReader.ReadElementContentAsString();
                                                                            if (addIt == "+" || addIt == "-")
                                                                            {
                                                                                EM_ILComponent comp = new EM_ILComponent() { name = parName, addit = addIt == "+", description = comment };
                                                                                il.components.Add(comp);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                if (il.name == string.Empty) throw new Exception("incomelist without name!");
                                                                if (!system.incomelists.ContainsKey(il.name)) system.incomelists.Add(il.name, il);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    country.systems.Add(system.year, system);
                                }
                            }
                        }
                        comboExists.Add(country.shortname, dict);
                    }
                }
                Plugin.countries.Add(country);
            }
            years.Sort();

            string msg = sw.ElapsedMilliseconds + "ms taken for xml parsing";
            sw.Restart();

            foreach (string x in years)
            {
                Plugin.chkData.Columns.Add(x, typeof(bool));
                columnsCheckBoxState.Add(false);
            }
            foreach (string x in countryShortNames)
            {
                object[] r = new object[years.Count];
                for (int i = 0; i < years.Count; i++) r[i] = false;
                Plugin.chkData.Rows.Add(r);
                rowsCheckBoxState.Add(false);
            }
            gridSelector.DataSource = Plugin.chkData;

            gridView1.CustomDrawColumnHeader += gridView1_CustomDrawColumnHeader;
            gridView1.ColumnPanelRowHeight = columnHeaderHeight;
            gridView1.RowHeight = 30;
            foreach (DevExpress.XtraGrid.Columns.GridColumn c in gridView1.Columns) c.Width = rowHeaderWidth;
            gridView1.CustomDrawRowIndicator += gridView1_CustomDrawRowIndicator;
            gridView1.IndicatorWidth = rowHeaderWidth;
            sw.Stop();
            msg += Environment.NewLine + sw.ElapsedMilliseconds + "ms taken for grid";
            if (timing)
            {
                MessageBox.Show(msg);
            }
        }

        void gridView1_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle > -1)
            {
                e.Info.DisplayText = countryShortNames[e.RowHandle];
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
                e.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                e.Painter.DrawObject(e.Info);
                DrawCheckBox(e.Graphics, e.Bounds, rowsCheckBoxState[e.RowHandle], false);
                e.Handled = true;
            }
        }

        void gridView1_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e)
        {
            if (e.Column != null && e.Column.ColumnHandle > -1)
            {
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                e.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
                e.Info.InnerElements.Clear();
                e.Painter.DrawObject(e.Info);
                DrawCheckBox(e.Graphics, e.Bounds, columnsCheckBoxState[e.Column.ColumnHandle], true);
                e.Handled = true;
            }
        }

        void DrawCheckBox(Graphics g, Rectangle r, bool chk, bool colHeader)
        {
            CheckEditViewInfo info = (edit.CreateViewInfo() as CheckEditViewInfo);
            CheckEditPainter painter = (edit.CreatePainter() as CheckEditPainter);
            ControlGraphicsInfoArgs args;
            info.EditValue = chk;
            if (colHeader) r.Height += 20; else r.Width += 40;
            info.Bounds = r;
            info.CalcViewInfo(g);
            args = new ControlGraphicsInfoArgs(info, new DevExpress.Utils.Drawing.GraphicsCache(g), r);
            painter.Draw(args);
            args.Cache.Dispose();
        }

        private void countrySystemSelector_MouseDown(object sender, MouseEventArgs e)
        {
            DevExpress.XtraGrid.GridControl gc = sender as DevExpress.XtraGrid.GridControl;
            GridView gv = gc.DefaultView as GridView;
            Point pt = gv.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = gv.CalcHitInfo(pt);
            if (info.InColumn)
            {
                columnsCheckBoxState[info.Column.ColumnHandle] = !columnsCheckBoxState[info.Column.ColumnHandle];
                gv.InvalidateColumnHeader(info.Column);
                markAllCheckboxes(true, info.Column.ColumnHandle, columnsCheckBoxState[info.Column.ColumnHandle]);
            }
            else if (info.InRow && info.HitPoint.X < rowHeaderWidth)
            {
                rowsCheckBoxState[info.RowHandle] = !rowsCheckBoxState[info.RowHandle];
                gv.InvalidateRow(info.RowHandle);
                markAllCheckboxes(false, info.RowHandle, rowsCheckBoxState[info.RowHandle]);
            }
            else if (info.HitPoint.X > 0 && info.HitPoint.X < rowHeaderWidth && info.HitPoint.Y > 0 && info.HitPoint.Y < columnHeaderHeight)
            {
                allCheckBoxState = !allCheckBoxState;
                for (int i = 0; i < rowsCheckBoxState.Count; i++) rowsCheckBoxState[i] = allCheckBoxState;
                for (int i = 0; i < columnsCheckBoxState.Count; i++) columnsCheckBoxState[i] = allCheckBoxState;
                gv.Invalidate();
                markAllCheckboxes(true, -1, allCheckBoxState);
            }
        }

        private void markAllCheckboxes(bool isCol, int num, bool chk)
        {
            gridSelector.BeginUpdate();
            if (num == -1)  // whole table
            {
                int rc = 0;
                foreach (DataRow r in Plugin.chkData.Rows)
                {
                    foreach (DataColumn c in Plugin.chkData.Columns)
                    {
                        if (comboExists[countryShortNames[rc]].ContainsKey(c.ColumnName)) r.SetField(c, chk);
                    }
                    rc++;
                }
            }
            else
            {
                if (isCol)  // a single column
                {
                    int rc = 0;
                    foreach (DataRow r in Plugin.chkData.Rows)
                    {
                        if (comboExists[countryShortNames[rc++]].ContainsKey(Plugin.chkData.Columns[num].ColumnName))
                        {
                            r.SetField(num, chk);
                        }
                    }
                }
                else    // a single row
                {
                    foreach (DataColumn c in Plugin.chkData.Columns)
                    {
                        if (comboExists[countryShortNames[num]].ContainsKey(c.ColumnName))
                        {
                            Plugin.chkData.Rows[num].SetField(c, chk);
                        }
                    }
                }
            }
            gridSelector.EndUpdate();
        }

        private void btnViewComponents_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void CheckCountriesYears_Shown(object sender, EventArgs e)
        {
            labelControl2.Visible = true;
            labelControl2.BringToFront();
            btnViewComponents.Visible = false;
            btnClose.Visible = false;
            btnExport.Visible = false;
            Refresh();
            readCountrySystemInfo(Plugin.dataPath);
            labelControl2.Visible = false;
            btnViewComponents.Visible = true;
            btnClose.Visible = true;
            btnExport.Visible = true;
            gridView1.ShownEditor += gridView1_ShownEditor;
        }

        void gridView1_ShownEditor(object sender, EventArgs e)
        {
            gridView1.ActiveEditor.MouseDown += ActiveEditor_MouseDown;
        }

        void ActiveEditor_MouseDown(object sender, MouseEventArgs e)
        {
            bool b = Plugin.chkData.Rows[gridView1.FocusedRowHandle].Field<bool>(gridView1.FocusedColumn.ColumnHandle);
            Plugin.chkData.Rows[gridView1.FocusedRowHandle].SetField(gridView1.FocusedColumn.ColumnHandle, !b);
            gridView1.ActiveEditor.Hide();
        }

        private void gridView1_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if ((comboExists[countryShortNames[e.RowHandle]] as Dictionary<string, bool>).ContainsKey(e.Column.FieldName))
            {
                e.Appearance.BackColor = Color.White;
            }
            else
            {
                e.Appearance.BackColor = Color.LightGray;
            }
        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            if (!(comboExists[countryShortNames[gridView1.FocusedRowHandle]] as Dictionary<string, bool>).ContainsKey(gridView1.FocusedColumn.FieldName))
            {
                e.Cancel = true;
            }
        }

        private void gridView1_CustomDrawRowIndicator_1(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            e.Info.ImageIndex = -1;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            // Create the dictionaries that will hold all the required data for each Excel document
            Dictionary<string, XL_ILS_LIST> country_ils = new Dictionary<string,XL_ILS_LIST>();
            Dictionary<string, XL_ILS_LIST> year_ils = new Dictionary<string,XL_ILS_LIST>();
            for (int r = 0; r < Plugin.chkData.Rows.Count; r++)
            {
                foreach (DataColumn c in Plugin.chkData.Columns)
                {
                    if (Plugin.chkData.Rows[r].Field<bool>(c))
                    {
                        string cn = Plugin.countries[r].name;
                        string y = c.ColumnName;
                        if (!country_ils.ContainsKey(cn)) country_ils.Add(cn, new XL_ILS_LIST());
                        if (!year_ils.ContainsKey(y)) year_ils.Add(y, new XL_ILS_LIST());

                        country_ils[cn].all_ils.Add(y, new Dictionary<string,EM_IncomeList>());
                        year_ils[y].all_ils.Add(cn, new Dictionary<string,EM_IncomeList>());

                        foreach (EM_IncomeList il in Plugin.countries[r].systems[c.ColumnName].incomelists.Values)
                        {
                            // only take all ils from the ilsdef policy
                            if (il.policy.ToLower() == "ilsdef_"+Plugin.countries[r].shortname.ToLower())
                            {
                                if (!country_ils[cn].all_ils_components.ContainsKey(il.name)) country_ils[cn].all_ils_components.Add(il.name, new List<string>());
                                if (!year_ils[y].all_ils_components.ContainsKey(il.name)) year_ils[y].all_ils_components.Add(il.name, new List<string>());
                                if (!country_ils[cn].all_ils_info.ContainsKey(il.name)) country_ils[cn].all_ils_info.Add(il.name, new Dictionary<string, string>());
                                if (!year_ils[y].all_ils_info.ContainsKey(il.name)) year_ils[y].all_ils_info.Add(il.name, new Dictionary<string, string>());
                                
                                country_ils[cn].all_ils[y].Add(il.name, il);
                                year_ils[y].all_ils[cn].Add(il.name, il);
                                foreach (EM_ILComponent comp in il.components)
                                {
                                    if (!country_ils[cn].all_ils_components[il.name].Contains(comp.name)) country_ils[cn].all_ils_components[il.name].Add(comp.name);
                                    if (!year_ils[y].all_ils_components[il.name].Contains(comp.name)) year_ils[y].all_ils_components[il.name].Add(comp.name);
                                    if (!country_ils[cn].all_ils_info[il.name].ContainsKey(comp.name)) country_ils[cn].all_ils_info[il.name].Add(comp.name, comp.description);
                                    if (!year_ils[y].all_ils_info[il.name].ContainsKey(comp.name)) year_ils[y].all_ils_info[il.name].Add(comp.name, comp.addit ? "+" : "-");
                                }
                            }
                        }
                    }
                }
            }

            // Create the Excel file that has one sheet per country
            using (ExcelPackage excelCountry = new ExcelPackage())
            {
                foreach (string country in country_ils.Keys)
                {
                    ExcelWorksheet workSheet = excelCountry.Workbook.Worksheets.Add(country);
                    int rPos = 1;

                    foreach (string il in country_ils[country].all_ils_components.Keys)
                    {
                        // IL title
                        SetStyle(workSheet.Cells[rPos, 1], XL_Style.Bold);
                        workSheet.Cells[rPos++, 1].Value = il;
                        // IL table titles
                        int startTableRow = rPos;
                        int cPos = 1;
                        SetStyle(workSheet.Cells[rPos, cPos], XL_Style.Bold);
                        workSheet.Cells[rPos, cPos++].Value = "Variable";
                        foreach (string y in country_ils[country].all_ils.Keys)
                        {
                            SetStyle(workSheet.Cells[rPos, cPos], XL_Style.Bold);
                            workSheet.Cells[rPos, cPos++].Value = y;
                        }
                        SetStyle(workSheet.Cells[rPos, cPos], XL_Style.Bold);
                        workSheet.Cells[rPos++, cPos++].Value = "Description";
                        // IL table
                        foreach (string il_comp in country_ils[country].all_ils_components[il])
                        {
                            cPos = 1;
                            workSheet.Cells[rPos, cPos++].Value = il_comp;

                            // for each year
                            foreach (string y in country_ils[country].all_ils.Keys)
                            {
                                EM_ILComponent comp = country_ils[country].all_ils[y].ContainsKey(il) ? country_ils[country].all_ils[y][il].components.FirstOrDefault(x => x.name == il_comp) : null;
                                workSheet.Cells[rPos, cPos++].Value = comp == null ? "n/a" : comp.addit ? "+" : "-";
                            }
                            SetStyle(workSheet.Cells[rPos, 1, rPos, cPos], XL_Style.Plain);
                            workSheet.Cells[rPos++, cPos].Value = country_ils[country].all_ils_info[il][il_comp];
                        }
                        // if no rows, no need to format
                        if (rPos >= startTableRow)
                        {
                            SetStyle(workSheet.Cells[startTableRow, 1, rPos - 1, cPos], XL_Style.Border);
                        }
                    }
                }
                string filename = Path.Combine(Plugin.dataPath, "Output", "PerCountry.xlsx");
                excelCountry.SaveAs(new FileInfo(filename));
            }

            // Create the Excel file that has one sheet per year
            using (ExcelPackage excelYear = new ExcelPackage())
            {
                foreach (string year in year_ils.Keys)
                {
                    ExcelWorksheet workSheet = excelYear.Workbook.Worksheets.Add(year);
                    int rPos = 0;

                    foreach (string il in year_ils[year].all_ils_components.Keys)
                    {
                        int cPos = 1;
                        // IL table titles
                        workSheet.Cells[++rPos, cPos].Value = il;
                        workSheet.Cells[rPos, ++cPos].Value = "Variable";
                        foreach (string c in year_ils[year].all_ils.Keys)
                        {
                            workSheet.Cells[rPos, ++cPos].Value = c;
                        }
                        SetStyle(workSheet.Cells[rPos, 1, rPos, cPos], XL_Style.Bold);
                        SetStyle(workSheet.Cells[rPos, 1, rPos, cPos], XL_Style.Border);
                        // IL table
                        int startTableRow = rPos + 1;
                        foreach (string il_comp in year_ils[year].all_ils_components[il])
                        {
                            cPos = 1;
                            workSheet.Cells[++rPos, cPos].Value = il_comp;
                            workSheet.Cells[rPos, ++cPos].Value = year_ils[year].all_ils_info[il][il_comp];
                            // for each year
                            foreach (string y in year_ils[year].all_ils.Keys)
                            {
                                EM_ILComponent comp = year_ils[year].all_ils[y].ContainsKey(il) ? year_ils[year].all_ils[y][il].components.FirstOrDefault(x => x.name == il_comp) : null;
                                workSheet.Cells[rPos, ++cPos].Value = comp == null ? "n/a" : comp.description;
                            }
                        }
                        // if no rows, no need to format
                        if (rPos >= startTableRow)
                        {
                            SetStyle(workSheet.Cells[startTableRow, 1, rPos, cPos], XL_Style.Plain);
                            SetStyle(workSheet.Cells[startTableRow, 1, rPos, cPos], XL_Style.Border);
                        }
                    }
                }
                string filename = Path.Combine(Plugin.dataPath, "Output", "PerYear.xlsx");
                excelYear.SaveAs(new FileInfo(filename));
            }

            Cursor = Cursors.Default;
        }

        private void SetStyle(ExcelRange r, XL_Style s)
        {
            switch (s)
            {
                case XL_Style.Bold:
                    r.Style.Font.Bold = true;
                    r.Style.Font.Size = 12;
                    r.Style.Font.Color.SetColor(Color.DarkBlue);
                break;
                case XL_Style.Border:
                    r.Style.Border.BorderAround(ExcelBorderStyle.Thick);
                break;
                case XL_Style.Plain:
                    r.Style.Font.Bold = false;
                    r.Style.Font.Size = 12;
                    r.Style.Font.Color.SetColor(Color.Black);
                break;
            }
        }

        enum XL_Style { Bold, Border, Plain }

        private class XL_ILS_LIST
        {
            internal Dictionary<string, List<string>> all_ils_components { get; set; }
            internal Dictionary<string, Dictionary<string, string>> all_ils_info { get; set; }
            internal Dictionary<string, Dictionary<string, EM_IncomeList>> all_ils { get; set; }

            public XL_ILS_LIST()
            {
                all_ils_components = new Dictionary<string, List<string>>();
                all_ils_info = new Dictionary<string, Dictionary<string, string>>();
                all_ils = new Dictionary<string, Dictionary<string, EM_IncomeList>>();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Abort;
        }
    }
}
