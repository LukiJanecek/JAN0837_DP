using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAN0837_DP.Communication.comMQTT
{
    public class MQTTBroker
    {
        public MqttServer mqttServer;
        public bool mqttserverRunning { get; set; }

        public async Task StartAsync(int port = 1883)
        {
            if (mqttserverRunning) return;
            
            var factory = new MqttServerFactory();
            mqttServer = factory.CreateMqttServer(new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(port)
                .Build());

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

            Console.WriteLine($"Broker running on port {port}");
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

        public MQTTClient()
        {
            var factory = new MqttClientFactory();
            mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;

                var bytes = e.ApplicationMessage.Payload.ToArray();   // ReadOnlySequence<byte> -> byte[]
                var payload = bytes.Length == 0 ? "" : Encoding.UTF8.GetString(bytes);

                Console.WriteLine($"RX {topic}: {payload}");
                return Task.CompletedTask;
            };

            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected");

                // Tady si dej topics podle scénáře
                await SubscribeAsync("plc/line1/telemetry");
                await SubscribeAsync("plc/line1/status");

                // publish online status (retained)
                await PublishAsync("pc/status", "online", retain: true);
            };

            mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine($"Disconnected: {e.Reason}");
                return Task.CompletedTask;
            };
        }

        public async Task StartAsync(string host, int port = 1883, string clientId = "PC_01")
        {
            if (clientConnected) return;

            cts = new CancellationTokenSource();

            options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithCleanSession(false)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(10))

                // Last Will & Testament (MQTTnet 5.x způsob)
                .WithWillTopic("pc/status")
                .WithWillPayload(Encoding.UTF8.GetBytes("offline"))
                .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithWillRetain(true)

                .Build();

            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected");

                // odebírání povelů (PLC->PC nebo PC->PLC dle scénáře)
                await mqttClient.SubscribeAsync("plc/line1/telemetry", MqttQualityOfServiceLevel.AtLeastOnce);
                await mqttClient.SubscribeAsync("plc/line1/status", MqttQualityOfServiceLevel.AtLeastOnce);

                // publish online status (retained)
                await PublishAsync("pc/status", "online", retain: true);
            };

            mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine($"Disconnected: {e.Reason}");
                return Task.CompletedTask;
            };

            clientConnected = true;

            // Reconnect loop
            _ = Task.Run(async () =>
            {
                while (!cts!.IsCancellationRequested)
                {
                    try
                    {
                        if (!mqttClient.IsConnected)
                            await mqttClient.ConnectAsync(options!, cts.Token);
                    }
                    catch
                    {
                        // ticho a zkusit znovu
                    }

                    await Task.Delay(1000, cts.Token);
                }
            }, cts.Token);
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
    }
}