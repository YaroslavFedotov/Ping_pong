using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ping_pong
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 7000);
            Console.WriteLine("Server started");
            serverSocket.Start();
            TcpClient clientSocket = serverSocket.AcceptTcpClient();

            NetworkStream stream = clientSocket.GetStream();
            byte[] data = { 12, 13, 14, 15, 16, 44, 55, 66 };

            stream.Write(data, 0, data.Length);
            stream.Flush();

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("Server stopped");
        }
    }
}

