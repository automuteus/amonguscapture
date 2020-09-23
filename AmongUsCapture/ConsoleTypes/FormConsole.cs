using System;
using System.Collections.Generic;
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
    }
}
