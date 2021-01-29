using EM_Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Web_Statistics
{
    public class Program : PiInterface
    {
        internal List<string> allCountries = new List<string>(new string[] { "BE", "BG", "CZ", "DK", "DE", "EE", "IE", "EL", "ES", "FR", "IT", "CY", "LV", "LT", "LU", "HU", "HR", "NL", "MT", "AT", "PL", "PT", "RO", "SI", "SK", "FI", "SE", "UK" });
        internal Form mainForm;                             // holds a link to the EUROMOD UI Form (used to center the input/output forms)
        private InputForm inputForm;                        // holds the Input Form
        private OutputForm outputForm;                      // holds the Output Form
        internal Dictionary<string, object> settings;    // holds the application settings that were passed by the EUROMOD UI 

        internal string inputPathEUROMOD;                   // holds the chosen path for the EUROMOD files
        internal string inputPathSTATA;                     // holds the chosen path for the STATA files
        internal string outputPath;                         // holds the chosen path for the output file
        internal List<EM_Country> countries;                // holds the extracted XML information from EUROMOD
        internal string euromod_version;                    // holds the EUROMOD version from the XML

        // This function is responsible for running the pluggin
        override public void Run(Dictionary<string, object> _settings)
        {
            mainForm = EM_Common_Win.UISessionInfo.GetActiveMainForm();
            var done = false;
            settings = _settings == null ? new Dictionary<string, object>() : _settings; // get the application settings
            inputForm = new InputForm(this);
            countries = new List<EM_Country>();
            // Then show the Input form until you Cancel or you manage to get proper results
            while (!done && inputForm.ShowDialog(mainForm) == DialogResult.OK)
            {
                outputForm = new OutputForm(this);
                outputForm.ShowDialog(mainForm);
                done = true;
            }
        }

        internal void prepareIncomeListInfo()
        {
            foreach (EM_Country c in countries)
            {
                foreach (EM_System s in c.systems.Values)
                {
                    foreach (EM_IncomeList il in s.incomelists.Values)
                    {
                        List<EM_ILComponent> comp = expandListComponents(s, il);
                        il.components.Clear();
                        foreach (EM_ILComponent com in comp) if (com.addit) il.components.Add(com);
                        foreach (EM_ILComponent com in comp) if (!com.addit) il.components.Add(com);

                        il.shortComponentList = " " + String.Join(" ", getListComponents(s, il, true));
                        il.longComponentList = " " + String.Join(" ", getListComponents(s, il, false));
                        if (il.shortComponentList == " ") il.shortComponentList = " - ";
                        else if (il.shortComponentList.Substring(0, 2) == " +") il.shortComponentList = il.shortComponentList.Substring(2);
                        if (il.longComponentList == " ") il.longComponentList = " - "; 
                        else if (il.longComponentList.Substring(0, 2) == " +") il.longComponentList = il.longComponentList.Substring(2);
                    }
                    addNonSimStuff(s, "tax");
                    addNonSimStuff(s, "ben");
                }
            }
        }

        private void addNonSimStuff(EM_System s, string cn)
        {
            EM_IncomeList il = new EM_IncomeList();
            il.name = "ils_" + cn + "data";
            s.incomelists.Add(il.name, il);
            il.description = "Non-simulated taxes";
            List<EM_ILComponent> comp = new List<EM_ILComponent>();
            foreach (EM_ILComponent c in s.incomelists["ils_" + cn].components)
            {
//                if (s.incomelists["ils_" + cn + "sim"].components.FindIndex(cm => cm.name == c.name) < 0)
                if (!c.name.EndsWith(DefGeneral.POSTFIX_SIMULATED))
                    comp.Add(c);
            }
            foreach (EM_ILComponent com in comp) if (com.addit) il.components.Add(com);
            foreach (EM_ILComponent com in comp) if (!com.addit) il.components.Add(com);
            il.shortComponentList = " " + String.Join(" ", getListComponents(s, il, true));
            il.longComponentList = " " + String.Join(" ", getListComponents(s, il, false));
            if (il.shortComponentList == " ") il.shortComponentList = " - ";
            else if (il.shortComponentList.Substring(0, 2) == " +") il.shortComponentList = il.shortComponentList.Substring(2);
            if (il.longComponentList == " ") il.longComponentList = " - ";
            else if (il.longComponentList.Substring(0, 2) == " +") il.longComponentList = il.longComponentList.Substring(2);
        }

        internal List<string> getListComponents(EM_System sys, EM_IncomeList il, bool isShort)
        {
            List<string> comp = new List<string>();
            foreach (EM_ILComponent com in il.components)
            {
                if (sys.incomelists.ContainsKey(com.name))
                {
                    comp.AddRange(getListComponents(sys, sys.incomelists[com.name], isShort));
                }
                else
                {
                    comp.Add((com.addit ? "+" : "-") + " " + (isShort ? com.name.Replace("\n", "") : com.description.Replace("- ", ": ").Replace(" -", " :").Replace("\n", "")));
                }
            }
            return comp;
        }

        internal List<EM_ILComponent> expandListComponents(EM_System sys, EM_IncomeList il)
        {
            List<EM_ILComponent> comp = new List<EM_ILComponent>();
            foreach (EM_ILComponent com in il.components)
            {
                if (sys.incomelists.ContainsKey(com.name))
                {
                    comp.AddRange(expandListComponents(sys, sys.incomelists[com.name]));
                }
                else
                {
                    comp.Add(com);
                }
            }

            return comp;
        }

        public override string GetTitle()
        {
            // This is the text you will see in the Toolbar
            return "Web Statistics";
        }

        public override string GetDescription()
        {
            // This is the plugin description (will show in a tooltip when you hover the mouse over the toolbar button)
            return "This Plug-in automatically generates a Web Statistics report.";
        }

        public override string GetFullFileName()
        {
            // This is the full path and will be used to extract the image for the toolbar button
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public override bool IsWebApplicable()
        {
            return false;
        }
    }

    /* Helpful Classes */
    internal class EM_ILComponent
    {
        internal string name { get; set; }
        internal string description { get; set; }
        internal bool addit { get; set; }
        public EM_ILComponent()
        {
        }
    }

    internal class EM_IncomeList
    {
        internal string name { get; set; }
        internal string description { get; set; }
        internal List<EM_ILComponent> components { get; set; }
        internal string shortComponentList { get; set; }
        internal string longComponentList { get; set; }

        public EM_IncomeList()
        {
            components = new List<EM_ILComponent>();
        }
    }

    internal class EM_System
    {
        internal string name { get; set; }
        internal string year { get; set; }
        internal Dictionary<string, EM_IncomeList> incomelists { get; set; }

        public EM_System()
        {
            incomelists = new Dictionary<string, EM_IncomeList>();
        }
    }

    internal class EM_Country
    {
        internal string name { get; set; }
        internal string shortname { get; set; }
        internal Dictionary<string, EM_System> systems { get; set; }

        public EM_Country()
        {
            systems = new Dictionary<string, EM_System>();
        }
    }
}
