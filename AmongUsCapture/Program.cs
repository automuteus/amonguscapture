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
        private static bool debugGui = false;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(!debugGui)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientSocket socket = new ClientSocket();

            string guildIDPath = "guildid.txt";
            string hostPath = "host.txt";
            
            //TODO this should fail if the ID is not present but I'm not changing that
            //TODO make proper properties file
            string guildID = File.Exists(guildIDPath) ? File.ReadAllText(guildIDPath) : "754465589958803548";
            string host = File.Exists(hostPath) ? File.ReadAllText(hostPath) : "http://localhost:8123";

            Task.Factory.StartNew(() => socket.Connect(host, guildID)); //synchronously force the socket to connect, and also broadcast the guildID

            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background

            if (debugGui)
            {
                Application.Run(new MainForm());
            } else
            {
                (new DebugConsole()).Run();
            }
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

    }
}
