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
using S7.Net.Types;

namespace JAN0837_DP.Communication.comModbusTCPIP
{
    // Shared helpers for float ↔ Modbus register conversion
    public static class ModbusHelper
    {
        // Float (32-bit) occupies 2 Modbus registers (CDAB word order: low word first, high word second)
        public static void FloatToRegisters(float value, ushort[] registers, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            registers[offset] = (ushort)((bytes[1] << 8) | bytes[0]);     // low word
            registers[offset + 1] = (ushort)((bytes[3] << 8) | bytes[2]); // high word
        }

        public static float RegistersToFloat(ushort[] registers, int offset)
        {
            byte[] bytes = new byte[4];
            bytes[1] = (byte)(registers[offset] >> 8);
            bytes[0] = (byte)(registers[offset] & 0xFF);
            bytes[3] = (byte)(registers[offset + 1] >> 8);
            bytes[2] = (byte)(registers[offset + 1] & 0xFF);
            return BitConverter.ToSingle(bytes, 0);
        }
    }

    public class ModbusTCPIPimClient
    {
        public string ipAddress;
        public int port;
        public TcpClient tcpClient;
        public ModbusIpMaster master;

        // Konstruktor
        public ModbusTCPIPimClient(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public bool StrToBool(string s)
            => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

        public string BoolToStr(bool b)
            => b ? "true" : "false";

        public bool IsConnected
        {
            get
            {
                try
                {
                    return tcpClient != null && tcpClient.Connected && master != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool ConnectToSlave()
        {
            try
            {
                if (IsConnected)
                    return true;

                // Clean up old connection if any
                DisconnectFromSlave();

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
                master?.Dispose();
                master = null;
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with disconnection from Slave: {ex.Message}.");
                Logger.LogException(ex, "Failed to disconnect from Modbus server");
                master = null;
                tcpClient = null;
                return false;
            }
        }

        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to read holding registers when Modbus is not connected");
                    return null;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to read input registers when Modbus is not connected");
                    return null;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to read coils when Modbus is not connected");
                    return null;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to write single register when Modbus is not connected");
                    return;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to write registers when Modbus is not connected");
                    return;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to write single coil when Modbus is not connected");
                    return;
                }
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
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to write coils when Modbus is not connected");
                    return;
                }
                master.WriteMultipleCoils(slaveId, startAddress, values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writting error: {ex.Message}");
                Logger.LogException(ex, "Failed to write multiple coils");
            }
        }

        // Helper methods for working with holding registers as boolean values
        public void WriteMultipleRegistersAsBool(byte slaveId, ushort startAddress, bool[] values)
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to write registers when Modbus is not connected");
                    return;
                }

                ushort[] registers = new ushort[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    registers[i] = (ushort)(values[i] ? 1 : 0);
                }

                master.WriteMultipleRegisters(slaveId, startAddress, registers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writing error: {ex.Message}");
                Logger.LogException(ex, "Failed to write multiple registers as bool");
            }
        }

        public bool[] ReadHoldingRegistersAsBool(byte slaveId, ushort startAddress, ushort count)
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.LogError("Attempt to read registers when Modbus is not connected");
                    return null;
                }

                ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, count);
                bool[] result = new bool[registers.Length];
                
                for (int i = 0; i < registers.Length; i++)
                {
                    result[i] = registers[i] != 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reading error: {ex.Message}");
                Logger.LogException(ex, "Failed to read holding registers as bool");
                return null;
            }
        }
    }

    public class ModbusTCPIPimServer
    {
        public TcpListener tcpListener;
        public ModbusSlave slave;
        private ushort[] holdingRegisters = new ushort[10]; // Registr pro uchování dat

        // Konstruktor 
        public ModbusTCPIPimServer(string ipAddress, int port)
        {
            // Bind to IPAddress.Any to listen on all interfaces
            // The ipAddress parameter is ignored for server mode - servers should listen on all interfaces
            tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public bool StrToBool(string s)
            => string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

        public int StrToInt(string s)
            => int.TryParse(s, out int result) ? result : 0;

        public float StrToFloat(string s)
            => float.TryParse(s, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : 0.0f;

        public string FloatToStr(float f)
            => f.ToString();

        public string BoolToStr(bool b)
            => b ? "true" : "false";
        
        public string IntToStr(int i)
            => i.ToString();

        public string IntToStr(bool value)
            => value ? "1" : "0";

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

            ushort idx = (ushort)(address);
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
                ushort idx = (ushort)(startAddress + i);
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

        // New methods for working with holding registers (for boolean values stored as 0/1)
        public void SetRegisters(ushort startAddress, bool[] values)
        {
            if (slave?.DataStore?.HoldingRegisters == null || values == null) return;

            for (int i = 0; i < values.Length; i++)
            {
                ushort idx = (ushort)(startAddress + i);
                if (idx < slave.DataStore.HoldingRegisters.Count)
                {
                    slave.DataStore.HoldingRegisters[idx] = (ushort)(values[i] ? 1 : 0);
                }
                else
                {
                    Console.WriteLine($"Wrong register address: {(startAddress + i)}");
                    Logger.LogError($"Wrong register address: {(startAddress + i)}");
                }
            }
        }

        public void SetRegisters(ushort startAddress, ushort[] values)
        {
            if (slave?.DataStore?.HoldingRegisters == null || values == null) return;

            for (int i = 0; i < values.Length; i++)
            {
                ushort idx = (ushort)(startAddress + i);
                if (idx < slave.DataStore.HoldingRegisters.Count)
                {
                    slave.DataStore.HoldingRegisters[idx] = values[i];
                }
                else
                {
                    Console.WriteLine($"Wrong register address: {(startAddress + i)}");
                    Logger.LogError($"Wrong register address: {(startAddress + i)}");
                }
            }
        }

        public bool[] GetRegisters(ushort startAddress, ushort count)
        {
            if (slave?.DataStore?.HoldingRegisters == null) return null;

            bool[] result = new bool[count];

            for (int i = 0; i < count; i++)
            {
                ushort idx = (ushort)(startAddress + i);
                if (idx < slave.DataStore.HoldingRegisters.Count)
                {
                    result[i] = slave.DataStore.HoldingRegisters[idx] != 0;
                }
                else
                {
                    Console.WriteLine($"Wrong register address: {(startAddress + i)}");
                    Logger.LogError($"Wrong register address: {(startAddress + i)}");
                }
            }

            return result;
        }

        public ushort[] GetRegistersRaw(ushort startAddress, ushort count)
        {
            if (slave?.DataStore?.HoldingRegisters == null) return null;

            ushort[] result = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                ushort idx = (ushort)(startAddress + i);
                if (idx < slave.DataStore.HoldingRegisters.Count)
                {
                    result[i] = slave.DataStore.HoldingRegisters[idx];
                }
                else
                {
                    Console.WriteLine($"Wrong register address: {(startAddress + i)}");
                    Logger.LogError($"Wrong register address: {(startAddress + i)}");
                }
            }

            return result;
        }

        // Delegates to shared ModbusHelper
        public static void FloatToRegisters(float value, ushort[] registers, int offset)
            => ModbusHelper.FloatToRegisters(value, registers, offset);

        public static float RegistersToFloat(ushort[] registers, int offset)
            => ModbusHelper.RegistersToFloat(registers, offset);
    }
}
