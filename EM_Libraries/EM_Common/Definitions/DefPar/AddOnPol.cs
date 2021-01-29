using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddOn_Pol
        {
            public const string Pol_Name = "Pol_Name";
            public const string Insert_Before_Pol = "Insert_Before_Pol";
            public const string Insert_After_Pol = "Insert_After_Pol";
            public const string Allow_Duplicates = "Allow_Duplicates"; // EM3-executable always allows duplicates (i.e. this is just for EM2)

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Pol_Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(Insert_After_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Insert_Before_Pol }
                });
                fun.par.Add(Insert_Before_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Insert_After_Pol }
                });
                fun.par.Add(Allow_Duplicates, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}
