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
        public string url { get; set; }
        private bool isRepo;
        public ReqURL(string s, bool v, bool repo)
        {
            InitializeComponent();
            Text = "Enter " + s + " URL";
            label1.Text = "Enter or paste " + s + " URL";
            verifyurl = v;
            isRepo = repo;
            string themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            JObject themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            if (themeinfo != null && themeinfo.GetValue("Changes icons?").ToString() == "true")
            {
                if (repo)
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
                    Uri lulz = new Uri(textBox1.Text);
                    if (isRepo)
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            JObject update = JObject.Parse(client.DownloadString(new Uri(textBox1.Text + "repo.json")));
                            JToken testJToken;
                            if (update.TryGetValue("Repo name", out testJToken))
                            {
                                if (update.TryGetValue("Repo description", out testJToken))
                                {
                                    if (update.TryGetValue("Repo latest", out testJToken))
                                    {
                                        DialogResult result = MessageBox.Show(
                                            "Are you sure you want to add '" + update.GetValue("Repo name") +
                                            "', with the URL '" + textBox1.Text + "' to the repo list?", "Add repo?",
                                            MessageBoxButtons.YesNoCancel);
                                        if (result == DialogResult.Yes)
                                        {
                                            DialogResult = DialogResult.OK;
                                            url = textBox1.Text;
                                            Close();
                                        }
                                        else
                                        {
                                            if (result == DialogResult.No)
                                            {

                                            }
                                            else
                                            {
                                                if (result == DialogResult.Cancel)
                                                {
                                                    DialogResult = DialogResult.Abort;
                                                    Close();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show(
                                            "This doesn't seem to be a valid repo. Check the URL and try again.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("This doesn't seem to be a valid repo. Check the URL and try again.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("This doesn't seem to be a valid repo. Check the URL and try again.");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("This doesn't seem to be a valid repo. Check the URL and try again.");
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("This isn't a valid URL, make sure you're including http://");
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
