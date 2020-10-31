using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AmongUsCaptureConsole.IPC;

namespace AmongUsCaptureConsole
{
    class Program
    {
        public static readonly ClientSocket socket = new ClientSocket();
        
        public static void OnTokenHandler(object sender, StartTokenEventArgs token)
        {
            Settings.ConInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColor()}{token.Host}{Color.White.ToTextColor()} with connect code {Color.Red.ToTextColor()}{token.ConnectCode}{Color.White.ToTextColor()}");
            socket.Connect(token.Host, token.ConnectCode);
        }
        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        static void Main(string[] args)
        {

            var uriStart = IpcAdapter.getInstance().HandleURIStart(args);
            
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
                IpcAdapter.getInstance().OnToken += OnTokenHandler;
                var socketTask = Task.Factory.StartNew(() => socket.Init()); // run socket in background. Important to wait for init to have actually finished before continuing
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
                GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
                GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
                var gameReader = Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                
                socketTask.Wait();
                IpcAdapter.getInstance().RegisterMinion();
                Settings.ConInterface = new ConsoleLogger();
                if (uriStart == URIStartResult.PARSE) 
                    IpcAdapter.getInstance().SendToken(args[0]);

                gameReader.Wait();
            });
            foreverTask.Wait();
        }

        private static void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            Settings.ConInterface.WriteModuleTextColored("Join", Color.DarkKhaki, $"Joined lobby: {e.LobbyCode}");
        }

        private static void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            Settings.ConInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki, $"{e.Sender}: {e.Message}");
        }

        private static void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            Settings.ConInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, $"{e.Name}: {e.Action}");
        }

        private static void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            Settings.ConInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
        }
    }
}
