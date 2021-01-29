using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_Executable
{
    public partial class Control
    {

        // provide the caller (generously) with info on what is run, i.e. all config-settings, system-name, data-name
        private Dictionary<string, string> ComposeRunInfo()
        {
            Dictionary<string, string> runInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cs in infoStore.runConfig.origSettings) runInfo.Add(cs.Key, cs.Value);
            runInfo.Add(Communicator.EXEPROG_DETINFO_SYSNAME, infoStore.country.sys.name);
            runInfo.Add(Communicator.EXEPROG_DETINFO_DATANAME, infoStore.country.data.Name);

            // maybe more to come ...
            return runInfo;
        }

        // output-info is generated later than run-info about:
        // run-info should be provided as soon as possible (before any errors could stop the run)
        // but output-info is only fully available once add-ons, extenstions, etc. are taken into account
        private Dictionary<string, string> ComposeOutputInfo()
        {
            Dictionary<string, string> outputInfo = new Dictionary<string, string>(); int n = 1;

            foreach (FunBase fun in infoStore.spine.Values)
            {
                if (!(fun is FunDefOutput)) continue;
                outputInfo.Add(Communicator.EXEPROG_DETINFO_OUTPUT_FILE + n.ToString(),
                               Path.GetFileNameWithoutExtension((fun as FunDefOutput).GetFileName())); ++n;
            }
            return outputInfo;
        }
    }
}
