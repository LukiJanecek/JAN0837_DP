using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics.Tracing;

namespace JAN0837_DP.Communication.comTCPIP
{
    public class comTCPIP
    {
        public Socket socket;
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

        public bool Disconnect()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    Console.WriteLine("Disconnected from PLC.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Socket is not connected.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnection failed: {ex.Message}");
                return false;
            }
        }

        public string ReadData()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = socket.Receive(buffer);
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return data;
            }
            catch (Exception ex)
            {
                // print message
                return null;
            }
        }

        public bool WriteData(string data)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                socket.Send(buffer);
                return true;
            }
            catch (Exception ex)
            {
                // print message 
                return false;
            }
        }


    }
}

