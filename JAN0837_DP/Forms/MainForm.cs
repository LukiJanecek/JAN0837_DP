//
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
//
using Newtonsoft;
//
using Opc;
using Modbus;
using MQTTnet;
using System.Net.Sockets;
using Sharp7;
using System.Collections.Concurrent;

namespace JAN0837_DP
{
    public partial class MainForm : Form
    {
        // Threads 
        private Thread communicationThread;
        private Thread visualizationThread;

        // FLags 
        // Thread Flags
        private bool communicationFlag = false;
        private bool visualizationFlag = false;
        // Communication Flags
        private bool opcuaFlag = false;
        private bool mqttFalg = false;
        private bool tcpipFlag = false;
        private bool RestApiFlag = false;
        private bool modbusFlag = false;
        //
        private bool connected = false;

        // Queues 
        private ConcurrentQueue<int> dataQueueIN = new ConcurrentQueue<int>();
        private ConcurrentQueue<int> dataQueueOUT = new ConcurrentQueue<int>();

        // Lists 

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // rbtn
            rbtnModbusTCPIP.Enabled = true;
            rbtnModbusTCPIP.Visible = true;
            rbtnModbusTCPIP.Checked = false;

            rbtnMQTT.Enabled = true;
            rbtnMQTT.Visible = true;
            rbtnMQTT.Checked = false;

            rbtnOPCUA.Enabled = true;
            rbtnOPCUA.Visible = true;
            rbtnOPCUA.Checked = false;

            rbtnRESTAPI.Enabled = true;
            rbtnRESTAPI.Visible = true;
            rbtnRESTAPI.Checked = false;

            rbtnTCPIP.Enabled = true;
            rbtnTCPIP.Visible = true;
            rbtnTCPIP.Checked = false;

            //btns
            btnStart.Enabled = true;
            btnStart.Visible = true;

            btnGenerateTIATemplate.Enabled = false;
            btnGenerateTIATemplate.Visible = false;


        }

        // Threads Methods 
        #region

        private void Communication()
        {

        }

        private void Visualization()
        {

        }

        #endregion

        private void btnGenerateTIATemplate_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Generating template to TIA Portal V19.";
        }

        private void btnOpenLocalhost_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Openning localhost in browser.";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Starting communication.";
        }

        private void rbtnOPCUA_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "OPC UA selected.";
        }

        private void rbtnMQTT_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "MQTT selected.";
        }

        private void rbtnTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "TCP/IP selected.";
        }

        private void rbtnRESTAPI_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "REST API selected.";
        }

        private void rbtnModbusTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Modbus TCP/IP selected.";
        }

        
    }
}
