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
using System.Drawing.Text;
using System.Runtime.InteropServices;
using NHunspell;
using NHunspellExtender;

namespace TomText
{
    public partial class EditorForm : Form
    {

        bool edited = false;
        string openfile = "Untitled";
        bool fileopened = false;
        string prevtext;
        public EditorForm()
        {
            InitializeComponent();
            Console.WriteLine("");
            foreach (String font in Properties.Settings.Default.generatedListOfFonts)
            {
                toolStripComboBox1.Items.Add(font);
            }
            this.Text = (openfile + " | Tom Text");
            richTextBox1.Font = Properties.Settings.Default.DefaultFont;
            richTextBox1.ForeColor = Properties.Settings.Default.DefaultFontColour;
            toolStripComboBox2.Text = Properties.Settings.Default.DefaultFont.SizeInPoints.ToString();
            toolStripComboBox1.Text = Properties.Settings.Default.DefaultFont.FontFamily.Name;
            EditorForm.CheckForIllegalCrossThreadCalls = false;
            nHunspellTextBoxExtender1.SetSpellCheckEnabled(richTextBox1, Enabled);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            updateCheckerWorker.RunWorkerAsync();
            guiModifier.Start();
            autosaveTimer.Start();
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
            undoStripMenuItem.Enabled = richTextBox1.CanUndo;
            redoStripMenuItem.Enabled = richTextBox1.CanRedo;
            if(richTextBox1.SelectionAlignment == HorizontalAlignment.Left)
            {
                toolStripButton14.Checked = true;
                toolStripButton15.Checked = false;
                toolStripButton16.Checked = false;
            }
            if (richTextBox1.SelectionAlignment == HorizontalAlignment.Center)
            {
                toolStripButton14.Checked = false;
                toolStripButton15.Checked = true;
                toolStripButton16.Checked = false;
 
            }
            if (richTextBox1.SelectionAlignment == HorizontalAlignment.Right)
            {
                toolStripButton14.Checked = false;
                toolStripButton15.Checked = false;
                toolStripButton16.Checked = true;
            }
            toolStripButton17.Checked = richTextBox1.WordWrap;
        }

        private void Close(object sender, EventArgs e)
        {
            Close();
        }

        private void ChangeFont(object sender, EventArgs e)
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
            Console.WriteLine("");
            Console.WriteLine("Update Check Started");
            statusLabel.Text = "Checking for Updates";
            statusLabel.Image = Properties.Resources.loading;
            WebClient client = new WebClient();
            try
            {
                Console.WriteLine("Begin version download - " + "http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel);
                client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel), @"ltstver");
				string ver = File.ReadAllText(@"ltstver");
                if (ver == AssemblyVersion)
                {
                    Properties.Settings.Default.UpdateOnExit = false;
                    Properties.Settings.Default.Save();
                    Console.WriteLine("No update avalable");
                    statusLabel.Text = "No updates avalable!";
                    statusLabel.Image = Properties.Resources.system_software_update;
                    Thread.Sleep(1000);
                    statusLabel.Text = "Idle";
                    statusLabel.Image = Properties.Resources.accessories_text_editor;
                }
                else
                {
                        Console.WriteLine("Update avalable");
                        statusLabel.Text = "Update Avalable!";
                        statusLabel.Image = Properties.Resources.software_update_available;
                        if (MessageBox.Show("An update is avalable, you have version " + String.Format("{0}", AssemblyVersion) + " and the server has version " + ver + ". Do you want to install on the next launch?", "Update Avalable!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {

                            Properties.Settings.Default.UpdateOnExit = true;
                            Properties.Settings.Default.Save();
                            Console.WriteLine("Update download started");
                            statusLabel.Text = "Downloading update...";
                            statusLabel.Image = Properties.Resources.loading;
                            try
                            {
                                Console.WriteLine("Update download started - " + String.Format("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"));
                                client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                            }
                            catch
                            {
                                Console.WriteLine("Download Error");
                                statusLabel.Text = "Download error";
                                statusLabel.Image = Properties.Resources.dialog_error;
                                if (MessageBox.Show("An error has occurred downloading the update file, do you want to Abort, Retry or Ignore?", "Update download failed.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information) == DialogResult.Retry)
                                {
                                    try
                                    {
                                        Console.WriteLine("Update download started - " + String.Format("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"));
                                        statusLabel.Text = "Downloading update...";
                                        statusLabel.Image = Properties.Resources.loading;
                                        client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Update download failed");
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

                            statusLabel.Text = "Idle";
                            statusLabel.Image = Properties.Resources.accessories_text_editor;
                        }
                        else
                        {
                            Properties.Settings.Default.UpdateOnExit = false;
                            Properties.Settings.Default.Save();
                            Console.WriteLine("Update refused");
                        }
                    }
                   
                }
            catch 
            {
                statusLabel.Text = "Idle";
                statusLabel.Image = Properties.Resources.accessories_text_editor;
                Console.WriteLine("Offline error");
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
            Console.WriteLine("Autosaving...");
            statusLabel.Text = "Autosaving...";
            statusLabel.Image = Properties.Resources.document_save;
            if (openfile == "Untitled")
            {
                File.Delete(@"autosaved.ttd");
                richTextBox1.SaveFile(@"autosaved.ttd");
            }
            else
            {
                richTextBox1.SaveFile(openfile);
            }
            Console.WriteLine("Saved");
            statusLabel.Text = "Idle";
            statusLabel.Image = Properties.Resources.accessories_text_editor;
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

        private void checkNow(object sender, EventArgs e)
        {
            updateCheckerWorker.RunWorkerAsync();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            if(toolStripButton14.Checked == true)
            {
                toolStripButton15.Checked = false;
                toolStripButton16.Checked = false;
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
            }
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            if (toolStripButton15.Checked == true)
            {
                toolStripButton14.Checked = false;
                toolStripButton16.Checked = false;
                richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            }
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            if (toolStripButton16.Checked == true)
            {
                toolStripButton15.Checked = false;
                toolStripButton14.Checked = false;
                richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
            }
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            if (richTextBox1.WordWrap == true)
            {
                richTextBox1.WordWrap = false;
            }
            else
            {
                richTextBox1.WordWrap = true;
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void toolStripComboBox1_TextUpdate(object sender, EventArgs e)
        {
            
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
        {

        }
    
    }
}
