namespace AUOffsetManager
{
    public static class
        GameOffsetsHelper //helper to build a GameOffsets object with common values. (Helpful for when Main offset change but not the whole offset) 
    {
        public static GameOffsets FromDefault(string GameHash, string description, int AmongUsClientOffset, int GameDataOffset, int MeetingHudOffset, int GameStartManagerOffset, int HudManagerOffset, int ServerManagerOffset, int WinDataOffset)
        {
            var a = new GameOffsets{Description = description};
            a.AmongUsClientOffset = AmongUsClientOffset;
            a.GameDataOffset = GameDataOffset;
            a.MeetingHudOffset = MeetingHudOffset;
            a.GameStartManagerOffset = GameStartManagerOffset;
            a.HudManagerOffset = HudManagerOffset;
            a.ServerManagerOffset = ServerManagerOffset;
            a.TempDataOffset = WinDataOffset;

            return a;
        }
    }
}