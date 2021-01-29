using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Restore
        {
            public const string PostFix = "PostFix";
            public const string PostLoop = "PostLoop";
            public const string Iteration = "Iteration";

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
                fun.par.Add(Iteration, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1
                });
            }
        }
    }
}
