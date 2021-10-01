namespace EM_UI.Dialogs
{
    partial class ComponentUseForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentUseForm));
            this.chkAllVariables = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnNoSystem = new System.Windows.Forms.Button();
            this.btnAllSystems = new System.Windows.Forms.Button();
            this.lstSystems = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkNonSimulated = new System.Windows.Forms.CheckBox();
            this.chkSimulated = new System.Windows.Forms.CheckBox();
            this.chkNonMonetary = new System.Windows.Forms.CheckBox();
            this.chkMonetary = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkIgnoreIfSwitchedOff = new System.Windows.Forms.CheckBox();
            this.txtComponentName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkQueries = new System.Windows.Forms.CheckBox();
            this.chkAssessmentUnits = new System.Windows.Forms.CheckBox();
            this.chkIncomelists = new System.Windows.Forms.CheckBox();
            this.chkCountrySpecific = new System.Windows.Forms.CheckBox();
            this.lvComponents = new System.Windows.Forms.ListView();
            this.colComponent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsedInPolicy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsedInFunction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsedInParameter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsedInSys = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsedInSystems = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnGoTo = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnExport = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkAllVariables
            // 
            this.chkAllVariables.AutoSize = true;
            this.chkAllVariables.Location = new System.Drawing.Point(6, 19);
            this.chkAllVariables.Name = "chkAllVariables";
            this.chkAllVariables.Size = new System.Drawing.Size(69, 17);
            this.chkAllVariables.TabIndex = 0;
            this.chkAllVariables.Text = "&Variables";
            this.chkAllVariables.UseVisualStyleBackColor = true;
            this.chkAllVariables.CheckedChanged += new System.EventHandler(this.chkAllVariables_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnNoSystem);
            this.groupBox1.Controls.Add(this.btnAllSystems);
            this.groupBox1.Controls.Add(this.lstSystems);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chkNonSimulated);
            this.groupBox1.Controls.Add(this.chkSimulated);
            this.groupBox1.Controls.Add(this.chkNonMonetary);
            this.groupBox1.Controls.Add(this.chkMonetary);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkIgnoreIfSwitchedOff);
            this.groupBox1.Controls.Add(this.txtComponentName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkQueries);
            this.groupBox1.Controls.Add(this.chkAssessmentUnits);
            this.groupBox1.Controls.Add(this.chkIncomelists);
            this.groupBox1.Controls.Add(this.chkCountrySpecific);
            this.groupBox1.Controls.Add(this.chkAllVariables);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(796, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(223, 480);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Included Components";
            // 
            // btnNoSystem
            // 
            this.btnNoSystem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNoSystem.Location = new System.Drawing.Point(41, 458);
            this.btnNoSystem.Margin = new System.Windows.Forms.Padding(2);
            this.btnNoSystem.Name = "btnNoSystem";
            this.btnNoSystem.Size = new System.Drawing.Size(30, 19);
            this.btnNoSystem.TabIndex = 15;
            this.btnNoSystem.Text = "No";
            this.btnNoSystem.UseVisualStyleBackColor = true;
            this.btnNoSystem.Click += new System.EventHandler(this.btnNoSystem_Click);
            // 
            // btnAllSystems
            // 
            this.btnAllSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllSystems.Location = new System.Drawing.Point(8, 458);
            this.btnAllSystems.Margin = new System.Windows.Forms.Padding(2);
            this.btnAllSystems.Name = "btnAllSystems";
            this.btnAllSystems.Size = new System.Drawing.Size(30, 19);
            this.btnAllSystems.TabIndex = 14;
            this.btnAllSystems.Text = "All";
            this.btnAllSystems.UseVisualStyleBackColor = true;
            this.btnAllSystems.Click += new System.EventHandler(this.btnAllSystems_Click);
            // 
            // lstSystems
            // 
            this.lstSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSystems.CheckOnClick = true;
            this.lstSystems.FormattingEnabled = true;
            this.lstSystems.Location = new System.Drawing.Point(8, 287);
            this.lstSystems.MultiColumn = true;
            this.lstSystems.Name = "lstSystems";
            this.lstSystems.Size = new System.Drawing.Size(202, 169);
            this.lstSystems.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 272);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Include Systems";
            // 
            // chkNonSimulated
            // 
            this.chkNonSimulated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNonSimulated.AutoSize = true;
            this.chkNonSimulated.Enabled = false;
            this.chkNonSimulated.Location = new System.Drawing.Point(105, 101);
            this.chkNonSimulated.Margin = new System.Windows.Forms.Padding(2);
            this.chkNonSimulated.Name = "chkNonSimulated";
            this.chkNonSimulated.Size = new System.Drawing.Size(103, 17);
            this.chkNonSimulated.TabIndex = 11;
            this.chkNonSimulated.Text = "... non-simulated";
            this.chkNonSimulated.UseVisualStyleBackColor = true;
            this.chkNonSimulated.CheckedChanged += new System.EventHandler(this.chkNonSimulated_CheckedChanged);
            // 
            // chkSimulated
            // 
            this.chkSimulated.AutoSize = true;
            this.chkSimulated.Enabled = false;
            this.chkSimulated.Location = new System.Drawing.Point(24, 101);
            this.chkSimulated.Margin = new System.Windows.Forms.Padding(2);
            this.chkSimulated.Name = "chkSimulated";
            this.chkSimulated.Size = new System.Drawing.Size(82, 17);
            this.chkSimulated.TabIndex = 10;
            this.chkSimulated.Text = "... simulated";
            this.chkSimulated.UseVisualStyleBackColor = true;
            this.chkSimulated.CheckedChanged += new System.EventHandler(this.chkSimulated_CheckedChanged);
            // 
            // chkNonMonetary
            // 
            this.chkNonMonetary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNonMonetary.AutoSize = true;
            this.chkNonMonetary.Enabled = false;
            this.chkNonMonetary.Location = new System.Drawing.Point(105, 79);
            this.chkNonMonetary.Margin = new System.Windows.Forms.Padding(2);
            this.chkNonMonetary.Name = "chkNonMonetary";
            this.chkNonMonetary.Size = new System.Drawing.Size(102, 17);
            this.chkNonMonetary.TabIndex = 9;
            this.chkNonMonetary.Text = "... non-monetary";
            this.chkNonMonetary.UseVisualStyleBackColor = true;
            this.chkNonMonetary.CheckedChanged += new System.EventHandler(this.chkNonMonetary_CheckedChanged);
            // 
            // chkMonetary
            // 
            this.chkMonetary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMonetary.AutoSize = true;
            this.chkMonetary.Enabled = false;
            this.chkMonetary.Location = new System.Drawing.Point(22, 79);
            this.chkMonetary.Margin = new System.Windows.Forms.Padding(2);
            this.chkMonetary.Name = "chkMonetary";
            this.chkMonetary.Size = new System.Drawing.Size(81, 17);
            this.chkMonetary.TabIndex = 8;
            this.chkMonetary.Text = "... monetary";
            this.chkMonetary.UseVisualStyleBackColor = true;
            this.chkMonetary.CheckedChanged += new System.EventHandler(this.chkMonetary_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "only if ...";
            // 
            // chkIgnoreIfSwitchedOff
            // 
            this.chkIgnoreIfSwitchedOff.AutoSize = true;
            this.chkIgnoreIfSwitchedOff.Checked = true;
            this.chkIgnoreIfSwitchedOff.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIgnoreIfSwitchedOff.Location = new System.Drawing.Point(6, 239);
            this.chkIgnoreIfSwitchedOff.Name = "chkIgnoreIfSwitchedOff";
            this.chkIgnoreIfSwitchedOff.Size = new System.Drawing.Size(124, 17);
            this.chkIgnoreIfSwitchedOff.TabIndex = 6;
            this.chkIgnoreIfSwitchedOff.Text = "&Ignore if switched off";
            this.chkIgnoreIfSwitchedOff.UseVisualStyleBackColor = true;
            // 
            // txtComponentName
            // 
            this.txtComponentName.Location = new System.Drawing.Point(105, 202);
            this.txtComponentName.Name = "txtComponentName";
            this.txtComponentName.Size = new System.Drawing.Size(103, 20);
            this.txtComponentName.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 206);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Component named";
            // 
            // chkQueries
            // 
            this.chkQueries.AutoSize = true;
            this.chkQueries.Location = new System.Drawing.Point(6, 178);
            this.chkQueries.Name = "chkQueries";
            this.chkQueries.Size = new System.Drawing.Size(62, 17);
            this.chkQueries.TabIndex = 4;
            this.chkQueries.Text = "&Queries";
            this.chkQueries.UseVisualStyleBackColor = true;
            // 
            // chkAssessmentUnits
            // 
            this.chkAssessmentUnits.AutoSize = true;
            this.chkAssessmentUnits.Location = new System.Drawing.Point(6, 133);
            this.chkAssessmentUnits.Name = "chkAssessmentUnits";
            this.chkAssessmentUnits.Size = new System.Drawing.Size(109, 17);
            this.chkAssessmentUnits.TabIndex = 2;
            this.chkAssessmentUnits.Text = "&Assessment Units";
            this.chkAssessmentUnits.UseVisualStyleBackColor = true;
            // 
            // chkIncomelists
            // 
            this.chkIncomelists.AutoSize = true;
            this.chkIncomelists.Location = new System.Drawing.Point(6, 156);
            this.chkIncomelists.Name = "chkIncomelists";
            this.chkIncomelists.Size = new System.Drawing.Size(78, 17);
            this.chkIncomelists.TabIndex = 3;
            this.chkIncomelists.Text = "&Incomelists";
            this.chkIncomelists.UseVisualStyleBackColor = true;
            // 
            // chkCountrySpecific
            // 
            this.chkCountrySpecific.AutoSize = true;
            this.chkCountrySpecific.Checked = true;
            this.chkCountrySpecific.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCountrySpecific.Enabled = false;
            this.chkCountrySpecific.Location = new System.Drawing.Point(24, 56);
            this.chkCountrySpecific.Name = "chkCountrySpecific";
            this.chkCountrySpecific.Size = new System.Drawing.Size(201, 17);
            this.chkCountrySpecific.TabIndex = 1;
            this.chkCountrySpecific.Text = "... having &country specific description";
            this.chkCountrySpecific.UseVisualStyleBackColor = true;
            // 
            // lvComponents
            // 
            this.lvComponents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colComponent,
            this.colType,
            this.colUsedInPolicy,
            this.colUsedInFunction,
            this.colUsedInParameter,
            this.colRow,
            this.colUsedInSys});
            this.lvComponents.FullRowSelect = true;
            this.lvComponents.HideSelection = false;
            this.lvComponents.Location = new System.Drawing.Point(12, 12);
            this.lvComponents.MultiSelect = false;
            this.lvComponents.Name = "lvComponents";
            this.lvComponents.Size = new System.Drawing.Size(764, 543);
            this.lvComponents.TabIndex = 3;
            this.lvComponents.UseCompatibleStateImageBehavior = false;
            this.lvComponents.View = System.Windows.Forms.View.Details;
            this.lvComponents.DoubleClick += new System.EventHandler(this.lvComponents_DoubleClick);
            // 
            // colComponent
            // 
            this.colComponent.Text = "Component";
            this.colComponent.Width = 100;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            // 
            // colUsedInPolicy
            // 
            this.colUsedInPolicy.Text = "Used in Policy";
            this.colUsedInPolicy.Width = 100;
            // 
            // colUsedInFunction
            // 
            this.colUsedInFunction.Text = "... Function";
            this.colUsedInFunction.Width = 80;
            // 
            // colUsedInParameter
            // 
            this.colUsedInParameter.Text = "... Parameter";
            this.colUsedInParameter.Width = 80;
            // 
            // colRow
            // 
            this.colRow.Text = "... Row";
            // 
            // colUsedInSys
            // 
            this.colUsedInSys.Text = "... Systems";
            this.colUsedInSys.Width = 250;
            // 
            // colUsedInSystems
            // 
            this.colUsedInSystems.Text = "Used in Systems";
            this.colUsedInSystems.Width = 100;
            // 
            // btnGoTo
            // 
            this.btnGoTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoTo.BackColor = System.Drawing.Color.Transparent;
            this.btnGoTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGoTo.Image = ((System.Drawing.Image)(resources.GetObject("btnGoTo.Image")));
            this.btnGoTo.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnGoTo.Location = new System.Drawing.Point(796, 509);
            this.btnGoTo.Name = "btnGoTo";
            this.btnGoTo.Size = new System.Drawing.Size(43, 46);
            this.btnGoTo.TabIndex = 6;
            this.btnGoTo.Text = "Goto";
            this.btnGoTo.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnGoTo.UseVisualStyleBackColor = false;
            this.btnGoTo.Click += new System.EventHandler(this.btnGoTo_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnCancel.Location = new System.Drawing.Point(976, 509);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(43, 46);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Close";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnRun.Location = new System.Drawing.Point(927, 509);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(43, 46);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "Start";
            this.btnRun.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Image = ((System.Drawing.Image)(resources.GetObject("btnExport.Image")));
            this.btnExport.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnExport.Location = new System.Drawing.Point(844, 509);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(43, 46);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Store";
            this.btnExport.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // ComponentUseForm
            // 
            this.AcceptButton = this.btnRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1031, 567);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnGoTo);
            this.Controls.Add(this.lvComponents);
            this.Controls.Add(this.groupBox1);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ComponentUse.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComponentUseForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Component Use";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ComponentUseForm_FormClosing);
            this.Load += new System.EventHandler(this.ComponentUseForm_Load);
            this.Shown += new System.EventHandler(this.ComponentUseForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAllVariables;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtComponentName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkQueries;
        private System.Windows.Forms.CheckBox chkAssessmentUnits;
        private System.Windows.Forms.CheckBox chkIncomelists;
        private System.Windows.Forms.CheckBox chkCountrySpecific;
        private System.Windows.Forms.ListView lvComponents;
        private System.Windows.Forms.Button btnGoTo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ColumnHeader colComponent;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colUsedInPolicy;
        private System.Windows.Forms.ColumnHeader colUsedInSystems;
        private System.Windows.Forms.ColumnHeader colUsedInFunction;
        private System.Windows.Forms.ColumnHeader colUsedInParameter;
        private System.Windows.Forms.ColumnHeader colUsedInSys;
        private System.Windows.Forms.ColumnHeader colRow;
        private System.Windows.Forms.CheckBox chkIgnoreIfSwitchedOff;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox chkNonSimulated;
        private System.Windows.Forms.CheckBox chkSimulated;
        private System.Windows.Forms.CheckBox chkNonMonetary;
        private System.Windows.Forms.CheckBox chkMonetary;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox lstSystems;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnNoSystem;
        private System.Windows.Forms.Button btnAllSystems;
    }
}