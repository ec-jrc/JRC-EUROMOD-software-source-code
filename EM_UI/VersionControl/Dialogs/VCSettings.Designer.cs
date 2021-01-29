namespace EM_UI.VersionControl.Dialogs
{
    partial class VCSettings
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.proxyPort = new System.Windows.Forms.TextBox();
            this.proxyURL = new System.Windows.Forms.TextBox();
            this.radioBtnNoProxy = new System.Windows.Forms.RadioButton();
            this.radioBtnYesProxy = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(228, 400);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.AutoSize = true;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(135, 400);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 23);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Proxy port";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Proxy URL address";
            this.label1.UseMnemonic = false;
            // 
            // proxyPort
            // 
            this.proxyPort.Location = new System.Drawing.Point(105, 101);
            this.proxyPort.Name = "proxyPort";
            this.proxyPort.Size = new System.Drawing.Size(88, 20);
            this.proxyPort.TabIndex = 7;
            // 
            // proxyURL
            // 
            this.proxyURL.Location = new System.Drawing.Point(105, 70);
            this.proxyURL.Name = "proxyURL";
            this.proxyURL.Size = new System.Drawing.Size(345, 20);
            this.proxyURL.TabIndex = 6;
            // 
            // radioBtnNoProxy
            // 
            this.radioBtnNoProxy.AutoSize = true;
            this.radioBtnNoProxy.Location = new System.Drawing.Point(6, 19);
            this.radioBtnNoProxy.Name = "radioBtnNoProxy";
            this.radioBtnNoProxy.Size = new System.Drawing.Size(169, 17);
            this.radioBtnNoProxy.TabIndex = 12;
            this.radioBtnNoProxy.TabStop = true;
            this.radioBtnNoProxy.Text = "Do not use proxy configuration";
            this.radioBtnNoProxy.UseVisualStyleBackColor = true;
            this.radioBtnNoProxy.CheckedChanged += new System.EventHandler(this.radioBtnNoProxy_CheckedChanged);
            // 
            // radioBtnYesProxy
            // 
            this.radioBtnYesProxy.AutoSize = true;
            this.radioBtnYesProxy.Location = new System.Drawing.Point(6, 42);
            this.radioBtnYesProxy.Name = "radioBtnYesProxy";
            this.radioBtnYesProxy.Size = new System.Drawing.Size(136, 17);
            this.radioBtnYesProxy.TabIndex = 13;
            this.radioBtnYesProxy.TabStop = true;
            this.radioBtnYesProxy.Text = "Use proxy configuration";
            this.radioBtnYesProxy.UseVisualStyleBackColor = true;
            this.radioBtnYesProxy.CheckedChanged += new System.EventHandler(this.radioBtnYesProxy_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioBtnNoProxy);
            this.groupBox1.Controls.Add(this.radioBtnYesProxy);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.proxyPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.proxyURL);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 131);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Proxy";
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(458, 199);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server";
            // 
            // VCSettings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(486, 439);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "VCSettings";
            this.ShowIcon = false;
            this.Text = "VC Settings";
            this.Load += new System.EventHandler(this.VCSettings_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox proxyPort;
        private System.Windows.Forms.TextBox proxyURL;
        private System.Windows.Forms.RadioButton radioBtnNoProxy;
        private System.Windows.Forms.RadioButton radioBtnYesProxy;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}