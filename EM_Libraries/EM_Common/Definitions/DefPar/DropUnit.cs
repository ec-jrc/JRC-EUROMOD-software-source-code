using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DropUnit
        {
            public const string Drop_Cond = "Drop_Cond";
            public const string Drop_Cond_Who = "Drop_Cond_Who";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Drop_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Drop_Cond_Who, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1, defaultValue = Value.WHO_ONE,
                    categValues = GetWhoMustBeEligValues()
                });
            }
        }
    }
}
