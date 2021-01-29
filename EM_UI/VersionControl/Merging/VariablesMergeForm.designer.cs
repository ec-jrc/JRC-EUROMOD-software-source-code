namespace EM_UI.VersionControl.Merging
{
    partial class VariablesMergeForm
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
            this.menAcceptReject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miAccept = new System.Windows.Forms.ToolStripMenuItem();
            this.miReject = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabVariables = new System.Windows.Forms.TabPage();
            this.tabAcronyms = new System.Windows.Forms.TabPage();
            this.tabCountryLabels = new System.Windows.Forms.TabPage();
            this.tabSwitchablePolicies = new System.Windows.Forms.TabPage();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.menAcceptReject.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
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
            // 
            // miReject
            // 
            this.miReject.Image = global::EM_UI.Properties.Resources.merge_reject;
            this.miReject.Name = "miReject";
            this.miReject.Size = new System.Drawing.Size(144, 32);
            this.miReject.Text = "Reject";
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabVariables);
            this.tabControl.Controls.Add(this.tabAcronyms);
            this.tabControl.Controls.Add(this.tabCountryLabels);
            //this.tabControl.Controls.Add(this.tabSwitchablePolicies);
            this.tabControl.Location = new System.Drawing.Point(12, 6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(827, 503);
            this.tabControl.TabIndex = 10;
            // 
            // tabVariables
            // 
            this.tabVariables.Location = new System.Drawing.Point(4, 25);
            this.tabVariables.Name = "tabVariables";
            this.tabVariables.Padding = new System.Windows.Forms.Padding(3);
            this.tabVariables.Size = new System.Drawing.Size(819, 474);
            this.tabVariables.TabIndex = 0;
            this.tabVariables.Text = "Variables";
            this.tabVariables.UseVisualStyleBackColor = true;
            // 
            // tabAcronyms
            // 
            this.tabAcronyms.Location = new System.Drawing.Point(4, 25);
            this.tabAcronyms.Name = "tabAcronyms";
            this.tabAcronyms.Padding = new System.Windows.Forms.Padding(3);
            this.tabAcronyms.Size = new System.Drawing.Size(819, 474);
            this.tabAcronyms.TabIndex = 1;
            this.tabAcronyms.Text = "Acronyms";
            this.tabAcronyms.UseVisualStyleBackColor = true;
            // 
            // tabCountryLabels
            // 
            this.tabCountryLabels.Location = new System.Drawing.Point(4, 25);
            this.tabCountryLabels.Name = "tabCountryLabels";
            this.tabCountryLabels.Padding = new System.Windows.Forms.Padding(3);
            this.tabCountryLabels.Size = new System.Drawing.Size(819, 474);
            this.tabCountryLabels.TabIndex = 2;
            this.tabCountryLabels.Text = "Country Labels";
            this.tabCountryLabels.UseVisualStyleBackColor = true;
            // 
            // tabSwitchablePolicies
            // 
            this.tabSwitchablePolicies.Location = new System.Drawing.Point(4, 25);
            this.tabSwitchablePolicies.Name = "tabSwitchablePolicies";
            this.tabSwitchablePolicies.Padding = new System.Windows.Forms.Padding(3);
            this.tabSwitchablePolicies.Size = new System.Drawing.Size(819, 474);
            this.tabSwitchablePolicies.TabIndex = 3;
            this.tabSwitchablePolicies.Text = "Switchable Policies";
            this.tabSwitchablePolicies.UseVisualStyleBackColor = true;
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
            // VariablesMergeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 567);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_VarMergeTool.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.MinimizeBox = false;
            this.Name = "VariablesMergeForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Merge";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.VariablesMergeForm_Load);
            this.menAcceptReject.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabVariables;
        private System.Windows.Forms.TabPage tabAcronyms;
        private System.Windows.Forms.TabPage tabCountryLabels;
        private System.Windows.Forms.TabPage tabSwitchablePolicies;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ContextMenuStrip menAcceptReject;
        private System.Windows.Forms.ToolStripMenuItem miAccept;
        private System.Windows.Forms.ToolStripMenuItem miReject;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}