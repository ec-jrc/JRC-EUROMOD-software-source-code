using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EM_Common
{
    public class PiLoader
    {
        public static bool GetPlugInList(out List<PiInterface> plugInList) { string dummy; return GetPlugInList(out plugInList, out dummy); }
        public static bool GetPlugInList(out List<PiInterface> plugInList, out string warnings)
        {
            plugInList = new List<PiInterface>(); warnings = string.Empty;
            try
            {
                string plugInFolder = EnvironmentInfo.isCompilerEnvironment
                        ? Path.Combine(EnvironmentInfo.GetAncestorFolder(Assembly.GetExecutingAssembly().Location, "EM_PRIVATE"), "EM_PlugIns\\")
                        : Path.Combine(EnvironmentInfo.GetPluginFolder());

                // search each subfolder in the PlugIns-folder ...
                foreach (string potentialPlugIn in Directory.GetDirectories(plugInFolder))
                { // ... for a dll-file with equal name as the subfolder (e.g. IncomeListComponents.dll in folder IncomeListComponents)
                    try
                    {
                        string[] dlls = null; string ppi = potentialPlugIn;
                        for (int trial = 1; trial <= 2; ++trial)
                        {
                            dlls = Directory.GetFiles(potentialPlugIn,
                                (new DirectoryInfo(ppi)).Name + ".dll", // only take dlls into account which are named like the sub-folder
                                SearchOption.AllDirectories); // dll may be in the root-folder (non-web), or in a bin-folder (web)
                            if (dlls.Count() > 0) break;

                            // second attempt takes account of folder name "Summary Statistics" with dll named "SummaryStatistics.dll"
                            // same for Income List Components, this should in future be avoided !!!
                            ppi = ppi.Replace(" ", "");
                        }
                        if (dlls.Count() == 0) continue; // do not issue a warning, might be some unnecessary folder

                        string dllPath = string.Empty;
                        if (dlls.Count() > 1 && EnvironmentInfo.isCompilerEnvironment)
                        { // in compiler-environment there may be up to 4 matching dlls (in 'obj\debug', 'bin\debug', 'obj\release' and 'bin\release')
                            foreach (var dll in dlls)
                            {
                                if (EnvironmentInfo.isDebugEnvironment && EnvironmentInfo.IsInDebugEnvironment(dll)) { dllPath = dll; break; }
                                if (EnvironmentInfo.isReleaseEnvironment && EnvironmentInfo.IsInReleaseEnvironment(dll)) { dllPath = dll; break; }
                            }
                        }
                        if (dllPath == string.Empty) dllPath = dlls.First(); // more accurate verification is picky, I guess

                        Assembly piAssembly = Assembly.LoadFrom(dllPath);
                        if (piAssembly == null)
                        {
                            warnings += string.Format("Failed to load plug-in library '{0}'", dllPath) + Environment.NewLine;
                            continue;
                        }
                        Type piType = null;
                        foreach (Type t in piAssembly.GetTypes())
                            if (t.IsSubclassOf(typeof(PiInterface))) { piType = t; break; }
                        if (piType == null)
                        {
                            // simply ignore this case, as there might be applications in the plug-in-folder which aren't plug-ins (e.g. StatisticsViewer)
                            // warnings += string.Format("Library '{0}' does not contain a plug-in interface.", dllPath) + Environment.NewLine;
                            continue;
                        }
                        if (piType.GetMethod("Run") == null)
                        {
                            warnings += string.Format("Plug-in '{0}' does not provide a 'Run' function.", dllPath) + Environment.NewLine;
                            continue;
                        }
                        plugInList.Add(Activator.CreateInstance(piType) as PiInterface); // create an instance of the plug-in
                    }
                    catch (Exception innerException) { warnings += innerException.Message + Environment.NewLine; }
                }
            }
            catch (Exception outerException) { warnings += outerException.Message + Environment.NewLine; }
            return warnings == string.Empty;
        }

        public static PiInterface GetPlugIn(string plugInTitle)
        {
            List<PiInterface> plugInList; GetPlugInList(out plugInList);
            foreach (PiInterface pi in plugInList)
                if (plugInTitle.ToLower() == pi.GetTitle().ToLower()) return pi;
            return null;
        }
    }
}
