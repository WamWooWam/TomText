using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;

namespace Updater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStartDownload_Click()
        {
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri("https://dl.dropboxusercontent.com/s/zn987zkl6ypg795/latest.zip"), "update.zip");

        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            double KBIn = bytesIn / 1024;
            double totalKB = totalBytes / 1024;
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            label2.Text = "Downloading: " + Math.Round(KBIn).ToString() + "kb /" + Math.Round(totalKB).ToString() + "kb";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs d)
        {
            System.Threading.Thread.Sleep(100);
            using (ZipFile zip = ZipFile.Read("update.zip"))
            {
                foreach (ZipEntry e in zip)
                {
                    e.Extract(Directory.GetCurrentDirectory(),ExtractExistingFileAction.OverwriteSilently);  // overwrite == true
                }
            }
            File.Delete("update.zip");
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            btnStartDownload_Click();
        }
    }
}
