using EM_Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.Tools
{
    class InUseFileHandler
    {
        static string GetInUseFilePath(string fullFileName)
        {
            return fullFileName + "_in_use.txt";
        }

        internal static bool CaptureFile(string filePath, string fileName, ref bool openedReadOnly)
        {
            string fullFileName = EMPath.AddSlash(filePath) + fileName;
            return CaptureFile(fullFileName, ref openedReadOnly);
        }

        internal static bool CaptureFile(string fullFileName, ref bool openedReadOnly)
        {
            try
            {
                openedReadOnly = false;
                string inUseFilePath = GetInUseFilePath(fullFileName);
                if (File.Exists(inUseFilePath))
                {
                    string usedBy; using (StreamReader reader = new StreamReader(inUseFilePath)) usedBy = reader.ReadLine();
                    DialogResult userDecision = UserInfoHandler.GetInfo(usedBy + Environment.NewLine +
                        "Do you want to open this country file in read-only mode?" + Environment.NewLine + Environment.NewLine +
                        "Click 'Yes' to open in read-only mode." + Environment.NewLine +
                        "If you know this lock has been caused by an error, click 'No' to unlock it and open in read-write mode." + Environment.NewLine +
                        "Click 'Cancel' if you do not want to open this country file." + Environment.NewLine, MessageBoxButtons.YesNoCancel);
                    if (userDecision == DialogResult.Cancel)
                        return false;
                    if (userDecision == DialogResult.Yes)
                    {
                        openedReadOnly = true;
                        return true;
                    }
                }

                using (StreamWriter writer = new StreamWriter(inUseFilePath))
                    writer.WriteLine(fullFileName + " is currently being edited by user '" + System.Environment.UserName + "'.");
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
            return true; //do not stop opening, just because something goes wrong with lock-file-generating
        }

        internal static void ReleaseFile(string filePath, string fileName)
        {
            string fullFileName = EMPath.AddSlash(filePath) + fileName;
            ReleaseFile(fullFileName);
        }

        internal static void ReleaseFile(string fullFileName)
        {
            try
            {
                string inUseFilePath = GetInUseFilePath(fullFileName);
                if (File.Exists(inUseFilePath))
                    File.Delete(inUseFilePath);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }
    }
}

