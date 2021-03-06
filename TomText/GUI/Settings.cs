﻿using System;
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
        private TreeNode unofficialNode;
        private TreeNode officialNode;
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
            JObject themeinfo;
            if (Properties.Settings.Default.Theme == "embedded")
            {
                themeinfo = JObject.Parse(Properties.Resources.embedded);
            }
            else
            {
                themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            }
            checkBox1.Checked = Properties.Settings.Default.HiDPI;
            richTextBox1.Text = themeinfo.GetValue("Theme Description").ToString() + "\r\n" + themeinfo.GetValue("Theme Licence").ToString();
            textBox1.Text = Properties.Settings.Default.UIFont.FontFamily.Name + ", " + Properties.Settings.Default.UIFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.UIFont.Style.ToString();
            textBox2.Text = Properties.Settings.Default.DocFont.FontFamily.Name + ", " + Properties.Settings.Default.DocFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.DocFont.Style.ToString();
            directoryTextBox.Text = themepath;
            JObject embedded = JObject.Parse(Properties.Resources.embedded);
            comboBox1.Items.Add(embedded.GetValue("Theme Name").ToString());
            foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\"))
            {
                if (File.Exists(dir + @"\theme.json"))
                {
                    JObject theme = JObject.Parse(File.ReadAllText(dir + @"\theme.json"));
                    comboBox1.Items.Add(theme.GetValue("Theme Name").ToString());
                }
            }
            officialNode = treeView1.Nodes.Add("Official");
            foreach (string repo in Properties.Settings.Default.UpdateRepos)
            {
                officialNode.Nodes.Add(repo);
            }
            
            unofficialNode = treeView1.Nodes.Add("Unofficial");
            foreach (string repo in Properties.Settings.Default.CustomUpdateRepos)
            {
                unofficialNode.Nodes.Add(repo);
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
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme;
            }
            JObject themeinfo;
                if (Properties.Settings.Default.Theme == "embedded")
                {
                    themeinfo = JObject.Parse(Properties.Resources.embedded);
                }
                else
                {
                    themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                }
                Properties.Settings.Default.Theme = comboBox1.Text;
                Properties.Settings.Default.Save();
                richTextBox1.Text = themeinfo.GetValue("Theme Description").ToString() + "\r\n" +
                                    themeinfo.GetValue("Theme Licence").ToString();
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
                RefreshGUI();
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
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme + @"\Hi-DPI";
                if (File.Exists(themepath + @"\theme.json"))
                {

                }
                else
                {
                    themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                                Properties.Settings.Default.Theme;
                    checkBox1.Checked = false;
                }

            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme;
                JObject themeinfo;
                if (Properties.Settings.Default.Theme == "embedded")
                {
                    themeinfo = JObject.Parse(Properties.Resources.embedded);
                }
                else
                {
                    themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                }
                string imgtype;
                try
                {
                    imgtype = themeinfo.GetValue("Image Type").ToString();
                }
                catch
                {
                    imgtype = ".png";
                }
                if (
                    File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                                Properties.Settings.Default.Theme + @"\Hi-DPI\theme.json"))
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
                                    imageList1.TransparentColor =
                                        Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                                }
                            }
                            catch
                            {
                            }
                            item.ImageIndex = no;
                            no = no + 1;
                        }
                    }
                    string name;
                    foreach (ToolStrip strip in panel1.Controls.OfType<ToolStrip>())
                    {
                        foreach (ToolStripButton item in strip.Items.OfType<ToolStripButton>())
                        {
                            name = item.Text;
                            name = name.Replace("&", "");
                            name = name.Replace(" ", "-");
                            name = name.Replace(".", "");
                            try
                            {
                                if (Properties.Settings.Default.Theme == "embedded")
                                {

                                }
                                else
                                {
                                    item.Image = Image.FromFile(themepath + @"\" + name.ToLower() + imgtype);
                                    try
                                    {
                                        if (Convert.ToBoolean(themeinfo.GetValue("Auto-scale images?".ToString())))
                                        {
                                            item.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                                        }
                                        else
                                        {
                                            item.ImageScaling = ToolStripItemImageScaling.None;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    try
                                    {
                                        if (imgtype == ".bmp")
                                        {
                                            item.ImageTransparentColor =
                                                Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                item.Font = Properties.Settings.Default.UIFont;
                            }
                            catch
                            {
                            }
                        }
                        foreach (ToolStripDropDownButton dd in strip.Items.OfType<ToolStripDropDownButton>())
                        {
                            dd.Font = Properties.Settings.Default.UIFont;
                            if (dd.HasDropDownItems == true)
                            {
                                foreach (ToolStripMenuItem item in dd.DropDownItems.OfType<ToolStripMenuItem>())
                                {
                                    name = item.Text;
                                    name = name.Replace("&", "");
                                    name = name.Replace(" ", "-");
                                    name = name.Replace(".", "");
                                    try
                                    {
                                        if (Properties.Settings.Default.Theme == "embedded")
                                        {
                                        }
                                        else
                                        {
                                            try
                                            {
                                                item.Image = Image.FromFile(themepath + @"\" + name.ToLower() + imgtype);
                                                try
                                                {
                                                    if (imgtype == ".bmp")
                                                    {
                                                        item.ImageTransparentColor =
                                                            Color.FromName(
                                                                themeinfo.GetValue("BMP Back Colour").ToString());
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                                ;
                                                try
                                                {
                                                    if (
                                                        Convert.ToBoolean(
                                                            themeinfo.GetValue("Auto-scale images?".ToString())))
                                                    {
                                                        item.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                                                    }
                                                    else
                                                    {
                                                        item.ImageScaling = ToolStripItemImageScaling.None;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        item.Font = Properties.Settings.Default.UIFont;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private
            void checkBox1_CheckedChanged(object sender, EventArgs e)
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
            JObject themeinfo;
            if (Properties.Settings.Default.Theme == "embedded")
            {
                themeinfo = JObject.Parse(Properties.Resources.embedded);
            }
            else
            {
                themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            }
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
              GUI.ReqURL url = new GUI.ReqURL("repo",true,true);
                if (url.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.CustomUpdateRepos.Add(url.url);
                    unofficialNode.Nodes.Add(url.url);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Nodes.Count == 0)
                {
                    if (treeView1.SelectedNode.Text != "Unofficial")
                    {
                        Properties.Settings.Default.CustomUpdateRepos.Remove(treeView1.SelectedNode.Text);
                        treeView1.Nodes.Remove(treeView1.SelectedNode);
                    }
                }
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        // Paint background with underlying graphics from other controls
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;

            if (Parent != null)
            {
                // Take each control in turn
                int index = Parent.Controls.GetChildIndex(this);
                for (int i = Parent.Controls.Count - 1; i > index; i--)
                {
                    Control c = Parent.Controls[i];

                    // Check it's visible and overlaps this control
                    if (c.Bounds.IntersectsWith(Bounds) && c.Visible)
                    {
                        // Load appearance of underlying control and redraw it on this background
                        Bitmap bmp = new Bitmap(c.Width, c.Height, g);
                        c.DrawToBitmap(bmp, c.ClientRectangle);
                        g.TranslateTransform(c.Left - Left, c.Top - Top);
                        g.DrawImageUnscaled(bmp, Point.Empty);
                        g.TranslateTransform(Left - c.Left, Top - c.Top);
                        bmp.Dispose();
                    }
                }
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            FontDialog fonts = new FontDialog();
            fonts.Font = Properties.Settings.Default.DocFont;
            fonts.ShowEffects = true;
            fonts.ShowColor = true;
            if (fonts.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.DocFont = fonts.Font;
                Properties.Settings.Default.Save();
                textBox2.Text = Properties.Settings.Default.DocFont.FontFamily.Name + ", " + Properties.Settings.Default.DocFont.SizeInPoints.ToString() + "pt, " + Properties.Settings.Default.DocFont.Style.ToString();
                RefreshGUI();
            }
        }
    }
}
