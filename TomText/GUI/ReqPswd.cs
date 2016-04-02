using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TomText.GUI
{
    public partial class ReqPswd : Form
    {
        string path;
        public string pw {get;set;}
        public ReqPswd(string filepath, bool isencrypting)
        {
            InitializeComponent();
            path = filepath;
            Text = "Password for: " + Path.GetFileName(path);
            if (isencrypting)
            {
                label1.Text = "Enter password to encrypt " + Path.GetFileName(path);
            }
            else
            {
                label1.Text = "Enter password to decrypt " + Path.GetFileName(path);
            }
        }

        private void ReqPswd_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Password cannot be blank!");
            }
            else
            {
                if (textBox1.Text.Length < 7)
                {
                    MessageBox.Show("Password is too short!");
                }
                else
                {
                    string[] commonpwds = { "123456", "password", "12345678", "qwerty", "12345", "123456789", "football", "1234", "1234567", "baseball" };
                    if (commonpwds.Contains<string>(textBox1.Text))
                    {
                        MessageBox.Show("Password is too common!");
                    }
                    else
                    {
                        this.DialogResult = DialogResult.OK;
                        pw = textBox1.Text;
                        Close();
                    }
                }
      
            }
        }
    }
}
