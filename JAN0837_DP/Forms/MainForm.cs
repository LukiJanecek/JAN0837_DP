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

using JAN0837_DP.Forms;
using JAN0837_DP.Data;
using JAN0837_DP.Communication;

namespace JAN0837_DP
{
    public partial class MainForm : Form
    {
        // Queues 
        public ConcurrentQueue<int> dataQueueIN = new ConcurrentQueue<int>();
        public ConcurrentQueue<int> dataQueueOUT = new ConcurrentQueue<int>();

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

            btnCommunicationControl.Visible = true;
            btnCommunicationControl.Enabled = true;
            
            btnGenerateTIATemplate.Visible = true;
            btnGenerateTIATemplate.Enabled = true;

            btnLocalHost.Visible = true;
            btnLocalHost.Enabled = true;
                        
            #endregion

            // starting visualization thread
            if (internalVariables.visualizationThread == null || !internalVariables.visualizationThread.IsAlive)
            {
                internalVariables.visualizationRunningFlag = true;
                internalVariables.visualizationThread = new Thread(Visualization);
                internalVariables.visualizationThread.Start();
            }
        }

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
            string clientFolder = Path.Combine("JAN0837_react", "JAN0837_react.client");
            string clientFile = Path.Combine(clientFolder, "JAN0837_react.CLient.csproj");
            
            string fullServerFilePath = Path.Combine(parentDirectory, serverFile);
            string fullClientFilePath = Path.Combine(parentDirectory, clientFile);

            mainWindow.Controls.Clear();
            var visual = new ucLocalhost();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);

            // Vizualization
            try
            {
                if (internalVariables.serverStarted == false)
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
                            internalVariables.localhosturl = ea.Data.Split(new[] { "Now listening on:" }, StringSplitOptions.None)[1].Trim();

                            lblStatus.Text = $"Now listening on {internalVariables.localhosturl}";

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = internalVariables.localhosturl, // $"{localhosturl}/app"
                                UseShellExecute = true
                            });

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"{internalVariables.localhosturl}/swagger/index.html",
                                UseShellExecute = true
                            });
                        }
                    };
                    serverProc.BeginOutputReadLine();
                    serverProc.BeginErrorReadLine();

                    internalVariables.serverStarted = true;
                }
                else
                {
                    lblStatus.Text = $"Openning {internalVariables.localhosturl} in browser";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = internalVariables.localhosturl, // $"{localhosturl}/app"
                        UseShellExecute = true
                    });

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{internalVariables.localhosturl}/swagger/index.html",
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

        public void Visualization()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
    }
}
