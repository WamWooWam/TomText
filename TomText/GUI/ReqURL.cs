using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TomText.GUI
{
    public partial class ReqURL : Form
    {
        private bool verifyurl;
        public ReqURL(string s, bool v)
        {
            InitializeComponent();
            Text = "Enter " + s + " URL";
            label1.Text = "Enter or paste " + s + " URL";
            verifyurl = v;
            string themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            if (themeinfo != null && themeinfo.GetValue("Changes icons?").ToString() == "true")
            {
                if (s == "repo")
                {
                    this.Icon =
                        new Icon(Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" +
                                 Properties.Settings.Default.Theme + @"/repo.ico");
                }
                else
                {
                    this.Icon =
                        new Icon(Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" +
                                 Properties.Settings.Default.Theme + @"/net.ico");
                }
            }
        }

        private void ReqURL_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (verifyurl)
            {
                try
                {
                    //WebClient client = new WebClient();
                    //client.DownloadFile(textBox1.Text + "repo.json",Path.GetTempPath() + "/repo.json");
                    JObject update = JObject.Parse(File.ReadAllText(Path.GetTempPath() + "/repo.json"));
                    GUI.VerifyRepo repo = new GUI.VerifyRepo();
                    repo.ShowDialog();
                }
                catch (Exception)
                {
                    MessageBox.Show("This doesn't seem to be a valid repo. Check the URL and try again.");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
