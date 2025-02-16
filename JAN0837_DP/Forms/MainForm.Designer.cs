namespace JAN0837_DP
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            statusStripMain = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            toolStripMain = new ToolStrip();
            btnGenerateTIATemplate = new ToolStripButton();
            rbtnOPCUA = new RadioButton();
            rbtnMQTT = new RadioButton();
            rbtnTCPIP = new RadioButton();
            rbtnRESTAPI = new RadioButton();
            rbtnModbusTCPIP = new RadioButton();
            btnStart = new Button();
            statusStripMain.SuspendLayout();
            toolStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripMain
            // 
            statusStripMain.ImageScalingSize = new Size(20, 20);
            statusStripMain.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStripMain.Location = new Point(0, 562);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Size = new Size(1096, 26);
            statusStripMain.TabIndex = 0;
            statusStripMain.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(90, 20);
            lblStatus.Text = "Default text.";
            // 
            // toolStripMain
            // 
            toolStripMain.ImageScalingSize = new Size(20, 20);
            toolStripMain.Items.AddRange(new ToolStripItem[] { btnGenerateTIATemplate });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(1096, 27);
            toolStripMain.TabIndex = 1;
            toolStripMain.Text = "toolStrip1";
            // 
            // btnGenerateTIATemplate
            // 
            btnGenerateTIATemplate.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGenerateTIATemplate.Image = (Image)resources.GetObject("btnGenerateTIATemplate.Image");
            btnGenerateTIATemplate.ImageTransparentColor = Color.Magenta;
            btnGenerateTIATemplate.Name = "btnGenerateTIATemplate";
            btnGenerateTIATemplate.Size = new Size(165, 24);
            btnGenerateTIATemplate.Text = "Generate TIA Template";
            btnGenerateTIATemplate.Click += btnGenerateTIATemplate_Click;
            // 
            // rbtnOPCUA
            // 
            rbtnOPCUA.AutoSize = true;
            rbtnOPCUA.Location = new Point(45, 54);
            rbtnOPCUA.Name = "rbtnOPCUA";
            rbtnOPCUA.Size = new Size(82, 24);
            rbtnOPCUA.TabIndex = 2;
            rbtnOPCUA.TabStop = true;
            rbtnOPCUA.Text = "OPC UA";
            rbtnOPCUA.UseVisualStyleBackColor = true;
            rbtnOPCUA.CheckedChanged += rbtnOPCUA_CheckedChanged;
            // 
            // rbtnMQTT
            // 
            rbtnMQTT.AutoSize = true;
            rbtnMQTT.Location = new Point(45, 84);
            rbtnMQTT.Name = "rbtnMQTT";
            rbtnMQTT.Size = new Size(69, 24);
            rbtnMQTT.TabIndex = 3;
            rbtnMQTT.TabStop = true;
            rbtnMQTT.Text = "MQTT";
            rbtnMQTT.UseVisualStyleBackColor = true;
            rbtnMQTT.CheckedChanged += rbtnMQTT_CheckedChanged;
            // 
            // rbtnTCPIP
            // 
            rbtnTCPIP.AutoSize = true;
            rbtnTCPIP.Location = new Point(45, 114);
            rbtnTCPIP.Name = "rbtnTCPIP";
            rbtnTCPIP.Size = new Size(72, 24);
            rbtnTCPIP.TabIndex = 4;
            rbtnTCPIP.TabStop = true;
            rbtnTCPIP.Text = "TCP/IP";
            rbtnTCPIP.UseVisualStyleBackColor = true;
            rbtnTCPIP.CheckedChanged += rbtnTCPIP_CheckedChanged;
            // 
            // rbtnRESTAPI
            // 
            rbtnRESTAPI.AutoSize = true;
            rbtnRESTAPI.Location = new Point(45, 144);
            rbtnRESTAPI.Name = "rbtnRESTAPI";
            rbtnRESTAPI.Size = new Size(89, 24);
            rbtnRESTAPI.TabIndex = 5;
            rbtnRESTAPI.TabStop = true;
            rbtnRESTAPI.Text = "REST API";
            rbtnRESTAPI.UseVisualStyleBackColor = true;
            rbtnRESTAPI.CheckedChanged += rbtnRESTAPI_CheckedChanged;
            // 
            // rbtnModbusTCPIP
            // 
            rbtnModbusTCPIP.AutoSize = true;
            rbtnModbusTCPIP.Location = new Point(45, 174);
            rbtnModbusTCPIP.Name = "rbtnModbusTCPIP";
            rbtnModbusTCPIP.Size = new Size(130, 24);
            rbtnModbusTCPIP.TabIndex = 6;
            rbtnModbusTCPIP.TabStop = true;
            rbtnModbusTCPIP.Text = "Modbus TCP/IP";
            rbtnModbusTCPIP.UseVisualStyleBackColor = true;
            rbtnModbusTCPIP.CheckedChanged += rbtnModbusTCPIP_CheckedChanged;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(230, 64);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 29);
            btnStart.TabIndex = 7;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1096, 588);
            Controls.Add(btnStart);
            Controls.Add(rbtnModbusTCPIP);
            Controls.Add(rbtnRESTAPI);
            Controls.Add(rbtnTCPIP);
            Controls.Add(rbtnMQTT);
            Controls.Add(rbtnOPCUA);
            Controls.Add(toolStripMain);
            Controls.Add(statusStripMain);
            Name = "MainForm";
            ShowIcon = false;
            Load += MainForm_Load;
            statusStripMain.ResumeLayout(false);
            statusStripMain.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStripMain;
        private ToolStrip toolStripMain;
        private RadioButton rbtnOPCUA;
        private RadioButton rbtnMQTT;
        private RadioButton rbtnTCPIP;
        private RadioButton rbtnRESTAPI;
        private RadioButton rbtnModbusTCPIP;
        private ToolStripStatusLabel lblStatus;
        private ToolStripButton btnGenerateTIATemplate;
        private Button btnStart;
    }
}
