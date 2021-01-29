using System;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Elig
        {
            public const string Elig_Cond = "Elig_Cond";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Elig_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    maxCount = 1, minCount = 1,
                    description = "A combination of conditions: output variable is set to 1 if they are fulfilled, else to 0." + Environment.NewLine + Environment.NewLine
                        + "The comibation consists of:" + Environment.NewLine
                        + "- conditions which must be fulfilled, e.g. {dag<19}," + Environment.NewLine
                        + "- conditions which must not be fulfilled, e.g. !{IsMarried}," + Environment.NewLine
                        + "- combined by the logical operators & (and) and | (or), e.g. {dag<19} & !{IsMarried}," + Environment.NewLine
                        + "- (possibly) grouped by parentheses, e.g. {dag<15} | ({dag<19} & !{IsMarried})." + Environment.NewLine + Environment.NewLine
                        + "A single condition has" + Environment.NewLine
                        + "- either 1 component, usually a yes-no-query, e.g. {IsUnemployed}" + Environment.NewLine
                        + "- or 2 components separated by a comparison operator >,<,>=,<=,=,!=, e.g. {poa=0}." + Environment.NewLine + Environment.NewLine
                        + "The operands left and right from the comparison operator are:" + Environment.NewLine
                        + "- numeric values, e.g. 10, 0.3, (-25), note that negative values must be bracketed" + Environment.NewLine
                        + "- numeric values with a period, e.g. 12000#m, 1000#y" + Environment.NewLine
                        + "- amount#i as place holders for numeric values specified by footnote parameters" + Environment.NewLine
                        + "- variables, e.g. yem" + Environment.NewLine
                        + "- incomelists, e.g. ils_dispy" + Environment.NewLine
                        + "- queries, e.g. IsUnemployed"
                });
            }
        }
    }
}
