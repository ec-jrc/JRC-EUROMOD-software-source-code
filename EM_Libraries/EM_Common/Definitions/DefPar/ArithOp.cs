using System;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class ArithOp
        {
            public const string Formula = "Formula";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Formula, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 1, maxCount = 1,
                    description = "Formula for calculating the function's result." + Environment.NewLine + Environment.NewLine 
                        + "Allowed operations:" + Environment.NewLine
                        + " - addition: operator +" + Environment.NewLine
                        + " - subtraction: operator -" + Environment.NewLine
                        + "- multiplication: operator *" + Environment.NewLine
                        + "- division: operator /" + Environment.NewLine
                        + "- raising to a power: operator ^, e.g. 2 ^ 3 (result: 8)" + Environment.NewLine
                        + "- percentage: operator %, e.g. yem*3% (result: yem*(3/100))" + Environment.NewLine
                        + "- reminder of division:  operator \\, e.g. 22\\5 (result: 2)" + Environment.NewLine
                        + "- minimum and maximum: operators <min> and <max>, e.g. 10<min>15 (result: 10)" + Environment.NewLine
                        + "- absolute value: operator <abs>(), e.g. <abs>(50-70) (result: 20)" + Environment.NewLine
                        + "- negation: operator !(), e.g. !(IsMarried), !(17) (result: 0), !(0) (result: 1)" + Environment.NewLine + Environment.NewLine
                        + "Allowed operands:" + Environment.NewLine
                        + "- numeric values, e.g. 10, 0.3, -25" + Environment.NewLine
                        + "- numeric values with a period, e.g. 12000#m, 1000#y" + Environment.NewLine
                        + "- amount#i as place holders for numeric values specified by footnote parameters" + Environment.NewLine
                        + "- variables, e.g. yem" + Environment.NewLine
                        + "- incomelists, e.g. ils_dispy" + Environment.NewLine
                        + "- queries, e.g. IsUnemployed" + Environment.NewLine
                        + "- random numbers: rand, e.g. rand * 100 (result: random number between 0 and 100, see function RandSeed for more information)" + Environment.NewLine + Environment.NewLine
                        + "Order of operation rules:" + Environment.NewLine
                        + "- ^, <min>, <max>, <abs>(), !(), %" + Environment.NewLine
                        + "- before multiplicative operations */ \\" + Environment.NewLine
                        + "- before additive operations +-" + Environment.NewLine + Environment.NewLine
                        + "Parentheses can be used to group operations, e.g. (2+3)*4."
                });
            }
        }
    }
}
