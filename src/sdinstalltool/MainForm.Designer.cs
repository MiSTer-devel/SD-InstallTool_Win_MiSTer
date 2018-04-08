namespace SDInstallTool
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboBoxDrives = new System.Windows.Forms.ComboBox();
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelStats = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelFileName = new System.Windows.Forms.Label();
            this.labelDriveTitle = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLogWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textDiskSize = new System.Windows.Forms.TextBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonFull = new System.Windows.Forms.Button();
            this.buttonUpdateBoot = new System.Windows.Forms.Button();
            this.buttonWipe = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonUpdateAll = new System.Windows.Forms.Button();
            this.cancelOperationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxDrives
            // 
            this.comboBoxDrives.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDrives.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDrives.ForeColor = System.Drawing.SystemColors.MenuText;
            this.comboBoxDrives.FormattingEnabled = true;
            this.comboBoxDrives.Location = new System.Drawing.Point(69, 18);
            this.comboBoxDrives.Name = "comboBoxDrives";
            this.comboBoxDrives.Size = new System.Drawing.Size(234, 21);
            this.comboBoxDrives.TabIndex = 0;
            this.comboBoxDrives.SelectedIndexChanged += new System.EventHandler(this.driveSelectionChanged);
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Location = new System.Drawing.Point(77, 31);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.ReadOnly = true;
            this.textBoxFileName.Size = new System.Drawing.Size(349, 20);
            this.textBoxFileName.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus,
            this.labelStats});
            this.statusStrip1.Location = new System.Drawing.Point(0, 197);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(448, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(44, 17);
            this.labelStatus.Text = "Started";
            // 
            // labelStats
            // 
            this.labelStats.Name = "labelStats";
            this.labelStats.Size = new System.Drawing.Size(0, 17);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(18, 34);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(39, 13);
            this.labelFileName.TabIndex = 7;
            this.labelFileName.Text = "Image:";
            // 
            // labelDriveTitle
            // 
            this.labelDriveTitle.AutoSize = true;
            this.labelDriveTitle.Location = new System.Drawing.Point(9, 22);
            this.labelDriveTitle.Name = "labelDriveTitle";
            this.labelDriveTitle.Size = new System.Drawing.Size(35, 13);
            this.labelDriveTitle.TabIndex = 8;
            this.labelDriveTitle.Text = "Drive:";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(8, 181);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(431, 10);
            this.progressBar.TabIndex = 9;
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(448, 24);
            this.menuStripMain.TabIndex = 12;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showLogWindowToolStripMenuItem,
            this.cancelOperationToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // showLogWindowToolStripMenuItem
            // 
            this.showLogWindowToolStripMenuItem.Name = "showLogWindowToolStripMenuItem";
            this.showLogWindowToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showLogWindowToolStripMenuItem.Text = "Show Log Window";
            this.showLogWindowToolStripMenuItem.Click += new System.EventHandler(this.showLogWindowToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textDiskSize);
            this.groupBox1.Controls.Add(this.buttonRefresh);
            this.groupBox1.Controls.Add(this.comboBoxDrives);
            this.groupBox1.Controls.Add(this.labelDriveTitle);
            this.groupBox1.Location = new System.Drawing.Point(8, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 82);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "SD Card";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Size:";
            // 
            // textDiskSize
            // 
            this.textDiskSize.Location = new System.Drawing.Point(69, 51);
            this.textDiskSize.Name = "textDiskSize";
            this.textDiskSize.ReadOnly = true;
            this.textDiskSize.Size = new System.Drawing.Size(349, 20);
            this.textDiskSize.TabIndex = 18;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(351, 17);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(66, 23);
            this.buttonRefresh.TabIndex = 17;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonFull
            // 
            this.buttonFull.Location = new System.Drawing.Point(8, 148);
            this.buttonFull.Name = "buttonFull";
            this.buttonFull.Size = new System.Drawing.Size(80, 23);
            this.buttonFull.TabIndex = 17;
            this.buttonFull.Text = "Full install";
            this.buttonFull.UseVisualStyleBackColor = true;
            this.buttonFull.Click += new System.EventHandler(this.buttonFull_Click);
            // 
            // buttonUpdateBoot
            // 
            this.buttonUpdateBoot.Location = new System.Drawing.Point(97, 148);
            this.buttonUpdateBoot.Name = "buttonUpdateBoot";
            this.buttonUpdateBoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonUpdateBoot.Size = new System.Drawing.Size(80, 23);
            this.buttonUpdateBoot.TabIndex = 18;
            this.buttonUpdateBoot.Text = "Update Boot";
            this.buttonUpdateBoot.UseVisualStyleBackColor = true;
            this.buttonUpdateBoot.Click += new System.EventHandler(this.buttonUpdateBoot_Click);
            // 
            // buttonWipe
            // 
            this.buttonWipe.Location = new System.Drawing.Point(359, 148);
            this.buttonWipe.Name = "buttonWipe";
            this.buttonWipe.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonWipe.Size = new System.Drawing.Size(80, 23);
            this.buttonWipe.TabIndex = 19;
            this.buttonWipe.Text = "Wipe";
            this.buttonWipe.UseVisualStyleBackColor = true;
            this.buttonWipe.Click += new System.EventHandler(this.buttonWipe_Click);
            // 
            // buttonUpdateAll
            // 
            this.buttonUpdateAll.Location = new System.Drawing.Point(187, 148);
            this.buttonUpdateAll.Name = "buttonUpdateAll";
            this.buttonUpdateAll.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonUpdateAll.Size = new System.Drawing.Size(123, 23);
            this.buttonUpdateAll.TabIndex = 20;
            this.buttonUpdateAll.Text = "Update Boot+Files";
            this.buttonUpdateAll.UseVisualStyleBackColor = true;
            this.buttonUpdateAll.Click += new System.EventHandler(this.buttonUpdateAll_Click);
            // 
            // cancelOperationToolStripMenuItem
            // 
            this.cancelOperationToolStripMenuItem.Enabled = false;
            this.cancelOperationToolStripMenuItem.Name = "cancelOperationToolStripMenuItem";
            this.cancelOperationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cancelOperationToolStripMenuItem.Text = "Cancel operation";
            this.cancelOperationToolStripMenuItem.Click += new System.EventHandler(this.cancelOperationToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 219);
            this.Controls.Add(this.buttonUpdateAll);
            this.Controls.Add(this.buttonWipe);
            this.Controls.Add(this.buttonUpdateBoot);
            this.Controls.Add(this.buttonFull);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStripMain);
            this.Controls.Add(this.textBoxFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MainMenuStrip = this.menuStripMain;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "MiSTer SD Card Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDrives;
        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.Label labelDriveTitle;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textDiskSize;
        private System.Windows.Forms.Button buttonFull;
        private System.Windows.Forms.Button buttonUpdateBoot;
        private System.Windows.Forms.Button buttonWipe;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem showLogWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel labelStats;
        private System.Windows.Forms.Button buttonUpdateAll;
        private System.Windows.Forms.ToolStripMenuItem cancelOperationToolStripMenuItem;
    }
}

