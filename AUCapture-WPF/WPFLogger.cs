using System;
using System.Drawing;
using System.IO;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using MahApps.Metro.Controls.Dialogs;

namespace AUCapture_WPF
{
    class WpfLogger : IConsoleInterface
    {
        private static readonly object locker = new Object();

        private readonly StreamWriter logFile;
        private MainWindow Form { get; set; }

        public WpfLogger(MainWindow userForm)
        {
            Form = userForm;
            logFile = File.CreateText(Path.Combine(Directory.GetParent(App.GetExecutablePath()).FullName, "CaptureLog.txt"));
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            throw new NotImplementedException();
        }

        public void WriteColoredText(string ColoredText)
        {
            Form.WriteColoredText(ColoredText);
            WriteToLog(ColoredText);
        }

        public bool CrackDetected()
        {
            Settings.ConInterface.WriteModuleTextColored("Crack", Color.Red, "Trying to show thing");
            var result = 
                Form.Context.DialogCoordinator.ShowMessageAsync(
                    Form.Context,
                    "Uh oh.",
                    "We have detected that you are running an unsupported version of the game. This may or may not work.",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "I understand",
                        NegativeButtonText = "Exit",
                        ColorScheme = MetroDialogColorScheme.Theme,
                        DefaultButtonFocus = MessageDialogResult.Negative
                    })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            Settings.ConInterface.WriteModuleTextColored("Crack", Color.Red, "finished show thing");

            return result == MessageDialogResult.Affirmative;
        }


        public Color getNormalColor()
        {
            return Form.NormalTextColor;
        }

        public void WriteLine(string s)
        {
            throw new NotImplementedException();
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            Form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
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
