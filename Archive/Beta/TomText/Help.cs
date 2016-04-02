using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TomText
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            MessageBox.Show("This is unavalable right now. Please wait for it to be finished OR change your update channel to Alpha");
            Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            if (node.Name == "Node2")
            {
                webBrowser1.Navigate("http://wamwoowam.tk/TomText/help.htm#_The_Main_Window");
            }
            if (node.Name == "Node4")
            {
                webBrowser1.Navigate("http://wamwoowam.tk/TomText/help.htm#_The_Sidebars");
            }
            if (node.Name == "Node1")
            {
                webBrowser1.Navigate("http://wamwoowam.tk/TomText/help.htm#_Section_1:_An");
            }
        }
    }
}
