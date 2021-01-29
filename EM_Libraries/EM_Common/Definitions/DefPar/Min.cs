namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Min
        {
            public const string Val = "Val";
            public const string Positive_Only = "Positive_Only";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Val, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(Positive_Only, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}
