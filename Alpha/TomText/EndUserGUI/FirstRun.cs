using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace TomText.EndUserGUI
{
    public partial class FirstRun : Form
    {
        public FirstRun()
        {
            InitializeComponent();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void FirstRun_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("Generating Font List");
            Properties.Settings.Default.generatedListOfFonts.Clear();
            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                if (fontFamily.IsStyleAvailable(FontStyle.Regular))
                {
                    Properties.Settings.Default.generatedListOfFonts.Add(fontFamily.Name);
                    Console.WriteLine("Added font           : " + fontFamily.Name);
                    label3.Font = new Font(fontFamily, 12);
                    label2.Text = ("Added font: " + fontFamily.Name);
                    Thread.Sleep(15);
                }
                if (fontFamily.IsStyleAvailable(FontStyle.Bold))
                {
                    Properties.Settings.Default.generatedListOfFonts.Add(fontFamily.Name + " Bold");
                    Console.WriteLine("Added font bold      : " + fontFamily.Name);
                    label3.Font = new Font(fontFamily, 12,FontStyle.Bold);
                    label2.Text = ("Added font: " + fontFamily.Name + " Bold");
                    Thread.Sleep(15);
                }
                if (fontFamily.IsStyleAvailable(FontStyle.Italic))
                {
                    Properties.Settings.Default.generatedListOfFonts.Add(fontFamily.Name + " Italic");
                    Console.WriteLine("Added font italic    : " + fontFamily.Name);
                    label3.Font = new Font(fontFamily, 12,FontStyle.Italic);
                    label2.Text = ("Added font: " + fontFamily.Name + " Italic");
                    Thread.Sleep(15);
                }
                if (fontFamily.IsStyleAvailable(FontStyle.Underline))
                {
                    Properties.Settings.Default.generatedListOfFonts.Add(fontFamily.Name + " Underline");
                    Console.WriteLine("Added font underline : " + fontFamily.Name);
                    label3.Font = new Font(fontFamily, 12, FontStyle.Underline);
                    label2.Text = ("Added font: " + fontFamily.Name + " Underline");
                    Thread.Sleep(15);
                }

            }
                
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Properties.Settings.Default.FirstStartup = false;
            Properties.Settings.Default.Save();
            Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location, "");
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
