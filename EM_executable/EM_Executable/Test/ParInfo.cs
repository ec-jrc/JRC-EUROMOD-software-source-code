using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintParInfo()
        {
            RandColor.WriteLine(Environment.NewLine + "parameter-order (140011-12) or function-order (140011) for analysis, x to exit", colorHeading);
            do
            {
                RandColor.Write("order: ", GetSwapColor()); string o = Console.ReadLine(); if (o == "x") break;
                string fo, parOrder;
                fo = o.Contains("-") ? o.Substring(0, o.IndexOf('-')) : o;
                parOrder = o.Contains("-") ? o.Substring(o.IndexOf('-')).TrimStart(new char[] { '-' }) : string.Empty;
                if (!double.TryParse(fo, out double funOrder) || !infoStore.spine.ContainsKey(funOrder)) continue;
                if (parOrder == string.Empty)
                {
                    foreach (var p in infoStore.spine[funOrder].TestGetAllPar(true))
                        RandColor.WriteLine($"{p.description.GetParName()} {p.description.par.order}", GetSwapColor());
                    continue;
                }
                ParBase anaPar = GetPar(funOrder, parOrder);
                if (anaPar != null) PrintParDetails(anaPar);
                RandColor.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - ", colorHeading);
            } while (true);

            ParBase GetPar(double funOrder, string parOrder)
            {
                return (from p in infoStore.spine[funOrder].TestGetAllPar(true)
                        where p.description.par.order == parOrder
                        select p).FirstOrDefault();
            }
        }

        private static void PrintParDetails(ParBase par)
        {
            RandColor.WriteLine(par.description.Get(), GetSwapColor());
            RandColor.WriteLine($"xml-value: {par.xmlValue}", GetSwapColor());

            if (infoStore.hhAdmin == null) return;
            HH dummyHH = infoStore.hhAdmin.GetFirstHH();
            List<Person> dummyTU = new List<Person>() { new Person(0) };

            if (par.GetType() == typeof(ParBool))
                RandColor.WriteLine($"value: {(par as ParBool).GetBoolValue()}", GetSwapColor());
            else if (par.GetType() == typeof(ParCateg))
                RandColor.WriteLine($"value: {(par as ParCateg).GetCateg()}", GetSwapColor());
            else if (par.GetType() == typeof(ParNumber))
                RandColor.WriteLine($"value: {(par as ParNumber).GetValue()}", GetSwapColor());
            else if (par.GetType() == typeof(ParTU))
            {
                string tuName = (par as ParTU).name;
                dummyHH.GetTUs(tuName); // that's to "use" the TU, i.e. to create it for the HH
                RandColor.WriteLine($"TU {tuName} has {dummyHH.hhTUs[tuName].Count} unit(s) in 1st HH", GetSwapColor());
            }
            else if (par.GetType() == typeof(ParVar))
                RandColor.WriteLine($"value: {(par as ParVar).GetValue(dummyHH, dummyTU)}", GetSwapColor());
            else if (par.GetType() == typeof(ParOutVar)) // there is actually nothing to check, out-vars just care for registration
                RandColor.WriteLine($"index: {(par as ParOutVar).index}", GetSwapColor());
            else if (par.GetType() == typeof(ParIL))
                RandColor.WriteLine($"value: {(par as ParIL).GetValue(dummyHH, dummyTU)}", GetSwapColor());
            else if (par.GetType() == typeof(ParVarIL))
                RandColor.WriteLine($"value: {(par as ParVarIL).GetValue(dummyHH, dummyTU)}", GetSwapColor());
            else if (par.GetType() == typeof(ParFormula))
                RandColor.WriteLine($"value: {(par as ParFormula).GetValue(dummyHH, dummyTU)}", GetSwapColor());
            else if (par.GetType() == typeof(ParCond))
                RandColor.WriteLine($"ParCond not yet handled", ConsoleColor.DarkMagenta);
            else RandColor.WriteLine($"unknow type: {par.GetType()}", GetSwapColor());
        }
    }
}
