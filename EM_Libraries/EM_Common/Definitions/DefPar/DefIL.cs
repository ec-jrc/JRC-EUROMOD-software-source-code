using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefIl
        {
            public const string Name = "Name";
            public const string Warn_If_NonMonetary = "Warn_If_NonMonetary";
            public const string RegExp_Def = "RegExp_Def";
            public const string RegExp_Factor = "RegExp_Factor";

            public const string GROUP_REGEXP = "REGEXP_X";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Warn_If_NonMonetary, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                // see FunDefIL for description on how this is checked
                fun.par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, // actually + / - / +0.5 / etc., i.e. requires specific check by FunDefIL
                    placeholderType = PAR_TYPE.VARorIL,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = GROUP_REGEXP,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(RegExp_Def, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.parGroups.Last().par.Add(RegExp_Factor, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
            }
        }
    }
}
