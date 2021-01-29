using System;
using System.Collections.Generic;
using System.Text;

namespace EM_Statistics
{
    public class DisplayResults
    {
        public DisplayInfo info = new DisplayInfo();
        public List<DisplayPage> displayPages = new List<DisplayPage>();
        public bool prepared = false;
        public bool calculated = false;

        public class DisplayInfo
        {
            public string title = string.Empty;
            public string subtitle = string.Empty;
            public string button = string.Empty;
            public string description = string.Empty;
            internal HardDefinitions.ExportDescriptionMode exportDescriptionMode = HardDefinitions.ExportDescriptionMode.No;
        }

        public class DisplayPage
        {
            public string name = string.Empty;
            public string title = string.Empty;
            public string subtitle = string.Empty;
            public string button = string.Empty;
            public string description = string.Empty;
            public bool visible;
            public readonly string key = Guid.NewGuid().ToString();
            public List<DisplayTable> displayTables = null;

            public class DisplayTable
            {
                public string name = string.Empty;
                public string title = string.Empty;
                public string subtitle = string.Empty;
                public string button = string.Empty;
                public string stringFormat = string.Empty;
                public string description = string.Empty;
                public bool visible;
                public readonly string key = Guid.NewGuid().ToString();
                public List<DisplayColumn> columns = null;
                public List<DisplayRow> rows = null;
                public List<List<DisplayCell>> cells = null;
                public DisplayGraph graph; // There is no processing of the graph info, so no need for a separate template class

                public string ToTabSeparated(bool formattedValues)
                {
                    StringBuilder txtTable = new StringBuilder();

                    foreach (DisplayColumn c in columns)
                        txtTable.Append('\t').Append(c.title);  // First tab is for the top-left empty cell
                    txtTable.Append(Environment.NewLine);

                    for (int i = 0; i < rows.Count; i++)
                    {
                        txtTable.Append(rows[i].title);
                        for (int j = 0; j < cells[i].Count; j++)
                            txtTable.Append('\t').Append(formattedValues ? cells[i][j].displayValue : cells[i][j].value.ToString());
                        txtTable.Append(Environment.NewLine);
                    }
                    return txtTable.ToString();
                }

                public class DisplayColumn : DisplayTableElement
                {
                    public string title = string.Empty;
                    public bool hasSeparatorBefore = false;
                    public bool hasSeparatorAfter = false;
                }

                public class DisplayRow : DisplayTableElement
                {
                    public string title = string.Empty;
                    public bool hasSeparatorBefore = false;
                    public bool hasSeparatorAfter = false;
                }

                public class DisplayCell : DisplayTableElement
                {
                    public enum SDC_STATUS { NONE, PRIMARY, SECONDARY }

                    public string displayValue = string.Empty;
                    public double value;
                    public int sdcObsNo;
                    public List<string> secondarySdcGroups = new List<string>();
                    public SDC_STATUS sdcStatus = SDC_STATUS.NONE;
                    internal bool isStringValue = false; // for exporting: if true, show displayValue instead of value in Excel-table
                }

                public class DisplayTableElement
                {
                    public bool strong = false;
                    public string stringFormat = string.Empty;
                    public string tooltip = string.Empty;
                    public string foregroundColour = string.Empty;
                    public string backgroundColour = string.Empty;
                }

                public class DisplayGraph
                {
                    public string title = "Graph Title";
                    public bool showTable = false;
                    public bool seriesInRows = true;
                    public Axis axisX = null;
                    public Axis axisY = null;
                    public Legend legend = null;
                    public List<Series> allSeries = new List<Series>();

                    public class Series
                    {
                        public bool visible = true;
                        public string name = string.Empty;
                        public string type = string.Empty;
                        public string colour = string.Empty;
                        public int size = int.MinValue;
                        public string markerStyle = string.Empty;
                    }

                    public class Axis
                    {
                        public string valuesFrom = string.Empty;
                        public bool startFromZero = true;
                        public string label = string.Empty;
                        public int interval = int.MinValue;
                        public string labelDocking = "bottom";
                    }

                    public class Legend
                    {
                        public string title = string.Empty; // not supported in the new presenter!
                        public string docking = "right";
                        public bool visible = true;
                    }
                }
            }
        }
    }
}
