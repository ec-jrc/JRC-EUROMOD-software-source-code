using System.Collections.Generic;

namespace EM_Common
{
    public class DefPeriod
    {
        public const string M = "#m";
        public const string MR = "#mr";
        public const string Y = "#y";
        public const string YR = "#yr";
        public const string C = "#c";
        public const string Q = "#q";
        public const string QR = "#qr";
        public const string W = "#w";
        public const string WR = "#wr";
        public const string D = "#d";
        public const string DR = "#dr";
        public const string L = "#l";
        public const string LR = "#lr";
        public const string S = "#s";
        public const string SR = "#sr";

        /// <summary>
        /// assess the factor that needs to be applied to obtain a monthly value if a period is used, e.g. 1/12 for #y
        /// </summary>
        public static Dictionary<string, double> GetPeriods(double scale = 1)
        {
            Dictionary<string, double> periods = new Dictionary<string, double>()
            {   // note: the .0 (e.g. 1.0) is necessary to get a double otherwise one gets 0
                { DefPeriod.MR, 1.0 }, { DefPeriod.M, scale },
                { DefPeriod.C, scale }, // c=capital (just to indicate monetary amount)
                { DefPeriod.YR, 1.0 / 12.0 }, { DefPeriod.Y, scale / 12.0 }, // year
                { DefPeriod.QR, 1.0 / 3.0 }, { DefPeriod.Q, scale / 3.0 }, // quarter
                { DefPeriod.WR, 1.0 * 365.0 / 12.0 / 7.0 }, { DefPeriod.W, scale * 365.0 / 12.0 / 7.0}, // week
                { DefPeriod.DR, 1.0 * 365 / 12.0 }, { DefPeriod.D, scale * 365.0 / 12.0 }, // day
                { DefPeriod.LR, 1.0 * 365.0 / 12.0 / 7.0 * 5.0 }, { DefPeriod.L, scale * 365.0 / 12.0 / 7.0 * 5.0 }, // labour day
                { DefPeriod.SR, 1.0 * 365.0 / 12.0 / 7.0 * 6.0 }, { DefPeriod.S, scale * 365.0 / 12.0 / 7.0 * 6.0 } // labour day in a 6 days week
            };
            return periods;
        }

        public static bool EndsWithPeriod(string s, out string period)
        {
            period = string.Empty;
            foreach (string p in GetPeriods().Keys) if (s.EndsWith(p)) { period = p; return true; }
            return false;
        }

        public static bool IsAmountWithPeriod(string s) { return IsAmountWithPeriod(s, out string sDummy, out double dDummy); }
        public static bool IsAmountWithPeriod(string s, out string period, out double amount)
        {
            amount = 0;
            if (!EndsWithPeriod(s, out period)) return false;
            return double.TryParse(s.Substring(0, s.Length - period.Length), out amount);
        }
    }
}
