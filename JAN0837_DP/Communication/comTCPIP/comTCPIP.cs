using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace JAN0837_DP.Communication.comTCPIP
{
    public class comTCPIP
    {
        private Socket socket;
        private IPEndPoint endPoint;

        public comTCPIP(string ipAddress, int port) 
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect()
        {
            try
            {
                socket.Connect(endPoint);
                Console.WriteLine("Connected to PLC.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Disconnected from PLC.");
            }
        }

        public string ReadData()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = socket.Receive(buffer);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public void WriteData(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            socket.Send(buffer);
        }
    }
}

