namespace HypotheticalHousehold
{
    partial class OutputForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputForm));
            this.labelMainMessage = new DevExpress.XtraEditors.LabelControl();
            this.buttonCancel = new DevExpress.XtraEditors.SimpleButton();
            this.buttonGenerate = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonChangePath = new DevExpress.XtraEditors.SimpleButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelPeople = new DevExpress.XtraEditors.LabelControl();
            this.labelHouseholds = new DevExpress.XtraEditors.LabelControl();
            this.labelFiles = new DevExpress.XtraEditors.LabelControl();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMainMessage
            // 
            this.labelMainMessage.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelMainMessage.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelMainMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelMainMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelMainMessage.Location = new System.Drawing.Point(9, 12);
            this.labelMainMessage.Name = "labelMainMessage";
            this.labelMainMessage.Size = new System.Drawing.Size(432, 100);
            this.labelMainMessage.TabIndex = 4;
            this.labelMainMessage.Text = "No countries selected!";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.buttonCancel.Appearance.Options.UseFont = true;
            this.buttonCancel.Location = new System.Drawing.Point(356, 179);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 12, 120, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(93, 36);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.buttonGenerate.Appearance.Options.UseFont = true;
            this.buttonGenerate.Location = new System.Drawing.Point(179, 179);
            this.buttonGenerate.Margin = new System.Windows.Forms.Padding(2, 12, 120, 2);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(93, 36);
            this.buttonGenerate.TabIndex = 0;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelControl1.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Location = new System.Drawing.Point(9, 129);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(318, 18);
            this.labelControl1.TabIndex = 9;
            this.labelControl1.Text = "Output Folder: ";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(9, 144);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(410, 20);
            this.textBoxOutput.TabIndex = 1;
            // 
            // buttonChangePath
            // 
            this.buttonChangePath.Image = ((System.Drawing.Image)(resources.GetObject("buttonChangePath.Image")));
            this.buttonChangePath.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.buttonChangePath.Location = new System.Drawing.Point(425, 140);
            this.buttonChangePath.Name = "buttonChangePath";
            this.buttonChangePath.Size = new System.Drawing.Size(27, 27);
            this.buttonChangePath.TabIndex = 2;
            this.buttonChangePath.Click += new System.EventHandler(this.buttonChangePath_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonChangePath);
            this.panel1.Controls.Add(this.textBoxOutput);
            this.panel1.Controls.Add(this.labelControl1);
            this.panel1.Controls.Add(this.labelMainMessage);
            this.panel1.Location = new System.Drawing.Point(3, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(464, 179);
            this.panel1.TabIndex = 27;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labelControl2);
            this.panel2.Controls.Add(this.labelPeople);
            this.panel2.Controls.Add(this.labelHouseholds);
            this.panel2.Controls.Add(this.labelFiles);
            this.panel2.Controls.Add(this.progressBar1);
            this.panel2.Location = new System.Drawing.Point(12, 203);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(461, 176);
            this.panel2.TabIndex = 29;
            this.panel2.Visible = false;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelControl2.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelControl2.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelControl2.Location = new System.Drawing.Point(31, 16);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(103, 16);
            this.labelControl2.TabIndex = 9;
            this.labelControl2.Text = "Generating data...";
            // 
            // labelPeople
            // 
            this.labelPeople.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelPeople.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelPeople.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelPeople.Location = new System.Drawing.Point(131, 93);
            this.labelPeople.Name = "labelPeople";
            this.labelPeople.Size = new System.Drawing.Size(26, 16);
            this.labelPeople.TabIndex = 8;
            this.labelPeople.Text = "Files ";
            // 
            // labelHouseholds
            // 
            this.labelHouseholds.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelHouseholds.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelHouseholds.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelHouseholds.Location = new System.Drawing.Point(131, 71);
            this.labelHouseholds.Name = "labelHouseholds";
            this.labelHouseholds.Size = new System.Drawing.Size(26, 16);
            this.labelHouseholds.TabIndex = 6;
            this.labelHouseholds.Text = "Files ";
            // 
            // labelFiles
            // 
            this.labelFiles.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelFiles.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelFiles.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelFiles.Location = new System.Drawing.Point(131, 49);
            this.labelFiles.Name = "labelFiles";
            this.labelFiles.Size = new System.Drawing.Size(26, 16);
            this.labelFiles.TabIndex = 5;
            this.labelFiles.Text = "Files ";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(31, 123);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(388, 16);
            this.progressBar1.TabIndex = 0;
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 229);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OutputForm";
            this.Text = "Hypothetical Data Generation";
            this.Shown += new System.EventHandler(this.OutputForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelMainMessage;
        private DevExpress.XtraEditors.SimpleButton buttonCancel;
        private DevExpress.XtraEditors.SimpleButton buttonGenerate;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.TextBox textBoxOutput;
        private DevExpress.XtraEditors.SimpleButton buttonChangePath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelPeople;
        private DevExpress.XtraEditors.LabelControl labelHouseholds;
        private DevExpress.XtraEditors.LabelControl labelFiles;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}