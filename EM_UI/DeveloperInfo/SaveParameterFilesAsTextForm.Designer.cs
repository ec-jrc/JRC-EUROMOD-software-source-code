namespace EM_UI.DeveloperInfo
{
    partial class SaveParameterFilesAsTextForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveParameterFilesAsTextForm));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.lvCountries = new System.Windows.Forms.ListView();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnSelectExportFolder = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.radWithLinebreaks = new System.Windows.Forms.RadioButton();
            this.radTextFormat = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnNo
            // 
            this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNo.Location = new System.Drawing.Point(469, 184);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(90, 23);
            this.btnNo.TabIndex = 7;
            this.btnNo.Text = "Select No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnAll
            // 
            this.btnAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAll.Location = new System.Drawing.Point(469, 155);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(90, 23);
            this.btnAll.TabIndex = 6;
            this.btnAll.Text = "Select All";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // lvCountries
            // 
            this.lvCountries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCountries.CheckBoxes = true;
            this.lvCountries.HideSelection = false;
            this.lvCountries.Location = new System.Drawing.Point(12, 12);
            this.lvCountries.Name = "lvCountries";
            this.lvCountries.Size = new System.Drawing.Size(451, 195);
            this.lvCountries.TabIndex = 1;
            this.lvCountries.UseCompatibleStateImageBehavior = false;
            this.lvCountries.View = System.Windows.Forms.View.List;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(469, 43);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(469, 12);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSelectExportFolder
            // 
            this.btnSelectExportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectExportFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectExportFolder.Image")));
            this.btnSelectExportFolder.Location = new System.Drawing.Point(519, 216);
            this.btnSelectExportFolder.Name = "btnSelectExportFolder";
            this.btnSelectExportFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectExportFolder.TabIndex = 3;
            this.btnSelectExportFolder.UseVisualStyleBackColor = true;
            this.btnSelectExportFolder.Click += new System.EventHandler(this.btnSelectExportFolder_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.Location = new System.Drawing.Point(12, 236);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(501, 20);
            this.txtFolder.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 220);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Export to Folder";
            // 
            // radWithLinebreaks
            // 
            this.radWithLinebreaks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radWithLinebreaks.AutoSize = true;
            this.radWithLinebreaks.Checked = true;
            this.radWithLinebreaks.Location = new System.Drawing.Point(469, 88);
            this.radWithLinebreaks.Name = "radWithLinebreaks";
            this.radWithLinebreaks.Size = new System.Drawing.Size(106, 17);
            this.radWithLinebreaks.TabIndex = 12;
            this.radWithLinebreaks.TabStop = true;
            this.radWithLinebreaks.Text = "With Line Breaks";
            this.radWithLinebreaks.UseVisualStyleBackColor = true;
            // 
            // radTextFormat
            // 
            this.radTextFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radTextFormat.AutoSize = true;
            this.radTextFormat.Location = new System.Drawing.Point(469, 111);
            this.radTextFormat.Name = "radTextFormat";
            this.radTextFormat.Size = new System.Drawing.Size(81, 17);
            this.radTextFormat.TabIndex = 13;
            this.radTextFormat.Text = "Text Format";
            this.radTextFormat.UseVisualStyleBackColor = true;
            // 
            // SaveParameterFilesAsTextForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(571, 268);
            this.Controls.Add(this.radTextFormat);
            this.Controls.Add(this.radWithLinebreaks);
            this.Controls.Add(this.btnSelectExportFolder);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lvCountries);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnAll);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_SaveAsText.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveParameterFilesAsTextForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Save Parameter Files Formatted";
            this.Load += new System.EventHandler(this.SaveParameterFilesAsTextForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.ListView lvCountries;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnSelectExportFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radWithLinebreaks;
        private System.Windows.Forms.RadioButton radTextFormat;
    }
}