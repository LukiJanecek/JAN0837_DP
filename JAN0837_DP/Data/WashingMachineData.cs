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
        public static string btnWashingMachineEmergencyStop { get; set; } = "false"; // bool
        public static string btnStartWashingMachine { get; set; } = "fasle"; // bool
        public static string btnStopWashingMachine { get; set; } = "false"; // bool
        public static string WashingMachineErrorSystem { get; set; } = "false"; // bool
        public static string WashingMachineMode { get; set; } = ""; // int?

        //outputs
        public static string WashingMachineLight_green { get; set; } = "fasle"; // bool
        public static string WashingMachineLight_yellow { get; set; } = "false"; // bool 
        public static string WashingMachineLight_red { get; set; } = "false"; // bool 
        public static string WashingMachineDoorClosed { get; set; } = "false"; // bool
        public static string WashingMachineChemicals { get; set; } = "false"; // bool
        public static string WashingMachinePrewash { get; set; } = "false"; // bool                                                                   
        public static string WashingMachineWater { get; set; } = "false"; // bool
        public static string WashingMachineWax { get; set; } = "false"; // bool
        public static string WashingMachineDry { get; set; } = "false"; // bool
        public static string WashingMachineBrushes { get; set; } = "false"; // bool
        public static string WashingMachineSoap { get; set; } = "false"; // bool
        public static string WashingMachineActiveFoam { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnWashingMachineEmergencyStop,
            string btnStartWashingMachine,
            string btnStopWashingMachine,
            string WashingMachineErrorSystem,
            string WashingMachineMode,
            string WashingMachineLight_green,
            string WashingMachineLight_yellow,
            string WashingMachineLight_red,
            string WashingMachineDoorClosed,
            string WashingMachineChemicals,
            string WashingMachinePrewash,
            string WashingMachineWater,
            string WashingMachineDry,
            string WashingMachineBrushes,
            string WashingMachineSoap,
            string WashingMachineActiveFoam
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnWashingMachineEmergencyStop, 
                    btnStartWashingMachine, 
                    btnStopWashingMachine, 
                    WashingMachineErrorSystem, 
                    WashingMachineMode, 
                    WashingMachineLight_green,
                    WashingMachineLight_yellow,
                    WashingMachineLight_red,
                    WashingMachineDoorClosed,
                    WashingMachineChemicals,
                    WashingMachinePrewash,
                    WashingMachineWater,
                    WashingMachineDry,
                    WashingMachineBrushes,
                    WashingMachineSoap,
                    WashingMachineActiveFoam
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                btnWashingMachineEmergencyStop = s.btnWashingMachineEmergencyStop; // ?
                btnStartWashingMachine = s.btnStartWashingMachine; // ? 
                btnStopWashingMachine = s.btnStopWashingMachine; // ? 
                WashingMachineErrorSystem = s.WashingMachineErrorSystem; // ? 
                WashingMachineMode = s.WashingMachineMode; // ? 

                WashingMachineLight_green = s.WashingMachineLight_green;
                WashingMachineLight_yellow = s.WashingMachineLight_yellow;
                WashingMachineLight_red = s.WashingMachineLight_red;
                WashingMachineDoorClosed = s.WashingMachineDoorClosed;
                WashingMachineChemicals = s.WashingMachineChemicals;
                WashingMachinePrewash = s.WashingMachinePrewash;
                WashingMachineWater = s.WashingMachineWater;
                WashingMachineDry = s.WashingMachineDry;
                WashingMachineBrushes = s.WashingMachineBrushes;
                WashingMachineSoap = s.WashingMachineSoap;
                WashingMachineActiveFoam = s.WashingMachineActiveFoam;
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
            public static string btnWashingMachineEmergencyStop { get; set; } = "ns=4;i=?"; // bool
            public static string btnStartWashingMachine { get; set; } = "ns=4;i=?"; // bool
            public static string btnStopWashingMachine { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineErrorSystem { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineMode { get; set; } = "ns=4;i=?"; // int?

            // Outputs
            public static string WashingMachineLight_green { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineLight_yellow { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineLight_red { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineDoorClosed { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineChemicals { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachinePrewash { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineWater { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineWax { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineDry { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineBrushes { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineSoap { get; set; } = "ns=4;i=?"; // bool
            public static string WashingMachineActiveFoam { get; set; } = "ns=4;i=?"; // bool
        }
    }
}
