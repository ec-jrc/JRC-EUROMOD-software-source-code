namespace EM_UI.VersionControl.Merging
{
    partial class MergeForm
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
            this.components = new System.ComponentModel.Container();
            this.labCountry = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNameLocal = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picCountryPrivateAcceptLocal = new System.Windows.Forms.PictureBox();
            this.menAcceptReject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miAccept = new System.Windows.Forms.ToolStripMenuItem();
            this.miReject = new System.Windows.Forms.ToolStripMenuItem();
            this.picCountryPrivateChangedLocal = new System.Windows.Forms.PictureBox();
            this.picCountryNameAcceptLocal = new System.Windows.Forms.PictureBox();
            this.picCountryNameChangedLocal = new System.Windows.Forms.PictureBox();
            this.chkCountryPrivateLocal = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.picCountryPrivateAcceptRemote = new System.Windows.Forms.PictureBox();
            this.picCountryPrivateChangedRemote = new System.Windows.Forms.PictureBox();
            this.picCountryNameAcceptRemote = new System.Windows.Forms.PictureBox();
            this.picCountryNameChangedRemote = new System.Windows.Forms.PictureBox();
            this.chkCountryPrivateRemote = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNameRemote = new System.Windows.Forms.TextBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSystem = new System.Windows.Forms.TabPage();
            this.tabSpine = new System.Windows.Forms.TabPage();
            this.tabData = new System.Windows.Forms.TabPage();
            this.tabCondForm = new System.Windows.Forms.TabPage();
            this.tabUpratingIndices = new System.Windows.Forms.TabPage();
            this.tabExtensions = new System.Windows.Forms.TabPage();
            this.tabLookGroups = new System.Windows.Forms.TabPage();
            this.tabExtSwitches = new System.Windows.Forms.TabPage();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateAcceptLocal)).BeginInit();
            this.menAcceptReject.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateChangedLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameAcceptLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameChangedLocal)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateAcceptRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateChangedRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameAcceptRemote)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameChangedRemote)).BeginInit();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // labCountry
            // 
            this.labCountry.AutoSize = true;
            this.labCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labCountry.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.labCountry.Location = new System.Drawing.Point(13, 19);
            this.labCountry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labCountry.Name = "labCountry";
            this.labCountry.Size = new System.Drawing.Size(88, 25);
            this.labCountry.TabIndex = 1;
            this.labCountry.Text = "Country";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name";
            // 
            // txtNameLocal
            // 
            this.txtNameLocal.BackColor = System.Drawing.SystemColors.Control;
            this.txtNameLocal.Enabled = false;
            this.txtNameLocal.Location = new System.Drawing.Point(52, 18);
            this.txtNameLocal.Name = "txtNameLocal";
            this.txtNameLocal.Size = new System.Drawing.Size(93, 22);
            this.txtNameLocal.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picCountryPrivateAcceptLocal);
            this.groupBox1.Controls.Add(this.picCountryPrivateChangedLocal);
            this.groupBox1.Controls.Add(this.picCountryNameAcceptLocal);
            this.groupBox1.Controls.Add(this.picCountryNameChangedLocal);
            this.groupBox1.Controls.Add(this.chkCountryPrivateLocal);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtNameLocal);
            this.groupBox1.Location = new System.Drawing.Point(162, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(332, 44);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Local";
            // 
            // picCountryPrivateAcceptLocal
            // 
            this.picCountryPrivateAcceptLocal.ContextMenuStrip = this.menAcceptReject;
            this.picCountryPrivateAcceptLocal.Image = global::EM_UI.Properties.Resources.merge_accept;
            this.picCountryPrivateAcceptLocal.Location = new System.Drawing.Point(297, 18);
            this.picCountryPrivateAcceptLocal.Name = "picCountryPrivateAcceptLocal";
            this.picCountryPrivateAcceptLocal.Size = new System.Drawing.Size(22, 22);
            this.picCountryPrivateAcceptLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryPrivateAcceptLocal.TabIndex = 8;
            this.picCountryPrivateAcceptLocal.TabStop = false;
            this.picCountryPrivateAcceptLocal.Visible = false;
            // 
            // menAcceptReject
            // 
            this.menAcceptReject.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAccept,
            this.miReject});
            this.menAcceptReject.Name = "menAcceptReject";
            this.menAcceptReject.Size = new System.Drawing.Size(145, 68);
            // 
            // miAccept
            // 
            this.miAccept.Image = global::EM_UI.Properties.Resources.merge_accept;
            this.miAccept.Name = "miAccept";
            this.miAccept.Size = new System.Drawing.Size(144, 32);
            this.miAccept.Text = "Accept";
            this.miAccept.Click += new System.EventHandler(this.miAccept_Click);
            // 
            // miReject
            // 
            this.miReject.Image = global::EM_UI.Properties.Resources.merge_reject;
            this.miReject.Name = "miReject";
            this.miReject.Size = new System.Drawing.Size(144, 32);
            this.miReject.Text = "Reject";
            this.miReject.Click += new System.EventHandler(this.miReject_Click);
            // 
            // picCountryPrivateChangedLocal
            // 
            this.picCountryPrivateChangedLocal.Image = global::EM_UI.Properties.Resources.ChangeTypeChanged;
            this.picCountryPrivateChangedLocal.Location = new System.Drawing.Point(271, 17);
            this.picCountryPrivateChangedLocal.Name = "picCountryPrivateChangedLocal";
            this.picCountryPrivateChangedLocal.Size = new System.Drawing.Size(22, 22);
            this.picCountryPrivateChangedLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryPrivateChangedLocal.TabIndex = 7;
            this.picCountryPrivateChangedLocal.TabStop = false;
            this.picCountryPrivateChangedLocal.Visible = false;
            // 
            // picCountryNameAcceptLocal
            // 
            this.picCountryNameAcceptLocal.ContextMenuStrip = this.menAcceptReject;
            this.picCountryNameAcceptLocal.Image = global::EM_UI.Properties.Resources.merge_accept;
            this.picCountryNameAcceptLocal.Location = new System.Drawing.Point(174, 18);
            this.picCountryNameAcceptLocal.Name = "picCountryNameAcceptLocal";
            this.picCountryNameAcceptLocal.Size = new System.Drawing.Size(22, 22);
            this.picCountryNameAcceptLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryNameAcceptLocal.TabIndex = 6;
            this.picCountryNameAcceptLocal.TabStop = false;
            this.picCountryNameAcceptLocal.Visible = false;
            // 
            // picCountryNameChangedLocal
            // 
            this.picCountryNameChangedLocal.Image = global::EM_UI.Properties.Resources.ChangeTypeChanged;
            this.picCountryNameChangedLocal.Location = new System.Drawing.Point(148, 17);
            this.picCountryNameChangedLocal.Name = "picCountryNameChangedLocal";
            this.picCountryNameChangedLocal.Size = new System.Drawing.Size(22, 22);
            this.picCountryNameChangedLocal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryNameChangedLocal.TabIndex = 5;
            this.picCountryNameChangedLocal.TabStop = false;
            this.picCountryNameChangedLocal.Visible = false;
            // 
            // chkCountryPrivateLocal
            // 
            this.chkCountryPrivateLocal.AutoSize = true;
            this.chkCountryPrivateLocal.Enabled = false;
            this.chkCountryPrivateLocal.Location = new System.Drawing.Point(202, 17);
            this.chkCountryPrivateLocal.Name = "chkCountryPrivateLocal";
            this.chkCountryPrivateLocal.Size = new System.Drawing.Size(74, 21);
            this.chkCountryPrivateLocal.TabIndex = 4;
            this.chkCountryPrivateLocal.Text = "Private";
            this.chkCountryPrivateLocal.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.picCountryPrivateAcceptRemote);
            this.groupBox2.Controls.Add(this.picCountryPrivateChangedRemote);
            this.groupBox2.Controls.Add(this.picCountryNameAcceptRemote);
            this.groupBox2.Controls.Add(this.picCountryNameChangedRemote);
            this.groupBox2.Controls.Add(this.chkCountryPrivateRemote);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtNameRemote);
            this.groupBox2.Location = new System.Drawing.Point(500, 9);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(332, 44);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Remote";
            // 
            // picCountryPrivateAcceptRemote
            // 
            this.picCountryPrivateAcceptRemote.ContextMenuStrip = this.menAcceptReject;
            this.picCountryPrivateAcceptRemote.Image = global::EM_UI.Properties.Resources.merge_accept;
            this.picCountryPrivateAcceptRemote.Location = new System.Drawing.Point(297, 18);
            this.picCountryPrivateAcceptRemote.Name = "picCountryPrivateAcceptRemote";
            this.picCountryPrivateAcceptRemote.Size = new System.Drawing.Size(22, 22);
            this.picCountryPrivateAcceptRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryPrivateAcceptRemote.TabIndex = 8;
            this.picCountryPrivateAcceptRemote.TabStop = false;
            this.picCountryPrivateAcceptRemote.Visible = false;
            // 
            // picCountryPrivateChangedRemote
            // 
            this.picCountryPrivateChangedRemote.Image = global::EM_UI.Properties.Resources.ChangeTypeChanged;
            this.picCountryPrivateChangedRemote.Location = new System.Drawing.Point(271, 17);
            this.picCountryPrivateChangedRemote.Name = "picCountryPrivateChangedRemote";
            this.picCountryPrivateChangedRemote.Size = new System.Drawing.Size(22, 22);
            this.picCountryPrivateChangedRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryPrivateChangedRemote.TabIndex = 7;
            this.picCountryPrivateChangedRemote.TabStop = false;
            this.picCountryPrivateChangedRemote.Visible = false;
            // 
            // picCountryNameAcceptRemote
            // 
            this.picCountryNameAcceptRemote.ContextMenuStrip = this.menAcceptReject;
            this.picCountryNameAcceptRemote.Image = global::EM_UI.Properties.Resources.merge_accept;
            this.picCountryNameAcceptRemote.Location = new System.Drawing.Point(174, 18);
            this.picCountryNameAcceptRemote.Name = "picCountryNameAcceptRemote";
            this.picCountryNameAcceptRemote.Size = new System.Drawing.Size(22, 22);
            this.picCountryNameAcceptRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryNameAcceptRemote.TabIndex = 6;
            this.picCountryNameAcceptRemote.TabStop = false;
            this.picCountryNameAcceptRemote.Visible = false;
            // 
            // picCountryNameChangedRemote
            // 
            this.picCountryNameChangedRemote.Image = global::EM_UI.Properties.Resources.ChangeTypeChanged;
            this.picCountryNameChangedRemote.Location = new System.Drawing.Point(148, 17);
            this.picCountryNameChangedRemote.Name = "picCountryNameChangedRemote";
            this.picCountryNameChangedRemote.Size = new System.Drawing.Size(22, 22);
            this.picCountryNameChangedRemote.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCountryNameChangedRemote.TabIndex = 5;
            this.picCountryNameChangedRemote.TabStop = false;
            this.picCountryNameChangedRemote.Visible = false;
            // 
            // chkCountryPrivateRemote
            // 
            this.chkCountryPrivateRemote.AutoSize = true;
            this.chkCountryPrivateRemote.Enabled = false;
            this.chkCountryPrivateRemote.Location = new System.Drawing.Point(202, 17);
            this.chkCountryPrivateRemote.Name = "chkCountryPrivateRemote";
            this.chkCountryPrivateRemote.Size = new System.Drawing.Size(74, 21);
            this.chkCountryPrivateRemote.TabIndex = 4;
            this.chkCountryPrivateRemote.Text = "Private";
            this.chkCountryPrivateRemote.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Name";
            // 
            // txtNameRemote
            // 
            this.txtNameRemote.BackColor = System.Drawing.SystemColors.Control;
            this.txtNameRemote.Enabled = false;
            this.txtNameRemote.Location = new System.Drawing.Point(52, 18);
            this.txtNameRemote.Name = "txtNameRemote";
            this.txtNameRemote.Size = new System.Drawing.Size(93, 22);
            this.txtNameRemote.TabIndex = 3;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabSystem);
            this.tabControl.Controls.Add(this.tabSpine);
            this.tabControl.Controls.Add(this.tabData);
            this.tabControl.Controls.Add(this.tabCondForm);
            this.tabControl.Controls.Add(this.tabUpratingIndices);
            this.tabControl.Controls.Add(this.tabExtensions);
            this.tabControl.Controls.Add(this.tabExtSwitches);
            this.tabControl.Controls.Add(this.tabLookGroups);
            this.tabControl.Location = new System.Drawing.Point(12, 59);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(827, 450);
            this.tabControl.TabIndex = 10;
            // 
            // tabSystem
            // 
            this.tabSystem.Location = new System.Drawing.Point(4, 25);
            this.tabSystem.Name = "tabSystem";
            this.tabSystem.Padding = new System.Windows.Forms.Padding(3);
            this.tabSystem.Size = new System.Drawing.Size(819, 421);
            this.tabSystem.TabIndex = 0;
            this.tabSystem.Text = "Systems";
            this.tabSystem.UseVisualStyleBackColor = true;
            // 
            // tabSpine
            // 
            this.tabSpine.Location = new System.Drawing.Point(4, 25);
            this.tabSpine.Name = "tabSpine";
            this.tabSpine.Padding = new System.Windows.Forms.Padding(3);
            this.tabSpine.Size = new System.Drawing.Size(819, 421);
            this.tabSpine.TabIndex = 1;
            this.tabSpine.Text = "Policy-Spine";
            this.tabSpine.UseVisualStyleBackColor = true;
            // 
            // tabData
            // 
            this.tabData.Location = new System.Drawing.Point(4, 25);
            this.tabData.Name = "tabData";
            this.tabData.Padding = new System.Windows.Forms.Padding(3);
            this.tabData.Size = new System.Drawing.Size(819, 421);
            this.tabData.TabIndex = 3;
            this.tabData.Text = "Data";
            this.tabData.UseVisualStyleBackColor = true;
            // 
            // tabCondForm
            // 
            this.tabCondForm.Location = new System.Drawing.Point(4, 25);
            this.tabCondForm.Name = "tabCondForm";
            this.tabCondForm.Padding = new System.Windows.Forms.Padding(3);
            this.tabCondForm.Size = new System.Drawing.Size(819, 421);
            this.tabCondForm.TabIndex = 2;
            this.tabCondForm.Text = "Conditional Formatting";
            this.tabCondForm.UseVisualStyleBackColor = true;
            // 
            // tabUpratingIndices
            // 
            this.tabUpratingIndices.Location = new System.Drawing.Point(4, 25);
            this.tabUpratingIndices.Name = "tabUpratingIndices";
            this.tabUpratingIndices.Padding = new System.Windows.Forms.Padding(3);
            this.tabUpratingIndices.Size = new System.Drawing.Size(819, 421);
            this.tabUpratingIndices.TabIndex = 2;
            this.tabUpratingIndices.Text = "Uprating Indices";
            this.tabUpratingIndices.UseVisualStyleBackColor = true;
            // 
            // tabExtensions
            // 
            this.tabExtensions.Location = new System.Drawing.Point(4, 25);
            this.tabExtensions.Name = "tabExtensions";
            this.tabExtensions.Padding = new System.Windows.Forms.Padding(3);
            this.tabExtensions.Size = new System.Drawing.Size(819, 421);
            this.tabExtensions.TabIndex = 2;
            this.tabExtensions.Text = "Extensions";
            this.tabExtensions.UseVisualStyleBackColor = true;
            //
            // tabExtSwitches
            //
            this.tabExtSwitches.Location = new System.Drawing.Point(4, 25);
            this.tabExtSwitches.Name = "tabExtSwitches";
            this.tabExtSwitches.Padding = new System.Windows.Forms.Padding(3);
            this.tabExtSwitches.Size = new System.Drawing.Size(819, 421);
            this.tabExtSwitches.TabIndex = 2;
            this.tabExtSwitches.Text = "Extensions Switches";
            this.tabExtSwitches.UseVisualStyleBackColor = true;
            // 
            // tabLookGroups
            // 
            this.tabLookGroups.Location = new System.Drawing.Point(4, 25);
            this.tabLookGroups.Name = "tabLookGroups";
            this.tabLookGroups.Padding = new System.Windows.Forms.Padding(3);
            this.tabLookGroups.Size = new System.Drawing.Size(819, 421);
            this.tabLookGroups.TabIndex = 2;
            this.tabLookGroups.Text = "Groups";
            this.tabLookGroups.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(731, 522);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(108, 33);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApply.Image = global::EM_UI.Properties.Resources.apply;
            this.btnApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnApply.Location = new System.Drawing.Point(503, 522);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(108, 33);
            this.btnApply.TabIndex = 12;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Image = global::EM_UI.Properties.Resources.Save16;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(617, 522);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(108, 33);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // MergeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 567);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labCountry);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_MergeTool.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.MinimizeBox = false;
            this.Name = "MergeForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Merge";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MergeForm_Load);
            this.Shown += MergeForm_Shown;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateAcceptLocal)).EndInit();
            this.menAcceptReject.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateChangedLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameAcceptLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameChangedLocal)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateAcceptRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryPrivateChangedRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameAcceptRemote)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCountryNameChangedRemote)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labCountry;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNameLocal;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox picCountryNameChangedLocal;
        private System.Windows.Forms.CheckBox chkCountryPrivateLocal;
        private System.Windows.Forms.PictureBox picCountryPrivateAcceptLocal;
        private System.Windows.Forms.PictureBox picCountryPrivateChangedLocal;
        private System.Windows.Forms.PictureBox picCountryNameAcceptLocal;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox picCountryPrivateAcceptRemote;
        private System.Windows.Forms.PictureBox picCountryPrivateChangedRemote;
        private System.Windows.Forms.PictureBox picCountryNameAcceptRemote;
        private System.Windows.Forms.PictureBox picCountryNameChangedRemote;
        private System.Windows.Forms.CheckBox chkCountryPrivateRemote;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNameRemote;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSystem;
        private System.Windows.Forms.TabPage tabSpine;
        private System.Windows.Forms.TabPage tabCondForm;
        private System.Windows.Forms.TabPage tabUpratingIndices;
        private System.Windows.Forms.TabPage tabExtensions;
        private System.Windows.Forms.TabPage tabLookGroups;
        private System.Windows.Forms.TabPage tabExtSwitches;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TabPage tabData;
        private System.Windows.Forms.ContextMenuStrip menAcceptReject;
        private System.Windows.Forms.ToolStripMenuItem miAccept;
        private System.Windows.Forms.ToolStripMenuItem miReject;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}