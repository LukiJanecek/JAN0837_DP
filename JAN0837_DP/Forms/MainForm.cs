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
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.Cmp;
using Microsoft.AspNetCore.Mvc;
using JAN0837_DP.ReactFE;

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

        public static string solutionRootPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
        public static string clientProjectDirectory = Path.Combine(solutionRootPath, "JAN0837_react/JAN0837_react.Client");

        private ReactFE.FEcommunicationControl _fe; 

        public MainForm()
        {
            InitializeComponent();

            _fe = new FEcommunicationControl(internalVariables.communicationURL);
            _fe.Start();
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
                    /*
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "run dev",
                        WorkingDirectory = clientProjectDirectory,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                    */


                    // starting React 
                    /*
                    lblStatus.Text = "Starting React dev server";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "run dev",
                        WorkingDirectory = clientProjectDirectory,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                    */

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

                    var serverProcess = Process.Start(psi);

                    serverProcess.OutputDataReceived += (s, ea) =>
                    {
                        if (ea.Data != null && ea.Data.Contains("Now listening on:"))
                        {
                            internalVariables.feURL = ea.Data.Split(new[] { "Now listening on:" }, StringSplitOptions.None)[1].Trim();

                            lblStatus.Text = $"Now listening on {internalVariables.feURL}";

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = internalVariables.feURL, // $"{localhosturl}/app"
                                UseShellExecute = true
                            });

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"{internalVariables.feURL}/swagger/index.html",
                                UseShellExecute = true
                            });
                        }
                    };
                    serverProcess.BeginOutputReadLine();
                    serverProcess.BeginErrorReadLine();

                    internalVariables.serverStarted = true;
                }
                else
                {
                    lblStatus.Text = "Starting React dev server";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "run dev",
                        WorkingDirectory = clientProjectDirectory,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                }



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

            _fe.Stop();
            this.Close();
            //Application.Exit();
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

        private async void btnTest_Click(object sender, EventArgs e)
        {
            string reactFolder = Path.Combine(projectRootPath, "ReactFE");
            string reactPath = Path.Combine(reactFolder, "jan0837_reactfe");
            /*
            Process.Start(new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "start",
                WorkingDirectory = reactPath,
                UseShellExecute = true,
                CreateNoWindow = false
            });
            */
            string url = await StartReactAndGetUrlAsync(reactPath);

            if (url != null)
            {
                lblStatus.Text = "Running React server on: " + url;

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                internalVariables.feURL = url;
            }
            else
            {
                // error
                lblStatus.Text = "Missing URL of React server.";
                MessageBox.Show("Nenašel jsem URL React serveru.", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Task<string> StartReactAndGetUrlAsync(string workingDir)
        {
            var tcs = new TaskCompletionSource<string>();
            var rx = new Regex(@"Local:\s+(http://localhost:\d+)", RegexOptions.Compiled);
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd /d \"{workingDir}\" && npm start",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

            // Regex na řádku, kde React vypisuje "Local: http://localhost:3000"
            //var rx = new Regex(@"Local:\s+(http://localhost:\d+)", RegexOptions.Compiled);

            proc.OutputDataReceived += (s, ea) =>
            {
                if (ea.Data == null) return;
                var m = rx.Match(ea.Data);
                if (m.Success)
                {
                    tcs.TrySetResult(m.Groups[1].Value);
                }
            };
            proc.ErrorDataReceived += (s, ea) =>
            {
                // (volitelně logovat chyby)
            };
            proc.Exited += (s, ea) =>
            {
                // pokud proces skončí dřív, než najdeme URL
                tcs.TrySetResult(null);
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // Timeout třeba 20 sekund, pak vrátíme null
            Task.Delay(20000).ContinueWith(_ => tcs.TrySetResult(null));

            return tcs.Task;
        }

        public async void btnSendData_Click(object sender, EventArgs e)
        {
            /*
            var rnd = new Random();
            int newVal = rnd.Next(1, 101);
            int newInterval = 2000;
            _fe.Update("status", "připojeno");
            _fe.Update("parameter1", newVal);
            _fe.Update("refreshInterval", newInterval);

            lblStatus.Text = "Data Trasnfered";

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(new { refreshInterval = newInterval });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync(InternalVariables.CommunicationUrl + "config", content);
            */
        }
    }
}
