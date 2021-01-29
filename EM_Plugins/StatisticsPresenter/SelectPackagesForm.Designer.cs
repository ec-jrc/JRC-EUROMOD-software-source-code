namespace StatisticsPresenter
{
    partial class SelectPackagesForm
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
            this.listPackages = new System.Windows.Forms.ListBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.panCaptions = new System.Windows.Forms.Panel();
            this.labSubCaption = new System.Windows.Forms.Label();
            this.labCaption = new System.Windows.Forms.Label();
            this.lblMultiSelect = new System.Windows.Forms.Label();
            this.panCaptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(749, 509);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 37);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(659, 509);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(81, 37);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // listPackages
            // 
            this.listPackages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listPackages.FormattingEnabled = true;
            this.listPackages.ItemHeight = 17;
            this.listPackages.Location = new System.Drawing.Point(14, 121);
            this.listPackages.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listPackages.Name = "listPackages";
            this.listPackages.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listPackages.Size = new System.Drawing.Size(816, 361);
            this.listPackages.TabIndex = 8;
            this.listPackages.SelectedIndexChanged += new System.EventHandler(this.listPackages_SelectedIndexChanged);
            this.listPackages.SizeChanged += new System.EventHandler(this.listPackages_SizeChanged);
            this.listPackages.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listPackages_KeyDown);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonDelete.Location = new System.Drawing.Point(104, 509);
            this.buttonDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(81, 37);
            this.buttonDelete.TabIndex = 10;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAdd.Location = new System.Drawing.Point(14, 509);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(81, 37);
            this.buttonAdd.TabIndex = 9;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
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
            this.panCaptions.Location = new System.Drawing.Point(14, 13);
            this.panCaptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panCaptions.Name = "panCaptions";
            this.panCaptions.Size = new System.Drawing.Size(816, 93);
            this.panCaptions.TabIndex = 11;
            // 
            // labSubCaption
            // 
            this.labSubCaption.AutoSize = true;
            this.labSubCaption.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labSubCaption.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labSubCaption.Location = new System.Drawing.Point(17, 57);
            this.labSubCaption.Name = "labSubCaption";
            this.labSubCaption.Size = new System.Drawing.Size(105, 18);
            this.labSubCaption.TabIndex = 2;
            this.labSubCaption.Text = "Select Packages";
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
            // lblMultiSelect
            // 
            this.lblMultiSelect.AutoSize = true;
            this.lblMultiSelect.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMultiSelect.Location = new System.Drawing.Point(12, 489);
            this.lblMultiSelect.Name = "lblMultiSelect";
            this.lblMultiSelect.Size = new System.Drawing.Size(197, 14);
            this.lblMultiSelect.TabIndex = 18;
            this.lblMultiSelect.Text = "Use Ctrl or Shift to delete multiple files";
            // 
            // SelectPackagesForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(844, 563);
            this.Controls.Add(this.lblMultiSelect);
            this.Controls.Add(this.panCaptions);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listPackages);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectPackagesForm";
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
        private System.Windows.Forms.ListBox listPackages;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Panel panCaptions;
        private System.Windows.Forms.Label labSubCaption;
        private System.Windows.Forms.Label labCaption;
        private System.Windows.Forms.Label lblMultiSelect;
    }
}