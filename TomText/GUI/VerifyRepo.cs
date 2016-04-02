using System;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TomText.GUI
{
    public partial class VerifyRepo : Form
    {
        public VerifyRepo()
        {
            InitializeComponent();
        }

        private void VerifyRepo_Load(object sender, EventArgs e)
        {
            JObject update = JObject.Parse(File.ReadAllText(Path.GetTempPath() + "/repo.json"));
            textBox1.Text = update.GetValue("Repo Name").ToString();
            richTextBox1.Text = update.GetValue("Repo Description").ToString();
            dynamic obj = JsonConvert.DeserializeObject(update.GetValue("Repo versions").ToString());
            MessageBox.Show(obj.ToString());
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
