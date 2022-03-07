using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EM_Common
{
    public class EnvironmentInfo
    {
        private static List<string> showComponents = null;

        public static string GetUserSettingsFolder() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EUROMOD"); }
        public static string GetPluginFolder() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EUROMOD", "PlugIns"); }
        public static string GetUserSelectableTemplateFolder() { return Path.Combine(GetPluginFolder(), "StatisticsPresenter", "UserSelectableTemplates"); }
        public static string GetSystemTemplateFolder() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EUROMOD", "SystemTemplates"); }
        public static string GetApplicationFolder() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "EUROMOD"); }

        public static string GetEM2ExecutableFile()
        {
            string location = isCompilerEnvironment
                        ? Path.Combine(GetAncestorFolder(Assembly.GetExecutingAssembly().Location, "EM_UI"), "bin", bit())
                        : GetApplicationFolder();
            return Path.Combine(location, "executable", "euromod.exe");
        }

        public static void GetEM3ExecutableCallerInfo(ProcessStartInfo processStartInfo)
        {
            if (!isCompilerEnvironment)
            {
                processStartInfo.FileName = Path.Combine(GetApplicationFolder(), "executable", "EM_ExecutableCaller.exe");
                processStartInfo.Arguments = string.Empty;
            }
            else
            {
                processStartInfo.FileName = "dotnet";
                string exeCallerPath = Path.Combine(GetAncestorFolder(Assembly.GetExecutingAssembly().Location, "EM_UI"),
                    "..", "EM_Executable", "EM_ExecutableCaller", "bin", "Debug", "netcoreapp2.0", "EM_ExecutableCaller.dll");
                processStartInfo.Arguments = EncloseByQuotes(exeCallerPath) + " "; // add a space for other arguments to be added
            }
        }

        public static bool isCompilerEnvironment { get { return Assembly.GetExecutingAssembly().Location.ToUpper().Contains("\\EM_UI\\"); } }
        public static bool isDebugEnvironment { get { return IsInDebugEnvironment(Assembly.GetExecutingAssembly().Location); } }
        public static bool isReleaseEnvironment { get { return IsInReleaseEnvironment(Assembly.GetExecutingAssembly().Location); } }
        public static bool IsInDebugEnvironment(string filePath) { return filePath.ToLower().Contains("\\bin\\" + bit() + "\\debug\\"); }
        public static bool IsInReleaseEnvironment(string filePath) { return filePath.ToLower().Contains("\\bin\\" + bit() + "\\release\\"); }

        public static string GetAncestorFolder(string childPath, string ancestorName)
        {
            for (DirectoryInfo parentInfo = new FileInfo(childPath).Directory;
                               parentInfo.FullName.ToLower() != Directory.GetDirectoryRoot(childPath).ToLower();
                               parentInfo = parentInfo.Parent)
                if (parentInfo.Name.ToLower() == ancestorName.ToLower()) return parentInfo.FullName;
            return null;
        }

        private static string bit ()
        {
            return (IntPtr.Size == 8 ? "x64" : (IntPtr.Size == 4 ? "x86" : "unknown"));
        }

        public static bool ShowComponent(string componentName)
        {
            // first check the VC special rule! 
            if (componentName == EnvironmentInfo.Display.VC_show && File.Exists(Path.Combine(GetUserSettingsFolder(), "VCSettings.xml"))) return true;

            if (true) // (showComponents == null)        remove "true" if you want this to load once and not check every time you ask a question
            {
                showComponents = new List<string>();
                if (File.Exists(Path.Combine(GetUserSettingsFolder(), "display.txt")))
                {
                    showComponents = File.ReadAllLines(Path.Combine(GetUserSettingsFolder(), "display.txt")).ToList();
                }
                for (int i = 0; i < showComponents.Count; i++) showComponents[i] = showComponents[i].Trim().ToLower();
            }

            return showComponents.Contains(componentName.Trim().ToLower());
        }

        public static string EncloseByQuotes(string path)
        {
            if (path.EndsWith("\\")) path = path.Substring(0, path.Length - 1);
            return "\"" + path + "\"";
        }

        public static class Display
        {
            public static string IndirectTaxes_debug = "IndirectTaxes_debug";
            public static string PET_advanced = "PET_advanced";
            public static string VC_show = "VC_show";
            public static string HHoT_developer = "HHoT_developer";
            public static string PR_producer = "PR_producer";
        }
    }
}
