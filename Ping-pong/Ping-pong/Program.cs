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
            char[,] playingField = new char[30, 120];
            Console.CursorVisible = false;
            GamePerformer.GameShow();
            // Console.WriteLine("сервер?");
            //string m = Console.ReadLine();
            //if (m == "y")
            //{
            //    сonnection = new ServerСonnection();
            // }
            // else if (m == "n")
            // {
            //     сonnection = new ClientСonnection();
            // }
            // else
            //  {
            //     Process.Start(Assembly.GetExecutingAssembly().Location);
            //     Environment.Exit(0);
            //  }
            // сonnection.TransferGameData();
        }
    }

    static class GamePerformer
    {
        static char[,] playingField = new char[30, 120];
        public static void GameShow()
        {
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 120; j++)
                {
                    playingField[i, j] = ' ';
                }
            }

            playingField[15, 60] = '@';
            playingField[15, 61] = '@';
            playingField[15, 59] = '@';
            playingField[14, 60] = '@';
            playingField[16, 60] = '@';

            playingField[13, 0] = '#';
            playingField[14, 0] = '#';
            playingField[15, 0] = '#';
            playingField[16, 0] = '#';
            playingField[17, 0] = '#';
            playingField[18, 0] = '#';
            playingField[13, 1] = '#';
            playingField[14, 1] = '#';
            playingField[15, 1] = '#';
            playingField[16, 1] = '#';
            playingField[17, 1] = '#';
            playingField[18, 1] = '#';

            playingField[3, 118] = '#';
            playingField[4, 118] = '#';
            playingField[5, 118] = '#';
            playingField[6, 118] = '#';
            playingField[7, 118] = '#';
            playingField[8, 118] = '#';
            playingField[3, 119] = '#';
            playingField[4, 119] = '#';
            playingField[5, 119] = '#';
            playingField[6, 119] = '#';
            playingField[7, 119] = '#';
            playingField[8, 119] = '#';

            foreach (char c in playingField)
                Console.Write(c);
        }
    }

    class UserPlate
    { 

    }

    class OpponentPlate
    {

    }


    abstract class Сonnection
    {
        abstract public void TransferGameData();
    }

    class ServerСonnection : Сonnection
    {
        TcpListener serverSocket;
        TcpClient clientSocket;
        NetworkStream stream;
        public ServerСonnection()
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
    class ClientСonnection : Сonnection
    {
        TcpClient client;
        NetworkStream stream;
        public ClientСonnection()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 7000);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
                Console.WriteLine(e.Message);
            }
        }
            
    }
}



