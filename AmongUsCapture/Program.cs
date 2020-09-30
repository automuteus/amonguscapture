using AmongUsCapture.ConsoleTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AmongUsCapture
{
    static class Program
    {
        private static Mutex mutex = null;
        private static bool doConsole = true;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (doConsole)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }
            string[] args = Environment.GetCommandLineArgs();
            const string appName = "AmongUsCapture";
            bool createdNew;
            mutex = new Mutex(true, appName, out createdNew);
            
            if (!createdNew)
            {
                var pipeClient = new NamedPipeClientStream(".", "AmongUsCapturePipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                pipeClient.Connect();
                var ss = new StreamString(pipeClient);
                if (ss.ReadString() == "Ready for token.")
                {
                    Console.WriteLine(args[1].Substring("capture:".Length, args[1].Length - "capture:".Length));
                    ss.WriteString(args[1].Substring("capture:".Length, args[1].Length - "capture:".Length));

                }
                else
                {
                    Console.WriteLine("Failed to verify pipe auth");
                }
                pipeClient.Close();
                return;
            }
            

           
            RegisterMyProtocol(args[0]);

            try
            {
                //if there's an argument passed, write it
                Console.WriteLine("Argument: " + args[1].Replace("capture:", string.Empty));  
            }
            catch
            {
                Console.WriteLine("No argument(s)");  //if there's an exception, there's no argument
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientSocket socket = new ClientSocket();
            var form = new UserForm(socket);
            Settings.conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Connect(Settings.PersistentSettings.host)); //synchronously force the socket to connect
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            Task.Factory.StartNew(() => IPCadapter.getInstance().runloop()); //Run listener for tokens

            //AllocConsole();
            Application.Run(form);
            
            //test
        }
        
        const string UriScheme = "capture";
        const string FriendlyName = "AmongUs Capture";

        static void RegisterMyProtocol(string myAppPath)  //myAppPath = full path to your application
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
                // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
                string applicationLocation = Process.GetCurrentProcess().MainModule.FileName;

                Console.WriteLine(applicationLocation);

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
        static extern bool AllocConsole();

    }


}
