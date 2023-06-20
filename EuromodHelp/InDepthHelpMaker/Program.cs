using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace InDepthHelpMaker
{
    class Program
    {
        static void Main()
        {
            string hhcExe = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), "HTML Help Workshop", "hhc.exe");
            string hhcExeX86 = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "HTML Help Workshop", "hhc.exe");
			string projectFolder = "InDepthHelpMaker";
			string filename = "In-depthAnalysisHelp.hhp";
			
			
            if (!File.Exists(hhcExe))
            {
                if (File.Exists(hhcExeX86)) hhcExe = hhcExeX86;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Neither {hhcExe} nor {hhcExeX86} found!");
                    Console.ReadLine(); return;
                }
            }

            string hhpFile = null;
            int i = Assembly.GetExecutingAssembly().Location.IndexOf(projectFolder);
            if (i >= 0) hhpFile = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, i + projectFolder.Length), filename);
            if (hhpFile == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"???\\{projectFolder}\\{filename} not found!");
                Console.ReadLine(); return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Process.Start(hhcExe, hhpFile);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadLine();
        }
    }
}
