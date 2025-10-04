using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// MQTT
using MQTTnet;

namespace JAN0837_DP.Communication.comMQTT
{
    public class MQTTBroker
    {
        /*
        
        private IMqttServer? _mqttServer;

        public async Task StartServerAsync()
        {
            var factory = new MqttFactory();
            var options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(1883) // MQTT běží na portu 1883
                .Build();

            _mqttServer = factory.CreateMqttServer();

            // Přidání handlerů
            _mqttServer.ClientConnectedAsync += OnClientConnected;
            _mqttServer.ClientDisconnectedAsync += OnClientDisconnected;
            _mqttServer.InterceptApplicationMessageAsync += OnMessageReceived;

            await _mqttServer.StartAsync(options);
            Console.WriteLine("🚀 MQTT Broker běží na portu 1883...");
        }

        public async Task StopServerAsync()
        {
            if (_mqttServer != null)
            {
                await _mqttServer.StopAsync();
                Console.WriteLine("🔴 MQTT Broker zastaven.");
            }
        }

        private Task OnClientConnected(ClientConnectedEventArgs e)
        {
            Console.WriteLine($"✅ Klient připojen: {e.ClientId}");
            return Task.CompletedTask;
        }

        private Task OnClientDisconnected(ClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"❌ Klient odpojen: {e.ClientId}");
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(InterceptApplicationMessageEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"📩 Přijatá zpráva: {e.ApplicationMessage.Topic} → {message}");
            return Task.CompletedTask;
        }

        */
    }
}