namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Break
        {
            public const string ProduceOutput = "ProduceOutput";
            public const string OutputFileName = "OutputFileName";
            public const string ProduceTUinfo = "ProduceTUinfo";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(ProduceOutput, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true
                });
                fun.par.Add(OutputFileName, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(ProduceTUinfo, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false
                });
            }
        }
    }
}

/*
Extract from ETM-Notes:
Add a “break” function in EUROMOD.
Decision: We should add a new function that allows the user to “break” the spine run at any point inside the spine.
This function should have the following parameters:
i.	 ProduceOutput: a true/false parameter, specifying if EUROMOD should dump all the memory variables in an output file.
ii.	 OutputFileName: a parameter to specify what/where the  output file should be.
iii. ProduceTUinfo: a true/false parameter, specifying if EUROMOD should also dump all the TU info.
Note: the default for OutputFileName should be the first "normal" outputfile
      i.e. usually cc_yyyy_std.txt in the respective output-folder (just look for the first switched on DefOuput that was dropped)
*/
