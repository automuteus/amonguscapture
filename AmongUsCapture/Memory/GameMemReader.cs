using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AmongUsCapture.Memory.Structs;
using AmongUsCapture.TextColorLibrary;
using AUOffsetManager;
using Newtonsoft.Json;

namespace AmongUsCapture {
    public class GameMemReader {
        private static readonly GameMemReader instance = new();
        private readonly Dictionary<string, ImmutablePlayer> CachedPlayerInfos = new();

        private bool cracked;
        public GameOffsets CurrentOffsets;
        private bool exileCausesEnd;
        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        public string GameHash = "";

        private LobbyEventArgs latestLobbyEventArgs;

        private Dictionary<string, PlayerInfo> newPlayerInfos = new(10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        public OffsetManager offMan = new(Settings.PersistentSettings.IndexURL);

        private Dictionary<string, PlayerInfo> oldPlayerInfos = new(10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead

        private GameState oldState = GameState.UNKNOWN;
        private int prevChatBubsVersion;
        private bool shouldForceTransmitState;
        private bool shouldForceUpdatePlayers;
        private bool shouldReadLobby;
        private bool shouldTransmitLobby;

        private bool Attached => ProcessMemory.getInstance().IsHooked &&
                                 ProcessMemory.getInstance().process is not null &&
                                 !ProcessMemory.getInstance().process.HasExited;

        public static GameMemReader getInstance() {
            return instance;
        }
        public event EventHandler<ValidatorEventArgs> GameVersionUnverified;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;
        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;
        public event EventHandler<LobbyEventArgs> JoinedLobby;
        public event EventHandler<ProcessHookArgs> ProcessHook;
        public event EventHandler<ProcessHookArgs> ProcessUnHook;
        public event EventHandler<GameOverEventArgs> GameOver;
        public event EventHandler<PlayerCosmeticChangedEventArgs> PlayerCosmeticChanged;
        private GameState GetGameState(ProcessMemory memInstance) {
            GameState state;
            var meetingHud = memInstance.Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.MeetingHudPtr);
            var meetingHud_cachePtr = meetingHud == IntPtr.Zero ? 0 : memInstance.Read<uint>(meetingHud, CurrentOffsets.MeetingHudCachePtrOffsets);
            var meetingHudState = meetingHud_cachePtr == 0 ? 4 : memInstance.ReadWithDefault(meetingHud, 4, CurrentOffsets.MeetingHudStateOffsets); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
            var gameState = memInstance.Read<int>(GameAssemblyPtr, CurrentOffsets.GameStateOffsets); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = ENDED (during "defeat" or "victory" screen only)

            switch (gameState) {
                case 0:
                    state = GameState.MENU;
                    exileCausesEnd = false;
                    break;
                case 1:
                    state = GameState.LOBBY;
                    exileCausesEnd = false;
                    break;
                case 3:
                    state = GameState.ENDED;
                    exileCausesEnd = false;
                    break;
                default: {
                    if (exileCausesEnd)
                        state = GameState.LOBBY;
                    else if (meetingHudState < 4)
                        state = GameState.DISCUSSION;
                    else
                        state = GameState.TASKS;

                    break;
                }
            }

            return state;
        }
        private int GetPlayerCount(ProcessMemory memInstance) {
            var allPlayersPtr = memInstance.Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
            var playerCount = memInstance.Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
            return playerCount;
        }
        private IEnumerable<PlayerInfo> GetPlayers(ProcessMemory memInstance) {
            var allPlayersPtr = memInstance.Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
            var allPlayers = memInstance.Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
            var playerCount = GetPlayerCount(memInstance);
            var playerAddrPtr = allPlayers + CurrentOffsets.PlayerListPtr;
            var Players = new List<PlayerInfo>(playerCount);
            for (var i = 0; i < playerCount; i++) {
                var pi = CurrentOffsets.isEpic ? (PlayerInfo) memInstance.Read<EpicPlayerInfo>(playerAddrPtr, 0, 0) : memInstance.Read<SteamPlayerInfo>(playerAddrPtr, 0, 0);
                playerAddrPtr += CurrentOffsets.AddPlayerPtr;
                Players.Add(pi);
            }
            return Players;
        }

        private PlayerInfo? GetPlayerById(ProcessMemory memInstance, int id) {
            var player = GetPlayers(memInstance).FirstOrDefault(x => x.PlayerId == id);
            return player;
        }

        private PlayerInfo? GetExiledPlayer(ProcessMemory memInstance) {
            var exiledPlayerId = memInstance.ReadWithDefault<byte>(GameAssemblyPtr, 255, CurrentOffsets.ExiledPlayerIdOffsets);
            return GetPlayerById(memInstance, exiledPlayerId);
        }

        private GameOverReason GetGameOverReason(ProcessMemory memInstance) {
            var rawGameOverReason = memInstance.Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
            var gameOverReason = (GameOverReason) rawGameOverReason;
            return gameOverReason;
        }

        private ImmutablePlayer[] GetEndingPlayerInfos(ProcessMemory memInstance, Dictionary<string, ImmutablePlayer> PlayerInfos) {
            var OurPlayerInfos = new Dictionary<string, ImmutablePlayer>(PlayerInfos); //Copy the dict so we don't modify unintentionally

            var winningPlayersPtr = memInstance.Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.WinningPlayersPtrOffsets);
            var winningPlayers = memInstance.Read<IntPtr>(winningPlayersPtr, CurrentOffsets.WinningPlayersOffsets);
            var winningPlayerCount = memInstance.Read<int>(winningPlayersPtr, CurrentOffsets.WinningPlayerCountOffsets);

            var winnerAddrPtr = winningPlayers + CurrentOffsets.PlayerListPtr;

            for (var i = 0; i < winningPlayerCount; i++) {
                var wpi = CurrentOffsets.isEpic ? (WinningPlayerData) memInstance.Read<EpicWinningPlayerData>(winnerAddrPtr, 0, 0) : memInstance.Read<SteamWinningPlayerData>(winnerAddrPtr, 0, 0);
                winnerAddrPtr += CurrentOffsets.AddPlayerPtr;
                try {
                    OurPlayerInfos[wpi.GetPlayerName()].IsImpostor = wpi.IsImpostor;
                }
                catch (KeyNotFoundException e) {
                    Console.WriteLine($"Could not find player with name \"{wpi.GetPlayerName()}\" in CachedPlayerInfos. JSON: {JsonConvert.SerializeObject(CachedPlayerInfos, Formatting.Indented)}");
                }
            }

            var endingPlayerInfos = new ImmutablePlayer[OurPlayerInfos.Count];
            OurPlayerInfos.Values.CopyTo(endingPlayerInfos, 0);
            return endingPlayerInfos;
        }

