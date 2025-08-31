using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Data
{
    public class crossroad
    {
        public bool btnCrosswalk1 { get; set; } = false;
        public bool btnCrosswalk2 { get; set; } = false;
        public bool trafficLight1_green { get; set; } = false;
        public bool trafficLight1_yellow { get; set; } = false;
        public bool trafficLight1_red { get; set; } = false;
        public bool trafficLight2_green { get; set; } = false;
        public bool trafficLight2_yellow { get;set; } = false;
        public bool trafficLight2_red { get;set; } = false;
        public bool pedestrian1_green { get; set; } = false;
        public bool pedestrian1_red { get; set; } = false;
        public bool pedestrian2_green { get; set; } = false;
        public bool pedestrian2_red { get; set; } = false;
    }
}
