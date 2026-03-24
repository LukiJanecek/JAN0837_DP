using JAN0837_DP.Data;
using JAN0837_DP.Forms;
using JAN0837_DP.Log;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Siemens.Engineering.HW;
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
            "JAN0837/Crossroad/Output",
            "JAN0837/Crosswalk/Output",
            "JAN0837/Regulator/Output",
            "JAN0837/CarLight/Output"
            //"JAN0837/CarWash/Output",
            //"JAN0837/WashingMachine/Output"
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

            _ = Task.Run(async () =>
            {
                while (!cts!.IsCancellationRequested)
                {
                    try
                    {
                        if (!mqttClient.IsConnected)
                            await mqttClient.ConnectAsync(options!, cts.Token);

                        await Task.Delay(1000, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // StopAsync cancelled the token – exit cleanly
                        break;
                    }
                    catch
                    {
                        Logger.LogError("Connection failed, retry in next loop.");

                        try { await Task.Delay(1000, cts.Token); }
                        catch (OperationCanceledException) { break; }
                    }
                }
            }, cts.Token);

            clientConnected = true;
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (!clientConnected) return;

            // Publish offline status and disconnect BEFORE cancelling the reconnect loop,
            // otherwise the loop could attempt a reconnect during shutdown.
            if (mqttClient.IsConnected)
            {
                await PublishAsync("pc/status", "offline", retain: true);
                await mqttClient.DisconnectAsync();
            }

            cts?.Cancel();
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
                public bool? trafficLightNorth_green { get; set; }
                public bool? trafficLightNorth_yellow { get; set; }
                public bool? trafficLightNorth_red { get; set; }
                public bool? trafficLightSouth_green { get; set; }
                public bool? trafficLightSouth_yellow { get; set; }
                public bool? trafficLightSouth_red { get; set; }
                public bool? trafficLightEast_green { get; set; }
                public bool? trafficLightEast_yellow { get; set; }
                public bool? trafficLightEast_red { get; set; }
                public bool? trafficLightWest_green { get; set; }
                public bool? trafficLightWest_yellow { get; set; }
                public bool? trafficLightWest_red { get; set; }
                public bool? pedestrianSouth1_green { get; set; }
                public bool? pedestrianSouth1_red { get; set; }
                public bool? pedestrianSouth2_green { get; set; }
                public bool? pedestrianSouth2_red { get; set; }
                public bool? pedestrianWest1_green { get; set; }
                public bool? pedestrianWest1_red { get; set; }
                public bool? pedestrianWest2_green { get; set; }
                public bool? pedestrianWest2_red { get; set; }
            }

            public static void ApplyOutputJsonToCrossroadData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    CrossroadData.crossroadType = dto.crossroadType.ToString();

                    // Preferovat lightsMask, pokud je > 0, jinak jednotlivé hodnoty
                    if (dto.lightsMask > 0)
                    {
                        CrossroadData.trafficLightNorth_green = ((dto.lightsMask >> 0) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightNorth_yellow = ((dto.lightsMask >> 1) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightNorth_red = ((dto.lightsMask >> 2) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightSouth_green = ((dto.lightsMask >> 3) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightSouth_yellow = ((dto.lightsMask >> 4) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightSouth_red = ((dto.lightsMask >> 5) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightEast_green = ((dto.lightsMask >> 6) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightEast_yellow = ((dto.lightsMask >> 7) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightEast_red = ((dto.lightsMask >> 8) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightWest_green = ((dto.lightsMask >> 9) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightWest_yellow = ((dto.lightsMask >> 10) & 1) == 1 ? "true" : "false";
                        CrossroadData.trafficLightWest_red = ((dto.lightsMask >> 11) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianSouth1_green = ((dto.lightsMask >> 12) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianSouth1_red = ((dto.lightsMask >> 13) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianSouth2_green = ((dto.lightsMask >> 14) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianSouth2_red = ((dto.lightsMask >> 15) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianWest1_green = ((dto.lightsMask >> 16) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianWest1_red = ((dto.lightsMask >> 17) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianWest2_green = ((dto.lightsMask >> 18) & 1) == 1 ? "true" : "false";
                        CrossroadData.pedestrianWest2_red = ((dto.lightsMask >> 19) & 1) == 1 ? "true" : "false";
                    }
                    else
                    {
                        CrossroadData.trafficLightNorth_green = dto.trafficLightNorth_green?.ToString().ToLower() ?? CrossroadData.trafficLightNorth_green;
                        CrossroadData.trafficLightNorth_yellow = dto.trafficLightNorth_yellow?.ToString().ToLower() ?? CrossroadData.trafficLightNorth_yellow;
                        CrossroadData.trafficLightNorth_red = dto.trafficLightNorth_red?.ToString().ToLower() ?? CrossroadData.trafficLightNorth_red;
                        CrossroadData.trafficLightSouth_green = dto.trafficLightSouth_green?.ToString().ToLower() ?? CrossroadData.trafficLightSouth_green;
                        CrossroadData.trafficLightSouth_yellow = dto.trafficLightSouth_yellow?.ToString().ToLower() ?? CrossroadData.trafficLightSouth_yellow;
                        CrossroadData.trafficLightSouth_red = dto.trafficLightSouth_red?.ToString().ToLower() ?? CrossroadData.trafficLightSouth_red;
                        CrossroadData.trafficLightEast_green = dto.trafficLightEast_green?.ToString().ToLower() ?? CrossroadData.trafficLightEast_green;
                        CrossroadData.trafficLightEast_yellow = dto.trafficLightEast_yellow?.ToString().ToLower() ?? CrossroadData.trafficLightEast_yellow;
                        CrossroadData.trafficLightEast_red = dto.trafficLightEast_red?.ToString().ToLower() ?? CrossroadData.trafficLightEast_red;
                        CrossroadData.trafficLightWest_green = dto.trafficLightWest_green?.ToString().ToLower() ?? CrossroadData.trafficLightWest_green;
                        CrossroadData.trafficLightWest_yellow = dto.trafficLightWest_yellow?.ToString().ToLower() ?? CrossroadData.trafficLightWest_yellow;
                        CrossroadData.trafficLightWest_red = dto.trafficLightWest_red?.ToString().ToLower() ?? CrossroadData.trafficLightWest_red;
                        CrossroadData.pedestrianSouth1_green = dto.pedestrianSouth1_green?.ToString().ToLower() ?? CrossroadData.pedestrianSouth1_green;
                        CrossroadData.pedestrianSouth1_red = dto.pedestrianSouth1_red?.ToString().ToLower() ?? CrossroadData.pedestrianSouth1_red;
                        CrossroadData.pedestrianSouth2_green = dto.pedestrianSouth2_green?.ToString().ToLower() ?? CrossroadData.pedestrianSouth2_green;
                        CrossroadData.pedestrianSouth2_red = dto.pedestrianSouth2_red?.ToString().ToLower() ?? CrossroadData.pedestrianSouth2_red;
                        CrossroadData.pedestrianWest1_green = dto.pedestrianWest1_green?.ToString().ToLower() ?? CrossroadData.pedestrianWest1_green;
                        CrossroadData.pedestrianWest1_red = dto.pedestrianWest1_red?.ToString().ToLower() ?? CrossroadData.pedestrianWest1_red;
                        CrossroadData.pedestrianWest2_green = dto.pedestrianWest2_green?.ToString().ToLower() ?? CrossroadData.pedestrianWest2_green;
                        CrossroadData.pedestrianWest2_red = dto.pedestrianWest2_red?.ToString().ToLower() ?? CrossroadData.pedestrianWest2_red;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse output JSON");
                }
            }
        }

        public static class CrosswalkOutputMapper
        {
            private class OutputDto
            {
                public int lightsMask { get; set; }
                public int crosswalkType { get; set; }
                public bool? trafficLight1_green { get; set; }
                public bool? trafficLight1_yellow { get; set; }
                public bool? trafficLight1_red { get; set; }
                public bool? trafficLight2_green { get; set; }
                public bool? trafficLight2_yellow { get; set; }
                public bool? trafficLight2_red { get; set; }
                public bool? pedestrian1_green { get; set; }
                public bool? pedestrian1_red { get; set; }
                public bool? pedestrian2_green { get; set; }
                public bool? pedestrian2_red { get; set; }
            }

            public static void ApplyOutputJsonToCrosswalkData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    CrosswalkData.crosswalkType = dto.crosswalkType.ToString();

                    if (dto.lightsMask > 0)
                    {
                        CrosswalkData.trafficLight1_green = ((dto.lightsMask >> 0) & 1) == 1 ? "true" : "false";
                        CrosswalkData.trafficLight1_yellow = ((dto.lightsMask >> 1) & 1) == 1 ? "true" : "false";
                        CrosswalkData.trafficLight1_red = ((dto.lightsMask >> 2) & 1) == 1 ? "true" : "false";
                        CrosswalkData.trafficLight2_green = ((dto.lightsMask >> 3) & 1) == 1 ? "true" : "false";
                        CrosswalkData.trafficLight2_yellow = ((dto.lightsMask >> 4) & 1) == 1 ? "true" : "false";
                        CrosswalkData.trafficLight2_red = ((dto.lightsMask >> 5) & 1) == 1 ? "true" : "false";
                        CrosswalkData.pedestrian1_green = ((dto.lightsMask >> 6) & 1) == 1 ? "true" : "false";
                        CrosswalkData.pedestrian1_red = ((dto.lightsMask >> 7) & 1) == 1 ? "true" : "false";
                        CrosswalkData.pedestrian2_green = ((dto.lightsMask >> 8) & 1) == 1 ? "true" : "false";
                        CrosswalkData.pedestrian2_red = ((dto.lightsMask >> 9) & 1) == 1 ? "true" : "false";
                    }
                    else
                    {
                        CrosswalkData.trafficLight1_green = dto.trafficLight1_green?.ToString().ToLower() ?? CrosswalkData.trafficLight1_green;
                        CrosswalkData.trafficLight1_yellow = dto.trafficLight1_yellow?.ToString().ToLower() ?? CrosswalkData.trafficLight1_yellow;
                        CrosswalkData.trafficLight1_red = dto.trafficLight1_red?.ToString().ToLower() ?? CrosswalkData.trafficLight1_red;
                        CrosswalkData.trafficLight2_green = dto.trafficLight2_green?.ToString().ToLower() ?? CrosswalkData.trafficLight2_green;
                        CrosswalkData.trafficLight2_yellow = dto.trafficLight2_yellow?.ToString().ToLower() ?? CrosswalkData.trafficLight2_yellow;
                        CrosswalkData.trafficLight2_red = dto.trafficLight2_red?.ToString().ToLower() ?? CrosswalkData.trafficLight2_red;
                        CrosswalkData.pedestrian1_green = dto.pedestrian1_green?.ToString().ToLower() ?? CrosswalkData.pedestrian1_green;
                        CrosswalkData.pedestrian1_red = dto.pedestrian1_red?.ToString().ToLower() ?? CrosswalkData.pedestrian1_red;
                        CrosswalkData.pedestrian2_green = dto.pedestrian2_green?.ToString().ToLower() ?? CrosswalkData.pedestrian2_green;
                        CrosswalkData.pedestrian2_red = dto.pedestrian2_red?.ToString().ToLower() ?? CrosswalkData.pedestrian2_red;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Crosswalk output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse Crosswalk output JSON");
                }
            }
        }

        public static class RegulatorOutputMapper
        {
            private class OutputDto
            {
                public string Uin { get; set; }
            }

            public static void ApplyOutputJsonToRegulatorData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    RegulatorData.Uin = dto.Uin ?? "0.0";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Regulator output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse Regulator output JSON");
                }
            }
        }

        public static class CarLightOutputMapper
        {
            private class OutputDto
            {
                public bool lowBeamLight { get; set; }
                public bool highBeamLight { get; set; }
                public bool turnLight { get; set; }
            }
            public static void ApplyOutputJsonToCarLightData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;
                    CarLightData.lowBeamLight = dto.lowBeamLight ? "true" : "false";
                    CarLightData.highBeamLight = dto.highBeamLight ? "true" : "false";
                    CarLightData.turnLight = dto.turnLight ? "true" : "false";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CarLight output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse CarLight output JSON");
                }
            }
        }

        public static class CarWashOutputMapper
        {
            private class OutputDto
            {
                public int statusMask { get; set; }
                public string mode { get; set; }
            }

            /*
            public static void ApplyOutputJsonToCarWashData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    CarWashData.Light_green = ((dto.statusMask >> 0) & 1) == 1 ? "true" : "false";
                    CarWashData.Light_yellow = ((dto.statusMask >> 1) & 1) == 1 ? "true" : "false";
                    CarWashData.Light_red = ((dto.statusMask >> 2) & 1) == 1 ? "true" : "false";
                    CarWashData.Door1_Up = ((dto.statusMask >> 3) & 1) == 1 ? "true" : "false";
                    CarWashData.Door1_Down = ((dto.statusMask >> 4) & 1) == 1 ? "true" : "false";
                    CarWashData.Door2_Up = ((dto.statusMask >> 5) & 1) == 1 ? "true" : "false";
                    CarWashData.Door2_Down = ((dto.statusMask >> 6) & 1) == 1 ? "true" : "false";
                    CarWashData.ChemicalsFront = ((dto.statusMask >> 7) & 1) == 1 ? "true" : "false";
                    CarWashData.ChemicalsSides = ((dto.statusMask >> 8) & 1) == 1 ? "true" : "false";
                    CarWashData.ChemicalsBack = ((dto.statusMask >> 9) & 1) == 1 ? "true" : "false";
                    CarWashData.Prewash = ((dto.statusMask >> 10) & 1) == 1 ? "true" : "false";
                    CarWashData.Water = ((dto.statusMask >> 11) & 1) == 1 ? "true" : "false";
                    CarWashData.Wax = ((dto.statusMask >> 12) & 1) == 1 ? "true" : "false";
                    CarWashData.Dry = ((dto.statusMask >> 13) & 1) == 1 ? "true" : "false";
                    CarWashData.Brushes = ((dto.statusMask >> 14) & 1) == 1 ? "true" : "false";
                    CarWashData.Soap = ((dto.statusMask >> 15) & 1) == 1 ? "true" : "false";
                    CarWashData.ActiveFoam = ((dto.statusMask >> 16) & 1) == 1 ? "true" : "false";
                    CarWashData.Mode = dto.mode ?? "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CarWash output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse CarWash output JSON");
                }
            }
            */
        }

        public static class WashingMachineOutputMapper
        {
            private class OutputDto
            {
                public int statusMask { get; set; }
                public string mode { get; set; }
            }

            /*
            public static void ApplyOutputJsonToWashingMachineData(string json)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<OutputDto>(json);
                    if (dto is null) return;

                    WashingMachineData.Light_green = ((dto.statusMask >> 0) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Light_yellow = ((dto.statusMask >> 1) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Light_red = ((dto.statusMask >> 2) & 1) == 1 ? "true" : "false";
                    WashingMachineData.DoorClosed = ((dto.statusMask >> 3) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Chemicals = ((dto.statusMask >> 4) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Prewash = ((dto.statusMask >> 5) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Water = ((dto.statusMask >> 6) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Dry = ((dto.statusMask >> 7) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Brushes = ((dto.statusMask >> 8) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Soap = ((dto.statusMask >> 9) & 1) == 1 ? "true" : "false";
                    WashingMachineData.ActiveFoam = ((dto.statusMask >> 10) & 1) == 1 ? "true" : "false";
                    WashingMachineData.Mode = dto.mode ?? "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WashingMachine output JSON parse failed: {ex.Message}");
                    Logger.LogException(ex, "Failed to parse WashingMachine output JSON");
                }
            }
            */
        }
    }
}