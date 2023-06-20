using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace EM_Statistics
{
    public partial class TemplateApi
    {
        private readonly Template template;
        private readonly ErrorCollector errorCollector;

        public enum ModifyMode { AddNew, AddOrKeep, AddOrReplace, MergeKeep, MergeReplace, Delete }
        public enum AddWhere { Tail, Head, After, Before, Appropriate }

        public TemplateApi(Template _template, ErrorCollector _errorCollector) { template = _template; errorCollector = _errorCollector; }

        #region GENERAL

        public Template GetTemplate() { return template; }

        private List<string> nonMonPrefixes = new List<string>() { "d", "l" }, nonMonVariableNames = new List<string>() { };
        private bool nonMonetaryVariablesCreationMode = false;
        /// <summary>
        /// if nonMonetaryVariablesCreationMode = true, (optional and required) variables modified by the API are set non-monetary where appropriate
        /// </summary>
        /// <param name="set">true: switch on, false (default): switch off</param>
        /// <param name="_nonMonPrefixes">list of variable prefixes, that classify a variable as non-monetary, e.g. d for demographic </param>
        /// <param name="_nonMonVariableNames">list of non-monetary variable names, e.g. bccmy, yemmy, ... </param>
        public void SetNonMonetaryVariablesCreationMode(bool set, List<string> _nonMonPrefixes = null, List<string> _nonMonVariableNames = null)
        {
            nonMonetaryVariablesCreationMode = set;
            if (_nonMonPrefixes != null) nonMonPrefixes = _nonMonPrefixes; if (_nonMonVariableNames != null) nonMonVariableNames = _nonMonVariableNames;
        }
        private bool IsPresumablyMonetaryVariable(string variableName)
        {
            try
            {
                if (nonMonVariableNames.Contains(variableName, StringComparer.OrdinalIgnoreCase)) return false;
                foreach (string nonMonPrefix in nonMonPrefixes) if (variableName.ToLower().StartsWith(nonMonPrefix.ToLower())) return false;
                return true;
            }
            catch { return true; }
        }

        public void SetEndUserFriendlyActionErrorMode() { template.info.endUserFriendlyActionErrorMode = true; }

        public List<Template.TemplateInfo.OptionalVariable> GetOptinalVariables() { return template.info.optionalVariables; }

        public List<Template.TemplateInfo.RequiredVariable> GetRequiredVariables() { return template.info.requiredVariables; }

        public static bool IsValidFormula(string formula, bool isFilter, out string error)
        {
            string errMsgHeader = "Invalid " + (isFilter ? "condition" : "formula") + ":" + Environment.NewLine;
            try
            {
                error = string.Empty;
                if (isFilter) { var result = (Func<List<double>, bool>) DynamicExpressionParser.ParseLambda(
                    new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") }, null, formula, 0).Compile(); }
                else { var result = (Func<List<double>, double>) DynamicExpressionParser.ParseLambda(
                    new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") }, null, $"{formula} * 1.0", 0).Compile(); }
                return true;
            }
            catch (InvalidCastException)
            {
                error = errMsgHeader + "expression does not yield " + (isFilter ? "true or false" : "a number of type double"); return false;
            }
            catch (Exception exception)
            {
                error = errMsgHeader + exception.Message; return false;
            }
        }


        #endregion GENERAL

        #region MODIFY_LIST_ELEMENTS

        public bool ModifyGlobalActions(Template.Action modAction, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToActionName = null)
        {
            string callingFun = "ModifyGlobalActions";
            try
            {
                return ModifyListElements(modAction, template.globalActions, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToActionName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyPageActions(Template.Action modAction, string pageName, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToActionName = null)
        {
            string callingFun = "ModifyPageActions";
            try
            {
                if (!GetPage(out Template.Page page, pageName, callingFun)) return false;
                return ModifyListElements(modAction, page.actions, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToActionName, callingFun, page.localMap);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyTableActions(Template.Action modAction, string pageName, string tableName, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToActionName = null)
        {
            string callingFun = "ModifyTableActions";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyListElements(modAction, table.actions, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToActionName, callingFun, table.localMap);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        public bool ModifyGlobalFilters(Template.Filter modFilter, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToFilterName = null)
        {
            string callingFun = "ModifyGlobalFilters";
            try
            {
                return ModifyListElements(modFilter, template.globalFilters, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToFilterName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyPageFilters(Template.Filter modFilter, string pageName, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToFilterName = null)
        {
            string callingFun = "ModifyPageFilters";
            try
            {
                if (!GetPage(out Template.Page page, pageName, callingFun)) return false;
                return ModifyListElements(modFilter, page.filters, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToFilterName, callingFun, page.localMap);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyTableFilters(Template.Filter modFilter, string pageName, string tableName, ModifyMode modifyMode = ModifyMode.AddNew, AddWhere addWhere = AddWhere.Appropriate, string nextToFilterName = null)
        {
            string callingFun = "ModifyTableFilters";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyListElements(modFilter, table.filters, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToFilterName, callingFun, table.localMap);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        public bool ModifyRows(Template.Page.Table.Row modRow, string pageName, string tableName, ModifyMode modifyMode, AddWhere addWhere = AddWhere.Appropriate, string nextToRowName = null)
        {
            string callingFun = "ModifyRows";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyListElements(modRow, table.rows, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToRowName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyColumns(Template.Page.Table.Column modCol, string pageName, string tableName, ModifyMode modifyMode, bool reform = false, AddWhere addWhere = AddWhere.Appropriate, string nextToColName = null)
        {
            string callingFun = "ModifyColumns";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyListElements(modCol, reform ? table.reformColumns : table.columns, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToColName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCells(Template.Page.Table.Cell modCell, string pageName, string tableName, ModifyMode modifyMode, bool reform = false)
        {
            string callingFun = "ModifyCells";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyListElements(modCell, reform ? table.reformCells : table.cells, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, AddWhere.Appropriate, null, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        public bool ModifyOptionalVariables(Template.TemplateInfo.OptionalVariable modOptionalVariable, ModifyMode modifyMode = ModifyMode.AddOrReplace, AddWhere addWhere = AddWhere.Appropriate, string nextToOptionalVariableName = null)
        {
            string callingFun = "ModifyOptionalVariables";
            try
            {
                if (nonMonetaryVariablesCreationMode) modOptionalVariable.monetary = IsPresumablyMonetaryVariable(modOptionalVariable.readVar);
                return ModifyListElements(modOptionalVariable, template.info.optionalVariables, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToOptionalVariableName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyRequiredVariables(Template.TemplateInfo.RequiredVariable modRequiredVariable, ModifyMode modifyMode = ModifyMode.AddOrReplace, AddWhere addWhere = AddWhere.Appropriate, string nextToRequiredVariableName = null)
        {
            string callingFun = "ModifyRequiredVariables";
            try
            {
                if (nonMonetaryVariablesCreationMode) modRequiredVariable.monetary = IsPresumablyMonetaryVariable(modRequiredVariable.readVar);
                return ModifyListElements(modRequiredVariable, template.info.requiredVariables, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToRequiredVariableName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCalculationLevels(Template.TemplateInfo.CalculationLevel modCalculationLevel, ModifyMode modifyMode = ModifyMode.AddOrReplace, AddWhere addWhere = AddWhere.Appropriate, string nextToCalculationLevelName = null)
        {
            string callingFun = "ModifyCalculationLevels";
            try
            {
                return ModifyListElements(modCalculationLevel, template.info.calculationLevels, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToCalculationLevelName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyUserVariables(Template.TemplateInfo.UserVariable modUserVariable, ModifyMode modifyMode = ModifyMode.AddOrReplace, AddWhere addWhere = AddWhere.Appropriate, string nextToUserVariableName = null)
        {
            string callingFun = "ModifyUserVariables";
            try
            {
                return ModifyListElements(modUserVariable, template.info.userVariables, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); },
                                          modifyMode, addWhere, nextToUserVariableName, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public Template.Page.Table.Row GetRow(string pageName, string tableName, string rowName)
        {
            string callingFun = "GetRow";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return null;
                return table.rows.FirstOrDefault(r => r.name == rowName);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return null; }
        }

        public Template.Page.Table.Column GetColumn(string pageName, string tableName, string colName)
        {
            string callingFun = "GetColumn";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return null;
                return table.columns.FirstOrDefault(c => c.name == colName);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return null; }
        }

        public Template.Page.Table.Cell GetCell(string pageName, string tableName, string cellName)
        {
            string callingFun = "GetCell";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return null;
                return table.cells.FirstOrDefault(c => c.name == cellName);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return null; }
        }

        public Template.Page.Table GetTable(string pageName, string tableName)
        {
            string callingFun = "GetTable";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return null;
                return table;
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return null; }
        }

        public List<Template.Page.Table.Cell> GetCells(string pageName, string tableName, string columnName = "")
        {
            string callingFun = "GetCells";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return null;
                if (string.IsNullOrEmpty(columnName)) return table.cells;
                else return table.cells.FindAll(x => x.colName == columnName);

            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return null; }
        }
            /// <summary>
            /// allows adding or changing an element of a list, for example an action of the global-actions-list, or a row of the rows of a certain table
            /// </summary>
            /// <typeparam name="T">
            /// can be Action, Filter, Row, Column, Cell, OptionalVariable, RequiredVariable, etc.
            /// the type must provide the field 'name' (see 'modElement' below)
            /// the type must provide a function 'ApiMerge' (see 'funcApiMerge' and 'modifyMode' below)
            /// </typeparam>
            /// <param name="modElement">
            /// the element to add, respectively the values which should be changed
            /// if the function attempts to change an element, the field 'name' is used to identify this element
            /// </param>
            /// <param name="elementsList">
            /// the list of elements, i.e. the container of the element to add or change
            /// </param>
            /// <param name="funcApiMerge">
            /// a function that accomplishes the changes if 'modifyMode' is set to 'MergeKeep/Replace' (see below)
            /// </param>
            /// <param name="modifyMode">
            /// AddNew: 'modElement' is added, if 'elementsList' already contains an element with the same 'name' an error is issued
            /// AddOrKeep: 'modElement' is added, if 'elementsList' does not contain an element with the same 'name'
            /// AddOrReplace: 'modElement' is added, if 'elementsList' does not contain an element with the same 'name',
            ///               otherwise 'modElement' replaces the existing element
            /// MergeKeep: 'modElement' is combined with an existing element in 'elementsList' with the same 'name'
            ///            the merging is accomplished by function 'funcApiMerge', with parameter 'keep' set to true 
            ///            'keep' has in priniciple has the following meaning:
            ///            keep = true: the existing element's values are prioritised, i.e. only properties with default-values in the existing element and non-default values in 'modElement' are changed
            ///            keep = false: 'modElements' values are prioritised
            ///            if 'elementsList' does not contain an element with the same 'name' an error is issued
            /// MergeReplace: same as above, but parameter 'keep' set to false
            /// </param>
            /// <param name="addWhere">
            /// - adding (AddNew, AddOrKeep/Replace with not existing 'modElement'): 'modElement' is added XXX (see below)
            /// - replacing (AddOrKeep/Replace with existing 'modElement'): 'modElement' is moved XXX (see below)
            /// - merging (AddMergeKeep/Replace): parameter 'addWhere' is ignored
            ///   XXX: Tail: at/to the end of 'elementList'
            ///        Head: at/to the top of 'elementList'
            ///        After: after the 'elementList's element with 'name'='nextToElementName' (must exist, otherwise an error is issued)
            ///        Before: before the 'elementList's element with 'name'='nextToElementName' (must exist, otherwise an error is issued)
            /// if set to 'Appropriate', 'addWhere' is changed to 'Tail' on adding, otherwise the parameter is ignored
            /// </param>
            /// <param name="nextToElementName">
            /// used in combination with 'addWhere' set to 'After' or 'Before' (see above)
            /// </param>
            /// <param name="callingFun">
            /// used internally, i.e. not provided by API-user (only relevant for error-messages)
            /// </param>
            /// <param name="localMap">
            /// optional and used internally, i.e. not provided by API-user (only relevant for Actions and Filters)
            /// </param>
            /// <returns>
            /// true for success
            /// false for non-success: error is added to errorCollector
            /// </returns>
            private bool ModifyListElements<T>(T modElement, List<T> elementsList, Func<T, T, bool, bool> funcApiMerge, ModifyMode modifyMode, AddWhere addWhere, string nextToElementName, string callingFun, LocalMap localMap = null)
        {
            if (modElement == null) { AddError("Modify element must not be null.", callingFun); return false; }

            FieldInfo fieldInfoName = modElement.GetType().GetField("name");
            string elementType = modElement.GetType().Name;
            if (fieldInfoName == null) throw new Exception($"Function ModifyListElements works only with classes providing field 'name', which is not the case for {elementType}.");
            string elementName = fieldInfoName.GetValue(modElement) == null ? string.Empty : fieldInfoName.GetValue(modElement).ToString();

            T origElement = (from ele in elementsList
                             where elementName != string.Empty && fieldInfoName.GetValue(ele) != null &&
                                   fieldInfoName.GetValue(ele).ToString().ToLower() == elementName.ToLower()
                             select ele).FirstOrDefault();

            if (origElement != null && modifyMode == ModifyMode.AddNew)
            {
                AddError($"{elementType} with name '{elementName}' already exists (hint: consider using another ModifyMode).", callingFun);
                return false;
            }
            if (origElement == null && (modifyMode == ModifyMode.MergeKeep || modifyMode == ModifyMode.MergeReplace || modifyMode == ModifyMode.Delete))
            {
                AddError($"No {elementType} with name '{elementName}' exists (hint: consider using another ModifyMode).", callingFun);
                return false;
            }

            if (addWhere == AddWhere.Appropriate)
            {
                switch (modifyMode)
                {
                    case ModifyMode.AddNew:
                        addWhere = AddWhere.Tail; break;
                    case ModifyMode.AddOrKeep:
                    case ModifyMode.AddOrReplace:
                        if (origElement == null) addWhere = AddWhere.Tail;
                        else { addWhere = AddWhere.After; nextToElementName = elementName; }
                        break;
                }
            }

            int nextToElementIndex = -1;
            if (addWhere == AddWhere.Before || addWhere == AddWhere.After)
            {
                if (string.IsNullOrEmpty(nextToElementName))
                {
                    AddError($"If AddWhere.{addWhere} is used, parameter 'nextTo{elementType}Name' must not be empty.", callingFun);
                    return false;
                }
                T nextToElement = (from ele in elementsList
                                   where fieldInfoName.GetValue(ele) != null &&
                                         fieldInfoName.GetValue(ele).ToString().ToLower() == nextToElementName.ToLower()
                                   select ele).FirstOrDefault();
                if (nextToElement == null)
                {
                    AddError($"Invalid parameter 'nextTo{elementType}Name': no {elementType} with name '{nextToElementName}' exists.", callingFun);
                    return false;
                }
                nextToElementIndex = elementsList.IndexOf(nextToElement);
            }

            FieldInfo fieldInfoLocalMap = null;
            if (localMap != null)
            {
                fieldInfoLocalMap = modElement.GetType().GetField("localMap");
                if (fieldInfoLocalMap == null) throw new Exception($"{elementType} does not provide field 'localMap'");
            }

            switch (modifyMode)
            {
                case ModifyMode.AddNew:
                    AddElement(); return true;
                case ModifyMode.AddOrReplace:
                    AddElement(); if (origElement != null) elementsList.Remove(origElement); return true;
                case ModifyMode.AddOrKeep:
                    if (origElement == null) AddElement(); return true;
                case ModifyMode.MergeReplace:
                case ModifyMode.MergeKeep:
                    return funcApiMerge(origElement, modElement, modifyMode == ModifyMode.MergeKeep);
                case ModifyMode.Delete:
                    elementsList.Remove(origElement); return true;
                default:
                    throw new Exception($"Unhandled ModifyMode '{modifyMode}'");
            }

            void AddElement()
            {
                if (fieldInfoLocalMap != null) fieldInfoLocalMap.SetValue(modElement, localMap);
                switch (addWhere)
                {
                    case AddWhere.Head: elementsList.Insert(0, modElement); break;
                    case AddWhere.Tail: elementsList.Add(modElement); break;
                    case AddWhere.Before: elementsList.Insert(nextToElementIndex, modElement); break;
                    case AddWhere.After:
                        if (nextToElementIndex == elementsList.Count - 1) elementsList.Add(modElement);
                        else elementsList.Insert(nextToElementIndex + 1, modElement);
                        break;
                    default: throw new Exception($"Unhandled AddWhere '{addWhere}'");
                }
            }
        }

        #endregion MODIFY_LIST_ELEMENTS

        #region MODIFY_CELL-ACTION_ACTION-FILTER

        public bool ModifyCellAction_Table(Template.Action modAction, string pageName, string tableName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyCellAction_Table";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return ModifyActionOrFilter(ref table.cellAction, modAction, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellAction_Column(Template.Action modAction, string pageName, string tableName, string columnName, ModifyMode modifyMode = ModifyMode.MergeReplace, bool reform = false)
        {
            string callingFun = "ModifyCellAction_Column";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Column column, columnName, reform ? table.reformColumns : table.columns, true, callingFun)) return false;
                return ModifyActionOrFilter(ref column.cellAction, modAction, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellAction_Row(Template.Action modAction, string pageName, string tableName, string rowName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyCellAction_Row";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Row row, rowName, table.rows, true, callingFun)) return false;
                return ModifyActionOrFilter(ref row.cellAction, modAction, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellAction_Cell(Template.Action modAction, string pageName, string tableName, string cellName, ModifyMode modifyMode = ModifyMode.MergeReplace, bool reform = false)
        {
            string callingFun = "ModifyCellAction_Cell";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Cell cell, cellName, reform ? table.reformCells : table.cells, true, callingFun)) return false;
                return ModifyActionOrFilter(ref cell.cellAction, modAction, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        public bool ModifyCellFilter_Table(Template.Filter modFilter, string pageName, string tableName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyCellFilter_Table";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (table.cellAction == null) table.cellAction = new Template.Action(table.localMap);
                return ModifyActionOrFilter(ref table.cellAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellFilter_Column(Template.Filter modFilter, string pageName, string tableName, string columnName, ModifyMode modifyMode = ModifyMode.MergeReplace, bool reform = false)
        {
            string callingFun = "ModifyCellFilter_Column";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Column column, columnName, reform ? table.reformColumns : table.columns, true, callingFun)) return false;
                if (column.cellAction == null) column.cellAction = new Template.Action(table.localMap);
                return ModifyActionOrFilter(ref column.cellAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellFilter_Row(Template.Filter modFilter, string pageName, string tableName, string rowName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyCellFilter_Row";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Row row, rowName, table.rows, true, callingFun)) return false;
                if (row.cellAction == null) row.cellAction = new Template.Action(table.localMap);
                return ModifyActionOrFilter(ref row.cellAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyCellFilter_Cell(Template.Filter modFilter, string pageName, string tableName, string cellName, ModifyMode modifyMode = ModifyMode.MergeReplace, bool reform = false)
        {
            string callingFun = "ModifyCellFilter_Cell";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Page.Table.Cell cell, cellName, reform ? table.reformCells : table.cells, true, callingFun)) return false;
                if (cell.cellAction == null) cell.cellAction = new Template.Action(table.localMap);
                return ModifyActionOrFilter(ref cell.cellAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        public bool ModifyFilter_GlobalAction(Template.Filter modFilter, string globalActionName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyFilter_GlobalAction";
            try
            {
                if (!GetListElement(out Template.Action globalAction, globalActionName, template.globalActions, true, callingFun)) return false;
                return ModifyActionOrFilter(ref globalAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, null, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyFilter_PageAction(Template.Filter modFilter, string pageName, string pageActionName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyFilter_PageAction";
            try
            {
                if (!GetPage(out Template.Page page, pageName, callingFun)) return false;
                if (!GetListElement(out Template.Action pageAction, pageActionName, page.actions, true, callingFun)) return false;
                return ModifyActionOrFilter(ref pageAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, page.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool ModifyFilter_TableAction(Template.Filter modFilter, string pageName, string tableName, string tableActionName, ModifyMode modifyMode = ModifyMode.MergeReplace)
        {
            string callingFun = "ModifyFilter_TableAction";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                if (!GetListElement(out Template.Action tableAction, tableActionName, table.actions, true, callingFun)) return false;
                return ModifyActionOrFilter(ref tableAction.filter, modFilter, (orig, api, keep) => { return orig.ApiMerge(api, keep, errorCollector); }, modifyMode, table.localMap, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }


        /// <summary>
        /// allows adding or changing
        /// - either a Cell-Action of a Table or a TableElement
        /// - or a Filter of an Action (Cell-Action, Global/Page/Table-Action)
        /// </summary>
        /// <typeparam name="T">
        /// must be Action or Filter, otherwise an error is issued
        /// </typeparam>
        /// <reference-param name="origActFun">
        /// Cell-Action or Action-Filter to be added or changed
        /// </reference-param>
        /// <param name="modActFun">
        /// Cell-Action or Action-Filter to add, respectively the values which should be changed
        /// </param>
        /// <param name="funcApiMerge">
        /// a function that accomplishes the changes if 'modifyMode' is set to 'MergeKeep/Replace' (see below)
        /// </param>
        /// <param name="modifyMode">
        /// AddNew: 'modActFun' is added, if 'origActFun' exists an error is issued
        /// AddOrKeep: 'modActFun' is added, but only if 'origActFun' does not yet exist
        /// AddOrReplace: 'modElement' is added or replaced
        /// MergeKeep: if 'origActFun' exists, 'modActFun' is combined with 'modActFun' otherwise 'origActFun' is set to 'modActFun'
        ///            the merging is accomplished by function 'funcApiMerge', with parameter 'keep' set to true
        ///            (see description of function 'ModifyListElements' for description of parameter 'keep')
        /// MergeReplace: same as above, but parameter 'keep' set to false
        /// </param>
        /// <param name="localMap">used internally, i.e. not provided by API-user</param>
        /// <param name="callingFun">used internally, i.e. not provided by API-user (only relevant for error-messages)</param>
        /// <returns>
        /// true for success
        /// false for non-success: error is added to errorCollector
        /// </returns>
        private bool ModifyActionOrFilter<T>(ref T origActFun, T modActFun, Func<T, T, bool, bool> funcApiMerge, ModifyMode modifyMode, LocalMap localMap, string callingFun)
        {
            if (modActFun == null) { AddError("Modify CellAction/Filter must not be null.", callingFun); return false; }

            string actFun; if (modActFun is Template.Filter) actFun = "Filter";
            else if (modActFun is Template.Action) actFun = "CellAction";
            else throw new Exception("ModifyActionOrFilter only works with classes 'Template.Action' or 'Template.Filter'");

            switch (modifyMode)
            {
                case ModifyMode.AddNew:
                    if (origActFun == null) { origActFun = modActFun; break; }
                    AddError($"{actFun} already exists (hint: consider using another ModifyMode).", callingFun); return false;
                case ModifyMode.AddOrReplace:
                    origActFun = modActFun; break;
                case ModifyMode.AddOrKeep:
                    if (origActFun == null) origActFun = modActFun; break;
                case ModifyMode.MergeReplace:
                case ModifyMode.MergeKeep:
                    if (origActFun == null) { origActFun = modActFun; break; }
                    if (!funcApiMerge(origActFun, modActFun, modifyMode == ModifyMode.MergeKeep)) return false; break;
                default:
                    throw new Exception($"Unhandled ModifyMode '{modifyMode}'");
            }

            FieldInfo fieldInfoLocalMap = modActFun.GetType().GetField("localMap");
            if (fieldInfoLocalMap == null) throw new Exception($"{modActFun.GetType().Name} does not provide field 'localMap'"); // should not happen
            fieldInfoLocalMap.SetValue(modActFun, localMap);
            return true;
        }

        #endregion MODIFY_CELL-ACTION_ACTION-FILTER

        #region MODIFY_TEMPLATE-INFO_PAGE_TABLE

        public bool ModifyTemplateInfo(Template.TemplateInfo templateInfo, bool modifyModeKeep = false)
        {
            try
            {
                return template.info.ApiMerge(templateInfo, modifyModeKeep, errorCollector);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", "ChangeTemplateInfo"); return false; }
        }

        public bool ModifyPage(Template.Page page, bool modifyModeKeep = false)
        {
            try
            {
                if (!GetPage(out Template.Page origPage, page.name, "ModifyPage")) return false;
                return origPage.ApiMerge(page, modifyModeKeep, errorCollector);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", "ModifyPage"); return false; }
        }

        public bool ModifyTable(Template.Page.Table table, string pageName, bool modifyModeKeep = false)
        {
            try
            {
                if (!GetTable(out Template.Page.Table origTable, pageName, table.name, "ChangeTable")) return false;
                return origTable.ApiMerge(table, modifyModeKeep, errorCollector);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", "ChangeTable"); return false; }
        }

        #endregion MODIFY_TEMPLATE-INFO_PAGE_TABLE

        #region COPY

        public bool CopyPage(string origPageName, string copyPageName, AddWhere addWhere = AddWhere.After, string nextToPageName = null)
        {
            string callingFun = "CopyPage";
            try
            {
                return CopyListElement(origPageName, copyPageName, template.pages, addWhere, nextToPageName, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool CopyTable(string pageName, string origTableName, string copyTableName, AddWhere addWhere = AddWhere.After, string nextToTableName = null, string moveToPageName = null)
        {
            string callingFun = "CopyTable";
            try
            {
                if (!GetPage(out Template.Page page, pageName, callingFun)) return false;
                return CopyListElement(origTableName, copyTableName, page.tables, addWhere, nextToTableName, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool CopyRow(string pageName, string tableName, string origRowName, string copyRowName, AddWhere addWhere = AddWhere.After, string nextToRowName = null)
        {
            string callingFun = "CopyRow";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return CopyListElement(origRowName, copyRowName, table.rows, addWhere, nextToRowName, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool CopyColumn(string pageName, string tableName, string origColumnName, string copyColumnName, bool reform = false, AddWhere addWhere = AddWhere.After, string nextToColumnName = null)
        {
            string callingFun = "CopyColumn";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return CopyListElement(origColumnName, copyColumnName, reform ? table.reformColumns : table.columns, addWhere, nextToColumnName, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool CopyCell(string pageName, string tableName, string origCellName, string copyCellName, bool reform = false)
        {
            string callingFun = "CopyCell";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return CopyListElement(origCellName, copyCellName, reform ? table.reformCells : table.cells, AddWhere.After, null, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        public bool CopyCell(string pageName, string tableName, Template.Page.Table.Cell origElement, string copyCellName, bool reform = false)
        {
            string callingFun = "CopyCell";
            try
            {
                if (!GetTable(out Template.Page.Table table, pageName, tableName, callingFun)) return false;
                return CopyListElement(origElement, copyCellName, reform ? table.reformCells : table.cells, AddWhere.After, null, o => { return o.Clone(); }, callingFun);
            }
            catch (Exception exception) { AddError($"Unexpected error: {exception.Message}", callingFun); return false; }
        }

        // This function finds an element by name and duplicates it
        private bool CopyListElement<T>(string origName, string copyName, List<T> elementList, AddWhere addWhere, string nextToName, Func<T, T> funcClone, string callingFun)
        {
            if (!GetListElement(out T origElement, origName, elementList, true, callingFun)) return false;
            return CopyListElement(origElement, copyName, elementList, addWhere, nextToName, funcClone, callingFun);
        }

        // This function duplicates a given element
        private bool CopyListElement<T>(T origElement, string copyName, List<T> elementList, AddWhere addWhere, string nextToName, Func<T, T> funcClone, string callingFun)
        {
            FieldInfo fieldInfoName = typeof(T).GetField("name");
            if (fieldInfoName == null)
            {
                AddError($"Function CopyListElement works only with classes providing field 'name', which is not the case for {typeof(T).Name}.", callingFun);
                return false;
            }

            int indexNextTo = -1;
            if (addWhere == AddWhere.After || addWhere == AddWhere.Before)
            {
                if (string.IsNullOrEmpty(nextToName)) indexNextTo = elementList.IndexOf(origElement);
                else
                {
                    if (!GetListElement(out T nextToElement, nextToName, elementList, true, callingFun)) return false;
                    indexNextTo = elementList.IndexOf(nextToElement);
                }
            }

            T copyElement = funcClone(origElement);
            fieldInfoName.SetValue(copyElement, copyName);

            if (copyElement is Template.Page) ChangePageLocalMap(copyElement as Template.Page);
            if (copyElement is Template.Page.Table) ChangeTableLocalMap(copyElement as Template.Page.Table);

            switch (addWhere)
            {
                case AddWhere.Head: elementList.Insert(0, copyElement); return true;
                case AddWhere.Tail: elementList.Add(copyElement); return true;
                case AddWhere.Before: elementList.Insert(indexNextTo, copyElement); return true;
                case AddWhere.After:
                    if (indexNextTo == elementList.Count - 1) elementList.Add(copyElement);
                    else elementList.Insert(indexNextTo + 1, copyElement);
                    return true;
                default: throw new Exception($"Unhandled AddWhere '{addWhere}'");
            }

            void ChangePageLocalMap(Template.Page page)
            {
                page.localMap = LocalMap.NewPageLocalMap();
                foreach (Template.Page.Table table in page.tables) ChangeTableLocalMap(table, page.localMap);
                foreach (Template.Action action in page.actions) action.localMap = page.localMap;
                foreach (Template.Filter filter in page.filters) filter.localMap = page.localMap;
            }

            void ChangeTableLocalMap(Template.Page.Table table, LocalMap pageLocalMap = null)
            {
                table.localMap = LocalMap.NewTableLocalMap(pageLocalMap ?? new LocalMap(table.localMap.GetPageId()));
                foreach (Template.Action action in table.actions) action.localMap = table.localMap;
                foreach (Template.Filter filter in table.filters) filter.localMap = table.localMap;
            }
        }

        #endregion COPY

        #region MODIFY_TITELS_AND_FORMULAS

        public void SetLabels(List<PrettyInfoProvider.PackageLabels> labels)
        {
            PrettyInfoProvider.labels = labels;
        }

        public void ReplaceInTitles(string replace, string by,
                                    string pageNamePattern = "-", bool replaceInPageTitles = false,
                                    string tableNamePattern = "-", bool replaceInTableTitles = false,
                                    string columnNamePattern = "-", string reformColumnNamePattern = "-",
                                    string rowNamePattern = "-")
        {
            foreach (Template.Page page in template.pages)
            {
                if (!SearchPatternMatches(pageNamePattern, page.name)) continue;
                if (replaceInPageTitles) page.title = Replace(page.title, replace, by);
                foreach (Template.Page.Table table in page.tables)
                {
                    if (!SearchPatternMatches(tableNamePattern, table.name)) continue;
                    if (replaceInTableTitles) table.title = Replace(table.title, replace, by);
                    foreach (Template.Page.Table.Column column in table.columns)
                    {
                        if (!SearchPatternMatches(columnNamePattern, column.name)) continue;
                        column.title = Replace(column.title, replace, by);
                    }
                    foreach (Template.Page.Table.Column reformColumn in table.reformColumns)
                    {
                        if (!SearchPatternMatches(reformColumnNamePattern, reformColumn.name)) continue;
                        reformColumn.title = Replace(reformColumn.title, replace, by);
                    }
                    foreach (Template.Page.Table.Row row in table.rows)
                    {
                        if (!SearchPatternMatches(rowNamePattern, row.name)) continue;
                        row.title = Replace(row.title, replace, by);
                    }
                }
            }
        }

        public void ReplaceInActionFormulas(string replace, string by, bool replaceInGlobalFormulas = false,
                                            string pageNamePattern = "-", bool replaceInPageFormulas = false,
                                            string tableNamePattern = "-")
        {
            if (replaceInGlobalFormulas)
                foreach (Template.Action action in template.globalActions)
                    if (!string.IsNullOrEmpty(action.formulaString)) action.formulaString = Replace(action.formulaString, replace, by);
            
            foreach (Template.Page page in template.pages)
            {
                if (!SearchPatternMatches(pageNamePattern, page.name)) continue;
                if (replaceInPageFormulas)
                    foreach (Template.Action action in page.actions)
                        if (!string.IsNullOrEmpty(action.formulaString)) action.formulaString = Replace(action.formulaString, replace, by);
                
                foreach (Template.Page.Table table in page.tables)
                {
                    if (!SearchPatternMatches(tableNamePattern, table.name)) continue;
                    foreach (Template.Action action in table.actions)
                        if (!string.IsNullOrEmpty(action.formulaString)) action.formulaString = Replace(action.formulaString, replace, by);
                }
            }
        }

        public void ReplaceInCellActionFormulas(string replace, string by,
                                                string pageNamePattern = "-",
                                                string tableNamePattern = "-", bool replaceInTableFormulas = false,
                                                string columnNamePattern = "-", string reformColumnNamePattern = "-",
                                                string rowNamePattern = "-",
                                                string cellNamePattern = "-", string reformCellNamePattern = "-")
        {
            foreach (Template.Page page in template.pages)
            {
                if (!SearchPatternMatches(pageNamePattern, page.name)) continue;
                foreach (Template.Page.Table table in page.tables)
                {
                    if (!SearchPatternMatches(tableNamePattern, table.name)) continue;
                    if (replaceInTableFormulas && table.cellAction != null && !string.IsNullOrEmpty(table.cellAction.formulaString))
                        table.cellAction.formulaString = Replace(table.cellAction.formulaString, replace, by);
                    foreach (Template.Page.Table.Column column in table.columns)
                    {
                        if (!SearchPatternMatches(columnNamePattern, column.name)) continue;
                        if (column.cellAction != null && !string.IsNullOrEmpty(column.cellAction.formulaString))
                            column.cellAction.formulaString = Replace(column.cellAction.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Column reformColumn in table.reformColumns)
                    {
                        if (!SearchPatternMatches(reformColumnNamePattern, reformColumn.name)) continue;
                        if (reformColumn.cellAction != null && !string.IsNullOrEmpty(reformColumn.cellAction.formulaString))
                            reformColumn.cellAction.formulaString = Replace(reformColumn.cellAction.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Row row in table.rows)
                    {
                        if (!SearchPatternMatches(rowNamePattern, row.name)) continue;
                        if (row.cellAction != null && !string.IsNullOrEmpty(row.cellAction.formulaString))
                            row.cellAction.formulaString = Replace(row.cellAction.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Cell cell in table.cells)
                    {
                        if (!SearchPatternMatches(cellNamePattern, cell.name)) continue;
                        if (cell.cellAction != null && !string.IsNullOrEmpty(cell.cellAction.formulaString))
                            cell.cellAction.formulaString = Replace(cell.cellAction.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Cell reformCell in table.reformCells)
                    {
                        if (!SearchPatternMatches(reformCellNamePattern, reformCell.name)) continue;
                        if (reformCell.cellAction != null && !string.IsNullOrEmpty(reformCell.cellAction.formulaString))
                            reformCell.cellAction.formulaString = Replace(reformCell.cellAction.formulaString, replace, by);
                    }
                }
            }
        }

        public void ReplaceInFilterFormulas(string replace, string by, bool replaceInGlobalFormulas,
                                            string pageNamePattern = "-", bool replaceInPageFormulas = false,
                                            string tableNamePattern = "-")
        {
            if (replaceInGlobalFormulas)
                foreach (Template.Filter filter in template.globalFilters)
                    if (!string.IsNullOrEmpty(filter.formulaString)) filter.formulaString = Replace(filter.formulaString, replace, by);

            foreach (Template.Page page in template.pages)
            {
                if (!SearchPatternMatches(pageNamePattern, page.name)) continue;
                if (replaceInPageFormulas)
                    foreach (Template.Filter filter in page.filters)
                        if (!string.IsNullOrEmpty(filter.formulaString)) filter.formulaString = Replace(filter.formulaString, replace, by);

                foreach (Template.Page.Table table in page.tables)
                {
                    if (!SearchPatternMatches(tableNamePattern, table.name)) continue;
                    foreach (Template.Filter filter in table.filters)
                        if (!string.IsNullOrEmpty(filter.formulaString)) filter.formulaString = Replace(filter.formulaString, replace, by);
                }
            }
        }

        public void ReplaceInCellFilterFormulas(string replace, string by,
                                                string pageNamePattern = "-",
                                                string tableNamePattern = "-", bool replaceInTableFormulas = false,
                                                string columnNamePattern = "-", string reformColumnNamePattern = "-",
                                                string rowNamePattern = "-",
                                                string cellNamePattern = "-", string reformCellNamePattern = "-")
        {
            foreach (Template.Page page in template.pages)
            {
                if (!SearchPatternMatches(pageNamePattern, page.name)) continue;
                foreach (Template.Page.Table table in page.tables)
                {
                    if (!SearchPatternMatches(tableNamePattern, table.name)) continue;
                    if (replaceInTableFormulas && table.cellAction != null && table.cellAction.filter != null && !string.IsNullOrEmpty(table.cellAction.filter.formulaString))
                        table.cellAction.filter.formulaString = Replace(table.cellAction.filter.formulaString, replace, by);
                    foreach (Template.Page.Table.Column column in table.columns)
                    {
                        if (!SearchPatternMatches(columnNamePattern, column.name)) continue;
                        if (column.cellAction != null && column.cellAction.filter != null  && !string.IsNullOrEmpty(column.cellAction.filter.formulaString))
                            column.cellAction.filter.formulaString = Replace(column.cellAction.filter.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Column reformColumn in table.reformColumns)
                    {
                        if (!SearchPatternMatches(reformColumnNamePattern, reformColumn.name)) continue;
                        if (reformColumn.cellAction != null && reformColumn.cellAction.filter != null && !string.IsNullOrEmpty(reformColumn.cellAction.filter.formulaString))
                            reformColumn.cellAction.filter.formulaString = Replace(reformColumn.cellAction.filter.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Row row in table.rows)
                    {
                        if (!SearchPatternMatches(rowNamePattern, row.name)) continue;
                        if (row.cellAction != null && row.cellAction.filter != null && !string.IsNullOrEmpty(row.cellAction.filter.formulaString))
                            row.cellAction.filter.formulaString = Replace(row.cellAction.filter.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Cell cell in table.cells)
                    {
                        if (!SearchPatternMatches(cellNamePattern, cell.name)) continue;
                        if (cell.cellAction != null && cell.cellAction.filter != null && !string.IsNullOrEmpty(cell.cellAction.filter.formulaString))
                            cell.cellAction.filter.formulaString = Replace(cell.cellAction.filter.formulaString, replace, by);
                    }
                    foreach (Template.Page.Table.Cell reformCell in table.reformCells)
                    {
                        if (!SearchPatternMatches(reformCellNamePattern, reformCell.name)) continue;
                        if (reformCell.cellAction != null && reformCell.cellAction.filter != null && !string.IsNullOrEmpty(reformCell.cellAction.filter.formulaString))
                            reformCell.cellAction.filter.formulaString = Replace(reformCell.cellAction.filter.formulaString, replace, by);
                    }
                }
            }
        }

        private static string Replace(string text, string replace, string by)
        {
            return text.Replace(replace, by); // this can be rewritten to sth more complicated, if necessary
        }

        private static bool SearchPatternMatches(string pattern, string name)
        {
            if (string.IsNullOrEmpty(pattern) || pattern == "*") return true;
            if (string.IsNullOrEmpty(name) || pattern == "-") return false;
            return EM_Helpers.DoesValueMatchPattern(pattern, name);
        }

        #endregion MODIFY_TITELS_AND_FORMULAS

        #region PRIVAT_HELPERS

        private bool GetListElement<T>(out T element, string elementName, List<T> elementList, bool? mustExist, string callingFun)
        {
            element = default;

            FieldInfo fieldInfoName = typeof(T).GetField("name");
            string elementType = typeof(T).Name;

            if (fieldInfoName == null)
            {
                AddError($"Function GetListElement works only with classes providing field 'name', which is not the case for {elementType}.", callingFun);
                return false;
            }

            if (string.IsNullOrEmpty(elementName))
            {
                if (mustExist != true) return true;
                AddError($"Searching for unnamed {elementType}.", callingFun); return false;
            }

            element = (from e in elementList
                       where fieldInfoName.GetValue(e) != null && fieldInfoName.GetValue(e).ToString().ToLower() == elementName.ToLower()
                       select e).FirstOrDefault();

            if (element == null && mustExist == true)
            {
                AddError($"No {elementType} with name '{elementName}' found.", callingFun);
                return false;
            }
            if (element != null && mustExist == false)
            {
                AddError($"{elementType} with name '{elementName}' already exists.", callingFun);
                return false;
            }
            return true;
        }

        private bool GetPage(out Template.Page page, string pageName, string callingFun = null)
        {
            return GetListElement(out page, pageName, template.pages, true, callingFun ?? "GetPage");
        }

        private bool GetTable(out Template.Page.Table table, string pageName, string tableName, string callingFun = null)
        {
            table = null;
            return !GetPage(out Template.Page page, pageName, callingFun ?? "GetTable") ? false :
                    GetListElement(out table, tableName, page.tables, true, callingFun ?? "GetTable");
        }

        public bool AddError(string error, string callingFun = null)
        {
            errorCollector.AddError($"TemplateApi{(callingFun == null ? string.Empty : $" {callingFun}")}: {error}"); return false;
        }

        #endregion PRIVAT_HELPERS
    }
}
