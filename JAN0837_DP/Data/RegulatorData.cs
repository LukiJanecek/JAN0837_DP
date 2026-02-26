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
        // Inputs (written TO PLC)
        public static string switchstate { get; set; } = "false"; // Bool
        public static string R { get; set; } = "0.0"; // Real
        public static string C { get; set; } = "0.0"; // Real
        public static string U { get; set; } = "0.0"; // Real
        public static string Td { get; set; } = "0.0"; // Real

        // Outputs (read FROM PLC)
        public static string Uc { get; set; } = "0.0"; // Real

        // Thread safety 
        private static readonly object _lock = new();

        // Snapshot
        public readonly record struct State(
            string switchstate,
            string R,
            string C,
            string U,
            string Td,
            string Uc
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    switchstate,
                    R,
                    C,
                    U,
                    Td,
                    Uc
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                //if (s.switchstate != null) switchstate = s.switchstate;
                //if (s.R != null) R = s.R;
                //if (s.C != null) C = s.C;
                //if (s.U != null) U = s.U;
                //if (s.Td != null) Td = s.Td;

                if (s.Uc != null) Uc = s.Uc;
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
            public static string switchstate { get; set; } = "ns=4;i=?";
            public static string R { get; set; } = "ns=4;i=?";
            public static string C { get; set; } = "ns=4;i=?";
            public static string U { get; set; } = "ns=4;i=?";
            public static string Td { get; set; } = "ns=4;i=?";

            // Outputs 
            public static string Uc { get; set; } = "ns=4;i=?";
        }
    }
}
