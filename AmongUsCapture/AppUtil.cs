using System.Diagnostics;

namespace AmongUsCapture
{
    static class AppUtil
    {
        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
    }
}
