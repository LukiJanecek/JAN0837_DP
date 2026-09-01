namespace JAN0837_DP.Forms
{
    partial class ucCommunicationControl
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
            lblPara1 = new Label();
            statusStripCommunicationControl = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            rbtnOPCUA = new RadioButton();
            rbtnMQTT = new RadioButton();
            rbtnModbusTCPIP = new RadioButton();
            rbtnRESTAPI = new RadioButton();
            lblPara2 = new Label();
            lblCheckBox = new Label();
            btnPreSet = new Button();
            btnStartCommunicationThread = new Button();
            btnStopCommunicationThread = new Button();
            lblCommunicationStatus = new Label();
            checkBoxMaster = new CheckBox();
            checkBoxSlave = new CheckBox();
            txtBoxPara1 = new TextBox();
            txtBoxPara2 = new TextBox();
            rbtnSharp7 = new RadioButton();
            lblEnabledPorts = new Label();
            statusStripCommunicationControl.SuspendLayout();
            SuspendLayout();
            // 
            // lblPara1
            // 
            lblPara1.AutoSize = true;
            lblPara1.Location = new Point(283, 2);
            lblPara1.Name = "lblPara1";
            lblPara1.Size = new Size(70, 15);
            lblPara1.TabIndex = 2;
            lblPara1.Text = "Parameter1:";
            // 
            // statusStripCommunicationControl
            // 
            statusStripCommunicationControl.ImageScalingSize = new Size(20, 20);
            statusStripCommunicationControl.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStripCommunicationControl.Location = new Point(0, 370);
            statusStripCommunicationControl.Name = "statusStripCommunicationControl";
            statusStripCommunicationControl.Padding = new Padding(1, 0, 12, 0);
            statusStripCommunicationControl.Size = new Size(895, 22);
            statusStripCommunicationControl.TabIndex = 3;
            statusStripCommunicationControl.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(44, 17);
            lblStatus.Text = "status1";
            // 
            // rbtnOPCUA
            // 
            rbtnOPCUA.AutoSize = true;
            rbtnOPCUA.Location = new Point(3, 2);
            rbtnOPCUA.Margin = new Padding(3, 2, 3, 2);
            rbtnOPCUA.Name = "rbtnOPCUA";
            rbtnOPCUA.Size = new Size(68, 19);
            rbtnOPCUA.TabIndex = 4;
            rbtnOPCUA.Text = "OPC UA";
            rbtnOPCUA.UseVisualStyleBackColor = true;
            rbtnOPCUA.CheckedChanged += rbtnOPCUA_CheckedChanged;
            // 
            // rbtnMQTT
            // 
            rbtnMQTT.AutoSize = true;
            rbtnMQTT.Location = new Point(3, 25);
            rbtnMQTT.Margin = new Padding(3, 2, 3, 2);
            rbtnMQTT.Name = "rbtnMQTT";
            rbtnMQTT.Size = new Size(56, 19);
            rbtnMQTT.TabIndex = 5;
            rbtnMQTT.Text = "MQTT";
            rbtnMQTT.UseVisualStyleBackColor = true;
            rbtnMQTT.CheckedChanged += rbtnMQTT_CheckedChanged;
            // 
            // rbtnModbusTCPIP
            // 
            rbtnModbusTCPIP.AutoSize = true;
            rbtnModbusTCPIP.Location = new Point(3, 47);
            rbtnModbusTCPIP.Margin = new Padding(3, 2, 3, 2);
            rbtnModbusTCPIP.Name = "rbtnModbusTCPIP";
            rbtnModbusTCPIP.Size = new Size(107, 19);
            rbtnModbusTCPIP.TabIndex = 7;
            rbtnModbusTCPIP.Text = "Modbus TCP/IP";
            rbtnModbusTCPIP.UseVisualStyleBackColor = true;
            rbtnModbusTCPIP.CheckedChanged += rbtnModbusTCPIP_CheckedChanged;
            // 
            // rbtnRESTAPI
            // 
            rbtnRESTAPI.AutoSize = true;
            rbtnRESTAPI.Location = new Point(3, 70);
            rbtnRESTAPI.Margin = new Padding(3, 2, 3, 2);
            rbtnRESTAPI.Name = "rbtnRESTAPI";
            rbtnRESTAPI.Size = new Size(71, 19);
            rbtnRESTAPI.TabIndex = 8;
            rbtnRESTAPI.Text = "REST API";
            rbtnRESTAPI.UseVisualStyleBackColor = true;
            rbtnRESTAPI.CheckedChanged += rbtnRESTAPI_CheckedChanged;
            // 
            // lblPara2
            // 
            lblPara2.AutoSize = true;
            lblPara2.Location = new Point(283, 42);
            lblPara2.Name = "lblPara2";
            lblPara2.Size = new Size(70, 15);
            lblPara2.TabIndex = 10;
            lblPara2.Text = "Parameter2:";
            // 
            // lblCheckBox
            // 
            lblCheckBox.AutoSize = true;
            lblCheckBox.Location = new Point(138, 26);
            lblCheckBox.Name = "lblCheckBox";
            lblCheckBox.Size = new Size(110, 15);
            lblCheckBox.TabIndex = 12;
            lblCheckBox.Text = "What is this device?";
            // 
            // btnPreSet
            // 
            btnPreSet.Location = new Point(138, 2);
            btnPreSet.Margin = new Padding(3, 2, 3, 2);
            btnPreSet.Name = "btnPreSet";
            btnPreSet.Size = new Size(82, 22);
            btnPreSet.TabIndex = 15;
            btnPreSet.Text = "Use PreSet";
            btnPreSet.UseVisualStyleBackColor = true;
            btnPreSet.Click += btnPreSet_Click;
            // 
            // btnStartCommunicationThread
            // 
            btnStartCommunicationThread.Location = new Point(478, 4);
            btnStartCommunicationThread.Margin = new Padding(3, 2, 3, 2);
            btnStartCommunicationThread.Name = "btnStartCommunicationThread";
            btnStartCommunicationThread.Size = new Size(160, 38);
            btnStartCommunicationThread.TabIndex = 16;
            btnStartCommunicationThread.Text = "Start Communication";
            btnStartCommunicationThread.UseVisualStyleBackColor = true;
            btnStartCommunicationThread.Click += btnStartCommunicationThread_Click;
            // 
            // btnStopCommunicationThread
            // 
            btnStopCommunicationThread.Location = new Point(478, 47);
            btnStopCommunicationThread.Margin = new Padding(3, 2, 3, 2);
            btnStopCommunicationThread.Name = "btnStopCommunicationThread";
            btnStopCommunicationThread.Size = new Size(160, 38);
            btnStopCommunicationThread.TabIndex = 17;
            btnStopCommunicationThread.Text = "Stop Communication";
            btnStopCommunicationThread.UseVisualStyleBackColor = true;
            btnStopCommunicationThread.Click += btnStopCommunicationThread_Click;
            // 
            // lblCommunicationStatus
            // 
            lblCommunicationStatus.AutoSize = true;
            lblCommunicationStatus.Location = new Point(478, 92);
            lblCommunicationStatus.Name = "lblCommunicationStatus";
            lblCommunicationStatus.Size = new Size(134, 15);
            lblCommunicationStatus.TabIndex = 18;
            lblCommunicationStatus.Text = "Communication status. ";
            // 
            // checkBoxMaster
            // 
            checkBoxMaster.AutoSize = true;
            checkBoxMaster.Location = new Point(138, 44);
            checkBoxMaster.Margin = new Padding(3, 2, 3, 2);
            checkBoxMaster.Name = "checkBoxMaster";
            checkBoxMaster.Size = new Size(83, 19);
            checkBoxMaster.TabIndex = 19;
            checkBoxMaster.Text = "checkBox1";
            checkBoxMaster.UseVisualStyleBackColor = true;
            checkBoxMaster.CheckedChanged += checkBoxMaster_CheckedChanged;
            // 
            // checkBoxSlave
            // 
            checkBoxSlave.AutoSize = true;
            checkBoxSlave.Location = new Point(138, 66);
            checkBoxSlave.Margin = new Padding(3, 2, 3, 2);
            checkBoxSlave.Name = "checkBoxSlave";
            checkBoxSlave.Size = new Size(83, 19);
            checkBoxSlave.TabIndex = 20;
            checkBoxSlave.Text = "checkBox2";
            checkBoxSlave.UseVisualStyleBackColor = true;
            checkBoxSlave.CheckedChanged += checkBoxSlave_CheckedChanged;
            // 
            // txtBoxPara1
            // 
            txtBoxPara1.Location = new Point(283, 20);
            txtBoxPara1.Margin = new Padding(3, 2, 3, 2);
            txtBoxPara1.Name = "txtBoxPara1";
            txtBoxPara1.Size = new Size(190, 23);
            txtBoxPara1.TabIndex = 21;
            txtBoxPara1.TextChanged += txtBoxPara1_TextChanged;
            // 
            // txtBoxPara2
            // 
            txtBoxPara2.Location = new Point(283, 59);
            txtBoxPara2.Margin = new Padding(3, 2, 3, 2);
            txtBoxPara2.Name = "txtBoxPara2";
            txtBoxPara2.Size = new Size(190, 23);
            txtBoxPara2.TabIndex = 22;
            txtBoxPara2.TextChanged += txtBoxPara2_TextChanged;
            // 
            // rbtnSharp7
            // 
            rbtnSharp7.AutoSize = true;
            rbtnSharp7.Location = new Point(3, 92);
            rbtnSharp7.Margin = new Padding(3, 2, 3, 2);
            rbtnSharp7.Name = "rbtnSharp7";
            rbtnSharp7.Size = new Size(61, 19);
            rbtnSharp7.TabIndex = 23;
            rbtnSharp7.Text = "Sharp7";
            rbtnSharp7.UseVisualStyleBackColor = true;
            rbtnSharp7.CheckedChanged += rbtnSharp7_CheckedChanged;
            // 
            // lblEnabledPorts
            // 
            lblEnabledPorts.AutoSize = true;
            lblEnabledPorts.Location = new Point(3, 113);
            lblEnabledPorts.Name = "lblEnabledPorts";
            lblEnabledPorts.Size = new Size(38, 15);
            lblEnabledPorts.TabIndex = 24;
            lblEnabledPorts.Text = "label1";
            // 
            // ucCommunicationControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblEnabledPorts);
            Controls.Add(rbtnSharp7);
            Controls.Add(txtBoxPara2);
            Controls.Add(txtBoxPara1);
            Controls.Add(checkBoxSlave);
            Controls.Add(checkBoxMaster);
            Controls.Add(lblCommunicationStatus);
            Controls.Add(btnStopCommunicationThread);
            Controls.Add(btnStartCommunicationThread);
            Controls.Add(btnPreSet);
            Controls.Add(lblCheckBox);
            Controls.Add(lblPara2);
            Controls.Add(rbtnRESTAPI);
            Controls.Add(rbtnModbusTCPIP);
            Controls.Add(rbtnMQTT);
            Controls.Add(rbtnOPCUA);
            Controls.Add(statusStripCommunicationControl);
            Controls.Add(lblPara1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "ucCommunicationControl";
            Size = new Size(895, 392);
            Load += CommunicationControl_Load;
            statusStripCommunicationControl.ResumeLayout(false);
            statusStripCommunicationControl.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblPara1;
        private StatusStrip statusStripCommunicationControl;
        private ToolStripStatusLabel lblStatus;
        private RadioButton rbtnOPCUA;
        private RadioButton rbtnMQTT;
        private RadioButton rbtnModbusTCPIP;
        private RadioButton rbtnRESTAPI;
        private Label lblPara2;
        private Label lblCheckBox;
        private Button btnPreSet;
        private Button btnStartCommunicationThread;
        private Button btnStopCommunicationThread;
        private Label lblCommunicationStatus;
        private CheckBox checkBoxMaster;
        private CheckBox checkBoxSlave;
        private TextBox txtBoxPara1;
        private TextBox txtBoxPara2;
        private RadioButton rbtnSharp7;
        private Label lblEnabledPorts;
    }
}
