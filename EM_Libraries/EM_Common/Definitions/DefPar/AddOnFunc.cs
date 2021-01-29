using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddOn_Func
        {
            public static string Id_Func = "Id_Func";
            public static string Insert_Before_Func = "Insert_Before_Func";
            public static string Insert_After_Func = "Insert_After_Func";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Id_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(Insert_After_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Insert_Before_Func }
                });
                fun.par.Add(Insert_Before_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Insert_After_Func }
                });
            }
        }
    }
}
