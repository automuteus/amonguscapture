using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace AmongcordClient
{
    class Program
    {
        private static bool muteAfterExile = true;

        private enum GameState
        {
            LOBBY,
            TASKS,
            DISCUSSION
        }
        private static IntPtr GameAssemblyPtr = IntPtr.Zero;
        private static IntPtr UnityPlayerPtr = IntPtr.Zero;
        private static GameState oldState = GameState.LOBBY;

        private static string[] playerColors = new string[]{"red", "blue", "green", "pink", "orange", "yellow", "black", "white", "purple", "brown", "cyan", "lime"};
        static void Main()
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

                        int modulesLeft = 2;
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
                    }
                }

                GameState state;
                bool inGame = ProcessMemory.Read<bool>(UnityPlayerPtr, 0x127B310, 0xF4, 0x18, 0xA8);
                bool inMeeting = ProcessMemory.Read<bool>(UnityPlayerPtr, 0x12A7A14, 0x64, 0x54, 0x18);
                int meetingHudState = ProcessMemory.Read<int>(GameAssemblyPtr, 0xDA58D0, 0x5C, 0, 0x84);
                
                IntPtr allPlayersPtr = ProcessMemory.Read<IntPtr>(GameAssemblyPtr, 0xDA5A60, 0x5C, 0, 0x24);
                IntPtr allPlayers = ProcessMemory.Read<IntPtr>(allPlayersPtr, 0x08);
                int playerCount = ProcessMemory.Read<int>(allPlayersPtr, 0x0C);

                IntPtr playerAddrPtr = allPlayers + 0x10;

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

                List<PlayerInfo> allPlayerInfos = new List<PlayerInfo>();

                for (int i = 0; i < playerCount; i++)
                {
                    IntPtr playerAddr = ProcessMemory.Read<IntPtr>(playerAddrPtr);
                    PlayerInfo pi = ProcessMemory.Read<PlayerInfo>(playerAddr);
                    allPlayerInfos.Add(pi);
                    playerAddrPtr += 4;
                }

                foreach (PlayerInfo pi in allPlayerInfos)
                {
                    Console.WriteLine($"Player ID {pi.PlayerId}; Name: {ProcessMemory.ReadString((IntPtr)pi.PlayerName)}; Color: {playerColors[pi.ColorId]}; Dead: " + ((pi.IsDead > 0) ? "yes": "no"));
                }


                if (state != oldState)
                {
                    Console.WriteLine("State changed to {0}", state);
                }

                oldState = state;
                Thread.Sleep(250);
            }
        }

        private static bool ExileEndsGame()
        {
            return false;
        }
    }
}
