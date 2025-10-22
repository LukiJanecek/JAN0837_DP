using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

using JAN0837_DP.Communication.comModbusTCPIP;
using JAN0837_DP.Communication.comS7;
using JAN0837_DP.Communication.comSharp7;
using JAN0837_DP.Communication.comTCPIP;
using JAN0837_DP.Communication.comRESTAPI;
using JAN0837_DP.Communication.comOPCUA;
using JAN0837_DP.Communication.comMQTT;
using JAN0837_DP.Data;
using Siemens.Engineering.HW;
using System.Security.Cryptography.X509Certificates;
using JAN0837_DP.Forms;

namespace JAN0837_DP.Communication
{
    public class CommunicationManager
    {
        public comS7.comS7 _s7;
        public comSharp7.comSharp7 _sharp7;
        public ucCommunicationControl ucCommunicationControl;

        public void Communication()
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
                            //TCPIP();

                            string ipAddress = internalVariables.txtBoxParam1;

                            break;
                        case "RESTAPI":

                            //RESTAPI();

                            string url = internalVariables.txtBoxParam1;

                            break;
                        case "Sharp7":
                            _sharp7 ??= new comSharp7.comSharp7();

                            //
                            string Sharp7_ipAddress = internalVariables.txtBoxParam1;

                            if (_sharp7.client != null)
                            {
                                _sharp7.connectToPLC(Sharp7_ipAddress);
                            }

                            // choose between these two methods -> please test me

                            // reading from PLC 
                            int activeDBnumber = CrossroadData.CrossroadDBnumber;

                            int read1 = _sharp7.readDB(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);
                            //bool read2 = _sharp7.readS7MultiVar(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);
                            int read2 = 0;

                            if (read1 == 0)
                            {
                                switch (activeDBnumber)
                                {
                                    case CrossroadData.CrossroadDBnumber:

                                        CrossroadData.crossroadType = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 0));
                                        //CrossroadData.btnCrossroadStart = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 2));
                                        //CrossroadData.btnCrossroadPause = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 3));
                                        //CrossroadData.btnCrossroadStop = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 4));
                                        //CrossroadData.btnCrosswalk1 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 5));
                                        //CrossroadData.btnCrosswalk2 = Convert.ToString(Sharp7.S7.GetBitAt(CrossroadData.CrossroadReadBuffer, 0, 6));

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

                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                // read failed -> Exception?
                            }

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

                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                // read failed -> Exception?
                            }

                            // writting to PLC 

                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 1, Convert.ToBoolean(CrossroadData.btnCrossroadStart));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 2, Convert.ToBoolean(CrossroadData.btnCrossroadPause));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 3, Convert.ToBoolean(CrossroadData.btnCrossroadStop));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 4, Convert.ToBoolean(CrossroadData.btnCrosswalk1));
                            Sharp7.S7.SetBitAt(CrossroadData.CrossroadWriteBuffer, 0, 5, Convert.ToBoolean(CrossroadData.btnCrosswalk2));

                            int write1 = _sharp7.writeDB(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadWriteBuffer, 0);
                            //bool write2 = _sharp7.writeS7MultiVar(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadWriteBuffer, 0);
                            int write2 = 0;

                            if (write1 == 0)
                            {
                                // write was successful
                            }
                            else
                            {
                                // write failed -> Exception?
                            }

                            if (write2 == 0)
                            {
                                // write was successful
                            }
                            else
                            {
                                // write failed -> Exception?
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in communication: {ex.Message}");
            }
        }
    }
}
