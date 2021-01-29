namespace StatisticsPresenter
{
    partial class SelectFilesForm
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
            this.btnSelectPath = new System.Windows.Forms.Button();
            this.listFiles = new System.Windows.Forms.ListBox();
            this.textPath = new System.Windows.Forms.TextBox();
            this.panCaptions = new System.Windows.Forms.Panel();
            this.labCaption = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnNoSuitableFiles = new System.Windows.Forms.Button();
            this.lblMissingFiles = new System.Windows.Forms.LinkLabel();
            this.lblMultiSelect = new System.Windows.Forms.Label();
            this.lblOrderRetained = new System.Windows.Forms.Label();
            this.panCaptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(324, 362);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 37);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(231, 362);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(81, 37);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSelectPath
            // 
            this.btnSelectPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectPath.Location = new System.Drawing.Point(592, 85);
            this.btnSelectPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectPath.Name = "btnSelectPath";
            this.btnSelectPath.Size = new System.Drawing.Size(32, 33);
            this.btnSelectPath.TabIndex = 9;
            this.btnSelectPath.Text = "...";
            this.btnSelectPath.UseVisualStyleBackColor = true;
            this.btnSelectPath.Click += new System.EventHandler(this.SelectPath_Click);
            // 
            // listFiles
            // 
            this.listFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listFiles.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listFiles.FormattingEnabled = true;
            this.listFiles.ItemHeight = 17;
            this.listFiles.Location = new System.Drawing.Point(12, 126);
            this.listFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listFiles.Name = "listFiles";
            this.listFiles.Size = new System.Drawing.Size(612, 208);
            this.listFiles.TabIndex = 5;
            this.listFiles.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listFiles_DrawItem);
            this.listFiles.SelectedIndexChanged += new System.EventHandler(this.listFiles_SelectedIndexChanged);
            this.listFiles.SizeChanged += new System.EventHandler(this.listFiles_SizeChanged);
            this.listFiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.list_MouseDoubleClick);
            // 
            // textPath
            // 
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.Font = new System.Drawing.Font("Calibri", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textPath.Location = new System.Drawing.Point(53, 91);
            this.textPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textPath.Name = "textPath";
            this.textPath.Size = new System.Drawing.Size(533, 20);
            this.textPath.TabIndex = 8;
            this.textPath.TextChanged += new System.EventHandler(this.textPath_TextChanged);
            // 
            // panCaptions
            // 
            this.panCaptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panCaptions.AutoScroll = true;
            this.panCaptions.BackColor = System.Drawing.SystemColors.Menu;
            this.panCaptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panCaptions.Controls.Add(this.labCaption);
            this.panCaptions.Location = new System.Drawing.Point(12, 13);
            this.panCaptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panCaptions.Name = "panCaptions";
            this.panCaptions.Size = new System.Drawing.Size(612, 62);
            this.panCaptions.TabIndex = 10;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 11;
            this.label1.Text = "Path:";
            // 
            // btnNoSuitableFiles
            // 
            this.btnNoSuitableFiles.Location = new System.Drawing.Point(12, 280);
            this.btnNoSuitableFiles.Name = "btnNoSuitableFiles";
            this.btnNoSuitableFiles.Size = new System.Drawing.Size(612, 54);
            this.btnNoSuitableFiles.TabIndex = 12;
            this.btnNoSuitableFiles.Text = "No suitable files found (click for info)";
            this.btnNoSuitableFiles.UseVisualStyleBackColor = true;
            this.btnNoSuitableFiles.Visible = false;
            this.btnNoSuitableFiles.Click += new System.EventHandler(this.btnNoSuitableFiles_Click);
            // 
            // lblMissingFiles
            // 
            this.lblMissingFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMissingFiles.AutoSize = true;
            this.lblMissingFiles.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMissingFiles.Location = new System.Drawing.Point(494, 374);
            this.lblMissingFiles.Name = "lblMissingFiles";
            this.lblMissingFiles.Size = new System.Drawing.Size(130, 14);
            this.lblMissingFiles.TabIndex = 13;
            this.lblMissingFiles.TabStop = true;
            this.lblMissingFiles.Text = "Is your file not visible?";
            this.lblMissingFiles.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblMissingFiles_LinkClicked);
            // 
            // lblMultiSelect
            // 
            this.lblMultiSelect.AutoSize = true;
            this.lblMultiSelect.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMultiSelect.Location = new System.Drawing.Point(429, 337);
            this.lblMultiSelect.Name = "lblMultiSelect";
            this.lblMultiSelect.Size = new System.Drawing.Size(195, 14);
            this.lblMultiSelect.TabIndex = 14;
            this.lblMultiSelect.Text = "Use Ctrl or Shift to select multiple files";
            // 
            // lblOrderRetained
            // 
            this.lblOrderRetained.AutoSize = true;
            this.lblOrderRetained.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrderRetained.Location = new System.Drawing.Point(429, 351);
            this.lblOrderRetained.Name = "lblOrderRetained";
            this.lblOrderRetained.Size = new System.Drawing.Size(194, 14);
            this.lblOrderRetained.TabIndex = 19;
            this.lblOrderRetained.Text = "The order of selection will be retained";
            // 
            // SelectFilesForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(636, 413);
            this.Controls.Add(this.lblOrderRetained);
            this.Controls.Add(this.lblMultiSelect);
            this.Controls.Add(this.lblMissingFiles);
            this.Controls.Add(this.btnNoSuitableFiles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panCaptions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSelectPath);
            this.Controls.Add(this.listFiles);
            this.Controls.Add(this.textPath);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 350);
            this.Name = "SelectFilesForm";
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
        private System.Windows.Forms.Button btnSelectPath;
        private System.Windows.Forms.ListBox listFiles;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.Panel panCaptions;
        private System.Windows.Forms.Label labCaption;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnNoSuitableFiles;
        private System.Windows.Forms.LinkLabel lblMissingFiles;
        private System.Windows.Forms.Label lblMultiSelect;
        private System.Windows.Forms.Label lblOrderRetained;
    }
}