using System;
using System.Collections.Generic;
using System.IO;
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

        public static bool ParseTemplate(string pathOrInputString, out Template template, out ErrorCollector errorCollector, bool readFromString = false)
        {
            errorCollector = new ErrorCollector(); template = new Template();
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment };
                using (XmlReader xmlReader = readFromString ? XmlReader.Create(new StringReader(pathOrInputString), settings) 
                                                            : XmlReader.Create(pathOrInputString, settings))
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
                        if (ele.Name == "Filters") template.globalFilters = ReadElementGroup(ele, ReadFilter, null, errorCollector);
                        else if (ele.Name == "Actions") template.globalActions = ReadElementGroup(ele, ReadAction, null, errorCollector);
                        else errorCollector.AddXmlUnkownEleError(ele, globalElement);
                    }

                    if (!xmlReader.ReadToNextSibling("Pages")) { errorCollector.AddError("<Pages> not found!"); return false; }
                    template.pages = GetElementGroupList(StreamChildElements(xmlReader), "Page", ReadPage, null, errorCollector);
                }
                return !errorCollector.HasErrors();
            }
            catch (Exception exception) { errorCollector.AddError(exception.Message); return false; }
        }

        private static Template.Page ReadPage(XElement xePage, LocalMap localMap, ErrorCollector errorCollector)
        {
            Template.Page page = new Template.Page(LocalMap.NewPageLocalMap());
            foreach (XElement xe in xePage.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": page.name = xe.Value; break;
                    case "Title": page.title = xe.Value; break;
                    case "Subtitle": page.subtitle = xe.Value; break;
                    case "Description": page.description = xe.Value; break;
                    case "Button": page.button = ReadPageButton(xe, errorCollector); break;
                    case "Html": page.html = xe.Value; break;
                    case "Visible": page.visible = errorCollector.XEleGetBool(xe, xePage, page.name); break;
                    case "Active": page.active = errorCollector.XEleGetBool(xe, xePage, page.name); break;
                    case "PerReform": page.perReform = errorCollector.XEleGetBool(xe, xePage, page.name); break;
                    case "Actions": page.actions = ReadElementGroup(xe, ReadAction, page.localMap, errorCollector); break;
                    case "Filters": page.filters = ReadElementGroup(xe, ReadFilter, page.localMap, errorCollector); break;
                    case "Tables": page.tables = ReadElementGroup(xe, ReadTable, page.localMap, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xePage, page.name); break;
                }
            }
            return page;
        }

        private static Template.Page.VisualElement ReadPageButton(XElement xeButton, ErrorCollector errorCollector)
        {
            Template.Page.VisualElement pageButton = new Template.Page.VisualElement();
            if (xeButton.HasElements)
            {
                foreach (XElement xe in xeButton.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (GetXEleName(xe))
                    {
                        case var cvf when commonVisualFields.Contains(cvf): ReadCommonVisualField(xe, pageButton, errorCollector, xeButton); break;
                        default: errorCollector.AddXmlUnkownEleError(xe, xeButton, pageButton.name); break;
                    }
                }
            }
            else    // support old templates where button was a simple string
            {
                pageButton.name = pageButton.title = xeButton.Value ?? "";
            }
            return pageButton;
        }

        private static Template.Page.Table ReadTable(XElement xeTable, LocalMap pageLocalMap, ErrorCollector errorCollector)
        {
            Template.Page.Table table = new Template.Page.Table(LocalMap.NewTableLocalMap(pageLocalMap));
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
                    case "Active": table.active = errorCollector.XEleGetBool(xe, xeTable, table.name); break;
                    case "PerReform": table.perReform = errorCollector.XEleGetBool(xe, xeTable, table.name); break;
                    case "CellAction": table.cellAction = ReadAction(xe, table.localMap, errorCollector); break;
                    case "Graph": table.graph = ReadGraph(xe, errorCollector); break;
                    case "Rows": table.rows = ReadElementGroup(xe, ReadRow, table.localMap, errorCollector); break;
                    case "Columns": table.columns = ReadElementGroup(xe, ReadColumn, table.localMap, errorCollector); break;
                    case "ReformColumns": table.reformColumns = ReadElementGroup(xe, ReadColumn, table.localMap, errorCollector); break;
                    case "Cells": table.cells = ReadElementGroup(xe, ReadCell, table.localMap, errorCollector); break;
                    case "ReformCells": table.reformCells = ReadElementGroup(xe, ReadCell, table.localMap, errorCollector); break;
                    case "Actions": table.actions = ReadElementGroup(xe, ReadAction, table.localMap, errorCollector); break;
                    case "Filters": table.filters = ReadElementGroup(xe, ReadFilter, table.localMap, errorCollector); break;
                    case "SDCDefinition": table.sdcDefinition = ReadTableSDCDefinition(errorCollector, xe); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeTable, table.name); break;
                }
            }
            return table;
        }

        private static Template.Page.Table.Row ReadRow(XElement xeRow, LocalMap tableLocalMap, ErrorCollector errorCollector)
        {
            Template.Page.Table.Row row = new Template.Page.Table.Row();
            foreach (XElement xe in xeRow.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "IsVisible": row.isVisible = errorCollector.XEleGetBool(xe, xeRow, row.title); break;
                    case "HasSeparatorBefore": row.hasSeparatorBefore = errorCollector.XEleGetBool(xe, xeRow, row.title); break;
                    case "HasSeparatorAfter": row.hasSeparatorAfter = errorCollector.XEleGetBool(xe, xeRow, row.title); break;
                    case "ForEachDataRow": row.forEachDataRow = errorCollector.XEleGetBool(xe, xeRow, row.title); break;
                    case "ForEachValueOf": row.forEachValueOf = xe.Value; break;
                    case "ForEachValueMaxCount": row.forEachValueMaxCount = errorCollector.XEleGetInt(xe, xeRow, row.title); break;
                    case "ForEachValueDescriptions": row.forEachValueDescriptions = ReadRowForEachValueDescriptions(xe, errorCollector); break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, row, tableLocalMap, errorCollector, xeRow, row.title); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeRow, row.title); break;
                }
            }
            return row;
        }

        private static Dictionary<double, string> ReadRowForEachValueDescriptions(XElement xeDescs, ErrorCollector errorCollector)
        {
            Dictionary<double, string> vevds = new Dictionary<double, string>();
            foreach (XElement xeDesc in xeDescs.Elements())
            {
                if (xeDesc.Value == null) continue;
                if (GetXEleName(xeDesc) != "ForEachValueDescription") { errorCollector.AddXmlUnkownEleError(xeDesc, xeDescs); continue; }
                double value = double.PositiveInfinity; string description = string.Empty;
                foreach (XElement xe in xeDesc.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (GetXEleName(xe))
                    {
                        case "Value": value = errorCollector.XEleGetDouble(xe, xeDesc); break;
                        case "Description": description = xe.Value; break;
                        default: errorCollector.AddXmlUnkownEleError(xe, xeDesc); break;
                    }
                }
                if (value != double.PositiveInfinity) vevds.Add(value, description);
            }
            return vevds;
        }

        private static Template.Page.Table.Column ReadColumn(XElement xeCol, LocalMap tableLocalMap, ErrorCollector errorCollector)
        {
            Template.Page.Table.Column col = new Template.Page.Table.Column();
            foreach (XElement xe in xeCol.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "IsVisible": col.isVisible = errorCollector.XEleGetBool(xe, xeCol, col.title); break;
                    case "HasSeparatorBefore": col.hasSeparatorBefore = errorCollector.XEleGetBool(xe, xeCol, col.title); break;
                    case "HasSeparatorAfter": col.hasSeparatorAfter = errorCollector.XEleGetBool(xe, xeCol, col.title); break;
                    case "TiesWith": col.tiesWith = xe.Value; break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, col, tableLocalMap, errorCollector, xeCol, col.title); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCol, col.title); break;
                }
            }
            return col;
        }

        private static Template.Page.Table.Cell ReadCell(XElement xeCell, LocalMap tableLocalMap, ErrorCollector errorCollector)
        {
            Template.Page.Table.Cell cell = new Template.Page.Table.Cell();
            foreach (XElement xe in xeCell.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "ColName": cell.colName = xe.Value; break;
                    case "RowName": cell.rowName = xe.Value; break;
                    case var ctf when commonTableFields.Contains(ctf): ReadCommonTableField(xe, cell, tableLocalMap, errorCollector, xeCell); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeCell); break;
                }
            }
            return cell;
        }

        private static List<string> commonVisualFields = new List<string>() {
            "Tooltip", "Strong", "ForegroundColour", "BackgroundColour", "Title", "Name", "TextAlign" };

        private static List<string> commonTableFields = new List<string>(commonVisualFields) {
            "StringFormat", "CellAction", "SDCDefinition"};

        private static void ReadCommonVisualField(XElement xeCommon, Template.Page.VisualElement visualElement, 
                                                 ErrorCollector errorCollector, XElement xeParent, string nameParent = null)
        {
            switch (GetXEleName(xeCommon))
            {
                case "Strong": visualElement.strong = errorCollector.XEleGetBool(xeCommon, xeParent, nameParent); break;
                case "ForegroundColour": visualElement.foregroundColour = xeCommon.Value; break;
                case "BackgroundColour": visualElement.backgroundColour = xeCommon.Value; break;
                case "TextAlign": visualElement.textAlign = xeCommon.Value; break;
                case "Tooltip": visualElement.tooltip = xeCommon.Value; break;
                case "Title": visualElement.title = xeCommon.Value; break;
                case "Name": visualElement.name = xeCommon.Value; break;
            }
        }

        private static void ReadCommonTableField(XElement xeCommon, Template.Page.Table.TableElement tableElement, LocalMap tableLocalMap,
                                                 ErrorCollector errorCollector, XElement xeParent, string nameParent = null)
        {
            switch (GetXEleName(xeCommon))
            {
                case "StringFormat": tableElement.stringFormat = xeCommon.Value; break;
                case "CellAction": tableElement.cellAction = ReadAction(xeCommon, tableLocalMap, errorCollector); break;
                case "SDCDefinition": tableElement.sdcDefinition = ReadTableSDCDefinition(errorCollector, xeCommon); break;
                case var cvf when commonVisualFields.Contains(cvf): ReadCommonVisualField(xeCommon, tableElement, errorCollector, xeParent, tableElement.title); break;
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

        private static Template.Action ReadAction(XElement xeAction, LocalMap localMap, ErrorCollector errorCollector)
        {
            Template.Action action = new Template.Action(localMap);
            foreach (XElement xe in xeAction.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": action.name = xe.Value; break;
                    case "CalculationLevel": action._calculationLevel = xe.Value; break;
                    case "CalculationType": action.calculationType = errorCollector.XEleGetEnum<HardDefinitions.CalculationType>(xe, xeAction); break;
                    case "OutputVar": action.outputVar = xe.Value; break;
                    case "FormulaString": action.formulaString = xe.Value; break;
                    case "Filter": action.filter = ReadFilter(xe, localMap, errorCollector); break;
                    case "Reform": action._reform = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "SaveResults": action._saveResult = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "BlendParameters": action._blendParameters = errorCollector.XEleGetBool(xe, xeAction); break;
                    case "Parameters": action.parameters = ReadElementGroup(xe, ReadParameter, localMap, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeAction); break;
                }
            }
            return action;
        }

        private static Template.Parameter ReadParameter(XElement xePar, LocalMap localMap, ErrorCollector errorCollector)
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
                    case "StringValue": par.stringValue = xe.Value; break;
                    case "Source": par._source = errorCollector.XEleGetEnum<Template.Parameter.Source>(xe, xePar, par.name); break;
                    case "Reform": if (!errorCollector.XEleGetBool(xe, xePar, par.name)) par._source = Template.Parameter.Source.BASELINE; break; // old-style, replaced by Source
                    default: errorCollector.AddXmlUnkownEleError(xe, xePar, par.name); break;
                }
            }
            return par;
        }

        private static Template.Filter ReadFilter(XElement xeFilter, LocalMap localMap, ErrorCollector errorCollector)
        {
            Template.Filter filter = new Template.Filter(localMap);
            foreach (XElement xe in xeFilter.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": filter.name = xe.Value; break;
                    case "FormulaString": filter.formulaString = xe.Value; break;
                    case "Reform": filter.reform = errorCollector.XEleGetBool(xe, xeFilter, filter.name); break;
                    case "Parameters": filter.parameters = ReadElementGroup(xe, ReadParameter, localMap, errorCollector); break;
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
                    case "AdditionalCalculationLevels": info.calculationLevels = ReadElementGroup(xe, ReadCalculationLevel, null, errorCollector); break;
                    case "RequiredVariables": info.requiredVariables = ReadElementGroup(xe, ReadRequiredVariable, null, errorCollector); break;
                    case "OptionalVariables": info.optionalVariables = ReadElementGroup(xe, ReadOptionalVariable, null, errorCollector); break;
                    case "UserVariables": info.userVariables = ReadElementGroup(xe, ReadUserVariable, null, errorCollector); break;
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
                    case "MinObsAlternatives": info.sdcMinObsAlternatives = ReadElementGroup(xeg, ReadSdcMinObsAlternative, null, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xeg, xe); break;
                }
            }
        }

        private static KeyValuePair<string, int> ReadSdcMinObsAlternative(XElement xeAlt, LocalMap localMap, ErrorCollector errorCollector)
        {
            string name = string.Empty; int minObs = int.MinValue;
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
            return new KeyValuePair<string, int>(name, minObs);
        }

        private static Template.TemplateInfo.CalculationLevel ReadCalculationLevel(XElement xeCL, LocalMap localMap, ErrorCollector errorCollector)
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

        private static Template.TemplateInfo.RequiredVariable ReadRequiredVariable(XElement xeRV, LocalMap localMap, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.RequiredVariable rv = new Template.TemplateInfo.RequiredVariable();
            foreach (XElement xe in xeRV.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "Name": rv.name = xe.Value; break;
                    case "ReadVar": rv.readVar = xe.Value; break;
                    case "Monetary": rv.monetary = errorCollector.XEleGetBool(xe, xeRV); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeRV, rv.name); break;
                }
            }
            return rv;
        }

        private static Template.TemplateInfo.OptionalVariable ReadOptionalVariable(XElement xeOV, LocalMap localMap, ErrorCollector errorCollector)
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
                    case "Monetary": ov.monetary = errorCollector.XEleGetBool(xe, xeOV); break;
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
                    case "Series": graph.allSeries = ReadElementGroup(xe, ReadGraphSerie, null, errorCollector); break;
                    case "Title": graph.title = xe.Value; break;
                    case "Round": graph.round = errorCollector.XEleGetInt(xe, xeGraph); break;
                    case "Legend": graph.legend = ReadLegend(xe, errorCollector); break;
                    case "AxisX": graph.axisX = ReadAxis(xe, errorCollector); break;
                    case "AxisY": graph.axisY = ReadAxis(xe, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeGraph); break;
                }
            }

            return graph;
        }

        private static DisplayResults.DisplayPage.DisplayTable.DisplayGraph.Series ReadGraphSerie(XElement xeSerie, LocalMap localMap, ErrorCollector errorCollector)
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
        
        private static Template.TemplateInfo.UserVariable ReadUserVariable(XElement xeUV, LocalMap localMap, ErrorCollector errorCollector)
        {
            Template.TemplateInfo.UserVariable uv = new Template.TemplateInfo.UserVariable();
            foreach (XElement xe in xeUV.Elements())
            {
                if (xe.Value == null) continue;
                switch (GetXEleName(xe))
                {
                    case "UserInputType": uv.inputType = errorCollector.XEleGetEnum<HardDefinitions.UserInputType>(xe, xeUV, uv.name); break;
                    case "Name": uv.name = xe.Value; break;
                    case "Monetary": uv.monetary = errorCollector.XEleGetBool(xe, xeUV); break;
                    case "Description": uv.description = xe.Value; break;
                    case "Title": uv.title = xe.Value; break;
                    case "DefaultValue": uv.defaultValue = xe.Value; break;
                    case "DisplayDescription": uv.displayDescription = errorCollector.XEleGetBool(xe, xeUV, uv.name); break;
                    case "ComboItems": uv.comboItems = ReadElementGroup(xe, ReadComboItem, null, errorCollector); break;
                    default: errorCollector.AddXmlUnkownEleError(xe, xeUV, uv.name); break;
                }
            }
            return uv;
        }

        private static Template.TemplateInfo.ComboItem ReadComboItem(XElement xeCI, LocalMap localMap, ErrorCollector errorCollector)
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

        private static List<T> ReadElementGroup<T>(XElement xeGroup, Func<XElement, LocalMap, ErrorCollector, T> readFunc, LocalMap localMap, ErrorCollector errorCollector)
        {
            return GetElementGroupList(xeGroup.Elements(), // e.g. all elements of <Actions>
                                       GetXEleName(xeGroup).Substring(0, GetXEleName(xeGroup).Length - 1), // e.g. Actions -> Action
                                       readFunc, localMap, errorCollector);
        }

        private static List<T> GetElementGroupList<T>(IEnumerable<XElement> xes, string nameChildXE, Func<XElement, LocalMap, ErrorCollector, T> readFunc,
                                                      LocalMap localMap, ErrorCollector errorCollector)
        {
            List<T> list = new List<T>(); // e.g. List<Template.Action>
            foreach (XElement xe in xes)
            {
                if (xe.Name != nameChildXE) // all child-elements of e.g. <Actions> need to be <Action>-elements
                    errorCollector.AddXmlUnkownEleError(xe, new XElement(nameChildXE + "s"));
                else
                {
                    T ele = readFunc(xe, localMap, errorCollector); // call e.g. ReadAction ...
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
