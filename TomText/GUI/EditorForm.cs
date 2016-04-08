using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
//using System.Text;
//using System.Threading;
using System.Windows.Forms;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ionic.Zip;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;


namespace TomText
{
    public partial class EditorForm : Form
    {
        //Loads debug outputer. Outputs to <APP-PATH>\Resources\Text\Logs\log-<DATE-TIME>.txt
        System.IO.StreamWriter _debug =
            new System.IO.StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Text\Logs\log-" +
                                       DateTime.Now.ToString().Replace('/', '-').Replace(':', '-') + ".txt");
        //Working document
        private string _doc = "Untitled";
        Stopwatch sw = new Stopwatch(); 
        //Shows if the document has been edited. This way the app can show a "Do you wish to save" box on closure or new document creation.
        private bool _edited = false;
        //Initialises dialog boxes.
        private SaveFileDialog _save = new SaveFileDialog();
        private OpenFileDialog _open = new OpenFileDialog();
        private ComputerInfo _info = new ComputerInfo();
        //App begins
        public EditorForm()
        {
            //This code is here to output current settings and environment info to aid debugging, stuff like current OS, Memory (Total and free)
            //launch time and current settings. NONE of this info IS EVER transmittted ANYWERE without the user's consent as detailed in PRIVACY.TXT
            #region Initial Debug Output
            _debug.AutoFlush = true;
            _debug.WriteLine("--- TomText Debug Output Begin ---");
            _debug.WriteLine("Welcome to " + Properties.Settings.Default.AppName + " ver. " + Application.ProductVersion);
            _debug.WriteLine("Current OS is             " + _info.OSFullName);
            _debug.WriteLine("Current .Net version is   " + Environment.Version);
            _debug.WriteLine("Current installed memory: " + _info.TotalPhysicalMemory/1048576 + "MB");
            _debug.WriteLine("Current avalable memory:  " + _info.AvailablePhysicalMemory/1048576 + "MB");
            _debug.WriteLine("Launch time               " + DateTime.Now);
            _debug.WriteLine("\r\n--- Current Settings ---");
            _debug.WriteLine("AppName               =   " + Properties.Settings.Default.AppName);
            _debug.WriteLine("Check for Updates     =   " + Properties.Settings.Default.CheckForUpdates.ToString());
            _debug.WriteLine("Language              =   " + Properties.Settings.Default.UILang);
            _debug.WriteLine("Spelling Language     =   " + Properties.Settings.Default.SpellLang);
            _debug.WriteLine("UIFont                =   " + Properties.Settings.Default.UIFont.FontFamily.Name + ", " +
                             Properties.Settings.Default.UIFont.SizeInPoints + "pt");
            _debug.WriteLine("DocFont               =   " + Properties.Settings.Default.DocFont.FontFamily.Name + ", " +
                             Properties.Settings.Default.DocFont.SizeInPoints + "pt");
            _debug.WriteLine("UIRes                 =   " + Properties.Settings.Default.UIRes.Width + "x" +
                             Properties.Settings.Default.UIRes.Height);
            _debug.WriteLine("Openable File Types   =   " + Properties.Settings.Default.OpenableFileTypes);
            _debug.WriteLine("Saveable File Types   =   " + Properties.Settings.Default.SaveableFileTypes);
            if (Properties.Settings.Default.HiDPI)
            {
                _debug.WriteLine("Theme               =  " + Properties.Settings.Default.Theme + " (Hi-DPI)");
            }
            else
            {
                _debug.WriteLine("Theme                 =   " + Properties.Settings.Default.Theme);
            }
            _debug.WriteLine("\r\n--- Font Cache ---");
            foreach (string font in Properties.Settings.Default.FontCache)
            {
                _debug.WriteLine("  " + font);
            }
            _debug.WriteLine("--- End Font Cache ---");

            #endregion
            //Begins app initialisation
            InitializeComponent();
            _debug.WriteLine("\r\nDesigner code initialised");
            _save.Filter = Properties.Settings.Default.SaveableFileTypes;
            _open.Filter = Properties.Settings.Default.OpenableFileTypes;
            _debug.WriteLine("Prepared Openable/Saveable filenames");
            //Sets default fonts
            fontComboBox.Text = editorBox.Font.FontFamily.Name;
            fontSizeComboBox.Text = editorBox.Font.SizeInPoints.ToString();
            _debug.WriteLine("");
            //Refreshes UI with images
            RefreshGUI();
            // This code is here to speed up populating fonts in fontComboBox. It caches the name of each font in it's regular form to a
            // String Collection
            #region Font Cache
            if (Properties.Settings.Default.FontCache.Count == 0)
            {
                foreach (FontFamily font in System.Drawing.FontFamily.Families)
                {
                    if (font.IsStyleAvailable(FontStyle.Regular))
                    {
                        Font add = new Font(font, 9, FontStyle.Regular);
                        Properties.Settings.Default.FontCache.Add(add.Name);
                        fontComboBox.Items.Add(font.Name);
                        Properties.Settings.Default.Save();
                    }
                }
            }
            else
            {
                foreach (String font in Properties.Settings.Default.FontCache)
                {
                    fontComboBox.Items.Add(font);
                }
            }
#endregion
            //This code will (at some point) initialise addins, adding their contents to the pluginsToolStripDropDownButton 
            InitialiseAddins();
            //This code sets stuff like justification and font combo box text and such
            #region Set formatting element values
            fontComboBox.Text = editorBox.SelectionFont.FontFamily.Name;
            fontSizeComboBox.Text = editorBox.SelectionFont.Size.ToString(CultureInfo.InvariantCulture);
            boldToolStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Bold);
            italicToolStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Italic);
            underlineStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Underline);
            justifyLeftToolStripButton1.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Left);
            justifyRightToolStripButton4.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Right);
            justifyCenterToolStripButton.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Center);
            wordWrapToolStripButton.Checked = editorBox.WordWrap;
            toolStripMenuItem2.Checked = editorBox.WordWrap;
            undoToolStripButton.Enabled = editorBox.CanUndo;
            undoToolStripMenuItem.Enabled = editorBox.CanUndo;
            undoToolStripMenuItem1.Enabled = editorBox.CanUndo;
            redoToolStripButton.Enabled = editorBox.CanRedo;
            redoToolStripMenuItem.Enabled = editorBox.CanRedo;
            redoToolStripMenuItem1.Enabled = editorBox.CanRedo;
            #endregion
            //This code prevents <APP-PATH>\Resources\Text\Logs from containing more than 20 files, deleting any older files.
            #region Manage Logs Folder
            if (Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath)).Length >= 20)
            {
                _debug.WriteLine("");
                foreach (
                    var file in
                        new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Text\Logs\")
                            .GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(20))
                {
                    file.Delete();
                    _debug.WriteLine("Deleted " + @"\Resources\Text\Logs\" + file.Name + "");
                }
                _debug.WriteLine("");
            }
            #endregion
            //Hides UI Elements that should be hidden
            
            label1.Text = "";
            //Writes to log that this process has finished
            _debug.WriteLine("Form Initialized");
        }

        private void editorBox_TextChanged(object sender, EventArgs e)
        {
            _edited = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            #region Opens files on launch

            string[] args = Environment.GetCommandLineArgs();
            if (File.Exists("temp"))
            {
                editorBox.LoadFile("temp");
                File.Delete("temp");
            }
            try
            {
                _doc = args[1];
                if (Path.GetExtension(_doc) == ".txt")
                {
                    editorBox.LoadFile(_doc, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    if (Path.GetExtension(_doc) == ".rtfc")
                    {
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(_doc))
                            {
                                zip.ExtractProgress += (ExtractProgress);
                                zip.ExtractAll(Path.GetDirectoryName(Application.ExecutablePath),ExtractExistingFileAction.OverwriteSilently);
                                editorBox.LoadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_doc));
                                
                                UseWaitCursor = false;
                                zip.Dispose();
                                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_doc));
                            }
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                            UseWaitCursor = false;
                        }
                    }
                    
                    else
                    {
                        if (Path.GetExtension(_doc) == ".rtfe")
                        {
                            var pwform = new GUI.ReqPswd(_doc, false);
                            if (pwform.ShowDialog() == DialogResult.OK)
                            {
                                ZipFile zip = ZipFile.Read(_doc);
                                zip.ExtractProgress += (ExtractProgress);
                                zip.Password = pwform.pw;
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                try
                                {
                                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                                    
                                    label1.Show();
                                    Thread.Sleep(1000);
                                    editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetFileName(_doc));
                                    label1.Text = "";
                                    UseWaitCursor = false;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Password Incorect");
                                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                    _debug.WriteLine("Error message: " + ex.Message);
                                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                    _debug.WriteLine("Faulting executable: " + ex.Source);
                                    _debug.WriteLine("Error handled sucsessfully");
                                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                    UseWaitCursor = false;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                editorBox.LoadFile(_doc, RichTextBoxStreamType.RichText);
                            }
                            catch
                            {
                                editorBox.LoadFile(_doc, RichTextBoxStreamType.PlainText);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
            }
#endregion
            _edited = false;           
            _debug.WriteLine("Form Shown");
        }

        /// <summary>
        /// This contains all functions that are executed by buttons within the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Button Functions

        //Creates a new file
        #region New
        private void New(object sender, EventArgs e)
        {
            //This asks the user if they want to save the current doccument
            if (_edited == true)
            {
                DialogResult result =
                    MessageBox.Show("The document " + _doc + " has not been saved. Do you want to save the changes?",
                        "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Save(null, null);
                }
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    //This clears the form and resets some options
                    editorBox.Clear();
                    _edited = false;
                    _doc = "Untitled";
                }
            }
            else
            {
                editorBox.Clear();
                _edited = false;
                _doc = "Untitled";
            }
        }
