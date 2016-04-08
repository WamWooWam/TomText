using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace TomText.GUI
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            JObject themeinfo;
            string themepath;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme;
            }
            try
            {
                if (Properties.Settings.Default.Theme == "embedded")
                {
                    themeinfo = JObject.Parse(Properties.Resources.embedded);
                    if (themeinfo != null && themeinfo.GetValue("Changes icons?").ToString() == "true") ;
                }
                else
                {
                    themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                }
                {
                    this.Icon =
                        new Icon(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                                 Properties.Settings.Default.Theme + @"\help.ico");
                }

            }
            catch
            {
            }
            geckoWebBrowser1.Navigate("http://html5test.com");
        }

        private void Help_FormClosed(object sender, FormClosedEventArgs e)
        {
            geckoWebBrowser1.Dispose();
        }
    }
}
