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
        // Inputs 
        public static string btnReset { get; set; } = "false"; // Bool
        public static string switchstate { get; set; } = "false"; // Bool
        public static string order { get; set; } = "1"; // int
        public static string R1 { get; set; } = "0.0"; // Real
        public static string R2 { get; set; } = "0.0"; // Real
        public static string C1 { get; set; } = "0.0"; // Real
        public static string C2 { get; set; } = "0.0"; // Real
        public static string Uin { get; set; } = "0.0"; // Real
        public static string Td { get; set; } = "0.0"; // Real -> transport delay                                                  
        public static string Ts { get; set; } = "0.1"; // sampling time

        // Outputs 
        public static string Uc1 { get; set; } = "0.0"; // Real
        public static string Uc2 { get; set; } = "0.0"; // Real

        // Thread safety 
        private static readonly object _lock = new();

        // Snapshot
        public readonly record struct State(
            string switchstate,
            string order,
            string R1,
            string R2,
            string C1,
            string C2,
            string Uin,
            string Td,
            string Ts,
            string Uc1,
            string Uc2
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    switchstate,
                    order,
                    R1,
                    R2,
                    C1,
                    C2,
                    Uin,
                    Td,
                    Ts,
                    Uc1,
                    Uc2
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

                if (s.Uc1 != null) Uc1 = s.Uc1;
                if (s.Uc2 != null) Uc2 = s.Uc2;
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
            public static string order { get; set; } = "ns=4;i=?";
            public static string R1 { get; set; } = "ns=4;i=?";
            public static string R2 { get; set; } = "ns=4;i=?";
            public static string C1 { get; set; } = "ns=4;i=?";
            public static string C2 { get; set; } = "ns=4;i=?";
            public static string Uin { get; set; } = "ns=4;i=?";
            public static string Td { get; set; } = "ns=4;i=?";
            public static string Ts { get; set; } = "ns=4;i=?";

            // Outputs 
            public static string Uc1 { get; set; } = "ns=4;i=?";
            public static string Uc2 { get; set; } = "ns=4;i=?";
        }
    }
}
