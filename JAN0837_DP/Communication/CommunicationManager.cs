using JAN0837_DP.Communication.comModbusTCPIP;
using JAN0837_DP.Communication.comMQTT;
using JAN0837_DP.Communication.comOPCUA;
using JAN0837_DP.Communication.comRESTAPI;
using JAN0837_DP.Communication.comS7;
using JAN0837_DP.Communication.comSharp7;
using JAN0837_DP.Communication.comTCPIP;
using JAN0837_DP.Data;
using JAN0837_DP.Forms;
using Microsoft.AspNetCore.Connections.Features;
using Newtonsoft.Json;
using Opc.Ua;
using Org.BouncyCastle.Asn1.Cmp;
using Siemens.Engineering.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static JAN0837_DP.Communication.comTCPIP.comTCPIP;

namespace JAN0837_DP.Communication
{
    public class CommunicationManager
    {
        public comS7.comS7 _s7;
        public comSharp7.comSharp7 _sharp7;
        public comTCPIP.comTCPIP _tcpip;
        public comModbusTCPIP.ModbusTCPIPimMaster _modbusMaster;
        public comModbusTCPIP.ModbusTCPIPimSlave _modbusSlave;
        public MQTTBroker _mqttBroker;
        public MQTTClient _mqttClient;

        public ucCommunicationControl _ucCommunicationControl;

        public CommunicationManager(ucCommunicationControl ucCommunicationControl)
        {
            _ucCommunicationControl = ucCommunicationControl;
        }

