using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Octokit;
using Application = Octokit.Application;

namespace AUCapture_WPF
{
    public class UserDataContext
    {
        public IDialogCoordinator DialogCoordinator { get; set; }
        public IAppSettings Settings { get; set; }

        public string Version { get; set; }
        public string LatestVersion { get; set; }
        private ICommand textBoxButtonCopyCmd;

        public ICommand TextBoxButtonCopyCmd
        {
            get
            {
                return this.textBoxButtonCopyCmd ??= new SimpleCommand
                {
                    CanExecuteDelegate = x =>
                    {
                        switch (x)
                        {
                            case string s:
                                return s != "";
                            case TextBox t:
                                return t.Text != "";
                            case PasswordBox p:
                                return p.Password != "";
                            default:
                                return true;
                        }
                    },
                    ExecuteDelegate = async x =>
                    {
                        if(x is string s)
                        {
                            Clipboard.SetText(s);
                        }
                        else if(x is TextBox t)
                        {
                            Clipboard.SetText(t.Text);
                        }
                        else if(x is PasswordBox p)
                        {
                            Clipboard.SetText(p.Password);
                        }
                    }
                };
            }
        }

        public UserDataContext(IDialogCoordinator dialogCoordinator, IAppSettings settings)
        {
            DialogCoordinator = dialogCoordinator;
            Settings = settings;
            Settings.debug = AmongUsCapture.Settings.PersistentSettings.debugConsole;
            Settings.PropertyChanged += SettingsOnPropertyChanged;

            FileVersionInfo v = FileVersionInfo.GetVersionInfo(App.GetExecutablePath());
            Version = $"{v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}";
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