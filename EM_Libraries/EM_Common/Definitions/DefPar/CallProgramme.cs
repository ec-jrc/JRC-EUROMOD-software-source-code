namespace EM_Common
{
    public static partial class DefPar
    {
        public static class CallProgramme
        {
            public const string Programme = "Programme";
            public const string Path = "Path";
            public const string Argument = "Argument";
            public const string UnifySlash = "UnifySlash";
            public const string Wait = "Wait";
            public const string RepByEMPath = "RepByEMPath";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Programme, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Path, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Argument, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(UnifySlash, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.par.Add(Wait, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
                fun.par.Add(RepByEMPath, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
            }
        }
    }
}
