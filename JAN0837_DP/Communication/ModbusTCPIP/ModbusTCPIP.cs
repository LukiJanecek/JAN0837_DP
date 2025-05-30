using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace JAN0837_DP.Communication.ModbusTCPIP
{
    public class ModbusTCPIPimMaster
    {
        private string ipAddress;
        private int port;
        private TcpClient tcpClient;
        private ModbusIpMaster master;

        // Konstruktor
        public ModbusTCPIPimMaster(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public bool ConnectToSlave()
        {
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                master = ModbusIpMaster.CreateIp(tcpClient);
                Console.WriteLine("✅ Připojeno k Modbus serveru.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba připojení: {ex.Message}");
                return false;
            }
        }

        public void DisconnectFromSlave()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
                master = null;
                Console.WriteLine("🔌 Odpojeno od Modbus serveru.");
            }
        }

        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                return master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba čtení: {ex.Message}");
                return null;
            }
        }

        public ushort[] ReadInputRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                return master.ReadInputRegisters(slaveId, startAddress, numRegisters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba čtení: {ex.Message}");
                return null;
            }
        }

        public bool[] ReadCoils(byte slaveId, ushort startAddress, ushort numCoils)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                return master.ReadCoils(slaveId, startAddress, numCoils);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba čtení: {ex.Message}");
                return null;
            }
        }

        public void WriteSingleRegister(byte slaveId, ushort address, ushort value)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                master.WriteSingleRegister(slaveId, address, value);
                Console.WriteLine($"✅ Zapsána hodnota {value} na adresu {address}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba zápisu: {ex.Message}");
            }
        }

        public void WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] values)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                master.WriteMultipleRegisters(slaveId, startAddress, values);
                Console.WriteLine($"✅ Zapsáno {values.Length} registrů od adresy {startAddress}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba zápisu: {ex.Message}");
            }
        }

        public void WriteSingleCoil(byte slaveId, ushort address, bool value)
        {
            try
            {
                if (master == null) throw new Exception("Modbus není připojen!");
                master.WriteSingleCoil(slaveId, address, value);
                Console.WriteLine($"✅ Zapsán Coil {value} na adresu {address}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba zápisu: {ex.Message}");
            }
        }

    }

    public class ModbusTCPIPimSlave
    {
        private TcpListener tcpListener;
        private ModbusSlave slave;
        private ushort[] holdingRegisters = new ushort[10]; // Registr pro uchování dat

        // Konstruktor 
        public ModbusTCPIPimSlave(string ipAddress, int port)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            tcpListener = new TcpListener(address, port);
        }
        public void Start()
        {
            try
            {
                tcpListener.Start();
                slave = ModbusTcpSlave.CreateTcp(1, tcpListener); // Slave ID = 1
                slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();

                Console.WriteLine("✅ Modbus TCP Slave spuštěn.");
                slave.Listen(); // Spustí poslouchání požadavků od Masteru
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chyba při spuštění serveru: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (slave != null)
            {
                slave.Dispose();
                tcpListener.Stop();
                Console.WriteLine("🔌 Modbus TCP Slave zastaven.");
            }
        }

        public void SetRegisterValue(ushort address, ushort value)
        {
            if (address < holdingRegisters.Length)
            {
                holdingRegisters[address] = value;
                Console.WriteLine($"✅ Nastaven registr [{address}] = {value}");
            }
            else
            {
                Console.WriteLine($"❌ Neplatná adresa registru: {address}");
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
                Console.WriteLine($"❌ Neplatná adresa registru: {address}");
                return 0;
            }
        }
    }
}
