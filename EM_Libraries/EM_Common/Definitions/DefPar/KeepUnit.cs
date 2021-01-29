using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class KeepUnit
        {
            public const string Keep_Cond = "Keep_Cond";
            public const string Keep_Cond_Who = "Keep_Cond_Who";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Keep_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Keep_Cond_Who, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1, defaultValue = Value.WHO_ONE,
                    categValues = GetWhoMustBeEligValues()
                });
            }
        }
    }
}
