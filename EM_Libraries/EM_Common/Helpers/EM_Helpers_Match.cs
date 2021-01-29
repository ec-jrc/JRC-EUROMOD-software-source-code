using System.Text.RegularExpressions;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static bool DoesValueMatchPattern(string pattern, string value,
                                                 bool matchCase = false, bool matchWord = false, bool regExpr = false)
        {
            if (!matchCase) { pattern = pattern.ToLower(); value = value.ToLower(); }
            if (!regExpr) pattern = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "."); // for non-regular expressions, replace wildcards with the regexp equivalents
            else pattern = pattern.Replace("$", "\\$");  // for regular expressions, escape "$" so that it can be used as a litteral character
            if (matchWord) pattern = "^(.*[^a-z_$A-Z0-9]+)?" + pattern + "([^a-z_$A-Z0-9]+.*)?$";
            else pattern = "^" + pattern + "$";
            Regex regEx = new Regex(pattern);
            return regEx.IsMatch(value);
        }
    }
}
