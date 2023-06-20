namespace EM_Statistics.StatisticsPresenter
{
    partial class SelectBaseAltsForm
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
            this.btnSelectBasePath = new System.Windows.Forms.Button();
            this.listBase = new System.Windows.Forms.ListBox();
            this.textBasePath = new System.Windows.Forms.TextBox();
            this.btnSelectAltPath = new System.Windows.Forms.Button();
            this.listAlt = new System.Windows.Forms.ListBox();
            this.textAltPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panCaptions = new System.Windows.Forms.Panel();
            this.labSubCaption = new System.Windows.Forms.Label();
            this.labCaption = new System.Windows.Forms.Label();
            this.lblMissingFiles = new System.Windows.Forms.LinkLabel();
            this.lblMultiSelect = new System.Windows.Forms.Label();
            this.lblOrderRetained = new System.Windows.Forms.Label();
            this.panCaptions.SuspendLayout();
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
            // btnSelectBasePath
            // 
            this.btnSelectBasePath.Location = new System.Drawing.Point(367, 129);
            this.btnSelectBasePath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectBasePath.Name = "btnSelectBasePath";
            this.btnSelectBasePath.Size = new System.Drawing.Size(32, 33);
            this.btnSelectBasePath.TabIndex = 9;
            this.btnSelectBasePath.Text = "...";
            this.btnSelectBasePath.UseVisualStyleBackColor = true;
            this.btnSelectBasePath.Click += new System.EventHandler(this.btnSelectBasePath_Click);
            // 
            // listBase
            // 
            this.listBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBase.FormattingEnabled = true;
            this.listBase.ItemHeight = 17;
            this.listBase.Location = new System.Drawing.Point(14, 163);
            this.listBase.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBase.Name = "listBase";
            this.listBase.Size = new System.Drawing.Size(385, 378);
            this.listBase.TabIndex = 5;
            // 
            // textBasePath
            // 
            this.textBasePath.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBasePath.Location = new System.Drawing.Point(14, 134);
            this.textBasePath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBasePath.Name = "textBasePath";
            this.textBasePath.Size = new System.Drawing.Size(346, 20);
            this.textBasePath.TabIndex = 8;
            this.textBasePath.TextChanged += new System.EventHandler(this.textBasePath_TextChanged);
            // 
            // btnSelectAltPath
            // 
            this.btnSelectAltPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectAltPath.Location = new System.Drawing.Point(773, 129);
            this.btnSelectAltPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectAltPath.Name = "btnSelectAltPath";
            this.btnSelectAltPath.Size = new System.Drawing.Size(32, 33);
            this.btnSelectAltPath.TabIndex = 12;
            this.btnSelectAltPath.Text = "...";
            this.btnSelectAltPath.UseVisualStyleBackColor = true;
            this.btnSelectAltPath.Click += new System.EventHandler(this.btnSelectAltPath_Click);
            // 
            // listAlt
            // 
            this.listAlt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listAlt.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listAlt.FormattingEnabled = true;
            this.listAlt.ItemHeight = 17;
            this.listAlt.Location = new System.Drawing.Point(421, 163);
            this.listAlt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listAlt.Name = "listAlt";
            this.listAlt.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listAlt.Size = new System.Drawing.Size(384, 378);
            this.listAlt.TabIndex = 10;
            this.listAlt.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listAlt_DrawItem);
            this.listAlt.SelectedIndexChanged += new System.EventHandler(this.listAlt_SelectedIndexChanged);
            this.listAlt.SizeChanged += new System.EventHandler(this.listAlt_SizeChanged);
            // 
            // textAltPath
            // 
            this.textAltPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAltPath.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAltPath.Location = new System.Drawing.Point(421, 134);
            this.textAltPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textAltPath.Name = "textAltPath";
            this.textAltPath.Size = new System.Drawing.Size(346, 20);
            this.textAltPath.TabIndex = 11;
            this.textAltPath.TextChanged += new System.EventHandler(this.textAltPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "Baseline Scenario";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(421, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Alternative Scenario(s)";
            // 
            // panCaptions
            // 
            this.panCaptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panCaptions.AutoScroll = true;
            this.panCaptions.BackColor = System.Drawing.SystemColors.Menu;
            this.panCaptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panCaptions.Controls.Add(this.labSubCaption);
            this.panCaptions.Controls.Add(this.labCaption);
            this.panCaptions.Location = new System.Drawing.Point(12, 13);
            this.panCaptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panCaptions.Name = "panCaptions";
            this.panCaptions.Size = new System.Drawing.Size(793, 93);
            this.panCaptions.TabIndex = 15;
            // 
            // labSubCaption
            // 
            this.labSubCaption.AutoSize = true;
            this.labSubCaption.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labSubCaption.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labSubCaption.Location = new System.Drawing.Point(17, 57);
            this.labSubCaption.Name = "labSubCaption";
            this.labSubCaption.Size = new System.Drawing.Size(219, 18);
            this.labSubCaption.TabIndex = 2;
            this.labSubCaption.Text = "Select Files for Calculating Statistic";
            // 
            // labCaption
            // 
            this.labCaption.AutoSize = true;
            this.labCaption.Font = new System.Drawing.Font("Calibri", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labCaption.Location = new System.Drawing.Point(12, 10);
            this.labCaption.Name = "labCaption";
            this.labCaption.Size = new System.Drawing.Size(205, 37);
            this.labCaption.TabIndex = 0;
            this.labCaption.Text = "EUROMOD ASC";
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
            // SelectBaseAltsForm
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
            this.Controls.Add(this.panCaptions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectAltPath);
            this.Controls.Add(this.listAlt);
            this.Controls.Add(this.textAltPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSelectBasePath);
            this.Controls.Add(this.listBase);
            this.Controls.Add(this.textBasePath);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectBaseAltsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Statistics Presenter - Select Files";
            this.panCaptions.ResumeLayout(false);
            this.panCaptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnSelectBasePath;
        private System.Windows.Forms.ListBox listBase;
        private System.Windows.Forms.TextBox textBasePath;
        private System.Windows.Forms.Button btnSelectAltPath;
        private System.Windows.Forms.ListBox listAlt;
        private System.Windows.Forms.TextBox textAltPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panCaptions;
        private System.Windows.Forms.Label labSubCaption;
        private System.Windows.Forms.Label labCaption;
        private System.Windows.Forms.LinkLabel lblMissingFiles;
        private System.Windows.Forms.Label lblMultiSelect;
        private System.Windows.Forms.Label lblOrderRetained;
    }
}