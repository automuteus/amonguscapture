using System;
using System.Collections.Generic;
using System.Linq;
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
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            if (debugGui)
            {
                Application.Run(new MainForm());
            } else
            {
                (new DebugConsole()).Run();
            }
        }

    }
}
