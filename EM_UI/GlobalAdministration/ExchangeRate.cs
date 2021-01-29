using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.GlobalAdministration
{
    internal class ExchangeRate
    {
        internal const string JUNE30 = "June 30";
        internal const string YEARAVERAGE = "Year Average";
        internal const string FIRSTSEMESTER = "First Semester";
        internal const string SECONDSEMESTER = "Second Semester";

        internal string Country = string.Empty;
        internal double June30 = -1;
        internal double YearAverage = -1;
        internal double FirstSemester = -1;
        internal double SecondSemester = -1;
        internal string Default = JUNE30;
        internal string ValidFor = string.Empty;

        internal ExchangeRate() { }
        internal ExchangeRate(ExchangeRatesConfig.ExchangeRatesRow rate)
        {
            Country = rate.Country; Default = rate.Default; ValidFor = rate.ValidFor;
            June30 = rate.June30; YearAverage = rate.YearAverage; FirstSemester = rate.FirstSemester; SecondSemester = rate.SecondSemester;
        }
        internal ExchangeRate(ExchangeRate rate)
        {
            Country = rate.Country; Default = rate.Default; ValidFor = rate.ValidFor;
            June30 = rate.June30; YearAverage = rate.YearAverage; FirstSemester = rate.FirstSemester; SecondSemester = rate.SecondSemester;
        }

        internal List<string> ValidForToList() { return ValidForToList(ValidFor); }
        internal static List<string> ValidForToList(string validFor)
        {
            string[] sys = validFor.Split(','); for (int i = 0; i < sys.Count(); ++i) sys[i] = sys[i].Trim().ToLower();
            return sys.ToList();
        }

        internal void AddToValidFor(string sysName) { ValidFor = AddToValidFor(ValidFor, sysName); }
        internal static string AddToValidFor(string validFor, string sysName)
        {
            validFor = RemoveFromValidFor(validFor, sysName); // to make sure that system name is not added twice
            return validFor == string.Empty ? sysName.ToLower() : validFor + ", " + sysName.ToLower();
        }

        internal void RemoveFromValidFor(string sysName) { ValidFor = RemoveFromValidFor(ValidFor, sysName); }
        internal static string RemoveFromValidFor(string validFor, string sysName)
        {
            validFor = validFor.ToLower(); sysName = sysName.ToLower();
            if (validFor == sysName) validFor = string.Empty;
            else if (validFor.StartsWith(sysName + ",") || validFor.Contains(", " + sysName + ",")) validFor = validFor.Replace(sysName + ", ", string.Empty);
            else if (validFor.EndsWith(", " + sysName)) validFor = validFor.Substring(0, validFor.Length - sysName.Length - 2);
            return validFor;
        }

        internal double DefaultRate()
        {
            switch (Default)
            {
                case FIRSTSEMESTER: return FirstSemester;
                case SECONDSEMESTER: return SecondSemester;
                case JUNE30: return June30;
                case YEARAVERAGE: return YearAverage;
                default: return -1;
            }
        }
        internal static double DefaultRate(ExchangeRatesConfig.ExchangeRatesRow row)
        {
            switch (row.Default)
            {
                case FIRSTSEMESTER: return row.FirstSemester;
                case SECONDSEMESTER: return row.SecondSemester;
                case JUNE30: return row.June30;
                case YEARAVERAGE: return row.YearAverage;
                default: return -1;
            }
        }

        internal void SetDefaultRate(double rate)
        {
            switch (Default)
            {
                case FIRSTSEMESTER: FirstSemester = rate; break;
                case SECONDSEMESTER: SecondSemester = rate; break;
                case JUNE30: June30 = rate; break;
                case YEARAVERAGE: YearAverage = rate; break;
            }
        }

        public override bool Equals(object what)
        {
            ExchangeRate erWhat = what as ExchangeRate;
            return (erWhat.Country.ToLower() == Country.ToLower() && erWhat.Default == Default &&
                    erWhat.June30 == June30 && erWhat.YearAverage == YearAverage &&
                    erWhat.FirstSemester == FirstSemester && erWhat.SecondSemester == SecondSemester);
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
