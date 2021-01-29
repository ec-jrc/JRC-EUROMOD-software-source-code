using EM_Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.Tools
{
    internal class XCopy
    {
        //Helper
        internal static bool Folder(string sourceFolderPath, string destinationFolderPath, string renameFolderTo = "")
        {
            bool undo = false;
            if (!FolderRecursivly(sourceFolderPath, destinationFolderPath, ref undo, renameFolderTo))
            {
                if (!undo)
                    return false;
                try
                {
                    //undo any copying that already happened
                    DirectoryInfo sourceFolder = new DirectoryInfo(sourceFolderPath);
                    if (renameFolderTo == string.Empty) destinationFolderPath = EMPath.AddSlash(destinationFolderPath + sourceFolder.Name);
                    else destinationFolderPath = EMPath.AddSlash(destinationFolderPath + renameFolderTo);

                    if (Directory.Exists(destinationFolderPath))
                        Directory.Delete(destinationFolderPath, true);
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception);
                }
                return false;
            }
            return true;
        }

        static bool FolderRecursivly(string sourceFolderPath, string destinationFolderPath, ref bool undo, string renameFolderTo = "")
        {
            sourceFolderPath = EMPath.AddSlash(sourceFolderPath);
            destinationFolderPath = EMPath.AddSlash(destinationFolderPath);

            try
            {
                DirectoryInfo sourceFolder = new DirectoryInfo(sourceFolderPath);
                if (renameFolderTo == string.Empty) destinationFolderPath = EMPath.AddSlash(destinationFolderPath + sourceFolder.Name);
                else destinationFolderPath = EMPath.AddSlash(destinationFolderPath + renameFolderTo);

                //check if trying to copy in same parent folder
                if (destinationFolderPath.ToLower() == sourceFolderPath.ToLower())
                {
                    UserInfoHandler.ShowError("Cannot duplicate folder '" + sourceFolder.Name + "' in its parent folder.");
                    return false;
                }

                //check if source folder already exists at destination path
                if (Directory.Exists(destinationFolderPath))
                {
                    if (UserInfoHandler.GetInfo("Folder '" + destinationFolderPath + "' already exists. Should it be deleted?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return false;
                    Directory.Delete(destinationFolderPath, true);
                }

                //create source folder at destination path
                Directory.CreateDirectory(destinationFolderPath);
                undo = true;

                //copy all files directly under this folder
                FileInfo[] subFiles = sourceFolder.GetFiles("*.*");
                if (subFiles != null)
                {
                    foreach (FileInfo subFile in subFiles)
                    {
                        File.Copy(subFile.FullName, destinationFolderPath + subFile.Name);
                    }
                }
                //find all subfolders under this folder for recursive call
                foreach (DirectoryInfo subFolder in sourceFolder.GetDirectories())
                {
                    FolderRecursivly(subFolder.FullName, destinationFolderPath, ref undo);
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Close and reopen the user interface and try again.");
                return false;
            }
            return true;
        }
    }
}
