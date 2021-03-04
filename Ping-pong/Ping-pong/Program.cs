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
using System.Threading.Tasks;
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
    public enum Signal : byte
    {
        Start,
        Process,
        End,
    }

    class Program
    {
        public const int ScreenWidth = 120;
        public const int ScreenHeight = 30;
        public static Сonnection сonnection = null;
        static void Main(string[] args)
        {
            char[,] playingField = new char[ScreenHeight, ScreenWidth];
            Console.CursorVisible = false;

            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);


            Console.Write("Создать сервер? yes/no [yes]");
            string m = Console.ReadLine();
            if (m == "y" || m == "yes")
            {
                сonnection = new ServerСonnection();
            }
            else if (m == "n" || m == "no")
            {
                сonnection = new ClientСonnection();
            }
            else
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(0);
            }
            GamePerformer.PlateControl();
            GamePerformer.GameStart();
            
            while (true)
            {
                //Console.WriteLine(сonnection.TransferGameData(Signal.Process, 12));
                GamePerformer.GameShow();
            }
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
        public static char[,] playingField = new char[30, 120];
        private static PlateDirection currentPlateDirection = PlateDirection.no;
        private static string c = String.Empty;
        private static Plate userPlate;
        private static Plate opponentPlate;
        private static Ball ball;
        private static PlateDirection opponentPlateBuffer;

        public static void PlateControl()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        c = Console.ReadKey().Key.ToString();
                        if (c == "UpArrow" || c == "DownArrow")
                        {
                            currentPlateDirection = arrowDirection[c];
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
            userPlate = new Plate();
            opponentPlate = new Plate();
            ball = new Ball();
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



            opponentPlateBuffer = (PlateDirection)Program.сonnection.TransferGameData(Signal.Process, (int)currentPlateDirection);
            userPlate.Move(playingField, PlateSide.left, currentPlateDirection);
            opponentPlate.Move(playingField, PlateSide.right, opponentPlateBuffer);
            ball.Move(playingField);


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
            Thread.Sleep(1);

        }
    }
    class Plate
    {
        private readonly int minimumPosition = 3;
        private readonly int maximumPosition = 26;
        private int position;
        private readonly int StartPosition = 15;
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
        private int x;
        private int y;
        private int startingDirection;
        private BallDirection moveDirection;
        public Ball()
        {
            InitializationState();
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
            if (y == 1 || y == 28)
            {
                moveDirection = wallRebound[moveDirection];
            }
            else if (GamePerformer.playingField[y, x - 3] == '#' || GamePerformer.playingField[y, x + 3] == '#')
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
                    if ((moveDirection == BallDirection.leftUp
                        || moveDirection == BallDirection.leftDown)
                        && IsLeftMiddle())
                    {
                        moveDirection = BallDirection.right;
                    }
                    else if ((moveDirection == BallDirection.rightUp
                        || moveDirection == BallDirection.rightDown)
                        && IsRightMiddle())
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
            startingDirection = new Random().Next(0, 3);
            // moveDirection = (BallDirection)new Random()
            //    .Next(Enum.GetNames(typeof(BallDirection)).Length);
            moveDirection = (BallDirection)(startingDirection + Program.сonnection.TransferGameData(Signal.Start, startingDirection));
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
        abstract public byte TransferGameData(Signal signal, int platePosition);
    }
    class ServerСonnection : Сonnection
    {
        private int port;
        TcpListener serverSocket;
        TcpClient clientSocket;
        NetworkStream stream;
        byte[] data = new byte[2];
        public ServerСonnection()
        {
            Console.Write("Введите port [7000]");
            port = Convert.ToInt32(Console.ReadLine());
            serverSocket = new TcpListener(IPAddress.Any, port);
            Console.WriteLine("Server started");
            serverSocket.Start();
            clientSocket = serverSocket.AcceptTcpClient();

        }
        public override byte TransferGameData(Signal signal, int platePosition)
        {
            stream = clientSocket.GetStream();
            data[0] = Convert.ToByte(signal);
            data[1] = Convert.ToByte(platePosition);
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Read(data, 0, data.Length);
            return data[1];

        }
    }
    class ClientСonnection : Сonnection
    {
        private string ip;
        private int port;
        TcpClient client;
        NetworkStream stream;
        private byte[] data = new byte[2];

        public ClientСonnection()
        {
            try
            {
                Console.Write("Введите ip [127.0.0.1]");
                ip = Console.ReadLine();
                Console.Write("Введите port [7000]");
                port = Convert.ToInt32(Console.ReadLine());
                client = new TcpClient(ip, port);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public override byte TransferGameData(Signal signal, int platePosition)
        {
            try
            {

                data[0] = Convert.ToByte(signal);
                data[1] = Convert.ToByte(platePosition);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Read(data, 0, data.Length);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return data[1];
        }
            
    }
}



