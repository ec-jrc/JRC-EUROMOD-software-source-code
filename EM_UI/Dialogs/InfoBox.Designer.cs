namespace EM_UI.Dialogs
{
    partial class InfoBox
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
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnOKForClosing = new System.Windows.Forms.Button();
            this.btnCancelForClosing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInfo.Location = new System.Drawing.Point(0, 0);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfo.Size = new System.Drawing.Size(506, 218);
            this.txtInfo.TabIndex = 1;
            this.txtInfo.TabStop = false;
            // 
            // btnOKForClosing
            // 
            this.btnOKForClosing.Location = new System.Drawing.Point(215, 242);
            this.btnOKForClosing.Name = "btnOKForClosing";
            this.btnOKForClosing.Size = new System.Drawing.Size(0, 0);
            this.btnOKForClosing.TabIndex = 2;
            this.btnOKForClosing.Text = "just for closing dialog with ok";
            this.btnOKForClosing.UseVisualStyleBackColor = true;
            this.btnOKForClosing.Click += new System.EventHandler(this.btnOKForClosing_Click);
            // 
            // btnCancelForClosing
            // 
            this.btnCancelForClosing.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelForClosing.Location = new System.Drawing.Point(173, 242);
            this.btnCancelForClosing.Name = "btnCancelForClosing";
            this.btnCancelForClosing.Size = new System.Drawing.Size(0, 0);
            this.btnCancelForClosing.TabIndex = 3;
            this.btnCancelForClosing.Text = "just for closing dialog with escape";
            this.btnCancelForClosing.UseVisualStyleBackColor = true;
            // 
            // InfoBox
            // 
            this.AcceptButton = this.btnOKForClosing;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelForClosing;
            this.ClientSize = new System.Drawing.Size(505, 217);
            this.Controls.Add(this.btnCancelForClosing);
            this.Controls.Add(this.btnOKForClosing);
            this.Controls.Add(this.txtInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InfoBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnOKForClosing;
        private System.Windows.Forms.Button btnCancelForClosing;
    }
}