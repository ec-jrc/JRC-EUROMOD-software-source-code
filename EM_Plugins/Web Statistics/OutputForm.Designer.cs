namespace Web_Statistics
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.spreadsheetControl = new DevExpress.XtraSpreadsheet.SpreadsheetControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.spreadsheetControlReader = new DevExpress.XtraSpreadsheet.SpreadsheetControl();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 697);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1280, 59);
            this.panel1.TabIndex = 24;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(1113, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(130, 44);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // spreadsheetControl
            // 
            this.spreadsheetControl.Location = new System.Drawing.Point(12, 396);
            this.spreadsheetControl.Name = "spreadsheetControl";
            this.spreadsheetControl.Size = new System.Drawing.Size(364, 268);
            this.spreadsheetControl.TabIndex = 1;
            this.spreadsheetControl.Text = "spreadsheetControl1";
            this.spreadsheetControl.Visible = false;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl1.Location = new System.Drawing.Point(0, 0);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
            this.labelControl1.Size = new System.Drawing.Size(1280, 697);
            this.labelControl1.TabIndex = 26;
            this.labelControl1.Text = "The Web Statistics report has been generated!\r\n\r\nLook under \"My Documents\"...\r\n";
            // 
            // spreadsheetControlReader
            // 
            this.spreadsheetControlReader.Location = new System.Drawing.Point(38, 30);
            this.spreadsheetControlReader.Name = "spreadsheetControlReader";
            this.spreadsheetControlReader.Size = new System.Drawing.Size(364, 268);
            this.spreadsheetControlReader.TabIndex = 27;
            this.spreadsheetControlReader.Text = "spreadsheetControl1";
            this.spreadsheetControlReader.Visible = false;
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 756);
            this.Controls.Add(this.spreadsheetControlReader);
            this.Controls.Add(this.spreadsheetControl);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.panel1);
            this.Name = "OutputForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Country & Year Selection";
            this.Shown += new System.EventHandler(this.CheckCountriesYears_Shown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraSpreadsheet.SpreadsheetControl spreadsheetControl;
        private DevExpress.XtraSpreadsheet.SpreadsheetControl spreadsheetControlReader;
    }
}