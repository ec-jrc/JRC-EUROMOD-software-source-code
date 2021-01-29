using System;
using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class ChangeParam
        {
            public const string Dataset = "Dataset";
            public const string Param_Id = "Param_Id";
            public const string Param_NewVal = "Param_NewVal";
            public const string Param_CondVal = "Param_CondVal"; // just necessary for transformer, does not exist for EM3

            public const string Param_X = "Param_X";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = Param_X, minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Param_Id, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.parGroups.Last().par.Add(Param_NewVal, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1
                });
                fun.par.Add(Dataset, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "If any 'Dataset' parameter is used (several can be used within one group), the change only takes place if one of them matches the dataset of the concerned run." + Environment.NewLine +
                                  "The wildcards * and ? can be used, where * stands for any character and ? stands for one arbitrary character (e.g.be_20 * _a ?)."
                });
            }
        }
    }
}
