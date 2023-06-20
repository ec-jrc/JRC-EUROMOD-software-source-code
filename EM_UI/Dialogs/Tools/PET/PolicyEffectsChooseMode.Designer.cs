namespace EM_UI.Dialogs.Tools
{
    partial class PolicyEffectsChooseMode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolicyEffectsChooseMode));
            this.btnBasic = new DevExpress.XtraEditors.SimpleButton();
            this.btnAdvanced = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // btnBasic
            // 
            this.btnBasic.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
            this.btnBasic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBasic.Appearance.Options.UseTextOptions = true;
            this.btnBasic.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnBasic.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnBasic.Location = new System.Drawing.Point(46, 17);
            this.btnBasic.Name = "btnBasic";
            this.btnBasic.Size = new System.Drawing.Size(246, 149);
            this.btnBasic.TabIndex = 0;
            this.btnBasic.Text = "<b><size=+2>Basic Version</size></b>\r\n<br><i>(limited options && single decomposi" +
    "tion)</i>\r\n<br><b><color=green>Use this for Country Reports!</color></b>";
            this.btnBasic.Click += new System.EventHandler(this.btnBasic_Click);
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdvanced.Appearance.Options.UseTextOptions = true;
            this.btnAdvanced.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnAdvanced.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnAdvanced.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAdvanced.Location = new System.Drawing.Point(318, 17);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(268, 149);
            this.btnAdvanced.TabIndex = 1;
            this.btnAdvanced.Text = resources.GetString("btnAdvanced.Text");
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(487, 179);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(365, 129);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(175, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://doi.org/10.1111/obes.12354";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // PolicyEffectsChooseMode
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(639, 178);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.btnBasic);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(655, 217);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(655, 217);
            this.Name = "PolicyEffectsChooseMode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please choose a PET mode";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnBasic;
        private DevExpress.XtraEditors.SimpleButton btnAdvanced;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}