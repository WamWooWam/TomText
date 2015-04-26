using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using ConsoleControl;
using ConsoleControlAPI;

namespace TomText.DebugTools
{
    public partial class DebugConsole : Form
    {
        
        public DebugConsole()
        {
            InitializeComponent();
        }

        private void DebugConsole_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.enableDebugConsole == true)
            {
                Properties.Settings.Default.enableDebugConsole = false;
                Properties.Settings.Default.Save();
                consoleControl1.StartProcess(System.Reflection.Assembly.GetEntryAssembly().Location, "");
                Properties.Settings.Default.enableDebugConsole = true;
            }
        }

        private void reporter_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void DebugConsole_MouseEnter(object sender, EventArgs e)
        {
            Form form = new DebugTools.DebugConsole();
            form.Opacity = 25;
        }

        private void DebugConsole_MouseLeave(object sender, EventArgs e)
        {
            Form form = new DebugTools.DebugConsole();
            form.Opacity = 100;
        }

        private void DebugConsole_FormClosed(object sender, FormClosedEventArgs e)
        {
            consoleControl1.StopProcess();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.enableDebugConsole == true)
            {
                Properties.Settings.Default.enableDebugConsole = false;
                Properties.Settings.Default.Save();
                consoleControl1.StartProcess(System.Reflection.Assembly.GetEntryAssembly().Location, "");
                Properties.Settings.Default.enableDebugConsole = true;
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var current = Process.GetCurrentProcess();
            Process.GetProcessesByName("TomText.exe")
                .Where(t => t.Id != current.Id)
                .ToList()
                .ForEach(t => t.Kill());

            
        }
    }
}
