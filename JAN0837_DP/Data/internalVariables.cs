using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JAN0837_DP.ReactFE;

namespace JAN0837_DP.Data
{
    public static class internalVariables
    {
        // threads
        public static Thread communicationThread { get; set; }
        public static Thread visualizationThread { get; set; }
        public static bool communicationRunningFlag { get; set; } = false;
        public static bool visualizationRunningFlag { get; set; } = false;
        public static FEserver feServer { get; set; }
        public static  CancellationTokenSource token { get; set; } // can it be static? 


        // communicationControl

        // checkboxes 
        public static bool checkBoxMaster { get; set; } = false;
        public static bool checkBoxSlave { get; set; } = false;

        // textboxes
        public static string txtBoxParam1 { get; set; } = "";
        public static string txtBoxParam2 { get; set; } = "";

        // Flags 
        public static bool opcuaFlag { get; set; } = false;
        public static bool mqttFlag { get; set; } = false;
        public static bool tcpipFlag { get; set; } = false;
        public static bool restapiFlag { get; set; } = false;
        public static bool modbustcpipFlag { get; set; } = false;
        public static bool s7Flag { get; set; } = false;

        // generateTIAtemplate

        // localhost
        public static string feURL { get; set; } = "http://localhost:3000/";
        public static string communicationURL { get; set; } = "http://localhost:5000/api/";
        public static bool serverStarted { get; set; } = false;

        // communication
        public static bool connected { get; set; } = false;
        public static bool communicationStatus { get; set; } = false;
    }
}
