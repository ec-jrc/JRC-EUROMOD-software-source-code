using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EM_Statistics.InDepthAnalysis
{
    public class InDepthAnalysis
    {
        internal static string HelpPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\Help", "In-depthAnalysisHelp.chm");

        internal static void SetShowHelp(Form dialog, HelpProvider helpProvider)
        {
            helpProvider.HelpNamespace = HelpPath;
            helpProvider.SetShowHelp(dialog, true);
        }
    }
}
