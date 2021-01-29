using EM_Common;
using System;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintHHInfo()
        {
            PrintHeading("Household info - variables");
            if (infoStore.hhAdmin == null) { RandColor.WriteLine("No info on households available", colorInfo); return; }

            int nTestHH = 5; int i = 0;           
            PrintSubHeading($"first and last {nTestHH} households");

            foreach (string varName in infoStore.operandAdmin.GetPersonVarNameList()) RandColor.Write($"{varName,7} ", GetSwapColor());
            RandColor.WriteLine(""); // header: names of variables

            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                if (Exclude(ref i, nTestHH, infoStore.hhAdmin.hhs.Count)) continue; ConsoleColor color = GetSwapColor();
                for (int iPerson = 0; iPerson < hh.personVarList.Count; ++iPerson)
                {        
                    foreach (var d in hh.personVarList[iPerson]) RandColor.Write($"{Math.Round(d,2, MidpointRounding.AwayFromZero),7} ", color);
                    foreach (var s in hh.personStringVarList[iPerson]) RandColor.Write($"{s} ", color);
                    Console.WriteLine();
                }
            }
        }
    }
}
