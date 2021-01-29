using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddOn_ExtensionSwitch
        {
            public static string Extension_Switch = "Extension_Switch";
            public static string Extension_Name = "Extension_Name";
            public static string Extension_Id = "Extension_Id";
            public static string Dataset = "Dataset";
            public static string System = "System";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Extension_Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Extension_Id }
                });
                fun.par.Add(Extension_Id, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Extension_Name }
                });
                fun.par.Add(Extension_Switch, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.par.Add(System, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
