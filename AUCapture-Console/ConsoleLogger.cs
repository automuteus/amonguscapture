using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;

namespace AUCapture_Console
{
    class ConsoleLogger : IConsoleInterface
    {
        public Color getNormalColor()
        {
            return Color.White;
        }

        public void WriteLine(string s)
        {
            Console.WriteLine(TextColor.StripColor(s));
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            WriteLine($"[{ModuleName}]: {text}");
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            WriteLine(text);
        }

        public void WriteColoredText(string ColoredText)
        {
            WriteLine(ColoredText);
        }

        public bool CrackDetected()
        {
            Console.WriteLine("Crack detected");
            return true;
        }
    }
}
