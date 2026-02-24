using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class CarLightData
    {
        // Inputs (written TO PLC)
        public static string btnStart { get; set; } = "false";
        public static string btnReset { get; set; } = "false";
        public static string markerLight { get; set; } = "false";
        public static string brakeLight { get; set; } = "false";
        public static string turnLight { get; set; } = "false";

        // Outputs (read FROM PLC)
        public static string sensorPosition { get; set; } = "false";
        public static string sensorConnectorConnected { get; set; } = "false";
        public static string done { get; set; } = "false";

        // Thread safety
        private static readonly object _lock = new();

        // Snapshot
        public readonly record struct State(
            string btnStart,
            string btnReset,
            string markerLight,
            string brakeLight,
            string turnLight,
            string sensorPosition,
            string sensorConnectorConnected,
            string done
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnStart,
                    btnReset,
                    markerLight,
                    brakeLight,
                    turnLight,
                    sensorPosition,
                    sensorConnectorConnected,
                    done
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.btnStart != null) btnStart = s.btnStart;
                if (s.btnReset != null) btnReset = s.btnReset;
                if (s.markerLight != null) markerLight = s.markerLight;
                if (s.brakeLight != null) brakeLight = s.brakeLight;
                if (s.turnLight != null) turnLight = s.turnLight;
                if (s.sensorPosition != null) sensorPosition = s.sensorPosition;
                if (s.sensorConnectorConnected != null) sensorConnectorConnected = s.sensorConnectorConnected;
                if (s.done != null) done = s.done;
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
            // Find these in UAExpert: DB_ProcessData > CarLight_Input > ...
            // ═══════════════════════════════════════════════════════════
            public static string btnStart { get; set; } = "ns=4;i=?";
            public static string btnReset { get; set; } = "ns=4;i=?";
            public static string markerLight { get; set; } = "ns=4;i=?";
            public static string brakeLight { get; set; } = "ns=4;i=?";
            public static string turnLight { get; set; } = "ns=4;i=?";

            // ═══════════════════════════════════════════════════════════
            // OUTPUT VARIABLES (read FROM PLC)
            // Find these in UAExpert: DB_ProcessData > CarLight_Output > ...
            // ═══════════════════════════════════════════════════════════
            public static string sensorPosition { get; set; } = "ns=4;i=?";
            public static string sensorConnectorConnected { get; set; } = "ns=4;i=?";
            public static string done { get; set; } = "ns=4;i=?";
        }
    }
}
