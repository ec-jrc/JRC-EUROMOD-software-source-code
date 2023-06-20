using System.Collections.Generic;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public const string uiDecimalSeparator = ".";

        public static bool TryConvertToDouble(string toConvert, out double dbl)
        {
            return double.TryParse(toConvert.Replace(uiDecimalSeparator, System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out dbl);
        }

        public static double SaveConvertToDouble(string value) { return string.IsNullOrEmpty(value) || !TryConvertToDouble(value, out double d) ? 0.0 : d; }

        public static int SaveConvertToInt(string value) { return string.IsNullOrEmpty(value) || !int.TryParse(value, out int i) ? 0 : i; }

        public static bool SaveConvertToBoolean(object value) { return value == null || !bool.TryParse(value.ToString(), out bool b) ? false : b; }

        public static string ConvertToString(double toConvert)
        {   // Apparently DataTables expect expressions to always have "." as the decimal separator...(!) this seems to be a Microsoft bug that I have not found a proper fix for
            return toConvert.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, uiDecimalSeparator);
        }

        public static bool? GetBool(string s)
        {
            if (!IsValidBool(s)) return null;
            return (new List<string>() { "1", "true", DefPar.Value.YES }).Contains(s.ToLower());
        }

        public static string AdaptDecimalSign(string formula)
        {
            char decsep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            return formula.Trim().Replace(decsep == '.' ? ',' : '.', decsep);
        }

        public static string SetCleanInvariantNumberFormat(string formula)
        {
            char decsep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            string grpsep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            formula = formula.Replace(grpsep, "");
            if (decsep != '.') formula = formula.Replace(decsep, '.');
            return formula;
        }

        public static string SetRegionalFromInvariantNumberFormat(string formula)
        {
            char decsep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            char decsepinv = System.Globalization.CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator[0];
            return formula.Replace(decsepinv, decsep);
        }
    }
}
