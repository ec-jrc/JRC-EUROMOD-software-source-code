namespace EM_UI.VersionControl.Dialogs
{
    partial class VCMultiUpDownload
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCMultiUpDownload));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnIncludeAll = new System.Windows.Forms.Button();
            this.btnClearInclude = new System.Windows.Forms.Button();
            this.btnUpDownload = new System.Windows.Forms.Button();
            this.chkVersion = new System.Windows.Forms.CheckBox();
            this.txtVersion = new System.Windows.Forms.TextBox();
			this.vcContent = new VCContentControl();            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(341, 480);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 35);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "   Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnIncludeAll
            // 
            this.btnIncludeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIncludeAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnIncludeAll.Location = new System.Drawing.Point(340, 11);
            this.btnIncludeAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnIncludeAll.Name = "btnIncludeAll";
            this.btnIncludeAll.Size = new System.Drawing.Size(70, 36);
            this.btnIncludeAll.TabIndex = 7;
            this.btnIncludeAll.Text = "Select All Units";
            this.btnIncludeAll.UseVisualStyleBackColor = true;
            this.btnIncludeAll.Click += new System.EventHandler(this.btnIncludeAll_Click);
            // 
            // btnClearInclude
            // 
            this.btnClearInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearInclude.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClearInclude.Location = new System.Drawing.Point(341, 50);
            this.btnClearInclude.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearInclude.Name = "btnClearInclude";
            this.btnClearInclude.Size = new System.Drawing.Size(70, 36);
            this.btnClearInclude.TabIndex = 6;
            this.btnClearInclude.Text = "Clear Selection";
            this.btnClearInclude.UseVisualStyleBackColor = true;
            this.btnClearInclude.Click += new System.EventHandler(this.btnClearInclude_Click);
            // 
            // btnUpDownload
            // 
            this.btnUpDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpDownload.Image = ((System.Drawing.Image)(resources.GetObject("btnUpDownload.Image")));
            this.btnUpDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUpDownload.Location = new System.Drawing.Point(341, 439);
            this.btnUpDownload.Name = "btnUpDownload";
            this.btnUpDownload.Size = new System.Drawing.Size(65, 35);
            this.btnUpDownload.TabIndex = 4;
            this.btnUpDownload.Text = "Upload";
            this.btnUpDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnUpDownload.UseVisualStyleBackColor = true;
            this.btnUpDownload.Click += new System.EventHandler(this.btnUpDownload_Click);
            // 
            // chkVersion
            // 
            this.chkVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkVersion.AutoSize = true;
            this.chkVersion.Location = new System.Drawing.Point(345, 259);
            this.chkVersion.Name = "chkVersion";
            this.chkVersion.Size = new System.Drawing.Size(61, 30);
            this.chkVersion.TabIndex = 28;
            this.chkVersion.Text = "Manual\r\nVersion";
            this.chkVersion.UseVisualStyleBackColor = true;
            this.chkVersion.CheckedChanged += new System.EventHandler(this.chkVersion_CheckedChanged);
            // 
            // txtVersion
            // 
            this.txtVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVersion.Location = new System.Drawing.Point(340, 295);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(70, 20);
            this.txtVersion.TabIndex = 29;
            // 
			// vcContent
            // 
            this.vcContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vcContent.Location = new System.Drawing.Point(9, 10);
            this.vcContent.Margin = new System.Windows.Forms.Padding(2);
            this.vcContent.Name = "vcContent";
            this.vcContent.Size = new System.Drawing.Size(325, 505);
            this.vcContent.TabIndex = 8;
            // 			// 
            this.vcContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vcContent.Location = new System.Drawing.Point(9, 10);
            this.vcContent.Margin = new System.Windows.Forms.Padding(2);
            this.vcContent.Name = "vcContent";
            this.vcContent.Size = new System.Drawing.Size(325, 505);
            this.vcContent.TabIndex = 8;
            //             // VCMultiUpDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(420, 526);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.chkVersion);
            this.Controls.Add(this.btnUpDownload);
            this.Controls.Add(this.btnClearInclude);
            this.Controls.Add(this.btnIncludeAll);
			this.Controls.Add(this.vcContent);            this.Controls.Add(this.btnCancel);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_AdminContent.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(367, 420);
            this.Name = "VCMultiUpDownload";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Version Control - Upload Bundle";
            this.Load += new System.EventHandler(this.VCMultiUpDownload_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private VCContentControl vcContent;
        private System.Windows.Forms.Button btnIncludeAll;
        private System.Windows.Forms.Button btnClearInclude;
        private System.Windows.Forms.Button btnUpDownload;
        private System.Windows.Forms.CheckBox chkVersion;
        private System.Windows.Forms.TextBox txtVersion;
    }
}