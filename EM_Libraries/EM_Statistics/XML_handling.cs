using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace EM_Statistics
{
    public static class XML_handling
    {
        public static bool ParseTemplateInfo(string path, out Template.TemplateInfo templateInfo, out ErrorCollector errorCollector)
        {
            errorCollector = new ErrorCollector(); templateInfo = new Template.TemplateInfo();
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(path, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                {
                    // first element must always be the TemplateInfo!
                    while (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "TemplateInfo")
                        if (!xmlReader.Read()) { errorCollector.AddError("<TemplateInfo> not found!"); return false; }
                    XElement xeInfo = XElement.ReadFrom(xmlReader) as XElement;
                    if (xeInfo == null || xeInfo.Name != "TemplateInfo") { errorCollector.AddError("<TemplateInfo> not found!"); return false; }
                    templateInfo = ReadInfo(xeInfo, errorCollector);
                    return true; // the only error ReadInfo may produce (except an exception which is caught below) is unknown element, which does no harm
                }
            }
            catch (Exception exception) { errorCollector.AddError(exception.Message); return false; }
        }

        public static bool ParseTemplate(string path, out Template template, out ErrorCollector errorCollector)
        {
            errorCollector = new ErrorCollector(); template = new Template();
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(path, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                {
                    xmlReader.Read();
                    while (xmlReader.NodeType != XmlNodeType.None && (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "Template")) xmlReader.Read();
                    if (xmlReader.NodeType == XmlNodeType.None) { errorCollector.AddError("Invalid Xml-structure!"); return false; }
                    if (!xmlReader.ReadToDescendant("TemplateInfo")) { errorCollector.AddError("<TemplateInfo> not found!"); return false; }
                    XElement infoElement = XElement.ReadFrom(xmlReader) as XElement;
                    if (infoElement == null || infoElement.Name != "TemplateInfo") { errorCollector.AddError("<TemplateInfo>: invalid Xml-structure!"); return false; }
                    template.info = ReadInfo(infoElement, errorCollector);

                    if (!xmlReader.ReadToNextSibling("Globals")) { errorCollector.AddError("<Globals> not found!"); return false; }
                    XElement globalElement = XElement.ReadFrom(xmlReader) as XElement;
                    if (globalElement == null || globalElement.Name != "Globals") { errorCollector.AddError("<Globals>: invalid Xml-structure!"); return false; }
                    foreach (XElement ele in globalElement.Elements())
                    {
                        if (ele.Name == "Filters") template.globalFilters = ReadElementGroup(ele, ReadFilter, errorCollector);
                        else if (ele.Name == "Actions") template.globalActions = ReadElementGroup(ele, ReadAction, errorCollector);
                        else errorCollector.AddXmlUnkownEleError(ele, globalElement);
                    }

                    if (!xmlReader.ReadToNextSibling("Pages")) { errorCollector.AddError("<Pages> not found!"); return false; }
                    template.pages = GetElementGroupList(StreamChildElements(xmlReader), "Page", ReadPage, errorCollector);

                    ReplaceNamedSdcMinObsAlternatives(template.pages, template.info.sdcMinObsAlternatives, errorCollector);
                }
                return !errorCollector.HasErrors();
            }
            catch (Exception exception) { errorCollector.AddError(exception.Message); return false; }
        }

        private static void ReplaceNamedSdcMinObsAlternatives(List<Template.Page> pages, Dictionary<string, int> sdcMinObsAlternatives, ErrorCollector errorCollector)
        {
            foreach (Template.Page page in pages)
                foreach (Template.Page.Table table in page.tables)
                {

                    if (table.sdcDefinition.minObsAlternative == null && table.sdcDefinition.minObsAlternativeName != null)
                    {
                        Replace (ref table.sdcDefinition);
                        foreach (Template.Page.Table.Row row in table.rows) Replace(ref row.sdcDefinition);
                        foreach (Template.Page.Table.Column column in table.columns) Replace(ref column.sdcDefinition);
                        foreach (Template.Page.Table.Column column in table.reformColumns) Replace(ref column.sdcDefinition);
                        foreach (Template.Page.Table.Cell cell in table.cells) Replace(ref cell.sdcDefinition);
                        foreach (Template.Page.Table.Cell cell in table.reformCells) Replace(ref cell.sdcDefinition);
                    }
                }

            void Replace(ref Template.Page.Table.SDCDefinition sdcDefinition)
            {
                if (sdcDefinition.minObsAlternativeName == null) return;
                if (int.TryParse(sdcDefinition.minObsAlternativeName, out int i))
                    sdcDefinition.minObsAlternative = i;
                else
                {
                    if (sdcMinObsAlternatives.ContainsKey(sdcDefinition.minObsAlternativeName))
                        sdcDefinition.minObsAlternative = sdcMinObsAlternatives[sdcDefinition.minObsAlternativeName];
                    else errorCollector.AddDebugOnlyError($"Unknown SdcMinObsAlternative '{sdcDefinition.minObsAlternativeName}'.");
                }
            }
        }

        private static Template.Page ReadPage(XElement xePage, ErrorCollector errorCollector)
        {
            Template.Page page = new Template.Page();
            foreach (XElement xe in xePage.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": page.name = xe.Value; break;
                    case "Title": page.title = xe.Value; break;
                    case "Subtitle": page.subtitle = xe.Value; break;
                    case "Description": page.description = xe.Value; break;
                    case "Button": page.button = xe.Value; break;
                    case "Visible": page.visible = errorCollector.XEleGetBool(xe, xePage, page.name); break;
                    case "PerReform": page.perReform = errorCollector.XEleGetBool(xe, xePage, page.name); break;
                    case "Actions": page.actions = ReadElementGroup(xe, ReadAction, errorCollector); break;
                    case "Tables": page.tables = ReadElementGroup(xe, ReadTable, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xePage, page.name); break;
                }
            }
            return page;
        }

        private static Template.Page.Table ReadTable(XElement xeTable, ErrorCollector errorCollector)
        {
            Template.Page.Table table = new Template.Page.Table();
            foreach (XElement xe in xeTable.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": table.name = xe.Value; break;
                    case "Title": table.title = xe.Value; break;
                    case "Subtitle": table.subtitle = xe.Value; break;
                    case "Description": table.description = xe.Value; break;
                    case "StringFormat": table.stringFormat = xe.Value; break;
                    case "ColumnGrouping": table.columnGrouping = errorCollector.XEleGetEnum<HardDefinitions.ColumnGrouping>(xe, xeTable, table.name); break;
                    case "Visible": table.visible = errorCollector.XEleGetBool(xe, xeTable, table.name); break;
                    case "Action": table.action = ReadAction(xe, errorCollector); break;
                    case "Graph": table.graph = ReadGraph(xe, errorCollector); break;
                    case "Rows": table.rows = ReadElementGroup(xe, ReadRow, errorCollector); break;
                    case "Columns": table.columns = ReadElementGroup(xe, ReadColumn, errorCollector); break;
                    case "ReformColumns": table.reformColumns = ReadElementGroup(xe, ReadColumn, errorCollector); break;
                    case "Cells": table.cells = ReadElementGroup(xe, ReadCell, errorCollector); break;
                    case "ReformCells": table.reformCells = ReadElementGroup(xe, ReadCell, errorCollector); break;
                    case "SDCDefinition": table.sdcDefinition = ReadTableSDCDefinition(errorCollector, xe); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeTable, table.name); break;
                }
            }
            return table;
        }

        private static Template.Page.Table.Row ReadRow(XElement xeRow, ErrorCollector errorCollector)
        {
            Template.Page.Table.Row row = new Template.Page.Table.Row();
            foreach (XElement xe in xeRow.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": row.name = xe.Value; break;
                    case "IsVisible": row.isVisible = errorCollector.XEleGetBool(xe, xeRow, row.name); break;
                    case "HasSeparatorBefore": row.hasSeparatorBefore = errorCollector.XEleGetBool(xe, xeRow, row.name); break;
                    case "HasSeparatorAfter": row.hasSeparatorAfter = errorCollector.XEleGetBool(xe, xeRow, row.name); break;
                    case "ForEachDataRow": row.forEachDataRow = errorCollector.XEleGetBool(xe, xeRow, row.name); break;
                    case "ForEachValueOf": row.forEachValueOf = xe.Value; break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, row, errorCollector, xeRow, row.name); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeRow, row.name); break;
                }
            }
            return row;
        }

        private static Template.Page.Table.Column ReadColumn(XElement xeCol, ErrorCollector errorCollector)
        {
            Template.Page.Table.Column col = new Template.Page.Table.Column();
            foreach (XElement xe in xeCol.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": col.name = xe.Value; break;
                    case "IsVisible": col.isVisible = errorCollector.XEleGetBool(xe, xeCol, col.name); break;
                    case "HasSeparatorBefore": col.hasSeparatorBefore = errorCollector.XEleGetBool(xe, xeCol, col.name); break;
                    case "HasSeparatorAfter": col.hasSeparatorAfter = errorCollector.XEleGetBool(xe, xeCol, col.name); break;
                    case "TiesWith": col.tiesWith = errorCollector.XEleGetDouble(xe, xeCol, col.name); break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, col, errorCollector, xeCol, col.name); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCol, col.name); break;
                }
            }
            return col;
        }

        private static Template.Page.Table.Cell ReadCell(XElement xeCell, ErrorCollector errorCollector)
        {
            Template.Page.Table.Cell cell = new Template.Page.Table.Cell();
            foreach (XElement xe in xeCell.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "ColNum": cell.colNum = errorCollector.XEleGetInt(xe, xeCell); break;
                    case "RowNum": cell.rowNum = errorCollector.XEleGetInt(xe, xeCell); break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, cell, errorCollector, xeCell); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCell); break;
                }
            }
            return cell;
        }

        private static List<string> commonTableFields = new List<string>() {
            "Action", "Tooltip", "SDCDefinition",
            "StringFormat", "Strong", "ForegroundColour", "BackgroundColour" };

        private static void ReadCommonTableField(XElement xeCommon, Template.Page.Table.TableElement tableElement,
                                                 ErrorCollector errorCollector, XElement xeParent, string nameParent = null)
        {
            switch (GetXEleName(xeCommon))
            {
                case "Strong": tableElement.strong = errorCollector.XEleGetBool(xeCommon, xeParent, nameParent); break;
                case "ForegroundColour": tableElement.foregroundColour = xeCommon.Value; break;
                case "BackgroundColour": tableElement.backgroundColour = xeCommon.Value; break;
                case "StringFormat": tableElement.stringFormat = xeCommon.Value; break;
                case "Tooltip": tableElement.tooltip = xeCommon.Value; break;
                case "Action": tableElement.action = ReadAction(xeCommon, errorCollector); break;
                case "SDCDefinition": tableElement.sdcDefinition = ReadTableSDCDefinition(errorCollector, xeCommon); break;
            }
        }

        private static Template.Page.Table.SDCDefinition ReadTableSDCDefinition(ErrorCollector errorCollector, XElement xe)
        {
            Template.Page.Table.SDCDefinition sdcDefinition = new Template.Page.Table.SDCDefinition();
            foreach (XElement xeSdc in xe.Elements())
            {
                if (xeSdc.Value == null) continue;
                switch (GetXEleName(xeSdc))
                {
                    case "MinObsAlternative": sdcDefinition.minObsAlternativeName = xeSdc.Value; break;
                    case "CountNonZeroObsOnly": sdcDefinition.countNonZeroObsOnly = errorCollector.XEleGetBool(xeSdc, xe); break;
                    case "IgnoreActionFilter": sdcDefinition.ignoreActionFilter = errorCollector.XEleGetBool(xeSdc, xe); break;
                    case "Suspend": sdcDefinition.suspendSdc = errorCollector.XEleGetBool(xeSdc, xe); break;
                    case "SecondaryGroups": sdcDefinition.secondaryGroups = ReadSecondarySdcGroups(xeSdc, errorCollector); break;
                    case "SuspendSecondaryGroups": sdcDefinition.suspendSecondaryGroups = errorCollector.XEleGetBool(xeSdc, xe); break;
                    default: errorCollector.AddXmlUnkownEleError(xeSdc, xe); break;
                }
            }
            return sdcDefinition;
        }

        private static List<string> ReadSecondarySdcGroups(XElement xeSecGroup, ErrorCollector errorCollector)
        {
            List<string> secGroups = new List<string>();
            foreach (XElement xe in xeSecGroup.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "NamedSecondaryGroup": if (!secGroups.Contains(xe.Value.ToLower().Trim())) secGroups.Add(xe.Value.ToLower().Trim()); break;
                    case "AutoGroup": secGroups.Add(Guid.NewGuid().ToString()); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeSecGroup); break;
                }
            }
            return secGroups;
        }

        private static Template.Action ReadAction(XElement xeAction, ErrorCollector errorCollector)
        {
            Template.Action action = new Template.Action();
            foreach (XElement xe in xeAction.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "CalculationLevel": action._calculationLevel = xe.Value; break;
                    case "CalculationType": action.calculationType = errorCollector.XEleGetEnum<HardDefinitions.CalculationType>(xe, xeAction); break;
                    case "OutputVar": action.outputVar = xe.Value; break;
                    case "FormulaString": action.formulaString = xe.Value; break;
                    case "Filter": action.filter = ReadFilter(xe, errorCollector); break;
                    case "Reform": action._reform = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "SaveResults": action._saveResult = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "BlendParameters": action._blendParameters = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "Parameters": action.parameters = ReadElementGroup(xe, ReadParameter, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeAction); break;
                }
            }
            return action;
        }

        private static Template.Parameter ReadParameter(XElement xePar, ErrorCollector errorCollector)
        {
            Template.Parameter par = new Template.Parameter();
            foreach (XElement xe in xePar.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": par.name = xe.Value; break;
                    case "VarName": par.variableName = xe.Value; break;
                    case "BoolValue": par.boolValue = errorCollector.XEleGetBool(xe, xePar, par.name); break;
                    case "NumericValue": par.numericValue = errorCollector.XEleGetDouble(xe, xePar, par.name); break;
                    case "Reform": par._reform = errorCollector.XEleGetBool(xe, xePar, par.name); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xePar, par.name); break;
                }
            }
            return par;
        }

        private static Template.Filter ReadFilter(XElement xeFilter, ErrorCollector errorCollector)
        {
            Template.Filter filter = new Template.Filter();
            foreach (XElement xe in xeFilter.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": filter.name = xe.Value; break;
                    case "FormulaString": filter.formulaString = xe.Value; break;
                    case "Reform": filter.reform = errorCollector.XEleGetBool(xe, xeFilter, filter.name); break;
                    case "Parameters": filter.parameters = ReadElementGroup(xe, ReadParameter, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeFilter, filter.name); break; ;
                }
            }
            return filter;
        }

        private static Template.TemplateInfo ReadInfo(XElement xeInfo, ErrorCollector errorCollector)
        {
            Template.TemplateInfo info = new Template.TemplateInfo();

            if (xeInfo.Element("DebugMode") != null && errorCollector.XEleGetBool(xeInfo.Element("DebugMode"), xeInfo)) errorCollector.SetDebugMode();

            foreach (XElement xe in xeInfo.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": info.name = xe.Value; break;
                    case "Title": info.title = xe.Value; break;
                    case "Subtitle": info.subtitle = xe.Value; break;
                    case "Button": info.button = xe.Value; break;
                    case "Description": info.description = xe.Value; break;
                    case "GeneralDescription": info.generalDescription = xe.Value; break;
                    case "MinFiles": info.minFiles = errorCollector.XEleGetInt(xe, xeInfo); break;
                    case "MaxFiles": info.maxFiles = errorCollector.XEleGetInt(xe, xeInfo); break;
                    case "HideMainSelectorForSingleFilePackage": info.HideMainSelectorForSingleFilePackage = errorCollector.XEleGetBool(xe, xeInfo); break;
                    case "TemplateType": info.templateType = errorCollector.XEleGetEnum<HardDefinitions.TemplateType>(xe, xeInfo); break;
                    case "AdditionalCalculationLevels": info.calculationLevels = ReadElementGroup(xe, ReadCalculationLevel, errorCollector); break;
                    case "RequiredVariables": info.requiredVariables = ReadElementGroup(xe, ReadRequiredVariable, errorCollector); break;
                    case "OptionalVariables": info.optionalVariables = ReadElementGroup(xe, ReadOptionalVariable, errorCollector); break;
                    case "UserVariables": info.userVariables = ReadElementGroup(xe, ReadUserVariable, errorCollector); break;
                    case "SDCDefinition": ReadTemplateInfoSDCDefinition(errorCollector, info, xe); break;
                    case "ExportDescriptionMode": info.exportDescriptionMode = errorCollector.XEleGetEnum<HardDefinitions.ExportDescriptionMode>(xe, xeInfo); break;
                    case "DebugMode": break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeInfo); break;
                }
            };
            return info;
        }

        private static void ReadTemplateInfoSDCDefinition(ErrorCollector errorCollector, Template.TemplateInfo info, XElement xe)
        {
            foreach (XElement xeg in xe.Elements())
            {
                if (xeg.Value == null) continue;
                switch (GetXEleName(xeg))
                {
                    case "HideZeroObs": info.sdcHideZeroObs = errorCollector.XEleGetBool(xeg, xe); break;
                    case "MinObsDefault": info.sdcMinObsDefault = errorCollector.XEleGetInt(xeg, xe); break;
                    case "MinObsAlternatives":
                        info.sdcMinObsAlternatives = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (var ele in ReadElementGroup(xeg, ReadSdcMinObsAlternative, errorCollector))
                        {
                            if (ele.Key == null) continue;
                            if (info.sdcMinObsAlternatives.ContainsKey(ele.Key)) errorCollector.AddDebugOnlyError($"Double definition of SdcMinObsAlternative '{ele.Key}'.");
                            else info.sdcMinObsAlternatives.Add(ele.Key, ele.Value);
                        }
                        break;
                    default: errorCollector.AddXmlUnkownEleError(xeg, xe); break;
                }
            }
        }

        private static KeyValuePair<string, int> ReadSdcMinObsAlternative(XElement xeAlt, ErrorCollector errorCollector)
        {
            string name = null; int minObs = int.MinValue;
            foreach (XElement xe in xeAlt.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": name = xe.Value; break;
                    case "MinObs": minObs = errorCollector.XEleGetInt(xe, xeAlt); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeAlt); break;
                }
            }
            if (minObs == int.MinValue || name == null) errorCollector.AddDebugOnlyError("Incomplete definition of SdcMinObsAlternative" +
                                                                                         (name == null ? ": missing 'Name'" : $" '{name}': ") +
                                                                                         (minObs == int.MinValue ? "missing 'MinObs'" : string.Empty));
            return new KeyValuePair<string, int>(name, minObs);
        }

        private static Template.TemplateInfo.CalculationLevel ReadCalculationLevel(XElement xeCL, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.CalculationLevel cl = new Template.TemplateInfo.CalculationLevel();
            foreach (XElement xe in xeCL.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": cl.name = xe.Value; break;
                    case "GroupingVar": cl.groupingVar = xe.Value; break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCL, cl.name); break;
                }
            }
            return cl;
        }

        private static Template.TemplateInfo.RequiredVariable ReadRequiredVariable(XElement xeRV, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.RequiredVariable rv = new Template.TemplateInfo.RequiredVariable();
            foreach (XElement xe in xeRV.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": rv.name = xe.Value; break;
                    case "ReadVar": rv.readVar = xe.Value; break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeRV, rv.name); break;
                }
            }
            return rv;
        }

        private static Template.TemplateInfo.OptionalVariable ReadOptionalVariable(XElement xeOV, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.OptionalVariable ov = new Template.TemplateInfo.OptionalVariable();
            foreach (XElement xe in xeOV.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": ov.name = xe.Value; break;
                    case "ReadVar": ov.readVar = xe.Value; break;
                    case "DefaultValue": ov.defaultValue = errorCollector.XEleGetDouble(xe, xeOV, ov.name); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeOV, ov.name); break;
                }
            }
            return ov;
        }

        private static DisplayResults.DisplayPage.DisplayTable.DisplayGraph ReadGraph(XElement xeGraph, ErrorCollector errorCollector)
        {
            DisplayResults.DisplayPage.DisplayTable.DisplayGraph graph = new DisplayResults.DisplayPage.DisplayTable.DisplayGraph();
            foreach (XElement xe in xeGraph.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "ShowTable": graph.showTable = errorCollector.XEleGetBool(xe, xeGraph); break;
                    case "SeriesInRows": graph.seriesInRows = errorCollector.XEleGetBool(xe, xeGraph); break;
                    case "Series": graph.allSeries = ReadElementGroup(xe, ReadGraphSerie, errorCollector); break;
                    case "Title": graph.title = xe.Value; break;
                    case "Legend": graph.legend = ReadLegend(xe, errorCollector); break;
                    case "AxisX": graph.axisX = ReadAxis(xe, errorCollector); break;
                    case "AxisY": graph.axisY = ReadAxis(xe, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeGraph); break;
                }
            }

            return graph;
        }

        private static DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series ReadGraphSerie(XElement xeSerie, ErrorCollector errorCollector)
        {
            DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series serie = new DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series();
            foreach (XElement xe in xeSerie.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Visible": serie.visible = errorCollector.XEleGetBool(xe, xeSerie); break;
                    case "Name": serie.name = xe.Value; break;
                    case "Size": serie.size = errorCollector.XEleGetInt(xe, xeSerie); break;
                    case "Colour": serie.colour = xe.Value; break;
                    case "Type": serie.type = xe.Value; break;
                    case "MarkerStyle": serie.markerStyle = xe.Value; break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeSerie); break;
                }
            }
            return serie;
        }

        private static DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Axis ReadAxis(XElement xeAxis, ErrorCollector errorCollector)
        {
            DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Axis axis = new DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Axis();
            foreach (XElement xe in xeAxis.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Label": axis.label = xe.Value; break;
                    case "ValuesFrom": axis.valuesFrom = xe.Value; break;
                    case "StartFromZero": axis.startFromZero = errorCollector.XEleGetBool(xe, xeAxis); break;
                    case "Interval": axis.interval = errorCollector.XEleGetInt(xe, xeAxis); break;
                    case "LabelDocking": axis.labelDocking = xe.Value; break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeAxis); break;
                }
            }
            return axis;
        }

        private static DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Legend ReadLegend(XElement xeLegend, ErrorCollector errorCollector)
        {
            DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Legend legend = new DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Legend();
            foreach (XElement xe in xeLegend.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Title": legend.title = xe.Value; break;   // not supported in the new presenter!
                    case "Docking": legend.docking = xe.Value; break;
                    case "Visible": legend.visible = errorCollector.XEleGetBool(xe, xeLegend); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeLegend); break;
                }
            }
            return legend;
        }
        
        private static Template.TemplateInfo.UserVariable ReadUserVariable(XElement xeUV, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.UserVariable uv = new Template.TemplateInfo.UserVariable();
            foreach (XElement xe in xeUV.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "UserInputType": uv.inputType = errorCollector.XEleGetEnum<HardDefinitions.UserInputType>(xe, xeUV, uv.name); break;
                    case "Name": uv.name = xe.Value; break;
                    case "Description": uv.description = xe.Value; break;
                    case "Title": uv.title = xe.Value; break;
                    case "DefaultValue": uv.defaultValue = xe.Value; break;
                    case "DisplayDescription": uv.displayDescription = errorCollector.XEleGetBool(xe, xeUV, uv.name); break;
                    case "ComboItems": uv.comboItems = ReadElementGroup(xe, ReadComboItem, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeUV, uv.name); break;
                }
            }
            return uv;
        }

        private static Template.TemplateInfo.ComboItem ReadComboItem(XElement xeCI, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.ComboItem ci = new Template.TemplateInfo.ComboItem();
            foreach (XElement xe in xeCI.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": ci.name = xe.Value; break;
                    case "Value": ci.value = xe.Value; break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCI, ci.name); break;
                }
            };
            return ci;
        }

        private static List<T> ReadElementGroup<T>(XElement xeGroup, Func<XElement, ErrorCollector, T> readFunc, ErrorCollector errorCollector)
        {
            return GetElementGroupList(xeGroup.Elements(), // e.g. all elements of <Actions>
                                       GetXEleName(xeGroup).Substring(0, GetXEleName(xeGroup).Length - 1), // e.g. Actions -> Action
                                       readFunc, errorCollector);
        }

        private static List<T> GetElementGroupList<T>(IEnumerable<XElement> xes, string nameChildXE, Func<XElement, ErrorCollector, T> readFunc,
                                                      ErrorCollector errorCollector)
        {
            List<T> list = new List<T>(); // e.g. List<Template.Action>
            foreach (XElement xe in xes)
            {
                if (xe.Name != nameChildXE) // all child-elements of e.g. <Actions> need to be <Action>-elements
                    errorCollector.AddXmlUnkownEleError(xe, new XElement(nameChildXE + "s"));
                else
                {
                    T ele = readFunc(xe, errorCollector); // call e.g. ReadAction ...
                    list.Add(ele); // ... and add the result to the List<Template.Action>
                }
            }
            return list;
        }

        private static IEnumerable<XElement> StreamChildElements(XmlReader reader)
        {
            using (var rdr = reader.ReadSubtree())
            {
                rdr.MoveToContent();
                while (rdr.Read())
                {
                    if (rdr.NodeType == XmlNodeType.Element)
                    {
                        var e = XElement.ReadFrom(rdr) as XElement;
                        yield return e;
                    }
                }
                rdr.Close();
            }
        }

        private static string GetXEleName(XElement xe) { return xe.Name == null ? string.Empty : xe.Name.ToString(); }
    }
}
