namespace InDepthAnalysis
{
    partial class SelectBaselinesReforms
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnSelectBaselinesPath = new System.Windows.Forms.Button();
            this.listBaselines = new System.Windows.Forms.ListBox();
            this.txtBaselinesPath = new System.Windows.Forms.TextBox();
            this.btnSelectReformsPath = new System.Windows.Forms.Button();
            this.listReforms = new System.Windows.Forms.ListBox();
            this.txtReformsPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMissingFiles = new System.Windows.Forms.LinkLabel();
            this.lblMultiSelect = new System.Windows.Forms.Label();
            this.lblOrderRetained = new System.Windows.Forms.Label();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(421, 566);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 37);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(318, 566);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(81, 37);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSelectBaselinesPath
            // 
            this.btnSelectBaselinesPath.Location = new System.Drawing.Point(367, 30);
            this.btnSelectBaselinesPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectBaselinesPath.Name = "btnSelectBaselinesPath";
            this.btnSelectBaselinesPath.Size = new System.Drawing.Size(32, 33);
            this.btnSelectBaselinesPath.TabIndex = 9;
            this.btnSelectBaselinesPath.Text = "...";
            this.btnSelectBaselinesPath.UseVisualStyleBackColor = true;
            this.btnSelectBaselinesPath.Click += new System.EventHandler(this.btnSelectBaselinesPath_Click);
            // 
            // listBaselines
            // 
            this.listBaselines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBaselines.FormattingEnabled = true;
            this.listBaselines.ItemHeight = 17;
            this.listBaselines.Location = new System.Drawing.Point(14, 78);
            this.listBaselines.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBaselines.Name = "listBaselines";
            this.listBaselines.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBaselines.Size = new System.Drawing.Size(385, 463);
            this.listBaselines.TabIndex = 5;
            // 
            // txtBaselinesPath
            // 
            this.txtBaselinesPath.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBaselinesPath.Location = new System.Drawing.Point(14, 35);
            this.txtBaselinesPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBaselinesPath.Name = "txtBaselinesPath";
            this.txtBaselinesPath.Size = new System.Drawing.Size(346, 20);
            this.txtBaselinesPath.TabIndex = 8;
            this.txtBaselinesPath.TextChanged += new System.EventHandler(this.txtBaselinesPath_TextChanged);
            // 
            // btnSelectReformsPath
            // 
            this.btnSelectReformsPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectReformsPath.Location = new System.Drawing.Point(773, 30);
            this.btnSelectReformsPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectReformsPath.Name = "btnSelectReformsPath";
            this.btnSelectReformsPath.Size = new System.Drawing.Size(32, 33);
            this.btnSelectReformsPath.TabIndex = 12;
            this.btnSelectReformsPath.Text = "...";
            this.btnSelectReformsPath.UseVisualStyleBackColor = true;
            this.btnSelectReformsPath.Click += new System.EventHandler(this.btnSelectReformsPath_Click);
            // 
            // listReforms
            // 
            this.listReforms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listReforms.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listReforms.FormattingEnabled = true;
            this.listReforms.ItemHeight = 17;
            this.listReforms.Location = new System.Drawing.Point(421, 78);
            this.listReforms.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listReforms.Name = "listReforms";
            this.listReforms.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listReforms.Size = new System.Drawing.Size(384, 463);
            this.listReforms.TabIndex = 10;
            this.listReforms.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listReforms_DrawItem);
            this.listReforms.SelectedIndexChanged += new System.EventHandler(this.listReforms_SelectedIndexChanged);
            this.listReforms.SizeChanged += new System.EventHandler(this.listReforms_SizeChanged);
            // 
            // txtReformsPath
            // 
            this.txtReformsPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReformsPath.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtReformsPath.Location = new System.Drawing.Point(421, 35);
            this.txtReformsPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtReformsPath.Name = "txtReformsPath";
            this.txtReformsPath.Size = new System.Drawing.Size(346, 20);
            this.txtReformsPath.TabIndex = 11;
            this.txtReformsPath.TextChanged += new System.EventHandler(this.txtReformsPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "Baselines";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(421, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Reforms";
            // 
            // lblMissingFiles
            // 
            this.lblMissingFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMissingFiles.AutoSize = true;
            this.lblMissingFiles.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMissingFiles.Location = new System.Drawing.Point(675, 578);
            this.lblMissingFiles.Name = "lblMissingFiles";
            this.lblMissingFiles.Size = new System.Drawing.Size(130, 14);
            this.lblMissingFiles.TabIndex = 16;
            this.lblMissingFiles.TabStop = true;
            this.lblMissingFiles.Text = "Is your file not visible?";
            this.lblMissingFiles.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblMissingFiles_LinkClicked);
            // 
            // lblMultiSelect
            // 
            this.lblMultiSelect.AutoSize = true;
            this.lblMultiSelect.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMultiSelect.Location = new System.Drawing.Point(610, 545);
            this.lblMultiSelect.Name = "lblMultiSelect";
            this.lblMultiSelect.Size = new System.Drawing.Size(195, 14);
            this.lblMultiSelect.TabIndex = 17;
            this.lblMultiSelect.Text = "Use Ctrl or Shift to select multiple files";
            // 
            // lblOrderRetained
            // 
            this.lblOrderRetained.AutoSize = true;
            this.lblOrderRetained.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrderRetained.Location = new System.Drawing.Point(610, 559);
            this.lblOrderRetained.Name = "lblOrderRetained";
            this.lblOrderRetained.Size = new System.Drawing.Size(194, 14);
            this.lblOrderRetained.TabIndex = 18;
            this.lblOrderRetained.Text = "The order of selection will be retained";
            // 
            // SelectBaselinesReforms
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(817, 620);
            this.Controls.Add(this.lblOrderRetained);
            this.Controls.Add(this.lblMultiSelect);
            this.Controls.Add(this.lblMissingFiles);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectReformsPath);
            this.Controls.Add(this.listReforms);
            this.Controls.Add(this.txtReformsPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSelectBaselinesPath);
            this.Controls.Add(this.listBaselines);
            this.Controls.Add(this.txtBaselinesPath);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider.SetHelpKeyword(this, "Settings_BaselinesReforms.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectBaselinesReforms";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select baselines and reforms";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnSelectBaselinesPath;
        private System.Windows.Forms.ListBox listBaselines;
        private System.Windows.Forms.TextBox txtBaselinesPath;
        private System.Windows.Forms.Button btnSelectReformsPath;
        private System.Windows.Forms.ListBox listReforms;
        private System.Windows.Forms.TextBox txtReformsPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lblMissingFiles;
        private System.Windows.Forms.Label lblMultiSelect;
        private System.Windows.Forms.Label lblOrderRetained;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}