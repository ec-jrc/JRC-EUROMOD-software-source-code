namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddOn_Applic
        {
            public const string Sys = "Sys";
            public const string SysNA = "SysNA";
            public const string Description = "Description";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Description, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(Sys, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(SysNA, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
