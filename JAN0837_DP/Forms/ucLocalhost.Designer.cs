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
            panel1 = new Panel();
            btnStopFE = new Button();
            btnOpenDevTool = new Button();
            lblData = new Label();
            btnGetData = new Button();
            btnShowPage = new Button();
            btnSendDataToFe = new Button();
            lblCommunicationStatus = new Label();
            txtBoxParam3 = new TextBox();
            txtBoxParam2 = new TextBox();
            lblParam3 = new Label();
            lblParam2 = new Label();
            txtBoxParam1 = new TextBox();
            lblParam1 = new Label();
            btnStartFE = new Button();
            btnShowData = new Button();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            panel1.SuspendLayout();
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
            // panel1
            // 
            panel1.Controls.Add(btnShowData);
            panel1.Controls.Add(btnStopFE);
            panel1.Controls.Add(btnOpenDevTool);
            panel1.Controls.Add(lblData);
            panel1.Controls.Add(btnGetData);
            panel1.Controls.Add(btnShowPage);
            panel1.Controls.Add(btnSendDataToFe);
            panel1.Controls.Add(lblCommunicationStatus);
            panel1.Controls.Add(txtBoxParam3);
            panel1.Controls.Add(txtBoxParam2);
            panel1.Controls.Add(lblParam3);
            panel1.Controls.Add(lblParam2);
            panel1.Controls.Add(txtBoxParam1);
            panel1.Controls.Add(lblParam1);
            panel1.Controls.Add(btnStartFE);
            panel1.Dock = DockStyle.Right;
            panel1.Location = new Point(510, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(310, 581);
            panel1.TabIndex = 2;
            panel1.Paint += panel1_Paint;
            // 
            // btnStopFE
            // 
            btnStopFE.Location = new Point(171, 198);
            btnStopFE.Name = "btnStopFE";
            btnStopFE.Size = new Size(125, 56);
            btnStopFE.TabIndex = 14;
            btnStopFE.Text = "Stop FE";
            btnStopFE.UseVisualStyleBackColor = true;
            btnStopFE.Click += btnStopFE_Click;
            // 
            // btnOpenDevTool
            // 
            btnOpenDevTool.Location = new Point(171, 325);
            btnOpenDevTool.Name = "btnOpenDevTool";
            btnOpenDevTool.Size = new Size(125, 59);
            btnOpenDevTool.TabIndex = 13;
            btnOpenDevTool.Text = "Open Dev Tool";
            btnOpenDevTool.UseVisualStyleBackColor = true;
            btnOpenDevTool.Click += btnOpenDevTool_Click;
            // 
            // lblData
            // 
            lblData.AutoSize = true;
            lblData.Location = new Point(171, 390);
            lblData.Name = "lblData";
            lblData.Size = new Size(41, 20);
            lblData.TabIndex = 12;
            lblData.Text = "Data";
            // 
            // btnGetData
            // 
            btnGetData.Location = new Point(171, 260);
            btnGetData.Name = "btnGetData";
            btnGetData.Size = new Size(125, 59);
            btnGetData.TabIndex = 11;
            btnGetData.Text = "Get data";
            btnGetData.UseVisualStyleBackColor = true;
            btnGetData.Click += btnGetData_Click;
            // 
            // btnShowPage
            // 
            btnShowPage.Location = new Point(40, 325);
            btnShowPage.Name = "btnShowPage";
            btnShowPage.Size = new Size(125, 59);
            btnShowPage.TabIndex = 10;
            btnShowPage.Text = "Show page";
            btnShowPage.UseVisualStyleBackColor = true;
            btnShowPage.Click += btnShowPage_Click;
            // 
            // btnSendDataToFe
            // 
            btnSendDataToFe.Location = new Point(40, 260);
            btnSendDataToFe.Name = "btnSendDataToFe";
            btnSendDataToFe.Size = new Size(125, 59);
            btnSendDataToFe.TabIndex = 9;
            btnSendDataToFe.Text = "Send data";
            btnSendDataToFe.UseVisualStyleBackColor = true;
            btnSendDataToFe.Click += btnSendDataToFe_Click;
            // 
            // lblCommunicationStatus
            // 
            lblCommunicationStatus.AutoSize = true;
            lblCommunicationStatus.Location = new Point(18, 545);
            lblCommunicationStatus.Name = "lblCommunicationStatus";
            lblCommunicationStatus.Size = new Size(160, 20);
            lblCommunicationStatus.TabIndex = 8;
            lblCommunicationStatus.Text = "Communication status ";
            // 
            // txtBoxParam3
            // 
            txtBoxParam3.Location = new Point(40, 165);
            txtBoxParam3.Name = "txtBoxParam3";
            txtBoxParam3.Size = new Size(125, 27);
            txtBoxParam3.TabIndex = 7;
            txtBoxParam3.TextChanged += txtBoxParam3_TextChanged;
            // 
            // txtBoxParam2
            // 
            txtBoxParam2.Location = new Point(40, 112);
            txtBoxParam2.Name = "txtBoxParam2";
            txtBoxParam2.Size = new Size(125, 27);
            txtBoxParam2.TabIndex = 6;
            txtBoxParam2.TextChanged += txtBoxParam2_TextChanged;
            // 
            // lblParam3
            // 
            lblParam3.AutoSize = true;
            lblParam3.Location = new Point(40, 142);
            lblParam3.Name = "lblParam3";
            lblParam3.Size = new Size(87, 20);
            lblParam3.TabIndex = 5;
            lblParam3.Text = "Parameter3:";
            // 
            // lblParam2
            // 
            lblParam2.AutoSize = true;
            lblParam2.Location = new Point(40, 89);
            lblParam2.Name = "lblParam2";
            lblParam2.Size = new Size(87, 20);
            lblParam2.TabIndex = 4;
            lblParam2.Text = "Parameter2:";
            // 
            // txtBoxParam1
            // 
            txtBoxParam1.Location = new Point(40, 59);
            txtBoxParam1.Name = "txtBoxParam1";
            txtBoxParam1.Size = new Size(125, 27);
            txtBoxParam1.TabIndex = 3;
            txtBoxParam1.TextChanged += txtBoxParam1_TextChanged;
            // 
            // lblParam1
            // 
            lblParam1.AutoSize = true;
            lblParam1.Location = new Point(40, 36);
            lblParam1.Name = "lblParam1";
            lblParam1.Size = new Size(87, 20);
            lblParam1.TabIndex = 1;
            lblParam1.Text = "Parameter1:";
            // 
            // btnStartFE
            // 
            btnStartFE.Location = new Point(40, 198);
            btnStartFE.Name = "btnStartFE";
            btnStartFE.Size = new Size(125, 56);
            btnStartFE.TabIndex = 0;
            btnStartFE.Text = "Start FE";
            btnStartFE.UseVisualStyleBackColor = true;
            btnStartFE.Click += btnStartFE_Click;
            // 
            // btnShowData
            // 
            btnShowData.Location = new Point(40, 390);
            btnShowData.Name = "btnShowData";
            btnShowData.Size = new Size(125, 59);
            btnShowData.TabIndex = 15;
            btnShowData.Text = "Show data from class";
            btnShowData.UseVisualStyleBackColor = true;
            btnShowData.Click += btnShowData_Click;
            // 
            // ucLocalhost
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(webView21);
            Name = "ucLocalhost";
            Size = new Size(820, 581);
            Load += ucLocalhost_Load;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Panel panel1;
        private Label lblParam1;
        private Button btnStartFE;
        private TextBox txtBoxParam2;
        private Label lblParam3;
        private Label lblParam2;
        private TextBox txtBoxParam1;
        private TextBox txtBoxParam3;
        private Label lblCommunicationStatus;
        private Button btnSendDataToFe;
        private Button btnShowPage;
        private Button btnGetData;
        private Label lblData;
        private Button btnOpenDevTool;
        private Button btnStopFE;
        private Button btnShowData;
    }
}
