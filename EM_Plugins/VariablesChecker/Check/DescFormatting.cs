using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VariablesChecker
{
    internal partial class Check
    {
        internal static string DescFormatting(List<string> print)
        {
            bool correct = false;
            DialogResult answer = MessageBox.Show("Correct any rule violations?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel) return "Action canceled";
            correct = answer == DialogResult.Yes;

            foreach (var v in VariablesChecker.varData.Variable)
                foreach (var cd in v.GetCountryLabelRows())
                    cd.Label = DescFormatting(cd.Label, string.Format(string.Format("variable {0} description for {1}", v.Name, cd.Country)), correct, print, true);

            foreach (var t in VariablesChecker.varData.AcronymType)
                t.LongName = DescFormatting(t.LongName, string.Format(string.Format("description for type {0}", t.ShortName.ToUpper())), correct, print);

            foreach (var l in VariablesChecker.varData.AcronymLevel)
                l.Name = DescFormatting(l.Name, string.Format(string.Format("description for acronym-level of type {0}", l.AcronymTypeRow.ShortName.ToUpper())), correct, print);

            foreach (var a in VariablesChecker.varData.Acronym)
                a.Description = DescFormatting(a.Description, string.Format(string.Format("description for acronym {0} in {1}-{2}",
                                a.Name, a.AcronymLevelRow.AcronymTypeRow.ShortName.ToUpper(), a.AcronymLevelRow.Name)), correct, print);

            foreach (var c in VariablesChecker.varData.Category)
            {
                c.Description = DescFormatting(c.Description, string.Format(string.Format("category of acronym {0} in {1}-{2}",
                                c.AcronymRow.Name, c.AcronymRow.AcronymLevelRow.AcronymTypeRow.ShortName.ToUpper(), c.AcronymRow.AcronymLevelRow.Name)), correct, print);
            }

            if (print.Count == 0) return "No description formatting problems found";

            int nProblems = print.Count;
            print.Insert(0, "______________________________________________________________________");
            print.Insert(0, "whitespaces indicated as follows: blank °, tabulator %, newline #");
            print.Insert(0, "E: empty (note, empty country description for variable should be indicated by a dash)");
            print.Insert(0, "W: contains tabulator or newline, B: unnecessary blanks, Q: contains quotes");
            print.Insert(0, "L E G E N D :");

            if (correct) VariablesChecker.SaveVar();

            return nProblems.ToString() + " description formatting problems found" + (correct ? " and corrected" : string.Empty);
        }

        private static string DescFormatting(string what, string where, bool correct, List<string> print, bool isCountryLabel = false)
        {
            string bkupWhat = what, showWhat = what.Replace(" ", "°").Replace("\t", "%").Replace(Environment.NewLine, "#");

            string faults = string.Empty;
            if (WhitspaceFault(ref what, correct)) faults += "W ";
            if (QuotesFault(ref what, correct)) faults += "Q ";
            if (BlankFault(ref what, correct)) faults += "B ";
            if (EmptyFault(ref what, correct & isCountryLabel)) faults += "E ";
            if (faults != string.Empty) print.Add(string.Format("({0}) {1}{2}[{3}]", faults.TrimEnd(), showWhat, faults.Contains("E") ? "" : "   ", where));

            return correct ? what : bkupWhat;
        }

        private static bool BlankFault(ref string d, bool correct)
        {
            if (d.Length == d.Trim().Replace("  ", "").Length) return false;
            if (correct)
            {
                d = d.Trim(); string d2;
                do { d2 = d; d = d.Replace("  ", " "); } while (d2.Length != d.Length);
            }
            return true;
        }

        private static bool WhitspaceFault(ref string d, bool correct)
        {
            if (!d.Contains("\t") && !d.Contains("\n") && !d.Contains("\r")) return false;
            if (correct) d = d.Replace("\t", " ").Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", "");
            return true;
        }
        private static bool QuotesFault(ref string d, bool correct)
        {
            if (!d.Contains("\"")) return false;
            if (correct) d = d.Replace("\"", "");
            return true;
        }
        private static bool EmptyFault(ref string d, bool correct)
        {
            if (d.Trim() != string.Empty) return false;
            if (correct) d = "-";
            return true;
        }
    }
}
