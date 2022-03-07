
namespace HypotheticalHousehold
{
    partial class OpenProjectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenProjectForm));
            this.txtProjectPath = new System.Windows.Forms.TextBox();
            this.buttonSelectPath = new DevExpress.XtraEditors.SimpleButton();
            this.buttonOK = new DevExpress.XtraEditors.SimpleButton();
            this.buttonCancel = new DevExpress.XtraEditors.SimpleButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtProjectPath
            // 
            this.txtProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectPath.Location = new System.Drawing.Point(12, 33);
            this.txtProjectPath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtProjectPath.Name = "txtProjectPath";
            this.txtProjectPath.Size = new System.Drawing.Size(647, 22);
            this.txtProjectPath.TabIndex = 1;
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectPath.Image = ((System.Drawing.Image)(resources.GetObject("buttonSelectPath.Image")));
            this.buttonSelectPath.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.buttonSelectPath.Location = new System.Drawing.Point(666, 27);
            this.buttonSelectPath.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(36, 33);
            this.buttonSelectPath.TabIndex = 5;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.buttonOK.Appearance.Options.UseFont = true;
            this.buttonOK.Location = new System.Drawing.Point(498, 72);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 15, 160, 2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(99, 33);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.buttonCancel.Appearance.Options.UseFont = true;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(603, 72);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 15, 160, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(99, 33);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Project Path";
            // 
            // OpenProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 124);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.txtProjectPath);
            this.Name = "OpenProjectForm";
            this.ShowIcon = false;
            this.Text = "Hypothetical Household Tool - Open Project";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProjectPath;
        private DevExpress.XtraEditors.SimpleButton buttonSelectPath;
        private DevExpress.XtraEditors.SimpleButton buttonOK;
        private DevExpress.XtraEditors.SimpleButton buttonCancel;
        private System.Windows.Forms.Label label1;
    }
}