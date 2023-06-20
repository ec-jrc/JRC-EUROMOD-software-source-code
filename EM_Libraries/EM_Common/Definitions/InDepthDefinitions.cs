using System;
using System.Collections.Generic;
using System.Text;

namespace EM_Common
{
    public class InDepthDefinitions
    {
        public const char SEPARATOR = '°';
        public const char SEPARATOR_INNER = '|';

        public const string ILS_ORIGY = "ils_origy";
        public const string ILS_DISPY = "ils_dispy";
        public const string ILS_EARNS = "ils_earns";
        public const string ILS_TAX = "ils_tax";
        public const string ILS_SICDY = "ils_sicdy";
        public const string ILS_SICER = "ils_sicer";
        public const string ILS_SICCT = "ils_sicct";
        public const string ILS_SICOT = "ils_sicot";
        public const string ILS_SICDYERCT = "ils_sic";
        public const string ILS_PEN = "ils_pen";
        public const string ILS_BEN = "ils_ben";
        public const string ILS_BENMT = "ils_benmt";
        public const string ILS_BENNT = "ils_bennt";
        public const string ILS_SICEE = "ils_sicee";
        public const string ILS_SICSE = "ils_sicse";
        public const string ILS_EXTSTAT_OTHER = "ils_extstat_other";
        public const string OtherOriginalIncome = "OtherOriginalIncome";
        public static string[] ILS_SICDYERCT_LIST = new string[] { ILS_SICDY, ILS_SICER, ILS_SICCT };

        public static readonly List<string> ILS_ALL = new List<string>() { ILS_ORIGY, ILS_PEN, ILS_BENMT, ILS_BENNT, ILS_TAX, ILS_SICEE, ILS_SICSE, ILS_SICOT, ILS_SICER, ILS_SICCT, ILS_EXTSTAT_OTHER };

        // The hardcoded rows for the Distributional (Inequality & Poverty) tables
        public static List<List<string>> ROWS_INEQUALITY = new List<List<string>>() { new List<string>() { "D1", "Decile 1" }, new List<string>() { "D2", "Decile 2" }, new List<string>() { "D3", "Decile 3" },
                                    new List<string>() { "D4", "Decile 4" }, new List<string>() { "D5", "Decile 5" }, new List<string>() { "D6", "Decile 6" }, new List<string>() { "D7", "Decile 7" },
                                    new List<string>() { "D8", "Decile 8" }, new List<string>() { "D9", "Decile 9" }, new List<string>() { "D10", "Decile 10" }, new List<string>() { "Median", "Median" },
                                    new List<string>() { "Mean", "Mean" }, new List<string>() { "Gini", "Gini" }, new List<string>() { "S8020", "S80/S20" } };

        public static List<List<string>> ROWS_POVERTY = new List<List<string>>() { new List<string>() { "Total40", "40% Median - Total" }, new List<string>() { "Male40", "40% Median - Males" }, new List<string>() { "Female40", "40% Median - Females" },
                                    new List<string>() { "Total50", "50% Median - Total" }, new List<string>() { "Male50", "50% Median - Males" }, new List<string>() { "Female50", "50% Median - Females" },
                                    new List<string>() { "Total60", "60% Median - Total" }, new List<string>() { "Male60", "60% Median - Males" }, new List<string>() { "Female60", "60% Median - Females" },
                                    new List<string>() { "Total70", "70% Median - Total" }, new List<string>() { "Male70", "70% Median - Males" }, new List<string>() { "Female70", "70% Median - Females" },
                                    new List<string>() { "Age0_15", "60% Median - 0-15 years" }, new List<string>() { "Age16_24", "60% Median - 16-24 years" }, new List<string>() { "Age25_49", "60% Median - 25-49 years" },
                                    new List<string>() { "Age50_64", "60% Median - 50-64 years" }, new List<string>() { "Age65", "60% Median - 65+ years" } };

        public const string DESTINATION_NONE = "None";
        public const string DESTINATION_ORIGINAL_INCOME = "OriginalIncome";
        public const string DESTINATION_TAXES_SIC = "TaxesSic";
        public const string DESTINATION_BENEFITS = "Benefits";
        public const string DESTINATION_INEQUALITY = "Inequality";
        public const string DESTINATION_POVERTY = "Poverty";
        public static readonly List<string> DestinationTables = new List<string>() { DESTINATION_NONE, DESTINATION_ORIGINAL_INCOME, DESTINATION_TAXES_SIC, DESTINATION_BENEFITS };
        public static readonly Dictionary<string, string> IncomelistDefaultDestination = new Dictionary<string, string>()
        {
            { ILS_ORIGY, DESTINATION_ORIGINAL_INCOME }, { OtherOriginalIncome, DESTINATION_ORIGINAL_INCOME }, { ILS_EARNS, DESTINATION_ORIGINAL_INCOME }, { ILS_PEN, DESTINATION_BENEFITS }, { ILS_BENMT, DESTINATION_BENEFITS }, { ILS_BENNT, DESTINATION_BENEFITS }, { ILS_TAX, DESTINATION_TAXES_SIC }, {ILS_SICEE, DESTINATION_TAXES_SIC }, {ILS_SICSE, DESTINATION_TAXES_SIC }, {ILS_SICOT, DESTINATION_TAXES_SIC }, {ILS_SICER, DESTINATION_TAXES_SIC }, {ILS_SICCT, DESTINATION_TAXES_SIC }, { ILS_EXTSTAT_OTHER, DESTINATION_NONE }
        };

    }
}
