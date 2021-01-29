using System.Collections.Generic;
using System;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Footnote
        {
            public const string LowLim = "#_LowLim";
            public const string UpLim = "#_UpLim";
            public const string LimPriority = "#_LimPriority";
            public const string Level = "#_Level";
            public const string Amount = "#_Amount";

            public static void Add(DefinitionAdmin.Fun fun, bool addAmount = true)
            {
                fun.par.Add(LowLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA, isFootnote = true, isCommon = true,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.NEG_INFINITE,
                    description = "Footnote parameter for the further specification of an operand: replaces operand if operand is smaller."
                });
                fun.par.Add(UpLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA, isFootnote = true, isCommon = true,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.POS_INFINITE,
                    description = "Footnote parameter for the further specification of an operand: replaces operand if operand is higher."
                });
                fun.par.Add(LimPriority, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG, isFootnote = true, isCommon = true,
                    minCount = 0, maxCount = 1, defaultValue = Value.NA,
                    categValues = new List<string>() { Value.LIMPRI_LOWER, Value.LIMPRI_UPPER, Value.NA },
                    description = "1-Footnote parameter for the further specification of an operand:" + Environment.NewLine
                        + "Possible values:" + Environment.NewLine
                        + "If upper limit (#_UpLim) is smaller than lower limit (#_LowLim) ..." + Environment.NewLine
                        + "- upper: ... upper limit dominates;" + Environment.NewLine
                        + "- lower: ... lower limit dominates;" + Environment.NewLine
                        + "- not defined: ... a warning is issued."
                });
                fun.par.Add(Level, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TU, isFootnote = true, isCommon = true,
                    minCount = 0, maxCount = 1,
                    description = "Footnote parameter for the further specification of an operand: indicates an alternative assessment unit for an operand."
                });

                if (addAmount)
                {
                    fun.par.Add(Amount, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.TEXT, // PAR_TYPE.NUMBER would not allow for (legal) usage of constants
                        isFootnote = true, isCommon = true,
                        minCount = 0, maxCount = 1,
                        description = "Footnote parameter for the further specification of an operand: indicates the numeric value of an operand."
                    });
                }
            }
        }
    }
}
