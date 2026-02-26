using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JAN0837_DP.Data
{
    public static class WashingMachineData
    {
        //inputs
        public static string btnEmergencyStop { get; set; } = "false"; // bool
        public static string btnStart { get; set; } = "fasle"; // bool
        public static string btnStop { get; set; } = "false"; // bool
        public static string ErrorSystem { get; set; } = "false"; // bool
        public static string Mode { get; set; } = ""; // int?

        //outputs
        public static string Light_green { get; set; } = "fasle"; // bool
        public static string Light_yellow { get; set; } = "false"; // bool 
        public static string Light_red { get; set; } = "false"; // bool 
        public static string DoorClosed { get; set; } = "false"; // bool
        public static string Chemicals { get; set; } = "false"; // bool
        public static string Prewash { get; set; } = "false"; // bool                                                                   
        public static string Water { get; set; } = "false"; // bool
        public static string Wax { get; set; } = "false"; // bool
        public static string Dry { get; set; } = "false"; // bool
        public static string Brushes { get; set; } = "false"; // bool
        public static string Soap { get; set; } = "false"; // bool
        public static string ActiveFoam { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnEmergencyStop,
            string btnStart,
            string btnStop,
            string ErrorSystem,
            string Mode,
            string Light_green,
            string Light_yellow,
            string Light_red,
            string DoorClosed,
            string Chemicals,
            string Prewash,
            string Water,
            string Dry,
            string Brushes,
            string Soap,
            string ActiveFoam
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnEmergencyStop, 
                    btnStart, 
                    btnStop, 
                    ErrorSystem, 
                    Mode, 
                    Light_green,
                    Light_yellow,
                    Light_red,
                    DoorClosed,
                    Chemicals,
                    Prewash,
                    Water,
                    Dry,
                    Brushes,
                    Soap,
                    ActiveFoam
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                //if (s.btnEmergencyStop != null) btnEmergencyStop = s.btnEmergencyStop;
                //if (s.btnStart != null) btnStart = s.btnStart;
                //if (s.btnStop != null) btnStop = s.btnStop;
                //if (s.ErrorSystem != null) ErrorSystem = s.ErrorSystem;
                //if (s.Mode != null) Mode = s.Mode;

                if (s.Light_green != null) Light_green = s.Light_green;
                if (s.Light_yellow != null) Light_yellow = s.Light_yellow;
                if (s.Light_red != null) Light_red = s.Light_red;
                if (s.DoorClosed != null) DoorClosed = s.DoorClosed;
                if (s.Chemicals != null) Chemicals = s.Chemicals;
                if (s.Prewash != null) Prewash = s.Prewash;
                if (s.Water != null) Water = s.Water;
                if (s.Dry != null) Dry = s.Dry;
                if (s.Brushes != null) Brushes = s.Brushes;
                if (s.Soap != null) Soap = s.Soap;
                if (s.ActiveFoam != null) ActiveFoam = s.ActiveFoam;
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
            public static string btnEmergencyStop { get; set; } = "ns=4;i=?"; // bool
            public static string btnStart { get; set; } = "ns=4;i=?"; // bool
            public static string btnStop { get; set; } = "ns=4;i=?"; // bool
            public static string Washing { get; set; } = "ns=4;i=?"; // bool
            public static string ErrorSystem { get; set; } = "ns=4;i=?"; // bool
            public static string Mode { get; set; } = "ns=4;i=?"; // int?

            // Outputs
            public static string Light_green { get; set; } = "ns=4;i=?"; // bool
            public static string Light_yellow { get; set; } = "ns=4;i=?"; // bool
            public static string Light_red { get; set; } = "ns=4;i=?"; // bool
            public static string DoorClosed { get; set; } = "ns=4;i=?"; // bool
            public static string Chemicals { get; set; } = "ns=4;i=?"; // bool
            public static string Prewash { get; set; } = "ns=4;i=?"; // bool
            public static string Water { get; set; } = "ns=4;i=?"; // bool
            public static string Wax { get; set; } = "ns=4;i=?"; // bool
            public static string Dry { get; set; } = "ns=4;i=?"; // bool
            public static string Brushes { get; set; } = "ns=4;i=?"; // bool
            public static string Soap { get; set; } = "ns=4;i=?"; // bool
            public static string ActiveFoam { get; set; } = "ns=4;i=?"; // bool
        }
    }
}
