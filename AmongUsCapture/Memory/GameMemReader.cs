﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AmongUsCapture.Memory.Structs;
using AmongUsCapture.TextColorLibrary;
using AUOffsetManager;
using Newtonsoft.Json;

namespace AmongUsCapture
{
    public enum GameState
    {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU,
        ENDED,
        UNKNOWN
    }

    public class GameMemReader
    {
        private static readonly GameMemReader instance = new GameMemReader();
        private bool exileCausesEnd;

        private bool shouldReadLobby = false;
        private IntPtr GameAssemblyPtr = IntPtr.Zero;

        public Dictionary<string, PlayerInfo>
            newPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private LobbyEventArgs latestLobbyEventArgs = null;

        public Dictionary<string, PlayerInfo>
            oldPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead

        private Dictionary<string, ImmutablePlayer> CachedPlayerInfos = new Dictionary<string, ImmutablePlayer>();

        private GameState oldState = GameState.UNKNOWN;

        private int prevChatBubsVersion;
        private bool shouldForceTransmitState;
        private bool shouldForceUpdatePlayers;
        private bool shouldTransmitLobby;

        public static GameMemReader getInstance()
        {
            return instance;
        }

        public OffsetManager offMan = new OffsetManager(Settings.PersistentSettings.IndexURL);
        public GameOffsets CurrentOffsets;
        public string GameHash = "";

        public event EventHandler<ValidatorEventArgs> GameVersionUnverified;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;

        public event EventHandler<LobbyEventArgs> JoinedLobby;
        public event EventHandler<ProcessHookArgs> ProcessHook;
        public event EventHandler<ProcessHookArgs> ProcessUnHook;
        public event EventHandler<GameOverEventArgs> GameOver;

        public event EventHandler<PlayerCosmeticChangedEventArgs> PlayerCosmeticChanged;


        private bool cracked = false;

