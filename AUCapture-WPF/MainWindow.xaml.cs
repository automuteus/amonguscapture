using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
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
using System.Windows.Threading;
using AUCapture_WPF.Models;
using Discord;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using Humanizer;
using PgpCore;
using Microsoft.Win32;
using Color = System.Drawing.Color;
using PlayerColor = AmongUsCapture.PlayerColor;

namespace AUCapture_WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Color NormalTextColor = Color.White;

        private readonly IAppSettings config;

        public UserDataContext context;
        private readonly bool connected;
        private readonly object locker = new object();
        private readonly Queue<Player> DeadMessages = new Queue<Player>();
        private Task ThemeGeneration;
        private bool Updated = false;

        public MainWindow()
        {
            InitializeComponent();
            string appFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            string appExtension = Path.GetExtension(Process.GetCurrentProcess().MainModule.FileName);
            string archivePath = Path.Combine(appFolder, appName + "_Old" + appExtension);
            if (File.Exists(archivePath))
            {
                Updated = true;
                try
                {
                    //Will wait for the other program to exit.
                    var me = Process.GetCurrentProcess();
                    Process[] aProcs = Process.GetProcessesByName(me.ProcessName);
                    aProcs = aProcs.Where(x => x.Id != me.Id).ToArray();
                    if (aProcs != null && aProcs.Length > 0)
                    {
                        aProcs[0].WaitForExit(1000);
                    }

                    File.Delete(archivePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not delete old file.");
                }
            }
            else
            {
                Updated = false;
            }

            try
            {
                config = new ConfigurationBuilder<IAppSettings>()
                    .UseJsonFile(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
            }
            catch (Newtonsoft.Json.JsonReaderException e) //Delete file and recreate config
            {
                Console.WriteLine($"Bad config. Clearing.");
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
            window.Topmost = context.Settings.alwaysOnTop;
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().ProcessHook += OnProcessHook;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().PlayerCosmeticChanged += OnPlayerCosmeticChanged;
            App.handler.OnReady += HandlerOnOnReady;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
            GameMemReader.getInstance().GameOver += OnGameOver;
            App.socket.OnConnected += SocketOnOnConnected;
            App.socket.OnDisconnected += SocketOnOnDisconnected;
            context.Players.CollectionChanged += PlayersOnCollectionChanged;
            
            IPCAdapter.getInstance().OnToken += (sender, token) =>
            {
                this.BeginInvoke((w) =>
                {
                    if (!w.context.Settings.FocusOnToken)
                    {
                        return;
                    }

                    if (w.WindowState.Equals(WindowState.Minimized))
                    {
                        w.WindowState = WindowState.Normal;
                    }

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

            byte[] encryptedBuff = JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken);
            discordTokenBox.Password = decryptToken(encryptedBuff);


            System.Windows.Media.Color savedColor;
            try
            {
                savedColor = JsonConvert.DeserializeObject<System.Windows.Media.Color>(context.Settings.SelectedAccent);
                AccentColorPicker.SelectedColor = savedColor;
                string BaseColor;
                if (context.Settings.DarkMode)
                {
                    BaseColor = ThemeManager.BaseColorDark;
                }
                else
                {
                    BaseColor = ThemeManager.BaseColorLight;
                }

                Theme newTheme = new Theme(name: "CustomTheme",
                    displayName: "CustomTheme",
                    baseColorScheme: BaseColor,
                    colorScheme: "CustomAccent",
                    primaryAccentColor: savedColor,
                    showcaseBrush: new SolidColorBrush(savedColor),
                    isRuntimeGenerated: true,
                    isHighContrast: false);

                ThemeManager.Current.ChangeTheme(this, newTheme);
            }
            catch (Exception e)
            {
            }
            context.Players.CollectionChanged += PlayersOnCollectionChanged;
            //ApplyDarkMode();
        }


        private void PlayersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            context.PlayerRows = (int)Math.Ceiling(Math.Sqrt(context.Players.Count));
            context.PlayerCols = (int)Math.Ceiling(context.Players.Count/Math.Ceiling(Math.Sqrt(context.Players.Count)));
            System.Diagnostics.Trace.WriteLine(context.PlayerCols);
            System.Diagnostics.Trace.WriteLine(context.PlayerRows);
        }

        private void OnPlayerCosmeticChanged(object? sender, PlayerCosmeticChangedEventArgs e)
        {
            if (context.Players.Any(x => x.Name == e.Name))
            {
                var player = context.Players.First(x => x.Name == e.Name);
                Console.WriteLine("Cosmetic change " + JsonConvert.SerializeObject(e));
                Dispatcher.Invoke((Action) (() =>
                {
                    player.HatID = e.HatId;
                    player.PantsID = e.SkinId;
                    player.PetID = e.PetId;
                }));
            }
        }

        private void SocketOnOnDisconnected(object? sender, EventArgs e)
        {
            context.ConnectionStatuses.First(x => x.ConnectionName == "AutoMuteUs").Connected = false;
        }

        private void SocketOnOnConnected(object? sender, ClientSocket.ConnectedEventArgs e)
        {
            context.ConnectionStatuses.First(x => x.ConnectionName == "AutoMuteUs").Connected = true;
        }

        private void HandlerOnOnReady(object? sender, DiscordHandler.ReadyEventArgs e)
        {
            context.ConnectionStatuses.First(x => x.ConnectionName == "User bot").Connected = true;
        }


        private void OnProcessHook(object? sender, ProcessHookArgs e)
        {
            context.Connected = true;
            //context.ConnectionStatuses.First(x => x.ConnectionName == "Among us").Connected = true;
            ProcessMemory.getInstance().process.Exited += ProcessOnExited;
        }

        private void ProcessOnExited(object? sender, EventArgs e)
        {
            Dispatcher.Invoke((Action) (() =>
            {
                context.Connected = false;
                //context.ConnectionStatuses.First(x => x.ConnectionName == "Among us").Connected = false;
            }));
            ProcessMemory.getInstance().process.Exited -= ProcessOnExited;
        }

        public bool VerifySignature(string pathToSig)
        {
            try
            {
                Stream AutoMuteUsPublicKeyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AUCapture_WPF.Resources.AutoMuteUs_PK.asc");
                using PGP pgp = new PGP();
                // Verify clear stream
                using FileStream inputFileStream = new FileStream(pathToSig, FileMode.Open);
                return pgp.VerifyClearStream(inputFileStream, AutoMuteUsPublicKeyStream);
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        public bool VerifyHashFromSig(string pathToFile, string pathToSignature) //Does not care if the signature is correct or not
        {
            try
            {
                string HashInSig = File.ReadAllLines(pathToSignature).First(x => x.Length == 64); //First line with 64 characters in it
                using SHA256Managed sha256 = new SHA256Managed();
                using FileStream fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
                using var bs = new BufferedStream(fs);
                var hash = sha256.ComputeHash(bs);
                StringBuilder CaptureHashSB = new StringBuilder(2 * hash.Length);
                foreach (byte byt in hash)
                {
                    CaptureHashSB.AppendFormat("{0:X2}", byt);
                }

                string CaptureHash = CaptureHashSB.ToString();
                Console.WriteLine($"Got SigHash: {HashInSig}, Downloaded Hash: {CaptureHash}");
                return String.Equals(HashInSig, CaptureHash, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async void ShowErrorBox(string errorMessage, string title="ERROR")
        {
            var errorBox = await context.DialogCoordinator.ShowMessageAsync(context, title,
                errorMessage, MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "retry",
                    NegativeButtonText = "cancel",
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AnimateShow = false
                });
            if (errorBox == MessageDialogResult.Affirmative)
            {
                await Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);
            }
        }

        public async void Update()
        {
            Version version = new Version();
            Version latestVersion = new Version();
            context.AutoUpdaterEnabled = false;
            try
            {
               version = new Version(context.Version);
               latestVersion = new Version(context.LatestVersion);
            }
            catch (Exception e)
            {
                return;
            }
            

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
                        ShowErrorBox($"We detected an update to {latestVersion}, But there is no public key in this capture so we will not be able to verify integrity. Please download the latest release off the github page.","AutoUpdater failed");
                        return;
                    }
                    var selection = await context.DialogCoordinator.ShowMessageAsync(context, "Caution",
                        $"We've detected you're using an older version of AmongUsCapture!\nYour version: {version}\nLatest version: {latestVersion}",
                        MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                        {
                            AffirmativeButtonText =
                                "Update",
                            NegativeButtonText = "No thanks", DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    if (selection == MessageDialogResult.Negative)
                    {
                        selection = await context.DialogCoordinator.ShowMessageAsync(context, "Warning",
                            $"Having an older version could cause compatibility issues with AutoMuteUs.\nWe can automagically update you to {latestVersion}.",
                            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                            {
                                AffirmativeButtonText =
                                    "Update",
                                NegativeButtonText = "no ty", DefaultButtonFocus = MessageDialogResult.Affirmative
                            });
                    }

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

        private string decryptToken(byte[] EncryptedBytes)
        {
            byte[] protectedBytes = ProtectedData.Unprotect(EncryptedBytes, null, DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(protectedBytes, 0, protectedBytes.Length);
        }

        private byte[] encryptToken(string token)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(token);
            byte[] protectedBytes = ProtectedData.Protect(buffer, null, DataProtectionScope.CurrentUser);
            return protectedBytes;
        }

        private void OnGameOver(object? sender, GameOverEventArgs e)
        {
            Dispatcher.Invoke((Action) (() =>
            {
                foreach (var player in context.Players)
                {
                    player.Alive = true;
                }
            }));
        }


        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            if (e.Action == PlayerAction.Died)
            {
                if (context.Players.Any(x => x.Name == e.Name))
                {
                    DeadMessages.Enqueue(context.Players.First(x => x.Name == e.Name));
                }
            }
            else
            {
                if (e.Action != PlayerAction.Joined && context.Players.Any(x => String.Equals(x.Name, e.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var player = context.Players.First(x => String.Equals(x.Name, e.Name, StringComparison.CurrentCultureIgnoreCase));
                    Dispatcher.Invoke((Action) (() =>
                    {
                        switch (e.Action)
                        {
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
                    }));
                }
                else
                {
                    if (e.Action == PlayerAction.Joined)
                    {
                        Dispatcher.Invoke((Action) (() => { context.Players.Add(new Player(e.Name, e.Color, !e.IsDead, 0, 0)); }));
                    }
                }
            }

            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }

        
        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
            //WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
        }


        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            context.GameCode = e.LobbyCode;
            context.GameMap = e.Map;
            this.BeginInvoke(a =>
            {
                if (context.Settings.AlwaysCopyGameCode)
                {
                    Clipboard.SetText(e.LobbyCode);
                }
            });
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            Color OutputCode = Color.White;
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

        private void SetDefaultThemeColor()
        {
            if (config.ranBefore)
            {
                return;
            }

            config.ranBefore = true;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            Theme newTheme = ThemeManager.Current.DetectTheme();
            config.DarkMode = newTheme.BaseColorScheme == ThemeManager.BaseColorDark;
            DarkMode_Toggleswitch.IsOn = config.DarkMode;
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
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
            if (!(sender is ToggleSwitch toggleSwitch))
            {
                return;
            }

            ApplyDarkMode();
        }

        private void ManualConnect_Click(object sender, RoutedEventArgs e)
        {
            //Open up the manual connection flyout.
            ManualConnectionFlyout.IsOpen = true;
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            setCurrentState(e.NewState);
            while (DeadMessages.Count > 0)
            {
                var playerToKill = DeadMessages.Dequeue();
                if (context.Players.Contains(playerToKill))
                {
                    playerToKill.Alive = false;
                }
            }

            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
            if (e.NewState == GameState.MENU)
            {
                setGameCode("");
                Dispatcher.Invoke((Action) (() =>
                {
                    context.GameState = e.NewState;
                    foreach (var player in context.Players)
                    {
                        player.Alive = true;
                    }
                }));
            }
            else if (e.NewState == GameState.LOBBY)
            {
                Dispatcher.Invoke((Action) (() =>
                {
                    context.GameState = e.NewState;
                    foreach (var player in context.Players)
                    {
                        player.Alive = true;
                    }
                }));
            }

            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }

        public void setGameCode(string gamecode)
        {
            context.GameCode = gamecode;
        }

        public void setCurrentState(GameState state)
        {
            context.GameState = state;
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Update, TaskCreationOptions.LongRunning);
            //context.Connected = true;
            //context.GameState = GameState.TASKS;
            //for (uint i = 0; i < 10; i++)
            //{
            //    context.Players.Add(new Player($"Test {i}", (PlayerColor) (i%12), true, i%10, i));
            //}
            
            if (Updated)
            {
                this.ShowMessageAsync("Update successful!", "The update was successful. Happy auto-muting",
                    MessageDialogStyle.Affirmative);
            }
        }

        public void PlayGotEm()
        {
            this.BeginInvoke((win) =>
            {
                //win.MemeFlyout.IsOpen = true;
                //win.MemePlayer.Position = TimeSpan.Zero;
            });
        }

        private void MainWindow_OnContentRendered(object? sender, EventArgs e)
        {
            //TestFillConsole(10);
            //setCurrentState("GAMESTATE");
            //setGameCode("GAMECODE");
            SetDefaultThemeColor();

            ApplyDarkMode();
            byte[] encryptedBuff = JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken);
            if (decryptToken(encryptedBuff) != "")
            {
                App.handler.Init(decryptToken(encryptedBuff));
            }
            else
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("Discord", Color.Red, "You do not have a self-host discord token set. Enabling this in settings will increase performance.");
            }
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
        private async void SubmitDiscordButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (discordTokenBox.Password != "")
            {
                var progressController = await context.DialogCoordinator.ShowProgressAsync(context, "Token Validation", "Validating discord token", false,
                    new MetroDialogSettings {AnimateShow = true, AnimateHide = false, NegativeButtonText = "OK"});
                progressController.SetIndeterminate();
                try
                {
                    Discord.TokenUtils.ValidateToken(TokenType.Bot, discordTokenBox.Password);
                    progressController.SetMessage("Token validated.");
                    context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                    App.handler.Close(); //Anytime we change the token we wanna close the connection. (Will not error if connection already closed)
                    App.handler.Init(
                        decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)));
                    progressController.SetProgress(1);
                }
                catch (ArgumentException er)
                {
                    progressController.SetMessage(er.Message);
                    progressController.SetProgress(0);
                    discordTokenBox.Password = decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)); //Roll back changes
                }

                progressController.SetCancelable(true);
                progressController.Canceled += delegate(object? o, EventArgs args)
                {
                    progressController.CloseAsync(); //Close the dialog. 
                };
            }
            else if (discordTokenBox.Password == string.Empty)
            {
                if (context.Settings.discordToken == "") //If we don't have any password in the config(meaning unencrypted)
                {
                    context.Settings.discordTokenEncrypted = true;
                    context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                }

                if (decryptToken(JsonConvert.DeserializeObject<byte[]>(context.Settings.discordToken)) == discordTokenBox.Password) return;
                //No reason to open the box if it didn't change.
                context.Settings.discordToken = JsonConvert.SerializeObject(encryptToken(discordTokenBox.Password));
                App.handler.Close(); //Close connection because token cleared.
                await this.ShowMessageAsync("Success!", "Discord token cleared!", MessageDialogStyle.Affirmative);
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
                //WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"Loaded offsets: {GameMemReader.getInstance().CurrentOffsets.Description}");
            }
            else
            {
                //WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"No offsets found for: {Color.Aqua.ToTextColor()}{GameMemReader.getInstance().GameHash.ToString()}.");
            }
        }

        private void HelpDiscordButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://www.youtube.com/watch?v=jKcEW5qpk8E");
        }

        private void APIServerToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch toggleSwitch))
            {
                return;
            }

            if (config.ApiServer)
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("APIServer", Color.Brown, "Starting server");
                ServerSocket.instance.Start();
            }
            else
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("APIServer", Color.Brown, "Stopping server");
                ServerSocket.instance.Stop();
            }
        }

        private async void AccentColorPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                string BaseColor;
                if (context.Settings.DarkMode)
                {
                    BaseColor = ThemeManager.BaseColorDark;
                }
                else
                {
                    BaseColor = ThemeManager.BaseColorLight;
                }

                context.Settings.SelectedAccent = JsonConvert.SerializeObject(e.NewValue.Value);

                Theme newTheme = new Theme(name: "CustomTheme",
                    displayName: "CustomTheme",
                    baseColorScheme: BaseColor,
                    colorScheme: "CustomAccent",
                    primaryAccentColor: e.NewValue.Value,
                    showcaseBrush: new SolidColorBrush(e.NewValue.Value),
                    isRuntimeGenerated: true,
                    isHighContrast: false);

                ThemeManager.Current.ChangeTheme(this, newTheme);
            }
        }


        private async void ResetConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Are you sure?",
                "This action will reset your config.\nThis cannot be undone.",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AnimateShow = true, AnimateHide = false});
            if (result == MessageDialogResult.Affirmative)
            {
                var progressBar = await this.context.DialogCoordinator.ShowProgressAsync(context, "Resetting config",
                    "Please wait....", false, new MetroDialogSettings {AnimateHide = false, AnimateShow = false});
                progressBar.Minimum = 0;
                progressBar.Maximum = 1;
                if (File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "\\AmongUsCapture\\AmongUsGUI", "Settings.json")))
                {
                    File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI", "Settings.json"));
                }

                if (File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AmongUsCapture", "Settings.json")))
                {
                    File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture", "Settings.json"));
                }

                for (int i = 0; i < 100; i++) //Useless loading to make the user think we are doing a big task
                {
                    var currentPercent = i / 100d;
                    progressBar.SetProgress(currentPercent);
                    await Task.Delay(10);
                }

                await progressBar.CloseAsync();
                var selection = await this.ShowMessageAsync("Config reset",
                    "Your config was reset successfully.",
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings {AnimateHide = true, AffirmativeButtonText = "Restart", NegativeButtonText = "Exit"});
                if (selection == MessageDialogResult.Affirmative)
                {
                    IPCAdapter.getInstance().mutex.ReleaseMutex(); //Release the mutex so the other app does not see us. 
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown(0);
                }
                else
                {
                    Application.Current.Shutdown(0);
                }
            }
        }

        private void AlwaysOnTopSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            window.Topmost = context.Settings.alwaysOnTop;
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

    }
}