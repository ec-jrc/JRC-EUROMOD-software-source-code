namespace EM_Common
{
    public static partial class DefPar
    {
        public static class RandSeed
        {
            public const string Seed = "Seed";
            public const string SeedByHHId = "SeedByHHId";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Seed, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 1
                });
                fun.par.Add(SeedByHHId, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}
