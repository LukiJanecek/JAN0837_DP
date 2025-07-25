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
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus1 });
            statusStrip1.Location = new Point(0, 275);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(441, 26);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus1
            // 
            lblStatus1.Name = "lblStatus1";
            lblStatus1.Size = new Size(55, 20);
            lblStatus1.Text = "status1";
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(441, 275);
            webView21.TabIndex = 1;
            webView21.ZoomFactor = 1D;
            webView21.Click += webView21_Click;
            webView21.EnsureCoreWebView2Async();
            // 
            // ucLocalhost
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(webView21);
            Controls.Add(statusStrip1);
            Name = "ucLocalhost";
            Size = new Size(441, 301);
            Load += ucLocalhost_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
    }
}
