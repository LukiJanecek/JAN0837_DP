using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class CrosswalkData
    {
        // inputs 
        public static string btnCrosswalkStart { get; set; } = "true"; // bool
        public static string btnCrosswalkPause { get; set; } = "false"; // bool
        public static string btnCrosswalkStop { get; set; } = "false"; // bool
        public static string btnCrosswalk1 { get; set; } = "false"; // bool
        public static string btnCrosswalk2 { get; set; } = "false"; // bool

        // outputs 
        public static string crosswalkType { get; set; } = "false"; // bool 
        public static string trafficLight1_green { get; set; } = "false"; // bool
        public static string trafficLight1_yellow { get; set; } = "false"; // bool
        public static string trafficLight1_red { get; set; } = "false"; // bool
        public static string trafficLight2_green { get; set; } = "false"; // bool
        public static string trafficLight2_yellow { get; set; } = "false"; // bool
        public static string trafficLight2_red { get; set; } = "false"; // bool
        public static string pedestrian1_green { get; set; } = "false"; // bool
        public static string pedestrian1_red { get; set; } = "false"; // bool
        public static string pedestrian2_green { get; set; } = "false"; // bool
        public static string pedestrian2_red { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string crosswalkType,
            string btnCrosswalkStart,
            string btnCrosswalkPause,
            string btnCrosswalkStop,
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
                    crosswalkType,
                    btnCrosswalkStart,
                    btnCrosswalkPause,
                    btnCrosswalkStop,
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
                crosswalkType = s.crosswalkType;

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

        public static class OpcUaNodeIds
        {
            // ═══════════════════════════════════════════════════════════
            // INPUT VARIABLES (written TO PLC)
            // Find these in UAExpert: DB_ProcessData > input > ...
            // ═══════════════════════════════════════════════════════════
            public static string btnCrosswalkStart { get; set; } = "ns=4;i=?";
            public static string btnCrosswalkPause { get; set; } = "ns=4;i=?";
            public static string btnCrosswalkStop { get; set; } = "ns=4;i=?";
            public static string btnCrosswalk1 { get; set; } = "ns=4;i=?";
            public static string btnCrosswalk2 { get; set; } = "ns=4;i=?";

            // ═══════════════════════════════════════════════════════════
            // OUTPUT VARIABLES (read FROM PLC)
            // Find these in UAExpert: DB_ProcessData > output > ...
            // ═══════════════════════════════════════════════════════════
            public static string crosswalkType { get; set; } = "ns=4;i=?";
            public static string trafficLight1_green { get; set; } = "ns=4;i=?";
            public static string trafficLight1_yellow { get; set; } = "ns=4;i=?";
            public static string trafficLight1_red { get; set; } = "ns=4;i=?";
            public static string trafficLight2_green { get; set; } = "ns=4;i=?";
            public static string trafficLight2_yellow { get; set; } = "ns=4;i=?";
            public static string trafficLight2_red { get; set; } = "ns=4;i=?";
            public static string pedestrian1_green { get; set; } = "ns=4;i=?";
            public static string pedestrian1_red { get; set; } = "ns=4;i=?";
            public static string pedestrian2_green { get; set; } = "ns=4;i=?";
            public static string pedestrian2_red { get; set; } = "ns=4;i=?";
        }
    }
}
