using EM_Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EM_Executable
{
    internal class FunCallProgramme : FunOutOfSpineBase
    {
        internal FunCallProgramme(InfoStore infoStore) : base(infoStore) { }

        private string programme = null, progPath = null, replaceByEMPath = null;
        private List<string> arguments = new List<string>();
        private bool wait = false, unifySlash = true;

        protected override void PrepareNonCommonPar()
        {
            programme = GetParBaseValueOrDefault(DefFun.CallProgramme, DefPar.CallProgramme.Programme);
            progPath = GetParBaseValueOrDefault(DefFun.CallProgramme, DefPar.CallProgramme.Path);
            foreach (ParBase a in GetNonUniquePar<ParBase>(DefPar.CallProgramme.Argument)) arguments.Add(a.xmlValue);
            replaceByEMPath = GetParBaseValueOrDefault(DefFun.CallProgramme, DefPar.CallProgramme.RepByEMPath);
            wait = GetParBoolValueOrDefault(DefFun.CallProgramme, DefPar.CallProgramme.Wait);
            unifySlash = GetParBoolValueOrDefault(DefFun.CallProgramme, DefPar.CallProgramme.UnifySlash);

            // one could do checking here to inform the user about errors earlier,
            // but checking at run-time is more flexible, as things may change
        }

        protected override void DoFunWork()
        {
            // arguments
            string args = string.Empty; foreach (string a in arguments) args += a + " "; args.TrimEnd();
            if (!string.IsNullOrEmpty(replaceByEMPath))
            {
                args = args.Replace(replaceByEMPath, GetEMPath(infoStore));
                args = args.Replace("\\\\", "\\"); // do not care about // which could be correct for a web-address
            }
            if (unifySlash) args.Replace('/', '\\');

            // path
            string fullPath = ComposePath(progPath, programme, replaceByEMPath, infoStore, unifySlash);

            Process process = new Process();
            process.StartInfo.FileName = fullPath;
            process.StartInfo.Arguments = args;

            try // contain any exceptions thrown by the process
            {
                process.Start(); // ... return value is not relevant - it's not indicating an error (just that no new instance of the process was started)
            }
            catch (Exception exception)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    message = $"{description.Get()}: {exception.Message}"
                });
            }

            if (wait) process.WaitForExit();
        }

        // is also called by FunDefInput
        internal static string ComposePath(string path, string file, string repByEMPath, InfoStore infoStore, bool? unifySlash = null)
        {
            if (string.IsNullOrEmpty(path)) return file;

            // remove quotation marks
            path = EM_Helpers.RemoveQuotes(path); file = EM_Helpers.RemoveQuotes(file);

            if (!string.IsNullOrEmpty(repByEMPath))
            {
                path = path.Replace(repByEMPath, GetEMPath(infoStore));
                path = path.Replace("\\\\", "\\").Replace("//", "/").Replace("/\\", "\\").Replace("\\/", "\\");
            }
            if (unifySlash == true) path.Replace('/', '\\');

            return Path.Combine(path, file);
        }

        private static string GetEMPath(InfoStore infoStore)
        {
            string emPath = infoStore.runConfig.pathHandler.GetFolderEuromodFiles();
            if (!emPath.EndsWith("\\")) emPath += "\\";
            return emPath;
        }
    }
}
