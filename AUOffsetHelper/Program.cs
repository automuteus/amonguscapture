using System;
using System.Collections.Generic;
using AUOffsetManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AUOffsetHelper
{
    static class Program
    {
        public static string hash = "FF1DAE62454312FCE09A39061999C26FD26440FDA5F36C1E6424290A34D05B08";
        public static string description = "v2020.11.17s (Release 2)";

        public static int AmongUsClientOffset = 0x143BE9C;

        public static int GameDataOffset = 0x143BF38;

        public static int MeetingHudOffset = 0x143BBB4;

        public static int GameStartManagerOffset = 0x1399BD0;

        public static int HudManagerOffset = 0x1060AC0;


        public static int ServerManagerOffset = 0x138FCD0;

        public static int WinDataOffset = 0x14B2AFC;

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
                WinDataOffset = WinDataOffset
            };
            Console.Write(JsonConvert.SerializeObject(a, Formatting.Indented));
            var b = new OffsetManager("");
            b.AddToLocalIndex(hash, a);
            Console.ReadLine();

            
        }
    }
}
