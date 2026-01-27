using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using JAN0837_DP.ReactFE;
using Siemens.Engineering.HW;

namespace JAN0837_DP.Data
{
    public static class paths
    {
        public static string projectRootPath { get; set; } = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\"));
        public static string dataDirectoryPath { get; set; } = Path.Combine(projectRootPath, "Data");
        public static string solutionRootPath { get; set; } = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
        public static string clientProjectDirectory { get; set; } = Path.Combine(solutionRootPath, "JAN0837_react/JAN0837_react.Client");
        public static string parentDirectory { get; set; } = Directory.GetParent(Directory.GetParent(projectRootPath).FullName).FullName;
        public static string serverFolder { get; set; } = Path.Combine("JAN0837_react", "JAN0837_react.Server");
        public static string serverFile { get; set; } = Path.Combine(serverFolder, "JAN0837_react.Server.csproj"); // "JAN0837_react.Server.csproj.user"
        public static string clientFolder { get; set; } = Path.Combine("JAN0837_react", "JAN0837_react.client");
        public static string clientFile { get; set; } = Path.Combine(clientFolder, "JAN0837_react.CLient.csproj");
        public static string fullServerFilePath { get; set; } = Path.Combine(parentDirectory, serverFile);
        public static string fullClientFilePath { get; set; } = Path.Combine(parentDirectory, clientFile);

        public static string feReactFolder { get; set; } = Path.Combine(MainForm.projectRootPath, "ReactFE");
        public static string feReactProjectPath { get; set; } = Path.Combine(feReactFolder, "jan0837_reactfe");

        public static string tiaDLLPath { get; set; } = "C:\\Program Files\\Siemens\\Automation\\Portal V19\\PublicAPI\\V19"; // Siemens.Engineering.dll
        public static string tiaPath { get; set; } = Path.Combine(projectRootPath, "TIA");
        public static string tiaProjectPath { get; set; } = Path.Combine(tiaPath, "TIA_Projects");
        public static string tiaExampleProjectPath { get; set; } = Path.Combine(tiaProjectPath, "Example");
        public static string tiaSampleProjectPath { get; set; } = Path.Combine(tiaProjectPath, "Sample");

        public static string pythonScriptsFolder { get; set; } = Path.Combine(tiaPath, "PythonScripts");

        public static string pythonExePath = Path.Combine(pythonScriptsFolder, "venv", "Scripts", "python.exe");
    }

    public static class internalVariables
    {
        // threads
        public static Thread communicationThread { get; set; }
        public static Task communicationTask { get; set; }
        public static Thread visualizationThread { get; set; }
        public static CancellationTokenSource communicationCancellationTokenSource { get; set; }
        public static bool communicationThreadRunningFlag { get; set; } = false;
        public static bool visualizationThreadRunningFlag { get; set; } = false;

        // localhost
        public static string feURL { get; set; } = "http://localhost:3000";
        public static string communicationBaseURL { get; set; } = "http://localhost:5000/api";
        public static string communicationDataURL => communicationBaseURL.TrimEnd('/') + "/data";
        public static bool communicationServerStarted { get; set; } = false;
        public static bool feServerStarted { get; set; } = false;
        public static int communicationRefreshInterval { get; set; } = 50;

        // checkboxes 
        public static bool checkBoxMaster { get; set; } = false;
        public static bool checkBoxSlave { get; set; } = false;

        // textboxes
        public static string txtBoxParam1 { get; set; } = "";
        public static string txtBoxParam2 { get; set; } = "";

        // Flags 
        public static string communicationFlag { get; set; } = "";
        /*
        public static bool opcuaFlag { get; set; } = false;
        public static bool mqttFlag { get; set; } = false;
        public static bool tcpipFlag { get; set; } = false;
        public static bool restapiFlag { get; set; } = false;
        public static bool modbustcpipFlag { get; set; } = false;
        public static bool s7Flag { get; set; } = false;
        public static bool sharp7Flag {  get; set; } = false;
        */
    
        // communication
        public static bool connected { get; set; } = false;
        public static bool communicationStatus { get; set; } = false;

        // generateTIAtemplate
        
        /// <summary>
        /// Get the local IPv4 address of this machine (network IP, not loopback)
        /// </summary>
        /// <returns>Local IP address or "localhost" if not found</returns>
        public static string GetLocalIPAddress()
        {
            try
            {
                // Get host name
                string hostName = Dns.GetHostName();
                
                // Get IP addresses for this host
                IPHostEntry host = Dns.GetHostEntry(hostName);
                
                // Find the first IPv4 address that is not loopback
                foreach (IPAddress ip in host.AddressList)
                {
                    // Check if it's IPv4 (not IPv6) and not loopback (127.0.0.1)
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
                
                // If no network IP found, return localhost
                return "localhost";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting local IP: {ex.Message}");
                // Fallback to localhost
                return "localhost";
            }
        }
    }
}
