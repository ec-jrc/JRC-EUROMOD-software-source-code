using System;
using System.IO;
using System.Reflection;

namespace EM_ExecutableCaller
{
    public class GetEM3TestFolder
    {
        private const string FOLDER_EM3_DEVELOPMENT = "EM3_DEVELOPMENT";
        private const string FOLDER_EM3_TESTDATA = "EM3_TESTDATA";
        private const string FOLDER_EM2_EUROMODFILES = "EM2_EuromodFiles";
        private const string FOLDER_EM_DATA = "EM_Data";
        private const string FOLDER_EM_OUTPUT = "EM_Output";
        private const string FOLDER_TESTPROJECTS = "TestProjects";
        internal const string FOLDER_EM3TEST = "EM3Test";

        public static string Root() { return Path.Combine(new DirectoryInfo(FindEM3DevelopmentRoot()).Parent.FullName, FOLDER_EM3_TESTDATA); }
        public static string EM2EuromodFiles() { return Path.Combine(Root(), FOLDER_EM2_EUROMODFILES); }
        public static string EM_Data() { return Path.Combine(Root(), FOLDER_EM_DATA); }
        public static string EM_Output() { return Path.Combine(Root(), FOLDER_EM_OUTPUT); }
        public static string TestProjects() { return Path.Combine(FindEM3DevelopmentRoot(), FOLDER_TESTPROJECTS); }
        public static string Rel_EM3Output(string em2OutputFolder) { return Path.Combine(em2OutputFolder, FOLDER_EM3TEST); }

        private static string FindEM3DevelopmentRoot()
        {
            try
            {
                DirectoryInfo di;
                for (di = new DirectoryInfo(new FileInfo(Assembly.GetExecutingAssembly().Location).FullName);
                     di.Name.ToLower() != FOLDER_EM3_DEVELOPMENT.ToLower();
                     di = new DirectoryInfo(di.Parent.FullName));
                return di.FullName;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to assess local folder {FOLDER_EM3_DEVELOPMENT}:{Environment.NewLine}{e.Message}");
            }
        }

        // that's the structure
        // - EM3_DEVELOPMENT
        // - EM3_TESTDATA
        //   - EM_Data
        //   - EM_Output
        //   - EM2_EuromodFiles
        //     - XMLParam (with subfolders Countries, Config, ...)
        //     - Output
        //       - EM3TEST (default path for EM3 output)
        //     - other EM2_Folders (Input, ...)
        //     - EM3TRANSLATION
        //       - XMLParam (with subfolders Countries, Config, ...)
    }
}
