namespace EM_UI.Dialogs
{
    partial class ConfigurationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
            this.tbcCofiguration = new System.Windows.Forms.TabControl();
            this.tbcGeneral = new System.Windows.Forms.TabPage();
            this.chkStoreViewSettings = new System.Windows.Forms.CheckBox();
            this.btnSelectInputFolder = new System.Windows.Forms.Button();
            this.btnSetStandardPaths = new System.Windows.Forms.Button();
            this.btnSelectOutputFolder = new System.Windows.Forms.Button();
            this.txtInputFolder = new System.Windows.Forms.TextBox();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.lblInputFolder = new System.Windows.Forms.Label();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.chkCloseWithLastCountry = new System.Windows.Forms.CheckBox();
            this.tabVersionControl = new System.Windows.Forms.TabPage();
            this.chkIsVersionControlled = new System.Windows.Forms.CheckBox();
            this.chkLogInAtProjectLoad = new System.Windows.Forms.CheckBox();
            this.lblEMVersion = new System.Windows.Forms.Label();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.tabAutoSave = new System.Windows.Forms.TabPage();
            this.chkAutosave = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAutosaveInterval = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabWarnings = new System.Windows.Forms.TabPage();
            this.lstWarnings = new System.Windows.Forms.CheckedListBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.tbcCofiguration.SuspendLayout();
            this.tbcGeneral.SuspendLayout();
            this.tabVersionControl.SuspendLayout();
            this.tabAutoSave.SuspendLayout();
            this.tabWarnings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbcCofiguration
            // 
            this.tbcCofiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbcCofiguration.Controls.Add(this.tbcGeneral);
            this.tbcCofiguration.Controls.Add(this.tabVersionControl);
            this.tbcCofiguration.Controls.Add(this.tabAutoSave);
            this.tbcCofiguration.Controls.Add(this.tabWarnings);
            this.tbcCofiguration.Location = new System.Drawing.Point(14, 12);
            this.tbcCofiguration.Name = "tbcCofiguration";
            this.tbcCofiguration.SelectedIndex = 0;
            this.tbcCofiguration.Size = new System.Drawing.Size(424, 262);
            this.tbcCofiguration.TabIndex = 0;
            // 
            // tbcGeneral
            // 
            this.tbcGeneral.Controls.Add(this.chkStoreViewSettings);
            this.tbcGeneral.Controls.Add(this.btnSelectInputFolder);
            this.tbcGeneral.Controls.Add(this.btnSetStandardPaths);
            this.tbcGeneral.Controls.Add(this.btnSelectOutputFolder);
            this.tbcGeneral.Controls.Add(this.txtInputFolder);
            this.tbcGeneral.Controls.Add(this.txtOutputFolder);
            this.tbcGeneral.Controls.Add(this.lblInputFolder);
            this.tbcGeneral.Controls.Add(this.lblOutputFolder);
            this.tbcGeneral.Controls.Add(this.chkCloseWithLastCountry);
            this.tbcGeneral.Location = new System.Drawing.Point(4, 22);
            this.tbcGeneral.Name = "tbcGeneral";
            this.tbcGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tbcGeneral.Size = new System.Drawing.Size(416, 236);
            this.tbcGeneral.TabIndex = 2;
            this.tbcGeneral.Text = "General";
            this.tbcGeneral.UseVisualStyleBackColor = true;
            // 
            // chkStoreViewSettings
            // 
            this.chkStoreViewSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkStoreViewSettings.AutoSize = true;
            this.chkStoreViewSettings.Location = new System.Drawing.Point(8, 205);
            this.chkStoreViewSettings.Margin = new System.Windows.Forms.Padding(2);
            this.chkStoreViewSettings.Name = "chkStoreViewSettings";
            this.chkStoreViewSettings.Size = new System.Drawing.Size(118, 17);
            this.chkStoreViewSettings.TabIndex = 28;
            this.chkStoreViewSettings.Text = "Store View Settings";
            this.chkStoreViewSettings.UseVisualStyleBackColor = true;
            // 
            // btnSelectInputFolder
            // 
            this.btnSelectInputFolder.AccessibleDescription = "";
            this.btnSelectInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectInputFolder.Image")));
            this.btnSelectInputFolder.Location = new System.Drawing.Point(371, 72);
            this.btnSelectInputFolder.Name = "btnSelectInputFolder";
            this.btnSelectInputFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectInputFolder.TabIndex = 27;
            this.btnSelectInputFolder.UseVisualStyleBackColor = true;
            this.btnSelectInputFolder.Click += new System.EventHandler(this.btnSelectInputFolder_Click);
            // 
            // btnSetStandardPaths
            // 
            this.btnSetStandardPaths.Location = new System.Drawing.Point(86, 141);
            this.btnSetStandardPaths.Name = "btnSetStandardPaths";
            this.btnSetStandardPaths.Size = new System.Drawing.Size(185, 27);
            this.btnSetStandardPaths.TabIndex = 17;
            this.btnSetStandardPaths.Text = "&Set Standard Input/Output-Paths";
            this.btnSetStandardPaths.UseVisualStyleBackColor = true;
            this.btnSetStandardPaths.Click += new System.EventHandler(this.btnSetStandardPaths_Click);
            // 
            // btnSelectOutputFolder
            // 
            this.btnSelectOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectOutputFolder.Image")));
            this.btnSelectOutputFolder.Location = new System.Drawing.Point(371, 11);
            this.btnSelectOutputFolder.Name = "btnSelectOutputFolder";
            this.btnSelectOutputFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectOutputFolder.TabIndex = 16;
            this.btnSelectOutputFolder.UseVisualStyleBackColor = true;
            this.btnSelectOutputFolder.Click += new System.EventHandler(this.btnSelectOutputFolder_Click);
            // 
            // txtInputFolder
            // 
            this.txtInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputFolder.Location = new System.Drawing.Point(86, 72);
            this.txtInputFolder.Multiline = true;
            this.txtInputFolder.Name = "txtInputFolder";
            this.txtInputFolder.Size = new System.Drawing.Size(281, 54);
            this.txtInputFolder.TabIndex = 14;
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputFolder.Location = new System.Drawing.Point(86, 11);
            this.txtOutputFolder.Multiline = true;
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(281, 54);
            this.txtOutputFolder.TabIndex = 13;
            // 
            // lblInputFolder
            // 
            this.lblInputFolder.AutoSize = true;
            this.lblInputFolder.Location = new System.Drawing.Point(6, 72);
            this.lblInputFolder.Name = "lblInputFolder";
            this.lblInputFolder.Size = new System.Drawing.Size(66, 13);
            this.lblInputFolder.TabIndex = 12;
            this.lblInputFolder.Text = "Input Folder:";
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(6, 11);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(74, 13);
            this.lblOutputFolder.TabIndex = 11;
            this.lblOutputFolder.Text = "Output Folder:";
            // 
            // chkCloseWithLastCountry
            // 
            this.chkCloseWithLastCountry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCloseWithLastCountry.AutoSize = true;
            this.chkCloseWithLastCountry.Location = new System.Drawing.Point(208, 205);
            this.chkCloseWithLastCountry.Name = "chkCloseWithLastCountry";
            this.chkCloseWithLastCountry.Size = new System.Drawing.Size(206, 17);
            this.chkCloseWithLastCountry.TabIndex = 0;
            this.chkCloseWithLastCountry.Text = "Close User Interface with Last Country";
            this.chkCloseWithLastCountry.UseVisualStyleBackColor = true;
            // 
            // tabVersionControl
            // 
            this.tabVersionControl.Controls.Add(this.chkIsVersionControlled);
            this.tabVersionControl.Controls.Add(this.chkLogInAtProjectLoad);
            this.tabVersionControl.Controls.Add(this.lblEMVersion);
            this.tabVersionControl.Controls.Add(this.txtProjectName);
            this.tabVersionControl.Location = new System.Drawing.Point(4, 22);
            this.tabVersionControl.Margin = new System.Windows.Forms.Padding(2);
            this.tabVersionControl.Name = "tabVersionControl";
            this.tabVersionControl.Padding = new System.Windows.Forms.Padding(2);
            this.tabVersionControl.Size = new System.Drawing.Size(416, 236);
            this.tabVersionControl.TabIndex = 3;
            this.tabVersionControl.Text = "Version Control";
            this.tabVersionControl.UseVisualStyleBackColor = true;
            // 
            // chkIsVersionControlled
            // 
            this.chkIsVersionControlled.AutoCheck = false;
            this.chkIsVersionControlled.AutoSize = true;
            this.chkIsVersionControlled.Location = new System.Drawing.Point(8, 13);
            this.chkIsVersionControlled.Margin = new System.Windows.Forms.Padding(2);
            this.chkIsVersionControlled.Name = "chkIsVersionControlled";
            this.chkIsVersionControlled.Size = new System.Drawing.Size(122, 17);
            this.chkIsVersionControlled.TabIndex = 30;
            this.chkIsVersionControlled.Text = "Is Version Controlled";
            this.chkIsVersionControlled.UseVisualStyleBackColor = true;
            // 
            // chkLogInAtProjectLoad
            // 
            this.chkLogInAtProjectLoad.AutoSize = true;
            this.chkLogInAtProjectLoad.Checked = true;
            this.chkLogInAtProjectLoad.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogInAtProjectLoad.Location = new System.Drawing.Point(8, 78);
            this.chkLogInAtProjectLoad.Margin = new System.Windows.Forms.Padding(2);
            this.chkLogInAtProjectLoad.Name = "chkLogInAtProjectLoad";
            this.chkLogInAtProjectLoad.Size = new System.Drawing.Size(132, 17);
            this.chkLogInAtProjectLoad.TabIndex = 29;
            this.chkLogInAtProjectLoad.Text = "Log In At Project Load";
            this.chkLogInAtProjectLoad.UseVisualStyleBackColor = true;
            this.chkLogInAtProjectLoad.Visible = false;
            // 
            // lblEMVersion
            // 
            this.lblEMVersion.AutoSize = true;
            this.lblEMVersion.Location = new System.Drawing.Point(5, 45);
            this.lblEMVersion.Name = "lblEMVersion";
            this.lblEMVersion.Size = new System.Drawing.Size(74, 13);
            this.lblEMVersion.TabIndex = 28;
            this.lblEMVersion.Text = "Project Name:";
            // 
            // txtProjectName
            // 
            this.txtProjectName.Location = new System.Drawing.Point(78, 45);
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.ReadOnly = true;
            this.txtProjectName.Size = new System.Drawing.Size(336, 20);
            this.txtProjectName.TabIndex = 27;
            // 
            // tabAutoSave
            // 
            this.tabAutoSave.Controls.Add(this.chkAutosave);
            this.tabAutoSave.Controls.Add(this.label4);
            this.tabAutoSave.Controls.Add(this.txtAutosaveInterval);
            this.tabAutoSave.Controls.Add(this.label3);
            this.tabAutoSave.Location = new System.Drawing.Point(4, 22);
            this.tabAutoSave.Name = "tabAutoSave";
            this.tabAutoSave.Padding = new System.Windows.Forms.Padding(3);
            this.tabAutoSave.Size = new System.Drawing.Size(416, 236);
            this.tabAutoSave.TabIndex = 0;
            this.tabAutoSave.Text = "Auto Save";
            this.tabAutoSave.UseVisualStyleBackColor = true;
            // 
            // chkAutosave
            // 
            this.chkAutosave.AutoSize = true;
            this.chkAutosave.Checked = true;
            this.chkAutosave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutosave.Location = new System.Drawing.Point(23, 23);
            this.chkAutosave.Name = "chkAutosave";
            this.chkAutosave.Size = new System.Drawing.Size(107, 17);
            this.chkAutosave.TabIndex = 41;
            this.chkAutosave.Text = "&Enable Autosave";
            this.chkAutosave.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(157, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 43;
            this.label4.Text = "min";
            // 
            // txtAutosaveInterval
            // 
            this.txtAutosaveInterval.Location = new System.Drawing.Point(114, 53);
            this.txtAutosaveInterval.Name = "txtAutosaveInterval";
            this.txtAutosaveInterval.Size = new System.Drawing.Size(42, 20);
            this.txtAutosaveInterval.TabIndex = 40;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 42;
            this.label3.Text = "Autosave Interval";
            // 
            // tabWarnings
            // 
            this.tabWarnings.Controls.Add(this.lstWarnings);
            this.tabWarnings.Location = new System.Drawing.Point(4, 22);
            this.tabWarnings.Name = "tabWarnings";
            this.tabWarnings.Padding = new System.Windows.Forms.Padding(3);
            this.tabWarnings.Size = new System.Drawing.Size(416, 236);
            this.tabWarnings.TabIndex = 1;
            this.tabWarnings.Text = "Warnings";
            this.tabWarnings.UseVisualStyleBackColor = true;
            // 
            // lstWarnings
            // 
            this.lstWarnings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstWarnings.CheckOnClick = true;
            this.lstWarnings.FormattingEnabled = true;
            this.lstWarnings.Location = new System.Drawing.Point(6, 6);
            this.lstWarnings.Name = "lstWarnings";
            this.lstWarnings.Size = new System.Drawing.Size(404, 199);
            this.lstWarnings.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(226, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(130, 280);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(448, 314);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbcCofiguration);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_Configuration.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigurationForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Project Configuration";
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            this.tbcCofiguration.ResumeLayout(false);
            this.tbcGeneral.ResumeLayout(false);
            this.tbcGeneral.PerformLayout();
            this.tabVersionControl.ResumeLayout(false);
            this.tabVersionControl.PerformLayout();
            this.tabAutoSave.ResumeLayout(false);
            this.tabAutoSave.PerformLayout();
            this.tabWarnings.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tbcCofiguration;
        private System.Windows.Forms.TabPage tabWarnings;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckedListBox lstWarnings;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.TabPage tbcGeneral;
        private System.Windows.Forms.CheckBox chkCloseWithLastCountry;
        private System.Windows.Forms.TabPage tabAutoSave;
        private System.Windows.Forms.CheckBox chkAutosave;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtAutosaveInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblInputFolder;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.TextBox txtInputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Button btnSelectOutputFolder;
        private System.Windows.Forms.Button btnSetStandardPaths;
        private System.Windows.Forms.Button btnSelectInputFolder;
        private System.Windows.Forms.TabPage tabVersionControl;
        private System.Windows.Forms.CheckBox chkLogInAtProjectLoad;
        private System.Windows.Forms.Label lblEMVersion;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.CheckBox chkIsVersionControlled;
        private System.Windows.Forms.CheckBox chkStoreViewSettings;
    }
}