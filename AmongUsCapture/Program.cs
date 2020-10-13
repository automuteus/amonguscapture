using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AmongUsCapture.CaptureGUI;
using AmongUsCapture.ConsoleTypes;
using CaptureGUI;
using Microsoft.Win32;
using SharedMemory;

namespace AmongUsCapture
{
    internal static class Program
    {
        public static MainWindow window;
        private static Mutex mutex = null;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (Settings.PersistentSettings.debugConsole)
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs

            var uriRes = IPCadapter.getInstance().HandleURIStart(args);
            switch (uriRes)
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


            
            var socket = new ClientSocket();
            
            //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Init()).Wait(); // run socket in background. Important to wait for init to have actually finished before continuing
            var thread = new Thread(OpenGUI);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            while (Settings.conInterface is null) Thread.Sleep(250);

            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            IPCadapter.getInstance().RegisterMinion();
            window.Loaded += (sender, eventArgs) =>
            {
                if (uriRes == URIStartResult.PARSE) IPCadapter.getInstance().SendToken(args[0]);
            };
            thread.Join();
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


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        
    }
}