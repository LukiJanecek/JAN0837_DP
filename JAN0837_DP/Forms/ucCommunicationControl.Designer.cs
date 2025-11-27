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
            rbtnTCPIP = new RadioButton();
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
            listBox1 = new ListBox();
            btnActualCrossroaddata = new Button();
            statusStripCommunicationControl.SuspendLayout();
            SuspendLayout();
            // 
            // lblPara1
            // 
            lblPara1.AutoSize = true;
            lblPara1.Location = new Point(323, 3);
            lblPara1.Name = "lblPara1";
            lblPara1.Size = new Size(87, 20);
            lblPara1.TabIndex = 2;
            lblPara1.Text = "Parameter1:";
            // 
            // statusStripCommunicationControl
            // 
            statusStripCommunicationControl.ImageScalingSize = new Size(20, 20);
            statusStripCommunicationControl.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStripCommunicationControl.Location = new Point(0, 496);
            statusStripCommunicationControl.Name = "statusStripCommunicationControl";
            statusStripCommunicationControl.Size = new Size(1023, 26);
            statusStripCommunicationControl.TabIndex = 3;
            statusStripCommunicationControl.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(55, 20);
            lblStatus.Text = "status1";
            // 
            // rbtnOPCUA
            // 
            rbtnOPCUA.AutoSize = true;
            rbtnOPCUA.Location = new Point(3, 3);
            rbtnOPCUA.Name = "rbtnOPCUA";
            rbtnOPCUA.Size = new Size(82, 24);
            rbtnOPCUA.TabIndex = 4;
            rbtnOPCUA.Text = "OPC UA";
            rbtnOPCUA.UseVisualStyleBackColor = true;
            rbtnOPCUA.CheckedChanged += rbtnOPCUA_CheckedChanged;
            // 
            // rbtnMQTT
            // 
            rbtnMQTT.AutoSize = true;
            rbtnMQTT.Location = new Point(3, 33);
            rbtnMQTT.Name = "rbtnMQTT";
            rbtnMQTT.Size = new Size(69, 24);
            rbtnMQTT.TabIndex = 5;
            rbtnMQTT.Text = "MQTT";
            rbtnMQTT.UseVisualStyleBackColor = true;
            rbtnMQTT.CheckedChanged += rbtnMQTT_CheckedChanged;
            // 
            // rbtnTCPIP
            // 
            rbtnTCPIP.AutoSize = true;
            rbtnTCPIP.Location = new Point(3, 63);
            rbtnTCPIP.Name = "rbtnTCPIP";
            rbtnTCPIP.Size = new Size(72, 24);
            rbtnTCPIP.TabIndex = 6;
            rbtnTCPIP.Text = "TCP/IP";
            rbtnTCPIP.UseVisualStyleBackColor = true;
            rbtnTCPIP.CheckedChanged += rbtnTCPIP_CheckedChanged;
            // 
            // rbtnModbusTCPIP
            // 
            rbtnModbusTCPIP.AutoSize = true;
            rbtnModbusTCPIP.Location = new Point(3, 93);
            rbtnModbusTCPIP.Name = "rbtnModbusTCPIP";
            rbtnModbusTCPIP.Size = new Size(130, 24);
            rbtnModbusTCPIP.TabIndex = 7;
            rbtnModbusTCPIP.Text = "Modbus TCP/IP";
            rbtnModbusTCPIP.UseVisualStyleBackColor = true;
            rbtnModbusTCPIP.CheckedChanged += rbtnModbusTCPIP_CheckedChanged;
            // 
            // rbtnRESTAPI
            // 
            rbtnRESTAPI.AutoSize = true;
            rbtnRESTAPI.Location = new Point(3, 123);
            rbtnRESTAPI.Name = "rbtnRESTAPI";
            rbtnRESTAPI.Size = new Size(89, 24);
            rbtnRESTAPI.TabIndex = 8;
            rbtnRESTAPI.Text = "REST API";
            rbtnRESTAPI.UseVisualStyleBackColor = true;
            rbtnRESTAPI.CheckedChanged += rbtnRESTAPI_CheckedChanged;
            // 
            // lblPara2
            // 
            lblPara2.AutoSize = true;
            lblPara2.Location = new Point(323, 56);
            lblPara2.Name = "lblPara2";
            lblPara2.Size = new Size(87, 20);
            lblPara2.TabIndex = 10;
            lblPara2.Text = "Parameter2:";
            // 
            // lblCheckBox
            // 
            lblCheckBox.AutoSize = true;
            lblCheckBox.Location = new Point(158, 35);
            lblCheckBox.Name = "lblCheckBox";
            lblCheckBox.Size = new Size(139, 20);
            lblCheckBox.TabIndex = 12;
            lblCheckBox.Text = "What is this device?";
            // 
            // btnPreSet
            // 
            btnPreSet.Location = new Point(158, 3);
            btnPreSet.Name = "btnPreSet";
            btnPreSet.Size = new Size(94, 29);
            btnPreSet.TabIndex = 15;
            btnPreSet.Text = "Use PreSet";
            btnPreSet.UseVisualStyleBackColor = true;
            btnPreSet.Click += btnPreSet_Click;
            // 
            // btnStartCommunicationThread
            // 
            btnStartCommunicationThread.Location = new Point(546, 6);
            btnStartCommunicationThread.Name = "btnStartCommunicationThread";
            btnStartCommunicationThread.Size = new Size(183, 51);
            btnStartCommunicationThread.TabIndex = 16;
            btnStartCommunicationThread.Text = "Start Communication";
            btnStartCommunicationThread.UseVisualStyleBackColor = true;
            btnStartCommunicationThread.Click += btnStartCommunicationThread_Click;
            // 
            // btnStopCommunicationThread
            // 
            btnStopCommunicationThread.Location = new Point(546, 63);
            btnStopCommunicationThread.Name = "btnStopCommunicationThread";
            btnStopCommunicationThread.Size = new Size(183, 51);
            btnStopCommunicationThread.TabIndex = 17;
            btnStopCommunicationThread.Text = "Stop Communication";
            btnStopCommunicationThread.UseVisualStyleBackColor = true;
            btnStopCommunicationThread.Click += btnStopCommunicationThread_Click;
            // 
            // lblCommunicationStatus
            // 
            lblCommunicationStatus.AutoSize = true;
            lblCommunicationStatus.Location = new Point(546, 122);
            lblCommunicationStatus.Name = "lblCommunicationStatus";
            lblCommunicationStatus.Size = new Size(163, 20);
            lblCommunicationStatus.TabIndex = 18;
            lblCommunicationStatus.Text = "Communication status. ";
            // 
            // checkBoxMaster
            // 
            checkBoxMaster.AutoSize = true;
            checkBoxMaster.Location = new Point(158, 58);
            checkBoxMaster.Name = "checkBoxMaster";
            checkBoxMaster.Size = new Size(101, 24);
            checkBoxMaster.TabIndex = 19;
            checkBoxMaster.Text = "checkBox1";
            checkBoxMaster.UseVisualStyleBackColor = true;
            checkBoxMaster.CheckedChanged += checkBoxMaster_CheckedChanged;
            // 
            // checkBoxSlave
            // 
            checkBoxSlave.AutoSize = true;
            checkBoxSlave.Location = new Point(158, 88);
            checkBoxSlave.Name = "checkBoxSlave";
            checkBoxSlave.Size = new Size(101, 24);
            checkBoxSlave.TabIndex = 20;
            checkBoxSlave.Text = "checkBox2";
            checkBoxSlave.UseVisualStyleBackColor = true;
            checkBoxSlave.CheckedChanged += checkBoxSlave_CheckedChanged;
            // 
            // txtBoxPara1
            // 
            txtBoxPara1.Location = new Point(323, 26);
            txtBoxPara1.Name = "txtBoxPara1";
            txtBoxPara1.Size = new Size(217, 27);
            txtBoxPara1.TabIndex = 21;
            txtBoxPara1.TextChanged += txtBoxPara1_TextChanged;
            // 
            // txtBoxPara2
            // 
            txtBoxPara2.Location = new Point(323, 79);
            txtBoxPara2.Name = "txtBoxPara2";
            txtBoxPara2.Size = new Size(217, 27);
            txtBoxPara2.TabIndex = 22;
            txtBoxPara2.TextChanged += txtBoxPara2_TextChanged;
            // 
            // rbtnSharp7
            // 
            rbtnSharp7.AutoSize = true;
            rbtnSharp7.Location = new Point(3, 153);
            rbtnSharp7.Name = "rbtnSharp7";
            rbtnSharp7.Size = new Size(76, 24);
            rbtnSharp7.TabIndex = 23;
            rbtnSharp7.Text = "Sharp7";
            rbtnSharp7.UseVisualStyleBackColor = true;
            rbtnSharp7.CheckedChanged += rbtnSharp7_CheckedChanged;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(158, 186);
            listBox1.Name = "listBox1";
            listBox1.ScrollAlwaysVisible = true;
            listBox1.Size = new Size(282, 224);
            listBox1.TabIndex = 24;
            // 
            // btnActualCrossroaddata
            // 
            btnActualCrossroaddata.Location = new Point(158, 148);
            btnActualCrossroaddata.Name = "btnActualCrossroaddata";
            btnActualCrossroaddata.Size = new Size(140, 29);
            btnActualCrossroaddata.TabIndex = 25;
            btnActualCrossroaddata.Text = "Aktualizovat výpis";
            btnActualCrossroaddata.UseVisualStyleBackColor = true;
            btnActualCrossroaddata.Click += btnActualCrossroaddata_Click;
            // 
            // ucCommunicationControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnActualCrossroaddata);
            Controls.Add(listBox1);
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
            Controls.Add(rbtnTCPIP);
            Controls.Add(rbtnMQTT);
            Controls.Add(rbtnOPCUA);
            Controls.Add(statusStripCommunicationControl);
            Controls.Add(lblPara1);
            Name = "ucCommunicationControl";
            Size = new Size(1023, 522);
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
        private RadioButton rbtnTCPIP;
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
        private ListBox listBox1;
        private Button btnActualCrossroaddata;
    }
}
