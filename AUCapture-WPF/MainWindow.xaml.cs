using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AUCapture_WPF.IPC;
using Config.Net;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Color = System.Drawing.Color;

namespace AUCapture_WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Color NormalTextColor = Color.White;

        private IAppSettings config;

        public UserDataContext context;
        private bool connected;
        private readonly object locker = new object();
        private Queue<string> DeadMessages = new Queue<string>();

        public MainWindow()
        {
            InitializeComponent();
            
            var p = ConsoleTextBox.Document.Blocks.FirstBlock as Paragraph;
            ConsoleTextBox.Document.Blocks.Clear();
            config = new ConfigurationBuilder<IAppSettings>()
                .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            context = new UserDataContext(DialogCoordinator.Instance, config);
            DataContext = context;
            config.PropertyChanged += ConfigOnPropertyChanged;
            App.handler.OnReady += (sender, args) =>
            {
                App.socket.AddHandler(App.handler);
            };
            if (context.Settings.discordToken != "")
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(2000);
                    App.handler.Init(context.Settings.discordToken);
                });
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(2000);
                    WriteConsoleLineFormatted("Discord", Color.Red, "You do not have a self-host discord token set. Enabling this in settings will increase performance.");
                });
            }
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
            GameMemReader.getInstance().GameOver += OnGameOver;
            IPCAdapter.getInstance().OnToken += (sender, token) => {
                this.BeginInvoke((w) =>
                {
                    if (!w.context.Settings.FocusOnToken) return;
                    if (!w.IsVisible)
                    {
                        w.Show();
                    }

                    if (w.WindowState == WindowState.Minimized)
                    {
                        w.WindowState = WindowState.Normal;
                    }

                    w.Activate();
                    w.Focus();         // important
                }); };


            
            Version version = new Version(context.Version);
            Version latestVersion = new Version(context.LatestVersion);

            if (latestVersion.CompareTo(version) > 0)
                this.ShowMessageAsync("Caution",
                    $"We've detected you're using an older version of AmongUsCapture!\nYour version: {version}\nLatest version: {latestVersion}",
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings{AffirmativeButtonText = "Download", NegativeButtonText = "No thanks", DefaultButtonFocus = MessageDialogResult.Affirmative}).ContinueWith(
                    task =>
                    {
                        if (task.Result == MessageDialogResult.Affirmative)
                        {
                            OpenBrowser(@"https://github.com/denverquane/amonguscapture/releases/latest/");
                        }
                    });
            //ApplyDarkMode();
        }

        private void OnGameOver(object? sender, GameOverEventArgs e)
        {
            WriteConsoleLineFormatted("GameOver", Color.BlueViolet, JsonConvert.SerializeObject(e, Formatting.None, new StringEnumConverter()));
            Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
        }


        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            if (e.Action == PlayerAction.Died)
            {
                DeadMessages.Enqueue($"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            }
            else
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki,$"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            }
            
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
            //WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
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
            if (e.PropertyName == "fontSize")
                ConsoleTextBox.BeginInvoke(tb =>
                {
                    tb.Document.FontSize = config.fontSize;
                    //foreach (var block in tb.Document.Blocks)
                    //{
                    //    block.FontSize = config.fontSize;
                    //}
                }, DispatcherPriority.Input);
        }

        private void SetDefaultThemeColor()
        {
            if (config.ranBefore) return;
            config.ranBefore = true;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            var newTheme = ThemeManager.Current.DetectTheme();
            config.DarkMode = newTheme.BaseColorScheme == ThemeManager.BaseColorDark;
            Darkmode_toggleswitch.IsOn = config.DarkMode;
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                // throw 
            }
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
            //Cracked();
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
            while (DeadMessages.Count > 0)
            {
                    AmongUsCapture.Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, DeadMessages.Dequeue());
            }
            
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
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
            if (connected)
                ThemeManager.Current.ChangeThemeColorScheme(this, "Green");
            else
                ThemeManager.Current.ChangeThemeColorScheme(this, "Red");
        }

        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            WriteColoredText(
                $"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(string ColoredText)
        {
            ConsoleTextBox.BeginInvoke(tb =>
            {
                var paragraph = new Paragraph();
                foreach (var part in TextColor.toParts(ColoredText))
                {
                    //Foreground="{DynamicResource MahApps.Brushes.Text}"
                    var run = new Run(part.text);

                    if (part.textColor.ToTextColor() != NormalTextColor.ToTextColor())
                    {
                        run.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(part.textColor.A,
                            part.textColor.R, part.textColor.G, part.textColor.B));
                    }
                    paragraph.Inlines.Add(run);
                    paragraph.LineHeight = 1;
                    //this.AppendText(part.text, part.textColor, false);
                }
                tb.Document.Blocks.Add(paragraph);
                tb.ScrollToEnd();
            }, DispatcherPriority.Input);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {
            for (var i = 0; i < entries; i++)
            {
                var nonString = "Wow! Look at this pretty text!";
                WriteConsoleLineFormatted("Rainbow", TextColor.Rainbow((float) i / entries),
                    TextColor.getRainbowText(nonString, i));
            }

            ;
            //this.WriteColoredText(getRainbowText("This is a Pre-Release from Carbon's branch."));
        }

        public bool Cracked()
        {
            PlayGotEm();
            MessageDialogResult x = MessageDialogResult.Affirmative;
            x = this.ShowMessageAsync("Uh oh.",
                "We have detected that you are running an unsupported version of the game. This may or may not work.",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "I understand", NegativeButtonText = "Exit",
                    ColorScheme = MetroDialogColorScheme.Theme,
                    DefaultButtonFocus = MessageDialogResult.Negative
                }).Result;
            return x == MessageDialogResult.Affirmative; 
        }
        public void PlayGotEm()
        {
            this.BeginInvoke((win) =>
            {
                win.MemeFlyout.IsOpen = true;
                win.MemePlayer.Position = TimeSpan.Zero;
            });
            
            
        }

        private void MainWindow_OnContentRendered(object? sender, EventArgs e)
        {
            //TestFillConsole(10);
            //setCurrentState("GAMESTATE");
            //setGameCode("GAMECODE");
            SetDefaultThemeColor();

            ApplyDarkMode();
        }

        private void SubmitConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            IPCAdapter.getInstance().SendToken(config.host, config.connectCode);
            ManualConnectionFlyout.IsOpen = false;
        }

        private void MemePlayer_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            this.BeginInvoke((win) =>
            {
                win.MemeFlyout.IsOpen = false;
            });
        }

        private void MemeFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (MemeFlyout.IsOpen)
            {
                MemePlayer.Play();
                Task.Factory.StartNew(()=>
                {
                    Thread.Sleep(5000);
                    MemeFlyout.Invoke(new Action(() =>
                    {
                        if (MemeFlyout.IsOpen)
                        {
                            MemeFlyout.CloseButtonVisibility = Visibility.Visible;
                        }
                    }));

                });
            }
            else
            {
                MemeFlyout.CloseButtonVisibility = Visibility.Hidden;
                MemePlayer.Close();
                GC.Collect();
            }
        }
        private void SubmitDiscordButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (context.Settings.discordToken != "")
            {
                App.handler.Init(context.Settings.discordToken);
            }
        }

        private async void ReloadOffsetsButton_OnClick(object sender, RoutedEventArgs e)
        {
            GameMemReader.getInstance().offMan.refreshLocal();
            await GameMemReader.getInstance().offMan.RefreshIndex();
            GameMemReader.getInstance().CurrentOffsets = GameMemReader.getInstance().offMan
                .FetchForHash(GameMemReader.getInstance().GameHash);
            if (GameMemReader.getInstance().CurrentOffsets is not null)
            {
                WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"Loaded offsets: {GameMemReader.getInstance().CurrentOffsets.Description}");
            }
            else
            {
                WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"No offsets found for: {Color.Aqua.ToTextColor()}{GameMemReader.getInstance().GameHash.ToString()}.");

            }
        }

        private void HelpDiscordButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://www.youtube.com/watch?v=jKcEW5qpk8E");
        }


        private void ConsoleTextBox_OnCopying(object sender, DataObjectCopyingEventArgs e)
        {
            e.CancelCommand();
        }

        private void APIServerToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch toggleSwitch)) return;
            if (config.ApiServer)
            {
                WriteConsoleLineFormatted("APIServer", Color.Brown, "Starting server");
                ServerSocket.instance.Start();
            }
            else
            {
                WriteConsoleLineFormatted("APIServer", Color.Brown, "Stopping server");
                ServerSocket.instance.Stop();
            }
        }
    }
}