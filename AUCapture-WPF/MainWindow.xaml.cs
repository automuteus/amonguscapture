using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AUCapture_WPF.IPC;
using Config.Net;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Color = System.Drawing.Color;

namespace AUCapture_WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Color NormalTextColor { get; private set; } = Color.White;

        private readonly IAppSettings config = 
            new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile(
                    Path.Join(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "AmongUsCapture",
                        "AmongUsGUI",
                        "Settings.json")
                    )
                .Build();

        public UserDataContext Context { get; protected set; }
        private readonly object locker = new object();

        public MainWindow()
        {
            InitializeComponent();

            ConsoleTextBox.Document.Blocks.Clear();
            Context = new UserDataContext(DialogCoordinator.Instance, config);
            DataContext = Context;
            config.PropertyChanged += ConfigOnPropertyChanged;

            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;

            IpcAdapter.getInstance().OnToken += (sender, token) =>
            {
                this.BeginInvoke((w) =>
                {
                    if (!w.IsVisible)
                    {
                        w.Show();
                    }

                    if (w.WindowState == WindowState.Minimized)
                    {
                        w.WindowState = WindowState.Normal;
                    }

                    w.Activate();
                    w.Topmost = true;  // important
                    w.Topmost = false; // important
                    w.Focus();         // important

                    w.Activate();
                });
            };
        }


        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            AmongUsCapture.Settings.ConInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            AmongUsCapture.Settings.ConInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
        }

        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            GameCodeBox.BeginInvoke(a => a.Text = e.LobbyCode);
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            var OutputCode = Color.White;
            switch (pColor)
            {
                case PlayerColor.Red:
                    OutputCode = Color.Red;
                    break;
                case PlayerColor.Blue:
                    OutputCode = Color.RoyalBlue;
                    break;
                case PlayerColor.Green:
                    OutputCode = Color.Green;
                    break;
                case PlayerColor.Pink:
                    OutputCode = Color.Magenta;
                    break;
                case PlayerColor.Orange:
                    OutputCode = Color.Orange;
                    break;
                case PlayerColor.Yellow:
                    OutputCode = Color.Yellow;
                    break;
                case PlayerColor.Black:
                    OutputCode = Color.Gray;
                    break;
                case PlayerColor.White:
                    OutputCode = Color.White;
                    break;
                case PlayerColor.Purple:
                    OutputCode = Color.MediumPurple;
                    break;
                case PlayerColor.Brown:
                    OutputCode = Color.SaddleBrown;
                    break;
                case PlayerColor.Cyan:
                    OutputCode = Color.Cyan;
                    break;
                case PlayerColor.Lime:
                    OutputCode = Color.Lime;
                    break;
            }

            return OutputCode;
        }

        private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(config.FontSize))
                ConsoleTextBox.BeginInvoke(tb =>
                {
                    tb.Document.FontSize = config.FontSize;
                });
        }

        private void SetDefaultThemeColor()
        {
            if (config.RanBefore) return;
            config.RanBefore = true;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            var newTheme = ThemeManager.Current.DetectTheme();
            config.DarkMode = newTheme.BaseColorScheme == ThemeManager.BaseColorDark;
            Darkmode_toggleswitch.IsOn = config.DarkMode;
        }


        private void ApplyDarkMode()
        {
            if (config.DarkMode)
            {
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorDark);
                NormalTextColor = Color.White;

            }
            else
            {
                NormalTextColor = Color.Black;
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorLight);
            }
        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            // Open up the settings flyout
            SettingsFlyout.IsOpen = true;
        }

        private void Darkmode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch toggleSwitch)) return;
            ApplyDarkMode();
        }

        private void ManualConnect_Click(object sender, RoutedEventArgs e)
        {
            //Open up the manual connection flyout.
            ManualConnectionFlyout.IsOpen = true;

        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            setCurrentState(e.NewState.ToString());
            AmongUsCapture.Settings.ConInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
        }


        private async void GameCodeBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await this.ShowMessageAsync("Gamecode copied to clipboard!", "", MessageDialogStyle.Affirmative);
            Clipboard.SetText(GameCodeBox.Text);
        }

        public void setGameCode(string gamecode)
        {
            GameCodeBox.BeginInvoke(tb => { GameCodeBox.Text = gamecode; });
        }

        public void setCurrentState(string state)
        {
            StatusBox.BeginInvoke(tb => { tb.Text = state; });
        }

        public void setConnectionStatus(bool connected)
        {
            ThemeManager.Current.ChangeThemeColorScheme(this, connected ? "Green" : "Red");
        }

        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            WriteColoredText(
                $"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(string ColoredText)
        {
            lock (locker)
            {
                ConsoleTextBox.BeginInvoke(tb =>
                {
                    var paragraph = new Paragraph();
                    foreach (var part in TextColor.toParts(ColoredText))
                    {
                        var run = new Run(part.text);

                        if (part.textColor.ToTextColor() != NormalTextColor.ToTextColor())
                        {
                            run.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(part.textColor.A,
                                part.textColor.R, part.textColor.G, part.textColor.B));
                        }
                        paragraph.Inlines.Add(run);
                        paragraph.LineHeight = 1;
                    }

                    tb.Document.Blocks.Add(paragraph);
                    tb.ScrollToEnd();
                });
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Nothing implemented
        }

        public bool Cracked()
        {
            var result =
                this.ShowMessageAsync(
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
                .Result;
            return result == MessageDialogResult.Affirmative;
        }

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            SetDefaultThemeColor();

            ApplyDarkMode();
        }

        private void SubmitConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            IpcAdapter.getInstance().SendToken(config.Host, config.ConnectCode);
            ManualConnectionFlyout.IsOpen = false;
        }
    }
}
