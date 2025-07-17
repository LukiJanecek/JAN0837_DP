using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace JAN0837_DP.Communication.MQTT
{
    public interface MQTT_Mosquito : IHostedService
    {
        List<MQTT_Variables> GetReceivedMessagesData();
        void ProcessingDataToClass(string data);
        Task StartAsync(CancellationToken stoppingToken);
        Task StopAsync(CancellationToken stoppingToken);
    }
}
