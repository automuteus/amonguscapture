using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using AmongUsCapture;
using AUCapture_Console.IPC;
using Newtonsoft.Json;


namespace AUCapture_Console
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly ClientSocket socket = new ClientSocket();
        
        public static void OnTokenHandler(object sender, StartToken token) {
            Logger.Info("Attempting to connect to host {host} with connect code {connectCode}", token.Host, token.ConnectCode);
            socket.Connect(token.Host, token.ConnectCode);
        }
        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Process.GetCurrentProcess().MainModule.ModuleName);
            var uriStart = IPCAdapter.getInstance().HandleURIStart(args);
            
            switch (uriStart)
            {
                case URIStartResult.CLOSE:
                    Environment.Exit(0);
                    break;
                case URIStartResult.PARSE:
                    Console.WriteLine($"Starting with args : {args[0]}");
                    break;
                case URIStartResult.CONTINUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var foreverTask = Task.Factory.StartNew(() =>
            {
                IPCAdapter.getInstance().OnToken += OnTokenHandler;
                var socketTask = Task.Factory.StartNew(() => socket.Init()); // run socket in background. Important to wait for init to have actually finished before continuing
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
                GameMemReader.getInstance().GameOver += OnGameOver;
                GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
                var gameReader = Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                
                socketTask.Wait();
                IPCAdapter.getInstance().RegisterMinion();
                if (uriStart == URIStartResult.PARSE) IPCAdapter.getInstance().SendToken(args[0]);
                gameReader.Wait();
            });
            foreverTask.Wait();
        }

        private static void OnGameOver(object? sender, GameOverEventArgs e)
        {
            Logger.Debug("PlayerChange: {e}", e);
        }

        private static void OnJoinedLobby(object? sender, LobbyEventArgs e)
        {
            Logger.Debug("Joined lobby: {lobbyCode}", e.LobbyCode);
        }
        

        private static void UserForm_PlayerChanged(object? sender, PlayerChangedEventArgs e)
        {
            Logger.Debug("PlayerChange: {e}", e);
        }

        private static void GameStateChangedHandler(object? sender, GameStateChangedEventArgs e)
        {
            Logger.Debug("GameState Change: {e}", e);
        }
    }
}