#endregion
        //Opens a file
        #region Open
        private void Open(object sender, EventArgs e)
        {
            //Asks if you want to save the current file before opening a new one
            if (_edited == true)
            {
                DialogResult result =
                    MessageBox.Show("The document " + _doc + " has not been saved. Do you want to save the changes?",
                        "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Save(null, null);
                }
                if (result == DialogResult.Cancel)
                {
                }
            }
            //Opens an open dialog and if the result is OK
            if (_open.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath));
                
                //Checks the extension of the file
                if (Path.GetExtension(_open.FileName) == ".txt")
                {
                    //Loads as plain text if it's a .txt file
                    editorBox.LoadFile(_open.FileName, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    label1.Text = "Extracting document. Please wait...";
                    Refresh();
                    //If the file is a compressed RTF
                    if (Path.GetExtension(_open.FileName) == ".rtfc")
                    {
                        
                        try
                        {
                            //Loads Ionic.Zip library
                            using (ZipFile zip = ZipFile.Read(_open.FileName))
                            {
                                //Sets extraction progress
                                zip.ExtractProgress += (ExtractProgress);
                                //Extracts all files to current app directory
                                
                                zip.ExtractAll(Path.GetDirectoryName(Application.ExecutablePath),ExtractExistingFileAction.OverwriteSilently);
                                //Loads the extracted file
                                editorBox.LoadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                //Hides stuff
                                label1.Text = "";
                                UseWaitCursor = false;
                                //Disposes of zip handler
                                zip.Dispose();
                                //Deletes extracted file as it's no longer needed.
                                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                //Sets text of _doc
                                _doc = _open.FileName;
                            }
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                            UseWaitCursor = false;
                            
                            label1.Text = "";
                        }
                    }
                    else
                    {
                        //If the file is an encrypted RTF file
                        if (Path.GetExtension(_open.FileName) == ".rtfe")
                        {
                            label1.Show();
                            Thread.Sleep(100);
                            //Loads password form
                            var pwform = new GUI.ReqPswd(_open.FileName, false);
                            //Shows password form an if a password is set and validated.
                            if (pwform.ShowDialog() == DialogResult.OK)
                            {
                                //Loads Ionic.Zip Library
                                ZipFile zip = ZipFile.Read(_open.FileName);
                                //Sets extraction progress
                                zip.ExtractProgress += (ExtractProgress);
                                zip.Password = pwform.pw;
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                try
                                {
                                    zip.ExtractAll(Path.GetDirectoryName(Application.ExecutablePath), ExtractExistingFileAction.OverwriteSilently);
                                    
                                    label1.Show();
                                    editorBox.LoadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                    label1.Text = "";
                                    UseWaitCursor = false;
                                    zip.Dispose();
                                    File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                    _doc = _open.FileName;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Password Incorect");
                                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                    _debug.WriteLine("Error message: " + ex.Message);
                                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                    _debug.WriteLine("Faulting executable: " + ex.Source);
                                    _debug.WriteLine("Error handled sucsessfully");
                                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                    UseWaitCursor = false;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                editorBox.LoadFile(_open.FileName, RichTextBoxStreamType.RichText);
                            }
                            catch
                            {
                                editorBox.LoadFile(_open.FileName, RichTextBoxStreamType.PlainText);
                            }
                        }
                    }
                }
                _edited = false;
                _doc = _open.FileName;
                FileInfo[] files = di.GetFiles("*.tmp")
                                     .Where(p => p.Extension == ".tmp").ToArray();
                foreach (FileInfo file in files)
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        File.Delete(file.FullName);
                    }
                    catch { }
            }
        }
