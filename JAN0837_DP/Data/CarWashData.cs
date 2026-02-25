using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public static class CarWashData
    {
        //inputs
        public static string btnEmergencyStop { get; set; } = "false"; // bool
        public static string btnStart { get; set; } = "fasle"; // bool
        public static string btnStop { get; set; } = "false"; // bool
        public static string ErrorSystem { get; set; } = "false"; // bool 
        public static string CarPosition { get; set; } = "false"; // bool
        public static string ShowerPosition { get; set; } = "false"; // bool 
        public static string Mode { get; set; } = ""; // int?

        //outputs
        public static string Light_green { get; set; } = "fasle"; // bool
        public static string Light_yellow { get; set; } = "false"; // bool 
        public static string Light_red { get; set; } = "false"; // bool 
        public static string Door1_Up { get; set; } = "false"; // bool
        public static string Door1_Down { get; set; } = "false"; // bool
        public static string Door2_Up { get; set; } = "false"; // bool
        public static string Door2_Down { get; set; } = "false"; // bool 
        public static string ChemicalsFront { get; set; } = "false"; // bool
        public static string ChemicalsSides { get; set; } = "false"; // bool
        public static string ChemicalsBack { get; set; } = "false"; // bool
        public static string Prewash { get; set; } = "false"; // bool                                                                   
        public static string Water { get; set; } = "false"; // bool
        public static string Wax { get; set; } = "false"; // bool
        public static string Dry { get; set; } = "false"; // bool
        public static string Brushes { get; set; } = "false"; // bool
        public static string Soap { get; set; } = "false"; // bool
        public static string ActiveFoam { get; set; } = "false"; // bool
        public static string TimeDoorMovement { get; set; } = ""; // int? (time)

        // MEMs
        public static string MEMDoor { get; set; } = "false"; // bool
        public static string MEMDoorTrig { get; set; } = "false"; // bool
        public static string MEMDoorClosingtrig { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnEmergencyStop,
            string btnStart,
            string btnStop,
            string ErrorSystem,
            string CarPosition,
            string ShowerPosition,
            string Mode,
            string Light_green,
            string Light_yellow,
            string Light_red,
            string Door1_Up,
            string Door1_Down,
            string Door2_Up,
            string Door2_Down,
            string ChemicalsFront,
            string ChemicalsSides,
            string ChemicalsBack, 
            string Prewash, 
            string Water, 
            string Wax,
            string Dry,
            string Brushes,
            string Soap,
            string ActiveFoam,
            string TimeDoorMovement,
            string MEMDoor,
            string MEMDoorTrig,
            string MEMDoorClosingtrig

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
                    CarPosition,
                    ShowerPosition,
                    Mode,
                    Light_green,
                    Light_yellow,
                    Light_red,
                    Door1_Up,
                    Door1_Down,
                    Door2_Up,
                    Door2_Down,
                    ChemicalsFront,
                    ChemicalsSides,
                    ChemicalsBack,
                    Prewash,
                    Water,
                    Wax,
                    Dry,
                    Brushes,
                    Soap,
                    ActiveFoam,
                    TimeDoorMovement,
                    MEMDoor,
                    MEMDoorTrig,
                    MEMDoorClosingtrig
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.btnEmergencyStop != null) btnEmergencyStop = s.btnEmergencyStop;
                if (s.btnStart != null) btnStart = s.btnStart;
                if (s.btnStop != null) btnStop = s.btnStop;
                if (s.ErrorSystem != null) ErrorSystem = s.ErrorSystem;
                if (s.CarPosition != null) CarPosition = s.CarPosition;
                if (s.ShowerPosition != null) ShowerPosition = s.ShowerPosition;
                if (s.Mode != null) Mode = s.Mode;

                if (s.Light_green != null) Light_green = s.Light_green;
                if (s.Light_yellow != null) Light_yellow = s.Light_yellow;
                if (s.Light_red != null) Light_red = s.Light_red;
                if (s.Door1_Up != null) Door1_Up = s.Door1_Up;
                if (s.Door1_Down != null) Door1_Down = s.Door1_Down;
                if (s.Door2_Up != null) Door2_Up = s.Door2_Up;
                if (s.Door2_Down != null) Door2_Down = s.Door2_Down;
                if (s.ChemicalsFront != null) ChemicalsFront = s.ChemicalsFront;
                if (s.ChemicalsSides != null) ChemicalsSides = s.ChemicalsSides;
                if (s.ChemicalsBack != null) ChemicalsBack = s.ChemicalsBack;
                if (s.Prewash != null) Prewash = s.Prewash;
                if (s.Water != null) Water = s.Water;
                if (s.Wax != null) Wax = s.Wax;
                if (s.Dry != null) Dry = s.Dry;
                if (s.Brushes != null) Brushes = s.Brushes;
                if (s.Soap != null) Soap = s.Soap;
                if (s.ActiveFoam != null) ActiveFoam = s.ActiveFoam;
                if (s.TimeDoorMovement != null) TimeDoorMovement = s.TimeDoorMovement;

                if (s.MEMDoor != null) MEMDoor = s.MEMDoor;
                if (s.MEMDoorTrig != null) MEMDoorTrig = s.MEMDoorTrig;
                if (s.MEMDoorClosingtrig != null) MEMDoorClosingtrig = s.MEMDoorClosingtrig;
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
            public static string ErrorSystem { get; set; } = "ns=4;i=?"; // bool
            public static string CarPosition { get; set; } = "ns=4;i=?"; // bool
            public static string ShowerPosition { get; set; } = "ns=4;i=?"; // bool
            public static string Mode { get; set; } = "ns=4;i=?"; // int?

            // Outputs
            public static string Light_green { get; set; } = "ns=4;i=?"; // bool
            public static string Light_yellow { get; set; } = "ns=4;i=?"; // bool
            public static string Light_red { get; set; } = "ns=4;i=?"; // bool
            public static string Door1_Up { get; set; } = "ns=4;i=?"; // bool
            public static string Door1_Down { get; set; } = "ns=4;i=?"; // bool
            public static string Door2_Up { get; set; } = "ns=4;i=?"; // bool
            public static string Door2_Down { get; set; } = "ns=4;i=?"; // bool
            public static string ChemicalsFront { get; set; } = "ns=4;i=?"; // bool
            public static string ChemicalsSides { get; set; } = "ns=4;i=?"; // bool
            public static string ChemicalsBack { get; set; } = "ns=4;i=?"; // bool
            public static string Prewash { get; set; } = "ns=4;i=?"; // bool
            public static string Water { get; set; } = "ns=4;i=?"; // bool
            public static string Wax { get; set; } = "ns=4;i=?"; // bool
            public static string Dry { get; set; } = "ns=4;i=?"; // bool
            public static string Brushes { get; set; } = "ns=4;i=?"; // bool
            public static string Soap { get; set; } = "ns=4;i=?"; // bool
            public static string ActiveFoam { get; set; } = "ns=4;i=?"; // bool
            public static string TimeDoorMovement { get; set; } = "ns=4;i=?"; // int? (time)

            // MEMs
            public static string MEMDoor { get; set; } = "ns=4;i=?"; // bool
            public static string MEMDoorTrig { get; set; } = "ns=4;i=?"; // bool
            public static string MEMDoorClosingtrig { get; set; } = "ns=4;i=?"; // bool
        }
    }
}
