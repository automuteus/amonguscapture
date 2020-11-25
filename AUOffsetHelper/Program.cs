using System;
using System.Collections.Generic;
using AUOffsetManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AUOffsetHelper
{
    static class Program
    {
        public static string hash = "38119B8551718D9016BAFEEDC105610D5B3AED5B0036D1A6060B8E2ABE523C02";
        public static string description = "v2020.11.17s (Release 1)";

        public static int AmongUsClientOffset = 0x14B2C9C;

        public static int GameDataOffset = 0x14B2E9C;

        public static int MeetingHudOffset = 0x14B2A7C;

        public static int GameStartManagerOffset = 0x13983DC;

        public static int HudManagerOffset = 0x138B9FC;


        public static int ServerManagerOffset = 0x138E51C;

        public static int TempDataOffset = 0x143B7AC;

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
                TempDataOffset = TempDataOffset
            };
            Console.Write(JsonConvert.SerializeObject(a, Formatting.Indented));
            var b = new OffsetManager("");
            b.AddToLocalIndex(hash, a);
            Console.ReadLine();

            
        }
    }
}
