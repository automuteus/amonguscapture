using AmongUsCapture.ConsoleTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SharedMemory;

namespace AmongUsCapture
{
    static class Program
    {
        private static UserForm form;
        private static Mutex mutex = null;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Settings.PersistentSettings.debugConsole)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }

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

            IPCadapter.getInstance().RegisterSlave();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientSocket socket = new ClientSocket();
            form = new UserForm(socket);
            Settings.form = form;
            Settings.conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Init()).Wait(); // run socket in background. Important to wait for init to have actually finished before continuing
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            form.Load += (sender, eventArgs) =>
            {
                if (uriRes == URIStartResult.PARSE) IPCadapter.getInstance().SendToken(args[0]);
            };
            //AllocConsole();
            Application.Run(form);
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
