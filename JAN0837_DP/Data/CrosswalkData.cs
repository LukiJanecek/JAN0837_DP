using Opc.Ua;
using Siemens.Engineering.HW;
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
        public static string btnStart { get; set; } = "true"; // bool
        public static string btnPause { get; set; } = "false"; // bool
        public static string btnStop { get; set; } = "false"; // bool
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
                    crosswalkType,
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
                //if (s.btnStart != null) btnStart = s.btnStart;
                //if (s.btnPause != null) btnPause = s.btnPause;
                //if (s.btnStop != null) btnStop = s.btnStop;

                //if (s.btnCrosswalk1 != null) btnCrosswalk1 = s.btnCrosswalk1;
                //if (s.btnCrosswalk2 != null) btnCrosswalk2 = s.btnCrosswalk2;

                if (s.crosswalkType != null) crosswalkType = s.crosswalkType;

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

        public static class Sharp7Addresses
        {
            public static int address_btnStart = 38;
            public static int bit_btnStart = 0;
            public static int address_btnPause = 38;
            public static int bit_btnPause = 1;
            public static int address_btnStop = 38;
            public static int bit_btnStop = 2;
            public static int address_btnCrosswalk1 = 38;
            public static int bit_btnCrosswalk1 = 3;
            public static int address_btnCrosswalk2 = 38;
            public static int bit_btnCrosswalk2 = 4;

            public static int address_crosswalkType = 56;
            public static int bit_crosswalkType = 0;
            public static int address_trafficLight1_green = 56;
            public static int bit_trafficLight1_green = 1;
            public static int address_trafficLight1_yellow = 56;
            public static int bit_trafficLight1_yellow = 2;
            public static int address_trafficLight1_red = 56;
            public static int bit_trafficLight1_red = 3;
            public static int address_trafficLight2_green = 56;
            public static int bit_trafficLight2_green = 4;
            public static int address_trafficLight2_yellow = 56;
            public static int bit_trafficLight2_yellow = 5;
            public static int address_trafficLight2_red = 56;
            public static int bit_trafficLight2_red = 6;

            public static int address_pedestrian1_green = 56;
            public static int bit_pedestrian1_green = 7;
            public static int address_pedestrian1_red = 57;
            public static int bit_pedestrian1_red = 0;
            public static int address_pedestrian2_green = 57;
            public static int bit_pedestrian2_green = 1;
            public static int address_pedestrian2_red = 57;
            public static int bit_pedestrian2_red = 2;
        }

        public static class ModbusBytes
        {
            public static int byte_btnStart = 0;
            public static int byte_btnPause = 0;
            public static int byte_btnStop = 0;
            public static int byte_btnCrosswalk1 = 0;
            public static int byte_btnCrosswalk2 = 0;

            public static int byte_crosswalkType = 1;
            public static int byte_trafficLight1_green = 2;
            public static int byte_trafficLight1_yellow = 2;
            public static int byte_trafficLight1_red = 2;
            public static int byte_trafficLight2_green = 2;
            public static int byte_trafficLight2_yellow = 2;
            public static int byte_trafficLight2_red = 2;
            public static int byte_pedestrian1_green = 3;
            public static int byte_pedestrian1_red = 3;
            public static int byte_pedestrian2_green = 3;
            public static int byte_pedestrian2_red = 3;
        }

        public static class OpcUaNodeIds
        {
            // Inputs
            public static string btnStart { get; set; } = "ns=4;i=?";
            public static string btnPause { get; set; } = "ns=4;i=?";
            public static string btnStop { get; set; } = "ns=4;i=?";
            public static string btnCrosswalk1 { get; set; } = "ns=4;i=?";
            public static string btnCrosswalk2 { get; set; } = "ns=4;i=?";

            //Outputs
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
