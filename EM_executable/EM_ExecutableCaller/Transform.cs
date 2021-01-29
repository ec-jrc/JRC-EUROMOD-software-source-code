using EM_Common;
using EM_Transformer;
using System;
using System.Collections.Generic;

namespace EM_ExecutableCaller
{
    internal partial class Transform
    {
        internal static int GoUI(string[] args)
        {
            try
            {
                if (args.Length < 2) return WrongArgumentError();

                string emPath = EM_Helpers.RemoveQuotes(args[0].Trim());
                string cc = args[1], ao = args.Length > 2 ? args[2] : string.Empty;

                foreach (string country in cc.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    EM3Country.Transform(emPath, country, out List<string> errors);
                    foreach (string error in errors) Console.Error.WriteLine(error);
                }
                foreach (string addOn in ao.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    EM3Country.TransformAddOn(emPath, addOn, out List<string> errors);
                    foreach (string error in errors) Console.Error.WriteLine(error);
                }

                EM3Global.Transform(emPath, out List<string> gErrors);
                foreach (string error in gErrors) Console.Error.WriteLine(error);

                EM3Variables.Transform(emPath, out List<string> vErrors);
                foreach (string error in vErrors) Console.Error.WriteLine(error);

                return 0; // as it is unlikely that one can run (with useful results) if there are errors, the UI shows the errors and asks the user
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("EUROMOD3 TRANSFORM: " + exception.Message);
                return 1;
            }

            int WrongArgumentError()
            {
                string actArgs = string.Empty; foreach (string arg in args) actArgs += arg + " ";
                Console.Error.WriteLine("EUROMOD3 TRANSFORM: invalid use. Correct: EM_ExecutableCaller UI_TRANSFORM emPath cc1[|cc2|...|ccN] [ao1[|ao2|...|aoN]]" +
                    Environment.NewLine + $"Acutal use: EM_ExecutableCaller TRANSFORM {actArgs}");
                return 1;
            }
        }

        internal static int Go(string[] args)
        {
            // consider a command-line-transform with options (probably wait for request)
            // e.g. EM_ExecutableCaller -prog TRANSFORM -emPath "some path" [-cc country1] ... [-cc countryN] [-ao addOn1] ... [-ao addOnN] /glob /var
            //      (glob = transform global files, var = transform variables file)
            return 1;
        }
    }
}
