using JAN0837_DP.Data;
using JAN0837_DP.Forms;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JAN0837_DP.Communication.comMQTT
{
    public class MQTTBroker
    {
        public MqttServer mqttServer;
        public bool mqttserverRunning { get; set; }

        public async Task StartAsync(string ipAddress, int port)
        {
            if (mqttserverRunning) return;

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(port);

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                optionsBuilder.WithDefaultEndpointBoundIPAddress(
                    IPAddress.Parse(ipAddress));
            }

            var factory = new MqttServerFactory();
            mqttServer = factory.CreateMqttServer(optionsBuilder.Build());

            mqttServer.ClientConnectedAsync += e =>
            {
                Console.WriteLine($"Client connected: {e.ClientId}");
                return Task.CompletedTask;
            };

            mqttServer.ClientDisconnectedAsync += e =>
            {
                Console.WriteLine($"Client disconnected: {e.ClientId}");
                return Task.CompletedTask;
            };

            await mqttServer.StartAsync();
            mqttserverRunning = true;

            Console.WriteLine($"Broker running on port {ipAddress}:{port}");
        }

        public async Task StopAsync()
        {
            if (!mqttserverRunning || mqttServer is null) return;

            await mqttServer.StopAsync();
            mqttserverRunning = false;

            Console.WriteLine("Broker stopped");
        }
    }

    public class MQTTClient
    {
        public IMqttClient mqttClient;
        public MqttClientOptions options;
        public CancellationTokenSource cts;

        public bool clientConnected { get; set; }
        public event Action<string, byte[], string>? OnMessage;
        public string[] SubscribeTopics { get; set; } = new[]
        {
            "JAN0837/plc/status",
            "JAN0837/Crossroad/Output"
        };

        public MQTTClient()
        {
            var factory = new MqttClientFactory();
            mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var bytes = e.ApplicationMessage.Payload.ToArray();   
                var payload = bytes.Length == 0 ? "" : Encoding.UTF8.GetString(bytes);

                OnMessage?.Invoke(topic, bytes, payload);

                return Task.CompletedTask;
            };

            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected");

                await SubscribeTopicsAsync();

                // publish online status (retained)
                await PublishAsync("pc/status", "online", retain: true);
            };

            mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine($"Disconnected: {e.Reason}");
                return Task.CompletedTask;
            };
        }

        public Task StartAsync(string host, int port, string clientId)
        {
            if (clientConnected) return Task.CompletedTask;

            cts = new CancellationTokenSource();

            options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithCleanSession(false)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))
                .WithWillTopic("pc/status")
                .WithWillPayload(Encoding.UTF8.GetBytes("offline"))
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithWillRetain(true)
                .Build();

            clientConnected = true;

            _ = Task.Run(async () =>
            {
                while (!cts!.IsCancellationRequested)
                {
                    try
                    {
                        if (!mqttClient.IsConnected)
                            await mqttClient.ConnectAsync(options!, cts.Token);
                    }
                    catch { }

                    await Task.Delay(1000, cts.Token);
                }
            }, cts.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (!clientConnected) return;

            cts?.Cancel();

            if (mqttClient.IsConnected)
            {
                await PublishAsync("pc/status", "offline", retain: true);
                await mqttClient.DisconnectAsync();
            }

            clientConnected = false;
        }

        public async Task SubscribeTopicsAsync()
        {
            if (!mqttClient.IsConnected) return;

            foreach (var t in SubscribeTopics)
            {
                await mqttClient.SubscribeAsync(t, MqttQualityOfServiceLevel.AtLeastOnce);
                Console.WriteLine($"Subscribed to {t}");
            }
        }

        public Task PublishAsync(string topic, string payload, bool retain = false)
        {
            if (!mqttClient.IsConnected) return Task.CompletedTask;

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(retain)
                .Build();

            return mqttClient.PublishAsync(msg);
        }

        public async Task SubscribeAsync(string topic)
        {
            if (!mqttClient.IsConnected)
                return;

            await mqttClient.SubscribeAsync(
                new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build()
            );

            Console.WriteLine($"Subscribed to {topic}");
        }

        public static class CrossroadOutputMapper
        {
            private class OutputDto
            {
                public int lightsMask { get; set; }
                public int crossroadType { get; set; }
            }

            public static void ApplyOutputJsonToCrossroadData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    CrossroadData.crossroadType = dto.crossroadType.ToString();
                    CrossroadData.trafficLight1_green = ((dto.lightsMask >> 0) & 1) == 1 ? "true" : "false";
                    CrossroadData.trafficLight1_yellow = ((dto.lightsMask >> 1) & 1) == 1 ? "true" : "false";
                    CrossroadData.trafficLight1_red = ((dto.lightsMask >> 2) & 1) == 1 ? "true" : "false";
                    CrossroadData.trafficLight2_green = ((dto.lightsMask >> 3) & 1) == 1 ? "true" : "false";
                    CrossroadData.trafficLight2_yellow = ((dto.lightsMask >> 4) & 1) == 1 ? "true" : "false";
                    CrossroadData.trafficLight2_red = ((dto.lightsMask >> 5) & 1) == 1 ? "true" : "false";
                    CrossroadData.pedestrian1_green = ((dto.lightsMask >> 6) & 1) == 1 ? "true" : "false";
                    CrossroadData.pedestrian1_red = ((dto.lightsMask >> 7) & 1) == 1 ? "true" : "false";
                    CrossroadData.pedestrian2_green = ((dto.lightsMask >> 8) & 1) == 1 ? "true" : "false";
                    CrossroadData.pedestrian2_red = ((dto.lightsMask >> 9) & 1) == 1 ? "true" : "false";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Output JSON parse failed: {ex.Message}");
                }
            }
        }
    }
}