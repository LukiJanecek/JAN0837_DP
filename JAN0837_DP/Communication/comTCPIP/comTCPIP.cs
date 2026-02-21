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
    // Alias for backwards compatibility
    using comTCPIP = comTCPIPClient;

    public class comTCPIPClient
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
            crossroadType = 1 << 0,
            Light1_Green = 1 << 1,
            Light1_Yellow = 1 << 2,
            Light1_Red = 1 << 3,
            Light2_Green = 1 << 4,
            Light2_Yellow = 1 << 5,
            Light2_Red = 1 << 6,
            Pedestrian1_Green = 1 << 7,
            Pedestrian1_Red = 2 << 0,
            Pedestrian2_Green = 2 << 1,
            Pedestrian2_Red = 2 << 2
        }

        [Flags]
        public enum LightFlagsByte1 : byte
        {
            None = 0,
            Pedestrian2_Green = 1 << 0,
            Pedestrian2_Red = 1 << 1
        }

        public comTCPIPClient(string ipAddress, int port) 
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

    public class comTCPIPServer
    {
        private TcpListener tcpListener;
        public Socket clientSocket;
        
        public comTCPIPServer(string ipAddress, int port)
        {
            // Bind to IPAddress.Any to listen on all interfaces
            // The ipAddress parameter is ignored for server mode - servers should listen on all interfaces
            tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public bool Start()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("TCP/IP Server started, waiting for connections...");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server start failed: {ex.Message}");
                Logger.LogException(ex, "Failed to start TCP/IP Server");
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                tcpListener?.Stop();
                Console.WriteLine("TCP/IP Server stopped.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server stop failed: {ex.Message}");
                Logger.LogException(ex, "Failed to stop TCP/IP Server");
                return false;
            }
        }

        public bool HasClientConnected()
        {
            try
            {
                if (tcpListener.Pending())
                {
                    clientSocket = tcpListener.AcceptSocket();
                    return true;
                }
                return clientSocket != null && clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error checking client connection");
                return false;
            }
        }

        public bool SendBytes(byte[] data)
        {
            try
            {
                if (clientSocket == null || !clientSocket.Connected)
                    return false;

                int sent = 0;
                while (sent < data.Length)
                {
                    sent += clientSocket.Send(data, sent, data.Length - sent, SocketFlags.None);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to send data to client");
                return false;
            }
        }

        public bool ReceiveExact(byte[] buffer)
        {
            try
            {
                if (clientSocket == null || !clientSocket.Connected)
                    return false;

                int read = 0;
                while (read < buffer.Length)
                {
                    int r = clientSocket.Receive(buffer, read, buffer.Length - read, SocketFlags.None);
                    if (r == 0)
                        return false;
                    read += r;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to receive data from client");
                return false;
            }
        }
    }
}

