using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Totals
        {
            public const string Varname_Sum = "Varname_Sum";
            public const string Varname_Mean = "Varname_Mean";
            public const string Varname_Median = "Varname_Median";
            public const string Varname_Decile = "Varname_Decile";
            public const string Varname_Quintile = "Varname_Quintile";
            public const string Varname_Count = "Varname_Count";
            public const string Varname_PosCount = "Varname_PosCount";
            public const string Varname_NegCount = "Varname_NegCount";
            public const string Varname_Min = "Varname_Min";
            public const string Varname_Max = "Varname_Max";
            public const string Agg = "Agg"; // note: Transformer merges agg_il and agg_var
            public const string Incl_Cond = "Incl_Cond";
            public const string Incl_Cond_Who = "Incl_Cond_Who";
            public const string Use_Weights = "Use_Weights";
            public const string Weight_Var = "Weight_Var";
            public const string WarnIfDuplicateDefinition = "WarnIfDuplicateDefinition";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Varname_Sum, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Mean, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Median, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Decile, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Quintile, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Count, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_PosCount, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_NegCount, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Min, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Varname_Max, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Agg, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VARorIL,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(Incl_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Incl_Cond_Who, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1,
                    categValues = GetWhoMustBeEligValues()
                });
                fun.par.Add(Use_Weights, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, defaultValue = true,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Weight_Var, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR, defaultValue = DefVarName.DWT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(WarnIfDuplicateDefinition, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = true,
                    description = $"Warn if function produces duplicate definitions."
                });
            }
        }
    }
}
