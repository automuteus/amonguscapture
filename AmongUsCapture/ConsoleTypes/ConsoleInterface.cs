﻿using System.Drawing;

namespace AmongUsCapture
{
    public interface IConsoleInterface
    {
        public Color getNormalColor();
        public void WriteLine(string s);
        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text);
        public void WriteTextFormatted(string text, bool acceptNewLines = true);
        public void WriteColoredText(string ColoredText);
        public bool CrackDetected();
    }
}
