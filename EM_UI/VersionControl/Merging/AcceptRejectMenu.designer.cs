namespace EM_UI.VersionControl.Merging
{
    partial class AcceptRejectMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AcceptRejectMenu));
            this.labReference = new System.Windows.Forms.Label();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.chkVisible = new System.Windows.Forms.CheckBox();
            this.btnExpand = new System.Windows.Forms.Button();
            this.btnCollapse = new System.Windows.Forms.Button();
            this.chkIncludeSubNodes = new System.Windows.Forms.CheckBox();
            this.labSeparator = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labReference
            // 
            this.labReference.AutoSize = true;
            this.labReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labReference.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.labReference.Location = new System.Drawing.Point(5, 7);
            this.labReference.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labReference.Name = "labReference";
            this.labReference.Size = new System.Drawing.Size(167, 17);
            this.labReference.TabIndex = 0;
            this.labReference.Text = "-------Reference-------";
            // 
            // btnAccept
            // 
            this.btnAccept.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAccept.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnAccept.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAccept.Location = new System.Drawing.Point(9, 26);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(2);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(142, 28);
            this.btnAccept.TabIndex = 1;
            this.btnAccept.Text = "Accept Changes";
            this.btnAccept.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            this.btnAccept.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AcceptRejectMenu_KeyUp);
            // 
            // btnReject
            // 
            this.btnReject.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReject.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnReject.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReject.Location = new System.Drawing.Point(9, 52);
            this.btnReject.Margin = new System.Windows.Forms.Padding(2);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(141, 28);
            this.btnReject.TabIndex = 2;
            this.btnReject.Text = "Reject Changes";
            this.btnReject.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReject.UseVisualStyleBackColor = true;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);
            this.btnReject.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AcceptRejectMenu_KeyUp);
            // 
            // chkVisible
            // 
            this.chkVisible.AutoSize = true;
            this.chkVisible.Checked = true;
            this.chkVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVisible.Location = new System.Drawing.Point(14, 84);
            this.chkVisible.Margin = new System.Windows.Forms.Padding(2);
            this.chkVisible.Name = "chkVisible";
            this.chkVisible.Size = new System.Drawing.Size(80, 17);
            this.chkVisible.TabIndex = 3;
            this.chkVisible.Text = "Visible Only";
            this.chkVisible.UseVisualStyleBackColor = true;
            this.chkVisible.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AcceptRejectMenu_KeyUp);
            // 
            // btnExpand
            // 
            this.btnExpand.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnExpand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpand.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExpand.Image = ((System.Drawing.Image)(resources.GetObject("btnExpand.Image")));
            this.btnExpand.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExpand.Location = new System.Drawing.Point(9, 119);
            this.btnExpand.Margin = new System.Windows.Forms.Padding(2);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(85, 28);
            this.btnExpand.TabIndex = 4;
            this.btnExpand.Text = "Expand";
            this.btnExpand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.btnExpand_Click);
            // 
            // btnCollapse
            // 
            this.btnCollapse.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnCollapse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCollapse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCollapse.Image = ((System.Drawing.Image)(resources.GetObject("btnCollapse.Image")));
            this.btnCollapse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCollapse.Location = new System.Drawing.Point(9, 145);
            this.btnCollapse.Margin = new System.Windows.Forms.Padding(2);
            this.btnCollapse.Name = "btnCollapse";
            this.btnCollapse.Size = new System.Drawing.Size(93, 28);
            this.btnCollapse.TabIndex = 5;
            this.btnCollapse.Text = "Collapse";
            this.btnCollapse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCollapse.UseVisualStyleBackColor = true;
            this.btnCollapse.Click += new System.EventHandler(this.btnCollapse_Click);
            // 
            // chkIncludeSubNodes
            // 
            this.chkIncludeSubNodes.AutoSize = true;
            this.chkIncludeSubNodes.Checked = true;
            this.chkIncludeSubNodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSubNodes.Location = new System.Drawing.Point(14, 176);
            this.chkIncludeSubNodes.Margin = new System.Windows.Forms.Padding(2);
            this.chkIncludeSubNodes.Name = "chkIncludeSubNodes";
            this.chkIncludeSubNodes.Size = new System.Drawing.Size(117, 17);
            this.chkIncludeSubNodes.TabIndex = 6;
            this.chkIncludeSubNodes.Text = "Include Sub-Nodes";
            this.chkIncludeSubNodes.UseVisualStyleBackColor = true;
            // 
            // labSeparator
            // 
            this.labSeparator.AutoSize = true;
            this.labSeparator.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labSeparator.ForeColor = System.Drawing.Color.Black;
            this.labSeparator.Location = new System.Drawing.Point(4, 102);
            this.labSeparator.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labSeparator.Name = "labSeparator";
            this.labSeparator.Size = new System.Drawing.Size(170, 17);
            this.labSeparator.TabIndex = 7;
            this.labSeparator.Text = "---------------------------";
            // 
            // AcceptRejectMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(153, 204);
            this.ControlBox = false;
            this.Controls.Add(this.labSeparator);
            this.Controls.Add(this.chkIncludeSubNodes);
            this.Controls.Add(this.btnCollapse);
            this.Controls.Add(this.btnExpand);
            this.Controls.Add(this.chkVisible);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.labReference);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AcceptRejectMenu";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "AcceptRejectMenu";
            this.Deactivate += new System.EventHandler(this.AcceptRejectMenu_Deactivate);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AcceptRejectMenu_KeyUp);
            this.Leave += new System.EventHandler(this.AcceptRejectMenu_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labReference;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnReject;
        private System.Windows.Forms.CheckBox chkVisible;
        private System.Windows.Forms.Button btnExpand;
        private System.Windows.Forms.Button btnCollapse;
        private System.Windows.Forms.CheckBox chkIncludeSubNodes;
        private System.Windows.Forms.Label labSeparator;
    }
}