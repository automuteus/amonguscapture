using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using AmongUsCapture.ConsoleTypes;
using CaptureGUI;
using ControlzEx.Theming;

namespace AmongUsCapture.CaptureGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ClientSocket Socket { get; } = new ClientSocket();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = e.Args;

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            // needs to be the first call in the program to prevent weird bugs
            if (Settings.PersistentSettings.DebugConsole)
                AllocConsole();

            var uriStart = IPCadapter.getInstance().HandleURIStart(args);

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

            // Run socket in background. Important to wait for init to have actually finished before continuing
            var socketTask = Task.Factory.StartNew(() => Socket.Init()); 
            // Init GameMemReader - Run loop in background
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); 

            socketTask.Wait();

            IPCadapter.getInstance().RegisterMinion();

            var mainWindow = new MainWindow();

            Settings.Form = mainWindow;
            Settings.ConInterface = new FormConsole(mainWindow);

            MainWindow = mainWindow;
            MainWindow.Loaded += (sender, args2) =>
            {
                if (uriStart == URIStartResult.PARSE)
                    IPCadapter.getInstance().SendToken(args[0]);
            };
            MainWindow.Closing += (sender, args2) =>
            {
                Environment.Exit(0);
            };
            MainWindow.Show();

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}
