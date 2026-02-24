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
        public static string btnCarWashEmergencyStop { get; set; } = "false"; // bool
        public static string btnStartCarWash { get; set; } = "fasle"; // bool
        public static string btnStopCarWash { get; set; } = "false"; // bool
        public static string CarWashErrorSystem { get; set; } = "false"; // bool 
        public static string CarWashCarPosition { get; set; } = "false"; // bool
        public static string CarWashShowerPosition { get; set; } = "false"; // bool 
        public static string CarWashMode { get; set; } = ""; // int?

        //outputs
        public static string CarWashLight_green { get; set; } = "fasle"; // bool
        public static string CarWashLight_yellow { get; set; } = "false"; // bool 
        public static string CarWashLight_red { get; set; } = "false"; // bool 
        public static string CarWashDoor1_Up { get; set; } = "false"; // bool
        public static string CarWashDoor1_Down { get; set; } = "false"; // bool
        public static string CarWashDoor2_Up { get; set; } = "false"; // bool
        public static string CarWashDoor2_Down { get; set; } = "false"; // bool 
        public static string CarWashChemicalsFront { get; set; } = "false"; // bool
        public static string CarWashChemicalsSides { get; set; } = "false"; // bool
        public static string CarWashChemicalsBack { get; set; } = "false"; // bool
        public static string CarWashPrewash { get; set; } = "false"; // bool                                                                   
        public static string CarWashWater { get; set; } = "false"; // bool
        public static string CarWashWax { get; set; } = "false"; // bool
        public static string CarWashDry { get; set; } = "false"; // bool
        public static string CarWashBrushes { get; set; } = "false"; // bool
        public static string CarWashSoap { get; set; } = "false"; // bool
        public static string CarWashActiveFoam { get; set; } = "false"; // bool
        public static string CarWashTimeDoorMovement { get; set; } = ""; // int? (time)

        // MEMs
        public static string CarWashMEMDoor { get; set; } = "false"; // bool
        public static string CarWashMEMDoorTrig { get; set; } = "false"; // bool
        public static string CarWashMEMDoorClosingtrig { get; set; } = "false"; // bool

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnCarWashEmergencyStop,
            string btnStartCarWash,
            string btnStopCarWash,
            string CarWashErrorSystem,
            string CarWashCarPosition,
            string CarWashShowerPosition,
            string CarWashMode,
            string CarWashLight_green,
            string CarWashLight_yellow,
            string CarWashLight_red,
            string CarWashDoor1_Up,
            string CarWashDoor1_Down,
            string CarWashDoor2_Up,
            string CarWashDoor2_Down,
            string CarWashChemicalsFront,
            string CarWashChemicalsSides,
            string CarWashChemicalsBack, 
            string CarWashPrewash, 
            string CarWashWater, 
            string CarWashWax,
            string CarWashDry,
            string CarWashBrushes,
            string CarWashSoap,
            string CarWashActiveFoam,
            string CarWashTimeDoorMovement,
            string CarWashMEMDoor,
            string CarWashMEMDoorTrig,
            string CarWashMEMDoorClosingtrig

        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnCarWashEmergencyStop,
                    btnStartCarWash,
                    btnStopCarWash,
                    CarWashErrorSystem,
                    CarWashCarPosition,
                    CarWashShowerPosition,
                    CarWashMode,
                    CarWashLight_green,
                    CarWashLight_yellow,
                    CarWashLight_red,
                    CarWashDoor1_Up,
                    CarWashDoor1_Down,
                    CarWashDoor2_Up,
                    CarWashDoor2_Down,
                    CarWashChemicalsFront,
                    CarWashChemicalsSides,
                    CarWashChemicalsBack,
                    CarWashPrewash,
                    CarWashWater,
                    CarWashWax,
                    CarWashDry,
                    CarWashBrushes,
                    CarWashSoap,
                    CarWashActiveFoam,
                    CarWashTimeDoorMovement,
                    CarWashMEMDoor,
                    CarWashMEMDoorTrig,
                    CarWashMEMDoorClosingtrig
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                if (s.btnCarWashEmergencyStop != null) btnCarWashEmergencyStop = s.btnCarWashEmergencyStop;
                if (s.btnStartCarWash != null) btnStartCarWash = s.btnStartCarWash;
                if (s.btnStopCarWash != null) btnStopCarWash = s.btnStopCarWash;
                if (s.CarWashErrorSystem != null) CarWashErrorSystem = s.CarWashErrorSystem;
                if (s.CarWashCarPosition != null) CarWashCarPosition = s.CarWashCarPosition;
                if (s.CarWashShowerPosition != null) CarWashShowerPosition = s.CarWashShowerPosition;
                if (s.CarWashMode != null) CarWashMode = s.CarWashMode;

                if (s.CarWashLight_green != null) CarWashLight_green = s.CarWashLight_green;
                if (s.CarWashLight_yellow != null) CarWashLight_yellow = s.CarWashLight_yellow;
                if (s.CarWashLight_red != null) CarWashLight_red = s.CarWashLight_red;
                if (s.CarWashDoor1_Up != null) CarWashDoor1_Up = s.CarWashDoor1_Up;
                if (s.CarWashDoor1_Down != null) CarWashDoor1_Down = s.CarWashDoor1_Down;
                if (s.CarWashDoor2_Up != null) CarWashDoor2_Up = s.CarWashDoor2_Up;
                if (s.CarWashDoor2_Down != null) CarWashDoor2_Down = s.CarWashDoor2_Down;
                if (s.CarWashChemicalsFront != null) CarWashChemicalsFront = s.CarWashChemicalsFront;
                if (s.CarWashChemicalsSides != null) CarWashChemicalsSides = s.CarWashChemicalsSides;
                if (s.CarWashChemicalsBack != null) CarWashChemicalsBack = s.CarWashChemicalsBack;
                if (s.CarWashPrewash != null) CarWashPrewash = s.CarWashPrewash;
                if (s.CarWashWater != null) CarWashWater = s.CarWashWater;
                if (s.CarWashWax != null) CarWashWax = s.CarWashWax;
                if (s.CarWashDry != null) CarWashDry = s.CarWashDry;
                if (s.CarWashBrushes != null) CarWashBrushes = s.CarWashBrushes;
                if (s.CarWashSoap != null) CarWashSoap = s.CarWashSoap;
                if (s.CarWashActiveFoam != null) CarWashActiveFoam = s.CarWashActiveFoam;
                if (s.CarWashTimeDoorMovement != null) CarWashTimeDoorMovement = s.CarWashTimeDoorMovement;

                if (s.CarWashMEMDoor != null) CarWashMEMDoor = s.CarWashMEMDoor;
                if (s.CarWashMEMDoorTrig != null) CarWashMEMDoorTrig = s.CarWashMEMDoorTrig;
                if (s.CarWashMEMDoorClosingtrig != null) CarWashMEMDoorClosingtrig = s.CarWashMEMDoorClosingtrig;
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
            public static string btnCarWashEmergencyStop { get; set; } = "ns=4;i=?"; // bool
            public static string btnStartCarWash { get; set; } = "ns=4;i=?"; // bool
            public static string btnStopCarWash { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashErrorSystem { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashCarPosition { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashShowerPosition { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashMode { get; set; } = "ns=4;i=?"; // int?

            // Outputs
            public static string CarWashLight_green { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashLight_yellow { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashLight_red { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashDoor1_Up { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashDoor1_Down { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashDoor2_Up { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashDoor2_Down { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashChemicalsFront { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashChemicalsSides { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashChemicalsBack { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashPrewash { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashWater { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashWax { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashDry { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashBrushes { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashSoap { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashActiveFoam { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashTimeDoorMovement { get; set; } = "ns=4;i=?"; // int? (time)

            // MEMs
            public static string CarWashMEMDoor { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashMEMDoorTrig { get; set; } = "ns=4;i=?"; // bool
            public static string CarWashMEMDoorClosingtrig { get; set; } = "ns=4;i=?"; // bool
        }
    }
}
