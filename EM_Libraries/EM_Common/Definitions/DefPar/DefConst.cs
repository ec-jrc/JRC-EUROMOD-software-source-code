using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefConst
        {
            public const string Const_SystemYear = "Const_SystemYear";
            public const string Const_Dataset = "Const_Dataset";
            public const string Condition = "Condition";

            public const string THE_ONLY_GROUP = "Constant-Name/" + Condition; // try to construct a group-name that's telling in error-messages

            public const string EM2_Const_Name = "Const_Name"; // very old EM2-notation

            public const string Const_Monetary = "Const_Monetary"; // this parameter is not allowed, but exists in countries (e.g. el, ie)
                                                                   // it is added below but suppressed in the add-param-dialog

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Const_SystemYear, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Const_Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = THE_ONLY_GROUP,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Condition, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.parGroups.Last().par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    placeholderType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = DefinitionAdmin.MANY, defaultValue = "0"
                });
                fun.parGroups.Last().par.Add(Const_Monetary, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}
