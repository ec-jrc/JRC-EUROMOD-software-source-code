using EM_Common;
using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Income_List_Components
{
    /*
     * The plugin consists of two main parts: the Program part and the Metadata part. 
     * The Metadata holds basic plugin info such as its name and path.
     * The Program part is the one responsible for all the actual work the plugin does. 
     */
    public class Program : PiInterface
    {
        internal Form mainForm;                             // holds a link to the EUROMOD UI Form (used to center the input/output forms)
        private CheckCountriesYears checkForm;              // holds the Input Form
        private InputForm inputForm;                        // holds the Input Form
        private OutputForm outputForm;                      // holds the Output Form
        internal Dictionary<string, object> settings;    // holds the application settings that were passed by the EUROMOD UI 
        internal string euromodFilesPath;                   // holds the path to the folder with parameter-files (EuromodFiles_xxx) currently used by the UI
        internal string dataPath;                           // holds the chosen dataPath
        internal DataTable chkData;                         // holds the selected countries/systems
        internal List<EM_Country> countries;                // holds the extracted XML information

        // This function is responsible for running the pluggin
        public override void Run(Dictionary<string, object> _settings)
        {
            // Get necessary information form active UI
            mainForm = EM_Common_Win.UISessionInfo.GetActiveMainForm();
            euromodFilesPath = UISessionInfo.GetEuromodFilesFolder();

            var done = false;
            settings = _settings ?? new Dictionary<string, object>(); // get the application settings
            inputForm = new InputForm(this);

            // Then show the Input form until you Cancel or you manage to get some results
            while (!done && inputForm.ShowDialog(mainForm) == DialogResult.OK)
            {
                checkForm = new CheckCountriesYears(this);
                DialogResult res = checkForm.ShowDialog(mainForm);
                if (res == DialogResult.Abort) done = true;
                while (!done && res == DialogResult.OK)
                {
                    outputForm = new OutputForm(this);
                    outputForm.ShowDialog(mainForm);
                    done = true;
                }
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
                        List<EM_ILComponent> comp = expandListComponents(s, il, true);
                        il.components.Clear();
                        foreach (EM_ILComponent com in comp) if (com.addit) il.components.Add(com);
                        foreach (EM_ILComponent com in comp) if (!com.addit) il.components.Add(com);

                        il.shortComponentList = " " + String.Join(" ", getListComponents(s, il, true));
                        il.longComponentList = " " + String.Join(" ", getListComponents(s, il, false));
                    }
                }
            }
        }

        internal static List<string> getListComponents(EM_System sys, EM_IncomeList il, bool isShort)
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

        internal static List<EM_ILComponent> expandListComponents(EM_System sys, EM_IncomeList il, bool add)
        {
            List<EM_ILComponent> comp = new List<EM_ILComponent>();
            foreach (EM_ILComponent com in il.components)
            {
                if (sys.incomelists.ContainsKey(com.name))
                {
                    comp.AddRange(expandListComponents(sys, sys.incomelists[com.name], !(com.addit ^ add)));
                }
                else
                {
                    EM_ILComponent c = new EM_ILComponent() { 
                        description = com.description, 
                        name = com.name, 
                        addit = !(com.addit ^ add) 
                    };
                    comp.Add(c);
                }
            }

            return comp;
        }

        public override string GetTitle()
        {
            // This is the text you will see in the Toolbar
            return "Income List Components";
        }

        public override string GetDescription()
        {
            // This is the plugin description (will show in a tooltip when you hover the mouse over the toolbar button)
            return "This Plug-in extracts information about the components that make up each Income List.";
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
    }

    internal class EM_IncomeList
    {
        internal string name { get; set; }
        internal string description { get; set; }
        internal List<EM_ILComponent> components { get; set; }
        internal string shortComponentList { get; set; }
        internal string longComponentList { get; set; }
        internal string policy { get; set; }

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
        internal string shortname { get; set; }
        internal string name { get; set; }
        internal Dictionary<string, EM_System> systems { get; set; }

        public EM_Country()
        {
            systems = new Dictionary<string, EM_System>();
        }
    }
}
