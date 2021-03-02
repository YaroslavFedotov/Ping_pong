using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;

namespace Ping_pong
{
    class Program
    {
        public const int ScreenWidth = 120;
        public const int ScreenHeight = 30;
        static void Main(string[] args)
        {
            Сonnection сonnection = null;
           
            char[,] playingField = new char[ScreenHeight, ScreenWidth];
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
        public static char[,] playingField = new char[30, 120];
        public static void GameShow()
        {
            Plate userPlate = new Plate();
            Plate opponentPlate = new Plate();
            Ball ball = new Ball();

            Console.SetBufferSize(Program.ScreenWidth, Program.ScreenHeight);
            while (true)
            {
                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 120; j++)
                    {
                        playingField[i, j] = ' ';
                    }
                }
                userPlate.RenderingLeftPlate(playingField, 15);
                opponentPlate.RenderingRightPlate(playingField, 15);
                ball.move(playingField);
                


                string temp = String.Empty;

                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 120; j++)
                    {
                        if (j % 119 == 0)
                        {
                            temp += playingField[i, j];
                        }
                        else
                        {
                            temp += playingField[i, j];
                        }
                    }
                }
                Console.Clear();
                Console.Write(temp);
                System.Threading.Thread.Sleep(3);
                
                
               
            }
        }
    }
    
    class Plate
    {
        private int position;
        private readonly int StartPosition = 15;
        public Plate()
        {
            position = StartPosition;
        }
        public char[,] RenderingLeftPlate(char[,] playingField)
        {
            playingField[position - 3, 0] = '#';
            playingField[position - 3, 1] = '#';
            playingField[position - 2, 0] = '#';
            playingField[position - 2, 1] = '#';
            playingField[position - 1, 0] = '#';
            playingField[position - 1, 1] = '#';
            playingField[position, 0] = '#';
            playingField[position, 1] = '#';
            playingField[position + 1, 0] = '#';
            playingField[position + 1, 1] = '#';
            playingField[position + 2, 0] = '#';
            playingField[position + 2, 1] = '#';
            playingField[position + 3, 0] = '#';
            playingField[position + 3, 1] = '#';
            return playingField;
        }
        public char[,] RenderingRightPlate(char[,] playingField, int position)
        {
            playingField[position - 3, 118] = '#';
            playingField[position - 3, 119] = '#';
            playingField[position - 2, 118] = '#';
            playingField[position - 2, 119] = '#';
            playingField[position - 1, 118] = '#';
            playingField[position - 1, 119] = '#';
            playingField[position, 118] = '#';
            playingField[position, 119] = '#';
            playingField[position + 1, 118] = '#';
            playingField[position + 1, 119] = '#';
            playingField[position + 2, 118] = '#';
            playingField[position + 2, 119] = '#';
            playingField[position + 3, 118] = '#';
            playingField[position + 3, 119] = '#';
            return playingField;
        }
    }
    public enum BallDirection
    {
        leftUp,
        left,
        leftDown,
        rightUp,
        right,
        rightDown
    }
    class Ball
    {
        private static readonly Dictionary<BallDirection, BallDirection> wallRebound
            = new Dictionary<BallDirection, BallDirection>()
            {
                { BallDirection.leftUp , BallDirection.leftDown },
                { BallDirection.leftDown , BallDirection.leftUp },
                { BallDirection.rightUp , BallDirection.rightDown },
                { BallDirection.rightDown , BallDirection.rightUp },
            };

        private static readonly Dictionary<BallDirection, BallDirection> plateRebound
            = new Dictionary<BallDirection, BallDirection>()
            {
                { BallDirection.leftUp , BallDirection.rightUp },
                { BallDirection.left , BallDirection.right },
                { BallDirection.leftDown , BallDirection.rightDown },
                { BallDirection.rightUp , BallDirection.leftUp },
                { BallDirection.right , BallDirection.left },
                { BallDirection.rightDown , BallDirection.leftDown },
            };
        private int x;
        private int y;
        private BallDirection moveDirection;
        public Ball()
        {
            InitializationState();
        }
        public char[,] move(char[,] playingField)
        {
            switch (moveDirection)
            {
                case BallDirection.leftUp:
                    x -= 1;
                    y += 1;
                    break;
                case BallDirection.left:
                    x -= 1;
                    break;
                case BallDirection.leftDown:
                    x -= 1;
                    y -= 1;
                    break;
                case BallDirection.rightUp:
                    x += 1;
                    y += 1;
                    break;
                case BallDirection.right:
                    x += 1;
                    break;
                case BallDirection.rightDown:
                    x += 1;
                    y -= 1;
                    break;
            }
            if (y == 1 || y == 28)
            {
                moveDirection = wallRebound[moveDirection];
            }
            if (GamePerformer.playingField[y, x - 3] == '#' || GamePerformer.playingField[y, x + 3] == '#')
            {
                moveDirection = plateRebound[moveDirection];
            }
            if (x - 3 == 0 || x + 3 == 119)
            {
                InitializationState();
            }
                return RenderingBall(playingField);
        }
        private void InitializationState()
        {
            x = 60;
            y = 15;
            moveDirection = (BallDirection)new Random()
                .Next(Enum.GetNames(typeof(BallDirection)).Length);
        }
        private char[,] RenderingBall(char[,] playingField)
        {
            playingField[y, x - 2] = '@';
            playingField[y + 1, x - 1] = '@';
            playingField[y, x - 1] = '@';
            playingField[y - 1, x - 1] = '@';
            playingField[y + 1, x] = '@';
            playingField[y, x] = '@';
            playingField[y - 1, x] = '@';
            playingField[y + 1, x + 1] = '@';
            playingField[y, x + 1] = '@';
            playingField[y - 1, x + 1] = '@';
            playingField[y, x + 2] = '@';
            return playingField;
        }
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



