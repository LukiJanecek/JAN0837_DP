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
    public class CrossroadData
    {
        public const int CrossroadDBnumber = 1;
        public const int CrossroadDBlength = 9999;
        public static byte[] CrossroadReadBuffer { get; set; } = new byte[CrossroadDBlength];
        public static byte[] CrossroadWriteBuffer { get; set; } = new byte[CrossroadDBlength];

        public  string btnCrossroadStart { get; set; } = "";
        public string btnCrossroadPause { get; set; } = "";
        public string btnCrossroadStop { get; set; } = "";

        public string btnCrosswalk1 { get; set; } = "";
        public string btnCrosswalk2 { get; set; } = "";

        public string trafficLight1_green { get; set; } = "";
        public string trafficLight1_yellow { get; set; } = "";
        public string trafficLight1_red { get; set; } = "";
        public string trafficLight2_green { get; set; } = "";
        public string trafficLight2_yellow { get;set; } = "";
        public string trafficLight2_red { get;set; } = "";
        public string pedestrian1_green { get; set; } = "";
        public string pedestrian1_red { get; set; } = "";
        public string pedestrian2_green { get; set; } = "";
        public string pedestrian2_red { get; set; } = "";

        public void Update(CrossroadData other)
        {
            if (other == null)
            {
                return;
            }
            else
            {
                btnCrosswalk1 = other.btnCrosswalk1;
                btnCrosswalk2 = other.btnCrosswalk2;

                btnCrossroadStart = other.btnCrossroadStart;
                btnCrossroadPause = other.btnCrossroadPause;
                btnCrossroadStop = other.btnCrossroadStop;

                trafficLight1_green = other.trafficLight1_green;
                trafficLight1_yellow = other.trafficLight1_yellow;
                trafficLight1_red = other.trafficLight1_red;

                trafficLight2_green = other.trafficLight2_green;
                trafficLight2_yellow = other.trafficLight2_yellow;
                trafficLight2_red = other.trafficLight2_red;

                pedestrian1_green = other.pedestrian1_green;
                pedestrian1_red = other.pedestrian1_red;
                pedestrian2_green = other.pedestrian2_green;
                pedestrian2_red = other.pedestrian2_red;
            }
        }

        public static class AppState
        {
            private static readonly object _lock = new();
            private static CrossroadData _data = new();

            public static CrossroadData Get()
            {
                lock (_lock)
                {
                    return new CrossroadData
                    {
                        btnCrosswalk1 = _data.btnCrosswalk1,
                        btnCrosswalk2 = _data.btnCrosswalk2,
                        btnCrossroadStart = _data.btnCrossroadStart,
                        btnCrossroadPause = _data.btnCrossroadPause,
                        btnCrossroadStop = _data.btnCrossroadStop,
                        trafficLight1_green = _data.trafficLight1_green,
                        trafficLight1_yellow = _data.trafficLight1_yellow,
                        trafficLight1_red = _data.trafficLight1_red,
                        trafficLight2_green = _data.trafficLight2_green,
                        trafficLight2_yellow = _data.trafficLight2_yellow,
                        trafficLight2_red = _data.trafficLight2_red,
                        pedestrian1_green = _data.pedestrian1_green,
                        pedestrian1_red = _data.pedestrian1_red,
                        pedestrian2_green = _data.pedestrian2_green,
                        pedestrian2_red = _data.pedestrian2_red
                    };
                }
            }

            public static void Set(CrossroadData value)
            {
                if (value == null) return;
                lock (_lock)
                {
                    _data.Update(value);
                }
            }
        }
    }
}
