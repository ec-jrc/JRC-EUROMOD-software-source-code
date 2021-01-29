namespace EM_UI.ExtensionAndGroupManagement
{
    partial class SetExtensionSwitchesForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgvSwitches = new System.Windows.Forms.DataGridView();
            this.ctmMultiSelect = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniAllSystemsOn = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsOff = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsNa = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniAllDatasetsOn = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllDatasetsOff = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllDatasetsNa = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniAllOn = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllOff = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllNa = new System.Windows.Forms.ToolStripMenuItem();
            this.labelCountry = new System.Windows.Forms.Label();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.lvSwitchablePolicies = new System.Windows.Forms.ListView();
            this.colLongName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNamePattern = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtSelectedSwitchPolicy = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSwitches)).BeginInit();
            this.ctmMultiSelect.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(557, 487);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(429, 487);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(120, 28);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // dgvSwitches
            // 
            this.dgvSwitches.AllowUserToAddRows = false;
            this.dgvSwitches.AllowUserToDeleteRows = false;
            this.dgvSwitches.AllowUserToResizeRows = false;
            this.dgvSwitches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSwitches.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvSwitches.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvSwitches.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvSwitches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSwitches.ContextMenuStrip = this.ctmMultiSelect;
            this.dgvSwitches.GridColor = System.Drawing.SystemColors.Window;
            this.dgvSwitches.Location = new System.Drawing.Point(429, 59);
            this.dgvSwitches.Margin = new System.Windows.Forms.Padding(4);
            this.dgvSwitches.Name = "dgvSwitches";
            this.dgvSwitches.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgvSwitches.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvSwitches.ShowCellErrors = false;
            this.dgvSwitches.ShowEditingIcon = false;
            this.dgvSwitches.ShowRowErrors = false;
            this.dgvSwitches.Size = new System.Drawing.Size(725, 409);
            this.dgvSwitches.TabIndex = 1;
            this.dgvSwitches.MouseDown += EM_UI.Dialogs.SingleClickForDataGridCombo.HandleDataGridViewMouseDown;
            // 
            // ctmMultiSelect
            // 
            this.ctmMultiSelect.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctmMultiSelect.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniAllSystemsOn,
            this.mniAllSystemsOff,
            this.mniAllSystemsNa,
            this.toolStripSeparator1,
            this.mniAllDatasetsOn,
            this.mniAllDatasetsOff,
            this.mniAllDatasetsNa,
            this.toolStripSeparator2,
            this.mniAllOn,
            this.mniAllOff,
            this.mniAllNa});
            this.ctmMultiSelect.Name = "rowContextMenu";
            this.ctmMultiSelect.ShowImageMargin = false;
            this.ctmMultiSelect.Size = new System.Drawing.Size(310, 232);
            this.ctmMultiSelect.Opening += new System.ComponentModel.CancelEventHandler(this.ctmMultiSelect_Opening);
            // 
            // mniAllSystemsOn
            // 
            this.mniAllSystemsOn.Name = "mniAllSystemsOn";
            this.mniAllSystemsOn.Size = new System.Drawing.Size(309, 24);
            this.mniAllSystemsOn.Text = "Set to ON for all systems";
            this.mniAllSystemsOn.Click += new System.EventHandler(this.mniAllSystemsOn_Click);
            // 
            // mniAllSystemsOff
            // 
            this.mniAllSystemsOff.Name = "mniAllSystemsOff";
            this.mniAllSystemsOff.Size = new System.Drawing.Size(309, 24);
            this.mniAllSystemsOff.Text = "Set to OFF for all systems";
            this.mniAllSystemsOff.Click += new System.EventHandler(this.mniAllSystemsOff_Click);
            // 
            // mniAllSystemsNa
            // 
            this.mniAllSystemsNa.Name = "mniAllSystemsNa";
            this.mniAllSystemsNa.Size = new System.Drawing.Size(309, 24);
            this.mniAllSystemsNa.Text = "Set to N/A for all systems";
            this.mniAllSystemsNa.Click += new System.EventHandler(this.mniAllSystemsNa_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(306, 6);
            // 
            // mniAllDatasetsOn
            // 
            this.mniAllDatasetsOn.Name = "mniAllDatasetsOn";
            this.mniAllDatasetsOn.Size = new System.Drawing.Size(309, 24);
            this.mniAllDatasetsOn.Text = "Set to ON for all datasets";
            this.mniAllDatasetsOn.Click += new System.EventHandler(this.mniAllDatasetsOn_Click);
            // 
            // mniAllDatasetsOff
            // 
            this.mniAllDatasetsOff.Name = "mniAllDatasetsOff";
            this.mniAllDatasetsOff.Size = new System.Drawing.Size(309, 24);
            this.mniAllDatasetsOff.Text = "Set to OFF for all datasets";
            this.mniAllDatasetsOff.Click += new System.EventHandler(this.mniAllDatasetsOff_Click);
            // 
            // mniAllDatasetsNa
            // 
            this.mniAllDatasetsNa.Name = "mniAllDatasetsNa";
            this.mniAllDatasetsNa.Size = new System.Drawing.Size(309, 24);
            this.mniAllDatasetsNa.Text = "Set to N/A for all datasets";
            this.mniAllDatasetsNa.Click += new System.EventHandler(this.mniAllDatasetsNa_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(306, 6);
            // 
            // mniAllOn
            // 
            this.mniAllOn.Name = "mniAllOn";
            this.mniAllOn.Size = new System.Drawing.Size(309, 24);
            this.mniAllOn.Text = "Set to ON for all systems and datasets";
            this.mniAllOn.Click += new System.EventHandler(this.mniAllOn_Click);
            // 
            // mniAllOff
            // 
            this.mniAllOff.Name = "mniAllOff";
            this.mniAllOff.Size = new System.Drawing.Size(309, 24);
            this.mniAllOff.Text = "Set to OFF for all systems and datasets";
            this.mniAllOff.Click += new System.EventHandler(this.mniAllOff_Click);
            // 
            // mniAllNa
            // 
            this.mniAllNa.Name = "mniAllNa";
            this.mniAllNa.Size = new System.Drawing.Size(309, 24);
            this.mniAllNa.Text = "Set to N/A for all systems and datasets";
            this.mniAllNa.Click += new System.EventHandler(this.mniAllNa_Click);
            // 
            // labelCountry
            // 
            this.labelCountry.AutoSize = true;
            this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCountry.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.labelCountry.Location = new System.Drawing.Point(16, 6);
            this.labelCountry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCountry.Name = "labelCountry";
            this.labelCountry.Size = new System.Drawing.Size(88, 25);
            this.labelCountry.TabIndex = 4;
            this.labelCountry.Text = "Country";
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // lvSwitchablePolicies
            // 
            this.lvSwitchablePolicies.CheckBoxes = true;
            this.lvSwitchablePolicies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colLongName,
            this.colNamePattern});
            this.lvSwitchablePolicies.Location = new System.Drawing.Point(21, 33);
            this.lvSwitchablePolicies.Margin = new System.Windows.Forms.Padding(4);
            this.lvSwitchablePolicies.MultiSelect = false;
            this.lvSwitchablePolicies.Name = "lvSwitchablePolicies";
            this.lvSwitchablePolicies.Size = new System.Drawing.Size(399, 434);
            this.lvSwitchablePolicies.TabIndex = 5;
            this.lvSwitchablePolicies.UseCompatibleStateImageBehavior = false;
            this.lvSwitchablePolicies.View = System.Windows.Forms.View.Details;
            this.lvSwitchablePolicies.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvSwitchablePolicies_ItemCheck);
            this.lvSwitchablePolicies.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvSwitchablePolicies_ItemChecked);
            this.lvSwitchablePolicies.SelectedIndexChanged += new System.EventHandler(this.lvSwitchablePolicies_SelectedIndexChanged);
            // 
            // colLongName
            // 
            this.colLongName.Text = "Long Name";
            this.colLongName.Width = 265;
            // 
            // colNamePattern
            // 
            this.colNamePattern.Text = "Short Name";
            this.colNamePattern.Width = 125;
            // 
            // txtSelectedSwitchPolicy
            // 
            this.txtSelectedSwitchPolicy.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.txtSelectedSwitchPolicy.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSelectedSwitchPolicy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSelectedSwitchPolicy.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.txtSelectedSwitchPolicy.Location = new System.Drawing.Point(429, 33);
            this.txtSelectedSwitchPolicy.Margin = new System.Windows.Forms.Padding(4);
            this.txtSelectedSwitchPolicy.Name = "txtSelectedSwitchPolicy";
            this.txtSelectedSwitchPolicy.ReadOnly = true;
            this.txtSelectedSwitchPolicy.Size = new System.Drawing.Size(797, 19);
            this.txtSelectedSwitchPolicy.TabIndex = 6;
            // 
            // SetPolicySwitchesForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1173, 530);
            this.ContextMenuStrip = this.ctmMultiSelect;
            this.Controls.Add(this.txtSelectedSwitchPolicy);
            this.Controls.Add(this.lvSwitchablePolicies);
            this.Controls.Add(this.labelCountry);
            this.Controls.Add(this.dgvSwitches);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_AdministratingPolicySwitches.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1189, 525);
            this.Name = "SetPolicySwitchesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Set Policy Switches";
            this.Load += new System.EventHandler(this.SetPolicySwitchesForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSwitches)).EndInit();
            this.ctmMultiSelect.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgvSwitches;
        private System.Windows.Forms.ContextMenuStrip ctmMultiSelect;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsOn;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsOff;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsNa;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mniAllDatasetsOn;
        private System.Windows.Forms.ToolStripMenuItem mniAllDatasetsOff;
        private System.Windows.Forms.ToolStripMenuItem mniAllDatasetsNa;
        private System.Windows.Forms.Label labelCountry;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.ListView lvSwitchablePolicies;
        private System.Windows.Forms.ColumnHeader colLongName;
        private System.Windows.Forms.ColumnHeader colNamePattern;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mniAllOn;
        private System.Windows.Forms.ToolStripMenuItem mniAllOff;
        private System.Windows.Forms.ToolStripMenuItem mniAllNa;
        private System.Windows.Forms.TextBox txtSelectedSwitchPolicy;
    }
}