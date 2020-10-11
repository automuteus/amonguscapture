using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using AmongUsCapture.CaptureGUI;
using AmongUsCapture.ConsoleTypes;
using CaptureGUI;
using Microsoft.Win32;

namespace AmongUsCapture
{
    internal static class Program
    {
        public static MainWindow window;
        private const string UriScheme = "aucapture";
        private const string FriendlyName = "AmongUs Capture";
        private static Mutex mutex = null;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (Settings.PersistentSettings.debugConsole)
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs

            var uriRes = HandleURIStart(args);
            if (uriRes == URIStartResult.CLOSE) return;
            var socket = new ClientSocket();


            var thread = new Thread(OpenGUI);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (Settings.conInterface is null) Thread.Sleep(250);
            //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Init())
                .Wait(); // run socket in background. Important to wait for init to have actually finished before continuing
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            Task.Factory.StartNew(() =>
                IPCadapter.getInstance()
                    .RunLoop(uriRes == URIStartResult.PARSE ? args[0] : null)); // Run listener for tokens

            Console.ReadLine();
            //test
        }

        private enum URIStartResult
        {
            CLOSE,
            PARSE,
            CONTINUE
        }

        private static void OpenGUI()
        {
            var a = new App();
            window = new MainWindow();
            a.MainWindow = window;
            Settings.form = window;
            Settings.conInterface = new FormConsole(window);
            a.Run(window);
            Environment.Exit(0);
        }

        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        private static URIStartResult HandleURIStart(string[] args)
        {
            Console.WriteLine(GetExecutablePath());
            const string appName = "AmongUsCapture";
            mutex = new Mutex(true, appName, out var createdNew);
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!createdNew) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart)
                {
                    var pipeClient = new NamedPipeClientStream(".", "AmongUsCapturePipe", PipeDirection.InOut,
                        PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    pipeClient.Connect();
                    var ss = new StreamString(pipeClient);
                    ss.WriteString(args[0]);
                    pipeClient.Close();
                }

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                result = URIStartResult.PARSE;
            }

            RegisterProtocol();

            return result;
        }

        private static void RegisterProtocol() //myAppPath = full path to your application
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
                // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
                var applicationLocation = GetExecutablePath();

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}