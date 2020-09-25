using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmongUsCapture
{
    public enum GameState
    {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU
    }
    class GameMemReader
    {
        private static GameMemReader instance = new GameMemReader();
        private bool shouldForceUpdate = false;
        private bool shouldTransmitState = false;

        private int AmongUsClientOffset = 0x1468840;
        private int GameDataOffset = 0x1468864;
        private int MeetingHudOffset = 0x14686A0;
        private int GameStartManagerOffset = 0x0;
        private int HudManagerOffset = 0x13EEB44;

        public static GameMemReader getInstance()
        {
            return instance;
        }
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;


        public Dictionary<string, PlayerInfo> oldPlayerInfos = new Dictionary<string, PlayerInfo>(10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead
        public Dictionary<string, PlayerInfo> newPlayerInfos = new Dictionary<string, PlayerInfo>(10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        private GameState oldState = GameState.LOBBY;
        private bool exileCausesEnd = false;

        private int prevChatBubsVersion;

        public void RunLoop()
        {
            while (true)
            {
                if (!ProcessMemory.IsHooked)
                {
                    if (!ProcessMemory.HookProcess("Among Us"))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    else
                    {
                        Program.conInterface.WriteLine($"Connected to Among Us process ({ProcessMemory.process.Id})");

                        bool foundModule = false;

                        while(true)
                        {
                            foreach (ProcessMemory.Module module in ProcessMemory.modules)
                            {
                                if (module.Name.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                                {
                                    GameAssemblyPtr = module.BaseAddress;
                                    foundModule = true;
                                    break;
                                }
                            }

                            if (!foundModule)
                            {
                                Program.conInterface.WriteLine("Still looking for modules..."); // TODO: This still isn't functional, we need to re-hook to reload module addresses
                                Thread.Sleep(500); // delay and try again
                            } else
                            {
                                break; // we have found all modules
                            }
                        }
                        

                        Console.WriteLine($"({GameAssemblyPtr})");
                        prevChatBubsVersion = ProcessMemory.Read<int>(GameAssemblyPtr, HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14, 0x10);
                    }
                }

                GameState state;
                //int meetingHudState = /*meetingHud_cachePtr == 0 ? 4 : */ProcessMemory.ReadWithDefault<int>(GameAssemblyPtr, 4, 0xDA58D0, 0x5C, 0, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                IntPtr meetingHud = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, MeetingHudOffset, 0x5C, 0);
                uint meetingHud_cachePtr = meetingHud == IntPtr.Zero ? 0 : ProcessMemory.Read<uint>(meetingHud, 0x8);
                int meetingHudState = meetingHud_cachePtr == 0 ? 4 : ProcessMemory.ReadWithDefault<int>(meetingHud, 4, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                int gameState = ProcessMemory.Read<int>(GameAssemblyPtr, AmongUsClientOffset, 0x5C, 0, 0x64); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = Ended (during "defeat" or "victory" screen only)

                if (gameState == 0)
                {
                    state = GameState.MENU;
                    exileCausesEnd = false;
                }
                else if (gameState == 1 || gameState == 3)
                {
                    state = GameState.LOBBY;
                    exileCausesEnd = false;
                }
                else if (exileCausesEnd)
                {
                    state = GameState.LOBBY;
                }
                else if (meetingHudState < 4)
                {
                    state = GameState.DISCUSSION;
                } else
                {
                    state = GameState.TASKS;
                }

                IntPtr allPlayersPtr = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, GameDataOffset, 0x5C, 0, 0x24);
                IntPtr allPlayers = ProcessMemory.Read<IntPtr>(allPlayersPtr, 0x08);
                int playerCount = ProcessMemory.Read<int>(allPlayersPtr, 0x0C);

                IntPtr playerAddrPtr = allPlayers + 0x10;

                // check if exile causes end
                if (oldState == GameState.DISCUSSION && state == GameState.TASKS)
                {
                    byte exiledPlayerId = ProcessMemory.ReadWithDefault<byte>(GameAssemblyPtr, 255, MeetingHudOffset, 0x5C, 0, 0x94, 0x08);
                    int impostorCount = 0, innocentCount = 0;

                    for (int i = 0; i < playerCount; i++)
                    {
                        PlayerInfo pi = ProcessMemory.Read<PlayerInfo>(playerAddrPtr, 0, 0);
                        playerAddrPtr += 4;

                        if (pi.PlayerId == exiledPlayerId)
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                            {
                                Action = PlayerAction.Exiled,
                                Name = pi.GetPlayerName(),
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                        }

                        // skip invalid, dead and exiled players
                        if (pi.PlayerName == 0 || pi.PlayerId == exiledPlayerId || pi.IsDead == 1 || pi.Disconnected == 1) { continue; }

                        if (pi.IsImpostor == 1) { impostorCount++; }
                        else { innocentCount++; }
                    }

                    if (impostorCount == 0 || impostorCount >= innocentCount)
                    {
                        exileCausesEnd = true;
                        state = GameState.LOBBY;
                    }
                }

                if (this.shouldTransmitState)
                {
                    shouldTransmitState = false;
                    GameStateChanged?.Invoke(this, new GameStateChangedEventArgs() { NewState = state });
                } else if (state != oldState)
                {
                    GameStateChanged?.Invoke(this, new GameStateChangedEventArgs() { NewState = state });
                }

                oldState = state;

                newPlayerInfos.Clear();

                playerAddrPtr = allPlayers + 0x10;

                for (int i = 0; i < playerCount; i++)
                {
                    PlayerInfo pi = ProcessMemory.Read<PlayerInfo>(playerAddrPtr, 0, 0);
                    playerAddrPtr += 4;
                    if (pi.PlayerName == 0) { continue; }
                    string playerName = pi.GetPlayerName();

                    newPlayerInfos[playerName] = pi; // add to new playerinfos for comparison later

                    if (!oldPlayerInfos.ContainsKey(playerName)) // player wasn't here before, they just joined
                    {
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                        {
                            Action = PlayerAction.Joined,
                            Name = playerName,
                            IsDead = pi.GetIsDead(),
                            Disconnected = pi.GetIsDisconnected(),
                            Color = pi.GetPlayerColor()
                        });
                    } else { // player was here before, we have an old playerInfo to compare against
                        PlayerInfo oldPlayerInfo = oldPlayerInfos[playerName];
                        if (!oldPlayerInfo.GetIsDead() && pi.GetIsDead()) // player just died
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                            {
                                Action = PlayerAction.Died,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                        }

                        if(oldPlayerInfo.ColorId != pi.ColorId)
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                            {
                                Action = PlayerAction.ChangedColor,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                        }

                        if(!oldPlayerInfo.GetIsDisconnected() && pi.GetIsDisconnected())
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                            {
                                Action = PlayerAction.Disconnected,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                        }
                    }
                }

                foreach (KeyValuePair<string, PlayerInfo> kvp in oldPlayerInfos)
                {
                    PlayerInfo pi = kvp.Value;
                    string playerName = kvp.Key;
                    if (!newPlayerInfos.ContainsKey(playerName)) // player was here before, isn't now, so they left
                    {
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                        {
                            Action = PlayerAction.Left,
                            Name = playerName,
                            IsDead = pi.GetIsDead(),
                            Disconnected = pi.GetIsDisconnected(),
                            Color = pi.GetPlayerColor()
                        });
                    }
                }

                oldPlayerInfos.Clear();

                bool emitAll = false;
                if (shouldForceUpdate)
                {
                    shouldForceUpdate = false;
                    emitAll = true;
                }

                foreach (KeyValuePair<string, PlayerInfo> kvp in newPlayerInfos) // do this instead of assignment so they don't point to the same object
                {
                    PlayerInfo pi = kvp.Value;
                    oldPlayerInfos[kvp.Key] = pi;
                    if (emitAll)
                    {
                        PlayerChanged?.Invoke(this, new PlayerChangedEventArgs()
                        {
                            Action = PlayerAction.ForceUpdated,
                            Name = kvp.Key,
                            IsDead = pi.GetIsDead(),
                            Disconnected = pi.GetIsDisconnected(),
                            Color = pi.GetPlayerColor()
                        });
                    }
                }

                IntPtr chatBubblesPtr = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14);

                int poolSize = 20; // = ProcessMemory.Read<int>(GameAssemblyPtr, 0xD0B25C, 0x5C, 0, 0x28, 0xC, 0xC)

                int numChatBubbles = ProcessMemory.Read<int>(chatBubblesPtr, 0xC);
                int chatBubsVersion = ProcessMemory.Read<int>(chatBubblesPtr, 0x10);
                IntPtr chatBubblesAddr = ProcessMemory.Read<IntPtr>(chatBubblesPtr, 0x8) + 0x10;
                IntPtr[] chatBubblePtrs = ProcessMemory.ReadArray(chatBubblesAddr, numChatBubbles);

                int newMsgs = 0;

                if (chatBubsVersion > prevChatBubsVersion) // new message has been sent
                {
                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                    {
                        if (prevChatBubsVersion > poolSize)
                        {
                            newMsgs = (chatBubsVersion - prevChatBubsVersion) >> 1;
                        }
                        else
                        {
                            newMsgs = (poolSize - prevChatBubsVersion) + ((chatBubsVersion - poolSize) >> 1);
                        }
                    }
                    else // single increments
                    {
                        newMsgs = chatBubsVersion - prevChatBubsVersion;
                    }
                }
                else if (chatBubsVersion < prevChatBubsVersion) // reset
                {
                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                    {
                        newMsgs = poolSize + ((chatBubsVersion - poolSize) >> 1);
                    }
                    else // single increments
                    {
                        newMsgs = chatBubsVersion;
                    }
                }

                prevChatBubsVersion = chatBubsVersion;

                for (int i = numChatBubbles - newMsgs; i < numChatBubbles; i++)
                {
                    string msgText = ProcessMemory.ReadString(ProcessMemory.Read<IntPtr>(chatBubblePtrs[i], 0x20, 0x28));
                    if (msgText.Length == 0) continue;
                    string msgSender = ProcessMemory.ReadString(ProcessMemory.Read<IntPtr>(chatBubblePtrs[i], 0x1C, 0x28));
                    ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs()
                    {
                        Sender = msgSender,
                        Message = msgText
                    });
                }

                //string gameCode = ProcessMemory.ReadString(ProcessMemory.Read<IntPtr>(GameAssemblyPtr, GameStartManagerOffset, 0x5c, 0, 0x20, 0x28));
                //Console.WriteLine(gameCode);

                Thread.Sleep(250);
            }
        }

        public void ForceUpdate()
        {
            this.shouldForceUpdate = true;
        }

        public void ForceTransmitState()
        {
            this.shouldTransmitState = true;
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
        public string Message { get; set; }
    }
}
