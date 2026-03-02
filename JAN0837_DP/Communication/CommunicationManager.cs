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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
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
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
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
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Input")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput))
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .WithRetainFlag(true)
                                        .Build());

                                    var regultorOutput = new
                                    {
                                        Uc1 = RegulatorData.Uc1,
                                        Uc2 = RegulatorData.Uc2, 
                                        PV = RegulatorData.PV
                                    };

                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regultorOutput))
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"

                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output 
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        sswitchstate = RegulatorData.switchstate == "true",
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Regulator/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var regulatorOutput = new
                                    {
                                        Uc1 = RegulatorData.Uc1,
                                        Uc2 = RegulatorData.Uc2,
                                        PV = RegulatorData.PV
                                    };

                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                                        .WithTopic("JAN0837/Regulator/Output")
                                        .WithPayload(System.Text.Json.JsonSerializer.Serialize(regulatorOutput))
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                        btnstart = CrossroadData.btnStart == "true",
                                        btnpause = CrossroadData.btnPause == "true",
                                        btnstop = CrossroadData.btnStop == "true",
                                        btncwW1 = CrossroadData.btnWestCrosswalk1 == "true",
                                        btncwW2 = CrossroadData.btnWestCrosswalk2 == "true",
                                        btncwS1 = CrossroadData.btnSouthCrosswalk1 == "true",
                                        btncwS2 = CrossroadData.btnSouthCrosswalk2 == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Input").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadInput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    var crossroadOutput = new
                                    {
                                        type = CrossroadData.crossroadType == "true",
                                        tlN_green = CrossroadData.trafficLightNorth_green == "true",
                                        tnN_yellow = CrossroadData.trafficLightNorth_yellow == "true",
                                        tlN_red = CrossroadData.trafficLightNorth_red == "true",
                                        tlS_green = CrossroadData.trafficLightSouth_green == "true",
                                        tlS_yellow = CrossroadData.trafficLightSouth_yellow == "true",
                                        tlS_red = CrossroadData.trafficLightSouth_red == "true",
                                        tlW_green = CrossroadData.trafficLightEast_green == "true",
                                        tlW_yellow = CrossroadData.trafficLightEast_yellow == "true",
                                        tlW_red = CrossroadData.trafficLightEast_red == "true",
                                        tlE_green = CrossroadData.trafficLightWest_green == "true",
                                        tlE_yellow = CrossroadData.trafficLightWest_yellow == "true",
                                        tlE_red = CrossroadData.trafficLightWest_red == "true",
                                        //pedN_green = CrossroadData.pedestrianNorth_green == "true",
                                        //pedN_red = CrossroadData.pedestrianNorth_red == "true",
                                        pedW_green = CrossroadData.pedestrianWest_green == "true",
                                        pedW_red = CrossroadData.pedestrianWest_red == "true",
                                        pedS_green = CrossroadData.pedestrianSouth_green == "true",
                                        pedS_red = CrossroadData.pedestrianSouth_red == "true",
                                        //pedE_green = CrossroadData.pedestrianEast_green == "true",
                                        //pedE_red = CrossroadData.pedestrianEast_red == "true"
                                    };
                                    await client.mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder().WithTopic("JAN0837/Crossroad/Output").WithPayload(System.Text.Json.JsonSerializer.Serialize(crossroadOutput)).WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag(true).Build());

                                    // Crosswalk Input/Output
                                    var crosswalkInput = new
                                    {
                                        start = CrosswalkData.btnStart == "true",
                                        pause = CrosswalkData.btnPause == "true",
                                        stop = CrosswalkData.btnStop == "true",
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
                                        order = RegulatorData.order,
                                        R1 = RegulatorData.R1,
                                        R2 = RegulatorData.R2,
                                        C1 = RegulatorData.C1,
                                        C2 = RegulatorData.C2,
                                        Uin = RegulatorData.Uin,
                                        Td = RegulatorData.Td,
                                        Ts = RegulatorData.Ts
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
                                        emergencyStop = WashingMachineData.btnEmergencyStop == "true",
                                        start = WashingMachineData.btnStart == "true",
                                        stop = WashingMachineData.btnStop == "true",
                                        errorSystem = WashingMachineData.ErrorSystem == "true",
                                        mode = WashingMachineData.Mode
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
                                        light_green = WashingMachineData.Light_green == "true",
                                        light_yellow = WashingMachineData.Light_yellow == "true",
                                        light_red = WashingMachineData.Light_red == "true",
                                        doorClosed = WashingMachineData.DoorClosed == "true",
                                        chemicals = WashingMachineData.Chemicals == "true",
                                        prewash = WashingMachineData.Prewash == "true",
                                        water = WashingMachineData.Water == "true",
                                        wax = WashingMachineData.Wax == "true",
                                        dry = WashingMachineData.Dry == "true",
                                        brushes = WashingMachineData.Brushes == "true",
                                        soap = WashingMachineData.Soap == "true",
                                        activeFoam = WashingMachineData.ActiveFoam == "true"
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
                                    CrossroadData.btnWestCrosswalk1 = opcuaServer.ReadVariable("BtnWestCrosswalk1") ? "true" : "false";
                                    CrossroadData.btnWestCrosswalk2 = opcuaServer.ReadVariable("BtnWestCrosswalk2") ? "true" : "false";
                                    CrossroadData.btnSouthCrosswalk1 = opcuaServer.ReadVariable("BtnSouthCrosswalk1") ? "true" : "false";
                                    CrossroadData.btnSouthCrosswalk2 = opcuaServer.ReadVariable("BtnSouthCrosswalk2") ? "true" : "false";

                                    opcuaServer.UpdateBoolVariable("CrossroadType", CrossroadData.crossroadType == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightNorth_Green", CrossroadData.trafficLightNorth_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightNorth_Yellow", CrossroadData.trafficLightNorth_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightNorth_Red", CrossroadData.trafficLightNorth_red == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightSouth_Green", CrossroadData.trafficLightSouth_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightSouth_Yellow", CrossroadData.trafficLightSouth_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightSouth_Red", CrossroadData.trafficLightSouth_red == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightEast_Green", CrossroadData.trafficLightEast_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightEast_Yellow", CrossroadData.trafficLightEast_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightEast_Red", CrossroadData.trafficLightEast_red == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightWest_Green", CrossroadData.trafficLightWest_green == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightWest_Yellow", CrossroadData.trafficLightWest_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("TrafficLightWest_Red", CrossroadData.trafficLightWest_red == "true");
                                    //opcuaServer.UpdateBoolVariable("PedestrianNorth_Green", CrossroadData.pedestrianNorth_green == "true");
                                    //opcuaServer.UpdateBoolVariable("PedestrianNorth_Red", CrossroadData.pedestrianNorth_red == "true");
                                    opcuaServer.UpdateBoolVariable("PedestrianSouth_Green", CrossroadData.pedestrianSouth_green == "true");
                                    opcuaServer.UpdateBoolVariable("PedestrianSouth_Red", CrossroadData.pedestrianSouth_red == "true");
                                    opcuaServer.UpdateBoolVariable("PedestrianWest_Green", CrossroadData.pedestrianWest_green == "true");
                                    opcuaServer.UpdateBoolVariable("PedestrianWest_Red", CrossroadData.pedestrianWest_red == "true");
                                    //opcuaServer.UpdateBoolVariable("PedestrianEast_Green", CrossroadData.pedestrianEast_green == "true");
                                    //opcuaServer.UpdateBoolVariable("PedestrianEast_Red", CrossroadData.pedestrianEast_red == "true");

                                    // CrosswalkData
                                    CrosswalkData.btnStart = opcuaServer.ReadVariable("BtnCrosswalkStart_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnPause = opcuaServer.ReadVariable("BtnCrosswalkPause_Crosswalk") ? "true" : "false";
                                    CrosswalkData.btnStop = opcuaServer.ReadVariable("BtnCrosswalkStop_Crosswalk") ? "true" : "false";
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

                                    // RegulatorData 
                                    RegulatorData.switchstate = opcuaServer.ReadVariable("Switchstate") ? "true" : "false";
                                    RegulatorData.order = opcuaServer.ReadVariableAsString("Order");
                                    RegulatorData.R1 = opcuaServer.ReadVariableAsString("R1");
                                    RegulatorData.R2 = opcuaServer.ReadVariableAsString("R2");
                                    RegulatorData.C1 = opcuaServer.ReadVariableAsString("C1");
                                    RegulatorData.C2 = opcuaServer.ReadVariableAsString("C2");
                                    RegulatorData.Uin = opcuaServer.ReadVariableAsString("Uin");
                                    RegulatorData.Td = opcuaServer.ReadVariableAsString("Td");
                                    RegulatorData.Ts = opcuaServer.ReadVariableAsString("Ts");

                                    opcuaServer.UpdateRealVariable("Uc1", double.TryParse(RegulatorData.Uc1, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var uc1Val) ? uc1Val : 0.0);
                                    opcuaServer.UpdateRealVariable("Uc2", double.TryParse(RegulatorData.Uc2, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var uc2Val) ? uc2Val : 0.0);
                                    opcuaServer.UpdateRealVariable("PV", double.TryParse(RegulatorData.PV, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pvVal) ? pvVal : 0.0);

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
                                    opcuaServer.UpdateStringVariable("TimeDoorMovement", CarWashData.TimeDoorMovement);
                                    opcuaServer.UpdateBoolVariable("MEMDoor", CarWashData.MEMDoor == "true");
                                    opcuaServer.UpdateBoolVariable("MEMDoorTrig", CarWashData.MEMDoorTrig == "true");
                                    opcuaServer.UpdateBoolVariable("MEMDoorClosingtrig", CarWashData.MEMDoorClosingtrig == "true");

                                    // WashingMachine
                                    WashingMachineData.btnEmergencyStop = opcuaServer.ReadVariable("BtnWashingMachineEmergencyStop") ? "true" : "false";
                                    WashingMachineData.btnStart = opcuaServer.ReadVariable("BtnStartWashingMachine") ? "true" : "false";
                                    WashingMachineData.btnStop = opcuaServer.ReadVariable("BtnStopWashingMachine") ? "true" : "false";
                                    WashingMachineData.ErrorSystem = opcuaServer.ReadVariable("WashingMachineErrorSystem") ? "true" : "false";
                                    WashingMachineData.Mode = opcuaServer.ReadVariable("WashingMachineMode").ToString();

                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Green", WashingMachineData.Light_green == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Yellow", WashingMachineData.Light_yellow == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineLight_Red", WashingMachineData.Light_red == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineDoorClosed", WashingMachineData.DoorClosed == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineChemicals", WashingMachineData.Chemicals == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachinePrewash", WashingMachineData.Prewash == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineWater", WashingMachineData.Water == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineWax", WashingMachineData.Wax == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineDry", WashingMachineData.Dry == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineBrushes", WashingMachineData.Brushes == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineSoap", WashingMachineData.Soap == "true");
                                    opcuaServer.UpdateBoolVariable("WashingMachineActiveFoam", WashingMachineData.ActiveFoam == "true");

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
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnWestCrosswalk1, CrossroadData.btnWestCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnWestCrosswalk2, CrossroadData.btnWestCrosswalk2 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnSouthCrosswalk1, CrossroadData.btnSouthCrosswalk1 == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrossroadData.OpcUaNodeIds.btnSouthCrosswalk2, CrossroadData.btnSouthCrosswalk2 == "true");

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
                                    CrossroadData.trafficLightNorth_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightNorth_green) ? "true" : "false";
                                    CrossroadData.trafficLightNorth_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightNorth_yellow) ? "true" : "false";
                                    CrossroadData.trafficLightNorth_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightNorth_red) ? "true" : "false";
                                    CrossroadData.trafficLightSouth_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightSouth_green) ? "true" : "false";
                                    CrossroadData.trafficLightSouth_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightSouth_yellow) ? "true" : "false";
                                    CrossroadData.trafficLightSouth_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightSouth_red) ? "true" : "false";
                                    CrossroadData.trafficLightEast_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightEast_green) ? "true" : "false";
                                    CrossroadData.trafficLightEast_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightEast_yellow) ? "true" : "false";
                                    CrossroadData.trafficLightEast_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightEast_red) ? "true" : "false";
                                    CrossroadData.trafficLightWest_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightWest_green) ? "true" : "false";
                                    CrossroadData.trafficLightWest_yellow = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightWest_yellow) ? "true" : "false";
                                    CrossroadData.trafficLightWest_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.trafficLightWest_red) ? "true" : "false";
                                    // CrossroadData.pedestrianNorth_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianNorth_green) ? "true" : "false";
                                    // CrossroadData.pedestrianNorth_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianNorth_red) ? "true" : "false";
                                    CrossroadData.pedestrianSouth_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianSouth_green) ? "true" : "false";
                                    CrossroadData.pedestrianSouth_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianSouth_red) ? "true" : "false";
                                    CrossroadData.pedestrianWest_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianWest_green) ? "true" : "false";
                                    CrossroadData.pedestrianWest_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianWest_red) ? "true" : "false";
                                    // CrossroadData.pedestrianEast_green = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianEast_green) ? "true" : "false";
                                    // CrossroadData.pedestrianEast_red = opcuaClient.ReadOPCUABool(opcuaClient, CrossroadData.OpcUaNodeIds.pedestrianEast_red) ? "true" : "false";

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
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnStart, CrosswalkData.btnStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnPause, CrosswalkData.btnPause == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, CrosswalkData.OpcUaNodeIds.btnStop, CrosswalkData.btnStop == "true");
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
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.order, int.Parse(RegulatorData.order));
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.R1, RegulatorData.R1);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.R2, RegulatorData.R2);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.C1, RegulatorData.C1);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.C2, RegulatorData.C2);
                                    opcuaClient.WriteOPCUAValue(opcuaClient, RegulatorData.OpcUaNodeIds.Uin, RegulatorData.Uin);

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, waiting for reconnection...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // read output values from PLC 
                                    RegulatorData.Uc1 = opcuaClient.ReadOPCUAFloat(opcuaClient, RegulatorData.OpcUaNodeIds.Uc1).ToString();
                                    RegulatorData.Uc2 = opcuaClient.ReadOPCUAFloat(opcuaClient, RegulatorData.OpcUaNodeIds.Uc2).ToString();
                                    RegulatorData.PV = opcuaClient.ReadOPCUAFloat(opcuaClient, RegulatorData.OpcUaNodeIds.PV).ToString();

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during read, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during read, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

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
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnEmergencyStop, WashingMachineData.btnEmergencyStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnStart, WashingMachineData.btnStart == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.btnStop, WashingMachineData.btnStop == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.ErrorSystem, WashingMachineData.ErrorSystem == "true");
                                    opcuaClient.WriteOPCUAValue(opcuaClient, WashingMachineData.OpcUaNodeIds.Mode, WashingMachineData.Mode == "true");

                                    // If session became invalid during writes, skip reads this cycle
                                    if (!opcuaClient.connected)
                                    {
                                        _ucCommunicationControl.SetStatus("OPC UA: Session lost during write, will reconnect...");
                                        Logger.LogWarning("OPC UA Client: Session lost during write, waiting for reconnection...");
                                        await Task.Delay(1000, token);
                                        continue;
                                    }

                                    // Read output values from PLC 
                                    WashingMachineData.Light_green = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Light_green) ? "true" : "false";
                                    WashingMachineData.Light_yellow = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Light_yellow) ? "true" : "false";
                                    WashingMachineData.Light_red = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Light_red) ? "true" : "false";
                                    WashingMachineData.DoorClosed = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.DoorClosed) ? "true" : "false";
                                    WashingMachineData.Chemicals = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Chemicals) ? "true" : "false";
                                    WashingMachineData.Prewash = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Prewash) ? "true" : "false";
                                    WashingMachineData.Water = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Water) ? "true" : "false";
                                    WashingMachineData.Wax = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Wax) ? "true" : "false";
                                    WashingMachineData.Dry = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Dry) ? "true" : "false";
                                    WashingMachineData.Brushes = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Brushes) ? "true" : "false";
                                    WashingMachineData.Soap = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.Soap) ? "true" : "false";
                                    WashingMachineData.ActiveFoam = opcuaClient.ReadOPCUABool(opcuaClient, WashingMachineData.OpcUaNodeIds.ActiveFoam) ? "true" : "false";

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
                                    bool[] crossroadInputs = new bool[7]
                                    {
                                        _modbusServer.StrToBool(CrossroadData.btnStart),
                                        _modbusServer.StrToBool(CrossroadData.btnPause),
                                        _modbusServer.StrToBool(CrossroadData.btnStop),
                                        _modbusServer.StrToBool(CrossroadData.btnWestCrosswalk1),
                                        _modbusServer.StrToBool(CrossroadData.btnWestCrosswalk2),
                                        _modbusServer.StrToBool(CrossroadData.btnSouthCrosswalk1),
                                        _modbusServer.StrToBool(CrossroadData.btnSouthCrosswalk2)
                                    };
                                    _modbusServer.SetRegisters(0, crossroadInputs);

                                    // CrosswalkData buttons: registers 5-9
                                    bool[] crosswalkInputs = new bool[5]
                                    {
                                        _modbusServer.StrToBool(CrosswalkData.btnStart),
                                        _modbusServer.StrToBool(CrosswalkData.btnPause),
                                        _modbusServer.StrToBool(CrosswalkData.btnStop),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalk1),
                                        _modbusServer.StrToBool(CrosswalkData.btnCrosswalk2)
                                    };
                                    _modbusServer.SetRegisters(5, crosswalkInputs);

                                    // RegulatorData inputs: registers 10-14 (switchstate + R,C,U,Td as bool for simplicity)
                                    bool[] regulatorInputs = new bool[9] // TO DO -> not only bool
                                    {
                                        _modbusServer.StrToBool(RegulatorData.switchstate),
                                        _modbusServer.StrToInt(RegulatorData.order) != 0,
                                        _modbusServer.StrToBool(RegulatorData.R1),
                                        _modbusServer.StrToBool(RegulatorData.R2),
                                        _modbusServer.StrToBool(RegulatorData.C1),
                                        _modbusServer.StrToBool(RegulatorData.C2),
                                        _modbusServer.StrToBool(RegulatorData.Uin),
                                        _modbusServer.StrToBool(RegulatorData.Td),
                                        _modbusServer.StrToBool(RegulatorData.Ts)
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
                                        _modbusServer.StrToInt(CarWashData.Mode) != 0
                                    };
                                    _modbusServer.SetRegisters(15, carwashInputs);

                                    // WashingMachine inputs: registers 22-26
                                    bool[] washingmachineInputs = new bool[5]
                                    {
                                        _modbusServer.StrToBool(WashingMachineData.btnEmergencyStop),
                                        _modbusServer.StrToBool(WashingMachineData.btnStart),
                                        _modbusServer.StrToBool(WashingMachineData.btnStop),
                                        _modbusServer.StrToBool(WashingMachineData.ErrorSystem),
                                        _modbusServer.StrToBool(WashingMachineData.Mode)
                                    };
                                    _modbusServer.SetRegisters(22, washingmachineInputs);

                                    // ═══════════════════════════════════════════════════════════
                                    // READ output values that slaves wrote to our holding registers
                                    // ═══════════════════════════════════════════════════════════

                                    // CrossroadData lights: registers 30-40 (11 values)
                                    bool[] crossroadOutputs = _modbusServer.GetRegisters(30, 11);
                                    if (crossroadOutputs != null && crossroadOutputs.Length >= 11)
                                    {
                                        CrossroadData.crossroadType = _modbusServer.BoolToStr(crossroadOutputs[0]);
                                        CrossroadData.trafficLightNorth_green = _modbusServer.BoolToStr(crossroadOutputs[1]);
                                        CrossroadData.trafficLightNorth_yellow = _modbusServer.BoolToStr(crossroadOutputs[2]);
                                        CrossroadData.trafficLightNorth_red = _modbusServer.BoolToStr(crossroadOutputs[3]);
                                        CrossroadData.trafficLightSouth_green = _modbusServer.BoolToStr(crossroadOutputs[4]);
                                        CrossroadData.trafficLightSouth_yellow = _modbusServer.BoolToStr(crossroadOutputs[5]);
                                        CrossroadData.trafficLightSouth_red = _modbusServer.BoolToStr(crossroadOutputs[6]);
                                        CrossroadData.trafficLightEast_green = _modbusServer.BoolToStr(crossroadOutputs[7]);
                                        CrossroadData.trafficLightEast_yellow = _modbusServer.BoolToStr(crossroadOutputs[8]);
                                        CrossroadData.trafficLightEast_red = _modbusServer.BoolToStr(crossroadOutputs[9]);
                                        CrossroadData.trafficLightWest_green = _modbusServer.BoolToStr(crossroadOutputs[10]);
                                        CrossroadData.trafficLightWest_yellow = _modbusServer.BoolToStr(crossroadOutputs[11]);
                                        CrossroadData.trafficLightWest_red = _modbusServer.BoolToStr(crossroadOutputs[12]);
                                        // CrossroadData.pedestrianNorth_green = _modbusServer.BoolToStr(crossroadLOutputs[13]);
                                        // CrossroadData.pedestrianNorth_red = _modbusServer.BoolToStr(crossroadOutputs[14]);
                                        CrossroadData.pedestrianSouth_green = _modbusServer.BoolToStr(crossroadOutputs[13]);
                                        CrossroadData.pedestrianSouth_green = _modbusServer.BoolToStr(crossroadOutputs[14]);
                                        CrossroadData.pedestrianWest_green = _modbusServer.BoolToStr(crossroadOutputs[15]);
                                        CrossroadData.pedestrianWest_red = _modbusServer.BoolToStr(crossroadOutputs[16]);
                                        // CrossroadData.pedestrianEast_green = _modbusServer.BoolToStr(crossroadOutputs[17]);4
                                        // CrossroadData.pedestrianEast_red = _modbusServer.BoolToStr(crossroadOutputs[18]);
                                    }

                                    // CrosswalkData lights: registers 41-51 (11 values)
                                    bool[] crosswalkOutputs = _modbusServer.GetRegisters(41, 11);
                                    if (crosswalkOutputs != null && crosswalkOutputs.Length >= 11)
                                    {
                                        CrosswalkData.crosswalkType = _modbusServer.BoolToStr(crosswalkOutputs[0]);
                                        CrosswalkData.trafficLight1_green = _modbusServer.BoolToStr(crosswalkOutputs[1]);
                                        CrosswalkData.trafficLight1_yellow = _modbusServer.BoolToStr(crosswalkOutputs[2]);
                                        CrosswalkData.trafficLight1_red = _modbusServer.BoolToStr(crosswalkOutputs[3]);
                                        CrosswalkData.trafficLight2_green = _modbusServer.BoolToStr(crosswalkOutputs[4]);
                                        CrosswalkData.trafficLight2_yellow = _modbusServer.BoolToStr(crosswalkOutputs[5]);
                                        CrosswalkData.trafficLight2_red = _modbusServer.BoolToStr(crosswalkOutputs[6]);
                                        CrosswalkData.pedestrian1_green = _modbusServer.BoolToStr(crosswalkOutputs[7]);
                                        CrosswalkData.pedestrian1_red = _modbusServer.BoolToStr(crosswalkOutputs[8]);
                                        CrosswalkData.pedestrian2_green = _modbusServer.BoolToStr(crosswalkOutputs[9]);
                                        CrosswalkData.pedestrian2_red = _modbusServer.BoolToStr(crosswalkOutputs[10]);
                                    }

                                    // RegulatorData
                                    bool[] regulaotOutputs = _modbusServer.GetRegisters(51, 9);
                                    if (regulaotOutputs != null && regulaotOutputs.Length >= 9)
                                    {
                                        RegulatorData.Uc1 = _modbusServer.IntToStr(regulaotOutputs[0]);
                                        RegulatorData.Uc2 = _modbusServer.IntToStr(regulaotOutputs[1]);
                                        RegulatorData.PV = _modbusServer.IntToStr(regulaotOutputs[2]);
                                    }

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
                                        WashingMachineData.Light_green = _modbusServer.BoolToStr(washingmachineOutputs[0]);
                                        WashingMachineData.Light_yellow = _modbusServer.BoolToStr(washingmachineOutputs[1]);
                                        WashingMachineData.Light_red = _modbusServer.BoolToStr(washingmachineOutputs[2]);
                                        WashingMachineData.DoorClosed = _modbusServer.BoolToStr(washingmachineOutputs[3]);
                                        WashingMachineData.Chemicals = _modbusServer.BoolToStr(washingmachineOutputs[4]);
                                        WashingMachineData.Prewash = _modbusServer.BoolToStr(washingmachineOutputs[5]);
                                        WashingMachineData.Water = _modbusServer.BoolToStr(washingmachineOutputs[6]);
                                        WashingMachineData.Wax = _modbusServer.BoolToStr(washingmachineOutputs[7]);
                                        WashingMachineData.Dry = _modbusServer.BoolToStr(washingmachineOutputs[8]);
                                        WashingMachineData.Brushes = _modbusServer.BoolToStr(washingmachineOutputs[9]);
                                        WashingMachineData.Soap = _modbusServer.BoolToStr(washingmachineOutputs[10]);
                                        WashingMachineData.ActiveFoam = _modbusServer.BoolToStr(washingmachineOutputs[11]);
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
                                
                            if (CrossroadData.btnWestCrosswalk1 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnWestCrosswalk1;
                            }
                                
                            if (CrossroadData.btnWestCrosswalk2 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnWestCrosswalk2;
                            }

                            if (CrossroadData.btnSouthCrosswalk1 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnSouthCrosswalk1;
                            }

                            if (CrossroadData.btnSouthCrosswalk2 == "true")
                            {
                                buttons |= (byte)comTCPIPClient.ButtonFlags.BtnSouthCrosswalk2;
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
                                CrossroadData.trafficLightNorth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightNorth_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightNorth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrianSouth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianWest_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrianSouth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianWest_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrianWest_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianEast_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrianWest_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianEast_Red) != 0) ? "true" : "false";
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
                                CrossroadData.trafficLightNorth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightNorth_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightNorth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightNorth_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightSouth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightSouth_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightWest_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightWest_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_yellow = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLightEast_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.LightEast_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrianSouth_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianWest_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrianSouth_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianWest_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrianWest_green = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianEast_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrianWest_red = ((inTelegram[0] & (byte)comTCPIPClient.LightFlagsByte0.PedestrianEast_Red) != 0) ? "true" : "false";
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

                            int read1 = _sharp7.readDB(activeDBnumber, readBuffer, 0);

                            if (read1 == 0)
                            {                                
                                // CrossroadData bytes 0-3
                                CrossroadData.crossroadType = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 0, 0));
                                CrossroadData.trafficLightNorth_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 0));
                                CrossroadData.trafficLightNorth_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 1));
                                CrossroadData.trafficLightNorth_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 2));
                                CrossroadData.trafficLightSouth_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 3));
                                CrossroadData.trafficLightSouth_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 4));
                                CrossroadData.trafficLightSouth_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 5));
                                CrossroadData.trafficLightWest_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 0));
                                CrossroadData.trafficLightWest_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 1));
                                CrossroadData.trafficLightWest_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 2));
                                CrossroadData.trafficLightEast_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 3));
                                CrossroadData.trafficLightEast_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 4));
                                CrossroadData.trafficLightEast_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 5));

                                CrossroadData.pedestrianSouth_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 6));
                                CrossroadData.pedestrianSouth_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 2, 7));
                                CrossroadData.pedestrianWest_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 0));
                                CrossroadData.pedestrianWest_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 3, 1));

                                // CrosswalkData bytes 4-7
                                CrosswalkData.crosswalkType = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 4, 0));
                                CrosswalkData.trafficLight1_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 0));
                                CrosswalkData.trafficLight1_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 1));
                                CrosswalkData.trafficLight1_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 2));
                                CrosswalkData.trafficLight2_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 3));
                                CrosswalkData.trafficLight2_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 4));
                                CrosswalkData.trafficLight2_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 5));
                                CrosswalkData.pedestrian1_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 6));
                                CrosswalkData.pedestrian1_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 6, 7));
                                CrosswalkData.pedestrian2_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 7, 0));
                                CrosswalkData.pedestrian2_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 7, 1));

                                // RegulatorData bytes 8-9
                                

                                // CarWashData bytes 10-14
                                CarWashData.Light_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 10, 0));
                                CarWashData.Light_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 10, 1));
                                CarWashData.Light_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 10, 2));
                                CarWashData.Door1_Up = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 0));
                                CarWashData.Door1_Down = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 1));
                                CarWashData.Door2_Up = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 2));
                                CarWashData.Door2_Down = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 3));
                                CarWashData.ChemicalsFront = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 4));
                                CarWashData.ChemicalsSides = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 5));
                                CarWashData.ChemicalsBack = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 6));
                                CarWashData.Prewash = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 11, 7));
                                CarWashData.Water = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 0));
                                CarWashData.Wax = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 1));
                                CarWashData.Dry = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 2));
                                CarWashData.Brushes = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 3));
                                CarWashData.Soap = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 4));
                                CarWashData.ActiveFoam = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 12, 5));
                                CarWashData.MEMDoor = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 13, 0));
                                CarWashData.MEMDoorTrig = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 13, 1));
                                CarWashData.MEMDoorClosingtrig = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 13, 2));

                                // WashingMachineData bytes 15-17
                                WashingMachineData.Light_green = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 0));
                                WashingMachineData.Light_yellow = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 1));
                                WashingMachineData.Light_red = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 2));
                                WashingMachineData.DoorClosed = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 3));
                                WashingMachineData.Chemicals = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 4));
                                WashingMachineData.Prewash = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 5));
                                WashingMachineData.Water = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 6));
                                WashingMachineData.Wax = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 15, 7));
                                WashingMachineData.Dry = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 16, 0));
                                WashingMachineData.Brushes = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 16, 1));
                                WashingMachineData.Soap = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 16, 2));
                                WashingMachineData.ActiveFoam = Convert.ToString(Sharp7.S7.GetBitAt(readBuffer, 16, 3));

                                _ucCommunicationControl.SetStatus($"Sharp7: ReadDB OK - All data read from DB{activeDBnumber}");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. ReadDB returns {read1}.");
                                Logger.LogError($"Sharp7 ReadDB error: Return code {read1} while reading DB{activeDBnumber}.");
                            }

                            // CrossroadData write inputs to byte 0, outputs to bytes 2-3
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 0, Convert.ToBoolean(CrossroadData.crossroadType));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 1, Convert.ToBoolean(CrossroadData.btnStart));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 2, Convert.ToBoolean(CrossroadData.btnPause));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 3, Convert.ToBoolean(CrossroadData.btnStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 4, Convert.ToBoolean(CrossroadData.btnWestCrosswalk1));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 5, Convert.ToBoolean(CrossroadData.btnWestCrosswalk2));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 6, Convert.ToBoolean(CrossroadData.btnSouthCrosswalk1));
                            Sharp7.S7.SetBitAt(writeBuffer, 0, 7, Convert.ToBoolean(CrossroadData.btnSouthCrosswalk2));

                            Sharp7.S7.SetBitAt(writeBuffer, 1, 0, Convert.ToBoolean(CrossroadData.trafficLightNorth_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 1, 1, Convert.ToBoolean(CrossroadData.trafficLightNorth_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 1, 2, Convert.ToBoolean(CrossroadData.trafficLightNorth_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 1, 3, Convert.ToBoolean(CrossroadData.trafficLightSouth_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 1, 4, Convert.ToBoolean(CrossroadData.trafficLightSouth_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 1, 5, Convert.ToBoolean(CrossroadData.trafficLightSouth_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 0, Convert.ToBoolean(CrossroadData.trafficLightWest_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 1, Convert.ToBoolean(CrossroadData.trafficLightWest_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 2, Convert.ToBoolean(CrossroadData.trafficLightWest_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 3, Convert.ToBoolean(CrossroadData.trafficLightEast_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 4, Convert.ToBoolean(CrossroadData.trafficLightEast_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 5, Convert.ToBoolean(CrossroadData.trafficLightEast_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 6, Convert.ToBoolean(CrossroadData.pedestrianSouth_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 2, 7, Convert.ToBoolean(CrossroadData.pedestrianSouth_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 3, 0, Convert.ToBoolean(CrossroadData.pedestrianWest_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 3, 1, Convert.ToBoolean(CrossroadData.pedestrianWest_red));
                            // CrosswalkData write to bytes 4-7
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 0, Convert.ToBoolean(CrosswalkData.crosswalkType));
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 1, Convert.ToBoolean(CrosswalkData.btnStart));
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 2, Convert.ToBoolean(CrosswalkData.btnPause));
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 3, Convert.ToBoolean(CrosswalkData.btnStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 4, Convert.ToBoolean(CrosswalkData.btnCrosswalk1));
                            Sharp7.S7.SetBitAt(writeBuffer, 4, 5, Convert.ToBoolean(CrosswalkData.btnCrosswalk2));

                            Sharp7.S7.SetBitAt(writeBuffer, 6, 0, Convert.ToBoolean(CrosswalkData.trafficLight1_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 1, Convert.ToBoolean(CrosswalkData.trafficLight1_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 2, Convert.ToBoolean(CrosswalkData.trafficLight1_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 3, Convert.ToBoolean(CrosswalkData.trafficLight2_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 4, Convert.ToBoolean(CrosswalkData.trafficLight2_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 5, Convert.ToBoolean(CrosswalkData.trafficLight2_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 6, Convert.ToBoolean(CrosswalkData.pedestrian1_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 6, 7, Convert.ToBoolean(CrosswalkData.pedestrian1_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 7, 0, Convert.ToBoolean(CrosswalkData.pedestrian2_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 7, 1, Convert.ToBoolean(CrosswalkData.pedestrian2_red));

                            // RegulatorData write to bytes 8-9
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 0, Convert.ToBoolean(RegulatorData.switchstate));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 0, Convert.ToInt64(RegulatorData.order) != 0);
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 1, Convert.ToBoolean(RegulatorData.R1));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 1, Convert.ToBoolean(RegulatorData.R2));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 2, Convert.ToBoolean(RegulatorData.C1));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 2, Convert.ToBoolean(RegulatorData.C2));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 3, Convert.ToBoolean(RegulatorData.Uin));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 4, Convert.ToBoolean(RegulatorData.Td));
                            Sharp7.S7.SetBitAt(writeBuffer, 8, 4, Convert.ToBoolean(RegulatorData.Ts));

                            Sharp7.S7.SetBitAt(writeBuffer, 9, 0, Convert.ToDouble(RegulatorData.Uc1) != 0.0);
                            Sharp7.S7.SetBitAt(writeBuffer, 9, 0, Convert.ToDouble(RegulatorData.Uc2) != 0.0);
                            Sharp7.S7.SetBitAt(writeBuffer, 9, 1, Convert.ToDouble(RegulatorData.PV) != 0.0);

                            // CarWashData write to bytes 10-14
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 0, Convert.ToBoolean(CarWashData.btnEmergencyStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 1, Convert.ToBoolean(CarWashData.btnStart));
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 2, Convert.ToBoolean(CarWashData.btnStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 3, Convert.ToBoolean(CarWashData.ErrorSystem));
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 4, Convert.ToBoolean(CarWashData.CarPosition));
                            Sharp7.S7.SetBitAt(writeBuffer, 10, 5, Convert.ToBoolean(CarWashData.ShowerPosition));
                            
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 0, Convert.ToBoolean(CarWashData.Light_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 1, Convert.ToBoolean(CarWashData.Light_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 2, Convert.ToBoolean(CarWashData.Light_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 3, Convert.ToBoolean(CarWashData.Door1_Up));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 4, Convert.ToBoolean(CarWashData.Door1_Down));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 5, Convert.ToBoolean(CarWashData.Door2_Up));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 6, Convert.ToBoolean(CarWashData.Door2_Down));
                            Sharp7.S7.SetBitAt(writeBuffer, 11, 7, Convert.ToBoolean(CarWashData.ChemicalsFront));
                
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 0, Convert.ToBoolean(CarWashData.ChemicalsSides));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 1, Convert.ToBoolean(CarWashData.ChemicalsBack));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 2, Convert.ToBoolean(CarWashData.Prewash));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 3, Convert.ToBoolean(CarWashData.Water));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 4, Convert.ToBoolean(CarWashData.Wax));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 5, Convert.ToBoolean(CarWashData.Dry));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 6, Convert.ToBoolean(CarWashData.Brushes));
                            Sharp7.S7.SetBitAt(writeBuffer, 12, 7, Convert.ToBoolean(CarWashData.Soap));
                            
                            Sharp7.S7.SetBitAt(writeBuffer, 13, 0, Convert.ToBoolean(CarWashData.ActiveFoam));
                            Sharp7.S7.SetBitAt(writeBuffer, 13, 1, Convert.ToBoolean(CarWashData.MEMDoor));
                            Sharp7.S7.SetBitAt(writeBuffer, 13, 2, Convert.ToBoolean(CarWashData.MEMDoorTrig));
                            Sharp7.S7.SetBitAt(writeBuffer, 13, 3, Convert.ToBoolean(CarWashData.MEMDoorClosingtrig));

                            // WashingMachineData write to bytes 15-17
                            Sharp7.S7.SetBitAt(writeBuffer, 15, 0, Convert.ToBoolean(WashingMachineData.btnEmergencyStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 15, 1, Convert.ToBoolean(WashingMachineData.btnStart));
                            Sharp7.S7.SetBitAt(writeBuffer, 15, 2, Convert.ToBoolean(WashingMachineData.btnStop));
                            Sharp7.S7.SetBitAt(writeBuffer, 15, 3, Convert.ToBoolean(WashingMachineData.ErrorSystem));
                            
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 0, Convert.ToBoolean(WashingMachineData.Light_green));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 1, Convert.ToBoolean(WashingMachineData.Light_yellow));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 2, Convert.ToBoolean(WashingMachineData.Light_red));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 3, Convert.ToBoolean(WashingMachineData.DoorClosed));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 4, Convert.ToBoolean(WashingMachineData.Chemicals));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 5, Convert.ToBoolean(WashingMachineData.Prewash));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 6, Convert.ToBoolean(WashingMachineData.Water));
                            Sharp7.S7.SetBitAt(writeBuffer, 16, 7, Convert.ToBoolean(WashingMachineData.Wax));
                            
                            Sharp7.S7.SetBitAt(writeBuffer, 17, 0, Convert.ToBoolean(WashingMachineData.Dry));
                            Sharp7.S7.SetBitAt(writeBuffer, 17, 1, Convert.ToBoolean(WashingMachineData.Brushes));
                            Sharp7.S7.SetBitAt(writeBuffer, 17, 2, Convert.ToBoolean(WashingMachineData.Soap));
                            Sharp7.S7.SetBitAt(writeBuffer, 17, 3, Convert.ToBoolean(WashingMachineData.ActiveFoam));

                            int write1 = _sharp7.writeDB(activeDBnumber, writeBuffer, 0);

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
