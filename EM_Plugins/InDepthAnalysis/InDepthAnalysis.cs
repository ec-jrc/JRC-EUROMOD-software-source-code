using EM_Common;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    public class InDepthAnalysis : PiInterface
    {
        public override string GetTitle()
        {
            return "In-depth Analysis";
        }

        public override string GetDescription()
        {
            return "This Plug-in allows for in-depth analysis of EUROMOD output.";
        }

        public override string GetFullFileName()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public override bool IsWebApplicable()
        {
            return false;
        }

        public override void Run(Dictionary<string, object> arguments = null)
        {
            new InDepthAnalysisForm().Show();
        }

        internal static string HelpPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Help.chm");

        internal static void SetShowHelp(Form dialog, HelpProvider helpProvider)
        {
            helpProvider.HelpNamespace = HelpPath;
            helpProvider.SetShowHelp(dialog, true);
        }
    }
}
