using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace TomText
{
    public partial class EditorForm : Form
    {
        bool edited = false;
        string openfile = "Untitled";
        bool fileopened = false;
        public EditorForm()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = (openfile + " | Tom Text");
            richTextBox1.Font = Properties.Settings.Default.DefaultFont;
            timer1.Start();
            timer2.Start();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new TTAbout();
            about.Show();
        }

        private void New(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            edited = false;
            fileopened = false;
            openfile = "Untitled";
            this.Text = (openfile + " | Tom Text");
        }

        private void Save(object sender, EventArgs e)
        {
            if (fileopened == true)
            {
                richTextBox1.SaveFile(openfile);
            }
            else
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Tom Text Document (*.ttd)|*.ttd";
                save.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
                if (save.ShowDialog() == DialogResult.OK)
                {
                    openfile = save.FileName;
                    richTextBox1.SaveFile(save.FileName);
                    edited = false;
                    this.Text = (openfile + " | Tom Text");
                }
            }
        }

        private void SaveAs(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Tom Text Document (*.ttd)|*.ttd";
            save.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (save.ShowDialog() == DialogResult.OK)
            {
                openfile = save.FileName;
                richTextBox1.SaveFile(save.FileName);
                edited = false;
                this.Text = (openfile + " | Tom Text");
            }
        }

        private void Open(object sender, EventArgs e)
        {
           OpenFileDialog save = new OpenFileDialog();
           save.Filter = "Tom Text Document (*.ttd)|*.ttd";
           save.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
           if (save.ShowDialog() == DialogResult.OK)
           {
               openfile = save.FileName;
               richTextBox1.LoadFile(save.FileName);
               edited = false;
               fileopened = true;
               this.Text = (openfile + " | Tom Text");
           }
        }

        private void Cut(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        private void Copy(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void Paste(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void Undo(object sender, EventArgs e)
        {
            richTextBox1.Undo();
        }

        private void Redo(object sender, EventArgs e)
        {
            richTextBox1.Redo();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            edited = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripButton18.Enabled = richTextBox1.CanUndo;
            toolStripButton19.Enabled = richTextBox1.CanRedo;
            toolStripMenuItem1.Enabled = richTextBox1.CanUndo;
            toolStripMenuItem2.Enabled = richTextBox1.CanRedo;
        }

        private void Close(object sender, EventArgs e)
        {
            Close();
        }

        private void Font(object sender, EventArgs e)
        {
            FontDialog font = new FontDialog();
            font.Font = richTextBox1.SelectionFont;
            font.Color = richTextBox1.SelectionColor;
            font.ShowColor = true;
            if (font.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                richTextBox1.SelectionFont = font.Font;
                richTextBox1.SelectionColor = font.Color;
            }
        }

        private void CheckForUpdates(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Checking for Updates";
            toolStripStatusLabel1.Image = Properties.Resources.system_software_update;
            WebClient client = new WebClient();
            try
            {
                client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel), @"ltstver");
                string ver = File.ReadAllText(@"ltstver");
                if (ver == String.Format("{0}", AssemblyVersion))
                {
                    Properties.Settings.Default.UpdateOnExit = false;
                    Properties.Settings.Default.Save();
                    toolStripStatusLabel1.Text = "No updates avalable!";
                    Thread.Sleep(1000);
                    toolStripStatusLabel1.Text = "Idle";
                    toolStripStatusLabel1.Image = Properties.Resources.accessories_text_editor;
                }
                else
                {
                        
                        toolStripStatusLabel1.Text = "Update Avalable!";
                        toolStripStatusLabel1.Image = Properties.Resources.software_update_available;
                        if (MessageBox.Show("An update is avalable, you have version " + String.Format("{0}", AssemblyVersion) + " and the server has version " + ver + ". Do you want to install on the next launch?", "Update Avalable!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {

                            Properties.Settings.Default.UpdateOnExit = true;
                            Properties.Settings.Default.Save();
                            toolStripStatusLabel1.Text = "Downloading update...";
                            toolStripStatusLabel1.Image = Properties.Resources.document_save;
                            try
                            {
                                client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                            }
                            catch
                            {
                                toolStripStatusLabel1.Text = "Download error";
                                toolStripStatusLabel1.Image = Properties.Resources.dialog_error;
                                if (MessageBox.Show("An error has occurred downloading the update file, do you want to Abort, Retry or Ignore?", "Update download failed.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information) == DialogResult.Retry)
                                {
                                    try
                                    {
                                        toolStripStatusLabel1.Text = "Downloading update...";
                                        toolStripStatusLabel1.Image = Properties.Resources.document_save;
                                        client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("The update download has failed, please relaunch or go to 'Tools > Updates > Check Now' to re-attempt.", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        Properties.Settings.Default.UpdateOnExit = false;
                                        Properties.Settings.Default.Save();
                                    }
                                }
                                else
                                {
                                    Properties.Settings.Default.UpdateOnExit = false;
                                    Properties.Settings.Default.Save();
                                }
                            }
                            toolStripStatusLabel1.Text = "Idle";
                            toolStripStatusLabel1.Image = Properties.Resources.accessories_text_editor;
                        }
                        else
                        {
                            Properties.Settings.Default.UpdateOnExit = false;
                            Properties.Settings.Default.Save();
                        }
                    }
                   
                }
            catch 
            {
                toolStripStatusLabel1.Text = "Idle";
                toolStripStatusLabel1.Image = Properties.Resources.accessories_text_editor;
                MessageBox.Show("It appears that you are offline and cannot check for updates or some other error has occurred, Please try again later", "INTERNET, Y U NO WORK?!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Properties.Settings.Default.UpdateOnExit = false;
                Properties.Settings.Default.Save();
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Properties.Settings.Default.UpdateOnExit == true)
            {
                Process.Start(@"updater.exe");
            }
            Properties.Settings.Default.Save();
        }

        private void Form1_StyleChanged(object sender, EventArgs e)
        {

        }

        private void WindowShown(object sender, EventArgs e)
        {
            EditorForm.CheckForIllegalCrossThreadCalls = false;
            backgroundWorker1.RunWorkerAsync();

        }
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Autosaving...";
            toolStripStatusLabel1.Image = Properties.Resources.document_save;
            if (openfile == "Untitled")
            {
                File.Delete(@"autosaved.ttd");
                richTextBox1.SaveFile(@"autosaved.ttd");
            }
            else
            {
                richTextBox1.SaveFile(openfile);
            }
            toolStripStatusLabel1.Text = "Idle";
            toolStripStatusLabel1.Image = Properties.Resources.accessories_text_editor;
        }

        private void helpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form help = new Help();
            help.Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private void fIleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void psteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}