        public async void Communication(CancellationToken token) 
        {
            try
            {
                while (internalVariables.communicationThreadRunningFlag && !token.IsCancellationRequested) // communicationRunningFlag
                {
                    switch (internalVariables.communicationFlag)
                    {
                        case "MQTT":
                            string broker_ipAddress = internalVariables.txtBoxParam1;
                            if (!int.TryParse(internalVariables.txtBoxParam2, out int broker_port))
                            {
                                _ucCommunicationControl.SetStatus($"Port is not a valid number.");
                                return; // break;
                            }

                            if (internalVariables.checkBoxMaster == true)
                            {
                                // publish
                                if (_mqttClient == null || !_mqttClient.mqttClient.IsConnected)
                                    break;

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

                                await _mqttClient.mqttClient.PublishAsync(msg);
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                // publish 
                                if (_mqttClient == null || !_mqttClient.mqttClient.IsConnected)
                                    break;

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

                                await _mqttClient.mqttClient.PublishAsync(msg);
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

                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {

                            }
                            else
                            {
                                // no checkbox selected 
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "ModbusTCPIP":
                            string ip = internalVariables.txtBoxParam1;
                            
                            if (!int.TryParse(internalVariables.txtBoxParam2, out int port))
                            {
                                _ucCommunicationControl.SetStatus("Error: Modbus TCP/IP port is not a valid number.");
                                break; // return;
                            }

                            if (internalVariables.checkBoxMaster == true)
                            {
                                //ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ModbusTCPIP_ipAddress, txpPort);
                                byte slaveId = 1;
                                /*
                                ushort startAddress = 0;

                                // Čtení jednoho registru
                                ushort[] values = _modbusMaster.ReadHoldingRegisters(slaveId, startAddress, 1);
                                if (values != null && values.Length > 0)
                                {
                                    Console.WriteLine($"Přečtená hodnota: {values[0]}");
                                }    

                                // Zápis do registru
                                _modbusMaster.WriteSingleRegister(slaveId, startAddress, 1234);
                                */

                                // writting multiple registers
                                bool[] cmd = new bool[6];

                                cmd[0] = _modbusMaster.StrToBool(CrossroadData.crossroadType);
                                cmd[1] = _modbusMaster.StrToBool(CrossroadData.btnCrossroadStart);
                                cmd[2] = _modbusMaster.StrToBool(CrossroadData.btnCrossroadPause);
                                cmd[3] = _modbusMaster.StrToBool(CrossroadData.btnCrossroadStop);
                                cmd[4] = _modbusMaster.StrToBool(CrossroadData.btnCrosswalk1);
                                cmd[5] = _modbusMaster.StrToBool(CrossroadData.btnCrosswalk2);

                                // startAddress = 0, count = 6
                                _modbusMaster.WriteMultipleCoils(slaveId, 0, cmd);

                                // reading multiple registers
                                // startAddress = 10, count = 10
                                bool[] st = _modbusMaster.ReadCoils(slaveId, 10, 10);

                                if (st == null || st.Length < 10) return;

                                CrossroadData.trafficLight1_green = _modbusMaster.BoolToStr(st[0]);
                                CrossroadData.trafficLight1_yellow = _modbusMaster.BoolToStr(st[1]);
                                CrossroadData.trafficLight1_red = _modbusMaster.BoolToStr(st[2]);

                                CrossroadData.trafficLight2_green = _modbusMaster.BoolToStr(st[3]);
                                CrossroadData.trafficLight2_yellow = _modbusMaster.BoolToStr(st[4]);
                                CrossroadData.trafficLight2_red = _modbusMaster.BoolToStr(st[5]);

                                CrossroadData.pedestrian1_green = _modbusMaster.BoolToStr(st[6]);
                                CrossroadData.pedestrian1_red = _modbusMaster.BoolToStr(st[7]);
                                CrossroadData.pedestrian2_green = _modbusMaster.BoolToStr(st[8]);
                                CrossroadData.pedestrian2_red = _modbusMaster.BoolToStr(st[9]);

                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                //ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ModbusTCPIP_ipAddress, txpPort);
                                
                                // Simulace změny hodnoty registru
                                //_modbusSlave.SetRegisterValue(0, 1234);

                                // write
                                bool[] cmd = new bool[]
                                {
                                    _modbusSlave.StrToBool(CrossroadData.crossroadType),
                                    _modbusSlave.StrToBool(CrossroadData.btnCrossroadStart),
                                    _modbusSlave.StrToBool(CrossroadData.btnCrossroadPause),
                                    _modbusSlave.StrToBool(CrossroadData.btnCrossroadStop),
                                    _modbusSlave.StrToBool(CrossroadData.btnCrosswalk1),
                                    _modbusSlave.StrToBool(CrossroadData.btnCrosswalk2)
                                };

                                _modbusSlave.SetCoils(0, cmd);

                                // read 
                                bool[] st = _modbusSlave.GetCoils(10, 10); // 10..19

                                if (st == null || st.Length < 10) return;

                                CrossroadData.trafficLight1_green = _modbusSlave.BoolToStr(st[0]); // 10
                                CrossroadData.trafficLight1_yellow = _modbusSlave.BoolToStr(st[1]); // 11
                                CrossroadData.trafficLight1_red = _modbusSlave.BoolToStr(st[2]); // 12

                                CrossroadData.trafficLight2_green = _modbusSlave.BoolToStr(st[3]); // 13
                                CrossroadData.trafficLight2_yellow = _modbusSlave.BoolToStr(st[4]); // 14
                                CrossroadData.trafficLight2_red = _modbusSlave.BoolToStr(st[5]); // 15

                                CrossroadData.pedestrian1_green = _modbusSlave.BoolToStr(st[6]); // 16
                                CrossroadData.pedestrian1_red = _modbusSlave.BoolToStr(st[7]); // 17
                                CrossroadData.pedestrian2_green = _modbusSlave.BoolToStr(st[8]); // 18
                                CrossroadData.pedestrian2_red = _modbusSlave.BoolToStr(st[9]); // 19

                            }
                            else
                            {
                                // no checkbox selected 
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "TCPIP":
                            
                            byte buttons = 0;

                            if (CrossroadData.btnCrossroadStart == "true")
                            {
                                buttons |= (byte)comTCPIP.comTCPIP.ButtonFlags.BtnCrossroadStart;
                            }
                                
                            if (CrossroadData.btnCrossroadPause == "true")
                            {
                                buttons |= (byte)comTCPIP.comTCPIP.ButtonFlags.BtnCrossroadPause;
                            }
                                
                            if (CrossroadData.btnCrossroadStop == "true")
                            {
                                buttons |= (byte)comTCPIP.comTCPIP.ButtonFlags.BtnCrossroadStop;
                            }
                                
                            if (CrossroadData.btnCrosswalk1 == "true")
                            {
                                buttons |= (byte)comTCPIP.comTCPIP.ButtonFlags.BtnCrosswalk1;
                            }
                                
                            if (CrossroadData.btnCrosswalk2 == "true")
                            {
                                buttons |= (byte)comTCPIP.comTCPIP.ButtonFlags.BtnCrosswalk2;
                            }

                            byte[] outTelegram = new byte[] { buttons };

                            if (internalVariables.checkBoxMaster == true)
                            {
                                // write data
                                bool send = _tcpip.SendBytes(outTelegram);

                                if (send == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data sent successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data sending.");
                                    break; //return;
                                }

                                // read data
                                byte[] inTelegram = new byte[2];
                                bool read = _tcpip.ReceiveExact(inTelegram);

                                if (read == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data read successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data reading.");
                                    break; //return;
                                }

                                CrossroadData.trafficLight1_green = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_yellow = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_red = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_green = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_yellow = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_red = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_green = ((inTelegram[0] & (byte)LightFlagsByte0.Pedestrian1_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_red = ((inTelegram[0] & (byte)LightFlagsByte0.Pedestrian1_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_green = ((inTelegram[1] & (byte)LightFlagsByte1.Pedestrian2_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_red = ((inTelegram[1] & (byte)LightFlagsByte1.Pedestrian2_Red) != 0) ? "true" : "false";
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                // write data
                                bool send = _tcpip.SendBytes(outTelegram);

                                if (send == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data sent successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data sending.");
                                    break; //return;
                                }

                                // read data
                                byte[] inTelegram = new byte[2];
                                bool read = _tcpip.ReceiveExact(inTelegram);

                                if (read == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP data read successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP data reading.");
                                    break; //return;
                                }

                                CrossroadData.trafficLight1_green = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_yellow = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight1_red = ((inTelegram[0] & (byte)LightFlagsByte0.Light1_Red) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_green = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Green) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_yellow = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Yellow) != 0) ? "true" : "false";
                                CrossroadData.trafficLight2_red = ((inTelegram[0] & (byte)LightFlagsByte0.Light2_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_green = ((inTelegram[0] & (byte)LightFlagsByte0.Pedestrian1_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian1_red = ((inTelegram[0] & (byte)LightFlagsByte0.Pedestrian1_Red) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_green = ((inTelegram[1] & (byte)LightFlagsByte1.Pedestrian2_Green) != 0) ? "true" : "false";
                                CrossroadData.pedestrian2_red = ((inTelegram[1] & (byte)LightFlagsByte1.Pedestrian2_Red) != 0) ? "true" : "false";
                            }
                            else
                            {
                                // choose what is this device
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            break;
                        case "RESTAPI":
                            /*
                            string url = internalVariables.txtBoxParam1;

                            HttpClient client = new HttpClient();
                            client.BaseAddress = new Uri(url);

                            comRESTAPI.comRESTAPI restAPIClient = new comRESTAPI.comRESTAPI();

                            bool status = await restAPIClient.apiGet(client);
                            
                            if (status == true)
                            {
                                _ucCommunicationControl.SetStatus($"REST API GET request successful.");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"REST API GET request failed.");
                            }

                            string message = await restAPIClient.apiPost(client);

                            if (!string.IsNullOrEmpty(message))
                            {
                                _ucCommunicationControl.SetStatus($"REST API POST request successful. Response: {message}");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"REST API POST request failed. Response: {message}");
                            }
                            */

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
                            break;
                    }

                    await Task.Delay(50, token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: Error in communication: {ex.Message}");
                //_ucCommunicationControl.SetStatus($"Exception: Error in communication: {ex.Message}");
            }
        }
    }
}
