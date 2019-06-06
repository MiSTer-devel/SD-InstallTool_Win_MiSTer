namespace MiSTerConfigurator
{
    partial class ConfiguratorForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Main MiSTer");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Boot Menu");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Computer");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Console");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Arcade");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Utility");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Cores", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6});
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Filters");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Fonts");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Cheats");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Game Boy Palettes");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Updater");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Scripts");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("SoundFont Installer");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfiguratorForm));
            this.tabControlSections = new System.Windows.Forms.TabControl();
            this.tabPageWizard = new System.Windows.Forms.TabPage();
            this.linkLabelUnixInstaller = new System.Windows.Forms.LinkLabel();
            this.chkEnableWiFi = new System.Windows.Forms.CheckBox();
            this.chkEnableSamba = new System.Windows.Forms.CheckBox();
            this.pnlSamba = new System.Windows.Forms.Panel();
            this.txtSambaPassword = new System.Windows.Forms.TextBox();
            this.lblSambaPassword = new System.Windows.Forms.Label();
            this.txtSambaUserName = new System.Windows.Forms.TextBox();
            this.lblSambaUserName = new System.Windows.Forms.Label();
            this.btnOptimalPreset = new System.Windows.Forms.Button();
            this.btnCompatibilityPreset = new System.Windows.Forms.Button();
            this.pnlWiFi = new System.Windows.Forms.Panel();
            this.txtWiFiPassword = new System.Windows.Forms.TextBox();
            this.lblWiFiPassword = new System.Windows.Forms.Label();
            this.txtWiFiSSID = new System.Windows.Forms.TextBox();
            this.lblWiFiSSID = new System.Windows.Forms.Label();
            this.txtWiFiCountry = new System.Windows.Forms.TextBox();
            this.lblWiFiCountry = new System.Windows.Forms.Label();
            this.cmbVSyncMode = new System.Windows.Forms.ComboBox();
            this.lblVSyncMode = new System.Windows.Forms.Label();
            this.cmbScalingMode = new System.Windows.Forms.ComboBox();
            this.lblScalingMode = new System.Windows.Forms.Label();
            this.lblEnablePAL_NTSC = new System.Windows.Forms.Label();
            this.chkEnablePAL_NTSC = new System.Windows.Forms.CheckBox();
            this.cmbVideoMode = new System.Windows.Forms.ComboBox();
            this.lblVideoMode = new System.Windows.Forms.Label();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btn1ClickSetup = new System.Windows.Forms.Button();
            this.tabPageCores = new System.Windows.Forms.TabPage();
            this.txtUtilityDir = new System.Windows.Forms.TextBox();
            this.lblUtilityDir = new System.Windows.Forms.Label();
            this.txtArcadeDir = new System.Windows.Forms.TextBox();
            this.lblArcadeDir = new System.Windows.Forms.Label();
            this.txtConsoleDir = new System.Windows.Forms.TextBox();
            this.lblConsoleDir = new System.Windows.Forms.Label();
            this.txtComputerDir = new System.Windows.Forms.TextBox();
            this.lblComputerDir = new System.Windows.Forms.Label();
            this.btnDownloadCores = new System.Windows.Forms.Button();
            this.treeViewCores = new System.Windows.Forms.TreeView();
            this.tabPageExtras = new System.Windows.Forms.TabPage();
            this.btnDownloadExtras = new System.Windows.Forms.Button();
            this.treeViewExtras = new System.Windows.Forms.TreeView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.lblMiSTerDir = new System.Windows.Forms.Label();
            this.linkLabelMiSTerWiki = new System.Windows.Forms.LinkLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cmbMiSTerDir = new System.Windows.Forms.ComboBox();
            this.tabControlSections.SuspendLayout();
            this.tabPageWizard.SuspendLayout();
            this.pnlSamba.SuspendLayout();
            this.pnlWiFi.SuspendLayout();
            this.tabPageCores.SuspendLayout();
            this.tabPageExtras.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlSections
            // 
            this.tabControlSections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlSections.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlSections.Controls.Add(this.tabPageWizard);
            this.tabControlSections.Controls.Add(this.tabPageCores);
            this.tabControlSections.Controls.Add(this.tabPageExtras);
            this.tabControlSections.Location = new System.Drawing.Point(0, 0);
            this.tabControlSections.Name = "tabControlSections";
            this.tabControlSections.SelectedIndex = 0;
            this.tabControlSections.Size = new System.Drawing.Size(406, 489);
            this.tabControlSections.TabIndex = 0;
            // 
            // tabPageWizard
            // 
            this.tabPageWizard.Controls.Add(this.linkLabelUnixInstaller);
            this.tabPageWizard.Controls.Add(this.chkEnableWiFi);
            this.tabPageWizard.Controls.Add(this.chkEnableSamba);
            this.tabPageWizard.Controls.Add(this.pnlSamba);
            this.tabPageWizard.Controls.Add(this.btnOptimalPreset);
            this.tabPageWizard.Controls.Add(this.btnCompatibilityPreset);
            this.tabPageWizard.Controls.Add(this.pnlWiFi);
            this.tabPageWizard.Controls.Add(this.cmbVSyncMode);
            this.tabPageWizard.Controls.Add(this.lblVSyncMode);
            this.tabPageWizard.Controls.Add(this.cmbScalingMode);
            this.tabPageWizard.Controls.Add(this.lblScalingMode);
            this.tabPageWizard.Controls.Add(this.lblEnablePAL_NTSC);
            this.tabPageWizard.Controls.Add(this.chkEnablePAL_NTSC);
            this.tabPageWizard.Controls.Add(this.cmbVideoMode);
            this.tabPageWizard.Controls.Add(this.lblVideoMode);
            this.tabPageWizard.Controls.Add(this.btnAdvanced);
            this.tabPageWizard.Controls.Add(this.btn1ClickSetup);
            this.tabPageWizard.Location = new System.Drawing.Point(4, 25);
            this.tabPageWizard.Name = "tabPageWizard";
            this.tabPageWizard.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWizard.Size = new System.Drawing.Size(398, 460);
            this.tabPageWizard.TabIndex = 2;
            this.tabPageWizard.Text = "Wizard";
            this.tabPageWizard.UseVisualStyleBackColor = true;
            // 
            // linkLabelUnixInstaller
            // 
            this.linkLabelUnixInstaller.AutoSize = true;
            this.linkLabelUnixInstaller.Location = new System.Drawing.Point(8, 439);
            this.linkLabelUnixInstaller.Name = "linkLabelUnixInstaller";
            this.linkLabelUnixInstaller.Size = new System.Drawing.Size(106, 13);
            this.linkLabelUnixInstaller.TabIndex = 18;
            this.linkLabelUnixInstaller.TabStop = true;
            this.linkLabelUnixInstaller.Text = "linkLabelUnixInstaller";
            this.linkLabelUnixInstaller.Visible = false;
            this.linkLabelUnixInstaller.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUnixInstaller_LinkClicked);
            // 
            // chkEnableWiFi
            // 
            this.chkEnableWiFi.AutoSize = true;
            this.chkEnableWiFi.Checked = true;
            this.chkEnableWiFi.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableWiFi.Location = new System.Drawing.Point(15, 240);
            this.chkEnableWiFi.Name = "chkEnableWiFi";
            this.chkEnableWiFi.Size = new System.Drawing.Size(86, 17);
            this.chkEnableWiFi.TabIndex = 17;
            this.chkEnableWiFi.Text = "Enable Wi-Fi";
            this.chkEnableWiFi.UseVisualStyleBackColor = true;
            this.chkEnableWiFi.CheckedChanged += new System.EventHandler(this.chkEnableWiFi_CheckedChanged);
            // 
            // chkEnableSamba
            // 
            this.chkEnableSamba.AutoSize = true;
            this.chkEnableSamba.Checked = true;
            this.chkEnableSamba.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableSamba.Location = new System.Drawing.Point(15, 146);
            this.chkEnableSamba.Name = "chkEnableSamba";
            this.chkEnableSamba.Size = new System.Drawing.Size(95, 17);
            this.chkEnableSamba.TabIndex = 16;
            this.chkEnableSamba.Text = "Enable Samba";
            this.chkEnableSamba.UseVisualStyleBackColor = true;
            this.chkEnableSamba.CheckedChanged += new System.EventHandler(this.chkEnableSamba_CheckedChanged);
            // 
            // pnlSamba
            // 
            this.pnlSamba.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSamba.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSamba.Controls.Add(this.txtSambaPassword);
            this.pnlSamba.Controls.Add(this.lblSambaPassword);
            this.pnlSamba.Controls.Add(this.txtSambaUserName);
            this.pnlSamba.Controls.Add(this.lblSambaUserName);
            this.pnlSamba.Location = new System.Drawing.Point(6, 153);
            this.pnlSamba.Name = "pnlSamba";
            this.pnlSamba.Size = new System.Drawing.Size(384, 77);
            this.pnlSamba.TabIndex = 15;
            // 
            // txtSambaPassword
            // 
            this.txtSambaPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSambaPassword.Location = new System.Drawing.Point(114, 41);
            this.txtSambaPassword.Name = "txtSambaPassword";
            this.txtSambaPassword.PasswordChar = '*';
            this.txtSambaPassword.Size = new System.Drawing.Size(265, 20);
            this.txtSambaPassword.TabIndex = 3;
            this.txtSambaPassword.Text = "1";
            // 
            // lblSambaPassword
            // 
            this.lblSambaPassword.AutoSize = true;
            this.lblSambaPassword.Location = new System.Drawing.Point(5, 44);
            this.lblSambaPassword.Name = "lblSambaPassword";
            this.lblSambaPassword.Size = new System.Drawing.Size(53, 13);
            this.lblSambaPassword.TabIndex = 2;
            this.lblSambaPassword.Text = "Password";
            // 
            // txtSambaUserName
            // 
            this.txtSambaUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSambaUserName.Location = new System.Drawing.Point(114, 15);
            this.txtSambaUserName.Name = "txtSambaUserName";
            this.txtSambaUserName.Size = new System.Drawing.Size(265, 20);
            this.txtSambaUserName.TabIndex = 1;
            this.txtSambaUserName.Text = "root";
            // 
            // lblSambaUserName
            // 
            this.lblSambaUserName.AutoSize = true;
            this.lblSambaUserName.Location = new System.Drawing.Point(5, 18);
            this.lblSambaUserName.Name = "lblSambaUserName";
            this.lblSambaUserName.Size = new System.Drawing.Size(60, 13);
            this.lblSambaUserName.TabIndex = 0;
            this.lblSambaUserName.Text = "User Name";
            // 
            // btnOptimalPreset
            // 
            this.btnOptimalPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOptimalPreset.Location = new System.Drawing.Point(209, 107);
            this.btnOptimalPreset.Name = "btnOptimalPreset";
            this.btnOptimalPreset.Size = new System.Drawing.Size(183, 23);
            this.btnOptimalPreset.TabIndex = 14;
            this.btnOptimalPreset.Text = "Optimal Preset";
            this.btnOptimalPreset.UseVisualStyleBackColor = true;
            this.btnOptimalPreset.Click += new System.EventHandler(this.btnOptimalPreset_Click);
            // 
            // btnCompatibilityPreset
            // 
            this.btnCompatibilityPreset.Location = new System.Drawing.Point(9, 107);
            this.btnCompatibilityPreset.Name = "btnCompatibilityPreset";
            this.btnCompatibilityPreset.Size = new System.Drawing.Size(183, 23);
            this.btnCompatibilityPreset.TabIndex = 13;
            this.btnCompatibilityPreset.Text = "Compatibility Preset";
            this.btnCompatibilityPreset.UseVisualStyleBackColor = true;
            this.btnCompatibilityPreset.Click += new System.EventHandler(this.btnCompatibilityPreset_Click);
            // 
            // pnlWiFi
            // 
            this.pnlWiFi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWiFi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlWiFi.Controls.Add(this.txtWiFiPassword);
            this.pnlWiFi.Controls.Add(this.lblWiFiPassword);
            this.pnlWiFi.Controls.Add(this.txtWiFiSSID);
            this.pnlWiFi.Controls.Add(this.lblWiFiSSID);
            this.pnlWiFi.Controls.Add(this.txtWiFiCountry);
            this.pnlWiFi.Controls.Add(this.lblWiFiCountry);
            this.pnlWiFi.Location = new System.Drawing.Point(6, 247);
            this.pnlWiFi.Name = "pnlWiFi";
            this.pnlWiFi.Size = new System.Drawing.Size(384, 102);
            this.pnlWiFi.TabIndex = 10;
            // 
            // txtWiFiPassword
            // 
            this.txtWiFiPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWiFiPassword.Location = new System.Drawing.Point(114, 67);
            this.txtWiFiPassword.Name = "txtWiFiPassword";
            this.txtWiFiPassword.PasswordChar = '*';
            this.txtWiFiPassword.Size = new System.Drawing.Size(265, 20);
            this.txtWiFiPassword.TabIndex = 7;
            // 
            // lblWiFiPassword
            // 
            this.lblWiFiPassword.AutoSize = true;
            this.lblWiFiPassword.Location = new System.Drawing.Point(5, 70);
            this.lblWiFiPassword.Name = "lblWiFiPassword";
            this.lblWiFiPassword.Size = new System.Drawing.Size(53, 13);
            this.lblWiFiPassword.TabIndex = 6;
            this.lblWiFiPassword.Text = "Password";
            // 
            // txtWiFiSSID
            // 
            this.txtWiFiSSID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWiFiSSID.Location = new System.Drawing.Point(114, 41);
            this.txtWiFiSSID.Name = "txtWiFiSSID";
            this.txtWiFiSSID.Size = new System.Drawing.Size(265, 20);
            this.txtWiFiSSID.TabIndex = 5;
            // 
            // lblWiFiSSID
            // 
            this.lblWiFiSSID.AutoSize = true;
            this.lblWiFiSSID.Location = new System.Drawing.Point(5, 44);
            this.lblWiFiSSID.Name = "lblWiFiSSID";
            this.lblWiFiSSID.Size = new System.Drawing.Size(32, 13);
            this.lblWiFiSSID.TabIndex = 4;
            this.lblWiFiSSID.Text = "SSID";
            // 
            // txtWiFiCountry
            // 
            this.txtWiFiCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWiFiCountry.Location = new System.Drawing.Point(114, 15);
            this.txtWiFiCountry.Name = "txtWiFiCountry";
            this.txtWiFiCountry.Size = new System.Drawing.Size(265, 20);
            this.txtWiFiCountry.TabIndex = 3;
            this.txtWiFiCountry.Text = "US";
            // 
            // lblWiFiCountry
            // 
            this.lblWiFiCountry.AutoSize = true;
            this.lblWiFiCountry.Location = new System.Drawing.Point(5, 18);
            this.lblWiFiCountry.Name = "lblWiFiCountry";
            this.lblWiFiCountry.Size = new System.Drawing.Size(43, 13);
            this.lblWiFiCountry.TabIndex = 2;
            this.lblWiFiCountry.Text = "Country";
            // 
            // cmbVSyncMode
            // 
            this.cmbVSyncMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbVSyncMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVSyncMode.FormattingEnabled = true;
            this.cmbVSyncMode.Items.AddRange(new object[] {
            "Match display frequency",
            "Match core frequency",
            "Low lag"});
            this.cmbVSyncMode.Location = new System.Drawing.Point(121, 80);
            this.cmbVSyncMode.Name = "cmbVSyncMode";
            this.cmbVSyncMode.Size = new System.Drawing.Size(269, 21);
            this.cmbVSyncMode.TabIndex = 9;
            // 
            // lblVSyncMode
            // 
            this.lblVSyncMode.AutoSize = true;
            this.lblVSyncMode.Location = new System.Drawing.Point(6, 83);
            this.lblVSyncMode.Name = "lblVSyncMode";
            this.lblVSyncMode.Size = new System.Drawing.Size(68, 13);
            this.lblVSyncMode.TabIndex = 8;
            this.lblVSyncMode.Text = "VSync Mode";
            // 
            // cmbScalingMode
            // 
            this.cmbScalingMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbScalingMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScalingMode.FormattingEnabled = true;
            this.cmbScalingMode.Items.AddRange(new object[] {
            "Scale to fit the screen height",
            "Use integer scale only",
            "Use 0.5 steps of scale",
            "Use 0.25 steps of scale"});
            this.cmbScalingMode.Location = new System.Drawing.Point(121, 53);
            this.cmbScalingMode.Name = "cmbScalingMode";
            this.cmbScalingMode.Size = new System.Drawing.Size(269, 21);
            this.cmbScalingMode.TabIndex = 7;
            // 
            // lblScalingMode
            // 
            this.lblScalingMode.AutoSize = true;
            this.lblScalingMode.Location = new System.Drawing.Point(6, 56);
            this.lblScalingMode.Name = "lblScalingMode";
            this.lblScalingMode.Size = new System.Drawing.Size(72, 13);
            this.lblScalingMode.TabIndex = 6;
            this.lblScalingMode.Text = "Scaling Mode";
            // 
            // lblEnablePAL_NTSC
            // 
            this.lblEnablePAL_NTSC.AutoSize = true;
            this.lblEnablePAL_NTSC.Location = new System.Drawing.Point(6, 33);
            this.lblEnablePAL_NTSC.Name = "lblEnablePAL_NTSC";
            this.lblEnablePAL_NTSC.Size = new System.Drawing.Size(97, 13);
            this.lblEnablePAL_NTSC.TabIndex = 5;
            this.lblEnablePAL_NTSC.Text = "Enable PAL/NTSC";
            // 
            // chkEnablePAL_NTSC
            // 
            this.chkEnablePAL_NTSC.AutoSize = true;
            this.chkEnablePAL_NTSC.Location = new System.Drawing.Point(121, 33);
            this.chkEnablePAL_NTSC.Name = "chkEnablePAL_NTSC";
            this.chkEnablePAL_NTSC.Size = new System.Drawing.Size(15, 14);
            this.chkEnablePAL_NTSC.TabIndex = 4;
            this.chkEnablePAL_NTSC.UseVisualStyleBackColor = true;
            // 
            // cmbVideoMode
            // 
            this.cmbVideoMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbVideoMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVideoMode.FormattingEnabled = true;
            this.cmbVideoMode.Items.AddRange(new object[] {
            "1280x720 60Hz",
            "1024x768 60Hz",
            "720x480 60Hz",
            "720x576 50Hz",
            "1280x1024 60Hz",
            "800x600 60Hz",
            "640x480 60Hz",
            "1280x720 50Hz",
            "1920x1080 60Hz",
            "1920x1080 50Hz",
            "1366x768 60Hz",
            "1024x600 60Hz"});
            this.cmbVideoMode.Location = new System.Drawing.Point(121, 6);
            this.cmbVideoMode.Name = "cmbVideoMode";
            this.cmbVideoMode.Size = new System.Drawing.Size(269, 21);
            this.cmbVideoMode.TabIndex = 3;
            this.cmbVideoMode.SelectedIndexChanged += new System.EventHandler(this.cmbVideoMode_SelectedIndexChanged);
            // 
            // lblVideoMode
            // 
            this.lblVideoMode.AutoSize = true;
            this.lblVideoMode.Location = new System.Drawing.Point(6, 9);
            this.lblVideoMode.Name = "lblVideoMode";
            this.lblVideoMode.Size = new System.Drawing.Size(64, 13);
            this.lblVideoMode.TabIndex = 2;
            this.lblVideoMode.Text = "Video Mode";
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvanced.Enabled = false;
            this.btnAdvanced.Location = new System.Drawing.Point(6, 413);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(384, 23);
            this.btnAdvanced.TabIndex = 1;
            this.btnAdvanced.Text = "Advanced...";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btn1ClickSetup
            // 
            this.btn1ClickSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn1ClickSetup.Enabled = false;
            this.btn1ClickSetup.Location = new System.Drawing.Point(6, 355);
            this.btn1ClickSetup.Name = "btn1ClickSetup";
            this.btn1ClickSetup.Size = new System.Drawing.Size(384, 52);
            this.btn1ClickSetup.TabIndex = 0;
            this.btn1ClickSetup.Text = "1 Click Setup and Install";
            this.btn1ClickSetup.UseVisualStyleBackColor = true;
            this.btn1ClickSetup.Click += new System.EventHandler(this.btn1ClickSetup_Click);
            // 
            // tabPageCores
            // 
            this.tabPageCores.Controls.Add(this.txtUtilityDir);
            this.tabPageCores.Controls.Add(this.lblUtilityDir);
            this.tabPageCores.Controls.Add(this.txtArcadeDir);
            this.tabPageCores.Controls.Add(this.lblArcadeDir);
            this.tabPageCores.Controls.Add(this.txtConsoleDir);
            this.tabPageCores.Controls.Add(this.lblConsoleDir);
            this.tabPageCores.Controls.Add(this.txtComputerDir);
            this.tabPageCores.Controls.Add(this.lblComputerDir);
            this.tabPageCores.Controls.Add(this.btnDownloadCores);
            this.tabPageCores.Controls.Add(this.treeViewCores);
            this.tabPageCores.Location = new System.Drawing.Point(4, 25);
            this.tabPageCores.Name = "tabPageCores";
            this.tabPageCores.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCores.Size = new System.Drawing.Size(398, 460);
            this.tabPageCores.TabIndex = 0;
            this.tabPageCores.Text = "Cores";
            this.tabPageCores.UseVisualStyleBackColor = true;
            // 
            // txtUtilityDir
            // 
            this.txtUtilityDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtUtilityDir.Location = new System.Drawing.Point(226, 417);
            this.txtUtilityDir.Name = "txtUtilityDir";
            this.txtUtilityDir.Size = new System.Drawing.Size(68, 20);
            this.txtUtilityDir.TabIndex = 9;
            this.txtUtilityDir.Text = "_Utility";
            // 
            // lblUtilityDir
            // 
            this.lblUtilityDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUtilityDir.AutoSize = true;
            this.lblUtilityDir.Location = new System.Drawing.Point(174, 420);
            this.lblUtilityDir.Name = "lblUtilityDir";
            this.lblUtilityDir.Size = new System.Drawing.Size(46, 13);
            this.lblUtilityDir.TabIndex = 8;
            this.lblUtilityDir.Text = "Utility dir";
            // 
            // txtArcadeDir
            // 
            this.txtArcadeDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtArcadeDir.Location = new System.Drawing.Point(80, 417);
            this.txtArcadeDir.Name = "txtArcadeDir";
            this.txtArcadeDir.Size = new System.Drawing.Size(68, 20);
            this.txtArcadeDir.TabIndex = 7;
            this.txtArcadeDir.Text = "_Arcade";
            // 
            // lblArcadeDir
            // 
            this.lblArcadeDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblArcadeDir.AutoSize = true;
            this.lblArcadeDir.Location = new System.Drawing.Point(19, 420);
            this.lblArcadeDir.Name = "lblArcadeDir";
            this.lblArcadeDir.Size = new System.Drawing.Size(55, 13);
            this.lblArcadeDir.TabIndex = 6;
            this.lblArcadeDir.Text = "Arcade dir";
            // 
            // txtConsoleDir
            // 
            this.txtConsoleDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtConsoleDir.Location = new System.Drawing.Point(226, 391);
            this.txtConsoleDir.Name = "txtConsoleDir";
            this.txtConsoleDir.Size = new System.Drawing.Size(68, 20);
            this.txtConsoleDir.TabIndex = 5;
            this.txtConsoleDir.Text = "_Console";
            // 
            // lblConsoleDir
            // 
            this.lblConsoleDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblConsoleDir.AutoSize = true;
            this.lblConsoleDir.Location = new System.Drawing.Point(161, 394);
            this.lblConsoleDir.Name = "lblConsoleDir";
            this.lblConsoleDir.Size = new System.Drawing.Size(59, 13);
            this.lblConsoleDir.TabIndex = 4;
            this.lblConsoleDir.Text = "Console dir";
            // 
            // txtComputerDir
            // 
            this.txtComputerDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtComputerDir.Location = new System.Drawing.Point(80, 391);
            this.txtComputerDir.Name = "txtComputerDir";
            this.txtComputerDir.Size = new System.Drawing.Size(68, 20);
            this.txtComputerDir.TabIndex = 3;
            this.txtComputerDir.Text = "_Computer";
            // 
            // lblComputerDir
            // 
            this.lblComputerDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblComputerDir.AutoSize = true;
            this.lblComputerDir.Location = new System.Drawing.Point(8, 394);
            this.lblComputerDir.Name = "lblComputerDir";
            this.lblComputerDir.Size = new System.Drawing.Size(66, 13);
            this.lblComputerDir.TabIndex = 2;
            this.lblComputerDir.Text = "Computer dir";
            // 
            // btnDownloadCores
            // 
            this.btnDownloadCores.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadCores.Location = new System.Drawing.Point(315, 403);
            this.btnDownloadCores.Name = "btnDownloadCores";
            this.btnDownloadCores.Size = new System.Drawing.Size(75, 23);
            this.btnDownloadCores.TabIndex = 1;
            this.btnDownloadCores.Text = "Download";
            this.btnDownloadCores.UseVisualStyleBackColor = true;
            this.btnDownloadCores.Click += new System.EventHandler(this.btnDownloadCores_Click);
            // 
            // treeViewCores
            // 
            this.treeViewCores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewCores.CheckBoxes = true;
            this.treeViewCores.Location = new System.Drawing.Point(6, 6);
            this.treeViewCores.Name = "treeViewCores";
            treeNode1.Name = "MiSTer";
            treeNode1.Tag = "https://github.com/MiSTer-devel/Main_MiSTer";
            treeNode1.Text = "Main MiSTer";
            treeNode2.Name = "menu";
            treeNode2.Tag = "https://github.com/MiSTer-devel/Menu_MiSTer";
            treeNode2.Text = "Boot Menu";
            treeNode3.Name = "cores";
            treeNode3.Tag = "";
            treeNode3.Text = "Computer";
            treeNode4.Name = "console-cores";
            treeNode4.Tag = "";
            treeNode4.Text = "Console";
            treeNode5.Name = "arcade-cores";
            treeNode5.Tag = "";
            treeNode5.Text = "Arcade";
            treeNode6.Name = "service-cores";
            treeNode6.Tag = "";
            treeNode6.Text = "Utility";
            treeNode7.Name = "root";
            treeNode7.Text = "Cores";
            this.treeViewCores.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode7});
            this.treeViewCores.Size = new System.Drawing.Size(384, 379);
            this.treeViewCores.TabIndex = 0;
            this.treeViewCores.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCores_AfterCheck);
            this.treeViewCores.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCores_AfterSelect);
            // 
            // tabPageExtras
            // 
            this.tabPageExtras.Controls.Add(this.btnDownloadExtras);
            this.tabPageExtras.Controls.Add(this.treeViewExtras);
            this.tabPageExtras.Location = new System.Drawing.Point(4, 25);
            this.tabPageExtras.Name = "tabPageExtras";
            this.tabPageExtras.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageExtras.Size = new System.Drawing.Size(398, 460);
            this.tabPageExtras.TabIndex = 1;
            this.tabPageExtras.Text = "Extras";
            this.tabPageExtras.UseVisualStyleBackColor = true;
            // 
            // btnDownloadExtras
            // 
            this.btnDownloadExtras.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadExtras.Location = new System.Drawing.Point(315, 403);
            this.btnDownloadExtras.Name = "btnDownloadExtras";
            this.btnDownloadExtras.Size = new System.Drawing.Size(75, 23);
            this.btnDownloadExtras.TabIndex = 2;
            this.btnDownloadExtras.Text = "Download";
            this.btnDownloadExtras.UseVisualStyleBackColor = true;
            this.btnDownloadExtras.Click += new System.EventHandler(this.btnDownloadExtras_Click);
            // 
            // treeViewExtras
            // 
            this.treeViewExtras.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewExtras.CheckBoxes = true;
            this.treeViewExtras.Location = new System.Drawing.Point(6, 6);
            this.treeViewExtras.Name = "treeViewExtras";
            treeNode8.Name = "Filters";
            treeNode8.Tag = "https://github.com/MiSTer-devel/Filters_MiSTer/tree/master/Filters|txt|Filters";
            treeNode8.Text = "Filters";
            treeNode9.Name = "Fonts";
            treeNode9.Tag = "https://github.com/MiSTer-devel/Fonts_MiSTer|pf|font";
            treeNode9.Text = "Fonts";
            treeNode10.Name = "Cheats";
            treeNode10.Tag = "https://gamehacking.org/mister/|fds:NES gb:GameBoy gbc:GameBoy gen:Genesis gg:SMS" +
    " nes:NES pce:TGFX16 sms:SMS snes:SNES|cheats";
            treeNode10.Text = "Cheats";
            treeNode11.Name = "Game Boy Palettes";
            treeNode11.Tag = "https://github.com/MiSTer-devel/Gameboy_MiSTer/tree/master/palettes|gbp|GameBoy";
            treeNode11.Text = "Game Boy Palettes";
            treeNode12.Name = "Updater";
            treeNode12.Tag = "https://github.com/MiSTer-devel/Updater_script_MiSTer|update.sh|Scripts";
            treeNode12.Text = "Updater";
            treeNode13.Name = "Scripts";
            treeNode13.Tag = "https://github.com/MiSTer-devel/Scripts_MiSTer|sh inc|Scripts";
            treeNode13.Text = "Scripts";
            treeNode14.Name = "SoundFont Installer";
            treeNode14.Tag = "https://github.com/bbond007/MiSTer_MidiLink/tree/master/INSTALL|sh inc|Scripts";
            treeNode14.Text = "SoundFont Installer";
            this.treeViewExtras.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11,
            treeNode12,
            treeNode13,
            treeNode14});
            this.treeViewExtras.Size = new System.Drawing.Size(384, 379);
            this.treeViewExtras.TabIndex = 0;
            this.treeViewExtras.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewExtras_AfterSelect);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 518);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(406, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // lblMiSTerDir
            // 
            this.lblMiSTerDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMiSTerDir.AutoSize = true;
            this.lblMiSTerDir.Location = new System.Drawing.Point(7, 498);
            this.lblMiSTerDir.Name = "lblMiSTerDir";
            this.lblMiSTerDir.Size = new System.Drawing.Size(86, 13);
            this.lblMiSTerDir.TabIndex = 2;
            this.lblMiSTerDir.Text = "MiSTer files path";
            // 
            // linkLabelMiSTerWiki
            // 
            this.linkLabelMiSTerWiki.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelMiSTerWiki.AutoSize = true;
            this.linkLabelMiSTerWiki.Location = new System.Drawing.Point(252, 523);
            this.linkLabelMiSTerWiki.Name = "linkLabelMiSTerWiki";
            this.linkLabelMiSTerWiki.Size = new System.Drawing.Size(142, 13);
            this.linkLabelMiSTerWiki.TabIndex = 4;
            this.linkLabelMiSTerWiki.TabStop = true;
            this.linkLabelMiSTerWiki.Text = "Please read the MiSTer Wiki";
            this.linkLabelMiSTerWiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMiSTerWiki_LinkClicked);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // cmbMiSTerDir
            // 
            this.cmbMiSTerDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMiSTerDir.FormattingEnabled = true;
            this.cmbMiSTerDir.Location = new System.Drawing.Point(99, 495);
            this.cmbMiSTerDir.Name = "cmbMiSTerDir";
            this.cmbMiSTerDir.Size = new System.Drawing.Size(295, 21);
            this.cmbMiSTerDir.TabIndex = 5;
            // 
            // ConfiguratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 540);
            this.Controls.Add(this.cmbMiSTerDir);
            this.Controls.Add(this.linkLabelMiSTerWiki);
            this.Controls.Add(this.lblMiSTerDir);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControlSections);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfiguratorForm";
            this.Text = "MiSTer Configurator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfiguratorForm_FormClosing);
            this.Load += new System.EventHandler(this.ConfiguratorForm_Load);
            this.tabControlSections.ResumeLayout(false);
            this.tabPageWizard.ResumeLayout(false);
            this.tabPageWizard.PerformLayout();
            this.pnlSamba.ResumeLayout(false);
            this.pnlSamba.PerformLayout();
            this.pnlWiFi.ResumeLayout(false);
            this.pnlWiFi.PerformLayout();
            this.tabPageCores.ResumeLayout(false);
            this.tabPageCores.PerformLayout();
            this.tabPageExtras.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlSections;
        private System.Windows.Forms.TabPage tabPageCores;
        private System.Windows.Forms.TabPage tabPageExtras;
        private System.Windows.Forms.TreeView treeViewCores;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button btnDownloadCores;
        private System.Windows.Forms.Label lblMiSTerDir;
        private System.Windows.Forms.LinkLabel linkLabelMiSTerWiki;
        private System.Windows.Forms.TextBox txtUtilityDir;
        private System.Windows.Forms.Label lblUtilityDir;
        private System.Windows.Forms.TextBox txtArcadeDir;
        private System.Windows.Forms.Label lblArcadeDir;
        private System.Windows.Forms.TextBox txtConsoleDir;
        private System.Windows.Forms.Label lblConsoleDir;
        private System.Windows.Forms.TextBox txtComputerDir;
        private System.Windows.Forms.Label lblComputerDir;
        private System.Windows.Forms.TreeView treeViewExtras;
        private System.Windows.Forms.Button btnDownloadExtras;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ComboBox cmbMiSTerDir;
        private System.Windows.Forms.TabPage tabPageWizard;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Button btn1ClickSetup;
        private System.Windows.Forms.ComboBox cmbVideoMode;
        private System.Windows.Forms.Label lblVideoMode;
        private System.Windows.Forms.Label lblEnablePAL_NTSC;
        private System.Windows.Forms.CheckBox chkEnablePAL_NTSC;
        private System.Windows.Forms.ComboBox cmbScalingMode;
        private System.Windows.Forms.Label lblScalingMode;
        private System.Windows.Forms.ComboBox cmbVSyncMode;
        private System.Windows.Forms.Label lblVSyncMode;
        private System.Windows.Forms.Button btnOptimalPreset;
        private System.Windows.Forms.Button btnCompatibilityPreset;
        private System.Windows.Forms.Panel pnlWiFi;
        private System.Windows.Forms.CheckBox chkEnableWiFi;
        private System.Windows.Forms.CheckBox chkEnableSamba;
        private System.Windows.Forms.Panel pnlSamba;
        private System.Windows.Forms.TextBox txtSambaPassword;
        private System.Windows.Forms.Label lblSambaPassword;
        private System.Windows.Forms.TextBox txtSambaUserName;
        private System.Windows.Forms.Label lblSambaUserName;
        private System.Windows.Forms.TextBox txtWiFiPassword;
        private System.Windows.Forms.Label lblWiFiPassword;
        private System.Windows.Forms.TextBox txtWiFiSSID;
        private System.Windows.Forms.Label lblWiFiSSID;
        private System.Windows.Forms.TextBox txtWiFiCountry;
        private System.Windows.Forms.Label lblWiFiCountry;
        private System.Windows.Forms.LinkLabel linkLabelUnixInstaller;

    }
}

