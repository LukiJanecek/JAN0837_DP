using JAN0837_DP;
using JAN0837_DP.Communication;
using JAN0837_DP.Communication.comModbusTCPIP;
using JAN0837_DP.Communication.comS7;
using JAN0837_DP.Communication.comSharp7;
using JAN0837_DP.Communication.comTCPIP;
using JAN0837_DP.Data;
using Opc.Ua;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Sharp7.S7Consts;

namespace JAN0837_DP.Forms
{
    public partial class ucCommunicationControl : UserControl
    {
        public comS7 _s7;
        public comSharp7 _sharp7;
        public comTCPIP _tcpip;
        public ModbusTCPIPimMaster _modbusMaster;
        public ModbusTCPIPimSlave _modbusSlave;

        public string ModbusTCPIP_ipaddress;
        public int ModbusTCPIP_port;

        public ucCommunicationControl()
        {
            InitializeComponent();
        }

        private void CommunicationControl_Load(object sender, EventArgs e)
        {
            if (internalVariables.communicationFlag == "")
            {
                // UI settings      
                #region UI settings 

                // rbtn
                rbtnModbusTCPIP.Enabled = true;
                rbtnModbusTCPIP.Visible = true;
                rbtnModbusTCPIP.Checked = false;

                rbtnMQTT.Enabled = true;
                rbtnMQTT.Visible = true;
                rbtnMQTT.Checked = false;

                rbtnTCPIP.Enabled = true;
                rbtnTCPIP.Visible = true;
                rbtnTCPIP.Checked = false;

                rbtnModbusTCPIP.Enabled = true;
                rbtnModbusTCPIP.Visible = true;
                rbtnModbusTCPIP.Checked = false;

                rbtnRESTAPI.Enabled = true;
                rbtnRESTAPI.Visible = true;
                rbtnRESTAPI.Checked = false;

                rbtnOPCUA.Tag = "OPCUA";
                rbtnMQTT.Tag = "MQTT";
                rbtnTCPIP.Tag = "TCPIP";
                rbtnModbusTCPIP.Tag = "ModbusTCPIP";
                rbtnRESTAPI.Tag = "RestApi";

                // btns
                btnStartCommunicationThread.Visible = false;
                btnStartCommunicationThread.Enabled = false;

                btnStopCommunicationThread.Visible = false;
                btnStopCommunicationThread.Enabled = false;

                btnPreSet.Visible = false;
                btnPreSet.Enabled = false;

                lblCommunicationStatus.Visible = false;

                // para
                lblPara1.Visible = false;
                lblPara1.Enabled = false;
                lblPara1.Text = "Parameter1: ";
                txtBoxPara1.Visible = false;
                txtBoxPara1.Enabled = false;
                txtBoxPara1.Text = "";

                lblPara2.Visible = false;
                lblPara2.Enabled = false;
                lblPara2.Text = "Parameter2: ";
                txtBoxPara2.Visible = false;
                txtBoxPara2.Enabled = false;
                txtBoxPara2.Text = "";

                // check box
                lblCheckBox.Visible = false;
                lblCheckBox.Enabled = false;
                lblCheckBox.Text = "";

                checkBoxMaster.Visible = false;
                checkBoxMaster.Enabled = false;
                checkBoxMaster.Text = "";
                checkBoxMaster.Checked = false;

                checkBoxSlave.Visible = false;
                checkBoxSlave.Enabled = false;
                checkBoxSlave.Text = "";
                checkBoxSlave.Checked = false;

                lblStatus.Text = "Select communication protocol.";

                #endregion
            }

            switch (internalVariables.communicationFlag)
            {
                case "OPCUA":
                    rbtnOPCUA.Checked = true;
                    rbtnOPCUA_CheckedChanged(sender, e);
                    break;
                case "MQTT":
                    rbtnMQTT.Checked = true;
                    rbtnMQTT_CheckedChanged(sender, e);
                    break;
                case "ModbusTCPIP":
                    rbtnModbusTCPIP.Checked = true;
                    rbtnModbusTCPIP_CheckedChanged(sender, e);
                    break;
                case "TCPIP":
                    rbtnTCPIP.Checked = true;
                    rbtnTCPIP_CheckedChanged(sender, e);
                    break;
                case "RESTAPI":
                    rbtnRESTAPI.Checked = true;
                    rbtnRESTAPI_CheckedChanged(sender, e);
                    break;
                case "Sharp7":
                    rbtnSharp7.Checked = true;
                    rbtnSharp7_CheckedChanged(sender, e);
                    break;
                default:
                    break;
            }

            if (internalVariables.communicationThreadRunningFlag == true)
            {
                // UI settings
                #region UI settings
                rbtnOPCUA.Enabled = false;
                rbtnMQTT.Enabled = false;
                rbtnTCPIP.Enabled = false;
                rbtnModbusTCPIP.Enabled = false;
                rbtnRESTAPI.Enabled = false;
                //rbtnS7.Enabled = false;
                rbtnSharp7.Enabled = false;

                checkBoxMaster.Enabled = false;
                checkBoxSlave.Enabled = false;

                txtBoxPara1.Enabled = false;
                txtBoxPara2.Enabled = false;

                btnPreSet.Enabled = false;

                btnStartCommunicationThread.Enabled = false;
                btnStopCommunicationThread.Enabled = true;

                #endregion
            }
        }

