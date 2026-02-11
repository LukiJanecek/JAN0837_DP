using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JAN0837_DP.Data
{
    /// <summary>
    /// OPC UA Node IDs for Siemens PLC
    /// Get these values from UAExpert by browsing to each variable and noting the NodeId
    /// Format: ns=4;i=XX (numeric) - NOT ns=4;s="..." (string)
    /// </summary>
    public static class OpcUaNodeIds
    {
        // ═══════════════════════════════════════════════════════════
        // INPUT VARIABLES (written TO PLC)
        // Find these in UAExpert: DB_ProcessData > input > ...
        // ═══════════════════════════════════════════════════════════
        public static string btnStart { get; set; } = "ns=4;i=15";        // Update with your actual node ID
        public static string btnPause { get; set; } = "ns=4;i=16";        // Update with your actual node ID
        public static string btnStop { get; set; } = "ns=4;i=17";         // Update with your actual node ID
        public static string btnCrosswalk1 { get; set; } = "ns=4;i=18";   // Update with your actual node ID
        public static string btnCrosswalk2 { get; set; } = "ns=4;i=19";   // Update with your actual node ID

        // ═══════════════════════════════════════════════════════════
        // OUTPUT VARIABLES (read FROM PLC)
        // Find these in UAExpert: DB_ProcessData > output > ...
        // ═══════════════════════════════════════════════════════════
        public static string crossroadType { get; set; } = "ns=4;i=33";          // Update with your actual node ID
        public static string trafficLightGreen1 { get; set; } = "ns=4;i=34";     // Update with your actual node ID
        public static string trafficLightsYellow1 { get; set; } = "ns=4;i=35";   // Update with your actual node ID
        public static string trafficLightsRed1 { get; set; } = "ns=4;i=36";      // Update with your actual node ID
        public static string trafficLightGreen2 { get; set; } = "ns=4;i=37";     // Update with your actual node ID
        public static string trafficLightsYellow2 { get; set; } = "ns=4;i=38";   // Update with your actual node ID
        public static string trafficLightsRed2 { get; set; } = "ns=4;i=39";      // Update with your actual node ID
        public static string pedestrianLightGreen1 { get; set; } = "ns=4;i=40";  // Update with your actual node ID
        public static string pedestrianLightRed1 { get; set; } = "ns=4;i=41";    // Update with your actual node ID
        public static string pedestrianLightGreen2 { get; set; } = "ns=4;i=42";  // Update with your actual node ID
        public static string pedestrianLightRed2 { get; set; } = "ns=4;i=43";    // Update with your actual node ID
    }

    public static class CrossroadData
    {
        public const int CrossroadDBnumber = 1;
        public const int CrossroadDBlength = 4;
        public static byte[] CrossroadReadBuffer { get; set; } = new byte[CrossroadDBlength];
        public static byte[] CrossroadWriteBuffer { get; set; } = new byte[CrossroadDBlength];

        public static string crossroadType { get; set; } = "false"; // bool
        //public static string crossroadDay { get; set; } = "";
        //public static string crossroadNight { get; set; } = "";
        public static string btnCrossroadStart { get; set; } = "true"; // bool
        public static string btnCrossroadPause { get; set; } = "false"; // bool
        public static string btnCrossroadStop { get; set; } = "false"; // bool

        public static string btnCrosswalk1 { get; set; } = "false"; // bool
        public static string btnCrosswalk2 { get; set; } = "false"; // bool

        public static string trafficLight1_green { get; set; } = "false"; // bool
        public static string trafficLight1_yellow { get; set; } = "false"; // bool
        public static string trafficLight1_red { get; set; } = "false"; // bool
        public static string trafficLight2_green { get; set; } = "false"; // bool
        public static string trafficLight2_yellow { get;set; } = "false"; // bool
        public static string trafficLight2_red { get;set; } = "false"; // bool
        public static string pedestrian1_green { get; set; } = "false"; // bool
        public static string pedestrian1_red { get; set; } = "false"; // bool
        public static string pedestrian2_green { get; set; } = "false"; // bool
        public static string pedestrian2_red { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string crossroadType,
            string btnCrossroadStart,
            string btnCrossroadPause,
            string btnCrossroadStop,
            string btnCrosswalk1,
            string btnCrosswalk2,
            string trafficLight1_green,
            string trafficLight1_yellow,
            string trafficLight1_red,
            string trafficLight2_green,
            string trafficLight2_yellow,
            string trafficLight2_red,
            string pedestrian1_green,
            string pedestrian1_red,
            string pedestrian2_green,
            string pedestrian2_red
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    crossroadType,
                    btnCrossroadStart,
                    btnCrossroadPause,
                    btnCrossroadStop,
                    btnCrosswalk1,
                    btnCrosswalk2,
                    trafficLight1_green,
                    trafficLight1_yellow,
                    trafficLight1_red,
                    trafficLight2_green,
                    trafficLight2_yellow,
                    trafficLight2_red,
                    pedestrian1_green,
                    pedestrian1_red,
                    pedestrian2_green,
                    pedestrian2_red
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                crossroadType = s.crossroadType;

                //btnCrossroadStart = s.btnCrossroadStart;
                //btnCrossroadPause = s.btnCrossroadPause;
                //btnCrossroadStop = s.btnCrossroadStop;

                //btnCrosswalk1 = s.btnCrosswalk1;
                //btnCrosswalk2 = s.btnCrosswalk2;

                trafficLight1_green = s.trafficLight1_green;
                trafficLight1_yellow = s.trafficLight1_yellow;
                trafficLight1_red = s.trafficLight1_red;

                trafficLight2_green = s.trafficLight2_green;
                trafficLight2_yellow = s.trafficLight2_yellow;
                trafficLight2_red = s.trafficLight2_red;

                pedestrian1_green = s.pedestrian1_green;
                pedestrian1_red = s.pedestrian1_red;
                pedestrian2_green = s.pedestrian2_green;
                pedestrian2_red = s.pedestrian2_red;
            }
        }

        public static void Update(Action updater)
        {
            if (updater is null) return;
            lock (_lock) updater();
        }

        // optional 
        /*
        public static void Reset()
        {
            lock (_lock)
            {
                crossroadType = "false";
                btnCrossroadStart = "false";
                btnCrossroadPause = "false";
                btnCrossroadStop = "false";
                btnCrosswalk1 = "false";
                btnCrosswalk2 = "false";
                trafficLight1_green = "false";
                trafficLight1_yellow = "false";
                trafficLight1_red = "false";
                trafficLight2_green = "false";
                trafficLight2_yellow = "false";
                trafficLight2_red = "false";
                pedestrian1_green = "false";
                pedestrian1_red = "false";
                pedestrian2_green = "false";
                pedestrian2_red = "false";
                Array.Clear(CrossroadReadBuffer, 0, CrossroadReadBuffer.Length);
                Array.Clear(CrossroadWriteBuffer, 0, CrossroadWriteBuffer.Length);
            }
        }
        */
    }
}
