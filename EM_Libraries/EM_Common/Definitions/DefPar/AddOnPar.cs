namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddOn_Par
        {
            public static string Insert_Func = "Insert_Func";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Insert_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, placeholderType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = DefinitionAdmin.MANY,
                });
            }
        }
    }
}
