using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using MahApps.Metro.Controls.Dialogs;

namespace AUCapture_WPF
{
    class WPFLogger : IConsoleInterface
    {
        private StreamWriter logFile;
        public MainWindow form;
        private static object locker = new Object();

        public WPFLogger(MainWindow userForm)
        {
            form = userForm;
            logFile = File.CreateText(Path.Combine(Directory.GetParent(App.GetExecutablePath()).FullName, "CaptureLog.txt"));
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

        public bool CrackDetected()
        {
            //Settings.conInterface.WriteModuleTextColored("Crack", Color.Red, "Trying to show thing");
            form.PlayGotEm();
            var x = form.context.DialogCoordinator.ShowMessageAsync(form.context, "YO HO YO HO YOOOOOOOOOOOOOUR A PIRATE",
                "We have detected that you are running an unsupported version of the game. This may or may not work.",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "RRRRRRRR MATEY", NegativeButtonText = "BACK TO THE SEA",
                    ColorScheme = MetroDialogColorScheme.Theme,
                    DefaultButtonFocus = MessageDialogResult.Negative
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            //Settings.conInterface.WriteModuleTextColored("Crack", Color.Red, "finished show thing");

            return x == MessageDialogResult.Affirmative;
        }


        public Color getNormalColor()
        {
            return form.NormalTextColor;
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
