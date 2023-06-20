using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public class ExeXml
    {
        public class Sys
        {
            public string id = string.Empty;
            public string name = string.Empty;
            public bool isOutputEuro = true;
            public bool areParamEuro = true;
            public string headDefInc = string.Empty;
            public string year = string.Empty;
        }

        public class Pol
        {
            public string name = string.Empty;
            public double order = -1.0;
            public bool on = false; // true: on, false: off, note: policies switched to n/a are ignored by the reading-procedure
            public string comment = string.Empty; // note that comments are only read on "demand" (e.g. for LightMaker)
            public Dictionary<string, Fun> funs = new Dictionary<string, Fun>(StringComparer.InvariantCultureIgnoreCase); // key: function-id
        }

        public class Fun
        {
            public string _name = string.Empty;
            public string Name { get { return _name; } set { _name = EM_Helpers.RemoveWhitespace(value); } }
            public double order = -1.0;
            public bool on = false; // see above
            public string comment = string.Empty; // see above
            public Dictionary<string, Par> pars = new Dictionary<string, Par>(StringComparer.InvariantCultureIgnoreCase); // key: parameter-id
        }

        public class Par
        {
            public string _name = string.Empty;
            public string Name { get { return _name; } set { _name = EM_Helpers.RemoveWhitespace(value); } }
            public string _group = string.Empty;
            public string Group { get { return _group; } set { _group = value.Trim(); } }
            public string order = string.Empty; // that's just for error-messages (order of parameters is irrelevant for executable)
            public string val = string.Empty; // note: handling of whitespaces is done in ExeXmlReader.GetParInfo
            public string comment = string.Empty; // see above
        }

        public class Data // remark: this is all stored in <Dataset>, as <SysData> actually does not contain info for the executable
        {
            private string _name = string.Empty;
            public string Name
            {
                get { return _name; }
                set { if (value == null) return; _name = value.ToLower().EndsWith(".txt") ? value : value + ".txt"; }
            }
            public string year = string.Empty; // of income
            public bool isEuro = true;
            public bool useCommonDefault = false; // true: assume all variables defined in variables file are available (set 0 if not in data)
            public bool readXVar = false; // true read x#, p# and q# variables
            public string specialPath = null;
            public string listStringOutVar = string.Empty; // variables which are supposed to exist in input-data and are transferred to output (separated by space)
            public string indirectTaxTableYear = string.Empty; // relevant year in the indirect-tax-table
        }

        public class UpIndDict
        {
            private Dictionary<string, double?> allYears = new Dictionary<string, double?>();
            public void SetYear(string year, double val)
            {
                // This should never really happen, but if for some reason there are duplicate uprating factors for the same year & var, keep the last one
                if (allYears.ContainsKey(year))
                    allYears[year] = val;
                else
                    allYears.Add(year, val);
            }
            public bool Get(string year, out double ui)
            {
                bool allFine = allYears.ContainsKey(year) && allYears[year] != null;
                ui = allFine ? (double)allYears[year] : 1;  // Note that if get fails, the default returned is 1!
                return allFine;
            }
            public Dictionary<string, double> GetAll()
            {
                Dictionary<string, double> allValidFactors = new Dictionary<string, double>();
                foreach (KeyValuePair<string, double?> item in allYears)
                    if (item.Value != null) allValidFactors.Add(item.Key, (double)item.Value);
                return allValidFactors;
            }
        }

        public class ExStatDict
        {
            private bool isAggregate = false; 
            private Dictionary<string, double?> allYearsValues = new Dictionary<string, double?>();
            private Dictionary<string, double?> allYearsNumbers = new Dictionary<string, double?>();
            private Dictionary<string, string> allYearsLevels = new Dictionary<string, string>();

            public ExStatDict(bool isAggregate = false)
            {
                this.isAggregate = isAggregate;
            }

            public bool IsAggregate()
            {
                return isAggregate;
            }

            public void SetYearValues(string year, double num, double amn)
            {
                SetYearNumber(year, num);
                SetYearAmount(year, amn);
//                SetYearLevel(year, "Individual");
            }

            public void SetYearAmount(string year, double val)
            {
                // This should never really happen, but if for some reason there are duplicate values for the same year & var, keep the last one
                if (allYearsValues.ContainsKey(year))
                    allYearsValues[year] = val;
                else
                    allYearsValues.Add(year, val);
            }
            public void SetYearNumber(string year, double val)
            {
                // This should never really happen, but if for some reason there are duplicate values for the same year & var, keep the last one
                if (allYearsNumbers.ContainsKey(year))
                    allYearsNumbers[year] = val;
                else
                    allYearsNumbers.Add(year, val);
            }
            /*
            public void SetYearLevel(string year, string val)
            {
                // This should never really happen, but if for some reason there are duplicate values for the same year & var, keep the last one
                if (allYearsLevels.ContainsKey(year))
                    allYearsLevels[year] = val;
                else
                    allYearsLevels.Add(year, val);
            }*/
            
            // return the real amount, so value in table * 1000000
            public bool GetAmount(string year, out double ui)
            {
                bool allFine = allYearsValues.ContainsKey(year) && allYearsValues[year] != null;
                ui = allFine ? (double)allYearsValues[year] : double.NaN;  // Note that if get fails, the default returned is NaN!
                return allFine;
            }
            // return the real number, so value in table * 1000
            public bool GetNumber(string year, out double ui)
            {
                bool allFine = allYearsNumbers.ContainsKey(year) && allYearsNumbers[year] != null;
                ui = allFine ? (double)allYearsNumbers[year] : double.NaN;  // Note that if get fails, the default returned is NaN!
                return allFine;
            }
            /*
            public bool GetLevel(string year, out string ui)
            {
                bool allFine = allYearsLevels.ContainsKey(year) && allYearsLevels[year] != null;
                ui = allFine ? allYearsLevels[year] : null;  // Note that if get fails, the default returned is null!
                return allFine;
            }*/
            public Dictionary<string, double> GetAllAmounts()
            {
                Dictionary<string, double> allValidValues = new Dictionary<string, double>();
                foreach (KeyValuePair<string, double?> item in allYearsValues)
                    if (item.Value != null) allValidValues.Add(item.Key, (double)item.Value);
                return allValidValues;
            }
            public Dictionary<string, double> GetAllNumbers()
            {
                Dictionary<string, double> allValidValues = new Dictionary<string, double>();
                foreach (KeyValuePair<string, double?> item in allYearsNumbers)
                    if (item.Value != null) allValidValues.Add(item.Key, (double)item.Value);
                return allValidValues;
            }
            /*
            public Dictionary<string, string> GetAllLevels()
            {
                Dictionary<string, string> allValidValues = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> item in allYearsLevels)
                    if (item.Value != null) allValidValues.Add(item.Key, item.Value);
                return allValidValues;
            }*/
        }

        public class Extension
        {
            public bool? on = null;
            public string localShortName = null, localLongName = null; // this is only set for local extensions and stays empty for global extensions
            public Dictionary<string, bool> polIds = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase); // key: id of pol/fun which are switched on or off
            public Dictionary<string, bool> funIds = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase); // value: baseOff (see HandleExtensions in exe for description)
            public Dictionary<string, bool> parIds = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        }

        public class CAO // contains the common part of Country and AddOn
        {
            public string shortName = string.Empty;
            public Dictionary<string, Pol> pols = new Dictionary<string, Pol>(StringComparer.InvariantCultureIgnoreCase); // content of the relevant system (key: policy-id)
        }

        public class Country
        {
            public Sys sys = new Sys();                          // info upon the relevant system
            public Data data = new Data();                       // info upon the relevant dataset
            public CAO cao = new CAO();                          // see above
            public Dictionary<string, UpIndDict> upFacs =        // uprating-factors for relevant data- and system-year (key: factor name)
                new Dictionary<string, UpIndDict>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, double> indTaxes =         // indirect-tax-table for relevant year, as indicated in data's indirectTaxTableYear (key: name)
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, Extension> extensions =
                new Dictionary<string, Extension>(StringComparer.InvariantCultureIgnoreCase); // key: ID (for e.g. BTA or any ctry-specific extension)
            public Dictionary<string, ExStatDict> extStats = //External Statistics
                new Dictionary<string, ExStatDict>(StringComparer.OrdinalIgnoreCase);
        }

        public class AddOn
        {
            public const string POL_AO_CONTROL = "ao_control_";
            public const string PAR_APPLIC_SYS = "sys";

            public List<string> applicSys = new List<string>();  // systems the add-on-sys can be applied on (as defined by AddOn_Applic-function)
            public Pol polAOControl = null;                      // the ao_control-policy (contains the AddOn_Pol/Fun/Par-functions)
            public CAO cao = new CAO();                          // see above
        }
    }
}
