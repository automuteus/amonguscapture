namespace AUOffsetManager
{
    public static class
        GameOffsetsHelper //helper to build a GameOffsets object with common values. (Helpful for when Main offset change but not the whole offset) 
    {
        public static GameOffsets FromDefault(int AmongUsClientOffset, int GameDataOffset, int MeetingHudOffset,
            int GameStartManagerOffset, int HudManagerOffset, int ServerManagerOffset)
        {
            var a = new GameOffsets();

            a.prevChatBubsVersionOffsets = new[]
            {
                HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14, 0x10
            };
            a.meetingHudPtrOffsets = new[] {HudManagerOffset, 0x5C, 0};

            a.meetingHudCacheOffsets = new[]
            {
                0x08
            };
            a.meetingHudStateOffsets = new[]
            {
                0x84
            };
            a.gameStateOffsets = new[]
            {
                AmongUsClientOffset, 0x5C, 0, 0x64
            };
            a.allPlayersPtrOffsets = new[]
            {
                GameDataOffset, 0x5C, 0, 0x24
            };
            a.allPlayersOffsets = new[]
            {
                0x08
            };
            a.playerCountOffsets = new[]
            {
                0x0C
            };
            a.exiledPlayerIdOffsets = new[]
            {
                MeetingHudOffset, 0x5C, 0, 0x94, 0x08
            };
            a.chatBubblesPtrOffsets = new[]
            {
                HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14
            };
            a.chatBubblePoolSize = 20;
            a.numChatBubblesOffsets = new[]
            {
                0xC
            };
            a.chatBubsVersionOffsets = new[]
            {
                0x10
            };
            a.chatBubblesAddrOffsets = new[]
            {
                0x8
            };
            a.messageTextOffsets = new[]
            {
                0x20, 0x28
            };
            a.messageSenderOffsets = new[]
            {
                0x1C, 0x28
            };
            a.gameCodeOffsets = new[]
            {
                GameStartManagerOffset, 0x5c, 0, 0x20, 0x28
            };
            a.regionOffsets = new[]
            {
                ServerManagerOffset, 0x5c, 0, 0x10, 0x8, 0x8
            };
            return a;
        }
    }
}