        public void RunLoop()
        {
            while (true)
            {
                try
                {
                    if (!ProcessMemory.getInstance().IsHooked || ProcessMemory.getInstance().process is null ||
                        ProcessMemory.getInstance().process.HasExited)
                    {
                        if (!ProcessMemory.getInstance().HookProcess("Among Us"))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                            $"Connected to Among Us process ({Color.Red.ToTextColor()}{ProcessMemory.getInstance().process.Id}{Settings.conInterface.getNormalColor().ToTextColor()})");


                        var foundModule = false;

                        while (true)
                        {
                            foreach (var module in ProcessMemory.getInstance().modules)
                                if (module.Name.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                                {
                                    GameAssemblyPtr = module.BaseAddress;
                                    if (!GameVerifier.VerifySteamHash(module.FileName))
                                    {
                                        cracked = true;
                                        Settings.conInterface.WriteModuleTextColored("GameVerifier", Color.Red,
                                            $"Client verification: {Color.Red.ToTextColor()}FAIL{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                    }
                                    else
                                    {
                                        cracked = false;
                                        Settings.conInterface.WriteModuleTextColored("GameVerifier", Color.Red,
                                            $"Client verification: {Color.Lime.ToTextColor()}PASS{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                    }

                                    using (SHA256Managed sha256 = new SHA256Managed())
                                    {
                                        using (FileStream fs = new FileStream(module.FileName, FileMode.Open,
                                            FileAccess.Read))
                                        {
                                            using (var bs = new BufferedStream(fs))
                                            {
                                                var hash = sha256.ComputeHash(bs);
                                                StringBuilder GameAssemblyhashSb = new StringBuilder(2 * hash.Length);
                                                foreach (byte byt in hash)
                                                {
                                                    GameAssemblyhashSb.AppendFormat("{0:X2}", byt);
                                                }

                                                Console.WriteLine(
                                                    $"GameAssembly Hash: {GameAssemblyhashSb.ToString()}");
                                                GameHash = GameAssemblyhashSb.ToString();
                                                CurrentOffsets = offMan.FetchForHash(GameAssemblyhashSb.ToString());
                                                if (CurrentOffsets is not null)
                                                {
                                                    Settings.conInterface.WriteModuleTextColored("GameMemReader",
                                                        Color.Lime, $"Loaded offsets: {CurrentOffsets.Description}");
                                                    ProcessHook?.Invoke(this, new ProcessHookArgs{PID = ProcessMemory.getInstance().process.Id});
                                                }
                                                else
                                                {
                                                    Settings.conInterface.WriteModuleTextColored("GameMemReader",
                                                        Color.Lime,
                                                        $"No offsets found for: {Color.Aqua.ToTextColor()}{GameAssemblyhashSb.ToString()}{Settings.conInterface.getNormalColor().ToTextColor()}.");

                                                }

                                            }
                                        }
                                    }

                                    foundModule = true;
                                    break;
                                }

                            if (!foundModule)
                            {
                                Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                                    "Still looking for modules...");
                                //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "Still looking for modules..."); // TODO: This still isn't functional, we need to re-hook to reload module addresses
                                Thread.Sleep(500); // delay and try again
                                ProcessMemory.getInstance().LoadModules();
                            }
                            else
                            {
                                break; // we have found all modules
                            }
                        }

                        try
                        {
                            if (CurrentOffsets is not null)
                            {
                            }

                            // prevGameOverReason = ProcessMemory.getInstance().Read<GameOverReason>(GameAssemblyPtr, _gameOffsets.TempDataOffset, 0x5c, 4);
                        }
                        catch
                        {
                            Settings.conInterface.WriteModuleTextColored("ERROR", Color.Red,
                                "Outdated version of the game.");
                        }

                    }

                    if (cracked && ProcessMemory.getInstance().IsHooked)
                    {
                        var result = Settings.conInterface.CrackDetected();
                        if (!result)
                            Environment.Exit(0);
                        else
                            cracked = false;
                        continue;
                    }

                    if (CurrentOffsets is null) continue;
                    GameState state;
                    //int meetingHudState = /*meetingHud_cachePtr == 0 ? 4 : */ProcessMemory.ReadWithDefault<int>(GameAssemblyPtr, 4, 0xDA58D0, 0x5C, 0, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                    var meetingHud = ProcessMemory.getInstance()
                        .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.MeetingHudPtr);
                    var meetingHud_cachePtr = meetingHud == IntPtr.Zero
                        ? 0
                        : ProcessMemory.getInstance().Read<uint>(meetingHud, CurrentOffsets.MeetingHudCachePtrOffsets);
                    var meetingHudState =
                        meetingHud_cachePtr == 0
                            ? 4
                            : ProcessMemory.getInstance().ReadWithDefault(meetingHud, 4, CurrentOffsets.MeetingHudStateOffsets
                                ); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                    var gameState =
                        ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.GameStateOffsets); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = ENDED (during "defeat" or "victory" screen only)

                    switch (gameState)
                    {
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
                        default:
                        {
                            if (exileCausesEnd)
                                state = GameState.LOBBY;
                            else if (meetingHudState < 4)
                                state = GameState.DISCUSSION;
                            else
                                state = GameState.TASKS;

                            break;
                        }
                    }
                    //Console.WriteLine($"Got state: {state}");


                    var allPlayersPtr =
                        ProcessMemory.getInstance()
                            .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);

                    var playerAddrPtr = allPlayers + 0x10;

                    // check if exile causes end
                    if (oldState == GameState.DISCUSSION && state == GameState.TASKS)
                    {
                        var exiledPlayerId = ProcessMemory.getInstance().ReadWithDefault<byte>(GameAssemblyPtr, 255,
                            CurrentOffsets.ExiledPlayerIdOffsets);
                        int impostorCount = 0, innocentCount = 0;

                        for (var i = 0; i < playerCount; i++)
                        {
                            // TODO: Actually implement this. Note the explicit types, also passed to ProcessMemory#Read()
                            PlayerInfo pi = isSteam ? ProcessMemory.getInstance().Read<SteamPlayerInfo>(playerAddrPtr, 0, 0) : ProcessMemory.getInstance().Read<EpicPlayerInfo>(playerAddrPtr, 0, 0);
                            playerAddrPtr += 4;

                            if (pi.PlayerId == exiledPlayerId)
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Exiled,
                                    Name = pi.GetPlayerName(),
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            // skip invalid, dead and exiled players
                            if (pi.PlayerName == IntPtr.Zero || pi.PlayerId == exiledPlayerId || pi.IsDead == 1 ||
                                pi.Disconnected == 1) continue;

                            if (pi.IsImpostor == 1)
                                impostorCount++;
                            else
                                innocentCount++;
                        }

                        if (impostorCount == 0 || impostorCount >= innocentCount)
                        {
                            exileCausesEnd = true;
                            state = GameState.LOBBY;
                        }
                    }

                    if (state != oldState || shouldForceTransmitState)
                    {
                        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs {NewState = state});
                        shouldForceTransmitState = false;
                    }

                    if (state != oldState && state == GameState.LOBBY)
                    {
                        shouldReadLobby = true; // will eventually transmit
                    }


                    if (oldState == GameState.ENDED && (state == GameState.LOBBY || state == GameState.MENU)) // game ended
                    {
                        int rawGameOverReason = ProcessMemory.getInstance()
                            .Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        GameOverReason gameOverReason = (GameOverReason) rawGameOverReason;

                        bool humansWon = rawGameOverReason <= 1 || rawGameOverReason == 5;
                        if (humansWon) // we will be reading humans data, so set all to simps
                        {
                            foreach (string playerName in CachedPlayerInfos.Keys)
                            {
                                try
                                {
                                    CachedPlayerInfos[playerName].IsImpostor = true;
                                }
                                catch (KeyNotFoundException e)
                                {
                                    Console.WriteLine($"Could not find User: \"{playerName}\" in CachedPlayerinfos");
                                }
                                
                            }
                        }

                        var winningPlayersPtr = ProcessMemory.getInstance()
                            .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.WinningPlayersPtrOffsets);
                        var winningPlayers = ProcessMemory.getInstance().Read<IntPtr>(winningPlayersPtr, CurrentOffsets.WinningPlayersOffsets);
                        var winningPlayerCount = ProcessMemory.getInstance().Read<int>(winningPlayersPtr, CurrentOffsets.WinningPlayerCountOffsets);

                        var winnerAddrPtr = winningPlayers + 0x10;

                        for (var i = 0; i < winningPlayerCount; i++)
                        {
                            WinningPlayerData wpi = ProcessMemory.getInstance()
                                .Read<WinningPlayerData>(winnerAddrPtr, 0, 0);
                            winnerAddrPtr += 4;
                            try
                            {
                                CachedPlayerInfos[wpi.GetPlayerName()].IsImpostor = wpi.IsImpostor;
                            }
                            catch (KeyNotFoundException e)
                            {
                                Console.WriteLine($"Could not find player with name \"{wpi.GetPlayerName()}\" in CachedPlayerInfos. JSON: {JsonConvert.SerializeObject(CachedPlayerInfos, Formatting.Indented)}");
                            }
                            
                        }

                        ImmutablePlayer[] endingPlayerInfos = new ImmutablePlayer[CachedPlayerInfos.Count];
                        CachedPlayerInfos.Values.CopyTo(endingPlayerInfos, 0);

                        GameOver?.Invoke(this, new GameOverEventArgs
                        {
                            GameOverReason = gameOverReason,
                            PlayerInfos = endingPlayerInfos
                        });
                    }

                    GameState cachedOldState = oldState;

                    oldState = state;


                    newPlayerInfos.Clear();

                    playerAddrPtr = allPlayers + 0x10;

                    for (var i = 0; i < playerCount; i++)
                    {
                        // TODO: Actually implement this. Note the explicit types, also passed to ProcessMemory#Read()
                        PlayerInfo pi = isSteam ? ProcessMemory.getInstance().Read<SteamPlayerInfo>(playerAddrPtr, 0, 0) : ProcessMemory.getInstance().Read<EpicPlayerInfo>(playerAddrPtr, 0, 0);
                        playerAddrPtr += 4;
                        if (pi.PlayerName == IntPtr.Zero) continue;
                        var playerName = pi.GetPlayerName();
                        if (playerName.Length == 0) continue;

                        newPlayerInfos[playerName] = pi; // add to new playerinfos for comparison later

                        if (!oldPlayerInfos.ContainsKey(playerName)) // player wasn't here before, they just joined
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.Joined,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                            PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs
                            {
                                Name = playerName,
                                HatId = pi.HatId,
                                SkinId = pi.SkinId,
                                PetId = pi.PetId
                            });
                        }
                        else
                        {
                            // player was here before, we have an old playerInfo to compare against
                            var oldPlayerInfo = oldPlayerInfos[playerName];
                            if (!oldPlayerInfo.GetIsDead() && pi.GetIsDead()) // player just died
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Died,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (oldPlayerInfo.ColorId != pi.ColorId)
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.ChangedColor,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (!oldPlayerInfo.GetIsDisconnected() && pi.GetIsDisconnected())
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Disconnected,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (oldPlayerInfo.HatId != pi.HatId || oldPlayerInfo.SkinId != pi.SkinId || oldPlayerInfo.PetId != pi.PetId)
                                PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs
                                {
                                    Name = playerName,
                                    HatId = pi.HatId,
                                    SkinId = pi.SkinId,
                                    PetId = pi.PetId
                                });
                        }
                    }

                    foreach (var kvp in oldPlayerInfos)
                    {
                        var pi = kvp.Value;
                        var playerName = kvp.Key;
                        if (!newPlayerInfos.ContainsKey(playerName)) // player was here before, isn't now, so they left
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.Left,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                    }

                    oldPlayerInfos.Clear();

                    var emitAll = false;
                    if (shouldForceUpdatePlayers)
                    {
                        shouldForceUpdatePlayers = false;
                        emitAll = true;
                    }


                    if (state != cachedOldState && (state == GameState.DISCUSSION || state == GameState.TASKS)
                    ) // game started, or at least we're still in game
                    {
                        CachedPlayerInfos.Clear();
                        foreach (var kvp in newPlayerInfos
                        ) // do this instead of assignment so they don't point to the same object
                        {
                            var pi = kvp.Value;
                            string playerName = pi.GetPlayerName();
                            CachedPlayerInfos[playerName] = new ImmutablePlayer()
                            {
                                Name = playerName,
                                IsImpostor = false
                            };
                        }
                    }

                    foreach (var kvp in newPlayerInfos
                    ) // do this instead of assignment so they don't point to the same object
                    {
                        var pi = kvp.Value;
                        oldPlayerInfos[kvp.Key] = pi;
                        if (emitAll)
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.ForceUpdated,
                                Name = kvp.Key,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                    }

                    if (shouldReadLobby)
                    {
                        var gameCode = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(
                            GameAssemblyPtr,
                            CurrentOffsets.GameCodeOffsets), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        string[] split;
                        if (!string.IsNullOrEmpty(gameCode) && (split = gameCode.Split('\n')).Length == 2)
                        {
                            PlayRegion region = (PlayRegion) ((4 - (ProcessMemory.getInstance()
                                .Read<int>(GameAssemblyPtr, CurrentOffsets.PlayRegionOffsets) & 0b11)) % 3); // do NOT ask
                            
                            //Recheck for GameOptionsOffset
                            PlayMap map = (PlayMap) ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);
                            
                            this.latestLobbyEventArgs = new LobbyEventArgs()
                            {
                                LobbyCode = split[1],
                                Region = region,
                                Map = map,
                            };
                            shouldReadLobby = false;
                            shouldTransmitLobby = true; // since this is probably new info
                        }
                    }

                    if (shouldTransmitLobby)
                    {
                        if (this.latestLobbyEventArgs != null)
                        {
                            JoinedLobby?.Invoke(this, this.latestLobbyEventArgs);
                        }

                        shouldTransmitLobby = false;
                    }

                    Thread.Sleep(250);

                }
                catch (Exception e)
                {
                    Settings.conInterface.WriteModuleTextColored("ERROR", Color.Red, $"Message: {e.Message} | stack: {e.StackTrace} | Retrying in 1000ms.");
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            }
        }

        public void ForceTransmitLobby()
        {
            shouldTransmitLobby = true;
        }

        public void ForceUpdatePlayers()
        {
            shouldForceUpdatePlayers = true;
        }

        public void ForceTransmitState()
        {
            shouldForceTransmitState = true;
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState NewState { get; set; }
    }

    public enum PlayerAction
    {
        Joined,
        Left,
        Died,
        ChangedColor,
        ForceUpdated,
        Disconnected,
        Exiled
    }

    public enum PlayerColor
    {
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

    public enum PlayRegion
    {
        NorthAmerica = 0,
        Asia = 1,
        Europe = 2
    }
    
    public enum PlayMap
    {
        Skeld = 0,
        Mira = 1,
        Polus = 2
    }

    public class PlayerCosmeticChangedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public uint HatId { get; set; } 
        public uint SkinId { get; set; }
        public uint PetId { get; set; }
    }

    public class PlayerChangedEventArgs : EventArgs
    {
        public PlayerAction Action { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool Disconnected { get; set; }
        public PlayerColor Color { get; set; }
    }

    public class ChatMessageEventArgs : EventArgs
    {
        public string Sender { get; set; }
        public PlayerColor Color { get; set; }
        public string Message { get; set; }
    }

    public class LobbyEventArgs : EventArgs
    {
        public string LobbyCode { get; set; }
        public PlayRegion Region { get; set; }
        public PlayMap Map { get; set; }
    }

    public class GameOverEventArgs : EventArgs
    {
        public GameOverReason GameOverReason { get; set; }
        public ImmutablePlayer[] PlayerInfos { get; set; }
    }
    public class ProcessHookArgs : EventArgs
    {
        public int PID { get; set; }
    }
}
