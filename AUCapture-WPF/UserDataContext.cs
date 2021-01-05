using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AUCapture_WPF.IPC;
using MahApps.Metro.Controls;
using Application = System.Windows.Application;

namespace AUCapture_WPF
{
    public class UserDataContext : INotifyPropertyChanged
    {
        public IDialogCoordinator DialogCoordinator { get; set; }
        public IAppSettings Settings { get; set; }

        public string Version { get; set; }
        public string LatestReleaseAssetURL { get; set; }
        public string LatestVersion { get; set; }
        private ICommand textBoxButtonCopyCmd;
        public List<AccentColorMenuData> AccentColors { get; set; }
        public class AccentColorMenuData
        {
            public string Name { get; set; }

            public Brush BorderColorBrush { get; set; }

            public Brush ColorBrush { get; set; }

            public AccentColorMenuData()
            {
                ChangeAccentCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = DoChangeTheme
                };

            }

            public ICommand ChangeAccentCommand { get; }

            protected virtual void DoChangeTheme(object sender)
            {
                ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, Name);
            }
        }

        public ICommand TextBoxButtonCopyCmd => textBoxButtonCopyCmd ??= new SimpleCommand
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
                if (x is string s)
                {
                    Clipboard.SetText(s);
                }
                else if (x is TextBox t)
                {
                    Clipboard.SetText(t.Text);
                }
                else if (x is PasswordBox p)
                {
                    Clipboard.SetText(p.Password);
                }
            }
        };

        public UserDataContext(IDialogCoordinator dialogCoordinator, IAppSettings settings)
        {
            DialogCoordinator = dialogCoordinator;
            Settings = settings;
            Settings.debug = AmongUsCapture.Settings.PersistentSettings.debugConsole;
            Settings.PropertyChanged += SettingsOnPropertyChanged;
            AccentColors = ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a => new AccentColorMenuData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                .ToList();
            FileVersionInfo v = FileVersionInfo.GetVersionInfo(App.GetExecutablePath());
            Version = $"{v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}";
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("AmongUsCapture", Version));
                Release latest = client.Repository.Release.GetLatest("denverquane", "amonguscapture").Result;
                LatestReleaseAssetURL = latest.Assets.First(x => x.Name == "AmongUsCapture.zip").BrowserDownloadUrl;
                LatestVersion = $"{latest.TagName}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LatestVersion = "ERROR";
            }
            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(AccentColors));



        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.debug))
            {
                AmongUsCapture.Settings.PersistentSettings.debugConsole = Settings.debug;
                Task.Factory.StartNew((() =>
                {
                    var selection = this.DialogCoordinator.ShowMessageAsync(this, "Restart required",
                        $"To {(Settings.debug ? "enable" : "disable")} debug mode, we need to restart.",
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                        new MetroDialogSettings
                        {
                            AnimateHide = true, AffirmativeButtonText = "Restart", NegativeButtonText = "Exit",
                            FirstAuxiliaryButtonText = "Later", DefaultButtonFocus = MessageDialogResult.Affirmative
                        }).Result;
                    if (selection == MessageDialogResult.Affirmative)
                    {
                        Application.Current.Invoke(()=>
                        {
                            IPCAdapter.getInstance().mutex.ReleaseMutex(); //Release the mutex so the other app does not see us. 
                            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                            Application.Current.Shutdown(0);
                        });
                    }
                    else if(selection == MessageDialogResult.Negative)
                    {
                        Application.Current.Invoke(() => { Application.Current.Shutdown(0); });
                    }
                }));


            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}