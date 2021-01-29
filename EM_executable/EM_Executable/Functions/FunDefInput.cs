using EM_Common;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace EM_Executable
{
    internal class FunDefInput : FunOutOfSpineBase
    {
        internal FunDefInput(InfoStore infoStore) : base(infoStore) { }

        private VarSpec rowMergeVar = null, colMergeVar = null, inputVar = null;
        private string path = string.Empty, file = string.Empty, repByEMPath = string.Empty;
        private bool doRanges = false, lookUpMode = false;
        private int ignoreNRows = 0, ignoreNCols = 0;
        private ParNumber parDefaultIfNoMatch = null;

        protected override void PrepareNonCommonPar()
        {
            path = GetParBaseValueOrDefault(DefFun.DefInput, DefPar.DefInput.path); // compulsory
            file = GetParBaseValueOrDefault(DefFun.DefInput, DefPar.DefInput.file); // compulsory

            ParVar parRowMergeVar = GetUniquePar<ParVar>(DefPar.DefInput.RowMergeVar); // compulsory
            if (parRowMergeVar != null) rowMergeVar = new VarSpec() { name = parRowMergeVar.name };
            ParVar parColMergeVar = GetUniquePar<ParVar>(DefPar.DefInput.ColMergeVar);
            if (parColMergeVar != null) colMergeVar = new VarSpec() { name = parColMergeVar.name };
            ParVar parInputVar = GetUniquePar<ParVar>(DefPar.DefInput.InputVar);
            if (parInputVar != null) inputVar = new VarSpec() { name = parInputVar.name };

            if ((colMergeVar != null && inputVar == null) || (colMergeVar == null && inputVar != null))
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: if {DefPar.DefInput.ColMergeVar} is defined {DefPar.DefInput.InputVar} must be defined too, and vice-versa" });
            lookUpMode = colMergeVar != null;

            repByEMPath = GetParBaseValueOrDefault(DefFun.DefInput, DefPar.DefInput.RepByEMPath);
            ignoreNRows = (int)GetParNumberValueOrDefault(DefFun.DefInput, DefPar.DefInput.IgnoreNRows);
            ignoreNCols = (int)GetParNumberValueOrDefault(DefFun.DefInput, DefPar.DefInput.IgnoreNCols);
            doRanges = GetParBoolValueOrDefault(DefFun.DefInput, DefPar.DefInput.DoRanges);
            parDefaultIfNoMatch = GetUniquePar<ParNumber>(DefPar.DefInput.DefaultIfNoMatch);

            // * * *   T O D O   * * *
            // consider parameters that allow defining and initialising variables
            // currently all input-variables need to be defined with DefVar or, if they are defined in variables-file, initialised with SetDefault
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            if (rowMergeVar != null) rowMergeVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(rowMergeVar.name);
            if (colMergeVar != null) colMergeVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(colMergeVar.name);
            if (inputVar != null) inputVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(inputVar.name);
        }

        protected override void DoFunWork()
        {
            string fullPath = FunCallProgramme.ComposePath(path, file, repByEMPath, infoStore);
            if (!fullPath.EndsWith(".txt")) fullPath += ".txt";
            if (lookUpMode) DoFunWork_LookUpMode(fullPath); // e.g. 1st row of input data represents gender and 1st col of input data represents regions
                                                            // for each person one variable (inputVar) is imported: input-data[gender of person, region of person]
            else DoFunWork_InputMode(fullPath); // e.g. input-data contains idPerson and x other variables - all variables are imported using idPerson to merge
        }

        private void DoFunWork_InputMode(string fullPath)
        {
            if (!Read_InputMode(fullPath,
                                out Dictionary<double, List<double>> inputContent, // key: value of merge var (e.g. idPerson), value: values of other variables
                                out Dictionary<int, int> indexMatches)) // key: index of variable in data-array, value: index of variable in inputContent.value
                return;

            double noMatchVal = 0; if (parDefaultIfNoMatch != null) noMatchVal = parDefaultIfNoMatch.GetValue();
            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                for (int indexPerson = 0; indexPerson < hh.GetPersonCount(); ++indexPerson)
                {
                    double mergeVal = hh.GetPersonValue(rowMergeVar.index, indexPerson);
                    List<double> inputValues = null;
                    if (inputContent.ContainsKey(mergeVal)) inputValues = inputContent[mergeVal];
                    else if (parDefaultIfNoMatch == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                        message = $"{description.Get()}: no match found for {DefVarName.IDPERSON} {infoStore.GetIDPerson(hh, indexPerson)} ({rowMergeVar.name}={mergeVal}), zero is used as default" });

                    foreach (var indexMatch in indexMatches)
                    {
                        double value = inputValues == null ? noMatchVal : inputValues[indexMatch.Value];
                        hh.SetPersonValue(value, indexMatch.Key, indexPerson);
                    }
                }
            }
        }

        private void DoFunWork_LookUpMode(string fullPath)
        {
            if (!Read_LookUpMode(fullPath, out Dictionary<double, List<double>> rows, out List<double> cols)) return;

            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                for (int indexPerson = 0; indexPerson < hh.GetPersonCount(); ++indexPerson)
                {
                    double rowVal = hh.GetPersonValue(rowMergeVar.index, indexPerson);
                    double colVal = hh.GetPersonValue(colMergeVar.index, indexPerson);

                    double value = 0; if (parDefaultIfNoMatch != null) value = parDefaultIfNoMatch.GetValue();

                    if (doRanges) FindCellDoRanges(); else FindCell();                   

                    hh.SetPersonValue(value, inputVar.index, indexPerson);

                    void FindCell()
                    {
                        int colIndex = -1;
                        if (cols.Contains(colVal)) colIndex = cols.IndexOf(colVal);
                        else if (parDefaultIfNoMatch == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                            message = $"{description.Get()}: no match found for {DefVarName.IDPERSON} {infoStore.GetIDPerson(hh, indexPerson)} ({colMergeVar.name}={colVal}), zero is used as default" });

                        if (colIndex >= 0)
                        {
                            if (rows.ContainsKey(rowVal)) value = rows[rowVal][colIndex];
                            else if (parDefaultIfNoMatch == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                                message = $"{description.Get()}: no match found for {DefVarName.IDPERSON} {infoStore.GetIDPerson(hh, indexPerson)} ({rowMergeVar.name}={rowVal}), zero is used as default" });
                        }
                    }

                    void FindCellDoRanges()
                    {
                        int colIndex = -1;
                        foreach (double cv in cols) if (colVal <= cv) { colIndex = cols.IndexOf(cv); break; }
                        if (colIndex == -1)
                        {
                            if (parDefaultIfNoMatch == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                                message = $"{description.Get()}: no match found for {DefVarName.IDPERSON} {infoStore.GetIDPerson(hh, indexPerson)} ({colMergeVar.name}={colVal}), zero is used as default" });
                        }
                        else
                        {
                            bool found = false;  foreach (double rv in rows.Keys) if (rowVal <= rv) { value = rows[rv][colIndex]; found = true; break; }
                            if (!found && parDefaultIfNoMatch == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                                message = $"{description.Get()}: no match found for {DefVarName.IDPERSON} {infoStore.GetIDPerson(hh, indexPerson)} ({rowMergeVar.name}={rowVal}), zero is used as default" });
                        }
                    }
                }
            }
        }

        private bool Read_InputMode(string fullPath, out Dictionary<double, List<double>> inputContent, out Dictionary<int, int> indexMatches)
        {
            inputContent = new Dictionary<double, List<double>>(); indexMatches = new Dictionary<int, int>();
            try
            {
                var inputLines = File.ReadLines(fullPath).Skip(ignoreNRows);
                List<string> headers = new List<string>(); foreach (string v in inputLines.First().ToLower().Split('\t').Skip(ignoreNCols)) headers.Add(v.Trim());

                int indexMergeVar = -1; Dictionary<int, int> tempIndexMatches = new Dictionary<int, int>();
                for (int iInputCol = 0; iInputCol < headers.Count; ++iInputCol)
                {
                    string varName = headers[iInputCol];
                    if (infoStore.operandAdmin.Exists(varName))
                    {
                        int iData = infoStore.operandAdmin.GetIndexInPersonVarList(varName);
                        if (iData >= 0)
                        {
                            if (iData == rowMergeVar.index) indexMergeVar = iInputCol; else tempIndexMatches.Add(iData, iInputCol);
                            continue;
                        }
                    }
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: {varName} is not the name of an existing variable ({varName} is not inputted)" });
                }

                if (indexMergeVar == -1)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: merge-variable {rowMergeVar.name} not found in input data - input is canceled" });
                    return false;
                }

                if (tempIndexMatches.Count == 0)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: no (valid) variables for input found - input is canceled" });
                    return false;
                }

                string idErr1 = Guid.NewGuid().ToString(), idErr2 = Guid.NewGuid().ToString(), idErr3 = Guid.NewGuid().ToString(), idErr4 = Guid.NewGuid().ToString();
                foreach (string inputLine in inputLines.Skip(1))
                {
                    if (inputLine == string.Empty) continue;
                    var splitInputLine = inputLine.Split('\t').Skip(ignoreNCols);

                    if (splitInputLine.Count() < headers.Count)
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = idErr1,
                            message = $"{description.Get()}: too short line is ignored: ({(inputLine.Length > 20 ? inputLine.Substring(0, 20) : inputLine)})" });
                        continue;
                    }

                    string strValMergeVar = splitInputLine.ElementAt(indexMergeVar);
                    if (!double.TryParse(EM_Helpers.AdaptDecimalSign(strValMergeVar), out double valMergeVar))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = idErr2,
                            message = $"{description.Get()}: value of {rowMergeVar.name} {strValMergeVar} is not a valid number - line is ignored" });
                        continue;
                    }

                    if (inputContent.ContainsKey(valMergeVar))
                    { 
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = idErr3,
                            message = $"{description.Get()}: {rowMergeVar.name} is not unique ({valMergeVar}) - only first value is taken into account" });
                        continue;
                    }

                    List<double> valOtherVar = new List<double>(tempIndexMatches.Count);
                    foreach (int im in tempIndexMatches.Values)
                    {
                        if (!double.TryParse(EM_Helpers.AdaptDecimalSign(splitInputLine.ElementAt(im)), out double number))
                        {
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = idErr4,
                                message = $"{description.Get()}: {splitInputLine.ElementAt(im)} is not a valid number - 0 is used as default" });
                            number = 0;
                        }
                        valOtherVar.Add(number);
                    }
                    inputContent.Add(valMergeVar, valOtherVar);
                }

                // example: input-row: inputVar1, mergeVar, inputVar2, ...
                //          tempIndexMatches = {27, 0}, {88, 2} // the first value is a phantasie-value for the index in the data-array
                //          indexMatches = {27, 0}, {88, 1}
                foreach (var im in tempIndexMatches) indexMatches.Add(im.Key, im.Value > indexMergeVar ? im.Value - 1 : im.Value);

                return true;
            }
            catch (Exception exception)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                    message = $"{description.Get()}: {exception.Message} - input is canceled" });
                return false;
            }
        }

        private bool Read_LookUpMode(string fullPath, out Dictionary<double, List<double>> rows, out List<double> cols)
        {
            rows = new Dictionary<double, List<double>>(); cols = new List<double>();
            try
            {
                var inputLines = File.ReadLines(fullPath).Skip(ignoreNRows);
                foreach (string cv in inputLines.First().Split('\t').Skip(ignoreNCols + 1)) // first column must contain row-identifiers
                {
                    if (!double.TryParse(EM_Helpers.AdaptDecimalSign(cv), out double colValue))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                            message = $"{description.Get()}: {cv} is not a valid number - input is canceled" });
                        return false;
                    }
                    if (cols.Contains(colValue))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                            message = $"{description.Get()}: colum identifier is not unique ({colValue}) - input is canceled" });
                        return false;
                    }
                    cols.Add(colValue);
                }
                if (cols.Count == 0)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: data contains no columns - input is canceled" });
                    return false;
                }

                string idErr1 = Guid.NewGuid().ToString();
                foreach (string inputLine in inputLines.Skip(1))
                {
                    if (inputLine == string.Empty) continue;
                    var splitInputLine = inputLine.Split('\t').Skip(ignoreNCols);

                    if (splitInputLine.Count() < cols.Count + 1)
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = idErr1,
                            message = $"{description.Get()}: too short line is ignored: ({(inputLine.Length > 20 ? inputLine.Substring(0, 20) : inputLine)})" });
                        continue;
                    }

                    double rowIdentifier = 0; List<double> rowValues = new List<double>();
                    for (int c = 0; c < splitInputLine.Count(); ++c)
                    {
                        if (!double.TryParse(EM_Helpers.AdaptDecimalSign(splitInputLine.ElementAt(c)), out double value))
                        {
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                                message = $"{description.Get()}: {splitInputLine.ElementAt(c)} is not a valid number - input is canceled" });
                            return false;
                        }
                        if (c == 0) rowIdentifier = value; else rowValues.Add(value);
                    }
                    if (rows.ContainsKey(rowIdentifier))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                            message = $"{description.Get()}: row identifier is not unique ({rowIdentifier}) - input is canceled" });
                        return false;
                    }
                    rows.Add(rowIdentifier, rowValues);
                }

                if (rows.Count == 0)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: data contains no rows - input is canceled" });
                    return false;
                }

                if (lookUpMode) { if (!CheckAsc(cols, "column ranges") || !CheckAsc(rows.Keys.ToList(), "row ranges")) return false; }

                return true;

                bool CheckAsc(List<double> vals, string what)
                {
                    double prev = double.MinValue;
                    foreach (double c in vals)
                    {
                        if (c < prev)
                        {
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                                message = $"{description.Get()}: {what} are not ascending ({prev} > {c}) - input is canceled" });
                            return false;
                        }
                        prev = c;
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                    message = $"{description.Get()}: {exception.Message} - input is canceled" });
                return false;
            }
        }
    }
}
