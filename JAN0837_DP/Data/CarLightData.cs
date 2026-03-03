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
        public static string btnReset { get; set; } = "false";
        public static string error { get; set; } = "false";
        public static string sensorLight { get; set; } = "false";
        public static string sensorConnectorConnected { get; set; } = "false";
        public static string lowBeamLight { get; set; } = "false";
        public static string highBeamLight { get; set; } = "false";
        public static string turnLight { get; set; } = "false";

        // Outputs 
        
        public static string result { get; set; } = "false";

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
                    sensorLight,
                    sensorConnectorConnected,
                    lowBeamLight,
                    highBeamLight,
                    turnLight,
                    result
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.result != null) result = s.result;
            }
        }

        public static void Update(Action updater)
        {
            if (updater is null) return;
            lock (_lock) updater();
        }

        public static class OpcUaNodeIds
        {
            // Inputs
            public static string btnReset { get; set; } = "ns=4;i=?";
            public static string error { get; set; } = "ns=4;i=?";
            public static string sensorLight { get; set; } = "ns=4;i=?";
            public static string sensorConnectorConnected { get; set; } = "ns=4;i=?";
            public static string lowBeamLight { get; set; } = "ns=4;i=?";
            public static string highBeamLight { get; set; } = "ns=4;i=?";
            public static string turnLight { get; set; } = "ns=4;i=?";

            // Outputs
            public static string result { get; set; } = "ns=4;i=?";
        }
    }
}
