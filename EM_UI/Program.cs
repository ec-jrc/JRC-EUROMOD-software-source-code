using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EM_UI_MainForm mainForm = null;
            if (args.Length == 1)
            {
                mainForm = new EM_UI_MainForm(args[0]);
            }
            else
            {
                mainForm = new EM_UI_MainForm(); 
            }

            EM_AppContext appContext = EM_AppContext.Create_EM_AppContext(mainForm);

            Application.Run(appContext);
        }
    }
}
