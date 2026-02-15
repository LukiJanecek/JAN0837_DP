using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics.Tracing;
using JAN0837_DP.Log;

namespace JAN0837_DP.Communication.comTCPIP
{
    public class comTCPIP
    {
        public Socket socket;
        private IPEndPoint endPoint;

        [Flags]
        public enum ButtonFlags : byte
        {
            None = 0,
            BtnCrossroadStart = 1 << 0,
            BtnCrossroadPause = 1 << 1,
            BtnCrossroadStop = 1 << 2,
            BtnCrosswalk1 = 1 << 3,
            BtnCrosswalk2 = 1 << 4
        }

        [Flags]
        public enum LightFlagsByte0 : byte
        {
            None = 0,
            Light1_Green = 1 << 0,
            Light1_Yellow = 1 << 1,
            Light1_Red = 1 << 2,
            Light2_Green = 1 << 3,
            Light2_Yellow = 1 << 4,
            Light2_Red = 1 << 5,
            Pedestrian1_Green = 1 << 6,
            Pedestrian1_Red = 1 << 7,
            Pedestrian2_Green = 2 << 0,
            Pedestrian2_Red = 2 << 1
        }

        [Flags]
        public enum LightFlagsByte1 : byte
        {
            None = 0,
            Pedestrian2_Green = 1 << 0,
            Pedestrian2_Red = 1 << 1
        }

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
                Logger.LogException(ex, "Failed to connect to PLC");
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
                Logger.LogException(ex, "Failed to disconnect from PLC");
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
                Logger.LogException(ex, "Failed to read data from PLC");
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
                Logger.LogException(ex, "Failed to write data to PLC");
                return false;
            }
        }

        public bool ReceiveExact(byte[] buffer)
        {
            try
            {
                int read = 0;
                while (read < buffer.Length)
                {
                    int r = socket.Receive(buffer, read, buffer.Length - read, SocketFlags.None);
                    if (r == 0)
                    {
                        return false;
                    }
                    read += r;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive failed: {ex.Message}");
                Logger.LogException(ex, "Failed to receive data from PLC");
                return false;
            }
        }

        public bool SendBytes(byte[] data)
        {
            try
            {
                int sent = 0;
                while (sent < data.Length)
                {
                    sent += socket.Send(data, sent, data.Length - sent, SocketFlags.None);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send failed: {ex.Message}");
                Logger.LogException(ex, "Failed to send data to PLC");
                return false;
            }
        }
    }
}

