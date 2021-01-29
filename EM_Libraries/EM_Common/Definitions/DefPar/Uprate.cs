using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Uprate
        {
            public const string Dataset = "Dataset";
            public const string Def_Factor = "Def_Factor";
            public const string Factor_Condition = "Factor_Condition";
            public const string AggVar_Name = "AggVar_Name";
            public const string AggVar_Part = "AggVar_Part";
            public const string AggVar_Tolerance = "AggVar_Tolerance";
            public const string WarnIfNoFactor = "WarnIfNoFactor";
            public const string WarnIfNonMonetary = "WarnIfNonMonetary";
            public const string Factor_Name = "Factor_Name";
            public const string Factor_Value = "Factor_Value";
            public const string RegExp_Def = "RegExp_Def";
            public const string RegExp_Factor = "RegExp_Factor";
            public const string DBYearVar = "DBYearVar";


            // try to construct a group-names which are telling in error-messages
            public const string GROUP_MAIN = "Var-Name/" + Factor_Condition;
            public const string GROUP_FACTOR_DEF = Factor_Name + "/" + Factor_Value;
            public const string GROUP_AGG = "AGGVAR_X";
            public const string GROUP_REGEXP = "REGEXP_X";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(DBYearVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Def_Factor, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1, defaultValue = 1.0
                });
                fun.par.Add(WarnIfNoFactor, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.par.Add(WarnIfNonMonetary, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = GROUP_MAIN,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Factor_Condition, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.parGroups.Last().par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, // factor can be name of factor or a number
                    placeholderType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = 1
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = GROUP_FACTOR_DEF,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Factor_Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.parGroups.Last().par.Add(Factor_Value, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 1, maxCount = 1
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = GROUP_AGG,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(AggVar_Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 1, maxCount = 1
                });
                fun.parGroups.Last().par.Add(AggVar_Part, new DefinitionAdmin.Par() {
                    valueType = PAR_TYPE.VAR,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(AggVar_Tolerance, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 0.0
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
                    minCount = 1, maxCount = 1,
                });
            }
        }
    }
}
