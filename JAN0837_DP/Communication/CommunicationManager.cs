using JAN0837_DP.Communication.comModbusTCPIP;
using JAN0837_DP.Communication.comMQTT;
using JAN0837_DP.Communication.comOPCUA;
using JAN0837_DP.Communication.comRESTAPI;
using JAN0837_DP.Communication.comS7;
using JAN0837_DP.Communication.comSharp7;
using JAN0837_DP.Communication.comTCPIP;
using JAN0837_DP.Data;
using JAN0837_DP.Forms;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
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
        public comTCPIP.comTCPIP _tcpip;

        public ucCommunicationControl _ucCommunicationControl;

        public CommunicationManager(ucCommunicationControl ucCommunicationControl)
        {
            _ucCommunicationControl = ucCommunicationControl;
        }

        public async void Communication()
        {
            try
            {
                while (internalVariables.communicationThreadRunningFlag) // communicationRunningFlag
                {
                    switch (internalVariables.communicationFlag)
                    {
                        case "MQTT":
                            //MQTT();

                            string brokerAddress = internalVariables.txtBoxParam1;
                            string secondPara = internalVariables.txtBoxParam2;

                            if (internalVariables.checkBoxMaster == true)
                            {

                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {

                            }
                            else
                            {
                                // no checkbox selected 
                            }

                            break;
                        case "OPCUA":
                            //OPCUA();

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
                            }

                            break;
                        case "ModbusTCPIP":
                            //ModbusTCPIP();

                            string ModbusTCPIP_ipAddress = internalVariables.txtBoxParam1;
                            string txtPort = internalVariables.txtBoxParam2;
                            int txpPort;

                            if (!int.TryParse(txtPort, out txpPort))
                            {
                                // error port not valid number 
                                return;
                            }

                            if (internalVariables.checkBoxMaster == true)
                            {
                                ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ModbusTCPIP_ipAddress, txpPort);

                                if (modbusClient.ConnectToSlave())
                                {
                                    byte slaveId = 1;
                                    ushort startAddress = 0;

                                    // Čtení jednoho registru
                                    ushort[] values = modbusClient.ReadHoldingRegisters(slaveId, startAddress, 1);
                                    if (values != null)
                                        Console.WriteLine($"Přečtená hodnota: {values[0]}");

                                    // Zápis do registru
                                    modbusClient.WriteSingleRegister(slaveId, startAddress, 1234);

                                    // Odpojení
                                    modbusClient.DisconnectFromSlave();
                                }
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ModbusTCPIP_ipAddress, txpPort);
                                modbusServer.Start(); // Spustíme Modbus Slave

                                // Simulace změny hodnoty registru
                                modbusServer.SetRegisterValue(0, 1234);

                                Console.ReadLine(); // Čekáme, dokud uživatel nestiskne Enter
                                modbusServer.Stop(); // Ukončení serveru
                            }
                            else
                            {
                                // no checkbox selected 
                            }

                            break;
                        case "TCPIP":
                            string ipAddress = internalVariables.txtBoxParam1;

                            // connect 
                            bool connect = _tcpip.Connect();

                            if (connect == true)
                            {
                                _ucCommunicationControl.SetStatus($"TCPIP connected to {ipAddress}.");
                            }
                            else
                            {
                                _ucCommunicationControl.SetStatus($"TCPIP connection to {ipAddress} failed.");
                                return; // break;
                            }
                            
                            if (internalVariables.checkBoxMaster == true)
                            {
                                // read data
                                string incoming_data = _tcpip.ReadData();

                                // write data
                                string outcoming_data = "";
                                bool write = _tcpip.WriteData(outcoming_data);
                            }
                            else if (internalVariables.checkBoxSlave == true)
                            {
                                // write data
                                string outcoming_data = "";
                                bool write = _tcpip.WriteData(outcoming_data);

                                // read data
                                string incoming_data = _tcpip.ReadData();
                            }
                            else
                            {
                                // choose what is this device
                                _ucCommunicationControl.SetStatus($"Please, choose what is your device.");
                            }

                            // disconnect
                            if (connect == true)
                            {
                                bool disconnected = _tcpip.Disconnect();

                                if (disconnected == true)
                                {
                                    _ucCommunicationControl.SetStatus("TCP/IP disconnected successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus("Error in TCP/IP disconnection.");
                                }
                            }

                            break;
                        case "RESTAPI":
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

                            break;
                        case "Sharp7":
                            _sharp7 ??= new comSharp7.comSharp7();

                            string Sharp7_ipAddress = internalVariables.txtBoxParam1;

                            if (_sharp7.client.Connected == false)
                            {
                                int plcConnect = _sharp7.connectToPLC(Sharp7_ipAddress);

                                if (plcConnect == 0)
                                {
                                    _ucCommunicationControl.SetStatus($"PLC connected successfully.");
                                }
                                else
                                {
                                    _ucCommunicationControl.SetStatus($"Error in Sharp7 communication. ConnectToPLC returns {plcConnect}.");
                                    return; // break;
                                }
                            }

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
