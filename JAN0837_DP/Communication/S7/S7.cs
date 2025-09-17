using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using S7.Net;
using Siemens.Engineering.HW;

namespace JAN0837_DP.Communication.S7
{
    public class S7
    {
        public static Plc plc;

        public bool connectToPLC(string cpuType, string ip)
        {
            short rack = 0;
            short slot = 1;
            CpuType cpu;

            switch (cpuType)
            {
                case "1200":
                    cpu = CpuType.S71200;
                    break;
                case "1500":
                    cpu = CpuType.S71500;
                    break;
                default:
                    cpu = CpuType.S71200;
                    break;
            }

            plc.Close();

            plc = new Plc(cpu, ip, rack, slot);
            
            plc.Open();

            if (plc.IsConnected == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool disconnectToPLC()
        {
            plc.Close(); 

            if (plc.IsConnected == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool put(string address, object value)
        {
            try
            {
                plc.Write(address, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool putBytes(int DBnumber, int buffer_length, byte[] buffer)
        {
            try
            {
                plc.WriteBytes(DataType.DataBlock, DBnumber, buffer_length, buffer);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool get(string address, out object value)
        {
            try
            {
                value = plc.Read(address);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        public bool getBytes(int DBnumber, int startByte, int buffer_length, byte[] buffer)
        {
            var result = plc.ReadBytes(DataType.DataBlock, DBnumber, startByte, buffer_length);

            if (result == null || result.Length != buffer_length)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
