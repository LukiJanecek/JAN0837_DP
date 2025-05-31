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
            btnLocalHost = new ToolStripButton();
            rbtnOPCUA = new RadioButton();
            rbtnMQTT = new RadioButton();
            rbtnTCPIP = new RadioButton();
            rbtnRESTAPI = new RadioButton();
            rbtnModbusTCPIP = new RadioButton();
            btnStartCommunication = new Button();
            lblPara1 = new Label();
            lblPara2 = new Label();
            txtBoxPara1 = new TextBox();
            txtBoxPara2 = new TextBox();
            btnUsePreset = new Button();
            btnStopCommunication = new Button();
            checkBoxMaster = new CheckBox();
            checkBoxSlave = new CheckBox();
            lblCheckBox = new Label();
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
            toolStripMain.Items.AddRange(new ToolStripItem[] { btnGenerateTIATemplate, btnLocalHost });
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
            // btnLocalHost
            // 
            btnLocalHost.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLocalHost.Image = (Image)resources.GetObject("btnLocalHost.Image");
            btnLocalHost.ImageTransparentColor = Color.Magenta;
            btnLocalHost.Name = "btnLocalHost";
            btnLocalHost.Size = new Size(113, 24);
            btnLocalHost.Text = "Open localhost";
            btnLocalHost.Click += btnLocalHost_Click;
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
            rbtnRESTAPI.Location = new Point(45, 174);
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
            rbtnModbusTCPIP.Location = new Point(45, 144);
            rbtnModbusTCPIP.Name = "rbtnModbusTCPIP";
            rbtnModbusTCPIP.Size = new Size(130, 24);
            rbtnModbusTCPIP.TabIndex = 6;
            rbtnModbusTCPIP.TabStop = true;
            rbtnModbusTCPIP.Text = "Modbus TCP/IP";
            rbtnModbusTCPIP.UseVisualStyleBackColor = true;
            rbtnModbusTCPIP.CheckedChanged += rbtnModbusTCPIP_CheckedChanged;
            // 
            // btnStartCommunication
            // 
            btnStartCommunication.Location = new Point(519, 54);
            btnStartCommunication.Name = "btnStartCommunication";
            btnStartCommunication.Size = new Size(180, 60);
            btnStartCommunication.TabIndex = 7;
            btnStartCommunication.Text = "Start Communication";
            btnStartCommunication.UseVisualStyleBackColor = true;
            btnStartCommunication.Click += btnStart_Click;
            // 
            // lblPara1
            // 
            lblPara1.AutoSize = true;
            lblPara1.Location = new Point(247, 54);
            lblPara1.Name = "lblPara1";
            lblPara1.Size = new Size(87, 20);
            lblPara1.TabIndex = 8;
            lblPara1.Text = "Parameter1:";
            // 
            // lblPara2
            // 
            lblPara2.AutoSize = true;
            lblPara2.Location = new Point(247, 107);
            lblPara2.Name = "lblPara2";
            lblPara2.Size = new Size(87, 20);
            lblPara2.TabIndex = 9;
            lblPara2.Text = "Parameter2:";
            // 
            // txtBoxPara1
            // 
            txtBoxPara1.Location = new Point(247, 77);
            txtBoxPara1.Name = "txtBoxPara1";
            txtBoxPara1.Size = new Size(211, 27);
            txtBoxPara1.TabIndex = 10;
            // 
            // txtBoxPara2
            // 
            txtBoxPara2.Location = new Point(247, 130);
            txtBoxPara2.Name = "txtBoxPara2";
            txtBoxPara2.Size = new Size(211, 27);
            txtBoxPara2.TabIndex = 11;
            // 
            // btnUsePreset
            // 
            btnUsePreset.Location = new Point(247, 264);
            btnUsePreset.Name = "btnUsePreset";
            btnUsePreset.Size = new Size(94, 29);
            btnUsePreset.TabIndex = 12;
            btnUsePreset.Text = "Use Preset";
            btnUsePreset.UseVisualStyleBackColor = true;
            btnUsePreset.Click += btnUsePreset_Click;
            // 
            // btnStopCommunication
            // 
            btnStopCommunication.Location = new Point(519, 126);
            btnStopCommunication.Name = "btnStopCommunication";
            btnStopCommunication.Size = new Size(180, 60);
            btnStopCommunication.TabIndex = 13;
            btnStopCommunication.Text = "Stop Communication";
            btnStopCommunication.UseVisualStyleBackColor = true;
            btnStopCommunication.Click += btnStopCommunication_Click;
            // 
            // checkBoxMaster
            // 
            checkBoxMaster.AutoSize = true;
            checkBoxMaster.Location = new Point(247, 204);
            checkBoxMaster.Name = "checkBoxMaster";
            checkBoxMaster.Size = new Size(76, 24);
            checkBoxMaster.TabIndex = 14;
            checkBoxMaster.Text = "Master";
            checkBoxMaster.UseVisualStyleBackColor = true;
            checkBoxMaster.CheckedChanged += checkBoxMaster_CheckedChanged;
            // 
            // checkBoxSlave
            // 
            checkBoxSlave.AutoSize = true;
            checkBoxSlave.Location = new Point(247, 234);
            checkBoxSlave.Name = "checkBoxSlave";
            checkBoxSlave.Size = new Size(66, 24);
            checkBoxSlave.TabIndex = 15;
            checkBoxSlave.Text = "Slave";
            checkBoxSlave.UseVisualStyleBackColor = true;
            checkBoxSlave.CheckedChanged += checkBoxSlave_CheckedChanged;
            // 
            // lblCheckBox
            // 
            lblCheckBox.AutoSize = true;
            lblCheckBox.Location = new Point(247, 183);
            lblCheckBox.Name = "lblCheckBox";
            lblCheckBox.Size = new Size(139, 20);
            lblCheckBox.TabIndex = 18;
            lblCheckBox.Text = "What is this device?";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1096, 588);
            Controls.Add(lblCheckBox);
            Controls.Add(checkBoxSlave);
            Controls.Add(checkBoxMaster);
            Controls.Add(btnStopCommunication);
            Controls.Add(btnUsePreset);
            Controls.Add(txtBoxPara2);
            Controls.Add(txtBoxPara1);
            Controls.Add(lblPara2);
            Controls.Add(lblPara1);
            Controls.Add(btnStartCommunication);
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
        private Button btnStartCommunication;
        private Label lblPara1;
        private Label lblPara2;
        private TextBox txtBoxPara1;
        private TextBox txtBoxPara2;
        private Button btnUsePreset;
        private Button btnStopCommunication;
        private CheckBox checkBoxMaster;
        private CheckBox checkBoxSlave;
        private Label lblCheckBox;
        private ToolStripButton btnLocalHost;
    }
}