        // radio buttons 
        #region radio buttons 
        private void rbtnOPCUA_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "OPC UA selected.";

            internalVariables.communicationFlag = "OPCUA";
            /*
            internalVariables.opcuaFlag = true;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 
            // btns 
            btnStartCommunicationThread.Visible = true;
            btnStartCommunicationThread.Enabled = true;

            btnStopCommunicationThread.Visible = true;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = true;
            btnPreSet.Enabled = true;

            lblCommunicationStatus.Visible = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "URL:";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type URL";

            lblPara2.Visible = false;
            lblPara2.Enabled = false;
            lblPara2.Text = "";
            txtBoxPara2.Visible = false;
            txtBoxPara2.Enabled = false;
            txtBoxPara2.Text = "";

            lblCheckBox.Visible = true;
            lblCheckBox.Enabled = true;
            lblCheckBox.Text = "What is this device?";

            checkBoxMaster.Visible = true;
            checkBoxMaster.Enabled = true;
            checkBoxMaster.Text = "Master";
            checkBoxMaster.Checked = false;

            checkBoxSlave.Visible = true;
            checkBoxSlave.Enabled = true;
            checkBoxSlave.Text = "Klient";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnMQTT_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "MQTT selected.";

            internalVariables.communicationFlag = "MQTT";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = true;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunicationThread.Visible = true;
            btnStartCommunicationThread.Enabled = true;

            btnStopCommunicationThread.Visible = true;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = true;
            btnPreSet.Enabled = true;

            lblCommunicationStatus.Visible = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "Broker: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type broker address";

            lblPara2.Visible = true;
            lblPara2.Enabled = true;
            lblPara2.Text = "Topic: ";
            txtBoxPara2.Visible = true;
            txtBoxPara2.Enabled = true;
            txtBoxPara2.Text = "Type topic address";

            // check box
            lblCheckBox.Visible = true;
            lblCheckBox.Enabled = false;
            lblCheckBox.Text = "What is this device?";

            checkBoxMaster.Visible = true;
            checkBoxMaster.Enabled = false;
            checkBoxMaster.Text = "Server/Broker";
            checkBoxMaster.Checked = true; // PC is always Broker, PLC cannot be broker

            checkBoxSlave.Visible = true;
            checkBoxSlave.Enabled = false;
            checkBoxSlave.Text = "Subscriber";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "TCP/IP selected.";

            internalVariables.communicationFlag = "TCPIP";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = true;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 

            // btns
            btnStartCommunicationThread.Visible = true;
            btnStartCommunicationThread.Enabled = true;

            btnStopCommunicationThread.Visible = true;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = true;
            btnPreSet.Enabled = true;

            lblCommunicationStatus.Visible = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "IP address: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type IP address";

            lblPara2.Visible = false;
            lblPara2.Enabled = false;
            lblPara2.Text = "";
            txtBoxPara2.Visible = false;
            txtBoxPara2.Enabled = false;
            txtBoxPara2.Text = "";

            // check box
            lblCheckBox.Visible = true;
            lblCheckBox.Enabled = false;
            lblCheckBox.Text = "What is this device?";

            checkBoxMaster.Visible = true;
            checkBoxMaster.Enabled = true;
            checkBoxMaster.Text = "Server";
            checkBoxMaster.Checked = false;

            checkBoxSlave.Visible = true;
            checkBoxSlave.Enabled = true;
            checkBoxSlave.Text = "Klient";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnModbusTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Modbus TCP/IP selected.";

            internalVariables.communicationFlag = "ModbusTCPIP";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = true;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunicationThread.Visible = true;
            btnStartCommunicationThread.Enabled = true;

            btnStopCommunicationThread.Visible = true;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = true;
            btnPreSet.Enabled = true;

            lblCommunicationStatus.Visible = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "IP address: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type IP address";

            lblPara2.Visible = true;
            lblPara2.Enabled = true;
            lblPara2.Text = "TCP port: ";
            txtBoxPara2.Visible = true;
            txtBoxPara2.Enabled = true;
            txtBoxPara2.Text = "Type TCP port";

            // check box
            // check box
            lblCheckBox.Visible = true;
            lblCheckBox.Enabled = true;
            lblCheckBox.Text = "What is this device?";

            checkBoxMaster.Visible = true;
            checkBoxMaster.Enabled = true;
            checkBoxMaster.Text = "Master";
            checkBoxMaster.Checked = false;

            checkBoxSlave.Visible = true;
            checkBoxSlave.Enabled = true;
            checkBoxSlave.Text = "Slave";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnRESTAPI_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "REST API selected. Server is already running on " + internalVariables.communicationDataURL + ".";

            internalVariables.communicationFlag = "RESTAPI";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = true;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunicationThread.Visible = false;
            btnStartCommunicationThread.Enabled = false;

            btnStopCommunicationThread.Visible = false;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = false;
            btnPreSet.Enabled = false;

            lblCommunicationStatus.Visible = false;
            lblCommunicationStatus.Text = "";

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = false;
            lblPara1.Text = "URL: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = false;
            txtBoxPara1.Text = internalVariables.communicationDataURL;

            lblPara2.Visible = true;
            lblPara2.Enabled = false;
            lblPara2.Text = "Server is already running.";
            txtBoxPara2.Visible = false;
            txtBoxPara2.Enabled = false;
            txtBoxPara2.Text = "";

            // check box
            lblCheckBox.Visible = false;
            lblCheckBox.Enabled = false;
            lblCheckBox.Text = "";

            checkBoxMaster.Visible = false;
            checkBoxMaster.Enabled = false;
            checkBoxMaster.Text = "";
            checkBoxMaster.Checked = false;

            checkBoxSlave.Visible = false;
            checkBoxSlave.Enabled = false;
            checkBoxSlave.Text = "";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnS7_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "S7 selected.";

            internalVariables.communicationFlag = "S7";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = true;
            internalVariables.sharp7Flag = false;
            */

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunicationThread.Visible = true;
            btnStartCommunicationThread.Enabled = true;

