using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AmongUsCapture;
using AUCapture_WPF.IPC;
using Config.Net;
using ControlzEx.Theming;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;
using WpfScreenHelper;
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
        private IAppSettings config;
        public static string LogFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture", "logs");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SetupLoggingConfig() {
            var LoggingConfig = new NLog.Config.LoggingConfiguration();
            FileVersionInfo v = FileVersionInfo.GetVersionInfo(App.GetExecutablePath());
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/latest.log",
                ArchiveFileName= "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/{#}.log",
                ArchiveNumbering= ArchiveNumberingMode.Date,
                Layout = "${time:universalTime=True}|${level:uppercase=true}|${logger}|${message}",
                MaxArchiveFiles = 100,
                ArchiveOldFileOnStartup = true,
                ArchiveDateFormat= "yyyy-MM-dd HH_mm_ss",
                Header = $"Capture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}\n",
                Footer = $"\nCapture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}"
            };
            LoggingConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = LoggingConfig;
        }
        public void OnTokenHandler(object sender, StartToken token)
        {
            Logger.Info("Attempting to connect to host: {host} with connect code: {connectCode}", token.Host, token.ConnectCode);
            socket.Connect(token.Host, token.ConnectCode);
        }

        public void PlaySound(string URL)
        {
            try
            {
                var req = WebRequest.Create(URL);
                using Stream stream = req.GetResponse().GetResponseStream();
                var myNewSound = new SoundPlayer(stream);
                myNewSound.Load();
                myNewSound.Play();
            }
            catch (Exception errrr)
            {
                Console.WriteLine("Minor error");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupLoggingConfig();
            var args = e.Args;

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
            Console.WriteLine(string.Join(", ",Assembly.GetExecutingAssembly().GetManifestResourceNames())); //Gets all the embedded resources
            
            try {
                config = new ConfigurationBuilder<IAppSettings>()
                    .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            }
            catch (JsonReaderException) //Delete file and recreate config
            {
                Console.WriteLine("Bad config. Clearing.");
                File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI", "Settings.json"));
                config = new ConfigurationBuilder<IAppSettings>()
                    .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            }
            
            var r = new Random();
            var rValue = r.Next(101);
            var goingToPop = rValue == 1;
            var goingToDouche = rValue == 2;
            var goingToMonke = rValue == 3;
            var Valentines = DateTime.UtcNow >= new DateTime(2021, 2, 7) && DateTime.UtcNow <= new DateTime(2021, 2, 20);  
            if (!config.startupMemes||(!goingToPop && !goingToDouche && !goingToMonke)||Valentines)
            {
                if (DateTime.Now.Month == 12)
                {
                    new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenChristmas.png").Show(true);
                }
                else if (Valentines)
                {
                    new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenLovely.png").Show(true);
                }
                else
                {
                    new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenNormal.png").Show(true);
                }
                //Console.WriteLine(string.Join(", ",Assembly.GetExecutingAssembly().GetManifestResourceNames())); //Gets all the embedded resources
            }
            else if(goingToPop)
            {
                new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenPop.png").Show(true);
                PlaySound("https://cdn.automute.us/Eggs/popcat.wav");
            }
            else if(goingToDouche)
            {
                new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenDouche.png").Show(true);
                PlaySound("https://cdn.automute.us/Eggs/douchebag.wav");
            }
            else
            {
                new SplashScreen(Assembly.GetExecutingAssembly(), "SplashScreens\\SplashScreenMonke.png").Show(true);
                PlaySound("https://cdn.automute.us/Eggs/stinky.wav");
            }
            
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
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
