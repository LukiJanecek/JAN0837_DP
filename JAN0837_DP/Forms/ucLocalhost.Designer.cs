namespace JAN0837_DP.Forms
{
    partial class ucLocalhost
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
            statusStrip1 = new StatusStrip();
            lblStatus1 = new ToolStripStatusLabel();
            lblLocalhost = new Label();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStrip1.Location = new Point(0, 180);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(260, 26);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus1
            // 
            lblStatus1.Name = "lblStatus1";
            lblStatus1.Size = new Size(55, 20);
            lblStatus1.Text = "status1";
            // 
            // lblLocalhost
            // 
            lblLocalhost.AutoSize = true;
            lblLocalhost.Location = new Point(3, 0);
            lblLocalhost.Name = "lblLocalhost";
            lblLocalhost.Size = new Size(50, 20);
            lblLocalhost.TabIndex = 1;
            lblLocalhost.Text = "label1";
            // 
            // ucLocalhost
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblLocalhost);
            Controls.Add(statusStrip1);
            Name = "ucLocalhost";
            Size = new Size(260, 206);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private Label lblLocalhost;
        private ToolStripStatusLabel lblStatus1;
    }
}
