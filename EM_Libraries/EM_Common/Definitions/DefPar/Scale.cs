namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Scale
        {
            public const string FactorVariables = "FactorVariables";
            public const string FactorParameter = "FactorParameter";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(FactorVariables, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(FactorParameter, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1
                });
            }
        }
    }
}
