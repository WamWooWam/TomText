using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace TomText.GUI
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            
            comboBox1.Text = Properties.Settings.Default.Theme;
            string themepath;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            }
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            checkBox1.Checked = Properties.Settings.Default.HiDPI;
            richTextBox1.Text = themeinfo.GetValue("Theme Description").ToString() + "\r\n" + themeinfo.GetValue("Theme Licence").ToString();
            textBox1.Text = Properties.Settings.Default.UIFont.FontFamily.Name + ", " + Properties.Settings.Default.UIFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.UIFont.Style.ToString();
            directoryTextBox.Text = themepath;
            foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\"))
            {
                if (File.Exists(dir + @"\theme.json"))
                {
                    JObject theme = JObject.Parse(File.ReadAllText(dir + @"\theme.json"));
                    comboBox1.Items.Add(theme.GetValue("Theme Name").ToString());
                }
            }
            
            RefreshGUI();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            string themepath;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            }
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            Properties.Settings.Default.Theme = comboBox1.Text;
            Properties.Settings.Default.Save();
            richTextBox1.Text = themeinfo.GetValue("Theme Description").ToString() + "\r\n" + themeinfo.GetValue("Theme Licence").ToString();
            directoryTextBox.Text = themepath;
            RefreshGUI();
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Properties.Settings.Default.Theme == "embedded")
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FontDialog fonts = new FontDialog();
            fonts.Font = Properties.Settings.Default.UIFont;
            fonts.ShowEffects = true;
            fonts.ShowColor = true;
            if(fonts.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.UIFont = fonts.Font;
                Properties.Settings.Default.Save();
                textBox1.Text = Properties.Settings.Default.UIFont.FontFamily.Name + ", " + Properties.Settings.Default.UIFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.UIFont.Style.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
        }
        private void RefreshGUI()
        {
            Image ico;
            int no = 0;
            imageList1.Images.Clear();
            string themepath;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            }
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            string imgtype;
            try
            {
                 imgtype = themeinfo.GetValue("Image Type").ToString();
            }
            catch
            {
                imgtype = ".png";
            }
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI\theme.json"))
            {
                checkBox1.Enabled = true;
            }
            else
            {
                checkBox1.Enabled = false;
            }
            checkBox1.Checked = Properties.Settings.Default.HiDPI;
            if (themeinfo.GetValue("Changes icons?").ToString() == "true")
            {
                Icon = new Icon(themepath + @"\settings.ico");
            }
            if (themeinfo.GetValue("Theme Name").ToString() == "embedded")
            {
                Console.WriteLine("Using embedded theme. Skipping initialisation");
            }
            else
            {
                foreach (TabControl control in this.Controls.OfType<TabControl>())
                {
                    foreach (TabPage item in control.TabPages.OfType<TabPage>())
                    {
                        try
                        {
                            ico = Image.FromFile(themepath + @"\" + item.Text + imgtype);
                            imageList1.Images.Add(item.Text, ico);
                            if (themeinfo.GetValue("Image Type").ToString() == ".bmp")
                            {
                                imageList1.TransparentColor = Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                            }
                        }
                        catch { }
                        item.ImageIndex = no;
                        no = no + 1;
                    }
                }
            } 
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.HiDPI = checkBox1.Checked;
        }

        private void checkBox1_EnabledChanged(object sender, EventArgs e)
        {
            if (checkBox1.Enabled == false)
            {
                checkBox1.Checked = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comboBox1.Text = Properties.Settings.Default.Theme;
            comboBox1.Items.Clear();
            string themepath;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            }
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            checkBox1.Checked = Properties.Settings.Default.HiDPI;
            richTextBox1.Text = themeinfo.GetValue("Theme Description").ToString() + "\r\n" + themeinfo.GetValue("Theme Licence").ToString();
            textBox1.Text = Properties.Settings.Default.UIFont.FontFamily.Name + ", " + Properties.Settings.Default.UIFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.UIFont.Style.ToString();
            directoryTextBox.Text = themepath;
            foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\"))
            {
                if (File.Exists(dir + @"\theme.json"))
                {
                    JObject theme = JObject.Parse(File.ReadAllText(dir + @"\theme.json"));
                    comboBox1.Items.Add(theme.GetValue("Theme Name").ToString());
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Unofficial repos are not managed by us and may contain malware. We are not responsible for the content of other repos. Do you still want to add an unofficial repo?","Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
              GUI.ReqURL url = new GUI.ReqURL("repo",true);
                url.ShowDialog();
            }
        }

    }
}