            btnStopCommunicationThread.Visible = true;
            btnStopCommunicationThread.Enabled = false;

            btnPreSet.Visible = true;
            btnPreSet.Enabled = true;

            lblCommunicationStatus.Visible = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "IP address: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type IP address";

            lblPara2.Visible = false;
            lblPara2.Enabled = false;
            lblPara2.Text = "";
            txtBoxPara2.Visible = false;
            txtBoxPara2.Enabled = false;
            txtBoxPara2.Text = "";

            // check box
            lblCheckBox.Visible = false;
            lblCheckBox.Enabled = false;
            lblCheckBox.Text = "";

            checkBoxMaster.Visible = false;
            checkBoxMaster.Enabled = false;
            checkBoxMaster.Text = "";
            checkBoxMaster.Checked = false;

            checkBoxSlave.Visible = false;
            checkBoxSlave.Enabled = false;
            checkBoxSlave.Text = "";
            checkBoxSlave.Checked = false;

            #endregion
        }

        private void rbtnSharp7_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Sharp7 selected";

            internalVariables.communicationFlag = "Sharp7";
            /*
            internalVariables.opcuaFlag = false;
            internalVariables.mqttFlag = false;
            internalVariables.tcpipFlag = false;
            internalVariables.restapiFlag = false;
            internalVariables.modbustcpipFlag = false;
            internalVariables.s7Flag = false;
            internalVariables.sharp7Flag = true;
            */

