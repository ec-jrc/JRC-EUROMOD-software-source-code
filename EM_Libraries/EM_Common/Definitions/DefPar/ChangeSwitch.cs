using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class ChangeSwitch
        {
            public const string PolFun = "PolFun";
            public const string Switch_NewVal = "Switch_NewVal";

            public const string Switch_X = "Switch_X";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = Switch_X, minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(PolFun, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    description = "The name or identifier of the policy respectively the identifier or symbolic-identifier (add-ons) of the function."
                });
                fun.parGroups.Last().par.Add(Switch_NewVal, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    description = "New switch of the policy/function."
                });
            }
        }
    }
}
