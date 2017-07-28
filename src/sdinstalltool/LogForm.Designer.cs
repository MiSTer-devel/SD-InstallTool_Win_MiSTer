namespace SDInstallTool
{
    partial class LogForm
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
            this.grpDriveStats = new System.Windows.Forms.GroupBox();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLogMessages = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.buttonClip = new System.Windows.Forms.Button();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDriveStats
            // 
            this.grpDriveStats.Location = new System.Drawing.Point(12, 12);
            this.grpDriveStats.Name = "grpDriveStats";
            this.grpDriveStats.Size = new System.Drawing.Size(496, 88);
            this.grpDriveStats.TabIndex = 0;
            this.grpDriveStats.TabStop = false;
            this.grpDriveStats.Text = "Drive Stats";
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.txtLogMessages);
            this.grpLog.Location = new System.Drawing.Point(12, 120);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(495, 266);
            this.grpLog.TabIndex = 1;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Log Messages";
            // 
            // txtLogMessages
            // 
            this.txtLogMessages.HideSelection = false;
            this.txtLogMessages.Location = new System.Drawing.Point(12, 23);
            this.txtLogMessages.MaxLength = 10000000;
            this.txtLogMessages.Multiline = true;
            this.txtLogMessages.Name = "txtLogMessages";
            this.txtLogMessages.ReadOnly = true;
            this.txtLogMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogMessages.Size = new System.Drawing.Size(473, 233);
            this.txtLogMessages.TabIndex = 0;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(418, 395);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(78, 29);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // buttonClip
            // 
            this.buttonClip.Location = new System.Drawing.Point(24, 396);
            this.buttonClip.Name = "buttonClip";
            this.buttonClip.Size = new System.Drawing.Size(78, 29);
            this.buttonClip.TabIndex = 3;
            this.buttonClip.Text = "Clip";
            this.buttonClip.UseVisualStyleBackColor = true;
            this.buttonClip.Click += new System.EventHandler(this.buttonClip_Click);
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 437);
            this.Controls.Add(this.buttonClip);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpDriveStats);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LogForm";
            this.Text = "Log Viewer";
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDriveStats;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.TextBox txtLogMessages;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button buttonClip;
    }
}