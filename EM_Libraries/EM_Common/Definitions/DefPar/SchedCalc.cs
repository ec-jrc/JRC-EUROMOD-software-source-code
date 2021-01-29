using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class SchedCalc
        {
            public const string Base = "Base";
            public const string Band_UpLim = "Band_UpLim";
            public const string Band_LowLim = "Band_LowLim";
            public const string Band_Rate = "Band_Rate";
            public const string Band_Amount = "Band_Amount";
            public const string Do_Average_Rates = "Do_Average_Rates";
            public const string Quotient = "Quotient";
            public const string BaseThreshold = "BaseThreshold";
            public const string Round_Base = "Round_Base";
            public const string Simple_Prog = "Simple_Prog";

            public const string Band_XXX = "Band_XXX"; // that's a more or less arbitrary name (note however, that it is used in error messages)

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Base, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(Do_Average_Rates, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Quotient, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(BaseThreshold, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Round_Base, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(Simple_Prog, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });


                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = Band_XXX,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Band_Rate, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Band_Amount }
                });
                fun.parGroups.Last().par.Add(Band_Amount, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Band_Rate }
                });
                fun.parGroups.Last().par.Add(Band_LowLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Band_UpLim }
                });
                fun.parGroups.Last().par.Add(Band_UpLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Band_LowLim }
                });
            }
        }
    }
}