            if (internalVariables.communicationThreadRunningFlag == false)
            {
                // UI settings 
                #region UI settings 

                // btns 
                btnStartCommunicationThread.Visible = true;
                btnStartCommunicationThread.Enabled = true;

                btnStopCommunicationThread.Visible = true;
                btnStopCommunicationThread.Enabled = false;

                btnPreSet.Visible = true;
                btnPreSet.Enabled = true;

                lblCommunicationStatus.Visible = true;

                // para
                lblPara1.Visible = true;
                lblPara1.Enabled = true;
                lblPara1.Text = "IP address: ";
                txtBoxPara1.Visible = true;
                txtBoxPara1.Enabled = true;
                txtBoxPara1.Text = "Type IP address";

                lblPara2.Visible = false;
                lblPara2.Enabled = false;
                lblPara2.Text = "";
                txtBoxPara2.Visible = false;
                txtBoxPara2.Enabled = false;
                txtBoxPara2.Text = "";

                // check box
                lblCheckBox.Visible = false;
                lblCheckBox.Enabled = false;
                lblCheckBox.Text = "";

                checkBoxMaster.Visible = false;
                checkBoxMaster.Enabled = false;
                checkBoxMaster.Text = "";
                checkBoxMaster.Checked = false;

                checkBoxSlave.Visible = false;
                checkBoxSlave.Enabled = false;
                checkBoxSlave.Text = "";
                checkBoxSlave.Checked = false;

                #endregion
            }
        }

        #endregion

        private void btnStartCommunicationThread_Click(object sender, EventArgs e)
        {
            lblCommunicationStatus.Text = "Starting communication.";
            lblStatus.Text = "Starting communication.";

            // starting communication thread
            if (internalVariables.communicationThread == null || !internalVariables.communicationThread.IsAlive)
            {
                switch (internalVariables.communicationFlag)
                {
                    case "MQTT":
                        lblCommunicationStatus.Text = "MQTT communication started.";
                        lblStatus.Text = "MQTT communication started.";

                        if (internalVariables.checkBoxMaster == true)
                        {
                            // start broker (server)

                            // create client and connect to broker
                        }
                        else if (internalVariables.checkBoxSlave == true)
                        {
                            // create client and connect to broker
                        }
                        else
                        {
                            // no checkbox selected 
                            lblStatus.Text = $"Error: Please select Master or Slave mode for Modbus TCP/IP.";
                            return; // break;
                        }

                        break;
                    case "OPCUA":
                        lblCommunicationStatus.Text = "OPC UA communication started.";
                        lblStatus.Text = "OPC UA communication started.";

                        break;
                    case "TCPIP":
                        lblCommunicationStatus.Text = "TCP/IP communication started.";
                        lblStatus.Text = "TCP/IP communication started.";

                        string ipAddress = internalVariables.txtBoxParam1;

                        // connect 
                        bool connect = _tcpip.Connect();

                        if (connect == true)
                        {
                            lblStatus.Text = $"TCPIP connected to {ipAddress}.";
                        }
                        else
                        {
                            lblStatus.Text = $"TCPIP connection to {ipAddress} failed.";
                            return; // break;
                        }

                        break;
                    case "ModbusTCPIP":
                        lblCommunicationStatus.Text = "Modbus TCP/IP communication started.";
                        lblStatus.Text = "MModbus TCP/IPQTT communication started.";

                        ModbusTCPIP_ipaddress = internalVariables.txtBoxParam1;

                        if (!int.TryParse(internalVariables.txtBoxParam2, out ModbusTCPIP_port))
                        {
                            // error port not valid number 
                            lblStatus.Text = $"Error: Modbus TCP/IP port is not a valid number.";
                            return;
                        }

                        if (internalVariables.checkBoxMaster == true)
                        {
                            //ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ModbusTCPIP_ipAddress, txpPort);

                            _modbusMaster.ipAddress = ModbusTCPIP_ipaddress;
                            _modbusMaster.port = ModbusTCPIP_port;

                            bool connectToSlave = _modbusMaster.ConnectToSlave();

                            if (connectToSlave == true)
                            {
                                lblStatus.Text = $"Modbus TCP/IP connected to {ModbusTCPIP_ipaddress}:{ModbusTCPIP_port}.";
                            }
                            else
                            {
                                lblStatus.Text = $"Modbus TCP/IP connection to {ModbusTCPIP_ipaddress}:{ModbusTCPIP_port} failed.";
                                return; // break;
                            }
                        }
                        else if (internalVariables.checkBoxSlave == true)
                        {
                            //ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ModbusTCPIP_ipAddress, txpPort);
                            _modbusSlave.Start();
                            lblStatus.Text = $"Modbus TCP/IP Slave mode started.";
                        }
                        else
                        {
                            // no checkbox selected
                            lblStatus.Text = $"Error: Please select Master or Slave mode for Modbus TCP/IP.";
                            return; // break;
                        }


                        break;
                    case "RESTAPI":
                        lblCommunicationStatus.Text = "REST API communication started.";
                        lblStatus.Text = "REST API communication started.";

                        break;
                    case "Sharp7":
                        lblCommunicationStatus.Text = "Sharp7 communication started.";
                        lblStatus.Text = "Sharp7 communication started.";

                        string Sharp7_ipAddress = internalVariables.txtBoxParam1;

                        if (_sharp7.client.Connected == false)
                        {
                            int plcConnect = _sharp7.connectToPLC(Sharp7_ipAddress);

                            if (plcConnect == 0)
                            {
                                lblStatus.Text = $"PLC connected successfully.";
                            }
                            else
                            {
                                lblStatus.Text = $"Error in Sharp7 communication. ConnectToPLC returns {plcConnect}.";
                                return; // break;
                            }
                        }
                        else
                        {
                            lblStatus.Text = "Sharp7 client is already connected.";
                        }

                        break;
                    /*case "S7":
                        lblCommunicationStatus.Text = "S7 communication started.";
                        lblStatus.Text = "S7 communication started.";

                        break;*/
                    default:
                        lblCommunicationStatus.Text = "No communication protocol selected.";
                        lblStatus.Text = "No communication protocol selected.";

                        break;
                }
            }

            internalVariables.communicationThreadRunningFlag = true;
            var communicationManager = new CommunicationManager(this);
            internalVariables.communicationThread = new Thread(communicationManager.Communication);
            internalVariables.communicationThread.IsBackground = true;
            internalVariables.communicationThread.Start();

            // UI 
            #region UI
            rbtnOPCUA.Enabled = false;
            rbtnMQTT.Enabled = false;
            rbtnTCPIP.Enabled = false;
            rbtnModbusTCPIP.Enabled = false;
            rbtnRESTAPI.Enabled = false;
            //rbtnS7.Enabled = false;
            rbtnSharp7.Enabled = false;

            checkBoxMaster.Enabled = false;
            checkBoxSlave.Enabled = false;

            txtBoxPara1.Enabled = false;
            txtBoxPara2.Enabled = false;

            btnPreSet.Enabled = false;

            btnStartCommunicationThread.Enabled = false;
            btnStopCommunicationThread.Enabled = true;

            #endregion
        }

        private void btnStopCommunicationThread_Click(object sender, EventArgs e)
        {
            lblCommunicationStatus.Text = "Stoppping communication.";
            lblStatus.Text = "Stopping communication.";

            switch (internalVariables.communicationFlag)
            {
                case "MQTT":
                    if (internalVariables.checkBoxMaster == true)
                    {
                        // disconnect client

                        // stop broker (server)

                    }
                    else if (internalVariables.checkBoxSlave == true)
                    {
                        // disconnect client
                    }
                    else
                    {
                        // no checkbox selected 
                        lblStatus.Text = $"Error: Please select Master or Slave mode for Modbus TCP/IP.";
                        return; // break;
                    }

                    lblCommunicationStatus.Text = "MQTT communication stopped.";
                    lblStatus.Text = "MQTT communication stopped.";

                    break;
                case "OPCUA":
                    break;
                case "ModbusTCPIP":
                    if (internalVariables.checkBoxMaster == true)
                    {
                        //ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ModbusTCPIP_ipaddress, ModbusTCPIP_port);
                        bool connectedModbusSlave = _modbusMaster.DisconnectFromSlave();

                        if (connectedModbusSlave == true)
                        {
                            lblStatus.Text = $"Modbus TCP/IP disconnected successfully.";
                        }
                        else
                        {
                            lblStatus.Text = $"Error in Modbus TCP/IP disconnection.";
                        }
                    }
                    else if (internalVariables.checkBoxSlave == true)
                    {
                        //ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ModbusTCPIP_ipaddress, ModbusTCPIP_port);
                        bool connectedModbusServer = _modbusSlave.Stop();

                        if (connectedModbusServer == true)
                        {
                            lblStatus.Text = $"Modbus TCP/IP Slave stopped successfully.";
                        }
                        else
                        {
                            lblStatus.Text = $"Error in Modbus TCP/IP Slave stopping.";
                        }
                    }
                    else
                    {
                        lblStatus.Text = $"Error: Please select Master or Slave mode for Modbus TCP/IP.";
                        return;
                    }

                    break;
                case "TCPIP":
                    if (_tcpip.socket == null)
                    {
                        lblStatus.Text = "TCP/IP socket is null, cannot disconnect.";
                        break; // return;
                    }
                    else
                    {
                        bool disconnect = _tcpip.Disconnect();
                        if (disconnect == true)
                        {
                            lblStatus.Text = "TCP/IP disconnected successfully.";
                        }
                        else
                        {
                            lblStatus.Text = "Error in TCP/IP disconnection.";
                        }
                    }

                    break;
                case "RESTAPI":
                    // 
                    break;
                case "Sharp7":
                    string Sharp7_ipAddress = internalVariables.txtBoxParam1;

                    if (_sharp7.client.Connected == true)
                    {
                        int plcConnect = _sharp7.disconnectFromPLC();

                        if (plcConnect == 0)
                        {
                            lblStatus.Text = ($"PLC connected successfully.");
                        }
                        else
                        {
                            lblStatus.Text = ($"Error in Sharp7 communication. ConnectToPLC returns {plcConnect}.");
                        }
                    }
                    else
                    {
                        lblStatus.Text = "Sharp7 client is not connected, cannot disconnect.";
                    }

                        break;
            }

            // UI 
            #region UI 
            rbtnOPCUA.Enabled = true;
            rbtnMQTT.Enabled = true;
            rbtnTCPIP.Enabled = true;
            rbtnModbusTCPIP.Enabled = true;
            rbtnRESTAPI.Enabled = true;
            //rbtnS7.Enabled = true;
            rbtnSharp7.Enabled = true;

            checkBoxMaster.Enabled = true;
            checkBoxSlave.Enabled = true;

            txtBoxPara1.Enabled = true;
            txtBoxPara2.Enabled = true;

            btnPreSet.Enabled = true;

            btnStartCommunicationThread.Enabled = true;
            btnStopCommunicationThread.Enabled = false;

            #endregion

            // stoping communication thread
            if (internalVariables.communicationThread != null && internalVariables.communicationThread.IsAlive)
            {
                lblCommunicationStatus.Text = $"Communication stopped. Communication thread isAlive: {internalVariables.communicationThread.IsAlive}";
                lblStatus.Text = "Communication stopped.";

                internalVariables.communicationThreadRunningFlag = false;
                internalVariables.communicationThread.Join(); // Počká na ukončení vlákna
            }
        }

        private void btnPreSet_Click(object sender, EventArgs e)
        {
            if (rbtnOPCUA.Checked == true)
            {
                // URL 
                txtBoxPara1.Text = "";
            }
            else if (rbtnMQTT.Checked == true)
            {
                // Broker 
                txtBoxPara1.Text = "";
                // Topic 
                txtBoxPara2.Text = "";
            }
            else if (rbtnTCPIP.Checked == true)
            {
                // IP address 
                txtBoxPara1.Text = "192.168.0.1";
            }
            else if (rbtnModbusTCPIP.Checked == true)
            {
                // IP address 
                txtBoxPara1.Text = "";
                // TCP port
                txtBoxPara2.Text = "502";
            }
            else if (rbtnRESTAPI.Checked == true)
            {
                // URL 
                txtBoxPara1.Text = "http://192.168.0.1/api/crossroad";
            }
            /*
            else if (rbtnS7.Checked == true)
            {
                // IP address
                txtBoxPara1.Text = "192.168.0.1";
            }
            */
            else if (rbtnSharp7.Checked == true)
            {
                // IP address
                txtBoxPara1.Text = "192.168.0.1";
            }
            else
            {
                // Error -> preset is not checked 
                return;
            }
        }

        private void checkBoxMaster_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxSlave.Checked = false;
            //checkBoxMaster.Checked = true;
            internalVariables.checkBoxMaster = true;
            internalVariables.checkBoxSlave = false;
        }

        private void checkBoxSlave_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxMaster.Checked = false;
            //checkBoxSlave.Checked = true;
            internalVariables.checkBoxMaster = false;
            internalVariables.checkBoxSlave = true;
        }

        public void SetStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => lblStatus.Text = message));
            }
            else
            {
                lblStatus.Text = message;
            }
        }

        private void txtBoxPara1_TextChanged(object sender, EventArgs e)
        {
            internalVariables.txtBoxParam1 = txtBoxPara1.Text;
            /*
            switch (internalVariables.communicationFlag)
            {
                case "MQTT":
                    break;
                case "OPCUA":
                    break;
                case "ModbusTCPIP":
                    break;
                case "TCPIP":
                    break;
                case "RESTAPI":
                    break;
                case "Sharp7":
                    internalVariables.txtBoxParam1 = txtBoxPara1.Text;
                    break;
            }
            */
        }

        private void txtBoxPara2_TextChanged(object sender, EventArgs e)
        {
            internalVariables.txtBoxParam2 = txtBoxPara1.Text;
        }

        private void btnActualCrossroaddata_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            var properties = typeof(CrossroadData).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            foreach (var prop in properties)
            {
                try
                {
                    var name = prop.Name;
                    var value = prop.GetValue(null); // null = protože static
                    listBox1.Items.Add($"{name}: {value}");
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add($"{prop.Name}: <error reading> ({ex.Message})");
                }
            }

            var fields = typeof(CrossroadData).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                var name = field.Name;
                var value = field.GetValue(null);
                listBox1.Items.Add($"{name}: {value}");
            }
        }
    }
}
