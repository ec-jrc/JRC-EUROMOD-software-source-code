using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static bool IsInteger(double d) { return d == (int)d; }
        public static bool IsNonNegInteger(double d, bool allowZero = true) { return IsInteger(d) && (allowZero ? d >= 0 : d > 0); }
        public static bool IsNonNegInteger(string s, bool allowZero = true) { return !int.TryParse(s, out int i) ? false : IsNonNegInteger(i, allowZero); }
        public static bool IsDigit(char c) { return (c >= '0' && c <= '9'); }
        public static bool IsLetter(char c) { return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'); }
        public static bool IsLetterOrDigit(char c) { return IsDigit(c) || IsLetter(c); }
        public static bool IsHexDigit(char c) { return (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'); }
        public static bool IsNumeric(string toTest, bool allowHexDigits = false)
        {
            for (int index = toTest.StartsWith("-") || toTest.StartsWith("+") ? 1 : 0; index < toTest.Length; ++index)
            {
                char c = toTest.ElementAt(index);
                if (!IsDigit(c) && c.ToString() != uiDecimalSeparator && (!allowHexDigits || !IsHexDigit(c)))
                    return false;
            }
            return true;
        }

        public static bool IsGuid(string ID) { return Guid.TryParse(ID, out Guid dummy); }

        public static bool IsValidBool(string s)
        {
            return (new List<string>() { "0", "1", bool.TrueString.ToLower(), bool.FalseString.ToLower(), DefPar.Value.NO, DefPar.Value.YES }).Contains(s.ToLower());
        }

        public static bool IsValidName(string name, out string illegal) // this is very strict, not sure whether it needs to be a eased ...
        {
            string validChar = "0123456789abcdefghijklmnopqrstuvwxyz_§$"; illegal = string.Empty;
            foreach (char c in name.ToLower()) if (validChar.IndexOf(c) < 0) if (illegal.IndexOf(c) < 0) illegal += c;
            return illegal == string.Empty;
        }

        public static bool IsValidFileName(string fileName)
        {
            try
            {
                new System.IO.FileInfo(fileName);
                return true;
            }
            catch { return false; }
        }

        public static bool ContainsIllegalChar(string toTest, ref string errorText, string additionallyAllowedChar = "")
        {
            string illegal = string.Empty;
            foreach (char c in toTest.ToList<char>())
                if (!EM_Helpers.IsLetterOrDigit(c) && c != '_' && !additionallyAllowedChar.Contains(c))
                    illegal += "'" + c + "' ";

            if (illegal != string.Empty)
                errorText = "Illegal character(s) " + illegal + "used";

            return illegal != string.Empty;
        }
    }
}
