using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TomText
{
    public partial class Settings : Form
    {
        String[] Languages = new String[] { "English", "English US", "French", "German", "Spanish" };
        string[] Icons = new string[] { "GNOME", "KDE", "Microsoft Classic", "Microsoft Modern" };
        String[] UpdateCheckStrings = new String[] { "Check on Launch (recommended)", "Check Daily", "Check Weekly", "Check Monthly (NOT RECOMMENDED)" };
        
        public Settings()
        {
            InitializeComponent();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {

            }
            else
            {
                if (
                MessageBox.Show("This option will DISABLE update checking ENTIRELY! This means you will NOT receve updates! If a security patch is releaced YOU WILL NOT RECIEVE IT", 
                "WARNING", 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Warning, 
                MessageBoxDefaultButton.Button2) ==
                System.Windows.Forms.DialogResult.Cancel)
                {
                    checkBox3.Checked = true;
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            MessageBox.Show("This section is NOT FINISHED and DOES NOT WORK, this is for preview purposes only", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            tabControl1.SelectedTab = tabPage2;
            comboBox3.Text = Languages[Properties.Settings.Default.Language];
            comboBox4.Text = Icons[Properties.Settings.Default.IconStyle];
            comboBox1.Text = Languages[Properties.Settings.Default.SpellCheckLanguage];
            checkBox1.Checked = Properties.Settings.Default.SpellCheckEnabled;
            checkBox2.Checked = Properties.Settings.Default.DuringType;
            checkBox3.Checked = Properties.Settings.Default.CheckForUpdates;
            if (Properties.Settings.Default.UpdateChannel == "main")
            {
                radioButton1.Checked = true;
            }
            if (Properties.Settings.Default.UpdateChannel == "beta")
            {
                radioButton2.Checked = true;
            }
            if (Properties.Settings.Default.UpdateChannel == "alpha")
            {
                radioButton3.Checked = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                Properties.Settings.Default.UpdateChannel = "main";
                Properties.Settings.Default.Save();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                Properties.Settings.Default.UpdateChannel = "beta";
                Properties.Settings.Default.Save();
            }
        }
        
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                Properties.Settings.Default.UpdateChannel = "alpha";
                Properties.Settings.Default.Save();
            }
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
            tabPage2.Select();
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {
            tabPage2.Select();
        }

        private void tabPage5_Click(object sender, EventArgs e)
        {
            tabPage2.Select();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {
            tabPage2.Select();
        }

        private void tabPage1_Enter(object sender, EventArgs e)
        {
            
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
            
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            
        }
    }
}
