using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AmongUsCapture.ConsoleTypes
{
    public class NormalConsole : ConsoleInterface
    {
        public void WriteLine(string str)
        {
            Console.WriteLine(str);
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            Console.WriteLine($"[{ModuleName}]: {text}");
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            Console.WriteLine(text);
        }
    }
}
