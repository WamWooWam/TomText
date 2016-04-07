using System;
using System.Diagnostics;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
        //Loads debug outputer
        System.IO.StreamWriter _debug =
            new System.IO.StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Text\Logs\log-" +
                                       DateTime.Now.ToString().Replace('/', '-').Replace(':', '-') + ".txt");

        //Working document
        private string _doc = "Untitled";
        //Shows if the document has been edited. This way the app can show a "Do you wish to save" box on closure or new document creation.
        private bool _edited = false;
        //Initialises dialog boxes.
        private SaveFileDialog _save = new SaveFileDialog();
        private OpenFileDialog _open = new OpenFileDialog();
        private ComputerInfo _info = new ComputerInfo();
        //App begins
        public EditorForm()
        {
            #region Initial Debug Output

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
            fontComboBox.Text = editorBox.Font.FontFamily.Name;
            fontSizeComboBox.Text = editorBox.Font.SizeInPoints.ToString();
            label1.Hide();
            progressBar1.Hide();
            _debug.WriteLine("");
            RefreshGUI();
            if (Properties.Settings.Default.FontCache.Count == 0)
            {
                label2.Text = "Building font cache, please wait.";
                foreach (FontFamily font in System.Drawing.FontFamily.Families)
                {
                    if (font.IsStyleAvailable(FontStyle.Regular))
                    {
                        Font add = new Font(font, 9, FontStyle.Regular);
                        Properties.Settings.Default.FontCache.Add(add.Name);
                        Properties.Settings.Default.Save();
                    }
                }
                foreach (String font in Properties.Settings.Default.FontCache)
                {
                    fontComboBox.Items.Add(font);
                }
                label2.Text = "Ready";
            }
            else
            {
                foreach (String font in Properties.Settings.Default.FontCache)
                {
                    fontComboBox.Items.Add(font);
                }
            }
            InitialiseAddins();
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

            if (Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath)).Length >= 10)
            {
                _debug.WriteLine("");
                foreach (
                    var file in
                        new DirectoryInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Text\Logs\")
                            .GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(10))
                {
                    file.Delete();
                    _debug.WriteLine("Deleted " + @"\Resources\Text\Logs\" + file.Name + "");
                }
                _debug.WriteLine("");
            }
            _debug.WriteLine("Form Initialized");
        }

        private void editorBox_TextChanged(object sender, EventArgs e)
        {
            _edited = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
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
                                progressBar1.Hide();
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
                                    progressBar1.Hide();
                                    label1.Show();
                                    Thread.Sleep(1000);
                                    editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetFileName(_doc));
                                    label1.Hide();
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
            _edited = false;           
            _debug.WriteLine("Form Shown");
        }

        #region Button Functions

        //Creates a new file
        private void New(object sender, EventArgs e)
        {
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

        //Opens a file
        private void Open(object sender, EventArgs e)
        {
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
            if (_open.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(_open.FileName) == ".txt")
                {
                    editorBox.LoadFile(_open.FileName, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    if (Path.GetExtension(_open.FileName) == ".rtfc")
                    {
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(_open.FileName))
                            {
                                zip.ExtractProgress += (ExtractProgress);
                                zip.ExtractAll(Path.GetDirectoryName(Application.ExecutablePath),ExtractExistingFileAction.OverwriteSilently);
                                editorBox.LoadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                progressBar1.Hide();
                                UseWaitCursor = false;
                                zip.Dispose();
                                File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
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
                        }
                    }
                    else
                    {
                        if (Path.GetExtension(_open.FileName) == ".rtfe")
                        {
                            var pwform = new GUI.ReqPswd(_open.FileName, false);
                            if (pwform.ShowDialog() == DialogResult.OK)
                            {
                                ZipFile zip = ZipFile.Read(_open.FileName);
                                zip.ExtractProgress += (ExtractProgress);
                                zip.Password = pwform.pw;
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                try
                                {
                                    zip.ExtractAll(Path.GetDirectoryName(Application.ExecutablePath), ExtractExistingFileAction.OverwriteSilently);
                                    progressBar1.Hide();
                                    label1.Show();
                                    editorBox.LoadFile(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + Path.GetFileName(_open.FileName));
                                    label1.Hide();
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
            }
        }

        //Saves a file
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
                                    progressBar1.Hide();
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
                                        progressBar1.Hide();
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
                                progressBar1.Hide();
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
                                    progressBar1.Hide();
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

        //Saves a file as a new file
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
                            progressBar1.Hide();
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
                                    progressBar1.Hide();
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

        //Undo
        private void Undo(object sender, EventArgs e)
        {
            editorBox.Undo();
        }

        //Redo
        private void Redo(object sender, EventArgs e)
        {
            editorBox.Redo();
        }

        //Cut
        private void Cut(object sender, EventArgs e)
        {
            editorBox.Cut();
        }

        //Copy
        private void Copy(object sender, EventArgs e)
        {
            editorBox.Copy();
        }

        //Paste
        private void Paste(object sender, EventArgs e)
        {
            editorBox.Paste();
        }

        //Select All
        private void SelectAll(object sender, EventArgs e)
        {
            editorBox.SelectAll();
        }

        //Insert date and time
        private void dateAndTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorBox.AppendText(DateTime.Now.ToString());
        }

        //Opens about window
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new GUI.AboutTomText();
            about.Show();
        }

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

        //This code goes through all MenuStrips and ToolStrips, Getting the contents and setting the images for each
        //control, this makes theming easy as all images are dynamic and can be modified with little effort.
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
            float NewSize;
            float.TryParse(fontSizeComboBox.SelectedItem.ToString(), out NewSize);
            Font NewFont = new Font(editorBox.SelectionFont.Name, NewSize, editorBox.SelectionFont.Style);
            editorBox.SelectionFont = NewFont;
        }

        private void justifyLeftToolStripButton1_Click(object sender, EventArgs e)
        {
            if (justifyLeftToolStripButton1.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Left;
            }
            justifyCenterToolStripButton.Checked = false;
            justifyRightToolStripButton4.Checked = false;
        }

        private void justifyCenterToolStripButton_Click(object sender, EventArgs e)
        {
            if (justifyCenterToolStripButton.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Center;
            }
            justifyLeftToolStripButton1.Checked = false;
            justifyRightToolStripButton4.Checked = false;
        }

        private void justifyrightToolStripButton4_Click(object sender, EventArgs e)
        {
            if (justifyRightToolStripButton4.Checked == true)
            {
                editorBox.SelectionAlignment = HorizontalAlignment.Right;
            }
            justifyCenterToolStripButton.Checked = false;
            justifyLeftToolStripButton1.Checked = false;
        }

        private void fullyJustifyToolStripButton_Click(object sender, EventArgs e)
        {
        }

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
                                        progressBar1.Hide();
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
                                            progressBar1.Hide();
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
                                progressBar1.Hide();
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
                                        progressBar1.Hide();
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

        private void Exit(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            double zoom = trackBar1.Value;
            zoom = zoom/100;
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
                var imgtypes = new[] {".jpeg", ".jpg", ".png", ".ico", ".gif", ".bmp", ".emp", ".wmf", ".tiff"};
                var txttypes = new[] {".txt", ".rtf", ".cs", ".vb", ".c", ".h", ".xml", ".json"};
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

        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            UseWaitCursor = true;
            progressBar1.Show();
            double bytestoextract = e.TotalBytesToTransfer;
            double bytestransferred = e.BytesTransferred;
            progressBar1.Maximum = int.Parse(Math.Round(bytestoextract).ToString());
            progressBar1.Value = int.Parse(Math.Round(bytestransferred).ToString());
        }

        private void SaveProgress(object sender, SaveProgressEventArgs e)
        {
            UseWaitCursor = true;
            progressBar1.Show();
            double bytestoextract = e.TotalBytesToTransfer;
            double bytestransferred = e.BytesTransferred;
            progressBar1.Maximum = int.Parse(Math.Round(bytestoextract).ToString());
            progressBar1.Value = int.Parse(Math.Round(bytestransferred).ToString());
        }

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
            _debug.Close();
        }

        private void InitialiseAddins()
        {
            pluginsToolStripDropDownButton.Visible = false;
        }
    }
}