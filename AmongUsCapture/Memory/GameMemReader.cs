using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AmongUsCapture.TextColorLibrary;
using AUOffsetManager;

namespace AmongUsCapture
{
    public enum GameState
    {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU,
        UNKNOWN
    }

    public class GameMemReader
    {
        private static readonly GameMemReader instance = new GameMemReader();
        private readonly IGameOffsets _gameOffsets = Settings.GameOffsets;
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

        private GameState oldState = GameState.UNKNOWN;

        private int prevChatBubsVersion;
        private bool shouldForceTransmitState;
        private bool shouldForceUpdatePlayers;
        private bool shouldTransmitLobby;
        public OffsetManager offMan = new OffsetManager(Settings.PersistentSettings.indexURL);
        public GameOffsets CurrentOffsets;
        public string GameHash = "";
        public static GameMemReader getInstance()
        {
            return instance;
        }

        public event EventHandler<ValidatorEventArgs> GameVersionUnverified;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;

        public event EventHandler<LobbyEventArgs> JoinedLobby;

        private bool cracked = false;

        public void RunLoop()
        {
            while (true)
            {
                if (!ProcessMemory.getInstance().IsHooked || ProcessMemory.getInstance().process is null || ProcessMemory.getInstance().process.HasExited)
                {
                    if (!ProcessMemory.getInstance().HookProcess("Among Us"))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"Connected to Among Us process ({Color.Red.ToTextColor()}{ProcessMemory.getInstance().process.Id}{Settings.conInterface.getNormalColor().ToTextColor()})");
                    
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
                                    Settings.conInterface.WriteModuleTextColored("GameVerifier", Color.Red, $"Client verification: {Color.Lime.ToTextColor()}PASS{Settings.conInterface.getNormalColor().ToTextColor()}.");
                                }
                                using (SHA256Managed sha256 = new SHA256Managed())
                                {
                                    using (FileStream fs = new FileStream(module.FileName, FileMode.Open, FileAccess.Read))
                                    {
                                        using (var bs = new BufferedStream(fs))
                                        {
                                            var hash = sha256.ComputeHash(bs);
                                            StringBuilder GameAssemblyhashSb = new StringBuilder(2 * hash.Length);
                                            foreach (byte byt in hash)
                                            {
                                                GameAssemblyhashSb.AppendFormat("{0:X2}", byt);
                                            }

                                            Console.WriteLine($"GameAssembly Hash: {GameAssemblyhashSb.ToString()}");
                                            GameHash = GameAssemblyhashSb.ToString();
                                            CurrentOffsets = offMan.FetchForHash(GameAssemblyhashSb.ToString());
                                            if (CurrentOffsets is not null)
                                            {
                                                Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"Loaded offsets: {CurrentOffsets.description}");
                                            }
                                            else
                                            {
                                                Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"No offsets found for: {Color.Aqua.ToTextColor()}{GameAssemblyhashSb.ToString()}.");
                                                cracked = true;

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
                        prevChatBubsVersion = ProcessMemory.getInstance().Read<int>(GameAssemblyPtr,
                            CurrentOffsets.chatBubsVersionOffsets);
                    }
                    catch
                    {
                        Settings.conInterface.WriteModuleTextColored("ERROR",Color.Red, "Outdated version of the game.");
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

                GameState state;
                //int meetingHudState = /*meetingHud_cachePtr == 0 ? 4 : */ProcessMemory.ReadWithDefault<int>(GameAssemblyPtr, 4, 0xDA58D0, 0x5C, 0, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                var meetingHud = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.meetingHudPtrOffsets);
                var meetingHudCachePtr = meetingHud == IntPtr.Zero ? 0 : ProcessMemory.getInstance().Read<uint>(meetingHud, CurrentOffsets.meetingHudCacheOffsets);
                var meetingHudState =
                    meetingHudCachePtr == 0
                        ? 4
                        : ProcessMemory.getInstance().ReadWithDefault(meetingHud, 4,
                            0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                var gameState =
                    ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.gameStateOffsets); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = Ended (during "defeat" or "victory" screen only)

                switch (gameState)
                {
                    case 0:
                        state = GameState.MENU;
                        exileCausesEnd = false;
                        break;
                    case 1:
                    case 3:
                        state = GameState.LOBBY;
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


                var allPlayersPtr =
                    ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.allPlayersPtrOffsets);
                var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.allPlayersOffsets);
                var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.playerCountOffsets);

                var playerAddrPtr = allPlayers + 0x10;

                // check if exile causes end
                if (oldState == GameState.DISCUSSION && state == GameState.TASKS)
                {
                    var exiledPlayerId = ProcessMemory.getInstance().ReadWithDefault<byte>(GameAssemblyPtr, 255,
                        CurrentOffsets.exiledPlayerIdOffsets);
                    int impostorCount = 0, innocentCount = 0;

                    for (var i = 0; i < playerCount; i++)
                    {
                        var pi = ProcessMemory.getInstance().Read<PlayerInfo>(playerAddrPtr, 0, 0);
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
                        if (pi.PlayerName == 0 || pi.PlayerId == exiledPlayerId || pi.IsDead == 1 ||
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

                oldState = state;

                newPlayerInfos.Clear();

                playerAddrPtr = allPlayers + 0x10;

                for (var i = 0; i < playerCount; i++)
                {
                    var pi = ProcessMemory.getInstance().Read<PlayerInfo>(playerAddrPtr, 0, 0);
                    playerAddrPtr += 4;
                    if (pi.PlayerName == 0) continue;
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

                var chatBubblesPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.chatBubblesPtrOffsets);

                var poolSize = 20; // = ProcessMemory.Read<int>(GameAssemblyPtr, 0xD0B25C, 0x5C, 0, 0x28, 0xC, 0xC)

                var numChatBubbles = ProcessMemory.getInstance().Read<int>(chatBubblesPtr, CurrentOffsets.numChatBubblesOffsets);
                var chatBubsVersion = ProcessMemory.getInstance().Read<int>(chatBubblesPtr, CurrentOffsets.chatBubsVersionOffsets);
                var chatBubblesAddr = ProcessMemory.getInstance().Read<IntPtr>(chatBubblesPtr, CurrentOffsets.chatBubblesAddrOffsets) + 0x10;
                var chatBubblePtrs = ProcessMemory.getInstance().ReadArray(chatBubblesAddr, numChatBubbles);

                var newMsgs = 0;

                if (chatBubsVersion > prevChatBubsVersion) // new message has been sent
                {
                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                    {
                        if (prevChatBubsVersion > poolSize)
                            newMsgs = (chatBubsVersion - prevChatBubsVersion) >> 1;
                        else
                            newMsgs = poolSize - prevChatBubsVersion + ((chatBubsVersion - poolSize) >> 1);
                    }
                    else // single increments
                    {
                        newMsgs = chatBubsVersion - prevChatBubsVersion;
                    }
                }
                else if (chatBubsVersion < prevChatBubsVersion) // reset
                {
                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                        newMsgs = poolSize + ((chatBubsVersion - poolSize) >> 1);
                    else // single increments
                        newMsgs = chatBubsVersion;
                }

                prevChatBubsVersion = chatBubsVersion;

                for (var i = numChatBubbles - newMsgs; i < numChatBubbles; i++)
                {
                    var msgText = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(chatBubblePtrs[i], CurrentOffsets.messageTextOffsets));
                    if (msgText.Length == 0) continue;
                    var msgSender = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(chatBubblePtrs[i], CurrentOffsets.messageSenderOffsets));
                    var oldPlayerInfo = oldPlayerInfos[msgSender];
                    ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs
                    {
                        Sender = msgSender,
                        Message = msgText,
                        Color = oldPlayerInfo.GetPlayerColor()
                    });
                }

                if (shouldReadLobby)
                {
                    var gameCode = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr,
                        CurrentOffsets.gameCodeOffsets));
                    string[] split;
                    if (gameCode != null && gameCode.Length > 0 && (split = gameCode.Split('\n')).Length == 2)
                    {
                        PlayRegion region = (PlayRegion)((4 - (ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.regionOffsets) & 0b11)) % 3); // do NOT ask

                        this.latestLobbyEventArgs = new LobbyEventArgs()
                        {
                            LobbyCode = split[1],
                            Region = region
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
    }
}