using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TomText.GUI
{
    public partial class DevConsole : Form
    {
        public DevConsole()
        {
            InitializeComponent();
        }

        private void DevConsole_Load(object sender, EventArgs e)
        {

        }

        private void DevConsole_Shown(object sender, EventArgs e)
        {
            Form editor = new EditorForm();
            editor.Show();
        }

        static public void PrintToBox(string print)
        {
            
        }
    }
}
