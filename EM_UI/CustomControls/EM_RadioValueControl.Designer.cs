namespace EM_UI.CustomControls
{
    partial class EM_RadioValueControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EM_RadioValueControl));
            this.lblHeader = new DevExpress.XtraEditors.LabelControl();
            this.rbDefault = new System.Windows.Forms.RadioButton();
            this.rbCustom = new System.Windows.Forms.RadioButton();
            this.numCustomValue = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numCustomValue)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.Location = new System.Drawing.Point(3, 3);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(115, 13);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Number of parallel runs:";
            // 
            // rbDefault
            // 
            this.rbDefault.AutoSize = true;
            this.rbDefault.BackColor = System.Drawing.Color.Transparent;
            this.rbDefault.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDefault.Location = new System.Drawing.Point(3, 22);
            this.rbDefault.Name = "rbDefault";
            this.rbDefault.Size = new System.Drawing.Size(48, 17);
            this.rbDefault.TabIndex = 1;
            this.rbDefault.TabStop = true;
            this.rbDefault.Text = "Auto";
            this.rbDefault.UseVisualStyleBackColor = false;
            this.rbDefault.CheckedChanged += new System.EventHandler(this.rbDefault_CheckedChanged);
            // 
            // rbCustom
            // 
            this.rbCustom.AutoSize = true;
            this.rbCustom.BackColor = System.Drawing.Color.Transparent;
            this.rbCustom.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbCustom.Location = new System.Drawing.Point(3, 45);
            this.rbCustom.Name = "rbCustom";
            this.rbCustom.Size = new System.Drawing.Size(61, 17);
            this.rbCustom.TabIndex = 2;
            this.rbCustom.TabStop = true;
            this.rbCustom.Text = "Custom";
            this.rbCustom.UseVisualStyleBackColor = false;
            this.rbCustom.CheckedChanged += new System.EventHandler(this.rbCustom_CheckedChanged);
            // 
            // numCustomValue
            // 
            this.numCustomValue.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numCustomValue.Location = new System.Drawing.Point(69, 45);
            this.numCustomValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCustomValue.Name = "numCustomValue";
            this.numCustomValue.Size = new System.Drawing.Size(47, 21);
            this.numCustomValue.TabIndex = 3;
            this.numCustomValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCustomValue.ValueChanged += new System.EventHandler(this.numCustomValue_ValueChanged);
            // 
            // EM_RadioValueControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.numCustomValue);
            this.Controls.Add(this.rbCustom);
            this.Controls.Add(this.rbDefault);
            this.Controls.Add(this.lblHeader);
            this.Name = "EM_RadioValueControl";
            this.Size = new System.Drawing.Size(135, 80);
            ((System.ComponentModel.ISupportInitialize)(this.numCustomValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblHeader;
        private System.Windows.Forms.RadioButton rbDefault;
        private System.Windows.Forms.RadioButton rbCustom;
        private System.Windows.Forms.NumericUpDown numCustomValue;
    }
}
