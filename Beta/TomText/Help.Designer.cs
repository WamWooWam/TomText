namespace TomText
{
    partial class Help
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("The Sidebars");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("The Menu Bar");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("The Status Bar", 1, 1);
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("The Main Window", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("General Tab", 1, 1);
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Updates Tab", 1, 1);
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Defaults Tab", 1, 1);
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Themes Tab", 1, 1);
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Misc. Tab", 1, 1);
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("The Settings Window", new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8,
            treeNode9});
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Section 1: An Overview", new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode10});
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Tom Text Help", new System.Windows.Forms.TreeNode[] {
            treeNode11});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Help));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1168, 739);
            this.splitContainer1.SplitterDistance = 237;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            treeNode1.ImageKey = "help-contents.png";
            treeNode1.Name = "Node4";
            treeNode1.SelectedImageIndex = 1;
            treeNode1.Text = "The Sidebars";
            treeNode2.ImageKey = "help-contents.png";
            treeNode2.Name = "Node5";
            treeNode2.SelectedImageIndex = 1;
            treeNode2.Text = "The Menu Bar";
            treeNode3.ImageIndex = 1;
            treeNode3.Name = "Node6";
            treeNode3.SelectedImageIndex = 1;
            treeNode3.Text = "The Status Bar";
            treeNode4.ImageIndex = 0;
            treeNode4.Name = "Node2";
            treeNode4.Text = "The Main Window";
            treeNode5.ImageIndex = 1;
            treeNode5.Name = "Node7";
            treeNode5.SelectedImageIndex = 1;
            treeNode5.Text = "General Tab";
            treeNode6.ImageIndex = 1;
            treeNode6.Name = "Node8";
            treeNode6.SelectedImageIndex = 1;
            treeNode6.Text = "Updates Tab";
            treeNode7.ImageIndex = 1;
            treeNode7.Name = "Node9";
            treeNode7.SelectedImageIndex = 1;
            treeNode7.Text = "Defaults Tab";
            treeNode8.ImageIndex = 1;
            treeNode8.Name = "Node10";
            treeNode8.SelectedImageIndex = 1;
            treeNode8.Text = "Themes Tab";
            treeNode9.ImageIndex = 1;
            treeNode9.Name = "Node11";
            treeNode9.SelectedImageIndex = 1;
            treeNode9.Text = "Misc. Tab";
            treeNode10.Name = "Node3";
            treeNode10.Text = "The Settings Window";
            treeNode11.Name = "Node1";
            treeNode11.Text = "Section 1: An Overview";
            treeNode12.ImageIndex = 0;
            treeNode12.Name = "Node0";
            treeNode12.Text = "Tom Text Help";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode12});
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(237, 739);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "system-help.png");
            this.imageList1.Images.SetKeyName(1, "help-contents.png");
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(927, 739);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Url = new System.Uri("http://wamwoowam.tk/TomText/help.htm", System.UriKind.Absolute);
            // 
            // Help
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1168, 739);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Help";
            this.Load += new System.EventHandler(this.Help_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ImageList imageList1;

    }
}
