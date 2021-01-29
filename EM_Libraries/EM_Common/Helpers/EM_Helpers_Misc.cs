using System;
using System.Linq;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static string RemoveWhitespace(string s)
        {
            for (int i = s.Length - 1; i >= 0; --i) if (Char.IsWhiteSpace(s, i)) s = s.Remove(i, 1);
            return s;
        }

        public static string EncloseByQuotes(string path)
        {
            if (path.EndsWith("\\")) path = path.Substring(0, path.Length - 1);
            return "\"" + path + "\"";
        }

        public static string RemoveQuotes(string path)
        {
            return path.StartsWith("\"") && path.EndsWith("\"") ? path.Trim(new char[] { '\"' }) : path;
        }

        public static bool DoesFormulaContainComponent(string formula, string component)
        {
            if (component == string.Empty)
                return false;

            formula = formula.ToLower();
            component = component.ToLower();

            for (int index = formula.IndexOf(component); index >= 0; index = formula.IndexOf(component))
            {
                if (index != 0) //check character before place of finding
                {
                    if (EM_Helpers.IsLetterOrDigit(formula.ElementAt(index - 1)) || formula.ElementAt(index - 1) == '_')
                    {
                        formula = formula.Substring(index + 1);
                        continue; //if a letter, digit or underscore: word does not begin with component (e.g. look for 'bun' find 'yabun')
                    }
                }

                index = index + component.Length;
                if (index < formula.Length - 1) //check character after place of finding
                {
                    if (EM_Helpers.IsLetterOrDigit(formula.ElementAt(index)) || formula.ElementAt(index) == '_')
                    {
                        formula = formula.Substring(1);
                        continue; //if a letter, digit or underscore: word does not end with component (e.g. look for 'bun_s' find 'bun')
                    }
                }
                return true;
            }

            return false;
        }
    }
}
