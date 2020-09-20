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
        DISCUSSION
    }
    class GameMemReader
    {
        private static GameMemReader instance = new GameMemReader();
        private bool shouldForceUpdate = false;

        public static GameMemReader getInstance()
        {
            return instance;
        }
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        private bool muteAfterExile = true;

        public Dictionary<string, PlayerInfo> oldPlayerInfos = new Dictionary<string, PlayerInfo>(10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead
        public Dictionary<string, PlayerInfo> newPlayerInfos = new Dictionary<string, PlayerInfo>(10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        private IntPtr UnityPlayerPtr = IntPtr.Zero;
        private GameState oldState = GameState.LOBBY;

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
                        Console.WriteLine("Connected to Among Us process ({0})", ProcessMemory.process.Id);

                        int modulesLeft;

                        while(true)
                        {
                            modulesLeft = 2;
                            foreach (ProcessMemory.Module module in ProcessMemory.modules)
                            {
                                if (modulesLeft == 0)
                                    break;
                                else if (module.Name.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                                {
                                    GameAssemblyPtr = module.BaseAddress;
                                    modulesLeft--;
                                }
                                else if (module.Name.Equals("UnityPlayer.dll", StringComparison.OrdinalIgnoreCase))
                                {
                                    UnityPlayerPtr = module.BaseAddress;
                                    modulesLeft--;
                                }
                            }

                            if (UnityPlayerPtr == IntPtr.Zero || GameAssemblyPtr == IntPtr.Zero) // either one or both hasn't been found yet
                            {
                                Task.Delay(500); // delay and try again
                            } else
                            {
                                break; // we have found all modules
                            }
                        }
                        

                        Console.WriteLine($"({GameAssemblyPtr}) ({UnityPlayerPtr})");
                    }
                }

                GameState state;
                bool inGame = ProcessMemory.Read<bool>(UnityPlayerPtr, 0x127B310, 0xF4, 0x18, 0xA8);
                bool inMeeting = ProcessMemory.Read<bool>(UnityPlayerPtr, 0x12A7A14, 0x64, 0x54, 0x18);
                int meetingHudState = ProcessMemory.Read<int>(GameAssemblyPtr, 0xDA58D0, 0x5C, 0, 0x84);


                if (!inGame || (inMeeting && meetingHudState > 2 && ExileEndsGame()))
                {
                    state = GameState.LOBBY;
                }
                else if (inMeeting && (muteAfterExile || meetingHudState < 4))
                {
                    state = GameState.DISCUSSION;
                }
                else
                {
                    state = GameState.TASKS;
                }

                if (state != oldState)
                {
                    GameStateChanged.Invoke(this, new GameStateChangedEventArgs() { NewState = state });
                }

                oldState = state;


                IntPtr allPlayersPtr = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, 0xDA5A60, 0x5C, 0, 0x24);
                IntPtr allPlayers = ProcessMemory.Read<IntPtr>(allPlayersPtr, 0x08);
                int playerCount = ProcessMemory.Read<int>(allPlayersPtr, 0x0C);

                IntPtr playerAddrPtr = allPlayers + 0x10;

                newPlayerInfos.Clear();

                for (int i = 0; i < playerCount; i++)
                {
                    IntPtr playerAddr = ProcessMemory.Read<IntPtr>(playerAddrPtr);
                    PlayerInfo pi = ProcessMemory.Read<PlayerInfo>(playerAddr);
                    if (pi.PlayerName == 0) { continue; }
                    string playerName = pi.GetPlayerName();

                    newPlayerInfos[playerName] = pi; // add to new playerinfos for comparison later

                    if (!oldPlayerInfos.ContainsKey(playerName)) // player wasn't here before, they just joined
                    {
                        PlayerChanged.Invoke(this, new PlayerChangedEventArgs()
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
                            PlayerChanged.Invoke(this, new PlayerChangedEventArgs()
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
                            PlayerChanged.Invoke(this, new PlayerChangedEventArgs()
                            {
                                Action = PlayerAction.ChangedColor,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                        }
                    }

                    playerAddrPtr += 4;
                }

                foreach (KeyValuePair<string, PlayerInfo> kvp in oldPlayerInfos)
                {
                    PlayerInfo pi = kvp.Value;
                    string playerName = kvp.Key;
                    if (!newPlayerInfos.ContainsKey(playerName)) // player was here before, isn't now, so they left
                    {
                        PlayerChanged.Invoke(this, new PlayerChangedEventArgs()
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
                        PlayerChanged.Invoke(this, new PlayerChangedEventArgs()
                        {
                            Action = PlayerAction.ForceUpdated,
                            Name = kvp.Key,
                            IsDead = pi.GetIsDead(),
                            Disconnected = pi.GetIsDisconnected(),
                            Color = pi.GetPlayerColor()
                        });
                    }
                }

                //foreach (KeyValuePair<string, PlayerInfo> kvp in oldPlayerInfos)
                //{
                //    PlayerInfo pi = kvp.Value;
                //    Console.WriteLine($"Player ID {pi.PlayerId}; Name: {ProcessMemory.ReadString((IntPtr)pi.PlayerName)}; Color: {pi.ColorId}; Dead: " + ((pi.IsDead > 0) ? "yes" : "no"));
                //}

                Thread.Sleep(250);
            }
        }

        private static bool ExileEndsGame()
        {
            return false;
        }

        public void ForceUpdate()
        {
            this.shouldForceUpdate = true;
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
        ForceUpdated
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
}
