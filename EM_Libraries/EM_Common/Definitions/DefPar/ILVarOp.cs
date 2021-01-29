using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class IlVarOp
        {
            public const string Operand = "Operand";
            public const string Operand_Factors = "Operand_Factors";
            public const string Operand_IL = "Operand_IL";
            public const string Operator_IL = "Operator_IL";
            public const string Operation = "Operation";
            public const string Sel_Var = "Sel_Var";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Operator_IL, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL,
                    minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Operand }
                });
                fun.par.Add(Operand, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Operand_IL }
                });
                fun.par.Add(Operand_Factors, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
                fun.par.Add(Operand_IL, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Operation, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1, defaultValue = Value.ILVAROP_MUL,
                    categValues = new List<string>() { Value.ILVAROP_MUL, Value.ILVAROP_ADD, Value.ILVAROP_NEGTOZERO }
                });
                fun.par.Add(Sel_Var, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1, defaultValue = Value.ILVAROP_ALL,
                    categValues = new List<string>() { Value.ILVAROP_ALL, Value.ILVAROP_MIN, Value.ILVAROP_MAX, Value.ILVAROP_MINPOS }
                });
            }
        }
    }
}
