namespace EM_UI.Dialogs
{
    partial class ConfigurePathsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurePathsForm));
            this.lblEuromodFolder = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSelectEuromodFolder = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.cmbEuromodFolder = new System.Windows.Forms.ComboBox();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // lblEuromodFolder
            // 
            this.lblEuromodFolder.AutoSize = true;
            this.lblEuromodFolder.Location = new System.Drawing.Point(12, 27);
            this.lblEuromodFolder.Name = "lblEuromodFolder";
            this.lblEuromodFolder.Size = new System.Drawing.Size(72, 13);
            this.lblEuromodFolder.TabIndex = 1;
            this.lblEuromodFolder.Text = "Project Folder";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(355, 62);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 28);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(426, 62);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 28);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSelectEuromodFolder
            // 
            this.btnSelectEuromodFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectEuromodFolder.Image = global::EM_UI.Properties.Resources.Folder1;
            this.btnSelectEuromodFolder.Location = new System.Drawing.Point(788, 12);
            this.btnSelectEuromodFolder.Name = "btnSelectEuromodFolder";
            this.btnSelectEuromodFolder.Size = new System.Drawing.Size(45, 39);
            this.btnSelectEuromodFolder.TabIndex = 1;
            this.toolTips.SetToolTip(this.btnSelectEuromodFolder, "Click to select Project Folder");
            this.btnSelectEuromodFolder.UseVisualStyleBackColor = true;
            this.btnSelectEuromodFolder.Click += new System.EventHandler(this.btnEuromod_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // cmbEuromodFolder
            // 
            this.cmbEuromodFolder.FormattingEnabled = true;
            this.cmbEuromodFolder.Location = new System.Drawing.Point(89, 24);
            this.cmbEuromodFolder.Margin = new System.Windows.Forms.Padding(2);
            this.cmbEuromodFolder.Name = "cmbEuromodFolder";
            this.cmbEuromodFolder.Size = new System.Drawing.Size(694, 21);
            this.cmbEuromodFolder.TabIndex = 25;
            this.toolTips.SetToolTip(this.cmbEuromodFolder, "Select existing Project from list");
            // 
            // ConfigurePathsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(842, 103);
            this.Controls.Add(this.cmbEuromodFolder);
            this.Controls.Add(this.btnSelectEuromodFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblEuromodFolder);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_OpenProject.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurePathsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowInTaskbar = false;
            this.Text = "Open Project";
            this.Load += new System.EventHandler(this.ConfigurePathsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblEuromodFolder;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSelectEuromodFolder;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.ComboBox cmbEuromodFolder;
        private System.Windows.Forms.ToolTip toolTips;
    }
}