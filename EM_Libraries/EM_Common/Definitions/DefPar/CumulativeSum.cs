using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class CumulativeSum
        {
            public const string SortingVar = "SortingVar";
            public const string SortingAsc = "SortingAsc";
            public const string SummingVar = "SummingVar";
            public const string SummingWeighted = "SummingWeighted";
            public const string SummingAbsolute = "SummingAbsolute";
            public const string OutputVar = "OutputVar";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                // Sorting parameters
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = SortingVar, minCount = 1, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(SortingVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VARorIL, minCount = 1, maxCount = 1,
                    description = "Variable or Incomelist based on which to sort observations."
                });
                fun.parGroups.Last().par.Add(SortingAsc, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = true,
                    description = "Whether sorting should be ascending or not."
                });
                // Summing parameters
                fun.par.Add(SummingVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VARorIL, minCount = 1, maxCount = 1,
                    description = "Variable or Incomelist to sum."
                });
                fun.par.Add(SummingWeighted, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = false,
                    description = $"Whether the summing should be weighted or not."
                });
                fun.par.Add(SummingAbsolute, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN, minCount = 0, maxCount = 1, defaultValue = true,
                    description = $"Whether the summing should be absolute or relative."
                });
                // Output parameter
                fun.par.Add(OutputVar, new DefinitionAdmin.Par()
                { 
                    valueType = PAR_TYPE.VAR, minCount = 1, maxCount = 1,
                    description = "The output variable that will hold the cumulative sum."
                });
            }
        }
    }
}
