namespace StatisticsPresenter
{
    partial class TakeUserInput
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
            this.panContent = new System.Windows.Forms.Panel();
            this.panCaptions = new System.Windows.Forms.Panel();
            this.labSubCaption = new System.Windows.Forms.Label();
            this.labCaption = new System.Windows.Forms.Label();
            this.panCaptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(152, 292);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 39);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(63, 292);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(81, 39);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panContent
            // 
            this.panContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panContent.Location = new System.Drawing.Point(0, 107);
            this.panContent.Margin = new System.Windows.Forms.Padding(4);
            this.panContent.Name = "panContent";
            this.panContent.Size = new System.Drawing.Size(297, 175);
            this.panContent.TabIndex = 0;
            this.panContent.TabStop = true;
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
            this.panCaptions.Location = new System.Drawing.Point(11, 11);
            this.panCaptions.Name = "panCaptions";
            this.panCaptions.Size = new System.Drawing.Size(276, 89);
            this.panCaptions.TabIndex = 5;
            // 
            // labSubCaption
            // 
            this.labSubCaption.AutoSize = true;
            this.labSubCaption.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labSubCaption.Location = new System.Drawing.Point(15, 54);
            this.labSubCaption.Name = "labSubCaption";
            this.labSubCaption.Size = new System.Drawing.Size(72, 18);
            this.labSubCaption.TabIndex = 2;
            this.labSubCaption.Text = "User Input";
            // 
            // labCaption
            // 
            this.labCaption.AutoSize = true;
            this.labCaption.Font = new System.Drawing.Font("Calibri", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labCaption.Location = new System.Drawing.Point(11, 8);
            this.labCaption.Name = "labCaption";
            this.labCaption.Size = new System.Drawing.Size(205, 37);
            this.labCaption.TabIndex = 0;
            this.labCaption.Text = "EUROMOD ASC";
            // 
            // TakeUserInput
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(297, 346);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.panCaptions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.panContent);
            this.Font = new System.Drawing.Font("Calibri", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TakeUserInput";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Statistics Presenter - User Input";
            this.panCaptions.ResumeLayout(false);
            this.panCaptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panContent;
        private System.Windows.Forms.Panel panCaptions;
        private System.Windows.Forms.Label labCaption;
        private System.Windows.Forms.Label labSubCaption;
    }
}