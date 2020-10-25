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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = e.Args;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            // needs to be the first call in the program to prevent weird bugs
            if (Settings.PersistentSettings.DebugConsole)
                AllocConsole();

            var uriStart = IPCadapter.getInstance().HandleURIStart(e.Args);

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

            var splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            Task.Factory.StartNew(() =>
            {
                //since we're not on the UI thread
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    var mainWindow = new MainWindow();
                    this.MainWindow = mainWindow;
                    Settings.Form = mainWindow;
                    Settings.ConInterface = new FormConsole(mainWindow);
                    Program.Main();
                    mainWindow.Loaded += (sender, args2) =>
                    {
                        if (uriStart == URIStartResult.PARSE) IPCadapter.getInstance().SendToken(args[0]);
                    };
                    mainWindow.Closing += (sender, args2) =>
                    {
                        Environment.Exit(0);
                    };
                    mainWindow.Show();
                    splashScreen.Close();
                });
            });

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}
