namespace EM_UI.VersionControl.Dialogs
{
    partial class VCSelectConfigFile
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
            this.label1 = new System.Windows.Forms.Label();
            this.varconfigRadio = new System.Windows.Forms.RadioButton();
            this.exchangeratesRadio = new System.Windows.Forms.RadioButton();
            this.hicpconfigRadio = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.switchablepolicyconfigRadio = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(285, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the configuration file that you would like to compare:";
            // 
            // varconfigRadio
            // 
            this.varconfigRadio.AutoSize = true;
            this.varconfigRadio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.varconfigRadio.Checked = true;
            this.varconfigRadio.Location = new System.Drawing.Point(15, 41);
            this.varconfigRadio.Name = "varconfigRadio";
            this.varconfigRadio.Size = new System.Drawing.Size(105, 17);
            this.varconfigRadio.TabIndex = 1;
            this.varconfigRadio.TabStop = true;
            this.varconfigRadio.Text = "VARCONFIG.xml";
            this.varconfigRadio.UseVisualStyleBackColor = true;
            // 
            // exchangeratesRadio
            // 
            this.exchangeratesRadio.AutoSize = true;
            this.exchangeratesRadio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.exchangeratesRadio.Location = new System.Drawing.Point(15, 64);
            this.exchangeratesRadio.Name = "exchangeratesRadio";
            this.exchangeratesRadio.Size = new System.Drawing.Size(178, 17);
            this.exchangeratesRadio.TabIndex = 2;
            this.exchangeratesRadio.TabStop = true;
            this.exchangeratesRadio.Text = "EXCHANGERATESCONFIG.xml";
            this.exchangeratesRadio.UseVisualStyleBackColor = true;
            // 
            // hicpconfigRadio
            // 
            this.hicpconfigRadio.AutoSize = true;
            this.hicpconfigRadio.Location = new System.Drawing.Point(15, 87);
            this.hicpconfigRadio.Name = "hicpconfigRadio";
            this.hicpconfigRadio.Size = new System.Drawing.Size(108, 17);
            this.hicpconfigRadio.TabIndex = 3;
            this.hicpconfigRadio.TabStop = true;
            this.hicpconfigRadio.Text = "HICPCONFIG.xml";
            this.hicpconfigRadio.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(70, 144);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 23);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(167, 144);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // switchablepolicyconfigRadio
            // 
            this.switchablepolicyconfigRadio.AutoSize = true;
            this.switchablepolicyconfigRadio.Location = new System.Drawing.Point(15, 110);
            this.switchablepolicyconfigRadio.Name = "switchablepolicyconfigRadio";
            this.switchablepolicyconfigRadio.Size = new System.Drawing.Size(191, 17);
            this.switchablepolicyconfigRadio.TabIndex = 13;
            this.switchablepolicyconfigRadio.TabStop = true;
            this.switchablepolicyconfigRadio.Text = "SWITCHABLEPOLICYCONFIG.xml";
            this.switchablepolicyconfigRadio.UseVisualStyleBackColor = true;
            // 
            // VCSelectConfigFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 179);
            this.Controls.Add(this.switchablepolicyconfigRadio);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.hicpconfigRadio);
            this.Controls.Add(this.exchangeratesRadio);
            this.Controls.Add(this.varconfigRadio);
            this.Controls.Add(this.label1);
            this.Name = "VCSelectConfigFile";
            this.ShowIcon = false;
            this.Text = "Version Control - Select configuration file";
            this.Load += new System.EventHandler(this.VCSelectConfigFile_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton varconfigRadio;
        private System.Windows.Forms.RadioButton exchangeratesRadio;
        private System.Windows.Forms.RadioButton hicpconfigRadio;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton switchablepolicyconfigRadio;
    }
}