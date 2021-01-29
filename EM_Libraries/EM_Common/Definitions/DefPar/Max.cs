namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Max
        {
            public const string Val = "Val";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Val, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
