using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AmongUsCapture
{
    public interface ConsoleInterface
    {
        public void WriteLine(string s);
        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text);
        public void WriteTextFormatted(string text, bool acceptNewLines = true);

        public void WriteColoredText(string ColoredText);
    }
}
