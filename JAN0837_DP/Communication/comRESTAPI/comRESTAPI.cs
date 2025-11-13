using JAN0837_DP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace JAN0837_DP.Communication.comRESTAPI
{
    public class comRESTAPI
    {
        public async Task<bool> apiGet(HttpClient client)
        {   
            var state = await client.GetFromJsonAsync<CrossroadData.State>("/data");

            if (state != null)
            {
                CrossroadData.btnCrossroadStart = state.btnCrossroadStart;
                CrossroadData.btnCrossroadPause = state.btnCrossroadPause;
                CrossroadData.btnCrossroadStop = state.btnCrossroadStop;
                CrossroadData.btnCrosswalk1 = state.btnCrosswalk1;
                CrossroadData.btnCrosswalk2 = state.btnCrosswalk2;

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> apiPost(HttpClient client)
        {
            var postData = new
            {
                crossroadType = CrossroadData.crossroadType,
                trafficLight1_green = CrossroadData.trafficLight1_green,
                trafficLight1_yellow = CrossroadData.trafficLight1_yellow,
                trafficLight1_red = CrossroadData.trafficLight1_red,
                trafficLight2_green = CrossroadData.trafficLight2_green,
                trafficLight2_yellow = CrossroadData.trafficLight2_yellow,
                trafficLight2_red = CrossroadData.trafficLight2_red,
                pedestrian1_green = CrossroadData.pedestrian1_green,
                pedestrian1_red = CrossroadData.pedestrian1_red,
                pedestrian2_green = CrossroadData.pedestrian2_green,
                pedestrian2_red = CrossroadData.pedestrian2_red
            };

            var json = System.Text.Json.JsonSerializer.Serialize(postData);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = client.PostAsync("/data", content).Result;
            return await response.Content.ReadAsStringAsync();
        }
    }
}
