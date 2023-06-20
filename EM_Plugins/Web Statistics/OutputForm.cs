using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Spreadsheet;
using EM_Common;

namespace Web_Statistics
{
    /*
     * The Input form is responsible for allowing the user to select which Countries/Systems should be processed.
     * It is also responsible for checking these files exist, for reading them and passing the data to the plugin.
     */
    public partial class OutputForm : Form
    {
        internal Program Plugin;                    // varialbe that links to the actual plugin

        public OutputForm(Program _plugin)
        {
            Plugin = _plugin;
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void CheckCountriesYears_Shown(object sender, EventArgs e)
        {
            labelControl1.Text = "Reading EUROMOD data, please wait...";
            button1.Visible = false;
            Refresh();
            readDataEUROMOD();
            labelControl1.Text = "Reading STATA data, please wait...";
            Refresh();
            readDataSTATA();
            labelControl1.Text = "Generating the Web Statistics report, please wait...";
            Refresh();
            generateWebStatistics();
            labelControl1.Text = "The Web Statistics report has been generated!";
            button1.Visible = true;
        }

        private XElement GetElement(XDocument doc, string elementName)
        {
            foreach (XNode node in doc.DescendantNodes())
            {
                if (node is XElement)
                {
                    XElement element = (XElement)node;
                    if (element.Name.LocalName.Equals(elementName))
                        return element;
                }
            }
            return null;
        }

        internal void readDataEUROMOD()
        {
            string countries_path = Path.Combine(Plugin.inputPathEUROMOD, "XMLParam", "Countries");
            if (!Directory.Exists(countries_path)) return;
            string version_path = Path.Combine(Plugin.inputPathEUROMOD, "XMLParam", "Config", "EuromodVersion.txt");
            if (File.Exists(version_path))
                Plugin.euromod_version = File.ReadLines(version_path).ElementAt<string>(0).Trim();
            else
                Plugin.euromod_version = "3.2.1";

            // First get all the country folders and the respective country XMLs
            List<string> countryXMLPaths = new List<string>();
            foreach (string cfp in Plugin.allCountries)
            {
                string tmp_path = Path.Combine(countries_path, cfp, cfp + ".xml");
                if (File.Exists(tmp_path))
                {
                    countryXMLPaths.Add(tmp_path);
                }
            }

            List<string> years = new List<string>();
            // Then read the different years in each XML
            int cnt = 0;
            foreach (string xmlPath in countryXMLPaths)
            {
                cnt++;
                labelControl1.Text = "Reading EUROMOD data, please wait... (" + cnt + " of " + countryXMLPaths.Count + ")";
                Refresh();
                EM_Country country = new EM_Country();
                XDocument doc = XDocument.Load(xmlPath);
                XElement cc = GetElement(doc, "Country");
                string csn = "";
                Dictionary<string, bool> dict = new Dictionary<string, bool>();
                foreach (XElement el in cc.Elements())  // for each country child element 
                {
                    if (el.Name.LocalName == "Name")
                    {
                        country.name = el.Value;
                    }
                    if (el.Name.LocalName == "ShortName")
                    {
                        csn = el.Value.ToUpper();
                        country.shortname = csn;
                    }
                    if (el.Name.LocalName == "System")
                    {
                        EM_System system = new EM_System();
                        string sn = "";
                        bool passed = false;
                        foreach (XElement el1 in el.Elements()) // for each system child element
                        {
                            if (el1.Name.LocalName == "Name")
                            {
                                sn = el1.Value.ToUpper();
                                system.name = sn;
                                int y;
                                if ((sn.Substring(0, csn.Length + 1) == csn + "_") && int.TryParse(sn.Substring(csn.Length + 1), out y) && (sn.Substring(csn.Length + 1) == y.ToString()))
                                {
                                    system.year = y.ToString();
                                    dict.Add(y.ToString(), true);
                                    if (!years.Contains(y.ToString())) years.Add(y.ToString());
                                    passed = true;
                                }
                            }
                            if (passed)
                            {
                                if (el1.Name.LocalName == "Policy")
                                {
                                    foreach (XElement el2 in el1.Elements()) // for each Policy child element
                                    {
                                        if (el2.Name.LocalName == "Switch")
                                        {
                                            if (el2.Value.ToLower() != "on")
                                            {
                                                break;
                                            }
                                        }
                                        else if (el2.Name.LocalName == "Function")
                                        {
                                            EM_IncomeList il = new EM_IncomeList();
                                            bool isDefil = false;
                                            foreach (XElement el3 in el2.Elements()) // for each Function child element
                                            {
                                                if (el3.Name.LocalName == "Switch")
                                                {
                                                    if (el3.Value.ToLower() != "on")
                                                    {
                                                        isDefil = false;
                                                        break;
                                                    }
                                                }
                                                else if (el3.Name.LocalName == "Name")
                                                {
                                                    if (el3.Value == "DefIl")   // Reached an Income List definition
                                                    {
                                                        isDefil = true;
                                                    }
                                                    else
                                                    {
                                                        isDefil = false;
                                                        break;
                                                    }
                                                }
                                                else if (el3.Name.LocalName == "Comment")
                                                {
                                                    il.description = el3.Value;
                                                }
                                                else if (isDefil && el3.Name.LocalName == "Parameter")
                                                {
                                                    EM_ILComponent comp = new EM_ILComponent();
                                                    bool isName = false;
                                                    foreach (XElement el4 in el3.Elements()) // for each Parameter child element
                                                    {
                                                        if (el4.Name.LocalName == "Name")
                                                        {
                                                            if (el4.Value.ToLower() == "name")
                                                            {
                                                                isName = true;
                                                            }
                                                            else
                                                            {
                                                                isName = false;
                                                                comp.name = el4.Value;
                                                            }
                                                        }
                                                        else if (el4.Name.LocalName == "Comment")
                                                        {
                                                            if (!isName) comp.description = el4.Value;
                                                        }
                                                        else if (el4.Name.LocalName == "Value")
                                                        {
                                                            if (isName)
                                                            {
                                                                if (el4.Value.ToLower() == "n/a")
                                                                {
                                                                    isDefil = false;
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    il.name = el4.Value;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (el4.Value == "+")
                                                                {
                                                                    comp.addit = true;
                                                                    il.components.Add(comp);
                                                                }
                                                                else if (el4.Value == "-")
                                                                {
                                                                    comp.addit = false;
                                                                    il.components.Add(comp);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (isDefil) system.incomelists.Add(il.name, il);
                                        }
                                    }
                                }
                            }
                        }
                        if (passed) country.systems.Add(system.year, system);
                    }
                }
                Plugin.countries.Add(country);
            }
            Plugin.prepareIncomeListInfo();
        }


        internal void readDataSTATA()
        {
            
        }

        internal void generateWebStatistics()
        {
            IWorkbook wb = spreadsheetControl.Document;
            Worksheet ws;   // a temp worksheet

/*          
            wb.LoadDocument(Plugin.outputPath);
            wb.Worksheets[0].Cells[1, 1].Value = "er";
            wb.SaveDocument(Plugin.outputPath);
            return;
 */
            wb.CreateNewDocument();

            // create the country sheets
            foreach (EM_Country country in Plugin.countries)
            {
                ws = wb.Worksheets.Add(country.shortname);
                setDefaultSheetStyle(ws);
                ws.Columns[0].WidthInPixels = 100;
            }

            // get the title years
            Dictionary<string, List<string>> tmpYears = new Dictionary<string, List<string>>();
            int minYear = 9999;
            int maxYear = 0;
            foreach (string c in Plugin.allCountries)
            {
                tmpYears.Add(c, null);
                tmpYears[c] = Directory.GetFiles(Plugin.inputPathSTATA, "webstatistics_" + c + "_*.xls").ToList<string>();
                if (tmpYears[c].Count > 0)
                {
                    for (int ty = 0; ty < tmpYears[c].Count; ty++) tmpYears[c][ty] = Path.GetFileName(tmpYears[c][ty]).ToLower().Replace("webstatistics_" + c.ToLower() + "_", "").Replace(".xls", "");
                    tmpYears[c].Sort();
                    if (minYear > int.Parse(tmpYears[c][0])) minYear = int.Parse(tmpYears[c][0]);
                    if (maxYear < int.Parse(tmpYears[c][tmpYears[c].Count - 1])) maxYear = int.Parse(tmpYears[c][tmpYears[c].Count - 1]);
                }
            }
            string titleYears = minYear + "-" + maxYear;

            // create the rest of the sheets
            createGiniSheet(wb, titleYears);
            createPovertyRiskSheet(wb, titleYears);
            createPovertyGapSheet(wb, titleYears);
            createPovertyLinesSheet(wb, titleYears);
            createNrrSheet(wb, titleYears);
            createMetrSheet(wb, titleYears);
            ws = wb.Worksheets.Add("Datasets");
            setDefaultSheetStyle(ws);
            createPopulationSheet(wb, titleYears);

            // then do the calculations
            int cnt = 0;
            int gini_row = 4;
            int risk_row = 4;
            int gap_row = 4;
            int lines_row = 4;
            int metr_row = 5;
            int nrr_row = 5;
            int pop_row = 3;
            foreach (EM_Country country in Plugin.countries)
            {
                Worksheet cws = wb.Worksheets[country.shortname];   // the country worksheet
                cnt++;
                labelControl1.Text = "Generating the Web Statistics report, please wait... (" + country.name + " - " + cnt + " of " + Plugin.countries.Count + ")";
                Refresh();
                // put the country mark in the cumulative sheets
                ws = wb.Worksheets["Gini"];
                ws.Cells[gini_row, 0].Value = country.name;
                ws = wb.Worksheets["Poverty risk"];
                ws.Cells[risk_row, 0].Value = country.name;
                ws = wb.Worksheets["Poverty gap"];
                ws.Cells[gap_row, 0].Value = country.name;
                ws = wb.Worksheets["Poverty lines"];
                ws.Cells[lines_row, 0].Value = country.name;
                ws = wb.Worksheets["NRR"];
                ws.Cells[nrr_row, 0].Value = country.name;
                ws = wb.Worksheets["METR"];
                ws.Cells[metr_row, 0].Value = country.name;
                ws = wb.Worksheets["Population"];
                ws.Cells[pop_row, 0].Value = country.name;

                // find the country files
                List<string> yearFilenames = Directory.GetFiles(Plugin.inputPathSTATA, "webstatistics_" + country.shortname + "_*.xls").ToList<string>();
                yearFilenames.Sort();
                yearFilenames.Reverse();    // from largest year to smallest
                int y = 0;
                foreach (string yr in yearFilenames)
                {
                    // keep the year no
                    string yrStr = Path.GetFileName(yr).ToLower().Replace("webstatistics_" + country.shortname.ToLower() + "_", "").Replace(".xls", "");
                    
                    spreadsheetControlReader.LoadDocument(yr);
                    Worksheet wsin = spreadsheetControlReader.Document.Worksheets[Path.GetFileName(yr).Replace(".xls", "")];

                    // add the country data
                    wb.Worksheets.ActiveWorksheet = cws;

                    int row = 5+y*30;
                    cws.Cells[0, 0].Value = country.name.ToUpper();
                    setCellStyle(cws.Cells[0, 0], "Header");
                    cws.Cells[row-3, 0].Value = yrStr + " Average monthly household income and income components per decile group, Euro (1)";
                    setCellStyle(cws.Cells[row - 3, 0], "Header");
                    cws.Range["A" + (row - 2) + ":J" + (row - 2)].Merge();
                    cws.Cells[row - 1, 0].Value = "Decile Group";
                    cws.Cells[row - 1, 1].Value = "Disposable Income";
                    cws.Cells[row - 1, 2].Value = "Original Income";
                    cws.Cells[row - 1, 3].Value = "Means-Tested Benefits";
                    cws.Cells[row - 1, 4].Value = "Non-Means-Tested Benefits";
                    cws.Cells[row - 1, 5].Value = "Public Pensions";
                    cws.Cells[row - 1, 6].Value = "All Taxes";
                    cws.Cells[row - 1, 7].Value = "Social Insurance Contrib. (SICs) (2)";
                    cws.Cells[row - 1, 8].Value = "Simulated Benefits, of All Benefits (%)";
                    cws.Cells[row - 1, 9].Value = "Simulated Taxes, of All Taxes (%)";
                    setCellStyle(cws.Range["A" + (row - 1) + ":J" + (row - 1)], "Normal", false, "", "", "b2");
                    setCellStyle(cws.Range["A" + row + ":J" + row], "Normal", true, "r", "c", "b1");
                    cws.Rows[row - 1].RowHeight = 200;
                    
                    for (int i=0; i<12; i++)
                    {
                        cws.Cells[row + i, 0].Value = (i < 10 ? (i + 1)+"" : (i == 10 ? "All" : "Poor (3)"));
                        setCellStyle(cws.Cells[row + i, 0], (i==10?"Title":"Normal"), false, "c","",(i>8?(i>10?"b2":"b1"):""));
                        for (int j=1; j<10; j++)
                        {
                            cws.Cells[row+i, j].Value = double.Parse(wsin.Cells[8+i,j].Value.ToString());
                            setCellStyle(cws.Cells[row + i, j], "Normal", false, "r", "", (i > 8 ? (i > 10 ? "b2" : "b1") : ""), "#,##0.0");
                        }
                        Refresh();
                        if (i == 9 || i == 10) cws.Range["A" + (row + i + 1) + ":J" + (row + i + 1)].Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                        if (i == 11) cws.Range["A" + (row + i + 1) + ":J" + (row + i + 1)].Borders.BottomBorder.LineStyle = BorderLineStyle.Thick;
                    }

                    row += 14;
                    cws.Cells[row++, 0].Value = "Income Components (sim. - simulated, data - non-simulated)";
                    cws.Range["A" + row + ":J" + row].Merge();
                    setCellStyle(cws.Range["A" + row + ":J" + row], "Title", false, "", "", "b1");

                    if (country.systems.ContainsKey(yrStr))
                    {
                        EM_System sys = country.systems[yrStr];

                        cws.Cells[row++, 0].Value = "original income";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists[DefVarName.ILSORIGY].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "taxes (sim.)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_taxsim"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "taxes (data)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_taxdata"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "employee SICs (sim.)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_sicee"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "self-empl. SICs (sim.)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_sicse"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "benefits (sim.)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_bensim"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                        cws.Cells[row++, 0].Value = "benefits (data)";
                        setCellStyle(cws.Cells[row - 1, 0], "SubTitle", true, "", "t", "b1");
                        cws.Range["B" + row + ":J" + row].Merge();
                        cws.Cells[row - 1, 1].Value = sys.incomelists["ils_bendata"].longComponentList;
                        setCellStyle(cws.Range["B" + row + ":J" + row], "Normal", true, "", "t", "b1");
                        setRowAutoHeight(cws, row - 1);
                    }
                    // add the Gini data
                    ws = wb.Worksheets["Gini"];
                    ws.Rows[gini_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[gini_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Range.FromLTRB(2, gini_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0.000");
                    ws.Cells[gini_row, 1].Value = yrStr;
                    ws.Cells[gini_row, 2].Value = double.Parse(wsin.Cells[23,1].Value.ToString());
                    ws.Cells[gini_row, 3].Value = double.Parse(wsin.Cells[24,1].Value.ToString());
                    ws.Cells[gini_row, 4].Value = double.Parse(wsin.Cells[25,1].Value.ToString());
                    ws.Cells[gini_row, 5].Value = double.Parse(wsin.Cells[26,1].Value.ToString());
                    ws.Cells[gini_row, 6].Value = double.Parse(wsin.Cells[27,1].Value.ToString());
                    ws.Cells[gini_row, 7].Value = double.Parse(wsin.Cells[28,1].Value.ToString());
                    ws.Cells[gini_row, 8].Value = double.Parse(wsin.Cells[29,1].Value.ToString());
                    gini_row++;

                    // add the Poverty risk data
                    ws = wb.Worksheets["Poverty risk"];
                    ws.Rows[risk_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[risk_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Range.FromLTRB(2, risk_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0.0");
                    ws.Cells[risk_row, 1].Value = yrStr;
                    ws.Cells[risk_row, 2].Value = double.Parse(wsin.Cells[37,1].Value.ToString()) * 100;
                    ws.Cells[risk_row, 3].Value = double.Parse(wsin.Cells[37,2].Value.ToString()) * 100;
                    ws.Cells[risk_row, 4].Value = double.Parse(wsin.Cells[37,3].Value.ToString()) * 100;
                    ws.Cells[risk_row, 5].Value = double.Parse(wsin.Cells[37,4].Value.ToString()) * 100;
                    ws.Cells[risk_row, 6].Value = double.Parse(wsin.Cells[37,5].Value.ToString()) * 100;
                    ws.Cells[risk_row, 7].Value = double.Parse(wsin.Cells[37,6].Value.ToString()) * 100;
                    ws.Cells[risk_row, 8].Value = double.Parse(wsin.Cells[37,7].Value.ToString()) * 100;
                    risk_row++;

                    // add the Poverty gap data
                    ws = wb.Worksheets["Poverty gap"];
                    ws.Rows[gap_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[gap_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Range.FromLTRB(2, gap_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0.0");
                    ws.Cells[gap_row, 1].Value = yrStr;
                    ws.Cells[gap_row, 2].Value = double.Parse(wsin.Cells[41,1].Value.ToString()) * 100;
                    ws.Cells[gap_row, 3].Value = double.Parse(wsin.Cells[41,2].Value.ToString()) * 100;
                    ws.Cells[gap_row, 4].Value = double.Parse(wsin.Cells[41,3].Value.ToString()) * 100;
                    ws.Cells[gap_row, 5].Value = double.Parse(wsin.Cells[41,4].Value.ToString()) * 100;
                    ws.Cells[gap_row, 6].Value = double.Parse(wsin.Cells[41,5].Value.ToString()) * 100;
                    ws.Cells[gap_row, 7].Value = double.Parse(wsin.Cells[41,6].Value.ToString()) * 100;
                    ws.Cells[gap_row, 8].Value = double.Parse(wsin.Cells[41,7].Value.ToString()) * 100;
                    gap_row++;

                    // add the Poverty lines data
                    ws = wb.Worksheets["Poverty lines"];
                    ws.Rows[lines_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[lines_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Cells[lines_row, 2], "Normal", false, "r", "b", "", "#,##0.0");
                    ws.Cells[lines_row, 1].Value = yrStr;
                    ws.Cells[lines_row, 2].Value = double.Parse(wsin.Cells[33,1].Value.ToString());
                    lines_row++;
                                   
                    // add the METR data
                    ws = wb.Worksheets["METR"];
                    ws.Rows[risk_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[gini_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Range.FromLTRB(2, gini_row, 5, gini_row), "Normal", false, "r", "b", "", "#,##0.00");
                    setCellStyle(ws.Range.FromLTRB(5, gini_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0");
                    ws.Cells[metr_row, 1].Value = yrStr;
                    ws.Cells[metr_row, 2].Value = double.Parse(wsin.Cells[45,1].Value.ToString());
                    ws.Cells[metr_row, 3].Value = double.Parse(wsin.Cells[45,2].Value.ToString());
                    ws.Cells[metr_row, 4].Value = double.Parse(wsin.Cells[45,3].Value.ToString());
                    ws.Cells[metr_row, 5].Value = double.Parse(wsin.Cells[45,4].Value.ToString());
                    ws.Cells[metr_row, 6].Value = double.Parse(wsin.Cells[45,5].Value.ToString());
                    ws.Cells[metr_row, 7].Value = double.Parse(wsin.Cells[45,6].Value.ToString());
                    ws.Cells[metr_row, 8].Value = double.Parse(wsin.Cells[45,7].Value.ToString());
                    metr_row++;

                    // add the NRR data
                    ws = wb.Worksheets["NRR"];
                    ws.Rows[risk_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[gini_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Range.FromLTRB(2, gini_row, 5, gini_row), "Normal", false, "r", "b", "", "#,##0.00");
                    setCellStyle(ws.Range.FromLTRB(5, gini_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0");
                    ws.Cells[nrr_row, 1].Value = yrStr;
                    ws.Cells[nrr_row, 2].Value = double.Parse(wsin.Cells[49, 1].Value.ToString());
                    ws.Cells[nrr_row, 3].Value = double.Parse(wsin.Cells[49, 2].Value.ToString());
                    ws.Cells[nrr_row, 4].Value = double.Parse(wsin.Cells[49, 3].Value.ToString());
                    ws.Cells[nrr_row, 5].Value = double.Parse(wsin.Cells[49, 4].Value.ToString());
                    ws.Cells[nrr_row, 6].Value = double.Parse(wsin.Cells[49, 5].Value.ToString());
                    ws.Cells[nrr_row, 7].Value = double.Parse(wsin.Cells[49, 6].Value.ToString());
                    ws.Cells[nrr_row, 8].Value = double.Parse(wsin.Cells[49, 7].Value.ToString());
                    nrr_row++;

                    // add the Population data
                    ws = wb.Worksheets["Population"];
                    ws.Rows[pop_row].RowHeight *= 0.90;
                    setCellStyle(ws.Cells[pop_row, 1], "Normal", false, "c", "b");
                    setCellStyle(ws.Cells[pop_row, 2], "Normal", false, "r", "b", "", "#,##0");
                    setCellStyle(ws.Cells[pop_row, 3], "Normal", false, "r", "b", "", "#,##0.0%");
                    ws.Cells[pop_row, 1].Value = yrStr;
                    ws.Cells[pop_row, 2].Value = double.Parse(wsin.Cells[33,2].Value.ToString());
                    pop_row++;

                    y++;
                }
                wb.Worksheets["Gini"].Range.FromLTRB(0, gini_row - 1, 8, gini_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["Poverty risk"].Range.FromLTRB(0, risk_row - 1, 8, risk_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["Poverty gap"].Range.FromLTRB(0, gap_row - 1, 8, gap_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["Poverty lines"].Range.FromLTRB(0, lines_row - 1, 2, lines_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["NRR"].Range.FromLTRB(0, nrr_row - 1, 8, nrr_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["METR"].Range.FromLTRB(0, metr_row - 1, 8, metr_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                wb.Worksheets["Population"].Range.FromLTRB(0, pop_row - 1, 3, pop_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;


                // add the Notes in the Country files
                int lastRow = cws.Rows.LastUsedIndex;
                cws.Cells[lastRow + 3, 0].Value = "Notes:";
                setCellStyle(cws.Cells[lastRow + 3, 0], "Title");
                cws.Cells[lastRow + 4, 0].Value = "1. The categories of income components chosen for these tables are simply for illustrative purposes. The categorisation of instruments is an area where EUROMOD offers a high degree of flexibility which is needed if results are to conform to different conventions and are to be used for a range of purposes. June " + titleYears + " market exchange rates are used for non-euro countries.";
                cws.Range["A" + (lastRow + 5) + ":J" + (lastRow + 5)].Merge();
                setCellStyle(cws.Range["A" + (lastRow + 5) + ":J" + (lastRow + 5)], "Normal", true);
                setRowAutoHeight(cws, lastRow + 4, 0, 9);
                cws.Cells[lastRow + 5, 0].Value = "2. Social insurance contributions refer here to the sum of employee and self-employed contributions and all benefits also include public pensions.";
                cws.Cells[lastRow + 6, 0].Value = "3. Poor: households at risk of being in poverty, i.e., with equivalised disposable income below 60% of the median.";
                cws.Cells[lastRow + 8, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
                cws.Cells[lastRow + 9, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();
                cws.Cells[lastRow + 9, 0].Font.Italic = true;
            }


            // then do the EU28 sums & Notes for Gini
            ws = wb.Worksheets["Gini"];
            ws.Cells[gini_row, 0].Value = "EU-28";
            for (int y = maxYear; y >= minYear; y--)
            {
                // keep the year no
                string yrStr = y.ToString();

                ws.Cells[gini_row, 1].Value = yrStr;
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                string str5 = "";
                string str6 = "";
                string str7 = "";
                for (int i = 4; i < gini_row; i++)
                {
                    if (ws.Cells[i, 1].Value.ToString() == yrStr)
                    {
                        str1 += "+C" + (i + 1) + "*Population!D" + i;
                        str2 += "+D" + (i + 1) + "*Population!D" + i;
                        str3 += "+E" + (i + 1) + "*Population!D" + i;
                        str4 += "+F" + (i + 1) + "*Population!D" + i;
                        str5 += "+G" + (i + 1) + "*Population!D" + i;
                        str6 += "+H" + (i + 1) + "*Population!D" + i;
                        str7 += "+I" + (i + 1) + "*Population!D" + i;
                    }
                }
                ws.Cells[gini_row, 2].Formula = "=" + str1.Substring(1);
                ws.Cells[gini_row, 3].Formula = "=" + str2.Substring(1);
                ws.Cells[gini_row, 4].Formula = "=" + str3.Substring(1);
                ws.Cells[gini_row, 5].Formula = "=" + str4.Substring(1);
                ws.Cells[gini_row, 6].Formula = "=" + str5.Substring(1);
                ws.Cells[gini_row, 7].Formula = "=" + str6.Substring(1);
                ws.Cells[gini_row, 8].Formula = "=" + str7.Substring(1);
                setCellStyle(ws.Cells[gini_row, 1], "Normal", false, "c", "b"); 
                setCellStyle(ws.Range.FromLTRB(2, gini_row, 8, gini_row), "Normal", false, "r", "b", "", "#,##0.000");
                ws.Rows[gini_row].RowHeight *= 0.90;
                gini_row++;
            }
            ws.Range.FromLTRB(0, gini_row - 1, 8, gini_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            gini_row++;
            ws.Cells[gini_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[gini_row, 0], "Title");
            ws.Cells[gini_row + 1, 0].Value = "The tables show what happens to the Gini coefficient of disposable income if each income component is added back (in the case of taxes) or deducted (in the case of benefits), in turn.";
            ws.Range.FromLTRB(0, gini_row + 1, 6, gini_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, gini_row + 1, 6, gini_row), "Normal", true); 
            setRowAutoHeight(ws, gini_row + 1, 0, 6);
            ws.Cells[gini_row + 2, 0].Value = "Changes between years and tax-benefit components are not necessarily statistically significant.";
            ws.Range.FromLTRB(0, gini_row + 2, 4, gini_row + 2).Merge();
            ws.Cells[gini_row + 3, 0].Value = "In the calculation of the Gini coefficients negative income has been recoded to zero.";
            ws.Range.FromLTRB(0, gini_row + 3, 4, gini_row + 3).Merge();
            ws.Cells[gini_row + 5, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[gini_row + 6, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();


            // then do the EU28 sums & Notes for Poverty risk
            ws = wb.Worksheets["Poverty risk"];
            ws.Cells[risk_row, 0].Value = "EU-28";
            for (int y = maxYear; y >= minYear; y--)
            {
                // keep the year no
                string yrStr = y.ToString();

                ws.Cells[risk_row, 1].Value = yrStr;
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                string str5 = "";
                string str6 = "";
                string str7 = "";
                for (int i = 3; i < risk_row; i++)
                {
                    if (ws.Cells[i, 1].Value.ToString() == yrStr)
                    {
                        str1 += "+C" + (i + 1) + "*Population!D" + i;
                        str2 += "+D" + (i + 1) + "*Population!D" + i;
                        str3 += "+E" + (i + 1) + "*Population!D" + i;
                        str4 += "+F" + (i + 1) + "*Population!D" + i;
                        str5 += "+G" + (i + 1) + "*Population!D" + i;
                        str6 += "+H" + (i + 1) + "*Population!D" + i;
                        str7 += "+I" + (i + 1) + "*Population!D" + i;
                    }
                }
                ws.Cells[risk_row, 2].Formula = "=" + str1.Substring(1);
                ws.Cells[risk_row, 3].Formula = "=" + str2.Substring(1);
                ws.Cells[risk_row, 4].Formula = "=" + str3.Substring(1);
                ws.Cells[risk_row, 5].Formula = "=" + str4.Substring(1);
                ws.Cells[risk_row, 6].Formula = "=" + str5.Substring(1);
                ws.Cells[risk_row, 7].Formula = "=" + str6.Substring(1);
                ws.Cells[risk_row, 8].Formula = "=" + str7.Substring(1);
                setCellStyle(ws.Cells[risk_row, 1], "Normal", false, "c", "b");
                setCellStyle(ws.Range.FromLTRB(2, risk_row, 8, risk_row), "Normal", false, "r", "b", "", "#,##0.0");

                ws.Rows[risk_row].RowHeight *= 0.90;
                risk_row++;
            }
            ws.Range.FromLTRB(0, risk_row - 1, 8, risk_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            risk_row++;
            ws.Cells[risk_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[risk_row, 0], "Title");
            ws.Cells[risk_row + 1, 0].Value = "Poverty risk is the percentage of people in households with equivalised disposable income below the national poverty threshold. The poverty threshold is 60% of the median equivalised disposable income. ";
            ws.Range.FromLTRB(0, risk_row + 1, 5, risk_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, risk_row + 1, 5, risk_row), "Normal", true);
            setRowAutoHeight(ws, risk_row + 1, 0, 5);
            ws.Cells[risk_row + 3, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[risk_row + 4, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();


            // then do the EU28 sums & Notes for Poverty gap
            ws = wb.Worksheets["Poverty gap"];
            ws.Cells[gap_row, 0].Value = "EU-28";
            for (int y = maxYear; y >= minYear; y--)
            {
                // keep the year no
                string yrStr = y.ToString();

                ws.Cells[gap_row, 1].Value = yrStr;
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                string str5 = "";
                string str6 = "";
                string str7 = "";
                for (int i = 3; i < gap_row; i++)
                {
                    if (ws.Cells[i, 1].Value.ToString() == yrStr)
                    {
                        str1 += "+C" + (i + 1) + "*Population!D" + i;
                        str2 += "+D" + (i + 1) + "*Population!D" + i;
                        str3 += "+E" + (i + 1) + "*Population!D" + i;
                        str4 += "+F" + (i + 1) + "*Population!D" + i;
                        str5 += "+G" + (i + 1) + "*Population!D" + i;
                        str6 += "+H" + (i + 1) + "*Population!D" + i;
                        str7 += "+I" + (i + 1) + "*Population!D" + i;
                    }
                }
                ws.Cells[gap_row, 2].Formula = "=" + str1.Substring(1);
                ws.Cells[gap_row, 3].Formula = "=" + str2.Substring(1);
                ws.Cells[gap_row, 4].Formula = "=" + str3.Substring(1);
                ws.Cells[gap_row, 5].Formula = "=" + str4.Substring(1);
                ws.Cells[gap_row, 6].Formula = "=" + str5.Substring(1);
                ws.Cells[gap_row, 7].Formula = "=" + str6.Substring(1);
                ws.Cells[gap_row, 8].Formula = "=" + str7.Substring(1);
                setCellStyle(ws.Cells[gap_row, 1], "Normal", false, "c", "b");
                setCellStyle(ws.Range.FromLTRB(2, gap_row, 8, gap_row), "Normal", false, "r", "b", "", "#,##0.0");
                ws.Rows[gap_row].RowHeight *= 0.90;
                gap_row++;
            }
            ws.Range.FromLTRB(0, gap_row - 1, 8, gap_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            gap_row++;
            ws.Cells[gap_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[gap_row, 0], "Title");
            ws.Cells[gap_row + 1, 0].Value = "The relative median poverty gap is the difference of the poverty threshold and the median equivalised income of persons in households with income below the poverty threshold, expressed as a proportion of the poverty threshold. The poverty threshold is 60% of the median equivalised disposable income.";
            ws.Range.FromLTRB(0, gap_row + 1, 5, gap_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, gap_row + 1, 5, gap_row), "Normal", true);
            setRowAutoHeight(ws, gap_row + 1, 0, 5);
            ws.Cells[gap_row + 3, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[gap_row + 4, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();
            

            // then do the Notes for Poverty lines
            ws = wb.Worksheets["Poverty lines"];
            lines_row++;
            ws.Cells[lines_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[lines_row, 0], "Title");
            ws.Cells[lines_row + 1, 0].Value = "1. Poverty thresholds are set at 60% of the median equivalised household disposable income, which is formed using the modified OECD equivalence scale and weighted by household size.";
            ws.Range.FromLTRB(0, lines_row + 1, 4, lines_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, lines_row + 1, 4, lines_row + 1), "Normal", true);
            setRowAutoHeight(ws, lines_row + 1, 0, 4);
            ws.Cells[lines_row + 2, 0].Value = "2. June " + titleYears + " market exchange rates are used for non-euro countries.";
            ws.Range.FromLTRB(0, lines_row + 2, 4, lines_row + 2).Merge();
            ws.Cells[lines_row + 4, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[lines_row + 5, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();

            // then do the Notes for NRR
            ws = wb.Worksheets["NRR"];
            nrr_row++;
            ws.Cells[nrr_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[nrr_row, 0], "Title");
            ws.Cells[nrr_row + 1, 0].Value = "The NRR Add-On simulates disposable income in case of unemployment for all people currently in work (i.e. all people observed with positive earnings in the data). The net replacement rate measures the proportion of household disposable income that is maintained after an individual gets unemployment. The net replacement rate can be negative (e.g. in case of an obligatory SIC) or exceed 100% (indicating that a person is better off if not working).";
            ws.Range.FromLTRB(0, nrr_row + 1, 5, nrr_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, nrr_row + 1, 5, nrr_row + 1), "Normal", true);
            setRowAutoHeight(ws, nrr_row + 1, 0, 5);
            ws.Cells[nrr_row + 3, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[nrr_row + 4, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();

            // then do the Notes for METR
            ws = wb.Worksheets["METR"];
            lines_row++;
            ws.Cells[lines_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[lines_row, 0], "Title");
            ws.Cells[lines_row + 1, 0].Value = "METRs are calculated for all individuals with earned income, taking account of the effect of earning 3% more such income (in gross terms) on their household disposable income. The calculations include some zero values (e.g. for people earning small amounts, below tax and contribution thresholds and in households with other income, making them ineligible for any means-tested benefit that might be withdrawn). They also include some very high values, exceeding 100%, corresponding to situations where people are near discontinuities in the tax-benefit schedules.";
            ws.Range.FromLTRB(0, lines_row + 1, 5, lines_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, lines_row + 1, 5, lines_row + 1), "Normal", true);
            setRowAutoHeight(ws, lines_row + 1, 0, 5);
            ws.Cells[lines_row + 3, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[lines_row + 4, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();

            // then do the EU28 sums, percentages & Notes for Population
            ws = wb.Worksheets["Population"];
            ws.Cells[pop_row, 0].Value = "EU-28";
            for (int y = maxYear; y >= minYear; y--)
            {
                // keep the year no
                string yrStr = y.ToString();

                ws.Cells[pop_row, 1].Value = yrStr;
                string str1 = "";
                for (int i = 3; i < pop_row; i++)
                {
                    if (ws.Cells[i, 1].Value.ToString() == yrStr)
                    {
                        str1 += "+C" + (i + 1);
                        ws.Cells[i, 3].Formula = "=C" + (i + 1) + "/C" + (pop_row + 1);
                    }
                }
                ws.Cells[pop_row, 2].Formula = "=" + str1.Substring(1);
                ws.Cells[pop_row, 3].Value = "100%";
                ws.Rows[pop_row].RowHeight *= 0.90;
                setCellStyle(ws.Cells[pop_row, 1], "Normal", false, "c", "b");
                setCellStyle(ws.Cells[pop_row, 2], "Normal", false, "r", "b", "", "#,##0");
                setCellStyle(ws.Cells[pop_row, 3], "Normal", false, "r", "b", "", "#,##0.0%");
                pop_row++;
            }
            ws.Range.FromLTRB(0, pop_row - 1, 3, pop_row - 1).Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            pop_row++;
            ws.Cells[pop_row, 0].Value = "Notes:";
            setCellStyle(ws.Cells[pop_row, 0], "Title");
            ws.Cells[pop_row + 1, 0].Value = "1. Population figures correspond to the EU-SILC datasets used for each policy year.";
            ws.Range.FromLTRB(0, pop_row + 1, 4, pop_row + 1).Merge();
            setCellStyle(ws.Range.FromLTRB(0, pop_row + 1, 4, pop_row), "Normal", true);
            ws.Cells[pop_row + 3, 0].Value = "Source: EUROMOD version no. " + Plugin.euromod_version;
            ws.Cells[pop_row + 4, 0].Value = "Last updated " + DateTime.Now.ToShortDateString();
            wb.Worksheets.Remove(wb.Worksheets["Sheet1"]);
            wb.Worksheets.ActiveWorksheet = wb.Worksheets[0];
            foreach (Worksheet w in wb.Worksheets)
                for (int c = 0; c <= w.Columns.LastUsedIndex; c++)
                {
                    w.Columns[c].Visible = true;
                    w.Columns[c].WidthInPixels = 100;
                }
            wb.SaveDocument(Plugin.outputPath);
        }

        private void setRowAutoHeight(Worksheet ws, int row, int startCol = 1, int endCol = 9)
        {
            int wip = ws.Columns[endCol +2].WidthInPixels;
            int twip = 0;
            for (int i = startCol; i <= endCol ; i++) twip += ws.Columns[i].WidthInPixels;
            ws.Columns[endCol + 2].WidthInPixels = twip;
            ws.Cells[row, endCol + 2].Value = ws.Cells[row, startCol].Value.TextValue;
            ws.Cells[row, endCol + 2].Alignment.WrapText = true;
            ws.Rows.AutoFit(row, row);
            ws.Cells[row, endCol + 2].Clear();
            ws.Columns[endCol + 2].WidthInPixels = wip;
            ws.Rows[row].RowHeight *= 1.15;   // this accommodates for the error in the AutoFit function
        }

        private void setCellStyle(Formatting r, string font, bool wrap = false, string hor = "", string ver = "", string bor = "", string num = "")
        {
            r.Font.Name = "Arial";
            switch (font)
            {
                case "Header":
                    r.Font.Bold = true;
                    r.Font.Size = 11;
                    r.Font.Color = Color.DarkBlue;
                    break;
                case "Title":
                    r.Font.Bold = true;
                    r.Font.Size = 8;
                    r.Font.Color = Color.DarkBlue;
                    break;
                case "SubTitle":
                    r.Font.Bold = false;
                    r.Font.Size = 8;
                    r.Font.Color = Color.DarkBlue;
                    break;
                case "Bold":
                    r.Font.Bold = true;
                    r.Font.Size = 8;
                    r.Font.Color = Color.Black;
                    break;
                case "Red":
                    r.Font.Bold = false;
                    r.Font.Size = 8;
                    r.Font.Color = Color.Red;
                    break;
                case "Normal":
                default:
                    r.Font.Bold = false;
                    r.Font.Size = 8;
                    r.Font.Color = Color.Black;
                    break;
            }
            switch (hor)
            {
                case "l":
                    r.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
                    break;
                case "c":
                    r.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    break;
                case "r":
                    r.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                    break;
                default:
                    r.Alignment.Horizontal = SpreadsheetHorizontalAlignment.General;
                    break;
            }
            switch (ver)
            {
                case "t":
                    r.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
                    break;
                case "c":
                    r.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                    break;
                case "b":
                default:
                    r.Alignment.Vertical = SpreadsheetVerticalAlignment.Bottom;
                    break;
            }
            switch (bor)
            {
                case "b1":
                    r.Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
                    break;
                case "b2":
                    r.Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
                    break;
                case "b3":
                    r.Borders.BottomBorder.LineStyle = BorderLineStyle.Thick;
                    break;
                default:
//                    r.Borders.BottomBorder.LineStyle = BorderLineStyle.None;
                    break;
            }
            if (num != "")
            {
                r.Flags.Number = true;
                r.NumberFormat = num;
            }
            r.Alignment.WrapText = wrap;
        }

        private void setDefaultSheetStyle(Worksheet ws)
        {
            setCellStyle(ws.Cells, "Normal");
            ws.DefaultColumnWidthInPixels = 86;
        }

        private void createGiniSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("Gini");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Effects of tax-benefit components on inequality (Gini index), " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:I2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A4"].Merge();
            setCellStyle(ws.Range["A3:A4"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B4"].Merge();
            setCellStyle(ws.Range["B3:B4"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Gini index";
            ws.Range["C3:I3"].Merge();
            setCellStyle(ws.Range["C3:I3"], "Normal", false, "c", "c", "b1");
            ws.Cells[3, 2].Value = "Disposable income (DPI)";
            setCellStyle(ws.Cells[3, 2], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 3].Value = "DPI less means-tested benefits";
            setCellStyle(ws.Cells[3, 3], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 4].Value = "DPI less non means-tested benefits";
            setCellStyle(ws.Cells[3, 4], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 5].Value = "DPI plus direct taxes";
            setCellStyle(ws.Cells[3, 5], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 6].Value = "DPI plus Social Insurance Contrib.";
            setCellStyle(ws.Cells[3, 6], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 7].Value = "Original Income";
            setCellStyle(ws.Cells[3, 7], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 8].Value = "Original Income plus pensions";
            setCellStyle(ws.Cells[3, 8], "Normal", true, "r", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Columns[0].WidthInPixels = 120;
        }

        private void createPovertyRiskSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("Poverty risk");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Effects of tax-benefit components on poverty risk, " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:I2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A4"].Merge();
            setCellStyle(ws.Range["A3:A4"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B4"].Merge();
            setCellStyle(ws.Range["B3:B4"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Poverty risk (%)";
            ws.Range["C3:I3"].Merge();
            setCellStyle(ws.Range["C3:I3"], "Normal", false, "c", "c", "b1");
            ws.Cells[3, 2].Value = "Disposable income (DPI)";
            setCellStyle(ws.Cells[3, 2], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 3].Value = "DPI less means-tested benefits";
            setCellStyle(ws.Cells[3, 3], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 4].Value = "DPI less non means-tested benefits";
            setCellStyle(ws.Cells[3, 4], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 5].Value = "DPI plus direct taxes";
            setCellStyle(ws.Cells[3, 5], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 6].Value = "DPI plus Social Insurance Contrib.";
            setCellStyle(ws.Cells[3, 6], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 7].Value = "Original Income";
            setCellStyle(ws.Cells[3, 7], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 8].Value = "Original Income plus pensions";
            setCellStyle(ws.Cells[3, 8], "Normal", true, "r", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Columns[0].WidthInPixels = 120;
        }

        private void createPovertyGapSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("Poverty gap");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Effects of tax-benefit components on poverty gap, " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:I2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A4"].Merge();
            setCellStyle(ws.Range["A3:A4"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B4"].Merge();
            setCellStyle(ws.Range["B3:B4"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Poverty gap (%)";
            ws.Range["C3:I3"].Merge();
            setCellStyle(ws.Range["C3:I3"], "Normal", false, "c", "c", "b1");
            ws.Cells[3, 2].Value = "Disposable income (DPI)";
            setCellStyle(ws.Cells[3, 2], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 3].Value = "DPI less means-tested benefits";
            setCellStyle(ws.Cells[3, 3], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 4].Value = "DPI less non means-tested benefits";
            setCellStyle(ws.Cells[3, 4], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 5].Value = "DPI plus direct taxes";
            setCellStyle(ws.Cells[3, 5], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 6].Value = "DPI plus Social Insurance Contrib.";
            setCellStyle(ws.Cells[3, 6], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 7].Value = "Original Income";
            setCellStyle(ws.Cells[3, 7], "Normal", true, "r", "c", "b2");
            ws.Cells[3, 8].Value = "Original Income plus pensions";
            setCellStyle(ws.Cells[3, 8], "Normal", true, "r", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Columns[0].WidthInPixels = 120;
        }

        private void createPovertyLinesSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("Poverty lines");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Poverty thresholds (1), " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:C2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A4"].Merge();
            setCellStyle(ws.Range["A3:A4"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B4"].Merge();
            setCellStyle(ws.Range["B3:B4"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Poverty lines";
            setCellStyle(ws.Cells[2, 2], "Bold", false, "r", "c", "b1");
            ws.Cells[3, 2].Value = "Euro (2)";
            setCellStyle(ws.Cells[3, 2], "Normal", false, "r", "c", "b2");
            ws.Rows[2].RowHeight *= 1.3;
            ws.Rows[3].RowHeight *= 1.3;
            ws.Columns[0].WidthInPixels = 120;
            ws.Columns[2].WidthInPixels = 120;
        }

        private void createMetrSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("METR");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Marginal Effective Tax Rates, " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:I2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A5"].Merge();
            setCellStyle(ws.Range["A3:A5"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B5"].Merge();
            setCellStyle(ws.Range["B3:B5"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Marginal Effective Tax Rates (METR)";
            ws.Range["C3:F3"].Merge();
            setCellStyle(ws.Range["C3:F3"], "Bold", false, "c", "c", "b1");
            ws.Range["G3:I3"].Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            ws.Cells[3, 2].Value = "mean";
            ws.Range["C4:C5"].Merge();
            setCellStyle(ws.Range["C4:C5"], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 3].Value = "median";
            ws.Range["D4:D5"].Merge();
            setCellStyle(ws.Range["D4:D5"], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 4].Value = "percentiles";
            ws.Range["E4:F4"].Merge();
            setCellStyle(ws.Range["E4:F4"], "Normal", false, "c", "c");
            ws.Cells[4, 4].Value = "25%";
            setCellStyle(ws.Cells[4, 4], "Normal", false, "r", "c", "b2");
            ws.Cells[4, 5].Value = "75%";
            setCellStyle(ws.Cells[4, 5], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 6].Value = "active population";
            setCellStyle(ws.Cells[3, 6], "Red", false, "c", "c");
            ws.Cells[4, 6].Value = "unweighted";
            setCellStyle(ws.Cells[4, 6], "Red", false, "c", "c", "b2");
            ws.Cells[3, 7].Value = "numb. Obs.";
            setCellStyle(ws.Cells[3, 7], "Red", false, "c", "c");
            ws.Cells[4, 7].Value = "METR<0";
            setCellStyle(ws.Cells[4, 7], "Red", false, "c", "c", "b2");
            ws.Cells[3, 8].Value = "numb. Obs.";
            setCellStyle(ws.Cells[3, 8], "Red", false, "c", "c");
            ws.Cells[4, 8].Value = "METR>100";
            setCellStyle(ws.Cells[4, 8], "Red", false, "c", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Rows[3].RowHeight *= 1.2;
            ws.Rows[4].RowHeight *= 1.2;
            ws.Columns[0].WidthInPixels = 120;
            ws.Columns[6].WidthInPixels = 120;
            ws.Columns[7].WidthInPixels = 100;
            ws.Columns[8].WidthInPixels = 100;
        }

        private void createNrrSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("NRR");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Net Replacement Rate, " + titleYears + " policies";
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:I2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            ws.Range["A3:A5"].Merge();
            setCellStyle(ws.Range["A3:A5"], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            ws.Range["B3:B5"].Merge();
            setCellStyle(ws.Range["B3:B5"], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Net Replacement Rate (NRR)";
            ws.Range["C3:F3"].Merge();
            setCellStyle(ws.Range["C3:F3"], "Bold", false, "c", "c", "b1");
            ws.Range["G3:I3"].Borders.BottomBorder.LineStyle = BorderLineStyle.Thin;
            ws.Cells[3, 2].Value = "mean";
            ws.Range["C4:C5"].Merge();
            setCellStyle(ws.Range["C4:C5"], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 3].Value = "median";
            ws.Range["D4:D5"].Merge();
            setCellStyle(ws.Range["D4:D5"], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 4].Value = "percentiles";
            ws.Range["E4:F4"].Merge();
            setCellStyle(ws.Range["E4:F4"], "Normal", false, "c", "c");
            ws.Cells[4, 4].Value = "25%";
            setCellStyle(ws.Cells[4, 4], "Normal", false, "r", "c", "b2");
            ws.Cells[4, 5].Value = "75%";
            setCellStyle(ws.Cells[4, 5], "Normal", false, "r", "c", "b2");
            ws.Cells[3, 6].Value = "active population";
            setCellStyle(ws.Cells[3, 6], "Red", false, "c", "c");
            ws.Cells[4, 6].Value = "unweighted";
            setCellStyle(ws.Cells[4, 6], "Red", false, "c", "c", "b2");
            ws.Cells[3, 7].Value = "numb. Obs.";
            setCellStyle(ws.Cells[3, 7], "Red", false, "c", "c");
            ws.Cells[4, 7].Value = "NRR<0";
            setCellStyle(ws.Cells[4, 7], "Red", false, "c", "c", "b2");
            ws.Cells[3, 8].Value = "numb. Obs.";
            setCellStyle(ws.Cells[3, 8], "Red", false, "c", "c");
            ws.Cells[4, 8].Value = "NRR>100";
            setCellStyle(ws.Cells[4, 8], "Red", false, "c", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Rows[3].RowHeight *= 1.2;
            ws.Rows[4].RowHeight *= 1.2;
            ws.Columns[0].WidthInPixels = 120;
            ws.Columns[6].WidthInPixels = 120;
            ws.Columns[7].WidthInPixels = 100;
            ws.Columns[8].WidthInPixels = 100;
        }

        private void createPopulationSheet(IWorkbook wb, string titleYears)
        {
            Worksheet ws = wb.Worksheets.Add("Population");
            setDefaultSheetStyle(ws);
            ws.Cells[0, 0].Value = "Population, " + titleYears;
            setCellStyle(ws.Cells[0, 0], "Header");
            ws.Range["A2:D2"].Borders.BottomBorder.LineStyle = BorderLineStyle.Medium;
            ws.Cells[2, 0].Value = "Countries";
            setCellStyle(ws.Cells[2, 0], "Normal", false, "l", "c", "b2");
            ws.Cells[2, 1].Value = "Policy Year";
            setCellStyle(ws.Cells[2, 1], "Normal", false, "c", "c", "b2");
            ws.Cells[2, 2].Value = "Population (1)";
            setCellStyle(ws.Cells[2, 2], "Bold", false, "r", "c", "b2");
            ws.Cells[2, 3].Value = "% of EU population";
            setCellStyle(ws.Cells[2, 3], "Bold", false, "r", "c", "b2");
            ws.Rows[2].RowHeight *= 2;
            ws.Columns[0].WidthInPixels = 120;
            ws.Columns[2].WidthInPixels = 120;
            ws.Columns[3].WidthInPixels = 140;
        }

    }
}
