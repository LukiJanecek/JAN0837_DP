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
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnRefreshCrossroadDat = new Button();
            listBox1 = new ListBox();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(820, 581);
            webView21.TabIndex = 1;
            webView21.ZoomFactor = 1D;
            webView21.Click += webView21_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnRefreshCrossroadDat);
            flowLayoutPanel1.Controls.Add(listBox1);
            flowLayoutPanel1.Dock = DockStyle.Right;
            flowLayoutPanel1.Location = new Point(570, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(250, 581);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // btnRefreshCrossroadDat
            // 
            btnRefreshCrossroadDat.Location = new Point(3, 3);
            btnRefreshCrossroadDat.Name = "btnRefreshCrossroadDat";
            btnRefreshCrossroadDat.Size = new Size(146, 29);
            btnRefreshCrossroadDat.TabIndex = 0;
            btnRefreshCrossroadDat.Text = "Refresh variables";
            btnRefreshCrossroadDat.UseVisualStyleBackColor = true;
            btnRefreshCrossroadDat.Click += btnRefreshCrossroadDat_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(3, 38);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(244, 404);
            listBox1.TabIndex = 1;
            // 
            // ucLocalhost
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(webView21);
            Name = "ucLocalhost";
            Size = new Size(820, 581);
            Load += ucLocalhost_Load;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnRefreshCrossroadDat;
        private ListBox listBox1;
    }
}
