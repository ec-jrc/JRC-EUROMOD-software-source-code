
namespace EM_UI.Dialogs.Tools
{
    partial class ChooseStatistics
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
            this.btnStatisticsPresenter = new System.Windows.Forms.Button();
            this.btnInDepth = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStatisticsPresenter
            // 
            this.btnStatisticsPresenter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStatisticsPresenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStatisticsPresenter.Image = global::EM_UI.Properties.Resources.StatisticsPresenter;
            this.btnStatisticsPresenter.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnStatisticsPresenter.Location = new System.Drawing.Point(95, 76);
            this.btnStatisticsPresenter.Name = "btnStatisticsPresenter";
            this.btnStatisticsPresenter.Size = new System.Drawing.Size(163, 152);
            this.btnStatisticsPresenter.TabIndex = 0;
            this.btnStatisticsPresenter.Text = "Statistics Presenter";
            this.btnStatisticsPresenter.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnStatisticsPresenter.UseVisualStyleBackColor = true;
            this.btnStatisticsPresenter.Click += new System.EventHandler(this.btnStatisticsPresenter_Click);
            // 
            // btnInDepth
            // 
            this.btnInDepth.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnInDepth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInDepth.Image = global::EM_UI.Properties.Resources.InDepthAnalysis;
            this.btnInDepth.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnInDepth.Location = new System.Drawing.Point(341, 76);
            this.btnInDepth.Name = "btnInDepth";
            this.btnInDepth.Size = new System.Drawing.Size(163, 152);
            this.btnInDepth.TabIndex = 1;
            this.btnInDepth.Text = "In-depth Analysis";
            this.btnInDepth.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnInDepth.UseVisualStyleBackColor = true;
            this.btnInDepth.Click += new System.EventHandler(this.btnInDepth_Click);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(453, 316);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 27);
            this.button1.TabIndex = 2;
            this.button1.TabStop = false;
            this.button1.Text = "cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ChooseStatistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(592, 290);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnInDepth);
            this.Controls.Add(this.btnStatisticsPresenter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseStatistics";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Please choose a tool";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStatisticsPresenter;
        private System.Windows.Forms.Button btnInDepth;
        private System.Windows.Forms.Button button1;
    }
}