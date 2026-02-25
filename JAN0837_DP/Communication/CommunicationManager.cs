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
using S7.Net.Types;
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
        public comTCPIPClient _tcpipClient;  
        public comTCPIPServer _tcpipServer;   
        public comModbusTCPIP.ModbusTCPIPimClient _modbusClient;  
        public comModbusTCPIP.ModbusTCPIPimServer _modbusServer;  
        public MQTTBroker _mqttBroker;
        public MQTTClient _mqttClient;
        public comOPCUA.opcuaKlient _opcuaClient;      
        public comOPCUA.opcuaServer _opcuaServer;   

        public ucCommunicationControl _ucCommunicationControl;

        public CommunicationManager(ucCommunicationControl ucCommunicationControl)
        {
            _ucCommunicationControl = ucCommunicationControl;
        }

        public async Task Communication(CancellationToken token) 
        {
            try
            {
                while (internalVariables.communicationThreadRunningFlag && !token.IsCancellationRequested)
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

                            if (internalVariables.checkBoxMaster == true)
                            {
                                // publish
                                if (client == null || client.mqttClient == null || !client.mqttClient.IsConnected)
                                {
                                    await Task.Delay(200, token);
                                    continue; // break x return
                                }

                                try
                                {
                                    // CrossroadData - Input
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    var msgCrossroadInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Crossroad/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCrossroadInput, token);

                                    // CrossroadData - Output
                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    var msgCrossroadOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Crossroad/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCrossroadOutput, token);

                                    // CrosswalkData - Input
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    var msgCrosswalkInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Crosswalk/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCrosswalkInput, token);

                                    // CrosswalkData - Output
                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    var msgCrosswalkOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Crosswalk/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCrosswalkOutput, token);

                                    // RegulatorData - Input
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    var msgRegulatorInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgRegulatorInput, token);

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Regulator/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        timeDoorMovement = CarWashData.TimeDoorMovement,
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // add carwash mems 

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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

                                // Slave publishes same structure to separate topics
                                try
                                {
                                    // Crossroad Input/Output
                                    var crossroadInput = new
                                    {
                                        start = CrossroadData.btnStart == "true",
                                        pause = CrossroadData.btnPause == "true",
                                        stop = CrossroadData.btnStop == "true",
                                        cw1 = CrossroadData.btnCrosswalk1 == "true",
                                        cw2 = CrossroadData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tl1_green = CrossroadData.trafficLight1_green == "true",
                                        tl1_yellow = CrossroadData.trafficLight1_yellow == "true",
                                        tl1_red = CrossroadData.trafficLight1_red == "true",
                                        tl2_green = CrossroadData.trafficLight2_green == "true",
                                        tl2_yellow = CrossroadData.trafficLight2_yellow == "true",
                                        tl2_red = CrossroadData.trafficLight2_red == "true",
                                        ped1_green = CrossroadData.pedestrian1_green == "true",
                                        ped1_red = CrossroadData.pedestrian1_red == "true",
                                        ped2_green = CrossroadData.pedestrian2_green == "true",
                                        ped2_red = CrossroadData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnCrosswalkStart == "true",
                                        pause = CrosswalkData.btnCrosswalkPause == "true",
                                        stop = CrosswalkData.btnCrosswalkStop == "true",
                                        cw1 = CrosswalkData.btnCrosswalk1 == "true",
                                        cw2 = CrosswalkData.btnCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crosswalkOutput = new
                                    {
                                        type = CrosswalkData.crosswalkType == "true",
                                        tl1_green = CrosswalkData.trafficLight1_green == "true",
                                        tl1_yellow = CrosswalkData.trafficLight1_yellow == "true",
                                        tl1_red = CrosswalkData.trafficLight1_red == "true",
                                        tl2_green = CrosswalkData.trafficLight2_green == "true",
                                        tl2_yellow = CrosswalkData.trafficLight2_yellow == "true",
                                        tl2_red = CrosswalkData.trafficLight2_red == "true",
                                        ped1_green = CrosswalkData.pedestrian1_green == "true",
                                        ped1_red = CrosswalkData.pedestrian1_red == "true",
                                        ped2_green = CrosswalkData.pedestrian2_green == "true",
                                        ped2_red = CrosswalkData.pedestrian2_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crosswalk/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crosswalkOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Regulator Input/Output
                                    var regulatorInput = new
                                    {
                                        switchstate = RegulatorData.switchstate == "true",
                                        R = RegulatorData.R,
                                        C = RegulatorData.C,
                                        U = RegulatorData.U,
                                        Td = RegulatorData.Td
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    // CarWash - Input
                                    var carwashInput = new
                                    {
                                        emergencyStop = CarWashData.btnEmergencyStop == "true",
                                        start = CarWashData.btnStart == "true",
                                        stop = CarWashData.btnStop == "true",
                                        errorSystem = CarWashData.ErrorSystem == "true",
                                        carPosition = CarWashData.CarPosition == "true",
                                        showerPosition = CarWashData.ShowerPosition == "true",
                                        mode = CarWashData.Mode
                                    };
                                    var msgCarwashInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashInput, token);

                                    // CarWash - Output
                                    var carwashOutput = new
                                    {
                                        light_green = CarWashData.Light_green == "true",
                                        light_yellow = CarWashData.Light_yellow == "true",
                                        light_red = CarWashData.Light_red == "true",
                                        door1_up = CarWashData.Door1_Up == "true",
                                        door1_down = CarWashData.Door1_Down == "true",
                                        door2_up = CarWashData.Door2_Up == "true",
                                        door2_down = CarWashData.Door2_Down == "true",
                                        chemicals_front = CarWashData.ChemicalsFront == "true",
                                        chemicals_sides = CarWashData.ChemicalsSides == "true",
                                        chemicals_back = CarWashData.ChemicalsBack == "true",
                                        prewash = CarWashData.Prewash == "true",
                                        water = CarWashData.Water == "true",
                                        wax = CarWashData.Wax == "true",
                                        dry = CarWashData.Dry == "true",
                                        brushes = CarWashData.Brushes == "true",
                                        soap = CarWashData.Soap == "true",
                                        activeFoam = CarWashData.ActiveFoam == "true",
                                        timeDoorMovement = CarWashData.TimeDoorMovement,
                                        memDoor = CarWashData.MEMDoor == "true",
                                        memDoorTrig = CarWashData.MEMDoorTrig == "true",
                                        memDoorClosingtrig = CarWashData.MEMDoorClosingtrig == "true"
                                    };
                                    var msgCarwashOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/CarWash/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(carwashOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgCarwashOutput, token);

                                    // WashingMachine - Input
                                    var washingmachineInput = new
                                    {
                                        emergencyStop = WashingMachineData.btnWashingMachineEmergencyStop == "true",
                                        start = WashingMachineData.btnStartWashingMachine == "true",
                                        stop = WashingMachineData.btnStopWashingMachine == "true",
                                        errorSystem = WashingMachineData.WashingMachineErrorSystem == "true",
                                        mode = WashingMachineData.WashingMachineMode
                                    };
                                    var msgWashingmachineInput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineInput, token);

                                    // WashingMachine - Output
                                    var washingmachineOutput = new
                                    {
                                        light_green = WashingMachineData.WashingMachineLight_green == "true",
                                        light_yellow = WashingMachineData.WashingMachineLight_yellow == "true",
                                        light_red = WashingMachineData.WashingMachineLight_red == "true",
                                        doorClosed = WashingMachineData.WashingMachineDoorClosed == "true",
                                        chemicals = WashingMachineData.WashingMachineChemicals == "true",
                                        prewash = WashingMachineData.WashingMachinePrewash == "true",
                                        water = WashingMachineData.WashingMachineWater == "true",
                                        wax = WashingMachineData.WashingMachineWax == "true",
                                        dry = WashingMachineData.WashingMachineDry == "true",
                                        brushes = WashingMachineData.WashingMachineBrushes == "true",
                                        soap = WashingMachineData.WashingMachineSoap == "true",
                                        activeFoam = WashingMachineData.WashingMachineActiveFoam == "true"
                                    };
                                    var msgWashingmachineOutput = new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/WashingMachine/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(washingmachineOutput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build();
                                    await client.mqttClient.PublishAsync(msgWashingmachineOutput, token);

                                    _ucCommunicationControl.SetStatus("MQTT: All data published successfully to separate topics");
                                }
                                catch (OperationCanceledException) 
                                { 
                                    Logger.LogException(new OperationCanceledException("MQTT publish operation was canceled."), "MQTT publish error: ");
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"Exception error MQTT: {ex}");
                                    Logger.LogException(ex, "Exception error MQTT: ");
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
                                    // CrossroadData
                                    CrossroadData.btnStart = opcuaServer.ReadVariable("BtnCrossroadStart") ? "true" : "false";
                                    CrossroadData.btnPause = opcuaServer.ReadVariable("BtnCrossroadPause") ? "true" : "false";
                                    CrossroadData.btnStop = opcuaServer.ReadVariable("BtnCrossroadStop") ? "true" : "false";
                                    CrossroadData.btnCrosswalk1 = opcuaServer.ReadVariable("BtnCrosswalk1") ? "true" : "false";
                                    CrossroadData.btnCrosswalk2 = opcuaServer.ReadVariable("BtnCrosswalk2") ? "true" : "false";

                                    opcuaServer.UpdateBoolVariable("CrossroadType", CrossroadData.crossroadType == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Green", CrossroadData.trafficLight1_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Yellow", CrossroadData.trafficLight1_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Red", CrossroadData.trafficLight1_red == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Green", CrossroadData.trafficLight2_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Yellow", CrossroadData.trafficLight2_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Red", CrossroadData.trafficLight2_red == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian1_Green", CrossroadData.pedestrian1_green == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian1_Red", CrossroadData.pedestrian1_red == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian2_Green", CrossroadData.pedestrian2_green == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian2_Red", CrossroadData.pedestrian2_red == "true");

                                    // CrosswalkData
                                    CrosswalkData.btnCrosswalkStart = opcuaServer.ReadVariable("BtnCrosswalkStart_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnCrosswalkPause = opcuaServer.ReadVariable("BtnCrosswalkPause_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnCrosswalkStop = opcuaServer.ReadVariable("BtnCrosswalkStop_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnCrosswalk1 = opcuaServer.ReadVariable("BtnCrosswalk1_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnCrosswalk2 = opcuaServer.ReadVariable("BtnCrosswalk2_Crosswalk") ? "true" : "false";
                                    
                                    opcuaServer.UpdateBoolVariable("CrosswalkType", CrosswalkData.crosswalkType == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Green_Crosswalk", CrosswalkData.trafficLight1_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Yellow_Crosswalk", CrosswalkData.trafficLight1_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight1_Red_Crosswalk", CrosswalkData.trafficLight1_red == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Green_Crosswalk", CrosswalkData.trafficLight2_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Yellow_Crosswalk", CrosswalkData.trafficLight2_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLight2_Red_Crosswalk", CrosswalkData.trafficLight2_red == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian1_Green_Crosswalk", CrosswalkData.pedestrian1_green == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian1_Red_Crosswalk", CrosswalkData.pedestrian1_red == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian2_Green_Crosswalk", CrosswalkData.pedestrian2_green == "true");
                                    opcuaServer.UpdateBoolVariable("Pedestrian2_Red_Crosswalk", CrosswalkData.pedestrian2_red == "true");

                                    // RegulatorData (read inputs FROM OPC UA server)
                                    RegulatorData.switchstate = opcuaServer.ReadVariable("Switchstate") ? "true" : "false";
                                    RegulatorData.R = opcuaServer.ReadVariable("R").ToString();
                                    RegulatorData.C = opcuaServer.ReadVariable("C").ToString();
                                    RegulatorData.U = opcuaServer.ReadVariable("U").ToString();
                                    RegulatorData.Td = opcuaServer.ReadVariable("Td").ToString();

                                    // CarWash
                                    CarWashData.btnEmergencyStop = opcuaServer.ReadVariable("BtnCarWashEmergencyStop") ? "true" : "false";
                                    CarWashData.btnStart = opcuaServer.ReadVariable("BtnStartCarWash") ? "true" : "false";
                                    CarWashData.btnStop = opcuaServer.ReadVariable("BtnStopCarWash") ? "true" : "false";
                                    CarWashData.ErrorSystem = opcuaServer.ReadVariable("CarWashErrorSystem") ? "true" : "false";
                                    CarWashData.CarPosition = opcuaServer.ReadVariable("CarWashCarPosition") ? "true" : "false";
                                    CarWashData.ShowerPosition = opcuaServer.ReadVariable("CarWashShowerPosition") ? "true" : "false";
                                    CarWashData.Mode = opcuaServer.ReadVariable("CarWashMode").ToString();

                                    opcuaServer.UpdateBoolVariable("Light_Green", CarWashData.Light_green == "true");
                                    opcuaServer.UpdateBoolVariable("Light_Yellow", CarWashData.Light_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("Light_Red", CarWashData.Light_red == "true");
                                    opcuaServer.UpdateBoolVariable("Door1_Up", CarWashData.Door1_Up == "true");
                                    opcuaServer.UpdateBoolVariable("Door1_Down", CarWashData.Door1_Down == "true");
                                    opcuaServer.UpdateBoolVariable("Door2_Up", CarWashData.Door2_Up == "true");
                                    opcuaServer.UpdateBoolVariable("Door2_Down", CarWashData.Door2_Down == "true");
                                    opcuaServer.UpdateBoolVariable("ChemicalsFront", CarWashData.ChemicalsFront == "true");
                                    opcuaServer.UpdateBoolVariable("ChemicalsSides", CarWashData.ChemicalsSides == "true");
                                    opcuaServer.UpdateBoolVariable("ChemicalsBack", CarWashData.ChemicalsBack == "true");
                                    opcuaServer.UpdateBoolVariable("Prewash", CarWashData.Prewash == "true");
                                    opcuaServer.UpdateBoolVariable("Water", CarWashData.Water == "true");
                                    opcuaServer.UpdateBoolVariable("Wax", CarWashData.Wax == "true");
                                    opcuaServer.UpdateBoolVariable("Dry", CarWashData.Dry == "true");
                                    opcuaServer.UpdateBoolVariable("Brushes", CarWashData.Brushes == "true");
                                    opcuaServer.UpdateBoolVariable("Soap", CarWashData.Soap == "true");
                                    opcuaServer.UpdateBoolVariable("ActiveFoam", CarWashData.ActiveFoam == "true");
                                    opcuaServer.UpdateSringVariable("TimeDoorMovement", CarWashData.TimeDoorMovement);
                                    opcuaServer.UpdateBoolVariable("MEMDoor", CarWashData.MEMDoor == "true");
                                    opcuaServer.UpdateBoolVariable("MEMDoorTrig", CarWashData.MEMDoorTrig == "true");
                                    opcuaServer.UpdateBoolVariable("MEMDoorClosingtrig", CarWashData.MEMDoorClosingtrig == "true");

                                    // WashingMachine
                                    WashingMachineData.btnWashingMachineEmergencyStop = opcuaServer.ReadVariable("BtnWashingMachineEmergencyStop") ? "true" : "false";
                                    WashingMachineData.btnStartWashingMachine = opcuaServer.ReadVariable("BtnStartWashingMachine") ? "true" : "false";
                                    WashingMachineData.btnStopWashingMachine = opcuaServer.ReadVariable("BtnStopWashingMachine") ? "true" : "false";
                                    WashingMachineData.WashingMachineErrorSystem = opcuaServer.ReadVariable("WashingMachineErrorSystem") ? "true" : "false";
                                    WashingMachineData.WashingMachineMode = opcuaServer.ReadVariable("WashingMachineMode").ToString();

                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Green", WashingMachineData.WashingMachineLight_green == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Yellow", WashingMachineData.WashingMachineLight_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Red", WashingMachineData.WashingMachineLight_red == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineDoorClosed", WashingMachineData.WashingMachineDoorClosed == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineChemicals", WashingMachineData.WashingMachineChemicals == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachinePrewash", WashingMachineData.WashingMachinePrewash == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineWater", WashingMachineData.WashingMachineWater == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineWax", WashingMachineData.WashingMachineWax == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineDry", WashingMachineData.WashingMachineDry == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineBrushes", WashingMachineData.WashingMachineBrushes == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineSoap", WashingMachineData.WashingMachineSoap == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineActiveFoam", WashingMachineData.WashingMachineActiveFoam == "true");

                                    _ucCommunicationControl.SetStatus("OPC UA Server: Hosting all data for external clients");
                                }
                                catch (Exception ex)
                                {
                                    _ucCommunicationControl.SetStatus($"OPC UA Server error: {ex.Message}");
                                    Logger.LogException(ex, "OPC UA Server error:");
                                    await Task.Delay(500, token);
                                    continue;
                                }                                
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
                                    // CrossroadData 
                                    // Write input values to PLC
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnStart, CrossroadData.btnStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnPause, CrossroadData.btnPause == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnStop, CrossroadData.btnStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnCrosswalk1, CrossroadData.btnCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnCrosswalk2, CrossroadData.btnCrosswalk2 == "true");

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values from PLC 
                                    CrossroadData.crossroadType = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.crossroadType) ? "true" : "false";
                                    CrossroadData.trafficLight1_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightGreen1) ? "true" : "false";
                                    CrossroadData.trafficLight1_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightsYellow1) ? "true" : "false";
                                    CrossroadData.trafficLight1_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightsRed1) ? "true" : "false";
                                    CrossroadData.trafficLight2_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightGreen2) ? "true" : "false";
                                    CrossroadData.trafficLight2_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightsYellow2) ? "true" : "false";
                                    CrossroadData.trafficLight2_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightsRed2) ? "true" : "false";
                                    CrossroadData.pedestrian1_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianLightGreen1) ? "true" : "false";
                                    CrossroadData.pedestrian1_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianLightRed1) ? "true" : "false";
                                    CrossroadData.pedestrian2_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianLightGreen2) ? "true" : "false";
                                    CrossroadData.pedestrian2_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianLightRed2) ? "true" : "false";

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // CrosswalkData
                                    // Write input values to PLC
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnCrosswalkStart, CrosswalkData.btnCrosswalkStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnCrosswalkPause, CrosswalkData.btnCrosswalkPause == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnCrosswalkStop, CrosswalkData.btnCrosswalkStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnCrosswalk1, CrosswalkData.btnCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnCrosswalk2, CrosswalkData.btnCrosswalk2 == "true");

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values from PLC 
                                    CrosswalkData.crosswalkType = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.crosswalkType) ? "true" : "false";
                                    CrosswalkData.trafficLight1_green = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight1_green) ? "true" : "false";
                                    CrosswalkData.trafficLight1_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight1_yellow) ? "true" : "false";
                                    CrosswalkData.trafficLight1_red = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight1_red) ? "true" : "false";
                                    CrosswalkData.trafficLight2_green = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight2_green) ? "true" : "false";
                                    CrosswalkData.trafficLight2_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight2_yellow) ? "true" : "false";
                                    CrosswalkData.trafficLight2_red = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.trafficLight2_red) ? "true" : "false";
                                    CrosswalkData.pedestrian1_green = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.pedestrian1_green) ? "true" : "false";
                                    CrosswalkData.pedestrian1_red = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.pedestrian1_red) ? "true" : "false";
                                    CrosswalkData.pedestrian2_green = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.pedestrian2_green) ? "true" : "false";
                                    CrosswalkData.pedestrian2_red = opcuaClient.ReadOPCUABool(opcuaClient, CrosswalkData.OpcUaNodeIds.pedestrian2_red) ? "true" : "false";

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // RegulatorData
                                    // Write input values to PLC
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.switchstate, RegulatorData.switchstate == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.R, RegulatorData.R);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.C, RegulatorData.C);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.U, RegulatorData.U);

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // read output values from PLC 

                                    // If session became invalid during writes, skip reads this cycle
                                    /*
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during read, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during read, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }
                                    */

                                    // CarWashData
                                    // Write input values to PLC
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.btnEmergencyStop, CarWashData.btnEmergencyStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.btnStart, CarWashData.btnStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.btnStop, CarWashData.btnStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.ErrorSystem, CarWashData.ErrorSystem == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.CarPosition, CarWashData.CarPosition == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.ShowerPosition, CarWashData.ShowerPosition == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CarWashData.OpcUaNodeIds.Mode, CarWashData.Mode);

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values from PLC 
                                    CarWashData.Light_green = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Light_green) ? "true" : "false";
                                    CarWashData.Light_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Light_yellow) ? "true" : "false";
                                    CarWashData.Light_red = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Light_red) ? "true" : "false";
                                    CarWashData.Door1_Up = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Door1_Up) ? "true" : "false";
                                    CarWashData.Door1_Down = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Door1_Down) ? "true" : "false";
                                    CarWashData.Door2_Up = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Door2_Up) ? "true" : "false";
                                    CarWashData.Door2_Down = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Door2_Down) ? "true" : "false";
                                    CarWashData.ChemicalsFront = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.ChemicalsFront) ? "true" : "false";
                                    CarWashData.ChemicalsSides = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.ChemicalsSides) ? "true" : "false";
                                    CarWashData.ChemicalsBack = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.ChemicalsBack) ? "true" : "false";
                                    CarWashData.Prewash = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Prewash) ? "true" : "false";
                                    CarWashData.Water = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Water) ? "true" : "false";
                                    CarWashData.Wax = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Wax) ? "true" : "false";
                                    CarWashData.Dry = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Dry) ? "true" : "false";
                                    CarWashData.Brushes = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Brushes) ? "true" : "false";
                                    CarWashData.Soap = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.Soap) ? "true" : "false";
                                    CarWashData.ActiveFoam = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.ActiveFoam) ? "true" : "false";
                                    CarWashData.TimeDoorMovement = opcuaClient.ReadOPCUAInt(opcuaClient, CarWashData.OpcUaNodeIds.TimeDoorMovement);
                                    CarWashData.MEMDoor = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.MEMDoor) ? "true" : "false";
                                    CarWashData.MEMDoorTrig = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.MEMDoorTrig) ? "true" : "false";
                                    CarWashData.MEMDoorClosingtrig = opcuaClient.ReadOPCUABool(opcuaClient, CarWashData.OpcUaNodeIds.MEMDoorClosingtrig) ? "true" : "false";

                                    // WashingMachineData
                                    // Write input values to PLC
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnWashingMachineEmergencyStop, WashingMachineData.btnWashingMachineEmergencyStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnStartWashingMachine, WashingMachineData.btnStartWashingMachine == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnStopWashingMachine, WashingMachineData.btnStopWashingMachine == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineErrorSystem, WashingMachineData.WashingMachineErrorSystem == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineMode, WashingMachineData.WashingMachineMode == "true");

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values from PLC 
                                    WashingMachineData.WashingMachineLight_green = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineLight_green) ? "true" : "false";
                                    WashingMachineData.WashingMachineLight_yellow = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineLight_yellow) ? "true" : "false";
                                    WashingMachineData.WashingMachineLight_red = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineLight_red) ? "true" : "false";
                                    WashingMachineData.WashingMachineDoorClosed = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineDoorClosed) ? "true" : "false";
                                    WashingMachineData.WashingMachineChemicals = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineChemicals) ? "true" : "false";
                                    WashingMachineData.WashingMachinePrewash = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachinePrewash) ? "true" : "false";
                                    WashingMachineData.WashingMachineWater = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineWater) ? "true" : "false";
                                    WashingMachineData.WashingMachineWax = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineWax) ? "true" : "false";
                                    WashingMachineData.WashingMachineDry = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineDry) ? "true" : "false";
                                    WashingMachineData.WashingMachineBrushes = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineBrushes) ? "true" : "false";
                                    WashingMachineData.WashingMachineSoap = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineSoap) ? "true" : "false";
                                    WashingMachineData.WashingMachineActiveFoam = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.WashingMachineActiveFoam) ? "true" : "false";

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during read, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during read, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }
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
                                    // ═══════════════════════════════════════════════════════════
                                    // WRITE input values to holding registers 
                                    // ═══════════════════════════════════════════════════════════
                                    
                                    // CrossroadData buttons: registers 0-4
                                    bool[] crossroadButtons = new bool[5]
                                    {
                                        _modbusServer.StrToBool(CrossroadData.btnStart),
                                        _modbusServer.StrToBool(CrossroadData.btnPause),
                                        _modbusServer.StrToBool(CrossroadData.btnStop),
                                        _modbusServer.StrToBool(CrossroadData.btnCrosswalk1),
                                        _modbusServer.StrToBool(CrossroadData.btnCrosswalk2)
                                    };
                                    _modbusServer.SetRegisters(0, crossroadButtons);

                                    // CrosswalkData buttons: registers 5-9
                                    bool[] crosswalkButtons = new bool[5]
                                    {
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalkStart),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalkPause),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalkStop),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalk1),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalk2)
                                    };
                                    _modbusServer.SetRegisters(5, crosswalkButtons);

                                    // RegulatorData inputs: registers 10-14 (switchstate + R,C,U,Td as bool for simplicity)
                                    bool[] regulatorInputs = new bool[5]
                                    {
                                        _modbusServer.StrToBool(RegulatorData.switchstate),
                                        !string.IsNullOrEmpty(RegulatorData.R),
                                        !string.IsNullOrEmpty(RegulatorData.C),
                                        !string.IsNullOrEmpty(RegulatorData.U),
                                        !string.IsNullOrEmpty(RegulatorData.Td)
                                    };
                                    _modbusServer.SetRegisters(10, regulatorInputs);

                                    // CarWash inputs: registers 15-21
                                    bool[] carwashInputs = new bool[7]
                                    {
                                        _modbusServer.StrToBool(CarWashData.btnEmergencyStop),
                                        _modbusServer.StrToBool(CarWashData.btnStart),
                                        _modbusServer.StrToBool(CarWashData.btnStop),
                                        _modbusServer.StrToBool(CarWashData.ErrorSystem),
                                        _modbusServer.StrToBool(CarWashData.CarPosition),
                                        _modbusServer.StrToBool(CarWashData.ShowerPosition),
                                        !string.IsNullOrEmpty(CarWashData.Mode)
                                    };
                                    _modbusServer.SetRegisters(15, carwashInputs);

                                    // WashingMachine inputs: registers 22-26
                                    bool[] washingmachineInputs = new bool[5]
                                    {
                                        _modbusServer.StrToBool(WashingMachineData.btnWashingMachineEmergencyStop),
                                        _modbusServer.StrToBool(WashingMachineData.btnStartWashingMachine),
                                        _modbusServer.StrToBool(WashingMachineData.btnStopWashingMachine),
                                        _modbusServer.StrToBool(WashingMachineData.WashingMachineErrorSystem),
                                        !string.IsNullOrEmpty(WashingMachineData.WashingMachineMode)
                                    };
                                    _modbusServer.SetRegisters(22, washingmachineInputs);

                                    // ═══════════════════════════════════════════════════════════
                                    // READ output values that slaves wrote to our holding registers
                                    // ═══════════════════════════════════════════════════════════

                                    // CrossroadData lights: registers 30-40 (11 values)
                                    bool[] crossroadLights = _modbusServer.GetRegisters(30, 11);
                                    if (crossroadLights != null && crossroadLights.Length >= 11)
                                    {
                                        CrossroadData.crossroadType = _modbusServer.BoolToStr(crossroadLights[0]);
                                        CrossroadData.trafficLight1_green = _modbusServer.BoolToStr(crossroadLights[1]);
                                        CrossroadData.trafficLight1_yellow = _modbusServer.BoolToStr(crossroadLights[2]);
                                        CrossroadData.trafficLight1_red = _modbusServer.BoolToStr(crossroadLights[3]);
                                        CrossroadData.trafficLight2_green = _modbusServer.BoolToStr(crossroadLights[4]);
                                        CrossroadData.trafficLight2_yellow = _modbusServer.BoolToStr(crossroadLights[5]);
                                        CrossroadData.trafficLight2_red = _modbusServer.BoolToStr(crossroadLights[6]);
                                        CrossroadData.pedestrian1_green = _modbusServer.BoolToStr(crossroadLights[7]);
                                        CrossroadData.pedestrian1_red = _modbusServer.BoolToStr(crossroadLights[8]);
                                        CrossroadData.pedestrian2_green = _modbusServer.BoolToStr(crossroadLights[9]);
                                        CrossroadData.pedestrian2_red = _modbusServer.BoolToStr(crossroadLights[10]);
                                    }

                                    // CrosswalkData lights: registers 41-51 (11 values)
                                    bool[] crosswalkLights = _modbusServer.GetRegisters(41, 11);
                                    if (crosswalkLights != null && crosswalkLights.Length >= 11)
                                    {
                                        CrosswalkData.crosswalkType = _modbusServer.BoolToStr(crosswalkLights[0]);
                                        CrosswalkData.trafficLight1_green = _modbusServer.BoolToStr(crosswalkLights[1]);
                                        CrosswalkData.trafficLight1_yellow = _modbusServer.BoolToStr(crosswalkLights[2]);
                                        CrosswalkData.trafficLight1_red = _modbusServer.BoolToStr(crosswalkLights[3]);
                                        CrosswalkData.trafficLight2_green = _modbusServer.BoolToStr(crosswalkLights[4]);
                                        CrosswalkData.trafficLight2_yellow = _modbusServer.BoolToStr(crosswalkLights[5]);
                                        CrosswalkData.trafficLight2_red = _modbusServer.BoolToStr(crosswalkLights[6]);
                                        CrosswalkData.pedestrian1_green = _modbusServer.BoolToStr(crosswalkLights[7]);
                                        CrosswalkData.pedestrian1_red = _modbusServer.BoolToStr(crosswalkLights[8]);
                                        CrosswalkData.pedestrian2_green = _modbusServer.BoolToStr(crosswalkLights[9]);
                                        CrosswalkData.pedestrian2_red = _modbusServer.BoolToStr(crosswalkLights[10]);
                                    }

                                    // RegulatorData

                                    // CarWashData outputs: registers 52-72 (21 values)
                                    bool[] carwashOutputs = _modbusServer.GetRegisters(52, 21);
                                    if (carwashOutputs != null && carwashOutputs.Length >= 21)
                                    {
                                        CarWashData.Light_green = _modbusServer.BoolToStr(carwashOutputs[0]);
                                        CarWashData.Light_yellow = _modbusServer.BoolToStr(carwashOutputs[1]);
                                        CarWashData.Light_red = _modbusServer.BoolToStr(carwashOutputs[2]);
                                        CarWashData.Door1_Up = _modbusServer.BoolToStr(carwashOutputs[3]);
                                        CarWashData.Door1_Down = _modbusServer.BoolToStr(carwashOutputs[4]);
                                        CarWashData.Door2_Up = _modbusServer.BoolToStr(carwashOutputs[5]);
                                        CarWashData.Door2_Down = _modbusServer.BoolToStr(carwashOutputs[6]);
                                        CarWashData.ChemicalsFront = _modbusServer.BoolToStr(carwashOutputs[7]);
                                        CarWashData.ChemicalsSides = _modbusServer.BoolToStr(carwashOutputs[8]);
                                        CarWashData.ChemicalsBack = _modbusServer.BoolToStr(carwashOutputs[9]);
                                        CarWashData.Prewash = _modbusServer.BoolToStr(carwashOutputs[10]);
                                        CarWashData.Water = _modbusServer.BoolToStr(carwashOutputs[11]);
                                        CarWashData.Wax = _modbusServer.BoolToStr(carwashOutputs[12]);
                                        CarWashData.Dry = _modbusServer.BoolToStr(carwashOutputs[13]);
                                        CarWashData.Brushes = _modbusServer.BoolToStr(carwashOutputs[14]);
                                        CarWashData.Soap = _modbusServer.BoolToStr(carwashOutputs[15]);
                                        CarWashData.ActiveFoam = _modbusServer.BoolToStr(carwashOutputs[16]);
                                        CarWashData.TimeDoorMovement = _modbusServer.IntToStr(carwashOutputs[20]); 
                                        CarWashData.MEMDoor = _modbusServer.BoolToStr(carwashOutputs[17]);
                                        CarWashData.MEMDoorTrig = _modbusServer.BoolToStr(carwashOutputs[18]);
                                        CarWashData.MEMDoorClosingtrig = _modbusServer.BoolToStr(carwashOutputs[19]);
                                        // carwashOutputs[20] reserved for CarWashTimeDoorMovement
                                    }

                                    // WashingMachineData outputs: registers 73-84 (12 values)
                                    bool[] washingmachineOutputs = _modbusServer.GetRegisters(73, 12);
                                    if (washingmachineOutputs != null && washingmachineOutputs.Length >= 12)
                                    {
                                        WashingMachineData.WashingMachineLight_green = _modbusServer.BoolToStr(washingmachineOutputs[0]);
                                        WashingMachineData.WashingMachineLight_yellow = _modbusServer.BoolToStr(washingmachineOutputs[1]);
                                        WashingMachineData.WashingMachineLight_red = _modbusServer.BoolToStr(washingmachineOutputs[2]);
                                        WashingMachineData.WashingMachineDoorClosed = _modbusServer.BoolToStr(washingmachineOutputs[3]);
                                        WashingMachineData.WashingMachineChemicals = _modbusServer.BoolToStr(washingmachineOutputs[4]);
                                        WashingMachineData.WashingMachinePrewash = _modbusServer.BoolToStr(washingmachineOutputs[5]);
                                        WashingMachineData.WashingMachineWater = _modbusServer.BoolToStr(washingmachineOutputs[6]);
                                        WashingMachineData.WashingMachineWax = _modbusServer.BoolToStr(washingmachineOutputs[7]);
                                        WashingMachineData.WashingMachineDry = _modbusServer.BoolToStr(washingmachineOutputs[8]);
                                        WashingMachineData.WashingMachineBrushes = _modbusServer.BoolToStr(washingmachineOutputs[9]);
                                        WashingMachineData.WashingMachineSoap = _modbusServer.BoolToStr(washingmachineOutputs[10]);
                                        WashingMachineData.WashingMachineActiveFoam = _modbusServer.BoolToStr(washingmachineOutputs[11]);
                                    }

                                    _ucCommunicationControl.SetStatus("Modbus Client: All data synchronized");
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

                            if (CrossroadData.btnStart == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrossroadStart;
                            }
                                
                            if (CrossroadData.btnPause == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnCrossroadPause;
                            }
                                
                            if (CrossroadData.btnStop == "true")
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

                                CrossroadData.crossroadType = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.crossroadType) != 0) ? "true" : "false";
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

                                CrossroadData.crossroadType = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.crossroadType) != 0) ? "true" : "false";   
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
                            int activeDBnumber = 1; // ? hard number 
                            byte[] readBuffer = new byte[20]; // ? find out 
                            byte[] writeBuffer = new byte[20]; // ? find out 

                            int read1 = _sharp7.readDB(activeDBnumber, CrossroadData.CrossroadReadBuffer, 0);

                            if (read1 == 0)
                            {                                
                                // CrossroadData bytes 0-3
                                CrossroadData.crossroadType = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 0));
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

                                // CrosswalkData bytes 4-7
                                CrosswalkData.crosswalkType = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 4, 0));
                                CrosswalkData.trafficLight1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 0));
                                CrosswalkData.trafficLight1_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 1));
                                CrosswalkData.trafficLight1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 2));
                                CrosswalkData.trafficLight2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 3));
                                CrosswalkData.trafficLight2_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 4));
                                CrosswalkData.trafficLight2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 5));
                                CrosswalkData.pedestrian1_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 6));
                                CrosswalkData.pedestrian1_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 6, 7));
                                CrosswalkData.pedestrian2_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 7, 0));
                                CrosswalkData.pedestrian2_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 7, 1));

                                // RegulatorData bytes 8-9
                                

                                // CarWashData bytes 10-14
                                CarWashData.Light_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 10, 0));
                                CarWashData.Light_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 10, 1));
                                CarWashData.Light_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 10, 2));
                                CarWashData.Door1_Up = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 0));
                                CarWashData.Door1_Down = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 1));
                                CarWashData.Door2_Up = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 2));
                                CarWashData.Door2_Down = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 3));
                                CarWashData.ChemicalsFront = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 4));
                                CarWashData.ChemicalsSides = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 5));
                                CarWashData.ChemicalsBack = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 6));
                                CarWashData.Prewash = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 11, 7));
                                CarWashData.Water = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 0));
                                CarWashData.Wax = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 1));
                                CarWashData.Dry = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 2));
                                CarWashData.Brushes = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 3));
                                CarWashData.Soap = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 4));
                                CarWashData.ActiveFoam = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 12, 5));
                                CarWashData.MEMDoor = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 13, 0));
                                CarWashData.MEMDoorTrig = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 13, 1));
                                CarWashData.MEMDoorClosingtrig = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 13, 2));

                                // WashingMachineData bytes 15-17
                                WashingMachineData.WashingMachineLight_green = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 0));
                                WashingMachineData.WashingMachineLight_yellow = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 1));
                                WashingMachineData.WashingMachineLight_red = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 2));
                                WashingMachineData.WashingMachineDoorClosed = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 3));
                                WashingMachineData.WashingMachineChemicals = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 4));
                                WashingMachineData.WashingMachinePrewash = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 5));
                                WashingMachineData.WashingMachineWater = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 6));
                                WashingMachineData.WashingMachineWax = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 15, 7));
                                WashingMachineData.WashingMachineDry = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 16, 0));
                                WashingMachineData.WashingMachineBrushes = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 16, 1));
                                WashingMachineData.WashingMachineSoap = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 16, 2));
                                WashingMachineData.WashingMachineActiveFoam = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 16, 3));

                                _ucCommunicationControl.SetStatus($"Sharp7: ReadDB OK - All data read from DB{activeDBnumber}");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. ReadDB returns {read1}.");
                                Logger.LogError($"Sharp7 ReadDB error: Return code {read1} while reading DB{activeDBnumber}.");
                            }

                            // CrossroadData write inputs to byte 0, outputs to bytes 2-3
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 0, Convert.ToBoolean(CrossroadData.crossroadType));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 1, Convert.ToBoolean(CrossroadData.btnStart));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 2, Convert.ToBoolean(CrossroadData.btnPause));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 3, Convert.ToBoolean(CrossroadData.btnStop));
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

                            // CrosswalkData write to bytes 4-7
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 0, Convert.ToBoolean(CrosswalkData.crosswalkType));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 1, Convert.ToBoolean(CrosswalkData.btnCrosswalkStart));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 2, Convert.ToBoolean(CrosswalkData.btnCrosswalkPause));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 3, Convert.ToBoolean(CrosswalkData.btnCrosswalkStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 4, Convert.ToBoolean(CrosswalkData.btnCrosswalk1));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 4, 5, Convert.ToBoolean(CrosswalkData.btnCrosswalk2));

                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 0, Convert.ToBoolean(CrosswalkData.trafficLight1_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 1, Convert.ToBoolean(CrosswalkData.trafficLight1_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 2, Convert.ToBoolean(CrosswalkData.trafficLight1_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 3, Convert.ToBoolean(CrosswalkData.trafficLight2_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 4, Convert.ToBoolean(CrosswalkData.trafficLight2_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 5, Convert.ToBoolean(CrosswalkData.trafficLight2_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 6, Convert.ToBoolean(CrosswalkData.pedestrian1_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 6, 7, Convert.ToBoolean(CrosswalkData.pedestrian1_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 7, 0, Convert.ToBoolean(CrosswalkData.pedestrian2_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 7, 1, Convert.ToBoolean(CrosswalkData.pedestrian2_red));

                            // RegulatorData write to bytes 8-9
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 8, 0, Convert.ToBoolean(RegulatorData.switchstate));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 8, 1, Convert.ToBoolean(RegulatorData.R));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 8, 2, Convert.ToBoolean(RegulatorData.C));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 8, 3, Convert.ToBoolean(RegulatorData.U));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 8, 4, Convert.ToBoolean(RegulatorData.Td));

                            // CarWashData write to bytes 10-14
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 0, Convert.ToBoolean(CarWashData.btnEmergencyStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 1, Convert.ToBoolean(CarWashData.btnStart));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 2, Convert.ToBoolean(CarWashData.btnStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 3, Convert.ToBoolean(CarWashData.ErrorSystem));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 4, Convert.ToBoolean(CarWashData.CarPosition));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 10, 5, Convert.ToBoolean(CarWashData.ShowerPosition));
                            
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 0, Convert.ToBoolean(CarWashData.Light_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 1, Convert.ToBoolean(CarWashData.Light_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 2, Convert.ToBoolean(CarWashData.Light_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 3, Convert.ToBoolean(CarWashData.Door1_Up));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 4, Convert.ToBoolean(CarWashData.Door1_Down));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 5, Convert.ToBoolean(CarWashData.Door2_Up));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 6, Convert.ToBoolean(CarWashData.Door2_Down));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 11, 7, Convert.ToBoolean(CarWashData.ChemicalsFront));
                
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 0, Convert.ToBoolean(CarWashData.ChemicalsSides));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 1, Convert.ToBoolean(CarWashData.ChemicalsBack));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 2, Convert.ToBoolean(CarWashData.Prewash));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 3, Convert.ToBoolean(CarWashData.Water));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 4, Convert.ToBoolean(CarWashData.Wax));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 5, Convert.ToBoolean(CarWashData.Dry));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 6, Convert.ToBoolean(CarWashData.Brushes));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 12, 7, Convert.ToBoolean(CarWashData.Soap));
                            
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 13, 0, Convert.ToBoolean(CarWashData.ActiveFoam));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 13, 1, Convert.ToBoolean(CarWashData.MEMDoor));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 13, 2, Convert.ToBoolean(CarWashData.MEMDoorTrig));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 13, 3, Convert.ToBoolean(CarWashData.MEMDoorClosingtrig));

                            // WashingMachineData write to bytes 15-17
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 15, 0, Convert.ToBoolean(WashingMachineData.btnWashingMachineEmergencyStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 15, 1, Convert.ToBoolean(WashingMachineData.btnStartWashingMachine));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 15, 2, Convert.ToBoolean(WashingMachineData.btnStopWashingMachine));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 15, 3, Convert.ToBoolean(WashingMachineData.WashingMachineErrorSystem));
                            
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 0, Convert.ToBoolean(WashingMachineData.WashingMachineLight_green));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 1, Convert.ToBoolean(WashingMachineData.WashingMachineLight_yellow));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 2, Convert.ToBoolean(WashingMachineData.WashingMachineLight_red));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 3, Convert.ToBoolean(WashingMachineData.WashingMachineDoorClosed));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 4, Convert.ToBoolean(WashingMachineData.WashingMachineChemicals));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 5, Convert.ToBoolean(WashingMachineData.WashingMachinePrewash));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 6, Convert.ToBoolean(WashingMachineData.WashingMachineWater));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 16, 7, Convert.ToBoolean(WashingMachineData.WashingMachineWax));
                            
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 17, 0, Convert.ToBoolean(WashingMachineData.WashingMachineDry));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 17, 1, Convert.ToBoolean(WashingMachineData.WashingMachineBrushes));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 17, 2, Convert.ToBoolean(WashingMachineData.WashingMachineSoap));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 17, 3, Convert.ToBoolean(WashingMachineData.WashingMachineActiveFoam));

                            int write1 = _sharp7.writeDB(activeDBnumber, CrossroadData.CrossroadWriteBuffer, 0);

                            if (write1 == 0)
                            {
                                _ucCommunicationControl.SetStatus($"Sharp7: WriteDB OK - All data written to DB{activeDBnumber}");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. WriteDB returns {write1}.");
                                Logger.LogError($"Sharp7 writeDB error: Return code {write1} for DB {activeDBnumber}");
                            }

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
