using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

using JAN0837_DP.Communication.ModbusTCPIP;
using JAN0837_DP.Communication.S7;
using JAN0837_DP.Communication.Sharp7;
using JAN0837_DP.Communication.TCPIP;
using JAN0837_DP.Communication.RESTAPI;
using JAN0837_DP.Communication.OPCUA;
using JAN0837_DP.Communication.MQTT;
using JAN0837_DP.Data;
using Siemens.Engineering.HW;
using System.Security.Cryptography.X509Certificates;
using JAN0837_DP.Forms;

namespace JAN0837_DP.Communication
{
    public class CommunicationManager
    {
        public S7.S7 _s7;
        public Sharp7.Sharp7 sharp7;
        public ucCommunicationControl ucCommunicationControl;

        public void Communication()
        {
            try
            {
                while (internalVariables.communicationThreadRunningFlag) // communicationRunningFlag
                {
                    if (internalVariables.opcuaFlag == true)
                    {
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
                    }
                    else if (internalVariables.mqttFlag == true)
                    {
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
                    }
                    else if (internalVariables.tcpipFlag == true)
                    {
                        //TCPIP();

                        string ipAddress = internalVariables.txtBoxParam1;
                    }
                    else if (internalVariables.modbustcpipFlag == true)
                    {
                        //ModbusTCPIP();

                        string ipAddress = internalVariables.txtBoxParam1;
                        string txtPort = internalVariables.txtBoxParam2;
                        int txpPort;

                        if (!int.TryParse(txtPort, out txpPort))
                        {
                            // error port not valid number 
                            return;
                        }

                        if (internalVariables.checkBoxMaster == true)
                        {
                            ModbusTCPIPimMaster modbusClient = new ModbusTCPIPimMaster(ipAddress, txpPort);

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
                            ModbusTCPIPimSlave modbusServer = new ModbusTCPIPimSlave(ipAddress, txpPort);
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
                    }
                    else if (internalVariables.restapiFlag == true)
                    {
                        //RESTAPI();

                        string url = internalVariables.txtBoxParam1;
                    }
                    else if (internalVariables.s7Flag == true)
                    {
                        // S7 -> Sharp7
                        string ipAddress = internalVariables.txtBoxParam1;
                        string cpuType = internalVariables.txtBoxParam2;
                        
                        if (_s7.plc.IsConnected != true)
                        {
                            // connect 
                            _s7.connectToPLC(cpuType, ipAddress);
                        }

                        // get data from plc

                        // crossroad
                        _s7.getBytes(CrossroadData.CrossroadDBnumber, 0, CrossroadData.CrossroadReadBuffer.Length, CrossroadData.CrossroadReadBuffer);
                    }
                    else if (internalVariables.sharp7Flag == true)
                    {
                        //
                        string ipAddress = internalVariables.txtBoxParam1;

                        if (sharp7.client != null)
                        {
                            sharp7.connectToPLC(ipAddress);
                        }

                        sharp7.readDB(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);
                        sharp7.readS7MultiVar(CrossroadData.CrossroadDBnumber, CrossroadData.CrossroadReadBuffer, 0);
                    }
                    else
                    {
                        // Error -> neni zaklikla predvolba 
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
