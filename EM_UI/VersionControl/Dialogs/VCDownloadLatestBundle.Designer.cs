namespace EM_UI.VersionControl.Dialogs
{
    partial class VCDownloadLatestBundle
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCDownloadLatestBundle));
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDestinationFolder = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textProject = new System.Windows.Forms.TextBox();
            this.txtBundle = new System.Windows.Forms.TextBox();
            this.listUnits = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSel = new System.Windows.Forms.Button();
            this.btnUnsel = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBundlePath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Project";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 45);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Bundle";
            // 
            // txtDestinationFolder
            // 
            this.txtDestinationFolder.Location = new System.Drawing.Point(11, 89);
            this.txtDestinationFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtDestinationFolder.Name = "txtDestinationFolder";
            this.txtDestinationFolder.Size = new System.Drawing.Size(647, 20);
            this.txtDestinationFolder.TabIndex = 24;
            this.txtDestinationFolder.TextChanged += new System.EventHandler(this.txtDestinationFolder_TextChanged);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectFolder.Image")));
            this.btnSelectFolder.Location = new System.Drawing.Point(663, 78);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectFolder.TabIndex = 25;
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(358, 467);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "  Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDownload.Image = ((System.Drawing.Image)(resources.GetObject("btnDownload.Image")));
            this.btnDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDownload.Location = new System.Drawing.Point(273, 467);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(80, 35);
            this.btnDownload.TabIndex = 26;
            this.btnDownload.Text = "Download";
            this.btnDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 69);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Project Path";
            // 
            // textProject
            // 
            this.textProject.Location = new System.Drawing.Point(59, 13);
            this.textProject.Margin = new System.Windows.Forms.Padding(2);
            this.textProject.Name = "textProject";
            this.textProject.ReadOnly = true;
            this.textProject.Size = new System.Drawing.Size(645, 20);
            this.textProject.TabIndex = 29;
            // 
            // txtBundle
            // 
            this.txtBundle.Location = new System.Drawing.Point(59, 42);
            this.txtBundle.Margin = new System.Windows.Forms.Padding(2);
            this.txtBundle.Name = "txtBundle";
            this.txtBundle.ReadOnly = true;
            this.txtBundle.Size = new System.Drawing.Size(645, 20);
            this.txtBundle.TabIndex = 30;
            // 
            // listUnits
            // 
            this.listUnits.CheckBoxes = true;
            this.listUnits.HideSelection = false;
            this.listUnits.Location = new System.Drawing.Point(11, 175);
            this.listUnits.Margin = new System.Windows.Forms.Padding(2);
            this.listUnits.Name = "listUnits";
            this.listUnits.Size = new System.Drawing.Size(693, 233);
            this.listUnits.TabIndex = 31;
            this.listUnits.UseCompatibleStateImageBehavior = false;
            this.listUnits.View = System.Windows.Forms.View.List;
            this.listUnits.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listUnits_ItemChecked);
            // 
            // label
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 158);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 32;
            this.label4.Text = "Content";
            // 
            // btnSel
            // 
            this.btnSel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSel.Location = new System.Drawing.Point(11, 413);
            this.btnSel.Name = "btnSel";
            this.btnSel.Size = new System.Drawing.Size(67, 23);
            this.btnSel.TabIndex = 33;
            this.btnSel.Text = "Select All";
            this.btnSel.UseVisualStyleBackColor = true;
            this.btnSel.Click += new System.EventHandler(this.btnSel_Click);
            // 
            // btnUnsel
            // 
            this.btnUnsel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnsel.Location = new System.Drawing.Point(84, 413);
            this.btnUnsel.Name = "btnUnsel";
            this.btnUnsel.Size = new System.Drawing.Size(72, 23);
            this.btnUnsel.TabIndex = 34;
            this.btnUnsel.Text = "Unselect All";
            this.btnUnsel.Click += new System.EventHandler(this.btnUnsel_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 113);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Bundle Path";
            // 
            // txtBundlePath
            // 
            this.txtBundlePath.Location = new System.Drawing.Point(11, 131);
            this.txtBundlePath.Margin = new System.Windows.Forms.Padding(2);
            this.txtBundlePath.Name = "txtBundlePath";
            this.txtBundlePath.ReadOnly = true;
            this.txtBundlePath.Size = new System.Drawing.Size(693, 20);
            this.txtBundlePath.TabIndex = 36;
            // 
            // VCDownloadLatestBundle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 510);
            this.Controls.Add(this.txtBundlePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnUnsel);
            this.Controls.Add(this.btnSel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listUnits);
            this.Controls.Add(this.txtBundle);
            this.Controls.Add(this.textProject);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtDestinationFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Name = "VCDownloadLatestBundle";
            this.ShowIcon = false;
            this.Text = "Version Control - Download Latest Bundle";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDestinationFolder;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textProject;
        private System.Windows.Forms.TextBox txtBundle;
        private System.Windows.Forms.ListView listUnits;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSel;
        private System.Windows.Forms.Button btnUnsel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBundlePath;
    }
}