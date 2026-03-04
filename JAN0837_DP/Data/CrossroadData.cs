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
        public static string pedestrianSouth1_green { get; set; } = "false"; // bool
        public static string pedestrianSouth1_red { get; set; } = "false"; // bool
        public static string pedestrianSouth2_green { get; set; } = "false"; // bool
        public static string pedestrianSouth2_red { get; set; } = "false"; // bool
        public static string pedestrianWest1_green { get; set; } = "false"; // bool
        public static string pedestrianWest1_red { get; set; } = "false"; // bool
        public static string pedestrianWest2_green { get; set; } = "false"; // bool
        public static string pedestrianWest2_red { get; set; } = "false"; // bool

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
            string pedestrianSouth1_green,
            string pedestrianSouth1_red,
            string pedestrianSouth2_green,
            string pedestrianSouth2_red,
            string pedestrianWest1_green,
            string pedestrianWest1_red,
            string pedestrianWest2_green,
            string pedestrianWest2_red
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
                    pedestrianSouth1_green,
                    pedestrianSouth1_red,
                    pedestrianSouth2_green,
                    pedestrianSouth2_red,
                    pedestrianWest1_green,
                    pedestrianWest1_red,
                    pedestrianWest2_green,
                    pedestrianWest2_red
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

                if (s.pedestrianSouth1_green != null) pedestrianSouth1_green = s.pedestrianSouth1_green;
                if (s.pedestrianSouth1_red != null) pedestrianSouth1_red = s.pedestrianSouth1_red;
                if (s.pedestrianSouth2_green != null) pedestrianSouth2_green = s.pedestrianSouth2_green;
                if (s.pedestrianSouth2_red != null) pedestrianSouth2_red = s.pedestrianSouth2_red;
                if (s.pedestrianWest1_green != null) pedestrianWest1_green = s.pedestrianWest1_green;
                if (s.pedestrianWest1_red != null) pedestrianWest1_red = s.pedestrianWest1_red;
                if (s.pedestrianWest2_green != null) pedestrianWest2_green = s.pedestrianWest2_green;
                if (s.pedestrianWest2_red != null) pedestrianWest2_red = s.pedestrianWest2_red;
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

        public static class Sharp7Addresses
        {
            public static int address_btnStart = 38;
            public static int bit_btnStart = 0;
            public static int address_btnPause = 39;
            public static int bit_btnPause = 0;
            public static int address_btnStop = 40;
            public static int bit_btnStop = 0;
            public static int address_btnWestCrosswalk1 = 41;
            public static int bit_btnWestCrosswalk1 = 0;
            public static int address_btnWestCrosswalk2 = 42;
            public static int bit_btnWestCrosswalk2 = 0;
            public static int address_btnSouthCrosswalk1 = 43;
            public static int bit_btnSouthCrosswalk1 = 0;
            public static int address_btnSouthCrosswalk2 = 44;
            public static int bit_btnSouthCrosswalk2 = 0;

            public static int address_crossroadType = 50;
            public static int bit_crossroadType = 0;

            public static int address_trafficLightNorth_green = 51;
            public static int bit_trafficLightNorth_green = 0;
            public static int address_trafficLightNorth_yellow = 52;
            public static int bit_trafficLightNorth_yellow = 0;
            public static int address_trafficLightNorth_red = 53;
            public static int bit_trafficLightNorth_red = 0;

            public static int address_trafficLightSouth_green = 54;
            public static int bit_trafficLightSouth_green = 0;
            public static int address_trafficLightSouth_yellow = 55;
            public static int bit_trafficLightSouth_yellow = 0;
            public static int address_trafficLightSouth_red = 56;
            public static int bit_trafficLightSouth_red = 0;

            public static int address_trafficLightWest_green = 56;
            public static int bit_trafficLightWest_green = 0;
            public static int address_trafficLightWest_yellow = 57;
            public static int bit_trafficLightWest_yellow = 0;
            public static int address_trafficLightWest_red = 58;
            public static int bit_trafficLightWest_red = 0;

            public static int address_trafficLightEast_green = 59;
            public static int bit_trafficLightEast_green = 0;
            public static int address_trafficLightEast_yellow = 60;
            public static int bit_trafficLightEast_yellow = 0;
            public static int address_trafficLightEast_red = 61;
            public static int bit_trafficLightEast_red = 0;

            public static int address_pedestrianSouth1_green = 62;
            public static int bit_pedestrianSouth1_green = 0;
            public static int address_pedestrianSouth1_red = 63;
            public static int bit_pedestrianSouth1_red = 0;
            public static int address_pedestrianSouth2_green = 64;
            public static int bit_pedestrianSouth2_green = 0;
            public static int address_pedestrianSouth2_red = 65;
            public static int bit_pedestrianSouth2_red = 0;

            public static int address_pedestrianWest1_green = 66;
            public static int bit_pedestrianWest1_green = 0;
            public static int address_pedestrianWest1_red = 67;
            public static int bit_pedestrianWest1_red = 0;
            public static int address_pedestrianWest2_green = 68;
            public static int bit_pedestrianWest2_green = 0;
            public static int address_pedestrianWest2_red = 69;
            public static int bit_pedestrianWest2_red = 0;
        }

        public static class ModbusBytes
        {
            public static int byte_btnStart = 0;
            public static int byte_btnPause = 1;
            public static int byte_btnStop = 2;
            public static int byte_btnWestCrosswalk1 = 3;
            public static int byte_btnWestCrosswalk2 = 4;
            public static int byte_btnSouthCrosswalk1 = 5;
            public static int byte_btnSouthCrosswalk2 = 6;

            public static int byte_crossroadType = 10;  
            public static int byte_trafficLightNorth_green = 11;
            public static int byte_trafficLightNorth_yellow = 12;
            public static int byte_trafficLightNorth_red = 13;

            public static int byte_trafficLightSouth_green = 14;
            public static int byte_trafficLightSouth_yellow = 15;
            public static int byte_trafficLightSouth_red = 16;

            public static int byte_trafficLightWest_green = 17;
            public static int byte_trafficLightWest_yellow = 18;
            public static int byte_trafficLightWest_red = 19;

            public static int byte_trafficLightEast_green = 20;
            public static int byte_trafficLightEast_yellow = 21;
            public static int byte_trafficLightEast_red = 22;

            public static int byte_pedestrianSouth1_green = 23;
            public static int byte_pedestrianSouth1_red = 24;
            public static int byte_pedestrianSouth2_green = 25;
            public static int byte_pedestrianSouth2_red = 26;

            public static int byte_pedestrianWest1_green = 27;
            public static int byte_pedestrianWest1_red = 28;
            public static int byte_pedestrianWest2_green = 29;
            public static int byte_pedestrianWest2_red = 30;
        }

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

            public static string pedestrianSouth1_green { get; set; } = "ns=4;i=48";
            public static string pedestrianSouth1_red { get; set; } = "ns=4;i=49";
            public static string pedestrianSouth2_green { get; set; } = "ns=4;i=48";
            public static string pedestrianSouth2_red { get; set; } = "ns=4;i=49";
            public static string pedestrianWest1_green { get; set; } = "ns=4;i=50";
            public static string pedestrianWest1_red { get; set; } = "ns=4;i=51";
            public static string pedestrianWest2_green { get; set; } = "ns=4;i=50";
            public static string pedestrianWest2_red { get; set; } = "ns=4;i=51";
        }
    }
}
