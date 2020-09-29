using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AmongUsCapture.ConsoleTypes
{
    public class FormConsole : ConsoleInterface
    {
        private UserForm form = null;
        public FormConsole(UserForm userForm)
        {
            form = userForm;
        }
        public void WriteLine(string str)
        {
            form.WriteLineToConsole(str);
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            form.WriteLineFormatted(text, acceptNewLines);
        }
    }
}
