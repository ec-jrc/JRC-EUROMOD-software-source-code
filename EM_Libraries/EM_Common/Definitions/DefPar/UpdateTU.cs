using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class UpdateTu
        {
            public const string Name = "Name";
            public const string Update_All = "Update_All";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Update_All }
                });
                fun.par.Add(Update_All, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false,
                    substitutes = new List<string>() { Name }
                });
            }
        }
    }
}
