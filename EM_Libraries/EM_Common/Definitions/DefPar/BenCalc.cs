using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class BenCalc
        {
            public const string Base = "Base";
            public const string Comp_perTU = "Comp_perTU";
            public const string Comp_perElig = "Comp_perElig";
            public const string Comp_Cond = "Comp_Cond";
            public const string Comp_LowLim = "Comp_LowLim";
            public const string Comp_UpLim = "Comp_UpLim";
            public const string Withdraw_Base = "Withdraw_Base";
            public const string Withdraw_Rate = "Withdraw_Rate";
            public const string Withdraw_Start = "Withdraw_Start";
            public const string Withdraw_End = "Withdraw_End";

            public const string Comp_X = "Comp_X"; // that's a more or less arbitrary name (note however, that it is used in error messages)

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Base, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    description = "Base amount that can be used with parameters compX_perTU / compX_perElig, referenced as $Base."
                });
                fun.par.Add(Withdraw_Base, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, 
                    description = "Withdraw_Base * Withdraw_Rate is deducted from function's (preliminary) result.\nNot that if withdraw parameters are used, the function's result cannot be negative."
                });
                fun.par.Add(Withdraw_Rate, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, defaultValue = 0.0,
                    substitutes = new List<string>() { Withdraw_End },
                    description = "Withdraw_Base * Withdraw_Rate is deducted from function's (preliminary) result.\nNot that if withdraw parameters are used, the function's result cannot be negative."
                });
                fun.par.Add(Withdraw_Start, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, defaultValue = 0.0,
                    description = "Level of Withdraw_Base where withdrawal starts."
                });
                fun.par.Add(Withdraw_End, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.POS_INFINITE,
                    substitutes = new List<string>() { Withdraw_Rate },
                    description = "Level of Withdraw_Base where withdrawal ends (i.e. benefit is totally withdrawn).\nNote that the parameter is ignored if Withdraw_Rate is indicated."
                });

                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = Comp_X,
                    minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(Comp_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 1, maxCount = 1,
                    description = "Condition that must be fulfilled to add the component (comp_perTU / comp_perElig) to the function's result.\nSyntax rules as for parameter Elig_Cond of function Elig apply."
                });
                fun.parGroups.Last().par.Add(Comp_perTU, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 1, maxCount = 1,
                    substitutes = new List<string>() { Comp_perElig },
                    description = "Formula to calculate one component of the function's result. The result of the formula is added once to the function's result, regardless whether one or more members of the assessment unit fulfill the components condition (Comp_Cond).\nSyntax rules as for parameter Formula of function ArithOp apply."
                });
                fun.parGroups.Last().par.Add(Comp_perElig, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Comp_perTU },
                    description = "Formula to calculate one component of the function's result. The result of the formula is added to the function's result once for each member of the assessment unit fulfiling the components condition (Comp_Cond).\nSyntax rules as for parameter Formula of function ArithOp apply."
                });
                fun.parGroups.Last().par.Add(Comp_LowLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.NEG_INFINITE,
                    description = "Replaces component if component is smaller."
                });
                fun.parGroups.Last().par.Add(Comp_UpLim, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.POS_INFINITE,
                    description = "Replaces component if component is higher."
                });
            }
        }
    }
}
