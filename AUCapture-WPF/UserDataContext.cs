using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using MahApps.Metro.Controls.Dialogs;
using Octokit;

namespace AUCapture_WPF
{
    public class UserDataContext
    {
        public IDialogCoordinator DialogCoordinator { get; set; }
        public IAppSettings Settings { get; set; }

        public string Version { get; set; }
        public string LatestVersion { get; set; }

        public UserDataContext(IDialogCoordinator dialogCoordinator, IAppSettings settings)
        {
            DialogCoordinator = dialogCoordinator;
            Settings = settings;
            Settings.debug = AmongUsCapture.Settings.PersistentSettings.debugConsole;
            Settings.PropertyChanged += SettingsOnPropertyChanged;

            FileVersionInfo v = FileVersionInfo.GetVersionInfo(App.GetExecutablePath());
            Version = $"{v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}";
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("AmongUsCapture", Version));
                var latest = client.Repository.Release.GetLatest("denverquane", "amonguscapture").Result;
                LatestVersion = $"{latest.TagName}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LatestVersion = "ERROR";
            }

            


        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.debug))
            {
                AmongUsCapture.Settings.PersistentSettings.debugConsole = Settings.debug;
            }
        }
    }
}