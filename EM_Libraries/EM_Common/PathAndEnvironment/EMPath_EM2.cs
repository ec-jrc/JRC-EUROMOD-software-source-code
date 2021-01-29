using System;
using System.IO;
using System.Linq;

namespace EM_Common
{
    /// <summary>
    /// the functions inside this file rebuild functions of the (deleted) EM_UI.PathHelper class
    /// this is done, instead of directly replacing all usages by new EMPath-functions,
    /// to avoid a lot of effort in following all branches, moreover it seems saver, given that calling functions e.g. assume a slash at the end ...
    /// </summary>
    public partial class EMPath
    {
        public static string Folder_Countries_withoutPath() { return AddSlash(Path.Combine(FOLDER_XMLPARAM, FOLDER_COUNTRIES)); }
        public static string Folder_Countries(string folderEuromodFiles) { return AddSlash(Path.Combine(folderEuromodFiles, Folder_Countries_withoutPath())); }
        public static string Folder_Config(string folderEuromodFiles) { return AddSlash(Path.Combine(folderEuromodFiles, FOLDER_XMLPARAM, FOLDER_CONFIG)); }
        public static string Folder_Temp(string folderEuromodFiles) { return AddSlash(Path.Combine(folderEuromodFiles, FOLDER_XMLPARAM, FOLDER_TEMP)); }
        public static string Folder_AddOns(string folderEuromodFiles) { return AddSlash(Path.Combine(folderEuromodFiles, FOLDER_XMLPARAM, FOLDER_ADDONS)); }
        public static string Folder_BackUps(string folderEuromodFiles) { return AddSlash(Path.Combine(Folder_Temp(folderEuromodFiles), "BackUp")); }

        public static string AddSlash(string path) { return path.Count() == 0 || path.Last() == '\\' || path.Last() == '/' ? path : path + "\\"; }
    }
}
