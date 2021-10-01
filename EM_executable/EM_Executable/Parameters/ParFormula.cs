using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class ParFormula : ParBase
    {
        internal ParFormula(InfoStore infoStore, bool _isCond = false) : base(infoStore)
        {
            isCond = _isCond;
            calc = new Calculator(infoStore.parScaleFactor, isCond);
        }

        internal Calculator calc;

        internal readonly bool isCond;

        internal Dictionary<string, ParBase_FormulaComponent> operands = new Dictionary<string, ParBase_FormulaComponent>();

        Calculator_Expression parsedExpression;
        internal override void CheckAndPrepare(FunBase fun)
        {
            if (FunIlArithOp.HandlesCheckAndPrepare(this)) return; // IlArithOp has a special handling for the Formula parameter (see FunIlArithOp.cs)

            if (xmlValue.Contains("}#"))
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, runTimeErrorId = description.funID, message = $"{description.Get()}: Footnotes should not be placed outside curly brackets! Please make sure that all footnotes are directly applied to a single operand." });
            }
            // for optimisation reasons handle #Amount and periods (#y, #w, ...) before parsing the formula:
            // alternatively one could allow for PAR_TYPE.NUMBER in the switch below, which would create a ParNumber that takes care
            // but then e.g. 1200#y*3 would, for each HH, ask for parNumber.GetValue()*3, while with this "preparsing" we just have 300
            string preParsedFormula = PreParse(xmlValue, fun);

            // parse the main formula
            try { parsedExpression = calc.CompileExpression(preParsedFormula); }
            catch (Exception e)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: {e.Message}" }); return;
            }

            foreach (string op in calc.Variables.Keys) // parse the formula ...
            {
                ParBase_FormulaComponent opPar = null;  // ... and prepare the operands

                GetFootnoteNumbers(op, out string opName); // temporarily remove potential footnotes to get the pure operand (e.g. yem#1->yem)

                // handle "partner:yem" or "head:yem"
                if (opName.Contains(":"))
                {
                    // this is not allowed outside DefTU!
                    if (description.fun.Name != DefFun.DefTu)
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $"{description.Get()}: invalid use of target outside of a DefTU '{opName}'." });
                    opName = opName.Substring(opName.IndexOf(":") + 1);
                }

                if (infoStore.operandAdmin.Exists(opName))
                {
                    switch (infoStore.operandAdmin.GetParType(opName))
                    {
                        //case DefPar.PAR_TYPE.NUMBER: opPar = new ParNumber(infoStore); break; // see PreParse above
                        case DefPar.PAR_TYPE.VAR: opPar = new ParVar(infoStore); break;
                        case DefPar.PAR_TYPE.IL: opPar = new ParIL(infoStore); break;
                        case DefPar.PAR_TYPE.QUERY: opPar = new ParQuery(infoStore); break;
                    }
                }
                else opPar = new ParVar(infoStore); // if not yet in the operand-list, the only (not faulty) option is a data-variable

                opPar.description = description;
                opPar.xmlValue = op;
                opPar.CheckAndPrepare(fun);
                operands.Add(op, opPar);
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            foreach (ParBase op in operands.Values) op.ReplaceVarNameByIndex();
        }

        // important note on parameter person:
        // person != null means that the caller explicitly asks for the value of a specific person (e.g. for condition evaluation)
        // person == null means the caller leaves it to the operands to decide the level, i.e. person or tu, which means for ...
        // - ParVar: monetary: tu-level, non-monetary: person-level, i.e. head
        // - ParIL: tu-level
        // - ParQuery: depends on the specific query, e.g. IsMarried -> head, nPersonsInTU -> tu
        internal double GetValue(HH hh, List<Person> tu, Person person = null)
        {
            Dictionary<string, double> ops = AssessOperands(hh, tu, person);
            return GetValue(ops, hh);
        }

        private Dictionary<string, double> AssessOperands(HH hh, List<Person> tu, Person person)
        {
            Dictionary<string, double> ops = new Dictionary<string, double>();
            foreach (var op in operands) ops.Add(op.Key, op.Value.GetValue(hh, tu, person));
            return ops;
        }

        // this is either called internally (see GetValue above) or by ParCond which does the operand assessment differently
        // (in essence it needs to know whether the condition is fulfilled for each person in the TU, thus individual operands
        // are assessed on individual level for the very person, other operands as usual)
        // HH is only passed to improve the error message
        internal double GetValue(Dictionary<string, double> ops, HH hh)
        {
            double x = 0;
            try {
                x = calc.CalculateExpression(ops, parsedExpression);
                if (double.IsNaN(x))
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = true, message = $"{description.Get()}: Invalid formula: division by zero ('{xmlValue}'{(hh.personVarList.Count>0?(" in hh '" + infoStore.GetIDHH(hh).ToString() + "'"):"")}), setting result to zero", runTimeErrorId = description.parID });
                    x = 0;
                }
            }
            catch (Exception e)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: {e.Message}", runTimeErrorId = description.parID });
            }
            return x;
        }

        /// <summary>
        /// does formula only use variables which are equal for each person, this is necessary for checking RunCond
        /// strong = true: condition may only contain global queries and numbers, i.e. no global variables (which may change at run-time)
        /// </summary>
        internal bool IsGlobal(bool strong = false)
        {
            foreach (var op in operands.Values)
                if (!op.isGlobal || (strong && op.GetType() == typeof(ParVar)))
                    return false;
            return true;
        }

        /// <summary> used for getting value at read time (provided the formula is global) or for global conditions (e.g. RunCond) </summary>
        internal double GetGlobalValue()
        {
            return GetValue(infoStore.hhAdmin == null ? new HH(infoStore) : infoStore.hhAdmin.GetFirstHH(),
                            new List<Person>() { new Person(0) });
        }

        private string PreParse(string formula, FunBase fun)
        {
            formula = EM_Helpers.RemoveWhitespace(formula).ToLower();
            formula = HandleAmount(formula, fun);
            formula = HandleOldStyleAmount(formula, fun); // this takes care of (very) old style {xyz < amount}#1
            formula = BenCalcBase(formula, fun); // BenCalc's conditions and formulas can contain "$Base"
            return formula;
        }

        private string HandleAmount(string formula, FunBase fun)
        {
            string amount = DefPar.Value.AMOUNT.ToLower() + "#";
            int amountIndex = formula.IndexOf(amount);
            while (amountIndex >= 0)
            {
                if (amountIndex == 0 || !calc.IsVarChar(formula[amountIndex - 1])) // make sure not touching abc_amount#1
                {
                    string number = string.Empty;
                    for (int i = amountIndex + amount.Length; i < formula.Length; ++i)
                    {
                        if ((formula[i] >= '0' && formula[i] <= '9') || (formula.ToLower()[i] >= 'a' && formula.ToLower()[i] <= 'z'))
                            number += formula[i];
                        else break;
                    }

                    ParBase parAmount = fun.GetFootnotePar<ParBase>(DefPar.Footnote.Amount, new List<string>() { number });
                    string replaceBy = "0";
                    if (parAmount != null) replaceBy = parAmount.xmlValue;
                    else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: missing footnote {DefPar.Footnote.Amount}({number})" });

                    formula = formula.Remove(amountIndex, amount.Length + number.Length);
                    formula = formula.Insert(amountIndex, replaceBy);
                }
                amountIndex = formula.IndexOf(amount, amountIndex + 1);
            }
            return formula;
        }

        // this handles the old style syntax which puts the footnote outside the brackets in context with Amount, e.g. {xyz < amount}#1
        // obviously, if the Calculator moves the footnotes inside, it's too late for Amount-replacement,
        // but if this would be the only problem one could think of changing the order
        // however the Calculator would produce {xyz#1 < amount#1} which is in the case I found wrong - there is no footnote for xyz
        // thus this would lead to the incorrect error message "missing footnote for xyz#1"
        // note that for e.g. {xyz < abc}#1, moving in is justified, because the programme never could know which side is meant
        // summarising: treating this as "special" is anyway necessary and thus it is done with this function
        private string HandleOldStyleAmount(string formula, FunBase fun)
        {
            string amount = DefPar.Value.AMOUNT.ToLower();
            int amountIndex = formula.IndexOf(amount);
            while (amountIndex >= 0)
            {
                if (amountIndex == 0 || calc.IsSymbol(formula[amountIndex - 1])) // make sure not touching abc_amount
                {
                    int footnoteIndex = formula.Substring(amountIndex).IndexOf("}#");
                    if (footnoteIndex == -1) return formula; // let Calculator catch the error
                    footnoteIndex += amountIndex + 1; // i.e. the # after the bracket

                    string number = string.Empty;
                    for (int i = footnoteIndex + 1; i < formula.Length; ++i) // start on character after the #
                    {
                        if ((formula[i] >= '0' && formula[i] <= '9') || (formula.ToLower()[i] >= 'a' && formula.ToLower()[i] <= 'z'))
                            number += formula[i];
                        else break;
                    }

                    ParBase parAmount = fun.GetFootnotePar<ParBase>(DefPar.Footnote.Amount, new List<string>() { number });
                    string replaceBy = "0";
                    if (parAmount != null) replaceBy = parAmount.xmlValue;
                    else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $"{description.Get()}: missing footnote {DefPar.Footnote.Amount}({number})" });

                    // if there is actually a second footnote (except #_Amount) with the same number, do not delete the footnote
                    // allows for e.g. {yem < amount}#1, with #_Amount(1) = 12000#y and #_Level(1) = tu_whatsoever
                    if (fun.GetFootnoteParCount(number) == 1)
                        formula = formula.Remove(footnoteIndex, number.Length + 1);
                    formula = formula.Remove(amountIndex, amount.Length);
                    formula = formula.Insert(amountIndex, replaceBy);
                }
                amountIndex = formula.IndexOf(amount, amountIndex + 1);
            }
            return formula;
        }

        private string BenCalcBase(string formula, FunBase fun)
        {
            if (fun.GetType() != typeof(FunBenCalc)) return formula;
            return (fun as FunBenCalc).HandleBase(formula, description);
        }
    }
}
