using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace TomText
{
    static class Programs
    {
        

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        public static void Main(string[] args)
        {
            String[] Languages = new String[] { "English", "English US", "French", "German", "Spanish" };
            string[] Icons = new string[] { "GNOME", "KDE", "Microsoft Classic", "Microsoft Modern" };
            String[] UpdateCheckStrings = new String[] { "Check on Launch (recommended)", "Check Daily", "Check Weekly", "Check Monthly (NOT RECOMMENDED)" };

            AttachConsole(ATTACH_PARENT_PROCESS);
            //Outputs application launched to debug console
            Console.WriteLine("Application Launched");
            Console.WriteLine("");
            Console.WriteLine("Welcome to TomText Debug Console");
            Console.WriteLine("This is version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //Outputs current settings to debug console
            Console.WriteLine("");
            Console.WriteLine("Current Settings:");
            Console.WriteLine("Current Language = " + Languages[Properties.Settings.Default.Language]);
            Console.WriteLine("Check for updates = " + Properties.Settings.Default.CheckForUpdates.ToString());
            Console.WriteLine("Update channel = " + Properties.Settings.Default.UpdateChannel);
            Console.WriteLine("Spell Check enabled = " + Properties.Settings.Default.SpellCheckEnabled.ToString());
            Console.WriteLine("Check spelling while typing = " + Properties.Settings.Default.DuringType.ToString());
            Console.WriteLine("Current icon set = " + Icons[Properties.Settings.Default.IconStyle]);
            Console.WriteLine("Default font = " + Properties.Settings.Default.DefaultFont.FontFamily.Name + ", " + Properties.Settings.Default.DefaultFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.DefaultFont.Style.ToString());

            if (Properties.Settings.Default.updateNeeded == true)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.updateNeeded = false;
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Properties.Settings.Default.FirstStartup == true)
            {
                Application.Run(new EndUserGUI.FirstRun());
            }
            else
            {
                Application.Run(new EditorForm());
            }           
            
        }
    }
}
