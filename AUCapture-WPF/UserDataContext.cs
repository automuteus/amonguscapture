using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AmongUsCapture;
using AUCapture_WPF.IPC;
using AUCapture_WPF.Models;
using Humanizer;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace AUCapture_WPF
{
    public class UserDataContext : INotifyPropertyChanged
    {
        public IDialogCoordinator DialogCoordinator { get; set; }
        public IAppSettings Settings { get; set; }

        public string Version { get; set; }
        public string LatestReleaseAssetURL { get; set; }
        public string LatestReleaseAssetSignedHashURL { get; set; }
        public string LatestVersion { get; set; }
        private ICommand textBoxButtonCopyCmd;
        private ICommand textBoxButtonHelpCmd;
        private ICommand openAmongUsCMD;
        public List<AccentColorMenuData> AccentColors { get; set; }
        private bool? _connected = false;
        public bool? Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }
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
        private string GetAmongUsLauncherLink()
        {
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\amongus\\shell\\open\\command");
            if (key is null)
            {
                return "steam://rungameid/945360";
            }
            else
            {
                var path = ((string)key.GetValue(""));
                path = path.Substring(0, path.Length - 4).Trim().Trim('\"');
                if (path.Contains("Epic Games"))
                {
                    return "com.epicgames.launcher://apps/963137e4c29d4c79a81323b8fab03a40?action=launch&silent=true";
                }
                else
                {
                    return "steam://rungameid/945360";
                }

            }

        }
        public ICommand OpenAmongUsCMD => openAmongUsCMD ??= new SimpleCommand
        {
            CanExecuteDelegate = x => true,
            ExecuteDelegate = async x =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(GetAmongUsLauncherLink()) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", GetAmongUsLauncherLink());
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", GetAmongUsLauncherLink());
                }
                else
                {
                    // throw 
                }
            }
        };
        public ICommand TextBoxButtonHelpCmd => textBoxButtonHelpCmd ??= new SimpleCommand
        {
            CanExecuteDelegate = x => true,
            ExecuteDelegate = async x =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo("https://www.youtube.com/watch?v=jKcEW5qpk8E") { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", "https://www.youtube.com/watch?v=jKcEW5qpk8E");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", "https://www.youtube.com/watch?v=jKcEW5qpk8E");
                }
                else
                {
                    // throw 
                }
            }
        };
        private ObservableCollection<Player> _players = new ObservableCollection<Player>();
        public ObservableCollection<Player> Players
        {
            get => _players;
            set
            {
                _players = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ConnectionStatus> _connectionStatuses = new ObservableCollection<ConnectionStatus>();
        public ObservableCollection<ConnectionStatus> ConnectionStatuses
        {
            get => _connectionStatuses;
            set
            {
                _connectionStatuses = value;
                OnPropertyChanged();
            }
        }

        private PlayMap? _gameMap;
        public PlayMap? GameMap
        {
            get => _gameMap;
            set
            {
                _gameMap = value;
                OnPropertyChanged();
            }
        }

        private string _gameCode;
        public string GameCode
        {
            get => _gameCode;
            set
            {
                _gameCode = value;
                OnPropertyChanged();
            }
        }
        private int _playerRows = 2;
        public int PlayerRows
        {
            get => _playerRows;
            set
            {
                _playerRows = value;
                OnPropertyChanged();
            }
        }

        private int _playerCols = 2;
        public int PlayerCols
        {
            get => _playerCols;
            set
            {
                _playerCols = value;
                OnPropertyChanged();
            }
        }

        private GameState? _gameState;
        public GameState? GameState
        {
            get => _gameState;
            set
            {
                _gameState = value;
                OnPropertyChanged();
            }
        }

        private bool _autoUpdaterEnabled;

        public bool AutoUpdaterEnabled
        {
            get => _autoUpdaterEnabled;
            set
            {
                _autoUpdaterEnabled = value;
                OnPropertyChanged();
            }
        }
        private static void Shuffle<T>(List<T> list)
        {
            Random rng = new Random(); 
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
        }
        public void GeneratePlayers(int numOfPlayers)
        {
            var nums = Enumerable.Range(0, 12).ToList();
            Shuffle(nums);
            var colors  = nums.Cast<PlayerColor>().Where(x=>!Players.Select(y=>y.Color).Contains(x)).Take(numOfPlayers).ToList();
            foreach (var color in colors)
            {
                var newPlayer = new Player(color.Humanize(), color, true, 0 ,0);
                Players.Add(newPlayer);
            }
        }
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
                Release latest = new Release();
                try
                {
                    latest = client.Repository.Release.GetLatest("automuteus", "amonguscapture").Result;

                }
                catch (Exception e)
                {
                    latest = client.Repository.Release.GetLatest("denverquane", "amonguscapture").Result;
                }
                
                LatestReleaseAssetURL = latest.Assets.First(x => x.Name == "AmongUsCapture.zip").BrowserDownloadUrl;
                if (latest.Assets.Any(x => x.Name == "AmongUsCapture.zip.sha256.pgp"))
                    LatestReleaseAssetSignedHashURL = latest.Assets.First(x => x.Name == "AmongUsCapture.zip.sha256.pgp").BrowserDownloadUrl;

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
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings
                        {
                            AnimateHide = true, AffirmativeButtonText = "Restart", NegativeButtonText = "Later",
                            DefaultButtonFocus = MessageDialogResult.Affirmative, 
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