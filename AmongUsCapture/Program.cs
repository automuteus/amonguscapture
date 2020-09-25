using AmongUsCapture.ConsoleTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmongUsCapture
{
    static class Program
    {
        private static bool doConsole = false;

        public static ConsoleInterface conInterface = null;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(doConsole)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientSocket socket = new ClientSocket();

            string hostPath = "host.txt";
          
            //TODO make proper properties file
            string host = File.Exists(hostPath) ? File.ReadAllText(hostPath) : "http://localhost:8123";
            var form = new UserForm(socket);
            conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Connect(host)); //synchronously force the socket to connect
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            //(new DebugConsole(debugGui)).Run();
            
            Application.Run(form);
            //test
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

    }
}
