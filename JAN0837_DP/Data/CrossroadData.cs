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
    public static class CrossroadData
    {
        public const int CrossroadDBlength = 4;
        public static byte[] CrossroadReadBuffer { get; set; } = new byte[CrossroadDBlength];
        public static byte[] CrossroadWriteBuffer { get; set; } = new byte[CrossroadDBlength];

        //public static string crossroadDay { get; set; } = "";
        //public static string crossroadNight { get; set; } = "";

        // inputs 
        public static string btnStart { get; set; } = "true"; // bool
        public static string btnPause { get; set; } = "false"; // bool
        public static string btnStop { get; set; } = "false"; // bool
        public static string btnCrosswalk1 { get; set; } = "false"; // bool
        public static string btnCrosswalk2 { get; set; } = "false"; // bool

        // outputs
        public static string crossroadType { get; set; } = "false"; // bool
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
            string btnStart,
            string btnPause,
            string btnStop,
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
                    btnStart,
                    btnPause,
                    btnStop,
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
                if (s.crossroadType != null) crossroadType = s.crossroadType;

                //if (s.btnCrossroadStart != null) btnCrossroadStart = s.btnCrossroadStart;
                //if (s.btnCrossroadPause != null) btnCrossroadPause = s.btnCrossroadPause;
                //if (s.btnCrossroadStop != null) btnCrossroadStop = s.btnCrossroadStop;

                //if (s.btnCrosswalk1 != null) btnCrosswalk1 = s.btnCrosswalk1;
                //if (s.btnCrosswalk2 != null) btnCrosswalk2 = s.btnCrosswalk2;

                if (s.trafficLight1_green != null) trafficLight1_green = s.trafficLight1_green;
                if (s.trafficLight1_yellow != null) trafficLight1_yellow = s.trafficLight1_yellow;
                if (s.trafficLight1_red != null) trafficLight1_red = s.trafficLight1_red;

                if (s.trafficLight2_green != null) trafficLight2_green = s.trafficLight2_green;
                if (s.trafficLight2_yellow != null) trafficLight2_yellow = s.trafficLight2_yellow;
                if (s.trafficLight2_red != null) trafficLight2_red = s.trafficLight2_red;

                if (s.pedestrian1_green != null) pedestrian1_green = s.pedestrian1_green;
                if (s.pedestrian1_red != null) pedestrian1_red = s.pedestrian1_red;
                if (s.pedestrian2_green != null) pedestrian2_green = s.pedestrian2_green;
                if (s.pedestrian2_red != null) pedestrian2_red = s.pedestrian2_red;
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
        public static class OpcUaNodeIds
        {
            // ═══════════════════════════════════════════════════════════
            // INPUT VARIABLES (written TO PLC)
            // Find these in UAExpert: DB_ProcessData > input > ...
            // ═══════════════════════════════════════════════════════════
            public static string btnStart { get; set; } = "ns=4;i=15";        
            public static string btnPause { get; set; } = "ns=4;i=16";        
            public static string btnStop { get; set; } = "ns=4;i=17";        
            public static string btnCrosswalk1 { get; set; } = "ns=4;i=18";  
            public static string btnCrosswalk2 { get; set; } = "ns=4;i=19";   

            // ═══════════════════════════════════════════════════════════
            // OUTPUT VARIABLES (read FROM PLC)
            // Find these in UAExpert: DB_ProcessData > output > ...
            // ═══════════════════════════════════════════════════════════
            public static string crossroadType { get; set; } = "ns=4;i=33";          
            public static string trafficLightGreen1 { get; set; } = "ns=4;i=34";   
            public static string trafficLightsYellow1 { get; set; } = "ns=4;i=35";   
            public static string trafficLightsRed1 { get; set; } = "ns=4;i=36";     
            public static string trafficLightGreen2 { get; set; } = "ns=4;i=37";     
            public static string trafficLightsYellow2 { get; set; } = "ns=4;i=38";   
            public static string trafficLightsRed2 { get; set; } = "ns=4;i=39";      
            public static string pedestrianLightGreen1 { get; set; } = "ns=4;i=40";  
            public static string pedestrianLightRed1 { get; set; } = "ns=4;i=41";    
            public static string pedestrianLightGreen2 { get; set; } = "ns=4;i=42"; 
            public static string pedestrianLightRed2 { get; set; } = "ns=4;i=43";  
        }
    }
}
