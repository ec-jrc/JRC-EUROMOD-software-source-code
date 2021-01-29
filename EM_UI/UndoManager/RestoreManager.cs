using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.UndoManager
{
    internal class RestoreManager
    {
        internal static string StoreCountry(EM_UI_MainForm mainForm, bool storeChanges = true)
        {
            CleanBackupFolder(); //delete all folders which are older than 3 days to avoid that the backup folder gets too big

            //outcommented as it is probably more confusing to inform the user than that one cannot undo actions before this (probably big) action ...
            //if (undoManager.HasChanges() && Tools.UserInfoHandler.GetInfo("Please note that the action generates a backup and therefore must reset the undo list. That means no undoing of actions up until now will be possible.",
            //    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            //    return false;

            //... instead pose this easier to understand question (i.e. only ask for saving unsaved changes, but not for any undo-stuff)
            if (storeChanges && mainForm.HasChanges() && Tools.UserInfoHandler.GetInfo("Do you want to save changes?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return string.Empty;

            try
            {
                //the action will directly operate the XML-files therefore changes need to be commited and saved, unless explicitly not wished
                if (storeChanges) SaveDirectXMLAction(mainForm);

                //create a backup by copying files from Countries-folder to a dated folder (see below) in BackUps-folder
                if (!Directory.Exists(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles)))
                    Directory.CreateDirectory(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles));
                string countryShortName = mainForm.GetCountryShortName();
                string backUpFolder = countryShortName + "_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss"); //backup-folder is name e.g. uk_2013-12-08_10-30-23
                if (!Directory.Exists(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder)) //is actually rather unlikely
                    Directory.CreateDirectory(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder);

                //copy XML-files (e.g. uk.xml + uk_DataConfig.xml or LMA.xml)
                string errorMessage;
                if (!Country.CopyXMLFiles(countryShortName, out errorMessage, string.Empty, EMPath.AddSlash(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder)))
                    throw new System.ArgumentException(errorMessage);

                return backUpFolder;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return string.Empty;
            }
        }

        internal static bool RestoreCountry(EM_UI_MainForm mainForm, string backUpFolder = "")
        {
            bool reportSuccess = false;
            if (backUpFolder == string.Empty) //called via button in MainForm
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.SelectedPath = EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles);
                folderBrowserDialog.Description = "Please select the back-up folder";
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    return true;
                backUpFolder = folderBrowserDialog.SelectedPath;
                reportSuccess = true;
            }
            else //called from a catch-branch, i.e. using just generated back-up in Temp/BackUp
                backUpFolder = EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder;

            string countryShortName = mainForm.GetCountryShortName();
            bool isAddOn = CountryAdministrator.IsAddOn(countryShortName);
            try
            {
                //check if backUpFolder contains the necessary files and if the files actually contain a backup of the loaded country/add-on ...
                string errorMessage;
                if (!Country.DoesFolderContainValidXMLFiles(countryShortName, out errorMessage, backUpFolder, isAddOn,
                    true)) //check if countryShortName corresponds to country's short name as stored in the XML-file
                    throw new System.ArgumentException(errorMessage);

                //... if yes, copy files form backup-folder to Countries-folder
                if (!Country.CopyXMLFiles(countryShortName, out errorMessage, backUpFolder))
                    throw new System.ArgumentException(errorMessage);

                mainForm.ReloadCountry();

                if (reportSuccess) //report success only if called via button (and not if called by a failed function)
                    UserInfoHandler.ShowSuccess("Successfully restored using back-up stored in" + Environment.NewLine + backUpFolder + ".");

                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowError("Restore failed because of the error stated below." + Environment.NewLine +
                                          "You may want to try a manual restore via the button in the ribbon 'Country Tools'." + Environment.NewLine + Environment.NewLine +
                                          exception.Message);
                return false;
            }
        }

        internal static void ReportSuccessAndInfo(string successfulAction, string backUpFolder, bool autoRestore = true)
        {
            UserInfoHandler.ShowSuccess(successfulAction + " succeeded!" + Environment.NewLine + Environment.NewLine +
                "Please note that a back-up of the version before the action is stored under " + Environment.NewLine +
                EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder + "." + Environment.NewLine + Environment.NewLine +
                (autoRestore ? "You may restore it via the button 'Restore' in the ribbon 'Country Tools'." : string.Empty));
        }

        static void CleanBackupFolder()
        { //delete all folders which are older than 3 days to avoid that the backup folder gets too big
            try
            {
                if (!Directory.Exists(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles))) Directory.CreateDirectory(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles));
                foreach (string subFolder in Directory.GetDirectories(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles)))
                    if ((new DirectoryInfo(subFolder)).CreationTime < DateTime.Today - new TimeSpan(3, 0, 0, 0))
                        Directory.Delete(subFolder, true);
            }
            catch (Exception exception) { UserInfoHandler.RecordIgnoredException("RestoreManager.CleanBackupFolder", exception); }
        }

        internal static void SaveDirectXMLAction(EM_UI_MainForm mainForm)
        {
            string countryShortName = mainForm.GetCountryShortName();
            CountryAdministrator.GetCountryConfigFacade(countryShortName).GetCountryConfig().AcceptChanges();
            if (!CountryAdministrator.IsAddOn(countryShortName))
                CountryAdministrator.GetDataConfigFacade(countryShortName).GetDataConfig().AcceptChanges();
            mainForm.GetUndoManager().Reset(); //reset undo-manager with the drawback that actions up until now cannot be undone
            lock (EM_UI_MainForm._performActionLock) //to not get into conflict with auto-saving
            {
                CountryAdministrator.WriteXML(countryShortName);
            }
        }

        internal static string StoreFile(string filePath)
        {
            CleanBackupFolder(); //delete all folders which are older than 3 days to avoid that the backup folder gets too big

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                //create a backup by copying file to a dated folder (see below) in BackUps-folder
                if (!Directory.Exists(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles)))
                    Directory.CreateDirectory(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles));
                string backUpFolder = fileInfo.Name + "_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss"); //backup-folder is name e.g. VarConfig.xml_2013-12-08_10-30-23
                if (!Directory.Exists(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder)) //is actually rather unlikely
                    Directory.CreateDirectory(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder);

                File.Copy(filePath, EMPath.AddSlash(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder) + fileInfo.Name);
                return backUpFolder;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return string.Empty;
            }
        }

        internal static bool RestoreFile(string filePath, string backUpFolder)
        {
            backUpFolder = EMPath.AddSlash(EMPath.Folder_BackUps(EM_AppContext.FolderEuromodFiles) + backUpFolder);

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                File.Copy(backUpFolder + fileInfo.Name, filePath, true);
                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowError("Restore failed because of the error stated below." + Environment.NewLine +
                                          "You may want to try a manual restore by copying the file in '" + backUpFolder + "'." + Environment.NewLine + Environment.NewLine +
                                          exception.Message);
                return false;
            }
        }
    }
}
