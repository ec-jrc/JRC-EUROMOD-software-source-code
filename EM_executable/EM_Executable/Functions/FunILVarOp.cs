using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunILVarOp  : FunInSpineBase
    {
        internal FunILVarOp(InfoStore infoStore) : base(infoStore) { }

        private class OpItem
        {
            internal VarSpec varOperator = null;
            internal VarSpec varOperand = null;
            internal double numOperator = 0;
            internal double numOperand = 0;
        }

        private enum OPERAND_TYPE { SCALAR, FORMULA, IL, FACTORS, NEGTOZERO, INVALID };

        private List<OpItem> opItems = new List<OpItem>();
        private ParFormula formulaOperand = null;
        private string mulAdd = DefPar.Value.ILVAROP_MUL;
        private string selVar = DefPar.Value.ILVAROP_ALL;
        private OPERAND_TYPE opTask = OPERAND_TYPE.INVALID;

        object ilVarOpLock = new object();

        protected override void PrepareNonCommonPar()
        {
            // get parameters
            ParIL parOperatorIL = GetUniquePar<ParIL>(DefPar.IlVarOp.Operator_IL); if (parOperatorIL == null) return; // error is handled by global check
            ParFormula parOperand = GetUniquePar<ParFormula>(DefPar.IlVarOp.Operand);
            bool useFactors = GetParBoolValueOrDefault(DefFun.IlVarOp, DefPar.IlVarOp.Operand_Factors);
            ParIL parOperandIL = GetUniquePar<ParIL>(DefPar.IlVarOp.Operand_IL);
            ParCateg parMulAdd = GetUniquePar<ParCateg>(DefPar.IlVarOp.Operation); if (parMulAdd != null) mulAdd = parMulAdd.GetCateg();
            ParCateg parSelVar = GetUniquePar<ParCateg>(DefPar.IlVarOp.Sel_Var); if (parSelVar != null) selVar = parSelVar.GetCateg();

            // assess what to do and check for insufficient or contradictory information
            if (parOperand != null)
            {
                //if (parOperand.IsGlobal()) opTask = OPERAND_TYPE.SCALAR; // out-commented to replace by the weaker condtion below, otherwise it produces an error with constants
                if (double.TryParse(parOperand.xmlValue, out double x)) opTask = OPERAND_TYPE.SCALAR;
                else { opTask = OPERAND_TYPE.FORMULA; formulaOperand = parOperand; }
            }
            else if (parOperandIL != null) { if (!CheckDoubleDef(OPERAND_TYPE.IL)) return; }
            if (useFactors) { if (!CheckDoubleDef(OPERAND_TYPE.FACTORS)) return; }
            if (mulAdd == DefPar.Value.ILVAROP_NEGTOZERO) { if (!CheckDoubleDef(OPERAND_TYPE.NEGTOZERO)) return; }
            if (opTask == OPERAND_TYPE.INVALID)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: missing definition of operand" });
                return;
            }
            if (opTask == OPERAND_TYPE.IL && parOperandIL.GetFlatContent().Count != parOperatorIL.GetFlatContent().Count)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: incomelists {parOperatorIL.name} and {parOperandIL.name} do not have the same number of variables, operation is not possible" });
                return;
            }

            // prepare information for run
            for (int i = 0; i < parOperatorIL.GetFlatContent().Count; ++i)
            {
                OpItem oi = new OpItem() { varOperator = parOperatorIL.GetFlatContent()[i].varSpec };
                switch (opTask)
                {
                    case OPERAND_TYPE.FACTORS: oi.numOperand = parOperatorIL.GetFlatContent()[i].addFactor; break;
                    case OPERAND_TYPE.SCALAR: oi.numOperand = parOperand.GetGlobalValue(); break;
                    case OPERAND_TYPE.IL: oi.varOperand = parOperandIL.GetFlatContent()[i].varSpec; break;
                }
                opItems.Add(oi);
            }
            

            bool CheckDoubleDef(OPERAND_TYPE ot)
            {
                if (opTask == OPERAND_TYPE.INVALID) { opTask = ot; return true; }
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: contradictory definition of operand" });
                return false;
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (OpItem oi in opItems)
            {
                oi.varOperator.index = infoStore.operandAdmin.GetIndexInPersonVarList(oi.varOperator.name);
                if (oi.varOperand != null) oi.varOperand.index = infoStore.operandAdmin.GetIndexInPersonVarList(oi.varOperand.name);
            }
        }

        // note that ILVarOp actually does not have a TAX_UNIT, thus this is the HHAdmin.DUMMY_INDIVIDUAL_TU (see FunInSpineBase.GetTUName)
        // that means, amongst others, that the function is called for each person in the household (i.e. tu always contains one person)
        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            lock (ilVarOpLock)
            {
                GetValues(hh, tu, out OpItem selItem, out bool noAction);
                if (!noAction)
                {
                    if (selItem != null) DoOp(hh, tu, selItem);
                    else foreach (OpItem oi in opItems) DoOp(hh, tu, oi);
                }
            }
            return 0; // return whatsoever
        }

        void DoOp(HH hh, List<Person> tu, OpItem oi)
        {
            double newVal = 0;
            if (opTask == OPERAND_TYPE.NEGTOZERO) newVal = Math.Max(oi.numOperator, 0);
            else if (mulAdd == DefPar.Value.ILVAROP_MUL) newVal = oi.numOperator * oi.numOperand;
            else newVal = oi.numOperator + oi.numOperand;
            hh.SetPersonValue(newVal, oi.varOperator.index, tu[0].indexInHH);
        }

        void GetValues(HH hh, List<Person> tu, out OpItem selItem, out bool noAction)
        {
            double formulaOperandValue = 0; selItem = null; noAction = false;
            if (opTask == OPERAND_TYPE.FORMULA) formulaOperandValue = formulaOperand.GetValue(hh, tu);

            double max = double.MinValue, min = double.MaxValue;
            foreach (OpItem oi in opItems)
            {
                oi.numOperator = hh.GetPersonValue(oi.varOperator.index, tu[0].indexInHH);
                if (opTask == OPERAND_TYPE.FORMULA) oi.numOperand = formulaOperandValue;
                if (opTask == OPERAND_TYPE.IL) oi.numOperand = hh.GetPersonValue(oi.varOperand.index, tu[0].indexInHH);

                // remark: the behaviour of the old executable cannot be completely reproduced if there are equal mins/maxs
                // the old executable does the operation only on one item, but which depends on the order of content ...
                switch (selVar)
                {
                    case DefPar.Value.ILVAROP_ALL: continue;
                    case DefPar.Value.ILVAROP_MAX: if (oi.numOperator > max) { max = oi.numOperator; selItem = oi; }; break;
                    case DefPar.Value.ILVAROP_MIN: if (oi.numOperator < min) { min = oi.numOperator; selItem = oi; }; break;
                    case DefPar.Value.ILVAROP_MINPOS: if (oi.numOperator <= min && oi.numOperator > 0) { min = oi.numOperator; selItem = oi; }; break;
                }
            }
            if (selVar == DefPar.Value.ILVAROP_MINPOS && selItem == null) noAction = true;
        }

        protected override void SetOutVars(double val, HH hh, List<Person> tu)
        {
            // do nothing, output was already set in DoCalculations
        }
    }
}
