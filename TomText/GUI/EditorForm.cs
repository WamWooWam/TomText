using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Text;
//using System.Threading;
using System.Windows.Forms;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ionic.Zip;

namespace TomText
{
    public partial class EditorForm : Form
    {
        //Working document
        string doc = "Untitled";
        //Shows if the document has been edited. This way the app can show a "Do you wish to save" box on closure or new document creation.
        bool edited = false;
        //Initialises dialog boxes.
        SaveFileDialog save = new SaveFileDialog();
        OpenFileDialog open = new OpenFileDialog();
        //App begins
        public EditorForm()
        {
            //Begins app initialisation
            InitializeComponent();
            save.Filter = "Rich Text Format (*.rtf)|*.rtf|Compressed Rich Text Format (*.rtfc)|*.rtfc|Encrypted Rich Text Format (*.rtfe)|*.rtfe|Text Document (*.txt)|*.txt";
            open.Filter = "All TomText Editable Formats (*.txt; *.rtf)|*.txt; *.rtf; *.ldf; *.tdf; *.rtfc; *.rtfe|Rich Text Format (*.rtf)|*.rtf*; .rtfc; *.rtfe|Text Document (*.txt)|*.txt";
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fontComboBox.Items.Add(font.Name);
            }
            fontComboBox.Text = editorBox.Font.FontFamily.Name.ToString();
            fontSizeComboBox.Text = editorBox.Font.SizeInPoints.ToString();
            label1.Hide();
            progressBar1.Hide();
            RefreshGUI();
            string[] args = Environment.GetCommandLineArgs();
            if (File.Exists("temp"))
            {
                editorBox.LoadFile("temp");
                File.Delete("temp");
            }
            try
            {
                doc = args[1];
                if (Path.GetExtension(doc) == ".txt")
                {
                    editorBox.LoadFile(doc, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    if (Path.GetExtension(doc) == ".rtfc")
                    {
                        ZipFile zip = ZipFile.Read(doc);
                        zip.ExtractProgress += new System.EventHandler<ExtractProgressEventArgs>(ExtractProgress);
                        zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                        progressBar1.Hide();
                        label1.Show();
                        editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetTempPath() + Path.GetFileName(doc));
                        label1.Hide();
                        File.Delete(Path.GetTempPath() + @"\" + Path.GetTempPath() + Path.GetFileName(doc));
                        UseWaitCursor = false;
                    }
                    else
                    {
                        if (Path.GetExtension(doc) == ".rtfe")
                        {
                            var pwform = new GUI.ReqPswd(doc, false);
                            if (pwform.ShowDialog() == DialogResult.OK)
                            {
                                ZipFile zip = ZipFile.Read(doc);
                                zip.ExtractProgress += new System.EventHandler<ExtractProgressEventArgs>(ExtractProgress);
                                zip.Password = pwform.pw;
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                try
                                {
                                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                                    progressBar1.Hide();
                                    label1.Show();
                                    editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetTempPath() + Path.GetFileName(doc));
                                    label1.Hide();
                                    File.Delete(Path.GetTempPath() + @"\" + Path.GetTempPath() + Path.GetFileName(doc));
                                    UseWaitCursor = false;
                                }
                                catch
                                {
                                    MessageBox.Show("Password Incorect");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                editorBox.LoadFile(doc, RichTextBoxStreamType.RichText);
                            }
                            catch
                            {
                                editorBox.LoadFile(doc, RichTextBoxStreamType.PlainText);
                            }
                        }
                    }
                }
                edited = false;
            }
            catch { }
        }

        private void editorBox_TextChanged(object sender, EventArgs e)
        {
            edited = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            
                
        }
        #region Button Functions
        //Creates a new file
        private void New(object sender, EventArgs e)
        {
            if (edited == true)
            {
                DialogResult result = MessageBox.Show("The document " + doc + " has not been saved. Do you want to save the changes?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Save(null, null);
                }
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    editorBox.Clear();
                    edited = false;
                    doc = "Untitled";
                }
            }
            else
            {
                editorBox.Clear();
                edited = false;
                doc = "Untitled";
            }

        }
        //Opens a file
        private void Open(object sender, EventArgs e)
        {
            if (edited == true)
            {
                DialogResult result = MessageBox.Show("The document " + doc + " has not been saved. Do you want to save the changes?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Save(null, null);
                }
                if (result == DialogResult.Cancel)
                {
                    
                }
            }
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(open.FileName) == ".txt")
                {
                    editorBox.LoadFile(open.FileName, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    if (Path.GetExtension(open.FileName) == ".rtfc")
                    {
                            ZipFile zip = ZipFile.Read(open.FileName);
                            zip.ExtractProgress += new System.EventHandler<ExtractProgressEventArgs>(ExtractProgress);
                            zip.ExtractAll(Path.GetTempPath(),ExtractExistingFileAction.OverwriteSilently);
                            progressBar1.Hide();
                            label1.Show();
                            editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetFileName(open.FileName));
                            label1.Hide();
                            File.Delete(Path.GetTempPath() + @"\" + Path.GetFileName(open.FileName));
                            UseWaitCursor = false;
                    }
                    else
                    {
                        if (Path.GetExtension(open.FileName) == ".rtfe")
                        {
                             var pwform = new GUI.ReqPswd(open.FileName,false);
                             if (pwform.ShowDialog() == DialogResult.OK)
                             {
                                 ZipFile zip = ZipFile.Read(open.FileName);
                                 zip.ExtractProgress += new System.EventHandler<ExtractProgressEventArgs>(ExtractProgress);
                                 zip.Password = pwform.pw;
                                 zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                 try
                                 {
                                     zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                                     progressBar1.Hide();
                                     label1.Show();
                                     editorBox.LoadFile(Path.GetTempPath() + @"\" + Path.GetFileName(open.FileName));
                                     label1.Hide();
                                     File.Delete(Path.GetTempPath() + @"\" + Path.GetFileName(open.FileName));
                                     UseWaitCursor = false;
                                 }
                                 catch
                                 {
                                     MessageBox.Show("Password Incorect");
                                 }
                             }
                        }
                        else
                        {
                            try
                            {
                                editorBox.LoadFile(open.FileName, RichTextBoxStreamType.RichText);
                            }
                            catch
                            {
                                editorBox.LoadFile(open.FileName, RichTextBoxStreamType.PlainText);
                            }
                        }
                    }
                }
                edited = false;
                doc = open.FileName;
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
                if (doc == "Untitled")
                {
                    save.Title = "Saving file: " + doc;
                    if (save.ShowDialog() == DialogResult.OK)
                    {
                        if (Path.GetExtension(save.FileName) == ".txt")
                        {
                            editorBox.SaveFile(save.FileName, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            if (Path.GetExtension(save.FileName) == ".rtfc")
                            {
                                string txt;
                                txt = Path.GetTempPath() + Path.GetFileName(save.FileName);
                                editorBox.SaveFile(txt, RichTextBoxStreamType.RichText);
                                ZipFile zip = new ZipFile();
                                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                zip.AddFile(txt);
                                zip.Save(save.FileName);
                                progressBar1.Hide();
                                UseWaitCursor = false;
                            }
                            else
                            {
                                if (Path.GetExtension(save.FileName) == ".rtfe")
                                {
                                    var pwform = new GUI.ReqPswd(save.FileName,true);
                                    if (pwform.ShowDialog() == DialogResult.OK)
                                    {
                                        editorBox.SaveFile(Path.GetTempPath() + Path.GetTempPath() + Path.GetFileName(save.FileName), RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                        zip.Password = pwform.pw;
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                        zip.AddFile(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                        zip.Save(save.FileName + ".tmp");
                                        File.Delete(save.FileName);
                                        File.Delete(Path.GetTempPath() + Path.GetTempPath() + Path.GetFileName(save.FileName));
                                        File.Move(save.FileName + ".tmp", save.FileName);
                                        progressBar1.Hide();
                                        UseWaitCursor = false;
                                    }
                                }
                                else
                                {
                                    editorBox.SaveFile(save.FileName, RichTextBoxStreamType.RichText);
                                }
                            }
                        }
                        doc = save.FileName;
                        edited = false;
                    }
                }
                else
                {
                    if (Path.GetExtension(doc) == ".txt")
                    {
                        editorBox.SaveFile(doc, RichTextBoxStreamType.PlainText);
                    }
                    else
                    {
                        if (Path.GetExtension(doc) == ".rtfc")
                        {
                            editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(doc), RichTextBoxStreamType.RichText);
                            ZipFile zip = new ZipFile();
                            zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                            zip.AddFile(Path.GetTempPath() + Path.GetFileName(doc));
                            try
                            { zip.Save(doc); }
                            catch { MessageBox.Show("Unable to save " + doc); }
                            progressBar1.Hide();
                            UseWaitCursor = false;
                        }
                        else
                        {
                            if (Path.GetExtension(doc) == ".rtfe")
                            {
                                var pwform = new GUI.ReqPswd(doc, true);
                                if (pwform.ShowDialog() == DialogResult.OK)
                                {
                                    editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(doc), RichTextBoxStreamType.RichText);
                                    ZipFile zip = new ZipFile();
                                    zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                    zip.Password = pwform.pw;
                                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                    zip.AddFile(Path.GetTempPath() + Path.GetFileName(doc));
                                    zip.Save(doc);
                                    File.Delete(Path.GetTempPath() + Path.GetFileName(doc));
                                    progressBar1.Hide();
                                    UseWaitCursor = false;
                                }
                            }
                            else
                            {
                                editorBox.SaveFile(save.FileName, RichTextBoxStreamType.RichText);
                            }
                        }
                    }
                    edited = false;
                }
            }
        }
        //Saves a file as a new file
        private void SaveAs(object sender, EventArgs e)
        {
            string themepath;
            if (Properties.Settings.Default.UsingVariant)
            {
                themepath =  Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\" + Properties.Settings.Default.ThemeVariant;
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) +@"/Resources/Themes/" + Properties.Settings.Default.Theme;
            }
            saveToolStripButton.Image =  Image.FromFile(themepath + @"/save-as.png");
            save.Title = "Saving file: " + Path.GetTempPath() + Path.GetFileName(doc);
             if (save.ShowDialog() == DialogResult.OK)
                    {
                        if (Path.GetExtension(save.FileName) == ".txt")
                        {
                            editorBox.SaveFile(save.FileName, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            if (Path.GetExtension(save.FileName) == ".rtfc")
                            {
                                editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(save.FileName), RichTextBoxStreamType.RichText);
                                ZipFile zip = new ZipFile();
                                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                zip.AddFile(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                File.Delete(save.FileName);
                                zip.Save(save.FileName);
                                progressBar1.Hide();
                                UseWaitCursor = false;
                            }
                            else
                            {
                                if (Path.GetExtension(save.FileName) == ".rtfe")
                                {
                                    var pwform = new GUI.ReqPswd(save.FileName, true);
                                    if (pwform.ShowDialog() == DialogResult.OK)
                                    {
                                        editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(save.FileName), RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                        zip.Password = pwform.pw;
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                        zip.AddFile(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                        zip.Save(save.FileName);
                                        File.Delete(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                        progressBar1.Hide();
                                        UseWaitCursor = false;
                                    }
                                }
                                else
                                {
                                    editorBox.SaveFile(save.FileName, RichTextBoxStreamType.RichText);
                                }
                            }
                        }
                        doc = save.FileName;
                        edited = false;
                    
                saveToolStripButton.Image = Image.FromFile(themepath + @"/save.png");
            }
            else
            {
                saveToolStripButton.Image = Image.FromFile(themepath + @"/save.png");
            }
        }
        //Undo
        private void Undo(object sender, EventArgs e) { editorBox.Undo(); }
        //Redo
        private void Redo(object sender, EventArgs e) { editorBox.Redo(); }
        //Cut
        private void Cut(object sender, EventArgs e) { editorBox.Cut(); }
        //Copy
        private void Copy(object sender, EventArgs e) { editorBox.Copy(); }
        //Paste
        private void Paste(object sender, EventArgs e) { editorBox.Paste(); }
        //Select All
        private void SelectAll(object sender, EventArgs e) { editorBox.SelectAll(); }
        //Insert date and time
        private void dateAndTimeToolStripMenuItem_Click(object sender, EventArgs e) { editorBox.AppendText(DateTime.Now.ToString()); }
        //Opens about window
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) { Form about = new GUI.AboutTomText(); about.Show(); }
        private void boldStripButton_Click(object sender, EventArgs e)
        {
            if (boldToolStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style | FontStyle.Bold); editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style ^ FontStyle.Bold); editorBox.SelectionFont = NewFont;
            }
        }
        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            if (italicToolStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style | FontStyle.Italic); editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style ^ FontStyle.Italic); editorBox.SelectionFont = NewFont;
            }
        }
        private void underlineStripButton_Click(object sender, EventArgs e)
        {
            if (underlineStripButton.Checked == true)
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style | FontStyle.Underline); editorBox.SelectionFont = NewFont;
            }
            else
            {
                Font NewFont = new Font(editorBox.Font.FontFamily, editorBox.SelectionFont.Size, editorBox.SelectionFont.Style ^ FontStyle.Underline); editorBox.SelectionFont = NewFont;
            }
        }
        //This code goes through all MenuStrips and ToolStrips, Getting the contents and setting the images for each
        //control, this makes theming easy as all images are dynamic and can be modified with little effort.
        private void RefreshGUI()
        {
            string name = "";
            //Reads theme .json files
            JObject themeinfo = null;
            string themepath;
            string imgtype = null;
            if (Properties.Settings.Default.HiDPI)
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
            }
            else
            {
                themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
            }
            try
            {
                themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
            }
            catch
            {
                MessageBox.Show(@"The theme you've selected doesn't have a valid theme configuration file (theme.json), Using embedded theme and reseting theme setting", "Theming Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Properties.Settings.Default.Reset();
                if (Properties.Settings.Default.HiDPI)
                {
                    themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme + @"\Hi-DPI";
                }
                else
                {
                    themepath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Resources\Themes\" + Properties.Settings.Default.Theme;
                }
                try
                {
                    themeinfo = JObject.Parse(File.ReadAllText(themepath + @"\theme.json"));
                }
                catch
                {
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
            //Sets window title
            this.Text = doc + " - " + Properties.Settings.Default.AppName;
            //Sets window resolution
            this.Size = Properties.Settings.Default.UIRes;
            //Sets the window icon if the theme specifies 
            if (themeinfo != null && themeinfo.GetValue("Changes icons?").ToString() == "true")
            {
                this.Icon = new Icon(Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/icon.ico");
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
                Console.WriteLine(@"Begining initialisation of: " + bt.Name);
                bt.Size = new Size(int.Parse(themeinfo.GetValue("Image Size").ToString()), int.Parse(themeinfo.GetValue("Image Size").ToString()));
                name = bt.Text;
                name = name.Replace("&", "");
                name = name.Replace(" ", "-");
                name = name.Replace(".", "");
                try
                {
                    if (themeinfo.GetValue("Theme Name").ToString() == "embedded")
                    {
                        Console.WriteLine("Using embedded theme. Skipping initialisation for " + bt.Name);
                    }
                    else
                    {
                        bt.Image = Image.FromFile(themepath + @"/" + name.ToLower() + imgtype);
                        Console.WriteLine("Initialised: " + bt.Name + " with image: " + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                    }
                }
                catch
                {
                    bt.Image = Properties.Resources.Blank;
                }
            }
                foreach (ToolStripMenuItem item in contextMenuStrip1.Items.OfType<ToolStripMenuItem>())
                {
                    name = item.Text;
                    name = name.Replace("&", "");
                    name = name.Replace(" ", "-");
                    name = name.Replace(".", "");
                    try
                    {
                        if (themeinfo.GetValue("Theme Name").ToString() == "embedded")
                        {
                            Console.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
                        }
                        else
                        {
                            try
                            {
                                item.Image = Image.FromFile(themepath + @"/" + name.ToLower() + imgtype);
                                Console.WriteLine("Initialised: " + item.Name + " with image: " + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                                try
                                {
                                    if (imgtype == ".bmp")
                                    {
                                        item.ImageTransparentColor = Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                                    }
                                }
                                catch { };
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
                                catch { }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        item.Font = Properties.Settings.Default.UIFont;
                    }
                    catch
                    {
                        try
                        {
                            if (themeinfo.GetValue("Uses Embedded").ToString() == "false")
                            {
                                item.Image = Properties.Resources.Blank;
                                MessageBox.Show("There was an error while trying to theme a menu item. Using failsafe icon. Ensure a file exists at: " + Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("There was an error while trying to theme a menu item. Using failsafe icon. Ensure a file exists at: " + Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                        }
                    }
                }
            
        }
        private void themeToolStrips(ToolStrip strip, String imgtype, String name, JObject themeinfo, string themepath)
        {
            try
            {
                if (themeinfo.GetValue("Theme Style").ToString() == "1") { strip.RenderMode = ToolStripRenderMode.System; }
                else
                {
                    if (themeinfo.GetValue("Theme Style").ToString() == "2") { strip.RenderMode = ToolStripRenderMode.Professional; }
                    else { if (themeinfo.GetValue("Theme Style").ToString() == "0") { strip.RenderMode = ToolStripRenderMode.ManagerRenderMode; } }
                }
                //Enables Hi-DPI images if specified
                strip.ImageScalingSize = new Size(int.Parse(themeinfo.GetValue("Image Size").ToString()), int.Parse(themeinfo.GetValue("Image Size").ToString()));
            }
            catch (Exception)
            {
                
            }
            //Gets all toolstrip buttons
            Console.WriteLine("Begining initialisation of: " + strip.Name);
            foreach (ToolStripButton item in strip.Items.OfType<ToolStripButton>())
            {
                name = item.Text;
                name = name.Replace("&", "");
                name = name.Replace(" ", "-");
                name = name.Replace(".", "");
                try
                {
                    if (themeinfo.GetValue("Theme Name").ToString() == "embedded")
                    {
                        Console.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
                    }
                    else
                    {
                        item.Image = Image.FromFile(themepath + @"/" + name.ToLower() + imgtype);
                        try
                        {
                            if(Convert.ToBoolean(themeinfo.GetValue("Auto-scale images?".ToString())))
                            {
                                item.ImageScaling =  ToolStripItemImageScaling.SizeToFit;
                            }
                            else
                            {
                                item.ImageScaling =  ToolStripItemImageScaling.None;
                            }
                        }
                        catch { }
                        Console.WriteLine("Initialised: " + item.Name + " with image: " + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                        try
                        {
                            if (imgtype == ".bmp")
                            {
                                item.ImageTransparentColor = Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                            }
                        }
                        catch { };
                    }
                    item.Font = Properties.Settings.Default.UIFont;
                }
                catch
                {
                    if (themeinfo.GetValue("Uses Embedded").ToString() == "false")
                    {
                        item.Image = Properties.Resources.Blank;
                        MessageBox.Show("There was an error while trying to theme a menu item. Using failsafe icon. Ensure a file exists at: " + Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                    }

                }
            }
            foreach (ToolStripDropDownButton dd in strip.Items.OfType<ToolStripDropDownButton>())
            {

                if (dd.HasDropDownItems == true)
                {
                    Console.WriteLine("Begining initialisation of: " + dd.Name);
                    foreach (ToolStripMenuItem item in dd.DropDownItems.OfType<ToolStripMenuItem>())
                    {
                        name = item.Text;
                        name = name.Replace("&", "");
                        name = name.Replace(" ", "-");
                        name = name.Replace(".", "");
                        try
                        {
                            if (themeinfo.GetValue("Theme Name").ToString() == "embedded")
                            {
                                Console.WriteLine("Using embedded theme. Skipping initialisation for " + item.Name);
                            }
                            else
                            {
                                try
                                {
                                    item.Image = Image.FromFile(themepath + @"/" + name.ToLower() + imgtype);
                                    Console.WriteLine("Initialised: " + item.Name + " with image: " + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                                    try
                                    {
                                        if (imgtype == ".bmp")
                                        {
                                            item.ImageTransparentColor = Color.FromName(themeinfo.GetValue("BMP Back Colour").ToString());
                                        }
                                    }
                                    catch { };
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
                                    catch { }
                                }
                                catch (Exception)
                                {
                                   
                                }
                            }
                            item.Font = Properties.Settings.Default.UIFont;
                        }
                        catch
                        {
                            try
                            {
                                if (themeinfo.GetValue("Uses Embedded").ToString() == "false")
                                {
                                    item.Image = Properties.Resources.Blank;
                                    MessageBox.Show("There was an error while trying to theme a menu item. Using failsafe icon. Ensure a file exists at: " + Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("There was an error while trying to theme a menu item. Using failsafe icon. Ensure a file exists at: " + Path.GetDirectoryName(Application.ExecutablePath) + @"/Resources/Themes/" + Properties.Settings.Default.Theme + @"/" + name.ToLower() + imgtype);
                            }
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
                if (MessageBox.Show("To select the embedded theme, the app must relaunch. Do you want to relaunch now?","Relaunch Required", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    editorBox.SaveFile("temp");
                    edited = false;
                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                    Close();
                }
                else
                {
                    
                }
            }
        }
        private void fontComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Font NewFont = new Font(fontComboBox.SelectedItem.ToString(), editorBox.SelectionFont.Size, editorBox.SelectionFont.Style); editorBox.SelectionFont = NewFont;
        }
        private void fontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            float NewSize; float.TryParse(fontSizeComboBox.SelectedItem.ToString(), out NewSize); Font NewFont = new Font(editorBox.SelectionFont.Name, NewSize, editorBox.SelectionFont.Style); editorBox.SelectionFont = NewFont;
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
        #endregion
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (edited == true)
            {
                DialogResult result = MessageBox.Show("The document " + Path.GetTempPath() + Path.GetFileName(doc) + " has not been saved. Do you want to save the changes?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    if (doc == "Untitled")
                    {
                        save.Title = "Saving file: " + doc;
                        if (save.ShowDialog() == DialogResult.OK)
                        {
                            if (Path.GetExtension(save.FileName) == ".txt")
                            {
                                editorBox.SaveFile(save.FileName, RichTextBoxStreamType.PlainText);
                            }
                            else
                            {
                                if (Path.GetExtension(save.FileName) == ".rtfc")
                                {
                                    string txt;
                                    txt = Path.GetTempPath() + Path.GetFileName(save.FileName);
                                    editorBox.SaveFile(txt, RichTextBoxStreamType.RichText);
                                    ZipFile zip = new ZipFile();
                                    zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                    zip.AddFile(txt);
                                    zip.Save(save.FileName);
                                    progressBar1.Hide();
                                    UseWaitCursor = false;
                                }
                                else
                                {
                                    if (Path.GetExtension(save.FileName) == ".rtfe")
                                    {
                                        var pwform = new GUI.ReqPswd(save.FileName, true);
                                        if (pwform.ShowDialog() == DialogResult.OK)
                                        {
                                            editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(save.FileName), RichTextBoxStreamType.RichText);
                                            ZipFile zip = new ZipFile();
                                            zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                            zip.Password = pwform.pw;
                                            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                            zip.AddFile(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                            zip.Save(save.FileName + ".tmp");
                                            File.Delete(save.FileName);
                                            File.Delete(Path.GetTempPath() + Path.GetFileName(save.FileName));
                                            File.Move(save.FileName + ".tmp", save.FileName);
                                            progressBar1.Hide();
                                            UseWaitCursor = false;

                                        }
                                    }
                                    else
                                    {
                                        editorBox.SaveFile(save.FileName, RichTextBoxStreamType.RichText);
                                    }
                                }
                            }
                            doc = save.FileName;
                            edited = false;
                        }
                    }
                    else
                    {
                        if (Path.GetExtension(doc) == ".txt")
                        {
                            editorBox.SaveFile(doc, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            if (Path.GetExtension(doc) == ".rtfc")
                            {
                                editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(doc), RichTextBoxStreamType.RichText);
                                ZipFile zip = new ZipFile();
                                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                zip.AddFile(Path.GetTempPath() + Path.GetFileName(doc));
                                try
                                { zip.Save(doc); }
                                catch { MessageBox.Show("Unable to save " + doc); }
                                progressBar1.Hide();
                                UseWaitCursor = false;
                            }
                            else
                            {
                                if (Path.GetExtension(doc) == ".rtfe")
                                {
                                    var pwform = new GUI.ReqPswd(doc, true);
                                    if (pwform.ShowDialog() == DialogResult.OK)
                                    {
                                        editorBox.SaveFile(Path.GetTempPath() + Path.GetFileName(doc), RichTextBoxStreamType.RichText);
                                        ZipFile zip = new ZipFile();
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(SaveProgress);
                                        zip.Password = pwform.pw;
                                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                        zip.AddFile(Path.GetTempPath() + Path.GetFileName(doc));
                                        zip.Save(doc + ".tmp");
                                        File.Delete(doc);
                                        File.Move(doc + ".tmp", doc);
                                        progressBar1.Hide();
                                        UseWaitCursor = false;
                                    }
                                }
                                else
                                {
                                    editorBox.SaveFile(save.FileName, RichTextBoxStreamType.RichText);
                                }
                            }
                        }
                        edited = false;
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
            }
            catch
            {

            }

        }
        private void Exit(object sender, EventArgs e)
        {
            Close();
        }


        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            double zoom = trackBar1.Value;
            zoom = zoom / 100;
            editorBox.ZoomFactor = float.Parse(zoom.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double val = trackBar1.Value +10;
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
            insert.Filter = "All Insertable Files (*.*)|*.*";
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
                        Image img;
                        img = Image.FromFile(path);
                        Clipboard.SetImage(img);
                        try
                        {
                            editorBox.Paste();
                        }
                        catch { }
                        
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
                            object item = File.ReadAllBytes(path);
                            Clipboard.SetDataObject(item);
                            editorBox.Paste();
                            Clipboard.SetDataObject(clpdata);
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

        private void EditorForm_DragDrop(object sender, DragEventArgs e)
        {
            
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void wordWrapToolStripButton_Click(object sender, EventArgs e)
        {
            editorBox.WordWrap = wordWrapToolStripButton.Checked;
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {

        }
    }
}

