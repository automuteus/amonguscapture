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

        public static GameMemReader getInstance()
        {
            return instance;
        }
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public Dictionary<string, PlayerInfo> oldPlayerInfos = new Dictionary<string, PlayerInfo>(10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead
        public Dictionary<string, PlayerInfo> newPlayerInfos = new Dictionary<string, PlayerInfo>(10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        private GameState oldState = GameState.LOBBY;
        private bool exileCausesEnd = false;

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
                                Console.WriteLine("Still looking for modules..."); // TODO: This still isn't functional, we need to re-hook to reload module addresses
                                Thread.Sleep(500); // delay and try again
                            } else
                            {
                                break; // we have found all modules
                            }
                        }
                        

                        Console.WriteLine($"({GameAssemblyPtr})");
                    }
                }

                GameState state;
                int meetingHudState = ProcessMemory.ReadWithDefault<int>(GameAssemblyPtr, 4, 0xDA58D0, 0x5C, 0, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                int gameState = ProcessMemory.Read<int>(GameAssemblyPtr, 0xDA5ACC, 0x5C, 0, 0x64); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = Ended (during "defeat" or "victory" screen only)

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

                IntPtr allPlayersPtr = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, 0xDA5A60, 0x5C, 0, 0x24);
                IntPtr allPlayers = ProcessMemory.Read<IntPtr>(allPlayersPtr, 0x08);
                int playerCount = ProcessMemory.Read<int>(allPlayersPtr, 0x0C);

                IntPtr playerAddrPtr = allPlayers + 0x10;

                // check if exile causes end
                if (oldState == GameState.DISCUSSION && state == GameState.TASKS)
                {
                    byte exiledPlayerId = ProcessMemory.ReadWithDefault<byte>(GameAssemblyPtr, 255, 0xDA58D0, 0x5C, 0, 0x94, 0x08);
                    Console.WriteLine($"Player with id {exiledPlayerId} was exiled.");
                    int impostorCount = 0, innocentCount = 0;

                    for (int i = 0; i < playerCount; i++)
                    {
                        PlayerInfo pi = ProcessMemory.Read<PlayerInfo>(playerAddrPtr, 0, 0);
                        playerAddrPtr += 4;

                        // skip invalid, dead and exiled players
                        if (pi.PlayerName == 0 || pi.PlayerId == exiledPlayerId || pi.IsDead == 1) { continue; }

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
                    GameStateChanged.Invoke(this, new GameStateChangedEventArgs() { NewState = state });
                } else if (state != oldState)
                {
                    if (oldState == GameState.DISCUSSION && state == GameState.TASKS) // send delayed
                    {
                        Task.Delay(7000).ContinueWith((task) => {
                            this.ForceTransmitState();
                        });
                    } else
                    {
                        GameStateChanged.Invoke(this, new GameStateChangedEventArgs() { NewState = state });
                    }
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
