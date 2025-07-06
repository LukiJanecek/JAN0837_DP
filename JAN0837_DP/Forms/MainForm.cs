// System Libraries 
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO.Ports;
using System.Security.Policy;
using System.Net.Http;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

// Communication Libraries 
// OPCUA
using Opc;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua.Buffers;
using Opc.Ua.Export;
using Opc.Ua.Security;
using JAN0837_DP.Communication.OPCUA;

    // Modbus
using Modbus;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Data;
using Modbus.Extensions;
using Modbus.IO;
using Modbus.Message;
using JAN0837_DP.Communication.ModbusTCPIP;

    // MQTT
using MQTTnet;
//using MQTTnet.Server;
//using MQTTnet.Client;
//using MQTTnet.Client.Options;

// REST API 


//TCP/IP
using JAN0837_DP.Communication.TCPIP;

// Sharp7
using Sharp7;

// Additional Libraries 
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using JAN0837_DP.Forms; // JSON library
//using Microsoft.AspNetCore.Hosting; // 

namespace JAN0837_DP
{
    public partial class MainForm : Form
    {
        // Threads 
        private Thread communicationThread;
        private Thread visualizationThread;
        private string localhosturl;
        private bool serverStarted = false;

        // FLags 
        // Thread Flags
        private bool communicationRunningFlag = false;
        private bool visualizationRunningFlag = false;
        // Communication Flags
        private bool opcuaFlag = false;
        private bool mqttFlag = false;
        private bool tcpipFlag = false;
        private bool restapiFlag = false;
        private bool modbustcpipFlag = false;
        private bool s7Flag = false;
        //
        public bool connected = false;

        // Queues 
        private ConcurrentQueue<int> dataQueueIN = new ConcurrentQueue<int>();
        private ConcurrentQueue<int> dataQueueOUT = new ConcurrentQueue<int>();

        // 
        Process serverProcess = null;

        //Paths
        public static string projectRootPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\"));
        public static string dataDirectoryPath = Path.Combine(projectRootPath, "Data");

        // Lists 

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
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

            rbtnS7.Enabled = true;
            rbtnS7.Visible = true;
            rbtnS7.Checked = false;

            rbtnOPCUA.Tag = "OPCUA";
            rbtnMQTT.Tag = "MQTT";
            rbtnTCPIP.Tag = "TCPIP";
            rbtnModbusTCPIP.Tag = "ModbusTCPIP";
            rbtnRESTAPI.Tag = "RestApi";
            rbtnS7.Tag = "S7";

            // btns
            btnStartCommunication.Visible = false;
            btnStartCommunication.Enabled = false;

            btnStopCommunication.Visible = false;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = false;
            btnUsePreset.Enabled = false;

            btnGenerateTIATemplate.Visible = true;
            btnGenerateTIATemplate.Enabled = true;

            btnLocalHost.Visible = true;
            btnLocalHost.Enabled = true;

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

            #endregion

            // Files 

            // starting visualization thread
            if (visualizationThread == null || !visualizationThread.IsAlive)
            {
                visualizationRunningFlag = true;
                visualizationThread = new Thread(Visualization);
                visualizationThread.Start();
            }
        }

        // Threads Methods 
        #region 

