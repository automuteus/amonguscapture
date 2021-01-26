using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AUCapture_Console.IPC;
using Newtonsoft.Json;

namespace AUCapture_Console
{
    class Program
    {
        public static readonly ClientSocket socket = new ClientSocket();
        
        public static void OnTokenHandler(object sender, StartToken token)
        {
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColor()}{token.Host}{Color.White.ToTextColor()} with connect code {Color.Red.ToTextColor()}{token.ConnectCode}{Color.White.ToTextColor()}");
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
                GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
                GameMemReader.getInstance().GameOver += OnGameOver;
                GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
                var gameReader = Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                
                socketTask.Wait();
                IPCAdapter.getInstance().RegisterMinion();
                Settings.conInterface = new ConsoleLogger();
                if (uriStart == URIStartResult.PARSE) IPCAdapter.getInstance().SendToken(args[0]);
                gameReader.Wait();
            });
            foreverTask.Wait();
        }

        private static void OnGameOver(object? sender, GameOverEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, $"{JsonConvert.SerializeObject(e.PlayerInfos)}");
        }

        private static void OnJoinedLobby(object? sender, LobbyEventArgs e)
        {
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("Join", Color.DarkKhaki, $"Joined lobby: {e.LobbyCode}");
        }

        private static void OnChatMessageAdded(object? sender, ChatMessageEventArgs e)
        {
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki, $"{e.Sender}: {e.Message}");
        }

        private static void UserForm_PlayerChanged(object? sender, PlayerChangedEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, $"{e.Name}: {e.Action}");

        }

        private static void GameStateChangedHandler(object? sender, GameStateChangedEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
        }
    }
}
