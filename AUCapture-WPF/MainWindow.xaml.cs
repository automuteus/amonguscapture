using AmongUsCapture;
using AUCapture_WPF.IPC;
using Config.Net;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AUCapture_WPF.Models;
using AUCapture_WPF.Properties;
using Discord;
using Gu.Localization;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using Humanizer;
using PgpCore;
using Microsoft.Win32;
using NLog;
using Color = System.Drawing.Color;
using PlayerColor = AmongUsCapture.PlayerColor;

namespace AUCapture_WPF {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public Color NormalTextColor = Color.White;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IAppSettings config;

        public UserDataContext context;
        private readonly bool connected;
        private readonly object locker = new();
        private readonly Queue<Player> DeadMessages = new();
        private Task ThemeGeneration;
        private readonly bool Updated;

        public MainWindow() {
            InitializeComponent();
            
            var appFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            var appExtension = Path.GetExtension(Process.GetCurrentProcess().MainModule.FileName);
            var archivePath = Path.Combine(appFolder, appName + "_Old" + appExtension);
            if (File.Exists(archivePath)) {
                Updated = true;
                try {
                    //Will wait for the other program to exit.
                    var me = Process.GetCurrentProcess();
                    var aProcs = Process.GetProcessesByName(me.ProcessName);
                    aProcs = aProcs.Where(x => x.Id != me.Id).ToArray();
                    if (aProcs != null && aProcs.Length > 0) aProcs[0].WaitForExit(1000);

                    File.Delete(archivePath);
                }
                catch (Exception e) {
                    Console.WriteLine("Could not delete old file.");
                }
            }
            else {
                Updated = false;
            }

            try {
                config = new ConfigurationBuilder<IAppSettings>()
                    .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            }
            catch (JsonReaderException e) //Delete file and recreate config
            {
                Console.WriteLine("Bad config. Clearing.");
                File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI", "Settings.json"));
                config = new ConfigurationBuilder<IAppSettings>()
                    .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            }

            context = new UserDataContext(DialogCoordinator.Instance, config);
            DataContext = context;
            App.handler.OnReady += (sender, args) => { App.socket.AddHandler(App.handler); };
            context.ConnectionStatuses.Add(new ConnectionStatus {Connected = false, ConnectionName = "AutoMuteUs"});
            context.ConnectionStatuses.Add(new ConnectionStatus {Connected = false, ConnectionName = "User bot"});
            Window.Topmost = context.Settings.alwaysOnTop;
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().ProcessHook += OnProcessHook;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().PlayerCosmeticChanged += OnPlayerCosmeticChanged;
            GameMemReader.getInstance().CrackDetected += OnCrackDetected;
            App.handler.OnReady += HandlerOnOnReady;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
            GameMemReader.getInstance().GameOver += OnGameOver;
            App.socket.OnConnected += SocketOnOnConnected;
            App.socket.OnDisconnected += SocketOnOnDisconnected;
            context.Players.CollectionChanged += PlayersOnCollectionChanged;

            IPCAdapter.getInstance().OnToken += (sender, token) => {
                this.BeginInvoke(w => {
                    if (!w.context.Settings.FocusOnToken) return;

                    if (w.WindowState.Equals(WindowState.Minimized)) w.WindowState = WindowState.Normal;

                    w.Show();
                    w.Activate();
                    w.Focus(); // important
                });
            };

            if (!context.Settings.discordTokenEncrypted) //Encrypt discord token if it is not encrypted.
            {
                context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(context.Settings.discordToken));
                context.Settings.discordTokenEncrypted = true;
            }

