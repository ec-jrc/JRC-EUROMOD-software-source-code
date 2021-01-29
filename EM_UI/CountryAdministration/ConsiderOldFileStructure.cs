using EM_Common;
using EM_UI.Tools;
using System.IO;
using System.Linq;

namespace EM_UI.CountryAdministration
{
    internal partial class CountryAdministrator
    {
        internal static bool _oldAddOnFileStructure = false;

        internal static bool IsOldAddOnFileStructure()
        {
            if (!Directory.Exists(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)))
                return false; //probably does not happen

            //try to find a folder within folder AddOns that contains an xml file named like the folder (e.g. MTR-folder containing MTR.xml)
            DirectoryInfo addOnContainerFolder = new DirectoryInfo(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles));
            if (addOnContainerFolder.GetFiles().Count() == 0)
                return false; //no info - assume "new style"

            foreach (DirectoryInfo potentialAddOnFolder in addOnContainerFolder.GetDirectories())
            {
                if (File.Exists(EMPath.AddSlash(potentialAddOnFolder.FullName) + potentialAddOnFolder.Name + ".xml"))
                    return false; //if such a folder is found it is assumed that "new style" file structure is already in place
            }
            return true;
        }

        internal static bool ConsiderOldAddOnFileStructure(bool isAddOn = true) //this is just a function with a telling name to spare a comment each time old-style-treatment is asked for (or not)
        {
            return isAddOn && _oldAddOnFileStructure;
        }

        static internal void LoadAddOns()
        {
            if (!Directory.Exists(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)))
                return;

            DirectoryInfo folderAddOns = new DirectoryInfo(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles));
            foreach (FileInfo addOnFile in folderAddOns.GetFiles("*.xml"))
            {
                string addOnShortName = addOnFile.Name.Substring(0, addOnFile.Name.Length - addOnFile.Extension.Length);
                if (_countries.Keys.Contains(addOnShortName.ToLower()))
                    continue; //add-on already in countries list

                Country addOn = new Country();
                addOn._shortName = addOnShortName;

                string flagPathAndFileName = new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages() + addOn._shortName + _imageExtension;
                addOn._flag = GetImageFromFile(flagPathAndFileName, global::EM_UI.Properties.Resources.NoAddOnImage);
                addOn._isAddOn = true;

                _countries.Add(addOn._shortName.ToLower(), addOn);
            }
        }
        
        static internal string GetFlagPath(string shortName, bool addOn)
        { //get "official" flag/symbol path, i.e. the path where country-flag/add-on-symbol ought to be stored from now on (even if it is not yet actually stored there)
            if (ConsiderOldAddOnFileStructure(addOn)) 
                return new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages(); //if add-ons are still stored "loosely" (not in folders) the symbols are still best stored in the Flags-folder (instead of also loosely in the AddOns-folder)
            //otherwise the official flag/symbol path is the folder of the country/add-on
            return EMPath.AddSlash((addOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + shortName);
        }

        static internal System.Drawing.Image GetFlag(string shortName, bool addOn)
        {
            string flagName = shortName + _imageExtension;
            string flagPath = GetFlagPath(shortName, addOn); //first search at the "new" storage place
            if (!File.Exists(flagPath + flagName))
                flagPath = new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderImages(); //if not found, try the "old" storage place
            return GetImageFromFile(flagPath + flagName, addOn ? global::EM_UI.Properties.Resources.NoAddOnImage : global::EM_UI.Properties.Resources.NoFlag);
        }
    }
}