#endregion
        //Saves a file
        #region Save
        private void Save(object sender, EventArgs e)
        {
            if (editorBox.Text == "")
            {
            }
            else
            {
                if (_doc == "Untitled")
                {
                    _save.Title = "Saving file: " + _doc;
                    if (_save.ShowDialog() == DialogResult.OK)
                    {
                        if (Path.GetExtension(_save.FileName) == ".txt")
                        {
                            editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            if (Path.GetExtension(_save.FileName) == ".rtfc")
                            {
                                try
                                {
                                    editorBox.SaveFile(Path.GetFileName(_save.FileName), RichTextBoxStreamType.RichText);
                                    ZipFile zip = new ZipFile();
                                    zip.SaveProgress += (SaveProgress);
                                    zip.AddFile(Path.GetFileName(_save.FileName));
                                    zip.Save(_save.FileName);
                                    File.Delete(Path.GetFileName(_save.FileName));
                                    
                                    UseWaitCursor = false;
                                    zip.Dispose();
                                }
                                catch (Exception ex)
                                {
                                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                    _debug.WriteLine("Error message: " + ex.Message);
                                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                    _debug.WriteLine("Faulting executable: " + ex.Source);
                                    _debug.WriteLine("Error handled sucsessfully");
                                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                    UseWaitCursor = false;
                                }
                            }
                            else
                            {
                                if (Path.GetExtension(_save.FileName) == ".rtfe")
                                {
                                    var pwform = new GUI.ReqPswd(_save.FileName, true);
                                    if (pwform.ShowDialog() == DialogResult.OK)
                                    {
                                        editorBox.SaveFile(Path.GetFileName(_save.FileName), RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += (SaveProgress);
                                        zip.Password = pwform.pw;
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                        zip.AddFile(Path.GetFileName(_save.FileName));
                                        zip.Save(_save.FileName);
                                        File.Delete(Path.GetFileName(_save.FileName));
                                        
                                        UseWaitCursor = false;
                                        zip.Dispose();
                                    }
                                }
                                else
                                {
                                    editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.RichText);
                                }
                            }
                        }
                        _doc = _save.FileName;
                        _edited = false;
                    }
                }
                else
                {
                    if (Path.GetExtension(_doc) == ".txt")
                    {
                        editorBox.SaveFile(_doc, RichTextBoxStreamType.PlainText);
                    }
                    else
                    {
                        if (Path.GetExtension(_doc) == ".rtfc")
                        {
                            try
                            {
                                editorBox.SaveFile(Path.GetFileName(_doc), RichTextBoxStreamType.RichText);
                                ZipFile zip = new ZipFile();
                                zip.SaveProgress += (SaveProgress);
                                zip.AddFile(Path.GetFileName(_doc));
                                zip.Save(_save.FileName);
                                File.Delete(Path.GetFileName(_doc));                               
                                UseWaitCursor = false;
                                zip.Dispose();
                            }
                            catch (Exception ex)
                            {
                                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                _debug.WriteLine("Error message: " + ex.Message);
                                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                _debug.WriteLine("Faulting executable: " + ex.Source);
                                _debug.WriteLine("Error handled sucsessfully");
                                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                UseWaitCursor = false;
                            }
                            
                        }
                        else
                        {
                            if (Path.GetExtension(_save.FileName) == ".rtfe")
                            {
                                var pwform = new GUI.ReqPswd(_save.FileName, true);
                                if (pwform.ShowDialog() == DialogResult.OK)
                                {
                                    editorBox.SaveFile(Path.GetFileName(_save.FileName), RichTextBoxStreamType.RichText);
                                    ZipFile zip = new ZipFile();
                                    zip.SaveProgress += (SaveProgress);
                                    zip.Password = pwform.pw;
                                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                    zip.AddFile(Path.GetFileName(_save.FileName));
                                    zip.Save(_save.FileName);
                                    File.Delete(Path.GetFileName(_save.FileName));
                                    UseWaitCursor = false;
                                }
                            }
                            else
                            {
                                editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.RichText);
                            }
                        }
                    }
                    _edited = false;
                }
            }
        }
        #endregion
        //Saves a file as a new file
        #region Save As
        private void SaveAs(object sender, EventArgs e)
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
            saveToolStripButton.Image = Image.FromFile(themepath + @"\save-as.png");
            _save.Title = "Saving file: " + Path.GetTempPath() + Path.GetFileName(_doc);
            if (_save.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(_save.FileName) == ".txt")
                {
                    editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    if (Path.GetExtension(_save.FileName) == ".rtfc")
                    {
                        try
                        {
                            editorBox.SaveFile(Path.GetFileName(_save.FileName), RichTextBoxStreamType.RichText);
                            ZipFile zip = new ZipFile();
                            zip.SaveProgress += (SaveProgress);
                            zip.AddFile(Path.GetFileName(_save.FileName));
                            zip.Save(_save.FileName);
                            File.Delete(Path.GetFileName(_save.FileName));
                            
                            UseWaitCursor = false;
                            zip.Dispose();
                            label1.Text = "";
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                            UseWaitCursor = false;
                        }
                    }
                    else
                    {
                        if (Path.GetExtension(_save.FileName) == ".rtfe")
                        {
                            var pwform = new GUI.ReqPswd(_save.FileName, true);
                            if (pwform.ShowDialog() == DialogResult.OK)
                            {
                                    editorBox.SaveFile(Path.GetFileName(_save.FileName), RichTextBoxStreamType.RichText);
                                    ZipFile zip = new ZipFile();
                                    zip.SaveProgress += (SaveProgress);
                                    zip.Password = pwform.pw;
                                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                    zip.AddFile(Path.GetFileName(_save.FileName));
                                    zip.Save(_save.FileName);
                                    File.Delete(Path.GetFileName(_save.FileName));
                                    
                                    UseWaitCursor = false;
                            }
                        }
                        else
                        {
                            editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.RichText);
                        }
                    }
                }
                _doc = _save.FileName;
                _edited = false;

                saveToolStripButton.Image = Image.FromFile(themepath + @"\save.png");
            }
            else
            {
                saveToolStripButton.Image = Image.FromFile(themepath + @"\save.png");
            }
        }
        #endregion
        //Undo
        #region Undo
        private void Undo(object sender, EventArgs e)
        {
            editorBox.Undo();
        }
        #endregion
        //Redo
        #region Redo
        private void Redo(object sender, EventArgs e)
        {
            editorBox.Redo();
        }
        #endregion
        //Cut
        #region Cut
        private void Cut(object sender, EventArgs e)
        {
            editorBox.Cut();
        }
        #endregion
        //Copy
        #region Copy
        private void Copy(object sender, EventArgs e)
        {
            editorBox.Copy();
        }
        #endregion
        //Paste
        #region Paste
        private void Paste(object sender, EventArgs e)
        {
            editorBox.Paste();
        }
        #endregion
        //Select All
        #region Select All
        private void SelectAll(object sender, EventArgs e)
        {
            editorBox.SelectAll();
        }
        #endregion
        //Insert
        #region Insert
        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog insert = new OpenFileDialog();
            insert.Filter =
                "All Insertable Files (*.jpeg; *.jpg; *.png; *.ico; *.gif; *.bmp; *.emp; *.wmf; *.tiff; *.txt; *.rtf; *.cs; *.vb; *.c; *.h; *.xml; *.json)|*.jpeg; *.jpg; *.png; *.ico; *.gif; *.bmp; *.emp; *.wmf; *.tiff; *.txt; *.rtf; *.cs; *.vb; *.c; *.h; *.xml; *.json";
            insert.Multiselect = true;
            if (insert.ShowDialog() == DialogResult.OK)
            {
                UseWaitCursor = true;
                var clpdata = Clipboard.GetDataObject();
                var imgtypes = new[] { ".jpeg", ".jpg", ".png", ".ico", ".gif", ".bmp", ".emp", ".wmf", ".tiff" };
                var txttypes = new[] { ".txt", ".rtf", ".cs", ".vb", ".c", ".h", ".xml", ".json" };
                foreach (string path in insert.FileNames)
                {
                    var ext = Path.GetExtension(path);
                    if (ext != null && imgtypes.Contains(ext.ToLower()))
                    {
                        try
                        {
                            Image img;
                            img = Image.FromFile(path);
                            Thread.Sleep(100);
                            Clipboard.SetImage(img);
                            Thread.Sleep(100);
                            editorBox.Paste();
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                        }
                    }
                    else
                    {
                        if (txttypes.Contains(Path.GetExtension(path).ToLower()))
                        {
                            if (Path.GetExtension(path).ToLower() == ".rtf")
                            {
                                RichTextBox temp = new RichTextBox();
                                temp.LoadFile(path);
                                temp.SelectAll();
                                temp.Copy();
                                editorBox.Paste();
                                Clipboard.SetDataObject(clpdata);
                                temp.Dispose();
                            }
                            else
                            {
                                editorBox.AppendText(File.ReadAllText(path));
                            }
                        }
                        else
                        {

                        }
                    }
                }
                Clipboard.SetDataObject(clpdata);
                UseWaitCursor = false;
            }
        }
        #endregion
        //Insert date and time
        #region Insert date and time
        private void dateAndTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorBox.AppendText(DateTime.Now.ToString());
        }
        #endregion
        //Opens about window
        #region About
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new GUI.AboutTomText();
            about.Show();
        }
        #endregion
        //Formatting buttons
        #region Formatting buttons
        private void boldStripButton_Click(object sender, EventArgs e)
        {
            if (boldToolStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style | FontStyle.Bold);
                editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style ^ FontStyle.Bold);
                editorBox.SelectionFont = NewFont;
            }
        }

        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            if (italicToolStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style | FontStyle.Italic);
                editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style ^ FontStyle.Italic);
                editorBox.SelectionFont = NewFont;
            }
        }

        private void underlineStripButton_Click(object sender, EventArgs e)
        {
            if (underlineStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style | FontStyle.Underline);
                editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style ^ FontStyle.Underline);
                editorBox.SelectionFont = NewFont;
            }
        }
        private void justifyLeftToolStripButton1_Click(object sender, EventArgs e)
        {
            if (justifyLeftToolStripButton1.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Left;
                justifyCenterToolStripButton.Checked = false;
                justifyRightToolStripButton4.Checked = false;

            }
            if (justifyLeftToolStripButton1.Checked == false)
            {
                if (justifyCenterToolStripButton.Checked == false && justifyRightToolStripButton4.Checked == false)
                {
                    justifyLeftToolStripButton1.Checked = true;
                }
            }
        }

        private void justifyCenterToolStripButton_Click(object sender, EventArgs e)
        {
            if (justifyCenterToolStripButton.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Center;
                justifyLeftToolStripButton1.Checked = false;
                justifyRightToolStripButton4.Checked = false;
            }
            if (justifyCenterToolStripButton.Checked == false)
            {
                if (justifyLeftToolStripButton1.Checked == false && justifyRightToolStripButton4.Checked == false)
                {
                    justifyCenterToolStripButton.Checked = true;
                }
            }
        }

        private void justifyrightToolStripButton4_Click(object sender, EventArgs e)
        {
            if (justifyRightToolStripButton4.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Right;
                justifyCenterToolStripButton.Checked = false;
                justifyLeftToolStripButton1.Checked = false;
            }
            if (justifyRightToolStripButton4.Checked == false)
            {
                if (justifyLeftToolStripButton1.Checked == false && justifyCenterToolStripButton.Checked == false)
                {
                    justifyRightToolStripButton4.Checked = true;
                }
            }
        }

        #endregion
        //Opens settings
        #region Open settings
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form settings = new GUI.Settings();
            if (settings.ShowDialog() == DialogResult.OK)
            {
                RefreshGUI();
            }
            else
            {
                if (
                    MessageBox.Show(
                        "To select the embedded theme, the app must relaunch. Do you want to relaunch now?",
                        "Relaunch Required", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    editorBox.SaveFile("temp");
                    _edited = false;
                    Process.Start(Application.ExecutablePath);
                    Close();
                }
                else
                {
                }
            }
        }
        #endregion
        //Font combo boxes
        #region Font combo boxes
        private void fontComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Font NewFont = new Font(fontComboBox.SelectedItem.ToString(), editorBox.SelectionFont.Size,
                    editorBox.SelectionFont.Style);
                editorBox.SelectionFont = NewFont;
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
            }
        }

        private void fontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                float NewSize;
                float.TryParse(fontSizeComboBox.SelectedItem.ToString(), out NewSize);
                Font NewFont = new Font(editorBox.SelectionFont.Name, NewSize, editorBox.SelectionFont.Style);
                editorBox.SelectionFont = NewFont;
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
            }
        }
        #endregion
        //Help
        #region Help
        private void Help(object sender, EventArgs e)
        {
            Form helpForm = new GUI.Help();
            helpForm.Show();
        }
        #endregion
        /// <summary>
        /// This code goes through all ToolStrips, getting buttons and dropdowns. Then it gets the corresponding image from 
        /// (APP-DIR)\Resources\Themes\(SELECTED-THEME\(BUTTON-TEXT).png/.bmp and sets the image value to it as well as fonts
        /// and other misc. settings
        /// </summary>
        #region RefreshGUI

        private void RefreshGUI()
        {
            string name = "";
            _debug.WriteLine("--- Begin Refresh GUI ---");
            //Reads theme .json files
            JObject themeinfo = null;
            string themepath;
            string imgtype = null;
            #region Load Theme Config Files

            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme + @"\Hi-DPI";
                _debug.WriteLine("Loading theme.json form " + @"\Resources\Themes\" + Properties.Settings.Default.Theme +
                                 @"\Hi-DPI");
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                            Properties.Settings.Default.Theme;
                _debug.WriteLine("Loading theme.json form " + @"\Resources\Themes\" + Properties.Settings.Default.Theme);
            }
            try
            {
                if (Properties.Settings.Default.Theme == "embedded")
                {
                    themeinfo = JObject.Parse(Properties.Resources.embedded);
                    _debug.WriteLine("Loaded theme.json form embedded file");
                    _debug.WriteLine("\r\n--- Loaded theme.json ---");
                    _debug.WriteLine(Properties.Resources.embedded);
                    _debug.WriteLine("--- End of loaded theme.json ---");
                }
                else
                {
                    themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                    _debug.WriteLine("Loaded theme.json form " + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\theme.json");
                    _debug.WriteLine("\r\n--- Loaded theme.json ---");
                    foreach (string line in File.ReadAllLines(themepath + @"\theme.json"))
                    {
                        _debug.WriteLine(line);
                    }
                    _debug.WriteLine("--- End of loaded theme.json ---");
                }
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("ERROR: MISSING/INVALID THEME.JSON FILE (CODE: TH001)");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                MessageBox.Show(
                    @"The theme you've selected doesn't have a valid theme configuration file (theme.json), Using embedded theme and reseting theme setting",
                    "Theming Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Properties.Settings.Default.Reset();
                _debug.WriteLine("SETTINGS RESET, ATTEMPT 2");
                if (Properties.Settings.Default.HiDPI)
                {
                    themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                                Properties.Settings.Default.Theme + @"\Hi-DPI";
                    _debug.WriteLine("Loading theme.json form " + @"\Resources\Themes\" +
                                     Properties.Settings.Default.Theme + @"\Hi-DPI");
                }
                else
                {
                    if (Properties.Settings.Default.Theme == "embedded")
                    {
                    }
                    else
                    {
                        themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                                    Properties.Settings.Default.Theme;
                        _debug.WriteLine("Loading theme.json form " + @"\Resources\Themes\" +
                                         Properties.Settings.Default.Theme + @"\Hi-DPI");
                    }
                }
                try
                {
                    if (Properties.Settings.Default.Theme == "embedded")
                    {
                        themeinfo = JObject.Parse(Properties.Resources.embedded);
                        _debug.WriteLine("Loaded theme.json form embedded file");
                        _debug.WriteLine("\r\n--- Loaded theme.json ---");
                        _debug.WriteLine(Properties.Resources.embedded);
                        _debug.WriteLine("--- End of loaded theme.json ---");
                    }
                    else
                    {
                        themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                        _debug.WriteLine("Loaded theme.json form " + themepath + @"\theme.json");
                        _debug.WriteLine("\r\n--- Loaded theme.json ---");
                        foreach (string line in File.ReadAllLines(themepath + @"\theme.json"))
                        {
                            _debug.WriteLine(line);
                        }
                        _debug.WriteLine("--- End of loaded theme.json ---");
                    }
                }
                catch (Exception ex2)
                {
                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                    _debug.WriteLine("ERROR: NO VALID THEME.JSON FILES (CODE: TH002)");
                    _debug.WriteLine("Error message: " + ex2.Message);
                    _debug.WriteLine("Fauling method: " + ex2.TargetSite);
                    _debug.WriteLine("Stack Trace: " + ex2.StackTrace);
                    _debug.WriteLine("Faulting executable: " + ex2.Source);
                    _debug.WriteLine("Error handled sucsessfully");
                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                    MessageBox.Show("There doesn't seem to be any valid themes, using embedded theme");
                    Properties.Settings.Default.Theme = "embedded";
                }
            }
            try
            {
                imgtype = themeinfo.GetValue("Image Type").ToString();
            }
            catch
            {
                imgtype = ".png";
            }

            #endregion

            //Sets window title
            this.Text = _doc + " - " + Properties.Settings.Default.AppName;
            _debug.WriteLine("Initialised: Window Title");
            //Sets the window icon if the theme specifies 
            if (themeinfo != null && themeinfo.GetValue("Changes icons?").ToString() == "true")
            {
                this.Icon =
                    new Icon(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" +
                             Properties.Settings.Default.Theme + @"\icon.ico");
                _debug.WriteLine("Initialised: Window Icon");
            }

            foreach (ToolStrip strip in container.TopToolStripPanel.Controls.OfType<ToolStrip>())
            {
                themeToolStrips(strip, imgtype, name, themeinfo, themepath);
            }

            foreach (ToolStrip strip in container.BottomToolStripPanel.Controls.OfType<ToolStrip>())
            {
                themeToolStrips(strip, imgtype, name, themeinfo, themepath);
            }

            foreach (ToolStrip strip in container.LeftToolStripPanel.Controls.OfType<ToolStrip>())
            {
                themeToolStrips(strip, imgtype, name, themeinfo, themepath);
            }

            foreach (ToolStrip strip in container.RightToolStripPanel.Controls.OfType<ToolStrip>())
            {
                themeToolStrips(strip, imgtype, name, themeinfo, themepath);
            }
            panel1.Size = new Size(panel1.Size.Width, int.Parse(themeinfo.GetValue("Image Size").ToString()));
            foreach (Button bt in panel1.Controls.OfType<Button>())
            {
                _debug.WriteLine("\r\nBegining initialisation of: " + bt.Name);
                bt.Size = new Size(int.Parse(themeinfo.GetValue("Image Size").ToString()),
                    int.Parse(themeinfo.GetValue("Image Size").ToString()));
                name = bt.Text;
                name = name.Replace("&", "");
                name = name.Replace(" ", "-");
                name = name.Replace(".", "");
                try
                {
                    if (Properties.Settings.Default.Theme == "embedded")
                    {
                        _debug.WriteLine("Using embedded theme. Skipping initialisation for " + bt.Name);
                    }
                    else
                    {
                        bt.Image = Image.FromFile(themepath + @"\" + name.ToLower() + imgtype);
                        _debug.WriteLine("Initialised: " + bt.Name + " with image: " + @"\Resources\Themes\" +
                                         Properties.Settings.Default.Theme + @"\" + name.ToLower() + imgtype);
                    }
                }
                catch (Exception ex)
                {
                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                    _debug.WriteLine("Error message: " + ex.Message);
                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    _debug.WriteLine("Faulting executable: " + ex.Source);
                    _debug.WriteLine("Error handled sucsessfully");
                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                }
            }
            _debug.WriteLine("\r\nBegining initialisation of: contextMenuStrip1");
            foreach (ToolStripMenuItem item in contextMenuStrip1.Items.OfType<ToolStripMenuItem>())
            {
                name = item.Text;
                name = name.Replace("&", "");
                name = name.Replace(" ", "-");
                name = name.Replace(".", "");
                try
                {
                    if (Properties.Settings.Default.Theme == "embedded")
                    {
                        _debug.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
                    }
                    else
                    {
                        try
                        {
                            item.Image = Image.FromFile(themepath + @"\" + name.ToLower() + imgtype);
                            _debug.WriteLine("Initialised: " + item.Name + " with image: " + @"\Resources\Themes\" +
                                             Properties.Settings.Default.Theme + @"\" + name.ToLower() + imgtype);
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
                                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                _debug.WriteLine("Error message: " + ex.Message);
                                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                _debug.WriteLine("Faulting executable: " + ex.Source);
                                _debug.WriteLine("Error handled sucsessfully");
                                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                            }
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
                                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                _debug.WriteLine("Error message: " + ex.Message);
                                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                _debug.WriteLine("Faulting executable: " + ex.Source);
                                _debug.WriteLine("Error handled sucsessfully");
                                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                        }
                    }
                    item.Font = Properties.Settings.Default.UIFont;
                }
                catch (Exception ex)
                {
                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                    _debug.WriteLine("Error message: " + ex.Message);
                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    _debug.WriteLine("Faulting executable: " + ex.Source);
                    _debug.WriteLine("Error handled sucsessfully");
                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                }
            }
            _debug.WriteLine("--- End Refresh GUI ---");
        }

        private void themeToolStrips(ToolStrip strip, String imgtype, String name, JObject themeinfo, string themepath)
        {
            //Gets all toolstrip buttons
            _debug.WriteLine("\r\nBegining initialisation of: " + strip.Name);
            try
            {
                if (themeinfo.GetValue("Theme Style").ToString() == "1")
                {
                    strip.RenderMode = ToolStripRenderMode.System;
                }
                else
                {
                    if (themeinfo.GetValue("Theme Style").ToString() == "2")
                    {
                        strip.RenderMode = ToolStripRenderMode.Professional;
                    }
                    else
                    {
                        if (themeinfo.GetValue("Theme Style").ToString() == "0")
                        {
                            strip.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                        }
                    }
                }
                //Enables Hi-DPI images if specified
                strip.ImageScalingSize = new Size(int.Parse(themeinfo.GetValue("Image Size").ToString()),
                    int.Parse(themeinfo.GetValue("Image Size").ToString()));
                _debug.WriteLine("Theme styles set");
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
            }
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
                        _debug.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
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
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                        }
                        _debug.WriteLine("Initialised: " + item.Name + " with image: " + @"\Resources\Themes\" +
                                         Properties.Settings.Default.Theme + @"\" + name.ToLower() + imgtype);
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
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                        }
                    }
                    item.Font = Properties.Settings.Default.UIFont;
                }
                catch (Exception ex)
                {
                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                    _debug.WriteLine("Error message: " + ex.Message);
                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    _debug.WriteLine("Faulting executable: " + ex.Source);
                    _debug.WriteLine("Error handled sucsessfully");
                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                }
            }
            foreach (ToolStripDropDownButton dd in strip.Items.OfType<ToolStripDropDownButton>())
            {
                dd.Font = Properties.Settings.Default.UIFont;
                if (dd.HasDropDownItems == true)
                {
                    _debug.WriteLine("\r\nBegining initialisation of: " + dd.Name);
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
                                _debug.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
                            }
                            else
                            {
                                try
                                {
                                    item.Image = Image.FromFile(themepath + @"\" + name.ToLower() + imgtype);
                                    _debug.WriteLine("Initialised: " + item.Name + " with image: " +
                                                     @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\" +
                                                     name.ToLower() + imgtype);
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
                                        _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                        _debug.WriteLine("Error message: " + ex.Message);
                                        _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                        _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                        _debug.WriteLine("Faulting executable: " + ex.Source);
                                        _debug.WriteLine("Error handled sucsessfully");
                                        _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                    }
                                    ;
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
                                        _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                        _debug.WriteLine("Error message: " + ex.Message);
                                        _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                        _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                        _debug.WriteLine("Faulting executable: " + ex.Source);
                                        _debug.WriteLine("Error handled sucsessfully");
                                        _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                    _debug.WriteLine("Error message: " + ex.Message);
                                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                    _debug.WriteLine("Faulting executable: " + ex.Source);
                                    _debug.WriteLine("Error handled sucsessfully");
                                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                }
                            }
                            item.Font = Properties.Settings.Default.UIFont;
                        }
                        catch (Exception ex)
                        {
                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                            _debug.WriteLine("Error message: " + ex.Message);
                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                            _debug.WriteLine("Faulting executable: " + ex.Source);
                            _debug.WriteLine("Error handled sucsessfully");
                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                        }
                    }
                }
            }
        }
        #endregion
        //Closing Form
        #region Closing Form
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_edited == true)
            {
                DialogResult result =
                    MessageBox.Show(
                        "The document " + Path.GetTempPath() + Path.GetFileName(_doc) +
                        " has not been saved. Do you want to save the changes?", "Save changes?",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    if (_doc == "Untitled")
                    {
                        _save.Title = "Saving file: " + _doc;
                        if (_save.ShowDialog() == DialogResult.OK)
                        {
                            if (Path.GetExtension(_save.FileName) == ".txt")
                            {
                                editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.PlainText);
                            }
                            else
                            {
                                if (Path.GetExtension(_save.FileName) == ".rtfc")
                                {
                                    try
                                    {
                                        string txt;
                                        txt = Path.GetFileName(_save.FileName);
                                        editorBox.SaveFile(txt, RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                        zip.AddFile(txt);
                                        zip.Save(_save.FileName);
                                        
                                        UseWaitCursor = false;
                                        zip.Dispose();
                                    }
                                    catch (Exception ex)
                                    {
                                        _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                        _debug.WriteLine("Error message: " + ex.Message);
                                        _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                        _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                        _debug.WriteLine("Faulting executable: " + ex.Source);
                                        _debug.WriteLine("Error handled sucsessfully");
                                        _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                        UseWaitCursor = false;
                                    }
                                }
                                else
                                {
                                    if (Path.GetExtension(_save.FileName) == ".rtfe")
                                    {
                                        var pwform = new GUI.ReqPswd(_save.FileName, true);
                                        if (pwform.ShowDialog() == DialogResult.OK)
                                        {
                                            editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(_save.FileName),
                                                RichTextBoxStreamType.RichText);
                                            ZipFile zip = new ZipFile();
                                            zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                            zip.Password = pwform.pw;
                                            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                            zip.AddFile(Path.GetTempPath() + Path.GetFileName(_save.FileName));
                                            try
                                            {
                                                zip.Save(_save.FileName);
                                            }
                                            catch (Exception ex)
                                            {
                                                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                                _debug.WriteLine("Error message: " + ex.Message);
                                                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                                _debug.WriteLine("Faulting executable: " + ex.Source);
                                                _debug.WriteLine("Error handled sucsessfully");
                                                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                                MessageBox.Show("Unable to save " + _save.FileName);
                                            }
                                            File.Delete(Path.GetTempPath() + Path.GetFileName(_save.FileName));
                                            
                                            UseWaitCursor = false;
                                        }
                                    }
                                    else
                                    {
                                        editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.RichText);
                                    }
                                }
                            }
                            _doc = _save.FileName;
                            _edited = false;
                        }
                        
                    }
                    else
                    {
                        if (Path.GetExtension(_doc) == ".txt")
                        {
                            editorBox.SaveFile(_doc, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            if (Path.GetExtension(_doc) == ".rtfc")
                            {
                                try
                            {
                                string txt;
                                txt = Path.GetFileName(_save.FileName);
                                editorBox.SaveFile(txt, RichTextBoxStreamType.RichText);
                                ZipFile zip = new ZipFile();
                                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                zip.AddFile(txt);
                                zip.Save(_save.FileName);
                                
                                UseWaitCursor = false;
                                zip.Dispose();
                                label1.Text = "";
                            }
                            catch (Exception ex)
                            {
                                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                _debug.WriteLine("Error message: " + ex.Message);
                                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                _debug.WriteLine("Faulting executable: " + ex.Source);
                                _debug.WriteLine("Error handled sucsessfully");
                                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                UseWaitCursor = false;
                            }
                            }
                            else
                            {
                                if (Path.GetExtension(_doc) == ".rtfe")
                                {
                                    var pwform = new GUI.ReqPswd(_doc, true);
                                    if (pwform.ShowDialog() == DialogResult.OK)
                                    {
                                        editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(_doc),
                                            RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                        zip.Password = pwform.pw;
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                        zip.AddFile(Path.GetTempPath() + Path.GetFileName(_doc));
                                        try
                                        {
                                            zip.Save(_doc);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("Unable to save " + _doc);
                                            _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                                            _debug.WriteLine("Error message: " + ex.Message);
                                            _debug.WriteLine("Fauling method: " + ex.TargetSite);
                                            _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                                            _debug.WriteLine("Faulting executable: " + ex.Source);
                                            _debug.WriteLine("Error handled sucsessfully");
                                            _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                                        }
                                        File.Delete(_doc);
                                        File.Move(_doc + ".tmp", _doc);
                                        
                                        UseWaitCursor = false;
                                    }
                                }
                                else
                                {
                                    editorBox.SaveFile(_save.FileName, RichTextBoxStreamType.RichText);
                                }
                            }
                        }
                        _edited = false;
                    }
                }
            }
            else
            {
            }
            
        }
        #endregion
        //Run when selection changes, sets format strip stuff
        #region Selection changes
        private void editorBox_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                fontComboBox.Text = editorBox.SelectionFont.FontFamily.Name;
                fontSizeComboBox.Text = editorBox.SelectionFont.Size.ToString(CultureInfo.InvariantCulture);
                boldToolStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Bold);
                italicToolStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Italic);
                underlineStripButton.Checked = editorBox.SelectionFont.Style.HasFlag(FontStyle.Underline);
                justifyLeftToolStripButton1.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Left);
                justifyRightToolStripButton4.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Right);
                justifyCenterToolStripButton.Checked = editorBox.SelectionAlignment.HasFlag(HorizontalAlignment.Center);
                wordWrapToolStripButton.Checked = editorBox.WordWrap;
                toolStripMenuItem2.Checked = editorBox.WordWrap;
                undoToolStripButton.Enabled = editorBox.CanUndo;
                undoToolStripMenuItem.Enabled = editorBox.CanUndo;
                undoToolStripMenuItem1.Enabled = editorBox.CanUndo;
                redoToolStripButton.Enabled = editorBox.CanRedo;
                redoToolStripMenuItem.Enabled = editorBox.CanRedo;
                redoToolStripMenuItem1.Enabled = editorBox.CanRedo;
            }
            catch (Exception ex)
            {
                _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                _debug.WriteLine("Error message: " + ex.Message);
                _debug.WriteLine("Fauling method: " + ex.TargetSite);
                _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                _debug.WriteLine("Faulting executable: " + ex.Source);
                _debug.WriteLine("Error handled sucsessfully");
                _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
            }
        }
        #endregion
        //Exit
        #region Exit
        private void Exit(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
        //Zoom
        #region Zoom
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            double zoom = trackBar1.Value;
            zoom = zoom / 100;
            editorBox.ZoomFactor = float.Parse(zoom.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double val = trackBar1.Value + 10;
            if (val > 200)
            {
                val = 200;
            }
            trackBar1.Value = int.Parse(val.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double val = trackBar1.Value - 10;
            if (val < 20)
            {
                val = 20;
            }
            trackBar1.Value = int.Parse(val.ToString());
        }
        #endregion
        #endregion

        /// <summary>
        /// This contains functions to enable a progress bar to display current extraction/compression of compressed/encrypted files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Zip Progress
        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            
            UseWaitCursor = true;
            progressBar1.Show();
            label1.Text = "Extracting document. Please wait...";
            double bytestoextract = e.TotalBytesToTransfer;
            double bytestransferred = e.BytesTransferred;
            progressBar1.Maximum = int.Parse(Math.Round(bytestoextract).ToString());
            progressBar1.Value = int.Parse(Math.Round(bytestransferred).ToString());
        }

        private void SaveProgress(object sender, SaveProgressEventArgs e)
        {
            label1.Show();
            UseWaitCursor = true;
            progressBar1.Show();
            label1.Text = "Compressing document. Please wait...";
            double bytestoextract = e.TotalBytesToTransfer;
            double bytestransferred = e.BytesTransferred;
            progressBar1.Maximum = int.Parse(Math.Round(bytestoextract).ToString());
            progressBar1.Value = int.Parse(Math.Round(bytestransferred).ToString());
        }
        #endregion

        private void wordWrapToolStripButton_Click(object sender, EventArgs e)
        {
            editorBox.WordWrap = wordWrapToolStripButton.Checked;
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://louisoft01.com/apphost/tomtext/");
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            _debug.WriteLine("Form Loaded");
        }

        private void EditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            editorBox.Clear();
            _debug.Close();
        }

        private void InitialiseAddins()
        {
            pluginsToolStripDropDownButton.Visible = false;
        }

        private void CheckForUpdates()
        {
            label1.Show();
            progressBar1.Show();
            label1.Text = "Checking for updates...";
            foreach (string updatesvr in Properties.Settings.Default.UpdateRepos)
            {
                WebClient client = new WebClient();
                
                client.DownloadFileCompleted += UpdateComplete;
                client.DownloadProgressChanged += ProgressChanged;
 
                try
                {
                    JObject update = JObject.Parse(client.DownloadString(new Uri(updatesvr + "repo.json")));
                    if (update.GetValue("Repo latest").ToString() != Application.ProductVersion)
                    {
                        label1.Text = "An official update is avalable! Initiating download.";
                        sw.Start();
                        MessageBox.Show(updatesvr + update.GetValue("Repo latest").ToString() + ".exe");
                        client.DownloadFileAsync(new Uri(updatesvr + update.GetValue("Repo latest")), update.GetValue("Repo latest").ToString() + ".exe");
                    }
                }
                catch (Exception ex)
                {
                    _debug.WriteLine("\r\n--- BEGIN HANDLED ERROR ---");
                    _debug.WriteLine("Error message: " + ex.Message);
                    _debug.WriteLine("Fauling method: " + ex.TargetSite);
                    _debug.WriteLine("Stack Trace: " + ex.StackTrace);
                    _debug.WriteLine("Faulting executable: " + ex.Source);
                    _debug.WriteLine("Error handled sucsessfully");
                    _debug.WriteLine("--- END HANDLED ERROR ---\r\n");
                }
                
            }
        }

        private void UpdateComplete(object sender, AsyncCompletedEventArgs e)
        {
            sw.Reset();
            sw.Stop();
            
            progressBar1.Value = 0;
            label1.Text = "";
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            string percentage = e.ProgressPercentage.ToString() + "%";
            string downloaded = string.Format("{0} MBs / {1} MBs",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            progressBar1.Show();
            progressBar1.Value = e.ProgressPercentage;
            label1.Show();
            label1.Text = "Downloading update, " + percentage + " complete. (" + downloaded + " @ " + speed + ")";
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CheckForUpdates();
        }
    }
}