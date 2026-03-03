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
        // inputs 
        public static string btnStart { get; set; } = "true"; // bool
        public static string btnPause { get; set; } = "false"; // bool
        public static string btnStop { get; set; } = "false"; // bool
        //public static string btnCrosswalk1 { get; set; } = "false"; // bool -> old 
        //public static string btnCrosswalk2 { get; set; } = "false"; // bool -> old
        // public static string btnNorthCrosswalk1 { get; set; } = "false"; // bool
        // public static string btnNorthCrosswalk2 { get; set; } = "false"; // bool
        public static string btnWestCrosswalk1 { get; set; } = "false"; // bool     
        public static string btnWestCrosswalk2 { get; set; } = "false"; // bool
        // public static string btnEastCrosswalk1 { get; set; } = "false"; // bool
        // public static string btnEastCrosswalk2 { get; set; } = "false"; // bool
        public static string btnSouthCrosswalk1 { get; set; } = "false"; // bool
        public static string btnSouthCrosswalk2 { get; set; } = "false"; // bool

        // outputs
        public static string crossroadType { get; set; } = "false"; // bool 
        public static string trafficLightNorth_green { get; set; } = "false"; // bool
        public static string trafficLightNorth_yellow { get; set; } = "false"; // bool
        public static string trafficLightNorth_red { get; set; } = "false"; // bool
        public static string trafficLightSouth_green { get; set; } = "false"; // bool
        public static string trafficLightSouth_yellow { get; set; } = "false"; // bool
        public static string trafficLightSouth_red { get; set; } = "false"; // bool
        public static string trafficLightWest_green { get; set; } = "false"; // bool
        public static string trafficLightWest_yellow { get; set; } = "false"; // bool
        public static string trafficLightWest_red { get; set; } = "false"; // bool
        public static string trafficLightEast_green { get; set; } = "false"; // bool
        public static string trafficLightEast_yellow { get; set; } = "false"; // bool
        public static string trafficLightEast_red { get; set; } = "false"; // bool
        public static string pedestrianSouth_green { get; set; } = "false"; // bool
        public static string pedestrianSouth_red { get; set; } = "false"; // bool
        public static string pedestrianWest_green { get; set; } = "false"; // bool
        public static string pedestrianWest_red { get; set; } = "false"; // bool
        //public static string pedestrianEast_green { get; set; } = "false"; // bool
        //public static string pedestrianEast_red { get; set; } = "false"; // bool
        //public static string pedestrianNorth_green { get; set; } = "false"; // bool
        //public static string pedestrianNorth_red { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnStart,
            string btnPause,
            string btnStop,
            string btnWestCrosswalk1,
            string btnWestCrosswalk2,
            string btnSouthCrosswalk1,
            string btnSouthCrosswalk2,
            string crossroadType,
            string trafficLightNorth_green,
            string trafficLightNorth_yellow,
            string trafficLightNorth_red,
            string trafficLightSouth_green,
            string trafficLightSouth_yellow,
            string trafficLightSouth_red,
            string trafficLightWest_green,
            string trafficLightWest_yellow,
            string trafficLightWest_red,
            string trafficLightEast_green,
            string trafficLightEast_yellow,
            string trafficLightEast_red,
            string pedestrianSouth_green,
            string pedestrianSouth_red,
            string pedestrianWest_green,
            string pedestrianWest_red
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnStart,
                    btnPause,
                    btnStop,
                    btnWestCrosswalk1,
                    btnWestCrosswalk2,
                    btnSouthCrosswalk1,
                    btnSouthCrosswalk2,
                    crossroadType,
                    trafficLightNorth_green,
                    trafficLightNorth_yellow,
                    trafficLightNorth_red,
                    trafficLightSouth_green,
                    trafficLightSouth_yellow,
                    trafficLightSouth_red,
                    trafficLightWest_green,
                    trafficLightWest_yellow,
                    trafficLightWest_red,
                    trafficLightEast_green,
                    trafficLightEast_yellow,
                    trafficLightEast_red,
                    pedestrianSouth_green,
                    pedestrianSouth_red,
                    pedestrianWest_green,
                    pedestrianWest_red
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.crossroadType != null) crossroadType = s.crossroadType;

                if (s.trafficLightNorth_green != null) trafficLightNorth_green = s.trafficLightNorth_green;
                if (s.trafficLightNorth_yellow != null) trafficLightNorth_yellow = s.trafficLightNorth_yellow;
                if (s.trafficLightNorth_red != null) trafficLightNorth_red = s.trafficLightNorth_red;
                if (s.trafficLightSouth_green != null) trafficLightSouth_green = s.trafficLightSouth_green;
                if (s.trafficLightSouth_yellow != null) trafficLightSouth_yellow = s.trafficLightSouth_yellow;
                if (s.trafficLightSouth_red != null) trafficLightSouth_red = s.trafficLightSouth_red;
                if (s.trafficLightWest_green != null) trafficLightWest_green = s.trafficLightWest_green;
                if (s.trafficLightWest_yellow != null) trafficLightWest_yellow = s.trafficLightWest_yellow;
                if (s.trafficLightWest_red != null) trafficLightWest_red = s.trafficLightWest_red;
                if (s.trafficLightEast_green != null) trafficLightEast_green = s.trafficLightEast_green;
                if (s.trafficLightEast_yellow != null) trafficLightEast_yellow = s.trafficLightEast_yellow;
                if (s.trafficLightEast_red != null) trafficLightEast_red = s.trafficLightEast_red;

                if (s.pedestrianSouth_green != null) pedestrianSouth_green = s.pedestrianSouth_green;
                if (s.pedestrianSouth_red != null) pedestrianSouth_red = s.pedestrianSouth_red;
                if (s.pedestrianWest_green != null) pedestrianWest_green = s.pedestrianWest_green;
                if (s.pedestrianWest_red != null) pedestrianWest_red = s.pedestrianWest_red;
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
            // Inputs
            public static string btnStart { get; set; } = "ns=4;i=15";        
            public static string btnPause { get; set; } = "ns=4;i=16";        
            public static string btnStop { get; set; } = "ns=4;i=17";        

            public static string btnSouthCrosswalk1 { get; set; } = "ns=4;i=24";
            public static string btnSouthCrosswalk2 { get; set; } = "ns=4;i=25";
            public static string btnWestCrosswalk1 { get; set; } = "ns=4;i=20";
            public static string btnWestCrosswalk2 { get; set; } = "ns=4;i=21";


            // Outputs
            public static string crossroadType { get; set; } = "ns=4;i=33";
            public static string trafficLightNorth_green { get; set; } = "ns=4;i=34";
            public static string trafficLightNorth_yellow { get; set; } = "ns=4;i=35";
            public static string trafficLightNorth_red { get; set; } = "ns=4;i=36";
            public static string trafficLightSouth_green { get; set; } = "ns=4;i=37";
            public static string trafficLightSouth_yellow { get; set; } = "ns=4;i=38";
            public static string trafficLightSouth_red { get; set; } = "ns=4;i=39";
            public static string trafficLightWest_green { get; set; } = "ns=4;i=40";
            public static string trafficLightWest_yellow { get; set; } = "ns=4;i=41";
            public static string trafficLightWest_red { get; set; } = "ns=4;i=42";
            public static string trafficLightEast_green { get; set; } = "ns=4;i=43";
            public static string trafficLightEast_yellow { get; set; } = "ns=4;i=44";
            public static string trafficLightEast_red { get; set; } = "ns=4;i=45";

            public static string pedestrianSouth_green { get; set; } = "ns=4;i=48";
            public static string pedestrianSouth_red { get; set; } = "ns=4;i=49";
            public static string pedestrianWest_green { get; set; } = "ns=4;i=50";
            public static string pedestrianWest_red { get; set; } = "ns=4;i=51";
        }
    }
}
