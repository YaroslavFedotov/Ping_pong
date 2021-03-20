using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace Ping_pong
{
    public enum PlateDirection
    {
        up,
        no,
        down
    }

    public enum PlateSide
    {
        left,
        right
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

    public static class Extensions
    {
        public static bool IsOneOf<T>(this T self, params T[] elems)
        {
            return elems.Contains(self);
        }
    }

    class Program
    {
        public static bool IsServer;
        public static bool IsExit = true;
       
        static void Main(string[] args)
        {
            Console.Write("Создать сервер? yes/no ");
            string m = Console.ReadLine();
            if (m.IsOneOf("y", "yes"))
            {
                IsServer = true;
                ServerСonnection.ServerStart();

            }
            else if (m.IsOneOf("n", "no"))
            {
                IsServer = false;
                ClientСonnection.ClientStart();

            }
            else
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(0);
            }
            GamePerformer.InputControl();
            GamePerformer.GameStart();
            while (IsExit)
            {
                GamePerformer.GameShow();
            }
            Environment.Exit(0);
        }
    }

    static class GamePerformer
    {
        private static readonly Dictionary<string, PlateDirection> arrowDirection
            = new Dictionary<string, PlateDirection>()
            {
                { "UpArrow" , PlateDirection.up },
                { "DownArrow" , PlateDirection.down },
            };
        public const int ScreenWidth = 120;
        public const int ScreenHeight = 30;
        public static char[,] playingField = new char[ScreenHeight, ScreenWidth];
        private static PlateDirection currentPlateDirection = PlateDirection.no;
        private static PlateDirection opponentPlateBuffer;
        private static string textImage = String.Empty;
        public static ConsoleKeyInfo currentKey;
        private static Plate userPlate;
        private static Plate opponentPlate;
        private static Ball ball;
       
        public static void InputControl()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        currentKey = Console.ReadKey();
                        if (currentKey.Key.IsOneOf(ConsoleKey.UpArrow, ConsoleKey.DownArrow))
                        {
                            currentPlateDirection = arrowDirection[currentKey.Key.ToString()];
                        }
                        if (currentKey.Key == ConsoleKey.Escape)
                        {
                            Program.IsExit = false;
                        }
                    }
                    else
                    {
                        currentPlateDirection = PlateDirection.no;
                    }
                    Thread.Sleep(1);
                }
            }).Start();
        }

        public static void GameStart()
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            userPlate = new Plate();
            opponentPlate = new Plate();
            ball = new Ball();
        }

        public static void GameIterating()
        {
            if (Program.IsServer)
            {
                opponentPlateBuffer = (PlateDirection)ServerСonnection.TransferGameData((int)currentPlateDirection);
                opponentPlate.Move(playingField, PlateSide.right, opponentPlateBuffer);
                userPlate.Move(playingField, PlateSide.left, currentPlateDirection);
                ball.Move(playingField);
                ServerСonnection.TransferGameData(ball.GetBallData());
            }
            else
            {
                opponentPlateBuffer = (PlateDirection)ClientСonnection.TransferGameData((int)currentPlateDirection);
                opponentPlate.Move(playingField, PlateSide.left, opponentPlateBuffer);
                userPlate.Move(playingField, PlateSide.right, currentPlateDirection);
                ball.SetBallData(ClientСonnection.TransferGameData());
                ball.RenderingBall(playingField);
            }
        }

        public static void GameShow()
        {
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 120; j++)
                {
                    playingField[i, j] = ' ';
                }
            }
            GameIterating();
            textImage = String.Empty;
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 120; j++)
                {
                    if (j % 119 == 0)
                    {
                        textImage += playingField[i, j];
                    }
                    else
                    {
                        textImage += playingField[i, j];
                    }
                }
            }
            Console.Clear();
            Console.Write(textImage);
            Thread.Sleep(1);
        }
    }

    class Plate
    {
        private const int minimumPosition = 3;
        private const int maximumPosition = 26;
        private const int StartPosition = 15;
        private int position;

        public Plate()
        {
            position = StartPosition;
        }

        public char[,] Move(char[,] playingField, PlateSide plateSide, PlateDirection plateDirection)
        {
            switch (plateDirection)
            {
                case PlateDirection.up:
                    if (position > minimumPosition)
                    {
                        position -= 1;
                    }
                    break;
                case PlateDirection.down:
                    if (position < maximumPosition)
                    {
                        position += 1;
                    }
                    break;
            }
            return plateSide switch
            {
                PlateSide.left => RenderingLeftPlate(playingField),
                PlateSide.right => RenderingRightPlate(playingField),
                _ => playingField,
            };
        }

        public char[,] RenderingLeftPlate(char[,] playingField)
        {
            playingField[position - 3, 1] = '#';
            playingField[position - 3, 2] = '#';
            playingField[position - 2, 1] = '#';
            playingField[position - 2, 2] = '#';
            playingField[position - 1, 1] = '#';
            playingField[position - 1, 2] = '#';
            playingField[position, 1] = '#';
            playingField[position, 2] = '#';
            playingField[position + 1, 1] = '#';
            playingField[position + 1, 2] = '#';
            playingField[position + 2, 1] = '#';
            playingField[position + 2, 2] = '#';
            playingField[position + 3, 1] = '#';
            playingField[position + 3, 2] = '#';
            return playingField;
        }

        public char[,] RenderingRightPlate(char[,] playingField)
        {
            playingField[position - 3, 117] = '#';
            playingField[position - 3, 118] = '#';
            playingField[position - 2, 117] = '#';
            playingField[position - 2, 118] = '#';
            playingField[position - 1, 117] = '#';
            playingField[position - 1, 118] = '#';
            playingField[position, 117] = '#';
            playingField[position, 118] = '#';
            playingField[position + 1, 117] = '#';
            playingField[position + 1, 118] = '#';
            playingField[position + 2, 117] = '#';
            playingField[position + 2, 118] = '#';
            playingField[position + 3, 117] = '#';
            playingField[position + 3, 118] = '#';
            return playingField;
        }
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
        private BallDirection moveDirection;
        private int x;
        private int y;
        
        public Ball()
        {
            InitializationState();
        }

        public byte[] GetBallData()
        {
            return new byte[3] { (byte)x, (byte)y, (byte)moveDirection };
        }

        public void SetBallData(byte[] data)
        {
            x = data[0];
            y = data[1];
            moveDirection = (BallDirection)data[2];
        }

        private bool IsLeftMiddle()
        {
            if (GamePerformer.playingField[y + 3, x - 3] == '#'
                && GamePerformer.playingField[y - 3, x - 3] == '#')
            {
                return true;
            }
            return false;
        }

        private bool IsRightMiddle()
        {
            if (GamePerformer.playingField[y + 3, x + 3] == '#'
                && GamePerformer.playingField[y - 3, x + 3] == '#')
            {
                return true;
            }
            return false;
        }

        public char[,] Move(char[,] playingField)
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
            if (y.IsOneOf(1, 28))
            {
                moveDirection = wallRebound[moveDirection];
            }
            else if ('#'.IsOneOf(GamePerformer.playingField[y, x - 3], GamePerformer.playingField[y, x + 3]))
            {
                if (moveDirection == BallDirection.left)
                {
                    if (IsLeftMiddle())
                    {
                        moveDirection = BallDirection.right;
                    }
                    else if (GamePerformer.playingField[y + 3, x - 3] == ' ')
                    {
                        moveDirection = BallDirection.rightUp;
                    }
                    else if (GamePerformer.playingField[y - 3, x - 3] == ' ')
                    {
                        moveDirection = BallDirection.rightDown;
                    }
                }
                else if (moveDirection == BallDirection.right)
                {
                    if (IsRightMiddle())
                    {
                        moveDirection = BallDirection.left;
                    }
                    else if (GamePerformer.playingField[y + 3, x + 3] == ' ')
                    {
                        moveDirection = BallDirection.rightUp;
                    }
                    else if (GamePerformer.playingField[y - 3, x + 3] == ' ')
                    {
                        moveDirection = BallDirection.rightDown;
                    }
                }
                else
                {
                    if (moveDirection.IsOneOf(BallDirection.leftUp, BallDirection.leftDown) && IsLeftMiddle())
                    {
                        moveDirection = BallDirection.right;
                    }
                    else if (moveDirection.IsOneOf(BallDirection.rightUp, BallDirection.rightDown) && IsRightMiddle())
                    {
                        moveDirection = BallDirection.left;
                    }
                    else
                    {
                        moveDirection = plateRebound[moveDirection];
                    }
                }
            }
            else if (x - 3 == 0 || x + 3 == 119)
            {
                InitializationState();
            }
            return RenderingBall(playingField);
        }

        private void InitializationState()
        {
            x = 60;
            y = 15;
            moveDirection = (BallDirection)new Random().Next(0, 6);
        }

        public char[,] RenderingBall(char[,] playingField)
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

    static class ServerСonnection
    {
        static private int port;
        static byte[] data = new byte[1];
        static private TcpListener serverSocket;
        static private TcpClient clientSocket;
        static private NetworkStream stream;
        
        static public void ServerStart()
        {
            Console.Write("Введите port [7000] ");
            port = Convert.ToInt32(Console.ReadLine());
            serverSocket = new TcpListener(IPAddress.Any, port);
            Console.WriteLine("Server started");
            serverSocket.Start();
            clientSocket = serverSocket.AcceptTcpClient();
        }

        static public byte TransferGameData(int platePosition)
        {
            stream = clientSocket.GetStream();
            data[0] = Convert.ToByte(platePosition);
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Read(data, 0, data.Length);
            return data[0];
        }

        static public void TransferGameData(byte[] ballData)
        {
            try
            {
                stream.Write(ballData, 0, ballData.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    static class ClientСonnection 
    {
        static private string ip;
        static private int port;
        static private byte[] data = new byte[1];
        static private TcpClient client;
        static private NetworkStream stream;

        static public void ClientStart()
        {
            try
            {
                Console.Write("Введите ip [127.0.0.1] ");
                ip = Console.ReadLine();
                Console.Write("Введите port [7000] ");
                port = Convert.ToInt32(Console.ReadLine());
                client = new TcpClient(ip, port);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        static public byte TransferGameData(int platePosition)
        {
            try
            {
                data[0] = Convert.ToByte(platePosition);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Read(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return data[0];
        }

        static public byte[] TransferGameData()
        {
            byte[] ballData = new byte[3];
            stream.Read(ballData, 0, ballData.Length);
            return ballData;
        }
    }
}



