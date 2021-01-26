using System;
using System.Collections.Generic;
using AUOffsetManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AUOffsetHelper
{
    static class Program
    {
        public static string hash = "0B010BD3195D39C089DC018D834B2EBD26BA67D2F49C4EBEA608A804FC0975B7";
        public static string description = "v2020.12.9s";

        public static int AmongUsClientOffset = 0x1C57F54;

        public static int GameDataOffset = 0x1C57BE8;

        public static int MeetingHudOffset = 0x1C573A4;

        public static int GameStartManagerOffset = 0x1AF20FC;

        public static int HudManagerOffset = 0x1AE16EC;


        public static int ServerManagerOffset = 0x1AE4DEC;

        public static int TempDataOffset = 0x1C58048;
        public static int GameOptionsOffset = 0x1C57F7C;

        static void Main(string[] args)
        {
            var a = new GameOffsets
            {
                Description = description,
                AmongUsClientOffset = AmongUsClientOffset,
                GameDataOffset = GameDataOffset,
                MeetingHudOffset = MeetingHudOffset,
                GameStartManagerOffset = GameStartManagerOffset,
                HudManagerOffset = HudManagerOffset,
                ServerManagerOffset = ServerManagerOffset,
                TempDataOffset = TempDataOffset,
                GameOptionsOffset = GameOptionsOffset,

                MeetingHudPtr = new []{MeetingHudOffset, 0x5C, 0},
                MeetingHudCachePtrOffsets = new []{0x8},
                MeetingHudStateOffsets = new []{0x84},
                GameStateOffsets = new []{ AmongUsClientOffset, 0x5C, 0, 0x64 },
                AllPlayerPtrOffsets = new []{ GameDataOffset, 0x5C, 0, 0x24},
                AllPlayersOffsets = new []{0x08},
                PlayerCountOffsets = new []{0x0C},
                ExiledPlayerIdOffsets = new []{MeetingHudOffset, 0x5C, 0, 0x94, 0x08},
                RawGameOverReasonOffsets = new []{TempDataOffset, 0x5c, 0x4},
                WinningPlayersPtrOffsets = new []{TempDataOffset, 0x5C, 0xC},
                WinningPlayersOffsets = new []{0x08},
                WinningPlayerCountOffsets = new []{0x0C},
                GameCodeOffsets = new []{GameStartManagerOffset, 0x5c, 0, 0x20, 0x28 },
                PlayRegionOffsets = new []{ServerManagerOffset, 0x5c, 0, 0x10, 0x8, 0x8},
                PlayMapOffsets = new []{GameOptionsOffset, 0x5c, 0x4, 0x10},
                StringOffsets = new []{0x8, 0xC},
                isEpic = false,
                AddPlayerPtr = 4,
                PlayerListPtr = 0x10

            };
            Console.Write(JsonConvert.SerializeObject(a, Formatting.Indented));
            var b = new OffsetManager("");
            b.AddToLocalIndex(hash, a);
            Console.ReadLine();

            
        }
    }
}
