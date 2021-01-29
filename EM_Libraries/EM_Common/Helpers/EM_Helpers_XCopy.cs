using System;
using System.IO;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static bool XCopy(string sourceFolder, string copyToFolder, bool overwrite, out string error)
        {
            try
            {
                error = string.Empty;
                string destFolder = Path.Combine(copyToFolder, Path.GetFileName(sourceFolder));
                if (Directory.Exists(destFolder))
                {
                    if (Path.GetFullPath(sourceFolder).ToLower() == Path.GetFullPath(destFolder).ToLower())
                        { error = errMsg("Source- and destination-folder are equal."); return false; }
                    if (!overwrite) { error = errMsg($"folder {destFolder} already exists."); return false; }
                    Directory.Delete(destFolder, true);
                }
                XCopy(new DirectoryInfo(sourceFolder), new DirectoryInfo(destFolder));
                return true;
            }
            catch (Exception exception) { error = errMsg(exception.Message); return false; }

            string errMsg(string msg) { return $"XCopy failed: {msg}"; }
        }

        private static void XCopy(DirectoryInfo sourceDI, DirectoryInfo targetDI)
        {
            Directory.CreateDirectory(targetDI.FullName);

            // copy each file into the new directory.
            foreach (FileInfo fi in sourceDI.GetFiles())
                fi.CopyTo(Path.Combine(targetDI.FullName, fi.Name), true);

            // copy each subdirectory using recursion.
            foreach (DirectoryInfo sourceSubDI in sourceDI.GetDirectories())
            {
                DirectoryInfo targetSubDI = targetDI.CreateSubdirectory(sourceSubDI.Name);
                XCopy(sourceSubDI, targetSubDI);
            }
        }

    }
}
