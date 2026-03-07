using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class CarLightData
    {
        // Inputs 
        public static string btnReset { get; set; } = "false"; // bool
        public static string error { get; set; } = "false"; // bool
        public static string sensorLight { get; set; } = "false"; // bool
        public static string sensorConnectorConnected { get; set; } = "false"; // bool

        // Outputs 
        public static string lowBeamLight { get; set; } = "false"; // bool
        public static string highBeamLight { get; set; } = "false"; // bool
        public static string turnLight { get; set; } = "false"; // bool
        public static string result { get; set; } = "false"; // bool

        // Thread safety
        private static readonly object _lock = new();

        // Snapshot
        public readonly record struct State(
            string btnReset,
            string error,
            string lowBeamLight,
            string highBeamLight,
            string sensorLight,
            string sensorConnectorConnected,
            string turnLight,
            string result
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnReset,
                    error,
                    lowBeamLight,
                    highBeamLight,
                    sensorLight,
                    sensorConnectorConnected,
                    turnLight,
                    result
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.lowBeamLight != null) lowBeamLight = s.lowBeamLight;
                if (s.highBeamLight != null) highBeamLight = s.highBeamLight;
                if (s.turnLight != null) turnLight = s.turnLight;
                if (s.result != null) result = s.result;
            }
        }

        public static void Update(Action updater)
        {
            if (updater is null) return;
            lock (_lock) updater();
        }

        public static class Sharp7Addresses
        {
            public static int address_btnReset = 146;
            public static int bit_btnReset = 0;
            public static int address_error = 146;
            public static int bit_error = 1;
            public static int address_sensorLight = 146;
            public static int bit_sensorLight = 2;
            public static int address_sensorConnectorConnected = 146;
            public static int bit_sensorConnectorConnected = 3;

            public static int address_lowBeamLight = 164;
            public static int bit_lowBeamLight = 0;
            public static int address_highBeamLight = 164;
            public static int bit_highBeamLight = 1;
            public static int address_turnLight = 164;
            public static int bit_turnLight = 2;
            public static int address_result = 164;
            public static int bit_result = 3;
        }

        public static class ModbusBytes // I will not need this :( 
        {
            public static int byte_btnReset = 0;
            public static int byte_error = 0;
            public static int byte_sensorLight = 0;
            public static int byte_sensorConnectorConnected = 0;
            public static int byte_lowBeamLight = 0;
            public static int byte_highBeamLight = 0;
            public static int byte_turnLight = 0;

            public static int byte_result = 0;
        }

        public static class OpcUaNodeIds
        {
            // Inputs
            public static string btnReset { get; set; } = "ns=4;i=114";
            public static string error { get; set; } = "ns=4;i=115";
            public static string sensorLight { get; set; } = "ns=4;i=116";
            public static string sensorConnectorConnected { get; set; } = "ns=4;i=117";

            // Outputs  
            public static string lowBeamLight { get; set; } = "ns=4;i=125";
            public static string highBeamLight { get; set; } = "ns=4;i=126";
            public static string turnLight { get; set; } = "ns=4;i=127";
            public static string result { get; set; } = "ns=4;i=128";
        }
    }
}
