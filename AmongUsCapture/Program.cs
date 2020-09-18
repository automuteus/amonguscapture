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

            string guildID = "754465589958803548";

            if (File.Exists("guildid.txt"))
            {
                guildID = File.ReadAllText("guildid.txt");
            }

            socket.Connect("http://localhost:8123", guildID); //synchronously force the socket to connect, and also broadcast the guildID

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
