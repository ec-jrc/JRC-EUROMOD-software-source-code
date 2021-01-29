using System;
using System.IO;
using System.Linq;

namespace EM_Common
{
    public partial class EMPath
    {
        private const string FOLDER_XMLPARAM = "XMLParam";
        private const string FOLDER_CONFIG = "Config";
        private const string FOLDER_COUNTRIES = "Countries";
        private const string FOLDER_ADDONS = "AddOns";
        private const string FOLDER_OUTPUT = "Output";
        public const string FOLDER_INPUT = "Input";
        private const string FOLDER_TEMP = "Temp";
        private const string FOLDER_LOG = "Log";
        private const string FOLDER_EM3TRANSLATION = "EM3Translation";
        private const string FILE_VARS = "Variables.xml";
        private const string FILE_EXRATES = "Exchangerates.xml";
        private const string FILE_HICP = "HICP.xml";
        private const string FILE_EXTENSIONS = "Extensions.xml";
        private const string PROJECT_SETTINGS = "ProjectSettings.xml";
        private const string FILE_EMCONFIG = "EMConfig";
        private const string EM2_FILE_EXT_DATACONFIG = "_DataConfig";
        public const string EM2_FILE_VARS = "VarConfig.xml";
        public const string EM2_FILE_EXRATES = "ExchangeRatesConfig.xml";
        public const string EM2_FILE_HICP = "HICPConfig.xml";
        public const string EM2_FILE_EXTENSIONS = "SwitchablePolicyConfig.xml";
        public const string FILE_EMLOG = "em_log.xlsx";

        private string folderEuromodFiles = string.Empty;
        private string folderGlobalFiles = string.Empty;

        private bool useFinalEM3Paths = false; // true: get "final" EM3-paths, i.e. without EM3Translation

        public EMPath(string _folderEuromodFiles, string _folderGlobalFiles = "", bool _useFinalEM3Paths = false) { Init(_folderEuromodFiles, _folderGlobalFiles,_useFinalEM3Paths); }
        public EMPath(string _folderEuromodFiles, bool _useFinalEM3Paths) { Init(_folderEuromodFiles, string.Empty, _useFinalEM3Paths); }
        private void Init(string _folderEuromodFiles, string _folderGlobalFiles, bool _useFinalEM3Paths)
        { folderEuromodFiles = _folderEuromodFiles; folderGlobalFiles = _folderGlobalFiles; useFinalEM3Paths = _useFinalEM3Paths; }

        public string GetFolderEuromodFiles() { return folderEuromodFiles; }
        public string GetFolderEM3Translation() { return useFinalEM3Paths ? folderEuromodFiles : Path.Combine(folderEuromodFiles, FOLDER_EM3TRANSLATION); }
        public string GetFolderXMLParam(bool em2 = false) { return Path.Combine(em2 ? folderEuromodFiles : GetFolderEM3Translation(), FOLDER_XMLPARAM); }
        public string GetFolderCountries(bool em2 = false) { return Path.Combine(GetFolderXMLParam(em2), FOLDER_COUNTRIES); }
        public string GetFolderAddOns(bool em2 = false) { return Path.Combine(GetFolderXMLParam(em2), FOLDER_ADDONS); }
        public string GetFolderConfig(bool em2 = false) { return folderGlobalFiles == string.Empty ? Path.Combine(GetFolderXMLParam(em2), FOLDER_CONFIG) : folderGlobalFiles; }
        public string GetFolderOutput() { return Path.Combine(folderEuromodFiles, FOLDER_OUTPUT); }
        public string GetFolderInput() { return Path.Combine(folderEuromodFiles, FOLDER_INPUT); }
        public string GetFolderTemp() { return Path.Combine(GetFolderXMLParam(em2: true), FOLDER_TEMP); }
        public string GetFolderLog() { return Path.Combine(folderEuromodFiles, FOLDER_LOG); }

        public string GetCountryFolderPath(string country, bool em2 = false) { return Path.Combine(GetFolderCountries(em2), country); }
        public static string GetCountryFileName(string country) { return country + ".xml"; }
        public string GetCountryFilePath(string country, bool em2 = false) { return Path.Combine(GetCountryFolderPath(country, em2), GetCountryFileName(country)); }
        public static string GetEM2DataConfigFileName(string country) { return $"{country}{EM2_FILE_EXT_DATACONFIG}.xml"; }
        public string GetEM2DataConfigFilePath(string country) { return Path.Combine(GetFolderCountries(em2: true), country, GetEM2DataConfigFileName(country)); }

