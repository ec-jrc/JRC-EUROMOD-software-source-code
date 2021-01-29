using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefVar
        {
            public const string Var_SystemYear = "Var_SystemYear";
            public const string Var_Dataset = "Var_Dataset";
            public const string Var_Monetary = "Var_Monetary";

            public const string THE_ONLY_GROUP = "Variable-Name/" + Var_Monetary; // try to construct a group-name that's telling in error-messages

            public const string EM2_Var_Name = "Var_Name"; // very old EM2-notation

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Var_SystemYear, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Var_Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = THE_ONLY_GROUP,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Var_Monetary, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.parGroups.Last().par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    placeholderType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1, defaultValue = "0"
                });
            }
        }
    }
}
