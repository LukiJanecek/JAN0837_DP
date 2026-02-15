using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using JAN0837_DP.Log;

namespace JAN0837_DP.Communication.comModbusTCPIP
{
    public class ModbusTCPIPimMaster
    {
        public string ipAddress;
        public int port;
        public TcpClient tcpClient;
        public ModbusIpMaster master;

        // Konstruktor
        public ModbusTCPIPimMaster(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public bool StrToBool(string s)
            => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

        public string BoolToStr(bool b)
            => b ? "true" : "false";

        public bool ConnectToSlave()
        {
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                master = ModbusIpMaster.CreateIp(tcpClient);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with connection to Modbus server: {ex.Message}.");
                Logger.LogException(ex, "Failed to connect to Modbus server");
                return false;
            }
        }

        public bool DisconnectFromSlave()
        {
            try
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                    master = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with disconnection from Slave: {ex.Message}.");
                Logger.LogException(ex, "Failed to disconnect from Modbus server");
                return false;
            }
        }

        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected.");
                return master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reading error: {ex.Message}");
                Logger.LogException(ex, "Failed to read holding registers");
                return null;
            }
        }

        public ushort[] ReadInputRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                return master.ReadInputRegisters(slaveId, startAddress, numRegisters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reading error: {ex.Message}");
                Logger.LogException(ex, "Failed to read input registers");
                return null;
            }
        }

        public bool[] ReadCoils(byte slaveId, ushort startAddress, ushort numCoils)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                return master.ReadCoils(slaveId, startAddress, numCoils);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reading error: {ex.Message}");
                Logger.LogException(ex, "Failed to read coils");
                return null;
            }
        }

        public void WriteSingleRegister(byte slaveId, ushort address, ushort value)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                master.WriteSingleRegister(slaveId, address, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writting error: {ex.Message}");
                Logger.LogException(ex, "Failed to write single register");
            }
        }

        public void WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] values)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                master.WriteMultipleRegisters(slaveId, startAddress, values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writting error: {ex.Message}");
                Logger.LogException(ex, "Failed to write multiple registers");
            }
        }

        public void WriteSingleCoil(byte slaveId, ushort address, bool value)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                master.WriteSingleCoil(slaveId, address, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writting error: {ex.Message}");
                Logger.LogException(ex, "Failed to write single coil");
            }
        }

        public void WriteMultipleCoils(byte slaveId, ushort startAddress, bool[] values)
        {
            try
            {
                if (master == null) throw new Exception("Modbus is not connected!");
                master.WriteMultipleCoils(slaveId, startAddress, values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writting error: {ex.Message}");
                Logger.LogException(ex, "Failed to write multiple coils");
            }
        }
    }

    public class ModbusTCPIPimSlave
    {
        public TcpListener tcpListener;
        public ModbusSlave slave;
        private ushort[] holdingRegisters = new ushort[10]; // Registr pro uchování dat

        // Konstruktor 
        public ModbusTCPIPimSlave(string ipAddress, int port)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            tcpListener = new TcpListener(address, port);
        }

        public bool StrToBool(string s)
            => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

        public string BoolToStr(bool b)
            => b ? "true" : "false";

        public bool Start()
        {
            try
            {
                tcpListener.Start();
                slave = ModbusTcpSlave.CreateTcp(1, tcpListener); // Slave ID = 1
                slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();

                slave.Listen(); // Spustí poslouchání požadavků od Masteru
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Starting server error: {ex.Message}");
                Logger.LogException(ex, "Failed to start Modbus TCP/IP Slave");
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (slave != null)
                {
                    slave.Dispose();
                    tcpListener.Stop();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stopping server error: {ex.Message}");
                Logger.LogException(ex, "Failed to stop Modbus TCP/IP Slave");
                return false;
            }
        }

        public void SetRegisterValue(ushort address, ushort value)
        {
            if (address < holdingRegisters.Length)
            {
                holdingRegisters[address] = value;
            }
            else
            {
                Console.WriteLine($"Wrong registr address: {address}");
                Logger.LogError($"Wrong register address: {address}");
            }
        }

        public ushort GetRegisterValue(ushort address)
        {
            if (address < holdingRegisters.Length)
            {
                return holdingRegisters[address];
            }
            else
            {
                Console.WriteLine($"Wrong registr address: {address}");
                Logger.LogError($"Wrong register address: {address}");
                return 0;
            }
        }

        public void SetCoil(ushort address, bool value)
        {
            if (slave?.DataStore?.CoilDiscretes == null) return;

            ushort idx = (ushort)(address + 1); // 1-based
            if (idx < slave.DataStore.CoilDiscretes.Count)
            {
                slave.DataStore.CoilDiscretes[idx] = value;
            }
            else 
            {
                Console.WriteLine($"Wrong coil address: {address}");
                Logger.LogError($"Wrong coil address: {address}");
            }
        }

        public void SetCoils(ushort startAddress, bool[] values)
        {
            if (slave?.DataStore?.CoilDiscretes == null || values == null) return;

            for (int i = 0; i < values.Length; i++)
            {
                ushort idx = (ushort)(startAddress + i + 1);
                if (idx < slave.DataStore.CoilDiscretes.Count)
                {
                    slave.DataStore.CoilDiscretes[idx] = values[i];
                }
                else
                {
                    Console.WriteLine($"Wrong coil address: {(startAddress + i)}");
                    Logger.LogError($"Wrong coil address: {(startAddress + i)}");
                }
            }
        }

        public bool[] GetCoils(ushort startAddress, ushort count)
        {
            if (slave?.DataStore?.CoilDiscretes == null) return null;

            bool[] result = new bool[count];

            for (int i = 0; i < count; i++)
            {
                ushort idx = (ushort)(startAddress + i + 1);
                if (idx < slave.DataStore.CoilDiscretes.Count)
                {
                    result[i] = slave.DataStore.CoilDiscretes[idx];
                }
                    
                else
                {
                    Console.WriteLine($"Wrong coil address: {(startAddress + i)}");
                    Logger.LogError($"Wrong coil address: {(startAddress + i)}");
                }
            }

            return result;
        }
    }
}
