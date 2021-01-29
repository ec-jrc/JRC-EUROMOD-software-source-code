using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class AddHHMembers
        {
            public const string Add_Who = "Add_Who";
            public const string ParentCond = "ParentCond";
            public const string PartnerCond = "PartnerCond";
            public const string HHCond = "HHCond";
            public const string IsPartnerParent = "IsPartnerParent";
            public const string FlagVar = "FlagVar";

            public const string GROUP_MAIN = Add_Who + "...";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(FlagVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = 1,
                    description = "If indicated, variable is set to 1 for persons added by this AddHHMembes function.\nNote that the variable must exist.\nAlso note that the variable is not changed for any other persons (thus it can e.g.be used to hold the flag for more than one AddHHMembers functions)."
                });
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = GROUP_MAIN,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Add_Who, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 1, maxCount = 1,
                    categValues = new List<string>() { Value.ADDHHMEMBERS_CHILD, Value.ADDHHMEMBERS_PARTNER, Value.ADDHHMEMBERS_OTHER }
                });
                fun.parGroups.Last().par.Add(ParentCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { PartnerCond, HHCond }
                });
                fun.parGroups.Last().par.Add(PartnerCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { ParentCond, HHCond }
                });
                fun.parGroups.Last().par.Add(HHCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { PartnerCond, ParentCond }
                });
                fun.parGroups.Last().par.Add(IsPartnerParent, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.parGroups.Last().par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    placeholderType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
            }
        }
    }
}
