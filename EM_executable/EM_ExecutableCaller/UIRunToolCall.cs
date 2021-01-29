using EM_Executable;
using EM_Transformer;
using System;
using System.Collections.Generic;

namespace EM_ExecutableCaller
{
    internal class UIRunToolCall
    {
        internal static int Go(string[] args)
        {
            try
            {
                if (args.Length != 1) return WrongArgumentError();

                // transform configuration file to EM3-structure
                if (!TransformEMConfig.Transform(args[0], out Dictionary<string, string> config, new Action<string>(
                                                 err => { Console.Error.WriteLine(err); }))) return 0;

                // call EM3 executable
                bool success = new Control().Run(config, progressInfo => { Console.WriteLine(progressInfo.message); return true; },
                    errorInfo => { Console.Error.WriteLine((errorInfo.isWarning ? "warning: " : "error: ") + errorInfo.message); });

                return success ? 0 : 1;

                int WrongArgumentError()
                {
                    Console.Error.WriteLine("EUROMOD3 UI_RUNTOOL_CALL: invalid use. Correct: EM_ExecutableCaller UI_RUNTOOL_CALL pathToEMConfig");
                    return 1;
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }
    }
}
