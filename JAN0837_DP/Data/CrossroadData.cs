using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JAN0837_DP.Data
{
    public static class CrossroadData
    {
        public const int CrossroadDBnumber = 1;
        public const int CrossroadDBlength = 9999;
        public static byte[] CrossroadReadBuffer { get; set; } = new byte[CrossroadDBlength];
        public static byte[] CrossroadWriteBuffer { get; set; } = new byte[CrossroadDBlength];

        public static string btnCrossroadStart { get; set; } = "";
        public static string btnCrossroadPause { get; set; } = "";
        public static string btnCrossroadStop { get; set; } = "";

        public static string btnCrosswalk1 { get; set; } = "";
        public static string btnCrosswalk2 { get; set; } = "";

        public static string trafficLight1_green { get; set; } = "";
        public static string trafficLight1_yellow { get; set; } = "";
        public static string trafficLight1_red { get; set; } = "";
        public static string trafficLight2_green { get; set; } = "";
        public static string trafficLight2_yellow { get;set; } = "";
        public static string trafficLight2_red { get;set; } = "";
        public static string pedestrian1_green { get; set; } = "";
        public static string pedestrian1_red { get; set; } = "";
        public static string pedestrian2_green { get; set; } = "";
        public static string pedestrian2_red { get; set; } = "";

        // thread safety 
        private static readonly object _lock = new();

        // snapshot
        public readonly record struct State(
            string btnCrossroadStart,
            string btnCrossroadPause,
            string btnCrossroadStop,
            string btnCrosswalk1,
            string btnCrosswalk2,
            string trafficLight1_green,
            string trafficLight1_yellow,
            string trafficLight1_red,
            string trafficLight2_green,
            string trafficLight2_yellow,
            string trafficLight2_red,
            string pedestrian1_green,
            string pedestrian1_red,
            string pedestrian2_green,
            string pedestrian2_red
        );

        public static State Get()
        {
            lock (_lock)
            {
                return new State(
                    btnCrossroadStart,
                    btnCrossroadPause,
                    btnCrossroadStop,
                    btnCrosswalk1,
                    btnCrosswalk2,
                    trafficLight1_green,
                    trafficLight1_yellow,
                    trafficLight1_red,
                    trafficLight2_green,
                    trafficLight2_yellow,
                    trafficLight2_red,
                    pedestrian1_green,
                    pedestrian1_red,
                    pedestrian2_green,
                    pedestrian2_red
                );
            }
        }

        public static void Set(State s)
        {
            lock (_lock)
            {
                btnCrossroadStart = s.btnCrossroadStart;
                btnCrossroadPause = s.btnCrossroadPause;
                btnCrossroadStop = s.btnCrossroadStop;

                btnCrosswalk1 = s.btnCrosswalk1;
                btnCrosswalk2 = s.btnCrosswalk2;

                trafficLight1_green = s.trafficLight1_green;
                trafficLight1_yellow = s.trafficLight1_yellow;
                trafficLight1_red = s.trafficLight1_red;

                trafficLight2_green = s.trafficLight2_green;
                trafficLight2_yellow = s.trafficLight2_yellow;
                trafficLight2_red = s.trafficLight2_red;

                pedestrian1_green = s.pedestrian1_green;
                pedestrian1_red = s.pedestrian1_red;
                pedestrian2_green = s.pedestrian2_green;
                pedestrian2_red = s.pedestrian2_red;
            }
        }

        public static void Update(Action updater)
        {
            if (updater is null) return;
            lock (_lock) updater();
        }

        public static void Reset()
        {
            lock (_lock)
            {
                btnCrossroadStart = btnCrossroadPause = btnCrossroadStop = "";
                btnCrosswalk1 = btnCrosswalk2 = "";
                trafficLight1_green = trafficLight1_yellow = trafficLight1_red = "";
                trafficLight2_green = trafficLight2_yellow = trafficLight2_red = "";
                pedestrian1_green = pedestrian1_red = "";
                pedestrian2_green = pedestrian2_red = "";
                Array.Clear(CrossroadReadBuffer, 0, CrossroadReadBuffer.Length);
                Array.Clear(CrossroadWriteBuffer, 0, CrossroadWriteBuffer.Length);
            }
        }
    }
}
