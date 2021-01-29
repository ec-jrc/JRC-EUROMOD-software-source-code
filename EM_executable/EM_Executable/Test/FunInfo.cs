using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintFunInfo()
        {
            foreach (var fun in infoStore.spine)
            {
                RandColor.Write($"{fun.Value.description.GetPolName()} {fun.Value.description.GetFunName()} ", colorHeading);
                RandColor.Write($"order: {fun.Key} ", GetSwapColor());
                RandColor.WriteLine($"id: {fun.Value.description.funID}", GetSwapColor());
            }

            RandColor.WriteLine(Environment.NewLine + "- - - - - - - - - - - - - - - - - - - - - - - - ", colorHeading);
            RandColor.WriteLine(Environment.NewLine + "Press x to exit, order to analyse function", colorHeading);
            do
            {
                RandColor.Write("order: ", GetSwapColor()); string o = Console.ReadLine(); if (o == "x") break;
                if (!int.TryParse(o, out int order) || !infoStore.spine.ContainsKey(order)) continue;

                PrintParAsDefined(infoStore.spine[order]);
                PrintParAsRead(infoStore.spine[order]);

                RandColor.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - ", colorHeading);

            } while (true);
        }

        private static void PrintParAsDefined(FunBase fun)
        {
            DefinitionAdmin.Fun funDef = DefinitionAdmin.GetFunDefinition(fun.description.GetFunName());
            PrintSubHeading("Possible parameters");

            CompOpt(funDef.GetParList());
            foreach (DefinitionAdmin.ParGroup group in funDef.GetGroupParList())
            {
                RandColor.WriteLine($"{(group.minCount > 0 ? "Compuslory" : "Optional")} group {group.groupName}:", colorHeading);
                CompOpt(group.par, "   ");
            }

            void CompOpt(Dictionary<string, DefinitionAdmin.Par> parList, string space = "")
            {
                RandColor.Write($"{space}Compulsory: ", colorHeading);
                foreach (var par in parList)
                    if (par.Value.minCount > 0) RandColor.Write($"{par.Key} ", GetSwapColor());
                RandColor.Write(Environment.NewLine + $"{space}Optional: ", colorHeading);
                foreach (var par in parList)
                    if (par.Value.minCount == 0) RandColor.Write($"{par.Key} ", GetSwapColor());
                Console.WriteLine();
            }
        }

        private static void PrintParAsRead(FunBase fun)
        {
            PrintSubHeading("Actual parameters");

            RandColor.Write($"Unique pars: ", colorHeading);
            foreach (var par in fun.TestGetUniquePar())
                RandColor.Write($"{par.Key} ", GetSwapColor());

            RandColor.Write(Environment.NewLine + $"Non-unique pars: ", colorHeading);
            foreach (var par in fun.TestGetNonUniquePar())
                RandColor.Write($"{par.Key} ({par.Value.Count}) ", GetSwapColor());

            RandColor.Write(Environment.NewLine + $"Groups: ", colorHeading);
            foreach (var cat in fun.TestGetGroupPar())
                foreach (var group in cat.Value)
                {
                    Console.WriteLine();
                    RandColor.Write($"   {cat.Key}{group.Key}: ", colorHeading);
                    foreach (var par in group.Value) RandColor.Write($"{par.description.GetParName()} ", GetSwapColor());
                }

            RandColor.Write(Environment.NewLine + $"Placeholder pars: ", colorHeading);
            foreach (var par in fun.TestGetPlaceholderPar())
                RandColor.Write($"{par.Key} ", GetSwapColor());

            RandColor.Write(Environment.NewLine + $"Footnote pars: ", colorHeading);
            foreach (var no in fun.TestGetFootnotePar())
                foreach (var par in no.Value)
                    RandColor.Write($"{par.Key}_{no.Key} ", GetSwapColor());
            Console.WriteLine();
        }
    }
}
