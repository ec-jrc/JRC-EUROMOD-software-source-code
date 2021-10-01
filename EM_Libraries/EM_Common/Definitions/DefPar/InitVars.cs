namespace EM_Common
{
    public static partial class DefPar
    {
        public static class InitVars
        {
            public const string InitOnce = "InitOnce";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(PAR_TYPE.PLACEHOLDER.ToString(), new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    placeholderType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "[Placeholder] stands for the name of a variable, defined in the policy column, whose initial value is specified in the respective system column."
                });
                fun.par.Add(InitOnce, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1,
                    defaultValue = false,
                    description = "Only relevant if used in loops or in reference policies. If set to 'yes', initialisation only takes place once, e.g. in the first iteration of a loop. Default is 'no', meaning that initialisation takes place whenever the run hits the function (e.g. in each iteration of a loop)."
                });
            }
        }
    }
}
