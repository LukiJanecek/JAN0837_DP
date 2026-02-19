using JAN0837_DP.Communication.comModbusTCPIP;
using JAN0837_DP.Communication.comMQTT;
using JAN0837_DP.Communication.comOPCUA;
using JAN0837_DP.Communication.comS7;
using JAN0837_DP.Communication.comSharp7;
using JAN0837_DP.Communication.comTCPIP;
using JAN0837_DP.Data;
using JAN0837_DP.Forms;
using JAN0837_DP.Log;
using Microsoft.AspNetCore.Connections.Features;
using Newtonsoft.Json;
using Opc.Ua;
using Siemens.Engineering.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace JAN0837_DP.Communication
{
    public class CommunicationManager
    {
        public comS7.comS7 _s7;
        public comSharp7.comSharp7 _sharp7;
        public comTCPIPClient _tcpipClient;   // TCP/IP Client
        public comTCPIPServer _tcpipServer;   // TCP/IP Server
        public comModbusTCPIP.ModbusTCPIPimClient _modbusClient;  // Modbus TCP Client
        public comModbusTCPIP.ModbusTCPIPimServer _modbusServer;  // Modbus TCP Server
        public MQTTBroker _mqttBroker;
        public MQTTClient _mqttClient;
        public comOPCUA.opcuaKlient _opcuaClient;      // Client for external PLC server (slave mode)
        public comOPCUA.opcuaServer _opcuaServer;      // Our server (master mode)
        public comOPCUA.opcuaKlient _opcuaInternalClient;  // Internal client connecting to our own server (master mode)

        public ucCommunicationControl _ucCommunicationControl;

        public CommunicationManager(ucCommunicationControl ucCommunicationControl)
        {
            _ucCommunicationControl = ucCommunicationControl;
        }

        public async Task Communication(CancellationToken token) 
        {
            try
            {
                while (internalVariables.communicationThreadRunningFlag && !token.IsCancellationRequested) // communicationRunningFlag
                {
                    switch (internalVariables.communicationFlag)
                    {
                        case "MQTT":

                            var client = _ucCommunicationControl._mqttClient;

                            string broker_ipAddress = internalVariables.txtBoxParam1;
                            if (!int.TryParse(internalVariables.txtBoxParam2.Trim(), out int broker_port))
                            {
                                _ucCommunicationControl.SetStatus("Port is not a valid number.");
                                return; // break
                            }

                            // code for master and slave is the same, it could change via new functions in future
                            if (internalVariables.checkBoxMaster == true)
                            {
                                // publish
                                if (client == null || client.mqttClient == null || !client.mqttClient.IsConnected)
                                {
                                    await Task.Delay(200, token);
                                    continue; // NE break
                                }

                                var obj = new
                                {
                                    start = CrossroadData.btnCrossroadStart == "true",
                                    pause = CrossroadData.btnCrossroadPause == "true",
                                    stop = CrossroadData.btnCrossroadStop == "true",
                                    cw1 = CrossroadData.btnCrosswalk1 == "true",
                                    cw2 = CrossroadData.btnCrosswalk2 == "true"
                                };

                                string json = System.Text.Json.JsonSerializer.Serialize(obj);

                                var msg = new MQTTnet.MqttApplicationMessageBuilder()
                                    .WithTopic("JAN0837/Crossroad/Input")
                                    .WithPayload(json)
                                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                    .WithRetainFlag(true)
                                    .Build();

                                try
                                {
                                    await client.mqttClient.PublishAsync(msg, token);
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error: {ex}");
                                    Logger.LogException(ex, "Exception error: ");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                if (client == null || client.mqttClient == null || !client.mqttClient.IsConnected)
                                {
                                    await Task.Delay(200, token);
                                    continue; // NE break
                                }

                                var obj = new
                                {
                                    start = CrossroadData.btnCrossroadStart == "true",
                                    pause = CrossroadData.btnCrossroadPause == "true",
                                    stop = CrossroadData.btnCrossroadStop == "true",
                                    cw1 = CrossroadData.btnCrosswalk1 == "true",
                                    cw2 = CrossroadData.btnCrosswalk2 == "true"
                                };

                                string json = System.Text.Json.JsonSerializer.Serialize(obj);

                                var msg = new MQTTnet.MqttApplicationMessageBuilder()
                                    .WithTopic("JAN0837/Crossroad/Input")
                                    .WithPayload(json)
                                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                    .WithRetainFlag(true)
                                    .Build();

                                await client.mqttClient.PublishAsync(msg);
                            }
                            else
                            {
                                // no checkbox selected 
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "OPCUA":
                            string opcUaServerUrl = internalVariables.txtBoxParam1;

                            if (internalVariables.checkBoxMaster == true)
                            {
                                // ═══════════════════════════════════════════════════════════
                                // SERVER MODE (like MQTT Broker)
                                // ═══════════════════════════════════════════════════════════
                                // Architecture (correct):
                                //   - OPC UA Server = Data broker (stores variables)
                                //   - Internal Client = Syncs CrossroadData ↔ Server
                                //   - External Clients (PLC/SCADA) = Read/Write to server
                                //
                                // Current implementation uses direct server access for simplicity
                                // TODO: Create internal client for proper separation
                                // ═══════════════════════════════════════════════════════════
                                
                                var opcuaServer = _ucCommunicationControl._opcuaServer;

                                if (opcuaServer == null || !opcuaServer.running)
                                {
                                    await Task.Delay(200, token);
                                    continue;
                                }

                                try
                                {
                                    // Read FROM server (commands written by external clients like PLC)
                                    CrossroadData.btnCrossroadStart = opcuaServer.ReadVariable("BtnCrossroadStart") ? "true" : "false";
                                    CrossroadData.btnCrossroadPause = opcuaServer.ReadVariable("BtnCrossroadPause") ? "true" : "false";
                                    CrossroadData.btnCrossroadStop = opcuaServer.ReadVariable("BtnCrossroadStop") ? "true" : "false";
                                    CrossroadData.btnCrosswalk1 = opcuaServer.ReadVariable("BtnCrosswalk1") ? "true" : "false";
                                    CrossroadData.btnCrosswalk2 = opcuaServer.ReadVariable("BtnCrosswalk2") ? "true" : "false";

                                    // Write TO server (so external clients can read current state)
                                    opcuaServer.UpdateVariable("CrossroadType", CrossroadData.crossroadType == "true");
                                    opcuaServer.UpdateVariable("TrafficLight1_Green", CrossroadData.trafficLight1_green == "true");
                                    opcuaServer.UpdateVariable("TrafficLight1_Yellow", CrossroadData.trafficLight1_yellow == "true");
                                    opcuaServer.UpdateVariable("TrafficLight1_Red", CrossroadData.trafficLight1_red == "true");
                                    opcuaServer.UpdateVariable("TrafficLight2_Green", CrossroadData.trafficLight2_green == "true");
                                    opcuaServer.UpdateVariable("TrafficLight2_Yellow", CrossroadData.trafficLight2_yellow == "true");
                                    opcuaServer.UpdateVariable("TrafficLight2_Red", CrossroadData.trafficLight2_red == "true");
                                    opcuaServer.UpdateVariable("Pedestrian1_Green", CrossroadData.pedestrian1_green == "true");
                                    opcuaServer.UpdateVariable("Pedestrian1_Red", CrossroadData.pedestrian1_red == "true");
                                    opcuaServer.UpdateVariable("Pedestrian2_Green", CrossroadData.pedestrian2_green == "true");
                                    opcuaServer.UpdateVariable("Pedestrian2_Red", CrossroadData.pedestrian2_red == "true");

                                    _ucCommunicationControl.SetStatus("OPC UA Server: Hosting for external clients");
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"OPC UA Server error: {ex.Message}");
                                    Logger.LogException(ex, "OPC UA Server error:");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                                /*
                                var opcuaClient = _ucCommunicationControl._opcuaClient;

                                if (opcuaClient == null || !opcuaClient.connected)
                                {
                                    await Task.Delay(200, token);
                                    continue;
                                }

                                try
                                {
                                    opcuaClient.WriteOPCUAValue(opcuaClient, "ns=2;s=DB1.BtnCrossroadStart", CrossroadData.btnCrossroadStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, "ns=2;s=DB1.BtnCrossroadPause", CrossroadData.btnCrossroadPause == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, "ns=2;s=DB1.BtnCrossroadStop", CrossroadData.btnCrossroadStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, "ns=2;s=DB1.BtnCrosswalk1", CrossroadData.btnCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, "ns=2;s=DB1.BtnCrosswalk2", CrossroadData.btnCrosswalk2 == "true");

                                    // Read output values FROM PLC server
                                    CrossroadData.crossroadType = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.CrossroadType") ? "true" : "false";
                                    CrossroadData.trafficLight1_green = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight1_Green") ? "true" : "false";
                                    CrossroadData.trafficLight1_yellow = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight1_Yellow") ? "true" : "false";
                                    CrossroadData.trafficLight1_red = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight1_Red") ? "true" : "false";
                                    CrossroadData.trafficLight2_green = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight2_Green") ? "true" : "false";
                                    CrossroadData.trafficLight2_yellow = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight2_Yellow") ? "true" : "false";
                                    CrossroadData.trafficLight2_red = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.TrafficLight2_Red") ? "true" : "false";
                                    CrossroadData.pedestrian1_green = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.Pedestrian1_Green") ? "true" : "false";
                                    CrossroadData.pedestrian1_red = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.Pedestrian1_Red") ? "true" : "false";
                                    CrossroadData.pedestrian2_green = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.Pedestrian2_Green") ? "true" : "false";
                                    CrossroadData.pedestrian2_red = opcuaClient.ReadOPCUABool(opcuaClient, "ns=2;s=DB1.Pedestrian2_Red") ? "true" : "false";
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"OPC UA Client mode error: {ex.Message}");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                                */
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                var opcuaClient = _ucCommunicationControl._opcuaClient;

                                // Check if client needs reconnection
                                if (opcuaClient == null || !opcuaClient.connected)
                                {
                                    _ucCommunicationControl.SetStatus("OPC UA Client disconnected, waiting for reconnection...");
                                    Logger.LogWarning("OPC UA Client disconnected, waiting for reconnection...");
                                    await Task.Delay(1000, token);
                                    continue;
                                }

                                try
                                {
                                    // Write to PLC using numeric node IDs from OpcUaNodeIds configuration
                                    opcuaClient.WriteOPCUAValue(opcuaClient, OpcUaNodeIds.btnStart, CrossroadData.btnCrossroadStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, OpcUaNodeIds.btnPause, CrossroadData.btnCrossroadPause == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, OpcUaNodeIds.btnStop, CrossroadData.btnCrossroadStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, OpcUaNodeIds.btnCrosswalk1, CrossroadData.btnCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, OpcUaNodeIds.btnCrosswalk2, CrossroadData.btnCrosswalk2 == "true");

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values FROM PLC server using numeric node IDs
                                    CrossroadData.crossroadType = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.crossroadType) ? "true" : "false";
                                    CrossroadData.trafficLight1_green = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightGreen1) ? "true" : "false";
                                    CrossroadData.trafficLight1_yellow = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightsYellow1) ? "true" : "false";
                                    CrossroadData.trafficLight1_red = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightsRed1) ? "true" : "false";
                                    CrossroadData.trafficLight2_green = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightGreen2) ? "true" : "false";
                                    CrossroadData.trafficLight2_yellow = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightsYellow2) ? "true" : "false";
                                    CrossroadData.trafficLight2_red = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.trafficLightsRed2) ? "true" : "false";
                                    CrossroadData.pedestrian1_green = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.pedestrianLightGreen1) ? "true" : "false";
                                    CrossroadData.pedestrian1_red = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.pedestrianLightRed1) ? "true" : "false";
                                    CrossroadData.pedestrian2_green = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.pedestrianLightGreen2) ? "true" : "false";
                                    CrossroadData.pedestrian2_red = opcuaClient.ReadOPCUABool(opcuaClient, OpcUaNodeIds.pedestrianLightRed2) ? "true" : "false";


                                    _ucCommunicationControl.SetStatus("OPC UA Client: Data synchronized");
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"OPC UA Client mode error: {ex.Message}");
                                    Logger.LogException(ex, "OPC UA Client mode error:");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                            }
                            else
                            {
                                // no checkbox selected 
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "ModbusTCPIP":                 
                            if (internalVariables.checkBoxMaster == true)
                            {
                                var _modbusServer = _ucCommunicationControl._modbusServer;

                                // Master (Server) mode
                                if (_modbusServer == null || _modbusServer.slave == null)
                                {
                                    _ucCommunicationControl.SetStatus("Modbus Server: Not running, waiting for startup...");
                                    await Task.Delay(500, token);
                                    continue;
                                }

                                try
                                {
                                    // Write button values to our coils (coils 0-5) so slaves can read them
                                    bool[] masterButtons = new bool[5]
                                    {
                                        _modbusServer.StrToBool(CrossroadData.btnCrossroadStart),
                                        _modbusServer.StrToBool(CrossroadData.btnCrossroadPause),
                                        _modbusServer.StrToBool(CrossroadData.btnCrossroadStop),
                                        _modbusServer.StrToBool(CrossroadData.btnCrosswalk1),
                                        _modbusServer.StrToBool(CrossroadData.btnCrosswalk2)
                                    };
                                    _modbusServer.SetCoils(1, masterButtons);

                                    // Read light values that slaves wrote to our coils (coils 10-19)
                                    bool[] masterLights = _modbusServer.GetCoils(10, 10);
                                    if (masterLights != null && masterLights.Length >= 10)
                                    {
                                        CrossroadData.crossroadType = _modbusServer.BoolToStr(masterLights[0]);
                                        CrossroadData.trafficLight1_green = _modbusServer.BoolToStr(masterLights[1]);
                                        CrossroadData.trafficLight1_yellow = _modbusServer.BoolToStr(masterLights[2]);
                                        CrossroadData.trafficLight1_red = _modbusServer.BoolToStr(masterLights[3]);
                                        CrossroadData.trafficLight2_green = _modbusServer.BoolToStr(masterLights[4]);
                                        CrossroadData.trafficLight2_yellow = _modbusServer.BoolToStr(masterLights[5]);
                                        CrossroadData.trafficLight2_red = _modbusServer.BoolToStr(masterLights[6]);
                                        CrossroadData.pedestrian1_green = _modbusServer.BoolToStr(masterLights[7]);
                                        CrossroadData.pedestrian1_red = _modbusServer.BoolToStr(masterLights[8]);
                                        CrossroadData.pedestrian2_green = _modbusServer.BoolToStr(masterLights[9]);
                                        CrossroadData.pedestrian2_red = _modbusServer.BoolToStr(masterLights[10]);
                                    }

                                    _ucCommunicationControl.SetStatus("Modbus Server: Data ready for slaves");
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Modbus Server error: {ex.Message}");
                                    Logger.LogException(ex, "Modbus Server error:");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                var _modbusClient = _ucCommunicationControl._modbusClient;

                                // Slave mode: We are CLIENT connecting to Master server
                                if (_modbusClient == null || _modbusClient.master == null)
                                {
                                    _ucCommunicationControl.SetStatus("Modbus Client: Not connected, waiting for reconnection...");
                                    Logger.LogWarning("Modbus Client: Not connected to Master, waiting for reconnection...");
                                    await Task.Delay(1000, token);
                                    continue;
                                }

                                try
                                {
                                    // write button values to Master (coils 0-5) 
                                    bool[] buttonCoils = new bool[5]
                                    {
                                        _modbusClient.StrToBool(CrossroadData.btnCrossroadStart),
                                        _modbusClient.StrToBool(CrossroadData.btnCrossroadPause),
                                        _modbusClient.StrToBool(CrossroadData.btnCrossroadStop),
                                        _modbusClient.StrToBool(CrossroadData.btnCrosswalk1),
                                        _modbusClient.StrToBool(CrossroadData.btnCrosswalk2)
                                    };

                                    byte slaveId = 1;
                                    _modbusClient.WriteMultipleCoils(slaveId, 1, buttonCoils);

                                    // read light values from Master (coils 10-19)
                                    bool[] lights = _modbusClient.ReadCoils(slaveId, 10, 10);
                                    if (lights != null && lights.Length >= 10)
                                    {
                                        CrossroadData.crossroadType = _modbusClient.BoolToStr(lights[0]);
                                        CrossroadData.trafficLight1_green = _modbusClient.BoolToStr(lights[1]);
                                        CrossroadData.trafficLight1_yellow = _modbusClient.BoolToStr(lights[2]);
                                        CrossroadData.trafficLight1_red = _modbusClient.BoolToStr(lights[3]);
                                        CrossroadData.trafficLight2_green = _modbusClient.BoolToStr(lights[4]);
                                        CrossroadData.trafficLight2_yellow = _modbusClient.BoolToStr(lights[5]);
                                        CrossroadData.trafficLight2_red = _modbusClient.BoolToStr(lights[6]);
                                        CrossroadData.pedestrian1_green = _modbusClient.BoolToStr(lights[7]);
                                        CrossroadData.pedestrian1_red = _modbusClient.BoolToStr(lights[8]);
                                        CrossroadData.pedestrian2_green = _modbusClient.BoolToStr(lights[9]);
                                        CrossroadData.pedestrian2_red = _modbusClient.BoolToStr(lights[10]);
                                        _ucCommunicationControl.SetStatus("Modbus Client: Data synchronized");
                                    }
                                    else
                                    {
                                        _ucCommunicationControl.SetStatus("Modbus Client: Failed to read lights from Master");
                                        Logger.LogError("Modbus Client: Failed to read lights from Master");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Modbus Client exception error: {ex.Message}");
                                    Logger.LogException(ex, "Mobus Client exception error:");
                                    await Task.Delay(500, token);
                                    continue;
                                }
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus("Please select Master or Slave mode for Modbus TCP/IP.");
                            }

                            break;
                        case "TCPIP":
                            
                            byte buttons = 0;

                            if (CrossroadData.btnCrossroadStart == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrossroadStart;
                            }
                                
                            if (CrossroadData.btnCrossroadPause == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrossroadPause;
                            }
                                
                            if (CrossroadData.btnCrossroadStop == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrossroadStop;
                            }
                                
                            if (CrossroadData.btnCrosswalk1 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrosswalk1;
                            }
                                
                            if (CrossroadData.btnCrosswalk2 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrosswalk2;
                            }

                            byte[] outTelegram = new byte[] { buttons };

                            if (internalVariables.checkBoxMaster == true)
                            {
                                // THIS DEVICE IS MASTER: Use server
                                var tcpipServer = _ucCommunicationControl._tcpipServer;
                                
                                if (tcpipServer == null || tcpipServer.clientSocket == null || !tcpipServer.clientSocket.Connected)
                                {
                                    await Task.Delay(200, token);
                                    continue;
                                }

                                // write data
                                bool send = tcpipServer.SendBytes(outTelegram);

                                if (send == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data sent successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data sending.");
                                    Logger.LogError("Error in TCP/IP data sending.");
                                    break;
                                }

                                // read data
                                byte[] inTelegram = new byte[2];
                                bool read = tcpipServer.ReceiveExact(inTelegram);

                                if (read == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data read successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data reading.");
                                    Logger.LogError("Error in TCP/IP data reading.");
                                    break;
                                }

                                CrossroadData.trafficLight1_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Pedestrian1_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Pedestrian1_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_green = ((inTelegram[1] & (byte)comTCPIPClient.LightFlagsByte1.Pedestrian2_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_red = ((inTelegram[1] & (byte)comTCPIPClient.LightFlagsByte1.Pedestrian2_Red) != 0) ? "true" : "false";
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                // THIS DEVICE IS SLAVE: Use client
                                var tcpipClient = _ucCommunicationControl._tcpipClient;
                                
                                if (tcpipClient == null || tcpipClient.socket == null || !tcpipClient.socket.Connected)
                                {
                                    await Task.Delay(200, token);
                                    continue;
                                }

                                // write data
                                bool send = tcpipClient.SendBytes(outTelegram);

                                if (send == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data sent successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data sending.");
                                    Logger.LogError("Error in TCP/IP data sending.");
                                    break;
                                }

                                // read data
                                byte[] inTelegram = new byte[2];
                                bool read = tcpipClient.ReceiveExact(inTelegram);

                                if (read == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data read successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data reading.");
                                    Logger.LogError($"TCP/IP Client error: Failed to read data from server.");
                                    break;
                                }

                                CrossroadData.trafficLight1_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light1_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Light2_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Pedestrian1_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.Pedestrian1_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_green = ((inTelegram[1] & (byte)comTCPIPClient.LightFlagsByte1.Pedestrian2_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_red = ((inTelegram[1] & (byte)comTCPIPClient.LightFlagsByte1.Pedestrian2_Red) != 0) ? "true" : "false";
                            }
                            else
                            {
                                // choose what is this device
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "RESTAPI":
                            _ucCommunicationControl.SetStatus($"This communication is already running in the project on http://{internalVariables.LocalIP}:{internalVariables.apiPort}/api/");
                            break;
                        case "Sharp7":
                            // choose between these two methods -> please test me
                            // reading from PLC 
                            int activeDBnumber = CrossroadData.CrossroadDBnumber;

                            int read1 = _sharp7.readDB(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);
                            //bool read2 = _sharp7.readS7MultiVar(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);

                            if (read1 == 0)
                            {
                                switch (activeDBnumber)
                                {
                                    case CrossroadData.CrossroadDBnumber:

                                        CrossroadData.crossroadType = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 0));
                                        /*
                                        CrossroadData.btnCrossroadStart = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 2));
                                        CrossroadData.btnCrossroadPause = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 3));
                                        CrossroadData.btnCrossroadStop = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 4));
                                        CrossroadData.btnCrosswalk1 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 5));
                                        CrossroadData.btnCrosswalk2 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 6));
                                        */
                                        CrossroadData.trafficLight1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 0));
                                        CrossroadData.trafficLight1_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 1));
                                        CrossroadData.trafficLight1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 2));
                                        CrossroadData.trafficLight2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 3));
                                        CrossroadData.trafficLight2_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 4));
                                        CrossroadData.trafficLight2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 5));
                                        CrossroadData.pedestrian1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 6));
                                        CrossroadData.pedestrian1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 7));
                                        CrossroadData.pedestrian2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 3, 0));
                                        CrossroadData.pedestrian2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 3, 1));

                                        _ucCommunicationControl.SetStatus($"ReadDB OK.");

                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                // read failed -> Exception?
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. ReadDB returns {read1}.");
                                Logger.LogError($"Sharp7 ReadDB error: Return code {read1} while reading DB{CrossroadData.CrossroadDBnumber}.");
                            }

                            /*
                            if (read2 == 0)
                            {
                                switch (activeDBnumber)
                                {
                                    case CrossroadData.CrossroadDBnumber:

                                        CrossroadData.crossroadType = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 1));
                                        //CrossroadData.btnCrossroadStart = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 0));
                                        //CrossroadData.btnCrossroadPause = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 1));
                                        //CrossroadData.btnCrossroadStop = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 2));
                                        //CrossroadData.btnCrosswalk1 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 3));
                                        //CrossroadData.btnCrosswalk2 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 4));

                                        CrossroadData.trafficLight1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 0));
                                        CrossroadData.trafficLight1_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 1));
                                        CrossroadData.trafficLight1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 2));
                                        CrossroadData.trafficLight2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 3));
                                        CrossroadData.trafficLight2_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 4));
                                        CrossroadData.trafficLight2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 5));
                                        CrossroadData.pedestrian1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 6));
                                        CrossroadData.pedestrian1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 2, 7));
                                        CrossroadData.pedestrian2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 3, 0));
                                        CrossroadData.pedestrian2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 3, 1));

                                        _ucCommunicationControl.SetStatus($"ReadS7MultiVar OK.");

                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                // read failed -> Exception?
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. ReadDB returns {read2}.");
                            }
                            */

                            // writting to PLC 

                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 0, Convert.ToBoolean(CrossroadData.crossroadType));
                            
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 1, Convert.ToBoolean(CrossroadData.btnCrossroadStart));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 2, Convert.ToBoolean(CrossroadData.btnCrossroadPause));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 3, Convert.ToBoolean(CrossroadData.btnCrossroadStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 4, Convert.ToBoolean(CrossroadData.btnCrosswalk1));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 5, Convert.ToBoolean(CrossroadData.btnCrosswalk2));

                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 0, Convert.ToBoolean(CrossroadData.trafficLight1_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 1, Convert.ToBoolean(CrossroadData.trafficLight1_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 2, Convert.ToBoolean(CrossroadData.trafficLight1_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 3, Convert.ToBoolean(CrossroadData.trafficLight2_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 4, Convert.ToBoolean(CrossroadData.trafficLight2_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 5, Convert.ToBoolean(CrossroadData.trafficLight2_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 6, Convert.ToBoolean(CrossroadData.pedestrian1_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 2, 7, Convert.ToBoolean(CrossroadData.pedestrian1_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 3, 0, Convert.ToBoolean(CrossroadData.pedestrian2_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 3, 1, Convert.ToBoolean(CrossroadData.pedestrian2_red));

                            int write1 = _sharp7.writeDB(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadWriteBuffer, 0);
                            //bool write2 = _sharp7.writeS7MultiVar(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadWriteBuffer, 0);

                            if (write1 == 0)
                            {
                                // write was successful
                                _ucCommunicationControl.SetStatus($"WriteDB OK.");
                            }
                            else
                            {
                                // write failed -> Exception?
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. WriteDB returns {write1}.");
                                Logger.LogError($"Sharp7 writeDB error: Return code {write1} for DB {CrossroadData.CrossroadDBnumber}");
                            }

                            /*
                            if (write2 == 0)
                            {
                                // write was successful
                                _ucCommunicationControl.SetStatus($"WriteS7MultiVar OK.");
                            }
                            else
                            {
                                // write failed -> Exception?
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. WriteDB returns {write2}.");
                            }
                            */

                            break;

                        default:
                            Logger.LogError($"Unknown communication type: {internalVariables.communicationFlag}");
                            break;
                    }

                    await Task.Delay(50, token);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "CommunicationManager");
                _ucCommunicationControl.SetStatus($"Exception: Error in communication: {ex.Message}");
            }
        }
    }
}
