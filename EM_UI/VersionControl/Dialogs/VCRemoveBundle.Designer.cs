namespace EM_UI.VersionControl.Dialogs
{
    partial class VCRemoveBundle
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCRemoveBundle));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnRemoveBundles = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lvBundles = new System.Windows.Forms.ListView();
            this.chkRemoveContent = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnRemoveBundles
            // 
            this.btnRemoveBundles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveBundles.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveBundles.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveBundles.Image")));
            this.btnRemoveBundles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRemoveBundles.Location = new System.Drawing.Point(492, 10);
            this.btnRemoveBundles.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveBundles.Name = "btnRemoveBundles";
            this.btnRemoveBundles.Size = new System.Drawing.Size(116, 42);
            this.btnRemoveBundles.TabIndex = 7;
            this.btnRemoveBundles.Text = "        Remove          Bundle(s)";
            this.btnRemoveBundles.UseVisualStyleBackColor = true;
            this.btnRemoveBundles.Click += new System.EventHandler(this.btnRemoveBundles_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Image = global::EM_UI.Properties.Resources.delete32;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(492, 288);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(116, 42);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lvBundles
            // 
            this.lvBundles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvBundles.CheckBoxes = true;
            this.lvBundles.HideSelection = false;
            this.lvBundles.Location = new System.Drawing.Point(9, 10);
            this.lvBundles.Margin = new System.Windows.Forms.Padding(2);
            this.lvBundles.Name = "lvBundles";
            this.lvBundles.Size = new System.Drawing.Size(480, 322);
            this.lvBundles.TabIndex = 9;
            this.lvBundles.UseCompatibleStateImageBehavior = false;
            this.lvBundles.View = System.Windows.Forms.View.Details;
            // 
            // chkRemoveContent
            // 
            this.chkRemoveContent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRemoveContent.AutoSize = true;
            this.chkRemoveContent.Location = new System.Drawing.Point(491, 144);
            this.chkRemoveContent.Margin = new System.Windows.Forms.Padding(2);
            this.chkRemoveContent.Name = "chkRemoveContent";
            this.chkRemoveContent.Size = new System.Drawing.Size(106, 17);
            this.chkRemoveContent.TabIndex = 10;
            this.chkRemoveContent.Text = "Remove Content";
            this.chkRemoveContent.UseVisualStyleBackColor = true;
            this.chkRemoveContent.Visible = false;
            // 
            // VCRemoveBundle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(617, 343);
            this.Controls.Add(this.chkRemoveContent);
            this.Controls.Add(this.lvBundles);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemoveBundles);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_Advanced.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(402, 278);
            this.Name = "VCRemoveBundle";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Version Control - Remove Bundles";
            this.Load += new System.EventHandler(this.VCRemoveBundle_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnRemoveBundles;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListView lvBundles;
        private System.Windows.Forms.CheckBox chkRemoveContent;
    }
}