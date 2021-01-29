using EM_Common;
using System;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintGeneralInfo()
        {
            PrintHeading("General info");
            if (infoStore.hhAdmin == null) { RandColor.WriteLine("No info on households available", colorInfo); return; }

            int pCnt = 0;
            foreach (HH hh in infoStore.hhAdmin.hhs) pCnt += hh.personVarList.Count;

            RandColor.WriteLine($"Number of households:        {infoStore.hhAdmin.hhs.Count:N0}", colorInfo);
            RandColor.WriteLine($"Number of persons:           {pCnt:N0}", colorInfo);
            HH anyHH = infoStore.hhAdmin.hhs[0];
            RandColor.WriteLine($"Number of numeric variables: {anyHH.personVarList[0].Count:N0}", colorInfo);
            RandColor.WriteLine($"Number of string variables:  {anyHH.personStringVarList[0].Count:N0}", colorInfo);
        }
    }
}
