using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
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
        public static string feReactFolder { get; set; } = Path.Combine(projectRootPath, "ReactFE");
        public static string feReactProjectPath { get; set; } = Path.Combine(feReactFolder, "jan0837_reactfe");

        // OPCUA xml paths 
        public static string communiactionFolderPath { get; set; } = Path.Combine(projectRootPath, "Communication");
        public static string serverXMLPath { get; set; } = Path.Combine(communiactionFolderPath, "comOPCUA", "OpcUaServer.Config.xml");
        public static string clientXMLPath { get; set; } = Path.Combine(communiactionFolderPath, "comOPCUA", "OpcUaClient.Config.xml");

        // TIA Portal paths 
        public static string tiaDLLPath { get; set; } = "C:\\Program Files\\Siemens\\Automation\\Portal V20\\PublicAPI\\V20"; // Siemens.Engineering.dll
        public static string defaultTIADLLPath { get; set; } = "C:\\Program Files\\Siemens\\Automation\\Portal V20\\PublicAPI\\V20"; // Siemens.Engineering.dll
        public static string tiaPath { get; set; } = Path.Combine(projectRootPath, "TIA");
        public static string tiaProjectPath { get; set; } = Path.Combine(tiaPath, "TIA_Projects");
        public static string tiaExampleProjectPath { get; set; } = Path.Combine(tiaProjectPath, "Example");
        public static string tiaSampleProjectPath { get; set; } = Path.Combine(tiaProjectPath, "Sample");

        // Python paths 
        public static string pythonScriptsFolder { get; set; } = Path.Combine(tiaPath, "PythonScripts");

        public static string pythonExePath = Path.Combine(pythonScriptsFolder, "venv", "Scripts", "python.exe");

        // Log paths
        public static string logDirectoryPath { get; set; } = Path.Combine(projectRootPath, "Log");
        public static string logFilePath { get; set; } = Path.Combine(logDirectoryPath, "log.txt");
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

        // localhost - dynamic IP detection
        private static string _localIP = null;
        public static string LocalIP
        {
            get
            {
                if (_localIP == null)
                {
                    _localIP = GetLocalIPAddress();
                }
                return _localIP;
            }
            set
            {
                _localIP = value;
            }
        }

        // Ports
        public static int fePort { get; set; } = 3000;
        public static int apiPort { get; set; } = 5000;

        // Actual bound address (set by server when it starts)
        // API server tracks what it bound to (may be localhost if network binding failed)
        public static string actualApiHost { get; set; } = "localhost";
        
        // Dynamic URLs based on detected IP (for external access from other devices)
        public static string feURL => $"http://{LocalIP}:{fePort}";
        public static string communicationBaseURL => $"http://{LocalIP}:{apiPort}/api";
        public static string communicationDataURL => $"{communicationBaseURL}/data";
        
        // Internal URLs (using localhost - for health checks from same machine)
        // Both API and FE servers are always accessible via localhost from the same machine
        public static string internalApiURL => $"http://{actualApiHost}:{apiPort}/api";
        public static string internalApiDataURL => $"{internalApiURL}/data";
        public static string internalFeURL => $"http://localhost:{fePort}";  // Always localhost for internal checks

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
        /// Get the local IPv4 address of this machine (real network adapter, not VPN/virtual)
        /// </summary>
        /// <returns>Local IP address or "localhost" if not found</returns>
        public static string GetLocalIPAddress()
        {
            try
            {
                // Use NetworkInterface to get more details about adapters
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                
                // Priority list of preferred adapter types
                var preferredTypes = new[]
                {
                    System.Net.NetworkInformation.NetworkInterfaceType.Ethernet,
                    System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211,
                    System.Net.NetworkInformation.NetworkInterfaceType.GigabitEthernet
                };

                // Names to exclude (VPN, virtual adapters, etc.)
                string[] excludePatterns = { "virtual", "vpn", "tailscale", "vmware", "virtualbox", 
                                             "hyper-v", "docker", "wsl", "loopback", "bluetooth" };

                foreach (var preferredType in preferredTypes)
                {
                    foreach (var ni in networkInterfaces)
                    {
                        // Skip if not up, not the right type, or matches excluded patterns
                        if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                            continue;
                        if (ni.NetworkInterfaceType != preferredType)
                            continue;
                        if (excludePatterns.Any(p => ni.Name.ToLower().Contains(p) || ni.Description.ToLower().Contains(p)))
                            continue;

                        var props = ni.GetIPProperties();
                        foreach (var addr in props.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == AddressFamily.InterNetwork && 
                                !IPAddress.IsLoopback(addr.Address))
                            {
                                var ip = addr.Address.ToString();
                                // Skip 100.x.x.x range (Tailscale) and 10.x.x.x (often VPN)
                                if (!ip.StartsWith("100.") && !ip.StartsWith("172."))
                                {
                                    Console.WriteLine($"Selected IP: {ip} from adapter: {ni.Name} ({ni.Description})");
                                    return ip;
                                }
                            }
                        }
                    }
                }

                // Fallback: try the old method but filter out known VPN ranges
                string hostName = Dns.GetHostName();
                IPHostEntry host = Dns.GetHostEntry(hostName);
                
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        var ipStr = ip.ToString();
                        // Prefer 192.168.x.x (home networks)
                        if (ipStr.StartsWith("192.168."))
                        {
                            Console.WriteLine($"Selected fallback IP: {ipStr}");
                            return ipStr;
                        }
                    }
                }

                // Last resort: any non-loopback IPv4
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        Console.WriteLine($"Selected last-resort IP: {ip}");
                        return ip.ToString();
                    }
                }
                
                return "localhost";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting local IP: {ex.Message}");
                return "localhost";
            }
        }

        /// <summary>
        /// Manually set the IP address (call this if auto-detection picks wrong adapter)
        /// </summary>
        public static void SetLocalIP(string ip)
        {
            _localIP = ip;
            Console.WriteLine($"Manually set IP to: {ip}");
        }

        /// <summary>
        /// Reset IP detection (will re-detect on next access)
        /// </summary>
        public static void ResetLocalIP()
        {
            _localIP = null;
        }
    }
}
