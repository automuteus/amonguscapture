using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using NLog.Targets;

namespace AUCapture_WPF
{
    class WPFLogger : IConsoleInterface
    {
        public MainWindow form;
        public static string LogFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture", "logs");
        private static Logger logger = LogManager.GetLogger("WPFLogger");

        public WPFLogger(MainWindow userForm)
        {
            form = userForm;
            //Cleanup old log
            if (File.Exists(Path.Combine(Directory.GetParent(App.GetExecutablePath()).FullName, "CaptureLog.txt")))
            {
                File.Delete(Path.Combine(Directory.GetParent(App.GetExecutablePath()).FullName, "CaptureLog.txt"));
            }

            var LoggingConfig = new NLog.Config.LoggingConfiguration();
            FileVersionInfo v = FileVersionInfo.GetVersionInfo(App.GetExecutablePath());
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/latest.log",
                ArchiveFileName= "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/{#}.log",
                ArchiveNumbering= ArchiveNumberingMode.Date,
                Layout = "${time:universalTime=True} | ${message}",
                MaxArchiveFiles = 5,
                ArchiveOldFileOnStartup = true,
                ArchiveDateFormat= "yyyy-MM-dd HH_mm_ss",
                Header = $"Capture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}\n",
                Footer = $"\nCapture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}"
            };
            LoggingConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = LoggingConfig;
            logger = LogManager.GetLogger("WPFLogger");
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            throw new NotImplementedException();
        }

        public void WriteColoredText(string ColoredText)
        {
            WriteToLog(ColoredText);
        }

        public bool CrackDetected()
        {
            //Settings.conInterface.WriteModuleTextColored("Crack", Color.Red, "Trying to show thing");
            form.PlayGotEm();
            var x = form.context.DialogCoordinator.ShowMessageAsync(form.context,
                "YO HO YO HO YOOOOOOOOOOOOOUR A PIRATE",
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
            //form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
            WriteLogLine(ModuleName, text);
        }


        public void WriteToLog(string textToLog)
        {
            WriteLogLine("UNKNOWN", textToLog);
        }

        private string StripColor(string text)
        {
            return TextColor.StripColor(text);
        }

        private void WriteLogLine(string ModuleName, string text)
        {
            logger.Debug($"{StripColor(ModuleName).ToUpper()} | {StripColor(text)}");
        }
    }
}
