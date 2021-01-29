namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Allocate
        {
            public const string Share = "Share";
            public const string Share_Between = "Share_Between";
            public const string Share_All_IfNoElig = "Share_All_IfNoElig";
            public const string Share_Prop = "Share_Prop";
            public const string Share_Equ_IfZero = "Share_Equ_IfZero";
            public const string Ignore_Neg_Prop = "Ignore_Neg_Prop";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Share, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Share_Between, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Share_All_IfNoElig, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.par.Add(Share_Prop, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VARorIL,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Share_Equ_IfZero, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
                fun.par.Add(Ignore_Neg_Prop, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}
