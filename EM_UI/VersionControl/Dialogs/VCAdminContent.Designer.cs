namespace EM_UI.VersionControl.Dialogs
{
    partial class VCAdminContent
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCAdminContent));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUploadUnits = new System.Windows.Forms.Button();
            this.btnRemoveUnits = new System.Windows.Forms.Button();
            this.btnDownloadUnits = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvUnits = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(158, 344);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 28);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnUploadUnits
            // 
            this.btnUploadUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUploadUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUploadUnits.Image = ((System.Drawing.Image)(resources.GetObject("btnUploadUnits.Image")));
            this.btnUploadUnits.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUploadUnits.Location = new System.Drawing.Point(7, 20);
            this.btnUploadUnits.Margin = new System.Windows.Forms.Padding(2);
            this.btnUploadUnits.Name = "btnUploadUnits";
            this.btnUploadUnits.Size = new System.Drawing.Size(160, 28);
            this.btnUploadUnits.TabIndex = 7;
            this.btnUploadUnits.Text = "Add Local Units to VC";
            this.btnUploadUnits.UseVisualStyleBackColor = true;
            this.btnUploadUnits.Click += new System.EventHandler(this.btnUploadUnits_Click);
            // 
            // btnRemoveUnits
            // 
            this.btnRemoveUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveUnits.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnRemoveUnits.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRemoveUnits.Location = new System.Drawing.Point(171, 20);
            this.btnRemoveUnits.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveUnits.Name = "btnRemoveUnits";
            this.btnRemoveUnits.Size = new System.Drawing.Size(182, 28);
            this.btnRemoveUnits.TabIndex = 8;
            this.btnRemoveUnits.Text = "Remove Units from VC";
            this.btnRemoveUnits.UseVisualStyleBackColor = true;
            this.btnRemoveUnits.Click += new System.EventHandler(this.btnRemoveUnits_Click);
            // 
            // btnDownloadUnits
            // 
            this.btnDownloadUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDownloadUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadUnits.Image = ((System.Drawing.Image)(resources.GetObject("btnDownloadUnits.Image")));
            this.btnDownloadUnits.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDownloadUnits.Location = new System.Drawing.Point(7, 50);
            this.btnDownloadUnits.Margin = new System.Windows.Forms.Padding(2);
            this.btnDownloadUnits.Name = "btnDownloadUnits";
            this.btnDownloadUnits.Size = new System.Drawing.Size(160, 28);
            this.btnDownloadUnits.TabIndex = 15;
            this.btnDownloadUnits.Text = "Get Units from VC";
            this.btnDownloadUnits.UseVisualStyleBackColor = true;
            this.btnDownloadUnits.Click += new System.EventHandler(this.btnDownloadUnits_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.btnUploadUnits);
            this.groupBox1.Controls.Add(this.btnDownloadUnits);
            this.groupBox1.Controls.Add(this.btnRemoveUnits);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 246);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(360, 84);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add / Remove Units";
            // 
            // lvUnits
            // 
            this.lvUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvUnits.HideSelection = false;
            this.lvUnits.Location = new System.Drawing.Point(9, 24);
            this.lvUnits.Margin = new System.Windows.Forms.Padding(2);
            this.lvUnits.Name = "lvUnits";
            this.lvUnits.Size = new System.Drawing.Size(361, 207);
            this.lvUnits.TabIndex = 18;
            this.lvUnits.UseCompatibleStateImageBehavior = false;
            this.lvUnits.View = System.Windows.Forms.View.List;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Currently Version Controlled Units";
            // 
            // VCAdminContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(380, 390);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lvUnits);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_AdminContent.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(396, 428);
            this.Name = "VCAdminContent";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Version Control - Administrate Content";
            this.Load += new System.EventHandler(this.VCAdminContent_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnUploadUnits;
        private System.Windows.Forms.Button btnRemoveUnits;
        private System.Windows.Forms.Button btnDownloadUnits;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lvUnits;
        private System.Windows.Forms.Label label1;
    }
}