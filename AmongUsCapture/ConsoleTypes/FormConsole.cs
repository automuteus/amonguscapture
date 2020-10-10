using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace AmongUsCapture.ConsoleTypes
{
    public class FormConsole : ConsoleInterface
    {
        private StreamWriter logFile;
        public UserForm form = null;
        private static object locker = new Object();
        public FormConsole(UserForm userForm)
        {
            form = userForm;
            var parent = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName);
            logFile = File.CreateText(Path.Join(parent.FullName, "CaptureLog.txt"));
        }

        public void WriteColoredText(string ColoredText)
        {
            form.WriteColoredText(ColoredText);
            WriteToLog(ColoredText);
        }

        public void WriteLine(string str)
        {
            form.WriteLineToConsole(str);
            WriteToLog(str);
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
            WriteToLog($"[{ModuleName}]: {text}");
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            form.WriteLineFormatted(text, acceptNewLines);
            WriteToLog(text);
        }

        public void WriteToLog(string textToLog)
        {
            WriteLogLine(DateTime.UtcNow, textToLog);
        }

        private string StripColor(string text)
        {
            return TextColorLibrary.TextColor.StripColor(text);
        }

        private void WriteLogLine(DateTime time, string textToLog)
        {
            lock (locker)
            {
                logFile.WriteLine($"{time.ToLongTimeString()} | {StripColor(textToLog)}");
                logFile.Flush();
            }
            
        }
    }
}
