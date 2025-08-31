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

        //Paths
        public static string projectRootPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\"));
        public static string dataDirectoryPath = Path.Combine(projectRootPath, "Data");

        public static string solutionRootPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
        public static string clientProjectDirectory = Path.Combine(solutionRootPath, "JAN0837_react/JAN0837_react.Client");

        public FEserver _feServer;
        public FEcommunicationControl _feCommunication; 
        public ucLocalhost ucLocalhost;
        public ucGenerateTIAtemplate ucGenerateTIAtemplate;

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
                internalVariables.visualizationThreadRunningFlag = true;
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

            // starting reading 
            PeriodicalReading.Interval = internalVariables.communicationRefreshInterval;
            PeriodicalReading.Start();

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
                if (internalVariables.reactServerStarted == false)
                {

                    // starting .NET server
                    lblStatus.Text = "Starting .NET server";
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{fullServerFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false, // false
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

                    internalVariables.reactServerStarted = true;
                }

                lblStatus.Text = "Servers started. Check your browser."; // Localhost openend on ...

            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při spouštění:\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void btnExit_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Exitting...";

            // communication thread stop
            if (internalVariables.communicationThread != null && internalVariables.communicationThread.IsAlive)
            {
                internalVariables.communicationThreadRunningFlag = false;
                internalVariables.communicationThread.Join();
            }

            // vizualization thread stop
            if (internalVariables.visualizationThread != null && internalVariables.visualizationThread.IsAlive)
            {
                internalVariables.visualizationThreadRunningFlag = false;

                if (_feServer != null)
                {
                    await _feServer.StopAsync();
                }
                
                if (_feCommunication != null)
                {
                    _feCommunication.Stop();

                }
                
                internalVariables.visualizationThread.Join();
                PeriodicalReading.Stop();
            }

            // stop polling 

            this.Close();
            //Application.Exit();
        }

        #endregion

        public async void Visualization()
        {
            try
            {
                _feCommunication = new FEcommunicationControl(internalVariables.communicationBaseURL);
                _feCommunication.Start();

                _feServer = new FEserver(_feCommunication);
                await _feServer.StartAsync();

                //PeriodicalReading.Interval = internalVariables.communicationRefreshInterval;
                //PeriodicalReading.Start();
            }
            catch (Exception ex)
            {

            }
        }

        public async void PeriodicalReading_Tick(object? sender, EventArgs e)
        {
            try
            {
                var snap = await _feCommunication.GetDataAsync();
                _feCommunication.ApplySnapshot(snap);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (!IsDisposed && PeriodicalReading.Enabled == false)
                {
                    PeriodicalReading.Start();
                }
            }
        }
    }
}
