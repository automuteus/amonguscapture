using System;
using System.Diagnostics;
using System.Drawing;
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
    static class Program
    {
        public static MainWindow window;
        private static readonly EventWaitHandle waitHandle = new AutoResetEvent(false);
        public static readonly ClientSocket socket = new ClientSocket();
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            //Create the Form Console interface. 
            var socketTask = Task.Factory.StartNew(() => socket.Init()); // run socket in background. Important to wait for init to have actually finished before continuing
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            socketTask.Wait();
            IPCadapter.getInstance().RegisterMinion();
            
        }


        private static void OpenGUI()
        {
            var a = new App();
            window = new MainWindow();
            waitHandle.Set();
            a.MainWindow = window;

            a.Run(window);
            Environment.Exit(0);
        }

        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }


        

        
    }
}