namespace EM_UI.Dialogs
{
    partial class ImportCountryForm
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
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtCountryFolder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCountryFolder = new System.Windows.Forms.Button();
            this.txtShortName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFlag = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFlag = new System.Windows.Forms.Button();
            this.chkAdaptGlobal = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(450, 117);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(354, 117);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtCountryFolder
            // 
            this.txtCountryFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCountryFolder.Location = new System.Drawing.Point(116, 14);
            this.txtCountryFolder.Name = "txtCountryFolder";
            this.txtCountryFolder.Size = new System.Drawing.Size(378, 20);
            this.txtCountryFolder.TabIndex = 1;
            this.txtCountryFolder.TextChanged += new System.EventHandler(this.txtCountryFolder_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 47;
            this.label3.Text = "Import Country Folder";
            // 
            // btnCountryFolder
            // 
            this.btnCountryFolder.AccessibleDescription = "btnModellDataDir";
            this.btnCountryFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCountryFolder.Image = global::EM_UI.Properties.Resources.Folder1;
            this.btnCountryFolder.Location = new System.Drawing.Point(500, 4);
            this.btnCountryFolder.Name = "btnCountryFolder";
            this.btnCountryFolder.Size = new System.Drawing.Size(40, 40);
            this.btnCountryFolder.TabIndex = 2;
            this.btnCountryFolder.UseVisualStyleBackColor = true;
            this.btnCountryFolder.Click += new System.EventHandler(this.btnCountryFolder_Click);
            // 
            // txtShortName
            // 
            this.txtShortName.Location = new System.Drawing.Point(116, 40);
            this.txtShortName.Name = "txtShortName";
            this.txtShortName.Size = new System.Drawing.Size(62, 20);
            this.txtShortName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 46;
            this.label2.Text = "Short Name";
            // 
            // txtFlag
            // 
            this.txtFlag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFlag.Location = new System.Drawing.Point(116, 66);
            this.txtFlag.Name = "txtFlag";
            this.txtFlag.Size = new System.Drawing.Size(378, 20);
            this.txtFlag.TabIndex = 48;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 50;
            this.label1.Text = "Flag (png)";
            // 
            // btnFlag
            // 
            this.btnFlag.AccessibleDescription = "btnModellDataDir";
            this.btnFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFlag.Image = global::EM_UI.Properties.Resources.Folder1;
            this.btnFlag.Location = new System.Drawing.Point(498, 55);
            this.btnFlag.Name = "btnFlag";
            this.btnFlag.Size = new System.Drawing.Size(40, 40);
            this.btnFlag.TabIndex = 49;
            this.btnFlag.UseVisualStyleBackColor = true;
            this.btnFlag.Click += new System.EventHandler(this.btnFlag_Click);
            // 
            // chkAdaptGlobal
            // 
            this.chkAdaptGlobal.AutoSize = true;
            this.chkAdaptGlobal.Location = new System.Drawing.Point(14, 121);
            this.chkAdaptGlobal.Name = "chkAdaptGlobal";
            this.chkAdaptGlobal.Size = new System.Drawing.Size(230, 17);
            this.chkAdaptGlobal.TabIndex = 51;
            this.chkAdaptGlobal.Text = "Adapt Global Files (HICP, Exchange Rates)";
            this.chkAdaptGlobal.UseVisualStyleBackColor = true;
            // 
            // ImportCountryForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(550, 164);
            this.Controls.Add(this.chkAdaptGlobal);
            this.Controls.Add(this.txtFlag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFlag);
            this.Controls.Add(this.txtCountryFolder);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCountryFolder);
            this.Controls.Add(this.txtShortName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingCountries.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportCountryForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import Country";
            this.Load += new System.EventHandler(this.ImportCountryForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtCountryFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCountryFolder;
        private System.Windows.Forms.TextBox txtShortName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFlag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnFlag;
        private System.Windows.Forms.CheckBox chkAdaptGlobal;
    }
}