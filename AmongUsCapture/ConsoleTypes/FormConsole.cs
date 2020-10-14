using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using AmongUsCapture.TextColorLibrary;
using CaptureGUI;

namespace AmongUsCapture.ConsoleTypes
{
    public class FormConsole : ConsoleInterface
    {
        private StreamWriter logFile;
        public MainWindow form;
        private static object locker = new Object();

        public FormConsole(MainWindow userForm)
        {
            form = userForm;
            logFile = File.CreateText(Path.Combine(Directory.GetParent(Program.GetExecutablePath()).FullName, "CaptureLog.txt"));
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            throw new NotImplementedException();
        }

        public void WriteColoredText(string ColoredText)
        {
            form.WriteColoredText(ColoredText);
            WriteToLog(ColoredText);
        }


        public void WriteLine(string s)
        {
            throw new NotImplementedException();
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
            WriteToLog($"[{ModuleName}]: {text}");
        }


        public void WriteToLog(string textToLog)
        {
            WriteLogLine(DateTime.UtcNow, textToLog);
        }

        private string StripColor(string text)
        {
            return TextColor.StripColor(text);
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
