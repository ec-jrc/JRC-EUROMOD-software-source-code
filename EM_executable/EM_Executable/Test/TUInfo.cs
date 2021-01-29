using EM_Common;
using System;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintTUInfo()
        {
            PrintHeading("Household info - taxunits");

            PrintSubHeading($"{infoStore.indexTUs.Count} TUs defined:");
            foreach (string tuName in infoStore.indexTUs.Keys) RandColor.Write($"{tuName} ", GetSwapColor()); Console.WriteLine();

            if (infoStore.hhAdmin == null) return;

            int nTestHH = 3; int i = 0;           
            PrintSubHeading($"first and last {nTestHH} households");

            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                if (Exclude(ref i, nTestHH, infoStore.hhAdmin.hhs.Count)) continue; ConsoleColor color = GetSwapColor();
                foreach (var tu in hh.hhTUs) RandColor.WriteLine($"{tu.Key}: {tu.Value.Count} unit(s) in HH {i}", color);
            }
        }
    }
}
