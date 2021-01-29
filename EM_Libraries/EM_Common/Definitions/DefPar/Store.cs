using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Store
        {
            public const string PostFix = "PostFix";
            public const string PostLoop = "PostLoop";
            public const string Var = "Var";
            public const string IL = "IL";
            //public const string LoopFromTo = "LoopFromTo"; // *** new *** see description in StoreVar.PrepareNonCommonPar, currently deactivated
            public const string Level = "Level";
            public const string Var_Level = "Var_Level";
            public const string IL_Level = "IL_Level";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(PostFix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { PostLoop }
                });
                fun.par.Add(PostLoop, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { PostFix }
                });
                fun.par.Add(Var, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(IL, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                //fun.par.Add(LoopFromTo, new DefinitionAdmin.Par()
                //{
                //    valueType = PAR_TYPE.TEXT,
                //    minCount = 0, maxCount = DefinitionAdmin.MANY
                //});
                fun.par.Add(Level, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TU,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Var_Level, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TU,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(IL_Level, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TU,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
