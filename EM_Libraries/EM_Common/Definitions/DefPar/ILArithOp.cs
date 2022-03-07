using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class IlArithOp
        {
            public const string Base_ILName = "Base_ILName";
            public const string Base_Prefix = "Base_Prefix";
            public const string Base_Postfix = "Base_Postfix";
            public const string ILName = "ILName";
            public const string Prefix = "Prefix";
            public const string Postfix = "Postfix";
            public const string Formula = "Formula";
            public const string Out_ILName = "Out_ILName";
            public const string Out_Prefix = "Out_Prefix";
            public const string Out_Postfix = "Out_Postfix";
            public const string WarnIfDuplicateDefinition = "WarnIfDuplicateDefinition";
            public const string ForceMonetaryOutput = "ForceMonetaryOutput";

            public const string BASE_IL_COMPONENT = "BASE_IL_COMPONENT";
            public const string IL_COMPONENT = "IL_COMPONENT";
            public const string OUT_IL_COMPONENT = "OUT_IL_COMPONENT";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Base_ILName, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL, minCount = 1, maxCount = 1,
                    description = "Incomelist specifying the variables on which the arithmetic operation takes place."
                });
                fun.par.Add(Base_Prefix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in (decomposed) incomelist '{Base_ILName}' must start with this prefix."
                });
                fun.par.Add(Base_Postfix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in (decomposed) incomelist '{Base_ILName}' must end with this postfix."
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = ILName, minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(ILName, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL, minCount = 1, maxCount = 1,
                    description = $"Additional incomelist containing variables used in the arithmetic operation (referred to by IL_COMPONENT). " +
                                  $"Decomposed content must at least contain all variables contained in '{Base_ILName}' " +
                                  $"only differing by '{Base_Prefix}'/'{Prefix}' and '{Base_Postfix}'/'{Postfix}'."
                });
                fun.parGroups.Last().par.Add(Prefix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in (decomposed) incomelist '{ILName}' with same group must start with this prefix."
                });
                fun.parGroups.Last().par.Add(Postfix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in (decomposed) incomelist '{ILName}' with same group must end with this postfix."
                });

                fun.par.Add(Formula, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA, minCount = 1, maxCount = 1,
                    description = $"Arithmetic operation to be performed, allowing for placeholders '{BASE_IL_COMPONENT}', '{IL_COMPONENT}['{ILName}']' and '{OUT_IL_COMPONENT}'."
                });


                fun.par.Add(Out_ILName, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 1, maxCount = 1,
                    description = "Name of the incomelist to contain the output variables. Will be created if it does not exist."
                });
                fun.par.Add(Out_Prefix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in incomelist '{Out_ILName}' must start with this prefix, if the incomelist exists; if not they will be created with this prefix."
                });
                fun.par.Add(Out_Postfix, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT, minCount = 0, maxCount = 1,
                    description = $"All variables in incomelist '{Out_ILName}' must end with this postfix, if the incomelist exists; if not they will be created with this postfix."
                });
                fun.par.Add(WarnIfDuplicateDefinition, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = true,
                    description = $"Warn if function produces duplicate definitions for income lists."
                });
                fun.par.Add(ForceMonetaryOutput, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = false,
                    description = $"Force the resulting new variables to be monetary, even if the base variables were non-monetary."
                });
            }
        }
    }
}