        private void DetectPlayerChanges(ProcessMemory memInstance, Dictionary<string, PlayerInfo> nowPlayers, Dictionary<string, PlayerInfo> oldPlayers) {
            foreach (var player in nowPlayers.Values)
                if (!oldPlayers.ContainsKey(player.GetPlayerName())) // player wasn't here before, they just joined
                {
                    PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                        Action = PlayerAction.Joined,
                        Name = player.GetPlayerName(),
                        IsDead = player.GetIsDead(),
                        Disconnected = player.GetIsDisconnected(),
                        Color = player.GetPlayerColor()
                    });
                    PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs {
                        Name = player.GetPlayerName(),
                        HatId = player.HatId,
                        SkinId = player.SkinId,
                        PetId = player.PetId
                    });
                }
                else {
                    // player was here before, we have an old playerInfo to compare against
                    var oldPlayerInfo = oldPlayers[player.GetPlayerName()];
                    if (!oldPlayerInfo.GetIsDead() && player.GetIsDead()) // player just died
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                            Action = PlayerAction.Died,
                            Name = player.GetPlayerName(),
                            IsDead = player.GetIsDead(),
                            Disconnected = player.GetIsDisconnected(),
                            Color = player.GetPlayerColor()
                        });

                    if (oldPlayerInfo.ColorId != player.ColorId)
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                            Action = PlayerAction.ChangedColor,
                            Name = player.GetPlayerName(),
                            IsDead = player.GetIsDead(),
                            Disconnected = player.GetIsDisconnected(),
                            Color = player.GetPlayerColor()
                        });

                    if (!oldPlayerInfo.GetIsDisconnected() && player.GetIsDisconnected())
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                            Action = PlayerAction.Disconnected,
                            Name = player.GetPlayerName(),
                            IsDead = player.GetIsDead(),
                            Disconnected = player.GetIsDisconnected(),
                            Color = player.GetPlayerColor()
                        });

                    if (oldPlayerInfo.HatId != player.HatId || oldPlayerInfo.SkinId != player.SkinId || oldPlayerInfo.PetId != player.PetId)
                        PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs {
                            Name = player.GetPlayerName(),
                            HatId = player.HatId,
                            SkinId = player.SkinId,
                            PetId = player.PetId
                        });
                }

            foreach (var (playerName, pi) in oldPlayers)
                if (!newPlayerInfos.ContainsKey(playerName)) // player was here before, isn't now, so they left
                    PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                        Action = PlayerAction.Left,
                        Name = playerName,
                        IsDead = pi.GetIsDead(),
                        Disconnected = pi.GetIsDisconnected(),
                        Color = pi.GetPlayerColor()
                    });
        }

        private string? GetGameCode(ProcessMemory memInstance) {
            var gameCode = memInstance.ReadString(ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.GameCodeOffsets), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
            if (string.IsNullOrEmpty(gameCode)) return null;
            var split = gameCode.Split("\n");
            return split.Length == 2 ? split[1] : null;
        }

        private PlayRegion GetPlayRegion(ProcessMemory memInstance) {
            return (PlayRegion) ((4 - (memInstance.Read<int>(GameAssemblyPtr, CurrentOffsets.PlayRegionOffsets) & 0b11)) % 3); // do NOT ask
        }

        private PlayMap GetMap(ProcessMemory memInstance) {
            return (PlayMap) memInstance.Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);
        }

        private string GetSha256Hash(string path) {
            using var sha256 = new SHA256Managed();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var bs = new BufferedStream(fs);
            var hash = sha256.ComputeHash(bs);
            var GameAssemblyhashSb = new StringBuilder(2 * hash.Length);
            foreach (var byt in hash) GameAssemblyhashSb.AppendFormat("{0:X2}", byt);

            return GameAssemblyhashSb.ToString();
        }

        public void RunLoop() {
            while (true) {
                try {
                    #region Game attaching
                    if (!Attached) {
                        var foundModule = false;
                        while (!foundModule) {
                            if (!ProcessMemory.getInstance().HookProcess("Among Us")) {
                                Thread.Sleep(1000);
                                continue;
                            }

                            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"Connected to Among Us process ({Color.Red.ToTextColor()}{ProcessMemory.getInstance().process.Id}{Settings.conInterface.getNormalColor().ToTextColor()})");

                            foreach (var module in ProcessMemory.getInstance().modules.Where(module => module.Name.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))) {
                                GameAssemblyPtr = module.BaseAddress;
                                if (!GameVerifier.VerifySteamHash(module.FileName)) {
                                    cracked = true;
                                    Settings.conInterface.WriteModuleTextColored("GameVerifier", Color.Red, $"Client verification: {Color.Red.ToTextColor()}FAIL{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                }
                                else {
                                    cracked = false;
                                    Settings.conInterface.WriteModuleTextColored("GameVerifier", Color.Red, $"Client verification: {Color.Lime.ToTextColor()}PASS{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                }

                                try {
                                    GameHash = GetSha256Hash(module.FileName);
                                    Console.WriteLine($"GameAssembly Hash: {GameHash}");
                                }
                                catch (Exception e) {
                                    cracked = false;
                                    GameHash = "windows_store";
                                }

                                CurrentOffsets = offMan.FetchForHash(GameHash);
                                if (CurrentOffsets is not null) {
                                    Settings.conInterface.WriteModuleTextColored("GameMemReader",
                                        Color.Lime, $"Loaded offsets: {CurrentOffsets.Description}");
                                    ProcessHook?.Invoke(this, new ProcessHookArgs {PID = ProcessMemory.getInstance().process.Id});
                                }
                                else {
                                    Settings.conInterface.WriteModuleTextColored("GameMemReader",
                                        Color.Lime,
                                        $"No offsets found for: {Color.Aqua.ToTextColor()}{GameHash}{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                }

                                foundModule = true;
                                break;
                            }

                            if (foundModule) continue;
                            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, "Still looking for modules...");
                            Thread.Sleep(500); // delay and try again
                            ProcessMemory.getInstance().LoadModules();
                        }

                        if (CurrentOffsets is null) Settings.conInterface.WriteModuleTextColored("ERROR", Color.Red, "Outdated version of the game.");
                    }

                    if (cracked && ProcessMemory.getInstance().IsHooked) {
                        var result = Settings.conInterface.CrackDetected();
                        if (!result)
                            Environment.Exit(0);
                        else
                            cracked = false;
                        continue;
                    }

                    if (CurrentOffsets is null) continue;
                    

                    #endregion
                    var state = GetGameState(ProcessMemory.getInstance());
                    #region Check if exile causes game end

                    if (oldState == GameState.DISCUSSION && state == GameState.TASKS) {
                        int impostorCount = 0, innocentCount = 0;
                        var ExiledPlayer = GetExiledPlayer(ProcessMemory.getInstance());
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                            Action = PlayerAction.Exiled,
                            Name = ExiledPlayer.GetPlayerName(),
                            IsDead = ExiledPlayer.GetIsDead(),
                            Disconnected = ExiledPlayer.GetIsDisconnected(),
                            Color = ExiledPlayer.GetPlayerColor()
                        });
                        impostorCount = GetPlayers(ProcessMemory.getInstance()).Count(x => x.GetIsImposter() && x.PlayerName != IntPtr.Zero && x.PlayerId != ExiledPlayer.PlayerId && !x.GetIsDead() && !x.GetIsDisconnected());
                        innocentCount = GetPlayers(ProcessMemory.getInstance()).Count(x => !x.GetIsImposter() && x.PlayerName != IntPtr.Zero && x.PlayerId != ExiledPlayer.PlayerId && !x.GetIsDead() && !x.GetIsDisconnected());

                        if (impostorCount == 0 || impostorCount >= innocentCount) {
                            exileCausesEnd = true;
                            state = GameState.LOBBY;
                        }
                    }

                    #endregion

                    #region State change checking

                    if (state != oldState || shouldForceTransmitState) {
                        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs {NewState = state});
                        shouldForceTransmitState = false;
                    }

                    #endregion

                    #region Lobby Reading

                    if (state != oldState && state == GameState.LOBBY || shouldReadLobby) {
                        var gameCode = GetGameCode(ProcessMemory.getInstance());
                        if (!string.IsNullOrEmpty(gameCode)) {
                            latestLobbyEventArgs = new LobbyEventArgs {
                                LobbyCode = gameCode,
                                Region = GetPlayRegion(ProcessMemory.getInstance()),
                                Map = GetMap(ProcessMemory.getInstance())
                            };
                            shouldReadLobby = false;
                            shouldTransmitLobby = true; // since this is probably new info
                        }
                        else {
                            shouldReadLobby = true; //We got a blank game code last time, so lets try again next time
                        }
                    }

                    #endregion

                    #region Lobby transmitting

                    if (shouldTransmitLobby) {
                        if (latestLobbyEventArgs != null) JoinedLobby?.Invoke(this, latestLobbyEventArgs);

                        shouldTransmitLobby = false;
                    }

                    #endregion

                    #region Game ended processing

                    if (oldState == GameState.ENDED && (state == GameState.LOBBY || state == GameState.MENU)) // game ended
                    {
                        var gameOverReason = GetGameOverReason(ProcessMemory.getInstance());

                        var humansWon = gameOverReason == GameOverReason.HumansByTask ||
                                        gameOverReason == GameOverReason.ImpostorDisconnect ||
                                        gameOverReason == GameOverReason.HumansByVote;
                        if (humansWon) // we will be reading humans data, so set all to simps
                            foreach (var playerName in CachedPlayerInfos.Keys)
                                try {
                                    CachedPlayerInfos[playerName].IsImpostor = true;
                                }
                                catch (KeyNotFoundException e) {
                                    Console.WriteLine($"Could not find User: \"{playerName}\" in CachedPlayerinfos");
                                }

                        GameOver?.Invoke(this, new GameOverEventArgs {
                            GameOverReason = gameOverReason,
                            PlayerInfos = GetEndingPlayerInfos(ProcessMemory.getInstance(), CachedPlayerInfos)
                        });
                    }

                    #endregion

                    #region Read player information

                    newPlayerInfos.Clear();
                    newPlayerInfos = GetPlayers(ProcessMemory.getInstance()).ToDictionary(x => x.GetPlayerName(), x => x);
                    DetectPlayerChanges(ProcessMemory.getInstance(), newPlayerInfos, oldPlayerInfos);

                    #endregion

                    #region Resend all players if requested

                    if (shouldForceUpdatePlayers) {
                        foreach (var player in newPlayerInfos.Values)
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs {
                                Action = PlayerAction.ForceUpdated,
                                Name = player.GetPlayerName(),
                                IsDead = player.GetIsDead(),
                                Disconnected = player.GetIsDisconnected(),
                                Color = player.GetPlayerColor()
                            });

                        shouldForceUpdatePlayers = false;
                    }

                    #endregion

                    #region Cache Immutable players for winning handling

                    if (state != oldState && (state == GameState.DISCUSSION || state == GameState.TASKS)) // game started, or at least we're still in game
                    {
                        CachedPlayerInfos.Clear();
                        foreach (var (playerName, pi) in newPlayerInfos)
                            CachedPlayerInfos[playerName] = new ImmutablePlayer {
                                Name = playerName,
                                IsImpostor = false
                            };
                    }

                    #endregion

                    #region Cache previous states to prime the state machine

                    oldPlayerInfos = new Dictionary<string, PlayerInfo>(newPlayerInfos);
                    oldState = state;

                    #endregion

                    Thread.Sleep(250);
                } catch (Exception e) {
                    Settings.conInterface.WriteModuleTextColored("ERROR", Color.Red, $"Message: {e.Message} | stack: {e.StackTrace} | Retrying in 1000ms.");
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            }
        }

        public void ForceTransmitLobby() {
            shouldTransmitLobby = true;
        }

        public void ForceUpdatePlayers() {
            shouldForceUpdatePlayers = true;
        }

        public void ForceTransmitState() {
            shouldForceTransmitState = true;
        }
    }

    public class GameStateChangedEventArgs : EventArgs {
        public GameState NewState { get; set; }
    }

    public enum PlayerAction {
        Joined,
        Left,
        Died,
        ChangedColor,
        ForceUpdated,
        Disconnected,
        Exiled
    }

    public enum PlayerColor {
        Red = 0,
        Blue = 1,
        Green = 2,
        Pink = 3,
        Orange = 4,
        Yellow = 5,
        Black = 6,
        White = 7,
        Purple = 8,
        Brown = 9,
        Cyan = 10,
        Lime = 11
    }

    public enum PlayRegion {
        NorthAmerica = 0,
        Asia = 1,
        Europe = 2
    }

    public enum PlayMap {
        Skeld = 0,
        Mira = 1,
        Polus = 2
    }

    public enum GameState {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU,
        ENDED,
        UNKNOWN
    }

    public class PlayerCosmeticChangedEventArgs : EventArgs {
        public string Name { get; set; }
        public uint HatId { get; set; }
        public uint SkinId { get; set; }
        public uint PetId { get; set; }
    }

    public class PlayerChangedEventArgs : EventArgs {
        public PlayerAction Action { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool Disconnected { get; set; }
        public PlayerColor Color { get; set; }
    }

    public class ChatMessageEventArgs : EventArgs {
        public string Sender { get; set; }
        public PlayerColor Color { get; set; }
        public string Message { get; set; }
    }

    public class LobbyEventArgs : EventArgs {
        public string LobbyCode { get; set; }
        public PlayRegion Region { get; set; }
        public PlayMap Map { get; set; }
    }

    public class GameOverEventArgs : EventArgs {
        public GameOverReason GameOverReason { get; set; }
        public ImmutablePlayer[] PlayerInfos { get; set; }
    }

    public class ProcessHookArgs : EventArgs {
        public int PID { get; set; }
    }
}