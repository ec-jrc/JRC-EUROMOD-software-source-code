namespace VariablesChecker
{
    partial class VariablesCheckerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VariablesCheckerForm));
            this.btnVarUsage = new System.Windows.Forms.Button();
            this.btnUnusedAcros = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.btnVarDuplicates = new System.Windows.Forms.Button();
            this.btnVarNaming = new System.Windows.Forms.Button();
            this.btnVarAcroTwins = new System.Windows.Forms.Button();
            this.btnDescFormatting = new System.Windows.Forms.Button();
            this.btnVarDataCheck = new System.Windows.Forms.Button();
            this.btnAcroDuplicates = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnCancelParallel = new System.Windows.Forms.Button();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // btnVarUsage
            // 
            this.btnVarUsage.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnVarUsage.Location = new System.Drawing.Point(13, 13);
            this.btnVarUsage.Margin = new System.Windows.Forms.Padding(4);
            this.btnVarUsage.Name = "btnVarUsage";
            this.btnVarUsage.Size = new System.Drawing.Size(117, 72);
            this.btnVarUsage.TabIndex = 0;
            this.btnVarUsage.Text = "Variable Usage";
            this.toolTip.SetToolTip(this.btnVarUsage, "Creates a list of variables that are used in each country and informs about missi" +
        "ng country specific description");
            this.btnVarUsage.UseVisualStyleBackColor = true;
            this.btnVarUsage.Click += new System.EventHandler(this.btnVarUsage_Click);
            // 
            // btnUnusedAcros
            // 
            this.btnUnusedAcros.ForeColor = System.Drawing.Color.Orange;
            this.btnUnusedAcros.Location = new System.Drawing.Point(138, 93);
            this.btnUnusedAcros.Margin = new System.Windows.Forms.Padding(4);
            this.btnUnusedAcros.Name = "btnUnusedAcros";
            this.btnUnusedAcros.Size = new System.Drawing.Size(116, 72);
            this.btnUnusedAcros.TabIndex = 4;
            this.btnUnusedAcros.Text = "Unused Acros";
            this.toolTip.SetToolTip(this.btnUnusedAcros, "Creates a list of unused acronyms");
            this.btnUnusedAcros.UseVisualStyleBackColor = true;
            this.btnUnusedAcros.Click += new System.EventHandler(this.btnUnusedAcros_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // btnVarDuplicates
            // 
            this.btnVarDuplicates.ForeColor = System.Drawing.Color.LimeGreen;
            this.btnVarDuplicates.Location = new System.Drawing.Point(138, 13);
            this.btnVarDuplicates.Margin = new System.Windows.Forms.Padding(4);
            this.btnVarDuplicates.Name = "btnVarDuplicates";
            this.btnVarDuplicates.Size = new System.Drawing.Size(116, 72);
            this.btnVarDuplicates.TabIndex = 1;
            this.btnVarDuplicates.Text = "Duplicate Variables";
            this.toolTip.SetToolTip(this.btnVarDuplicates, "Creates a list of duplicate variables");
            this.btnVarDuplicates.UseVisualStyleBackColor = true;
            this.btnVarDuplicates.Click += new System.EventHandler(this.btnVarDuplicates_Click);
            // 
            // btnVarNaming
            // 
            this.btnVarNaming.ForeColor = System.Drawing.Color.DeepPink;
            this.btnVarNaming.Location = new System.Drawing.Point(14, 93);
            this.btnVarNaming.Margin = new System.Windows.Forms.Padding(4);
            this.btnVarNaming.Name = "btnVarNaming";
            this.btnVarNaming.Size = new System.Drawing.Size(116, 72);
            this.btnVarNaming.TabIndex = 3;
            this.btnVarNaming.Text = "Variable Naming";
            this.toolTip.SetToolTip(this.btnVarNaming, "Creates a list of variables that do not obey the naming rules");
            this.btnVarNaming.UseVisualStyleBackColor = true;
            this.btnVarNaming.Click += new System.EventHandler(this.btnVarNaming_Click);
            // 
            // btnVarAcroTwins
            // 
            this.btnVarAcroTwins.ForeColor = System.Drawing.Color.Tomato;
            this.btnVarAcroTwins.Location = new System.Drawing.Point(262, 13);
            this.btnVarAcroTwins.Margin = new System.Windows.Forms.Padding(4);
            this.btnVarAcroTwins.Name = "btnVarAcroTwins";
            this.btnVarAcroTwins.Size = new System.Drawing.Size(116, 72);
            this.btnVarAcroTwins.TabIndex = 2;
            this.btnVarAcroTwins.Text = "Variable Acro-Twins";
            this.toolTip.SetToolTip(this.btnVarAcroTwins, "Creates a list of variables that use the exact same acronyms, but in different or" +
        "der");
            this.btnVarAcroTwins.UseVisualStyleBackColor = true;
            this.btnVarAcroTwins.Click += new System.EventHandler(this.btnVarAcroTwins_Click);
            // 
            // btnDescFormatting
            // 
            this.btnDescFormatting.ForeColor = System.Drawing.Color.RoyalBlue;
            this.btnDescFormatting.Location = new System.Drawing.Point(262, 93);
            this.btnDescFormatting.Margin = new System.Windows.Forms.Padding(4);
            this.btnDescFormatting.Name = "btnDescFormatting";
            this.btnDescFormatting.Size = new System.Drawing.Size(116, 72);
            this.btnDescFormatting.TabIndex = 6;
            this.btnDescFormatting.Text = "Description Formatting";
            this.toolTip.SetToolTip(this.btnDescFormatting, resources.GetString("btnDescFormatting.ToolTip"));
            this.btnDescFormatting.UseVisualStyleBackColor = true;
            this.btnDescFormatting.Click += new System.EventHandler(this.btnDescFormatting_Click);
            // 
            // btnVarDataCheck
            // 
            this.btnVarDataCheck.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnVarDataCheck.Location = new System.Drawing.Point(13, 173);
            this.btnVarDataCheck.Margin = new System.Windows.Forms.Padding(4);
            this.btnVarDataCheck.Name = "btnVarDataCheck";
            this.btnVarDataCheck.Size = new System.Drawing.Size(116, 72);
            this.btnVarDataCheck.TabIndex = 10;
            this.btnVarDataCheck.Text = "Variable Data Check";
            this.toolTip.SetToolTip(this.btnVarDataCheck, "Creates a list of variables that are used in each country and informs about missi" +
        "ng country specific description");
            this.btnVarDataCheck.UseVisualStyleBackColor = true;
            this.btnVarDataCheck.Click += new System.EventHandler(this.btnVarDataCheck_Click);
            // 
            // btnAcroDuplicates
            // 
            this.btnAcroDuplicates.ForeColor = System.Drawing.Color.OliveDrab;
            this.btnAcroDuplicates.Location = new System.Drawing.Point(138, 173);
            this.btnAcroDuplicates.Margin = new System.Windows.Forms.Padding(4);
            this.btnAcroDuplicates.Name = "btnAcroDuplicates";
            this.btnAcroDuplicates.Size = new System.Drawing.Size(116, 72);
            this.btnAcroDuplicates.TabIndex = 11;
            this.btnAcroDuplicates.Text = "Duplicate Acros";
            this.toolTip.SetToolTip(this.btnAcroDuplicates, "Creates a list of duplicate acronyms within one level (e.g. Income)");
            this.btnAcroDuplicates.UseVisualStyleBackColor = true;
            this.btnAcroDuplicates.Click += new System.EventHandler(this.btnAcroDuplicates_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.MidnightBlue;
            this.btnClose.Image = global::VariablesChecker.Properties.Resources.Turn_off;
            this.btnClose.Location = new System.Drawing.Point(386, 93);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(116, 72);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(386, 41);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(90, 26);
            this.progressBar.Step = 1;
            this.progressBar.TabIndex = 8;
            this.progressBar.Value = 1;
            this.progressBar.Visible = false;
            // 
            // btnCancelParallel
            // 
            this.btnCancelParallel.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnCancelParallel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelParallel.Image")));
            this.btnCancelParallel.Location = new System.Drawing.Point(476, 41);
            this.btnCancelParallel.Name = "btnCancelParallel";
            this.btnCancelParallel.Size = new System.Drawing.Size(26, 26);
            this.btnCancelParallel.TabIndex = 9;
            this.btnCancelParallel.UseVisualStyleBackColor = true;
            this.btnCancelParallel.Visible = false;
            this.btnCancelParallel.Click += new System.EventHandler(this.btnCancelParallel_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // VariablesCheckerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(513, 311);
            this.Controls.Add(this.btnAcroDuplicates);
            this.Controls.Add(this.btnVarDataCheck);
            this.Controls.Add(this.btnCancelParallel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDescFormatting);
            this.Controls.Add(this.btnVarAcroTwins);
            this.Controls.Add(this.btnVarNaming);
            this.Controls.Add(this.btnVarDuplicates);
            this.Controls.Add(this.btnUnusedAcros);
            this.Controls.Add(this.btnVarUsage);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VariablesCheckerForm";
            this.Text = "Variables Checker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnVarUsage;
        private System.Windows.Forms.Button btnUnusedAcros;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button btnVarDuplicates;
        private System.Windows.Forms.Button btnVarNaming;
        private System.Windows.Forms.Button btnVarAcroTwins;
        private System.Windows.Forms.Button btnDescFormatting;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnCancelParallel;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.Button btnVarDataCheck;
        private System.Windows.Forms.Button btnAcroDuplicates;
    }
}