        public string GetAddOnFolderPath(string addOn, bool em2 = false) { return Path.Combine(GetFolderAddOns(em2), addOn); }
        public static string GetAddOnFileName(string addOn) { return addOn + ".xml"; }
        public string GetAddOnFilePath(string addOn, bool em2 = false) { return Path.Combine(GetAddOnFolderPath(addOn, em2), GetAddOnFileName(addOn)); }
        public string GetVarFilePath(bool em2 = false) { return Path.Combine(GetFolderConfig(em2), em2 ? EM2_FILE_VARS : FILE_VARS); }
        public string GetProjectSettingsPath(bool em2 = false) { return Path.Combine(GetFolderConfig(em2), PROJECT_SETTINGS); }

        public string GetAnyConfigFilePath(string fileName, bool em2 = false) { return Path.Combine(GetFolderConfig(em2), fileName); }
        public string FormatAnyConfigFileName(string fileName)
        {
            if (fileName.ToLower().Equals(EM2_FILE_EXRATES.ToLower())) { fileName = EM2_FILE_EXRATES; }
            else if (fileName.ToLower().Equals(EM2_FILE_HICP.ToLower())) { fileName = EM2_FILE_HICP; }
            else if (fileName.ToLower().Equals(EM2_FILE_VARS.ToLower())) { fileName = EM2_FILE_VARS; }
            else if (fileName.ToLower().Equals(EM2_FILE_EXTENSIONS.ToLower())) { fileName = EM2_FILE_EXTENSIONS; }

            return fileName;

        }
        public string GetExRatesFilePath(bool em2 = false) { return Path.Combine(GetFolderConfig(em2), em2 ? EM2_FILE_EXRATES : FILE_EXRATES); }
        public string GetHICPFilePath(bool em2 = false) { return Path.Combine(GetFolderConfig(em2), em2 ? EM2_FILE_HICP : FILE_HICP ); }
        public string GetExtensionsFilePath(bool em2 = false) { return Path.Combine(GetFolderConfig(em2), em2 ? EM2_FILE_EXTENSIONS : FILE_EXTENSIONS); }
        public string GetEmLogFilePath() { return Path.Combine(GetFolderLog(), FILE_EMLOG); }

        public static string GetEMConfigFileName(string id) { return $"{FILE_EMCONFIG}{id}.xml"; }

        public static string GetCountriesFolderName() { return FOLDER_COUNTRIES; }
        public static string GetConfigFolderName() { return FOLDER_CONFIG; }

        public string GetFolderImages()
        {
            string pathImages = Path.Combine(GetFolderConfig(true), "Images"), pathFlages = Path.Combine(GetFolderConfig(true), "Flags");
            if (!Directory.Exists(pathImages) && Directory.Exists(pathFlages))
                return AddSlash(pathFlages); // if folder 'Images' does not (yet) exist, but the old-file-structure-folder 'Flags' does: return folder 'Flags'
            return AddSlash(pathImages); // else return folder 'Images' whether it exists or not (failure to be catched in calling function)
        }

        public string File_AtAlternativeEMPath(string fileInEuromodFileStructure, string alternativePath) { return FileFolder_AtAlternativeEMPath(fileInEuromodFileStructure, alternativePath); }
        public string Folder_AtAlternativeEMPath(string fileInEuromodFileStructure, string alternativePath) { return FileFolder_AtAlternativeEMPath(fileInEuromodFileStructure, alternativePath); }
        string FileFolder_AtAlternativeEMPath(string fileInEuromodFileStructure, string alternativePath)
        {
            if (!fileInEuromodFileStructure.StartsWith(folderEuromodFiles)) return string.Empty; //should not happen, wrong function-call
            return AddSlash(alternativePath) + fileInEuromodFileStructure.Substring(folderEuromodFiles.Count());
        }

        public static bool IsSamePath(string path1, string path2)
        {
            if (path1 == null && path2 == null) return true; if (path1 == null || path2 == null) return false;
            return string.Compare(Path.GetFullPath(path1).TrimEnd('\\'), Path.GetFullPath(path2).TrimEnd('\\'),
                                  StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