            var encryptedBuff = JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken);
            discordTokenBox.Password = decryptToken(encryptedBuff);
            if(context.Settings.language == "") {
                var cultures = Translator.Cultures;
                var ci = CultureInfo.CurrentUICulture;
                if(cultures.Any(x=>x.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)) {
                    Translator.Culture = CultureInfo.GetCultureInfo(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                }
            }
            else {
                Translator.Culture = CultureInfo.GetCultureInfo(context.Settings.language);
            }
            Translator.CurrentCultureChanged += TranslatorOnCurrentCultureChanged; 
            context.Players.CollectionChanged += PlayersOnCollectionChanged;
            
            //ApplyDarkMode();
        }

        private void OnCrackDetected(object? sender, EventArgs e) {
            var x = context.DialogCoordinator.ShowMessageAsync(context, "Crack detected", "We have detected that you are running an unsupported version of the game. This may or may not work.",MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Continue", NegativeButtonText = "Exit",
                    ColorScheme = MetroDialogColorScheme.Theme,
                    DefaultButtonFocus = MessageDialogResult.Negative
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            if (x == MessageDialogResult.Negative) {
                Environment.Exit(0);
            }
            else {
                GameMemReader.getInstance().cracked = false;
            }
            
        }

        private void TranslatorOnCurrentCultureChanged(object? sender, CultureChangedEventArgs e) {
            context.Settings.language = e.Culture.Name;
        }


        private void PlayersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            context.PlayerRows = (int) Math.Ceiling(Math.Sqrt(context.Players.Count));
            context.PlayerCols = (int) Math.Ceiling(context.Players.Count / Math.Ceiling(Math.Sqrt(context.Players.Count)));
            Trace.WriteLine(context.PlayerCols);
            Trace.WriteLine(context.PlayerRows);
        }

        private void OnPlayerCosmeticChanged(object? sender, PlayerCosmeticChangedEventArgs e) {
            if (context.Players.Any(x => x.Name == e.Name)) {
                var player = context.Players.First(x => x.Name == e.Name);
                Console.WriteLine("Cosmetic change " + JsonConvert.SerializeObject(e));
                Dispatcher.Invoke(() => {
                    player.HatID = e.HatId;
                    player.PantsID = e.SkinId;
                    player.PetID = e.PetId;
                });
            }
        }

        private void SocketOnOnDisconnected(object? sender, EventArgs e) {
            context.ConnectionStatuses.First(x => x.ConnectionName == "AutoMuteUs").Connected = false;
        }

        private void SocketOnOnConnected(object? sender, ClientSocket.ConnectedEventArgs e) {
            context.ConnectionStatuses.First(x => x.ConnectionName == "AutoMuteUs").Connected = true;
        }

        private void HandlerOnOnReady(object? sender, DiscordHandler.ReadyEventArgs e) {
            context.ConnectionStatuses.First(x => x.ConnectionName == "User bot").Connected = true;
        }


        private void OnProcessHook(object? sender, ProcessHookArgs e) {
            context.Connected = true;
            //context.ConnectionStatuses.First(x => x.ConnectionName == "Among us").Connected = true;
            ProcessMemory.getInstance().process.Exited += ProcessOnExited;
        }

        private void ProcessOnExited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                context.Connected = false;
                //context.ConnectionStatuses.First(x => x.ConnectionName == "Among us").Connected = false;
            });
            ProcessMemory.getInstance().process.Exited -= ProcessOnExited;
        }

        public bool VerifySignature(string pathToSig) {
            try {
                var AutoMuteUsPublicKeyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AUCapture_WPF.Resources.AutoMuteUs_PK.asc");
                using var pgp = new PGP();
                // Verify clear stream
                using var inputFileStream = new FileStream(pathToSig, FileMode.Open);
                return pgp.VerifyClearStream(inputFileStream, AutoMuteUsPublicKeyStream);
            }
            catch (Exception e) {
                return false;
            }
        }

        public bool VerifyHashFromSig(string pathToFile, string pathToSignature) //Does not care if the signature is correct or not
        {
            try {
                var HashInSig = File.ReadAllLines(pathToSignature).First(x => x.Length == 64); //First line with 64 characters in it
                using var sha256 = new SHA256Managed();
                using var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
                using var bs = new BufferedStream(fs);
                var hash = sha256.ComputeHash(bs);
                var CaptureHashSB = new StringBuilder(2 * hash.Length);
                foreach (var byt in hash) CaptureHashSB.AppendFormat("{0:X2}", byt);

                var CaptureHash = CaptureHashSB.ToString();
                Console.WriteLine($"Got SigHash: {HashInSig}, Downloaded Hash: {CaptureHash}");
                return string.Equals(HashInSig, CaptureHash, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception e) {
                return false;
            }
        }

        public async void ShowErrorBox(string errorMessage, string title = "ERROR") {
            var errorBox = await context.DialogCoordinator.ShowMessageAsync(context, title,
                errorMessage, MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings {
                    AffirmativeButtonText = Translate.Key("RetryText"),
                    NegativeButtonText = Translate.Key("CancelText"),
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false
                });
            if (errorBox == MessageDialogResult.Affirmative) await Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);
        }
        
        public async void Update() {
            var version = new Version();
            var latestVersion = new Version();
            context.AutoUpdaterEnabled = false;
            try {
                version = new Version(context.Version);
                latestVersion = new Version(context.LatestVersion);
            }
            catch (Exception e) { }


#if PUBLISH
            try
            {
                var PublicKey = Assembly.GetExecutingAssembly().GetManifestResourceStream("AUCapture_WPF.Resources.AutoMuteUs_PK.asc");
                context.AutoUpdaterEnabled = PublicKey is not null;
            }
            catch (Exception)
            {
                context.AutoUpdaterEnabled = false;
                return;
            }
            if(!context.AutoUpdaterEnabled)
            {
              return;
            }
          
            try
            {
                int maxStep = 6;
                if (latestVersion.CompareTo(version) > 0)
                {
                    if (Assembly.GetExecutingAssembly().GetManifestResourceNames().All(x => x != "AUCapture_WPF.Resources.AutoMuteUs_PK.asc"))
                    {
                        ShowErrorBox(String.Format(Translate.Key("PrivateKeyErrorMessage"), latestVersion.ToString(3)),Translate.Key("PrivateKeyErrorMessage"));
                        return;
                    }
                    var selection = await context.DialogCoordinator.ShowMessageAsync(context, Translate.Key("UpdateNotificationHeader"),
                        String.Format(Translate.Key("UpdateNotificationMessage"), version.ToString(3), latestVersion.ToString(3)),
                        MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                        {
                            AffirmativeButtonText = Translate.Key("UpdateNotificationUpdate"),
                            NegativeButtonText = Translate.Key("UpdateNotificationDecline"), DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    if (selection == MessageDialogResult.Affirmative)
                    {
                        var DownloadProgress =
                            await context.DialogCoordinator.ShowProgressAsync(context, $"Step 1/{maxStep} - Downloading", "Percent: 0% (0/0)", isCancelable: false, new MetroDialogSettings{AnimateHide = false});
                        DownloadProgress.Maximum = 100;
                        using (var client = new WebClient())
                        {
                            var downloadPath = Path.GetTempFileName();
                            client.DownloadProgressChanged += (sender, args) =>
                            {
                                DownloadProgress.SetProgress(args.ProgressPercentage);
                                DownloadProgress.SetMessage($"Percent: {args.ProgressPercentage}% ({args.BytesReceived.Bytes().Humanize("#.##")}/{args.TotalBytesToReceive.Bytes().Humanize("#.##")})");
                            };
                            client.DownloadFileCompleted += async (sender, args) =>
                            {
                                if (args.Error is not null)
                                {
                                    await DownloadProgress.CloseAsync();
                                }
                                else
                                {
                                    DownloadProgress.SetTitle($"Step 2/{maxStep} - Downloading signature");
                                    using var client2 = new WebClient();
                                    var downloadPathSignedHash = Path.GetTempFileName();
                                    client2.DownloadProgressChanged += (sender, args) =>
                                    {
                                        DownloadProgress.SetProgress(args.ProgressPercentage);
                                        DownloadProgress.SetMessage($"Percent: {args.ProgressPercentage}% ({args.BytesReceived.Bytes().Humanize("#.##")}/{args.TotalBytesToReceive.Bytes().Humanize("#.##")})");
                                    };
                                    client2.DownloadFileCompleted += async (sender, args) =>
                                    {
                                        if (args.Error is not null)
                                        {
                                            await DownloadProgress.CloseAsync();
                                            ShowErrorBox(args.Error.Message);
                                        }
                                        else
                                        {
                                            DownloadProgress.SetTitle($"Step 3/{maxStep} - Verifying signature");
                                            DownloadProgress.SetMessage("");
                                            DownloadProgress.SetIndeterminate();
                                            bool SignatureValid = VerifySignature(downloadPathSignedHash);
                                            if (!SignatureValid)
                                            {
                                                await DownloadProgress.CloseAsync();
                                                ShowErrorBox("File signature invalid. If you get this after retrying tell us on discord. It is potentially a security risk.");
                                                return;
                                            }

                                            DownloadProgress.SetTitle($"Step 4/{maxStep} - Verifying hash");
                                            DownloadProgress.SetMessage("");
                                            DownloadProgress.SetIndeterminate();
                                            bool HashValid = VerifyHashFromSig(downloadPath, downloadPathSignedHash);
                                            if (!HashValid)
                                            {
                                                await DownloadProgress.CloseAsync();
                                                ShowErrorBox("Capture hash invalid. If you get this after retrying tell us on discord. It is potentially a security risk.");
                                                return;
                                            }

                                            DownloadProgress.SetTitle($"Step 5/{maxStep} - Extracting");
                                            DownloadProgress.SetMessage("Please wait, we may go unresponsive but don't close the window, we will restart the program after.");
                                            DownloadProgress.SetIndeterminate();
                                            if (!Directory.Exists(Path.Join(
                                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                "\\AmongUsCapture\\AmongUsGUI\\Update")))
                                            {
                                                Directory.CreateDirectory(Path.Join(
                                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                    "\\AmongUsCapture\\AmongUsGUI\\Update"));
                                            }

                                            using (ZipArchive archive = ZipFile.OpenRead(downloadPath))
                                            {
                                                try
                                                {
                                                    var entry = archive.Entries.First(x => x.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                                                    entry.ExtractToFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                        "\\AmongUsCapture\\AmongUsGUI\\Update", "AmongUsCapture.exe"), true);
                                                }
                                                catch (Exception e)
                                                {
                                                    var errorBox = await context.DialogCoordinator.ShowMessageAsync(context, "ERROR",
                                                        e.Message, MessageDialogStyle.AffirmativeAndNegative,
                                                        new MetroDialogSettings
                                                        {
                                                            AffirmativeButtonText = "retry",
                                                            NegativeButtonText = "cancel",
                                                            DefaultButtonFocus = MessageDialogResult.Affirmative
                                                        });
                                                    if (errorBox == MessageDialogResult.Affirmative)
                                                    {
                                                        await Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);
                                                    }
                                                }
                                            }


                                            //You can't delete a running application. But you can rename it.
                                            string appFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                                            string appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
                                            string appExtension = Path.GetExtension(Process.GetCurrentProcess().MainModule.FileName);
                                            string archivePath = Path.Combine(appFolder, appName + "_Old" + appExtension);

                                            DownloadProgress.SetTitle($"Step 6/{maxStep} - Copying files");
                                            DownloadProgress.SetMessage("Finishing up..");
                                            File.Move(Process.GetCurrentProcess().MainModule.FileName, archivePath);

                                            File.Move(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI\\Update", "AmongUsCapture.exe"),
                                                Path.Combine(appFolder, appName + appExtension), true);
                                            Application.Current.Invoke(() =>
                                            {
                                                IPCAdapter.getInstance().mutex.ReleaseMutex(); //Release the mutex so the other app does not see us. 
                                                Process.Start(Path.Combine(appFolder, appName + appExtension));
                                                Environment.Exit(0);
                                            });
                                        }
                                    };
                                    if (!string.IsNullOrEmpty(context.LatestReleaseAssetSignedHashURL))
                                    {
                                        var signatureDownloader = client2.DownloadFileTaskAsync(context.LatestReleaseAssetSignedHashURL, downloadPathSignedHash);
                                    }
                                    else
                                    {
                                        ShowErrorBox("Release does not have a signature. Not downloading, please retry later.");
                                    }
                                    
                                }
                            };
                            var downloaderClient = client.DownloadFileTaskAsync(context.LatestReleaseAssetURL, downloadPath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ShowErrorBox(e.Message);
            }

#endif
        }

        private string decryptToken(byte[] EncryptedBytes) {
            var protectedBytes = ProtectedData.Unprotect(EncryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(protectedBytes, 0, protectedBytes.Length);
        }

        private byte[] encryptToken(string token) {
            var buffer = Encoding.UTF8.GetBytes(token);
            var protectedBytes = ProtectedData.Protect(buffer, null, DataProtectionScope.CurrentUser);
            return protectedBytes;
        }

        private void OnGameOver(object? sender, GameOverEventArgs e) {
            Dispatcher.Invoke(() => {
                foreach (var player in context.Players) player.Alive = true;
            });
        }


        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e) {
            if (e.Action == PlayerAction.Died) {
                if (context.Players.Any(x => x.Name == e.Name)) DeadMessages.Enqueue(context.Players.First(x => x.Name == e.Name));
            }
            else {
                if (e.Action != PlayerAction.Joined && context.Players.Any(x => string.Equals(x.Name, e.Name, StringComparison.CurrentCultureIgnoreCase))) {
                    var player = context.Players.First(x => string.Equals(x.Name, e.Name, StringComparison.CurrentCultureIgnoreCase));
                    Dispatcher.Invoke(() => {
                        switch (e.Action) {
                            case PlayerAction.ChangedColor:
                                player.Color = e.Color;
                                break;

                            case PlayerAction.Disconnected:
                            case PlayerAction.Left:
                                context.Players.Remove(player);
                                break;

                            case PlayerAction.Exiled:
                            case PlayerAction.Died:
                                player.Alive = false;
                                break;
                        }
                    });
                }
                else {
                    if (e.Action == PlayerAction.Joined) Dispatcher.Invoke(() => { context.Players.Add(new Player(e.Name, e.Color, !e.IsDead, 0, 0, 0)); });
                }
            }
            Logger.Debug("{@e}", e);
        }




        private void OnJoinedLobby(object sender, LobbyEventArgs e) {
            context.GameCode = e.LobbyCode;
            context.GameMap = e.Map;
            this.BeginInvoke(a => {
                if (context.Settings.AlwaysCopyGameCode) Clipboard.SetText(e.LobbyCode);
            });
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor) {
            var OutputCode = Color.White;
            switch (pColor) {
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
                case PlayerColor.Maroon:
                    OutputCode = Color.Maroon;
                    break;
                case PlayerColor.Rose:
                    OutputCode = Color.MistyRose;
                    break;
                case PlayerColor.Banana:
                    OutputCode = Color.LemonChiffon;
                    break;
                case PlayerColor.Gray:
                    OutputCode = Color.Gray;
                    break;
                case PlayerColor.Tan:
                    OutputCode = Color.Tan;
                    break;
                case PlayerColor.Coral:
                    OutputCode = Color.LightCoral;
                    break;
            }

            return OutputCode;
        }

        private void SetDefaultThemeColor() {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;

            string BaseColor = ThemeManager.BaseColorDark;

            var newTheme2 = new Theme("CustomTheme",
                "CustomTheme",
                BaseColor,
                "CustomAccent",
                System.Windows.Media.Color.FromArgb(255,140,158,255),
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,140,158,255)),
                true,
                false);
            ThemeManager.Current.ChangeTheme(this, newTheme2);
        }

        public static void OpenBrowser(string url) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", url);
            }
        }

        private void ApplyDarkMode() {
            if (config.DarkMode) {
                context.BackgroundBrush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Misc/AutoBG.png")));
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorDark);
                NormalTextColor = Color.White;
            }
            else {
                context.BackgroundBrush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Misc/AutoBG.png")));
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorDark);
                NormalTextColor = Color.White;
            }
        }

        private void Settings(object sender, RoutedEventArgs e) {
            // Open up the settings flyout
            //Cracked();
            SettingsFlyout.IsOpen = true;
        }

        private void Darkmode_Toggled(object sender, RoutedEventArgs e) {
            if (!(sender is ToggleSwitch toggleSwitch)) return;

            ApplyDarkMode();
        }

        private void ManualConnect_Click(object sender, RoutedEventArgs e) {
            //Open up the manual connection flyout.
            ManualConnectionFlyout.IsOpen = true;
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e) {
            setCurrentState(e.NewState);
            while (DeadMessages.Count > 0) {
                var playerToKill = DeadMessages.Dequeue();
                if (context.Players.Contains(playerToKill)) playerToKill.Alive = false;
            }
            Logger.Info("State change: {@e}", e);
            if (e.NewState == GameState.MENU) {
                setGameCode("");
                Dispatcher.Invoke(() => {
                    context.GameState = e.NewState;
                    foreach (var player in context.Players) player.Alive = true;
                });
            }
            else if (e.NewState == GameState.LOBBY) {
                Dispatcher.Invoke(() => {
                    context.GameState = e.NewState;
                    foreach (var player in context.Players) player.Alive = true;
                });
            }

            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }

        public void setGameCode(string gamecode) {
            context.GameCode = gamecode;
        }

        public void setCurrentState(GameState state) {
            context.GameState = state;
        }

        private void RandomizePlayers() {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            var r = new Random();
            var playerToChange = context.Players[r.Next(context.Players.Count)];
            var hatID = r.Next(94);
            var Alive = r.Next(0, 2) == 1;
            var pantId = r.Next(0, 16);
            var petID = r.Next(0, 12);
            playerToChange.Alive = Alive;
            if (!Alive) {
                playerToChange.HatID = (uint) hatID;
                playerToChange.PantsID = (uint) pantId;
                playerToChange.PetID = (uint) petID;
            }

            

        }

        private void TestUsers() {
            context.Connected = true;
            context.GameState = GameState.TASKS;
            var numOfPlayers = 14;
            for (uint i = 0; i < numOfPlayers; i++) context.Players.Add(new Player($"{i}Cool4u", (PlayerColor) (i % 12), true, i % 10, i, 0));

            RandomizePlayers();
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);

            //TestUsers();
            if (Updated) this.ShowMessageAsync("Update successful!", "The update was successful. Happy auto-muting");
        }

        public void PlayGotEm() {
            this.BeginInvoke(win => {
                //win.MemeFlyout.IsOpen = true;
                //win.MemePlayer.Position = TimeSpan.Zero;
            });
        }

        private void MainWindow_OnContentRendered(object? sender, EventArgs e) {
            //TestFillConsole(10);
            //setCurrentState("GAMESTATE");
            //setGameCode("GAMECODE");
            SetDefaultThemeColor();

            ApplyDarkMode();
            var encryptedBuff = JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken);
            if (decryptToken(encryptedBuff) != "")
                App.handler.Init(decryptToken(encryptedBuff));
            else
                Logger.Info("No discord token set");
            if (!config.startupMemes) {
                Logger.Info("Meme Module disabled :(");
            }
        }

        private void SubmitConnectButton_OnClick(object sender, RoutedEventArgs e) {
            IPCAdapter.getInstance().SendToken(config.host, config.connectCode);
            ManualConnectionFlyout.IsOpen = false;
        }

        private void MemePlayer_OnMediaEnded(object sender, RoutedEventArgs e) {
            this.BeginInvoke(win => {
                //win.MemeFlyout.IsOpen = false;
            });
        }

        //private void MemeFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e)
        //{
        //if (MemeFlyout.IsOpen)
        //{
        //    MemePlayer.Play();
        //    Task.Factory.StartNew(() =>
        //   {
        //       Thread.Sleep(5000);
        //        MemeFlyout.Invoke(new Action(() =>
        //        {
        //            if (MemeFlyout.IsOpen)
        //             {
        //                 MemeFlyout.CloseButtonVisibility = Visibility.Visible;
        //             }
        //        }));
        //
        //      });
        // }
        // else
        // {
        //    MemeFlyout.CloseButtonVisibility = Visibility.Hidden;
        //    MemePlayer.Close();
        //    GC.Collect();
        // }
        //}
        private async void SubmitDiscordButton_OnClick(object sender, RoutedEventArgs e) {
            if (discordTokenBox.Password != "") {
                var progressController = await context.DialogCoordinator.ShowProgressAsync(context, "Token Validation", "Validating discord token", false,
                    new MetroDialogSettings {AnimateShow = true, AnimateHide = false, NegativeButtonText = "OK"});
                progressController.SetIndeterminate();
                try {
                    TokenUtils.ValidateToken(TokenType.Bot, discordTokenBox.Password);
                    progressController.SetMessage("Token validated.");
                    context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                    App.handler.Close(); //Anytime we change the token we wanna close the connection. (Will not error if connection already closed)
                    App.handler.Init(
                        decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)));
                    progressController.SetProgress(1);
                }
                catch (ArgumentException er) {
                    progressController.SetMessage(er.Message);
                    progressController.SetProgress(0);
                    discordTokenBox.Password = decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)); //Roll back changes
                }

                progressController.SetCancelable(true);
                progressController.Canceled += delegate {
                    progressController.CloseAsync(); //Close the dialog. 
                };
            }
            else if (discordTokenBox.Password == string.Empty) {
                if (context.Settings.discordToken == "") //If we don't have any password in the config(meaning unencrypted)
                {
                    context.Settings.discordTokenEncrypted = true;
                    context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                }

                if (decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)) == discordTokenBox.Password) return;
                //No reason to open the box if it didn't change.
                context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                App.handler.Close(); //Close connection because token cleared.
                await this.ShowMessageAsync("Success!", "Discord token cleared!");
            }
        }

        private async void ReloadOffsetsButton_OnClick(object sender, RoutedEventArgs e) {
            GameMemReader.getInstance().offMan.refreshLocal();
            await GameMemReader.getInstance().offMan.RefreshIndex();
            GameMemReader.getInstance().CurrentOffsets = GameMemReader.getInstance().offMan
                .FetchForHash(GameMemReader.getInstance().GameHash);
            if (GameMemReader.getInstance().CurrentOffsets is not null) {
                //WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"Loaded offsets: {GameMemReader.getInstance().CurrentOffsets.Description}");
            }
        }

        private void HelpDiscordButton_OnClick(object sender, RoutedEventArgs e) {
            OpenBrowser("https://www.youtube.com/watch?v=jKcEW5qpk8E");
        }

        private void APIServerToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (!(sender is ToggleSwitch toggleSwitch)) return;

            if (config.ApiServer) {
                Logger.Info("API server starting");
                ServerSocket.instance.Start();
            }
            else {
                Logger.Info("API server stopping");
                ServerSocket.instance.Stop();
            }
        }

        private async void ResetConfigButton_OnClick(object sender, RoutedEventArgs e) {
            var result = await this.ShowMessageAsync("Are you sure?",
                "This action will reset your config.\nThis cannot be undone.",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AnimateShow = true, AnimateHide = false});
            if (result == MessageDialogResult.Affirmative) {
                var progressBar = await context.DialogCoordinator.ShowProgressAsync(context, "Resetting config",
                    "Please wait....", false, new MetroDialogSettings {AnimateHide = false, AnimateShow = false});
                progressBar.Minimum = 0;
                progressBar.Maximum = 1;
                if (File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "\\AmongUsCapture\\AmongUsGUI", "Settings.json")))
                    File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI", "Settings.json"));

                if (File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AmongUsCapture", "Settings.json")))
                    File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture", "Settings.json"));

                for (var i = 0; i < 100; i++) //Useless loading to make the user think we are doing a big task
                {
                    var currentPercent = i / 100d;
                    progressBar.SetProgress(currentPercent);
                    await Task.Delay(10);
                }

                await progressBar.CloseAsync();
                var selection = await this.ShowMessageAsync("Config reset",
                    "Your config was reset successfully.",
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AnimateHide = true, AffirmativeButtonText = "Restart", NegativeButtonText = "Exit"});
                if (selection == MessageDialogResult.Affirmative) {
                    IPCAdapter.getInstance().mutex.ReleaseMutex(); //Release the mutex so the other app does not see us. 
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown(0);
                }
                else {
                    Application.Current.Shutdown(0);
                }
            }
        }

        private void AlwaysOnTopSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            Window.Topmost = context.Settings.alwaysOnTop;
        }

        private void ContributorsButton_OnClick(object sender, RoutedEventArgs e)
        {
            Contributors c = new Contributors(context.Settings.DarkMode);
            c.Show();
        }

        private void PremiumButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://automute.us/premium");
        }

        private void OpenLogsFolderButton_OnClick(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(App.LogFolder)) return;
            Process.Start(new ProcessStartInfo(App.LogFolder) {UseShellExecute = true});
        }
    }
}