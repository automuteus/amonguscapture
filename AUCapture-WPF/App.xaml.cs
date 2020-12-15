using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AUCapture_WPF.IPC;
using ControlzEx.Theming;
using IPCAdapter = AUCapture_WPF.IPC.IPCAdapter;
using URIStartResult = AUCapture_WPF.IPC.URIStartResult;

namespace AUCapture_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly ClientSocket socket = new ClientSocket();
        public static readonly DiscordHandler handler = new DiscordHandler();
        
        public void OnTokenHandler(object sender, StartToken token)
        {
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColor()}{token.Host}{Color.White.ToTextColor()} with connect code {Color.Red.ToTextColor()}{token.ConnectCode}{Color.White.ToTextColor()}");
            socket.Connect(token.Host, token.ConnectCode);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = e.Args;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
             // needs to be the first call in the program to prevent weird bugs
             if (Settings.PersistentSettings.debugConsole)
                 AllocConsole();

            var uriStart = IPCAdapter.getInstance().HandleURIStart(e.Args);
            
            switch (uriStart)
            {
                case URIStartResult.CLOSE:
                    Environment.Exit(0);
                    break;
                case URIStartResult.PARSE:
                    Console.WriteLine($"Starting with args : {e.Args[0]}");
                    break;
                case URIStartResult.CONTINUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.Write(string.Join(", ",System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            if (DateTime.Now.Month == 12)
            {
                new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenChristmas.png").Show(true);
            }
            else
            {
                new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenNormal.png").Show(true);
            }

            
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            Settings.conInterface = new WPFLogger(mainWindow);
            IPCAdapter.getInstance().OnToken += OnTokenHandler;
            socket.Init();
            IPCAdapter.getInstance().RegisterMinion();
            mainWindow.Loaded += (sender, args2) =>
            {
                Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                if (uriStart == URIStartResult.PARSE) IPCAdapter.getInstance().SendToken(args[0]);
            };
            mainWindow.Closing += (sender, args2) =>
            {
                Environment.Exit(0);
            };
            mainWindow.Show();

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
