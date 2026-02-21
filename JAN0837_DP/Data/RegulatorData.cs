using Microsoft.VisualBasic.Devices;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class RegulatorData
    {
        // inputs 
        public static string btnStart { get; set; } = "false"; // bool
        public static string R { get; set; } = ""; // int
        public static string C { get; set; } = ""; // int
        public static string U { get; set; } = ""; // int
        public static string I { get; set; } = ""; // int

        // outputs 

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnStart,
            string R,
            string C,
            string U,
            string I
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnStart,
                    R,
                    C,
                    U,
                    I
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                btnStart = s.btnStart; // ?
                R = s.R; // ?
                C = s.C; // ? 
                U = s.U; // ? 
                I = s.I; // ?
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
            public static string btnStart { get; set; } = "ns=4;i=?";
            public static string R { get; set; } = "ns=4;i=?";
            public static string C { get; set; } = "ns=4;i=?";
            public static string U { get; set; } = "ns=4;i=?";
            public static string I { get; set; } = "ns=4;i=?";

            // ═══════════════════════════════════════════════════════════
            // OUTPUT VARIABLES (read FROM PLC)
            // Find these in UAExpert: DB_ProcessData > output > ...
            // ═══════════════════════════════════════════════════════════
            
        }
    }
}
