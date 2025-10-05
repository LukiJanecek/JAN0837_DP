using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAN0837_DP.Data;
using Sharp7;

namespace JAN0837_DP.Communication.comSharp7
{
    public class comSharp7
    {
        public S7Client client = new S7Client();

        public bool connectToPLC(string ip)
        {
            //0 -> MPI -> Multi Point Interface -> didnt work  
            //1 -> PPI -> Point to Point interface
            //2 -> OP -> Engineering point
            //3 -> S7 Basic -> S7 communication using Ethernet or Profibus
            //10 -> ISOTCP -> TCP/IP protocol -> Ethernet -> didnt work
            client.SetConnectionType(1);

            int rack = 0;
            int slot = 1;

            int plcConnect = client.ConnectTo(ip, rack, slot);

            if (plcConnect == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool disconnectFromPLC()
        {
            int plcDisconnect = client.Disconnect();

            if (plcDisconnect == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool readS7MultiVar(int DBnumber, byte[] buffer, int startBit = 0)
        {
            S7MultiVar reader = new S7MultiVar(client);

            reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, DBnumber, startBit, buffer.Length, ref buffer);

            int result = reader.Read();

            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool writeS7MultiVar(int DBnumber, byte[] buffer, int startBit = 0)
        {
            S7MultiVar writer = new S7MultiVar(client);

            writer.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, DBnumber, startBit, buffer.Length, ref buffer);

            int result = writer.Write();

            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool readDB(int DBnumber, byte[] buffer, int startBit = 0)
        {
            int result = client.DBRead(DBnumber, startBit, buffer.Length, buffer);

            if (result == 0)
            {       
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool writeDB(int DBnumber, byte[] buffer, int startBit = 0)
        {
            int result = client.DBWrite(DBnumber, startBit, buffer.Length, buffer);

            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
