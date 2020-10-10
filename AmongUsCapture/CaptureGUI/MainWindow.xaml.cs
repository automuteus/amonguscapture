using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Config.Net;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TextColorLibrary;
using Color = System.Drawing.Color;

namespace CaptureGUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static Color NormalTextColor = Color.White;

        private readonly AppSettings config = new ConfigurationBuilder<AppSettings>()
            .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();

        private bool connected;
        private readonly object locker = new object();

        public MainWindow()
        {
            InitializeComponent();
            var p = ConsoleTextBox.Document.Blocks.FirstBlock as Paragraph;
            ConsoleTextBox.Document.Blocks.Clear();
            DataContext = config;
            config.PropertyChanged += ConfigOnPropertyChanged;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAccent;
            ThemeManager.Current.SyncTheme();
            SetDefaultThemeColor();
            ApplyDarkMode();
            //ApplyDarkMode();
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
                });
        }

        private void SetDefaultThemeColor()
        {
            if (config.ranBefore) return;
            config.ranBefore = true;
            var currentTheme = ThemeManager.Current.DetectTheme();
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
            var newTheme = ThemeManager.Current.DetectTheme();
            config.DarkMode = newTheme.BaseColorScheme == ThemeManager.BaseColorDark;
            ThemeManager.Current.ChangeTheme(this, currentTheme ?? throw new InvalidOperationException());
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
            //this.ManualConnectButton.IsEnabled = false;
            setConnectionStatus(connected);
            connected = !connected;
        }

        private async void GameCodeBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await this.ShowMessageAsync("Gamecode copied to clipboard!", "");
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
            lock (locker)
            {
                ConsoleTextBox.BeginInvoke(tb =>
                {
                    var paragraph = new Paragraph();
                    foreach (var part in TextColor.toParts(ColoredText))
                    {
                        var run = new Run(part.text)
                        {
                            Foreground =
                                new SolidColorBrush(System.Windows.Media.Color.FromArgb(part.textColor.A,
                                    part.textColor.R, part.textColor.G, part.textColor.B))
                        };
                        paragraph.Inlines.Add(run);
                        paragraph.LineHeight = 1;
                        //this.AppendText(part.text, part.textColor, false);
                    }

                    tb.Document.Blocks.Add(paragraph);
                    tb.ScrollToEnd();
                });
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //TestFillConsole(10);
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

        private void MainWindow_OnContentRendered(object? sender, EventArgs e)
        {
            TestFillConsole(10);
            setCurrentState("GAMESTATE");
            setGameCode("GAMECODE");
        }
    }
}