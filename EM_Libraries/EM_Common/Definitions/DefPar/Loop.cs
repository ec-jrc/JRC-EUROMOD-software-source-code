using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Loop
        {
            public const string Loop_Id = "Loop_Id";
            public const string BreakCond = "BreakCond";
            public const string Num_Iterations = "Num_Iterations";
            public const string Last_Pol = "Last_Pol";
            public const string Last_Func = "Last_Func";
            public const string Stop_Before_Pol = "Stop_Before_Pol";
            public const string Stop_Before_Func = "Stop_Before_Func";
            public const string First_Pol = "First_Pol";
            public const string First_Func = "First_Func";
            public const string Start_After_Pol = "Start_After_Pol";
            public const string Start_After_Func = "Start_After_Func";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Loop_Id, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(BreakCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Num_Iterations }
                });
                fun.par.Add(Num_Iterations, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { BreakCond }
                });
                fun.par.Add(Last_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Last_Func, Stop_Before_Pol, Stop_Before_Func }
                });
                fun.par.Add(Last_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Last_Pol, Stop_Before_Pol, Stop_Before_Func }
                });
                fun.par.Add(Stop_Before_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Last_Pol, Last_Func, Stop_Before_Func }
                });
                fun.par.Add(Stop_Before_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Last_Pol, Last_Func, Stop_Before_Pol }
                });
                fun.par.Add(First_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { First_Func, Start_After_Pol, Start_After_Func }
                });
                fun.par.Add(First_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { First_Pol, Start_After_Pol, Start_After_Func }
                });
                fun.par.Add(Start_After_Pol, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { First_Pol, First_Func, Start_After_Func }
                });
                fun.par.Add(Start_After_Func, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { First_Pol, First_Func, Start_After_Pol }
                });
            }
        }
    }
}
