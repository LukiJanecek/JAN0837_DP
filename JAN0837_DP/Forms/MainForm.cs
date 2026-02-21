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
using JAN0837_DP.Communication.comOPCUA;

    // Modbus
using Modbus;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Data;
using Modbus.Extensions;
using Modbus.IO;
using Modbus.Message;
using JAN0837_DP.Communication.comModbusTCPIP;

    // MQTT
using MQTTnet;
//using MQTTnet.Server;
//using MQTTnet.Client;
//using MQTTnet.Client.Options;

// REST API 


//TCP/IP
using JAN0837_DP.Communication.comTCPIP;

// Sharp7
using Sharp7;

// Additional Libraries 
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JAN0837_DP.Forms;
using JAN0837_DP.Data;
using JAN0837_DP.Communication;
using JAN0837_DP.Log;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using JAN0837_DP.ReactFE;

namespace JAN0837_DP
{
    public partial class MainForm : Form
    {
        // Queues 
        //public ConcurrentQueue<int> dataQueueIN = new ConcurrentQueue<int>();
        //public ConcurrentQueue<int> dataQueueOUT = new ConcurrentQueue<int>();

        public FEserver _feServer;
        public FEcommunicationControl _feCommunication;
        public ucLocalhost ucLocalhost;
        public ucTIAControl ucGenerateTIAtemplate;

        private readonly ucCommunicationControl _ucCommControl;
        private readonly ucLocalhost _ucLocalhost;
        private readonly ucTIAControl _ucTIAControl;

        public MainForm()
        {
            InitializeComponent();
            this.MinimumSize = new Size(900, 700);

            _ucCommControl = new ucCommunicationControl { Dock = DockStyle.Fill };
            mainWindow.Controls.Add(_ucCommControl);
            _ucCommControl.Visible = false;

            _ucLocalhost = new ucLocalhost { Dock = DockStyle.Fill };
            mainWindow.Controls.Add(_ucLocalhost);
            _ucLocalhost.Visible = false;

            _ucTIAControl = new ucTIAControl { Dock = DockStyle.Fill};
            mainWindow.Controls.Add(_ucTIAControl);
            _ucTIAControl.Visible = false;
        }

        private void ShowInMain(UserControl uc)
        {
            foreach (Control c in mainWindow.Controls) c.Visible = false;

            if (!mainWindow.Controls.Contains(uc))
                mainWindow.Controls.Add(uc);

            uc.Visible = true;
            uc.BringToFront();
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

            // TIA version resolver
            AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
            {
                var name = new System.Reflection.AssemblyName(args.Name).Name + ".dll";
                var path = System.IO.Path.Combine(paths.tiaDLLPath, name);
                return System.IO.File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
            };
        }

        // toolStripMain components
        #region toolStripMain components
        private void btnGenerateTIATemplate_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Openning TIA Portal control.";

            /*
            mainWindow.Controls.Clear();
            var visual = new ucTIAControl();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);
            */

            ShowInMain(_ucTIAControl);
        }

        private void btnCommunicationControl_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Openning communication control.";

            /*
            mainWindow.Controls.Clear();
            var visual = new ucCommunicationControl();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);
            */

            ShowInMain(_ucCommControl);
        }

        private void btnLocalHost_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Starting React dev server and ASP.NET server…"; // Openning localhost in browser.

            // starting reading 
            PeriodicalReading.Interval = internalVariables.communicationRefreshInterval;
            PeriodicalReading.Start();
            
            /*
            string parentDirectory = Directory.GetParent(Directory.GetParent(projectRootPath).FullName).FullName;
            string serverFolder = Path.Combine("JAN0837_react", "JAN0837_react.Server");
            string serverFile = Path.Combine(serverFolder, "JAN0837_react.Server.csproj"); // "JAN0837_react.Server.csproj.user"
            string clientFolder = Path.Combine("JAN0837_react", "JAN0837_react.client");
            string clientFile = Path.Combine(clientFolder, "JAN0837_react.CLient.csproj");

            string fullServerFilePath = Path.Combine(parentDirectory, serverFile);
            string fullClientFilePath = Path.Combine(parentDirectory, clientFile);
            */

            /*
            mainWindow.Controls.Clear();
            var visual = new ucLocalhost();
            visual.Dock = DockStyle.Fill;
            mainWindow.Controls.Add(visual);
            */

            ShowInMain(_ucLocalhost);

            // Vizualization
            /*
            try
            {
                if (internalVariables.feServerStarted == false)
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

                    internalVariables.feServerStarted = true;
                }

                lblStatus.Text = "Servers started. Check your browser."; // Localhost openend on ...

            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při spouštění:\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */
        }

        private async void btnExit_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Exitting...";

            // communication thread stop
            
            if (internalVariables.communicationThread != null || internalVariables.communicationThreadRunningFlag == true)
            {
                internalVariables.communicationThreadRunningFlag = false;
                //internalVariables.communicationThread.Join();
                internalVariables.communicationCancellationTokenSource.Cancel();
            }
            

            // vizualization thread stop
            if (internalVariables.visualizationThread != null || internalVariables.visualizationThread.IsAlive || internalVariables.visualizationThreadRunningFlag == true)
            {
                if (_feServer != null)
                {
                    await _feServer.serverStop();
                }

                if (_feCommunication != null)
                {
                    _feCommunication.communicationStop();
                }

                internalVariables.visualizationThread.Join();
                PeriodicalReading.Stop();
                internalVariables.visualizationThreadRunningFlag = false;
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
                _feCommunication.communicationStart();

                _feServer = new FEserver(_feCommunication);
                await _feServer.serverStart();

                SetStatus($"Server started on {internalVariables.LocalIP}:{internalVariables.apiPort}");
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
                Logger.LogException(ex, "Visualization");
            }
        }

        public async void PeriodicalReading_Tick(object? sender, EventArgs e)
        {
            try
            {
                // TestData
                var snapTestData = await _feCommunication.GetTestDataAsync();
                _feCommunication.ApplySnapshot(snapTestData);

                // CrossroadData
                var snapCrossroadData = await _feCommunication.GetCrossroadDataAsync();
                _feCommunication.ApplySnapshot(snapCrossroadData);

                // CrosswalkData
                var snapCrosswalkData = await _feCommunication.GetCrosswalkDataAsync();
                _feCommunication.ApplySnapshot(snapCrosswalkData);

                // RegulatorData
                var snapRegulatorData = await _feCommunication.GetRegulatorDataAsync();
                _feCommunication.ApplySnapshot(snapRegulatorData);

                // CarWash
                var snapCarWashData = await _feCommunication.GetCarWashDataAsync();
                _feCommunication.ApplySnapshot(snapCarWashData);

                // WashingMachine
                var snapWashingMachineData = await _feCommunication.GetWashingMachineDataAsync();
                _feCommunication.ApplySnapshot(snapWashingMachineData);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "PeriodicalReading_Tick");
            }
            finally
            {
                if (!IsDisposed && PeriodicalReading.Enabled == false)
                {
                    PeriodicalReading.Start();
                }
            }
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

        private void mainWindow_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
