using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TomText
{
    public partial class EditorForm : Form
    {
        //Initialises variables
        bool edited = false;
        string openfile = "Untitled";
        bool fileopened = false;

        public EditorForm()
        {
            InitializeComponent();
            
        }

        //Starts when form is loaded
        private void Form1_Load(object sender, EventArgs e)
        {
            //Sets title text
            this.Text = (openfile + " | Tom Text");
            //Changes textbox font to settings default
            editorTextBox.Font = Properties.Settings.Default.DefaultFont;
            //Starts timers
            UndoRedoTimer.Start();
            AutosaveTimer.Start();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Opens about window
            Form about = new TTAbout();
            about.Show();
        }

        /// <summary>
        /// Runs when any new button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void New(object sender, EventArgs e)
        {
            //Clears TextBox
            editorTextBox.Clear();
            //Sets edited
            edited = false;
            //Sets fileopened
            fileopened = false;
            //Changes openfile text
            openfile = "Untitled";
            //Sets window text
            this.Text = (openfile + " | Tom Text");
        }

        /// <summary>
        /// Runs when any save button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save(object sender, EventArgs e)
        {
            //If a file is opened
            if (fileopened == true)
            {
                //if true saves opened file
                editorTextBox.SaveFile(openfile);
            }
            else
            {
                //Else opens a save file dialog
                SaveFileDialog save = new SaveFileDialog();
                //Sets filter and initial directory
                save.Filter = "Tom Text Document (*.ttd)|*.ttd";
                save.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
                //If OK is pressed
                if (save.ShowDialog() == DialogResult.OK)
                {
                    //Sets openfile to Save filename
                    openfile = save.FileName;
                    //Saves file
                    editorTextBox.SaveFile(save.FileName);
                    //Sets edited to false
                    edited = false;
                    //Sets window text
                    this.Text = (openfile + " | Tom Text");
                }
            }
        }
        /// <summary>
        /// Runs when any save as button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAs(object sender, EventArgs e)
        {
            //Opens a save file dialog
            SaveFileDialog save = new SaveFileDialog();
            //Sets filter and initial directory
            save.Filter = "Tom Text Document (*.ttd)|*.ttd";
            save.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            //If OK is pressed
            if (save.ShowDialog() == DialogResult.OK)
            {
                //Sets openfile to save filename
                openfile = save.FileName;
                //Saves file
                editorTextBox.SaveFile(save.FileName);
                //Sets edited to false
                edited = false;
                //Sets file opened to true
                fileopened = true;
                //Sets window text
                this.Text = (openfile + " | Tom Text");
            }
        }
        /// <summary>
        /// Runs wwhen any open button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open(object sender, EventArgs e)
        {
           //Opens an open file dialog
           OpenFileDialog open = new OpenFileDialog();
           //Sets filter and initial directly
           open.Filter = "Tom Text Document (*.ttd)|*.ttd";
           open.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
           //If OK is pressed
           if (open.ShowDialog() == DialogResult.OK)
           {
               //Sets openfile to the file just opened
               openfile = open.FileName;
               //Loads file
               editorTextBox.LoadFile(open.FileName);
               //Sets edited and fileopened
               edited = false;
               fileopened = true;
               //Sets window text
               this.Text = (openfile + " | Tom Text");
           }
        }

        /// <summary>
        /// Cut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cut(object sender, EventArgs e)
        {
            editorTextBox.Cut();
        }

        /// <summary>
        /// Copy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy(object sender, EventArgs e)
        {
            editorTextBox.Copy();
        }

        /// <summary>
        /// Paste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Paste(object sender, EventArgs e)
        {
            editorTextBox.Paste();
        }
        
        /// <summary>
        /// Undo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Undo(object sender, EventArgs e)
        {
            editorTextBox.Undo();
        }

        /// <summary>
        /// Redo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redo(object sender, EventArgs e)
        {
            editorTextBox.Redo();
        }

        /// <summary>
        /// Sets edited to true once text is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            edited = true;
        }

        /// <summary>
        /// Sets can undo/redo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripButton18.Enabled = editorTextBox.CanUndo;
            toolStripButton19.Enabled = editorTextBox.CanRedo;
            toolStripMenuItem1.Enabled = editorTextBox.CanUndo;
            toolStripMenuItem2.Enabled = editorTextBox.CanRedo;
        }

        /// <summary>
        /// Closes form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Runs wwhen any font button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Font(object sender, EventArgs e)
        {
            //Opens font dialog
            FontDialog font = new FontDialog();
            //Sets font based on current font
            font.Font = editorTextBox.SelectionFont;
            font.Color = editorTextBox.SelectionColor;
            font.ShowColor = true;
            font.ShowEffects = true;
            //If OK is pressed
            if (font.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Sets font to selected font
                editorTextBox.SelectionFont = font.Font;
                editorTextBox.SelectionColor = font.Color;
            }
        }

        /// <summary>
        /// Checks for updates when run by background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForUpdates(object sender, EventArgs e)
        {
            //Sets status labels
            statusLabel.Text = "Checking for Updates";
            statusLabel.Image = Properties.Resources.loading;
            //Starts web client
            WebClient client = new WebClient();
            try
            {
                //Downloads file containing version number (main beta alpha)
                client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel), @"ltstver");
                //Reads all text
                string ver = File.ReadAllText(@"ltstver");
                //Compares current version no. to downloaded ver no.
                if (ver == String.Format("{0}", AssemblyVersion))
                {
                    //if no update avalable
                    //Sets settings
                    Properties.Settings.Default.UpdateOnExit = false;
                    Properties.Settings.Default.Save();
                    //Sets status labels and images
                    statusLabel.Text = "No updates avalable!";
                    statusLabel.Image = Properties.Resources.system_software_update;
                    //Waits for 1 second
                    Thread.Sleep(1000);
                    //Sts status labels and images
                    statusLabel.Text = "Idle";
                    statusLabel.Image = Properties.Resources.accessories_text_editor;
                }
                else
                {
                    //If update avalable
                    //Sets status label
                    statusLabel.Text = "Update Avalable!";
                    statusLabel.Image = Properties.Resources.software_update_available;
                    if (MessageBox.Show("An update is avalable, you have version " + String.Format("{0}", AssemblyVersion) + " and the server has version " + ver + ". Do you want to install on the next launch?", "Update Avalable!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        //Sets update on exit
                        Properties.Settings.Default.UpdateOnExit = true;
                        Properties.Settings.Default.Save();
                        //Set status label and image
                        statusLabel.Text = "Downloading update...";
                        statusLabel.Image = Properties.Resources.loading;
                        //Try to download update
                        try
                        {
                            client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                        }
                        //On error
                        catch
                        {
                            //Set status label and image
                            statusLabel.Text = "Download error";
                            statusLabel.Image = Properties.Resources.dialog_error;
                            //Show message box to ask user what they want to use
                            if (MessageBox.Show("An error has occurred downloading the update file, do you want to Abort, Retry or Ignore?",
                                "Update download failed.",
                                MessageBoxButtons.AbortRetryIgnore,
                                MessageBoxIcon.Information) 
                                == 
                                DialogResult.Retry)
                            {
                                //Retry
                                try
                                {
                                    statusLabel.Text = "Downloading update...";
                                    statusLabel.Image = Properties.Resources.loading;
                                    client.DownloadFile(new Uri("http://wamwoowam.tk/TomText/" + Properties.Settings.Default.UpdateChannel + ".exe"), @"update.exe");
                                }
                                catch
                                {
                                    //On two failures ask user to try again manually
                                    MessageBox.Show("The update download has failed, please go to 'Tools > Updates > Check Now' to re-attempt.", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            //Set ststus label
                            statusLabel.Text = "Idle";
                            statusLabel.Image = Properties.Resources.accessories_text_editor;
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
                //If update CHECK fails tells user
                statusLabel.Text = "Idle";
                statusLabel.Image = Properties.Resources.accessories_text_editor;
                MessageBox.Show("An error occurred during the update checking process, Please check your internet connection and try again later", "INTERNET, Y U NO WORK?!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Properties.Settings.Default.UpdateOnExit = false;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// On form Closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Is update needed
            if (Properties.Settings.Default.UpdateOnExit == true)
            {
                //Start updater
                Process.Start(@"updater.exe");
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// On window shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowShown(object sender, EventArgs e)
        {
            //Screw everything up (potentialy)
            EditorForm.CheckForIllegalCrossThreadCalls = false;
            updaterWorker.RunWorkerAsync();

        }
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Runs on autosave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = "Autosaving...";
            statusLabel.Image = Properties.Resources.document_save;
            if (openfile == "Untitled")
            {
                File.Delete(@"autosaved.ttd");
                //deletes and saves file
                editorTextBox.SaveFile(@"autosaved.ttd");
            }
            else
            {
                editorTextBox.SaveFile(openfile);
            }
            statusLabel.Text = "Idle";
            statusLabel.Image = Properties.Resources.accessories_text_editor;
        }

        private void helpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Shows help
            Form help = new Help();
            help.Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updaterWorker.RunWorkerAsync();
        }
    }
}


