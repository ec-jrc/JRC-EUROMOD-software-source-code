using System.IO;

namespace EM_Common
{
    public class DefStdOutput
    {
        /// <summary> does policy name follow the rules for being the standard output policy, i.e output_std_cc or output_std_hh_cc </summary>
        public static bool IsOutputStdPol(string polName, string country = null) { return IsOutputStdPol(polName, "output_std_", country); }
        public static bool IsOutputStdHHPol(string polName, string country = null) { return IsOutputStdPol(polName, "output_std_hh_", country); }
        private static bool IsOutputStdPol(string polName, string prefix, string country = null)
        {
            return country == null ? polName.ToLower().StartsWith(prefix) : polName.ToLower() == $"{prefix.ToLower()}{country.ToLower()}";
        }

        /// <summary> does filename follow the rules for being a standard output file, i.e. 'system-name + _std' or 'system-name + _std_hh' </summary>
        public static bool IsOutputStdFileName(string fileName, string sysName = null) { return IsOutputStdFileName(fileName, "std", sysName); }
        public static bool IsOutputStdHHFileName(string fileName, string sysName = null) { return IsOutputStdFileName(fileName, "std_hh", sysName); }
        private static bool IsOutputStdFileName(string fileName, string suffix, string sysName = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            return EM_Helpers.DoesValueMatchPattern($"{sysName ?? "*"}_{suffix}", fileName);
        }
    }
}
