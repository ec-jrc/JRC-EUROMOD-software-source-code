namespace EM_Common
{
    public static partial class DefPar
    {
        public static class SetDefault
        {
            public const string Dataset = "Dataset";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    placeholderType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
