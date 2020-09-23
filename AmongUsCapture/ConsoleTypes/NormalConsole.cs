using System;
using System.Collections.Generic;
using System.Text;

namespace AmongUsCapture.ConsoleTypes
{
    public class NormalConsole : ConsoleInterface
    {
        public void WriteLine(string str)
        {
            Console.WriteLine(str);
        }
    }
}
