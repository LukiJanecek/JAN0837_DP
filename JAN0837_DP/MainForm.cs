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
        private Thread drawingThread;

        // FLags 
        private bool communicationFlag = false;
        private bool drawingFlag = false;

        // Queues 
        private ConcurrentQueue<int> dataQueue = new ConcurrentQueue<int>();

        // Lists 

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
