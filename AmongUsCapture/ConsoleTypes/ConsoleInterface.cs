using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AmongUsCapture
{
    interface ConsoleInterface
    {
        public void WriteLine(string s);
        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text);
    }
}