        private void Communication()
        {
            try
            {
                while (communicationRunningFlag)
                {
                    if (rbtnOPCUA.Checked == true)
                    {
                        //OPCUA();

                        string opcUaServerUrl = txtBoxPara1.Text;

                        if (checkBoxMaster.Checked == true)
                        {

                        }
                        else if (checkBoxSlave.Checked == true)
                        {

                        }
                        else
                        {
                            // no checkbox selected 
                        }
                    }
                    else if (rbtnMQTT.Checked == true)
                    {
                        //MQTT();

                        string brokerAddress = txtBoxPara1.Text;
                        string secondPara = txtBoxPara2.Text;

                        if (checkBoxMaster.Checked == true)
                        {

                        }
                        else if (checkBoxSlave.Checked == true)
                        {

                        }
                        else
                        {
                            // no checkbox selected 
                        }
                    }
                    else if (rbtnTCPIP.Checked == true)
                    {
                        //TCPIP();

                        string ipAddress = txtBoxPara1.Text;
                    }
                    else if (rbtnModbusTCPIP.Checked == true)
                    {
                        //ModbusTCPIP();

                        string ipAddress = txtBoxPara1.Text;
                        string txtPort = txtBoxPara2.Text;
                        int txpPort;

                        if (!int.TryParse(txtPort, out txpPort))
                        {
                            // error port not valid number 
                            return;
                        }

                        if (checkBoxMaster.Checked == true)
                        {
                            ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ipAddress, txpPort);

                            if (modbusClient.ConnectToSlave())
                            {
                                byte slaveId = 1;
                                ushort startAddress = 0;

                                // Čtení jednoho registru
                                ushort[] values = modbusClient.ReadHoldingRegisters(slaveId, startAddress, 1);
                                if (values != null)
                                    Console.WriteLine($"📥 Přečtená hodnota: {values[0]}");

                                // Zápis do registru
                                modbusClient.WriteSingleRegister(slaveId, startAddress, 1234);

                                // Odpojení
                                modbusClient.DisconnectFromSlave();
                            }
                        }
                        else if (checkBoxSlave.Checked == true)
                        {
                            ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ipAddress, txpPort);
                            modbusServer.Start(); // Spustíme Modbus Slave

                            // Simulace změny hodnoty registru
                            modbusServer.SetRegisterValue(0, 1234);

                            Console.ReadLine(); // Čekáme, dokud uživatel nestiskne Enter
                            modbusServer.Stop(); // Ukončení serveru
                        }
                        else
                        {
                            // no checkbox selected 
                        }
                    }
                    else if (rbtnRESTAPI.Checked == true)
                    {
                        //RESTAPI();

                        string url = txtBoxPara1.Text;
                    }
                    else if (rbtnS7.Checked == true)
                    {
                        // S7 -> Sharp7
                        string ipAddress = txtBoxPara1.Text;
                    }
                    else
                    {
                        // Error -> neni zaklikla predvolba 
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Visualization()
        {
            try
            {
                while (visualizationRunningFlag)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        // Communication Methods
        #region Communication Methods

        // OPCUA
        #region OPCUA 
        /*
        private void OPCUA(string opcUaServerUrl)
        {
            try
            {
                using (var client = new OpcClient(opcUaServerUrl))
                {
                    client.Connect();
                    //Console.WriteLine($"✅ Připojeno k OPC UA serveru: {opcUaServerUrl}");

                    // 2️⃣ Procházení dostupných uzlů
                    BrowseNodes(client, OpcObjectTypes.ObjectsFolder);

                    // 3️⃣ Čtení hodnoty konkrétního tagu
                    ReadTagValue(client, "ns=2;s=MyTag"); // Změň na vlastní OPC UA tag
                }
            }
            catch (Exception ex)
            {

            }
        }
        
        static void BrowseNodes(OpcClient client, string nodeId)
        {
            //Console.WriteLine("📋 Seznam dostupných uzlů:");

            foreach (var node in client.BrowseNode(nodeId).Children())
            {
                //Console.WriteLine($"  - {node.NodeId} ({node.DisplayName})");
            }
        }

        static void ReadTagValue(OpcClient client, string tagNodeId)
        {
            try
            {
                var value = client.ReadNode(tagNodeId);
                //Console.WriteLine($"📊 Tag [{tagNodeId}] = {value}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"⚠️ Chyba při čtení tagu [{tagNodeId}]: {ex.Message}");
            }
        }
        */
        #endregion

        //MQTT
        #region MQTT 
        /*
        private void MQTT(string broker, string topic, int port)
        {
            try
            {
                var factory = new MqttFactory();
                using var mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // TCP připojení na defaultní port
                .WithClientId("MqttClientExample")
                .WithCleanSession()
                .Build();

                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    Console.WriteLine($"[Přijato] {e.ApplicationMessage.Topic}: {message}");
                });

                mqttClient.UseConnectedHandler(async e =>
                {
                    Console.WriteLine("✅ Připojeno k MQTT brokeru.");

                    // Přihlášení k tématu
                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic(topic)
                        .Build());
                    Console.WriteLine($"📩 Odběr tématu: {topic}");

                    // Odeslání testovací zprávy
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload("Hello from MQTTnet!")
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await mqttClient.PublishAsync(message);
                    Console.WriteLine("📤 Zpráva odeslána!");

                    // Připojení
                    await mqttClient.ConnectAsync(options, CancellationToken.None);

                    // Počkej na ukončení (Ctrl+C)
                    Console.WriteLine("Stiskni Enter pro ukončení...");
                    Console.ReadLine();

                    // Odpojení klienta
                    await mqttClient.DisconnectAsync();
                    Console.WriteLine("❌ Odpojeno.");
                });

            }
            catch (Exception ex)
            {

            }
        }
        */
        #endregion

        // TCPIP
        #region TCPIP 
        private void TCPIP(string IPAddress)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        //Modbus TCP/IP
        #region Modbus TCP/IP 
        private void ModbusTCPIP(string IPAddress, int TCPport)
        {
            try
            {
                using (TcpClient client = new TcpClient(IPAddress, TCPport))
                {
                    // Vytvoření Modbus TCP master zařízení
                    ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                    ushort startAddress = 0; // Adresa registru
                    ushort numRegisters = 1; // Počet registrů ke čtení

                    // Čtení hodnoty z Modbus registru
                    ushort[] registers = master.ReadHoldingRegisters(1, startAddress, numRegisters);
                    Console.WriteLine($"Hodnota na adrese {startAddress}: {registers[0]}");

                    // Zápis hodnoty do Modbus registru
                    ushort newValue = 1234;
                    master.WriteSingleRegister(1, startAddress, newValue);
                    Console.WriteLine($"Zapsaná hodnota {newValue} na adresu {startAddress}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        // REST API 
        #region REST API 
        private void RESTAPI(string url)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        // S7 
        #region S7 

        private void S7(string ipAddress)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #endregion

        // toolStripMain components
        #region toolStripMain components
        private void btnGenerateTIATemplate_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Generating template to TIA Portal V19.";

            mainWindow.Controls.Clear();
            var visual = new ucGenerateTIAtemplate();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);
        }

        private void btnCommunicationControl_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Openning communication control.";

            mainWindow.Controls.Clear();
            var visual = new ucCommunicationControl();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);
        }

        private void btnLocalHost_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Starting React dev server and ASP.NET server…"; // Openning localhost in browser.

            string parentDirectory = Directory.GetParent(Directory.GetParent(projectRootPath).FullName).FullName;
            string serverFolder = Path.Combine("JAN0837_react", "JAN0837_react.Server");
            string serverFile = Path.Combine(serverFolder, "JAN0837_react.Server.csproj"); // "JAN0837_react.Server.csproj.user"
            string clientFolder = Path.Combine("JAN0837_react", "JAN0837_react.Client");
            string clientFile = Path.Combine(clientFolder, "JAN0837_react.CLient.csproj");
            
            string fullServerFilePath = Path.Combine(parentDirectory, serverFile);
            string fullClientFilePath = Path.Combine(parentDirectory, clientFile);

            try
            {
                if (serverStarted == false)
                {
                    lblStatus.Text = "Starting React dev server";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "run dev",
                        WorkingDirectory = clientFolder,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });

                    // starting .NET server
                    lblStatus.Text = "Starting .NET server";
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{fullServerFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var serverProc = Process.Start(psi);

                    serverProc.OutputDataReceived += (s, ea) =>
                    {
                        if (ea.Data != null && ea.Data.Contains("Now listening on:"))
                        {
                            localhosturl = ea.Data.Split(new[] { "Now listening on:" }, StringSplitOptions.None)[1].Trim();

                            lblStatus.Text = $"Now listening on {localhosturl}";

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = localhosturl, // $"{localhosturl}/app"
                                UseShellExecute = true
                            });

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"{localhosturl}/swagger/index.html",
                                UseShellExecute = true
                            });
                        }
                    };
                    serverProc.BeginOutputReadLine();
                    serverProc.BeginErrorReadLine();

                    serverStarted = true;
                }
                else
                {
                    lblStatus.Text = $"Openning {localhosturl} in browser";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = localhosturl, // $"{localhosturl}/app"
                        UseShellExecute = true
                    });

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{localhosturl}/swagger/index.html",
                        UseShellExecute = true
                    });
                }
                
                // starting React dev server
              
                /*
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                Process.Start(new ProcessStartInfo
                {
                    FileName = $"{url}/swagger/index.html",
                    UseShellExecute = true
                });
                */
                /*
                //starting ASP:NET project 
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{fullServerFilePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(processInfo);

                var urlSwagger = "http://localhost:5203/swagger/index.html";

                //starting localhost swagger window in browser 
                Process.Start(new ProcessStartInfo
                {
                    FileName = urlSwagger,
                    UseShellExecute = true
                });

                var urlReact = "http://localhost:5203/app";

                //starting localhost react window in browser 
                Process.Start(new ProcessStartInfo
                {
                    FileName = urlReact,
                    UseShellExecute = true
                });

                
                processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{fullServerFilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true, // Přesměrování výstupu
                    CreateNoWindow = false
                };

                using (var process = Process.Start(processInfo))
                {
                    // Čtení výstupu a hledání URL
                    using (var reader = process.StandardOutput)
                    {
                        string? output;
                        while ((output = reader.ReadLine()) != null)
                        {
                            if (output.Contains("Server running on:"))
                            {
                                var url = output.Split(": ")[1];
                                // Otevření prohlížeče na správné URL
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = url,
                                    UseShellExecute = true
                                });
                                break;
                            }
                        }
                    }
                }

                // přímé spuštění klienta
                Process.Start(new ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "run dev",
                    WorkingDirectory = fullClientFilePath,
                    UseShellExecute = true
                });

                // spusteni dotnet server
                Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{fullServerFilePath}\"",
                    UseShellExecute = true
                });

                */

                lblStatus.Text = "Servers started. Check your browser."; // Localhost openend on ...

            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při spouštění:\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Exitting...";

            // threads stop 
            // communication stop

            Application.Exit();
        }

        #endregion

        private void btnStart_Click(object sender, EventArgs e)
        {
            lblCommunicationStatus.Text = "Starting communication.";
            lblStatus.Text = "Starting communication.";

            // starting communication thread
            if (communicationThread == null || !communicationThread.IsAlive)
            {
                communicationRunningFlag = true;
                communicationThread = new Thread(Communication);
                communicationThread.IsBackground = true;
                communicationThread.Start();
                lblCommunicationStatus.Text = "Communication started.";
                lblStatus.Text = "Communication started.";

            }

            // UI 
            #region UI
            rbtnOPCUA.Enabled = false;
            rbtnMQTT.Enabled = false;
            rbtnTCPIP.Enabled = false;
            rbtnModbusTCPIP.Enabled = false;
            rbtnRESTAPI.Enabled = false;
            rbtnS7.Enabled = false;

            checkBoxMaster.Enabled = false;
            checkBoxSlave.Enabled = false;

            txtBoxPara1.Enabled = false;
            txtBoxPara2.Enabled = false;

            btnUsePreset.Enabled = false;

            btnStartCommunication.Enabled = false;
            btnStopCommunication.Enabled = true;

            #endregion

            // tady by měli být ještě připojení .connect() pro dané komunikační protokoly
        }

        private void btnStopCommunication_Click(object sender, EventArgs e)
        {
            lblCommunicationStatus.Text = "Stoppping communication.";
            lblStatus.Text = "Stopping communication.";
            
            // stoping communication thread
            communicationRunningFlag = false;

            if (communicationThread != null && communicationThread.IsAlive)
            {
                communicationThread.Join(); // Počká na ukončení vlákna

                lblCommunicationStatus.Text = "Communication stopped.";
                lblStatus.Text = "Communication stopped.";
            }

            // UI 
            #region UI 
            rbtnOPCUA.Enabled = true;
            rbtnMQTT.Enabled = true;
            rbtnTCPIP.Enabled = true;
            rbtnModbusTCPIP.Enabled = true;
            rbtnRESTAPI.Enabled = true;
            rbtnS7.Enabled = true;

            checkBoxMaster.Enabled = true;
            checkBoxSlave.Enabled = true;

            txtBoxPara1.Enabled = true;
            txtBoxPara2.Enabled = true;

            btnUsePreset.Enabled = true;

            btnStartCommunication.Enabled = true;
            btnStopCommunication.Enabled = false;

            #endregion

            // tady by měli být ještě odpojení .disconnect() pro dané komunikační protokoly
        }

        // Preset 
        private void btnUsePreset_Click(object sender, EventArgs e)
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
                txtBoxPara1.Text = "";
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
                txtBoxPara1.Text = "";
            }
            else if (rbtnS7.Checked == true)
            {
                // IP address
                txtBoxPara1.Text = "";
            }
            else
            {
                // Error -> neni zaklikla predvolba 
                return;
            }
        }

        private void rbtnOPCUA_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "OPC UA selected.";

            // UI settings 
            #region UI settings 
            // btns 
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

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

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

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
            lblCheckBox.Enabled = true;
            lblCheckBox.Text = "What is this device?";

            checkBoxMaster.Visible = true;
            checkBoxMaster.Enabled = true;
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

            // UI settings 
            #region UI sttings 

            // btns
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

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

        private void rbtnModbusTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "Modbus TCP/IP selected.";

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

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
            lblStatus.Text = "REST API selected.";

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

            // para
            lblPara1.Visible = true;
            lblPara1.Enabled = true;
            lblPara1.Text = "URL: ";
            txtBoxPara1.Visible = true;
            txtBoxPara1.Enabled = true;
            txtBoxPara1.Text = "Type URL";

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

        private void rbtnS7_CheckedChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "S7 selected.";

            // UI settings 
            #region UI settings 

            // btns 
            btnStartCommunication.Visible = true;
            btnStartCommunication.Enabled = true;

            btnStopCommunication.Visible = true;
            btnStopCommunication.Enabled = false;

            btnUsePreset.Visible = true;
            btnUsePreset.Enabled = true;

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

        private void checkBoxMaster_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxSlave.Checked = false;
        }

        private void checkBoxSlave_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxMaster.Checked = false;
        }

        
    }
}
