using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TomText
{
    static class Programs
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (Properties.Settings.Default.updateNeeded == true)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.updateNeeded = false;
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EditorForm());
            
        }
    }
}
