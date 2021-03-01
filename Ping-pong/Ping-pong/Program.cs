using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Ping_pong
{
    class Program
    {
        static void Main(string[] args)
        {
            Сonnection сonnection = null;
            Console.WriteLine("сервер?");
            string m = Console.ReadLine();
            if (m == "y")
            {
                сonnection = new ServerСonnectionBehavior();
            }
            else if (m == "n")
            {
                сonnection = new ClientСonnectionBehavior();
            }
            else
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(0);
            }
            сonnection.TransferGameData();
        }
    }
    abstract class Сonnection
    {
        abstract public void TransferGameData();
    }

    class ServerСonnectionBehavior : Сonnection
    {
        TcpListener serverSocket;
        TcpClient clientSocket;
        NetworkStream stream;
        public ServerСonnectionBehavior()
        {
            serverSocket = new TcpListener(IPAddress.Any, 7000);
            Console.WriteLine("Server started");
            serverSocket.Start();
            clientSocket = serverSocket.AcceptTcpClient();
           // clientSocket.Close();
            //serverSocket.Stop();
            Console.WriteLine("Server stopped");
        }
        public override void TransferGameData()
        {
            stream = clientSocket.GetStream();
            byte[] data = { 12, 13, 14, 15, 16, 44, 55, 66 };
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }
    class ClientСonnectionBehavior : Сonnection
    {
        TcpClient client;
        NetworkStream stream;
        public ClientСonnectionBehavior()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 7000);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "1");
            }
        }
        public override void TransferGameData()
        {
            try
            {
                byte[] data = new byte[256];
                stream.Read(data, 0, data.Length);
                foreach (byte b in data)
                    Console.Write(b + " ");
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "2");
            }
        }
            
    }
}



