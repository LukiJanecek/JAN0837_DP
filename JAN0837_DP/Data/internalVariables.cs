using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class internalVariables
    {
        // threads
        public static Thread communicationThread { get; set; }
        public static Thread visualizationThread { get; set; }
        public static bool communicationRunningFlag { get; set; }
        public static bool visualizationRunningFlag { get; set; }


        // communicationControl

        // checkboxes 
        public static bool checkBoxMaster { get; set; }
        public static bool checkBoxSlave { get; set; }

        // textboxes
        public static string txtBoxParam1 { get; set; }
        public static string txtBoxParam2 { get; set; }

        // Flags 
        public static bool opcuaFlag { get; set; }
        public static bool mqttFlag { get; set; }
        public static bool tcpipFlag { get; set; }
        public static bool restapiFlag { get; set; }
        public static bool modbustcpipFlag { get; set; }
        public static bool s7Flag { get; set; }

        // generateTIAtemplate

        // localhost
        public static string localhosturl { get; set; }
        public static bool serverStarted { get; set; }

        // communication
        public static bool connected { get; set; }
        public static bool communicationStatus {  get; set; }
    }
}
