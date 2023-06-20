namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefInput
        {
            public const string path = "path";
            public const string file = "file";
            public const string RowMergeVar = "RowMergeVar";
            public const string ColMergeVar = "ColMergeVar";
            public const string InputVar = "InputVar";
            public const string DefaultIfNoMatch = "DefaultIfNoMatch";
            public const string IgnoreNRows = "IgnoreNRows";
            public const string IgnoreNCols = "IgnoreNCols";
            public const string DoRanges = "DoRanges";
            public const string RepByEMPath = "RepByEMPath";
            public const string AutoRenameWhenCopying = "AutoRenameWhenCopying";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(file, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(path, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(RowMergeVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(ColMergeVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(InputVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(DefaultIfNoMatch, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = double.NaN
                });
                fun.par.Add(IgnoreNRows, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 0.0
                });
                fun.par.Add(IgnoreNCols, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 0.0
                });
                fun.par.Add(DoRanges, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
                fun.par.Add(RepByEMPath, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(AutoRenameWhenCopying, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0,
                    maxCount = 1,
                    defaultValue = true,
                    description = "If set to false, the filename will not be automatically renamed when copying systems."
                });
            }
        }
    }
}
