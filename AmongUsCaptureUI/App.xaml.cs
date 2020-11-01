using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AmongUsCaptureUI.IPC;
using ControlzEx.Theming;
using IpcAdapter = AmongUsCaptureUI.IPC.IpcAdapter;
using URIStartResult = AmongUsCaptureUI.IPC.URIStartResult;

namespace AmongUsCaptureUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ClientSocket Socket { get; set; } = new ClientSocket();
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Init(e.Args);
        }

        protected void Init(string[] args)
        {
            // Init Theming
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();

            // needs to be the first call in the program to prevent weird bugs
            if (Settings.PersistentSettings.DebugConsole)
                AllocConsole();

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
                    throw new ArgumentException("Unknown URIStartResult");
            }


            //initialize the main window, set it as the application main window
            //and close the splash screen
            var mainWindow = new MainWindow();

            Settings.ConInterface = new WpfLogger(mainWindow);

            IpcAdapter.getInstance().OnToken += OnTokenHandler;

            // Run socket in background. Important to wait for init to have actually finished before continuing
            var socketTask = Task.Factory.StartNew(() => Socket.Init());
            // Init GameMemReader - Run loop in background
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop());
            socketTask.Wait();

            IpcAdapter.getInstance().RegisterMinion();

            // Init main window
            
            this.MainWindow = mainWindow;

            mainWindow.Loaded += (sender, args2) =>
            {   
                if (uriStart == URIStartResult.PARSE) 
                    IpcAdapter.getInstance().SendToken(args[0]);
            };

            mainWindow.Closing += (sender, args2) =>
            {
                Environment.Exit(0);
            };

            mainWindow.Show();
        }

        protected void OnTokenHandler(object sender, StartTokenEventArgs token)
        {
            Settings.ConInterface.WriteModuleTextColored(
                "ClientSocket",
                Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColor()}{token.Host}{Color.White.ToTextColor()} " +
                $"with connect code {Color.Red.ToTextColor()}{token.ConnectCode}{Color.White.ToTextColor()}");
            Socket.Connect(token.Host, token.ConnectCode);
        }

        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}
